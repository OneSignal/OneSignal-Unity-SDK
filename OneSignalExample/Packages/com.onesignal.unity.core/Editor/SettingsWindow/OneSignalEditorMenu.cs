using UnityEditor;

namespace Com.OneSignal.Editor
{
    static class OneSignalEditorMenu
    {
        [MenuItem("Window/" + OneSignalSettings.ProductName)]
        public static void Services()
        {
            OneSignalSettingsWindow.ShowTowardsInspector();
        }

    }
}
