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

using System.Collections.Generic;
using Laters;
using UnityEngine;

namespace OneSignalSDK {
    public sealed partial class OneSignalIOS : OneSignal {
        private delegate void BooleanResponseDelegate(int hashCode, bool response);
        private delegate void StringResponseDelegate(int hashCode, string response);
        
        private delegate void BooleanListenerDelegate(bool response);
        private delegate void StringListenerDelegate(string response);
        private delegate void StateListenerDelegate(string current, string previous);

        private delegate bool NotificationWillShowInForegroundDelegate(string notification);

        private static readonly Dictionary<int, ILater> WaitingProxies = new Dictionary<int, ILater>();

        private static (Later<TResult> proxy, int hashCode) _setupProxy<TResult>() {
            var proxy    = new Later<TResult>();
            var hashCode = proxy.GetHashCode();
            WaitingProxies[hashCode] = proxy;
            return (proxy, hashCode);
        }

        private static void ResolveCallbackProxy<TResponse>(int hashCode, TResponse response) {
            if (!WaitingProxies.ContainsKey(hashCode))
                return;
            
            if (WaitingProxies[hashCode] is Later<TResponse> later)
                later.Complete(response);
            
            WaitingProxies.Remove(hashCode);
        }

        [AOT.MonoPInvokeCallback(typeof(BooleanResponseDelegate))]
        private static void BooleanCallbackProxy(int hashCode, bool response)
            => ResolveCallbackProxy(hashCode, response);

        [AOT.MonoPInvokeCallback(typeof(StringResponseDelegate))]
        private static void StringCallbackProxy(int hashCode, string response)
            => ResolveCallbackProxy(hashCode, response);
        
        /*
         * Global Callbacks
         */

        private static OneSignalIOS _instance;

        /// <summary>
        /// Used to provide a reference for and sets up the global callbacks
        /// </summary>
        public OneSignalIOS() {
            if (_instance != null)
                SDKDebug.Error("Additional instance of OneSignalIOS created.");

            _setNotificationReceivedCallback(_onNotificationReceived);
            _setNotificationOpenedCallback(_onNotificationOpened);
            
            _setInAppMessageWillDisplayCallback(_onInAppMessageWillDisplay);
            _setInAppMessageDidDisplayCallback(_onInAppMessageDidDisplay);
            _setInAppMessageWillDismissCallback(_onInAppMessageWillDismiss);
            _setInAppMessageDidDismissCallback(_onInAppMessageDidDismiss);
            _setInAppMessageClickedCallback(_onInAppMessageClicked);
            
            _setPermissionStateChangedCallback(_onPermissionStateChanged);
            _setSubscriptionStateChangedCallback(_onSubscriptionStateChanged);
            _setEmailSubscriptionStateChangedCallback(_onEmailSubscriptionStateChanged);
            _setSMSSubscriptionStateChangedCallback(_onSMSSubscriptionStateChanged);
            
            _instance = this;
        }

        /// <param name="response">OSNotification</param>
        [AOT.MonoPInvokeCallback(typeof(NotificationWillShowInForegroundDelegate))]
        private static bool _onNotificationReceived(string response) {
            if (_instance.NotificationWillShow == null)
                return true;

            var notification = JsonUtility.FromJson<Notification>(response);
            _fillNotifFromObj(ref notification, Json.Deserialize(response));
            
            var resultNotif = _instance.NotificationWillShow(notification);
            return resultNotif != null;
        }

        /// <param name="response">OSNotificationOpenedResult</param>
        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onNotificationOpened(string response) {
            var notifOpenResult = JsonUtility.FromJson<NotificationOpenedResult>(response);

            if (Json.Deserialize(response) is Dictionary<string, object> resultDict && resultDict.ContainsKey("notification"))
                _fillNotifFromObj(ref notifOpenResult.notification, resultDict["notification"]);

            if (DidInitialize)
                _instance.NotificationOpened?.Invoke(notifOpenResult);
            else {
                void invokeOpened(string appId) {
                    OnInitialize -= invokeOpened;
                    _instance.NotificationOpened?.Invoke(notifOpenResult);
                }
                
                OnInitialize += invokeOpened; 
            }
        }
        
        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onInAppMessageWillDisplay(string response)
            => _instance.InAppMessageWillDisplay?.Invoke(new InAppMessage { messageId = response });

        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onInAppMessageDidDisplay(string response)
            => _instance.InAppMessageDidDisplay?.Invoke(new InAppMessage { messageId = response });

        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onInAppMessageWillDismiss(string response)
            => _instance.InAppMessageWillDismiss?.Invoke(new InAppMessage { messageId = response });

        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onInAppMessageDidDismiss(string response)
            => _instance.InAppMessageDidDismiss?.Invoke(new InAppMessage { messageId = response });

        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onInAppMessageClicked(string response)
            => _instance.InAppMessageTriggeredAction?.Invoke(JsonUtility.FromJson<InAppMessageAction>(response));

        [AOT.MonoPInvokeCallback(typeof(StateListenerDelegate))]
        private static void _onPermissionStateChanged(string current, string previous) {
            var curr = JsonUtility.FromJson<NotificationPermissionState>(current);
            var prev = JsonUtility.FromJson<NotificationPermissionState>(previous);
            _instance.NotificationPermissionChanged?.Invoke(curr, prev);
        }
        
        [AOT.MonoPInvokeCallback(typeof(StateListenerDelegate))]
        private static void _onSubscriptionStateChanged(string current, string previous) {
            var curr = JsonUtility.FromJson<PushSubscriptionState>(current);
            var prev = JsonUtility.FromJson<PushSubscriptionState>(previous);
            _instance.PushSubscriptionStateChanged?.Invoke(curr, prev);
        }
        
        [AOT.MonoPInvokeCallback(typeof(StateListenerDelegate))]
        private static void _onEmailSubscriptionStateChanged(string current, string previous) {
            var curr = JsonUtility.FromJson<EmailSubscriptionState>(current);
            var prev = JsonUtility.FromJson<EmailSubscriptionState>(previous);
            _instance.EmailSubscriptionStateChanged?.Invoke(curr, prev);
        }
        
        [AOT.MonoPInvokeCallback(typeof(StateListenerDelegate))]
        private static void _onSMSSubscriptionStateChanged(string current, string previous) {
            var curr = JsonUtility.FromJson<SMSSubscriptionState>(current);
            var prev = JsonUtility.FromJson<SMSSubscriptionState>(previous);
            _instance.SMSSubscriptionStateChanged?.Invoke(curr, prev);
        }
    }
}