/*
 * Modified MIT License
 *
 * Copyright 2022 OneSignal
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

/*
 * Testing Notes
 * When making any changes please test the following senerios
 * 1. Building to a new directory
 * 2. Appending. Running a 2nd time
 * 2. Appending. Coming from the last released version
 * 3. Appending. Coming from a project without OneSignal
 *
 * In each of the tests ensure the NSE + App Groups work by doing the following:
 * 1. Send a notification with Badge set to Increase by 1
 * 2. Send a 2nd identical notification
 * 3. Observe Badge value on device as 2. (NSE is working)
 * 4. Open app and then background it again, Badge value will be cleared.
 * 5. Send a 3rd identical notification.
 * 6. Observe Badge value is 1. (If it is 3 there is an App Group issue)
*/

// Flag if an App Group should created for the main target and the NSE
// Try renaming NOTIFICATION_SERVICE_EXTENSION_TARGET_NAME below first before
//   removing ADD_APP_GROUP if you run into Provisioning errors in Xcode that
//   can't be fix.
// ADD_APP_GROUP is required for;
//   Outcomes, Badge Increment, and possibly for future features

#define ADD_APP_GROUP

using System.IO;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using System.Text.RegularExpressions;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode.Extensions;
using Debug = UnityEngine.Debug;

namespace OneSignalSDK {
    /// <summary>
    /// Adds required frameworks to the iOS project, and adds the OneSignalNotificationServiceExtension. Also handles
    /// making sure both targets (app and extension service) have the correct dependencies
    /// </summary>
    public class BuildPostProcessor : IPostprocessBuildWithReport {
        private const string ServiceExtensionTargetName = "OneSignalNotificationServiceExtension";
        private const string ServiceExtensionFilename = "NotificationService.swift";
        private const string DependenciesFilename = "OneSignalIOSDependencies.xml";
        private const string PackageName = "com.onesignal.unity.ios";

        private static readonly string EditorFilesPath = Path.Combine("Packages", PackageName, "Editor");
        private static readonly string PluginLibrariesPath = Path.Combine(PackageName, "Runtime", "Plugins", "iOS");
        private static readonly string PluginFilesPath = Path.Combine("Packages", PluginLibrariesPath);

        private readonly string _appGroupName = $"group.{PlayerSettings.applicationIdentifier}.onesignal";
        
        private string _outputPath;
        private string _projectPath;
        
        private readonly PBXProject _project = new PBXProject();

        /// <summary>
        /// must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's
        /// added before "pod install" (50)
        /// https://github.com/googlesamples/unity-jar-resolver#appending-text-to-generated-podfile
        /// </summary>
        public int callbackOrder => 45;

        /// <summary>
        /// Entry for the build post processing necessary to get the OneSignal SDK iOS up and running
        /// </summary>
        public void OnPostprocessBuild(BuildReport report) {
            if (report.summary.platform != BuildTarget.iOS)
                return;

            // Load the project
            _outputPath  = report.summary.outputPath;
            _projectPath = PBXProject.GetPBXProjectPath(_outputPath);
            _project.ReadFromString(File.ReadAllText(_projectPath));
            
            // Turn on capabilities required by OneSignal
            AddProjectCapabilities();
            
            // Add the service extension
            AddNotificationServiceExtension();

            // Save the project back out
            File.WriteAllText(_projectPath, _project.WriteToString());
        }

        /// <summary>
        /// Get existing entitlements file if exists or creates a new file, adds it to the project, and returns the path
        /// </summary>
        private string GetEntitlementsPath(string targetGuid, string targetName) {
            var relativePath = _project.GetBuildPropertyForAnyConfig(targetGuid, "CODE_SIGN_ENTITLEMENTS");

            if (relativePath != null) {
                var fullPath = Path.Combine(_outputPath, relativePath);

                if (File.Exists(fullPath))
                    return fullPath;
            }
            
            var entitlementsPath = Path.Combine(_outputPath, targetName, $"{targetName}.entitlements");
            
            // make new file
            var entitlementsPlist = new PlistDocument();
            entitlementsPlist.WriteToFile(entitlementsPath);
            
            // Copy the entitlement file to the xcode project
            var entitlementFileName = Path.GetFileName(entitlementsPath);
            var relativeDestination = targetName + "/" + entitlementFileName;

            // Add the pbx configs to include the entitlements files on the project
            _project.AddFile(relativeDestination, entitlementFileName);
            _project.SetBuildProperty(targetGuid, "CODE_SIGN_ENTITLEMENTS", relativeDestination);

            return entitlementsPath;
        }
        
        /// <summary>
        /// Add the required capabilities and entitlements for OneSignal
        /// </summary>
        private void AddProjectCapabilities() {
            var targetGuid = _project.GetMainTargetGuid();
            var targetName = _project.GetMainTargetName();

            var entitlementsPath = GetEntitlementsPath(targetGuid, targetName);
            var projCapability = new ProjectCapabilityManager(_projectPath, entitlementsPath, targetName);

            projCapability.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
            projCapability.AddPushNotifications(false);
            projCapability.AddAppGroups(new[] { _appGroupName });
            
            projCapability.WriteToFile();
        }
        
