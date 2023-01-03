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

#if ONE_SIGNAL_INSTALLED
using System;
using OneSignalSDKNew; // TODO: Change namespace and delete MiniJson copy, OneSignalExampleBehaviourOld, OneSignalExampleSceneOld
using UnityEngine;
using UnityEngine.UI;
using OneSignalSDKNew.Debug.Utilities;
using OneSignalSDKNew.Debug.Models;
using OneSignalSDKNew.Notifications.Models;
using OneSignalSDKNew.InAppMessages.Models;
using OneSignalSDKNew.User.Models;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
/// <summary>
/// Example class to show how an application can utilize the public methods of the OneSignal SDK
/// </summary>
public class OneSignalExampleBehaviour : MonoBehaviour {
    /// <summary>
    /// set to an email address you would like to test notifications against
    /// </summary>
    public string email;

    /// <summary>
    /// set to an external user id you would like to test notifications against
    /// </summary>
    public string externalId;

    /// <summary>
    /// set to an external user id you would like to test notifications against
    /// </summary>
    public string phoneNumber;

    /// <summary>
    /// set to your app id (https://documentation.onesignal.com/docs/accounts-and-keys)
    /// </summary>
    public string appId;

    /// <summary>
    /// whether you would prefer OneSignal Unity SDK prevent initialization until consent is granted via
    /// <see cref="OneSignal.RequiresPrivacyConsent"/> in this test MonoBehaviour
    /// </summary>
    public bool requiresUserPrivacyConsent;

    /// <summary>
    /// used to set if launch URLs should be opened in safari or within the application
    /// </summary>
    public bool launchURLsInApp;

    /// <summary>
    /// 
    /// </summary>
    public string tagKey;

    /// <summary>
    /// 
    /// </summary>
    public string tagValue;

    /// <summary>
    /// 
    /// </summary>
    public string triggerKey;

    /// <summary>
    /// 
    /// </summary>
    public string triggerValue;

    /// <summary>
    /// 
    /// </summary>
    public string outcomeKey;

    /// <summary>
    /// 
    /// </summary>
    public float outcomeValue;

    /// <summary>
    /// we recommend initializing OneSignal early in your application's lifecycle such as in the Start method of a
    /// MonoBehaviour in your opening Scene
    /// </summary>
    private void Start() {
        // Enable lines below to debug issues with OneSignal
        OneSignal.Default.Debug.LogLevel = LogLevel.Info;
        OneSignal.Default.Debug.AlertLevel = LogLevel.Fatal;

        // Setting RequiresPrivacyConsent to true will prevent the OneSignalSDK from operating until
        // PrivacyConsent is also set to true
        OneSignal.Default.RequiresPrivacyConsent = requiresUserPrivacyConsent;

        // Setup the below to listen for and respond to events from notifications
        OneSignal.Default.Notifications.Clicked += _notificationOnClick;
        OneSignal.Default.Notifications.WillShow += _notificationOnDisplay;

        // Setup the below to listen for and respond to events from in-app messages
        OneSignal.Default.InAppMessages.WillDisplay += _iamWillDisplay;
        OneSignal.Default.InAppMessages.DidDisplay += _iamDidDisplay;
        OneSignal.Default.InAppMessages.WillDismiss += _iamWillDismiss;
        OneSignal.Default.InAppMessages.DidDismiss += _iamDidDismiss;
        OneSignal.Default.InAppMessages.Clicked += _iamOnClick;

        // Setup the below to listen for and respond to state changes
        OneSignal.Default.Notifications.PermissionChanged += _notificationPermissionChanged;
        OneSignal.Default.User.PushSubscription.Changed += _pushSubscriptionChanged;
    }

    /*
     * SDK events
     */
    
    private void _notificationOnClick(NotificationClickedResult result) {
        _log($"Notification was clicked with result: {JsonUtility.ToJson(result)}");
    }

    private Notification _notificationOnDisplay(Notification notification) {
        var additionalData = notification.additionalData != null
            ? Json.Serialize(notification.additionalData)
                : null;

        _log($"Notification was received in foreground: {JsonUtility.ToJson(notification)}\n{additionalData}");
        return notification; // show the notification
    }

    private void _iamWillDisplay(InAppMessage inAppMessage) {
        _log($"IAM will display: {JsonUtility.ToJson(inAppMessage)}");
    }

