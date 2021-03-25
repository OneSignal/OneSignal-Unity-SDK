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
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IPHONE)
    #define ONESIGNAL_PLATFORM
#endif


using UnityEngine;
using System.Collections.Generic;
using System;
using Com.OneSignal.MiniJSON;

namespace Com.OneSignal
{
    public class OneSignal : MonoBehaviour
    {
        /// <summary>
        /// Dictionary of GUIDs and delegates to help control several delegates for the same public method call when they return from native SDKs.
        /// </summary>
        private static Dictionary<string, Delegate> delegates;
        
        /// <summary>
        /// Delegate is called when a push notification is received when the user is in your game.
        /// </summary>
        /// <param name="notification"> The Notification dictionary filled from a serialized native OSNotification object </param>
        public delegate void NotificationReceived(OSNotification notification);
        
        /// <summary>
        /// Delegate is called when external user id for push or email channel is set or removed.
        /// </summary>
        /// <param name="results">The dictionary payload containing the success status for the channels updating exteranl user id.</param>
        public delegate void OnExternalUserIdUpdateCompletion(Dictionary<string, object> results);

        public delegate void OnSetEmailSuccess();

        public delegate void OnSetEmailFailure(Dictionary<string, object> error);

        public delegate void OnLogoutEmailSuccess();

        public delegate void OnLogoutEmailFailure(Dictionary<string, object> error);

        public delegate void OnSendOutcomeSuccess(OSOutcomeEvent outcomeEvent);
        
        /// <summary>
        /// Delegate is called when a push notification is opened.
        /// </summary>
        /// <param name="result">The Notification open result describing: 1. The notification opened 2. The action taken by the user. </param>
        public delegate void NotificationOpened(OSNotificationOpenedResult result);

        /// <summary>
        /// Sets a In App Message opened handler. The instance will be called when an In App Message action is tapped on.
        /// </summary>
        /// <param name="action">Instance to a class implementing this interference.</param>
        public delegate void InAppMessageClicked(OSInAppMessageAction action);

        public delegate void IdsAvailableCallback(string playerID, string pushToken);

        /// <summary>
        /// Delegate you can define to get the all the tags set on a player from onesignal.com.
        /// </summary>
        /// <param name="tags">Dictionary of key value pairs retrieved from the OneSignal server.</param>
        public delegate void TagsReceived(Dictionary<string, object> tags);

        public delegate void PromptForPushNotificationsUserResponse(bool accepted);

        /// <summary>
        /// Delegate fires when the notification was created on OneSignal's server.
        /// </summary>
        /// <param name="response">Json response from OneSignal's server.</param>
        public delegate void OnPostNotificationSuccess(Dictionary<string, object> response);

        /// <summary>
        /// Delegate fires when the notification failed to create.
        /// </summary>
        /// <param name="response">Json response from OneSignal's server.</param>
        public delegate void OnPostNotificationFailure(Dictionary<string, object> response);

        private static PromptForPushNotificationsUserResponse notificationUserResponseDelegate;

        public delegate void PermissionObservable(OSPermissionStateChanges stateChanges);

        private static PermissionObservable internalPermissionObserver;

        public static event PermissionObservable permissionObserver
        {
            add
            {
                if (oneSignalPlatform != null)
                {
                    internalPermissionObserver += value;
                    addPermissionObserver();
                }
            }
            remove
            {
                if (oneSignalPlatform != null)
                {
                    internalPermissionObserver -= value;
                    if (addedPermissionObserver && internalPermissionObserver.GetInvocationList().Length == 0)
                    {
                        addedPermissionObserver = false;
                        oneSignalPlatform.RemovePermissionObserver();
                    }
                }
            }
        }

        private static bool addedPermissionObserver;

        private static void addPermissionObserver()
        {
            if (!addedPermissionObserver && internalPermissionObserver != null && internalPermissionObserver.GetInvocationList().Length > 0)
            {
                addedPermissionObserver = true;
                oneSignalPlatform.AddPermissionObserver();
            }
        }

        public delegate void SubscriptionObservable(OSSubscriptionStateChanges stateChanges);

        private static SubscriptionObservable internalSubscriptionObserver;

        public static event SubscriptionObservable subscriptionObserver
        {
            add
            {
                if (oneSignalPlatform != null)
                {
                    internalSubscriptionObserver += value;
                    addSubscriptionObserver();
                }
            }
            remove
            {
                if (oneSignalPlatform != null)
                {
                    internalSubscriptionObserver -= value;
                    if (addedSubscriptionObserver && internalSubscriptionObserver.GetInvocationList().Length == 0)
                        oneSignalPlatform.RemoveSubscriptionObserver();
                }
            }
        }

        private static bool addedSubscriptionObserver;

        private static void addSubscriptionObserver()
        {
            if (!addedSubscriptionObserver && internalSubscriptionObserver != null && internalSubscriptionObserver.GetInvocationList().Length > 0)
            {
                addedSubscriptionObserver = true;
                oneSignalPlatform.AddSubscriptionObserver();
            }
        }

        public delegate void EmailSubscriptionObservable(OSEmailSubscriptionStateChanges stateChanges);

        private static EmailSubscriptionObservable internalEmailSubscriptionObserver;

        public static event EmailSubscriptionObservable emailSubscriptionObserver
        {
            add
            {
                if (oneSignalPlatform != null)
                {
                    internalEmailSubscriptionObserver += value;
                    addEmailSubscriptionObserver();
                }
            }
            remove
            {
                if (oneSignalPlatform != null)
                {
                    internalEmailSubscriptionObserver -= value;
                    if (addedEmailSubscriptionObserver && internalEmailSubscriptionObserver.GetInvocationList().Length == 0)
                        oneSignalPlatform.RemoveEmailSubscriptionObserver();
                }
            }
        }

