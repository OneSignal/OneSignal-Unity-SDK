using System.IO;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using System.Text;

/*
   Adds required frameworks to the iOS project, and adds the OneSignalNotificationServiceExtension
   Also handles making sure both targets (app and extension service) have the correct dependencies
*/

#if UNITY_IPHONE && UNITY_EDITOR

public class BuildPostProcessor
{
   [PostProcessBuildAttribute(1)]
   public static void OnPostProcessBuild(BuildTarget target, string path)
   {
      if (target == BuildTarget.iOS)
      {
         string projectPath = PBXProject.GetPBXProjectPath(path);
         PBXProject project = new PBXProject();

         // Read.
         project.ReadFromString(File.ReadAllText(projectPath));
         string targetName = PBXProject.GetUnityTargetName();
         string targetGUID = project.TargetGuidByName(targetName);

         project.AddFrameworkToProject(targetGUID, "UserNotifications.framework", false);

         // Write.
         File.WriteAllText(projectPath, project.WriteToString());

         var projPath = PBXProject.GetPBXProjectPath(path);
         
         project.ReadFromFile (projPath);

         var pathToNotificationService = path + "/OneSignalNotificationExtensionService";

         Directory.CreateDirectory (pathToNotificationService);

         var plistPath = "Assets/OneSignal/Editor/Info.plist";

         var notificationServicePlistPath = path + "/OneSignalNotificationExtensionService/Info.plist";

         PlistDocument notificationServicePlist = new PlistDocument();
         notificationServicePlist.ReadFromFile (plistPath);
         notificationServicePlist.root.SetString ("CFBundleShortVersionString", PlayerSettings.bundleVersion);
         notificationServicePlist.root.SetString ("CFBundleVersion", PlayerSettings.iOS.buildNumber.ToString ());

         var notificationServiceTarget = PBXProjectExtensions.AddAppExtension (project, targetGUID, "OneSignalNotificationExtensionService", PlayerSettings.GetApplicationIdentifier (BuildTargetGroup.iOS) + ".OneSignalNotificationExtensionService", notificationServicePlistPath);

         project.AddFileToBuild (notificationServiceTarget, project.AddFile ("OneSignalNotificationExtensionService/NotificationService.h", "OneSignalNotificationExtensionService/NotificationService.h", PBXSourceTree.Source));
         project.AddFileToBuild (notificationServiceTarget, project.AddFile ("OneSignalNotificationExtensionService/NotificationService.m", "OneSignalNotificationExtensionService/NotificationService.m", PBXSourceTree.Source));

         project.AddFrameworkToProject (notificationServiceTarget, "NotificationCenter.framework", true);
         project.AddFrameworkToProject (notificationServiceTarget, "UserNotifications.framework", true);
         project.AddFrameworkToProject (notificationServiceTarget, "UIKit.framework", true);
         project.AddFrameworkToProject (notificationServiceTarget, "SystemConfiguration.framework", true);

         project.SetBuildProperty (notificationServiceTarget, "ARCHS", "$(ARCHS_STANDARD)");
         project.SetBuildProperty (notificationServiceTarget, "DEVELOPMENT_TEAM", PlayerSettings.iOS.appleDeveloperTeamID);

         notificationServicePlist.WriteToFile (notificationServicePlistPath);

         FileUtil.CopyFileOrDirectory ("Assets/OneSignal/Editor/NotificationService.h", path + "/OneSignalNotificationExtensionService/NotificationService.h");
         FileUtil.CopyFileOrDirectory ("Assets/OneSignal/Editor/NotificationService.m", path + "/OneSignalNotificationExtensionService/NotificationService.m");

         project.WriteToFile (projPath);

         string projectFile = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
         string contents = File.ReadAllText(projectFile);

         InsertStaticFrameworkIntoTargetBuildPhaseFrameworks("libOneSignal", "CD84C25F20742FAB0035D524", notificationServiceTarget, ref contents, project);

         File.WriteAllText(projectFile, contents);
      }
   }

   // This function takes a static framework that is already linked to a different target in the project and links it to the specified target
   public static void InsertStaticFrameworkIntoTargetBuildPhaseFrameworks(string staticFrameworkName, string frameworkGuid, string target, ref string contents, PBXProject project) {

      //in order to find the fileRef, find the PBXBuildFile objects section of the PBXProject
      string splitString = " /* " + staticFrameworkName + ".a in Frameworks */ = {isa = PBXBuildFile; fileRef = ";
      string[] splitComponents = contents.Split(new string[] {splitString}, StringSplitOptions.None);

      if (splitComponents.Length < 2) {
         Debug.Log ("COMPONENT NOT FOUND 1");
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

      //fild the build phase ID number
      string targetBuildPhaseId = project.GetFrameworksBuildPhaseByTarget(target);
      string[] components = contents.Split(new string[] { targetBuildPhaseId + " /* Frameworks */ = {\n\t\t\tisa = PBXFrameworksBuildPhase;\n\t\t\tbuildActionMask = " }, StringSplitOptions.None);

      if (components.Length < 2) {
         Debug.Log("COMPONENT NOT FOUND 2");
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
}

#endif