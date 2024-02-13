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
using System.Threading.Tasks;
using UnityEngine;

namespace OneSignalSDK.User.Models {
    /// <summary>
    /// Several states associated with the SDK can be changed in and outside of the application.
    /// </summary>
    public class PushSubscriptionChangedEventArgs : EventArgs
    {
        public PushSubscriptionChangedState State { get; }

        public PushSubscriptionChangedEventArgs(PushSubscriptionChangedState state) {
            State = state;
        }
    }

    /// <summary>
    /// A push subscription allows a user to receive notifications through the push
    /// channel.
    /// </summary>
    public interface IPushSubscription {
        /// <summary>
        /// When this uers's subscription to push notifications has changed
        /// </summary>
        //event SubscriptionChangedDelegate Changed;
        event EventHandler<PushSubscriptionChangedEventArgs> Changed;

        /// <summary>
        /// Unique id of this subscription
        /// </summary>
        /// <remarks>See https://documentation.onesignal.com/docs/subscriptions for more information</remarks>
        string Id { get; }

        /// <summary>
        /// The unique token provided by the device's operating system used to send push notifications
        /// </summary>
        string Token { get; }

        /// <summary>
        /// Whether the user of this subscription is opted-in to received notifications. When true,
        /// the user is able to receive notifications through this subscription. Otherwise, the
        /// user will not receive notifications through this subscription (even when the user has
        /// granted app permission).
        /// </summary>
        bool OptedIn { get; }

        /// <summary>
        /// Opt the user into this push subscription.  If the application does not have permission,
        /// the user will be prompted by Android to permit push permissions.  If the user has
        /// granted app permission, the user will be able to receive push notification.  If the user
        /// rejects app permission, the user will only be able to receive push notifications once
        /// the app permission has been granted.
        /// </summary>
        void OptIn();

        /// <summary>
        /// Opt the user out of this push subscription.  The user will no longer received push
        /// notifications, although the app permission state will not be changed.
        /// </summary>
        void OptOut();
    }
}