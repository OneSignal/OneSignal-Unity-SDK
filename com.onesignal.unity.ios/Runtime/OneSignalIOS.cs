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
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace OneSignalSDK {
    public sealed partial class OneSignalIOS : OneSignal {
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
        
        public override NotificationPermission NotificationPermission {
            get {
                var deviceState = JsonUtility.FromJson<DeviceState>(_getDeviceState());
                return (NotificationPermission)deviceState.notificationPermissionStatus;
            }
        }
        
        public override PushSubscriptionState PushSubscriptionState
            => JsonUtility.FromJson<DeviceState>(_getDeviceState());
        
        public override EmailSubscriptionState EmailSubscriptionState
            => JsonUtility.FromJson<DeviceState>(_getDeviceState());
        
        public override SMSSubscriptionState SMSSubscriptionState 
            => JsonUtility.FromJson<DeviceState>(_getDeviceState());

        public override LogLevel LogLevel {
            get => _logLevel;
            set {
                _logLevel = value;
                _setLogLevel((int) _logLevel, (int) _alertLevel);
            }
        }

        public override LogLevel AlertLevel {
            get => _alertLevel;
            set {
                _alertLevel = value;
                _setLogLevel((int) _logLevel, (int) _alertLevel);
            }
        }
        
        public override bool PrivacyConsent {
            get => _getPrivacyConsent();
            set => _setPrivacyConsent(value);
        }
        
        public override bool RequiresPrivacyConsent {
            get => _getRequiresPrivacyConsent();
            set => _setRequiresPrivacyConsent(value);
        }

        public override void SetLaunchURLsInApp(bool launchInApp)
            => _setLaunchURLsInApp(launchInApp);

        public override void Initialize(string appId) {
            _initialize(appId);
            _completedInit(appId);
        }

        public override async Task<NotificationPermission> PromptForPushNotificationsWithUserResponse() {
            var (proxy, hashCode) = _setupProxy<bool>();
            _promptForPushNotificationsWithUserResponse(hashCode, BooleanCallbackProxy);
            return await proxy ? NotificationPermission.Authorized : NotificationPermission.Denied;
        }

        public override void ClearOneSignalNotifications()
            => SDKDebug.Info("ClearOneSignalNotifications invoked on iOS, does nothing");

        public override bool PushEnabled {
            get {
                var deviceState = JsonUtility.FromJson<DeviceState>(_getDeviceState());
                return !deviceState.isPushDisabled;
            }
            set => _disablePush(!value);
        }

        public override async Task<Dictionary<string, object>> PostNotification(Dictionary<string, object> options) {
            var (proxy, hashCode) = _setupProxy<string>();
            _postNotification(Json.Serialize(options), hashCode, StringCallbackProxy);
            return Json.Deserialize(await proxy) as Dictionary<string, object>;
        }

        public override void SetTrigger(string key, string value)
            => _setTrigger(key, value);

        public override void SetTriggers(Dictionary<string, string> triggers)
            => _setTriggers(Json.Serialize(triggers));

        public override void RemoveTrigger(string key)
            => _removeTrigger(key);

        public override void RemoveTriggers(params string[] keys)
            => _removeTriggers(Json.Serialize(keys));

        public override string GetTrigger(string key)
            => _getTrigger(key);

        public override Dictionary<string, string> GetTriggers() {
            var triggersDict = Json.Deserialize(_getTriggers()) as Dictionary<string, object>;
            return triggersDict?.ToDictionary(item => item.Key, 
                item => item.Value as string
            );
        }

        public override bool InAppMessagesArePaused {
            get => _getInAppMessagesArePaused();
            set => _setInAppMessagesArePaused(value);
        }
        
        public override async Task<bool> SendTag(string key, object value) {
            var (proxy, hashCode) = _setupProxy<bool>();
            _sendTag(key, value.ToString(), hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public override async Task<bool> SendTags(Dictionary<string, object> tags) {
            var (proxy, hashCode) = _setupProxy<bool>();
            _sendTags(Json.Serialize(tags), hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public override async Task<Dictionary<string, object>> GetTags() {
            var (proxy, hashCode) = _setupProxy<string>();
            _getTags(hashCode, StringCallbackProxy);
            return Json.Deserialize(await proxy) as Dictionary<string, object>;
        }

        public override async Task<bool> DeleteTag(string key) {
            var (proxy, hashCode) = _setupProxy<bool>();
            _deleteTag(key, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public override async Task<bool> DeleteTags(params string[] keys) {
            var (proxy, hashCode) = _setupProxy<bool>();
            _deleteTags(Json.Serialize(keys), hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public override async Task<bool> SetExternalUserId(string externalId, string authHash = null) {
            var (proxy, hashCode) = _setupProxy<bool>();
            _setExternalUserId(externalId, authHash, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public override async Task<bool> SetEmail(string email, string authHash = null) {
            var (proxy, hashCode) = _setupProxy<bool>();
            _setEmail(email, authHash, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public override async Task<bool> SetSMSNumber(string smsNumber, string authHash = null) {
            var (proxy, hashCode) = _setupProxy<bool>();
            _setSMSNumber(smsNumber, authHash, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public override async Task<bool> RemoveExternalUserId() {
            var (proxy, hashCode) = _setupProxy<bool>();
            _removeExternalUserId(hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public override async Task<bool> LogOutEmail() {
            var (proxy, hashCode) = _setupProxy<bool>();
            _logoutEmail(hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public override async Task<bool> LogOutSMS() {
            var (proxy, hashCode) = _setupProxy<bool>();
            _logoutSMSNumber(hashCode, BooleanCallbackProxy);
            return await proxy;
        }
        
        public override async Task<bool> SetLanguage(string languageCode) {
            var (proxy, hashCode) = _setupProxy<bool>();
            _setLanguage(languageCode, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public override void PromptLocation()
            => _promptLocation();

        public override bool ShareLocation {
            get => _getShareLocation();
            set => _setShareLocation(value);
        }
        
        public override async Task<bool> SendOutcome(string name) {
            var (proxy, hashCode) = _setupProxy<bool>();
            _sendOutcome(name, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public override async Task<bool> SendUniqueOutcome(string name) {
            var (proxy, hashCode) = _setupProxy<bool>();
            _sendUniqueOutcome(name, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public override async Task<bool> SendOutcomeWithValue(string name, float value) {
            var (proxy, hashCode) = _setupProxy<bool>();
            _sendOutcomeWithValue(name, value, hashCode, BooleanCallbackProxy);
            return await proxy;
        }
    }
}