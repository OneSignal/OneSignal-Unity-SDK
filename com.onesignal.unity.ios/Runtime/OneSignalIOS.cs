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
        public override event NotificationReceivedDelegate NotificationReceived;
        public override event NotificationOpenedDelegate NotificationOpened;
        public override event InAppMessageClickedDelegate InAppMessageClicked;
        public override event OnStateChangeDelegate<PermissionState> PermissionStateChanged;
        public override event OnStateChangeDelegate<SubscriptionState> SubscriptionStateChanged;
        public override event OnStateChangeDelegate<EmailSubscriptionState> EmailSubscriptionStateChanged;
        public override event OnStateChangeDelegate<SMSSubscriptionState> SMSSubscriptionStateChanged;

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

        public override void RegisterForPushNotifications()
            => _registerForPushNotifications();

        public override async Task<OSNotificationPermission> PromptForPushNotificationsWithUserResponse() {
            var proxy = new BooleanCallbackProxy();
            _promptForPushNotificationsWithUserResponse(proxy.OnResponse);
            return await proxy ? OSNotificationPermission.Authorized : OSNotificationPermission.Denied;
        }

        public override void ClearOneSignalNotifications()
            => _clearOneSignalNotifications();

        public override async Task<Dictionary<string, object>> PostNotification(Dictionary<string, object> options) {
            var proxy = new StringCallbackProxy();
            _postNotification(Json.Serialize(options), proxy.OnResponse);
            return Json.Deserialize(await proxy) as Dictionary<string, object>;
        }

        public override void SetTrigger(string key, object value)
            => _setTrigger(key, value.ToString());

        public override void SetTriggers(Dictionary<string, object> triggers) {
            throw new System.NotImplementedException();
        }

        public override void RemoveTrigger(string key)
            => _removeTrigger(key);

        public override void RemoveTriggers(params string[] keys) {
            throw new System.NotImplementedException();
        }

        public override object GetTrigger(string key)
            => _getTrigger(key);

        public override Dictionary<string, object> GetTriggers() {
            throw new System.NotImplementedException();
        }

        public override bool InAppMessagesArePaused {
            get => _getInAppMessagesArePaused();
            set => _setInAppMessagesArePaused(value);
        }
        
        public override Task<Dictionary<string, object>> SendTag(string tagName, object tagValue) {
            throw new System.NotImplementedException();
        }

        public override Task<Dictionary<string, object>> SendTags(Dictionary<string, object> tags) {
            throw new System.NotImplementedException();
        }

        public override Task<Dictionary<string, object>> GetTags() {
            throw new System.NotImplementedException();
        }

        public override Task<Dictionary<string, object>> DeleteTag(string key) {
            throw new System.NotImplementedException();
        }

        public override Task<Dictionary<string, object>> DeleteTags(IEnumerable<string> keys) {
            throw new System.NotImplementedException();
        }

        public override Task<Dictionary<string, object>> SetExternalUserId(string externalId, string authHash = null) {
            throw new System.NotImplementedException();
        }

        public override Task SetEmail(string email, string authHash = null) {
            throw new System.NotImplementedException();
        }

        public override Task<Dictionary<string, object>> SetSMSNumber(string smsNumber, string authHash = null) {
            throw new System.NotImplementedException();
        }

        public override Task<Dictionary<string, object>> LogOut(LogOutOptions options = LogOutOptions.Email | LogOutOptions.SMS | LogOutOptions.ExternalUserId) {
            throw new System.NotImplementedException();
        }

        public override void PromptLocation()
            => _promptLocation();

        public override bool ShareLocation {
            get => _getShareLocation();
            set => _setShareLocation(value);
        }
        
        public override async Task<OutcomeEvent> SendOutcome(string name) {
            var proxy = new StringCallbackProxy();
            _sendOutcome(name, proxy.OnResponse);

            var response = Json.Deserialize(await proxy);
            
            // todo - convert to OutcomeEvent
            
            return null;
        }

        public override async Task<OutcomeEvent> SendUniqueOutcome(string name) {
            var proxy = new StringCallbackProxy();
            _sendUniqueOutcome(name, proxy.OnResponse);

            var response = Json.Deserialize(await proxy);
            
            // todo - convert to OutcomeEvent
            return null;
        }

        public override async Task<OutcomeEvent> SendOutcomeWithValue(string name, float value) {
            var proxy = new StringCallbackProxy();
            _sendOutcomeWithValue(name, value, proxy.OnResponse);

            var response = Json.Deserialize(await proxy);
            
            // todo - convert to OutcomeEvent
            return null;
        }
    }
}