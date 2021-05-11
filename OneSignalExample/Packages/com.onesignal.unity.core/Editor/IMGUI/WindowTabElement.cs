using UnityEngine;

namespace Com.OneSignal.Editor.IMGUI
{
    abstract class WindowTabElement : ScriptableObject
    {
        public abstract void OnGUI();
    }
}
