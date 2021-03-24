using System;
using System.Collections.Generic;

namespace Com.OneSignal
{
    interface IOneSignalPlatform
    {
        void Init();

        /// <summary>
        /// Enable logging to help debug OneSignal implementation.
        /// </summary>
        /// <param name="logLevel">Sets the logging level to print to the Android LogCat log or Xcode log.</param>
        /// <param name="visualLevel">Sets the logging level to show as alert dialogs.</param>
        void SetLogLevel(OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel);
        void RegisterForPushNotifications();

        /// <summary>
        /// Prompt the user for notification permissions.
        /// Callback fires as soon as the user accepts or declines notifications.
        /// Must set `kOSSettingsKeyAutoPrompt` to `false` when calling <see href="https://documentation.onesignal.com/docs/unity-sdk#initwithlaunchoptions">initWithLaunchOptions</see>.
        ///
        /// Recommended: Set to false and follow <see href="https://documentation.onesignal.com/docs/ios-push-opt-in-prompt">iOS Push Opt-In Prompt</see>.
        /// </summary>
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


        [Obsolete("Use the new setNotificationWillShowInForegroundHandler method.")]
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

        OSPermissionStateChanges ParseOSPermissionStateChanges(string stateChangesJsonString);
        OSSubscriptionStateChanges ParseOSSubscriptionStateChanges(string stateChangesJsonString);
        OSEmailSubscriptionStateChanges ParseOSEmailSubscriptionStateChanges(string stateChangesJsonString);

        // Currently only supported by Android
        void EnableVibrate(bool enable);
        void EnableSound(bool enable);
        void ClearOneSignalNotifications();
    }
}
