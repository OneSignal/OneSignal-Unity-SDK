using System.IO;
using UnityEngine;

/// <summary>
/// Inventory distributed with the *.unitypackage in order to determine if there are any legacy files in need of removal
/// </summary>
internal sealed class OneSignalFileInventory : ScriptableObject
{
    /// <summary>
    /// Array of paths within the OneSignal directory which were determined to be part of the distributed unitypackage
    /// </summary>
    public string[] DistributedPaths;

    /// <summary>
    /// Array of current paths within the OneSignal directory
    /// </summary>
    public string[] CurrentPaths
        => _currentPaths ??= Directory.GetFiles(PackageAssetsPath, "*", SearchOption.AllDirectories);

    public const string PackageAssetsPath = "Assets/OneSignal";
    public const string AssetName = "OneSignalFileInventory.asset";
    public const string EditorResourcesPath = "Assets/OneSignal/Editor/Resources";
    public static readonly string AssetPath = $"{EditorResourcesPath}/{AssetName}";

    private string[] _currentPaths;
}