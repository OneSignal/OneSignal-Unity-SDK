/*
 *  Adds required frameworks to the iOS project, and adds the OneSignalNotificationServiceExtension
 *  Also handles making sure both targets (app and extension service) have the correct dependencies
*/

/** Testing Notes
 * When making any changes please test the following senerios
 * 1. Building to a new directory
 * 2. Appending. Running a 2nd time
 * 2. Appending. Comming from the last released version
 * 3. Appending. Comming from a project without OneSignal
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

#if UNITY_5_4_OR_NEWER && UNITY_IPHONE && UNITY_EDITOR

   using System.IO;
   using System;
   using UnityEngine;
   using UnityEditor;
   using UnityEditor.Callbacks;
   using UnityEditor.iOS.Xcode;
   using System.Text;
   using System.Collections.Generic;

   #if UNITY_2017_2_OR_NEWER
      using UnityEditor.iOS.Xcode.Extensions;
   #endif

   public static class BuildPostProcessor
   {
      public static readonly string DEFAULT_PROJECT_TARGET_NAME = "Unity-iPhone";
      public static readonly string NOTIFICATION_SERVICE_EXTENSION_TARGET_NAME = "OneSignalNotificationServiceExtension";
      public static readonly string NOTIFICATION_SERVICE_EXTENSION_OBJECTIVEC_FILENAME = "NotificationService";

      private static readonly char DIR_CHAR = Path.DirectorySeparatorChar;
      public static readonly string OS_PLATFORM_LOCATION = "Assets" + DIR_CHAR + "OneSignal" + DIR_CHAR + "Platforms" + DIR_CHAR;

      private static readonly string[] FRAMEWORKS_TO_ADD = {
         "NotificationCenter.framework",
         "UserNotifications.framework",
         "UIKit.framework",
         "SystemConfiguration.framework",
         "CoreGraphics.framework",
         "WebKit.framework"
      };

      private enum EntitlementOptions {
         ApsEnv,
         AppGroups
      }

      // Unity 2019.3 made large changes to the Xcode build system / API.
      // There is now two targets;
      //  * Unity-Iphone (Main)
      //  * UnityFramework
      //     - Plugins are now added this instead of the main target
      #if UNITY_2019_3_OR_NEWER
         private static string GetPBXProjectTargetName(PBXProject project)
         {
            // var projectUUID = project.GetUnityMainTargetGuid();
            // return project.GetBuildPhaseName(projectUUID);
            // The above always returns null, using a static value for now.
            return DEFAULT_PROJECT_TARGET_NAME;
         }

         private static string GetPBXProjectTargetGUID(PBXProject project)
         {
            return project.GetUnityMainTargetGuid();
         }

         private static string GetPBXProjectUnityFrameworkGUID(PBXProject project)
         {
            return project.GetUnityFrameworkTargetGuid();
         }
      #else
         private static string GetPBXProjectTargetName(PBXProject project)
         {
            return PBXProject.GetUnityTargetName();
         }

         private static string GetPBXProjectTargetGUID(PBXProject project)
         { 
            return project.TargetGuidByName(PBXProject.GetUnityTargetName());
         }

         private static string GetPBXProjectUnityFrameworkGUID(PBXProject project)
         {
            return GetPBXProjectTargetGUID(project);
         }
      #endif

      [PostProcessBuildAttribute(1)]
      public static void OnPostProcessBuild(BuildTarget target, string path)
      { 
         var projectPath = PBXProject.GetPBXProjectPath(path);
         var project = new PBXProject();

         project.ReadFromString(File.ReadAllText(projectPath));

         var mainTargetName = GetPBXProjectTargetName(project);
         var mainTargetGUID = GetPBXProjectTargetGUID(project);
         var unityFrameworkGUID = GetPBXProjectUnityFrameworkGUID(project);

         foreach(var framework in FRAMEWORKS_TO_ADD) {
            project.AddFrameworkToProject(unityFrameworkGUID, framework, false);
         }

         AddOrUpdateEntitlements(
            path,
            project,
            mainTargetGUID,
            mainTargetName,
            new HashSet<EntitlementOptions> {
               EntitlementOptions.ApsEnv, EntitlementOptions.AppGroups
            }
         );

         // Add the NSE target to the Xcode project
         AddNotificationServiceExtension(project, path);
         
         // Reload file after changes from AddNotificationServiceExtension
         project.WriteToFile(projectPath);
         var contents = File.ReadAllText(projectPath);
         project.ReadFromString(contents);

         // Add push notifications as a capability on the main app target
         AddPushCapability(project, path, mainTargetGUID, mainTargetName);

         File.WriteAllText(projectPath, project.WriteToString());
      }

      // Returns exisiting file if found, otherwises provides a default name to use
      private static string GetEntitlementsPath(string path, PBXProject project, string targetGUI, string targetName)
      {
         // Check if there is already an eltitlements file configured in the Xcode project
         #if UNITY_2018_2_OR_NEWER
         var relativeEntitlementPath = project.GetBuildPropertyForConfig(targetGUI, "CODE_SIGN_ENTITLEMENTS");
         if (relativeEntitlementPath != null) {
            var entitlementPath = path + DIR_CHAR + relativeEntitlementPath;
            if (File.Exists(entitlementPath)) {
                return entitlementPath;
            }
         }
         #endif

         // No existing file, use a new name
         return path + DIR_CHAR + targetName + DIR_CHAR + targetName + ".entitlements";
      }
      
      private static void AddOrUpdateEntitlements(string path, PBXProject project, string targetGUI, string targetName, HashSet<EntitlementOptions> options)
      {
         string entitlementPath = GetEntitlementsPath(path, project, targetGUI, targetName);
         var entitlements = new PlistDocument();

         // Check if the file already exisits and read it
         if (File.Exists(entitlementPath)) {
            entitlements.ReadFromFile(entitlementPath);
         }

         if (options.Contains(EntitlementOptions.ApsEnv)) {
            if (entitlements.root["aps-environment"] == null)
               entitlements.root.SetString("aps-environment", "development");
         }

         // TOOD: This can be updated to use project.AddCapability() in the future
         #if ADD_APP_GROUP
            if (options.Contains(EntitlementOptions.AppGroups) && entitlements.root["com.apple.security.application-groups"] == null) {
               var groups = entitlements.root.CreateArray("com.apple.security.application-groups");
               groups.AddString("group." + PlayerSettings.applicationIdentifier + ".onesignal");
            }
         #endif

         entitlements.WriteToFile(entitlementPath);

         // Copy the entitlement file to the xcode project
         var entitlementFileName = Path.GetFileName(entitlementPath);
         var relativeDestination = targetName + "/" + entitlementFileName;

         // Add the pbx configs to include the entitlements files on the project
         project.AddFile(relativeDestination, entitlementFileName);
         project.AddBuildProperty(targetGUI, "CODE_SIGN_ENTITLEMENTS", relativeDestination);
      }

      private static void AddPushCapability(PBXProject project, string path, string targetGUID, string targetName)
      {
         var projectPath = PBXProject.GetPBXProjectPath(path);
         project.AddCapability(targetGUID, PBXCapabilityType.PushNotifications);
         project.AddCapability(targetGUID, PBXCapabilityType.BackgroundModes);

         var entitlementsPath = GetEntitlementsPath(path, project, targetGUID, targetName);
         // NOTE: ProjectCapabilityManager's 4th constructor param requires Unity 2019.3+
         var projCapability = new ProjectCapabilityManager(projectPath, entitlementsPath, targetName);
         projCapability.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
         projCapability.WriteToFile();
      }

      private static void AddNotificationServiceExtension(PBXProject project, string path)
      {
#if UNITY_2017_2_OR_NEWER && !UNITY_CLOUD_BUILD
         var projectPath = PBXProject.GetPBXProjectPath(path);
         var mainTargetGUID = GetPBXProjectTargetGUID(project);
         var extensionTargetName = NOTIFICATION_SERVICE_EXTENSION_TARGET_NAME;
      
         var exisitingPlistFile = CreateNotificationExtensionPlistFile(path);
         // If file exisits then the below has been completed before from another build
         // The below will not be updated on Append builds
         // Changes would most likely need to be made to support Append builds
         if (exisitingPlistFile)
            return;

         var extensionGUID = PBXProjectExtensions.AddAppExtension(
            project,
            mainTargetGUID,
            extensionTargetName,
            PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS) + "." + extensionTargetName,
            extensionTargetName + "/" + "Info.plist" // Unix path as it's used by Xcode
         );

         AddNotificationServiceSourceFilesToTarget(project, extensionGUID, path);

         foreach(var framework in FRAMEWORKS_TO_ADD) {
            project.AddFrameworkToProject(extensionGUID, framework, true);
         }

         // Makes it so that the extension target is Universal (not just iPhone) and has an iOS 10 deployment target
         project.SetBuildProperty(extensionGUID, "TARGETED_DEVICE_FAMILY", "1,2");
         project.SetBuildProperty(extensionGUID, "IPHONEOS_DEPLOYMENT_TARGET", "10.0");

         project.SetBuildProperty(extensionGUID, "ARCHS", "$(ARCHS_STANDARD)");
         project.SetBuildProperty(extensionGUID, "DEVELOPMENT_TEAM", PlayerSettings.iOS.appleDeveloperTeamID);

         project.AddBuildProperty(extensionGUID, "LIBRARY_SEARCH_PATHS", "$(PROJECT_DIR)/Libraries/OneSignal/Platforms/iOS");
         project.WriteToFile(projectPath);

         // Add libOneSignal.a to the OneSignalNotificationServiceExtension target
         var contents = File.ReadAllText(projectPath);

         // This method only modifies the PBXProject string passed in (contents).
         // After this method finishes, we must write the contents string to disk
         InsertStaticFrameworkIntoTargetBuildPhaseFrameworks("libOneSignal", "CD84C25F20742FAB0035D524", extensionGUID, ref contents, project);
         File.WriteAllText(projectPath, contents);

         AddOrUpdateEntitlements(
            path,
            project,
            extensionGUID,
            extensionTargetName,
            new HashSet<EntitlementOptions> { EntitlementOptions.AppGroups }
         );
#endif
   }

   // Copies NotificationService.m and .h files into the OneSignalNotificationServiceExtension folder adds them to the Xcode target
   private static void AddNotificationServiceSourceFilesToTarget(PBXProject project, string extensionGUID, string path)
   {
      var buildPhaseID = project.AddSourcesBuildPhase(extensionGUID);
      foreach (var type in new string[] { "m", "h" }) {
         var nativeFileName = NOTIFICATION_SERVICE_EXTENSION_OBJECTIVEC_FILENAME + "." + type;
         var sourcePath = OS_PLATFORM_LOCATION + "iOS" + DIR_CHAR + nativeFileName;
         var nativeFileRelativeDestination = NOTIFICATION_SERVICE_EXTENSION_TARGET_NAME + "/" + nativeFileName;

         var destPath = path + DIR_CHAR + nativeFileRelativeDestination;
         if (!File.Exists(destPath))
            FileUtil.CopyFileOrDirectory(sourcePath, destPath);

         var sourceFileGUID = project.AddFile(nativeFileRelativeDestination, nativeFileRelativeDestination, PBXSourceTree.Source);
         project.AddFileToBuildSection(extensionGUID, buildPhaseID, sourceFileGUID);
      }
   }

      // Create a .plist file for the NSE
      // NOTE: File in Xcode project is replaced everytime, never appends
      private static bool CreateNotificationExtensionPlistFile(string path)
      {
      #if UNITY_2017_2_OR_NEWER
         var pathToNotificationService = path + DIR_CHAR + NOTIFICATION_SERVICE_EXTENSION_TARGET_NAME;
         Directory.CreateDirectory(pathToNotificationService);

         var notificationServicePlistPath = pathToNotificationService + DIR_CHAR + "Info.plist";
         bool exisiting = File.Exists(notificationServicePlistPath);

         // Read from the OneSignal plist template file.
         var notificationServicePlist = new PlistDocument();
         notificationServicePlist.ReadFromFile(OS_PLATFORM_LOCATION + "iOS" + DIR_CHAR + "Info.plist");
         notificationServicePlist.root.SetString("CFBundleShortVersionString", PlayerSettings.bundleVersion);
         notificationServicePlist.root.SetString("CFBundleVersion", PlayerSettings.iOS.buildNumber.ToString());
         notificationServicePlist.WriteToFile(notificationServicePlistPath);
         return exisiting;
      #else
         return true;
      #endif
      }

      // Takes a static framework that is already linked to a different target in the project and links it to the specified target
      private static void InsertStaticFrameworkIntoTargetBuildPhaseFrameworks(string staticFrameworkName, string frameworkGuid, string target, ref string contents, PBXProject project)
      {
      #if UNITY_2017_2_OR_NEWER && !UNITY_CLOUD_BUILD
         // In order to find the fileRef, find the PBXBuildFile objects section of the PBXProject
         var splitString = " /* " + staticFrameworkName + ".a in Frameworks */ = {isa = PBXBuildFile; fileRef = ";
         var splitComponents = contents.Split(new string[] {splitString}, StringSplitOptions.None);

         if (splitComponents.Length < 2) {
            Debug.LogError ("(error 1) OneSignal's Build Post Processor has encountered an error while attempting to add the Notification Extension Service to your project. Please create an issue on our OneSignal-Unity-SDK repo on GitHub.");
            return;
         }

         var afterSplit = splitComponents[1];

         // To get the fileRef of the static framework, read the last 24 characters of the beforeSplit string
         var fileRefBuilder = new StringBuilder();

         for (int i = 0; i < 24; i++) {
            fileRefBuilder.Append(afterSplit[i]);
         }

         var fileRef = fileRefBuilder.ToString();

         project.AddFileToBuild(target, fileRef);

         // Add the framework as an additional object in PBXBuildFile objects
         contents = contents.Replace("; fileRef = " + fileRef + " /* " + staticFrameworkName + ".a */; };", "; fileRef = " + fileRef + " /* " + staticFrameworkName + ".a */; };\n\t\t" + frameworkGuid + " /* " + staticFrameworkName + ".a in Frameworks */ = {isa = PBXBuildFile; fileRef = " + fileRef + " /* " + staticFrameworkName + ".a */; };");

         // Find the build phase ID number
         var targetBuildPhaseId = project.GetFrameworksBuildPhaseByTarget(target);
         string[] components = contents.Split(new string[] { targetBuildPhaseId + " /* Frameworks */ = {\n\t\t\tisa = PBXFrameworksBuildPhase;\n\t\t\tbuildActionMask = " }, StringSplitOptions.None);

         if (components.Length < 2) {
            Debug.LogError("(error 2) OneSignal's Build Post Processor has encountered an error while attempting to add the Notification Extension Service to your project. Please create an issue on our OneSignal-Unity-SDK repo on GitHub.");
            return;
         }

         var buildPhaseString = components[1];

         var replacer = new StringBuilder();

         for (int i = 0; i < buildPhaseString.Length; i++) {
            var seq = buildPhaseString [i];

            if (char.IsNumber (seq)) {
               replacer.Append (seq);
            } else {
               break;
            }
         }

         // insert the framework into the PBXFrameworksBuildPhase 
         var beginString = targetBuildPhaseId + " /* Frameworks */ = {\n\t\t\tisa = PBXFrameworksBuildPhase;\n\t\t\tbuildActionMask = " + replacer.ToString() + ";\n\t\t\tfiles = (";
         contents = contents.Replace(beginString, beginString + "\n" + "\t\t\t\t" + frameworkGuid + " /* " + staticFrameworkName + ".a in Frameworks */,");
      #endif
      }
   }
#endif