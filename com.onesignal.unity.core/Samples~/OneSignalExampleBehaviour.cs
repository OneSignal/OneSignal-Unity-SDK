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
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using OneSignalSDK;
using OneSignalSDK.Notifications.Models;
using OneSignalSDK.InAppMessages.Models;
using OneSignalSDK.User.Models;
using OneSignalSDK.Debug.Models;
using OneSignalSDK.Debug.Utilities;

using OneSignalSDK.Notifications; // needed for NotificationPermissionChangedEventArgs

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
    /// <see cref="OneSignal.ConsentRequired"/> in this test MonoBehaviour
    /// </summary>
    public bool consentRequired;

    /// <summary>
    /// 
    /// </summary>
    public bool consentGiven;

    /// <summary>
    /// used to set if launch URLs should be opened in safari or within the application
    /// </summary>
    public bool launchURLsInApp;

    /// <summary>
    /// 
    /// </summary>
    public string language;

    /// <summary>
    /// 
    /// </summary>
    public string aliasKey;

    /// <summary>
    /// 
    /// </summary>
    public string aliasValue;

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
    /// 
    /// </summary>
    public string liveActivityId;

    /// <summary>
    /// 
    /// </summary>
    public string liveActivityToken;

    /// <summary>
    /// we recommend initializing OneSignal early in your application's lifecycle such as in the Start method of a
    /// MonoBehaviour in your opening Scene
    /// </summary>
    private void Start() {
        // Enable lines below to debug issues with OneSignal
        OneSignal.Default.Debug.LogLevel = LogLevel.Info;
        OneSignal.Default.Debug.AlertLevel = LogLevel.Fatal;

        _log($"Initializing with appId <b>{appId}</b>");
        OneSignal.Default.Initialize(appId);

        // Setting ConsentRequired to true will prevent the OneSignalSDK from operating until
        // PrivacyConsent is also set to true
        OneSignal.Default.ConsentRequired = consentRequired;

        // Setup the below to listen for and respond to events from notifications
        OneSignal.Default.Notifications.Clicked += _notificationOnClick;
        OneSignal.Default.Notifications.ForegroundWillDisplay += _notificationOnDisplay;
        OneSignal.Default.Notifications.PermissionChanged += _notificationPermissionChanged;

        // Setup the below to listen for and respond to events from in-app messages
        //OneSignal.Default.InAppMessages.WillDisplay += _iamWillDisplay;
        //OneSignal.Default.InAppMessages.DidDisplay += _iamDidDisplay;
        //OneSignal.Default.InAppMessages.WillDismiss += _iamWillDismiss;
        //OneSignal.Default.InAppMessages.DidDismiss += _iamDidDismiss;
        //OneSignal.Default.InAppMessages.Clicked += _iamOnClick;

        // Setup the below to listen for and respond to state changes
        OneSignal.Default.User.PushSubscription.Changed += _pushSubscriptionChanged;
    }

    /*
     * SDK events
     */
    
    private void _notificationOnClick(object sender, NotificationClickEventArgs e) {
        _log($"Notification was clicked with Notification: {JsonUtility.ToJson(e.Notification)}");
        _log($"Notification was clicked with Result: {JsonUtility.ToJson(e.Result)}");
    }

    private void _notificationOnDisplay(object sender, NotificationWillDisplayEventArgs e) {
        var additionalData = e.Notification.AdditionalData != null
            ? Json.Serialize(e.Notification.AdditionalData)
                : null;

        _log($"Notification was received in foreground: {JsonUtility.ToJson(e.Notification)}\n{additionalData}");
        
        e.Notification.Display();
    }

    private void _notificationPermissionChanged(object sender, NotificationPermissionChangedEventArgs e) {
        _log($"Notification Permission changed to: {e.Permission}");
    }

    private void _iamWillDisplay(IInAppMessage inAppMessage) {
        _log($"IAM will display: {JsonUtility.ToJson(inAppMessage)}");
    }

    private void _iamDidDisplay(IInAppMessage inAppMessage) {
        _log($"IAM did display: {JsonUtility.ToJson(inAppMessage)}");
    }

    private void _iamWillDismiss(IInAppMessage inAppMessage) {
        _log($"IAM will dismiss: {JsonUtility.ToJson(inAppMessage)}");
    }

    private void _iamDidDismiss(IInAppMessage inAppMessage) {
        _log($"IAM did dismiss: {JsonUtility.ToJson(inAppMessage)}");
    }

    private void _iamOnClick(InAppMessageClickedResult result) {
        _log($"IAM was clicked with Action: {JsonUtility.ToJson(result.Action)}");
    }

    private void _pushSubscriptionChanged(object sender, PushSubscriptionChangedEventArgs e) {
        _log($"Push subscription changed from previous: {JsonUtility.ToJson(e.State.Previous)}");
        _log($"Push subscription changed to current: {JsonUtility.ToJson(e.State.Current)}");
    }

    /*
     * SDK setup
     */

    public void ToggleConsentRequired() {
        consentRequired = !consentRequired;
        _log($"Toggling ConsentRequired to <b>{consentRequired}</b>");
        OneSignal.Default.ConsentRequired = consentRequired;
    }

    public void ToggleConsentGiven() {
        consentGiven = !consentGiven;
        _log($"Toggling PrivacyConsent to <b>{consentGiven}</b>");
        OneSignal.Default.ConsentGiven = consentGiven;
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

    public void LoginOneSignalUser() {
        _log($"Logging in user (<b>{externalId}</b>)");
        OneSignal.Default.Login(externalId);
    }
    
    public void LogoutOneSignalUser() {
        _log($"Logging out user");
        OneSignal.Default.Logout();
    }

    public void PushSubscriptionOptIn() {
        _log($"Opting in push subscription");
        OneSignal.Default.User.PushSubscription.OptIn();
    }

    public void PushSubscriptionOptOut() {
        _log($"Opting out push subscription");
        OneSignal.Default.User.PushSubscription.OptOut();
    }

    public void AddEmail() {
        _log($"Adding email (<b>{email}</b>)");
        OneSignal.Default.User.AddEmail(email);
    }
    
    public void RemoveEmail() {
        _log($"Removing email (<b>{email}</b>)");
        OneSignal.Default.User.RemoveEmail(email);
    }

    public void AddSms() {
        _log($"Adding sms (<b>{phoneNumber}</b>)");
        OneSignal.Default.User.AddSms(phoneNumber);
    }
    
    public void RemoveSms() {
        _log($"Removing sms (<b>{phoneNumber}</b>)");
        OneSignal.Default.User.RemoveSms(phoneNumber);
    }

    public void SetLanguage() {
        _log($"Setting language for the user to (<b>{language}</b>)");
        OneSignal.Default.User.Language = language;
    }

    public void AddAlias() {
        _log($"Adding alias with label <b>{aliasKey}</b> and id <b>{aliasValue}</b>");
        OneSignal.Default.User.AddAlias(aliasKey, aliasValue);
    }

    public void RemoveAlias() {
        _log($"Removing alias with label <b>{aliasKey}</b>");
        OneSignal.Default.User.RemoveAlias(aliasKey);
    }

    /*
     * Push
     */

    public async void PromptForPush() {
        _log("Opening permission prompt for push notifications and awaiting result...");

        var result = await OneSignal.Default.Notifications.RequestPermissionAsync(true);

        if (result)
            _log("Notification permission accepeted");
        else
            _log("Notification permission denied");

        _log($"Notification permission is: {OneSignal.Default.Notifications.Permission}");
    }

    public void ClearPush() {
        _log("Clearing existing OneSignal push notifications");
        
        OneSignal.Default.Notifications.ClearAllNotifications();
        
        _log("Notifications cleared");
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

        var result = await OneSignal.Default.Location.RequestPermissionAsync();

        if (result)
            _log("Location permission accepeted");
        else
            _log("Location permission denied");
    }

    public void ToggleShareLocation() {
        _log($"Toggling Location IsShared to <b>{!OneSignal.Default.Location.IsShared}</b>");
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

    public async void EnterLiveActivityAsync() {
        _log($"Entering Live Activity with id: <b>{liveActivityId}</b> and token: <b>{liveActivityToken}</b> and awaiting result...");

        var result = await OneSignal.Default.LiveActivities.EnterAsync(liveActivityId, liveActivityToken);

        if (result)
            _log("Live Activity enter success");
        else
            _log("Live Activity enter failed");
    }

    public async void ExitLiveActivityAsync() {
        _log($"Exiting Live Activity with id: <b>{liveActivityId}</b> and awaiting result...");

        var result = await OneSignal.Default.LiveActivities.ExitAsync(liveActivityId);

        if (result)
            _log("Live Activity exit success");
        else
            _log("Live Activity exit failed");
    }

#region Rendering
    /*
     * You can safely ignore everything in this region and below
     */

    public Text console;
    public Text appIdText;

    public void SetAppIdString(string newVal) => appId = newVal;

    public void SetExternalIdString(string newVal) => externalId = newVal;
    public void SetEmailString(string newVal) => email = newVal;
    public void SetPhoneNumberString(string newVal) => phoneNumber = newVal;
    public void SetLanguageString(string newVal) => language = newVal;

    public void SetAliasKey(string newVal) => aliasKey = newVal;
    public void SetAliasValue(string newVal) => aliasValue = newVal;

    public void SetTriggerKey(string newVal) => triggerKey = newVal;
    public void SetTriggerValue(string newVal) => triggerValue = newVal;

    public void SetTagKey(string newVal) => tagKey = newVal;
    public void SetTagValue(string newVal) => tagValue = newVal;

    public void SetOutcomeKey(string newVal) => outcomeKey = newVal;
    public void SetOutcomeValue(string newVal) => outcomeValue = Convert.ToSingle(newVal);

    public void SetLiveActivityId(string newVal) => liveActivityId = newVal;
    public void SetLiveActivityToken(string newVal) => liveActivityToken = newVal;

    private void Awake() {
        SDKDebug.LogIntercept   += _log;
        SDKDebug.WarnIntercept  += _warn;
        SDKDebug.ErrorIntercept += _error;
        appIdText.text = appId;
    }

    private void _log(object message) {
        string green = "#3BB674";
        UnityEngine.Debug.Log(message);
        console.text += $"\n<color={green}><b>I></b></color> {message}";
    }

    private void _warn(object message) {
        string yellow = "#FFA940";
        UnityEngine.Debug.LogWarning(message);
        console.text += $"\n<color={yellow}><b>W></b></color> {message}";
    }

    private void _error(object message) {
        string red = "#E54B4D";
        UnityEngine.Debug.LogError(message);
        console.text += $"\n<color={red}><b>E></b></color> {message}";
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