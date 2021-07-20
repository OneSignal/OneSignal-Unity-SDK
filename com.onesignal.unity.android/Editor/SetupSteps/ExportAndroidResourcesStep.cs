using System;
using System.IO;
using System.Linq;
using UnityEditor;

/// <summary>
/// Copies the OneSignalConfig.plugin to Assets/Plugins/Android/*
/// </summary>
public sealed class ExportAndroidResourcesStep : OneSignalSetupStep
{
    public override string Summary
        => "Copy Android plugin to Assets";

    public override string Details
        => $"Will create a plugin directory of {_pluginExportPath} filled with files necessary for the OneSignal SDK " +
           "to operate on Android.";

    public override bool IsRequired 
        => true;

    protected override bool _getIsStepCompleted()
    {
        if (!Directory.Exists(_pluginExportPath))
            return false;

        var packagePaths = Directory.GetFiles(_pluginPackagePath, "*", SearchOption.AllDirectories)
            .Select(path => path.Remove(0, path.LastIndexOf(_pluginName, StringComparison.InvariantCulture)));
        
        var exportPaths = Directory.GetFiles(_pluginExportPath, "*", SearchOption.AllDirectories)
            .Select(path => path.Remove(0, path.LastIndexOf(_pluginName, StringComparison.InvariantCulture)));

        var fileDiff = packagePaths.Except(exportPaths);
        return !fileDiff.Any();
    }

    protected override void _runStep()
    {
        var files = Directory.GetFiles(_pluginPackagePath, "*", SearchOption.AllDirectories);
        var filteredFiles = files.Where(file => !file.EndsWith(".meta"));

        foreach (var file in filteredFiles)
        {
            var trimmedPath = file.Remove(0, _pluginPackagePath.Length + 1);
            var fileExportPath = Path.Combine(_pluginExportPath, trimmedPath);
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
    private static readonly string _packagePath = Path.Combine("Packages", "com.onesignal.unity.android", "Editor");
    private static readonly string _androidPluginsPath = Path.Combine("Assets", "Plugins", "Android");
    private static readonly string _pluginPackagePath = Path.Combine(_packagePath, _pluginName);
    private static readonly string _pluginExportPath = Path.Combine(_androidPluginsPath, _pluginName);
}