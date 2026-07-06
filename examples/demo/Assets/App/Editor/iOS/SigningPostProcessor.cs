#if UNITY_IOS

using System;
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
    /// flips aps-environment to "development", normalizes extension bundle
    /// IDs, pins DEVELOPMENT_TEAM on every target, and (in CI only) switches
    /// signing to Manual with the OneSignal-owned Appium provisioning
    /// profiles so xcodebuild archive can sign the IPA unattended.
    /// </summary>
    public class SigningPostProcessor : IPostprocessBuildWithReport
    {
        private const string AppleTeamId = "99SW8E36CT";
        private const string ApsEnvironment = "development";

        private const string NseTargetName = "OneSignalNotificationServiceExtension";
        private const string WidgetTargetName = "OneSignalWidgetExtension";

        // Short bundle-id suffixes (match the Flutter demo).
        private const string NseBundleSuffix = "NSE";
        private const string WidgetBundleSuffix = "LA";

        // CI-only manual signing config. Profile names must match the
        // OneSignal-owned provisioning profiles fetched by
        // .github/workflows/e2e.yml (`Download provisioning profiles`) and
        // referenced by examples/demo/iOS/ExportOptions.plist. Local devs
        // keep Xcode auto-signing because IsCiBuild only returns true when
        // GitHub Actions / similar set CI=true.
        private const string MainProfileName = "Appium Demo - Main";
        private const string NseProfileName = "Appium Demo - NSE";
        private const string WidgetProfileName = "Appium Demo - Live Activity";
        private const string DevSignIdentity = "Apple Development";

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

            var appId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS);
            var manualSigning = IsCiBuild();

            var mainGuid = project.GetUnityMainTargetGuid();
            ApplyTeamId(project, mainGuid, "Unity-iPhone");
            if (manualSigning)
                ApplyManualSigning(project, mainGuid, MainProfileName, "Unity-iPhone");

            ApplyExtensionFixup(
                project,
                NseTargetName,
                $"{appId}.{NseBundleSuffix}",
                manualSigning ? NseProfileName : null
            );
            ApplyExtensionFixup(
                project,
                WidgetTargetName,
                $"{appId}.{WidgetBundleSuffix}",
                manualSigning ? WidgetProfileName : null
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

        private static void ApplyManualSigning(
            PBXProject project,
            string targetGuid,
            string profileName,
            string label
        )
        {
            if (string.IsNullOrEmpty(targetGuid))
                return;

            project.SetBuildProperty(targetGuid, "CODE_SIGN_STYLE", "Manual");
            project.SetBuildProperty(targetGuid, "PROVISIONING_PROFILE_SPECIFIER", profileName);
            // PROVISIONING_PROFILE was deprecated by Xcode in favor of the
            // specifier but Unity still emits it on every target; clear it so
            // the specifier above wins instead of an empty UUID lookup.
            project.SetBuildProperty(targetGuid, "PROVISIONING_PROFILE", "");
            project.SetBuildProperty(targetGuid, "CODE_SIGN_IDENTITY", DevSignIdentity);
            project.SetBuildProperty(
                targetGuid,
                "CODE_SIGN_IDENTITY[sdk=iphoneos*]",
                DevSignIdentity
            );

            Debug.Log(
                $"[SigningPostProcessor] Manual signing on {label}: profile=\"{profileName}\""
            );
        }

        private static void ApplyExtensionFixup(
            PBXProject project,
            string targetName,
            string bundleId,
            string profileName
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
            if (!string.IsNullOrEmpty(profileName))
                ApplyManualSigning(project, guid, profileName, targetName);
            Debug.Log(
                $"[SigningPostProcessor] Set {targetName} PRODUCT_BUNDLE_IDENTIFIER={bundleId}"
            );
        }

        private static bool IsCiBuild()
        {
            return string.Equals(
                Environment.GetEnvironmentVariable("CI"),
                "true",
                StringComparison.OrdinalIgnoreCase
            );
        }
    }
}

#endif
