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
using UnityEditor;
using UnityEngine;

namespace OneSignalSDK
{
    public static class OneSignalSDKSettings
    {
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

        public static void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath));
            File.WriteAllText(_settingsPath, JsonUtility.ToJson(_settings, true));
        }

        [MenuItem("OneSignal/Disable Location Module")]
        private static void ToggleDisableLocation()
        {
            DisableLocation = !DisableLocation;
        }

        [MenuItem("OneSignal/Disable Location Module", true)]
        private static bool ToggleDisableLocationValidate()
        {
            Menu.SetChecked("OneSignal/Disable Location Module", DisableLocation);
            return true;
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
                return JsonUtility.FromJson<Settings>(File.ReadAllText(_settingsPath))
                    ?? new Settings();
            }
            catch
            {
                return new Settings();
            }
        }

        [Serializable]
        private sealed class Settings
        {
            public bool disableLocation;
        }
    }
}
