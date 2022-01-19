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
using System.Collections.Generic;
using OneSignalSDK;
using UnityEngine;
using UnityEngine.UI;

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
    /// <see cref="OneSignal.PrivacyConsent"/> in this test MonoBehaviour
    /// </summary>
    public bool requiresUserPrivacyConsent;

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
        OneSignal.Default.LogLevel   = LogLevel.Info;
        OneSignal.Default.AlertLevel = LogLevel.Fatal;

        // Setting RequiresPrivacyConsent to true will prevent the OneSignalSDK from operating until
        // PrivacyConsent is also set to true
        OneSignal.Default.RequiresPrivacyConsent = requiresUserPrivacyConsent;

        // Setup the below to listen for and respond to events from notifications
        OneSignal.Default.NotificationOpened   += _notificationOpened;
        OneSignal.Default.NotificationWillShow += _notificationReceived;

        // Setup the below to listen for and respond to events from in app messages
        OneSignal.Default.InAppMessageWillDisplay     += _iamWillDisplay;
        OneSignal.Default.InAppMessageDidDisplay      += _iamDidDisplay;
        OneSignal.Default.InAppMessageWillDismiss     += _iamWillDismiss;
        OneSignal.Default.InAppMessageDidDismiss      += _iamDidDismiss;
        OneSignal.Default.InAppMessageTriggeredAction += _iamTriggeredAction;

        // Setup the below to listen for and respond to state changes
        OneSignal.Default.NotificationPermissionChanged += _notificationPermissionChanged;
        OneSignal.Default.PushSubscriptionStateChanged  += _pushStateChanged;
        OneSignal.Default.EmailSubscriptionStateChanged += _emailStateChanged;
        OneSignal.Default.SMSSubscriptionStateChanged   += _smsStateChanged;
    }

    /*
     * SDK events
     */

    private void _notificationOpened(NotificationOpenedResult result) {
        _log($"Notification was opened with result: {JsonUtility.ToJson(result)}");
    }

    private Notification _notificationReceived(Notification notification) {
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

    private void _iamTriggeredAction(InAppMessageAction inAppMessageAction) {
        _log($"IAM triggered action: {JsonUtility.ToJson(inAppMessageAction)}");
    }

    private void _notificationPermissionChanged(NotificationPermission current, NotificationPermission previous) {
        _log($"Notification Permissions changed to: {current}");
    }

    private void _pushStateChanged(PushSubscriptionState current, PushSubscriptionState previous) {
        _log($"Push state changed to: {JsonUtility.ToJson(current)}");
    }

    private void _emailStateChanged(EmailSubscriptionState current, EmailSubscriptionState previous) {
        _log($"Email state changed to: {JsonUtility.ToJson(current)}");
    }

    private void _smsStateChanged(SMSSubscriptionState current, SMSSubscriptionState previous) {
        _log($"SMS state changed to: {JsonUtility.ToJson(current)}");
    }

    /*
     * SDK setup
     */

    public void Initialize() {
        _log($"Initializing with appId <b>{appId}</b>");
        OneSignal.Default.Initialize(appId);
    }

    public void ToggleRequiresPrivacyConsent() {
        _log($"Toggling RequiresPrivacyConsent to <b>{!OneSignal.Default.RequiresPrivacyConsent}</b>");
        OneSignal.Default.RequiresPrivacyConsent = !OneSignal.Default.RequiresPrivacyConsent;
    }

    public void TogglePrivacyConsent() {
        _log($"Toggling PrivacyConsent to <b>{!OneSignal.Default.PrivacyConsent}</b>");
        OneSignal.Default.PrivacyConsent = !OneSignal.Default.PrivacyConsent;
    }

    public void SetLogLevel() {
        var newLevel = _nextEnum(OneSignal.Default.LogLevel);
        _log($"Setting LogLevel to <b>{newLevel}</b>");

        // LogLevel uses the standard Unity LogType
        OneSignal.Default.LogLevel = newLevel;
    }

    public void SetAlertLevel() {
        var newLevel = _nextEnum(OneSignal.Default.AlertLevel);
        _log($"Setting AlertLevel to <b>{newLevel}</b>");

        // AlertLevel uses the standard Unity LogType
        OneSignal.Default.AlertLevel = newLevel;
    }

    /*
     * User identification
     */

    public async void SetEmail() {
        _log($"Calling SetEmail(<b>{email}</b>) and awaiting result...");

        var result = await OneSignal.Default.SetEmail(email);

        if (result)
            _log("Set succeeded");
        else
            _error("Set failed");
    }

    public async void SetExternalId() {
        _log($"Calling SetExternalUserId(<b>{externalId}</b>) and awaiting result...");

        var result = await OneSignal.Default.SetExternalUserId(externalId);

        if (result)
            _log("Set succeeded");
        else
            _error("Set failed");
    }

    public async void SetSMSNumber() {
        _log($"Calling SetSMSNumber(<b>{phoneNumber}</b>) and awaiting result...");

        var result = await OneSignal.Default.SetSMSNumber(phoneNumber);

        if (result)
            _log("Set succeeded");
        else
            _error("Set failed");
    }

    /*
     * Push
     */

    public async void PromptForPush() {
        _log("Calling PromptForPushNotificationsWithUserResponse and awaiting result...");

        var result = await OneSignal.Default.PromptForPushNotificationsWithUserResponse();

        _log($"Prompt completed with <b>{result}</b>");
    }

    public void ClearPush() {
        _log("Clearing existing OneSignal push notifications...");
        OneSignal.Default.ClearOneSignalNotifications();
    }

    public async void SendPushToSelf() {
        _log("Sending push notification to this device via PostNotification...");

        // Check out our API docs at https://documentation.onesignal.com/reference/create-notification
        // for a full list of possibilities for notification options.
        var pushOptions = new Dictionary<string, object> {
            ["contents"] = new Dictionary<string, string> {
                ["en"] = "Test Message"
            },

            // Send notification to this user
            ["include_external_user_ids"] = new List<string> { externalId },

            // Example of scheduling a notification in the future
            ["send_after"] = DateTime.Now.ToUniversalTime().AddSeconds(30).ToString("U")
        };

        var result = await OneSignal.Default.PostNotification(pushOptions);

        if (Json.Serialize(result) is string resultString)
            _log($"Notification sent with result <b>{resultString}</b>");
        else
            _error("Could not serialize result of PostNotification");
    }

    /*
     * In App Messages
     */

    public void SetTrigger() {
        _log($"Setting trigger with key <b>{triggerKey}</b> and value <b>{triggerValue}</b>");
        OneSignal.Default.SetTrigger(triggerKey, triggerValue);
    }

    public void GetTrigger() {
        _log($"Getting trigger for key <b>{triggerKey}</b>");
        var value = OneSignal.Default.GetTrigger(triggerKey);
        _log($"Trigger for key <b>{triggerKey}</b> is of value <b>{value}</b>");
    }

    public void RemoveTrigger() {
        _log($"Removing trigger for key <b>{triggerKey}</b>");
        OneSignal.Default.RemoveTrigger(triggerKey);
    }

    public void GetTriggers() {
        _log("Getting all trigger keys and values");
        var triggers = OneSignal.Default.GetTriggers();

        if (Json.Serialize(triggers) is string triggersString)
            _log($"Current triggers are <b>{triggersString}</b>");
        else
            _error("Could not serialize triggers");
    }

    public void ToggleInAppMessagesArePaused() {
        _log($"Toggling InAppMessagesArePaused to <b>{!OneSignal.Default.InAppMessagesArePaused}</b>");
        OneSignal.Default.InAppMessagesArePaused = !OneSignal.Default.InAppMessagesArePaused;
    }

    /*
     * Tags
     */

    public async void SetTag() {
        _log($"Setting tag with key <b>{tagKey}</b> and value <b>{tagValue}</b> and awaiting result...");

        var result = await OneSignal.Default.SendTag(tagKey, tagValue);

        if (result)
            _log("Set succeeded");
        else
            _error("Set failed");
    }

    public async void RemoveTag() {
        _log($"Removing tag for key <b>{triggerKey}</b> and awaiting result...");

        var result = await OneSignal.Default.DeleteTag(tagKey);

        if (result)
            _log("Remove succeeded");
        else
            _error("Remove failed");
    }

    public async void GetTags() {
        _log("Requesting all tag keys and values for this user...");
        var tags = await OneSignal.Default.GetTags();

        if (Json.Serialize(tags) is string tagsString)
            _log($"Current tags are <b>{tagsString}</b>");
        else
            _error("Could not serialize tags");
    }

    /*
     * Outcomes
     */

    public async void SetOutcome() {
        _log($"Setting outcome with key <b>{outcomeKey}</b> and awaiting result...");

        var result = await OneSignal.Default.SendOutcome(outcomeKey);

        if (result)
            _log("Set succeeded");
        else
            _error("Set failed");
    }

    public async void SetUniqueOutcome() {
        _log($"Setting unique outcome with key <b>{outcomeKey}</b> and awaiting result...");

        var result = await OneSignal.Default.SendUniqueOutcome(outcomeKey);

        if (result)
            _log("Set succeeded");
        else
            _error("Set failed");
    }

    public async void SetOutcomeWithValue() {
        _log($"Setting outcome with key <b>{outcomeKey}</b> and value <b>{outcomeValue}</b> and awaiting result...");

        var result = await OneSignal.Default.SendOutcomeWithValue(outcomeKey, outcomeValue);

        if (result)
            _log("Set succeeded");
        else
            _error("Set failed");
    }

    /*
     * Location
     */

    public void PromptLocation() {
        _log("Opening prompt to ask for user consent to access location");
        OneSignal.Default.PromptLocation();
    }

    public void ToggleShareLocation() {
        _log($"Toggling ShareLocation to <b>{!OneSignal.Default.ShareLocation}</b>");
        OneSignal.Default.ShareLocation = !OneSignal.Default.ShareLocation;
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
        Debug.Log(message);
        console.text += $"\n<color=green><b>I></b></color> {message}";
    }

    private void _warn(object message) {
        Debug.LogWarning(message);
        console.text += $"\n<color=orange><b>W></b></color> {message}";
    }

    private void _error(object message) {
        Debug.LogError(message);
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