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

        public override void AddTrigger(string key, object value) {
            throw new System.NotImplementedException();
        }

        public override void AddTriggers(Dictionary<string, object> triggers) {
            throw new System.NotImplementedException();
        }

        public override void RemoveTriggerForKey(string key) {
            throw new System.NotImplementedException();
        }

        public override void RemoveTriggersForKeys(IList<string> keys) {
            throw new System.NotImplementedException();
        }

        public override object GetTriggerValueForKey(string key) {
            throw new System.NotImplementedException();
        }

        public override bool InAppMessagesArePaused { get; set; }
        public override void SendTag(string tagName, string tagValue) {
            throw new System.NotImplementedException();
        }

        public override void SendTags(IDictionary<string, string> tags) {
            throw new System.NotImplementedException();
        }

        public override void GetTags() {
            throw new System.NotImplementedException();
        }

        public override Task<Dictionary<string, object>> RefreshTags() {
            throw new System.NotImplementedException();
        }

        public override void DeleteTag(string key) {
            throw new System.NotImplementedException();
        }

        public override void DeleteTags(IEnumerable<string> keys) {
            throw new System.NotImplementedException();
        }

        public override void SetExternalUserId(string externalId) {
            throw new System.NotImplementedException();
        }

        public override void SetExternalUserId(string externalId, string authHashToken) {
            throw new System.NotImplementedException();
        }

        public override void SetEmail(string email) {
            throw new System.NotImplementedException();
        }

        public override void SetEmail(string email, string emailAuthToken) {
            throw new System.NotImplementedException();
        }

        public override void LogOut(LogOutOptions options = LogOutOptions.ExternalUserId) {
            throw new System.NotImplementedException();
        }

        public override void PromptLocation() {
            throw new System.NotImplementedException();
        }

        public override bool ShareLocation { get; set; }
        public override void SendOutcome(string name) {
            throw new System.NotImplementedException();
        }

        public override void SendUniqueOutcome(string name) {
            throw new System.NotImplementedException();
        }

        public override void SendOutcomeWithValue(string name, float value) {
            throw new System.NotImplementedException();
        }
    }
}