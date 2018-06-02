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

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using OneSignalPush.MiniJSON;
using System.Collections.Generic;

#if !UNITY_CLOUD_BUILD && UNITY_EDITOR 

[InitializeOnLoad]
public class OneSignalEditorCheckUpdateScript : AssetPostprocessor {
   
   //this key is used for the current unity session only
   public static string sessionState = "onesignal_checked_update";
   
   //this key is saved between unity sessions and is essentially permanent
   public static string sessionStatePersisted = "onesignal_checked_update_persisted";
   
   static OneSignalEditorCheckUpdateScript() {
      Request();
   }
   
   static void Request()
   {
      if (SessionState.GetBool(sessionState, false) == true) {
         return;
      }

      SessionState.SetBool(sessionState, true);
      
      // issue a GET request to get the latest release
      var url = "https://api.github.com/repos/OneSignal/OneSignal-Unity-SDK/releases/latest";
      
      var request = UnityWebRequest.Get(url);

      request.SendWebRequest();

      while (request.isDone == false) { }

      if (request.isNetworkError || request.isHttpError)
      {
         Debug.LogError("OneSignal Update Checker Encountered an error: ");
         Debug.LogError(request.error);
      }
      
      //parse JSON to extract the latest version
      var json = Json.Deserialize(request.downloadHandler.text) as Dictionary<string, object>;
      
      var latestVersionString = (json["tag_name"] as string);
      var currentVersionString = File.ReadAllText("Assets/OneSignal/VERSION");

      //remove . to convert "2.8.2" to "282" for example
      var latestVersion = int.Parse(latestVersionString.Replace(".", ""));

      var localVersion = int.Parse(currentVersionString.Replace(".", ""));

      if (latestVersion > localVersion)
      {
         var updateMessage = "A new version of the OneSignal Unity SDK (" + latestVersionString + ") is available. You are currently using " + currentVersionString;
         
         if (EditorPrefs.GetBool(sessionStatePersisted + latestVersionString, false) == true) {
            EditorPrefs.SetBool(sessionStatePersisted + latestVersionString, true);
            //display an update message
            Debug.LogWarning(updateMessage);
         } else {
            EditorUtility.DisplayDialog("OneSignal Update Available", updateMessage, "Ok");
         }
      }
      
   }
   
}

#endif