using System.Collections.Generic;

namespace Jb.Test.DAL.Impl.DTOs
{
	/// <summary>
	/// Класс идентифицированного пользователя системы.
	/// </summary>
	public class NugetUser
	{
		/// <summary>
		/// Возвращает / устанавливает api-ключ пользователя.
		/// </summary>
		public string ApiKey { get; set; }

		/// <summary>
		/// Возвращает / устанавливает имя пользователя.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Возвращает / устанавливает набор идентификаторов пакетов, которые обновлял пользователь.
		/// </summary>
		public ICollection<PackageId> PackageIds { get; set; }
	}
}
