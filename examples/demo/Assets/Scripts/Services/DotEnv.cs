using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OneSignalDemo.Services
{
    /// <summary>Loads key/value pairs from the demo's .env file (StreamingAssets in players, project root in editor).</summary>
    public static class DotEnv
    {
        private static readonly Dictionary<string, string> _values = new();
        private static bool _loaded;

        public static void Load()
        {
            if (_loaded)
                return;
            _loaded = true;

            try
            {
                var content = ReadEnvContent();
                if (string.IsNullOrEmpty(content))
                    return;

                foreach (var line in content.Split('\n'))
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length == 0 || trimmed.StartsWith("#") || !trimmed.Contains("="))
                        continue;

                    var eqIndex = trimmed.IndexOf('=');
                    var key = trimmed.Substring(0, eqIndex).Trim();
                    var value = trimmed.Substring(eqIndex + 1).Trim();

                    int commentIdx = value.IndexOf('#');
                    if (commentIdx >= 0)
                        value = value.Substring(0, commentIdx).Trim();

                    if (
                        value.Length >= 2
                        && (
                            (value[0] == '"' && value[value.Length - 1] == '"')
                            || (value[0] == '\'' && value[value.Length - 1] == '\'')
                        )
                    )
                        value = value.Substring(1, value.Length - 2);

                    _values[key] = value;
                }
            }
            catch (Exception)
            {
                // .env not bundled or unreadable -- keys remain empty
            }
        }

        public static string Get(string key) =>
            _values.TryGetValue(key, out var value) ? value : "";

        public static bool IsE2EMode =>
            string.Equals(Get("E2E_MODE"), "true", StringComparison.OrdinalIgnoreCase);

        private static string ReadEnvContent()
        {
#if UNITY_EDITOR
            var editorPath = Path.Combine(Application.dataPath, "..", ".env");
            if (File.Exists(editorPath))
                return File.ReadAllText(editorPath);
#endif
            var streamingPath = Path.Combine(Application.streamingAssetsPath, ".env");
            if (File.Exists(streamingPath))
                return File.ReadAllText(streamingPath);

            return null;
        }
    }
}
