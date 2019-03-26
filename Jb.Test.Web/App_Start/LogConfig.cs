using Serilog;
using System.IO;

namespace Jb.Test.Web
{
	public static class LogConfig
	{
		public static void Configure()
		{
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.Enrich
				.FromLogContext()
				.WriteTo.RollingFile(
					pathFormat: Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/logs") ?? "logs", "log-{Date}.log"),
					outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}[{Level:u3}] {Message}{NewLine}{Exception}"
				)
				.CreateLogger();
		}
	}
}