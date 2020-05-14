using System;
using UnityEditor;
using UnityEngine;

namespace OneSignalPush.Editor.IMGUI
{
    [Serializable]
    class HyperLabel : HyperButton
    {
        [SerializeField] GUIContent m_content;
        [SerializeField] GUIContent m_highlightedContext;

        [SerializeField] GUIStyle m_style;
        [SerializeField] GUIStyle m_mouseOverStyle;

        [SerializeField] bool m_overrideGuiColor;


        public HyperLabel(GUIContent content) : this(content, EditorStyles.label) {}

        public HyperLabel(GUIContent content, GUIStyle style) {
            m_content = content;
            m_style = new GUIStyle(style);
            m_mouseOverStyle = new GUIStyle(style);
        }



		public void SetColor(Color color) {
            m_style.normal.textColor = color;
        }

        public void SetMouseOverColor(Color color) {
            m_mouseOverStyle.normal.textColor = color;
        }


        public void DisableHighLight()
        {
            m_highlightedContext = null;
        }



        public bool DrawWithCalcSize() {
            var width = CalcSize().x + 5f;
            return Draw(GUILayout.Width(width));
        }


        protected override void OnNormal(params GUILayoutOption[] options) {
            if(m_overrideGuiColor)
            {
                using(new SA_GuiChangeColor(m_style.normal.textColor))
                {
                    using (new SA_GuiChangeContentColor(m_style.normal.textColor))
                    {
                        EditorGUILayout.LabelField(HighlightedContext ?? m_content, m_style, options);
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField(HighlightedContext ?? m_content, m_style, options);
            }
        }


        protected override void OnMouseOver(params GUILayoutOption[] options) {
            var c = GUI.color;
            GUI.color = m_mouseOverStyle.normal.textColor;


            EditorGUILayout.LabelField(m_content, m_mouseOverStyle, options);
            GUI.color = c;
        }

        public Vector2 CalcSize() {
            return m_style.CalcSize(m_content);
        }

		public void SetContent(GUIContent content) {
			m_content = content;
		}

		public void SetStyle(GUIStyle style) {
			m_style = new GUIStyle(style);
		}


        public void GuiColorOverride(bool value) {
            m_overrideGuiColor = value;
        }

        public GUIContent Content {
            get {
                return m_content;
            }
        }

        public Color Color {
            get {
                return m_style.normal.textColor;
            }
        }

        private GUIContent HighlightedContext
        {
            get
            {
                if (m_highlightedContext != null && string.IsNullOrEmpty(m_highlightedContext.text))
                {
                    return null;
                }

                return m_highlightedContext;
            }
        }
    }
}
