using System.Collections.Generic;

class OneSignalEditor : IOneSignalPlatform
{
    public void Init()
    {
        OneSignal.LogDebug("Please run OneSignal on a device to see push notifications.");
    }

    public void SetLogLevel(OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel)
    {
    }

    public void RegisterForPushNotifications()
    {
    }

    public void PromptForPushNotificationsWithUserResponse()
    {
    }

    public void SendTag(string tagName, string tagValue)
    {
    }

    public void SendTags(IDictionary<string, string> tags)
    {
    }

    public void GetTags(string delegateId)
    {
    }

    public void DeleteTag(string key)
    {
    }

    public void DeleteTags(IList<string> keys)
    {
    }

    public void IdsAvailable(string delegateId)
    {
    }

    public void SetSubscription(bool enable)
    {
    }

    public void PostNotification(string delegateIdSuccess, string delegateIdFailure, Dictionary<string, object> data)
    {
    }

    public void SyncHashedEmail(string email)
    {
    }

    public void PromptLocation()
    {
    }

    public void SetLocationShared(bool shared)
    {
    }

    public void SetEmail(string delegateIdSuccess, string delegateIdFailure, string email)
    {
    }

    public void SetEmail(string delegateIdSuccess, string delegateIdFailure, string email, string emailAuthToken)
    {
    }

    public void LogoutEmail(string delegateIdSuccess, string delegateIdFailure)
    {
    }

    public void SetInFocusDisplaying(OneSignal.OSInFocusDisplayOption display)
    {
    }

    public void UserDidProvideConsent(bool consent)
    {
    }

    public bool UserProvidedConsent()
    {
        return true;
    }

    public void SetRequiresUserPrivacyConsent(bool required)
    {
    }

    public void SetExternalUserId(string delegateId, string externalId)
    {
    }

    public void SetExternalUserId(string delegateId, string delegateIdFailure, string externalId,
        string externalIdAuthHash)
    {
    }

    public void RemoveExternalUserId(string delegateId)
    {
    }

    public void AddPermissionObserver()
    {
    }

    public void RemovePermissionObserver()
    {
    }

    public void AddSubscriptionObserver()
    {
    }

    public void RemoveSubscriptionObserver()
    {
    }

    public void AddEmailSubscriptionObserver()
    {
    }

    public void RemoveEmailSubscriptionObserver()
    {
    }

    public OSPermissionSubscriptionState GetPermissionSubscriptionState()
    {
        var state = new OSPermissionSubscriptionState();
        state.permissionStatus = new OSPermissionState();
        state.subscriptionStatus = new OSSubscriptionState();
        return state;
    }

    public void AddTrigger(string key, object value)
    {
    }

    public void AddTriggers(IDictionary<string, object> triggers)
    {
    }

    public void RemoveTriggerForKey(string key)
    {
    }

    public void RemoveTriggersForKeys(IList<string> keys)
    {
    }

    public object GetTriggerValueForKey(string key)
    {
        return null;
    }

    public void PauseInAppMessages(bool pause)
    {
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
    }

    public void SendUniqueOutcome(string delegateId, string name)
    {
    }

    public void SendOutcomeWithValue(string delegateId, string name, float value)
    {
    }

    public OSPermissionState ParseOSPermissionState(object stateDict)
    {
        return null;
    }

    public OSSubscriptionState ParseOSSubscriptionState(object stateDict)
    {
        return null;
    }

    public OSEmailSubscriptionState ParseOSEmailSubscriptionState(object stateDict)
    {
        return null;
    }

    public OSPermissionStateChanges ParseOSPermissionStateChanges(string stateChangesJSONString)
    {
        return null;
    }

    public OSSubscriptionStateChanges ParseOSSubscriptionStateChanges(string stateChangesJSONString)
    {
        return null;
    }

    public OSEmailSubscriptionStateChanges ParseOSEmailSubscriptionStateChanges(string stateChangesJSONString)
    {
        return null;
    }
}