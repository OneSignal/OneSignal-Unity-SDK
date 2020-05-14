using System;
using UnityEngine;

namespace OneSignalPush.Editor.IMGUI
{
    public class SA_GuiChangeContentColor : IDisposable
    {
        private Color PreviousColor { get; set; }


        public SA_GuiChangeContentColor(string htmlColor) {
            PreviousColor = GUI.contentColor;

            Color color = PreviousColor;
            ColorUtility.TryParseHtmlString(htmlColor, out color);
            GUI.contentColor = color;

        }

        public SA_GuiChangeContentColor(Color newColor) {
            PreviousColor = GUI.contentColor;
            GUI.contentColor = newColor;
        }

        public void Dispose() {
            GUI.contentColor = PreviousColor;
        }
    }
}
