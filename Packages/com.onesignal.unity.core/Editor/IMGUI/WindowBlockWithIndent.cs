using System;
using UnityEditor;
using UnityEngine;

namespace Com.OneSignal.Editor.IMGUI
{
    public class WindowBlockWithIndent : IDisposable
    {
        public WindowBlockWithIndent(GUIContent header)
        {
            if (header.image != null)
            {
                header.text = " " + header.text;
            }

            GUILayout.Space(10);
            using (new GuiBeginHorizontal())
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField(header, OneSignalImguiStyles.ServiceBlockHeader);
            }

            GUILayout.Space(5);
            EditorGUI.indentLevel++;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel--;
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(OneSignalImguiStyles.SeparationStyle);
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }
    }
}
