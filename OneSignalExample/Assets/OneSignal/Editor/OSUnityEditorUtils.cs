using System;
using System.IO;

using UnityEditor;
using UnityEngine;

internal class OSUnityEditorUtils {

    // Searches for a list of file names accross the whole Unity project and provides
    // the the GUID of the first found result.
    internal static string FindFirstFileGUIDByName(String[] filenames) {
        foreach (var filename in filenames) {
            var pathGUIDs = AssetDatabase.FindAssets(filename);
            if (pathGUIDs.Length > 0)
                return pathGUIDs[0];
        }
        return null;
    }

    internal static void AppendFileExtensionIfMissing(string pathedFile, string extension) {
        if (pathedFile == null) {
            Debug.LogError($"AppendFileExtensionIfMissing: pathedFile can not be null");
            return;
        }

        var pathParts = pathedFile.Split(Path.DirectorySeparatorChar);
        var fileName = pathParts[pathParts.Length - 1];
        if (!fileName.Contains("." + extension)) {
            var result = AssetDatabase.MoveAsset(pathedFile, $"{pathedFile}.{extension}");
            if (!String.IsNullOrEmpty(result))
                Debug.LogError($"Could not add '.{extension}' to '{pathedFile}' due to error: {result}");
        }
    }
}