#define ADD_APP_GROUP
//remove to prevent the addition of the app group

#if UNITY_5_4_OR_NEWER && UNITY_IPHONE && UNITY_EDITOR

   using System.IO;
   using System;
   using UnityEngine;
   using UnityEditor;
   using UnityEditor.Callbacks;
   using UnityEditor.iOS.Xcode;
   using System.Text;

   #if UNITY_2017_2_OR_NEWER
      using UnityEditor.iOS.Xcode.Extensions;
   #endif

   /*
         Adds required frameworks to the iOS project, and adds the OneSignalNotificationServiceExtension
         Also handles making sure both targets (app and extension service) have the correct dependencies
      */

   public class BuildPostProcessor
   {
      [PostProcessBuildAttribute(1)]
      public static void OnPostProcessBuild(BuildTarget target, string path)
      {
         var separator = Path.DirectorySeparatorChar;
         
         string projectPath = PBXProject.GetPBXProjectPath(path);
         PBXProject project = new PBXProject();

         project.ReadFromString(File.ReadAllText(projectPath));
         string targetName = PBXProject.GetUnityTargetName();
         string targetGUID = project.TargetGuidByName(targetName);

         // UserNotifications.framework is required by libOneSignal.a
         project.AddFrameworkToProject(targetGUID, "UserNotifications.framework", false);
        
         #if UNITY_2017_2_OR_NEWER && !UNITY_CLOUD_BUILD
           
            var platformsLocation = "Assets" + separator + "OneSignal" + separator + "Platforms" + separator;
            var extensionTargetName = "OneSignalNotificationServiceExtension";
            var pathToNotificationService = path + separator + extensionTargetName;

            var notificationServicePlistPath = pathToNotificationService + separator + "Info.plist";
      
            //if this is a rebuild, we've already added the extension service, no need to run this script a second time
            if (File.Exists(notificationServicePlistPath))
               return;
      
            Directory.CreateDirectory(pathToNotificationService);

            PlistDocument notificationServicePlist = new PlistDocument();
            notificationServicePlist.ReadFromFile (platformsLocation + "iOS" + separator + "Info.plist");
            notificationServicePlist.root.SetString ("CFBundleShortVersionString", PlayerSettings.bundleVersion);
            notificationServicePlist.root.SetString ("CFBundleVersion", PlayerSettings.iOS.buildNumber.ToString ());

            var notificationServiceTarget = PBXProjectExtensions.AddAppExtension (project, targetGUID, extensionTargetName, PlayerSettings.GetApplicationIdentifier (BuildTargetGroup.iOS) + "." + extensionTargetName, notificationServicePlistPath);

            var sourceDestination = extensionTargetName + "/NotificationService";

            project.AddFileToBuild (notificationServiceTarget, project.AddFile (sourceDestination + ".h", sourceDestination + ".h", PBXSourceTree.Source));
            project.AddFileToBuild (notificationServiceTarget, project.AddFile (sourceDestination + ".m", sourceDestination + ".m", PBXSourceTree.Source));

            var frameworks = new string[] {"NotificationCenter.framework", "UserNotifications.framework", "UIKit.framework", "SystemConfiguration.framework"};

            foreach (string framework in frameworks) {
               project.AddFrameworkToProject (notificationServiceTarget, framework, true);
            }

            //makes it so that the extension target is Universal (not just iPhone) and has an iOS 10 deployment target
            project.SetBuildProperty(notificationServiceTarget, "TARGETED_DEVICE_FAMILY", "1,2");
            project.SetBuildProperty(notificationServiceTarget, "IPHONEOS_DEPLOYMENT_TARGET", "10.0");

            project.SetBuildProperty (notificationServiceTarget, "ARCHS", "$(ARCHS_STANDARD)");
            project.SetBuildProperty (notificationServiceTarget, "DEVELOPMENT_TEAM", PlayerSettings.iOS.appleDeveloperTeamID);

            notificationServicePlist.WriteToFile (notificationServicePlistPath);
      
            foreach (string type in new string[] { "m", "h" })
               if (!File.Exists(path + separator + sourceDestination + "." + type))
                  FileUtil.CopyFileOrDirectory(platformsLocation + "iOS" + separator + "NotificationService." + type, path + separator + sourceDestination + "." + type);

            project.WriteToFile (projectPath);

            //add libOneSignal.a to the OneSignalNotificationServiceExtension target
            string contents = File.ReadAllText(projectPath);

            //this method only modifies the PBXProject string passed in (contents).
            //after this method finishes, we must write the contents string to disk
            InsertStaticFrameworkIntoTargetBuildPhaseFrameworks("libOneSignal", "CD84C25F20742FAB0035D524", notificationServiceTarget, ref contents, project);
            File.WriteAllText(projectPath, contents);

         #else 
            project.WriteToFile (projectPath);

            string contents = File.ReadAllText(projectPath);
         #endif

         // enable the Notifications capability in the main app target
         project.ReadFromString(contents);
         var entitlementPath = path + separator + targetName + separator + targetName + ".entitlements";

         PlistDocument entitlements = new PlistDocument();

         if (File.Exists(entitlementPath))
            entitlements.ReadFromFile(entitlementPath);
         
         if (entitlements.root["aps-environment"] == null)
            entitlements.root.SetString("aps-environment", "development");

         #if !UNITY_CLOUD_BUILD && ADD_APP_GROUP
            if (entitlements.root["com.apple.security.application-groups"] == null) {
               var groups = entitlements.root.CreateArray("com.apple.security.application-groups");
               groups.AddString("group." + PlayerSettings.applicationIdentifier + ".onesignal");
            }
         #endif

         entitlements.WriteToFile(entitlementPath);

         // Copy the entitlement file to the xcode project
         var entitlementFileName = Path.GetFileName(entitlementPath);
         var unityTarget = PBXProject.GetUnityTargetName();
         var relativeDestination = unityTarget + "/" + entitlementFileName;

         // Add the pbx configs to include the entitlements files on the project
         project.AddFile(relativeDestination, entitlementFileName);
         project.AddBuildProperty(targetGUID, "CODE_SIGN_ENTITLEMENTS", relativeDestination);

         // Add push notifications as a capability on the target
         project.AddBuildProperty(targetGUID, "SystemCapabilities", "{com.apple.Push = {enabled = 1;};}");
         File.WriteAllText(projectPath, project.WriteToString());
      }

      #if UNITY_2017_2_OR_NEWER && !UNITY_CLOUD_BUILD
         
         // This function takes a static framework that is already linked to a different target in the project and links it to the specified target
         public static void InsertStaticFrameworkIntoTargetBuildPhaseFrameworks(string staticFrameworkName, string frameworkGuid, string target, ref string contents, PBXProject project) {
            //in order to find the fileRef, find the PBXBuildFile objects section of the PBXProject
            string splitString = " /* " + staticFrameworkName + ".a in Frameworks */ = {isa = PBXBuildFile; fileRef = ";
            string[] splitComponents = contents.Split(new string[] {splitString}, StringSplitOptions.None);

            if (splitComponents.Length < 2) {
               Debug.LogError ("(error 1) OneSignal's Build Post Processor has encountered an error while attempting to add the Notification Extension Service to your project. Please create an issue on our OneSignal-Unity-SDK repo on GitHub.");
               return;
            }

            string afterSplit = splitComponents[1];

            //to get the fileRef of the static framework, read the last 24 characters of the beforeSplit string
            StringBuilder fileRefBuilder = new StringBuilder();

            for (int i = 0; i < 24; i++) {
               fileRefBuilder.Append(afterSplit[i]);
            }

            string fileRef = fileRefBuilder.ToString();

            project.AddFileToBuild(target, fileRef);

            //add the framework as an additional object in PBXBuildFile objects
            contents = contents.Replace("; fileRef = " + fileRef + " /* " + staticFrameworkName + ".a */; };", "; fileRef = " + fileRef + " /* " + staticFrameworkName + ".a */; };\n\t\t" + frameworkGuid + " /* " + staticFrameworkName + ".a in Frameworks */ = {isa = PBXBuildFile; fileRef = " + fileRef + " /* " + staticFrameworkName + ".a */; };");

            //find the build phase ID number
            string targetBuildPhaseId = project.GetFrameworksBuildPhaseByTarget(target);
            string[] components = contents.Split(new string[] { targetBuildPhaseId + " /* Frameworks */ = {\n\t\t\tisa = PBXFrameworksBuildPhase;\n\t\t\tbuildActionMask = " }, StringSplitOptions.None);

            if (components.Length < 2) {
               Debug.LogError("(error 2) OneSignal's Build Post Processor has encountered an error while attempting to add the Notification Extension Service to your project. Please create an issue on our OneSignal-Unity-SDK repo on GitHub.");
               return;
            }

            string buildPhaseString = components[1];

            StringBuilder replacer = new StringBuilder ();

            for (int i = 0; i < buildPhaseString.Length; i++) {
               char seq = buildPhaseString [i];

               if (char.IsNumber (seq)) {
                  replacer.Append (seq);
               } else {
                  break;
               }
            }

            // insert the framework into the PBXFrameworksBuildPhase 
            string beginString = targetBuildPhaseId + " /* Frameworks */ = {\n\t\t\tisa = PBXFrameworksBuildPhase;\n\t\t\tbuildActionMask = " + replacer.ToString() + ";\n\t\t\tfiles = (";
            contents = contents.Replace(beginString, beginString + "\n" + "\t\t\t\t" + frameworkGuid + " /* " + staticFrameworkName + ".a in Frameworks */,");

            //add library search paths to add build configurations of the target
            contents = contents.Replace ("PRODUCT_BUNDLE_IDENTIFIER = ", "LIBRARY_SEARCH_PATHS = (\n\t\t\t\t\t\"$(inherited)\",\n\t\t\t\t\t\"$(PROJECT_DIR)/Libraries/OneSignal/Platforms/iOS\",\n\t\t\t\t);\nPRODUCT_BUNDLE_IDENTIFIER = ");
         }

      #endif
   }

#endif