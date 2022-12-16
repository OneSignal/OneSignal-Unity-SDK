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
using System.Threading.Tasks;
using UnityEngine;
using OneSignalSDKNew.Notifications;
using OneSignalSDKNew.InAppMessages;
using OneSignalSDKNew.Debug;
using OneSignalSDKNew.Location;
using OneSignalSDKNew.Session;
using OneSignalSDKNew.User;

namespace OneSignalSDKNew { // TODO: Change namespace to OneSignalSDK and delete OneSignalOld, OneSignalInternalOld
    /// <summary>
    /// OneSignal SDK for Unity
    /// </summary>
    public abstract partial class OneSignal {
        public const string Version = "3.0.10";

        /// <summary>
        /// The default static instance of the OneSignal Unity SDK
        /// </summary>
        public static OneSignal Default {
            get => _getDefaultInstance();
            internal set {
                UnityEngine.Debug.Log($"[OneSignal] OneSignal.Default set to platform SDK {value.GetType()}. Current version is {Version}");
                _default = value;
            }
        }

        /// <summary>
        /// The user manager for accessing user-scoped management.  Initialized only after [initWithContext]
        /// has been called, and initialized with a device-scoped user until (or if) [login] has been
        /// called.
        /// </summary>
        public abstract IUserManager User { get; }

        /// <summary>
        /// The session manager for accessing session-scoped management.  Initialized only after [initWithContext]
        /// has been called.
        /// </summary>
        public abstract ISessionManager Session { get; }

        /// <summary>
        /// The notification manager for accessing device-scoped notification management. Initialized
        /// only after [initWithContext] has been called.
        /// </summary>
        public abstract INotificationsManager Notifications{ get; }

        /// <summary>
        /// The notification manager for accessing device-scoped notification management. Initialized
        /// only after [initWithContext] has been called.
        /// </summary>
        public abstract ILocationManager Location { get; }

        /// <summary>
        /// The In-App Messaging manager for accessing device-scoped IAP management. Initialized
        /// only after [initWithContext] has been called.
        /// </summary>
        public abstract IInAppMessagesManager InAppMessages { get; }

        /// <summary>
        /// Access to debug the SDK in the additional information is required to diagnose any
        /// SDK-related issues.  Initialized immediately (can be used prior to [initWithContext]).
        /// 
        /// WARNING: This should not be used in a production setting.
        /// 
        /// </summary>
        public abstract IDebugManager Debug { get; }

        /// <summary>
        /// Provides privacy consent. OneSignal Unity SDK will not initialize until this is true.
        /// </summary>
        public abstract bool PrivacyConsent { get; set; }

        /// <summary>
        /// Allows you to delay the initialization of the SDK until <see cref="PrivacyConsent"/> is set to true. Must
        /// be set before <see cref="Initialize"/> is called.
        /// </summary>
        public abstract bool RequiresPrivacyConsent { get; set; }

        /// <summary>
        /// Used to set if launch URLs should be opened in safari or within the application. Make sure to set before 
        /// <see cref="Initialize"/> is called.
        /// </summary>
        /// <remarks>iOS Only</remarks>
        public abstract void SetLaunchURLsInApp(bool launchInApp); // TODO: after init iOS

        /// <summary>
        /// Starts the OneSignal SDK
        /// </summary>
        /// <param name="appId">Your application id from the OneSignal dashboard</param>
        public abstract void Initialize(string appId);

        /// <summary>
        /// Login to OneSignal under the user identified by the [externalId] provided. The act of
        /// logging a user into the OneSignal SDK will switch the [user] context to that specific user.
        /// 
        /// * If the [externalId] exists the user will be retrieved and the context set from that
        ///   user information. If operations have already been performed under a guest user, they
        ///   *will not* be applied to the now logged in user (they will be lost).
        /// * If the [externalId] does not exist the user will be created and the context set from
        ///   the current local state. If operations have already been performed under a guest user
        ///   those operations *will* be applied to the newly created user.
        /// 
        /// *Push Notifications and In-App Messaging*
        /// Logging in a new user will automatically transfer push notification and in-app messaging
        /// subscriptions from the current user (if there is one) to the newly logged in user.  This is
        /// because both Push and IAM are owned by the device.
        /// </summary>
        /// <param name="externalId">The external ID of the user that is to be logged in.</param>
        /// <param name="jwtBearerToken">
        /// The optional JWT bearer token generated by your backend to establish
        /// trust for the login operation.  Required when identity verification has been enabled. See
        /// [Identity Verification | OneSignal](https://documentation.onesignal.com/docs/identity-verification)
        /// </param>
        public abstract Task LoginAsync(string externalId, string jwtBearerToken = null);

        /// <summary>
        /// Logout the user previously logged in via [login]. The [user] property now references
        /// a new device-scoped user. A device-scoped user has no user identity that can later
        /// be retrieved, except through this device as long as the app remains installed and the app
        /// data is not cleared.
        /// </summary>
        public abstract void Logout();
    }
}