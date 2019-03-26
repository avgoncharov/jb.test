namespace Jb.Test.DAL.Interfaces.Model
{
	/// <summary>
	/// Класс идентифицированного пользователя фида.
	/// </summary>
	public sealed class NugetUser
	{
		/// <summary>
		/// Возвращает / устанавливает api-ключ пользователя.
		/// </summary>
		public string ApiKey { get; set; }


		/// <summary>
		/// Возвращает / устанавливает имя пользователя.
		/// </summary>
		public string Name { get; set; }  
	}
}
