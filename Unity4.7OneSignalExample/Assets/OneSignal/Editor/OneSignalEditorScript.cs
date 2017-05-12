/**
 * Modified MIT License
 * 
 * Copyright 2016 OneSignal
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

using System;
using System.IO;

using UnityEditor;

#if UNITY_ANDROID && UNITY_EDITOR
using System.Collections.Generic;

[InitializeOnLoad]
public class OneSignalEditorScriptAndroid : AssetPostprocessor {

   /// <summary>Instance of the PlayServicesSupport resolver</summary>
   public static object svcSupport;

   private static readonly string PluginName = "OneSignal";
   // If you run into conflicts OneSignal supports back to version 9.0.0.
   private static readonly string PLAY_SERVICES_VERSION = "10.0.+";
   private static readonly string ANDROID_SUPPORT_VERSION = "24.0.+";

   static OneSignalEditorScriptAndroid() {
      createOneSignalAndroidManifest();
      addGMSLibrary();
   }

   private static void addGMSLibrary() {
      Type playServicesSupport = Google.VersionHandler.FindClass(
         "Google.JarResolver", "Google.JarResolver.PlayServicesSupport");
      if (playServicesSupport == null)
         return;

      svcSupport = svcSupport ?? Google.VersionHandler.InvokeStaticMethod(
        playServicesSupport, "CreateInstance",
        new object[] {
                PluginName,
                EditorPrefs.GetString("AndroidSdkRoot"),
                "ProjectSettings"
        });

      Google.VersionHandler.InvokeInstanceMethod(
         svcSupport, "DependOn",
         new object[] {
            "com.google.android.gms",
            "play-services-gcm",
            PLAY_SERVICES_VERSION
         },
         namedArgs: new Dictionary<string, object>() {
             {"packageIds", new string[] { "extra-google-m2repository" } }
         });

      Google.VersionHandler.InvokeInstanceMethod(
         svcSupport, "DependOn",
         new object[] {
            "com.google.android.gms",
            "play-services-location",
            PLAY_SERVICES_VERSION
         },
         namedArgs: new Dictionary<string, object>() {
             {"packageIds", new string[] { "extra-google-m2repository" } }
         });


      Google.VersionHandler.InvokeInstanceMethod(
         svcSupport, "DependOn",
         new object[] {
            "com.android.support",
            "customtabs",
            ANDROID_SUPPORT_VERSION
         },
         namedArgs: new Dictionary<string, object>() {
             {"packageIds", new string[] { "extra-android-m2repository" } }
         });

      // Adds play-services-base, play-services-basement, play-services-iid, and support-v4 will be automaticly added.
      // Also adds play-services-tasks but this isn't used by OneSignal, it just added as a depency from the above.


      // Setting 8.3+ does not work with unity-jar-resolver-1.2.0 and GooglePlayGamesPlugin-0.9.34.
      //   It creates conflicting aar files with mismatched version of 8.4 and 9.4
      // svcSupport.DependOn("com.google.android.gms", "play-services-gcm", "8.3+");
      // svcSupport.DependOn("com.google.android.gms", "play-services-location", "8.3+");
      // play-services-base, play-services-basement, and support-v4 will be automaticly added.
      // play-services-maps and play-services-measurement are not used by OneSignal
      //    but are included as depencies from the other parts of play-services.
   }
   
   // Copies `AndroidManifestTemplate.xml` to `AndroidManifest.xml`
   //   then replace `${manifestApplicationId}` with current packagename in the Unity settings. 
   private static void createOneSignalAndroidManifest() {
      string oneSignalConfigPath = "Assets/Plugins/Android/OneSignalConfig/";
      string manifestFullPath = oneSignalConfigPath + "AndroidManifest.xml";

      File.Copy(oneSignalConfigPath + "AndroidManifestTemplate.xml", manifestFullPath, true);

      StreamReader streamReader = new StreamReader(manifestFullPath);
      string body = streamReader.ReadToEnd();
      streamReader.Close();

      #if UNITY_5_6_OR_NEWER
         body = body.Replace("${manifestApplicationId}", PlayerSettings.applicationIdentifier);
      #else
         body = body.Replace("${manifestApplicationId}", PlayerSettings.bundleIdentifier);     
      #endif
      using (var streamWriter = new StreamWriter(manifestFullPath, false)) {
         streamWriter.Write(body);
      }
   }
}
#endif