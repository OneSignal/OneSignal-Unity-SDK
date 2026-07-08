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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OneSignalSDK.iOS.Notifications.Models;
using OneSignalSDK.iOS.Utilities;
using OneSignalSDK.Notifications;
using OneSignalSDK.Notifications.Internal;
using OneSignalSDK.Notifications.Models;
using UnityEngine;

namespace OneSignalSDK.iOS.Notifications
{
    internal sealed class iOSNotificationsManager : INotificationsManager
    {
        [DllImport("__Internal")]
        private static extern bool _oneSignalNotificationsGetPermission();

        [DllImport("__Internal")]
        private static extern bool _oneSignalNotificationsGetCanRequestPermission();

        [DllImport("__Internal")]
        private static extern int _oneSignalNotificationsGetPermissionNative();

        [DllImport("__Internal")]
        private static extern void _oneSignalNotificationsRequestPermission(
            bool fallbackToSettings,
            int hashCode,
            BooleanResponseDelegate callback
        );

        [DllImport("__Internal")]
        private static extern void _oneSignalNotificationsClearAll();

        [DllImport("__Internal")]
        private static extern void _oneSignalNotificationsAddPermissionObserver(
            PermissionListenerDelegate callback
        );

        [DllImport("__Internal")]
        private static extern void _oneSignalNotificationsSetForegroundWillDisplayCallback(
            WillDisplayListenerDelegate callback
        );

        [DllImport("__Internal")]
        private static extern void _oneSignalNotificationsWillDisplayEventPreventDefault(
            string notificationId
        );

        [DllImport("__Internal")]
        private static extern void _oneSignalNotificationsSetClickCallback(
            ClickListenerDelegate callback
        );

        private delegate void PermissionListenerDelegate(bool permission);
        private delegate void WillDisplayListenerDelegate(string notification);
        private delegate void ClickListenerDelegate(
            string notification,
            string resultActionId,
            string resultUrl
        );
        private delegate void BooleanResponseDelegate(int hashCode, bool response);

        public event EventHandler<NotificationWillDisplayEventArgs> ForegroundWillDisplay;
        public event EventHandler<NotificationPermissionChangedEventArgs> PermissionChanged;

        // Only set the native listener once
        private bool _clickNativeListenerSet;

        private readonly object _clickRegistrationLock = new object();

        private EventHandler<NotificationClickEventArgs> _clicked;

        /// <summary>
        /// The native click listener is registered lazily on the first subscription. The native SDK
        /// queues clicks that occur before a listener is added (e.g. a cold start from a notification
        /// tap) and replays them when one is registered, so deferring registration until a C# handler
        /// exists ensures those events are not dropped. The handler must be attached before the
        /// native call, since the replay fires as soon as the listener registers. The flag is only
        /// set after a successful native call so a transient failure can retry on the next
        /// subscription.
        /// </summary>
        public event EventHandler<NotificationClickEventArgs> Clicked
        {
            add
            {
                lock (_clickRegistrationLock)
                {
                    _clicked += value;

                    if (!_clickNativeListenerSet)
                    {
                        try
                        {
                            _oneSignalNotificationsSetClickCallback(_onClicked);
                        }
                        catch
                        {
                            // Roll back so a failed subscription has no effect; retrying won't
                            // add the same handler twice.
                            _clicked -= value;
                            throw;
                        }
                        _clickNativeListenerSet = true;
                    }
                }
            }
            remove
            {
                lock (_clickRegistrationLock)
                {
                    _clicked -= value;
                }
            }
        }

        private static iOSNotificationsManager _instance;

        public iOSNotificationsManager()
        {
            _instance = this;
        }

        public bool Permission
        {
            get => _oneSignalNotificationsGetPermission();
        }

        public bool CanRequestPermission
        {
            get => _oneSignalNotificationsGetCanRequestPermission();
        }

        public NotificationPermission PermissionNative
        {
            get => (NotificationPermission)_oneSignalNotificationsGetPermissionNative();
        }

