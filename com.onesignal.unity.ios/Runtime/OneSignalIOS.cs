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
using System.Threading.Tasks;

namespace OneSignalSDK {
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class OneSignalIOS : OneSignal {
        public override event NotificationLifecycleDelegate NotificationReceived;
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
            get => _getPrivacyConsent();
            set => _setPrivacyConsent(value);
        }
        
        public override bool RequiresPrivacyConsent {
            get => _getRequiresPrivacyConsent();
            set => _setRequiresPrivacyConsent(value);
        }

        public override void Initialize(string appId)
            => _initialize(appId);

        public override async Task<NotificationPermission> PromptForPushNotificationsWithUserResponse() {
            var proxy = new BooleanCallbackProxy();
            _promptForPushNotificationsWithUserResponse(proxy.OnResponse);
            return await proxy ? NotificationPermission.Authorized : NotificationPermission.Denied;
        }

        public override void ClearOneSignalNotifications()
            => SDKDebug.Log("ClearOneSignalNotifications invoked on iOS, does nothing");

        public override async Task<Dictionary<string, object>> PostNotification(Dictionary<string, object> options) {
            var proxy = new StringCallbackProxy();
            _postNotification(Json.Serialize(options), proxy.OnResponse);
            return Json.Deserialize(await proxy) as Dictionary<string, object>;
        }

        public override void SetTrigger(string key, object value)
            => _setTrigger(key, value.ToString());

        public override void SetTriggers(Dictionary<string, object> triggers)
            => _setTriggers(Json.Serialize(triggers));

        public override void RemoveTrigger(string key)
            => _removeTrigger(key);

        public override void RemoveTriggers(params string[] keys)
            => _removeTriggers(Json.Serialize(keys));

        public override object GetTrigger(string key)
            => _getTrigger(key);

        public override Dictionary<string, object> GetTriggers()
            => Json.Deserialize(_getTriggers()) as Dictionary<string, object>;

        public override bool InAppMessagesArePaused {
            get => _getInAppMessagesArePaused();
            set => _setInAppMessagesArePaused(value);
        }
        
        public override async Task<bool> SendTag(string key, object value) {
            var proxy = new BooleanCallbackProxy();
            _sendTag(key, value.ToString(), proxy.OnResponse);
            return await proxy;
        }

        public override async Task<bool> SendTags(Dictionary<string, object> tags) {
            var proxy = new BooleanCallbackProxy();
            _sendTags(Json.Serialize(tags), proxy.OnResponse);
            return await proxy;
        }

        public override async Task<Dictionary<string, object>> GetTags() {
            var proxy = new StringCallbackProxy();
            _getTags(proxy.OnResponse);
            return Json.Deserialize(await proxy) as Dictionary<string, object>;
        }

        public override async Task<bool> DeleteTag(string key) {
            var proxy = new BooleanCallbackProxy();
            _deleteTag(key, proxy.OnResponse);
            return await proxy;
        }

        public override async Task<bool> DeleteTags(params string[] keys) {
            var proxy = new BooleanCallbackProxy();
            _deleteTags(Json.Serialize(keys), proxy.OnResponse);
            return await proxy;
        }

        public override async Task<bool> SetExternalUserId(string externalId, string authHash = null) {
            var proxy = new BooleanCallbackProxy();
            _setExternalUserId(externalId, authHash, proxy.OnResponse);
            return await proxy;
        }

        public override async Task<bool> SetEmail(string email, string authHash = null) {
            var proxy = new BooleanCallbackProxy();
            _setEmail(email, authHash, proxy.OnResponse);
            return await proxy;
        }

        public override async Task<bool> SetSMSNumber(string smsNumber, string authHash = null) {
            var proxy = new BooleanCallbackProxy();
            _setSMSNumber(smsNumber, authHash, proxy.OnResponse);
            return await proxy;
        }

        public override async Task<Dictionary<string, object>> LogOut(LogOutOptions options = LogOutOptions.Email | LogOutOptions.SMS | LogOutOptions.ExternalUserId) {
            throw new System.NotImplementedException();
        }

        public override void PromptLocation()
            => _promptLocation();

        public override bool ShareLocation {
            get => _getShareLocation();
            set => _setShareLocation(value);
        }
        
        public override async Task<bool> SendOutcome(string name) {
            var proxy = new BooleanCallbackProxy();
            _sendOutcome(name, proxy.OnResponse);
            return await proxy;
        }

        public override async Task<bool> SendUniqueOutcome(string name) {
            var proxy = new BooleanCallbackProxy();
            _sendUniqueOutcome(name, proxy.OnResponse);
            return await proxy;
        }

        public override async Task<bool> SendOutcomeWithValue(string name, float value) {
            var proxy = new BooleanCallbackProxy();
            _sendOutcomeWithValue(name, value, proxy.OnResponse);
            return await proxy;
        }
    }
}