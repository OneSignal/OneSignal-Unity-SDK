using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Creates a unitypackage file for publishing
/// </summary>
public static class OneSignalPackagePublisher
{
    public static void UpdateProjectVersion()
    {
        var packageVersion = File.ReadAllText(_versionFilePath);
        PlayerSettings.bundleVersion = packageVersion;
    }
    
    public static void ExportUnityPackage()
    {
        AssetDatabase.Refresh();
        var packageVersion = File.ReadAllText(_versionFilePath);
        var packageName = $"OneSignal-v{packageVersion}.unitypackage";

        AssetDatabase.ExportPackage(
            _filesPath,
            packageName,
            ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies
        );
    }

    private const string _versionFilePath = "Assets/OneSignal/VERSION";
    private const string _filesPath = "Assets/OneSignal";
}