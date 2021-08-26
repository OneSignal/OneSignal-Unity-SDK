/*
 * Modified MIT License
 *
 * Copyright 2021 OneSignal
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace OneSignalSDK {
    /// <summary>
    /// todo - desc
    /// </summary>
    public abstract partial class OneSignal {
        /// <summary>
        /// 
        /// </summary>
        public const string Version = "3.0.0";
        
        /// <summary>
        /// The default static instance of the OneSignal Unity SDK
        /// </summary>
        public static OneSignal Default {
            get => _getDefaultInstance();
            internal set {
                Debug.Log($"Setting OneSignal.Default to platform SDK {value.GetType()}");
                _default = value;
            }
        }

    #region Delegate Definitions
        /*
         * Notifications
         */

        /// <summary>
        /// When a push notification is received when the user is in your game.
        /// </summary>
        /// <param name="notification"> The Notification dictionary filled from a serialized native OSNotification object</param>
        public delegate void NotificationReceivedDelegate(Notification notification);

        /// <summary>
        /// When a push notification is opened.
        /// </summary>
        /// <param name="result">The Notification open result describing: 1. The notification opened 2. The action taken by the user. </param>
        public delegate void NotificationOpenedDelegate(NotificationOpenedResult result);

        /*
         * In App Messages
         */

        /// <summary>
        /// Sets a In App Message opened handler. The instance will be called when an In App Message action is tapped on.
        /// </summary>
        /// <param name="action">Instance to a class implementing this interference.</param>
        public delegate void InAppMessageClickedDelegate(InAppMessageAction action);

        /*
         * Outcomes
         */

        /// <summary>
        /// todo - this
        /// </summary>
        /// <param name="outcomeEvent"></param>
        public delegate void OnSendOutcomeSuccessDelegate(OutcomeEvent outcomeEvent);

        /*
         * Properties
         */

        /// <summary>
        /// When external user id for push or email channel is set or removed.
        /// todo - Dictionary should be an object
        /// </summary>
        /// <param name="results">The dictionary payload containing the success status for the channels updating external user id.</param>
        public delegate void OnUpdateExternalUserIdDelegate(Dictionary<string, object> results);

        /// <summary>
        /// When update of external user id for push or email channel has failed.
        /// todo - Dictionary should be an object
        /// todo - Should this be public?
        /// </summary>
        /// <param name="error">The dictionary payload containing errors.</param>
        public delegate void OnUpdateExternalUserIdFailedDelegate(Dictionary<string, object> error);

        /// <summary>
        /// When the email was set on OneSignal's server.
        /// todo - should this exist?
        /// </summary>
        public delegate void OnEmailUpdatedDelegate();

        /// <summary>
        /// When the email failed to be set.
        /// todo - should this exist?
        /// </summary>
        public delegate void OnEmailUpdateFailureDelegate(Dictionary<string, object> error);

        /// <summary>
        /// Delegate fires when the email was set on OneSignal's server.
        /// todo - should this exist? what is this?
        /// </summary>
        public delegate void OnLogoutEmailSuccessDelegate();

        /// <summary>
        /// Delegate fires when the email failed to be set.
        /// todo - should this exist? what is this?
        /// </summary>
        public delegate void OnLogoutEmailFailureDelegate(Dictionary<string, object> error);

        /// <summary>
        /// todo - desc
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="current"></param>
        /// <param name="previous"></param>
        public delegate void OnStateChangeDelegate<in TState>(TState current, TState previous);

        /*
         * ???
         */

        /// <summary>
        /// Delegate you can define to get the all the tags set on a player from onesignal.com
        /// todo - not a dictionary
        /// todo - is
        /// </summary>
        /// <param name="tags">Dictionary of key value pairs retrieved from the OneSignal server.</param>
        public delegate void TagsReceivedDelegate(Dictionary<string, object> tags);

        /// <summary>
        /// todo - this
        /// </summary>
        public delegate void PushPermissionResponseDelegate(bool accepted);
    #endregion

    #region Events
        /*
         * Notifications
         */

        /// <summary>
        /// When an In App Message action is received. 
        /// todo - is this desc accurate? is it ONLY IAM?
        /// todo - more desc!
        /// </summary>
        public abstract event NotificationReceivedDelegate NotificationReceived;

        /// <summary>
        /// When an In App Message action is opened.
        /// todo - is this desc accurate? is it ONLY IAM?
        /// todo - more desc!
        /// </summary>
        public abstract event NotificationOpenedDelegate NotificationOpened;

        /*
         * In App Messages
         */

        /// <summary>
        /// The delegate will be called when an In App Message action is tapped on.
        /// </summary>
        public abstract event InAppMessageClickedDelegate InAppMessageClicked;

        /*
         * Data Changes
         */

        /// <summary>
        /// todo - this
        /// </summary>
        public abstract event OnStateChangeDelegate<PermissionState> PermissionStateChanged;

        /// <summary>
        /// todo - this
        /// </summary>
        public abstract event OnStateChangeDelegate<SubscriptionState> SubscriptionStateChanged;

        /// <summary>
        /// todo - this
        /// </summary>
        public abstract event OnStateChangeDelegate<EmailSubscriptionState> EmailSubscriptionStateChanged;
    #endregion

    #region SDK Setup
        /// <summary>
        /// todo - custom level?
        /// </summary>
        public LogType LogLevel { get; set; }
        
        /// <summary>
        /// todo - custom level?
        /// </summary>
        public LogType AlertLevel { get; set; }

        /// <summary>
        /// Your Google Project Number that is only required for Android GCM pushes.
        /// todo - isn't this deprecated?
        /// </summary>
        public string GoogleProjectNumber { get; set; }

        /// <summary>
        /// todo - not a dictionary
        /// </summary>
        public Dictionary<string, bool> iOSSettings { get; set; }

        /// <summary>
        /// Provides privacy consent. OneSignal Unity SDK will not initialize until this is true.
        /// </summary>
        public abstract bool PrivacyConsent { get; set; }
        
        /// <summary>
        /// Allows you to delay the initialization of the SDK until the user provides privacy consent. The SDK will not
        /// be fully initialized until 'PrivacyConsent = true'
        /// </summary>
        public abstract bool RequiresPrivacyConsent { get; set; }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="appId"></param>
        public abstract void Initialize(string appId);
    #endregion

    #region Push Notifications
        /// <summary>
        /// Call this when you would like to prompt an iOS user accept push notifications with the default system prompt.
        /// Only use if you passed false to autoRegister when calling Init.
        /// </summary>
        /// <remarks>iOS Only</remarks>
        public abstract void RegisterForPushNotifications();

        /// <summary>
        /// Prompt the user for notification permissions.
        /// Callback fires as soon as the user accepts or declines notifications.
        /// Must set `kOSSettingsKeyAutoPrompt` to `false` when calling <see href="https://documentation.onesignal.com/docs/unity-sdk#initwithlaunchoptions">initWithLaunchOptions</see>.
        /// </summary>
        /// <remarks>Recommended: Set to false and follow <see href="https://documentation.onesignal.com/docs/ios-push-opt-in-prompt">iOS Push Opt-In Prompt</see>.</remarks>
        /// <returns>Awaitable <see cref="Task{TResult}"/> which provides the user's consent status</returns>
        public abstract Task<OSNotificationPermission> PromptForPushNotificationsWithUserResponse();

        /// <summary>
        /// Removes all OneSignal app notifications from the Notification Shade
        /// </summary>
        public abstract void ClearOneSignalNotifications();
    #endregion

    #region In App Message
        /// <summary>
        /// Add a trigger, may show an In-App Message if its triggers conditions were met.
        /// </summary>
        /// <param name="key">Key for the trigger.</param>
        /// <param name="value">Value for the trigger. Object passed in will be converted to a string.</param>
        public abstract void AddTrigger(string key, object value);

        /// <summary>
        /// Allows you to set multiple trigger key/value pairs simultaneously.
        /// </summary>
        public abstract void AddTriggers(Dictionary<string, object> triggers);

        /// <summary>
        /// Removes a single trigger for the given key. May show an In-App Message if its trigger conditions were met.
        /// </summary>
        /// <param name="key">Key for the trigger.</param>
        public abstract void RemoveTriggerForKey(string key);

        /// <summary>
        /// Removes a list of triggers based on a collection of keys. May show an In-App Message if its trigger
        /// conditions were met.
        /// </summary>
        /// <param name="keys">Removes a collection of triggers from their keys.</param>
        public abstract void RemoveTriggersForKeys(IList<string> keys);

        /// <summary>
        /// Gets a trigger value for a provided trigger key.
        /// </summary>
        /// <param name="key">Key for the trigger.</param>
        /// <returns>Value if added with 'addTrigger', or null/nil (iOS) if never set.</returns>
        public abstract object GetTriggerValueForKey(string key);

        /// <summary>
        /// Allows you to temporarily pause all In-App Messages. You may want to do this while the user is engaged in
        /// an activity that you don't want a message to interrupt (such as watching a video).
        /// </summary>
        public abstract bool InAppMessagesArePaused { get; set; }
    #endregion

    #region Tags
        /// <summary>
        /// Tag player with a key value pair to later create segments on them at onesignal.com
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="tagValue"></param>
        public abstract void SendTag(string tagName, string tagValue);

        /// <summary>
        /// Tag player with a key value pairs to later create segments on them at onesignal.com
        /// </summary>
        /// <param name="tags"></param>
        public abstract void SendTags(IDictionary<string, string> tags);

        /// <summary>
        /// Retrieve a list of tags that have been set on the player from the OneSignal server
        /// todo - what does this do?
        /// </summary>
        public abstract void GetTags();

        /// <summary>
        /// todo - this
        /// </summary>
        /// <returns>Awaitable task which will provide the complete <see cref="Dictionary{TKey,TValue}"/> of tags after
        /// querying OneSignal</returns>
        public abstract Task<Dictionary<string, object>> RefreshTags();

        /// <summary>
        /// Delete a Tag from current device record
        /// </summary>
        /// <param name="key"></param>
        public abstract void DeleteTag(string key);

        /// <summary>
        /// Delete multiple Tags from current device record
        /// </summary>
        /// <param name="keys"></param>
        public abstract void DeleteTags(IEnumerable<string> keys);
    #endregion

    #region User Properties
        /// <summary>
        /// Allows you to use your own system's user ID's to send push notifications to your users. To tie a user to a given user ID, you can use this method.
        /// </summary>
        public abstract void SetExternalUserId(string externalId);

        /// <summary>
        /// todo - desc
        /// </summary>
        /// <param name="externalId"></param>
        /// <param name="authHashToken"></param>
        public abstract void SetExternalUserId(string externalId, string authHashToken);

        /// <summary>
        /// Allows you to set the user's email address with the OneSignal SDK. We offer several overloaded versions of this method.
        /// If the user changes their email, you need to call logoutEmail and then setEmail to update it.
        /// </summary>
        /// <param name="email"></param>
        public abstract void SetEmail(string email);

        /// <summary>
        /// Allows you to set the user's email address with the OneSignal SDK. We offer several overloaded versions of this method.
        /// If the user changes their email, you need to call logoutEmail and then setEmail to update it.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="emailAuthToken">
        /// If you have a backend server, we strongly recommend using <see href="https://documentation.onesignal.com/docs/identity-verification">Identity Verification</see> with your users.
        /// Your backend can generate an email authentication token and send it to your app.
        /// </param>
        public abstract void SetEmail(string email, string emailAuthToken);

        /// <summary>
        /// 
        /// </summary>
        [Flags] public enum LogOutOptions {
            /// <summary>todo - desc</summary>
            Email,
            
            /// <summary>
            /// If your user logs out of your app and you would like to disassociate their custom user ID from your
            /// system with their OneSignal user ID
            /// </summary>
            ExternalUserId
        }
        
        /// <summary>
        /// If your app implements logout functionality, you can call logoutEmail to dissociate the email and/or
        /// external user id from the device (todo - language clarification, device or OneSignal user id?)
        /// </summary>
        /// <param name="options">todo - desc. Defaults to both</param>
        public abstract void LogOut(LogOutOptions options = LogOutOptions.Email | LogOutOptions.ExternalUserId);
    #endregion

    #region Location
        /// <summary>
        /// Helper method to show the native prompt to ask the user for consent to share their location
        /// </summary>
        public abstract void PromptLocation();
        
        /// <summary>
        /// Disable or enable location collection by OneSignal (defaults to enabled if your app has location permission).
        /// </summary>
        /// <remarks>This method must be called before OneSignal `initWithLaunchOptions` on iOS.</remarks>
        public abstract bool ShareLocation { get; set; }
    #endregion

    #region Outcomes
        /// <summary>
        /// todo - desc
        /// </summary>
        /// <param name="name"></param>
        public abstract void SendOutcome(string name);

        /// <summary>
        /// todo - desc
        /// </summary>
        /// <param name="name"></param>
        public abstract void SendUniqueOutcome(string name);

        /// <summary>
        /// todo - desc
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public abstract void SendOutcomeWithValue(string name, float value);
    #endregion

    }
}