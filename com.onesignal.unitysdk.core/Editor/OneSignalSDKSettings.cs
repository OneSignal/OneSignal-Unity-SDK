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
using System.IO;
using UnityEditor;

namespace OneSignalSDK
{
    public static class OneSignalSDKSettings
    {
        public const string DisableLocationEnvVar = "ONESIGNAL_DISABLE_LOCATION";

        public static event Action Changed;

        public static bool DisableLocation
        {
            get => _settings.disableLocation;
            set
            {
                if (_settings.disableLocation == value)
                    return;

                _settings.disableLocation = value;
                Save();
                Changed?.Invoke();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Resolved value used by the build pipeline. The <see cref="DisableLocationEnvVar"/>
        /// environment variable, when present, overrides the persisted Editor setting so CLI
        /// and CI builds can opt out without mutating project settings.
        /// </summary>
        public static bool EffectiveDisableLocation
        {
            get
            {
                var environmentValue = Environment.GetEnvironmentVariable(DisableLocationEnvVar);
                if (!string.IsNullOrEmpty(environmentValue))
                {
                    var normalized = environmentValue.Trim();
                    return normalized.Equals("true", StringComparison.OrdinalIgnoreCase)
                        || normalized == "1";
                }

                return _settings.disableLocation;
            }
        }

        public static void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath));
            File.WriteAllText(
                _settingsPath,
                Json.Serialize(
                    new Dictionary<string, object>
                    {
                        { nameof(Settings.disableLocation), _settings.disableLocation },
                    },
                    true
                )
            );
        }

        private static readonly string _settingsPath = Path.Combine(
            "ProjectSettings",
            "OneSignalSettings.json"
        );

        private static Settings _settings = Load();

        private static Settings Load()
        {
            if (!File.Exists(_settingsPath))
                return new Settings();

            try
            {
                var values =
                    Json.Deserialize(File.ReadAllText(_settingsPath))
                    as Dictionary<string, object>;
                if (
                    values != null
                    && values.TryGetValue(nameof(Settings.disableLocation), out var value)
                    && value is bool disableLocation
                )
                    return new Settings { disableLocation = disableLocation };
            }
            catch
            {
                return new Settings();
            }

            return new Settings();
        }

        [Serializable]
        private sealed class Settings
        {
            public bool disableLocation;
        }
    }
}
