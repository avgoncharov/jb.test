using System;

namespace Jb.Test.DAL.Impl.DTOs
{
	/// <summary>
	/// Класс версии пакета.
	/// </summary>
	public class PackageVersion
	{
		/// <summary>
		/// Возвращает / устанавливает идентификатор.
		/// </summary>
		public string Id { get; set; }


		/// <summary>
		/// Возвращает / устанавливает версию.
		/// </summary>
		public string Version { get; set; }


		/// <summary>
		/// Возвращает / устанавливает заголовок.
		/// </summary>
		public string Title { get; set; }


		/// <summary>
		/// Возвращает / устанавливает 	описание.
		/// </summary>
		public string Description { get; set; }


		/// <summary>
		/// Возвращает / устанавливает дату публикации.
		/// </summary>
		public DateTime Published { get; set; }


		/// <summary>
		/// Возвращает / устанавливает дату последнего изменения.
		/// </summary>
		public DateTime LastUpdated { get; set; }


		/// <summary>
		/// Возвращает / устанавливает признак является ли данная версию крайней.
		/// </summary>
		public bool IsLatestVersion { get; set; }

		/// <summary>
		/// Возвращает / устанавливает 
		/// </summary>
		public PackageId PackageId { get; set; }
	}
}
