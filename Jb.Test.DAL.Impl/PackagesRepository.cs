using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jb.Test.DAL.Impl.DTOs;
using Jb.Test.DAL.Interfaces;
using Jb.Test.DAL.Interfaces.Model;
using System.Data.Entity;
using Jb.Test.DAL.Impl.Extensions;
using Serilog;
using NugetUser = Jb.Test.DAL.Impl.DTOs.NugetUser;
using Package = Jb.Test.DAL.Interfaces.Model.Package;

namespace Jb.Test.DAL.Impl
{

	/// <summary>
	/// Класс реализации EF-репозитория.
	/// </summary>
	public sealed class PackagesRepository : IPackagesRepository
	{
		private readonly IPacakgeStore _store;
		private readonly ILogger _logger = Log.Logger.ForContext<PackagesRepository>();

		/// <summary>
		/// Создает новый экземпляр репозитория.
		/// </summary>
		/// <param name="store">Контектс репозитория.</param>
		public PackagesRepository(IPacakgeStore store)
		{
			_store = store;
		}


		/// <summary>
		/// Сохраняет новый нугет-пакет, или изменяет существующий.
		/// </summary>
		/// <param name="package">Описание пакета.</param>
		/// <param name="data">Данные пакета.</param>
		/// <param name="apiKey">Ключ пользователяю</param>
		/// <returns>Описание пакета.</returns>
		public async Task<Package> CreateOrUpdateAsync(
			Package package,
			byte[] data,
			string apiKey)
		{
			if(package == null)
				throw new ArgumentNullException(nameof(package));

			if(string.IsNullOrWhiteSpace(apiKey))
				throw new ArgumentException("ApiKey can't be empty.");

			if(data == null || data.Length == 0)
				throw new InvalidOperationException("Data can't be empty.");

			var innerUser = await _store.NugetUsers.FirstOrDefaultAsync(itr => itr.ApiKey == apiKey);
			if(innerUser == null)
				throw new InvalidOperationException($"User with api key '{apiKey}' not found.");

			using(var scope = _store.BeginTransaction())
			{
				//C транзакциями мутновато получается.
				try
				{
					var old = await _store.PackageIds
						.Where(itr => itr.Id == package.Id)
						.FirstOrDefaultAsync();

					if(old == null)
					{
						return await CreateNewPackage(package, data, innerUser, scope);
					}

					old.LastUpdate = DateTime.UtcNow;
					old.NugetUser = innerUser;

					var oldVersion = await _store.PackageVersions.FirstOrDefaultAsync(itr =>
						itr.Id == package.Id && itr.Version == package.Version);

					if(oldVersion == null)
					{
						return await AddNewVersion(old, package, data, scope);
					}

					return await UpdateOnlyVersionData(oldVersion, package, data, scope);
				}
				catch(Exception ex)
				{
					_logger.Error($"Save data for package {{{package}}} failed. Reason: {ex}");
					_store.Rollback(scope);
					throw;
				}
			}
		}


		/// <summary>
		/// Выполняет поиск пакет или все его версии, по id.
		/// </summary>
		/// <param name="id">Идентификатор пакета.</param>
		/// <param name="level">Уровень загрузки.</param>
		/// <returns>Набор описаний пакетов с заданным Id.</returns>
		public async Task<IEnumerable<Package>> FindByIdAsync(string id, LoadingLevel level)
		{
			var query = _store.PackageVersions.Where(itr => itr.Id == id);

			switch(level)
			{
				case LoadingLevel.LatestVersion:
				var buf = (await query.FirstOrDefaultAsync(itr => itr.IsLatestVersion))?.ConvertToModel();
				return buf != null ? new[] { buf } : new Package[0];

				case LoadingLevel.AllVersion:
				return query.ConvertToModels();

				default:
				throw new ArgumentOutOfRangeException(nameof(level), level, null);
			}
		}


		/// <summary>
		/// Возвращает бинарные данные заданной версии пакета.
		/// </summary>
		/// <param name="id">Идентификатор пакета.</param>
		/// <param name="version">Версия пакета.</param>
		/// <returns>Бинарные данные пакета.</returns>
		public async Task<byte[]> GetDataByIdAndVersionAsync(string id, string version)
		{
			var key = BuildKey(id, version);

			var result = await _store.PackageVersionDatas
				.Where(itr => itr.PackageVersionId == key)
				.FirstOrDefaultAsync();

			return result?.Data;
		}


