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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace OneSignalSDK {
    /// <summary>
    /// 
    /// </summary>
    public class OneSignalExampleBehaviourOLD : MonoBehaviour {
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

        /// <summary>
        /// whether you would prefer OneSignal Unity SDK prevent initialization until consent is granted via
        /// <see cref="OneSignal.UserConsent"/> in this test MonoBehaviour
        /// </summary>
        public bool requiresUserPrivacyConsent;

        // added to shorten total code
        private readonly OneSignal _onesignal = OneSignal.Default;

        /// <summary>
        /// we recommend initializing OneSignal early in your application's lifecycle such as in the Start method of a
        /// MonoBehaviour in your opening Scene
        /// </summary>
        private void Start() {
            _logMessage = null;

            // Enable lines below to debug issues with OneSignal. (logLevel, visualLogLevel)
            _onesignal.LogLevel   = LogType.Log;
            _onesignal.AlertLevel = LogType.Exception;

            /*
             * If you set to true, the user will have to provide consent via OneSignal.UserDidProvideConsent(true) before
             * the SDK will initialize
             */
            _onesignal.RequiresPrivacyConsent = requiresUserPrivacyConsent;

            /*
             * The only required method you need to call to setup OneSignal to receive push notifications.
             * Call before using any other methods on OneSignal (except setLogLevel or SetRequiredUserPrivacyConsent)
             * Should only invoke once when your app is loaded.
             */
            _onesignal.NotificationReceived += HandleNotificationReceived;
            _onesignal.NotificationWasOpened   += HandleNotificationOpened;
            _onesignal.InAppMessageTriggeredAction  += OnInAppMessageTriggeredAction;

            _onesignal.Initialize(appId);

            // todo
            // // Control how OneSignal notifications will be shown when one is received while your app is in focus
            // OneSignal.inFocusDisplayType = OneSignal.OSInFocusDisplayOption.Notification;

            // Each of these events can inform your application when the user's OneSignal states have change
            _onesignal.PermissionStateChanged        += OnPermissionStateChange;
            _onesignal.PushSubscriptionStateChanged      += OnPushSubscriptionStateChange;
            _onesignal.EmailSubscriptionStateChanged += OnEmailSubscriptionStateChange;

            // todo
            // // You can also get the current states directly
            // var fullUserState          = OneSignal.GetPermissionSubscriptionState();
            // var permissionState        = fullUserState.permissionStatus;
            // var subscriptionState      = fullUserState.subscriptionStatus;
            // var emailSubscriptionState = fullUserState.emailSubscriptionStatus;

            OneSignalInAppMessageTriggerExamples();
            OneSignalOutcomeEventsExamples();
        }

        /// <summary>
        /// Examples of using OneSignal External User Id
        /// </summary>
        private void OnUpdatedExternalUserId(Dictionary<string, object> results) {
            // The results will contain push and email success statuses
            print($"External user id updated with results: {Json.Serialize(results)}");

            // Push can be expected in almost every situation with a success status, but
            // as a pre-caution its good to verify it exists
            if (results.ContainsKey("push")) {
                if (results["push"] is Dictionary<string, object> pushStatus && pushStatus.ContainsKey("success"))
                    print($"External user id updated for push with results: {pushStatus["success"]}");
            }

            // Verify the email is set or check that the results have an email success status
            if (results.ContainsKey("email")) {
                if (results["email"] is Dictionary<string, object> emailStatus && emailStatus.ContainsKey("success"))
                    print($"External user id updated for email with results: {emailStatus["success"]}");
            }
        }

        private void OnUpdatedExternalUserIdFailure(Dictionary<string, object> error) {
            // As above the results will contain push and email statuses
            print($"External user id failed with error: {Json.Serialize(error)}");
        }

        /// <summary>
        /// Examples of using OneSignal In-App Message triggers
        /// https://documentation.onesignal.com/docs/in-app-message-examples
        /// </summary>
        private void OneSignalInAppMessageTriggerExamples() {
            // Add a single trigger
            _onesignal.SetTrigger("key", "value");

            // Get the current value to a trigger by key
            var triggerKey   = "key";
            var triggerValue = _onesignal.GetTrigger(triggerKey);
            print($"Trigger key: {triggerKey} value: {triggerValue}");

            // Add multiple triggers
            _onesignal.SetTriggers(new Dictionary<string, object> {
                { "key1", "value1" },
                { "key2", 2 }
            });

            // Delete a trigger
            _onesignal.RemoveTrigger("key");

            // Delete a list of triggers
            _onesignal.RemoveTriggers("key1", "key2");

            //_onesignal.RemoveTriggers(new string[] { "key1", "key2" });

            // Temporarily pause In-App messages; If true is passed in.
            // Great to ensure you never interrupt your user while they are in the middle of a match in your game.
            _onesignal.InAppMessagesArePaused = false;
        }

        /// <summary>
        /// Send data to OneSignal which will allow you to track the result of notifications
        /// https://documentation.onesignal.com/docs/outcomes
        /// </summary>
        private void OneSignalOutcomeEventsExamples() {
            // Send a result which can occur multiple times. In this case the return is being ignored.
            _onesignal.SendOutcome("normal_1");

            // If you'd like to monitor whether the send was successful you can await the task like below
            _onesignal.SendOutcome("normal_2")
               .ContinueWith(task => {
                    
                });

            // Send a result which can only occur once
            _onesignal.SendUniqueOutcome("unique_1");

            // todo
            // var result = await _onesignal.SendUniqueOutcome("unique_2");

            // Send a result which can occur multiple times with a float value
            _onesignal.SendOutcomeWithValue("value_1", 3.2f);
            _onesignal.SendOutcomeWithValue("value_2", 3.2f);
        }

        /*
         * State change events provide both the new (to) and previous (from) states
         */

        private void OnPushSubscriptionStateChange(PushSubscriptionState current, PushSubscriptionState previous) {
            print("SUBSCRIPTION stateChanges: " + current);
            print("SUBSCRIPTION stateChanges.to.userId: " + current.userId);
            print("SUBSCRIPTION stateChanges.to.subscribed: " + current.subscribed);
        }

        private void OnPermissionStateChange(PermissionState current, PermissionState previous) {
            print($"PERMISSION stateChanges.from.status: {previous.status}");
            print($"PERMISSION stateChanges.to.status: {current.status}");
        }

        private void OnEmailSubscriptionStateChange(EmailSubscriptionState current, EmailSubscriptionState previous) {
            print($"EMAIL stateChanges.from.status: {previous.emailUserId}, {previous.emailAddress}");
            print($"EMAIL stateChanges.to.status: {current.emailUserId}, {current.emailAddress}");
        }

        /// <summary>
        /// Called when your app is in focus and a notification is received.
        /// The name of the method can be anything as long as the signature matches.
        /// Method must be static or this object should be marked as DontDestroyOnLoad
        /// </summary>
        private static void HandleNotificationReceived(Notification notification) {
            var payload = notification.payload;
            var message = payload.body;

            print("GameControllerExample:HandleNotificationReceived: " + message);
            print("displayType: " + notification.displayType);
            _logMessage = "Notification received with text: " + message;

            if (payload.additionalData == null)
                print("[HandleNotificationReceived] Additional Data == null");
            else if (Json.Serialize(payload.additionalData) is { } dataString)
                print($"[HandleNotificationReceived] message {message}, additionalData: {dataString}");
            else
                print("[HandleNotificationReceived] Additional Data could not be serialized");
        }

        /// <summary>
        /// Called when a notification is opened.
        /// The name of the method can be anything as long as the signature matches.
        /// Method must be static or this object should be marked as DontDestroyOnLoad
        /// </summary>
        private static void HandleNotificationOpened(NotificationOpenedResult notificationOpenedResult) {
            var payload  = notificationOpenedResult.notification.payload;
            var message  = payload.body;
            var actionID = notificationOpenedResult.action.id;

            print("GameControllerExample:HandleNotificationOpened: " + message);
            _logMessage = "Notification opened with text: " + message;

            if (payload.additionalData == null)
                print("[HandleNotificationOpened] Additional Data == null");
            else if (Json.Serialize(payload.additionalData) is { } dataString)
                print($"[HandleNotificationOpened] message {message}, additionalData: {dataString}");
            else
                print("[HandleNotificationOpened] Additional Data could not be serialized");

            if (actionID != null) {
                // actionSelected equals the id on the button the user pressed.
                // actionSelected will equal "__DEFAULT__" when the notification itself was tapped when buttons were present.
                _logMessage = "Pressed ButtonId: " + actionID;
            }
        }

        private static void OnInAppMessageTriggeredAction(InAppMessageAction inAppMessageAction) {
            var logInAppClickEvent = "In-App Message Clicked: " +
                "\nClick Name: " + inAppMessageAction.clickName +
                "\nClick Url: " + inAppMessageAction.clickUrl +
                "\nFirst Click: " + inAppMessageAction.firstClick +
                "\nCloses Message: " + inAppMessageAction.closesMessage;

            print(logInAppClickEvent);
            _logMessage = logInAppClickEvent;
        }

        /// <summary>
        /// See https://documentation.onesignal.com/reference/create-notification for a full list of options.
        /// </summary>
        /// <remarks>
        /// You can not use included_segments or any fields that require your OneSignal 'REST API Key' in your app for
        /// security reasons.
        /// If you need to use your OneSignal 'REST API Key' you will need your own server where you can make this call.
        /// </remarks>
        private async void SendTestNotification(string userId) {
            var notification = new Dictionary<string, object> {
                ["contents"] = new Dictionary<string, string> { { "en", "Test Message" } },

                // Send notification to this user
                ["include_player_ids"] = new List<string> { userId },

                // Example of scheduling a notification in the future.
                ["send_after"] = DateTime.Now.ToUniversalTime().AddSeconds(30).ToString("U")
            };

            _logMessage
                = "Posting test notification now. By default the example should arrive 30 seconds in the future." +
                "If you would like to see it as a Push and not an In App Alert then please leave the application.";

            var task = _onesignal.PostNotification(notification);

            try {
                if (await task is { } response)
                    OnNotificationPostSuccess(response);
            }
            catch (TaskCanceledException cancelException) {
                printError(cancelException.Message);
            }
        }

        private static void OnNotificationPostSuccess(Dictionary<string, object> response)
            => _logMessage = "Notification post success!\n" + Json.Serialize(response);

        private static void OnNotificationPostFailure(Dictionary<string, object> response)
            => _logMessage = "Notification failed to post:\n" + Json.Serialize(response);

        /*
         * UI Rendering 
         */

        private const float ItemOriginX = 50.0f;
        private const float BoxOriginY = 120.0f;
        private const float ItemStartY = 200.0f;
        private const float ItemHeightOffset = 90.0f;
        private const float ItemHeight = 60.0f;

        private static string _logMessage;

        private GUIStyle _customTextSize;
        private GUIStyle _guiBoxStyle;

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
            _customTextSize ??= new GUIStyle(GUI.skin.button) {
                fontSize = 30
            };

            _guiBoxStyle ??= new GUIStyle(GUI.skin.box) {
                fontSize  = 30,
                alignment = TextAnchor.UpperLeft,
                wordWrap  = true
            };

            GUI.Box(MainMenuRect, "Test Menu", _guiBoxStyle);

            int position = 0;

            if (MenuButton(ref position, "Send Example Tags")) {
                // You can tags users with key value pairs like this:
                _onesignal.SendTag("UnityTestKey", "TestValue");

                // Or use an IDictionary if you need to set more than one tag.
                _onesignal.SendTags(new Dictionary<string, object> {
                    { "UnityTestKey2", "value2" },
                    { "UnityTestKey3", "value3" }
                });

                // You can delete a single tag with it's key.
                // OneSignal.DeleteTag("UnityTestKey");
                // Or delete many with an IList.
                // OneSignal.DeleteTags(new List<string>() {"UnityTestKey2", "UnityTestKey3" });
            }

            if (MenuButton(ref position, "Get Ids")) {
                // todo
                // OneSignal.IdsAvailable((userId, pushToken) => {
                //     _logMessage = $"UserID:\n{userId}\n\nPushToken:\n{pushToken}";
                // });
            }

            if (MenuButton(ref position, "Test Notification")) {
                _logMessage
                    = "Waiting to get a OneSignal userId. Uncomment OneSignal.SetLogLevel in the Start method if " +
                    "it hangs here to debug the issue.";

                // todo
                // // Checking to make sure this device is registered or you will not receive the notification
                // OneSignal.IdsAvailable((userId, pushToken) => {
                //     if (pushToken != null)
                //         SendTestNotification(userId);
                //     else
                //         _logMessage = "ERROR: Device is not registered.";
                // });
            }

            email = GUI.TextField(ItemRect(ref position), email, _customTextSize);

            if (MenuButton(ref position, "Set Email")) {
                _logMessage = "Setting email to " + email;

                // todo
                // _onesignal.SetEmail(email,
                //     () => print("Successfully set email"),
                //     error => printError("Error setting email: " + Json.Serialize(error))
                // );
            }

            if (MenuButton(ref position, "Logout Email")) {
                _logMessage = "Logging Out of example@example.com";

                // todo
                // OneSignal.LogoutEmail(
                //     () => print("Successfully logged out of email"),
                //     error => printError("Error logging out of email: " + Json.Serialize(error))
                // );
            }

            externalId = GUI.TextField(ItemRect(ref position), externalId, _customTextSize);

            // todo
            // if (MenuButton(ref position, "Set External Id"))
            //     OneSignal.SetExternalUserId(externalId, OnUpdatedExternalUserId);
            //
            // if (MenuButton(ref position, "Remove External Id"))
            //     _onesignal.RemoveExternalUserId(OnUpdatedExternalUserId);

            if (requiresUserPrivacyConsent) {
                var consentText = _onesignal.PrivacyConsent
                    ? "Revoke Privacy Consent"
                    : "Provide Privacy Consent";

                if (GUI.Button(ItemRect(ref position), consentText, _customTextSize)) {
                    _logMessage = "Providing user privacy consent";

                    _onesignal.PrivacyConsent = !_onesignal.PrivacyConsent;
                }
            }

            if (_logMessage != null) {
                var logRect = new Rect(
                    10,
                    BoxOriginY + BoxHeight + 20,
                    Screen.width - 20,
                    Screen.height - (BoxOriginY + BoxHeight + 40)
                );

                GUI.Box(logRect, _logMessage, _guiBoxStyle);
            }
        }

        private static void printError(object message) => Debug.LogError(message);
    }
}
#endif