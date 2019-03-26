using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using System.Web.Http.Results;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;

namespace Jb.Test.ODataODataInfrastructura.Query
{
	public class QueryResult<TModel> : IHttpActionResult
	{
		private const string BadRequestMsg = "Could not execute OData query.";
		private readonly ODataQueryOptions<TModel> _queryOptions;
		private readonly IQueryable<TModel> _queryable;
		private readonly ApiController _controller;
		private readonly long? _totalResults;
		private readonly Func<ODataQueryOptions<TModel>, ODataQuerySettings, long?, Uri> _generateNextLink;
		private readonly bool _isPagedResult;
		private readonly ODataValidationSettings _validationSettings;
		private readonly ODataQuerySettings _querySettings;


		public bool FormatAsCountResult { get; set; }
		public bool FormatAsSingleResult { get; set; }


		public QueryResult(
			ODataQueryOptions<TModel> queryOptions,
			IQueryable<TModel> queryable,
			ApiController controller,
			int maxPageSize)
		    : this(
			      queryOptions,
			      queryable,
			      controller,
			      maxPageSize,
			      null,
			      null)
		{
		}


		public QueryResult(
		    ODataQueryOptions<TModel> queryOptions,
		    IQueryable<TModel> queryable,
		    ApiController controller,
		    int maxPageSize,
		    long? totalResults,
		    Func<ODataQueryOptions<TModel>, ODataQuerySettings, long?, Uri> generateNextLink)
		{
			_queryOptions = queryOptions;
			_queryable = queryable;
			_controller = controller;
			_totalResults = totalResults;
			_generateNextLink = generateNextLink;

			if(_totalResults.HasValue && generateNextLink != null)
			{
				_isPagedResult = true;
			}

			_validationSettings = new ODataValidationSettings()
			{
				MaxNodeCount = 250
			};

			_querySettings = new ODataQuerySettings(QueryResultDefaults.DefaultQuerySettings)
			{
				PageSize = maxPageSize
			};
		}


