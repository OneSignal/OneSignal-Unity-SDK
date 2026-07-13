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
using UnityEngine;

namespace OneSignalSDK
{
    public static class OneSignalDistributionValidator
    {
        [MenuItem("OneSignal/Validate Distribution Layout", false, 101)]
        public static void Validate()
        {
            ValidateInventory();
            ValidateSample();
            Debug.Log("[OneSignalDistributionValidator] Distribution layout is valid.");
        }

        internal static string[] ValidateInventory()
        {
            var inventory = AssetDatabase.LoadAssetAtPath<OneSignalFileInventory>(
                OneSignalFileInventory.AssetPath
            );
            if (inventory == null)
                throw new InvalidOperationException(
                    $"Missing inventory at {OneSignalFileInventory.AssetPath}."
                );

            var actualPaths = OneSignalFileInventory.GetDistributedPaths();
            var recordedPaths = inventory.DistributedPaths ?? Array.Empty<string>();
            if (!actualPaths.SequenceEqual(recordedPaths))
            {
                var missing = actualPaths.Except(recordedPaths);
                var stale = recordedPaths.Except(actualPaths);
                throw new InvalidOperationException(
                    "OneSignal file inventory is out of date."
                        + FormatPaths(" Missing", missing)
                        + FormatPaths(" Stale", stale)
                        + " Run OneSignal/Generate File Inventory and commit the result."
                );
            }

            var unimportedPaths = actualPaths.Where(path =>
                !path.EndsWith(".meta") && AssetDatabase.LoadMainAssetAtPath(path) == null
            );
            if (unimportedPaths.Any())
                throw new InvalidOperationException(
                    "Some distributed files cannot be exported by Unity."
                        + FormatPaths(" Unimported", unimportedPaths)
                );

            return actualPaths;
        }

        private static void ValidateSample()
        {
            var repositoryRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "..", "..")
            );
            var samplePath = Path.Combine(repositoryRoot, "com.onesignal.unity.core", "Samples~");
            var requiredFiles = new[]
            {
                "INCONSOLATA-VARIABLEFONT_WDTH,WGHT.TTF",
                "INCONSOLATA-VARIABLEFONT_WDTH,WGHT.TTF.meta",
                "OneSignal.UnityPackage.Example.asmdef",
                "OneSignal.UnityPackage.Example.asmdef.meta",
                "OneSignalExampleBehaviour.cs",
                "OneSignalExampleBehaviour.cs.meta",
                "OneSignalExampleScene.unity",
                "OneSignalExampleScene.unity.meta",
            };
            var missingFiles = requiredFiles.Where(file =>
                !File.Exists(Path.Combine(samplePath, file))
            );
            if (missingFiles.Any())
                throw new InvalidOperationException(
                    "The Full Usage sample is incomplete."
                        + FormatPaths(" Missing", missingFiles)
                );

            var scene = File.ReadAllText(Path.Combine(samplePath, "OneSignalExampleScene.unity"));
            if (scene.Contains("OneSignalSDK.OneSignalExampleBehaviour"))
                throw new InvalidOperationException(
                    "The Full Usage sample scene contains stale OneSignalExampleBehaviour references."
                );
        }

        private static string FormatPaths(string label, System.Collections.Generic.IEnumerable<string> paths)
        {
            var pathList = paths.ToArray();
            return pathList.Length == 0 ? string.Empty : $"{label}: {string.Join(", ", pathList)}.";
        }
    }
}
