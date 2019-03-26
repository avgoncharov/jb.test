namespace Jb.Test.DAL.Impl.Migrations
{
	using System.Data.Entity.Migrations;
	using DTOs;

	internal sealed class Configuration : DbMigrationsConfiguration<Jb.Test.DAL.Impl.PackageStore>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
		}

		protected override void Seed(Jb.Test.DAL.Impl.PackageStore context)
		{
			context.NugetUsers.AddOrUpdate(new NugetUser { ApiKey = "admin_1", Name = "Default Admin" });
		}
	}
}
