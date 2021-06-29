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

using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;


[InitializeOnLoad]
public class OneSignalEditorScriptAndroid
{
    const string k_Edm4UVersion = "1.2.165";

    static readonly string k_AndroidConfigFolder =
        $"Packages/com.onesignal.unity.android/Runtime/Plugins/Android/OneSignalConfig.plugin";

    static readonly string k_Edm4UPackageDownloadUrl =
        $"https://github.com/googlesamples/unity-jar-resolver/blob/v{k_Edm4UVersion}/external-dependency-manager-{k_Edm4UVersion}.unitypackage?raw=true";

    static OneSignalEditorScriptAndroid()
    {
        CreateOneSignalAndroidManifest();
        InstallEdm4U();
    }

    static bool IsEdm4UInstalled()
    {
        var precompiledAssemblies = CompilationPipeline.GetPrecompiledAssemblyNames();
        foreach (var assemblyName in precompiledAssemblies)
        {
            if (assemblyName.StartsWith("Google.VersionHandler"))
            {
                return true;
            }
        }

        return false;
    }

    static void InstallEdm4U()
    {
        // If Edm4U is already installed we would do nothing.
        if (IsEdm4UInstalled())
        {
            return;
        }

        var request = EditorWebRequest.Get(k_Edm4UPackageDownloadUrl);
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

    // Copies `AndroidManifestTemplate.xml` to `AndroidManifest.xml`
    // then replace `${manifestApplicationId}` with current packagename in the Unity settings.
    static void CreateOneSignalAndroidManifest()
    {
        var configFullPath = Path.GetFullPath($"{k_AndroidConfigFolder}");
        var manifestPath = Path.GetFullPath($"{configFullPath}{Path.DirectorySeparatorChar}AndroidManifest.xml");
        var manifestTemplatePath =
            Path.GetFullPath($"{configFullPath}{Path.DirectorySeparatorChar}AndroidManifestTemplate.xml");

        File.Copy(manifestTemplatePath, manifestPath, true);
        var streamReader = new StreamReader(manifestPath);
        var body = streamReader.ReadToEnd();
        streamReader.Close();
        body = body.Replace("${manifestApplicationId}", PlayerSettings.applicationIdentifier);
        using (var streamWriter = new StreamWriter(manifestPath, false))
        {
            streamWriter.Write(body);
        }
    }
}