        private static bool addedEmailSubscriptionObserver;

        private static void addEmailSubscriptionObserver()
        {
            if (!addedEmailSubscriptionObserver && internalEmailSubscriptionObserver != null && internalEmailSubscriptionObserver.GetInvocationList().Length > 0)
            {
                addedEmailSubscriptionObserver = true;
                oneSignalPlatform.AddEmailSubscriptionObserver();
            }
        }

        /// <summary>
        /// Auto prompt user for notification permissions.
        /// </summary>
        public const string kOSSettingsAutoPrompt = "kOSSettingsAutoPrompt";
        /// <summary>
        /// Launch notifications with a launch URL as an in app webview.
        /// </summary>
        public const string kOSSettingsInAppLaunchURL = "kOSSettingsInAppLaunchURL";

        public enum LOG_LEVEL
        {
            NONE,
            FATAL,
            ERROR,
            WARN,
            INFO,
            DEBUG,
            VERBOSE
        }

        public enum OSInFocusDisplayOption
        {
            None,
            InAppAlert,
            Notification
        }

        public class UnityBuilder
        {
            /// <summary>
            /// Your OneSignal AppId from onesignal.com
            /// </summary>
            public string appID;
            /// <summary>
            /// Your Google Project Number that is only required for Android GCM pushes.
            /// </summary>
            public string googleProjectNumber;
            public Dictionary<string, bool> iOSSettings;
            /// <summary>
            /// The delegate will be called when an In App Message action is received.
            /// </summary>
            public NotificationReceived notificationReceivedDelegate;
            /// <summary>
            /// The delegate will be called when an In App Message action is opened.
            /// </summary>
            public NotificationOpened notificationOpenedDelegate;
            /// <summary>
            /// The delegate will be called when an In App Message action is tapped on.
            /// </summary>
            public InAppMessageClicked inAppMessageClickHandlerDelegate;
            
            /// <summary>
            /// Builder Method.
            /// Sets a In App Message opened handler. The instance will be called when an In App Message action is received.
            /// Method must be static or this object should be marked as DontDestroyOnLoad.
            /// </summary>
            /// <param name="inNotificationReceivedDelegate">Calls this delegate when a notification is received.</param>
            /// <returns></returns>
            public UnityBuilder HandleNotificationReceived(NotificationReceived inNotificationReceivedDelegate)
            {
                notificationReceivedDelegate = inNotificationReceivedDelegate;
                return this;
            }
            
            /// <summary>
            /// Builder Method.
            /// Sets a In App Message opened handler. The instance will be called when an In App Message action is opened.
            /// Method must be static or this object should be marked as DontDestroyOnLoad.
            /// </summary>
            /// <param name="inNotificationOpenedDelegate">Calls this delegate when a push notification is opened.</param>
            /// <returns></returns>
            public UnityBuilder HandleNotificationOpened(NotificationOpened inNotificationOpenedDelegate)
            {
                notificationOpenedDelegate = inNotificationOpenedDelegate;
                return this;
            }
            
            /// <summary>
            /// Builder Method.
            /// Sets a In App Message click handler. The instance will be called when an In App Message action is tapped on.
            /// Method must be static or this object should be marked as DontDestroyOnLoad.
            /// </summary>
            /// <param name="inInAppMessageClickedDelegate">Calls this delegate when an In-App Message is opened.</param>
            /// <returns></returns>
            public UnityBuilder HandleInAppMessageClicked(InAppMessageClicked inInAppMessageClickedDelegate)
            {
                inAppMessageClickHandlerDelegate = inInAppMessageClickedDelegate;
                return this;
            }


            /// <summary>
            /// Setting to control how OneSignal notifications will be shown when one is received while your app is in focus.
            /// </summary>
            /// <param name="display">Display options.</param>
            /// <returns></returns>
            public UnityBuilder InFocusDisplaying(OSInFocusDisplayOption display)
            {
                inFocusDisplayType = display;
                return this;
            }

            /// <summary>
            /// Pass one if the define kOSSettings strings as keys only. Only affects iOS platform.
            /// autoPrompt = Set false to delay the iOS accept notification system prompt. Defaults true.
            /// You can then call RegisterForPushNotifications at a better point in your game to prompt them.
            /// inAppLaunchURL = (iOS) Set false to force a URL to launch through Safari instead of in-app webview.
            /// </summary>
            /// <param name="settings">Settings dictionary.</param>
            /// <returns></returns>
            public UnityBuilder Settings(Dictionary<string, bool> settings)
            {
                //bool autoPrompt, bool inAppLaunchURL
#if UNITY_IPHONE
                iOSSettings = settings;
#endif
                return this;
            }

            /// <summary>
            /// Must be called after 'StartInit' to complete initialization of OneSignal.
            /// </summary>
            public void EndInit()
            {
                Init();
            }

            /// <summary>
            /// Delays initialization of the SDK until the user provides privacy consent.
            /// </summary>
            /// <param name="required">
            /// If you pass in 'true', your application will need to call provideConsent(true) before the OneSignal SDK gets fully initialized.
            /// </param>
            /// <returns></returns>
            public UnityBuilder SetRequiresUserPrivacyConsent(bool required)
            {
                System.Diagnostics.Debug.WriteLine("Did call setRequiresUserPrivacyConsent in OneSignal.cs");
#if ONESIGNAL_PLATFORM
                OneSignal.requiresUserConsent = true;
#endif
                return this;
            }

        }

        internal static UnityBuilder builder = null;

