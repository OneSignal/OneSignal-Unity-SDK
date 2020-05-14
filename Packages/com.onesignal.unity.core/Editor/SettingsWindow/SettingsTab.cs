using OneSignalPush.Editor.IMGUI;
using UnityEditor;
using UnityEngine;

namespace OneSignalPush.Editor
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
