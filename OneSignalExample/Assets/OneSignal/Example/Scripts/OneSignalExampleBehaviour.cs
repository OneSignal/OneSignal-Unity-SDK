#if ONE_SIGNAL_INSTALLED
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

using UnityEngine;
using System.Collections.Generic;
using System;
using OneSignalSDK;

public class OneSignalExampleBehaviour : MonoBehaviour {

    /// <summary>
    /// set to an email address you would like to test notifications against
    /// </summary>
    public string email = "EMAIL_ADDRESS";

    /// <summary>
    /// set to an external user id you would like to test notifications against
    /// </summary>
    public string externalId = "EXTERNAL_USER_ID";

    /// <summary>
    /// set to your app id (https://documentation.onesignal.com/docs/accounts-and-keys)
    /// </summary>
    public string appId = "ONESIGNAL_APP_ID";

        // Enable lines below to debug issues with OneSignal. (logLevel, visualLogLevel)
        OneSignal.Default.LogLevel   = LogType.Log;
        OneSignal.Default.AlertLevel = LogType.Exception;

        // If you set to true, the user will have to provide consent
        // using OneSignal.UserDidProvideConsent(true) before the
        // SDK will initialize
        OneSignal.Default.RequiresPrivacyConsent = requiresUserPrivacyConsent;

        // The only required method you need to call to setup OneSignal to receive push notifications.
        // Call before using any other methods on OneSignal (except setLogLevel or SetRequiredUserPrivacyConsent)
        // Should only be called once when your app is loaded.
        OneSignal.Default.NotificationReceived += notification => { };
        OneSignal.Default.NotificationOpened   += notification => { };
        OneSignal.Default.InAppMessageClicked  += iamAction => { };

        OneSignal.Default.Initialize("99015f5e-87b1-462e-a75b-f99bf7c2822e");

        // todo - what happened to this?
        // OneSignal.inFocusDisplayType = OneSignal.OSInFocusDisplayOption.Notification;

        OneSignal.Default.PermissionStateChanged        += (current, previous) => { };
        OneSignal.Default.SubscriptionStateChanged      += (current, previous) => { };
        OneSignal.Default.EmailSubscriptionStateChanged += (current, previous) => { };

        // todo - get current state
        // var pushState = OneSignal.GetPermissionSubscriptionState();