		public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			try
			{
				return await GetInnerResult().ExecuteAsync(cancellationToken);
			}
			catch(ODataException e)
			{
				return _controller.Request.CreateErrorResponse(
				    HttpStatusCode.BadRequest,
				    $"URI or query string invalid. {e.Message}",
				    e);
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public IHttpActionResult GetInnerResult()
		{
			IQueryable queryResults = null;

			if(FormatAsCountResult)
			{
				_querySettings.PageSize = null;
			}

			var queryOptions = _queryOptions;

			if(PagedResult() != true)
			{
				queryResults = ExecuteQuery(_queryable, queryOptions);
			}
			else
			{
				queryResults = _queryable.ToList().AsQueryable();

				if(queryOptions.Filter != null)
				{
					queryResults = queryOptions.Filter.ApplyTo(queryResults, _querySettings);
				}

				if(ExistOrderBy(queryOptions.OrderBy))
				{
					queryResults = queryOptions.OrderBy.ApplyTo(queryResults, _querySettings);
				}

				if(queryOptions.Top != null)
				{
					queryResults = queryOptions.Top.ApplyTo(queryResults, _querySettings);
				}

				queryOptions = null;
			}

			if(queryResults == null)
			{
				return NotFoundResult();
			}

			var modelQueryResults = queryResults as IQueryable<TModel>;
			var projectedQueryResults = queryResults as IQueryable<IEdmEntityObject>;

			if(FormatAsSingleResult)
			{
				return CreateFormatAsSingleResult(
					modelQueryResults,
					projectedQueryResults);
			}

			if(PagedResult())
			{
				return CreatPagedResult(
					modelQueryResults,
					projectedQueryResults);
			}

			if(FormatAsCountResult)
			{
				return CreateCountResult(
					modelQueryResults,
					projectedQueryResults);
			}

			return CreateRegularResult(
				modelQueryResults,
				projectedQueryResults);
		}


		private IHttpActionResult CreateRegularResult(
			IQueryable<TModel> modelQueryResults,
			IQueryable<IEdmEntityObject> projectedQueryResults)
		{
			if(modelQueryResults != null)
			{
				return NegotiatedContentResult(modelQueryResults);
			}

			if(projectedQueryResults == null)
			{
				return BadRequest(BadRequestMsg);
			}

			var elementType = projectedQueryResults.GetType().GenericTypeArguments.FirstOrDefault();

			return elementType != null
				? ProjectedNegotiatedContentResult(projectedQueryResults, elementType)
				: NegotiatedContentResult(projectedQueryResults);
		}


		private IHttpActionResult CreateCountResult(
			IQueryable<TModel> modelQueryResults,
			IQueryable<IEdmEntityObject> projectedQueryResults)
		{
			if(modelQueryResults != null)
			{
				return CountResult(modelQueryResults.Count());
			}

			if(projectedQueryResults != null)
			{
				return CountResult(projectedQueryResults.AsEnumerable().Count());
			}

			return BadRequest(BadRequestMsg);
		}


		private IHttpActionResult CreatPagedResult(
			IQueryable<TModel> modelQueryResults,
			IQueryable<IEdmEntityObject> projectedQueryResults)
		{
			if(FormatAsCountResult)
			{
				return CountResult(_totalResults.Value);
			}

			if(modelQueryResults != null)
			{
				return NegotiatedContentResult(
				    new PageResult<TModel>(
					    modelQueryResults,
					    _generateNextLink(
						    _queryOptions,
						    _querySettings,
						    _totalResults),
					    _totalResults));
			}

			if(projectedQueryResults != null)
			{
				return NegotiatedContentResult(
				    new PageResult<IEdmEntityObject>(
					    projectedQueryResults,
					    _generateNextLink(
						    _queryOptions,
						    _querySettings,
						    _totalResults),
					    _totalResults));
			}

			return BadRequest(BadRequestMsg);
		}


		private IHttpActionResult CreateFormatAsSingleResult(
			IQueryable<TModel> modelQueryResults,
			IQueryable<IEdmEntityObject> projectedQueryResults)
		{
			if(FormatAsCountResult)
			{
				var count = 0;
				if(modelQueryResults != null)
				{
					count = Math.Min(1, modelQueryResults.Count());
				}
				else if(projectedQueryResults != null)
				{
					count = Math.Min(1, projectedQueryResults.Count());
				}

				return CountResult(count);
			}

			if(modelQueryResults != null)
			{
				return NegotiatedContentResult(modelQueryResults.FirstOrDefault());
			}

			if(projectedQueryResults != null)
			{
				return NegotiatedContentResult(projectedQueryResults.AsEnumerable().FirstOrDefault());
			}

			return BadRequest(BadRequestMsg);
		}


		private bool PagedResult()
		{
			return _isPagedResult
				&& _generateNextLink != null
				&& _totalResults.HasValue;
		}


		private bool ExistOrderBy(OrderByQueryOption orderBy)
		{
			if(orderBy == null)
				return false;
			if(orderBy
				.OrderByClause
				.ItemType
				.Definition
				.TypeKind == Microsoft.Data.Edm.EdmTypeKind.Primitive)
				return true;


			return orderBy.OrderByClause.Expression.Kind == QueryNodeKind.None;
		}


		private void ValidateQuery(ODataQueryOptions<TModel> queryOptions)
		{
			var queryParameters = _controller.Request.GetQueryNameValuePairs();
			foreach(var kvp in queryParameters)
			{
				if(ODataQueryOptions.IsSystemQueryOption(kvp.Key) != true &&
				     kvp.Key.StartsWith("$", StringComparison.Ordinal))
				{
					// We don't support any custom query options that start with $
					var response = _controller
						.Request
						.CreateErrorResponse(
						HttpStatusCode.BadRequest,
						$"Query parameter {kvp.Key} is not supported.");

					throw new HttpResponseException(response);
				}
			}

			queryOptions.Validate(_validationSettings);
		}


		private IQueryable ExecuteQuery(IQueryable<TModel> queryable, ODataQueryOptions<TModel> queryOptions)
		{
			if(queryOptions == null)
			{
				return queryable;
			}

			ValidateQuery(queryOptions);

			var queryResult = queryOptions.ApplyTo(queryable, _querySettings);

			var projection = queryResult as IQueryable<IEdmEntityObject>;
			if(projection != null)
			{
				return projection;
			}

			return queryResult;
		}


		private BadRequestErrorMessageResult BadRequest(string message)
		{
			return new BadRequestErrorMessageResult(message, _controller);
		}


		private NotFoundResult NotFoundResult()
		{
			return new NotFoundResult(_controller.Request);
		}


		private PlainTextResult CountResult(long count)
		{
			return new PlainTextResult(
				count.ToString(CultureInfo.InvariantCulture),
				_controller.Request);
		}


		private OkNegotiatedContentResult<TResponseModel> NegotiatedContentResult<TResponseModel>(TResponseModel content)
		{
			return new OkNegotiatedContentResult<TResponseModel>(content, _controller);
		}


		private IHttpActionResult ProjectedNegotiatedContentResult<TResponseModel>(TResponseModel content, Type projectedType)
		{
			var resultType = typeof(OkNegotiatedContentResult<>)
			    .MakeGenericType(typeof(IQueryable<>)
			    .MakeGenericType(projectedType));

			return Activator.CreateInstance(
				resultType,
				content,
				_controller) as IHttpActionResult;
		}
	}
}