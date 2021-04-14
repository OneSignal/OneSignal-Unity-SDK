using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Com.OneSignal.Editor
{
    static class OneSignalImguiStyles
    {
        public const int ContentIndent = 13;
        public const int Padding = 5;

        public static Color ProDisabledImageColor => GetColorFromHtml("#999999ED");
        public static Color SelectedElementColor => GetColorFromHtml(EditorGUIUtility.isProSkin ? "#1BE1F2ED" : "#5CBFCD");
        public static Color DisabledImageColor => EditorGUIUtility.isProSkin ? ProDisabledImageColor : GetColorFromHtml("#3C3C3CFF");


        static GUIStyle s_LabelHeaderStyle;
        public static GUIStyle LabelHeaderStyle
        {
            get
            {
                if (s_LabelHeaderStyle == null)
                    s_LabelHeaderStyle = new GUIStyle { fontSize = 18, fontStyle = FontStyle.Bold };

                if (EditorGUIUtility.isProSkin)
                    s_LabelHeaderStyle.normal.textColor = GetColorFromHtml("#F8F8F8FF");

                return s_LabelHeaderStyle;
            }
        }

        static GUIStyle s_DescriptionLabelStyle;
        public static GUIStyle DescriptionLabelStyle
        {
            get
            {
                if (s_DescriptionLabelStyle == null)
                    s_DescriptionLabelStyle = new GUIStyle { wordWrap = true };

                if (EditorGUIUtility.isProSkin)
                    s_DescriptionLabelStyle.normal.textColor = GetColorFromHtml("#959995FF");


                return s_DescriptionLabelStyle;
            }
        }

        static GUIStyle s_SeparationStyle;
        public static GUIStyle SeparationStyle
        {
            get
            {
                if (s_SeparationStyle == null)
                    s_SeparationStyle = new GUIStyle();

                if (s_SeparationStyle.normal.background == null)
                    s_SeparationStyle.normal.background = GetIconFromHtmlColorString(EditorGUIUtility.isProSkin ? "#292929FF" : "#A2A2A2FF");

                return s_SeparationStyle;
            }
        }

        static GUIStyle s_ServiceBlockHeader;
        public static GUIStyle ServiceBlockHeader => s_ServiceBlockHeader
            ?? (s_ServiceBlockHeader = new GUIStyle { fontSize = 13, fontStyle = FontStyle.Bold, normal = { textColor = DisabledImageColor } });
        
        static Color GetColorFromHtml(string htmlString)
        {
            ColorUtility.TryParseHtmlString(htmlString, out var color);
            return color;
        }

        public static Texture2D GetIconFromHtmlColorString (string htmlString)
        {
            ColorUtility.TryParseHtmlString (htmlString, out var color);
            return GetIcon (color);
        }

        static readonly Dictionary<float, Texture2D> s_ColorIcons = new Dictionary<float, Texture2D>();
        public static Texture2D GetIcon(Color color, int width = 1, int height = 1)
        {
            var colorId = color.r * 100000f + color.g * 10000f + color.b * 1000f + color.a * 100f + width * 10f + height;

            if (s_ColorIcons.ContainsKey(colorId) && s_ColorIcons[colorId] != null) {
                return s_ColorIcons[colorId];
            }

            var tex = new Texture2D(width, height);
            for (var w = 0; w < width; w++)
            {
                for (var h = 0; h < height; h++)
                {
                    tex.SetPixel(w, h, color);
                }
            }

            tex.Apply();

            s_ColorIcons[colorId] = tex;
            return GetIcon(color, width, height);
        }
    }
}
