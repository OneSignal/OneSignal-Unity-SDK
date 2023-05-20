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

using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OneSignalSDK.Notifications;
using OneSignalSDK.Notifications.Models;
using OneSignalSDK.Notifications.Internal;
using OneSignalSDK.iOS.Utilities;
using OneSignalSDK.iOS.Notifications.Models;

namespace OneSignalSDK.iOS.Notifications {
    internal sealed class iOSNotificationsManager : INotificationsManager {
        [DllImport("__Internal")] private static extern bool _notificationsGetPermission();
        [DllImport("__Internal")] private static extern int _notificationsGetPermissionNative();
        [DllImport("__Internal")] private static extern void _notificationsRequestPermission(bool fallbackToSettings, int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _notificationsClearAll();
        [DllImport("__Internal")] private static extern void _notificationsAddPermissionObserver(PermissionListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _notificationsSetForegroundWillDisplayCallback(WillDisplayListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _notificationsWillDisplayEventPreventDefault(string notificationId);
        [DllImport("__Internal")] private static extern void _notificationsSetClickCallback(ClickListenerDelegate callback);

        public delegate void PermissionListenerDelegate(bool permission);
        private delegate void WillDisplayListenerDelegate(string notification);
        private delegate void ClickListenerDelegate(string notification, string resultActionId, string resultUrl);
        private delegate void BooleanResponseDelegate(int hashCode, bool response);

        public event EventHandler<NotificationWillDisplayEventArgs> ForegroundWillDisplay;
        public event EventHandler<NotificationClickEventArgs> Clicked;
        public event EventHandler<NotificationPermissionChangedEventArgs> PermissionChanged;

        private static iOSNotificationsManager _instance;

        public iOSNotificationsManager() {
            _instance = this;
        }

        public bool Permission {
            get => _notificationsGetPermission();
        }

        public NotificationPermission PermissionNative {
            get => (NotificationPermission)_notificationsGetPermissionNative();
        }

        public async Task<bool> RequestPermissionAsync(bool fallbackToSettings) {
            var (proxy, hashCode) = WaitingProxy._setupProxy<bool>();
            _notificationsRequestPermission(fallbackToSettings, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public void ClearAllNotifications() {
            _notificationsClearAll();
        }

        public void Initialize() {
            _notificationsAddPermissionObserver(_onPermissionStateChanged);
            _notificationsSetForegroundWillDisplayCallback(_onForegroundWillDisplay);
            _notificationsSetClickCallback(_onClicked);
        }

        [AOT.MonoPInvokeCallback(typeof(PermissionListenerDelegate))]
        private static void _onPermissionStateChanged(bool permission) {
            NotificationPermissionChangedEventArgs args = new NotificationPermissionChangedEventArgs(permission);

            EventHandler<NotificationPermissionChangedEventArgs> handler = _instance.PermissionChanged;
            if (handler != null)
            {
                UnityMainThreadDispatch.Post(state => handler(_instance, args));
            }
        }

        [AOT.MonoPInvokeCallback(typeof(WillDisplayListenerDelegate))]
        private static void _onForegroundWillDisplay(string notification) {
            var notif = JsonUtility.FromJson<iOSDisplayableNotification>(notification);
            _fillNotifFromObj(ref notif, Json.Deserialize(notification));

            InternalNotificationWillDisplayEventArgs args = new InternalNotificationWillDisplayEventArgs(notif);

            EventHandler<NotificationWillDisplayEventArgs> handler = _instance.ForegroundWillDisplay;
            if (handler != null)
            {
                // We use Send instead of Post because we need to *not* return to our caller until the
                // event handlers have returned themselves. This allows a handler to call PreventDefault()
                // which will get passed down to Android in InternalNotificationWillDisplayEventArgs.
                UnityMainThreadDispatch.Send(state => handler(_instance, args));
            }
        }

        public class InternalNotificationWillDisplayEventArgs : NotificationWillDisplayEventArgs {
            public InternalNotificationWillDisplayEventArgs(IDisplayableNotification notification) : base(notification) { }

            public override void PreventDefault() {
                _notificationsWillDisplayEventPreventDefault(this.Notification.NotificationId);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(ClickListenerDelegate))]
        private static void _onClicked(string notification, string resultActionId, string resultUrl) {
            var notif = JsonUtility.FromJson<iOSDisplayableNotification>(notification);
            _fillNotifFromObj(ref notif, Json.Deserialize(notification));

            var result = new NotificationClickResult(resultActionId, resultUrl);

            NotificationClickEventArgs args = new NotificationClickEventArgs(notif, result);

            EventHandler<NotificationClickEventArgs> handler = _instance.Clicked;
            if (handler != null)
            {
                if (OneSignalPlatform.DidInitialize)
                    UnityMainThreadDispatch.Post(state => handler(_instance, args));
                else {
                    void invokeOpened(string appId) {
                        OneSignalPlatform.OnInitialize -= invokeOpened;
                        UnityMainThreadDispatch.Post(state => handler(_instance, args));
                    }

                    OneSignalPlatform.OnInitialize += invokeOpened; 
                }
            }
        }

        private static void _fillNotifFromObj(ref iOSDisplayableNotification notif, object notifObj) {
            if (!(notifObj is Dictionary<string, object> notifDict)) 
                return;

            if (notifDict.ContainsKey("additionalData"))
                notif.additionalData = notifDict["additionalData"] as Dictionary<string, object>;
            
            if (notifDict.ContainsKey("attachments"))
                notif.attachments = notifDict["attachments"] as Dictionary<string, object>;

            if (notifDict.ContainsKey("rawPayload") && notifDict["rawPayload"] is Dictionary<string, object> payloadDict)
                notif.rawPayload = Json.Serialize(payloadDict);
        }

        [AOT.MonoPInvokeCallback(typeof(BooleanResponseDelegate))]
        private static void BooleanCallbackProxy(int hashCode, bool response)
            => WaitingProxy.ResolveCallbackProxy(hashCode, response);
    }
}