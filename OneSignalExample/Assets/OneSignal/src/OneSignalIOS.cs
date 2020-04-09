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

#if UNITY_IPHONE
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using OneSignalPush.MiniJSON;
using System;

public class OneSignalIOS : OneSignalPlatform {

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _init(string listenerName, string appId, bool autoPrompt, bool inAppLaunchURLs, int displayOption, int logLevel, int visualLogLevel, bool requiresUserPrivacyConsent);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _registerForPushNotifications();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _sendTag(string tagName, string tagValue);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _sendTags(string tags);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _getTags(string delegateId);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _deleteTag(string key);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _deleteTags(string keys);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _idsAvailable(string delegateId);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _setSubscription(bool enable);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _postNotification(string delegateIdSuccess, string delegateIdFailure, string json);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _syncHashedEmail(string email);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _promptLocation();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _setInFocusDisplayType(int type);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _promptForPushNotificationsWithUserResponse();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _addPermissionObserver();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _removePermissionObserver();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _addSubscriptionObserver();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _removeSubscriptionObserver();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _addEmailSubscriptionObserver();
   
    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _removeEmailSubscriptionObserver();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public string _getPermissionSubscriptionState();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _setUnauthenticatedEmail(string delegateIdSuccess, string delegateIdFailure, string email);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _setEmail(string delegateIdSuccess, string delegateIdFailure, string email, string emailAuthCode);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _logoutEmail(string delegateIdSuccess, string delegateIdFailure);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _setOneSignalLogLevel(int logLevel, int visualLogLevel);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _userDidProvideConsent(bool consent);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public bool _userProvidedConsent();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _setRequiresUserPrivacyConsent(bool required);   

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _setLocationShared(bool enable);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _setExternalUserId(string delegateId, string externalId);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _removeExternalUserId(string delegateId);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _addTriggers(string triggers);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _removeTriggerForKey(string key);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _removeTriggersForKeys(string keys);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public string _getTriggerValueForKey(string key);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _pauseInAppMessages(bool pause);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _sendOutcome(string delegateId, string name);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _sendUniqueOutcome(string delegateId, string name);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _sendOutcomeWithValue(string delegateId, string name, float value);

    public OneSignalIOS(string gameObjectName, string appId, bool autoPrompt, bool inAppLaunchURLs, OneSignal.OSInFocusDisplayOption displayOption, OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel, bool requiresUserPrivacyConsent) {
        _init(gameObjectName, appId, autoPrompt, inAppLaunchURLs, (int)displayOption, (int)logLevel, (int)visualLevel, requiresUserPrivacyConsent);
    }

    public void SetLocationShared(bool shared) {
        _setLocationShared(shared);
    }

    public void RegisterForPushNotifications() {
        _registerForPushNotifications();
    }

    public void SendTag(string tagName, string tagValue) {
        _sendTag(tagName, tagValue);
    }

    public void SendTags(IDictionary<string, string> tags) {
        _sendTags(Json.Serialize(tags));
    }

    public void GetTags(string delegateId) {
        _getTags(delegateId);
    }

    public void DeleteTag(string key) {
        _deleteTag(key);
    }

    public void DeleteTags(IList<string> keys) {
        _deleteTags(Json.Serialize(keys));
    }

    public void IdsAvailable(string delegateId) {
        _idsAvailable(delegateId);
    }

    public void SetSubscription(bool enable) {
        _setSubscription(enable);
    }

    public void PostNotification(string delegateIdSuccess, string delegateIdFailure, Dictionary<string, object> data) {
        _postNotification(delegateIdSuccess, delegateIdFailure, Json.Serialize(data));
    }

    public void SyncHashedEmail(string email) {
        _syncHashedEmail(email);
    }

    public void PromptLocation() {
        _promptLocation();
    }

    public void SetLogLevel(OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel) {
        _setOneSignalLogLevel((int) logLevel, (int) visualLevel);
    }

    public void SetInFocusDisplaying(OneSignal.OSInFocusDisplayOption display) {
        _setInFocusDisplayType((int) display);
    }

    public void PromptForPushNotificationsWithUserResponse() {
       _promptForPushNotificationsWithUserResponse();
    }

    public void AddPermissionObserver() {
       _addPermissionObserver();
    }

    public void RemovePermissionObserver() {
       _removePermissionObserver();
    }

    public void AddSubscriptionObserver() {
       _addSubscriptionObserver();
    }

