using System;
using System.Web.Http.OData.Formatter.Serialization;
using Microsoft.Data.Edm;

namespace Jb.Test.ODataODataInfrastructura
{
	/// <summary>
	/// Кастомезированный провайдер сериализации.
	/// </summary>
	public class CustomSerializerProvider : DefaultODataSerializerProvider
	{
		private readonly ODataEdmTypeSerializer _entitySerializer;

		/// <summary>
		/// Создает новый экземпляр класса.
		/// </summary>
		/// <param name="factory">Фабрика сериализатора типов.</param>
		public CustomSerializerProvider(Func<DefaultODataSerializerProvider, ODataEdmTypeSerializer> factory)
		{
			_entitySerializer = factory(this);
		}


		/// <summary>
		/// Возвращает специализированный сериализаторв, в случае если работаем с сущностью.
		/// </summary>
		/// <param name="edmType">Тип edm.</param>
		/// <returns>Соответствующий сериализатор.</returns>
		public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
		{
			return edmType.IsEntity()
				? _entitySerializer
				: base.GetEdmTypeSerializer(edmType);
		}
	}
}