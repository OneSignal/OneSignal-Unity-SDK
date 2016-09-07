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

#if UNITY_WP8 && !UNITY_EDITOR
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

class OneSignalWP80 : OneSignalPlatform {

    public OneSignalWP80(string appId) {
		OneSignalSDK_WP80.OneSignal.Init(appId, (message, inAdditionalData, isActive) => {
            if (OneSignal.notificationDelegate != null) {
				Dictionary<string, object> additionalData = null;
				if (inAdditionalData != null)
					additionalData = inAdditionalData.ToDictionary(pair => pair.Key, pair=>(object)pair.Value);
				OneSignal.notificationDelegate(message, additionalData, isActive);
			}
        });
    }

    public void SendTag(string tagName, string tagValue) {
		OneSignalSDK_WP80.OneSignal.SendTag(tagName, tagValue);
    }

	public void SendTags(IDictionary<string, string> tags) {
		OneSignalSDK_WP80.OneSignal.SendTags(tags.ToDictionary(pair => pair.Key, pair=>(object)pair.Value));
	}
    
    public void SendPurchase(double amount) {
		OneSignalSDK_WP80.OneSignal.SendPurchase(amount);
    }
    
    public void GetTags() {
		OneSignalSDK_WP80.OneSignal.GetTags((tags) => {
           OneSignal.tagsReceivedDelegate(tags.ToDictionary(pair => pair.Key, pair=>(object)pair.Value));
        });
    }

    public void DeleteTag(string key) {
		OneSignalSDK_WP80.OneSignal.DeleteTag(key);
    }

	public void DeleteTags(IList<string> key) {
		OneSignalSDK_WP80.OneSignal.DeleteTags(key);
	}

    public void IdsAvailable() {
		OneSignalSDK_WP80.OneSignal.GetIdsAvailable((playerId, channelUri) => {
            OneSignal.idsAvailableDelegate(playerId, channelUri);
        });
    }

	// Not available the WP SDK.
	public void EnableInAppAlertNotification(bool enable) { }

	// Not available in WP SDK.
	public void SetSubscription(bool enable) {}

	// Not available in WP SDK.
	public void PostNotification(Dictionary<string, object> data) { }

    // Doesn't apply to Windows Phone: The Callback is setup in the constructor so this is never called.
    public void FireNotificationReceivedEvent(string jsonString, OneSignal.NotificationReceived notificationReceived) {}

    public void RegisterForPushNotifications() { } // Doesn't apply to Windows Phone: The Native SDK always registers.

    // The Native SDK does not implement these.
    public void SetEmail(string email) { }
    public void PromptLocation() { }
    public void SetLogLevel(OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel) {}
}
#endif