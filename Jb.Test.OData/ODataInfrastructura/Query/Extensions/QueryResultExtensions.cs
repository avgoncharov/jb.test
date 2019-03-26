using System.Web.Http;

namespace Jb.Test.ODataODataInfrastructura.Query.Extensions
{
	public static class QueryResultExtensions
	{
		public static IHttpActionResult FormattedAsCountResult<T>(this IHttpActionResult current)
		{
			var queryResult = current as QueryResult<T>;
			if(queryResult != null)
			{
				queryResult.FormatAsCountResult = true;
				return queryResult;
			}

			return current;
		}


		public static IHttpActionResult FormattedAsSingleResult<T>(this IHttpActionResult current)
		{
			var queryResult = current as QueryResult<T>;
			if(queryResult != null)
			{
				queryResult.FormatAsSingleResult = true;
				return queryResult;
			}

			return current;
		}
	}
}