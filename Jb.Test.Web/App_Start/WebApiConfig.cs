using Jb.Test.DAL.Interfaces.Model;
using Jb.Test.ODataODataInfrastructura;
using Jb.Test.ODataODataInfrastructura.Conventions;
using Microsoft.Data.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Routing;

namespace Jb.Test.Web
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{				
			config.MapHttpAttributeRoutes();

			var conventions = ODataRoutingConventions.CreateDefault();
			var oDatacontrollerName = "NugetOData";
			conventions.Insert(0, new MethodNameActionRoutingConvention(oDatacontrollerName));
			conventions.Insert(0, new CompositeKeyRoutingConvention());

			var oDataModel = BuildNuGetODataModel();

			var routeName = "nuget";
			var routeUrlRoot = routeName;

			conventions = conventions.Select(c => new ControllerAliasingODataRoutingConvention(c, "Packages", oDatacontrollerName))
				.Cast<IODataRoutingConvention>()
				.ToList();

			config.Routes.MapHttpRoute(
				name: routeName + "_upload",
				routeTemplate: routeUrlRoot + "/",
				defaults: new { controller = oDatacontrollerName, action = "UploadPackage" },
				constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Put) }
			);

			config.Routes.MapODataServiceRoute(
				routeName, 
				routeUrlRoot, 
				oDataModel, 
				new CountODataPathHandler(),
				conventions);
		}


		internal static Microsoft.Data.Edm.IEdmModel BuildNuGetODataModel()
		{
			var builder = new ODataConventionModelBuilder {
				DataServiceVersion = new Version(2, 0)
			};

			builder.MaxDataServiceVersion = builder.DataServiceVersion;

			var packagesCollection = builder.EntitySet<Package>("Packages");
			packagesCollection.EntityType.HasKey(pkg => pkg.Id);
			packagesCollection.EntityType.HasKey(pkg => pkg.Version);

			var downloadPackageAction = packagesCollection.EntityType.Action("Download");

			var searchAction = builder.Action("Search");
			searchAction.Parameter<string>("searchTerm");
			searchAction.Parameter<string>("targetFramework");
			searchAction.Parameter<bool>("includePrerelease");
			searchAction.ReturnsCollectionFromEntitySet(packagesCollection);

			var findPackagesAction = builder.Action("FindPackagesById");
			findPackagesAction.Parameter<string>("id");
			findPackagesAction.ReturnsCollectionFromEntitySet(packagesCollection);
						       							     
			var retValue = builder.GetEdmModel();
			retValue.SetHasDefaultStream(
				retValue.FindDeclaredType(typeof(Package).FullName) as Microsoft.Data.Edm.IEdmEntityType, 
				hasStream: true);

			return retValue;
		}
	}
}