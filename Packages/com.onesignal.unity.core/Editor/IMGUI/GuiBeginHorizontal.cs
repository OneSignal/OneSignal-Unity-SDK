using System;
using UnityEditor;
using UnityEngine;

namespace OneSignalPush.Editor.IMGUI
{
    class GuiBeginHorizontal : IDisposable
    {
        public GuiBeginHorizontal(params GUILayoutOption[] layoutOptions)
        {
            EditorGUILayout.BeginHorizontal(layoutOptions);
        }

        public GuiBeginHorizontal(GUIStyle style, params GUILayoutOption[] layoutOptions)
        {
            EditorGUILayout.BeginHorizontal(style, layoutOptions);
        }

        public void Dispose()
        {
            EditorGUILayout.EndHorizontal();
        }
    }
}
