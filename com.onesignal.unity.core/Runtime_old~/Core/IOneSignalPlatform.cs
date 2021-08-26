using System.Collections.Generic;

interface IOneSignalPlatform
{
    // void SetSubscription(bool enable); Replaced by disablePush()

    void PostNotification(string delegateIdSuccess, string delegateIdFailure, Dictionary<string, object> data);

    void PromptLocation();
    void SetLocationShared(bool shared);

    void SetEmail(string delegateIdSuccess, string delegateIdFailure, string email);
    void SetEmail(string delegateIdSuccess, string delegateIdFailure, string email, string emailAuthToken);
    void LogoutEmail(string delegateIdSuccess, string delegateIdFailure);

    // void SetInFocusDisplaying(OneSignal.OSInFocusDisplayOption display); Replaced by setNotificationWillShowInForegroundHandler

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

    // OSPermissionSubscriptionState GetPermissionSubscriptionState(); REMOVED

    // In-App Messaging
    void AddTrigger(string key, object value);
    void AddTriggers(IDictionary<string, object> triggers);
    void RemoveTriggerForKey(string key);
    void RemoveTriggersForKeys(IList<string> keys);
    object GetTriggerValueForKey(string key);
    void PauseInAppMessages(bool pause);

    // Android only atm
    // void EnableVibrate(bool enable); REMOVED
    // void EnableSound(bool enable); REMOVED
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