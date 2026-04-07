using System.IO;
using UnityEditor;
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

            if (!File.Exists(source))
            {
                Debug.LogWarning(
                    "[OneSignalDemo] No .env file found at project root. "
                        + "Live Activity API calls will be disabled. "
                        + "Copy .env.example to .env and add your key."
                );
                if (File.Exists(dest))
                    File.Delete(dest);
                return;
            }

            Directory.CreateDirectory(Application.streamingAssetsPath);
            File.Copy(source, dest, overwrite: true);
            Debug.Log("[OneSignalDemo] Copied .env to StreamingAssets");
        }
    }
}
