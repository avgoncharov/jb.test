using System;
using System.Collections.Generic;

namespace Jb.Test.DAL.Impl.DTOs
{
	/// <summary>
	/// Класс идентификатора пакета.
	/// </summary>
	public class PackageId
	{
		/// <summary>
		/// Возвращает / устанавливает идентификатор пакета.
		/// </summary>
		public string Id { get; set; }


		/// <summary>
		/// Возвращает / устанавливает дату последнего изменения пакета.
		/// </summary>
		public DateTime LastUpdate { get; set; }


		/// <summary>
		/// Возвращает / устанавливает пользователя, который последним менял пакет.
		/// </summary>
		public NugetUser NugetUser { get; set; }


		/// <summary>
		/// Возвращает / устанавливает версии пакета.
		/// </summary>
		public ICollection<PackageVersion> Versions { get; set; }
	}
}
