using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

/// <summary>
/// Copies the OneSignalConfig.plugin to Assets/Plugins/Android/*
/// </summary>
public class ExportAndroidResources : InstallStep
{
    public override string Summary
        => "Copy Android plugin to Assets";

    public override string Details
        => $"Will create a plugin directory of {_getExportPath()} filled with files necessary for the OneSignal SDK " +
           "to operate on Android.";

    public override string DocumentationLink
        => "";

    protected override bool _getIsStepCompleted()
    {
        var packagePath = _getPackagePath();
        var exportPath = _getExportPath();

        var packageFiles = Directory.GetFiles(packagePath, "*", SearchOption.AllDirectories);
        var exportFiles = Directory.GetFiles(exportPath, "*", SearchOption.AllDirectories);

        var fileDiff = packageFiles.Except(exportFiles);

        return fileDiff.Any();
    }

    protected override void _install()
    {
        var packagePath = _getPackagePath();
        var exportPath = _getExportPath();
        var files = Directory.GetFiles(packagePath, "*", SearchOption.AllDirectories);
        var filteredFiles = files.Where(file => !file.EndsWith(".meta"));

        foreach (var file in filteredFiles)
        {
            var trimmedPath = file.Remove(0, packagePath.Length + 1);
            var fileExportPath = Path.Combine(exportPath, trimmedPath);
            var containingPath = fileExportPath.Remove(fileExportPath.LastIndexOf(Path.DirectorySeparatorChar));

            /*
             * Export the file.
             * By default the CreateDirectory and Copy methods don't overwrite but we use the Exists
             * checks to avoid polluting the console with warnings.
             */

            if (!Directory.Exists(containingPath))
                Directory.CreateDirectory(containingPath);

            if (!File.Exists(fileExportPath))
                File.Copy(file, fileExportPath);
        }

        AssetDatabase.Refresh();
    }

    private const string _pluginName = "OneSignalConfig.plugin";
    private const string _androidPluginPackagePath = "Packages/com.onesignal.unity.android/Editor";
    private const string _androidPluginExportPath = "Assets/Plugins/Android";

    private static string _getPackagePath()
        => Path.GetFullPath(Path.Combine(_androidPluginPackagePath, _pluginName));

    private static string _getExportPath()
        => Path.GetFullPath(Path.Combine(_androidPluginExportPath, _pluginName));
}