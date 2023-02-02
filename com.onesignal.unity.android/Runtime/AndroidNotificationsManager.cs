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
using System.Threading.Tasks;
using OneSignalSDK.Notifications;
using OneSignalSDK.Notifications.Models;
using OneSignalSDK.Android.Utilities;
using System.Collections.Generic;

namespace OneSignalSDK.Android.Notifications {
    internal sealed class AndroidNotificationsManager : INotificationsManager {
        private readonly AndroidJavaObject _notifications;
        
        public AndroidNotificationsManager(AndroidJavaClass sdkClass) {
            _notifications = sdkClass.CallStatic<AndroidJavaObject>("getNotifications");
        }

        public event NotificationWillShowDelegate WillShow;
        public event NotificationClickedDelegate Clicked;
        public event PermissionChangedDelegate PermissionChanged;

        public bool Permission {
            get => _notifications.Call<bool>("getPermission");
        }

        public async Task<bool> RequestPermissionAsync(bool fallbackToSettings) {
            var continuation = new BoolContinuation();
            _notifications.Call<AndroidJavaObject>("requestPermission", fallbackToSettings, continuation.Proxy);
            return await continuation;
        }

        public void ClearAllNotifications() {
            var continuation = new Continuation();
            _notifications.Call<AndroidJavaObject>("clearAllNotifications", continuation.Proxy);
        }

        public void Initialize() {
            _notifications.Call("addPermissionChangedHandler", new InternalPermissionChangedHandler(this));
            _notifications.Call("setNotificationWillShowInForegroundHandler", new InternalNotificationWillShowInForegroundHandler(this));
            _notifications.Call("setNotificationClickHandler", new InternalNotificationClickHandler(this));
        }

        private sealed class InternalPermissionChangedHandler : OneSignalAwaitableAndroidJavaProxy<bool> {
            private AndroidNotificationsManager _parent;
            
            public InternalPermissionChangedHandler(AndroidNotificationsManager notificationsManager) : base("notifications.IPermissionChangedHandler") {
                _parent = notificationsManager;
            }

            /// <param name="permission">boolean</param>
            public void onPermissionChanged(bool permission) {
                UnityMainThreadDispatch.Post(state => _parent.PermissionChanged?.Invoke(permission));
            }
        }

        private sealed class InternalNotificationWillShowInForegroundHandler : OneSignalAndroidJavaProxy {
            private AndroidNotificationsManager _parent;

            public InternalNotificationWillShowInForegroundHandler(AndroidNotificationsManager notificationsManager) : base("notifications.INotificationWillShowInForegroundHandler") {
                _parent = notificationsManager;
            }

            /// <param name="notificationReceivedEvent">INotificationReceivedEvent</param>
            public void notificationWillShowInForeground(AndroidJavaObject notificationReceivedEvent) {
                var notifJO = notificationReceivedEvent.Call<AndroidJavaObject>("getNotification");

                if (_parent.WillShow == null) {
                    notificationReceivedEvent.Call("complete", notifJO);
                    return;
                }

                var notification = _getNotification(notifJO);

                UnityMainThreadDispatch.Post(state => notificationReceivedEvent.Call("complete", _parent.WillShow(notification) != null ? notifJO : null));
            }
        }

        private sealed class InternalNotificationClickHandler : OneSignalAndroidJavaProxy {
            private AndroidNotificationsManager _parent;

            public InternalNotificationClickHandler(AndroidNotificationsManager notificationsManager) : base("notifications.INotificationClickHandler") {
                _parent = notificationsManager;
            }

            /// <param name="result">INotificationClickResult</param>
            public void notificationClicked(AndroidJavaObject result) {
                var notifJO = result.Call<AndroidJavaObject>("getNotification");
                var notification = _getNotification(notifJO);

                var actionJO = result.Call<AndroidJavaObject>("getAction");
                var action = _getAction(actionJO);

                var notifClickResult = new NotificationClickedResult {
                    notification = notification,
                    action = action
                };

                UnityMainThreadDispatch.Post(state => _parent.Clicked?.Invoke(notifClickResult));
            }
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
            action.type = (ActionType) actionJO.Call<AndroidJavaObject>("getType").Call<int>("ordinal");

            return action;
        }
    }
}