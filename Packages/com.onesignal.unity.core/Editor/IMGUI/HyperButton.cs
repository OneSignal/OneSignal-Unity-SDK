using System;
using UnityEditor;
using UnityEngine;

namespace OneSignalPush.Editor.IMGUI
{
    [Serializable]
	abstract class HyperButton
    {
        bool m_IsMouseOver;
        Rect m_LabelRect;

        [SerializeField] bool m_IsSelected;

        protected abstract void OnNormal(params GUILayoutOption[] options);
        protected abstract void OnMouseOver(params GUILayoutOption[] options);

        public bool IsSelectionLock => m_IsSelected;

        public void LockSelectedState(bool val)
        {
            m_IsSelected = val;
        }

        public virtual bool Draw(params GUILayoutOption[] options)
        {
            if(m_IsSelected)
            {
                OnMouseOver(options);
                return false;
            }

            if(!m_IsMouseOver) {
                OnNormal(options);
            } else {
                OnMouseOver(options);
            }

            if (Event.current.type == EventType.Repaint)
            {
                m_LabelRect = GUILayoutUtility.GetLastRect();
                m_IsMouseOver = m_LabelRect.Contains(Event.current.mousePosition);
            }

            if (Event.current.type == EventType.Repaint)
            {
                if (m_IsMouseOver)
                {
                    EditorGUIUtility.AddCursorRect(m_LabelRect, MouseCursor.Link);
                }
            }

            var clicked = false;
            if (m_IsMouseOver)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                    clicked = true;
                    GUI.changed = true;
                    Event.current.Use();
                }
            }

            return clicked;
        }
    }
}
