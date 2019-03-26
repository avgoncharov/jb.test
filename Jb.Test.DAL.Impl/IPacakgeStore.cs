using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Jb.Test.DAL.Impl.DTOs;

namespace Jb.Test.DAL.Impl
{
	/// <summary>
	/// Интерфейс контекста БД.
	/// </summary>
	public interface IPacakgeStore : IDisposable
	{
		/// <summary>
		/// Возвращает /устанавливает множество идентификаторов пакетов.
		/// </summary>
		DbSet<PackageId> PackageIds { get; set; }


		/// <summary>
		/// Возвращает / устанавливает множество версий пакетов.
		/// </summary>
		DbSet<PackageVersion> PackageVersions { get; set; }


		/// <summary>
		/// Возвращает / устанавливает множество бинарных данных версий пакета.
		/// </summary>
		DbSet<PackageVersionData> PackageVersionDatas { get; set; }


		/// <summary>
		/// Возвращает / устанавливает множество идентифицированных пользователей системы.
		/// </summary>
		DbSet<NugetUser> NugetUsers { get; set; }


		/// <summary>
		/// Начинает транзакцию.
		/// </summary>
		/// <returns>Объект транзакции.</returns>
		IDisposable BeginTransaction();


		/// <summary>
		/// Фиксирует транзакцию.
		/// </summary>
		/// <param name="transaction">Объект транзакции.</param>
		void Commit(IDisposable transaction);

		/// <summary>
		/// Откатывает транзакцию.
		/// </summary>
		/// <param name="transaction">Объект транзакции.</param>
		void Rollback(IDisposable transaction);


		/// <summary>
		/// Сохраняет изменения.
		/// </summary>
		/// <returns>Количество измененных записей.</returns>
		Task<int> SaveAsync();

		/// <summary>
		/// Сохраняет, в синхронном режиме, изменения.
		/// </summary>
		/// <returns>Количество измененных записей.</returns>
		int Save();
	}
}
