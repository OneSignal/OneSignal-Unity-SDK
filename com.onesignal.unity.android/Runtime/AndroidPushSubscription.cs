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
using OneSignalSDK.User.Models;
using OneSignalSDK.Android.Utilities;

namespace OneSignalSDK.Android.User.Models {
    internal sealed class AndroidPushSubscription : IPushSubscription {
        public event SubscriptionChangedDelegate Changed;

        private readonly AndroidJavaObject _user;
        
        public AndroidPushSubscription(AndroidJavaObject user) {
            _user = user;
        }

        private AndroidJavaObject _pushSubscription => _user.Call<AndroidJavaObject>("getPushSubscription");

        public string Id {
            get => _pushSubscription.Call<string>("getId");
        }

        public string Token {
            get => _pushSubscription.Call<string>("getToken");
        }

        public bool OptedIn {
            get => _pushSubscription.Call<bool>("getOptedIn");
        }

        public void OptIn()
            => _pushSubscription.Call("optIn");

        public void OptOut()
            => _pushSubscription.Call("optOut");

        public void Initialize() {
            _pushSubscription.Call("addChangeHandler", new InternalSubscriptionChangedHandler(this));
        }

        private sealed class InternalSubscriptionChangedHandler : OneSignalAndroidJavaProxy {
            private AndroidPushSubscription _parent;

            public InternalSubscriptionChangedHandler(AndroidPushSubscription pushSubscription) : base("user.subscriptions.ISubscriptionChangedHandler") {
                _parent = pushSubscription;
            }

            /// <param name="subscription">ISubscription</param>
            public void onSubscriptionChanged(AndroidJavaObject subscription) {
                string id = subscription.Call<string>("getId");
                bool optedIn = subscription.Call<bool>("getOptedIn");
                string token = subscription.Call<string>("getToken");
                PushSubscriptionState pushSubcriptionState = new PushSubscriptionState(id, optedIn, token);

                UnityMainThreadDispatch.Post(state => _parent.Changed?.Invoke(pushSubcriptionState));
            }
        }
    }
}