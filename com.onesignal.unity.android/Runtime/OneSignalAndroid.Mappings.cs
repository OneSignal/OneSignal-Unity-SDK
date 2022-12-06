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

using UnityEngine;
using System.Collections.Generic;

namespace OneSignalSDK {
    /// <summary>
    /// Helpers for converting from <see cref="AndroidJavaObject"/> to OneSignal C# model classes
    /// </summary>
    public sealed partial class OneSignalAndroid : OneSignal {
        private static (TState curr, TState prev) _getStatesChanges<TState>(AndroidJavaObject stateChanges) {
            var currJO  = stateChanges.Call<AndroidJavaObject>("getTo");
            var prevJO  = stateChanges.Call<AndroidJavaObject>("getFrom");
            var currStr = currJO.Call<string>("toString");
            var prevStr = prevJO.Call<string>("toString");
            var curr    = JsonUtility.FromJson<TState>(currStr);
            var prev    = JsonUtility.FromJson<TState>(prevStr);

            return (curr, prev);
        }

        private static void _handleError(AndroidJavaObject error) {
            var type = error.Call<AndroidJavaObject>("getType");
            var typeStr = type.Call<string>("name");
            var msg  = error.Call<string>("getMessage");
            SDKDebug.Error($"{typeStr} - {msg}");
        }

        private static NotificationPermission _stateNotificationPermission(AndroidJavaObject stateWithPermissions) {
            var enabled = stateWithPermissions.Call<bool>("areNotificationsEnabled");
            return enabled ? NotificationPermission.Authorized : NotificationPermission.Denied;
        }

        private static Notification _getNotification(AndroidJavaObject notifJO) {
            var notification = notifJO.ToSerializable<Notification>();
            
            var dataJson = notifJO.Call<AndroidJavaObject>("getAdditionalData");
            if (dataJson != null) {
                var dataJsonStr = dataJson.Call<string>("toString");
                notification.additionalData = Json.Deserialize(dataJsonStr) as Dictionary<string, object>;
            }

            var groupedNotificationsJson = notifJO.Call<AndroidJavaObject>("getGroupedNotifications");
            if (groupedNotificationsJson != null) {
                var groupedNotificationsStr = groupedNotificationsJson.Call<string>("toString");
                notification.groupedNotifications = Json.Deserialize(groupedNotificationsStr) as List<NotificationBase>;
            }

            return notification;
        }

        private static NotificationAction _getAction(AndroidJavaObject actionJO) {
            var action = actionJO.ToSerializable<NotificationAction>();
            action.actionID = actionJO.Call<string>("getActionId");

            return action;
        }
    }
}