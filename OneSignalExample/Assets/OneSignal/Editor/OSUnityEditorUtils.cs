using System;
using System.IO;

using UnityEditor;
using UnityEngine;

 internal class OSUnityEditorUtils {
     
    // Searches for a list of file names accross the whole Unity project and provides
    // the the GUID of the first found result.
     internal static string FindFirstFileGUIDByName(String[] filenames) {
        foreach(var filename in filenames) {
           var pathGUIDs = AssetDatabase.FindAssets(filename);
            if (pathGUIDs.Length > 0)
                return pathGUIDs[0];
        }
        return null;
    }

    internal static void AppendFileExtensionIfMissing(string pathedFile, string extension) {
       if (pathedFile == null) {
           Debug.LogError($"Could not find {pathedFile} to add file extension.");
           return;
       }

      var pathParts = pathedFile.Split(Path.DirectorySeparatorChar);
      var fileName = pathParts[pathParts.Length - 1];
      if (!fileName.Contains(extension))
          AssetDatabase.MoveAsset(pathedFile, $"{pathedFile}.{extension}");
   }
 }