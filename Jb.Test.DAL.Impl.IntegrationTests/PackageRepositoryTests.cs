using System;
using System.Threading.Tasks;
using Xunit;
using Jb.Test.DAL.Interfaces.Model;
using System.Linq;
using AutoFixture.Xunit2;
using Jb.Test.DAL.Interfaces;
using System.Data.Entity;
using System.Collections.Generic;

namespace Jb.Test.DAL.Impl.IntegrationTests
{
	public sealed class PackageRepositoryTests : IDisposable
	{
		private const string ApiKey = "admin_1";
		private readonly IPacakgeStore _store;
		private readonly IPackagesRepository _repository;

		public PackageRepositoryTests()
		{
			_store = new PackageStore();
			_repository = new PackagesRepository(_store);
		}

		[Theory, AutoData]
		public async Task CreateTest(Package expected, byte[] data)
		{
			expected.Version = "1.0";

			var checkObj = await _store.PackageIds.FirstOrDefaultAsync(itr => itr.Id == expected.Id);
			Assert.Null(checkObj);

			var savingResult = await _repository.CreateOrUpdateAsync(expected, data, ApiKey);
			AssertAreEquals(expected, savingResult);

			var actual = (await _repository.FindByIdAsync(expected.Id, LoadingLevel.LatestVersion)).FirstOrDefault();

			Assert.NotNull(actual);
			AssertAreEquals(expected, actual);
		}


		[Theory, AutoData]
		public async Task FindByIdAsync(Package expected, byte[] data)
		{
			expected.Version = "1.0";

			//Полагаем что тест CreateTest и мы доверяем CreateOrUpdateAsync.
			await _repository.CreateOrUpdateAsync(expected, data, ApiKey);

			var actual = (await _repository.FindByIdAsync(expected.Id, LoadingLevel.LatestVersion)).FirstOrDefault();

			Assert.NotNull(actual);
			AssertAreEquals(expected, actual);
		}


		[Theory, AutoData]
		public async Task FindByIdAllVersionsTest(
			Package expected, 
			Package expectedV2, 
			Package other, 
			byte[] data, 
			byte[] dataV2)
		{
			other.Version = "32";

			const string Id = "ABC";
			expected.Id = expectedV2.Id = Id;
			expected.Version = "1.0";
			expectedV2.Version = "2.0";
			
			var expectedVersions = new Dictionary<string, Package> {
				[expected.Version] = expected,
				[expectedV2.Version] = expectedV2
			};
						
			await _repository.CreateOrUpdateAsync(expected, data, ApiKey);
			await _repository.CreateOrUpdateAsync(expectedV2, dataV2, ApiKey);
			await _repository.CreateOrUpdateAsync(other, data, ApiKey);

			var actual =( await _repository.FindByIdAsync(expected.Id, LoadingLevel.AllVersion)).ToArray();

			Assert.NotEmpty(actual);
			Assert.True(actual.Length == 2);

			AssertСompleteOverlapping(expectedVersions, actual);			
		}
				

		[Theory, AutoData]
		public async Task GetByFilterTest(
			Package expected,
			Package expectedV2,
			Package other,
			byte[] data,
			byte[] dataV2)
		{
			other.Version = "32";

			const string Id = "ABC";
			expected.Id = expectedV2.Id = Id;     
			expected.Description = "This is test";
			expected.Version = "1.0";
			expectedV2.Version = "2.0";

			var expectedVersions = new Dictionary<string, Package>
			{
				[expected.Version] = expected,
				[expectedV2.Version] = expectedV2
			};

			await _repository.CreateOrUpdateAsync(expected, data, ApiKey);
			await _repository.CreateOrUpdateAsync(expectedV2, dataV2, ApiKey);
			await _repository.CreateOrUpdateAsync(other, data, ApiKey);

			var actual = (await _repository.GetAllByFilterAsync(new Filter { IdPattern= Id})).ToArray();
			Assert.NotEmpty(actual);
			Assert.True(actual.Length == expectedVersions.Count);
			AssertСompleteOverlapping(expectedVersions, actual);
			
			actual = (await _repository.GetAllByFilterAsync(new Filter { DescriptionPattern = expected.Description})).ToArray();
			Assert.NotEmpty(actual);
			Assert.True(actual.Length == 1);
					       
			AssertAreEquals(expected, actual[0]);

			actual = (await _repository.GetAllByFilterAsync(new Filter { IdPattern = other.Id, DescriptionPattern = expected.Description })).ToArray();
			Assert.Empty(actual);
		}


		[Theory, AutoData]
		public async Task GetByFilterWithNullAndEmtyFilterTest(
			Package expected,
			Package expectedV2,
			Package other,
			byte[] data,
			byte[] dataV2)
		{
			other.Version = "32";

			const string Id = "ABC";
			expected.Id = expectedV2.Id = Id;
			expected.Version = "1.0";
			expectedV2.Version = "2.0";

			var expectedPackages = new Dictionary<string, Package>
			{
				[expectedV2.Id] = expectedV2,
				[other.Id] = other
			};

			await _repository.CreateOrUpdateAsync(expected, data, ApiKey);
			await _repository.CreateOrUpdateAsync(expectedV2, dataV2, ApiKey);
			await _repository.CreateOrUpdateAsync(other, data, ApiKey);

			var actual = (await _repository.GetAllByFilterAsync(null)).ToArray();

			AssertOnlyLastAndDifferent(expectedPackages, actual);

			actual = (await _repository.GetAllByFilterAsync(new Filter())).ToArray();

			AssertOnlyLastAndDifferent(expectedPackages, actual);
		}

				
		[Theory, AutoData]
		public async Task GetVersionDataTest(Package expected, byte[] data)
		{
			expected.Version = "1.0";

			await _repository.CreateOrUpdateAsync(expected, data, ApiKey);

			var actual = await _repository.GetDataByIdAndVersionAsync(expected.Id, expected.Version);

			AssertAreEquals(data, actual);
		}