        /// <summary>
        /// Create and add the notification extension to the project
        /// </summary>
        private void AddNotificationServiceExtension() {
#if !UNITY_CLOUD_BUILD
            // If file exists then the below has been completed before from another build
            // The below will not be updated on Append builds
            // Changes would most likely need to be made to support Append builds
            if (ExtensionCreatePlist(_outputPath)) {
                ExtensionAddPodsToTarget();
                return;
            }

            var extensionGuid = _project.AddAppExtension(_project.GetMainTargetGuid(),
                ServiceExtensionTargetName,
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS) + "." + ServiceExtensionTargetName,
                ServiceExtensionTargetName + "/" + "Info.plist" // Unix path as it's used by Xcode
            );

            ExtensionAddSourceFiles(extensionGuid);

            // Makes it so that the extension target is Universal (not just iPhone) and has an iOS 10 deployment target
            _project.SetBuildProperty(extensionGuid, "TARGETED_DEVICE_FAMILY", "1,2");
            _project.SetBuildProperty(extensionGuid, "IPHONEOS_DEPLOYMENT_TARGET", "10.0");
            _project.SetBuildProperty(extensionGuid, "SWIFT_VERSION", "5.0");
            _project.SetBuildProperty(extensionGuid, "ARCHS", "arm64");
            _project.SetBuildProperty(extensionGuid, "DEVELOPMENT_TEAM", PlayerSettings.iOS.appleDeveloperTeamID);

            _project.AddBuildProperty(extensionGuid, "LIBRARY_SEARCH_PATHS",
                $"$(PROJECT_DIR)/Libraries/{PluginLibrariesPath.Replace("\\", "/")}");

            _project.WriteToFile(_projectPath);

            // add capabilities + entitlements
            var entitlementsPath = GetEntitlementsPath(extensionGuid, ServiceExtensionTargetName);
            var projCapability = new ProjectCapabilityManager(_projectPath, entitlementsPath, ServiceExtensionTargetName);
            
            projCapability.AddAppGroups(new[] { _appGroupName });
            
            projCapability.WriteToFile();
            
            ExtensionAddPodsToTarget();
#endif
        }

        /// <summary>
        /// Add the swift source file required by the notification extension
        /// </summary>
        private void ExtensionAddSourceFiles(string extensionGuid) {
            var buildPhaseID     = _project.AddSourcesBuildPhase(extensionGuid);
            var sourcePath       = Path.Combine(PluginFilesPath, ServiceExtensionFilename);
            var destPathRelative = Path.Combine(ServiceExtensionTargetName, ServiceExtensionFilename);

            var destPath = Path.Combine(_outputPath, destPathRelative);
            if (!File.Exists(destPath))
                FileUtil.CopyFileOrDirectory(sourcePath.Replace("\\", "/"), destPath.Replace("\\", "/"));

            var sourceFileGuid = _project.AddFile(destPathRelative, destPathRelative);
            _project.AddFileToBuildSection(extensionGuid, buildPhaseID, sourceFileGuid);
        }
        
        /// <summary>
        /// Create a .plist file for the extension
        /// </summary>
        /// <remarks>NOTE: File in Xcode project is replaced everytime, never appends</remarks>
        private bool ExtensionCreatePlist(string path) {
            var extensionPath = Path.Combine(path, ServiceExtensionTargetName);
            Directory.CreateDirectory(extensionPath);

            var plistPath     = Path.Combine(extensionPath, "Info.plist");
            var alreadyExists = File.Exists(plistPath);

            var notificationServicePlist = new PlistDocument();
            notificationServicePlist.ReadFromFile(Path.Combine(PluginFilesPath, "Info.plist"));
            notificationServicePlist.root.SetString("CFBundleShortVersionString", PlayerSettings.bundleVersion);
            notificationServicePlist.root.SetString("CFBundleVersion", PlayerSettings.iOS.buildNumber);

            notificationServicePlist.WriteToFile(plistPath);
            
            return alreadyExists;
        }

        private void ExtensionAddPodsToTarget() {
            var podfilePath = Path.Combine(_outputPath, "Podfile");

            if (!File.Exists(podfilePath)) {
                Debug.LogError($"Could not find Podfile. {ServiceExtensionFilename} will have errors.");
                return;
            }

            var podfile = File.ReadAllText(podfilePath);

            var extensionEntryRegex = new Regex($@"target '{ServiceExtensionTargetName}' do\n(.+)\nend");
            if (extensionEntryRegex.IsMatch(podfile))
                return;

            var versionRegex = new Regex("(?<=<iosPod name=\"OneSignalXCFramework\" version=\").+(?=\" addToAllTargets=\"true\" />)");
            var dependenciesFilePath = Path.Combine(EditorFilesPath, DependenciesFilename);

            if (!File.Exists(dependenciesFilePath)) {
                Debug.LogError($"Could not find {DependenciesFilename}");
                return;
            }
            
            var dependenciesFile = File.ReadAllText(dependenciesFilePath);

            if (!versionRegex.IsMatch(dependenciesFile)) {
                Debug.LogError($"Could not read current iOS framework dependency version from {DependenciesFilename}");
                return;
            }

            var version = versionRegex.Match(dependenciesFile)
                .ToString();
            
            podfile += $"target '{ServiceExtensionTargetName}' do\n  pod 'OneSignalXCFramework', '{version}'\nend\n";
            File.WriteAllText(podfilePath, podfile);
        }
    }
}