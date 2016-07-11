#if UNITY_ANDROID
using UnityEditor;
using System.IO;
using UnityEngine;

[InitializeOnLoad]
public class OneSignalEditorScript {

   // Copies `AndroidManifestTemplate.xml` to `AndroidManifest.xml`
   //   then replace `${manifestApplicationId}` with current packagename in the Unity settings. 
   static OneSignalEditorScript () {
      string oneSignalConfigPath = "Assets/Plugins/Android/OneSignalConfig/";
      string manifestFullPath = oneSignalConfigPath + "AndroidManifest.xml";

      File.Copy(oneSignalConfigPath + "AndroidManifestTemplate.xml", manifestFullPath, true);

      StreamReader streamReader = new StreamReader(manifestFullPath);
      string body = streamReader.ReadToEnd();
      streamReader.Close();

      body = body.Replace("${manifestApplicationId}", PlayerSettings.bundleIdentifier);
      using (var streamWriter = new StreamWriter(manifestFullPath, false))
      {
         streamWriter.Write(body);
      }
   }
}
#endif