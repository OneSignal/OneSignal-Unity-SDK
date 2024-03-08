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

using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OneSignalSDK.Notifications;
using OneSignalSDK.Notifications.Models;
using OneSignalSDK.Notifications.Internal;
using OneSignalSDK.Android.Utilities;
using OneSignalSDK.Android.Notifications.Models;

namespace OneSignalSDK.Android.Notifications {
    internal sealed class AndroidNotificationsManager : INotificationsManager {
        private readonly AndroidJavaObject _notifications;

        public AndroidNotificationsManager(AndroidJavaClass sdkClass) {
            _notifications = sdkClass.CallStatic<AndroidJavaObject>("getNotifications");
        }

        public event EventHandler<NotificationWillDisplayEventArgs> ForegroundWillDisplay;
        public event EventHandler<NotificationClickEventArgs> Clicked;
        public event EventHandler<NotificationPermissionChangedEventArgs> PermissionChanged;

        public bool Permission {
            get => _notifications.Call<bool>("getPermission");
        }

        public bool CanRequestPermission {
            get => _notifications.Call<bool>("getCanRequestPermission");
        }

        public NotificationPermission PermissionNative {
            get => _notifications.Call<bool>("getPermission") ? NotificationPermission.Authorized : NotificationPermission.Denied;
        }

        public async Task<bool> RequestPermissionAsync(bool fallbackToSettings) {
            var continuation = new BoolContinuation();
            _notifications.Call<AndroidJavaObject>("requestPermission", fallbackToSettings, continuation.Proxy);
            return await continuation;
        }

        public void ClearAllNotifications() {
            _notifications.Call("clearAllNotifications");
        }

        public void Initialize() {
            _notifications.Call("addPermissionObserver", new InternalPermissionObserver(this));
            _notifications.Call("addForegroundLifecycleListener", new InternalNotificationLifecycleListener(this));
            _notifications.Call("addClickListener", new InternalNotificationClickListener(this));
        }

        private sealed class InternalPermissionObserver : OneSignalAwaitableAndroidJavaProxy<bool> {
            private AndroidNotificationsManager _parent;
            
            public InternalPermissionObserver(AndroidNotificationsManager notificationsManager) : base("notifications.IPermissionObserver") {
                _parent = notificationsManager;
            }

            /// <param name="permission">boolean</param>
            public void onNotificationPermissionChange(bool permission) {
                NotificationPermissionChangedEventArgs args = new NotificationPermissionChangedEventArgs(permission);

                EventHandler<NotificationPermissionChangedEventArgs> handler = _parent.PermissionChanged;
                if (handler != null)
                {
                    UnityMainThreadDispatch.Post(state => handler(_parent, args));
                }
            }
        }

        private sealed class InternalNotificationLifecycleListener : OneSignalAndroidJavaProxy {
            private AndroidNotificationsManager _parent;

            public InternalNotificationLifecycleListener(AndroidNotificationsManager notificationsManager) : base("notifications.INotificationLifecycleListener") {
                _parent = notificationsManager;
            }

            /// <param name="willDisplayEvent">INotificationWillDisplayEvent</param>
            public void onWillDisplay(AndroidJavaObject willDisplayEvent) {
                var notifJO = willDisplayEvent.Call<AndroidJavaObject>("getNotification");
                var notification = _getNotification(notifJO);

                InternalNotificationWillDisplayEventArgs args = new InternalNotificationWillDisplayEventArgs(willDisplayEvent, notification);

                EventHandler<NotificationWillDisplayEventArgs> handler = _parent.ForegroundWillDisplay;
                if (handler != null)
                {
                    // We use Send instead of Post because we need to *not* return to our caller until the
                    // event handlers have returned themselves. This allows a handler to call PreventDefault()
                    // which will get passed down to Android in InternalNotificationWillDisplayEventArgs.
                    UnityMainThreadDispatch.Send(state => handler(_parent, args));
                }
            }
        }

        public class InternalNotificationWillDisplayEventArgs : NotificationWillDisplayEventArgs {
            private AndroidJavaObject _willDisplayEvent;

            public InternalNotificationWillDisplayEventArgs(AndroidJavaObject willDisplayEvent, IDisplayableNotification notification) : base(notification) {
                _willDisplayEvent = willDisplayEvent;
            }

            public override void PreventDefault() => _willDisplayEvent.Call("preventDefault");
        }

        private sealed class InternalNotificationClickListener : OneSignalAndroidJavaProxy {
            private AndroidNotificationsManager _parent;

            public InternalNotificationClickListener(AndroidNotificationsManager notificationsManager) : base("notifications.INotificationClickListener") {
                _parent = notificationsManager;
            }

            /// <param name="clickEvent">INotificationClickEvent</param>
            public void onClick(AndroidJavaObject clickEvent) {
                var notifJO = clickEvent.Call<AndroidJavaObject>("getNotification");
                var notification = _getNotification(notifJO);

                var resultJO = clickEvent.Call<AndroidJavaObject>("getResult");
                var result = resultJO.ToSerializable<NotificationClickResult>();

                NotificationClickEventArgs args = new NotificationClickEventArgs(notification, result);

                EventHandler<NotificationClickEventArgs> handler = _parent.Clicked;
                if (handler != null)
                {
                    UnityMainThreadDispatch.Post(state => handler(_parent, args));
                }
            }
        }

        private static AndroidDisplayableNotification _getNotification(AndroidJavaObject notifJO) {
            var notification = notifJO.ToSerializable<AndroidDisplayableNotification>();
            
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

            var rawPayloadJson = notifJO.Call<AndroidJavaObject>("getRawPayload");
            if (rawPayloadJson != null) {
                var rawPayloadJsonStr = rawPayloadJson.Call<string>("toString");
                notification.rawPayload = rawPayloadJsonStr;
            }

            // attach the Java-Object to the notifification just built.
            notification.NotifJO = notifJO;

            return notification;
        }
    }
}