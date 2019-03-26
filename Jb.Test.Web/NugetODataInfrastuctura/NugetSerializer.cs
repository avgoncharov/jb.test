using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.OData.Routing;
using Jb.Test.DAL.Interfaces.Model;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Atom;

namespace Jb.Test.Web.ODataNugetODataInfrastuctura
{
	public class NugetSerializer : ODataEntityTypeSerializer
	{
		public NugetSerializer(ODataSerializerProvider serializerProvider)
		    : base(serializerProvider)
		{
		}


		public override ODataEntry CreateEntry(
			SelectExpandNode selectExpandNode,
			EntityInstanceContext entityInstanceContext)
		{
			var entry = base.CreateEntry(selectExpandNode, entityInstanceContext);

			TryAnnotateV2FeedPackage(entry, entityInstanceContext);

			return entry;
		}


		private void TryAnnotateV2FeedPackage(ODataEntry entry, EntityInstanceContext entityInstanceContext)
		{
			var instance = entityInstanceContext.EntityInstance as Package;

			if(instance == null)
				return;

			var atomEntryMetadata = new AtomEntryMetadata { Title = instance.Id };

			if(string.IsNullOrEmpty(instance.Authors) != true)
			{
				atomEntryMetadata.Authors = new[] { new AtomPersonMetadata { Name = instance.Authors } };
			}

			if(instance.LastUpdated > DateTime.MinValue)
			{
				atomEntryMetadata.Updated = instance.LastUpdated;
			}

			if(instance.Published > DateTime.MinValue)
			{
				atomEntryMetadata.Published = instance.Published;
			}

			if(!string.IsNullOrEmpty(instance.Summary))
			{
				atomEntryMetadata.Summary = instance.Summary;
			}

			entry.SetAnnotation(atomEntryMetadata);

			entry.Id = BuildId(instance, entityInstanceContext);
			entry.ReadLink = new Uri(entry.Id);
			entry.EditLink = entry.ReadLink;

			entry.MediaResource = new ODataStreamReferenceValue
			{
				ContentType = ContentType,
				ReadLink = BuildLinkForStreamProperty(instance, entityInstanceContext)
			};

			entry.Actions = entry
			    .Actions
			    .Select(action =>
				    StringComparer.OrdinalIgnoreCase.Equals("Download", action.Title) != true
					? action
					: new ODataAction
					{
						Metadata = action.Metadata,
						Target = entry.MediaResource.ReadLink,
						Title = action.Title
					}).ToList();
		}


		public string ContentType => "application/zip";


		private static string BuildId(Package package, EntityInstanceContext context)
		{
			return context.Url.CreateODataLink(GetPackagePathSegments(package));
		}


		private static Uri BuildLinkForStreamProperty(Package package, EntityInstanceContext context)
		{
			var segments = GetPackagePathSegments(package);
			segments.Add(new ActionPathSegment("Download"));

			return new Uri(context.Url.CreateODataLink(segments));
		}


		private static List<ODataPathSegment> GetPackagePathSegments(Package package)
		{
			return new List<ODataPathSegment> {
				new EntitySetPathSegment("Packages"),
				new KeyValuePathSegment($"Id='{package.Id}',Version='{RemoveVersionMetadata(package.Version)}'")
			};
		}


		private static string RemoveVersionMetadata(string version)
		{
			var plusIndex = version.IndexOf('+');
			return plusIndex >= 0
				? version.Substring(0, plusIndex)
				: version;
		}
	}
}