        private static IOneSignalPlatform oneSignalPlatform = null;

        // Name of the GameObject that gets automaticly created in your game scene.
        internal const string GameObjectName = "OneSignalRuntimeObject_KEEP";
        internal static LOG_LEVEL logLevel = LOG_LEVEL.INFO, visualLogLevel = LOG_LEVEL.NONE;
        internal static bool requiresUserConsent = false;

        /// <summary>
        /// Call this on the first scene that is loaded. Only required method you call to setup OneSignal to receive push notifications.
        /// </summary>
        public static UnityBuilder StartInit()
        {
            return StartInit(OneSignalSettings.Instance.ApplicationId);
        }

        /// <summary>
        /// Call this on the first scene that is loaded. Only required method you call to setup OneSignal to receive push notifications.
        /// </summary>
        /// <param name="appId">Your OneSignal AppId from onesignal.com</param>
        /// <returns></returns>
        public static UnityBuilder StartInit(string appId)
        {
            return StartInit(appId, null);
        }

        /// <summary>
        /// Call this on the first scene that is loaded. Only required method you call to setup OneSignal to receive push notifications.
        /// </summary>
        /// <param name="appId">Your OneSignal AppId from onesignal.com</param>
        /// <param name="googleProjectNumber">Your Google Project Number that is only required for Android GCM pushes.</param>
        /// <returns></returns>
        public static UnityBuilder StartInit(string appId, string googleProjectNumber)
        {
            if (builder == null)
                builder = new UnityBuilder();
#if ONESIGNAL_PLATFORM
            builder.appID = appId;
            builder.googleProjectNumber = googleProjectNumber;
#endif
            return builder;
        }

        private static void Init()
        {
            if (Application.isEditor)
            {
                print("Please run OneSignal on a device to see push notifications.");
                return;
            }

            if (delegates == null)
                delegates = new Dictionary<string, Delegate>();

            if(oneSignalPlatform == null)
                throw new InvalidOperationException($"{Application.platform} platform is not supported by OneSignal.");

            oneSignalPlatform.Init();
            if (!Application.isEditor)
            {
                var go = new GameObject(GameObjectName);
                go.AddComponent<OneSignal>();
                DontDestroyOnLoad(go);
            }

            addPermissionObserver();
            addSubscriptionObserver();
        }

        internal static void RegisterPlatform(IOneSignalPlatform platform)
        {
            Debug.Log("RegisterPlatform: " + platform);
            if(oneSignalPlatform != null)
                throw new InvalidOperationException($"{nameof(oneSignalPlatform)} has already been initialized.");

            oneSignalPlatform = platform;
        }


        private static OSInFocusDisplayOption _inFocusDisplayType = OSInFocusDisplayOption.InAppAlert;

        /// <summary>
        /// Setting to control how OneSignal notifications will be shown when one is received while your app is in focus.
        /// </summary>
        public static OSInFocusDisplayOption inFocusDisplayType
        {
            get { return _inFocusDisplayType; }
            set
            {
                _inFocusDisplayType = value;
                if (oneSignalPlatform != null)
                    oneSignalPlatform.SetInFocusDisplaying(_inFocusDisplayType);
            }
        }

        /// <summary>
        /// Enable logging to help debug OneSignal implementation.
        /// </summary>
        /// <param name="inLogLevel">Sets the logging level to print to the Android LogCat log or Xcode log.</param>
        /// <param name="inVisualLevel">Sets the logging level to show as alert dialogs.</param>
        public static void SetLogLevel(LOG_LEVEL inLogLevel, LOG_LEVEL inVisualLevel)
        {
            logLevel = inLogLevel; visualLogLevel = inVisualLevel;
        }

        /// <summary>
        /// Disable or enable location collection (defaults to enabled if your app has location permission).
        /// **Note:** This method must be called before OneSignal `initWithLaunchOptions` on iOS.
        /// </summary>
        public static void SetLocationShared(bool shared)
        {
            Debug.Log("Called OneSignal.cs SetLocationShared");
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.SetLocationShared(shared);
#endif
        }

        /// <summary>
        /// Tag player with a key value pair to later create segments on them at onesignal.com.
        /// </summary>
        public static void SendTag(string tagName, string tagValue)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.SendTag(tagName, tagValue);
#endif
        }

        /// <summary>
        /// Tag player with a key value pairs to later create segments on them at onesignal.com.
        /// </summary>
        /// <param name="tags">Tags dictionary.</param>
        public static void SendTags(Dictionary<string, string> tags)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.SendTags(tags);
#endif
        }

        /// <summary>
        /// Retrieve a list of tags that have been set on the player from the OneSignal server.
        /// </summary>
        public static void GetTags()
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.GetTags(null);
#endif
        }

        /// <summary>
        /// Makes a request to onesignal.com to get current tags set on the player and then run the callback passed in.
        /// </summary>
        public static void GetTags(TagsReceived inTagsReceivedDelegate)
        {
#if ONESIGNAL_PLATFORM
            string delegateGuid = OneSignalUnityUtils.GetNewGuid();
            delegates.Add(delegateGuid, inTagsReceivedDelegate);

            oneSignalPlatform.GetTags(delegateGuid);
#endif
        }

        /// <summary>
        /// Delete a Tag from current device record.
        /// </summary>
        public static void DeleteTag(string key)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.DeleteTag(key);
#endif
        }

        /// <summary>
        /// Delete multiple Tags from current device record.
        /// </summary>
        public static void DeleteTags(IList<string> keys)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.DeleteTags(keys);
