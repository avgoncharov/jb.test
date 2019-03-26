using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Jb.Test.ODataInfrastructura
{
	public class ClientCompatibility
	{
		public static readonly ClientCompatibility Max = new ClientCompatibility(
			semVerLevel: new SemanticVersion("2.0.0"));


		public static readonly ClientCompatibility Default = new ClientCompatibility(
			semVerLevel: new SemanticVersion("1.0.0"));


		public ClientCompatibility(SemanticVersion semVerLevel)
		{
			SemVerLevel = semVerLevel ?? throw new ArgumentNullException(nameof(semVerLevel));
			AllowSemVer2 = semVerLevel.Version.Major >= 2;
		}


		public SemanticVersion SemVerLevel { get; }

		public bool AllowSemVer2 { get; }
	}


	[TypeConverter(typeof(SemanticVersionTypeConverter))]
	[Serializable]
	public sealed class SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
	{
		private static readonly Regex _semanticVersionRegex = new Regex("^(?<Version>\\d+(\\s*\\.\\s*\\d+){0,3})(?<Release>-([0]\\b|[0]$|[0][0-9]*[A-Za-z-]+|[1-9A-Za-z-][0-9A-Za-z-]*)+(\\.([0]\\b|[0]$|[0][0-9]*[A-Za-z-]+|[1-9A-Za-z-][0-9A-Za-z-]*)+)*)?(?<Metadata>\\+[0-9A-Za-z-]+(\\.[0-9A-Za-z-]+)*)?$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		private static readonly Regex _strictSemanticVersionRegex = new Regex("^(?<Version>([0-9]|[1-9][0-9]*)(\\.([0-9]|[1-9][0-9]*)){2})(?<Release>-([0]\\b|[0]$|[0][0-9]*[A-Za-z-]+|[1-9A-Za-z-][0-9A-Za-z-]*)+(\\.([0]\\b|[0]$|[0][0-9]*[A-Za-z-]+|[1-9A-Za-z-][0-9A-Za-z-]*)+)*)?(?<Metadata>\\+[0-9A-Za-z-]+(\\.[0-9A-Za-z-]+)*)?$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		private const RegexOptions _flags = RegexOptions.ExplicitCapture | RegexOptions.Compiled;
		private readonly string _originalString;
		private string _normalizedVersionString;

		public SemanticVersion(string version)
		  : this(SemanticVersion.Parse(version))
		{
			this._originalString = version;
		}

		public SemanticVersion(int major, int minor, int build, int revision)
		  : this(new Version(major, minor, build, revision))
		{
		}

		public SemanticVersion(int major, int minor, int build, string specialVersion)
		  : this(new Version(major, minor, build), specialVersion)
		{
		}

		public SemanticVersion(
		  int major,
		  int minor,
		  int build,
		  string specialVersion,
		  string metadata)
		  : this(new Version(major, minor, build), specialVersion, metadata)
		{
		}

		public SemanticVersion(Version version)
		  : this(version, string.Empty)
		{
		}

		public SemanticVersion(Version version, string specialVersion)
		  : this(version, specialVersion, (string) null, (string) null)
		{
		}

		public SemanticVersion(Version version, string specialVersion, string metadata)
		  : this(version, specialVersion, metadata, (string) null)
		{
		}

		private SemanticVersion(
		  Version version,
		  string specialVersion,
		  string metadata,
		  string originalString)
		{
			if(version == (Version) null)
				throw new ArgumentNullException(nameof(version));
			this.Version = SemanticVersion.NormalizeVersionValue(version);
			this.SpecialVersion = specialVersion ?? string.Empty;
			this.Metadata = metadata;
			this._originalString = string.IsNullOrEmpty(originalString) ? version.ToString() + (!string.IsNullOrEmpty(specialVersion) ? 45.ToString() + specialVersion : (string) null) + (!string.IsNullOrEmpty(metadata) ? 43.ToString() + metadata : (string) null) : originalString;
		}

		internal SemanticVersion(SemanticVersion semVer)
		{
			this._originalString = semVer.ToOriginalString();
			this.Version = semVer.Version;
			this.SpecialVersion = semVer.SpecialVersion;
			this.Metadata = semVer.Metadata;
		}

		public Version Version { get; private set; }

		public string SpecialVersion { get; private set; }

		public string Metadata { get; private set; }

		public string[] GetOriginalVersionComponents()
		{
			if(string.IsNullOrEmpty(this._originalString))
				return SemanticVersion.SplitAndPadVersionString(this.Version.ToString());
			int length = this._originalString.IndexOfAny(new char[2]
			{
	'-',
	'+'
			});
			return SemanticVersion.SplitAndPadVersionString(length == -1 ? this._originalString : this._originalString.Substring(0, length));
		}

		private static string[] SplitAndPadVersionString(string version)
		{
			string[] strArray1 = version.Split('.');
			if(strArray1.Length == 4)
				return strArray1;
			string[] strArray2 = new string[4]
			{
	"0",
	"0",
	"0",
	"0"
			};
			Array.Copy((Array) strArray1, 0, (Array) strArray2, 0, strArray1.Length);
			return strArray2;
		}

		public static SemanticVersion Parse(string version)
		{
			if(string.IsNullOrEmpty(version))
				throw new ArgumentException("Property may not be used in every assembly it is imported into", nameof(version));
			SemanticVersion semanticVersion;
			if(!SemanticVersion.TryParse(version, out semanticVersion))
				throw new ArgumentException("xz", nameof(version));
			return semanticVersion;
		}

		public static bool TryParse(string version, out SemanticVersion value)
		{
			return SemanticVersion.TryParseInternal(version, SemanticVersion._semanticVersionRegex, out value);
		}

		public static bool TryParseStrict(string version, out SemanticVersion value)
		{
			return SemanticVersion.TryParseInternal(version, SemanticVersion._strictSemanticVersionRegex, out value);
		}

		private static bool TryParseInternal(string version, Regex regex, out SemanticVersion semVer)
		{
			semVer = (SemanticVersion) null;
			if(string.IsNullOrEmpty(version))
				return false;
			Match match = regex.Match(version.Trim());
			Version result;
			if(!match.Success || !Version.TryParse(match.Groups["Version"].Value, out result))
				return false;
			semVer = new SemanticVersion(SemanticVersion.NormalizeVersionValue(result), SemanticVersion.RemoveLeadingChar(match.Groups["Release"].Value), SemanticVersion.RemoveLeadingChar(match.Groups["Metadata"].Value), version.Replace(" ", ""));
			return true;
		}

		private static string RemoveLeadingChar(string s)
		{
			if(s != null && s.Length > 0)
				return s.Substring(1, s.Length - 1);
			return s;
		}

		public static SemanticVersion ParseOptionalVersion(string version)
		{
			SemanticVersion semanticVersion;
			SemanticVersion.TryParse(version, out semanticVersion);
			return semanticVersion;
		}

		private static Version NormalizeVersionValue(Version version)
		{
			return new Version(version.Major, version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0));
		}

		public int CompareTo(object obj)
		{
			if(object.ReferenceEquals(obj, (object) null))
				return 1;
			SemanticVersion other = obj as SemanticVersion;
			if(other == (SemanticVersion) null)
				throw new ArgumentException("TypeMustBeASemanticVersion", nameof(obj));
			return this.CompareTo(other);
		}

		public int CompareTo(SemanticVersion other)
		{
			if(object.ReferenceEquals((object) other, (object) null))
				return 1;
			int num = this.Version.CompareTo(other.Version);
			if(num != 0)
				return num;
			bool flag1 = string.IsNullOrEmpty(this.SpecialVersion);
			bool flag2 = string.IsNullOrEmpty(other.SpecialVersion);
			if(flag1 && flag2)
				return 0;
			if(flag1)
				return 1;
			if(flag2)
				return -1;
			return SemanticVersion.CompareReleaseLabels((IEnumerable<string>) this.SpecialVersion.Split('.'), (IEnumerable<string>) other.SpecialVersion.Split('.'));
		}

		public static bool operator ==(SemanticVersion version1, SemanticVersion version2)
		{
			if(object.ReferenceEquals((object) version1, (object) null))
				return object.ReferenceEquals((object) version2, (object) null);
			return version1.Equals(version2);
		}

		public static bool operator !=(SemanticVersion version1, SemanticVersion version2)
		{
			return !(version1 == version2);
		}

		public static bool operator <(SemanticVersion version1, SemanticVersion version2)
		{
			if(version1 == (SemanticVersion) null)
				throw new ArgumentNullException(nameof(version1));
			return version1.CompareTo(version2) < 0;
		}

		public static bool operator <=(SemanticVersion version1, SemanticVersion version2)
		{
			if(!(version1 == version2))
				return version1 < version2;
			return true;
		}

		public static bool operator >(SemanticVersion version1, SemanticVersion version2)
		{
			if(version1 == (SemanticVersion) null)
				throw new ArgumentNullException(nameof(version1));
			return version2 < version1;
		}

		public static bool operator >=(SemanticVersion version1, SemanticVersion version2)
		{
			if(!(version1 == version2))
				return version1 > version2;
			return true;
		}

		public override string ToString()
		{
			if(this.IsSemVer2())
				return this.ToNormalizedString();
			int length = this._originalString.IndexOf('+');
			if(length > -1)
				return this._originalString.Substring(0, length);
			return this._originalString;
		}

		public string ToNormalizedString()
		{
			if(this._normalizedVersionString == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(this.Version.Major).Append('.').Append(this.Version.Minor).Append('.').Append(Math.Max(0, this.Version.Build));
				if(this.Version.Revision > 0)
					stringBuilder.Append('.').Append(this.Version.Revision);
				if(!string.IsNullOrEmpty(this.SpecialVersion))
					stringBuilder.Append('-').Append(this.SpecialVersion);
				this._normalizedVersionString = stringBuilder.ToString();
			}
			return this._normalizedVersionString;
		}

		public string ToFullString()
		{
			string str = this.ToNormalizedString();
			if(!string.IsNullOrEmpty(this.Metadata))
				str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}+{1}", (object) str, (object) this.Metadata);
			return str;
		}

		public string ToOriginalString()
		{
			return this._originalString;
		}

		public bool IsSemVer2()
		{
			if(!string.IsNullOrEmpty(this.Metadata))
				return true;
			if(!string.IsNullOrEmpty(this.SpecialVersion))
				return this.SpecialVersion.Contains(".");
			return false;
		}

		public bool Equals(SemanticVersion other)
		{
			if(!object.ReferenceEquals((object) null, (object) other) && this.Version.Equals(other.Version))
				return this.SpecialVersion.Equals(other.SpecialVersion, StringComparison.OrdinalIgnoreCase);
			return false;
		}

		public override bool Equals(object obj)
		{
			SemanticVersion other = obj as SemanticVersion;
			if(!object.ReferenceEquals((object) null, (object) other))
				return this.Equals(other);
			return false;
		}

		public override int GetHashCode()
		{
			int num = this.Version.GetHashCode();
			if(this.SpecialVersion != null)
				num = num * 4567 + this.SpecialVersion.GetHashCode();
			return num;
		}

		private static int CompareReleaseLabels(
		  IEnumerable<string> version1,
		  IEnumerable<string> version2)
		{
			int num = 0;
			IEnumerator<string> enumerator1 = version1.GetEnumerator();
			IEnumerator<string> enumerator2 = version2.GetEnumerator();
			bool flag1 = enumerator1.MoveNext();
			for(bool flag2 = enumerator2.MoveNext(); flag1 || flag2; flag2 = enumerator2.MoveNext())
			{
				if(!flag1 && flag2)
					return -1;
				if(flag1 && !flag2)
					return 1;
				num = SemanticVersion.CompareRelease(enumerator1.Current, enumerator2.Current);
				if(num != 0)
					return num;
				flag1 = enumerator1.MoveNext();
			}
			return num;
		}

		private static int CompareRelease(string version1, string version2)
		{
			int result1 = 0;
			int result2 = 0;
			bool flag1 = int.TryParse(version1, out result1);
			bool flag2 = int.TryParse(version2, out result2);
			return !flag1 || !flag2 ? (flag1 || flag2 ? (!flag1 ? 1 : -1) : StringComparer.OrdinalIgnoreCase.Compare(version1, version2)) : result1.CompareTo(result2);
		}
	}

	public class SemanticVersionTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public override object ConvertFrom(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value)
		{
			string version = value as string;
			SemanticVersion semanticVersion;
			if(version != null && SemanticVersion.TryParse(version, out semanticVersion))
				return (object) semanticVersion;
			return (object) null;
		}
	}
}