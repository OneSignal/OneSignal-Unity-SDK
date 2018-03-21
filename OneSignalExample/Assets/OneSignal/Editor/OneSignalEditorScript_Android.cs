/**
 * Modified MIT License
 * 
 * Copyright 2018 OneSignal
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

   static OneSignalEditorScriptAndroid() {
      createOneSignalAndroidManifest();
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