    private void _iamDidDisplay(InAppMessage inAppMessage) {
        _log($"IAM did display: {JsonUtility.ToJson(inAppMessage)}");
    }

    private void _iamWillDismiss(InAppMessage inAppMessage) {
        _log($"IAM will dismiss: {JsonUtility.ToJson(inAppMessage)}");
    }

    private void _iamDidDismiss(InAppMessage inAppMessage) {
        _log($"IAM did dismiss: {JsonUtility.ToJson(inAppMessage)}");
    }

    private void _iamOnClick(InAppMessageClickedResult result) {
        _log($"IAM was clicked with result: {JsonUtility.ToJson(result)}");
    }

    private void _notificationPermissionChanged(bool permission) {
        _log($"Notification Permissions changed to: {permission}");
    }

    private void _pushSubscriptionChanged(IPushSubscription subscription) {
        _log($"Push subscription changed to: {subscription.Id}");
    }

    /*
     * SDK setup
     */
    
    public void Initialize() {
        _log($"Initializing with appId <b>{appId}</b>");
        OneSignal.Default.Initialize(appId);
    }

    public void ToggleRequiresPrivacyConsent() {
        if (OneSignal.Default.RequiresPrivacyConsent)
            _error($"Cannot toggle RequiresPrivacyConsent from TRUE to FALSE");
        else {
            _log($"Toggling RequiresPrivacyConsent to <b>{!OneSignal.Default.RequiresPrivacyConsent}</b>");
            OneSignal.Default.RequiresPrivacyConsent = !OneSignal.Default.RequiresPrivacyConsent;
        }
    }

    public void TogglePrivacyConsent() {
        _log($"Toggling PrivacyConsent to <b>{!OneSignal.Default.PrivacyConsent}</b>");
        OneSignal.Default.PrivacyConsent = !OneSignal.Default.PrivacyConsent;
    }

    public void SetLogLevel() {
        var newLevel = _nextEnum(OneSignal.Default.Debug.LogLevel);
        _log($"Setting LogLevel to <b>{newLevel}</b>");
        
        // LogLevel uses the standard Unity LogType
        OneSignal.Default.Debug.LogLevel = newLevel;
    }

    public void SetAlertLevel() {
        var newLevel = _nextEnum(OneSignal.Default.Debug.AlertLevel);
        _log($"Setting AlertLevel to <b>{newLevel}</b>");

        // AlertLevel uses the standard Unity LogType
        OneSignal.Default.Debug.AlertLevel = newLevel;
    }

    /*
     * User identification
     */

    public async void LoginOneSignalUser() {
        _log($"Logging in user (<b>{externalId}</b>) and awaiting result...");

        await OneSignal.Default.LoginAsync(externalId);
    }
    
    public void LogoutOneSignalUser() {
        _log($"Logging out user ");
        OneSignal.Default.LogoutAsync();
    }

    public void AddEmail() {
        _log($"Adding email (<b>{email}</b>)");
        OneSignal.Default.User.AddEmailSubscription(email);
    }
    
    public void RemoveEmail() {
        _log($"Removing email (<b>{email}</b>)");
        OneSignal.Default.User.RemoveEmailSubscription(email);
    }

    public void AddSms() {
        _log($"Adding sms (<b>{phoneNumber}</b>)");
        OneSignal.Default.User.AddSmsSubscription(phoneNumber);
    }
    
    public void RemoveSms() {
        _log($"Removing sms (<b>{phoneNumber}</b>)");
        OneSignal.Default.User.RemoveSmsSubscription(phoneNumber);
    }

    /*
     * Push
     */

    public async void PromptForPush() {
        _log("Opening permission prompt for push notifications and awaiting result...");

        var result = await OneSignal.Default.Notifications.RequestPermissionAsync(true);

        if (result)
            _log("User opted in");
        else
            _log("User opted out");
    }

    public void ClearPush() {
        _log("Clearing existing OneSignal push notifications...");
        OneSignal.Default.Notifications.ClearAllNotifications();
    }

    /*
     * In-App Messages
     */

    public void AddTrigger() {
        _log($"Adding trigger with key <b>{triggerKey}</b> and value <b>{triggerValue}</b>");
        OneSignal.Default.InAppMessages.AddTrigger(triggerKey, triggerValue);
    }

    public void RemoveTrigger() {
        _log($"Removing trigger for key <b>{triggerKey}</b>");
        OneSignal.Default.InAppMessages.RemoveTrigger(triggerKey);
    }

