using System;
using UnityEditor;
using UnityEngine;

namespace OneSignalPush.Editor.IMGUI
{
    public class SA_GuiBeginHorizontal : IDisposable
    {
        public SA_GuiBeginHorizontal(params GUILayoutOption[] layoutOptions) {
            EditorGUILayout.BeginHorizontal(layoutOptions);
        }

        public SA_GuiBeginHorizontal(GUIStyle style, params GUILayoutOption[] layoutOptions) {
            EditorGUILayout.BeginHorizontal(style, layoutOptions);
        }

        public void Dispose() {
            EditorGUILayout.EndHorizontal();
        }
    }
}
