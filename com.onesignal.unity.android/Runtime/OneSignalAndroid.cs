/*
 * Modified MIT License
 *
 * Copyright 2021 OneSignal
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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OneSignalSDK {
    /// <summary>
    /// public static (?!void|class)
    /// </summary>
    public partial class OneSignalAndroid : OneSignal {
        public override event NotificationReceivedDelegate NotificationReceived;
        public override event NotificationOpenedDelegate NotificationOpened;
        public override event InAppMessageClickedDelegate InAppMessageClicked;
        public override event OnStateChangeDelegate<PermissionState> PermissionStateChanged;
        public override event OnStateChangeDelegate<SubscriptionState> SubscriptionStateChanged;
        public override event OnStateChangeDelegate<EmailSubscriptionState> EmailSubscriptionStateChanged;

        public override bool PrivacyConsent {
            get => _sdkClass.CallStatic<bool>("userProvidedPrivacyConsent");
            set => _sdkClass.CallStatic("provideUserConsent", value);
        }
        
        public override bool RequiresPrivacyConsent {
            get => _sdkClass.CallStatic<bool>("requiresUserPrivacyConsent"); 
            set => _sdkClass.CallStatic("setRequiresUserPrivacyConsent", value); 
        }
        
        public override void Initialize(string appId) {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var context = activity.Call<AndroidJavaObject>("getApplicationContext");

            _sdkClass.CallStatic("initWithContext", context);
            _sdkClass.CallStatic("setAppId", appId);
        }

        public override void RegisterForPushNotifications() {
            // do nothing - iOS Only
        }

        public override Task<OSNotificationPermission> PromptForPushNotificationsWithUserResponse() {
            // cancels immediately, iOS only
            return Task.FromCanceled<OSNotificationPermission>(new CancellationToken());
        }

        public override void ClearOneSignalNotifications()
            => _sdkClass.CallStatic("clearOneSignalNotifications");

        public override void AddTrigger(string key, object value)
            => _sdkClass.CallStatic("addTrigger", key, value);

        public override void AddTriggers(Dictionary<string, object> triggers)
        {
            throw new System.NotImplementedException();
        }

        public override void RemoveTriggerForKey(string key)
            => _sdkClass.CallStatic("removeTriggerForKey", key);

        public override void RemoveTriggersForKeys(IList<string> keys)
        {
            throw new System.NotImplementedException();
        }

        public override object GetTriggerValueForKey(string key)
        {
            throw new System.NotImplementedException();
        }

        public override bool InAppMessagesArePaused {
            get => _sdkClass.CallStatic<bool>("isInAppMessagingPaused");
            set => _sdkClass.CallStatic("pauseInAppMessages", value);
        }

        public override void SendTag(string tagName, string tagValue)
        {
            throw new System.NotImplementedException();
        }

        public override void SendTags(IDictionary<string, string> tags)
        {
            throw new System.NotImplementedException();
        }

        public override void GetTags()
        {
            throw new System.NotImplementedException();
        }

        public async Task<Dictionary<string, object>> GetTagsAsync()
            => await _callAsync<Dictionary<string, object>, OSGetTagsHandler>("GetTags");

        public override Task<Dictionary<string, object>> RefreshTags()
        {
            throw new System.NotImplementedException();
        }

        public override void DeleteTag(string key)
        {
            throw new System.NotImplementedException();
        }

        public override void DeleteTags(IEnumerable<string> keys)
        {
            throw new System.NotImplementedException();
        }

        public override void SetExternalUserId(string externalId)
        {
            throw new System.NotImplementedException();
        }

        public override void SetExternalUserId(string externalId, string authHashToken)
        {
            throw new System.NotImplementedException();
        }

        public override void SetEmail(string email)
        {
            throw new System.NotImplementedException();
        }

        public override void SetEmail(string email, string emailAuthToken)
        {
            throw new System.NotImplementedException();
        }

        public override void LogOut(LogOutOptions options = LogOutOptions.ExternalUserId)
        {
            throw new System.NotImplementedException();
        }

        public override void PromptLocation()
        {
            throw new System.NotImplementedException();
        }

        public override bool ShareLocation { get; set; }

        public override void SendOutcome(string name)
        {
            throw new System.NotImplementedException();
        }

        public override void SendUniqueOutcome(string name)
        {
            throw new System.NotImplementedException();
        }

        public override void SendOutcomeWithValue(string name, float value)
        {
            throw new System.NotImplementedException();
        }
    }
}