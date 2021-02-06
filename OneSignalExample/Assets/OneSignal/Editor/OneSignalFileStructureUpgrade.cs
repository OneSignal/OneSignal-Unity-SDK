#if UNITY_EDITOR

// Add any files or folders that need to be rename, moved, or deleted in
// as part of the process when updating this SDK.
internal class OneSignalFileStructureUpgrade {

   internal static void DoUpgrade() {
      UpgradeToSDKVersion2_13_3();
   }

   private static void UpgradeToSDKVersion2_13_3() {
      #if UNITY_ANDROID
      RenameAndroidOneSignalConfig();
      #endif
   }

   // This renames the folder "OneSignalConfig" to "OneSignal.plugin".
   // ".plugin" is Unity documented folder post-fix required for some plugins features.
   //    - This is required to fix Unity 2020+ capability.
   // This is done via a script since .unitypackage files do not support renaming
   // and so this is required for those upgrading from an older version of the SDK.
   private static void RenameAndroidOneSignalConfig() {
      var path = OneSignalFileLocator.GetOneSignalConfigFolderNameWithPath();
      OSUnityEditorUtils.AppendFileExtensionIfMissing(path, "plugin");
   }
}
#endif