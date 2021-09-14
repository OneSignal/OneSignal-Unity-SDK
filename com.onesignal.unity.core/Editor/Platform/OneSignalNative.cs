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
        public override event OnStateChangeDelegate<SubscriptionState> SubscriptionStateChanged;
        public override event OnStateChangeDelegate<EmailSubscriptionState> EmailSubscriptionStateChanged;

        public override bool PrivacyConsent { get; set; }
        public override bool RequiresPrivacyConsent { get; set; }

        public override void Initialize(string appId) {
            throw new System.NotImplementedException();
        }

        public override void RegisterForPushNotifications() {
            throw new System.NotImplementedException();
        }

        public override Task<OSNotificationPermission> PromptForPushNotificationsWithUserResponse() {
            throw new System.NotImplementedException();
        }

        public override void ClearOneSignalNotifications() {
            throw new System.NotImplementedException();
        }

        public override Task<Dictionary<string, object>> PostNotification(Dictionary<string, object> options) {
            throw new System.NotImplementedException();
        }

        public override void SetTrigger(string key, object value) {
            throw new System.NotImplementedException();
        }

        public override void SetTriggers(Dictionary<string, object> triggers) {
            throw new System.NotImplementedException();
        }

        public override void RemoveTrigger(string key) {
            throw new System.NotImplementedException();
        }

        public override void RemoveTriggers(IEnumerable<string> keys) {
            throw new System.NotImplementedException();
        }

        public override object GetTrigger(string key) {
            throw new System.NotImplementedException();
        }

        public override Dictionary<string, object> GetTriggers() {
            throw new System.NotImplementedException();
        }

        public override bool InAppMessagesArePaused { get; set; }

        public override Task<Dictionary<string, object>> SendTag(string tagName, string tagValue) {
            throw new System.NotImplementedException();
        }

        public override Task<Dictionary<string, object>> SendTags(IDictionary<string, string> tags) {
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

        public override Task<Dictionary<string, object>> SetEmail(string email, string authHash = null) {
            throw new System.NotImplementedException();
        }

        public override Task<Dictionary<string, object>> SetSMSNumber(string smsNumber, string authHash = null) {
            throw new System.NotImplementedException();
        }

        public override Task<Dictionary<string, object>> LogOut(
            LogOutOptions options = LogOutOptions.Email | LogOutOptions.SMS | LogOutOptions.ExternalUserId
        ) {
            throw new System.NotImplementedException();
        }

        public override void PromptLocation() {
            throw new System.NotImplementedException();
        }

        public override bool ShareLocation {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public override Task<OutcomeEvent> SendOutcome(string name) {
            throw new System.NotImplementedException();
        }

        public override Task<OutcomeEvent> SendUniqueOutcome(string name) {
            throw new System.NotImplementedException();
        }

        public override Task<OutcomeEvent> SendOutcomeWithValue(string name, float value) {
            throw new System.NotImplementedException();
        }
    }
}