using System;
using System.Text.RegularExpressions;

namespace hezaerd.monorepotools.packagecreator
{
	[Serializable]
	public class PackageInfo
	{
		public string packageName = "com.yourname.newpackage";
		public string displayName = "New Package";
		public string version = "0.1.0";
		public string description = "A description of the new package.";
		public string rootNamespace = "YourNamespace";
		public AuthorInfo author = new AuthorInfo();
		public LTSVersion ltsVersion = LTSVersion.NONE;

		public static void ExtractAuthorAndDisplayName(PackageInfo packageInfo, string packageName)
		{
			// Attempt to extract author name and package name from packageName
			const string pattern = @"^com\.([a-zA-Z0-9_-]+)\.([a-zA-Z0-9_-]+)$";
			Match match = Regex.Match(packageName, pattern);
			if (match.Success)
			{
				packageInfo.author.name = ToProperCase(match.Groups[1].Value);
				packageInfo.displayName = ToProperCase(match.Groups[2].Value);
			}
		}

		private static string ToProperCase(string str)
		{
			return System.Globalization.CultureInfo.CurrentCulture.TextInfo
				.ToTitleCase(str);
		}

		public static bool ValidatePackageName(string packageName)
		{
			const string pattern = @"^com\.[a-z0-9_-]+\.[a-z0-9_-]+$";
			return Regex.IsMatch(packageName, pattern);
		}
	}
}