using System.IO;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

#if UNITY_IPHONE && UNITY_EDITOR

/*
   Adds required frameworks (currently just UserNotifications.framework) to the iOS project
   To add further frameworks in the build process, just add a new framework to the Frameworks array
*/

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

         AddFrameworks(project, targetGUID);

         // Write.
         File.WriteAllText(projectPath, project.WriteToString());

         var projPath = PBXProject.GetPBXProjectPath(path);
         
         project.ReadFromFile (projPath);

         var pathToNotificationService = path + "/OneSignalNotificationExtensionService";

         Debug.Log ("Creating directory for notification service extension: " + pathToNotificationService);

         Directory.CreateDirectory (pathToNotificationService);

         var plistPath = "Assets/OneSignal/Editor/Info.plist";

         var notificationServicePlistPath = path + "/OneSignalNotificationExtensionService/Info.plist";

         Debug.Log ("Extension PLIST Path: " + notificationServicePlistPath);
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
         project.AddFrameworkToProject (notificationServiceTarget, "libOneSignal.a", true);

         project.SetBuildProperty (notificationServiceTarget, "ARCHS", "$(ARCHS_STANDARD)");
         project.SetBuildProperty (notificationServiceTarget, "DEVELOPMENT_TEAM", PlayerSettings.iOS.appleDeveloperTeamID);
         notificationServicePlist.WriteToFile (notificationServicePlistPath);

         FileUtil.CopyFileOrDirectory ("Assets/OneSignal/Editor/NotificationService.h", path + "/OneSignalNotificationExtensionService/NotificationService.h");
         FileUtil.CopyFileOrDirectory ("Assets/OneSignal/Editor/NotificationService.m", path + "/OneSignalNotificationExtensionService/NotificationService.m");

         project.WriteToFile (projPath);
      }
   }

   static void AddFrameworks(PBXProject project, string targetGUID)
   {
      project.AddFrameworkToProject(targetGUID, "UserNotifications.framework", false);
   }
}

#endif