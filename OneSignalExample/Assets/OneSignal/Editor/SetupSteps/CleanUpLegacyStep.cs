using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

/// <summary>
/// Handles if there are files within the Assets/OneSignal folder which should not be there. Typically this
/// indicates the presence of legacy files.
/// </summary>
public sealed class CleanUpLegacyStep : OneSignalSetupStep
{
    public override string Summary
        => "Remove legacy files";

    public override string Details
        => "Checks for the diff between the files distributed with the package and those which are in the " +
           OneSignalFileInventory.PackageAssetsPath;

    public override bool IsRequired 
        => true;

    protected override bool _getIsStepCompleted()
    {
        var diff = _getDiff();
        
        if (diff == null)
            return true; // error
        
        return !diff.Any();
    }

    protected override void _runStep()
    {
        var diff = _getDiff();
        
        if (diff == null)
            return; // error

        foreach (var path in diff)
            File.Delete(path);
    }

    private IEnumerable<string> _getDiff() 
    {
        if (_inventory == null)
            _inventory = AssetDatabase.LoadAssetAtPath<OneSignalFileInventory>(OneSignalFileInventory.AssetPath);

        if (_inventory == null)
            return null; // error
        
        var currentPaths = OneSignalFileInventory.GetCurrentPaths();
        return currentPaths.Except(_inventory.DistributedPaths);
    }

    private OneSignalFileInventory _inventory;
}