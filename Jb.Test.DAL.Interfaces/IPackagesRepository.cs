using System.Collections.Generic;
using System.Threading.Tasks;

using Jb.Test.DAL.Interfaces.Model;

namespace Jb.Test.DAL.Interfaces
{
	/// <summary>
	/// Интерфейс репозитория пакетов.
	/// </summary>
	public interface IPackagesRepository
	{
		/// <summary>
		/// Создает или обновляет пакет.
		/// </summary>
		/// <param name="package">Информация пакета.</param>
		/// <param name="data">Данные пакета.</param>
		/// <param name="apiKey">API-ключ пользователя.</param>
		/// <returns>Информация о пакете.</returns>
		Task<Package> CreateOrUpdateAsync(Package package, byte[] data, string apiKey);


		/// <summary>
		/// Выполняет поиск пакета/версий пакета, по Id.
		/// </summary>
		/// <param name="id">Идентификатор пакета.</param>
		/// <param name="level">Уровен поиска.</param>
		/// <returns>Результат поиска.</returns>
		Task<IEnumerable<Package>> FindByIdAsync(string id, LoadingLevel level);


		/// <summary>
		/// Возвращает данные версии пакета.
		/// </summary>
		/// <param name="id">Идентификатор пакета.</param>
		/// <param name="version">Версия пакета.</param>
		/// <returns>Данные пакеты.</returns>
		Task<byte[]> GetDataByIdAndVersionAsync(string id, string version);


		/// <summary>
		/// Возвращает множество информаций о пакетах, по фильтру.
		/// </summary>
		/// <param name="filter">Фильтр пакетов.</param>
		/// <returns>Множество пакетов.</returns>
		Task<IReadOnlyCollection<Package>> GetAllByFilterAsync(Filter filter);
	}
}
