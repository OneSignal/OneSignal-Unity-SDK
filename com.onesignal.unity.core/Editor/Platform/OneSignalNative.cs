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
    /// Implementationless variation of the OneSignal SDK so that it "runs" in the Editor
    /// </summary>
    internal sealed class OneSignalNative : OneSignal {
        public override event NotificationReceivedDelegate NotificationReceived;
        public override event NotificationOpenedDelegate NotificationOpened;
        public override event InAppMessageClickedDelegate InAppMessageClicked;
        public override event OnStateChangeDelegate<PermissionState> PermissionStateChanged;
        public override event OnStateChangeDelegate<PushSubscriptionState> PushSubscriptionStateChanged;
        public override event OnStateChangeDelegate<EmailSubscriptionState> EmailSubscriptionStateChanged;
        public override event OnStateChangeDelegate<SMSSubscriptionState> SMSSubscriptionStateChanged;

        public override bool PrivacyConsent { get; set; }
        public override bool RequiresPrivacyConsent { get; set; }

        public override void Initialize(string appId) {
            
        }

        public override void RegisterForPushNotifications() {
            
        }

        public override Task<NotificationPermission> PromptForPushNotificationsWithUserResponse() {
            return Task.FromResult(NotificationPermission.NotDetermined);
        }

        public override void ClearOneSignalNotifications() {
            
        }

        public override Task<Dictionary<string, object>> PostNotification(Dictionary<string, object> options) {
            return Task.FromResult(new Dictionary<string, object>());
        }

        public override void SetTrigger(string key, object value) {
            
        }

        public override void SetTriggers(Dictionary<string, object> triggers) {
            
        }

        public override void RemoveTrigger(string key) {
            
        }

        public override void RemoveTriggers(params string[] keys) {
            
        }

        public override object GetTrigger(string key) {
            return null;
        }

        public override Dictionary<string, object> GetTriggers() {
            return new Dictionary<string, object>();
        }

        public override bool InAppMessagesArePaused { get; set; }

        public override Task<bool> SendTag(string tagName, object tagValue) {
            return Task.FromResult(false);
        }

        public override Task<bool> SendTags(Dictionary<string, object> tags) {
            return Task.FromResult(false);
        }

        public override Task<Dictionary<string, object>> GetTags() {
            return Task.FromResult(new Dictionary<string, object>());
        }

        public override Task<bool> DeleteTag(string key) {
            return Task.FromResult(false);
        }

        public override Task<bool> DeleteTags(IEnumerable<string> keys) {
            return Task.FromResult(false);
        }

        public override Task<Dictionary<string, object>> SetExternalUserId(string externalId, string authHash = null) {
            return Task.FromResult(new Dictionary<string, object>());
        }

        public override Task SetEmail(string email, string authHash = null) {
            return Task.CompletedTask;
        }

        public override Task<Dictionary<string, object>> SetSMSNumber(string smsNumber, string authHash = null) {
            return Task.FromResult(new Dictionary<string, object>());
        }

        public override Task<Dictionary<string, object>> LogOut(
            LogOutOptions options = LogOutOptions.Email | LogOutOptions.SMS | LogOutOptions.ExternalUserId
        ) {
            return Task.FromResult(new Dictionary<string, object>());
        }

        public override void PromptLocation() {
            
        }

        public override bool ShareLocation  { get; set; }

        public override Task<bool> SendOutcome(string name) {
            return Task.FromResult(false);
        }

        public override Task<bool> SendUniqueOutcome(string name) {
            return Task.FromResult(false);
        }

        public override Task<bool> SendOutcomeWithValue(string name, float value) {
            return Task.FromResult(false);
        }
    }
}