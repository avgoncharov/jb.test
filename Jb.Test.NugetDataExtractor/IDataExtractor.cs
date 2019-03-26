using Jb.Test.DAL.Interfaces.Model;

namespace Jb.Test.NugetDataExtractor
{
	/// <summary>
	/// Интерфейс извлечения данных из архива nuget.
	/// </summary>
	public interface IDataExtractor
	{
		/// <summary>
		/// Извлекает данные в виде пакета.
		/// </summary>
		/// <param name="data">Данные в бинарном виде.</param>
		/// <returns>Пакет.</returns>
		Package ExtractPackage(byte[] data);
						     

		/// <summary>
		/// Извлекает метаданные пакета в виде строки xml.
		/// </summary>
		/// <param name="data">Пакет в бинарном виде.</param>
		/// <returns>Строка xml метаданных пакета.</returns>
		string ExtractRawMetadata(byte[] data);
	}
}
