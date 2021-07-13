using System.IO;
using System.Linq;
using UnityEditor;

/// <summary>
/// 
/// </summary>
public class CleanUpLegacyStep : OneSignalInstallerStep
{
    public override string Summary
        => "Remove legacy files";

    public override string Details
        => "Checks for the diff between the files distributed with the package and those which are in the " +
           OneSignalFileInventory.PackageAssetsPath;

    public override string DocumentationLink
        => "";

    protected override bool _getIsStepCompleted()
    {
        if (_inventory == null)
            _inventory = AssetDatabase.LoadAssetAtPath<OneSignalFileInventory>(OneSignalFileInventory.AssetPath);

        if (_inventory == null)
            return true;
        
        var diff = _inventory.CurrentPaths.Except(_inventory.DistributedPaths);
        return !diff.Any();
    }

    protected override void _install()
    {
        if (_inventory == null)
            _inventory = AssetDatabase.LoadAssetAtPath<OneSignalFileInventory>(OneSignalFileInventory.AssetPath);

        if (_inventory == null)
            return; // error
        
        var diff = _inventory.CurrentPaths.Except(_inventory.DistributedPaths);

        foreach (var path in diff)
            File.Delete(path);
    }

    private OneSignalFileInventory _inventory;
}