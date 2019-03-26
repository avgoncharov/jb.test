using System.Web.Http.OData.Query;

namespace Jb.Test.ODataODataInfrastructura.Query
{
	public static class QueryResultDefaults
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
		public static ODataQuerySettings DefaultQuerySettings = new ODataQuerySettings()
		{
			HandleNullPropagation = HandleNullPropagationOption.False,
			EnsureStableOrdering = true,
			EnableConstantParameterization = false
		};
	}
}