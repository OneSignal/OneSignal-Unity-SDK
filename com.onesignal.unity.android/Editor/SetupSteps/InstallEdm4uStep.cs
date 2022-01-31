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

using System.Linq;
using UnityEditor.Compilation;

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

        protected override bool _getIsStepCompleted()
            => CompilationPipeline.GetPrecompiledAssemblyNames()
               .Any(assemblyName => assemblyName.StartsWith("Google.VersionHandler"));

        protected override void _runStep() {
            const string msg = "Downloading Google External Dependency Manager";
            UnityPackageInstaller.DownloadAndInstall(_edm4UPackageDownloadUrl, msg, result => {
                if (result)
                    _shouldCheckForCompletion = true;
            });
        }

        private const string _edm4UVersion = "1.2.169";

        private static readonly string _edm4UPackageDownloadUrl
            = $"https://github.com/googlesamples/unity-jar-resolver/blob/v{_edm4UVersion}/external-dependency-manager-{_edm4UVersion}.unitypackage?raw=true";
    }
}