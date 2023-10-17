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

using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace OneSignalSDK {
    /// <summary>
    /// Copies the OneSignalConfig.androidlib to Assets/Plugins/Android/*
    /// </summary>
    public sealed class ExportAndroidResourcesStep : OneSignalSetupStep {
        public override string Summary
            => "Copy Android plugin to Assets";

        public override string Details
            => $"Will create the {_pluginExportPath} directory filled with notification icons to be customized for your app";

        public override bool IsRequired
            => true;

        protected override bool _getIsStepCompleted() {
            if (!Directory.Exists(_pluginExportPath) || Directory.Exists(_pluginV3ExportPath))
                return false;

            var packagePaths = Directory.GetFiles(_pluginPackagePath, "*", SearchOption.AllDirectories)
               .Select(path => path.Remove(0, path.LastIndexOf(_pluginName, StringComparison.InvariantCulture)));
            packagePaths = packagePaths.Where(file => !file.EndsWith(".meta"));

            var exportPaths = Directory.GetFiles(_pluginExportPath, "*", SearchOption.AllDirectories)
               .Select(path => path.Remove(0, path.LastIndexOf(_pluginName, StringComparison.InvariantCulture)));

            var fileDiff = packagePaths.Except(exportPaths);

            if (fileDiff.Any())
                return false;

            var pluginManifest = File.ReadAllText(_manifestPackagePath);
            var projectManifest = File.ReadAllText(_manifestExportPath);
            
            return pluginManifest == projectManifest;
        }

        protected override void _runStep() {
            MigratePluginToAndroidlib();

            var files = Directory.GetFiles(_pluginPackagePath, "*", SearchOption.AllDirectories);
            var filteredFiles = files.Where(file => !file.EndsWith(".meta"));

            foreach (var file in filteredFiles) {
                var trimmedPath    = file.Remove(0, _pluginPackagePath.Length + 1);
                var fileExportPath = Path.Combine(_pluginExportPath, trimmedPath);
                var containingPath = fileExportPath.Remove(fileExportPath.LastIndexOf(Path.DirectorySeparatorChar));

                /*
                 * Export the file.
                 * By default the CreateDirectory and Copy methods don't overwrite but we use the Exists
                 * checks to avoid polluting the console with warnings.
                 */

                if (!Directory.Exists(containingPath))
                    Directory.CreateDirectory(containingPath);
                
                if (!fileExportPath.Contains(".png")) // always refresh non-pngs
                    File.Copy(file, fileExportPath, true);
                else if (!File.Exists(fileExportPath)) // don't copy over existing png files
                    File.Copy(file, fileExportPath);
            }

            AssetDatabase.Refresh();
        }

        private void MigratePluginToAndroidlib() {
            if (Directory.Exists(_pluginV3ExportPath)) {
                if (!Directory.Exists(_pluginExportPath)) {
                    try
                    {
                        AssetDatabase.StartAssetEditing();

                        // Remove project.properties
                        if (File.Exists(_projectPropertiesV3ExportPath)) {
                            AssetDatabase.DeleteAsset(_projectPropertiesV3ExportPath);
                        }
                        
                        // Rename OneSignalConfig.plugin to OneSignalConfig.androidlib
                        AssetDatabase.MoveAsset(_pluginV3ExportPath, _pluginExportPath);
                    }
                    finally
                    {
                        AssetDatabase.StopAssetEditing();
                    }

                    // Move the icons and .wav file to /src/main
                    if (Directory.Exists(_resV3ExportPath)) {
                        Directory.CreateDirectory(Path.GetDirectoryName(_resExportPath));

                        FileUtil.MoveFileOrDirectory(_resV3ExportPath, _resExportPath);
                    }
                } else {
                    AssetDatabase.DeleteAsset(_pluginV3ExportPath);
                }
            }
        }

        private const string _pluginName = "OneSignalConfig.androidlib";
        private static readonly string _packagePath = Path.Combine("Packages", "com.onesignal.unity.android", "Editor");
        private static readonly string _androidPluginsPath = Path.Combine("Assets", "Plugins", "Android");
        
        private static readonly string _pluginPackagePath = Path.Combine(_packagePath, _pluginName);
        private static readonly string _pluginExportPath = Path.Combine(_androidPluginsPath, _pluginName);
        
        private static readonly string _manifestPackagePath = Path.Combine(_pluginPackagePath, "AndroidManifest.xml");
        private static readonly string _manifestExportPath = Path.Combine(_pluginExportPath, "AndroidManifest.xml");

        private const string _resPath = "src/main/res";
        private static readonly string _resExportPath = Path.Combine(_pluginExportPath, _resPath);

        // Old OneSignalConfig name used from 3.x.x to 5.0.2
        private const string _pluginNameV3 = "OneSignalConfig.plugin";
        private static readonly string _pluginV3ExportPath = Path.Combine(_androidPluginsPath, _pluginNameV3);
        private static readonly string _projectPropertiesV3ExportPath = Path.Combine(_pluginV3ExportPath, "project.properties");
        private const string _resV3Path = "res";
        private static readonly string _resV3ExportPath = Path.Combine(_pluginExportPath, _resV3Path);
    }
}