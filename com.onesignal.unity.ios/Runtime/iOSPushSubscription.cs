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
using UnityEngine;
using System.Runtime.InteropServices;
using OneSignalSDK.User.Models;
using OneSignalSDK.User.Internal;

using OneSignalSDK.Debug.Utilities;

namespace OneSignalSDK.iOS.User.Models {
    internal sealed class iOSPushSubscription : IPushSubscription {
        [DllImport("__Internal")] private static extern string _oneSignalPushSubscriptionGetId();
        [DllImport("__Internal")] private static extern string _oneSignalPushSubscriptionGetToken();
        [DllImport("__Internal")] private static extern bool _oneSignalPushSubscriptionGetOptedIn();
        [DllImport("__Internal")] private static extern void _oneSignalPushSubscriptionOptIn();
        [DllImport("__Internal")] private static extern void _oneSignalPushSubscriptionOptOut();
        [DllImport("__Internal")] private static extern void _oneSignalPushSubscriptionAddStateChangedCallback(StateListenerDelegate callback);

        public delegate void StateListenerDelegate(string current, string previous);

        public event EventHandler<PushSubscriptionChangedEventArgs> Changed;

        private static iOSPushSubscription _instance;

        public iOSPushSubscription() {
            _instance = this;
        }

        public string Id {
            get => _oneSignalPushSubscriptionGetId();
        }

        public string Token {
            get => _oneSignalPushSubscriptionGetToken();
        }

        public bool OptedIn {
            get => _oneSignalPushSubscriptionGetOptedIn();
        }

        public void OptIn()
            => _oneSignalPushSubscriptionOptIn();

        public void OptOut()
            => _oneSignalPushSubscriptionOptOut();

        public void Initialize() {
            _oneSignalPushSubscriptionAddStateChangedCallback(_onPushSubscriptionStateChanged);
        }

        [AOT.MonoPInvokeCallback(typeof(StateListenerDelegate))]
        private static void _onPushSubscriptionStateChanged(string current, string previous) {
            var curr = JsonUtility.FromJson<PushSubscriptionState>(current);
            var prev = JsonUtility.FromJson<PushSubscriptionState>(previous);

            PushSubscriptionChangedState state = new PushSubscriptionChangedState(prev, curr);
            PushSubscriptionChangedEventArgs args = new PushSubscriptionChangedEventArgs(state);

            EventHandler<PushSubscriptionChangedEventArgs> handler = _instance.Changed;
            if (handler != null)
            {
                UnityMainThreadDispatch.Post(state => handler(_instance, args));
            }
        }
    }
}