using System;
using UnityEngine;

namespace OneSignalPush.Editor.IMGUI
{
    class GuiChangeContentColor : IDisposable
    {
        Color PreviousColor { get; }

        public GuiChangeContentColor(string htmlColor)
        {
            PreviousColor = GUI.contentColor;

            ColorUtility.TryParseHtmlString(htmlColor, out var color);
            GUI.contentColor = color;
        }

        public GuiChangeContentColor(Color newColor)
        {
            PreviousColor = GUI.contentColor;
            GUI.contentColor = newColor;
        }

        public void Dispose()
        {
            GUI.contentColor = PreviousColor;
        }
    }
}
