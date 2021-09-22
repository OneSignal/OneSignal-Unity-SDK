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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OneSignalSDK {
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class OneSignalAndroid : OneSignal {
        public override event NotificationReceivedDelegate NotificationReceived;
        public override event NotificationOpenedDelegate NotificationOpened;
        public override event InAppMessageClickedDelegate InAppMessageClicked;
        public override event OnStateChangeDelegate<PermissionState> PermissionStateChanged;
        public override event OnStateChangeDelegate<SubscriptionState> SubscriptionStateChanged;
        public override event OnStateChangeDelegate<EmailSubscriptionState> EmailSubscriptionStateChanged;
        public override event OnStateChangeDelegate<SMSSubscriptionState> SMSSubscriptionStateChanged;

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
            var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var context     = activity.Call<AndroidJavaObject>("getApplicationContext");

            _sdkClass.CallStatic("initWithContext", context);
            _sdkClass.CallStatic("setAppId", appId);
        }

        public override void RegisterForPushNotifications()
            => SDKDebug.Log("RegisterForPushNotifications invoked on Android, does nothing.");

        public override Task<OSNotificationPermission> PromptForPushNotificationsWithUserResponse() {
            // cancels immediately, iOS only
            // todo - is cancellation the right solution?
            return Task.FromCanceled<OSNotificationPermission>(new CancellationToken());
        }

        public override void ClearOneSignalNotifications()
            => _sdkClass.CallStatic("clearOneSignalNotifications");

        public override Task<Dictionary<string, object>> PostNotification(Dictionary<string, object> options)
            => _callAsync<Dictionary<string, object>, PostNotificationResponseHandler>("postNotification", options.ToJSONObject());

        public override void SetTrigger(string key, object value)
            => _sdkClass.CallStatic("addTrigger", key, value);

        public override void SetTriggers(Dictionary<string, object> triggers)
            => _sdkClass.CallStatic("addTriggers", triggers.ToMap());

        public override void RemoveTrigger(string key)
            => _sdkClass.CallStatic("removeTriggerForKey", key);

        public override void RemoveTriggers(params string[] keys)
            => _sdkClass.CallStatic("removeTriggersForKeys", keys.ToList()); // todo - test me thoroughly

        public override object GetTrigger(string key)
            => _sdkClass.CallStatic<object>("getTriggerValueForKey", key);

        public override Dictionary<string, object> GetTriggers()
            => _sdkClass.CallStatic<AndroidJavaObject>("getTriggers").MapToDictionary();

        public override bool InAppMessagesArePaused {
            get => _sdkClass.CallStatic<bool>("isInAppMessagingPaused");
            set => _sdkClass.CallStatic("pauseInAppMessages", value);
        }

        public override async Task<Dictionary<string, object>> SendTag(string tagName, object tagValue)
            => await _callAsync<Dictionary<string, object>, ChangeTagsUpdateHandler>("sendTag", tagName, tagValue.ToString());

        public override async Task<Dictionary<string, object>> SendTags(Dictionary<string, object> tags)
            => await _callAsync<Dictionary<string, object>, ChangeTagsUpdateHandler>("sendTags", tags.ToJSONObject());

        public override async Task<Dictionary<string, object>> GetTags()
            => await _callAsync<Dictionary<string, object>, OSGetTagsHandler>("getTags");

        public override async Task<Dictionary<string, object>> DeleteTag(string key)
            => await _callAsync<Dictionary<string, object>, ChangeTagsUpdateHandler>("deleteTag", key);

        public override async Task<Dictionary<string, object>> DeleteTags(IEnumerable<string> keys)
            => await _callAsync<Dictionary<string, object>, ChangeTagsUpdateHandler>("deleteTags", keys.ToList());

        public override async Task<Dictionary<string, object>> SetExternalUserId(string externalId, string authHash = null)
            => await _callAsync<Dictionary<string, object>, OSExternalUserIdUpdateCompletionHandler>("setExternalUserId", externalId, authHash);

        public override async Task SetEmail(string email, string authHash = null)
            => await _callAsync<object, EmailUpdateHandler>("setEmail", email, authHash);

        public override async Task<Dictionary<string, object>> SetSMSNumber(string smsNumber, string authHash = null)
            => await _callAsync<Dictionary<string, object>, OSSMSUpdateHandler>("setSMSNumber", smsNumber, authHash);

        public override Task<Dictionary<string, object>> LogOut(
            LogOutOptions options = LogOutOptions.Email | LogOutOptions.SMS | LogOutOptions.ExternalUserId
        ) {
            throw new System.NotImplementedException();
        }

        public override void PromptLocation()
            => SDKDebug.Log("PromptLocation invoked on Android, does nothing.");

        public override bool ShareLocation {
            get => _sdkClass.CallStatic<bool>("isLocationShared");
            set => _sdkClass.CallStatic("setLocationShared", value);
        }

        public override Task<OutcomeEvent> SendOutcome(string name)
            => _callAsync<OutcomeEvent, OutcomeCallback>("sendOutcome", name);

        public override Task<OutcomeEvent> SendUniqueOutcome(string name)
            => _callAsync<OutcomeEvent, OutcomeCallback>("sendUniqueOutcome", name);

        public override Task<OutcomeEvent> SendOutcomeWithValue(string name, float value)
            => _callAsync<OutcomeEvent, OutcomeCallback>("sendOutcomeWithValue", name);
    }
}