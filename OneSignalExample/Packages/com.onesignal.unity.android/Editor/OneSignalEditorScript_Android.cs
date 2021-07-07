/*
 * Modified MIT License
 *
 * Copyright 2018 OneSignal
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * 1. The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * 2. All copies of substantial portions of the Software may only be used in connection
 * with services provided by OneSignal.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

/// <summary>
/// Handles installation of remaining dependencies and resources required by the android package
/// </summary>
[InitializeOnLoad]
public class OneSignalEditorScriptAndroid
{
    /// <remarks>
    /// will attempt to perform operation on load of the Editor
    /// </remarks>
    static OneSignalEditorScriptAndroid()
    {
        InstallAndroidDependencies();
    }

    [MenuItem("OneSignal/Install Android Dependencies")]
    public static void InstallAndroidDependencies()
    {
        _installEdm4U();
        _exportResources();
        _replaceApplicationIdInManifest();
    }
    
    private const string _edm4UVersion = "1.2.165";
    private const string _keyInManifestToReplace = "{applicationId}";
    private const string _androidPluginPackagePath = "Packages/com.onesignal.unity.android/Editor/OneSignalConfig.plugin";
    private const string _androidPluginExportPath = "Assets/Plugins/Android/OneSignalConfig.plugin";

    static readonly string _edm4UPackageDownloadUrl = $"https://github.com/googlesamples/unity-jar-resolver/blob/v{_edm4UVersion}/external-dependency-manager-{_edm4UVersion}.unitypackage?raw=true";

    private static bool _isEdm4UInstalled()
    {
        var precompiledAssemblies = CompilationPipeline.GetPrecompiledAssemblyNames();
        foreach (var assemblyName in precompiledAssemblies)
        {
            if (assemblyName.StartsWith("Google.VersionHandler"))
                return true;
        }

        return false;
    }

    private static void _installEdm4U()
    {
        // If Edm4U is already installed we would do nothing.
        if (_isEdm4UInstalled())
            return;

        var request = EditorWebRequest.Get(_edm4UPackageDownloadUrl);
        request.AddEditorProgressDialog("Downloading Google External Dependency Manager");
        request.Send(unityRequest =>
        {
            if (unityRequest.error != null)
            {
                EditorUtility.DisplayDialog("Package Download failed.", unityRequest.error, "Ok");
                return;
            }

            //Asset folder name remove
            var projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
            var tmpPackageFile = projectPath + FileUtil.GetUniqueTempPathInProject() + ".unityPackage";

            File.WriteAllBytes(tmpPackageFile, unityRequest.downloadHandler.data);

            AssetDatabase.ImportPackage(tmpPackageFile, false);
        });
    }

    private static void _exportResources()
    {
        var packagePath = Path.GetFullPath($"{_androidPluginPackagePath}");
        var exportPath = Path.GetFullPath($"{_androidPluginExportPath}");
        var paths = Directory.GetFiles(packagePath, "*", SearchOption.AllDirectories);
        var filteredPaths = paths.Where(path => !path.EndsWith(".meta"));

        foreach (var path in filteredPaths)
        {
            var trimmedPath = path.Remove(0, packagePath.Length + 1);
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
                File.Copy(path, fileExportPath);
        }

        AssetDatabase.Refresh();
    }

    private static void _replaceApplicationIdInManifest()
    {
        var exportPath = Path.GetFullPath($"{_androidPluginExportPath}");
        var manifestPath = Path.GetFullPath($"{exportPath}{Path.DirectorySeparatorChar}AndroidManifest.xml");
        var replacements = new Dictionary<string, string>
        {
            [_keyInManifestToReplace] = PlayerSettings.applicationIdentifier
        };

        // modifies the manifest in place
        _replaceStringsInFile(manifestPath, manifestPath, replacements);
    }

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