using System.IO;
using Xunit;

namespace Jb.Test.NugetDataExtractor.Tests
{
	public class NugetDataExctractorTests
	{
		[Fact]
		public void ExtractAsStrTest()
		{
			var expected = File.ReadAllText("Microsoft.AspNet.WebApi.Client.nuspec");
			var extr = new DataExtractor();
			var data = File.ReadAllBytes("Microsoft.AspNet.WebApi.Client.5.2.4.nupkg");

			var str = extr.ExtractRawMetadata(data);

			Assert.Equal(expected, str);
		}

		[Fact]
		public void ExtractPackageTest()
		{
			var extr = new DataExtractor();
			var data = File.ReadAllBytes("Microsoft.AspNet.WebApi.Client.5.2.4.nupkg");

			var pkg = extr.ExtractPackage(data);
			Assert.Equal("Microsoft.AspNet.WebApi.Client", pkg.Id);
			Assert.Equal("5.2.4", pkg.Version);

		}
	}
}