    public void ClearTriggers() {
        _log("Clearing all trigger keys and values from user");
        OneSignal.Default.InAppMessages.ClearTriggers();
    }

    public void TogglePauseInAppMessages() {
        _log($"Toggling Pausing InAppMessages to <b>{!OneSignal.Default.InAppMessages.Paused}</b>");
        OneSignal.Default.InAppMessages.Paused = !OneSignal.Default.InAppMessages.Paused;
    }

    /*
     * Tags
     */

    public void AddTag() {
        _log($"Adding tag with key <b>{tagKey}</b> and value <b>{tagValue}</b>");
        OneSignal.Default.User.AddTag(tagKey, tagValue);
    }

    public void RemoveTag() {
        _log($"Removing tag for key <b>{tagKey}</b>");
        OneSignal.Default.User.RemoveTag(tagKey);
    }

    /*
     * Outcomes
     */

    public void AddOutcome() {
        _log($"Adding outcome with key <b>{outcomeKey}</b>");
        OneSignal.Default.Session.AddOutcome(outcomeKey);
    }

    public void AddUniqueOutcome() {
        _log($"Adding unique outcome with key <b>{outcomeKey}</b>");
        OneSignal.Default.Session.AddUniqueOutcome(outcomeKey);
    }

    public void AddOutcomeWithValue() {
        _log($"Adding outcome with key <b>{outcomeKey}</b> and value <b>{outcomeValue}</b>");
        OneSignal.Default.Session.AddOutcomeWithValue(outcomeKey, outcomeValue);
    }

    /*
     * Location
     */

    public async void PromptLocation() {
        _log("Opening permission prompt for location and awaiting result...");

        var result = await OneSignal.Default.Location.RequestPermissionAsync(true);

        if (result)
            _log("User opted in");
        else
            _log("User opted out");
    }

    public void ToggleShareLocation() {
        _log($"Toggling ShareLocation to <b>{!OneSignal.Default.Location.IsShared}</b>");
        OneSignal.Default.Location.IsShared = !OneSignal.Default.Location.IsShared;
    }

    /*
     * iOS
     */

    public void ToggleLaunchURLsInApp() {
        _log($"Toggling LaunchURLsInApp to <b>{!launchURLsInApp}</b>");
        launchURLsInApp = !launchURLsInApp;
        // Call setLaunchURLsInApp before the Initialize call
        OneSignal.Default.SetLaunchURLsInApp(launchURLsInApp);
    }

#region Rendering
    /*
     * You can safely ignore everything in this region and below
     */

    public Text console;

    public void SetAppIdString(string newVal) => appId = newVal;

    public void SetExternalIdString(string newVal) => externalId = newVal;
    public void SetEmailString(string newVal) => email = newVal;
    public void SetPhoneNumberString(string newVal) => phoneNumber = newVal;

    public void SetTriggerKey(string newVal) => triggerKey = newVal;
    public void SetTriggerValue(string newVal) => triggerValue = newVal;

    public void SetTagKey(string newVal) => tagKey = newVal;
    public void SetTagValue(string newVal) => tagValue = newVal;

    public void SetOutcomeKey(string newVal) => outcomeKey = newVal;
    public void SetOutcomeValue(string newVal) => outcomeValue = Convert.ToSingle(newVal);

    private void Awake() {
        SDKDebug.LogIntercept   += _log;
        SDKDebug.WarnIntercept  += _warn;
        SDKDebug.ErrorIntercept += _error;
    }

    private void _log(object message) {
        UnityEngine.Debug.Log(message);
        console.text += $"\n<color=green><b>I></b></color> {message}";
    }

    private void _warn(object message) {
        UnityEngine.Debug.LogWarning(message);
        console.text += $"\n<color=orange><b>W></b></color> {message}";
    }

    private void _error(object message) {
        UnityEngine.Debug.LogError(message);
        console.text += $"\n<color=red><b>E></b></color> {message}";
    }
#endregion

#region Helpers
    private static T _nextEnum<T>(T src) where T : struct {
        if (!typeof(T).IsEnum)
            throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");
        var vals = (T[])Enum.GetValues(src.GetType());
        var next = Array.IndexOf(vals, src) + 1;
        return vals.Length == next ? vals[0] : vals[next];
    }
#endregion
}
#endif