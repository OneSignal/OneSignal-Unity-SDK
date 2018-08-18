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
using System.Collections;

#if !UNITY_CLOUD_BUILD && UNITY_EDITOR && UNITY_2017_1_OR_NEWER

[InitializeOnLoad]
public class OneSignalEditorCheckUpdateScript : AssetPostprocessor
{
   //this key is used for the current unity session only
   public static string sessionState = "onesignal_checked_update";

   static OneSignalUpdateRequest request;

   static OneSignalEditorCheckUpdateScript()
   {
      Request();
   }

   static void Request()
   {
      //if the SDK already checked for an update during this session, no need to do so again
      if (SessionState.GetBool(sessionState, false)) {
         return;
      }
      SessionState.SetBool(sessionState, true);

      //because the update request is a MonoBehaviour object, must add as a component to a GameObject...
      GameObject obj = new GameObject();

      request = obj.AddComponent<OneSignalUpdateRequest>();

      request.Start();
   }
}


public class OneSignalUpdateRequest : MonoBehaviour
{

   //this key is saved between unity sessions and is essentially permanent
   public static string sessionStatePersisted = "onesignal_checked_update_persisted";

   private IEnumerator coroutine;

   public void Start()
   {
      coroutine = FetchCurrentVersion("https://api.github.com/repos/OneSignal/OneSignal-Unity-SDK/releases/latest");

      StartCoroutine(coroutine);
   }

   IEnumerator FetchCurrentVersion(string url)
   {
      // issue a GET request to get the latest release
      var request = UnityWebRequest.Get(url);

      yield return request.SendWebRequest();
      
      if (request.isNetworkError || request.isHttpError) {
         if (request.error != null) {
            Debug.LogError("OneSignal Update Checker encountered an unknown error");
         } else {
            Debug.LogError("OneSignal Update Checker encountered an error: " + request.error);
         }

         //since there was an error, we shouldn't attempt to serialize data from the request
         yield break;
      }
      
      var separator = Path.DirectorySeparatorChar;

      //parse JSON to extract the latest version
      var json = Json.Deserialize(request.downloadHandler.text) as System.Collections.Generic.Dictionary<string, object>;

      var latestVersionString = (json["tag_name"] as string);
      var currentVersionString = File.ReadAllText("Assets" + separator + "OneSignal" + separator + "VERSION").Replace("\n", "");

      //remove . to convert "2.8.2" to "282" for example
      var latestVersion = int.Parse(latestVersionString.Replace(".", ""));

      var localVersion = int.Parse(currentVersionString.Replace(".", ""));

      if (latestVersion > localVersion) {
         var updateMessage = "A new version of the OneSignal Unity SDK (" + latestVersionString + ") is available. You are currently using " + currentVersionString;
 
         Debug.LogWarning(updateMessage + "\nYou can download the new update from https://github.com/OneSignal/OneSignal-Unity-SDK/releases/latest\n");

         // if the user has already seen an Update dialog for a new version, don't show it to them again for this version
         if (!EditorPrefs.GetBool(sessionStatePersisted + latestVersionString, false)) {
           //display an update message
           EditorUtility.DisplayDialog("OneSignal Update Available", updateMessage + ". See the Console for a link to our latest release.", "Ok");
         }

         EditorPrefs.SetBool(sessionStatePersisted + latestVersionString, true);
      }

      yield break;
   }
}

#endif