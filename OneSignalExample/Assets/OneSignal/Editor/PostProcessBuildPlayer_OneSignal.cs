using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.IO;

public struct framework {
	public string name;
	public string id;
	public string fileId;

	public framework(string fName, string fId, string fFileid) {
		name = fName;
		id = fId;
		fileId = fFileid;
	}
}

public class PostBuildTrigger {

	[PostProcessBuild]
	public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject) {

		LinkLibraries(target, pathToBuiltProject);
	}

	public static void LinkLibraries(BuildTarget target, string pathToBuiltProject) {
		if(target == BuildTarget.iOS)
		{
			framework[] frameworksToAdd = new framework[1];

			frameworksToAdd[0] = new framework("UserNotifications", "CAF63D112040CD8E00A651DC", "CAF63D102040CD8E00A651DC");

			string projectFile = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
			string contents = File.ReadAllText(projectFile);

			foreach (framework framework in frameworksToAdd) {
				Debug.Log("Adding framework " + framework.name + " to project");

				AddFrameworkToProject(framework, ref contents);
			}

			File.WriteAllText(projectFile, contents);

			Debug.Log ("Saving file to disk " + projectFile + "\n\n" + contents);
		}
	}

	public static void AddFrameworkToProject(framework framework, ref string contents) {
		Debug.Log("Switching: \n " + "Ref = 8AC71EC319E7FBA90027502F /* OrientationSupport.mm */; };" + " \nwith:\n" + "Ref = 8AC71EC319E7FBA90027502F /* OrientationSupport.mm */; };\n\t\t" + framework.id + " /* " + framework.name + ".framework in Frameworks */ = {isa = PBXBuildFile; fileRef = " + framework.fileId + " /* " + framework.name + ".framework */; };");

		contents = contents.Replace("Ref = 8AC71EC319E7FBA90027502F /* OrientationSupport.mm */; };",
			"Ref = 8AC71EC319E7FBA90027502F /* OrientationSupport.mm */; };\n\t\t" + framework.id + " /* " + framework.name + ".framework in Frameworks */ = {isa = PBXBuildFile; fileRef = " + framework.fileId + " /* " + framework.name + ".framework */; };");
		contents = contents.Replace("wnFileType = text.plist.xml; path = Info.plist; sourceTree = \"<group>\"; };",
			"wnFileType = text.plist.xml; path = Info.plist; sourceTree = \"<group>\"; };\n\t\t" + framework.fileId + " /* " + framework.name + ".framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = " + framework.name + ".framework; path = System/Library/Frameworks/" + framework.name + ".framework; sourceTree = SDKROOT; };");
		contents = contents.Replace("1D60589F0D05DD5A006BFB54 /* Foundation.framework in Frameworks */,",
			framework.id + " /* " + framework.name + ".framework in Frameworks */,\n\t\t\t\t1D60589F0D05DD5A006BFB54 /* Foundation.framework in Frameworks */,");
		contents = contents.Replace("1D30AB110D05D00D00671497 /* Foundation.framework */,",
			framework.fileId + " /* " + framework.name + ".framework */,\n\t\t\t\t1D30AB110D05D00D00671497 /* Foundation.framework */,");
	}
}
