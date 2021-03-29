/**
 * Modified MIT License
 *
 * Copyright 2017 OneSignal
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

using UnityEngine;
using System.Collections.Generic;
using System;
using Com.OneSignal.MiniJSON;

namespace Com.OneSignal.Android
{
    class OneSignalAndroid : IOneSignalPlatform
    {
        static AndroidJavaObject s_OneSignal;

        public void Init()
        {
            s_OneSignal = new AndroidJavaObject("com.onesignal.OneSignalUnityProxy", OneSignal.GameObjectName,
                OneSignal.builder.googleProjectNumber, OneSignal.builder.appID,
                (int)OneSignal.logLevel, (int)OneSignal.visualLogLevel, OneSignal.requiresUserConsent);

            SetInFocusDisplaying(OneSignal.inFocusDisplayType);
        }

        public void SetLogLevel(OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel) {
            s_OneSignal.Call("setLogLevel", (int)logLevel, (int)visualLevel);
        }

        public void SetLocationShared(bool shared) {
            s_OneSignal.Call("setLocationShared", shared);
        }

        public void SendTag(string tagName, string tagValue) {
            s_OneSignal.Call("sendTag", tagName, tagValue);
        }

        public void SendTags(IDictionary<string, string> tags) {
            s_OneSignal.Call("sendTags", Json.Serialize(tags));
        }

        public void GetTags(string delegateId) {
            s_OneSignal.Call("getTags", delegateId);
        }

        public void DeleteTag(string key) {
            s_OneSignal.Call("deleteTag", key);
        }

        public void DeleteTags(IList<string> keys) {
            s_OneSignal.Call("deleteTags", Json.Serialize(keys));
        }

        public void IdsAvailable(string delegateId) {
            s_OneSignal.Call("idsAvailable", delegateId);
        }

        // Doesn't apply to Android, doesn't have a native permission prompt
        public void RegisterForPushNotifications() {}
        public void PromptForPushNotificationsWithUserResponse() {}

        public void EnableVibrate(bool enable) {
            s_OneSignal.Call("enableVibrate", enable);
        }

        public void EnableSound(bool enable) {
            s_OneSignal.Call("enableSound", enable);
        }

        public void SetInFocusDisplaying(OneSignal.OSInFocusDisplayOption display) {
            s_OneSignal.Call("setInFocusDisplaying", (int)display);
        }

        public void SetSubscription(bool enable) {
            s_OneSignal.Call("setSubscription", enable);
        }

        public void PostNotification(string delegateIdSuccess, string delegateIdFailure, Dictionary<string, object> data){
            s_OneSignal.Call("postNotification", delegateIdSuccess, delegateIdFailure, Json.Serialize(data));
        }

        public void SyncHashedEmail(string email) {
            s_OneSignal.Call("syncHashedEmail", email);
        }

        public void PromptLocation() {
            s_OneSignal.Call("promptLocation");
        }

        public void ClearOneSignalNotifications() {
            s_OneSignal.Call("clearOneSignalNotifications");
        }

        public void AddPermissionObserver() {
            s_OneSignal.Call("addPermissionObserver");
        }

        public void RemovePermissionObserver() {
            s_OneSignal.Call("removePermissionObserver");
        }

        public void AddSubscriptionObserver() {
            s_OneSignal.Call("addSubscriptionObserver");
        }
        public void RemoveSubscriptionObserver() {
            s_OneSignal.Call("removeSubscriptionObserver");
        }

        public void AddEmailSubscriptionObserver() {
            s_OneSignal.Call("addEmailSubscriptionObserver");
        }

        public void RemoveEmailSubscriptionObserver() {
            s_OneSignal.Call("removeEmailSubscriptionObserver");
        }

        public void UserDidProvideConsent(bool consent) {
            s_OneSignal.Call("provideUserConsent", consent);
        }

        public bool UserProvidedConsent() {
            return s_OneSignal.Call<bool>("userProvidedPrivacyConsent");
        }

        public void SetRequiresUserPrivacyConsent(bool required) {
            s_OneSignal.Call("setRequiresUserPrivacyConsent", required);
        }

        public void SetExternalUserId(string delegateId, string externalId) {
            s_OneSignal.Call("setExternalUserId", delegateId, externalId);
        }

        public void RemoveExternalUserId(string delegateId) {
            s_OneSignal.Call("removeExternalUserId", delegateId);
        }

        public OSPermissionSubscriptionState GetPermissionSubscriptionState() {
            return OneSignalPlatformHelper.ParsePermissionSubscriptionState(this, s_OneSignal.Call<string>("getPermissionSubscriptionState"));
        }

        public OSPermissionStateChanges ParseOSPermissionStateChanges(string jsonStat) {
            return OneSignalPlatformHelper.ParseOSPermissionStateChanges(this, jsonStat);
        }

        public OSSubscriptionStateChanges ParseOSSubscriptionStateChanges(string jsonStat) {
            return OneSignalPlatformHelper.ParseOSSubscriptionStateChanges(this, jsonStat);
        }

        public OSEmailSubscriptionStateChanges ParseOSEmailSubscriptionStateChanges(string jsonState) {
            return OneSignalPlatformHelper.ParseOSEmailSubscriptionStateChanges (this, jsonState);
        }

        public OSPermissionState ParseOSPermissionState(object stateDict) {
            var stateDictCasted = stateDict as Dictionary<string, object>;

            var state = new OSPermissionState { hasPrompted = true };
            var toIsEnabled = Convert.ToBoolean(stateDictCasted["enabled"]);
            state.status = toIsEnabled ? OSNotificationPermission.Authorized : OSNotificationPermission.Denied;

            return state;
        }

        public OSSubscriptionState ParseOSSubscriptionState(object stateDict) {
            var stateDictCasted = stateDict as Dictionary<string, object>;

            var state = new OSSubscriptionState();
            state.subscribed = Convert.ToBoolean(stateDictCasted["subscribed"]);
            state.userSubscriptionSetting = Convert.ToBoolean(stateDictCasted["userSubscriptionSetting"]);
            state.userId = stateDictCasted["userId"] as string;
            state.pushToken = stateDictCasted["pushToken"] as string;

            return state;
        }

        public OSEmailSubscriptionState ParseOSEmailSubscriptionState(object stateDict) {
            var stateDictCasted = stateDict as Dictionary<string, object>;

            var state = new OSEmailSubscriptionState ();
            state.subscribed = Convert.ToBoolean (stateDictCasted ["subscribed"]);
            state.emailUserId = stateDictCasted ["emailUserId"] as string;
            state.emailAddress = stateDictCasted ["emailAddress"] as string;

            return state;
        }

        public void SetEmail(string delegateIdSuccess, string delegateIdFailure, string email) {
            s_OneSignal.Call("setEmail", delegateIdSuccess, delegateIdFailure, email, null);
        }

        public void SetEmail(string delegateIdSuccess, string delegateIdFailure, string email, string emailAuthCode) {
            s_OneSignal.Call("setEmail", delegateIdSuccess, delegateIdFailure, email, emailAuthCode);
        }

        public void LogoutEmail(string delegateIdSuccess, string delegateIdFailure) {
            s_OneSignal.Call("logoutEmail", delegateIdSuccess, delegateIdFailure);
        }

        public void AddTrigger(string key, object value) {
            s_OneSignal.Call("addTrigger", key, value.ToString());
        }

        public void AddTriggers(IDictionary<string, object> triggers) {
            s_OneSignal.Call("addTriggers", Json.Serialize(triggers));
        }

        public void RemoveTriggerForKey(string key) {
            s_OneSignal.Call("removeTriggerForKey", key);
        }

        public void RemoveTriggersForKeys(IList<string> keys) {
            s_OneSignal.Call("removeTriggersForKeys", Json.Serialize(keys));
        }

        public object GetTriggerValueForKey(string key) {
            var valueJsonStr = s_OneSignal.Call<string>("getTriggerValueForKey", key);
            if (valueJsonStr == null)
                return null;
            var valueDict = Json.Deserialize(valueJsonStr) as Dictionary<string, object>;
            if (valueDict.ContainsKey("value"))
                return valueDict["value"];
            return null;
        }

        public void PauseInAppMessages(bool pause) {
            s_OneSignal.Call("pauseInAppMessages", pause);
        }

        public void SendOutcome(string delegateId, string name) {
            s_OneSignal.Call("sendOutcome", delegateId, name);
        }

        public void SendUniqueOutcome(string delegateId, string name) {
            s_OneSignal.Call("sendUniqueOutcome", delegateId, name);
        }

        public void SendOutcomeWithValue(string delegateId, string name, float value) {
            s_OneSignal.Call("sendOutcomeWithValue", delegateId, name, value);
        }
    }
}