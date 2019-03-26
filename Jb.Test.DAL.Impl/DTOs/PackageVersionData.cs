namespace Jb.Test.DAL.Impl.DTOs
{
	/// <summary>
	/// Класс данных версии пакета.
	/// </summary>
	public class PackageVersionData
	{
		/// <summary>
		/// Возвращает / устанавливает идентификатор версии пакета.
		/// </summary>
		public string PackageVersionId { get; set; }


		/// <summary>
		/// Возвращает / устанавливает данные версии пакета.
		/// </summary>
		public byte[] Data { get; set; }
	}
}
