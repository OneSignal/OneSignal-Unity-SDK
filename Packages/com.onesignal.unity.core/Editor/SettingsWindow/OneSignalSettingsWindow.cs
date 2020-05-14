using System;
using OneSignalPush.Editor.IMGUI;
using UnityEditor;
using UnityEngine;

namespace OneSignalPush.Editor
{
    class OneSignalSettingsWindow : EditorWindow
    {
        const string k_HeaderText = "OneSignal is the market leader in customer engagement, powering mobile push, web push, email, and in-app messages.";

        [SerializeField]
        HyperLabel m_DocumentationLink;

        bool m_WindowInitialized;

        void OnEnable()
        {
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
            titleContent = new GUIContent(OneSignalSettings.PluginName);
        }

        void OnInit()
        {
            m_WindowInitialized = true;
            m_DocumentationLink = new HyperLabel(new GUIContent("Go To Documentation"), EditorStyles.miniLabel);
            m_DocumentationLink.SetMouseOverColor(OneSignalImguiStyles.SelectedElementColor);
        }

        void OnGUI()
        {
            if(!m_WindowInitialized)
                OnInit();

            DrawToolbar();

            EditorGUILayout.BeginVertical(OneSignalImguiStyles.SeparationStyle);
            {
                GUILayout.Space(20);
                using (new SA_GuiBeginHorizontal())
                {
                    GUILayout.Space(OneSignalImguiStyles.ContentIndent);
                    EditorGUILayout.LabelField(OneSignalSettings.PluginName, OneSignalImguiStyles.LabelHeaderStyle);
                }

                GUILayout.Space(8);

                using (new SA_GuiBeginHorizontal())
                {
                    GUILayout.Space(OneSignalImguiStyles.ContentIndent);
                    EditorGUILayout.LabelField(k_HeaderText, OneSignalImguiStyles.DescriptionLabelStyle);
                }

                GUILayout.Space(2);
            }

        }

        void DrawToolbar() {
            GUILayout.Space(2);
            using (new SA_GuiBeginHorizontal()) {
                DrawDocumentationLink();
                EditorGUILayout.Space();
            }
            GUILayout.Space(5);
        }

        void DrawDocumentationLink() {
            var width = m_DocumentationLink.CalcSize().x + 5f;
            var clicked = m_DocumentationLink.Draw(GUILayout.Width(width));
            if (clicked) {
                //Application.OpenURL(m_documentationUrl);
            }
        }

        public static void ShowTowardsInspector()
        {
            var inspectorType = Type.GetType("UnityEditor.InspectorWindow, UnityEditor.dll");
            var window = GetWindow<OneSignalSettingsWindow>(inspectorType);
            window.Show();
            window.minSize = new Vector2(350, 100);
        }
    }
}
