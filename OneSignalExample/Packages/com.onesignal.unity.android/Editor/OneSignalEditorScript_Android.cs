/**
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
using Com.OneSignal.Editor;
using UnityEditor;

namespace Com.OneSignal.Android.Editor
{
    [InitializeOnLoad]
    public class OneSignalEditorScriptAndroid
    {
        static readonly string k_AndroidConfigFolder = $"Packages/{ScopeRegistriesConfig.OneSignalScope}.android/Plugins/Android/OneSignalConfig";
        static readonly string k_PackageManifestPath = $"Packages/{ScopeRegistriesConfig.OneSignalScope}.android/package.json";

        static OneSignalEditorScriptAndroid()
        {
            CreateOneSignalAndroidManifest();
            PackageManifestSanityCheck();
        }

        /// <summary>
        /// We can't add EDM4UP into package.json before making sure that google scope registry if available for the project
        /// The method will
        ///  * Add Google scope registry to the project manifest.json if necessary
        ///  * Will update or add configured version of `com.google.external-dependency-manager` dependency into package.json
        /// </summary>
        static void PackageManifestSanityCheck()
        {
            Manifest mainManifest = new Manifest();
            mainManifest.Fetch();

            var manifestUpdated = false;

            if (!mainManifest.IsRegistryPresent(ScopeRegistriesConfig.GoogleScopeRegistryUrl))
            {
                mainManifest.AddScopeRegistry(ScopeRegistriesConfig.GoogleScopeRegistry);
                manifestUpdated = true;
            }

            if (manifestUpdated)
                mainManifest.ApplyChanges();

            manifestUpdated = false;

            var manifest = new Manifest(k_PackageManifestPath);
            manifest.Fetch();

            if (!manifest.IsDependencyPresent(ScopeRegistriesConfig.EDM4UName))
            {
                manifest.AddDependency(ScopeRegistriesConfig.EDM4UName, ScopeRegistriesConfig.EDM4UVersion);
                manifestUpdated = true;
            }
            else
            {
                var edm4UPackageDependency = manifest.GetDependency(ScopeRegistriesConfig.EDM4UName);
                if (!edm4UPackageDependency.Version.Equals(ScopeRegistriesConfig.EDM4UVersion))
                {
                    edm4UPackageDependency.SetVersion(ScopeRegistriesConfig.EDM4UVersion);
                    manifestUpdated = true;
                }
            }

            if (manifestUpdated)
                manifest.ApplyChanges();
        }

        // Copies `AndroidManifestTemplate.xml` to `AndroidManifest.xml`
        // then replace `${manifestApplicationId}` with current packagename in the Unity settings.
        static void CreateOneSignalAndroidManifest()
        {
            var configFullPath = Path.GetFullPath($"{k_AndroidConfigFolder}");
            var manifestPath = Path.GetFullPath($"{configFullPath}{Path.DirectorySeparatorChar}AndroidManifest.xml");
            var manifestTemplatePath = Path.GetFullPath($"{configFullPath}{Path.DirectorySeparatorChar}AndroidManifestTemplate.xml");

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
}
