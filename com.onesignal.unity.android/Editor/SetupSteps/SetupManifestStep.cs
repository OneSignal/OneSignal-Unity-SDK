using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

/// <summary>
/// Makes sure that the OneSignalConfig.plugin folder exists and is populated with the correct files within the project
/// </summary>
public class SetupManifestStep : OneSignalSetupStep
{
    public override string Summary
        => "Setup keys in Android manifest";

    public override string Details
        => $"Replaces the {_keyInManifestToReplace} with your app's applicationIdentifier " +
           $"({PlayerSettings.applicationIdentifier}). If you intend to change this id then please re-run this step";

    public override string DocumentationLink
        => "";

    protected override bool _getIsStepCompleted()
    {
        if (!File.Exists(_manifestPath))
            return false;
        
        var reader = new StreamReader(_manifestPath);
        var contents = reader.ReadToEnd();
        reader.Close();
        
        return !contents.Contains(_keyInManifestToReplace);
    }

    protected override void _runStep()
    {
        var replacements = new Dictionary<string, string>
        {
            [_keyInManifestToReplace] = PlayerSettings.applicationIdentifier
        };

        // modifies the manifest in place
        _replaceStringsInFile(_manifestPath, _manifestPath, replacements);
    }
    
    private const string _keyInManifestToReplace = "{applicationId}";
    private const string _pluginName = "OneSignalConfig.plugin";
    private static readonly string _androidPluginsPath = Path.Combine("Assets", "Plugins", "Android");
    private static readonly string _manifestPath = Path.Combine(_androidPluginsPath, _pluginName, "AndroidManifest.xml");
    
    private static void _replaceStringsInFile(string sourcePath, string destinationPath,
        IReadOnlyDictionary<string, string> replacements)
    {
        try
        {
            if (!File.Exists(sourcePath))
            {
                Debug.LogError($"could not find {sourcePath}");
                return;
            }

            var reader = new StreamReader(sourcePath);
            var contents = reader.ReadToEnd();
            reader.Close();

            foreach (var replacement in replacements)
                contents = contents.Replace(replacement.Key, replacement.Value);

            var writer = new StreamWriter(destinationPath);
            writer.Write(contents);
            writer.Close();
        }
        catch (Exception exception)
        {
            Debug.LogError($"could not replace strings of {sourcePath} because:\n{exception.Message}");
        }
    }
}