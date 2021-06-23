using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[Serializable]
class HyperToolbar
{
    [SerializeField] int m_ButtonsWidth = 0;
    [SerializeField] int m_ButtonsHeight = 0;
    [SerializeField] float m_ItemsSpace = 5f;
    [SerializeField] List<HyperLabel> m_Buttons = new List<HyperLabel>();

    public List<HyperLabel> Buttons => m_Buttons;

    public int SelectionIndex =>
        (from button in m_Buttons where button.IsSelectionLock select m_Buttons.IndexOf(button)).FirstOrDefault();

    public void AddButtons(params HyperLabel[] buttons)
    {
        if (m_Buttons == null)
            m_Buttons = new List<HyperLabel>();

        foreach (var newBtn in buttons)
            m_Buttons.Add(newBtn);

        ValidateButtons();
    }

    void ValidateButtons()
    {
        if (m_Buttons.Count == 0)
            return;

        var hasActive = m_Buttons.Any(button => button.IsSelectionLock);
        if (!hasActive)
            m_Buttons[0].LockSelectedState(true);
    }

    public void SetSelectedIndex(int index)
    {
        foreach (var button in m_Buttons)
        {
            button.LockSelectedState(false);
        }

        var selectedButton = m_Buttons[index];
        selectedButton.LockSelectedState(true);
    }

    public int Draw()
    {
        if (m_Buttons == null)
        {
            return 0;
        }

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.Space();

            foreach (var button in m_Buttons)
            {
                float width;
                if (m_ButtonsWidth == 0)
                    width = button.CalcSize().x + m_ItemsSpace;
                else
                    width = m_ButtonsWidth;

                bool click;
                click = m_ButtonsHeight != 0
                    ? button.Draw(GUILayout.Width(width), GUILayout.Height(m_ButtonsHeight))
                    : button.Draw(GUILayout.Width(width));

                if (click)
                {
                    SetSelectedIndex(m_Buttons.IndexOf(button));
                }
            }

            EditorGUILayout.Space();
        }
        EditorGUILayout.EndHorizontal();

        return SelectionIndex;
    }
}