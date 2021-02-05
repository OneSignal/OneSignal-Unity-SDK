#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public class OneSignalEditorScriptInit : AssetPostprocessor {
    static OneSignalEditorScriptInit() {
        OneSignalFileStructureUpgrade.DoUpgrade();
        #if UNITY_ANDROID
        OneSignalEditorScriptAndroid.createOneSignalAndroidManifest();
        #endif
    }
}
#endif