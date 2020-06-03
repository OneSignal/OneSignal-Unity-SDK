using System;
using UnityEditor;
using UnityEngine;

namespace Com.OneSignal.Editor.IMGUI
{
    [Serializable]
    class HyperLabel : HyperButton
    {
        [SerializeField]
        GUIContent m_Content;
        [SerializeField]
        GUIStyle m_Style;
        [SerializeField]
        GUIStyle m_MouseOverStyle;

        public GUIContent Content => m_Content;
        public Color Color => m_Style.normal.textColor;

        public HyperLabel(GUIContent content)
            : this(content, EditorStyles.label) { }

        public HyperLabel(GUIContent content, GUIStyle style)
        {
            m_Content = content;
            m_Style = new GUIStyle(style);
            m_MouseOverStyle = new GUIStyle(style);
        }

        public void SetMouseOverColor(Color color)
        {
            m_MouseOverStyle.normal.textColor = color;
        }

        protected override void OnNormal(params GUILayoutOption[] options)
        {
            EditorGUILayout.LabelField(m_Content, m_Style, options);
        }

        protected override void OnMouseOver(params GUILayoutOption[] options)
        {
            var c = GUI.color;
            GUI.color = m_MouseOverStyle.normal.textColor;

            EditorGUILayout.LabelField(m_Content, m_MouseOverStyle, options);
            GUI.color = c;
        }

        public Vector2 CalcSize()
        {
            return m_Style.CalcSize(m_Content);
        }

        public void SetStyle(GUIStyle style)
        {
            m_Style = new GUIStyle(style);
        }
    }
}
