using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Deserialization;
using Jb.Test.ODataODataInfrastructura;

namespace Jb.Test.Web.ODataNugetODataInfrastuctura
{
	/// <summary>
	/// Атрибудт для nuget-odata контроллера.
	/// Для подмены и изменения порядка форматеров.
	/// </summary>
	internal class NugetControllerAttribute : Attribute, IControllerConfiguration
	{
		private static IList<ODataMediaTypeFormatter> _formatters;
		private static object _syncLock = new object();

		/// <summary>
		/// Выполняет инициализацию.
		/// </summary>
		/// <param name="controllerSettings">Настройки контроллера.</param>
		/// <param name="controllerDescriptor">Описатель контроллера.</param>
		public void Initialize(
			HttpControllerSettings controllerSettings,
			HttpControllerDescriptor controllerDescriptor)
		{
			controllerSettings.Formatters.Clear();
			controllerSettings.Formatters.InsertRange(0, GetFormatters());
		}


		private IList<ODataMediaTypeFormatter> GetFormatters()
		{
			if(_formatters != null)
				return _formatters;

			lock(_syncLock)
			{
				if(_formatters != null)
				{
					return _formatters;
				}

				var serProvider = new CustomSerializerProvider(
					provider => new NugetSerializer(provider));

				var createdFormatters = ODataMediaTypeFormatters.Create(
					serProvider, new DefaultODataDeserializerProvider());

				SetJsonAfterXmlFormater(createdFormatters);

				_formatters = createdFormatters;
			}

			return _formatters;
		}

		private static void SetJsonAfterXmlFormater(IList<ODataMediaTypeFormatter> createdFormatters)
		{
			var jsonFormatters = createdFormatters
					.Where(x => x.SupportedMediaTypes.Any(y => y.MediaType.Contains("json")))
					.ToArray();

			var list = createdFormatters.Where(x => jsonFormatters.Contains(x)).ToList();
			foreach(var itr in list)
			{
				createdFormatters.Remove(itr);
			}

			var xmlFormatterIndex = createdFormatters
				.IndexOf(createdFormatters.Last(x => x.SupportedMediaTypes.Any(y => y.MediaType.Contains("xml"))));

			foreach(var formatter in jsonFormatters)
			{
				createdFormatters.Insert(xmlFormatterIndex++, formatter);
			}
		}
	}
}