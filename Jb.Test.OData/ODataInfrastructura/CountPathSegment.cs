using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using System.Web.Http.OData.Routing;

namespace Jb.Test.ODataODataInfrastructura
{
	/// <summary>
	/// Класс объектного представления сегмента путик 'count'.
	/// </summary>
	public class CountPathSegment : ODataPathSegment
	{
		/// <summary>
		/// Возвращает тип сегмента.
		/// </summary>
		public override string SegmentKind => "$count";


		/// <summary>
		/// Возвращает EDM тип ожидаемого результата сегмента.
		/// </summary>
		/// <param name="previousEdmType">Тип предыдущего сегмента пути.</param>
		/// <returns>EDM тип ожидаемого результата сегмента.</returns>
		public override IEdmType GetEdmType(IEdmType previousEdmType)
			=> EdmCoreModel.Instance.FindDeclaredType("Edm.Int32");


		/// <summary>
		/// Взвращает нобор EMD-сущносте. 
		/// </summary>
		/// <param name="previousEntitySet">Предыдущий набор сущностей.</param>
		/// <returns>Нобор EMD-сущносте. </returns>
		public override IEdmEntitySet GetEntitySet(IEdmEntitySet previousEntitySet) => previousEntitySet;


		/// <summary>
		/// Возвращает строковое представление сегмента.
		/// </summary>
		/// <returns></returns>
		public override string ToString() => "$count";
	}
}