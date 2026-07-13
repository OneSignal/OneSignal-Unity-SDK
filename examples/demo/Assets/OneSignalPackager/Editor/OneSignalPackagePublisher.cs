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

using System.IO;
using System.Linq;
using UnityEditor;

namespace OneSignalSDK
{
    /// <summary>
    /// Creates a unitypackage file for publishing
    /// </summary>
    public static class OneSignalPackagePublisher
    {
        public static void UpdateProjectVersion()
        {
            var packageVersion = File.ReadAllText(VersionFilePath);
            PlayerSettings.bundleVersion = packageVersion;
        }

        [MenuItem("OneSignal/ExportUnityPackage")]
        public static void ExportUnityPackage()
        {
            UnityEngine.Debug.Log($"[OneSignalPackagePublisher] start exporting package");
            AssetDatabase.Refresh();
            var inventoryPaths = OneSignalDistributionValidator.ValidateInventory();
            var filePaths = inventoryPaths
                .Where(path => !path.EndsWith(".meta"))
                .Concat(
                    inventoryPaths
                        .Where(path =>
                            path.EndsWith(".meta")
                            && Directory.Exists(path.Substring(0, path.Length - 5))
                        )
                        .Select(path => path.Substring(0, path.Length - 5))
                )
                .Distinct()
                .ToArray();
            var packageVersion = File.ReadAllText(VersionFilePath);
            var packageName = $"OneSignal-v{packageVersion}.unitypackage";

            UnityEngine.Debug.Log($"[OneSignalPackagePublisher] package name: {packageName}");

            UnityEngine.Debug.Log(
                $"[OneSignalPackagePublisher] Found {filePaths.Length} files/directories to include:"
            );
            foreach (var path in filePaths)
            {
                UnityEngine.Debug.Log($"[OneSignalPackagePublisher]   - {path}");
            }

            AssetDatabase.ExportPackage(
                filePaths,
                packageName,
                ExportPackageOptions.IncludeDependencies
            );
        }

        private static readonly string PackagePath = Path.Combine("Assets", "OneSignal");
        private static readonly string VersionFilePath = Path.Combine(PackagePath, "VERSION");
    }
}
