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
using UnityEditor;

namespace OneSignalSDK {
    /// <summary>
    /// Handles informing the user on startup/import if the legacy SDK has been detected
    /// </summary>
    public static class OneSignalBootstrapper {
        /// <summary>
        /// Asks to open the SDK Setup if legacy files are found or core is missing
        /// </summary>
        [InitializeOnLoadMethod] public static void CheckForLegacy() {
            if (SessionState.GetBool(_sessionCheckKey, false))
                return;

            SessionState.SetBool(_sessionCheckKey, true);

            EditorApplication.delayCall += _checkForLegacy;
        }

        private static void _checkForLegacy() {
        #if !ONE_SIGNAL_INSTALLED
            EditorApplication.delayCall += _showOpenSetupDialog;
        #else
            var inventory = AssetDatabase.LoadAssetAtPath<OneSignalFileInventory>(OneSignalFileInventory.AssetPath);

            if (inventory == null)
                return; // error

            var currentPaths = OneSignalFileInventory.GetCurrentPaths();
            var diff         = currentPaths.Except(inventory.DistributedPaths);

            if (diff.Any())
                EditorApplication.delayCall += _showOpenSetupDialog;
        #endif
        }

        private static void _showOpenSetupDialog() {
            var dialogResult = EditorUtility.DisplayDialog(
                "OneSignal",
                "The project contains an outdated or incomplete install of OneSignal SDK! We recommend running the OneSignal SDK Setup.",
                "Open SDK Setup",
                "Cancel"
            );

            if (dialogResult)
                OneSignalSetupWindow.ShowWindow();
        }

        private const string _sessionCheckKey = "onesignal.bootstrapper.check";
    }
}