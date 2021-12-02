using System.IO;
using System.Linq;
using UnityEditor;

/// <summary>
/// Creates a unitypackage file for publishing
/// </summary>
public static class OneSignalPackagePublisher {
    public static void UpdateProjectVersion() {
        var packageVersion = File.ReadAllText(VersionFilePath);
        PlayerSettings.bundleVersion = packageVersion;
    }
    
    [MenuItem("OneSignal/ExportUnityPackage")]
    public static void ExportUnityPackage() {
        AssetDatabase.Refresh();
        var packageVersion = File.ReadAllText(VersionFilePath);
        var packageName = $"OneSignal-v{packageVersion}.unitypackage";

        AssetDatabase.ExportPackage(
            _filePaths(),
            packageName,
            ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies
        );
    }

    private static readonly string PackagePath = Path.Combine("Assets", "OneSignal");
    private static readonly string VersionFilePath = Path.Combine(PackagePath, "VERSION");
    
    private static readonly string[] Exclusions = {
        Path.Combine(PackagePath, "Attribution"),
        ".DS_Store"
    };

    private static string[] _filePaths() {
        var files = Directory.GetFileSystemEntries(PackagePath);
        var pathsToInclude = files.Where(file => {
            if (file.EndsWith(".meta"))
                file = file.Substring(0, file.Length - 5);

            return !Exclusions.Contains(file);
        });

        return pathsToInclude.ToArray();
    }
}