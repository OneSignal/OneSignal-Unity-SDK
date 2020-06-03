using Com.OneSignal.Editor.IMGUI;
using UnityEditor;
using UnityEngine;

namespace Com.OneSignal.Editor
{
    class SettingsTab : WindowTabElement
    {
        public override void OnGUI()
        {
            var settings = OneSignalSettings.Instance;
            using (new WindowBlockWithIndent(new GUIContent("Settings")))
            {
                settings.ApplicationId = EditorGUILayout.TextField("Application Id: ", settings.ApplicationId);
            }
        }
    }
}
