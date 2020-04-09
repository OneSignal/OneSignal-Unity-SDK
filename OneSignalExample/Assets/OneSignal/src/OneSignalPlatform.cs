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

using System.Collections.Generic;

// Shared interface so OneSignal.cs can use each mobile platform in a generic way
public interface OneSignalPlatform {
    void SetLogLevel(OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel);
    void RegisterForPushNotifications();
    void PromptForPushNotificationsWithUserResponse();

    void SendTag(string tagName, string tagValue);
    void SendTags(IDictionary<string, string> tags);
    void GetTags(string delegateId);
    void DeleteTag(string key);
    void DeleteTags(IList<string> keys);

    void IdsAvailable(string delegateId);

    void SetSubscription(bool enable);

    void PostNotification(string delegateIdSuccess, string delegateIdFailure, Dictionary<string, object> data);

    void SyncHashedEmail(string email);
    void PromptLocation();
    void SetLocationShared(bool shared);

    void SetEmail(string delegateIdSuccess, string delegateIdFailure, string email);
    void SetEmail(string delegateIdSuccess, string delegateIdFailure, string email, string emailAuthToken);
    void LogoutEmail(string delegateIdSuccess, string delegateIdFailure);

    void SetInFocusDisplaying(OneSignal.OSInFocusDisplayOption display);

    void UserDidProvideConsent(bool consent);
    bool UserProvidedConsent();
    void SetRequiresUserPrivacyConsent(bool required);

    void SetExternalUserId(string delegateId, string externalId);
    void RemoveExternalUserId(string delegateId);

    void AddPermissionObserver();
    void RemovePermissionObserver();
    void AddSubscriptionObserver();
    void RemoveSubscriptionObserver();
    void AddEmailSubscriptionObserver();
    void RemoveEmailSubscriptionObserver();

    OSPermissionSubscriptionState GetPermissionSubscriptionState();

    // In-App Messaging
    void AddTrigger(string key, object value);
    void AddTriggers(IDictionary<string, object> triggers);
    void RemoveTriggerForKey(string key);
    void RemoveTriggersForKeys(IList<string> keys);
    object GetTriggerValueForKey(string key);
    void PauseInAppMessages(bool pause);

    // Outcome Events
    void SendOutcome(string delegateId, string name);
    void SendUniqueOutcome(string delegateId, string name);
    void SendOutcomeWithValue(string delegateId, string name, float value);

    OSPermissionState ParseOSPermissionState(object stateDict);
    OSSubscriptionState ParseOSSubscriptionState(object stateDict);
    OSEmailSubscriptionState ParseOSEmailSubscriptionState(object stateDict);

    OSPermissionStateChanges ParseOSPermissionStateChanges(string stateChangesJSONString);
    OSSubscriptionStateChanges ParseOSSubscriptionStateChanges(string stateChangesJSONString);
    OSEmailSubscriptionStateChanges ParseOSEmailSubscriptionStateChanges(string stateChangesJSONString);
}
