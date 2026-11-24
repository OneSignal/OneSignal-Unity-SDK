using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OneSignalDemo.Editor
{
    public class CopyEnvPreBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var source = Path.Combine(projectRoot, ".env");
            var dest = Path.Combine(Application.streamingAssetsPath, ".env");

            // Without a .env the app falls back to the shared example app; drop any
            // copy left by an earlier build so it doesn't keep stale values.
            if (!File.Exists(source))
            {
                foreach (var path in new[] { dest, dest + ".meta" })
                    if (File.Exists(path))
                        File.Delete(path);
                Debug.Log("[OneSignalDemo] No .env; using the default app ID");
                return;
            }

            Directory.CreateDirectory(Application.streamingAssetsPath);
            File.Copy(source, dest, overwrite: true);
            Debug.Log("[OneSignalDemo] Copied .env to StreamingAssets");
        }
    }
}
