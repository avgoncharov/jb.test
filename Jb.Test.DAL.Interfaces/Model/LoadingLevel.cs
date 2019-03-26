namespace Jb.Test.DAL.Interfaces.Model
{
	/// <summary>
	/// Перечисление уровня загрузки, при поиске по Id.
	/// </summary>
	public enum LoadingLevel
	{
		/// <summary>
		/// Только последнюю версию.
		/// </summary>
		LatestVersion = 0,


		/// <summary>
		/// Все версии.
		/// </summary>
		AllVersion = 1 
	}
}
