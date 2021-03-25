using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Com.OneSignal.Editor
{
    static class Skin
    {
        static readonly Dictionary<string, Texture2D> s_Icons = new Dictionary<string, Texture2D>();
        public static Texture2D SettingsWindowIcon => GetTextureAtPath(IconsPath);

        private static string IconsPath => "Packages/com.onesignal-test.unity.core/Editor/Icons/"
                                           + (EditorGUIUtility.isProSkin
                                               ? "icon_pro.png"
                                               : "icon_default.png" );

        private static Texture2D GetTextureAtPath(string path)
        {
            if (s_Icons.ContainsKey(path)) return s_Icons[path];

            var tex = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
            s_Icons.Add(path, tex);
            return tex;
        }
    }
}
