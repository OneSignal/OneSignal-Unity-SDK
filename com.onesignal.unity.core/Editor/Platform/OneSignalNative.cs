/*
 * Modified MIT License
 *
 * Copyright 2022 OneSignal
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

#pragma warning disable 0067 // the event 'x' is never used
namespace OneSignalSDK {
    /// <summary>
    /// Implementationless variation of the OneSignal SDK so that it "runs" in the Editor
    /// </summary>
    internal sealed class OneSignalNative : OneSignal {
        public override event NotificationWillShowDelegate NotificationWillShow;
        public override event NotificationActionDelegate NotificationOpened;
        public override event InAppMessageLifecycleDelegate InAppMessageWillDisplay;
        public override event InAppMessageLifecycleDelegate InAppMessageDidDisplay;
        public override event InAppMessageLifecycleDelegate InAppMessageWillDismiss;
        public override event InAppMessageLifecycleDelegate InAppMessageDidDismiss;
        public override event InAppMessageActionDelegate InAppMessageTriggeredAction;
        public override event StateChangeDelegate<NotificationPermission> NotificationPermissionChanged;
        public override event StateChangeDelegate<PushSubscriptionState> PushSubscriptionStateChanged;
        public override event StateChangeDelegate<EmailSubscriptionState> EmailSubscriptionStateChanged;
        public override event StateChangeDelegate<SMSSubscriptionState> SMSSubscriptionStateChanged;

        public override LogLevel LogLevel { get; set; }
        public override LogLevel AlertLevel { get; set; }
        
        public override bool PrivacyConsent { get; set; }
        public override bool RequiresPrivacyConsent { get; set; }
        public override void SetLaunchURLsInApp(bool launchInApp) {
            
        }
        
        public override NotificationPermission NotificationPermission { get; }
        public override PushSubscriptionState PushSubscriptionState { get; }
        public override EmailSubscriptionState EmailSubscriptionState { get; }
        public override SMSSubscriptionState SMSSubscriptionState { get; }

        public override void Initialize(string appId) {
            if (string.IsNullOrEmpty(appId)) {
                SDKDebug.Error("appId is null or empty");
                return;
            }
            
            SDKDebug.Warn("Native SDK is placeholder. Please run on supported platform (iOS or Android).");
        }

        public override Task<NotificationPermission> PromptForPushNotificationsWithUserResponse() {
            return Task.FromResult(NotificationPermission.NotDetermined);
        }

        public override void ClearOneSignalNotifications() {
            
        }

        public override bool PushEnabled { get; set; }

        public override Task<Dictionary<string, object>> PostNotification(Dictionary<string, object> options) {
            return Task.FromResult(new Dictionary<string, object>());
        }

        public override void SetTrigger(string key, string value) {
            
        }

        public override void SetTriggers(Dictionary<string, string> triggers) {
            
        }

        public override void RemoveTrigger(string key) {
            
        }

        public override void RemoveTriggers(params string[] keys) {
            
        }

        public override string GetTrigger(string key) {
            return null;
        }

        public override Dictionary<string, string> GetTriggers() {
            return new Dictionary<string, string>();
        }

        public override bool InAppMessagesArePaused { get; set; }

        public override Task<bool> SendTag(string key, object value) {
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

        public override Task<bool> DeleteTags(params string[] keys) {
            return Task.FromResult(false);
        }

        public override Task<bool> SetExternalUserId(string externalId, string authHash = null) {
            return Task.FromResult(false);
        }

        public override Task<bool> SetEmail(string email, string authHash = null) {
            return Task.FromResult(false);
        }

        public override Task<bool> SetSMSNumber(string smsNumber, string authHash = null) {
            return Task.FromResult(false);
        }

        public override Task<bool> RemoveExternalUserId() {
            return Task.FromResult(false);
        }

        public override Task<bool> LogOutEmail() {
            return Task.FromResult(false);
        }

        public override Task<bool> LogOutSMS() {
            return Task.FromResult(false);
        }

        public override Task<bool> SetLanguage(string languageCode) {
            return Task.FromResult(false);
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