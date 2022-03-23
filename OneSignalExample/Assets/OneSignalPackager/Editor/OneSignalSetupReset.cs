/*
 * Modified MIT License
 *
 * Copyright 2022 OneSignal
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

namespace OneSignalSDK {
    /// <summary>
    /// For debugging all of the OneSignalSetupSteps
    /// </summary>
    public static class OneSignalSetupReset {
        /// <summary>
        /// Resets all setup steps
        /// </summary>
        [MenuItem("OneSignal/Reset All Setup Steps", false, 100)]
        public static void ResetAllSteps() {
            /*
             * ExportAndroidResourcesStep
             * deletes the OneSignalConfig.plugin directory
             */
            AssetDatabase.DeleteAsset(Path.Combine("Assets", "Plugins", "Android", "OneSignalConfig.plugin"));

            /*
             * InstallEdm4UStep
             * deletes the edm4u directory
             */
            AssetDatabase.DeleteAsset(Path.Combine("Assets", "ExternalDependencyManager"));

            /*
             * SetupManifestStep
             * handled by ExportAndroidResourcesStep
             */
            // do nothing

            /*
             * CleanUpLegacyStep
             * adds a random file to the Assets/OneSignal folder
             */
            File.Create(Path.Combine("Assets", "OneSignal", "tempfile"));

            /*
             * ImportPackagesStep
             * removes packages from manifest
             */
            var manifest = new Manifest();
            manifest.Fetch();
            manifest.RemoveDependency("com.onesignal.unity.core");
            manifest.RemoveDependency("com.onesignal.unity.android");
            manifest.RemoveDependency("com.onesignal.unity.ios");
            manifest.RemoveScopeRegistry("https://registry.npmjs.org");
            manifest.ApplyChanges();

        #if UNITY_2020_1_OR_NEWER
            UnityEditor.PackageManager.Client.Resolve();
        #endif
        }
    }
}