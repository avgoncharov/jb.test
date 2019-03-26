using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;

namespace Jb.Test.ODataODataInfrastructura
{
	/// <summary>
	/// Класс обработчика сегмента "count."
	/// </summary>
	public class CountODataPathHandler : DefaultODataPathHandler
	{
		/// <summary>
		/// Выполняет парсинг сегмента.
		/// </summary>
		/// <param name="model">Модель EDM.</param>
		/// <param name="previous">Предыдущий сегмент.</param>
		/// <param name="previousEdmType">Предыдущий EDM-тип.</param>
		/// <param name="segment">Текущее строковое представление сегмента.</param>
		/// <returns>Результат парсинга.</returns>
		protected override ODataPathSegment ParseAtEntityCollection(
			IEdmModel model,
			ODataPathSegment previous,
			IEdmType previousEdmType,
			string segment)
		{
			if(segment == "$count")
			{
				return new CountPathSegment();
			}

			return base.ParseAtEntityCollection(
				model,
				previous,
				previousEdmType,
				segment);
		}
	}
}