using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OneSignalDemo.Editor
{
    /// <summary>
    /// Turns on Firebase Installation IDs when the FIREBASE_INSTALLATION_ID_ENABLED
    /// scripting define is set (E2E adds it through Assets/csc.rsp).
    /// </summary>
    public class FirebaseInstallationIdBuild
        : IPreprocessBuildWithReport,
            IPostGenerateGradleAndroidProject
    {
        private const string GoogleServicesFileName = "google-services.json";
        private const string MetaDataName = "firebase_messaging_installation_id_enabled";
        private const string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

#if FIREBASE_INSTALLATION_ID_ENABLED
        private static readonly bool Enabled = true;
#else
        private static readonly bool Enabled = false;
#endif

        public int callbackOrder => 0;

        private static string GoogleServicesPath =>
            Path.Combine(Path.GetDirectoryName(Application.dataPath), GoogleServicesFileName);

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            if (Enabled && !File.Exists(GoogleServicesPath))
            {
                throw new BuildFailedException(
                    $"FIREBASE_INSTALLATION_ID_ENABLED requires examples/demo/{GoogleServicesFileName}"
                );
            }
        }

        public void OnPostGenerateGradleAndroidProject(string unityLibraryPath)
        {
            // The launcher template applies the Google Services plugin only when this copy exists.
            var launcherCopy = Path.Combine(
                unityLibraryPath,
                "..",
                "launcher",
                GoogleServicesFileName
            );
            if (File.Exists(GoogleServicesPath))
                File.Copy(GoogleServicesPath, launcherCopy, overwrite: true);
            else if (File.Exists(launcherCopy))
                File.Delete(launcherCopy);

            // Always write the value: Unity's incremental export keeps the previous build's manifest.
            SetManifestValue(Path.Combine(unityLibraryPath, "src", "main", "AndroidManifest.xml"));
        }

        private static void SetManifestValue(string manifestPath)
        {
            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.Load(manifestPath);
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("android", AndroidXmlNamespace);

            var metaData =
                doc.SelectSingleNode(
                    $"/manifest/application/meta-data[@android:name='{MetaDataName}']",
                    ns
                ) as XmlElement;
            if (metaData == null)
                throw new BuildFailedException($"{MetaDataName} meta-data is missing from {manifestPath}");

            metaData.SetAttribute("value", AndroidXmlNamespace, Enabled ? "true" : "false");
            doc.Save(manifestPath);
        }
    }
}
