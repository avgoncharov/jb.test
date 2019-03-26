using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Jb.Test.DAL.Impl.DTOs;

namespace Jb.Test.DAL.Impl.Configurations
{
	public sealed class NugetUserConfiguration : EntityTypeConfiguration<NugetUser>
	{
		public NugetUserConfiguration()
		{
			ToTable("NugetUser");
			Property(itr => itr.ApiKey)
				.HasMaxLength(400)
				.IsRequired()
				.HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
			HasKey(itr => itr.ApiKey);
			HasIndex(itr => itr.ApiKey).IsUnique(true);
		}
	}
}
