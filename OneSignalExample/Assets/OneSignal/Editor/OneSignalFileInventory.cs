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
using UnityEngine;

namespace OneSignalSDK {
    /// <summary>
    /// Inventory distributed with the *.unitypackage in order to determine if there are any legacy files in need of removal
    /// </summary>
    internal sealed class OneSignalFileInventory : ScriptableObject {
        /// <summary>
        /// Array of paths within the OneSignal directory which were determined to be part of the distributed unitypackage
        /// </summary>
        public string[] DistributedPaths;

        /// <summary>
        /// Array of current paths within the OneSignal directory
        /// </summary>
        public static string[] GetCurrentPaths()
            => ConvertPathsToUnix(Directory.GetFiles(PackageAssetsPath, "*", SearchOption.AllDirectories));

        /// <summary>
        /// Makes sure <see cref="paths"/> are using forward slash to be Unix compatible.
        /// https://docs.microsoft.com/en-us/dotnet/api/system.io.path.altdirectoryseparatorchar?view=net-5.0#examples
        /// </summary>
        /// <param name="paths">the paths to check and convert</param>
        /// <returns>paths with / as the directory separator</returns>
        public static string[] ConvertPathsToUnix(string[] paths) {
            if (Path.DirectorySeparatorChar == Path.AltDirectorySeparatorChar)
                return paths;

            var fixedPaths = paths.Select(path =>
                path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            );

            return fixedPaths.ToArray();
        }

        public const string AssetName = "OneSignalFileInventory.asset";
        public static readonly string PackageAssetsPath = Path.Combine("Assets", "OneSignal");
        public static readonly string EditorResourcesPath = Path.Combine(PackageAssetsPath, "Editor", "Resources");
        public static readonly string AssetPath = Path.Combine(EditorResourcesPath, AssetName);
    }
}