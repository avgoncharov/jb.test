using System.Collections.Generic;
using Jb.Test.DAL.Impl.DTOs;

namespace Jb.Test.DAL.Impl.Extensions
{
	public static class PackageExtensions
	{
		static PackageExtensions()
		{
			AutoMapper.Mapper.Initialize(cfg =>
			{
				cfg.CreateMap<PackageVersion, Interfaces.Model.Package>();
				cfg.CreateMap<Interfaces.Model.Package, PackageVersion>();
			});
		}


		public static IEnumerable<Interfaces.Model.Package> ConvertToModels(this IEnumerable<PackageVersion> source)
		{
			return AutoMapper.Mapper.Map<IEnumerable<Interfaces.Model.Package>>(source);
		}


		public static Interfaces.Model.Package ConvertToModel(this PackageVersion source)
		{
			return AutoMapper.Mapper.Map<Interfaces.Model.Package>(source);
		}


		public static IEnumerable<PackageVersion> ConvertToDtos(this IEnumerable<Interfaces.Model.Package> source)
		{
			return AutoMapper.Mapper.Map<IEnumerable<PackageVersion>>(source);
		}


		public static PackageVersion ConvertToDto(this Interfaces.Model.Package source)
		{
			return AutoMapper.Mapper.Map<PackageVersion>(source);
		}
	}
}
