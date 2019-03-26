using System;
using System.Data;
using System.Data.Entity;
using System.Threading.Tasks;
using Jb.Test.DAL.Impl.Configurations;
using Jb.Test.DAL.Impl.DTOs;

namespace Jb.Test.DAL.Impl
{
	/// <summary>
	/// Класс контекста БД.
	/// </summary>
	public sealed class PackageStore : DbContext, IPacakgeStore
	{
		/// <summary>
		/// Создает новый экземпляр класса.
		/// </summary>
		public PackageStore() : base("NugetFeedDb")
		{
			Configuration.AutoDetectChangesEnabled = false;
			Configuration.ProxyCreationEnabled = false;
		}


		/// <summary>
		/// Возвращает /устанавливает множество идентификаторов пакетов.
		/// </summary>
		public DbSet<PackageId> PackageIds { get; set; }


		/// <summary>
		/// Возвращает / устанавливает множество версий пакетов.
		/// </summary>
		public DbSet<PackageVersion> PackageVersions { get; set; }


		/// <summary>
		/// Возвращает / устанавливает множество бинарных данных версий пакета.
		/// </summary>
		public DbSet<PackageVersionData> PackageVersionDatas { get; set; }


		/// <summary>
		/// Возвращает / устанавливает множество идентифицированных пользователей системы.
		/// </summary>
		public DbSet<NugetUser> NugetUsers { get; set; }


		/// <summary>
		/// Начинает транзакцию.
		/// </summary>
		/// <returns>Объект транзакции.</returns>
		public IDisposable BeginTransaction()
		{
			return Database.BeginTransaction(IsolationLevel.Serializable);
		}


		/// <summary>
		/// Фиксирует транзакцию.
		/// </summary>
		/// <param name="transaction">Объект транзакции.</param>
		public void Commit(IDisposable transaction)
		{
			var tran = transaction as DbContextTransaction;
			tran?.Commit();
		}


		/// <summary>
		/// Откатывает транзакцию.
		/// </summary>
		/// <param name="transaction">Объект транзакции.</param>
		public void Rollback(IDisposable transaction)
		{
			var tran = transaction as DbContextTransaction;
			tran?.Rollback();
		}


		/// <summary>
		/// Сохраняет изменения.
		/// </summary>
		/// <returns>Количество измененных записей.</returns>
		public Task<int> SaveAsync()
		{
			ChangeTracker.DetectChanges();
			return SaveChangesAsync();
		}

		/// <summary>
		/// Сохраняет, в синхронном режиме, изменения.
		/// </summary>
		/// <returns>Количество измененных записей.</returns>
		public int Save()
		{
			ChangeTracker.DetectChanges();
			return SaveChanges();
		}


		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Configurations.Add(new NugetUserConfiguration());
			modelBuilder.Configurations.Add(new PackageVersionDataConfiguration());
			modelBuilder.Configurations.Add(new PackagesVersionConfiguration());
			modelBuilder.Configurations.Add(new PackageIdConfiguration());
		}
	}
}