#endif
        }


        /// <summary>
        /// Call this when you would like to prompt an iOS user accept push notifications with the default system prompt.
        /// Only use if you passed false to autoRegister when calling Init.
        /// </summary>
        public static void RegisterForPushNotifications()
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.RegisterForPushNotifications();
#endif
        }

        /// <summary>
        /// Prompt the user for notification permissions.
        /// Callback fires as soon as the user accepts or declines notifications.
        /// Must set `kOSSettingsKeyAutoPrompt` to `false` when calling <see href="https://documentation.onesignal.com/docs/unity-sdk#initwithlaunchoptions">initWithLaunchOptions</see>.
        ///
        /// Recommended: Set to false and follow <see href="https://documentation.onesignal.com/docs/ios-push-opt-in-prompt">iOS Push Opt-In Prompt</see>.
        /// </summary>
        public static void PromptForPushNotificationsWithUserResponse(PromptForPushNotificationsUserResponse inDelegate)
        {
#if ONESIGNAL_PLATFORM
            notificationUserResponseDelegate = inDelegate;
            oneSignalPlatform.PromptForPushNotificationsWithUserResponse();
#endif
        }
        
        /// <summary>
        /// <remarks> Set OneSignal.idsAvailableDelegate before calling this method or use the method above.</remarks>
        /// </summary>
        public static void IdsAvailable()
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.IdsAvailable(null);
#endif
        }
        
        /// <summary>
        /// Call this if you need the playerId and/or pushToken
        /// **NOTE:** pushToken maybe null if notifications are not accepted or there is connectivity issues.
        /// </summary>
        /// <param name="inIdsAvailableDelegate"></param>
        public static void IdsAvailable(IdsAvailableCallback inIdsAvailableDelegate)
        {
#if ONESIGNAL_PLATFORM
            string delegateGuid = OneSignalUnityUtils.GetNewGuid();
            delegates.Add(delegateGuid, inIdsAvailableDelegate);

            oneSignalPlatform.IdsAvailable(delegateGuid);
#endif
        }

        /// <summary>
        /// Android - When user receives notification, vibrate device less.
        /// </summary>
        public static void EnableVibrate(bool enable)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.EnableVibrate(enable);
#endif
        }

        /// <summary>
        /// Android - When user receives notification, do not play a sound
        /// </summary>
        public static void EnableSound(bool enable)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.EnableSound(enable);
#endif
        }

        /// <summary>
        /// Removes all OneSignal app notifications from the Notification Shade.
        /// </summary>
        public static void ClearOneSignalNotifications()
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.ClearOneSignalNotifications();
#endif
        }

        /// <summary>
        /// Disable OneSignal from sending notifications to current device.
        /// </summary>
        public static void SetSubscription(bool enable)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.SetSubscription(enable);
#endif
        }

        /// <summary>
        /// Allows you to set the user's email address with the OneSignal SDK. We offer several overloaded versions of this method.
        /// If the user changes their email, you need to call logoutEmail and then setEmail to update it.
        /// </summary>
        public static void SetEmail(string email)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.SetEmail(null, null, email);
#endif
        }

        /// <summary>
        /// Allows you to set the user's email address with the OneSignal SDK. We offer several overloaded versions of this method.
        /// If the user changes their email, you need to call logoutEmail and then setEmail to update it.
        /// </summary>
        /// <param name="successDelegate">Delegate fires when the email was set on OneSignal's server.</param>
        /// <param name="failureDelegate">Delegate fires when the email failed to be set.</param>
        public static void SetEmail(string email, OnSetEmailSuccess successDelegate, OnSetEmailFailure failureDelegate)
        {
#if ONESIGNAL_PLATFORM
            string delegateGuidSuccess = OneSignalUnityUtils.GetNewGuid();
            string delegateGuidFailure = OneSignalUnityUtils.GetNewGuid();

            delegates.Add(delegateGuidSuccess, successDelegate);
            delegates.Add(delegateGuidFailure, failureDelegate);

            oneSignalPlatform.SetEmail(delegateGuidSuccess, delegateGuidFailure, email);
#endif
        }

        /// <summary>
        /// Allows you to set the user's email address with the OneSignal SDK. We offer several overloaded versions of this method.
        /// If the user changes their email, you need to call logoutEmail and then setEmail to update it.
        /// </summary>
        /// <param name="emailAuthToken">
        /// If you have a backend server, we strongly recommend using **Identity Verification** with your users.
        /// Your backend can generate an email authentication token and send it to your app.
        /// </param>
        public static void SetEmail(string email, string emailAuthToken)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.SetEmail(null, null, email, emailAuthToken);
