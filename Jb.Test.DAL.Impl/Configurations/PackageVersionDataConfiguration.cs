using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Jb.Test.DAL.Impl.DTOs;

namespace Jb.Test.DAL.Impl.Configurations
{
	public class PackageVersionDataConfiguration : EntityTypeConfiguration<PackageVersionData>
	{
		public PackageVersionDataConfiguration()
		{
			ToTable("PackageVersionDatas");
			Property(itr => itr.PackageVersionId).HasMaxLength(1600).IsRequired();
			Property(itr => itr.PackageVersionId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
			HasKey(itr => itr.PackageVersionId);
			Property(itr => itr.Data).IsRequired();				    				
		}
	}
}
