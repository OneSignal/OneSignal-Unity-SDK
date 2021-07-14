using System;
using UnityEditor;
using UnityEngine;


class GuiBeginVertical : IDisposable
{
    public GuiBeginVertical(params GUILayoutOption[] layoutOptions)
    {
        EditorGUILayout.BeginVertical(layoutOptions);
    }

    public GuiBeginVertical(GUIStyle style, params GUILayoutOption[] layoutOptions)
    {
        EditorGUILayout.BeginVertical(style, layoutOptions);
    }

    public void Dispose()
    {
        EditorGUILayout.EndVertical();
    }
}