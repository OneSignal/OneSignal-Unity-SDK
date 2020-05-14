using System;
using System.Collections.Generic;
using OneSignalPush.Editor.IMGUI;
using UnityEditor;
using UnityEngine;

namespace OneSignalPush.Editor
{
    abstract class OneSignalBaseSettingsWindow<TWindow> : EditorWindow where TWindow : EditorWindow
    {
        float m_HeaderHeight;
        float m_ScrollContentHeight;
        Vector2 m_ScrollPos;
        bool m_MouseInside;

        [SerializeField] bool m_ShouldEnabled;
        [SerializeField] bool m_ShouldAwake;

        [SerializeField] string m_HeaderTitle;
        [SerializeField] string m_HeaderDescription;
        [SerializeField] string m_DocumentationUrl;

        [SerializeField] ScriptableObject m_SerializationStateIndicator;
        [SerializeField] HyperLabel m_DocumentationLink;

        // MenuTabs
        [SerializeField] protected bool m_IsToolBarWasAlreadyCreated;
        [SerializeField] protected HyperToolbar m_MenuToolbar;
        [SerializeField] protected List<WindowTabElement> m_TabsLayout = new List<WindowTabElement>();

        protected abstract void OnAwake();
        protected abstract void BeforeGUI();
        protected abstract void AfterGUI();

        protected void SetHeaderTitle(string headerTitle)
        {
            m_HeaderTitle = headerTitle;
        }

        protected void SetHeaderDescription(string headerDescription)
        {
            m_HeaderDescription = headerDescription;
        }

        protected void SetDocumentationUrl(string documentationUrl)
        {
            m_DocumentationUrl = documentationUrl;
        }

        protected void AddMenuItem(string itemName, WindowTabElement layout, bool forced = false)
        {
            // It could be 2 cases
            // 1 When the window is created and we need to create everything
            // 2 When Unity called Awake and only ScriptableObjects are destroyed, so we only need to re-create ScriptableObjects
            if (!m_IsToolBarWasAlreadyCreated || forced)
            {
                var button = new HyperLabel(new GUIContent(itemName), EditorStyles.boldLabel);
                button.SetMouseOverColor(OneSignalImguiStyles.SelectedElementColor);
                m_MenuToolbar.AddButtons(button);
            }

            m_TabsLayout.Add(layout);
        }

        void Awake() {

            if(!m_IsToolBarWasAlreadyCreated)
                OnCreate();

            m_TabsLayout = new List<WindowTabElement>();
            m_ShouldAwake = true;
            m_SerializationStateIndicator = CreateInstance<ScriptableObject>();
        }

        protected virtual void OnEnable()
        {
            m_ShouldEnabled = true;

            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
        }

        protected void OnCreate()
        {
            m_MenuToolbar = new HyperToolbar();
        }

        void OnLayoutEnable()
        {
            m_DocumentationLink = new HyperLabel(new GUIContent("Documentation"), EditorStyles.miniLabel);
            m_DocumentationLink.SetMouseOverColor(OneSignalImguiStyles.SelectedElementColor);

            //Update toolbar Styles
            foreach (var button in m_MenuToolbar.Buttons)
            {
                button.SetStyle(EditorStyles.boldLabel);
                button.SetMouseOverColor(OneSignalImguiStyles.SelectedElementColor);
            }
        }

        void CheckForGUIEvents()
        {
            // Just a workaround, since in play-mode scriptable object could get destroyed
            if (m_SerializationStateIndicator == null) {
                Awake();
            }

            if (m_ShouldAwake) {
                OnAwake();
                m_ShouldAwake = false;

                // When entering play mode both OnAwake & OnEnable get called
                // But when we exit play mode only OnAwake is called, so we need to add
                // one more extra OnEnable emulation
                m_ShouldEnabled = true;
            }

            if (m_ShouldEnabled) {
                OnLayoutEnable();
                m_ShouldEnabled = false;
            }

            // first GUI call, we assume all tool bar items has been added already
            m_IsToolBarWasAlreadyCreated = true;
        }

