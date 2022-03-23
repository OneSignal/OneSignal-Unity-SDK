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

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OneSignalSDK {
    /// <summary>
    /// Checks if this code bundle is of a mismatched version than the currently imported packages and updates
    /// </summary>
    public sealed class SyncCodeBundleStep : OneSignalSetupStep {
        public override string Summary
            => "Sync example code bundle";

        public override string Details
            => "Checks if the project scope code bundle (example code) is of a mismatched version than the currently " +
                "imported packages";

        public override bool IsRequired
            => false;

        protected override bool _getIsStepCompleted() {
            if (!File.Exists(_packageJsonPath)) {
                Debug.LogError($"Could not find {_packageJsonPath}");
                return true;
            }

            if (!File.Exists(_versionPath))
                return true;

            var packageJson = File.ReadAllText(_packageJsonPath);

            if (Json.Deserialize(packageJson) is Dictionary<string, object> packageInfo) {
                _sdkVersion = packageInfo["version"] as string;

                return _bundleVersion == _sdkVersion;
            }

            Debug.LogError("Could not deserialize package.json");

            return true;
        }

        protected override void _runStep() {
            var msg = $"Downloading OneSignal Unity SDK {_sdkVersion}";
            UnityPackageInstaller.DownloadAndInstall(_onesignalUnityPackageDownloadUrl, msg, result => {
                if (!result)
                    _shouldCheckForCompletion = true;
            });
        }

        private static readonly string _versionPath = Path.Combine("Assets", "OneSignal", "VERSION");
        private static string _bundleVersion => File.ReadAllText(_versionPath);

        private static string _onesignalUnityPackageDownloadUrl
            => $"https://github.com/OneSignal/OneSignal-Unity-SDK/blob/{_sdkVersion}/OneSignal-v{_sdkVersion}.unitypackage";

        private static readonly string _packagePath = Path.Combine("Packages", "com.onesignal.unity.core");
        private static readonly string _packageJsonPath = Path.Combine(_packagePath, "package.json");

        private static string _sdkVersion;
    }
}