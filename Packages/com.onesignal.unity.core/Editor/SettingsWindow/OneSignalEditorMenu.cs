using UnityEditor;

namespace OneSignalPush.Editor
{
    static class OneSignalEditorMenu
    {
        [MenuItem("Window/" + OneSignalSettings.PluginName)]
        public static void Services()
        {
            OneSignalSettingsWindow.ShowTowardsInspector();
        }
    }
}
