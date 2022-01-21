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

using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace OneSignalSDK {
    /// <summary>
    /// Copies the OneSignal SDK default Android resources to Assets/Plugins/Android/*
    /// </summary>
    public sealed class ExportAndroidResourcesStep : OneSignalSetupStep {
        public override string Summary
            => "Copy Android resources to Assets";

        public override string Details
            => $"Will export necessary files (such as default notification icons) to {_androidPluginPath}";

        public override bool IsRequired
            => false;

        protected override bool _getIsStepCompleted() {
            if (!Directory.Exists(_androidPluginPath))
                return false;

            var packagePaths = Directory.GetFiles(_exportsPath, "*", SearchOption.AllDirectories)
               .Select(path => path.Remove(0, _exportsPath.Length + 1));

            var exportPaths = Directory.GetFiles(_androidPluginPath, "*", SearchOption.AllDirectories)
               .Select(path => path.Remove(0, _androidPluginPath.Length + 1));

            var fileDiff = packagePaths.Except(exportPaths);

            return !fileDiff.Any();
        }

        protected override void _runStep() {
            var files         = Directory.GetFiles(_exportsPath, "*", SearchOption.AllDirectories);
            var filteredFiles = files.Where(file => !file.EndsWith(".meta"));

            foreach (var file in filteredFiles) {
                var trimmedPath    = file.Remove(0, _exportsPath.Length + 1);
                var fileExportPath = Path.Combine(_androidPluginPath, trimmedPath);
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

        
        private static readonly string _exportsPath = Path.Combine("Packages", "com.onesignal.unity.android", "Editor", "Exports");
        private static readonly string _androidPluginPath = Path.Combine("Assets", "Plugins", "Android");
    }
}