using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;
using Jb.Test.DAL.Interfaces.Model;

namespace Jb.Test.NugetDataExtractor
{
	/// <inheritdoc />
	/// <summary>
	/// Класс реализации экстрактора.
	/// </summary>
	public sealed class DataExtractor : IDataExtractor
	{
		/// <inheritdoc cref="IDataExtractor"/>
		public Package ExtractPackage(byte[] data)
		{
			var str = ExtractRawMetadata(data);
			
			str = Regex.Replace(str, @"xmlns(:\w+)?=""([^""]+)""|xsi(:\w+)?=""([^""]+)""", "");

			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(str);

			var result= new Package();

			var idElem =  xmlDoc.SelectSingleNode("//metadata/id");
			if (idElem == null) throw new InvalidOperationException("Id can't be empty.");
			result.Id = idElem.InnerText;

			var idVersion = xmlDoc.SelectSingleNode("//metadata/version");
			if (idVersion == null) throw new InvalidOperationException("version can't be empty.");
			
			result.Version = idVersion.InnerText;

			var titleElem = xmlDoc.SelectSingleNode("//metadata/title");
			if (titleElem != null)
			{
				result.Title = titleElem.InnerText;
			}

			var descriptionElem = xmlDoc.SelectSingleNode("//metadata/description");
			if (descriptionElem != null)
			{
				result.Description = descriptionElem.InnerText;
			}

			var authorsElem = xmlDoc.SelectSingleNode("//metadata/authors");
			if (authorsElem != null)
			{
				result.Authors = authorsElem.InnerText;
			}				    

			return result;

		}

		/// <inheritdoc cref="IDataExtractor"/>
		public string ExtractRawMetadata(byte[] data)
		{
			if(data == null || data.Length == 0) 
				throw new ArgumentException("Data can't be empty.");

			using(var dataStream = new MemoryStream(data))
			using (var archive = new ZipArchive(dataStream, ZipArchiveMode.Read))
			{
				foreach (var entry in archive.Entries)
				{	
					if (entry.FullName.EndsWith(".nuspec"))
					{
						using (var stream = new StreamReader(entry.Open()))
						{
							return stream.ReadToEnd();
						}
					}
				}

				throw new InvalidOperationException("Bad data format.");
			}
		}	
	}
}