        void OnGUI() {

#if UNITY_2017_4_OR_NEWER
            if (Event.current.type == EventType.MouseMove)
#else
            if (Event.current.type == EventType.mouseMove)
#endif
            {
                m_MouseInside = true;
            }

            if (Event.current.type == EventType.MouseEnterWindow)
            {
                m_MouseInside = true;
                if(focusedWindow == null)
                {
                    return;
                }

                FocusWindowIfItsOpen<TWindow>();
            }

            if (Event.current.type == EventType.MouseLeaveWindow)
            {
                m_MouseInside = false;
            }

            CheckForGUIEvents();
            BeforeGUI();
            OnLayoutGUI();
            AfterGUI();

            if (m_MouseInside)
                Repaint();
        }

        protected void OnLayoutGUI()
        {
            DrawToolbar();
            DrawHeader();
            var tabIndex = DrawMenu();

            DrawScrollView(() =>
            {
                OnTabsGUI(tabIndex);
            });

        }

        protected void OnTabsGUI(int tabIndex)
        {
            m_TabsLayout[tabIndex].OnGUI();
        }

        protected void DrawScrollView(Action onContent)
        {
            if (Event.current.type == EventType.Repaint)
            {
                m_HeaderHeight = GUILayoutUtility.GetLastRect().yMax + OneSignalImguiStyles.Padding;
            }

            using (new GuiBeginScrollView(ref m_ScrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - m_HeaderHeight)))
            {

                onContent.Invoke();
                GUILayout.Space(1);
                if (Event.current.type == EventType.Repaint)
                {
                    m_ScrollContentHeight = GUILayoutUtility.GetLastRect().yMax + OneSignalImguiStyles.Padding;
                }

                if (Event.current.type == EventType.Layout)
                {
                    var totalHeight = m_ScrollContentHeight + m_HeaderHeight + 20;
                    if (position.height > totalHeight) {
                        using (new GuiBeginVertical(OneSignalImguiStyles.SeparationStyle)) {
                            GUILayout.Space(position.height - totalHeight);
                        }
                    }
                }
            }
        }

        protected int DrawMenu()
        {
            GUILayout.Space(2);
            int index;
            index = m_MenuToolbar.Draw ();
                GUILayout.Space(4);

            EditorGUILayout.BeginVertical(OneSignalImguiStyles.SeparationStyle);
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

			return index;
        }

        protected void DrawToolbar()
        {
            GUILayout.Space(2);
            using (new GuiBeginHorizontal())
            {
                DrawDocumentationLink();
                EditorGUILayout.Space();
            }
            GUILayout.Space(5);
        }

        void DrawDocumentationLink()
        {
            var width = m_DocumentationLink.CalcSize().x + 5f;
            var clicked = m_DocumentationLink.Draw(GUILayout.Width(width));
            if (clicked) {
                Application.OpenURL(m_DocumentationUrl);
            }
        }

        protected void DrawHeader()
        {
            EditorGUILayout.BeginVertical(OneSignalImguiStyles.SeparationStyle);
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(OneSignalImguiStyles.ContentIndent);
                    EditorGUILayout.LabelField(m_HeaderTitle, OneSignalImguiStyles.LabelHeaderStyle);
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(8);


                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(OneSignalImguiStyles.ContentIndent);
                    EditorGUILayout.LabelField(m_HeaderDescription, OneSignalImguiStyles.DescriptionLabelStyle);
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();

        }

        public static void ShowTowardsInspector()
        {
            var inspectorType = Type.GetType("UnityEditor.InspectorWindow, UnityEditor.dll");
            var window = GetWindow<TWindow>(inspectorType);
            window.Show();
            window.minSize = new Vector2(350, 100);
        }
    }
}
