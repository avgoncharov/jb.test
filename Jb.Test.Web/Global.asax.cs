using System.Web.Http;

namespace Jb.Test.Web
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			LogConfig.Configure();
			UnityConfig.RegisterComponents();
			GlobalConfiguration.Configure(WebApiConfig.Register);	
		}
	}
}
