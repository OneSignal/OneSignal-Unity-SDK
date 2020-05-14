using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OneSignalPush.Editor
{
    static class OneSignalImguiStyles
    {
        public const int ContentIndent = 13;

        static GUIStyle s_LabelHeaderStyle;
        public static GUIStyle LabelHeaderStyle {
            get {
                if (s_LabelHeaderStyle == null) {
                    s_LabelHeaderStyle = new GUIStyle();
                    s_LabelHeaderStyle.fontSize = 18;
                    s_LabelHeaderStyle.fontStyle = FontStyle.Bold;
                }

                if (EditorGUIUtility.isProSkin) {
                    s_LabelHeaderStyle.normal.textColor = GetColorFromHtml("#F8F8F8FF");
                }

                return s_LabelHeaderStyle;
            }
        }

        static GUIStyle s_DescriptionLabelStyle;
        public static GUIStyle DescriptionLabelStyle {
            get {
                if (s_DescriptionLabelStyle == null) {

                    s_DescriptionLabelStyle = new GUIStyle();
                    s_DescriptionLabelStyle.wordWrap = true;
                }

                if (EditorGUIUtility.isProSkin) {
                    s_DescriptionLabelStyle.normal.textColor = GetColorFromHtml("#959995FF");
                }

                return s_DescriptionLabelStyle;
            }
        }

        static GUIStyle s_SeparationStyle = null;
        public static GUIStyle SeparationStyle {
            get {
                if (s_SeparationStyle == null) {
                    s_SeparationStyle = new GUIStyle();
                }

                if (s_SeparationStyle.normal.background == null) {
                    if (EditorGUIUtility.isProSkin) {
                        s_SeparationStyle.normal.background = GetIconFromHtmlColorString("#292929FF");
                    } else {
                        s_SeparationStyle.normal.background = GetIconFromHtmlColorString("#A2A2A2FF");
                    }
                }
                return s_SeparationStyle;
            }
        }

        public static Color SelectedElementColor => GetColorFromHtml(EditorGUIUtility.isProSkin ? "#1BE1F2ED" : "#5CBFCD");

        static Color GetColorFromHtml(string htmlString)
        {
            ColorUtility.TryParseHtmlString(htmlString, out var color);
            return color;
        }

        public static Texture2D GetIconFromHtmlColorString (string htmlString) {
            ColorUtility.TryParseHtmlString (htmlString, out var color);
            return GetIcon (color);
        }

        private static Dictionary<float, Texture2D> s_colorIcons = new Dictionary<float, Texture2D>();

        public static Texture2D GetIcon(Color color, int width = 1, int height = 1) {
            float colorId = color.r * 100000f + color.g * 10000f + color.b * 1000f + color.a * 100f + width * 10f + height;

            if (s_colorIcons.ContainsKey(colorId) && s_colorIcons[colorId] != null) {
                return s_colorIcons[colorId];
            } else {


                Texture2D tex = new Texture2D(width, height);
                for (int w = 0; w < width; w++) {
                    for (int h = 0; h < height; h++) {
                        tex.SetPixel(w, h, color);
                    }
                }

                tex.Apply();


                s_colorIcons[colorId] = tex;
                return GetIcon(color, width, height);
            }
        }
    }
}
