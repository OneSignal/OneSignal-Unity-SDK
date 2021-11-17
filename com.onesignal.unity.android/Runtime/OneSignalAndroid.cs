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
    public sealed partial class OneSignalAndroid : OneSignal {
        public override event NotificationWillShowDelegate NotificationWillShow;
        public override event NotificationActionDelegate NotificationOpened;
        public override event InAppMessageLifecycleDelegate InAppMessageWillDisplay;
        public override event InAppMessageLifecycleDelegate InAppMessageDidDisplay;
        public override event InAppMessageLifecycleDelegate InAppMessageWillDismiss;
        public override event InAppMessageLifecycleDelegate InAppMessageDidDismiss;
        public override event InAppMessageActionDelegate InAppMessageTriggeredAction;
        public override event StateChangeDelegate<PermissionState> PermissionStateChanged;
        public override event StateChangeDelegate<PushSubscriptionState> PushSubscriptionStateChanged;
        public override event StateChangeDelegate<EmailSubscriptionState> EmailSubscriptionStateChanged;
        public override event StateChangeDelegate<SMSSubscriptionState> SMSSubscriptionStateChanged;

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

            _sdkClass.CallStatic("initWithContext", activity);
            _sdkClass.CallStatic("setAppId", appId);

            _completedInit(appId);
        }

        public override Task<NotificationPermission> PromptForPushNotificationsWithUserResponse()
            => Task.FromResult(NotificationPermission.NotDetermined);

        public override void ClearOneSignalNotifications()
            => _sdkClass.CallStatic("clearOneSignalNotifications");

        public override async Task<Dictionary<string, object>> PostNotification(Dictionary<string, object> options) {
            var proxy = new PostNotificationResponseHandler();
            _sdkClass.CallStatic("postNotification", options.ToJSONObject(), proxy);
            return await proxy;
        }

        public override void SetTrigger(string key, string value)
            => _sdkClass.CallStatic("addTrigger", key, value);

        public override void SetTriggers(Dictionary<string, string> triggers)
            => _sdkClass.CallStatic("addTriggers", triggers.ToMap());

        public override void RemoveTrigger(string key)
            => _sdkClass.CallStatic("removeTriggerForKey", key);

        public override void RemoveTriggers(params string[] keys)
            => _sdkClass.CallStatic("removeTriggersForKeys", Json.Serialize(keys));

        public override string GetTrigger(string key) {
            var triggerVal = _sdkClass.CallStatic<AndroidJavaObject>("getTriggerValueForKey", key);
            return triggerVal.Call<string>("toString");
        }

        public override Dictionary<string, string> GetTriggers()
            => _sdkClass.CallStatic<AndroidJavaObject>("getTriggers").MapToDictionary();

        public override bool InAppMessagesArePaused {
            get => _sdkClass.CallStatic<bool>("isInAppMessagingPaused");
            set => _sdkClass.CallStatic("pauseInAppMessages", value);
        }

        public override async Task<bool> SendTag(string key, object value) {
            _sdkClass.CallStatic("sendTag", key, value.ToString());
            return await Task.FromResult(true); // no callback currently available on Android
        }

        public override async Task<bool> SendTags(Dictionary<string, object> tags) {
            var proxy = new ChangeTagsUpdateHandler();
            _sdkClass.CallStatic("sendTags", tags.ToJSONObject(), proxy);
            return await proxy;
        }

        public override async Task<Dictionary<string, object>> GetTags() {
            var proxy = new OSGetTagsHandler();
            _sdkClass.CallStatic("getTags", proxy);
            return await proxy;
        }

        public override async Task<bool> DeleteTag(string key) {
            var proxy = new ChangeTagsUpdateHandler();
            _sdkClass.CallStatic("deleteTag", key, proxy);
            return await proxy;
        }

        public override async Task<bool> DeleteTags(params string[] keys) {
            var proxy = new ChangeTagsUpdateHandler();
            _sdkClass.CallStatic("deleteTags", keys.ToList(), proxy);
            return await proxy;
        }

        public override async Task<bool> SetExternalUserId(string externalId, string authHash = null) {
            var proxy = new OSExternalUserIdUpdateCompletionHandler();
            _sdkClass.CallStatic("setExternalUserId", externalId, authHash, proxy);
            return await proxy;
        }

        public override async Task<bool> SetEmail(string email, string authHash = null) {
            var proxy = new EmailUpdateHandler();
            _sdkClass.CallStatic("setEmail", email, authHash, proxy);
            return await proxy;
        }

        public override async Task<bool> SetSMSNumber(string smsNumber, string authHash = null) {
            var proxy = new OSSMSUpdateHandler();
            _sdkClass.CallStatic("setSMSNumber", smsNumber, authHash, proxy);
            return await proxy;
        }

        public override async Task<bool> LogOutEmail() {
            var proxy = new EmailUpdateHandler();
            _sdkClass.CallStatic("logoutEmail", proxy);
            return await proxy;
        }

        public override async Task<bool> LogOutSMS() {
            var proxy = new OSSMSUpdateHandler();
            _sdkClass.CallStatic("logoutSMSNumber", proxy);
            return await proxy;
        }

        public override void PromptLocation()
            => _sdkClass.CallStatic("promptLocation");

        public override bool ShareLocation {
            get => _sdkClass.CallStatic<bool>("isLocationShared");
            set => _sdkClass.CallStatic("setLocationShared", value);
        }

        public override async Task<bool> SendOutcome(string name) {
            var proxy = new OutcomeCallback();
            _sdkClass.CallStatic("sendOutcome", name, proxy);
            return await proxy;
        }

        public override async Task<bool> SendUniqueOutcome(string name) {
            var proxy = new OutcomeCallback();
            _sdkClass.CallStatic("sendUniqueOutcome", name, proxy);
            return await proxy;
        }

        public override async Task<bool> SendOutcomeWithValue(string name, float value) {
            var proxy = new OutcomeCallback();
            _sdkClass.CallStatic("sendOutcomeWithValue", name, value, proxy);
            return await proxy;
        }
    }
}