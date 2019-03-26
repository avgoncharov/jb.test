using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Jb.Test.DAL.Impl.DTOs;

namespace Jb.Test.DAL.Impl.Configurations
{
	internal sealed class PackageIdConfiguration : EntityTypeConfiguration<PackageId>
	{
		public PackageIdConfiguration()
		{
			ToTable("PackageIds");
			Property(itr => itr.Id)
				.HasMaxLength(800)
				.IsRequired()
				.HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
			HasKey(itr => itr.Id);
			HasIndex(itr => itr.Id).IsUnique(true);

			HasMany(itr => itr.Versions).WithRequired(itr => itr.PackageId);
			HasRequired(itr => itr.NugetUser).WithMany(itr => itr.PackageIds);
		}
	}
}