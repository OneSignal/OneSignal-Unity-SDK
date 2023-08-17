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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace OneSignalSDK {
    /// <summary>
    /// Handles if there are files within the Assets/OneSignal folder which should not be there. Typically this
    /// indicates the presence of legacy files.
    /// </summary>
    public sealed class CleanUpLegacyStep : OneSignalSetupStep {
        public override string Summary
            => "Remove legacy files";

        public override string Details
            => "Checks for the diff between the files distributed with the package and those which are in the " +
                OneSignalFileInventory.PackageAssetsPath;

        public override bool IsRequired
            => true;

        protected override bool _getIsStepCompleted() {
            var diff = _getDiff();

            if (diff == null)
                return true; // error

            return !diff.Any();
        }

        protected override void _runStep() {
            var diff = _getDiff();

            if (diff == null)
                return; // error

            foreach (var path in diff)
                File.Delete(path);
        }

        private IEnumerable<string> _getDiff() {
            if (_inventory == null)
                _inventory = AssetDatabase.LoadAssetAtPath<OneSignalFileInventory>(OneSignalFileInventory.AssetPath);

            if (_inventory == null)
                return null; // error

            var currentPaths = OneSignalFileInventory.GetCurrentPaths();

            return currentPaths.Except(_inventory.DistributedPaths);
        }

        private OneSignalFileInventory _inventory;
    }
}