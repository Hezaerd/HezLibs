using System;
using UnityEngine;

namespace hezaerd.monorepotools.packagecreator
{
	[Serializable]
	public class AssemblyDefinition
	{
		public string name;
		public string rootNamespace;
		public string[] references;
		public string[] includePlatforms;
		public string[] excludePlatforms;
		public bool allowUnsafeCode;
		public bool overrideReferences;
		public string[] precompiledReferences;
		public bool autoReferenced;
		public string[] defineConstraints;
		public string[] versionDefines;
		public bool noEngineReferences;

		public static void CreateAssemblyDefinitionFile(string path, string assemblyName, 
			string[] referenceNames, PackageInfo packageInfo)
		{
			AssemblyDefinition asmdef = new AssemblyDefinition
			{
				name = assemblyName,
				rootNamespace = packageInfo.rootNamespace,
				references = referenceNames,
				includePlatforms = Array.Empty<string>(),
				excludePlatforms = Array.Empty<string>(),
				allowUnsafeCode = false,
				overrideReferences = false,
				precompiledReferences = Array.Empty<string>(),
				autoReferenced = true,
				defineConstraints = Array.Empty<string>(),
				versionDefines = Array.Empty<string>(),
				noEngineReferences = false
			};

			var asmdefContent = JsonUtility.ToJson(asmdef, true);
			System.IO.File.WriteAllText(path, asmdefContent);
		}
	}
}