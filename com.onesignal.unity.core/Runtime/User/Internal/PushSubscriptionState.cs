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
using OneSignalSDK.User.Models;

namespace OneSignalSDK.User.Internal {
    [Serializable] public sealed class PushSubscriptionState : IPushSubscriptionState {
        /// <summary>
        /// Unique id of this subscription
        /// </summary>
        /// <remarks>See https://documentation.onesignal.com/docs/subscriptions for more information</remarks>
        public string Id => id;
        
        /// <summary>
        /// Whether this subscription is currently active
        /// </summary>
        public bool OptedIn => optedIn;
        
        /// <summary>
        /// The unique token provided by the device's operating system used to send push notifications
        /// </summary>
        public string Token => token;

        #region Native Field Handling
            public string id;
            public bool optedIn;
            public string token;
        #endregion

        public PushSubscriptionState() {}

        public PushSubscriptionState(string id, bool optedIn, string token) {
            this.id = id;
            this.optedIn = optedIn;
            this.token = token;
        }
    }
}