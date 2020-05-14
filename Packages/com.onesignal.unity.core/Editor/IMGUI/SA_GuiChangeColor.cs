using System;
using UnityEngine;

namespace OneSignalPush.Editor.IMGUI
{
    public class SA_GuiChangeColor : IDisposable
    {
        private Color m_previousColor { get; set; }


        public SA_GuiChangeColor(string htmlColor) {
            m_previousColor = GUI.color;

            Color color = m_previousColor;
            ColorUtility.TryParseHtmlString(htmlColor, out color);
            GUI.color = color;

        }


        public SA_GuiChangeColor(Color newColor) {
            m_previousColor = GUI.color;
            GUI.color = newColor;
        }

        public void Dispose() {
            GUI.color = m_previousColor;
        }
    }
}
