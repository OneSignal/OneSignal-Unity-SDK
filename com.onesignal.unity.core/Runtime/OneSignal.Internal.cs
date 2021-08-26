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

namespace OneSignalSDK {
    public abstract partial class OneSignal {
        private static OneSignal _default;

        private static OneSignal _getDefaultInstance() {
            if (_default != null)
                return _default;

            // todo - reflection if it has not yet been assigned

            return _default;
        }

#if true
    }
}
#else    
        /// <summary>
        /// notif serializer
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static Notification serializeFromDict(Dictionary<string, object> source) {
            Notification        notification = new Notification();
            NotificationPayload payload      = new NotificationPayload();

            var qwe = OneSignal.Default;
            
            //Build OSNotification object from jsonString
            var payloadObj = source["payload"] as Dictionary<string, object>;
            if (payloadObj.ContainsKey("notificationID"))
                payload.notificationID =
                    payloadObj["notificationID"] as string;

            if (payloadObj.ContainsKey("sound")) payload.sound         = payloadObj["sound"] as string;
            if (payloadObj.ContainsKey("title")) payload.title         = payloadObj["title"] as string;
            if (payloadObj.ContainsKey("body")) payload.body           = payloadObj["body"] as string;
            if (payloadObj.ContainsKey("subtitle")) payload.subtitle   = payloadObj["subtitle"] as string;
            if (payloadObj.ContainsKey("launchURL")) payload.launchURL = payloadObj["launchURL"] as string;

            if (payloadObj.ContainsKey("additionalData")) {
                if (payloadObj["additionalData"] is string)
                    payload.additionalData =
                        Json.Deserialize(payloadObj["additionalData"] as string) as Dictionary<string, object>;
                else
                    payload.additionalData = payloadObj["additionalData"] as Dictionary<string, object>;
            }

            if (payloadObj.ContainsKey("actionButtons")) {
                if (payloadObj["actionButtons"] is string)
                    payload.actionButtons =
                        Json.Deserialize(payloadObj["actionButtons"] as string) as Dictionary<string, object>;
                else
                    payload.actionButtons = payloadObj["actionButtons"] as Dictionary<string, object>;
            }

            if (payloadObj.ContainsKey("contentAvailable"))
                payload.contentAvailable =
                    (bool)payloadObj["contentAvailable"];

            if (payloadObj.ContainsKey("badge")) payload.badge           = Convert.ToInt32(payloadObj["badge"]);
            if (payloadObj.ContainsKey("smallIcon")) payload.smallIcon   = payloadObj["smallIcon"] as string;
            if (payloadObj.ContainsKey("largeIcon")) payload.largeIcon   = payloadObj["largeIcon"] as string;
            if (payloadObj.ContainsKey("bigPicture")) payload.bigPicture = payloadObj["bigPicture"] as string;
            if (payloadObj.ContainsKey("smallIconAccentColor"))
                payload.smallIconAccentColor = payloadObj["smallIconAccentColor"] as string;

            if (payloadObj.ContainsKey("ledColor")) payload.ledColor = payloadObj["ledColor"] as string;
            if (payloadObj.ContainsKey("lockScreenVisibility"))
                payload.lockScreenVisibility =
                    Convert.ToInt32(payloadObj["lockScreenVisibility"]);

            if (payloadObj.ContainsKey("groupKey")) payload.groupKey         = payloadObj["groupKey"] as string;
            if (payloadObj.ContainsKey("groupMessage")) payload.groupMessage = payloadObj["groupMessage"] as string;
            if (payloadObj.ContainsKey("fromProjectNumber"))
                payload.fromProjectNumber =
                    payloadObj["fromProjectNumber"] as string;

            notification.Payload = payload;

            if (source.ContainsKey("isAppInFocus")) notification.IsAppInFocus = (bool)source["isAppInFocus"];
            if (source.ContainsKey("shown")) notification.Shown               = (bool)source["shown"];
            if (source.ContainsKey("silentNotification"))
                notification.silentNotification =
                    (bool)source["silentNotification"];

            if (source.ContainsKey("androidNotificationId"))
                notification.androidNotificationId =
                    Convert.ToInt32(jsonObject["androidNotificationId"]);

            if (source.ContainsKey("displayType"))
                notification.DisplayType =
                    (NotificationDisplayType)Convert.ToInt32(source["displayType"]);

            return notification;
        }
        
        
        /*
         * Native Callbacks
         */

        // Called from the native SDK - Called when a push notification received.
        void onPushNotificationReceived(string jsonString) {
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;
            Notification notif;
            
            NotificationReceived?.Invoke(notif);
        }

        // Called from the native SDK - Called when a push notification is opened by the user
        void onPushNotificationOpened(string jsonString) {
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
                result.action       = action;

                builder.notificationOpenedDelegate(result);
            }
        }

        // Called from the native SDK - Called when device is registered with onesignal.com service or right after IdsAvailable
        //   if already registered.
        void onIdsAvailable(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidDelegate(jsonObject))
                return;

            var delegateId = jsonObject["delegate_id"] as string;
            var response   = jsonObject["response"] as string;

            var ids       = Json.Deserialize(response) as Dictionary<string, object>;
            var userId    = ids["userId"] as string;
            var pushToken = ids["pushToken"] as string;

            if (delegates.ContainsKey(delegateId)) {
                var idsAvailableCallback = (IdsAvailableCallback)delegates[delegateId];
                delegates.Remove(delegateId);
                idsAvailableCallback(userId, pushToken);
            }
        }

        // Called from the native SDK - Called After calling GetTags(...)
        void onTagsReceived(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidDelegate(jsonObject))
                return;

            var delegateId = jsonObject["delegate_id"] as string;
            var response   = jsonObject["response"] as string;

            var tags = Json.Deserialize(response) as Dictionary<string, object>;

            if (!string.IsNullOrEmpty(delegateId) && delegates.ContainsKey(delegateId)) {
                var tagsReceivedDelegate = (TagsReceived)delegates[delegateId];
                delegates.Remove(delegateId);
                tagsReceivedDelegate(tags);
            }
        }

        // Called from the native SDK
        void onPostNotificationSuccess(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidSuccessFailureDelegate(jsonObject))
                return;

            var delegateId        = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;
            var delegateIdSuccess = delegateId["success"] as string;
            var delegateIdFailure = delegateId["failure"] as string;

            var response            = jsonObject["response"] as string;
            var postNotificationDic = Json.Deserialize(response) as Dictionary<string, object>;

            if (delegates.ContainsKey(delegateIdSuccess)) {
                var postNotificationSuccessDelegate = (OnPostNotificationSuccess)delegates[delegateIdSuccess];
                delegates.Remove(delegateIdSuccess);
                delegates.Remove(delegateIdFailure);
                postNotificationSuccessDelegate(postNotificationDic);
            }
        }

        // Called from the native SDK
        void onPostNotificationFailed(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidSuccessFailureDelegate(jsonObject))
                return;

            var delegateId        = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;
            var delegateIdSuccess = delegateId["success"] as string;
            var delegateIdFailure = delegateId["failure"] as string;

            var response            = jsonObject["response"] as string;
            var postNotificationDic = Json.Deserialize(response) as Dictionary<string, object>;

            if (delegates.ContainsKey(delegateIdFailure)) {
                var postNotificationFailureDelegate = (OnPostNotificationFailure)delegates[delegateIdFailure];
                delegates.Remove(delegateIdSuccess);
                delegates.Remove(delegateIdFailure);
                postNotificationFailureDelegate(postNotificationDic);
            }
        }

        // Called from the native SDK
        void onExternalUserIdUpdateCompletion(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidDelegate(jsonObject))
                return;

            var delegateId = jsonObject["delegate_id"] as string;

            var response = jsonObject["response"] as string;
            var results  = Json.Deserialize(response) as Dictionary<string, object>;

            if (delegates.ContainsKey(delegateId)) {
                var externalUserIdUpdateCompletionDelegate = (OnExternalUserIdUpdateCompletion)delegates[delegateId];
                delegates.Remove(delegateId);
                externalUserIdUpdateCompletionDelegate(results);
            }
        }

        // Called from the native SDK
        void onSetEmailSuccess(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidSuccessFailureDelegate(jsonObject))
                return;

            var delegateId        = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;
            var delegateIdSuccess = delegateId["success"] as string;
            var delegateIdFailure = delegateId["failure"] as string;

            var response = jsonObject["response"] as string;

            if (delegates.ContainsKey(delegateIdSuccess)) {
                var setEmailSuccessDelegate = (OnSetEmailSuccess)delegates[delegateIdSuccess];
                delegates.Remove(delegateIdSuccess);
                delegates.Remove(delegateIdFailure);
                setEmailSuccessDelegate();
            }
        }

        // Called from the native SDK
        void onSetEmailFailure(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidSuccessFailureDelegate(jsonObject))
                return;

            var delegateId        = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;
            var delegateIdSuccess = delegateId["success"] as string;
            var delegateIdFailure = delegateId["failure"] as string;

            var response = jsonObject["response"] as string;
            var failure  = Json.Deserialize(response) as Dictionary<string, object>;

            if (delegates.ContainsKey(delegateIdFailure)) {
                var setEmailFailureDelegate = (OnSetEmailFailure)delegates[delegateIdFailure];
                delegates.Remove(delegateIdSuccess);
                delegates.Remove(delegateIdFailure);
                setEmailFailureDelegate(failure);
            }
        }

        // Called from the native SDK
        void onLogoutEmailSuccess(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidSuccessFailureDelegate(jsonObject))
                return;

            var delegateId        = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;
            var delegateIdSuccess = delegateId["success"] as string;
            var delegateIdFailure = delegateId["failure"] as string;

            var response = jsonObject["response"] as string;

            if (delegates.ContainsKey(delegateIdSuccess)) {
                var logoutEmailSuccessDelegate = (OnLogoutEmailSuccess)delegates[delegateIdSuccess];
                delegates.Remove(delegateIdSuccess);
                delegates.Remove(delegateIdFailure);
                logoutEmailSuccessDelegate();
            }
        }

        // Called from the native SDK
        void onLogoutEmailFailure(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidSuccessFailureDelegate(jsonObject))
                return;

            var delegateId        = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;
            var delegateIdSuccess = delegateId["success"] as string;
            var delegateIdFailure = delegateId["failure"] as string;

            var response = jsonObject["response"] as string;
            var failure  = Json.Deserialize(response) as Dictionary<string, object>;

            if (delegates.ContainsKey(delegateIdFailure)) {
                var logoutEmailFailureDelegate = (OnLogoutEmailFailure)delegates[delegateIdFailure];
                delegates.Remove(delegateIdSuccess);
                delegates.Remove(delegateIdFailure);
                logoutEmailFailureDelegate(failure);
            }
        }

        // Called from the native SDK - Called After calling SendOutcome(...), SendUniqueOutcome(...), SendOutcomeWithValue(...)
        void onSendOutcomeSuccess(string jsonString) {
            if (string.IsNullOrEmpty(jsonString))
                return;

            // Break part the jsonString which might contain a 'delegate_id' and a 'response'
            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            // Check if the delegate should be processed
            if (!isValidDelegate(jsonObject))
                return;

            var delegateId = jsonObject["delegate_id"] as string;
            var response   = jsonObject["response"] as string;

            OutcomeEvent outcomeEvent;

            if (string.IsNullOrEmpty(response)) {
                outcomeEvent = new OutcomeEvent();
            }
            else {
                // Parse outcome json string and return it through the callback
                var outcomeObject = Json.Deserialize(response) as Dictionary<string, object>;
                outcomeEvent = new OutcomeEvent(outcomeObject);
            }

            if (delegates.ContainsKey(delegateId) && delegates[delegateId] != null) {
                var sendOutcomeSuccess = (OnSendOutcomeSuccess)delegates[delegateId];
                delegates.Remove(delegateId);
                sendOutcomeSuccess(outcomeEvent);
            }
        }

        // Called from native SDK
        void onOSPermissionChanged(string stateChangesJSONString) {
            OSPermissionStateChanges stateChanges =
                oneSignalPlatform.ParseOSPermissionStateChanges(stateChangesJSONString);

            internalPermissionObserver(stateChanges);
        }

        // Called from native SDK
        void onOSSubscriptionChanged(string stateChangesJSONString) {
            OSSubscriptionStateChanges stateChanges =
                oneSignalPlatform.ParseOSSubscriptionStateChanges(stateChangesJSONString);

            internalSubscriptionObserver(stateChanges);
        }

        // Called from native SDK
        void onOSEmailSubscriptionChanged(string stateChangesJSONString) {
            OSEmailSubscriptionStateChanges stateChanges =
                oneSignalPlatform.ParseOSEmailSubscriptionStateChanges(stateChangesJSONString);

            internalEmailSubscriptionObserver(stateChanges);
        }

        // Called from native SDk
        void onPromptForPushNotificationsWithUserResponse(string accepted) {
            notificationUserResponseDelegate(Convert.ToBoolean(accepted));
        }

        // Called from native SDK
        void onInAppMessageClicked(string jsonString) {
            if (builder.inAppMessageClickHandlerDelegate == null)
                return;

            var jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;

            var action = new InAppMessageAction();
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
        bool isValidDelegate(Dictionary<string, object> jsonObject) {
            // Make sure 'delegate_id' exists
            if (!jsonObject.ContainsKey("delegate_id"))
                return false;

            // Make sure 'response' exists
            if (!jsonObject.ContainsKey("response"))
                return false;

            return true;
        }

        // Some functions have a 'success' and 'failure' delegates, so this validates nothing is missing the json response
        bool isValidSuccessFailureDelegate(Dictionary<string, object> jsonObject) {
            if (!isValidDelegate(jsonObject))
                return false;

            // Make sure success and failure delegate exist
            var delegateId = Json.Deserialize(jsonObject["delegate_id"] as string) as Dictionary<string, object>;

            if (!delegateId.ContainsKey("success") || !delegateId.ContainsKey("failure"))
                return false;

            return true;
        }
    }
}
#endif