using System.Data.Entity.ModelConfiguration;
using Jb.Test.DAL.Impl.DTOs;

namespace Jb.Test.DAL.Impl.Configurations
{
	internal sealed class PackagesVersionConfiguration : EntityTypeConfiguration<PackageVersion>
	{
		public PackagesVersionConfiguration()
		{
			ToTable("PackageVersions");
			Property(itr => itr.Id).HasMaxLength(800).IsRequired();
			Property(itr => itr.Version).HasMaxLength(800).IsRequired();
			HasKey(itr => new { itr.Id, itr.Version });
			HasIndex(itr => new { itr.Id, itr.Version }).IsUnique(true);
		}
	}
}
