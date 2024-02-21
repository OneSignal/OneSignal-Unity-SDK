/*
 * Modified MIT License
 *
 * Copyright 2023 OneSignal
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
using OneSignalSDK.Notifications;
using OneSignalSDK.InAppMessages;
using OneSignalSDK.User;
using OneSignalSDK.User.Models;
using OneSignalSDK.Debug.Models;
using OneSignalSDK.Debug.Utilities;

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
    /// set to your app id (https://documentation.onesignal.com/docs/keys-and-ids)
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
        OneSignal.Debug.LogLevel = LogLevel.Info;
        OneSignal.Debug.AlertLevel = LogLevel.Fatal;

        _log($"Initializing with appId <b>{appId}</b>");
        OneSignal.Initialize(appId);

        // Setting ConsentRequired to true will prevent the OneSignalSDK from operating until
        // PrivacyConsent is also set to true
        OneSignal.ConsentRequired = consentRequired;

        // Setup the below to listen for and respond to events from notifications
        OneSignal.Notifications.Clicked += _notificationOnClick;
        OneSignal.Notifications.ForegroundWillDisplay += _notificationOnDisplay;
        OneSignal.Notifications.PermissionChanged += _notificationPermissionChanged;

        // Setup the below to listen for and respond to events from in-app messages
        OneSignal.InAppMessages.WillDisplay += _iamWillDisplay;
        OneSignal.InAppMessages.DidDisplay += _iamDidDisplay;
        OneSignal.InAppMessages.WillDismiss += _iamWillDismiss;
        OneSignal.InAppMessages.DidDismiss += _iamDidDismiss;
        OneSignal.InAppMessages.Clicked += _iamOnClick;

        // Setup the below to listen for and respond to state changes
        OneSignal.User.PushSubscription.Changed += _pushSubscriptionChanged;
        OneSignal.User.Changed += _userStateChanged;
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

    private void _iamWillDisplay(object sender, InAppMessageWillDisplayEventArgs e) {
        _log($"IAM will display: {JsonUtility.ToJson(e.Message)}");
    }

    private void _iamDidDisplay(object sender, InAppMessageDidDisplayEventArgs e) {
        _log($"IAM did display: {JsonUtility.ToJson(e.Message)}");
    }

    private void _iamWillDismiss(object sender, InAppMessageWillDismissEventArgs e) {
        _log($"IAM will dismiss: {JsonUtility.ToJson(e.Message)}");
    }

    private void _iamDidDismiss(object sender, InAppMessageDidDismissEventArgs e) {
        _log($"IAM did dismiss: {JsonUtility.ToJson(e.Message)}");
    }

    private void _iamOnClick(object sender, InAppMessageClickEventArgs e) {
        _log($"IAM was clicked with Message: {JsonUtility.ToJson(e.Message)}");
        _log($"IAM was clicked with Result: {JsonUtility.ToJson(e.Result)}");
        _log($"IAM was clicked with Result UrlTarget: " + e.Result.UrlTarget.ToString());
    }

    private void _pushSubscriptionChanged(object sender, PushSubscriptionChangedEventArgs e) {
        _log($"Push subscription changed from previous: {JsonUtility.ToJson(e.State.Previous)}");
        _log($"Push subscription changed to current: {JsonUtility.ToJson(e.State.Current)}");
    }

    private void _userStateChanged(object sender, UserStateChangedEventArgs e) {
        _log($"OneSignalId changed : {e.State.Current.OneSignalId}");
        _log($"ExternalId changed : {e.State.Current.ExternalId}");
    }

    /*
     * SDK setup
     */

    public void ToggleConsentRequired() {
        consentRequired = !consentRequired;
        _log($"Toggling ConsentRequired to <b>{consentRequired}</b>");
        OneSignal.ConsentRequired = consentRequired;
    }

    public void ToggleConsentGiven() {
        consentGiven = !consentGiven;
        _log($"Toggling ConsentGiven to <b>{consentGiven}</b>");
        OneSignal.ConsentGiven = consentGiven;
    }

    public void SetLogLevel() {
        var newLevel = _nextEnum(OneSignal.Debug.LogLevel);
        _log($"Setting LogLevel to <b>{newLevel}</b>");
        OneSignal.Debug.LogLevel = newLevel;
    }

    public void SetAlertLevel() {
        var newLevel = _nextEnum(OneSignal.Debug.AlertLevel);
        _log($"Setting AlertLevel to <b>{newLevel}</b>");
        OneSignal.Debug.AlertLevel = newLevel;
    }

    /*
     * User identification
     */

    public void LoginOneSignalUser() {
        _log($"Logging in user (<b>{externalId}</b>)");
        OneSignal.Login(externalId);
    }
    
    public void LogoutOneSignalUser() {
        _log($"Logging out user");
        OneSignal.Logout();
    }

    public void PushSubscriptionOptIn() {
        _log($"Opting in push subscription");
        OneSignal.User.PushSubscription.OptIn();
    }

    public void PushSubscriptionOptOut() {
        _log($"Opting out push subscription");
        OneSignal.User.PushSubscription.OptOut();
    }

    public void AddEmail() {
        _log($"Adding email (<b>{email}</b>)");
        OneSignal.User.AddEmail(email);
    }
    
    public void RemoveEmail() {
        _log($"Removing email (<b>{email}</b>)");
        OneSignal.User.RemoveEmail(email);
    }

    public void AddSms() {
        _log($"Adding sms (<b>{phoneNumber}</b>)");
        OneSignal.User.AddSms(phoneNumber);
    }
    
    public void RemoveSms() {
        _log($"Removing sms (<b>{phoneNumber}</b>)");
        OneSignal.User.RemoveSms(phoneNumber);
    }

    public void SetLanguage() {
        _log($"Setting language for the user to (<b>{language}</b>)");
        OneSignal.User.Language = language;
    }

    public void AddAlias() {
        _log($"Adding alias with label <b>{aliasKey}</b> and id <b>{aliasValue}</b>");
        OneSignal.User.AddAlias(aliasKey, aliasValue);
    }

    public void RemoveAlias() {
        _log($"Removing alias with label <b>{aliasKey}</b>");
        OneSignal.User.RemoveAlias(aliasKey);
    }

    public void GetOneSignalId() {
        string onesignalId = OneSignal.User.OneSignalId ?? "null";
        _log($"Get OneSignalId <b>{onesignalId}</b>");
    }

    public void GetExternalId() {
        string externalId = OneSignal.User.ExternalId ?? "null";
        _log($"Get ExternalId <b>{externalId}</b>");
    }

    /*
     * Push
     */

    public async void PromptForPush() {
        _log($"Can request push notification permission: {OneSignal.Notifications.CanRequestPermission}");

        _log("Opening permission prompt for push notifications and awaiting result...");

        var result = await OneSignal.Notifications.RequestPermissionAsync(true);

        if (result)
            _log("Notification permission accepeted");
        else
            _log("Notification permission denied");
    }

    public void ClearPush() {
        _log("Clearing existing OneSignal push notifications");
        
        OneSignal.Notifications.ClearAllNotifications();
        
        _log("Notifications cleared");
    }

    public void GetNotificationsPermission() {
        var permission = OneSignal.Notifications.Permission;

        _log($"Notifications permission is: <b>{permission}</b>");
    }

    public void GetNotificationsPermissionNative() {
        var permissionNative = OneSignal.Notifications.PermissionNative;

        _log($"Notifications native permission is: <b>{permissionNative.ToString()}</b>");
    }

    /*
     * In-App Messages
     */

    public void AddTrigger() {
        _log($"Adding trigger with key <b>{triggerKey}</b> and value <b>{triggerValue}</b>");
        OneSignal.InAppMessages.AddTrigger(triggerKey, triggerValue);
    }

    public void RemoveTrigger() {
        _log($"Removing trigger for key <b>{triggerKey}</b>");
        OneSignal.InAppMessages.RemoveTrigger(triggerKey);
    }

    public void ClearTriggers() {
        _log("Clearing all trigger keys and values from user");
        OneSignal.InAppMessages.ClearTriggers();
    }

    public void TogglePauseInAppMessages() {
        _log($"Toggling Pausing InAppMessages to <b>{!OneSignal.InAppMessages.Paused}</b>");
        OneSignal.InAppMessages.Paused = !OneSignal.InAppMessages.Paused;
    }

    /*
     * Tags
     */

    public void AddTag() {
        _log($"Adding tag with key <b>{tagKey}</b> and value <b>{tagValue}</b>");
        OneSignal.User.AddTag(tagKey, tagValue);
    }

    public void RemoveTag() {
        _log($"Removing tag for key <b>{tagKey}</b>");
        OneSignal.User.RemoveTag(tagKey);
        
    }

    public void GetTags() {
        Dictionary<string, string> dict = OneSignal.User.GetTags();
        string dictionaryString = "{";
        foreach(KeyValuePair <string, string> keyValues in dict) {  
            dictionaryString += keyValues.Key + " : " + keyValues.Value + ", ";
        }
        _log($"Get all user tags " + dictionaryString.TrimEnd(',', ' ') + "}");
    }

    /*
     * Outcomes
     */

    public void AddOutcome() {
        _log($"Adding outcome with key <b>{outcomeKey}</b>");
        OneSignal.Session.AddOutcome(outcomeKey);
    }

    public void AddUniqueOutcome() {
        _log($"Adding unique outcome with key <b>{outcomeKey}</b>");
        OneSignal.Session.AddUniqueOutcome(outcomeKey);
    }

    public void AddOutcomeWithValue() {
        _log($"Adding outcome with key <b>{outcomeKey}</b> and value <b>{outcomeValue}</b>");
        OneSignal.Session.AddOutcomeWithValue(outcomeKey, outcomeValue);
    }

    /*
     * Location
     */

    public void PromptLocation() {
        _log("Opening permission prompt for location");
        OneSignal.Location.RequestPermission();
    }

    public void ToggleShareLocation() {
        _log($"Toggling Location IsShared to <b>{!OneSignal.Location.IsShared}</b>");
        OneSignal.Location.IsShared = !OneSignal.Location.IsShared;
    }

    /*
     * iOS
     */

    public async void EnterLiveActivityAsync() {
        _log($"Entering Live Activity with id: <b>{liveActivityId}</b> and token: <b>{liveActivityToken}</b> and awaiting result...");

        var result = await OneSignal.LiveActivities.EnterAsync(liveActivityId, liveActivityToken);

        if (result)
            _log("Live Activity enter success");
        else
            _log("Live Activity enter failed");
    }

    public async void ExitLiveActivityAsync() {
        _log($"Exiting Live Activity with id: <b>{liveActivityId}</b> and awaiting result...");

        var result = await OneSignal.LiveActivities.ExitAsync(liveActivityId);

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