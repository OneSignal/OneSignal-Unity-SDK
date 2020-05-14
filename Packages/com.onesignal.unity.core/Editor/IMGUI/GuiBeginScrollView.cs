using System;
using UnityEngine;

namespace OneSignalPush.Editor.IMGUI
{
    class GuiBeginScrollView : IDisposable
	{
        public GuiBeginScrollView(ref Vector2 scrollPosition)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        }

        public GuiBeginScrollView(ref Vector2 scrollPosition, params GUILayoutOption[] options)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, options);
        }

        public GuiBeginScrollView(Vector2 scrollPosition, GUIStyle style)
        {
            GUILayout.BeginScrollView(scrollPosition, style);
        }

        public GuiBeginScrollView(Vector2 scrollPosition, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.BeginScrollView(scrollPosition, style, options);
        }

        public GuiBeginScrollView(Vector2 scrollPosition, GUIStyle horizontalScrollBar, GUIStyle verticalScrollBar)
        {
            GUILayout.BeginScrollView(scrollPosition, horizontalScrollBar, verticalScrollBar);
        }

        public GuiBeginScrollView(Vector2 scrollPosition, bool alwaysShowHorizontalScrollBar,
            bool alwaysShowVerticalScrollBar, GUIStyle horizontalScrollBar, GUIStyle verticalScrollBar)
        {
            GUILayout.BeginScrollView(scrollPosition, alwaysShowHorizontalScrollBar, alwaysShowVerticalScrollBar,
                horizontalScrollBar, verticalScrollBar);
        }

        public GuiBeginScrollView(Vector2 scrollPosition, bool alwaysShowHorizontalScrollBar,
            bool alwaysShowVerticalScrollBar)
        {
            GUILayout.BeginScrollView(scrollPosition, alwaysShowHorizontalScrollBar, alwaysShowVerticalScrollBar);
        }

        public GuiBeginScrollView(Vector2 scrollPosition, GUIStyle horizontalScrollBar, GUIStyle verticalScrollBar,
            params GUILayoutOption[] options)
        {
            GUILayout.BeginScrollView(scrollPosition, horizontalScrollBar, verticalScrollBar, options);
        }

        public void Dispose()
        {
            GUILayout.EndScrollView();
        }
    }
}
