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
using OneSignalSDK.iOS.Utilities;

namespace OneSignalSDK.iOS.Notifications {
    internal sealed class iOSNotificationsManager : INotificationsManager {
        [DllImport("__Internal")] private static extern bool _notificationsGetPermission();
        //[DllImport("__Internal")] private static extern bool _notificationsGetCanRequestPermission(); // iOS only for now
        [DllImport("__Internal")] private static extern void _notificationsRequestPermission(bool fallbackToSettings, int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _notificationsClearAll();
        //[DllImport("__Internal")] private static extern void _registerForProvisionalAuthorization(); // iOS only
        [DllImport("__Internal")] private static extern void _notificationsAddPermissionStateChangedCallback(StateListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _notificationsSetWillShowHandler(NotificationWillShowInForegroundDelegate callback);
        [DllImport("__Internal")] private static extern void _notificationsSetOpenedHandler(StringListenerDelegate callback);

        public delegate void StateListenerDelegate(string current, string previous);

        private delegate bool NotificationWillShowInForegroundDelegate(string notification);
        private delegate void StringListenerDelegate(string response);
        private delegate void BooleanResponseDelegate(int hashCode, bool response);

        public event NotificationWillShowDelegate WillShow;
        public event NotificationClickedDelegate Clicked;
        public event PermissionChangedDelegate PermissionChanged;

        private static iOSNotificationsManager _instance;

        public iOSNotificationsManager() {
            _instance = this;
        }

        public bool Permission {
            get => _notificationsGetPermission();
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
            _notificationsAddPermissionStateChangedCallback(_onPermissionStateChanged);
            _notificationsSetWillShowHandler(_onWillShow);
            _notificationsSetOpenedHandler(_onOpened);
        }

        [AOT.MonoPInvokeCallback(typeof(StateListenerDelegate))] // temp
        private static void _onPermissionStateChanged(string current, string previous) {
            var curr = JsonUtility.FromJson<PermissionState>(current);
            //var prev = JsonUtility.FromJson<PermissionState>(previous);
            UnityMainThreadDispatch.Post(state => _instance.PermissionChanged?.Invoke(curr.reachable));
        }

        /// <param name="response">OSNotification</param>
        [AOT.MonoPInvokeCallback(typeof(NotificationWillShowInForegroundDelegate))]
        private static bool _onWillShow(string response) {
            if (_instance.WillShow == null)
                return true;

            var notification = JsonUtility.FromJson<Notification>(response);
            _fillNotifFromObj(ref notification, Json.Deserialize(response));

            Notification resultNotif = null;
            UnityMainThreadDispatch.Send(state => { resultNotif = _instance.WillShow(notification);});

            return resultNotif != null;
        }

        /// <param name="response">OSNotificationOpenedResult</param>
        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onOpened(string response) {
            var notifOpenResult = JsonUtility.FromJson<NotificationClickedResult>(response);

            if (Json.Deserialize(response) is Dictionary<string, object> resultDict && resultDict.ContainsKey("notification"))
                _fillNotifFromObj(ref notifOpenResult.notification, resultDict["notification"]);

            if (OneSignal.DidInitialize)
                UnityMainThreadDispatch.Post(state => _instance.Clicked?.Invoke(notifOpenResult));
            else {
                void invokeOpened(string appId) {
                    OneSignal.OnInitialize -= invokeOpened;
                    UnityMainThreadDispatch.Post(state => _instance.Clicked?.Invoke(notifOpenResult));
                }
                
                OneSignal.OnInitialize += invokeOpened; 
            }
        }

        private static void _fillNotifFromObj(ref Notification notif, object notifObj) {
            if (!(notifObj is Dictionary<string, object> notifDict)) 
                return;
            
            if (notifDict.ContainsKey("additionalData"))
                notif.additionalData = notifDict["additionalData"] as Dictionary<string, object>;
            
            if (notifDict.ContainsKey("attachments"))
                notif.attachments = notifDict["attachments"] as Dictionary<string, object>;

            if (notifDict.ContainsKey("rawPayload") && notifDict["rawPayload"] is Dictionary<string, object> payloadDict)
                notif.rawPayload = Json.Serialize(payloadDict);
        }

        [Serializable] private sealed class PermissionState { // temp
            public bool reachable;
        }

        [AOT.MonoPInvokeCallback(typeof(BooleanResponseDelegate))]
        private static void BooleanCallbackProxy(int hashCode, bool response)
            => WaitingProxy.ResolveCallbackProxy(hashCode, response);
    }
}