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

#if UNITY_ANDROID
using UnityEngine;
using System.Collections.Generic;
using OneSignalPush.MiniJSON;
using System;

public class OneSignalAndroid : OneSignalPlatform {
    private static AndroidJavaObject mOneSignal = null;

    public OneSignalAndroid(string gameObjectName, string googleProjectNumber, string appId, OneSignal.OSInFocusDisplayOption displayOption, OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel, bool requiresUserConsent) {
        mOneSignal = new AndroidJavaObject("com.onesignal.OneSignalUnityProxy", gameObjectName, googleProjectNumber, appId, (int)logLevel, (int)visualLevel, requiresUserConsent);
        SetInFocusDisplaying(displayOption);
    }

    public void SetLogLevel(OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel) {
        mOneSignal.Call("setLogLevel", (int)logLevel, (int)visualLevel);
    }

    public void SetLocationShared(bool shared) {
        mOneSignal.Call("setLocationShared", shared);
    }

    public void SendTag(string tagName, string tagValue) {
        mOneSignal.Call("sendTag", tagName, tagValue);
    }

    public void SendTags(IDictionary<string, string> tags) {
        mOneSignal.Call("sendTags", Json.Serialize(tags));
    }

    public void GetTags(string delegateId) {
        mOneSignal.Call("getTags", delegateId);
    }

    public void DeleteTag(string key) {
        mOneSignal.Call("deleteTag", key);
    }

    public void DeleteTags(IList<string> keys) {
        mOneSignal.Call("deleteTags", Json.Serialize(keys));
    }

    public void IdsAvailable(string delegateId) {
        mOneSignal.Call("idsAvailable", delegateId);
    }

    // Doesn't apply to Android, doesn't have a native permission prompt
    public void RegisterForPushNotifications() {}
    public void PromptForPushNotificationsWithUserResponse() {}

    public void EnableVibrate(bool enable) {
        mOneSignal.Call("enableVibrate", enable);
    }

    public void EnableSound(bool enable) {
        mOneSignal.Call("enableSound", enable);
    }

    public void SetInFocusDisplaying(OneSignal.OSInFocusDisplayOption display) {
        mOneSignal.Call("setInFocusDisplaying", (int)display);
    }

    public void SetSubscription(bool enable) {
        mOneSignal.Call("setSubscription", enable);
    }

    public void PostNotification(string delegateIdSuccess, string delegateIdFailure, Dictionary<string, object> data){
        mOneSignal.Call("postNotification", delegateIdSuccess, delegateIdFailure, Json.Serialize(data));
    }

    public void SyncHashedEmail(string email) {
        mOneSignal.Call("syncHashedEmail", email);
    }

    public void PromptLocation() {
        mOneSignal.Call("promptLocation");
    }

    public void ClearOneSignalNotifications() {
        mOneSignal.Call("clearOneSignalNotifications");
    }

    public void AddPermissionObserver() {
        mOneSignal.Call("addPermissionObserver");
    }

    public void RemovePermissionObserver() {
        mOneSignal.Call("removePermissionObserver");
    }

    public void AddSubscriptionObserver() {
        mOneSignal.Call("addSubscriptionObserver");
    }
    public void RemoveSubscriptionObserver() {
        mOneSignal.Call("removeSubscriptionObserver");
    }
   
    public void AddEmailSubscriptionObserver() {
        mOneSignal.Call("addEmailSubscriptionObserver");
    }

    public void RemoveEmailSubscriptionObserver() {
        mOneSignal.Call("removeEmailSubscriptionObserver");
    }

    public void UserDidProvideConsent(bool consent) {
        mOneSignal.Call("provideUserConsent", consent);
    }

    public bool UserProvidedConsent() {
        return mOneSignal.Call<bool>("userProvidedPrivacyConsent");
    }

    public void SetRequiresUserPrivacyConsent(bool required) {
        mOneSignal.Call("setRequiresUserPrivacyConsent", required);
    }

    public void SetExternalUserId(string delegateId, string externalId) {
        mOneSignal.Call("setExternalUserId", delegateId, externalId);
    }

    public void RemoveExternalUserId(string delegateId) {
        mOneSignal.Call("removeExternalUserId", delegateId);
    }

    public OSPermissionSubscriptionState GetPermissionSubscriptionState() {
        return OneSignalPlatformHelper.ParsePermissionSubscriptionState(this, mOneSignal.Call<string>("getPermissionSubscriptionState"));
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

        var state = new OSPermissionState();
        state.hasPrompted = true;
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
        mOneSignal.Call("setEmail", delegateIdSuccess, delegateIdFailure, email, null);
    }

    public void SetEmail(string delegateIdSuccess, string delegateIdFailure, string email, string emailAuthCode) {
        mOneSignal.Call("setEmail", delegateIdSuccess, delegateIdFailure, email, emailAuthCode);
    }

    public void LogoutEmail(string delegateIdSuccess, string delegateIdFailure) {
        mOneSignal.Call("logoutEmail", delegateIdSuccess, delegateIdFailure);
    }

    public void AddTrigger(string key, object value) {
        mOneSignal.Call("addTrigger", key, value.ToString());
    }

    public void AddTriggers(IDictionary<string, object> triggers) {
        mOneSignal.Call("addTriggers", Json.Serialize(triggers));
    }

    public void RemoveTriggerForKey(string key) {
        mOneSignal.Call("removeTriggerForKey", key);
    }

    public void RemoveTriggersForKeys(IList<string> keys) {
        mOneSignal.Call("removeTriggersForKeys", Json.Serialize(keys));
    }

    public object GetTriggerValueForKey(string key) {
        var valueJsonStr = mOneSignal.Call<string>("getTriggerValueForKey", key);
        if (valueJsonStr == null)
            return null;
        var valueDict = Json.Deserialize(valueJsonStr) as Dictionary<string, object>;
        if (valueDict.ContainsKey("value"))
            return valueDict["value"];
        return null;
    }

    public void PauseInAppMessages(bool pause) {
        mOneSignal.Call("pauseInAppMessages", pause);
    }

    public void SendOutcome(string delegateId, string name) {
        mOneSignal.Call("sendOutcome", delegateId, name);
    }

    public void SendUniqueOutcome(string delegateId, string name) {
        mOneSignal.Call("sendUniqueOutcome", delegateId, name);
    }

    public void SendOutcomeWithValue(string delegateId, string name, float value) {
        mOneSignal.Call("sendOutcomeWithValue", delegateId, name, value);
    }

}
#endif