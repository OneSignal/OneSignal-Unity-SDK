using System;

using UnityEditor;
using UnityEngine;

internal class OneSignalFileLocator {

    // Returns the full path of the OneSignalConfig folder.
    // The folder contains a AndroidManifest.xml file which is required
    // for Android push notifications to be received by the app.
    internal static string GetOneSignalConfigFolderNameWithPath() {
        var guid = GetOneSignalConfigGUID();
        if (guid != null)
            return AssetDatabase.GUIDToAssetPath(guid);

        Debug.LogError($"OneSignal could not find required OneSignalConfig folder, push notifications will NOT display on Android!");
        return null;
    }

    private static string GetOneSignalConfigGUID() {
        // 1. Attempt to get GUID for folder
        //    - This will be successful unless the Unity Dev deletes the matching .meta file or
        //    moves the folder outside of Unity.
        var guid = GetOneSignalConfigGUIDIfAvailable();
        if (guid != null)
            return guid;
       
        Debug.Log($"Didn't find '{FOLDER_NAME_V1_ONESIGNAL_CONFIG_NAME}' by GUID '{FOLDER_GUID_ONESIGNAL_CONFIG}', seaching by name.");
    
        // 2. Attempt by name if GUID look up fails
        return GetOneSignalConfigGUIDByName();
    }

    // This is a static GUID defined in the OneSignalConfig.plugin.meta file in this repo.
    private static readonly string FOLDER_GUID_ONESIGNAL_CONFIG = "4913d3bd89f197541850fb5382f2456d";
    // Only provides GUID if it is found in project
    private static string GetOneSignalConfigGUIDIfAvailable() {
        var path = AssetDatabase.GUIDToAssetPath(FOLDER_GUID_ONESIGNAL_CONFIG);
        if (!String.IsNullOrEmpty(path))
            return FOLDER_GUID_ONESIGNAL_CONFIG;
        return null;
    }

    private static readonly string FOLDER_NAME_V1_ONESIGNAL_CONFIG_NAME = "OneSignalConfig";
    private static readonly string FOLDER_NAME_V2_ONESIGNAL_CONFIG_NAME = "OneSignalConfig.plugin";
    private static readonly string[] FOLDER_NAME_ONESIGNAL_CONFIG_NAMES = new String[] {
        FOLDER_NAME_V2_ONESIGNAL_CONFIG_NAME,
        FOLDER_NAME_V1_ONESIGNAL_CONFIG_NAME
    };
    // Returns GUID of first found folder by name, null if not found.
    private static string GetOneSignalConfigGUIDByName() {
        var guid = OSUnityEditorUtils.FindFirstFileGUIDByName(FOLDER_NAME_ONESIGNAL_CONFIG_NAMES);
        if (guid != null)
            return guid;

        var folderNames = String.Join(",", FOLDER_NAME_ONESIGNAL_CONFIG_NAMES);
        Debug.LogError($"OneSignal - Could not find required Android folder from list of names; '{folderNames}'");
        return null;
    }
}