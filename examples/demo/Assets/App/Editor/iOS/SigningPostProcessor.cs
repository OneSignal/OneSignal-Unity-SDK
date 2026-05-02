#if UNITY_IOS

using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace App.Editor.iOS
{
    /// <summary>
    /// Final iOS post-processor for the demo app. Runs AFTER the OneSignal
    /// SDK and demo widget post-processors so it can correct things they set:
    ///
    ///  1. Flips the main target's aps-environment from "production" (the SDK
    ///     default) to "development". The demo only ever runs on simulator or
    ///     a development device; "production" mismatches the simulator's APNS
    ///     environment and triggers iOS's "Keep receiving notifications?"
    ///     tuning prompt on first delivery (matches what the Flutter demo
    ///     ships with).
    ///
    ///  2. Normalizes extension bundle IDs to short suffixes (`.NSE`, `.LA`)
    ///     to match the Flutter demo and keep provisioning profile names
    ///     consistent across SDKs.
    ///
    ///  3. Pins DEVELOPMENT_TEAM on all targets so a future Manual signing
    ///     setup with the OneSignal-owned profiles works without manual
    ///     fix-up in Xcode.
    /// </summary>
    public class SigningPostProcessor : IPostprocessBuildWithReport
    {
        private const string AppleTeamId = "99SW8E36CT";
        private const string ApsEnvironment = "development";

        private const string NseTargetName = "OneSignalNotificationServiceExtension";
        private const string WidgetTargetName = "OneSignalWidget";

        // Short bundle-id suffixes (match the Flutter demo).
        private const string NseBundleSuffix = "NSE";
        private const string WidgetBundleSuffix = "LA";

        // Run after both demo widget post-processor (45) and SDK
        // post-processor (45). 100 puts us after pod install (50) too.
        public int callbackOrder => 100;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
                return;

            var outputPath = report.summary.outputPath;
            FixupApsEnvironment(outputPath);
            FixupSigningAndBundleIds(outputPath);
        }

        private static void FixupApsEnvironment(string outputPath)
        {
            var project = new PBXProject();
            var projectPath = PBXProject.GetPBXProjectPath(outputPath);
            project.ReadFromString(File.ReadAllText(projectPath));

            var mainTargetGuid = project.GetUnityMainTargetGuid();
            var relPath = project.GetBuildPropertyForAnyConfig(
                mainTargetGuid,
                "CODE_SIGN_ENTITLEMENTS"
            );

            if (string.IsNullOrEmpty(relPath))
            {
                Debug.LogWarning(
                    "[SigningPostProcessor] Main target has no CODE_SIGN_ENTITLEMENTS; "
                        + "skipping aps-environment fixup."
                );
                return;
            }

            var fullPath = Path.Combine(outputPath, relPath);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning(
                    $"[SigningPostProcessor] Entitlements file not found at {fullPath}; skipping."
                );
                return;
            }

            var plist = new PlistDocument();
            plist.ReadFromFile(fullPath);
            plist.root.SetString("aps-environment", ApsEnvironment);
            plist.WriteToFile(fullPath);

            Debug.Log(
                $"[SigningPostProcessor] Set aps-environment=\"{ApsEnvironment}\" in {relPath}"
            );
        }

        private static void FixupSigningAndBundleIds(string outputPath)
        {
            var project = new PBXProject();
            var projectPath = PBXProject.GetPBXProjectPath(outputPath);
            project.ReadFromString(File.ReadAllText(projectPath));

            var appId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);

            ApplyTeamId(project, project.GetUnityMainTargetGuid(), "Unity-iPhone");

            ApplyExtensionFixup(
                project,
                NseTargetName,
                $"{appId}.{NseBundleSuffix}"
            );
            ApplyExtensionFixup(
                project,
                WidgetTargetName,
                $"{appId}.{WidgetBundleSuffix}"
            );

            File.WriteAllText(projectPath, project.WriteToString());
        }

        private static void ApplyTeamId(PBXProject project, string targetGuid, string label)
        {
            if (string.IsNullOrEmpty(targetGuid))
                return;

            project.SetBuildProperty(targetGuid, "DEVELOPMENT_TEAM", AppleTeamId);
            Debug.Log($"[SigningPostProcessor] Pinned DEVELOPMENT_TEAM={AppleTeamId} on {label}");
        }

        private static void ApplyExtensionFixup(
            PBXProject project,
            string targetName,
            string bundleId
        )
        {
            var guid = project.TargetGuidByName(targetName);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning(
                    $"[SigningPostProcessor] Target '{targetName}' not found; skipping."
                );
                return;
            }

            project.SetBuildProperty(guid, "PRODUCT_BUNDLE_IDENTIFIER", bundleId);
            ApplyTeamId(project, guid, targetName);
            Debug.Log(
                $"[SigningPostProcessor] Set {targetName} PRODUCT_BUNDLE_IDENTIFIER={bundleId}"
            );
        }
    }
}

#endif
