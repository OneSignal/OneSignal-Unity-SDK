using System.IO;
using UnityEditor;

/// <summary>
/// For debugging all of the OneSignalSetupSteps
/// </summary>
public static class OneSignalSetupReset
{
    /// <summary>
    /// Resets all setup steps
    /// </summary>
    [MenuItem("OneSignal/Reset All Setup Steps", false, 100)]
    public static void ResetAllSteps()
    {
        /*
         * ExportAndroidResourcesStep
         * deletes the OneSignalConfig.plugin directory
         */
        AssetDatabase.DeleteAsset(Path.Combine("Assets", "Plugins", "Android", "OneSignalConfig.plugin"));

        /*
         * InstallEdm4UStep
         * deletes the edm4u directory
         */
        AssetDatabase.DeleteAsset(Path.Combine("Assets", "ExternalDependencyManager"));

        /*
         * SetupManifestStep
         * handled by ExportAndroidResourcesStep
         */
        // do nothing

        /*
         * CleanUpLegacyStep
         * adds a random file to the Assets/OneSignal folder
         */
        File.Create(Path.Combine("Assets", "OneSignal", "tempfile"));

        /*
         * ImportPackagesStep
         * removes packages from manifest
         */
        var manifest = new Manifest();
        manifest.Fetch();
        manifest.RemoveDependency("com.onesignal.unity.core");
        manifest.RemoveDependency("com.onesignal.unity.android");
        manifest.RemoveDependency("com.onesignal.unity.ios");
        manifest.RemoveScopeRegistry("https://registry.npmjs.org");
        manifest.ApplyChanges();
        
        UnityEditor.PackageManager.Client.Resolve();
    }
}