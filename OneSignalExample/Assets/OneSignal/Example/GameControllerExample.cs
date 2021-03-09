/**
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

using OneSignalPush.MiniJSON;
using System;

public class GameControllerExample : MonoBehaviour {

    private static string extraMessage;
    public string email = "Email Address";
    public string externalId = "External User ID";

    private static bool requiresUserPrivacyConsent = false;

    void Start() {
        extraMessage = null;

        // Enable line below to debug issues with OneSignal. (logLevel, visualLogLevel)
        OneSignal.SetLogLevel(OneSignal.LOG_LEVEL.VERBOSE, OneSignal.LOG_LEVEL.NONE);

        // If you set to true, the user will have to provide consent
        // using OneSignal.UserDidProvideConsent(true) before the
        // SDK will initialize
        OneSignal.SetRequiresUserPrivacyConsent(requiresUserPrivacyConsent);

        // The only required method you need to call to setup OneSignal to receive push notifications.
        // Call before using any other methods on OneSignal (except setLogLevel or SetRequiredUserPrivacyConsent)
        // Should only be called once when your app is loaded.
        OneSignal.StartInit("99015f5e-87b1-462e-a75b-f99bf7c2822e")
            .HandleNotificationReceived(HandleNotificationReceived)
            .HandleNotificationOpened(HandleNotificationOpened)
            .HandleInAppMessageClicked(HandlerInAppMessageClicked)
            .EndInit();
      
        OneSignal.inFocusDisplayType = OneSignal.OSInFocusDisplayOption.Notification;

        OneSignal.permissionObserver += OneSignal_permissionObserver;
        OneSignal.subscriptionObserver += OneSignal_subscriptionObserver;
        OneSignal.emailSubscriptionObserver += OneSignal_emailSubscriptionObserver;

        var pushState = OneSignal.GetPermissionSubscriptionState();

        OneSignalInAppMessageTriggerExamples();
        OneSignalOutcomeEventsExamples();
    }


    // Examples of using OneSignal External User Id
    private void OneSignalExternalUserIdCallback(Dictionary<string, object> results)
    {
        // The results will contain push and email success statuses
        Console.WriteLine("External user id updated with results: " + Json.Serialize(results));

        // Push can be expected in almost every situation with a success status, but
        // as a pre-caution its good to verify it exists
        if (results.ContainsKey("push"))
        {
            Dictionary<string, object> pushStatusDict = results["push"] as Dictionary<string, object>;
            if (pushStatusDict.ContainsKey("success"))
            {
                Console.WriteLine("External user id updated for push with results: " + pushStatusDict["success"] as string);
            }
        }

        // Verify the email is set or check that the results have an email success status
        if (results.ContainsKey("email"))
        {
            Dictionary<string, object> emailStatusDict = results["email"] as Dictionary<string, object>;
            if (emailStatusDict.ContainsKey("success"))
            {
                Console.WriteLine("External user id updated for email with results: " + emailStatusDict["success"] as string);
            }
        }
    }

    private void OneSignalExternalUserIdCallbackFailure(Dictionary<string, object> error)
    {
        // The results will contain push and email success statuses
        Console.WriteLine("External user id failed with error: " + Json.Serialize(error));
    }

    // Examples of using OneSignal In-App Message triggers
    private void OneSignalInAppMessageTriggerExamples() {
        // Add a single trigger
        OneSignal.AddTrigger("key", "value");

        // Get the current value to a trigger by key
        var triggerKey = "key";
        var triggerValue = OneSignal.GetTriggerValueForKey(triggerKey);
        String output = "Trigger key: " + triggerKey + " value: " + (String) triggerValue;
        Console.WriteLine(output);

        // Add multiple triggers
        OneSignal.AddTriggers(new Dictionary<string, object>() { { "key1", "value1" }, { "key2", 2 } });

        // Delete a trigger
        OneSignal.RemoveTriggerForKey("key");

        // Delete a list of triggers
        OneSignal.RemoveTriggersForKeys(new List<string>() { "key1", "key2" });

        // Temporarily pause In-App messages; If true is passed in.
        // Great to ensure you never interrupt your user while they are in the middle of a match in your game.
        OneSignal.PauseInAppMessages(false);
   }

   private void OneSignalOutcomeEventsExamples() {
        OneSignal.SendOutcome("normal_1");
        OneSignal.SendOutcome("normal_2", (OSOutcomeEvent outcomeEvent) => {
            printOutcomeEvent(outcomeEvent);
        });

        OneSignal.SendUniqueOutcome("unique_1");
        OneSignal.SendUniqueOutcome("unique_2", (OSOutcomeEvent outcomeEvent) => {
            printOutcomeEvent(outcomeEvent);
        });

        OneSignal.SendOutcomeWithValue("value_1", 3.2f);
        OneSignal.SendOutcomeWithValue("value_2", 3.2f, (OSOutcomeEvent outcomeEvent) => {
            printOutcomeEvent(outcomeEvent);
        });
   }

    private void printOutcomeEvent(OSOutcomeEvent outcomeEvent) {
        Console.WriteLine(outcomeEvent.session + "\n" +
                string.Join(", ", outcomeEvent.notificationIds) + "\n" +
                outcomeEvent.name + "\n" +
                outcomeEvent.timestamp + "\n" +
                outcomeEvent.weight);
    }

    private void OneSignal_subscriptionObserver(OSSubscriptionStateChanges stateChanges) {
	    Debug.Log("SUBSCRIPTION stateChanges: " + stateChanges);
	    Debug.Log("SUBSCRIPTION stateChanges.to.userId: " + stateChanges.to.userId);
	    Debug.Log("SUBSCRIPTION stateChanges.to.subscribed: " + stateChanges.to.subscribed);
   }

   private void OneSignal_permissionObserver(OSPermissionStateChanges stateChanges) {
	    Debug.Log("PERMISSION stateChanges.from.status: " + stateChanges.from.status);
	    Debug.Log("PERMISSION stateChanges.to.status: " + stateChanges.to.status);
   }

    private void OneSignal_emailSubscriptionObserver(OSEmailSubscriptionStateChanges stateChanges) {
	    Debug.Log("EMAIL stateChanges.from.status: " + stateChanges.from.emailUserId + ", " + stateChanges.from.emailAddress);
	    Debug.Log("EMAIL stateChanges.to.status: " + stateChanges.to.emailUserId + ", " + stateChanges.to.emailAddress);
    }

    // Called when your app is in focus and a notification is received.
    // The name of the method can be anything as long as the signature matches.
    // Method must be static or this object should be marked as DontDestroyOnLoad
    private static void HandleNotificationReceived(OSNotification notification) {
        OSNotificationPayload payload = notification.payload;
        string message = payload.body;

        print("GameControllerExample:HandleNotificationReceived: " + message);
        print("displayType: " + notification.displayType);
        extraMessage = "Notification received with text: " + message;

        Dictionary<string, object> additionalData = payload.additionalData;
        if (additionalData == null) 
            Debug.Log ("[HandleNotificationReceived] Additional Data == null");
        else
            Debug.Log("[HandleNotificationReceived] message " + message + ", additionalData: " + Json.Serialize(additionalData) as string);
    }

    // Called when a notification is opened.
    // The name of the method can be anything as long as the signature matches.
    // Method must be static or this object should be marked as DontDestroyOnLoad
    public static void HandleNotificationOpened(OSNotificationOpenedResult result) {
        OSNotificationPayload payload = result.notification.payload;
        string message = payload.body;
        string actionID = result.action.actionID;

        print("GameControllerExample:HandleNotificationOpened: " + message);
        extraMessage = "Notification opened with text: " + message;
      
        Dictionary<string, object> additionalData = payload.additionalData;
        if (additionalData == null) 
            Debug.Log ("[HandleNotificationOpened] Additional Data == null");
        else
            Debug.Log("[HandleNotificationOpened] message " + message + ", additionalData: " + Json.Serialize(additionalData) as string);

        if (actionID != null) {
            // actionSelected equals the id on the button the user pressed.
            // actionSelected will equal "__DEFAULT__" when the notification itself was tapped when buttons were present.
            extraMessage = "Pressed ButtonId: " + actionID;
        }
    }

    public static void HandlerInAppMessageClicked(OSInAppMessageAction action) {
        String logInAppClickEvent = "In-App Message Clicked: " +
            "\nClick Name: " + action.clickName +
            "\nClick Url: " + action.clickUrl +
            "\nFirst Click: " + action.firstClick +
            "\nCloses Message: " + action.closesMessage;

        print(logInAppClickEvent);
        extraMessage = logInAppClickEvent;
    }

    // Test Menu
    // Includes SendTag/SendTags, getting the userID and pushToken, and scheduling an example notification
    void OnGUI() {
        GUIStyle customTextSize = new GUIStyle("button");
        customTextSize.fontSize = 30;

        GUIStyle guiBoxStyle = new GUIStyle("box");
        guiBoxStyle.fontSize = 30;

        GUIStyle textFieldStyle = new GUIStyle("textField");
        textFieldStyle.fontSize = 30;


        float itemOriginX = 50.0f;
        float itemWidth = Screen.width - 120.0f;
        float boxWidth = Screen.width - 20.0f;
        float boxOriginY = 120.0f;
        float boxHeight = requiresUserPrivacyConsent ? 980.0f : 890.0f;
        float itemStartY = 200.0f;
        float itemHeightOffset = 90.0f;
        float itemHeight = 60.0f;

        GUI.Box(new Rect(10, boxOriginY, boxWidth, boxHeight), "Test Menu", guiBoxStyle);

        float count = 0.0f;

        if (GUI.Button (new Rect (itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight), "SendTags", customTextSize)) {
            // You can tags users with key value pairs like this:
            OneSignal.SendTag("UnityTestKey", "TestValue");
            // Or use an IDictionary if you need to set more than one tag.
            OneSignal.SendTags(new Dictionary<string, string>() { { "UnityTestKey2", "value2" }, { "UnityTestKey3", "value3" } });

            // You can delete a single tag with it's key.
            // OneSignal.DeleteTag("UnityTestKey");
            // Or delete many with an IList.
            // OneSignal.DeleteTags(new List<string>() {"UnityTestKey2", "UnityTestKey3" });
        }

        count++;

        if (GUI.Button (new Rect (itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight), "GetIds", customTextSize)) {
            OneSignal.IdsAvailable((userId, pushToken) => {
                extraMessage = "UserID:\n" + userId + "\n\nPushToken:\n" + pushToken;
            });
        }

        count++;

        if (GUI.Button (new Rect (itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight), "TestNotification", customTextSize)) {
            extraMessage = "Waiting to get a OneSignal userId. Uncomment OneSignal.SetLogLevel in the Start method if it hangs here to debug the issue.";
            OneSignal.IdsAvailable((userId, pushToken) => {
                if (pushToken != null) {
                    // See https://documentation.onesignal.com/reference/create-notification for a full list of options.
                    // You can not use included_segments or any fields that require your OneSignal 'REST API Key' in your app for security reasons.
                    // If you need to use your OneSignal 'REST API Key' you will need your own server where you can make this call.

                    var notification = new Dictionary<string, object>();
                    notification["contents"] = new Dictionary<string, string>() { {"en", "Test Message"} };
                    // Send notification to this device.
                    notification["include_player_ids"] = new List<string>() { userId };
                    // Example of scheduling a notification in the future.
                    //notification["send_after"] = System.DateTime.Now.ToUniversalTime().AddSeconds(30).ToString("U");

                    extraMessage = "Posting test notification now.";

                    OneSignal.PostNotification(notification, (responseSuccess) => {
                        extraMessage = "Notification posted successful! Delayed by about 30 seconds to give you time to press the home button to see a notification vs an in-app alert.\n" + Json.Serialize(responseSuccess);
                    }, (responseFailure) => {
                        extraMessage = "Notification failed to post:\n" + Json.Serialize(responseFailure);
                    });
                } else {
                    extraMessage = "ERROR: Device is not registered.";
                }
            });
        }

        count++;

        email = GUI.TextField(new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight), email, customTextSize);

        count++;

        if (GUI.Button (new Rect (itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight), "SetEmail", customTextSize)) {
            extraMessage = "Setting email to " + email;

            OneSignal.SetEmail (email, () => {
                Debug.Log("Successfully set email");
            }, (error) => {
                Debug.Log("Encountered error setting email: " + Json.Serialize(error));
            });
        }

        count++;

        if (GUI.Button (new Rect (itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight), "LogoutEmail", customTextSize)) {
            extraMessage = "Logging Out of example@example.com";
         
            OneSignal.LogoutEmail (() => {
                Debug.Log("Successfully logged out of email");
            }, (error) => {
                Debug.Log("Encountered error logging out of email: " + Json.Serialize(error));
            });
        }

        count++;

        externalId = GUI.TextField(new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight), externalId, customTextSize);

        count++;

        if (GUI.Button(new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight), "SetExternalId", customTextSize)) {
            OneSignal.SetExternalUserId(externalId, OneSignalExternalUserIdCallback);
            // Auth external id method
            // OneSignal.SetExternalUserId(externalId, "your_auth_hash_token", OneSignalExternalUserIdCallback, OneSignalExternalUserIdCallbackFailure);
        }

        count++;

        if (GUI.Button(new Rect(itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight), "RemoveExternalId", customTextSize)) {
            OneSignal.RemoveExternalUserId(OneSignalExternalUserIdCallback);
        }

        if (requiresUserPrivacyConsent) {
            count++;

            if (GUI.Button (new Rect (itemOriginX, itemStartY + (count * itemHeightOffset), itemWidth, itemHeight), (OneSignal.UserProvidedConsent() ? "Revoke Privacy Consent" : "Provide Privacy Consent"), customTextSize)) {
                extraMessage = "Providing user privacy consent";
            
                OneSignal.UserDidProvideConsent(!OneSignal.UserProvidedConsent());
            }
        }

        if (extraMessage != null) {
            guiBoxStyle.alignment = TextAnchor.UpperLeft;
            guiBoxStyle.wordWrap = true;
            GUI.Box (new Rect (10, boxOriginY + boxHeight + 20, Screen.width - 20, Screen.height - (boxOriginY + boxHeight + 40)), extraMessage, guiBoxStyle);
        }
    }
}
