using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace AssetStoreTools.Utility
{
    internal static class FileUtility
    {
        private class RenameInfo
        {
            public string OriginalName;
            public string CurrentName;
        }

        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        public static bool IsMissingMetaFiles(params string[] sourcePaths)
        {
            foreach (var sourcePath in sourcePaths)
            {
                var allDirectories = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
                foreach (var dir in allDirectories)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    if (dirInfo.Name.EndsWith("~"))
                    {
                        var nestedContent = dirInfo.GetFileSystemInfos("*", SearchOption.AllDirectories);

                        foreach (var nested in nestedContent)
                        {
                            // .meta files, hidden files and OSX .DS_STORE files do not require their own metas
                            if (nested.FullName.EndsWith(".meta")
                                || nested.FullName.EndsWith("~")
                                || nested.Name.Equals(".DS_Store"))
                                continue;

                            if (!File.Exists(nested.FullName + ".meta"))
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        public static void GenerateMetaFiles(params string[] sourcePaths)
        {
            var renameInfos = new List<RenameInfo>();

            foreach (var sourcePath in sourcePaths)
            {
                var hiddenDirectoriesInPath = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories).Where(x => x.EndsWith("~"));
                foreach (var hiddenDir in hiddenDirectoriesInPath)
                    renameInfos.Add(new RenameInfo() { CurrentName = hiddenDir, OriginalName = hiddenDir });
            }


            try
            {
                EditorApplication.LockReloadAssemblies();

                // Order paths from longest to shortest to avoid having to rename them multiple times
                renameInfos = renameInfos.OrderByDescending(x => x.OriginalName.Length).ToList();

                try
                {
                    AssetDatabase.StartAssetEditing();
                    foreach (var renameInfo in renameInfos)
                    {
                        renameInfo.CurrentName = renameInfo.OriginalName.TrimEnd('~');
                        Directory.Move(renameInfo.OriginalName, renameInfo.CurrentName);
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                }

                // Restore the original path names in reverse order
                renameInfos = renameInfos.OrderBy(x => x.OriginalName.Length).ToList();

                try
                {
                    AssetDatabase.StartAssetEditing();
                    foreach (var renameInfo in renameInfos)
                    {
                        Directory.Move(renameInfo.CurrentName, renameInfo.OriginalName);

                        if (File.Exists($"{renameInfo.CurrentName}.meta"))
                            File.Delete($"{renameInfo.CurrentName}.meta");
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                }
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
            }
        }
    }
}