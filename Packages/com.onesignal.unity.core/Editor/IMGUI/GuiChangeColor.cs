using System;
using UnityEngine;

namespace OneSignalPush.Editor.IMGUI
{
    class GuiChangeColor : IDisposable
    {
        Color PreviousColor { get; }

        public GuiChangeColor(string htmlColor)
        {
            PreviousColor = GUI.color;
            ColorUtility.TryParseHtmlString(htmlColor, out var color);
            GUI.color = color;
        }

        public GuiChangeColor(Color newColor)
        {
            PreviousColor = GUI.color;
            GUI.color = newColor;
        }

        public void Dispose()
        {
            GUI.color = PreviousColor;
        }
    }
}