		/// <summary>
		/// Возвращает набор пакетов, удовлетворяющих фильтру.
		/// </summary>
		/// <param name="filter">Фильтр поиска.</param>
		/// <returns>Набор искомы пакетов.</returns>
		public async Task<IReadOnlyCollection<Package>> GetAllByFilterAsync(Filter filter)
		{
			if(filter == null || IsEmpty(filter))
			{
				var result = _store.PackageVersions.Where(itr => itr.IsLatestVersion);
				return result.ConvertToModels().ToList().AsReadOnly();
			}

			var packages = _store.PackageVersions.AsQueryable();

			if(string.IsNullOrWhiteSpace(filter.IdPattern) != true)
			{
				packages = packages.Where(itr => itr.Id.Contains(filter.IdPattern));
			}

			if(string.IsNullOrWhiteSpace(filter.TitlePattern) != true)
			{
				packages = packages.Where(itr => itr.Title.Contains(filter.TitlePattern));
			}

			if(string.IsNullOrWhiteSpace(filter.VersionPattern) != true)
			{
				packages = packages.Where(itr => itr.Version.Contains(filter.VersionPattern));
			}

			if(string.IsNullOrWhiteSpace(filter.DescriptionPattern) != true)
			{
				packages = packages.Where(itr => itr.Description.Contains(filter.DescriptionPattern));
			}

			return (await packages.ToListAsync())
				.ConvertToModels()
				.ToList()
				.AsReadOnly();
		}

		private static bool IsEmpty(Filter filter)
		{
			return string.IsNullOrWhiteSpace(filter.IdPattern)
				&& string.IsNullOrWhiteSpace(filter.VersionPattern)
				&& string.IsNullOrWhiteSpace(filter.TitlePattern)
				&& string.IsNullOrWhiteSpace(filter.DescriptionPattern);

		}

		private async Task<Package> UpdateOnlyVersionData(
			PackageVersion oldVersion,
			Package package,
			byte[] data,
			IDisposable scope)
		{
			var key = BuildKey(package);
			var oldData = await _store.PackageVersionDatas.FirstOrDefaultAsync(itr => itr.PackageVersionId == key);

			if(oldData == null)
			{ throw new InvalidOperationException("Data wasn't found."); }

			oldData.Data = data;
			oldVersion.LastUpdated = DateTime.UtcNow;

			await _store.SaveAsync();

			_store.Commit(scope);

			return oldVersion.ConvertToModel();
		}


		private async Task<Package> AddNewVersion(
			PackageId old,
			Package package,
			byte[] data,
			IDisposable scope)
		{
			var verData = new PackageVersionData
			{
				PackageVersionId = BuildKey(package),
				Data = data
			};

			var version = package.ConvertToDto();
			version.LastUpdated = DateTime.UtcNow;
			version.Published = version.LastUpdated;

			version.PackageId = old;

			var latest = await _store.PackageVersions.FirstOrDefaultAsync(itr => itr.Id == package.Id && itr.IsLatestVersion);
			version.IsLatestVersion = latest == null || CheckIsLatest(version.Version, latest.Version);

			if(version.IsLatestVersion && latest != null)
			{
				latest.IsLatestVersion = false;
			}

			_store.PackageVersions.Add(version);
			_store.PackageVersionDatas.Add(verData);

			await _store.SaveAsync();

			_store.Commit(scope);

			return version.ConvertToModel();
		}

		private static bool CheckIsLatest(string newVersion, string currentLatest)
		{
			var newParts = newVersion.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
			var currParts = currentLatest.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);

			var minLen = Math.Min(newParts.Length, currParts.Length);

			for(var i = 0; i < minLen; ++i)
			{
				var newNum = Convert.ToInt32(newParts[i]);
				var currNum = Convert.ToInt32(currParts[i]);

				if(newNum > currNum)
					return true;

				if(newNum < currNum)
					return false;
			}

			return newParts.Length > currParts.Length;
		}


		private static string BuildKey(Package package) => BuildKey(package.Id, package.Version);


		private static string BuildKey(string id, string version) => $"{id}.{version}";


		private async Task<Package> CreateNewPackage(
			Package package,
			byte[] data,
			NugetUser innerUser,
			IDisposable scope)
		{
			var verData = new PackageVersionData
			{
				PackageVersionId = BuildKey(package),
				Data = data
			};

			var version = package.ConvertToDto();
			version.IsLatestVersion = true;
			version.LastUpdated = DateTime.UtcNow;
			version.Published = version.LastUpdated;

			var newPackageId = new PackageId
			{
				Id = package.Id,
				LastUpdate = DateTime.UtcNow,
				Versions = new List<PackageVersion> { version },
				NugetUser = innerUser
			};

			_store.PackageIds.Add(newPackageId);
			version.PackageId = newPackageId;
			_store.PackageVersions.Add(version);
			_store.PackageVersionDatas.Add(verData);

			await _store.SaveAsync();

			_store.Commit(scope);

			return version.ConvertToModel();
		}

	}
}
