/*
 * Modified MIT License
 *
 * Copyright 2023 OneSignal
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

using System.Linq;
using UnityEditor.Compilation;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using OneSignalSDK.Debug.Utilities;

namespace OneSignalSDK {
    /// <summary>
    /// Checks for EDM4U assemblies and installs the package from its github releases
    /// </summary>
    public sealed class InstallEdm4uStep : OneSignalSetupStep {
        public override string Summary
            => $"Install EDM4U {_edm4UVersion}";

        public override string Details
            => $"Downloads and imports version {_edm4UVersion} from Google's repo. This library resolves dependencies " +
                $"among included libraries on Android.";

        public override bool IsRequired
            => true;

        private Version _getAssetsEDM4UVersion() {
            var isInstalled = CompilationPipeline.GetPrecompiledAssemblyNames()
               .Any(assemblyName => assemblyName.StartsWith("Google.VersionHandler"));

            if (!isInstalled)
                return null;

            var path = "Assets/ExternalDependencyManager/Editor";
            var directoryInfo = new DirectoryInfo(path);

            if (!directoryInfo.Exists)
                return null;

            FileInfo[] files;

            try {
                files = directoryInfo.GetFiles("external-dependency-manager_version-*_manifest.txt");
            } catch (Exception) {
                return null;
            }

            if (files.Length != 1) {
                return null;
            }

            var file = files[0];
            var pattern = @"external-dependency-manager_version-(.+)_manifest\.txt";
            var match = Regex.Match(file.Name, pattern);
            var version = new Version(match.Groups[1].Value);

            return version;
        }

        private Version _getPackagesEDM4UVersion() {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name.StartsWith("Google.PackageManagerResolver"));
            if (assembly == null)
                return null;

            var type = assembly.GetType("Google.PackageManagerResolverVersionNumber", false);
            if (type == null)
                return null;

            var property = type.GetProperty("Value", BindingFlags.Static | BindingFlags.Public);
            if (property == null)
                return null;

            var version = (Version)property.GetValue(null);
            return version;
        }

        protected override bool _getIsStepCompleted() {
            var version = _getAssetsEDM4UVersion();
            if (version == null)
                version = _getPackagesEDM4UVersion();

            if (version == null) {
                SDKDebug.Warn("EDM4U version number could not be determined.");
                return false;
            }

            var expectedVersion = new Version(_edm4UVersion);

            return version >= expectedVersion;
        }

        protected override void _runStep() {
            const string msg = "Downloading Google External Dependency Manager";
            UnityPackageInstaller.DownloadAndInstall(_edm4UPackageDownloadUrl, msg, result => {
                if (result)
                    _shouldCheckForCompletion = true;
            });
        }

        private const string _edm4UVersion = "1.2.177";

        private static readonly string _edm4UPackageDownloadUrl
            = $"https://github.com/googlesamples/unity-jar-resolver/blob/v{_edm4UVersion}/external-dependency-manager-{_edm4UVersion}.unitypackage?raw=true";
    }
}