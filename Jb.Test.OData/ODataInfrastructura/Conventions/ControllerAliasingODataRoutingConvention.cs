using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;

namespace Jb.Test.ODataODataInfrastructura.Conventions
{
	public class ControllerAliasingODataRoutingConvention : IODataRoutingConvention
	{
		private readonly IODataRoutingConvention _delegateRoutingConvention;
		private readonly string _controllerAlias;
		private readonly string _targetControllerName;


		public ControllerAliasingODataRoutingConvention(
			IODataRoutingConvention delegateRoutingConvention,
			string controllerAlias,
			string targetControllerName)
		{
			_delegateRoutingConvention = delegateRoutingConvention;
			_controllerAlias = controllerAlias;
			_targetControllerName = targetControllerName;
		}


		public string SelectController(ODataPath odataPath, HttpRequestMessage request)
		{
			var controller = _delegateRoutingConvention.SelectController(odataPath, request);
			return string.Equals(controller, _controllerAlias, StringComparison.OrdinalIgnoreCase)
				? _targetControllerName
				: controller;
		}


		public string SelectAction(
			ODataPath odataPath,
			HttpControllerContext controllerContext,
			ILookup<string, HttpActionDescriptor> actionMap)
		{
			var retValue = _delegateRoutingConvention.SelectAction(
				odataPath,
				controllerContext,
				actionMap);

			return retValue;
		}
	}
}