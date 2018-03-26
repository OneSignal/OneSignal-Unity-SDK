using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

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
      }
   }

   static void AddFrameworks(PBXProject project, string targetGUID)
   {
      project.AddFrameworkToProject(targetGUID, "UserNotifications.framework", false);
   }
}

#endif