        public async Task<bool> RequestPermissionAsync(bool fallbackToSettings)
        {
            var (proxy, hashCode) = WaitingProxy._setupProxy<bool>();
            _oneSignalNotificationsRequestPermission(
                fallbackToSettings,
                hashCode,
                BooleanCallbackProxy
            );
            return await proxy;
        }

        public void ClearAllNotifications()
        {
            _oneSignalNotificationsClearAll();
        }

        public void Initialize()
        {
            _oneSignalNotificationsAddPermissionObserver(_onPermissionStateChanged);
            _oneSignalNotificationsSetForegroundWillDisplayCallback(_onForegroundWillDisplay);
        }

        [AOT.MonoPInvokeCallback(typeof(PermissionListenerDelegate))]
        private static void _onPermissionStateChanged(bool permission)
        {
            NotificationPermissionChangedEventArgs args =
                new NotificationPermissionChangedEventArgs(permission);

            EventHandler<NotificationPermissionChangedEventArgs> handler =
                _instance.PermissionChanged;
            if (handler != null)
            {
                UnityMainThreadDispatch.Post(state => handler(_instance, args));
            }
        }

        [AOT.MonoPInvokeCallback(typeof(WillDisplayListenerDelegate))]
        private static void _onForegroundWillDisplay(string notification)
        {
            var notif = JsonUtility.FromJson<iOSDisplayableNotification>(notification);
            _fillNotifFromObj(ref notif, Json.Deserialize(notification));

            InternalNotificationWillDisplayEventArgs args =
                new InternalNotificationWillDisplayEventArgs(notif);

            EventHandler<NotificationWillDisplayEventArgs> handler =
                _instance.ForegroundWillDisplay;
            if (handler != null)
            {
                // We use Send instead of Post because we need to *not* return to our caller until the
                // event handlers have returned themselves. This allows a handler to call PreventDefault()
                // which will get passed down to iOS in InternalNotificationWillDisplayEventArgs.
                UnityMainThreadDispatch.Send(state => handler(_instance, args));
            }
        }

        public class InternalNotificationWillDisplayEventArgs : NotificationWillDisplayEventArgs
        {
            public InternalNotificationWillDisplayEventArgs(IDisplayableNotification notification)
                : base(notification) { }

            public override void PreventDefault()
            {
                _oneSignalNotificationsWillDisplayEventPreventDefault(
                    this.Notification.NotificationId
                );
            }
        }

        [AOT.MonoPInvokeCallback(typeof(ClickListenerDelegate))]
        private static void _onClicked(string notification, string resultActionId, string resultUrl)
        {
            var notif = JsonUtility.FromJson<iOSDisplayableNotification>(notification);
            _fillNotifFromObj(ref notif, Json.Deserialize(notification));

            var result = new NotificationClickResult(resultActionId, resultUrl);
            NotificationClickEventArgs args = new NotificationClickEventArgs(notif, result);

            EventHandler<NotificationClickEventArgs> handler = _instance._clicked;
            if (handler != null)
            {
                UnityMainThreadDispatch.Post(state => handler(_instance, args));
            }
        }

        private static void _fillNotifFromObj(ref iOSDisplayableNotification notif, object notifObj)
        {
            if (!(notifObj is Dictionary<string, object> notifDict))
                return;

            if (notifDict.ContainsKey("additionalData"))
                notif.additionalData = notifDict["additionalData"] as Dictionary<string, object>;

            if (notifDict.ContainsKey("attachments"))
                notif.attachments = notifDict["attachments"] as Dictionary<string, object>;

            if (
                notifDict.ContainsKey("rawPayload")
                && notifDict["rawPayload"] is Dictionary<string, object> payloadDict
            )
                notif.rawPayload = Json.Serialize(payloadDict);
        }

        [AOT.MonoPInvokeCallback(typeof(BooleanResponseDelegate))]
        private static void BooleanCallbackProxy(int hashCode, bool response) =>
            WaitingProxy.ResolveCallbackProxy(hashCode, response);
    }
}
