namespace Jb.Test.DAL.Interfaces.Model
{
	/// <summary>
	/// Класс фильтра для поиска пакетов.
	/// </summary>
	public sealed class Filter
	{
		/// <summary>
		/// Возвращает / устанавливает шаблон идентификатора пакета.
		/// </summary>
		public string IdPattern { get; set; }


		/// <summary>
		/// Возвращает / устанавливает шаблон заголовка пакета.
		/// </summary>
		public string TitlePattern { get; set; }


		/// <summary>
		/// Возвращает / устанавливает шаблон версии пакета.
		/// </summary>
		public string VersionPattern { get; set; }


		/// <summary>
		/// Возвращает / устанавливает шаблон описания пакета.
		/// </summary>
		public string DescriptionPattern { get; set; }
	}
}
