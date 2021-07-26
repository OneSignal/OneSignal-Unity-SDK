using System.Linq;
using UnityEditor;

/// <summary>
/// Handles informing the user on startup/import if the legacy SDK has been detected
/// </summary>
public static class OneSignalBootstrapper
{
    /// <summary>
    /// Asks to open the SDK Setup if legacy files are found or core is missing
    /// </summary>
    [InitializeOnLoadMethod]
    public static void CheckForLegacy()
    {
        if (SessionState.GetBool(_sessionCheckKey, false))
            return;

        SessionState.SetBool(_sessionCheckKey, true);
        
        EditorApplication.delayCall += _checkForLegacy;
    }

    private static void _checkForLegacy()
    {
#if !ONE_SIGNAL_INSTALLED
        EditorApplication.delayCall += _showOpenSetupDialog;
#else
        var inventory = AssetDatabase.LoadAssetAtPath<OneSignalFileInventory>(OneSignalFileInventory.AssetPath);

        if (inventory == null)
            return; // error
        
        var currentPaths = OneSignalFileInventory.GetCurrentPaths();
        var diff = currentPaths.Except(inventory.DistributedPaths);

        if (diff.Any())
            EditorApplication.delayCall += _showOpenSetupDialog;
#endif
    }

    private static void _showOpenSetupDialog()
    {
        var dialogResult = EditorUtility.DisplayDialog(
            "OneSignal",
            "The project contains an outdated version of OneSignal SDK! We recommend running the OneSignal SDK Setup.",
            "Open SDK Setup",
            "Cancel"
        );

        if (dialogResult)
            OneSignalSetupWindow.ShowWindow();
    }

    private const string _sessionCheckKey = "onesignal.bootstrapper.check";
}