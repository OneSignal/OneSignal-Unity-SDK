using System.IO;
using System.Linq;
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
    public static string[] GetCurrentPaths()
        => ConvertPathsToUnix(Directory.GetFiles(PackageAssetsPath, "*", SearchOption.AllDirectories));

    /// <summary>
    /// Makes sure <see cref="paths"/> are using forward slash to be Unix compatible.
    /// https://docs.microsoft.com/en-us/dotnet/api/system.io.path.altdirectoryseparatorchar?view=net-5.0#examples
    /// </summary>
    /// <param name="paths">the paths to check and convert</param>
    /// <returns>paths with / as the directory separator</returns>
    public static string[] ConvertPathsToUnix(string[] paths) {
        if (Path.DirectorySeparatorChar == Path.AltDirectorySeparatorChar) 
            return paths;
        
        var fixedPaths = paths.Select(path =>
            path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        );

        return fixedPaths.ToArray();
    }

    public const string AssetName = "OneSignalFileInventory.asset";
    public static readonly string PackageAssetsPath = Path.Combine("Assets", "OneSignal");
    public static readonly string EditorResourcesPath = Path.Combine(PackageAssetsPath, "Editor", "Resources");
    public static readonly string AssetPath = Path.Combine(EditorResourcesPath, AssetName);
}