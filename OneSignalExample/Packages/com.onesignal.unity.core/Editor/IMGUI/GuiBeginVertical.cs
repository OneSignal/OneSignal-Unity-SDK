using System;
using UnityEditor;
using UnityEngine;

namespace Com.OneSignal.Editor.IMGUI
{
    class GuiBeginVertical : IDisposable
    {
        public GuiBeginVertical(params GUILayoutOption[] layoutOptions) {
            EditorGUILayout.BeginVertical(layoutOptions);
        }

        public GuiBeginVertical(GUIStyle style,  params GUILayoutOption[] layoutOptions) {
            EditorGUILayout.BeginVertical(style, layoutOptions);
        }

        public void Dispose() {
            EditorGUILayout.EndVertical();
        }
    }
}
