using Jb.Test.DAL.Interfaces.Model;
using Jb.Test.ODataODataInfrastructura.Query;
using Jb.Test.ODataODataInfrastructura.Query.Extensions;
using Jb.Test.Web.ODataNugetODataInfrastuctura;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using Jb.Test.NugetDataExtractor;
using Jb.Test.DAL.Interfaces;
using Serilog;

namespace Jb.Test.Web.Controllers
{
	/// <summary>
	/// Класс контроллера для OData API взаимодействия с nuget-клиентами.
	/// </summary>
	[NugetController]
	public sealed class NugetODataController : ODataController
	{
		private const string ApiKeyHeader = "X-NUGET-APIKEY";
		private const int MaxPageSize = 25;

		private readonly IPackagesRepository _repository;
		private readonly ILogger _logger = Log.ForContext<NugetODataController>();

		/// <summary>
		/// Создает новый экземпляр класса.
		/// </summary>
		/// <param name="repository">Репозиторий пакетов.</param>
		public NugetODataController(IPackagesRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}


		/// <summary>
		/// Возвращает последнии версии имеющихся пакетов.
		/// </summary>
		/// <param name="options">Опции запроса.</param>
		/// <param name="semVerLevel"></param>
		/// <param name="token">Токен коардинированной отмены.</param>
		/// <returns>Набор последних версий пакетов.</returns>
		[HttpGet]
		public async Task<IHttpActionResult> Get(
		    ODataQueryOptions<Package> options,
		    [FromUri] string semVerLevel = "",
		    CancellationToken token = default(CancellationToken))
		{
			var sourceQuery = await _repository.GetAllByFilterAsync(new Filter());

			return TransformToQueryResult(options, sourceQuery);
		}


		/// <summary>
		/// Возвращает заданную версию искомого пакета. 
		/// </summary>
		/// <param name="options">Опеции.</param>
		/// <param name="id">Идентификатор пакета.</param>
		/// <param name="version">Версия пакета.</param>
		/// <param name="token"></param>
		/// <returns>Искомая версия пакета, или NotFound.</returns>
		[HttpGet]
		public async Task<IHttpActionResult> Get(
		    ODataQueryOptions<Package> options,
		    string id,
		    string version,
		    CancellationToken token)
		{
			var package = await RetrieveFromRepositoryAsync(id, version, token);

			if (package == null)
			{
				return NotFound();
			}

			return TransformToQueryResult(options, new[] { package }).FormattedAsSingleResult<Package>();
		}


		/// <summary>
		/// Выполняет поиск по Id пакета.
		/// </summary>
		/// <param name="options">Опции.</param>
		/// <param name="id">Идентификатор пакета.</param>
		/// <param name="semVerLevel"></param>
		/// <param name="token"></param>
		/// <returns>Последняя версия искомого пакета.</returns>
		[HttpGet]
		public async Task<IHttpActionResult> FindPackagesById(
		    ODataQueryOptions<Package> options,
		    [FromODataUri] string id,
		    [FromUri] string semVerLevel = "",
		    CancellationToken token = default(CancellationToken))
		{
			if (string.IsNullOrEmpty(id))
			{
				var emptyResult = Enumerable.Empty<Package>().AsQueryable();
				return new QueryResult<Package>(options, emptyResult, this, MaxPageSize);
			}
			
			var sourceQuery = (await _repository.FindByIdAsync(id, LoadingLevel.LatestVersion));

			return TransformToQueryResult(options, sourceQuery);
		}


		/// <summary>
		/// Выполняет пописк пакетов по шаблону.
		/// </summary>
		/// <param name="options">Опции.</param>
		/// <param name="searchTerm">Шаблон Id для поиска.</param>
		/// <param name="targetFramework">Целевой фреймворк.</param>
		/// <param name="includePrerelease">Флаг включения в поиск пререлизов.</param>
		/// <param name="includeDelisted"></param>
		/// <param name="semVerLevel"></param>
		/// <param name="token"></param>
		/// <returns>Набор описаний пакетов, попадающих по фильтр.</returns>
		[HttpGet]
		public async Task<IHttpActionResult> Search(
		    ODataQueryOptions<Package> options,
		    [FromODataUri] string searchTerm = "",
		    [FromODataUri] string targetFramework = "",
		    [FromODataUri] bool includePrerelease = false,
		    [FromODataUri] bool includeDelisted = false,
		    [FromUri] string semVerLevel = "",
		    CancellationToken token = default(CancellationToken))
		{
			var sourceQuery = await _repository.GetAllByFilterAsync(new Filter { IdPattern = searchTerm });

			return TransformToQueryResult(options, sourceQuery);
		}


