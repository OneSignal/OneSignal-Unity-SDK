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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace OneSignalSDK {
    /// <summary>
    /// Makes sure that the OneSignalConfig.plugin folder exists and is populated with the correct files within the project
    /// </summary>
    public class SetupManifestStep : OneSignalSetupStep {
        public override string Summary
            => "Setup keys in Android manifest";

        public override string Details
            => $"Adds the {PlayerSettings.applicationIdentifier} applicationIdentifier to the {_manifestPath}." +
                $"If you intend to change this id then please re-run this step";

        public override bool IsRequired
            => true;

        protected override bool _getIsStepCompleted() {
            if (!File.Exists(_manifestPath))
                return false;

            var reader   = new StreamReader(_manifestPath);
            var contents = reader.ReadToEnd();
            reader.Close();

            var matches = Regex.Matches(contents, _manifestRegex);

            foreach (var match in matches) {
                if (match.ToString() != PlayerSettings.applicationIdentifier)
                    return false;
            }

            return true;
        }

        protected override void _runStep() {
            var replacements = new Dictionary<string, string> {
                [_manifestRegex] = PlayerSettings.applicationIdentifier
            };

            // modifies the manifest in place
            _replaceStringsInFile(_manifestPath, _manifestPath, replacements);
        }

        private const string _pluginName = "OneSignalConfig.plugin";
        private static readonly string _androidPluginsPath = Path.Combine("Assets", "Plugins", "Android");

        private static readonly string _manifestPath
            = Path.Combine(_androidPluginsPath, _pluginName, "AndroidManifest.xml");

        private const string _manifestRegex
            = @"((?<=<permission android:name="").+?(?=\.permission\.C2D_MESSAGE"" android:protectionLevel=""signature"" \/>)|(?<=<uses-permission android:name="").+?(?=\.permission\.C2D_MESSAGE"" \/>)|(?<=<category android:name="").+?(?="" \/>))";

        private static void _replaceStringsInFile(
            string sourcePath,
            string destinationPath,
            IReadOnlyDictionary<string, string> replacements
        ) {
            try {
                if (!File.Exists(sourcePath)) {
                    Debug.LogError($"could not find {sourcePath}");

                    return;
                }

                var reader   = new StreamReader(sourcePath);
                var contents = reader.ReadToEnd();
                reader.Close();

                foreach (var replacement in replacements)
                    contents = Regex.Replace(contents, replacement.Key, replacement.Value);

                var writer = new StreamWriter(destinationPath);
                writer.Write(contents);
                writer.Close();
            }
            catch (Exception exception) {
                Debug.LogError($"could not replace strings of {sourcePath} because:\n{exception.Message}");
            }
        }
    }
}