/*
 * Modified MIT License
 *
 * Copyright 2022 OneSignal
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
using System.Threading.Tasks;
using UnityEngine;

namespace OneSignalSDK {
    /// <summary>
    /// OneSignal SDK for Unity
    /// </summary>
    public abstract partial class OneSignal {
        public const string Version = "3.0.4";

        /// <summary>
        /// The default static instance of the OneSignal Unity SDK
        /// </summary>
        public static OneSignal Default {
            get => _getDefaultInstance();
            internal set {
                Debug.Log($"[OneSignal] OneSignal.Default set to platform SDK {value.GetType()}. Current version is {Version}");
                _default = value;
            }
        }

    #region Delegate Definitions
        /// <summary>
        /// When a push notification has been received and is about to be displayed
        /// </summary>
        /// <param name="notification">Details of the notification to be shown</param>
        /// <returns>The notification object or null if the notification should not be displayed</returns>
        public delegate Notification NotificationWillShowDelegate(Notification notification);

        /// <summary>
        /// When a push notification was acted on by the user.
        /// </summary>
        /// <param name="result">The Notification open result describing:
        ///     1. The notification opened
        ///     2. The action taken by the user.
        /// </param>
        public delegate void NotificationActionDelegate(NotificationOpenedResult result);

        /// <summary>
        /// When any client side event in an In-App Message's occurs there will be a corresponding event with this
        /// delegate signature.
        /// </summary>
        public delegate void InAppMessageLifecycleDelegate(InAppMessage message);
        
        /// <summary>
        /// Sets a In App Message opened handler. The instance will be called when an In App Message action is tapped on.
        /// </summary>
        public delegate void InAppMessageActionDelegate(InAppMessageAction action);

        /// <summary>
        /// Several states associated with the SDK can be changed can be changed in and outside of the application.
        /// </summary>
        public delegate void StateChangeDelegate<in TState>(TState current, TState previous);
    #endregion

    #region Events
        /*
         * Notifications
         */

        /// <summary>
        /// When a push notification has been received while app is in the foreground
        /// </summary>
        public abstract event NotificationWillShowDelegate NotificationWillShow;

        /// <summary>
        /// When a push notification has been opened by the user
        /// </summary>
        public abstract event NotificationActionDelegate NotificationOpened;

        /*
         * In App Messages
         */

        /// <summary>
        /// When an In-App Message is ready to be displayed to the screen
        /// </summary>
        public abstract event InAppMessageLifecycleDelegate InAppMessageWillDisplay;

        /// <summary>
        /// When an In-App Message is has been displayed to the screen
        /// </summary>
        public abstract event InAppMessageLifecycleDelegate InAppMessageDidDisplay;

        /// <summary>
        /// When a user has chosen to dismiss an In-App Message
        /// </summary>
        public abstract event InAppMessageLifecycleDelegate InAppMessageWillDismiss;

        /// <summary>
        /// When an In-App Message has finished being dismissed
        /// </summary>
        public abstract event InAppMessageLifecycleDelegate InAppMessageDidDismiss;

        /// <summary>
        /// When a user has triggered an action attached to an In-App Message
        /// </summary>
        public abstract event InAppMessageActionDelegate InAppMessageTriggeredAction;

        /*
         * States
         */

        /// <summary>
        /// When this device's permissions for authorization of push notifications have changed.
        /// </summary>
        public abstract event StateChangeDelegate<NotificationPermission> NotificationPermissionChanged;

        /// <summary>
        /// When this device's subscription to push notifications has changed
        /// </summary>
        public abstract event StateChangeDelegate<PushSubscriptionState> PushSubscriptionStateChanged;

        /// <summary>
        /// When this device's subscription to email has changed
        /// </summary>
        public abstract event StateChangeDelegate<EmailSubscriptionState> EmailSubscriptionStateChanged;

        /// <summary>
        /// When this device's subscription to sms has changed
        /// </summary>
        public abstract event StateChangeDelegate<SMSSubscriptionState> SMSSubscriptionStateChanged;
    #endregion

    #region SDK Setup
        /// <summary>
        /// The minimum level of logs which will be logged to the console
        /// </summary>
        public abstract LogLevel LogLevel { get; set; }

        /// <summary>
        /// The minimum level of log events which will be converted into foreground alerts
        /// </summary>
        public abstract LogLevel AlertLevel { get; set; }

        /// <summary>
        /// Provides privacy consent. OneSignal Unity SDK will not initialize until this is true.
        /// </summary>
        public abstract bool PrivacyConsent { get; set; }

        /// <summary>
        /// Allows you to delay the initialization of the SDK until <see cref="PrivacyConsent"/> is set to true. Must
        /// be set before <see cref="Initialize"/> is called.
        /// </summary>
        public abstract bool RequiresPrivacyConsent { get; set; }

        /// <summary>
        /// Used to set if launch URLs should be opened in safari or within the application. Make sure to set before 
        /// <see cref="Initialize"/> is called.
        /// </summary>
        /// <remarks>iOS Only</remarks>
        public abstract void SetLaunchURLsInApp(bool launchInApp);

        /// <summary>
        /// Starts the OneSignal SDK
        /// </summary>
        /// <param name="appId">Your application id from the OneSignal dashboard</param>
        public abstract void Initialize(string appId);
    #endregion

    #region Push Notifications
        /// <summary>
        /// Prompt the user for notification permissions.
        /// </summary>
        /// <returns>Awaitable NotificationPermission which provides the user's consent status</returns>
        /// <remarks>Recommended: Do not use and instead follow
        /// <a href="https://documentation.onesignal.com/docs/ios-push-opt-in-prompt">Push Opt-In Prompt</a></remarks>
        public abstract Task<NotificationPermission> PromptForPushNotificationsWithUserResponse();

        /// <summary>
        /// Removes all OneSignal app notifications from the Notification Shade
        /// </summary>
        /// <remarks>Android Only</remarks>
        public abstract void ClearOneSignalNotifications();

        /// <summary>
        /// Whether push notifications are currently enabled for an active push subscription.
        /// </summary>
        /// <remarks>Can be used to turn off push notifications for a user without removing their user data</remarks>
        public abstract bool PushEnabled { get; set; }

        /// <summary>
        /// Allows you to send notifications from user to user or schedule ones in the future to be delivered to the
        /// current device.
        /// </summary>
        /// <param name="options">Contains notification options, see
        /// <a href="https://documentation.onesignal.com/reference#create-notification">Create Notification POST </a>
        /// call for all options.</param>
        /// <remarks>
        /// You can only use include_player_ids as a targeting parameter from your app. Other target options such as
        /// {@code tags} and {@code included_segments} require your OneSignal App REST API key which can only be used
        /// from your server.</remarks>
        public abstract Task<Dictionary<string, object>> PostNotification(Dictionary<string, object> options);
    #endregion

    #region In App Messages
        /// <summary>
        /// Add a local trigger. May show an In-App Message if its triggers conditions were met.
        /// </summary>
        /// <param name="key">Key for the trigger</param>
        /// <param name="value">Value for the trigger</param>
        public abstract void SetTrigger(string key, string value);

        /// <summary>
        /// Allows you to set multiple local trigger key/value pairs simultaneously. May show an In-App Message if its
        /// triggers conditions were met.
        /// </summary>
        public abstract void SetTriggers(Dictionary<string, string> triggers);

        /// <summary>
        /// Removes a single local trigger for the given key.
        /// </summary>
        /// <param name="key">Key for the trigger.</param>
        public abstract void RemoveTrigger(string key);

        /// <summary>
        /// Removes a list of local triggers based on a collection of keys.
        /// </summary>
        /// <param name="keys">Removes a collection of triggers from their keys.</param>
        public abstract void RemoveTriggers(params string[] keys);

        /// <summary>
        /// Gets a local trigger value for a provided trigger key.
        /// </summary>
        /// <param name="key">Key for the trigger.</param>
        /// <returns>Value if added with 'addTrigger', or null/nil (iOS) if never set.</returns>
        public abstract string GetTrigger(string key);
        
        /// <summary>
        /// Returns all local trigger key-values for the current user
        /// </summary>
        public abstract Dictionary<string, string> GetTriggers();

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
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> SendTag(string key, object value);

        /// <summary>
        /// Tag player with a key value pairs to later create segments on them at onesignal.com
        /// </summary>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> SendTags(Dictionary<string, object> tags);

        /// <summary>
        /// Retrieve a list of tags that have been set on the user from the OneSignal server
        /// </summary>
        /// <returns>Awaitable <see cref="Dictionary{TKey,TValue}"/> of this user's tags</returns>
        public abstract Task<Dictionary<string, object>> GetTags();

        /// <summary>
        /// Delete a Tag from current device record
        /// </summary>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> DeleteTag(string key);

        /// <summary>
        /// Delete multiple Tags from current device record
        /// </summary>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> DeleteTags(params string[] keys);
    #endregion

    #region User & Device Properties
        /// <summary>
        /// Allows you to use your own application's user id to send OneSignal messages to your user. To tie the user
        /// to a given user id, you can use this method.
        /// </summary>
        /// <param name="externalId">A unique id associated with the current user</param>
        /// <param name="authHash">If you have a backend server, we strongly recommend using
        /// <a href="https://documentation.onesignal.com/docs/identity-verification">Identity Verification</a> with
        /// your users. Your backend can generate an email authentication token and send it to your app.</param>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> SetExternalUserId(string externalId, string authHash = null);

        /// <summary>
        /// Allows you to set the user's email address with the OneSignal SDK. If the user changes their email, you
        /// need to call LogOut(LogOutOptions.Email) and then SetEmail to update it.
        /// </summary>
        /// <param name="email">The email that you want subscribe and associate with the device</param>
        /// <param name="authHash">If you have a backend server, we strongly recommend using
        /// <a href="https://documentation.onesignal.com/docs/identity-verification">Identity Verification</a> with
        /// your users. Your backend can generate an email authentication token and send it to your app.</param>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> SetEmail(string email, string authHash = null);

        /// <summary>
        /// Set an sms number for the device to later send sms to this number
        /// </summary>
        /// <param name="smsNumber">The sms number that you want subscribe and associate with the device</param>
        /// <param name="authHash">If you have a backend server, we strongly recommend using
        /// <a href="https://documentation.onesignal.com/docs/identity-verification">Identity Verification</a> with
        /// your users. Your backend can generate an email authentication token and send it to your app.</param>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> SetSMSNumber(string smsNumber, string authHash = null);

        /// <summary>
        /// Removes the current external user id from each known subscription
        /// </summary>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> RemoveExternalUserId();

        /// <summary>
        /// If this user logs out of your app and/or you would like to disassociate their email with the current
        /// OneSignal user
        /// </summary>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> LogOutEmail();
        
        /// <summary>
        /// If this user logs out of your app and/or you would like to disassociate their phone number with the
        /// current OneSignal user
        /// </summary>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> LogOutSMS();

        /// <summary>
        /// Change the system detected language by passing in the desired language code. See
        /// https://documentation.onesignal.com/docs/language-localization#what-languages-are-supported
        /// for supported languages.
        /// </summary>
        /// <param name="languageCode">ISO 639-1 code representation for user input language</param>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> SetLanguage(string languageCode);
        
        /// <summary>
        /// Current status of permissions granted by this device for push notifications
        /// </summary>
        public abstract NotificationPermission NotificationPermission { get; }
        
        /// <summary>
        /// Current status of this device's subscription to push notifications
        /// </summary>
        public abstract PushSubscriptionState PushSubscriptionState { get; }
        
        /// <summary>
        /// Current status of this device's subscription to email
        /// </summary>
        public abstract EmailSubscriptionState EmailSubscriptionState { get; }
        
        /// <summary>
        /// Current status of this device's subscription to sms
        /// </summary>
        public abstract SMSSubscriptionState SMSSubscriptionState { get; }
    #endregion

    #region Location
        /// <summary>
        /// Helper method to show the native prompt to ask the user for consent to share their location
        /// </summary>
        /// <remarks>iOS Only</remarks>
        public abstract void PromptLocation();

        /// <summary>
        /// Disable or enable location collection by OneSignal (defaults to enabled if your app has location permission).
        /// </summary>
        /// <remarks>This method must be called before <see cref="OneSignal.Initialize"/> on iOS.</remarks>
        public abstract bool ShareLocation { get; set; }
    #endregion

    #region Outcomes
        /// <summary>
        /// Send a trackable custom event which is tied to push notification campaigns
        /// </summary>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> SendOutcome(string name);

        /// <summary>
        /// Send a trackable custom event which can only happen once and is tied to push notification campaigns
        /// </summary>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> SendUniqueOutcome(string name);

        /// <summary>
        /// Send a trackable custom event with an attached value which is tied to push notification campaigns
        /// </summary>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        public abstract Task<bool> SendOutcomeWithValue(string name, float value);
    #endregion

    }
}