		/// <summary>
		/// Выполняет загузку пакета на сторону клиента.
		/// </summary>
		/// <param name="id">Идентификатор пакета.</param>
		/// <param name="version">Версия пакета.</param>
		/// <param name="token"></param>
		/// <returns>Данные пакета.</returns>
		[HttpGet, HttpHead]
		public async Task<HttpResponseMessage> Download(
		    string id,
		    string version = "",
		    CancellationToken token = default(CancellationToken))
		{
			var requestedPackage = await RetrieveFromRepositoryAsync(id, version, token);

			if (requestedPackage == null)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Package {id} {version}' Not found.");
			}

			var data = await _repository.GetDataByIdAndVersionAsync(id, version);

			if (data == null)
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Data for package {id} {version}' Not found.");
			}

			var responseMessage = Request.CreateResponse(HttpStatusCode.OK);

			if (Request.Method == HttpMethod.Get)
			{
				responseMessage.Content = new StreamContent(new MemoryStream(data));
			}
			else
			{
				responseMessage.Content = new StringContent(string.Empty);
			}

			responseMessage.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("binary/octet-stream");
			responseMessage.Content.Headers.LastModified = requestedPackage.LastUpdated;
			responseMessage.Headers.ETag = new EntityTagHeaderValue('"' + requestedPackage.PackageHash + '"');

			responseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(DispositionTypeNames.Attachment)
			{
				FileName = $"{requestedPackage.Id}.{requestedPackage.Version}.nupkg",
				Size = data.Length,
				ModificationDate = responseMessage.Content.Headers.LastModified,
			};

			return responseMessage;
		}


		/// <summary>
		/// Выполняет загрузку пакета на сервер.
		/// </summary>
		/// <param name="token"></param>
		/// <returns>Результат загрузки.</returns>
		[HttpPut]
		public async Task<HttpResponseMessage> UploadPackage(CancellationToken token)
		{
			try
			{
				var apiKey = GetApiKeyFromHeader();

				byte[] buf;
				using (var temporaryFileStream = new MemoryStream())
				{
					if (Request.Content.IsMimeMultipartContent())
					{
						var multipartContents = await Request.Content.ReadAsMultipartAsync();
						await multipartContents.Contents.First().CopyToAsync(temporaryFileStream);
					}
					else
					{
						await Request.Content.CopyToAsync(temporaryFileStream);
					}

					temporaryFileStream.Flush();
					buf = temporaryFileStream.GetBuffer();
				}


				var de = new DataExtractor();
				var package = de.ExtractPackage(buf);

				await _repository.CreateOrUpdateAsync(package, buf, apiKey);
				return Request.CreateResponse(HttpStatusCode.Created);
			}
			catch (Exception ex)
			{
				   _logger.Error($"UploadPackage failed, reason: {ex}");
				return CreateStringResponse(HttpStatusCode.InternalServerError, "Could not remove temporary upload file.");
			}
		}


		private HttpResponseMessage CreateStringResponse(HttpStatusCode statusCode, string response)
		{
			var responseMessage = new HttpResponseMessage(statusCode) { Content = new StringContent(response) };
			return responseMessage;
		}


		private string GetApiKeyFromHeader()
		{
			string apiKey = null;
			if (Request.Headers.TryGetValues(ApiKeyHeader, out var values))
			{
				apiKey = values.FirstOrDefault();
			}

			return apiKey;
		}


		private async Task<Package> RetrieveFromRepositoryAsync(
		    string id,
		    string version,
		    CancellationToken token)
		{
			return string.IsNullOrEmpty(version)
				? (await _repository.FindByIdAsync(id, LoadingLevel.LatestVersion)).FirstOrDefault()
				: (await _repository.GetAllByFilterAsync(new Filter { IdPattern = id, VersionPattern = version })).FirstOrDefault();
		}


		private static IQueryable<Package> TransformPackages(IEnumerable<Package> packages)
		{
			return packages
			    .Distinct()
			    .AsQueryable()
			    .InterceptWith(new NormalizeVersionInterceptor());
		}


		private IHttpActionResult TransformToQueryResult(ODataQueryOptions<Package> options, IEnumerable<Package> sourceQuery)
		{
			var transformedQuery = TransformPackages(sourceQuery);
			return new QueryResult<Package>(options, transformedQuery, this, MaxPageSize);
		}
	}
}
