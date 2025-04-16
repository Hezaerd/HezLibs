using System;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace hezaerd.monorepotools.packagecreator
{
    public class PackageCreatorEditor : EditorWindow
    {
        private const string PACKAGE_INFO_KEY = "PackageCreation.PackageInfo";

        private PackageInfo _packageInfo;

        [MenuItem("Tools/Package Creator")]
        public static void ShowWindow()
        {
            GetWindow<PackageCreatorEditor>("Package Creator");
        }

        private void OnEnable()
        {
            // Load saved PackageInfo from EditorPrefs
            var json = EditorPrefs.GetString(PACKAGE_INFO_KEY,
                JsonUtility.ToJson(new PackageInfo()));
            _packageInfo = JsonUtility.FromJson<PackageInfo>(json);
        }

        private void OnDisable()
        {
            // Save PackageInfo to EditorPrefs
            var json = JsonUtility.ToJson(_packageInfo);
            EditorPrefs.SetString(PACKAGE_INFO_KEY, json);
        }

        private void OnGUI()
        {
            GUILayout.Label("Package Information", EditorStyles.boldLabel);

            var previousPackageName = _packageInfo.packageName;
            _packageInfo.packageName = EditorGUILayout.TextField("Package Name",
                _packageInfo.packageName);

            using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                if (_packageInfo.packageName != previousPackageName)
                {
                    PackageInfo.ExtractAuthorAndDisplayName(_packageInfo,
                        _packageInfo.packageName);
                }
            }

            _packageInfo.displayName = EditorGUILayout.TextField("Display Name",
                _packageInfo.displayName);
            _packageInfo.version = EditorGUILayout.TextField("Version",
                _packageInfo.version);
            _packageInfo.description = EditorGUILayout.TextField("Description",
                _packageInfo.description);
            _packageInfo.rootNamespace = EditorGUILayout.TextField("Root Namespace",
                _packageInfo.rootNamespace);
            _packageInfo.ltsVersion = (LTSVersion)EditorGUILayout.EnumPopup("LTS Version",
                _packageInfo.ltsVersion);

            GUILayout.Label("Author Information", EditorStyles.boldLabel);

            _packageInfo.author.name = EditorGUILayout.TextField("Author Name",
                _packageInfo.author.name);
            _packageInfo.author.url = EditorGUILayout.TextField("Author URL",
                _packageInfo.author.url);

            if (GUILayout.Button("Create Package"))
            {
                var dialog = EditorUtility.DisplayDialog(
                    "Confirm Package Creation",
                    $"Create package '{_packageInfo.displayName}'?",
                    "Create",
                    "Cancel");

                if (!dialog)
                {
                    return;
                }

                CreatePackage();
            }
        }

        private void CreatePackage()
        {
            if (string.IsNullOrEmpty(_packageInfo.packageName) ||
                string.IsNullOrEmpty(_packageInfo.displayName))
            {
                EditorUtility.DisplayDialog("Error",
                    "Package Name and Display Name are required.",
                    "OK");
                return;
            }

            var packagesDirectory = Path.Combine(Application.dataPath, "../Packages");

            //Ensure the Packages directory exist
            if (!Directory.Exists(packagesDirectory))
            {
                Directory.CreateDirectory(packagesDirectory);
            }

            var packageDirectory = Path.Combine(packagesDirectory, _packageInfo.packageName);

            if (Directory.Exists(packageDirectory))
            {
                EditorUtility.DisplayDialog("Error",
                    "Package directory already exists.", "OK");
                return;
            }

            // Create the package directory
            Directory.CreateDirectory(packageDirectory);

            // Create the Editor and Runtime directories
            var editorDirectory = Path.Combine(packageDirectory, "Editor");
            Directory.CreateDirectory(editorDirectory);
            var runtimeDirectory = Path.Combine(packageDirectory, "Runtime");
            Directory.CreateDirectory(runtimeDirectory);

            // Create the Tests directory and subfolders
            var testsDirectory = Path.Combine(packageDirectory, "Tests");
            Directory.CreateDirectory(testsDirectory);
            var testsEditorDirectory = Path.Combine(testsDirectory, "Editor");
            Directory.CreateDirectory(testsEditorDirectory);
            var testsRuntimeDirectory = Path.Combine(testsDirectory, "Runtime");
            Directory.CreateDirectory(testsRuntimeDirectory);

            // Create the package.json file
            PackageJson.CreatePackageJsonFile(packageDirectory, _packageInfo);

            // Create assembly definition files
            CreateAssemblyDefinitions(packageDirectory);

            AssetDatabase.Refresh();
            CompilationPipeline.RequestScriptCompilation();

            Debug.Log("Package created successfully at: " + packageDirectory);
        }

        private void CreateAssemblyDefinitions(string packageDirectory)
        {
            var editorDirectory = Path.Combine(packageDirectory, "Editor");
            var runtimeDirectory = Path.Combine(packageDirectory, "Runtime");
            var testsDirectory = Path.Combine(packageDirectory, "Tests");
            var testsEditorDirectory = Path.Combine(testsDirectory, "Editor");
            var testsRuntimeDirectory = Path.Combine(testsDirectory, "Runtime");

            var runtimeAsmdefPath = Path.Combine(runtimeDirectory, "Runtime.asmdef");
            var editorAsmdefPath = Path.Combine(editorDirectory, "Editor.asmdef");
            var testsEditorAsmdefPath = Path.Combine(testsEditorDirectory,
                "Editor.asmdef");
            var testsRuntimeAsmdefPath = Path.Combine(testsRuntimeDirectory,
                "Runtime.asmdef");

            var runtimeAsmdefName = _packageInfo.rootNamespace; // Use the provided namespace
            var editorAsmdefName = _packageInfo.rootNamespace + ".Editor";
            var testsEditorAsmdefName = _packageInfo.rootNamespace + ".Tests.Editor";
            var testsRuntimeAsmdefName = _packageInfo.rootNamespace + ".Tests.Runtime";

            AssemblyDefinition.CreateAssemblyDefinitionFile(runtimeAsmdefPath,
                runtimeAsmdefName, Array.Empty<string>(), _packageInfo);
            AssemblyDefinition.CreateAssemblyDefinitionFile(editorAsmdefPath,
                editorAsmdefName, new[] { runtimeAsmdefName }, _packageInfo);
            AssemblyDefinition.CreateAssemblyDefinitionFile(testsEditorAsmdefPath,
                testsEditorAsmdefName, new[] { editorAsmdefName },
                _packageInfo);
            AssemblyDefinition.CreateAssemblyDefinitionFile(testsRuntimeAsmdefPath,
                testsRuntimeAsmdefName, new[] { runtimeAsmdefName },
                _packageInfo);
        }
    }
}
