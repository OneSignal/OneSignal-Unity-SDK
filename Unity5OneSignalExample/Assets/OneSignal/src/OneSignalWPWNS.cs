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

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

#if UNITY_WP_8_1 && !UNITY_EDITOR
public class OneSignalWPWNS : OneSignalPlatform {

   public OneSignalWPWNS(string appId) {
      OneSignalSDK_WP_WNS.ExternalInitUnity.Init(appId, (message, inAdditionalData, isActive) => {
         if (OneSignal.builder != null && OneSignal.builder.notificationOpenedDelegate != null) {
            Dictionary<string, object> additionalData = null;
            if (inAdditionalData != null)
               additionalData = inAdditionalData.ToDictionary(pair => pair.Key, pair => (object)pair.Value);

            OSNotificationOpenedResult result = new OSNotificationOpenedResult();
            result.action = new OSNotificationAction();
            result.action.type = OSNotificationAction.ActionType.Opened;

            result.notification = new OSNotification();
            result.notification.shown = !isActive;

            result.notification.payload = new OSNotificationPayload();
            result.notification.payload.body = message;
            result.notification.payload.additionalData = additionalData;

            OneSignal.builder.notificationOpenedDelegate(result);
         }
      });
   }
   
   public void SendTag(string tagName, string tagValue) {
      OneSignalSDK_WP_WNS.OneSignal.SendTag(tagName, tagValue);
   }
   
   public void SendTags(IDictionary<string, string> tags) {
      OneSignalSDK_WP_WNS.OneSignal.SendTags(tags.ToDictionary(pair => pair.Key, pair => (object)pair.Value));
   }
   
   public void SendPurchase(double amount) {
      OneSignalSDK_WP_WNS.OneSignal.SendPurchase(amount);
   }
   
   public void GetTags() {
      OneSignalSDK_WP_WNS.OneSignal.GetTags((tags) => {
         OneSignal.tagsReceivedDelegate(tags.ToDictionary(pair => pair.Key, pair => (object)pair.Value));
      });
   }
   
   public void DeleteTag(string key) {
      OneSignalSDK_WP_WNS.OneSignal.DeleteTag(key);
   }
   
   public void DeleteTags(IList<string> key) {
      OneSignalSDK_WP_WNS.OneSignal.DeleteTags(key);
   }
   
   public void IdsAvailable() {
      OneSignalSDK_WP_WNS.OneSignal.GetIdsAvailable((playerId, channelUri) => {
         OneSignal.idsAvailableDelegate(playerId, channelUri);
      });
   }
   
   // The following have not been implemented by the native WP8.1 SDK.
   public void SetSubscription(bool enable) {}
   public void PostNotification(Dictionary<string, object> data) { }
   public void PromptLocation() {}
   public void SyncHashedEmail(string email) {}
   public void SetLogLevel(OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel) {}


   // Doesn't apply to Windows Phone: The Native SDK always registers.
   public void RegisterForPushNotifications() {}
}
#endif