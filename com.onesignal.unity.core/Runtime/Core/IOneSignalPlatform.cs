using System.Collections.Generic;

interface IOneSignalPlatform
{
    void Init();
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

    void SetExternalUserId(string delegateId, string delegateIdFailure, string externalId,
        string externalIdAuthHash);

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

    // Android only atm
    void EnableVibrate(bool enable);
    void EnableSound(bool enable);
    void ClearOneSignalNotifications();


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