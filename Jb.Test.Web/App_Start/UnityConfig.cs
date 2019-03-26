using System.Web.Http;
using Jb.Test.DAL.Impl;
using Jb.Test.DAL.Interfaces;
using Unity;
using Unity.Lifetime;
using Unity.WebApi;

namespace Jb.Test.Web
{
	public static class UnityConfig
	{
		public static void RegisterComponents()
		{
			var container = new UnityContainer();

			container.RegisterType<IPacakgeStore, PackageStore>(new HierarchicalLifetimeManager());
			container.RegisterType<IPackagesRepository, PackagesRepository>(new HierarchicalLifetimeManager());
			
			GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
		}
	}
}