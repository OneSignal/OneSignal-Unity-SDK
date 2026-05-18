/*
 * Modified MIT License
 *
 * Copyright 2023 OneSignal
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * 1. The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * 2. All copies of substantial portions of the Software may only be used in connection
 * with services provided by OneSignal.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#if UNITY_IOS

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace App.Editor.iOS
{
    /// <summary>
    /// Adds the OneSignalWidgetExtension to the iOS project and enables the main target
    /// for Live Activities.
    /// </summary>
    public class BuildPostProcessor : IPostprocessBuildWithReport
    {
        private static readonly string WidgetExtensionTargetRelativePath = "OneSignalWidget";
        private static readonly string WidgetExtensionTargetName = "OneSignalWidgetExtension";
        private static readonly string WidgetExtensionPath = Path.Combine("iOS", "OneSignalWidget");
        private static readonly string[] WidgetExtensionFiles = new string[]
        {
            "Assets.xcassets",
            "OneSignalWidgetBundle.swift",
            "OneSignalWidgetLiveActivity.swift",
        };

        private static readonly string SoundsSourceDir = Path.Combine("iOS", "Sounds");
        private static readonly string[] CustomSoundFiles = new string[] { "vine_boom.wav" };

        /// <summary>
        /// must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's
        /// added before "pod install" (50)
        /// https://github.com/googlesamples/unity-jar-resolver#appending-text-to-generated-podfile
        /// </summary>
        public int callbackOrder => 45;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
                return;

            Debug.Log(
                "BuildPostProcessor.OnPostprocessBuild for target "
                    + report.summary.platform
                    + " at path "
                    + report.summary.outputPath
                    + " with CWD "
                    + Directory.GetCurrentDirectory()
            );

            EnableAppForLiveActivities(report.summary.outputPath);
            CreateWidgetExtension(report.summary.outputPath);
            AddCustomSoundsToMainTarget(report.summary.outputPath);

            Debug.Log("BuildPostProcessor.OnPostprocessBuild complete");
        }

        static void EnableAppForLiveActivities(string outputPath)
        {
            var plistPath = Path.Combine(outputPath, "Info.plist");

            if (!File.Exists(plistPath))
            {
                Debug.LogError($"Could not find PList {plistPath}!");
                return;
            }

            var mainPlist = new PlistDocument();
            mainPlist.ReadFromFile(plistPath);
            mainPlist.root.SetBoolean("NSSupportsLiveActivities", true);
            mainPlist.WriteToFile(plistPath);
        }

        static void CreateWidgetExtension(string outputPath)
        {
            AddWidgetExtensionToProject(outputPath);
            AddWidgetExtensionToPodFile(outputPath);
        }

        /// <summary>
        /// Copies bundled .wav files into the Xcode project and registers them on the main
        /// target's Resources build phase so `ios_sound` payload values resolve to a real
        /// `UNNotificationSound` resource in the .app bundle.
        /// </summary>
        static void AddCustomSoundsToMainTarget(string outputPath)
        {
            var project = new PBXProject();
            var projectPath = PBXProject.GetPBXProjectPath(outputPath);
            project.ReadFromString(File.ReadAllText(projectPath));

            var mainTargetGuid = project.GetUnityMainTargetGuid();

            foreach (var fileName in CustomSoundFiles)
            {
                var sourcePath = Path.Combine(SoundsSourceDir, fileName);
                if (!File.Exists(sourcePath))
                {
                    Debug.LogWarning(
                        $"Custom sound asset missing at {sourcePath}; skipping iOS bundling."
                    );
                    continue;
                }

                var destAbsolutePath = Path.Combine(outputPath, fileName);
                File.Copy(sourcePath, destAbsolutePath, true);

                if (!string.IsNullOrEmpty(project.FindFileGuidByProjectPath(fileName)))
                    continue;

                var fileGuid = project.AddFile(fileName, fileName);
                project.AddFileToBuild(mainTargetGuid, fileGuid);
            }

            project.WriteToFile(projectPath);
        }

        static void AddWidgetExtensionToProject(string outputPath)
        {
            var project = new PBXProject();
            var projectPath = PBXProject.GetPBXProjectPath(outputPath);
            project.ReadFromString(File.ReadAllText(projectPath));

            var extensionGuid = project.TargetGuidByName(WidgetExtensionTargetName);

            // skip target setup if already present
            if (!string.IsNullOrEmpty(extensionGuid))
                return;

            var widgetDestPath = Path.Combine(outputPath, WidgetExtensionTargetRelativePath);

            Directory.CreateDirectory(widgetDestPath);
            CopyFileOrDirectory(
                Path.Combine(WidgetExtensionPath, "Info.plist"),
                Path.Combine(widgetDestPath, "Info.plist")
            );

            extensionGuid = project.AddAppExtension(
                project.GetUnityMainTargetGuid(),
                WidgetExtensionTargetName,
                $"{PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS)}.{WidgetExtensionTargetName}",
                $"{WidgetExtensionTargetRelativePath}/Info.plist"
            );

            var buildPhaseID = project.AddSourcesBuildPhase(extensionGuid);

            foreach (var file in WidgetExtensionFiles)
            {
                var destPathRelative = Path.Combine(WidgetExtensionTargetRelativePath, file);
                var sourceFileGuid = project.AddFile(destPathRelative, destPathRelative);
                project.AddFileToBuildSection(extensionGuid, buildPhaseID, sourceFileGuid);
                CopyFileOrDirectory(
                    Path.Combine(WidgetExtensionPath, file),
                    Path.Combine(outputPath, destPathRelative)
                );
            }

            project.SetBuildProperty(extensionGuid, "TARGETED_DEVICE_FAMILY", "1,2");
            project.SetBuildProperty(extensionGuid, "IPHONEOS_DEPLOYMENT_TARGET", "17.0");
            project.SetBuildProperty(extensionGuid, "SWIFT_VERSION", "5.0");

            project.WriteToFile(projectPath);
        }

        static void AddWidgetExtensionToPodFile(string outputPath)
        {
            var podfilePath = Path.Combine(outputPath, "Podfile");

            if (!File.Exists(podfilePath))
            {
                Debug.LogError($"Could not find Podfile {podfilePath}!");
                return;
            }

            // Keep the widget extension pinned to the same OneSignalXCFramework version as the
            // core plugin so CocoaPods can resolve a single shared version across targets.
            var requiredVersion = ResolveOneSignalXCFrameworkVersion();
            var versionConstraint =
                requiredVersion != null ? $"'{requiredVersion}'" : "'>= 5.0.2', '< 6.0.0'";
            var requiredTarget =
                $"target '{WidgetExtensionTargetName}' do\n  pod 'OneSignalXCFramework', {versionConstraint}\nend\n";

            var podfile = File.ReadAllText(podfilePath);
            var podfileRegex = new Regex(
                $@"target '{WidgetExtensionTargetName}' do\n  pod 'OneSignalXCFramework', '(.+)'\nend\n"
            );

            if (!podfileRegex.IsMatch(podfile))
                podfile += requiredTarget;
            else
            {
                var podfileTarget = podfileRegex.Match(podfile).ToString();
                podfile = podfile.Replace(podfileTarget, requiredTarget);
            }

            File.WriteAllText(podfilePath, podfile);
        }

        static string ResolveOneSignalXCFrameworkVersion()
        {
            var dependenciesFilePath = Path.Combine(
                "Packages",
                "com.onesignal.unity.ios",
                "Editor",
                "OneSignaliOSDependencies.xml"
            );

            if (!File.Exists(dependenciesFilePath))
            {
                Debug.LogWarning(
                    $"Could not find {dependenciesFilePath}; falling back to default OneSignalXCFramework version range."
                );
                return null;
            }

            var dependenciesFile = File.ReadAllText(dependenciesFilePath);
            var dependenciesRegex = new Regex(
                "(?<=<iosPod name=\"OneSignalXCFramework\" version=\")[^\"]+(?=\" addToAllTargets=\"true\" />)"
            );

            if (!dependenciesRegex.IsMatch(dependenciesFile))
            {
                Debug.LogWarning(
                    $"Could not read OneSignalXCFramework version from {dependenciesFilePath}; falling back to default version range."
                );
                return null;
            }

            return dependenciesRegex.Match(dependenciesFile).ToString();
        }

        static void CopyFileOrDirectory(string sourcePath, string destinationPath)
        {
            var file = new FileInfo(sourcePath);
            var dir = new DirectoryInfo(sourcePath);

            if (!file.Exists && !dir.Exists)
            {
                return;
            }

            if (file.Exists)
            {
                file.CopyTo(destinationPath, true);
            }
            else
            {
                Directory.CreateDirectory(destinationPath);

                foreach (
                    FileInfo childFile in dir.EnumerateFiles().Where(f => !f.Name.EndsWith(".meta"))
                )
                {
                    CopyFileOrDirectory(
                        childFile.FullName,
                        Path.Combine(destinationPath, childFile.Name)
                    );
                }

                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    CopyFileOrDirectory(
                        subDir.FullName,
                        Path.Combine(destinationPath, subDir.Name)
                    );
                }
            }
        }
    }
}
#endif
