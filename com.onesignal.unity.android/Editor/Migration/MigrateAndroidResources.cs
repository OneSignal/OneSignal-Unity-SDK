using System.IO;
using UnityEditor;

namespace OneSignalSDK {

    [InitializeOnLoad]
    sealed class MigrateAndroidResources {
        static MigrateAndroidResources() {
            UpdateBuildDotGradleContains();
        }

        /// <summary>
        /// Updates Assets/Plugins/Android/OneSignalConfig.androidlib/build.gradle
        /// with contains provided by OneSignal-Unity-SDK 5.1.13.
        /// Includes compatibility with Unity 6, as it's Gradle version has new
        /// requirements.
        /// </summary>
        private static void UpdateBuildDotGradleContains() {
            if (!Directory.Exists(ExportAndroidResourcesStep._pluginExportPath))
                return;

            string exportedFilename = Path.Combine(
                ExportAndroidResourcesStep._pluginExportPath,
                "build.gradle"
            );
            string exportedContains = File.ReadAllText(exportedFilename);

            string packageFilename = Path.Combine(
                ExportAndroidResourcesStep._pluginPackagePath,
                "build.gradle"
            );
            string packageContains = File.ReadAllText(packageFilename);

            // We want to copy only when needed, otherwise it can reset file
            // properties, such as permissions and timestamps
            if (exportedContains != packageContains) {
                File.Copy(packageFilename, exportedFilename, true);
            }
        }
    }
}
