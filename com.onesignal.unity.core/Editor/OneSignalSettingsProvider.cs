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
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OneSignalSDK
{
    /// <summary>
    /// Adds a "OneSignal" page under Project Settings for persistent SDK configuration.
    /// </summary>
    public static class OneSignalSettingsProvider
    {
        private const string _path = "Project/OneSignal";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            var keywords = new HashSet<string>(new[]
            {
                "OneSignal", "Push", "Notifications", "Location", "Disable Location"
            });

            return new SettingsProvider(_path, SettingsScope.Project)
            {
                label = "OneSignal",
                keywords = keywords,
                guiHandler = _ => DrawGUI()
            };
        }

        private static void DrawGUI()
        {
            EditorGUILayout.Space();

            var environmentOverride = Environment.GetEnvironmentVariable(
                OneSignalSDKSettings.DisableLocationEnvVar
            );
            var hasEnvironmentOverride = !string.IsNullOrEmpty(environmentOverride);

            EditorGUILayout.LabelField("Location", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(hasEnvironmentOverride))
            {
                var newValue = EditorGUILayout.ToggleLeft(
                    new GUIContent(
                        "Disable Location Module",
                        "Excludes the OneSignal location dependency from the generated native "
                            + "build. Enable this if your app does not use location features."
                    ),
                    OneSignalSDKSettings.DisableLocation
                );

                if (newValue != OneSignalSDKSettings.DisableLocation)
                    OneSignalSDKSettings.DisableLocation = newValue;
            }

            EditorGUILayout.Space();

            if (hasEnvironmentOverride)
            {
                EditorGUILayout.HelpBox(
                    $"The {OneSignalSDKSettings.DisableLocationEnvVar} environment variable is set "
                        + $"to \"{environmentOverride}\" and overrides this setting for the current "
                        + "session. The effective value is "
                        + $"{(OneSignalSDKSettings.EffectiveDisableLocation ? "disabled" : "enabled")}.",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"Set the {OneSignalSDKSettings.DisableLocationEnvVar} environment variable "
                        + "(\"true\" or \"1\") to override this setting for CLI and CI builds "
                        + "without modifying project settings.",
                    MessageType.None
                );
            }
        }
    }
}
