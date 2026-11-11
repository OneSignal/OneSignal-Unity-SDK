using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OneSignalDemo.Editor
{
    public class CopyEnvPreBuild : IPreprocessBuildWithReport
    {
        private static readonly Regex AppIdLine = new Regex(
            @"^[ \t]*ONESIGNAL_APP_ID[ \t]*=[ \t]*[^\s#]",
            RegexOptions.Multiline
        );

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var source = Path.Combine(projectRoot, ".env");
            var dest = Path.Combine(Application.streamingAssetsPath, ".env");

            if (!File.Exists(source) || !AppIdLine.IsMatch(File.ReadAllText(source)))
            {
                throw new BuildFailedException(
                    "ONESIGNAL_APP_ID is not set. Add it to examples/demo/.env."
                );
            }

            Directory.CreateDirectory(Application.streamingAssetsPath);
            File.Copy(source, dest, overwrite: true);
            Debug.Log("[OneSignalDemo] Copied .env to StreamingAssets");
        }
    }
}
