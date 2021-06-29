using System.Linq;
using UnityEditor;
#if UNITY_2020
using System;
using System.Collections.Generic;
using UnityEngine;
#endif

static class CleanUpUtility
{
    internal static void RemoveDirectories(params string[] directories)
    {
        var validDirectories = directories.Where(AssetDatabase.IsValidFolder).ToArray();
        if (validDirectories.Any())
        {
#if UNITY_2020
            var failedPathsList = new List<string>();
            if (!AssetDatabase.DeleteAssets(validDirectories, failedPathsList))
            {
                var pathsCombined = string.Join(Environment.NewLine, failedPathsList);
                Debug.LogError($"Failed to remove following assets:{Environment.NewLine}{pathsCombined}");
            }
#else
                foreach (var path in validDirectories) {
                    FileUtil.DeleteFileOrDirectory(path);
                    FileUtil.DeleteFileOrDirectory(path + ".meta");
                }

                AssetDatabase.Refresh();
#endif
        }
    }
}