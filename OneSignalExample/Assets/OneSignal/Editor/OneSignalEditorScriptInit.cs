#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public class OneSignalEditorScriptInit : AssetPostprocessor {
    static OneSignalEditorScriptInit() {
        OneSignalFileStructureUpgrade.DoUpgrade();
        #if UNITY_ANDROID
        OneSignalEditorScriptAndroid.createOneSignalAndroidManifest();
        #endif
        #if !UNITY_CLOUD_BUILD && UNITY_2017_1_OR_NEWER
        OneSignalEditorCheckUpdateScript.Request();
        #endif
    }
}
#endif