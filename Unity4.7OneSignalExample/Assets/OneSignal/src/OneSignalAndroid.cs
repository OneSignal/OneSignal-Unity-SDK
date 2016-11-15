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

#if UNITY_ANDROID
using UnityEngine;
using System.Collections.Generic;
using OneSignalPush.MiniJSON;

public class OneSignalAndroid : OneSignalPlatform {
   private static AndroidJavaObject mOneSignal = null;

   public OneSignalAndroid(string gameObjectName, string googleProjectNumber, string appId, OneSignal.OSInFocusDisplayOption displayOption, OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel) {
        mOneSignal = new AndroidJavaObject("com.onesignal.OneSignalUnityProxy", gameObjectName, googleProjectNumber, appId, (int)logLevel, (int)visualLevel);
        SetInFocusDisplaying(displayOption);
   }

    public void SetLogLevel(OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel) {
        mOneSignal.Call("setLogLevel", (int)logLevel, (int)visualLevel);
    }

   public void SendTag(string tagName, string tagValue) {
      mOneSignal.Call("sendTag", tagName, tagValue);
   }

   public void SendTags(IDictionary<string, string> tags) {
      mOneSignal.Call("sendTags", Json.Serialize(tags));
   }

   public void GetTags() {
      mOneSignal.Call("getTags");
   }

   public void DeleteTag(string key) {
      mOneSignal.Call("deleteTag", key);
   }

   public void DeleteTags(IList<string> keys) {
      mOneSignal.Call("deleteTags", Json.Serialize(keys));
   }


   public void IdsAvailable() {
      mOneSignal.Call("idsAvailable");
   }

   public void RegisterForPushNotifications() { } // Doesn't apply to Android as the Native SDK always registers with GCM.

   public void EnableVibrate(bool enable) {
      mOneSignal.Call("enableVibrate", enable);
   }

   public void EnableSound(bool enable) {
      mOneSignal.Call("enableSound", enable);
   }

   public void SetInFocusDisplaying(OneSignal.OSInFocusDisplayOption display) {
      mOneSignal.Call("setInFocusDisplaying", (int)display);
   }

   public void SetSubscription(bool enable) {
      mOneSignal.Call("setSubscription", enable);
   }

   public void PostNotification(Dictionary<string, object> data) {
      mOneSignal.Call("postNotification", Json.Serialize(data));
   }

  public void SyncHashedEmail(string email) {
    mOneSignal.Call("syncHashedEmail", email);
  }

  public void PromptLocation() {
    mOneSignal.Call("promptLocation");
  }
  
  public void ClearOneSignalNotifications() {
    mOneSignal.Call("clearOneSignalNotifications");
  }
}
#endif