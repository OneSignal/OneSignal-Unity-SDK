/*
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

using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;

class OneSignalIOS : IOneSignalPlatform
{
    [DllImport("__Internal")]
    public static extern void _init(string listenerName, string appId, bool autoPrompt, bool inAppLaunchUrLs,
        int displayOption, int logLevel, int visualLogLevel, bool requiresUserPrivacyConsent);

    [DllImport("__Internal")]
    public static extern void _registerForPushNotifications();

    [DllImport("__Internal")]
    public static extern void _sendTag(string tagName, string tagValue);

    [DllImport("__Internal")]
    public static extern void _sendTags(string tags);

    [DllImport("__Internal")]
    public static extern void _getTags(string delegateId);

    [DllImport("__Internal")]
    public static extern void _deleteTag(string key);

    [DllImport("__Internal")]
    public static extern void _deleteTags(string keys);

    [DllImport("__Internal")]
    public static extern void _idsAvailable(string delegateId);

    [DllImport("__Internal")]
    public static extern void _setSubscription(bool enable);

    [DllImport("__Internal")]
    public static extern void _postNotification(string delegateIdSuccess, string delegateIdFailure, string json);

    [DllImport("__Internal")]
    public static extern void _syncHashedEmail(string email);

    [DllImport("__Internal")]
    public static extern void _promptLocation();

    [DllImport("__Internal")]
    public static extern void _setInFocusDisplayType(int type);

    [DllImport("__Internal")]
    public static extern void _promptForPushNotificationsWithUserResponse();

    [DllImport("__Internal")]
    public static extern void _addPermissionObserver();

    [DllImport("__Internal")]
    public static extern void _removePermissionObserver();

    [DllImport("__Internal")]
    public static extern void _addSubscriptionObserver();

    [DllImport("__Internal")]
    public static extern void _removeSubscriptionObserver();

    [DllImport("__Internal")]
    public static extern void _addEmailSubscriptionObserver();

    [DllImport("__Internal")]
    public static extern void _removeEmailSubscriptionObserver();

    [DllImport("__Internal")]
    public static extern string _getPermissionSubscriptionState();

    [DllImport("__Internal")]
    public static extern void _setUnauthenticatedEmail(string delegateIdSuccess, string delegateIdFailure,
        string email);

    [DllImport("__Internal")]
    public static extern void _setEmail(string delegateIdSuccess, string delegateIdFailure, string email,
        string emailAuthCode);

    [DllImport("__Internal")]
    public static extern void _logoutEmail(string delegateIdSuccess, string delegateIdFailure);

    [DllImport("__Internal")]
    public static extern void _setOneSignalLogLevel(int logLevel, int visualLogLevel);

    [DllImport("__Internal")]
    public static extern void _userDidProvideConsent(bool consent);

    [DllImport("__Internal")]
    public static extern bool _userProvidedConsent();

    [DllImport("__Internal")]
    public static extern void _setRequiresUserPrivacyConsent(bool required);

    [DllImport("__Internal")]
    public static extern void _setLocationShared(bool enable);

    [DllImport("__Internal")]
    public static extern void _setExternalUserId(string delegateId, string externalId);

    [DllImport("__Internal")]
    public static extern void _setExternalUserIdWithAuthToken(string delegateId, string delegateIdFailure,
        string externalId, string authHashToken);

    [DllImport("__Internal")]
    public static extern void _removeExternalUserId(string delegateId);

    [DllImport("__Internal")]
    public static extern void _addTriggers(string triggers);

    [DllImport("__Internal")]
    public static extern void _removeTriggerForKey(string key);

    [DllImport("__Internal")]
    public static extern void _removeTriggersForKeys(string keys);

    [DllImport("__Internal")]
    public static extern string _getTriggerValueForKey(string key);

    [DllImport("__Internal")]
    public static extern void _pauseInAppMessages(bool pause);

    [DllImport("__Internal")]
    public static extern void _sendOutcome(string delegateId, string name);

    [DllImport("__Internal")]
    public static extern void _sendUniqueOutcome(string delegateId, string name);

    [DllImport("__Internal")]
    public static extern void _sendOutcomeWithValue(string delegateId, string name, float value);

    public void Init()
    {
        bool autoPrompt = true, inAppLaunchUrl = true;
        if (OneSignal.builder.iOSSettings != null)
        {
            if (OneSignal.builder.iOSSettings.ContainsKey(OneSignal.kOSSettingsAutoPrompt))
                autoPrompt = OneSignal.builder.iOSSettings[OneSignal.kOSSettingsAutoPrompt];
            if (OneSignal.builder.iOSSettings.ContainsKey(OneSignal.kOSSettingsInAppLaunchURL))
                inAppLaunchUrl = OneSignal.builder.iOSSettings[OneSignal.kOSSettingsInAppLaunchURL];
        }

        _init(OneSignal.GameObjectName, OneSignal.builder.appID,
            autoPrompt, inAppLaunchUrl, (int) OneSignal.inFocusDisplayType,
            (int) OneSignal.logLevel, (int) OneSignal.visualLogLevel, OneSignal.requiresUserConsent);
    }

    public void SetLocationShared(bool shared)
    {
        _setLocationShared(shared);
    }

    public void RegisterForPushNotifications()
    {
        _registerForPushNotifications();
    }

    public void SendTag(string tagName, string tagValue)
    {
        _sendTag(tagName, tagValue);
    }

    public void SendTags(IDictionary<string, string> tags)
    {
        _sendTags(Json.Serialize(tags));
    }

    public void GetTags(string delegateId)
    {
        _getTags(delegateId);
    }

    public void DeleteTag(string key)
    {
        _deleteTag(key);
    }

    public void DeleteTags(IList<string> keys)
    {
        _deleteTags(Json.Serialize(keys));
    }

    public void IdsAvailable(string delegateId)
    {
        _idsAvailable(delegateId);
    }

    public void SetSubscription(bool enable)
    {
        _setSubscription(enable);
    }

    public void PostNotification(string delegateIdSuccess, string delegateIdFailure,
        Dictionary<string, object> data)
    {
        _postNotification(delegateIdSuccess, delegateIdFailure, Json.Serialize(data));
    }

    public void SyncHashedEmail(string email)
    {
        _syncHashedEmail(email);
    }

    public void PromptLocation()
    {
        _promptLocation();
    }

    public void SetLogLevel(OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel)
    {
        _setOneSignalLogLevel((int) logLevel, (int) visualLevel);
    }

    public void SetInFocusDisplaying(OneSignal.OSInFocusDisplayOption display)
    {
        _setInFocusDisplayType((int) display);
    }

    public void PromptForPushNotificationsWithUserResponse()
    {
        _promptForPushNotificationsWithUserResponse();
    }

    public void AddPermissionObserver()
    {
        _addPermissionObserver();
    }

    public void RemovePermissionObserver()
    {
        _removePermissionObserver();
    }

    public void AddSubscriptionObserver()
    {
        _addSubscriptionObserver();
    }

    public void RemoveSubscriptionObserver()
    {
        _removeSubscriptionObserver();
    }

    public void AddEmailSubscriptionObserver()
    {
        _addEmailSubscriptionObserver();
    }

    public void RemoveEmailSubscriptionObserver()
    {
        _removeEmailSubscriptionObserver();
    }

    public void SetEmail(string delegateIdSuccess, string delegateIdFailure, string email)
    {
        _setUnauthenticatedEmail(delegateIdSuccess, delegateIdFailure, email);
    }

    public void SetEmail(string delegateIdSuccess, string delegateIdFailure, string email, string emailAuthCode)
    {
        _setEmail(delegateIdSuccess, delegateIdFailure, email, emailAuthCode);
    }

    public void LogoutEmail(string delegateIdSuccess, string delegateIdFailure)
    {
        _logoutEmail(delegateIdSuccess, delegateIdFailure);
    }

    public void UserDidProvideConsent(bool consent)
    {
        _userDidProvideConsent(consent);
    }

    public bool UserProvidedConsent()
    {
        return _userProvidedConsent();
    }

    public void SetRequiresUserPrivacyConsent(bool required)
    {
        _setRequiresUserPrivacyConsent(required);
    }

    public void SetExternalUserId(string delegateId, string externalId)
    {
        _setExternalUserId(delegateId, externalId);
    }

    public void SetExternalUserId(string delegateId, string delegateIdFailure, string externalId,
        string externalIdAuthHash)
    {
        _setExternalUserIdWithAuthToken(delegateId, delegateIdFailure, externalId, externalIdAuthHash);
    }

    public void RemoveExternalUserId(string delegateId)
    {
        _removeExternalUserId(delegateId);
    }

    public void AddTrigger(string key, object value)
    {
        IDictionary<string, object> trigger = new Dictionary<string, object>() {{key, value}};
        _addTriggers(Json.Serialize(trigger));
    }

    public void AddTriggers(IDictionary<string, object> triggers)
    {
        _addTriggers(Json.Serialize(triggers));
    }

    public void RemoveTriggerForKey(string key)
    {
        _removeTriggerForKey(key);
    }

    public void RemoveTriggersForKeys(IList<string> keys)
    {
        _removeTriggersForKeys(Json.Serialize(keys));
    }

    public object GetTriggerValueForKey(string key)
    {
        Dictionary<string, object> triggerValue =
            Json.Deserialize(_getTriggerValueForKey(key)) as Dictionary<string, object>;
        return triggerValue["value"];
    }

    public void PauseInAppMessages(bool pause)
    {
        _pauseInAppMessages(pause);
    }

    public void EnableVibrate(bool enable)
    {
    }

    public void EnableSound(bool enable)
    {
    }

    public void ClearOneSignalNotifications()
    {
    }

    public void SendOutcome(string delegateId, string name)
    {
        _sendOutcome(delegateId, name);
    }

    public void SendUniqueOutcome(string delegateId, string name)
    {
        _sendUniqueOutcome(delegateId, name);
    }

    public void SendOutcomeWithValue(string delegateId, string name, float value)
    {
        _sendOutcomeWithValue(delegateId, name, value);
    }

    public OSPermissionSubscriptionState GetPermissionSubscriptionState()
    {
        return OneSignalPlatformHelper.ParsePermissionSubscriptionState(this, _getPermissionSubscriptionState());
    }

    public OSPermissionStateChanges ParseOSPermissionStateChanges(string jsonStat)
    {
        return OneSignalPlatformHelper.ParseOSPermissionStateChanges(this, jsonStat);
    }

    public OSEmailSubscriptionStateChanges ParseOSEmailSubscriptionStateChanges(string jsonState)
    {
        return OneSignalPlatformHelper.ParseOSEmailSubscriptionStateChanges(this, jsonState);
    }

    public OSSubscriptionStateChanges ParseOSSubscriptionStateChanges(string jsonStat)
    {
        return OneSignalPlatformHelper.ParseOSSubscriptionStateChanges(this, jsonStat);
    }

    public OSPermissionState ParseOSPermissionState(object stateDict)
    {
        var stateDictCasted = stateDict as Dictionary<string, object>;

        var state = new OSPermissionState();
        state.hasPrompted = Convert.ToBoolean(stateDictCasted["hasPrompted"]);
        state.status = (OSNotificationPermission) Convert.ToInt32(stateDictCasted["status"]);

        return state;
    }

    public OSSubscriptionState ParseOSSubscriptionState(object stateDict)
    {
        var stateDictCasted = stateDict as Dictionary<string, object>;

        var state = new OSSubscriptionState();
        state.subscribed = Convert.ToBoolean(stateDictCasted["subscribed"]);
        state.userSubscriptionSetting = Convert.ToBoolean(stateDictCasted["userSubscriptionSetting"]);
        state.userId = stateDictCasted["userId"] as string;
        state.pushToken = stateDictCasted["pushToken"] as string;

        return state;
    }

    public OSEmailSubscriptionState ParseOSEmailSubscriptionState(object stateDict)
    {
        var stateDictCasted = stateDict as Dictionary<string, object>;

        var state = new OSEmailSubscriptionState();

        if (stateDictCasted.ContainsKey("emailUserId"))
        {
            state.emailUserId = stateDictCasted["emailUserId"] as string;
        }
        else
        {
            state.emailUserId = "";
        }

        if (stateDictCasted.ContainsKey("emailAddress"))
        {
            state.emailAddress = stateDictCasted["emailAddress"] as string;
        }
        else
        {
            state.emailAddress = "";
        }

        state.subscribed = stateDictCasted.ContainsKey("emailUserId") && stateDictCasted["emailUserId"] != null;

        return state;
    }
}