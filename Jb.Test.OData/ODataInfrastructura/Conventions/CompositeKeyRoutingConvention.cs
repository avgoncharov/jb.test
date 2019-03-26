using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;

namespace Jb.Test.ODataODataInfrastructura.Conventions
{
	/// <summary>
	/// Класс-реализация конвенции составного ключа.
	/// </summary>
	public class CompositeKeyRoutingConvention : EntityRoutingConvention
	{
		public override string SelectAction(
			ODataPath odataPath,
			HttpControllerContext controllerContext,
			ILookup<string, HttpActionDescriptor> actionMap)
		{
			var routeValues = controllerContext.RouteData.Values;

			var action = base.SelectAction(odataPath, controllerContext, actionMap);
			if(action != null)
			{
				if(routeValues.ContainsKey(ODataRouteConstants.Key))
				{
					var keyRaw = routeValues[ODataRouteConstants.Key] as string;
					if(keyRaw != null)
					{
						CompositeODataKeyHelper.TryEnrichRouteValues(keyRaw, routeValues);
					}
				}

				return action;
			}

			if(odataPath.PathTemplate == "~/entityset/key/action" ||
				odataPath.PathTemplate == "~/entityset/key/cast/action")
			{
				var keyValueSegment = odataPath.Segments[1] as KeyValuePathSegment;
				var actionSegment = odataPath.Segments.Last() as ActionPathSegment;
				var actionFunctionImport = actionSegment.Action;

				controllerContext.RouteData.Values[ODataRouteConstants.Key] = keyValueSegment.Value;
				CompositeODataKeyHelper.TryEnrichRouteValues(keyValueSegment.Value, routeValues);

				return actionFunctionImport.Name;
			}

			return action;
		}


		private static class CompositeODataKeyHelper
		{
			private static readonly char[] KeyTrimChars = { ' ', '\'' };


			public static bool TryEnrichRouteValues(string keyRaw, IDictionary<string, object> routeValues)
			{
				var compoundKeyPairs = keyRaw.Split(',');
				if(!compoundKeyPairs.Any())
				{
					return false;
				}

				foreach(var compoundKeyPair in compoundKeyPairs)
				{
					var pair = compoundKeyPair.Split('=');
					if(pair.Length != 2)
					{
						continue;
					}

					var keyName = pair[0].Trim(KeyTrimChars);
					var keyValue = pair[1].Trim(KeyTrimChars);

					routeValues.Add(keyName, keyValue);
				}

				return true;
			}
		}
	}
}