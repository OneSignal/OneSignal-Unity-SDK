using System.Linq;
using UnityEditor;

/// <summary>
/// Handles informing the user on startup/import if the legacy SDK has been detected
/// </summary>
public static class Bootstrapper
{
    /// <summary>
    /// Asks to open the installer if legacy files are found
    /// </summary>
    [InitializeOnLoadMethod]
    public static void CheckForLegacy()
    {
        if (SessionState.GetBool(_sessionCheckKey, false))
            return;
        
        SessionState.SetBool(_sessionCheckKey, true);
        
        var inventory = AssetDatabase.LoadAssetAtPath<OneSignalFileInventory>(OneSignalFileInventory.AssetPath);

        if (inventory == null)
            return; // error
        
        var diff = inventory.CurrentPaths.Except(inventory.DistributedPaths);

        if (!diff.Any())
            return;
        
        var dialogResult = EditorUtility.DisplayDialog(
            "OneSignal",
            "The project contains an outdated version of OneSignal SDK! We recommend running the OneSignal installer.",
            "Open Installer",
            "Cancel"
        );

        if (dialogResult) 
            OneSignalInstallerWindow.ShowWindow();
    }

    private const string _sessionCheckKey = "onesignal.bootstrapper.check";
}