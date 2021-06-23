using UnityEditor;
using UnityEngine;


class SettingsTab : WindowTabElement
{
    public override void OnGUI()
    {
        var settings = OneSignalSettings.Instance;
        using (new WindowBlockWithIndent(new GUIContent("Settings")))
        {
            EditorGUILayout.HelpBox(
                "When Application Id is provided here, you may skip an appID param in the OneSignal.StartInit method.",
                MessageType.Info);
            settings.ApplicationId = EditorGUILayout.TextField("Application Id: ", settings.ApplicationId);
        }
    }
}