    public void RemoveSubscriptionObserver() {
       _removeSubscriptionObserver();
    }

    public void AddEmailSubscriptionObserver() {
       _addEmailSubscriptionObserver();
    }

    public void RemoveEmailSubscriptionObserver() {
       _removeEmailSubscriptionObserver();
    }

    public void SetEmail(string delegateIdSuccess, string delegateIdFailure, string email) {
        _setUnauthenticatedEmail(delegateIdSuccess, delegateIdFailure, email);
    }

    public void SetEmail(string delegateIdSuccess, string delegateIdFailure, string email, string emailAuthCode) {
        _setEmail(delegateIdSuccess, delegateIdFailure, email, emailAuthCode);
    }

    public void LogoutEmail(string delegateIdSuccess, string delegateIdFailure) {
        _logoutEmail(delegateIdSuccess, delegateIdFailure);
    }

    public void UserDidProvideConsent(bool consent) {
        _userDidProvideConsent(consent);
    }

    public bool UserProvidedConsent() {
        return _userProvidedConsent();
    }

    public void SetRequiresUserPrivacyConsent(bool required) {
        _setRequiresUserPrivacyConsent(required);
    }

    public void SetExternalUserId(string delegateId, string externalId) {
        _setExternalUserId(delegateId, externalId);
    }

    public void RemoveExternalUserId(string delegateId) {
        _removeExternalUserId(delegateId);
    }

    public void AddTrigger(string key, object value) {
        IDictionary<string, object> trigger = new Dictionary<string, object>() { { key, value } };
        _addTriggers(Json.Serialize(trigger));
    }

    public void AddTriggers(IDictionary<string, object> triggers) {
        _addTriggers(Json.Serialize(triggers));
    }

    public void RemoveTriggerForKey(string key) {
        _removeTriggerForKey(key);
    }

    public void RemoveTriggersForKeys(IList<string> keys) {
        _removeTriggersForKeys(Json.Serialize(keys));
    }

    public object GetTriggerValueForKey(string key) {
        Dictionary<string, object> triggerValue = Json.Deserialize(_getTriggerValueForKey(key)) as Dictionary<string, object>;
        return triggerValue["value"];
    }

    public void PauseInAppMessages(bool pause) {
        _pauseInAppMessages(pause);
    }

    public void SendOutcome(string delegateId, string name) {
        _sendOutcome(delegateId, name);
    }

    public void SendUniqueOutcome(string delegateId, string name) {
        _sendUniqueOutcome(delegateId, name);
    }

    public void SendOutcomeWithValue(string delegateId, string name, float value) {
        _sendOutcomeWithValue(delegateId, name, value);
    }

    public OSPermissionSubscriptionState GetPermissionSubscriptionState() {
        return OneSignalPlatformHelper.ParsePermissionSubscriptionState(this, _getPermissionSubscriptionState());
    }

    public OSPermissionStateChanges ParseOSPermissionStateChanges(string jsonStat) {
        return OneSignalPlatformHelper.ParseOSPermissionStateChanges(this, jsonStat);
    }

    public OSEmailSubscriptionStateChanges ParseOSEmailSubscriptionStateChanges(string jsonState) {
        return OneSignalPlatformHelper.ParseOSEmailSubscriptionStateChanges (this, jsonState);
    }

    public OSSubscriptionStateChanges ParseOSSubscriptionStateChanges(string jsonStat) {
        return OneSignalPlatformHelper.ParseOSSubscriptionStateChanges(this, jsonStat);
    }

    public OSPermissionState ParseOSPermissionState(object stateDict) {
        var stateDictCasted = stateDict as Dictionary<string, object>;

        var state = new OSPermissionState();
        state.hasPrompted = Convert.ToBoolean(stateDictCasted["hasPrompted"]);
        state.status = (OSNotificationPermission) Convert.ToInt32(stateDictCasted["status"]);

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

        var state = new OSEmailSubscriptionState();

        if (stateDictCasted.ContainsKey ("emailUserId")) {
            state.emailUserId = stateDictCasted["emailUserId"] as string;
        } else {
            state.emailUserId = "";
        }

        if (stateDictCasted.ContainsKey ("emailAddress")) {
            state.emailAddress = stateDictCasted["emailAddress"] as string;
        } else {
            state.emailAddress = "";
        }

        state.subscribed = stateDictCasted.ContainsKey("emailUserId") && stateDictCasted["emailUserId"] != null;

        return state;
    }

}
#endif