#endif
        }
        
        /// <summary>
        /// Allows you to set the user's email address with the OneSignal SDK. We offer several overloaded versions of this method.
        /// If the user changes their email, you need to call logoutEmail and then setEmail to update it.
        /// </summary>
        /// <param name="emailAuthToken">
        /// If you have a backend server, we strongly recommend using **Identity Verification** with your users.
        /// Your backend can generate an email authentication token and send it to your app.
        /// </param>
        /// <param name="successDelegate">Delegate fires when the email was set on OneSignal's server.</param>
        /// <param name="failureDelegate">Delegate fires when the email failed to be set.</param>
        public static void SetEmail(string email, string emailAuthToken, OnSetEmailSuccess successDelegate, OnSetEmailFailure failureDelegate)
        {
#if ONESIGNAL_PLATFORM
            string delegateGuidSuccess = OneSignalUnityUtils.GetNewGuid();
            string delegateGuidFailure = OneSignalUnityUtils.GetNewGuid();

            delegates.Add(delegateGuidSuccess, successDelegate);
            delegates.Add(delegateGuidFailure, failureDelegate);

            oneSignalPlatform.SetEmail(delegateGuidSuccess, delegateGuidFailure, email, emailAuthToken);
#endif
        }

        /// <summary>
        /// If your app implements logout functionality, you can call logoutEmail to dissociate the email from the device
        /// </summary>
        public static void LogoutEmail()
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.LogoutEmail(null, null);
#endif
        }

        /// <summary>
        /// If your app implements logout functionality, you can call logoutEmail to dissociate the email from the device
        /// </summary>
        /// <param name="successDelegate">Delegate fires when the email was set on OneSignal's server.</param>
        /// <param name="failureDelegate">Delegate fires when the email failed to be set.</param>
        public static void LogoutEmail(OnLogoutEmailSuccess successDelegate, OnLogoutEmailFailure failureDelegate)
        {
#if ONESIGNAL_PLATFORM
            string delegateGuidSuccess = OneSignalUnityUtils.GetNewGuid();
            string delegateGuidFailure = OneSignalUnityUtils.GetNewGuid();

            delegates.Add(delegateGuidSuccess, successDelegate);
            delegates.Add(delegateGuidFailure, failureDelegate);

            oneSignalPlatform.LogoutEmail(delegateGuidSuccess, delegateGuidFailure);
#endif
        }

        /// <summary>
        /// Allows you to send notifications from user to user or schedule ones in the future to be delivered to the current device.
        /// </summary>
        /// <param name="data">Dictionary of notification options, see our Create notification POST call for all options.</param>
        public static void PostNotification(Dictionary<string, object> data)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.PostNotification(null, null, data);
#endif
        }

        /// <summary>
        /// Send or schedule a notification to a OneSignal Player ID.
        /// </summary>
        /// <param name="data">Dictionary of notification options, see our **Create notification** POST call for all options.</param>
        /// <param name="inOnPostNotificationSuccess">Delegate fires when the notification was created on OneSignal's server.</param>
        /// <param name="inOnPostNotificationFailure">Delegate fires when the notification failed to create.</param>
        public static void PostNotification(Dictionary<string, object> data, OnPostNotificationSuccess inOnPostNotificationSuccess, OnPostNotificationFailure inOnPostNotificationFailure)
        {
#if ONESIGNAL_PLATFORM
            string delegateGuidSuccess = OneSignalUnityUtils.GetNewGuid();
            string delegateGuidFailure = OneSignalUnityUtils.GetNewGuid();

            delegates.Add(delegateGuidSuccess, inOnPostNotificationSuccess);
            delegates.Add(delegateGuidFailure, inOnPostNotificationFailure);

            oneSignalPlatform.PostNotification(delegateGuidSuccess, delegateGuidFailure, data);
#endif
        }

        public static void SyncHashedEmail(string email)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.SyncHashedEmail(email);
#endif
        }

        public static void PromptLocation()
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.PromptLocation();
#endif
        }

        /// <summary>
        /// Get the current notification and permission state. Returns a OSPermissionSubscriptionState type.
        /// </summary>
        /// <returns></returns>
        public static OSPermissionSubscriptionState GetPermissionSubscriptionState()
        {
#if ONESIGNAL_PLATFORM
            return oneSignalPlatform.GetPermissionSubscriptionState();
#else
            var state = new OSPermissionSubscriptionState();
            state.permissionStatus = new OSPermissionState();
            state.subscriptionStatus = new OSSubscriptionState();
            return state;
#endif
        }

        public static void UserDidProvideConsent(bool consent)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.UserDidProvideConsent(consent);
#endif
        }

        public static bool UserProvidedConsent()
        {
#if ONESIGNAL_PLATFORM
            return oneSignalPlatform.UserProvidedConsent();
#else
            return true;
#endif
        }

        public static void SetRequiresUserPrivacyConsent(bool required)
        {
#if ONESIGNAL_PLATFORM
            OneSignal.requiresUserConsent = required;
#endif
        }


        public static void SetExternalUserId(string externalId)
        {
#if ONESIGNAL_PLATFORM
            string delegateGuidCompletion = OneSignalUnityUtils.GetNewGuid();
            oneSignalPlatform.SetExternalUserId(delegateGuidCompletion, externalId);
#endif
        }

        public static void SetExternalUserId(string externalId, OnExternalUserIdUpdateCompletion completion)
        {
#if ONESIGNAL_PLATFORM
            string delegateGuidCompletion = OneSignalUnityUtils.GetNewGuid();
            delegates.Add(delegateGuidCompletion, completion);
            oneSignalPlatform.SetExternalUserId(delegateGuidCompletion, externalId);
#endif
        }

        public static void RemoveExternalUserId()
        {
#if ONESIGNAL_PLATFORM
            string delegateGuidCompletion = OneSignalUnityUtils.GetNewGuid();
            oneSignalPlatform.RemoveExternalUserId(delegateGuidCompletion);
#endif
        }

        public static void RemoveExternalUserId(OnExternalUserIdUpdateCompletion completion)
        {
#if ONESIGNAL_PLATFORM
            string delegateGuidCompletion = OneSignalUnityUtils.GetNewGuid();
            delegates.Add(delegateGuidCompletion, completion);
            oneSignalPlatform.RemoveExternalUserId(delegateGuidCompletion);
#endif
        }

        public static void AddTrigger(string key, object value)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.AddTrigger(key, value);
#endif
        }

        public static void AddTriggers(Dictionary<string, object> triggers)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.AddTriggers(triggers);
#endif
        }

        public static void RemoveTriggerForKey(string key)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.RemoveTriggerForKey(key);
#endif
        }

        public static void RemoveTriggersForKeys(IList<string> keys)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.RemoveTriggersForKeys(keys);
