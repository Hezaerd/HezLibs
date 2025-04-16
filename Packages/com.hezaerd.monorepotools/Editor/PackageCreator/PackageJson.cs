using System;
using System.IO;
using UnityEngine;

namespace hezaerd.monorepotools.packagecreator
{
	[Serializable]
	public class PackageJson
	{
		public string name;
		public string version;
		public string displayName;
		public string description;
		public string unity;
		public AuthorInfo author;
		public string[] keywords;
		public string license;
		
		public static void CreatePackageJsonFile(string packageDirectory, PackageInfo packageInfo)
		{
			var packageJsonPath = Path.Combine(packageDirectory, "package.json");

			PackageJson packageJson = new PackageJson
			{
				name = packageInfo.packageName,
				version = packageInfo.version,
				displayName = packageInfo.displayName,
				description = packageInfo.description,
				unity = PackageInfo.GetUnityVersionStr(packageInfo.ltsVersion),
				author = packageInfo.author,
				keywords = new[] { "unity", "package" },
				license = "MIT"
			};

			var packageJsonContent = JsonUtility.ToJson(packageJson, true);
			File.WriteAllText(packageJsonPath, packageJsonContent);
		}
	}
}