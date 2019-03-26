using Jb.Test.ODataInfrastructura;

namespace Jb.Test.ODataODataInfrastructura
{
	public static class ClientCompatibilityFactory
	{
		public static ClientCompatibility FromProperties(string unparsedSemVerLevel)
		{
			if(string.IsNullOrWhiteSpace(unparsedSemVerLevel) ||
				SemanticVersion.TryParse(unparsedSemVerLevel, out var semVerLevel) != true)
			{
				return new ClientCompatibility(ClientCompatibility.Default.SemVerLevel);
			}

			if(semVerLevel == ClientCompatibility.Default.SemVerLevel)
			{
				return ClientCompatibility.Default;
			}

			if(semVerLevel == ClientCompatibility.Max.SemVerLevel)
			{
				return ClientCompatibility.Max;
			}

			return new ClientCompatibility(semVerLevel);
		}
	}
}