#endif
        }

        public static object GetTriggerValueForKey(string key)
        {
#if ONESIGNAL_PLATFORM
            return oneSignalPlatform.GetTriggerValueForKey(key);
#else
            return null;
#endif
        }

        public static void PauseInAppMessages(bool pause)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.PauseInAppMessages(pause);
#endif
        }

        public static void SendOutcome(string name)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.SendOutcome(null, name);
#endif
        }

        public static void SendOutcome(string name, OnSendOutcomeSuccess onSendOutcomeSuccess)
        {
#if ONESIGNAL_PLATFORM
            string delegateGuid = OneSignalUnityUtils.GetNewGuid();
            delegates.Add(delegateGuid, onSendOutcomeSuccess);

            oneSignalPlatform.SendOutcome(delegateGuid, name);
#endif
        }

        public static void SendUniqueOutcome(string name)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.SendUniqueOutcome(null, name);
#endif
        }

        public static void SendUniqueOutcome(string name, OnSendOutcomeSuccess onSendOutcomeSuccess)
        {
#if ONESIGNAL_PLATFORM
            string delegateGuid = OneSignalUnityUtils.GetNewGuid();
            delegates.Add(delegateGuid, onSendOutcomeSuccess);

            oneSignalPlatform.SendUniqueOutcome(delegateGuid, name);
#endif
        }

        public static void SendOutcomeWithValue(string name, float value)
        {
#if ONESIGNAL_PLATFORM
            oneSignalPlatform.SendOutcomeWithValue(null, name, value);
#endif
        }

        public static void SendOutcomeWithValue(string name, float value, OnSendOutcomeSuccess onSendOutcomeSuccess)
        {
#if ONESIGNAL_PLATFORM
            string delegateGuid = OneSignalUnityUtils.GetNewGuid();
            delegates.Add(delegateGuid, onSendOutcomeSuccess);

            oneSignalPlatform.SendOutcomeWithValue(delegateGuid, name, value);
#endif
        }

        /*** protected and private methods ****/