        OneSignalInAppMessageTriggerExamples();
        OneSignalOutcomeEventsExamples();
    }

    // Examples of using OneSignal External User Id
    private void OneSignalExternalUserIdCallback(Dictionary<string, object> results) {
        // The results will contain push and email success statuses
        print($"External user id updated with results: {Json.Serialize(results)}");

        // Push can be expected in almost every situation with a success status, but
        // as a pre-caution its good to verify it exists
        if (results.ContainsKey("push")) {
            var pushStatusDict = results["push"] as Dictionary<string, object>;

            if (pushStatusDict.ContainsKey("success")) {
                Console.WriteLine(
                    "External user id updated for push with results: " + pushStatusDict["success"] as string);
            }
        }

        // Verify the email is set or check that the results have an email success status
        if (results.ContainsKey("email")) {
            var emailStatusDict = results["email"] as Dictionary<string, object>;

            if (emailStatusDict.ContainsKey("success")) {
                Console.WriteLine(
                    "External user id updated for email with results: " + emailStatusDict["success"] as string);
            }
        }
    }

    private void OneSignalExternalUserIdCallbackFailure(Dictionary<string, object> error) {
        // The results will contain push and email success statuses
        Console.WriteLine("External user id failed with error: " + Json.Serialize(error));
    }

    /// <summary>
    /// Examples of using OneSignal In-App Message triggers
    /// https://documentation.onesignal.com/docs/in-app-message-examples
    /// </summary>
    private static void OneSignalInAppMessageTriggerExamples() {
        // Add a single trigger
        OneSignal.Default.AddTrigger("key", "value");

        // Get the current value to a trigger by key
        var triggerKey   = "key";
        var triggerValue = OneSignal.Default.GetTriggerValueForKey(triggerKey);
        Console.WriteLine($"Trigger key: {triggerKey} value: {triggerValue}");

        // Add multiple triggers
        OneSignal.Default.AddTriggers(new Dictionary<string, object>() { { "key1", "value1" }, { "key2", 2 } });

        // Delete a trigger
        OneSignal.Default.RemoveTriggerForKey("key");

        // Delete a list of triggers
        OneSignal.Default.RemoveTriggersForKeys(new List<string>() { "key1", "key2" });

        // Temporarily pause In-App messages; If true is passed in.
        // Great to ensure you never interrupt your user while they are in the middle of a match in your game.
        OneSignal.Default.InAppMessagesArePaused = true;
    }

    private void OneSignalOutcomeEventsExamples() {
        OneSignal.Default.SendOutcome("normal_1");
        
        // todo - this
        //OneSignal.Default.SendOutcome("normal_2", (OutcomeEvent outcomeEvent) => { printOutcomeEvent(outcomeEvent); });

        OneSignal.Default.SendUniqueOutcome("unique_1");
        
        // todo - this
        //OneSignal.Default.SendUniqueOutcome("unique_2", (OutcomeEvent outcomeEvent) => { printOutcomeEvent(outcomeEvent); });

        OneSignal.Default.SendOutcomeWithValue("value_1", 3.2f);
        
        // todo - this
        //OneSignal.Default.SendOutcomeWithValue("value_2", 3.2f,
            // (OutcomeEvent outcomeEvent) => { printOutcomeEvent(outcomeEvent); });
    }

    private void printOutcomeEvent(OutcomeEvent outcomeEvent) {
        Console.WriteLine(outcomeEvent.session + "\n" +
            string.Join(", ", outcomeEvent.notificationIds) + "\n" +
            outcomeEvent.name + "\n" +
            outcomeEvent.timestamp + "\n" +
            outcomeEvent.weight);
    }

    // Called when your app is in focus and a notification is received.
    // The name of the method can be anything as long as the signature matches.
    // Method must be static or this object should be marked as DontDestroyOnLoad
    private static void HandleNotificationReceived(Notification notification) {
        var payload = notification.Payload;
        string                message = payload.body;

        print("GameControllerExample:HandleNotificationReceived: " + message);
        print("displayType: " + notification.DisplayType);
        extraMessage = "Notification received with text: " + message;

<<<<<<< HEAD
        if (payload.additionalData == null)
            print("[HandleNotificationReceived] Additional Data == null");
        else if (Json.Serialize(payload.additionalData) is string dataString)
            print($"[HandleNotificationReceived] message {message}, additionalData: {dataString}");
=======
        Dictionary<string, object> additionalData = payload.additionalData;
        if (additionalData == null)
            Debug.Log("[HandleNotificationReceived] Additional Data == null");
>>>>>>> 8187a27 (Heavy WIP on core SDK and public interface)
        else
            Debug.Log("[HandleNotificationReceived] message " + message + ", additionalData: " +
                Json.Serialize(additionalData) as string);
    }

    // Called when a notification is opened.
    // The name of the method can be anything as long as the signature matches.
    // Method must be static or this object should be marked as DontDestroyOnLoad
    public static void HandleNotificationOpened(NotificationOpenedResult result) {
        var payload  = result.notification.Payload;
        var message  = payload.body;
        var actionID = result.action.ActionId;

        print("GameControllerExample:HandleNotificationOpened: " + message);
<<<<<<< HEAD
        _logMessage = "Notification opened with text: " + message;

        if (payload.additionalData == null)
            print("[HandleNotificationOpened] Additional Data == null");
        else if (Json.Serialize(payload.additionalData) is string dataString)
            print($"[HandleNotificationOpened] message {message}, additionalData: {dataString}");
        else
            print("[HandleNotificationOpened] Additional Data could not be serialized");
=======
        extraMessage = "Notification opened with text: " + message;

        var additionalData = payload.additionalData;
        if (additionalData == null)
            Debug.Log("[HandleNotificationOpened] Additional Data == null");
        else {
            Debug.Log("[HandleNotificationOpened] message " + message + ", additionalData: " +
                Json.Serialize(additionalData) as string);
        }
>>>>>>> 8187a27 (Heavy WIP on core SDK and public interface)

        if (actionID != null) {
            // actionSelected equals the id on the button the user pressed.
            // actionSelected will equal "__DEFAULT__" when the notification itself was tapped when buttons were present.
            _logMessage = "Pressed ButtonId: " + actionID;
        }
    }

    public static void HandlerInAppMessageClicked(InAppMessageAction action) {
        var logInAppClickEvent = "In-App Message Clicked: " +
            "\nClick Name: " + action.ClickName +
            "\nClick Url: " + action.ClickUrl +
            "\nFirst Click: " + action.FirstClick +
            "\nCloses Message: " + action.ClosesMessage;

        print(logInAppClickEvent);
        _logMessage = logInAppClickEvent;
    }

    // Test Menu
    // Includes SendTag/SendTags, getting the userID and pushToken, and scheduling an example notification
    private void OnGUI() {
        var customTextSize = new GUIStyle("button") {
            fontSize = 30
        };

        var guiBoxStyle = new GUIStyle("box") {
            fontSize = 30
        };

        var textFieldStyle = new GUIStyle("textField") {
            fontSize = 30
        };

        float itemOriginX      = 50.0f;
        float itemWidth        = Screen.width - 120.0f;
        float boxWidth         = Screen.width - 20.0f;
        float boxOriginY       = 120.0f;
        float boxHeight        = requiresUserPrivacyConsent ? 980.0f : 890.0f;
        float itemStartY       = 200.0f;
        float itemHeightOffset = 90.0f;
        float itemHeight       = 60.0f;

    private static string _logMessage;

    private GUIStyle _customTextSize;
    private GUIStyle _guiBoxStyle;

<<<<<<< HEAD
    private static float ItemWidth => Screen.width - 120.0f;
    private static float BoxWidth => Screen.width - 20.0f;
    private float BoxHeight => requiresUserPrivacyConsent ? 980.0f : 890.0f;

    private Rect MainMenuRect => new Rect(10, BoxOriginY, BoxWidth, BoxHeight);

    private static Rect ItemRect(ref int position) => new Rect(
        ItemOriginX,
        ItemStartY + position++ * ItemHeightOffset,
        ItemWidth,
        ItemHeight
    );

    private bool MenuButton(ref int position, string label)
        => GUI.Button(ItemRect(ref position), label, _customTextSize);

    // Test Menu
    // Includes SendTag/SendTags, getting the userID and pushToken, and scheduling an example notification
    private void OnGUI() {
        _customTextSize = _customTextSize ?? new GUIStyle(GUI.skin.button) {
            fontSize = 30
        };
        
        _guiBoxStyle = _guiBoxStyle ?? new GUIStyle(GUI.skin.box) {
            fontSize  = 30,
            alignment = TextAnchor.UpperLeft,
            wordWrap  = true
        };

        GUI.Box(MainMenuRect, "Test Menu", _guiBoxStyle);

        int position = 0;

        if (MenuButton(ref position, "Send Example Tags")) {
=======
        if (GUI.Button(new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight),
            "SendTags", customTextSize)) {
>>>>>>> 8187a27 (Heavy WIP on core SDK and public interface)
            // You can tags users with key value pairs like this:
            OneSignal.Default.SendTag("UnityTestKey", "TestValue");

            // Or use an IDictionary if you need to set more than one tag.
            OneSignal.Default.SendTags(new Dictionary<string, string>()
                { { "UnityTestKey2", "value2" }, { "UnityTestKey3", "value3" } });

            // You can delete a single tag with it's key.
            // OneSignal.DeleteTag("UnityTestKey");
            // Or delete many with an IList.
            // OneSignal.DeleteTags(new List<string>() {"UnityTestKey2", "UnityTestKey3" });
        }

        count++;

        if (GUI.Button(new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight), "GetIds",
            customTextSize)) {
            // todo - substitute with getDeviceState
            // OneSignal.IdsAvailable((userId, pushToken) => {
            //     extraMessage = "UserID:\n" + userId + "\n\nPushToken:\n" + pushToken;
            // });
        }

        if (MenuButton(ref position, "Test Notification")) {
            _logMessage = "Waiting to get a OneSignal userId. Uncomment OneSignal.SetLogLevel in the Start method if " +
                "it hangs here to debug the issue.";

        if (GUI.Button(new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight),
            "TestNotification", customTextSize)) {
            extraMessage
                = "Waiting to get a OneSignal userId. Uncomment OneSignal.SetLogLevel in the Start method if it hangs here to debug the issue.";

            // todo - substitute with getDeviceState
            // OneSignal.IdsAvailable((userId, pushToken) => {
            //     if (pushToken != null) {
            //         // See https://documentation.onesignal.com/reference/create-notification for a full list of options.
            //         // You can not use included_segments or any fields that require your OneSignal 'REST API Key' in your app for security reasons.
            //         // If you need to use your OneSignal 'REST API Key' you will need your own server where you can make this call.
            //
            //         var notification = new Dictionary<string, object>();
            //         notification["contents"] = new Dictionary<string, string>() { { "en", "Test Message" } };
            //
            //         // Send notification to this device.
            //         notification["include_player_ids"] = new List<string>() { userId };
            //
            //         // Example of scheduling a notification in the future.
            //         //notification["send_after"] = System.DateTime.Now.ToUniversalTime().AddSeconds(30).ToString("U");
            //
            //         extraMessage = "Posting test notification now.";
            //
            //         OneSignal.PostNotification(notification,
            //             (responseSuccess) => {
            //                 extraMessage
            //                     = "Notification posted successful! Delayed by about 30 seconds to give you time to press the home button to see a notification vs an in-app alert.\n" +
            //                     Json.Serialize(responseSuccess);
            //             },
            //             (responseFailure) => {
            //                 extraMessage = "Notification failed to post:\n" + Json.Serialize(responseFailure);
            //             });
            //     }
            //     else {
            //         extraMessage = "ERROR: Device is not registered.";
            //     }
            // });
        }

        email = GUI.TextField(ItemRect(ref position), email, _customTextSize);

        email = GUI.TextField(new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight),
            email, customTextSize);

        count++;

        if (GUI.Button(new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight),
            "SetEmail", customTextSize)) {
            extraMessage = "Setting email to " + email;

            // todo - this
            // OneSignal.SetEmail(email, () => { Debug.Log("Successfully set email"); },
                // (error) => { Debug.Log("Encountered error setting email: " + Json.Serialize(error)); });
        }

        if (MenuButton(ref position, "Logout Email")) {
            _logMessage = "Logging Out of example@example.com";

        if (GUI.Button(new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight),
            "LogoutEmail", customTextSize)) {
            extraMessage = "Logging Out of example@example.com";

            // todo - this
            // OneSignal.LogoutEmail(() => { Debug.Log("Successfully logged out of email"); },
            //     (error) => { Debug.Log("Encountered error logging out of email: " + Json.Serialize(error)); });
        }

        externalId = GUI.TextField(ItemRect(ref position), externalId, _customTextSize);

        externalId = GUI.TextField(
            new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight), externalId,
            customTextSize);

        if (MenuButton(ref position, "Remove External Id"))
            OneSignal.RemoveExternalUserId(OnUpdatedExternalUserId);

        if (GUI.Button(new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight),
            "SetExternalId", customTextSize)) {
            
            // todo - this
            // OneSignal.SetExternalUserId(externalId, OneSignalExternalUserIdCallback);

            // Auth external id method
            // OneSignal.SetExternalUserId(externalId, "your_auth_hash_token", OneSignalExternalUserIdCallback, OneSignalExternalUserIdCallbackFailure);
        }

            if (GUI.Button(ItemRect(ref position), consentText, _customTextSize)) {
                _logMessage = "Providing user privacy consent";

        if (GUI.Button(new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight),
            "RemoveExternalId", customTextSize)) {
            // todo - this
            // OneSignal.RemoveExternalUserId(OneSignalExternalUserIdCallback);
        }

        if (requiresUserPrivacyConsent) {
            count++;

            // todo - this
            // if (GUI.Button(new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight),
            //     (OneSignal.UserProvidedConsent() ? "Revoke Privacy Consent" : "Provide Privacy Consent"),
            //     customTextSize)) {
            //     extraMessage = "Providing user privacy consent";
            //
            //     OneSignal.UserDidProvideConsent(!OneSignal.UserProvidedConsent());
            // }
        }

        if (extraMessage != null) {
            guiBoxStyle.alignment = TextAnchor.UpperLeft;
            guiBoxStyle.wordWrap  = true;
            GUI.Box(
                new Rect(10, boxOriginY + boxHeight + 20, Screen.width - 20,
                    Screen.height - (boxOriginY + boxHeight + 40)), extraMessage, guiBoxStyle);
        }
    }

    private static void printError(object message) => Debug.LogError(message);
}
#endif