		[Theory, AutoData]
		public async Task AddNewVersionTest(Package expected, Package expectedV2, byte[] data, byte[] dataV2)
		{
			//Полагаем что GetVersionDataTest и FindByIdAsync успешны.
			const string Id = "ABC";
			expected.Id = expectedV2.Id = Id;
			expected.Version = "1.0";
			expectedV2.Version = "2.0";

			await _repository.CreateOrUpdateAsync(expected, data, ApiKey);
			var actual = (await _repository.FindByIdAsync(Id, LoadingLevel.LatestVersion)).FirstOrDefault();
			var actualData = await _repository.GetDataByIdAndVersionAsync(Id, expected.Version);

			Assert.NotNull(actual);
			AssertAreEquals(expected, actual);
			Assert.True(actual.IsLatestVersion);
			AssertAreEquals(data, actualData);

			await _repository.CreateOrUpdateAsync(expectedV2, dataV2, ApiKey);

			actual = (await _repository.FindByIdAsync(Id, LoadingLevel.LatestVersion)).FirstOrDefault();
			actualData = await _repository.GetDataByIdAndVersionAsync(Id, expectedV2.Version);
			
			Assert.NotNull(actual);
			AssertAreEquals(expectedV2, actual);
			Assert.True(actual.IsLatestVersion);
			AssertAreEquals(dataV2, actualData);

			AssertAreNotEquals(data, dataV2);
		}

		       				        
		[Theory, AutoData]
		public async Task UpdateVersionDataTest(Package expected, byte[] data, byte[] dataV2)
		{
			expected.Version = "1.0";

			await _repository.CreateOrUpdateAsync(expected, data, ApiKey);
			var actual = (await _repository.FindByIdAsync(expected.Id, LoadingLevel.LatestVersion)).FirstOrDefault();

			Assert.NotNull(actual);
			AssertAreEquals(expected, actual);
			Assert.True(actual.IsLatestVersion);

			var actualData = await _repository.GetDataByIdAndVersionAsync(expected.Id, expected.Version);
			AssertAreEquals(data, actualData);
			
			
			await _repository.CreateOrUpdateAsync(expected, dataV2, ApiKey);

			actual = (await _repository.FindByIdAsync(expected.Id, LoadingLevel.LatestVersion)).FirstOrDefault();
			
			Assert.NotNull(actual);
			AssertAreEquals(expected, actual);
			Assert.True(actual.IsLatestVersion);

			actualData = await _repository.GetDataByIdAndVersionAsync(expected.Id, expected.Version);
			AssertAreEquals(dataV2, actualData);
			AssertAreNotEquals(data, dataV2);
		}


		public void Dispose()
		{
			_store.PackageIds.RemoveRange(_store.PackageIds);
			_store.PackageVersions.RemoveRange(_store.PackageVersions);
			_store.PackageVersionDatas.RemoveRange(_store.PackageVersionDatas);
			_store.Save();

			_store.Dispose();
		}


		private static void AssertAreEquals(Package expected, Package actual)
		{
			Assert.Equal(expected.Id, actual.Id);
			Assert.Equal(expected.Title, actual.Title);
			Assert.Equal(expected.Version, actual.Version);
			Assert.Equal(expected.Description, actual.Description);
		}
			
				 
		private static void AssertAreEquals(byte[] expected, byte[] actual)
		{ 
			Assert.True(AreEquals(expected, actual));
		}


		private static void AssertAreNotEquals(byte[] expected, byte[] actual)
		{
			Assert.False(AreEquals(expected, actual));
		}


		private static bool AreEquals(byte[] expected, byte[] actual)
		{
			if((expected == null && actual != null) || (actual == null && expected != null))
				return false;

			if(expected.Length != actual.Length)
				return false;

			for(int i = 0; i < expected.Length; ++i)
			{
				if(expected[i] != actual[i])
					return false;
			}

			return true;
		}


		private void AssertСompleteOverlapping(Dictionary<string, Package> expectedVersions, Package[] actual)
		{
			var currentV = "zzz";

			foreach(var itr in actual)
			{
				if(expectedVersions.ContainsKey(itr.Version) != true)
					Assert.True(false);
								
				AssertAreEquals(expectedVersions[itr.Version], itr);
				Assert.NotEqual(currentV, itr.Version);
			}
		}


		private void AssertOnlyLastAndDifferent(Dictionary<string, Package> expectedPackages, Package[] actual)
		{
			Assert.NotEmpty(actual);
			Assert.True(actual.Length == 2);

			foreach(var itr in actual)
			{
				if(expectedPackages.ContainsKey(itr.Id) != true)
					Assert.True(false);

				AssertAreEquals(expectedPackages[itr.Id], itr);
				Assert.True(itr.IsLatestVersion);
			}
		}
	}
}