#if ONESIGNAL_PLATFORM

        private OSNotification DictionaryToNotification(Dictionary<string, object> jsonObject) {
            OSNotification notification = new OSNotification();
            OSNotificationPayload payload = new OSNotificationPayload();

            //Build OSNotification object from jsonString
            var payloadObj = jsonObject["payload"] as Dictionary<string, object>;
            if (payloadObj.ContainsKey("notificationID")) payload.notificationID = payloadObj["notificationID"] as string;
            if (payloadObj.ContainsKey("sound")) payload.sound = payloadObj["sound"] as string;
            if (payloadObj.ContainsKey("title")) payload.title = payloadObj["title"] as string;
            if (payloadObj.ContainsKey("body")) payload.body = payloadObj["body"] as string;
            if (payloadObj.ContainsKey("subtitle")) payload.subtitle = payloadObj["subtitle"] as string;
            if (payloadObj.ContainsKey("launchURL")) payload.launchURL = payloadObj["launchURL"] as string;
            if (payloadObj.ContainsKey("additionalData")) {
                if(payloadObj["additionalData"] is string)
                payload.additionalData = Json.Deserialize(payloadObj["additionalData"] as string) as Dictionary<string, object>;
                else
                payload.additionalData = payloadObj["additionalData"] as Dictionary<string, object>;
            }
            if (payloadObj.ContainsKey("actionButtons")) {
                if(payloadObj["actionButtons"] is string)
                    payload.actionButtons = Json.Deserialize(payloadObj["actionButtons"] as string) as Dictionary<string, object>;
                else
                    payload.actionButtons = payloadObj["actionButtons"] as Dictionary<string, object>;
            }
            if (payloadObj.ContainsKey("contentAvailable")) payload.contentAvailable = (bool)payloadObj["contentAvailable"];
            if (payloadObj.ContainsKey("badge")) payload.badge = Convert.ToInt32(payloadObj["badge"]);
            if (payloadObj.ContainsKey("smallIcon")) payload.smallIcon = payloadObj["smallIcon"] as string;
            if (payloadObj.ContainsKey("largeIcon")) payload.largeIcon = payloadObj["largeIcon"] as string;
            if (payloadObj.ContainsKey("bigPicture")) payload.bigPicture = payloadObj["bigPicture"] as string;
            if (payloadObj.ContainsKey("smallIconAccentColor")) payload.smallIconAccentColor = payloadObj["smallIconAccentColor"] as string;
            if (payloadObj.ContainsKey("ledColor")) payload.ledColor = payloadObj["ledColor"] as string;
            if (payloadObj.ContainsKey("lockScreenVisibility")) payload.lockScreenVisibility = Convert.ToInt32(payloadObj["lockScreenVisibility"]);
            if (payloadObj.ContainsKey("groupKey")) payload.groupKey = payloadObj["groupKey"] as string;
            if (payloadObj.ContainsKey("groupMessage")) payload.groupMessage = payloadObj["groupMessage"] as string;
            if (payloadObj.ContainsKey("fromProjectNumber")) payload.fromProjectNumber = payloadObj["fromProjectNumber"] as string;
            notification.payload = payload;

            if (jsonObject.ContainsKey("isAppInFocus")) notification.isAppInFocus = (bool)jsonObject["isAppInFocus"];
            if (jsonObject.ContainsKey("shown")) notification.shown = (bool)jsonObject["shown"];
            if (jsonObject.ContainsKey("silentNotification")) notification.silentNotification = (bool)jsonObject["silentNotification"];
            if (jsonObject.ContainsKey("androidNotificationId")) notification.androidNotificationId = Convert.ToInt32(jsonObject["androidNotificationId"]);
            if (jsonObject.ContainsKey("displayType")) notification.displayType = (OSNotification.DisplayType)Convert.ToInt32(jsonObject["displayType"]);

            return notification;
        }

        // Called from the native SDK - Called when a push notification received.
        private void onPushNotificationReceived(string jsonString) {
            if (builder.notificationReceivedDelegate != null) {
                var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;
                builder.notificationReceivedDelegate(DictionaryToNotification(jsonObject));
            }
        }

        // Called from the native SDK - Called when a push notification is opened by the user
        private void onPushNotificationOpened(string jsonString) {
            if (builder.notificationOpenedDelegate != null) {
                Dictionary<string, object> jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

                OSNotificationAction action = new OSNotificationAction();
                if (jsonObject.ContainsKey("action")) {
                    Dictionary<string, object> actionJsonObject = jsonObject["action"] as Dictionary<string, object>;

                    if (actionJsonObject.ContainsKey("actionID"))
                        action.actionID = actionJsonObject["actionID"] as string;
                    if (actionJsonObject.ContainsKey("type"))
                        action.type = (OSNotificationAction.ActionType)Convert.ToInt32(actionJsonObject["type"]);
                }

                OSNotificationOpenedResult result = new OSNotificationOpenedResult();
                result.notification = DictionaryToNotification((Dictionary<string, object>)jsonObject["notification"]);
                result.action = action;

                builder.notificationOpenedDelegate(result);
            }
        }

        // Called from the native SDK - Called when device is registered with onesignal.com service or right after IdsAvailable
        //   if already registered.
        private void onIdsAvailable(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidDelegate(jsonObject))
                return;

            var delegateId = jsonObject["delegate_id"] as string;
            var response = jsonObject["response"] as string;

            var ids = Json.Deserialize(response) as Dictionary<string, object>;
            var userId = ids["userId"] as string;
            var pushToken = ids["pushToken"] as string;

            if (delegates.ContainsKey(delegateId)) {
                var idsAvailableCallback = (IdsAvailableCallback) delegates[delegateId];
                delegates.Remove(delegateId);
                idsAvailableCallback(userId, pushToken);
            }
        }

        // Called from the native SDK - Called After calling GetTags(...)
        private void onTagsReceived(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidDelegate(jsonObject))
                return;

            var delegateId = jsonObject["delegate_id"] as string;
            var response = jsonObject["response"] as string;

            var tags = Json.Deserialize(response) as Dictionary<string, object>;

            if (!string.IsNullOrEmpty(delegateId) && delegates.ContainsKey(delegateId)) {
                var tagsReceivedDelegate = (TagsReceived) delegates[delegateId];
                delegates.Remove(delegateId);
                tagsReceivedDelegate(tags);
            }
        }

        // Called from the native SDK
        private void onPostNotificationSuccess(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidSuccessFailureDelegate(jsonObject))
                return;

            var delegateId = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;
            var delegateIdSuccess = delegateId["success"] as string;
            var delegateIdFailure = delegateId["failure"] as string;

            var response = jsonObject["response"] as string;
            var postNotificationDic = Json.Deserialize(response) as Dictionary<string, object>;

            if (delegates.ContainsKey(delegateIdSuccess)) {
                var postNotificationSuccessDelegate = (OnPostNotificationSuccess) delegates[delegateIdSuccess];
                delegates.Remove(delegateIdSuccess);
                delegates.Remove(delegateIdFailure);
                postNotificationSuccessDelegate(postNotificationDic);
            }
        }

        // Called from the native SDK
        private void onPostNotificationFailed(string jsonString) {
             if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidSuccessFailureDelegate(jsonObject))
                return;

            var delegateId = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;
            var delegateIdSuccess = delegateId["success"] as string;
            var delegateIdFailure = delegateId["failure"] as string;

            var response = jsonObject["response"] as string;
            var postNotificationDic = Json.Deserialize(response) as Dictionary<string, object>;

            if (delegates.ContainsKey(delegateIdFailure)) {
                var postNotificationFailureDelegate = (OnPostNotificationFailure) delegates[delegateIdFailure];
                delegates.Remove(delegateIdSuccess);
                delegates.Remove(delegateIdFailure);
                postNotificationFailureDelegate(postNotificationDic);
            }
        }

        // Called from the native SDK
        private void onExternalUserIdUpdateCompletion(string jsonString) {
             if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidDelegate(jsonObject))
                return;

            var delegateId = jsonObject["delegate_id"] as string;

            var response = jsonObject["response"] as string;
            var results = Json.Deserialize(response) as Dictionary<string, object>;

            if (delegates.ContainsKey(delegateId)) {
                var externalUserIdUpdateCompletionDelegate = (OnExternalUserIdUpdateCompletion) delegates[delegateId];
                delegates.Remove(delegateId);
                externalUserIdUpdateCompletionDelegate(results);
            }
        }

        // Called from the native SDK
        private void onSetEmailSuccess(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidSuccessFailureDelegate(jsonObject))
                return;

            var delegateId = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;
            var delegateIdSuccess = delegateId["success"] as string;
            var delegateIdFailure = delegateId["failure"] as string;

            var response = jsonObject["response"] as string;

            if (delegates.ContainsKey(delegateIdSuccess)) {
                var setEmailSuccessDelegate = (OnSetEmailSuccess) delegates[delegateIdSuccess];
                delegates.Remove(delegateIdSuccess);
                delegates.Remove(delegateIdFailure);
                setEmailSuccessDelegate();
            }
        }

        // Called from the native SDK
        private void onSetEmailFailure(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidSuccessFailureDelegate(jsonObject))
                return;

            var delegateId = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;
            var delegateIdSuccess = delegateId["success"] as string;
            var delegateIdFailure = delegateId["failure"] as string;

            var response = jsonObject["response"] as string;
            var failure = Json.Deserialize(response) as Dictionary<string, object>;

            if (delegates.ContainsKey(delegateIdFailure)) {
                var setEmailFailureDelegate = (OnSetEmailFailure) delegates[delegateIdFailure];
                delegates.Remove(delegateIdSuccess);
                delegates.Remove(delegateIdFailure);
                setEmailFailureDelegate(failure);
            }
        }

        // Called from the native SDK
        private void onLogoutEmailSuccess(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidSuccessFailureDelegate(jsonObject))
                return;

            var delegateId = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;
            var delegateIdSuccess = delegateId["success"] as string;
            var delegateIdFailure = delegateId["failure"] as string;

            var response = jsonObject["response"] as string;

            if (delegates.ContainsKey(delegateIdSuccess)) {
                var logoutEmailSuccessDelegate = (OnLogoutEmailSuccess) delegates[delegateIdSuccess];
                delegates.Remove(delegateIdSuccess);
                delegates.Remove(delegateIdFailure);
                logoutEmailSuccessDelegate();
            }
        }

        // Called from the native SDK
        private void onLogoutEmailFailure(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidSuccessFailureDelegate(jsonObject))
                return;

            var delegateId = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;
            var delegateIdSuccess = delegateId["success"] as string;
            var delegateIdFailure = delegateId["failure"] as string;

            var response = jsonObject["response"] as string;
            var failure = Json.Deserialize(response) as Dictionary<string, object>;

            if (delegates.ContainsKey(delegateIdFailure)) {
                var logoutEmailFailureDelegate = (OnLogoutEmailFailure) delegates[delegateIdFailure];
                delegates.Remove(delegateIdSuccess);
                delegates.Remove(delegateIdFailure);
                logoutEmailFailureDelegate(failure);
            }
        }

        // Called from the native SDK - Called After calling SendOutcome(...), SendUniqueOutcome(...), SendOutcomeWithValue(...)
        private void onSendOutcomeSuccess(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidDelegate(jsonObject))
                return;

            var delegateId = jsonObject["delegate_id"] as string;
            var response = jsonObject["response"] as string;

            OSOutcomeEvent outcomeEvent;
            if (string.IsNullOrEmpty(response)) {
                outcomeEvent = new OSOutcomeEvent();
            }
            else {
                // Parse outcome json string and return it through the callback
                var outcomeObject = Json.Deserialize(response) as Dictionary<string, object>;
                outcomeEvent = new OSOutcomeEvent(outcomeObject);
            }

            if (delegates.ContainsKey(delegateId) && delegates[delegateId] != null) {
                var sendOutcomeSuccess = (OnSendOutcomeSuccess) delegates[delegateId];
                delegates.Remove(delegateId);
                sendOutcomeSuccess(outcomeEvent);
            }
        }

        // Called from native SDK
        private void onOSPermissionChanged(string stateChangesJSONString) {
            OSPermissionStateChanges stateChanges = oneSignalPlatform.ParseOSPermissionStateChanges(stateChangesJSONString);
            internalPermissionObserver(stateChanges);
        }

        // Called from native SDK
        private void onOSSubscriptionChanged(string stateChangesJSONString) {
            OSSubscriptionStateChanges stateChanges = oneSignalPlatform.ParseOSSubscriptionStateChanges(stateChangesJSONString);
            internalSubscriptionObserver(stateChanges);
        }

        // Called from native SDK
	    private void onOSEmailSubscriptionChanged(string stateChangesJSONString) {
		    OSEmailSubscriptionStateChanges stateChanges = oneSignalPlatform.ParseOSEmailSubscriptionStateChanges(stateChangesJSONString);
		    internalEmailSubscriptionObserver(stateChanges);
	    }

        // Called from native SDk
        private void onPromptForPushNotificationsWithUserResponse(string accepted) {
            notificationUserResponseDelegate(Convert.ToBoolean(accepted));
        }

        // Called from native SDK
        private void onInAppMessageClicked(string jsonString) {
            if (builder.inAppMessageClickHandlerDelegate == null)
                return;

            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            var action = new OSInAppMessageAction();
            if (jsonObject.ContainsKey("click_name"))
                action.clickName = jsonObject["click_name"] as String;
            if (jsonObject.ContainsKey("click_url"))
                action.clickUrl = jsonObject["click_url"] as String;
            if (jsonObject.ContainsKey("closes_message"))
                action.closesMessage = (bool)jsonObject["closes_message"];
            if (jsonObject.ContainsKey("first_click"))
                action.firstClick = (bool)jsonObject["first_click"];

            builder.inAppMessageClickHandlerDelegate(action);
        }

        // Some functions have a single delegate, so this validates nothing is missing from the json response
        private bool isValidDelegate(Dictionary<string, object> jsonObject) {
            // Make sure 'delegate_id' exists
            if (!jsonObject.ContainsKey("delegate_id"))
                return false;

            // Make sure 'response' exists
            if (!jsonObject.ContainsKey("response"))
                return false;

            return true;
        }

        // Some functions have a 'success' and 'failure' delegates, so this validates nothing is missing the json response
        private bool isValidSuccessFailureDelegate(Dictionary<string, object> jsonObject) {
            if (!isValidDelegate(jsonObject))
                return false;

            // Make sure success and failure delegate exist
            var delegateId = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;
            if (!delegateId.ContainsKey("success") || !delegateId.ContainsKey("failure"))
                return false;

            return true;
        }

#endif
    }
}
