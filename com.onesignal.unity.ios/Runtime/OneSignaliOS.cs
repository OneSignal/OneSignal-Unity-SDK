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
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System.Runtime.InteropServices;
using OneSignalSDK.iOS.Notifications;
using OneSignalSDK.iOS.InAppMessages;
using OneSignalSDK.iOS.Debug;
using OneSignalSDK.iOS.Location;
using OneSignalSDK.iOS.Session;
using OneSignalSDK.iOS.User;
using OneSignalSDK.iOS.Utilities;
using OneSignalSDK.Notifications;
using OneSignalSDK.InAppMessages;
using OneSignalSDK.Debug;
using OneSignalSDK.Debug.Utilities;
using OneSignalSDK.Location;
using OneSignalSDK.Session;
using OneSignalSDK.User;

namespace OneSignalSDK.iOS {
    public sealed partial class OneSignaliOS : OneSignal {
        [DllImport("__Internal")] private static extern bool _getPrivacyConsent();
        [DllImport("__Internal")] private static extern void _setPrivacyConsent(bool consent);
        [DllImport("__Internal")] private static extern bool _getRequiresPrivacyConsent();
        [DllImport("__Internal")] private static extern void _setRequiresPrivacyConsent(bool required);
        [DllImport("__Internal")] private static extern void _setLaunchURLsInApp(bool launchInApp);
        [DllImport("__Internal")] private static extern void _initialize(string appId);
        [DllImport("__Internal")] private static extern void _login(string externalId);
        [DllImport("__Internal")] private static extern void _loginWithJwtBearerToken(string externalId, string jwtBearerToken);
        [DllImport("__Internal")] private static extern void _logout();
        [DllImport("__Internal")] private static extern void _enterLiveActivity(string activityId, string token, int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _exitLiveActivity(string activityId, int hashCode, BooleanResponseDelegate callback);

        private delegate void BooleanResponseDelegate(int hashCode, bool response);

        private iOSUserManager _user;
        private iOSSessionManager _session;
        private iOSNotificationsManager _notifications;
        private iOSLocationManager _location;
        private iOSInAppMessagesManager _inAppMessages;
        private iOSDebugManager _debug;

        private static OneSignaliOS _instance;

        /// <summary>
        /// Used to provide a reference for and sets up the global callbacks
        /// </summary>
        public OneSignaliOS() {
            if (_instance != null)
                SDKDebug.Error("Additional instance of OneSignaliOS created.");

            _instance = this;
            _debug = new iOSDebugManager();
        }

        public override IUserManager User {
            get => _user;
        }

        public override ISessionManager Session {
            get => _session;
        }

        public override INotificationsManager Notifications {
            get => _notifications;
        }

        public override ILocationManager Location {
            get => _location;
        }

        public override IInAppMessagesManager InAppMessages {
            get => _inAppMessages;
        }

        public override IDebugManager Debug {
            get => _debug;
        }

        public override bool PrivacyConsent {
            get => _getPrivacyConsent();
            set => _setPrivacyConsent(value);
        }

        public override bool RequiresPrivacyConsent {
            get => _getRequiresPrivacyConsent();
            set => _setRequiresPrivacyConsent(value);
        }

        public override void SetLaunchURLsInApp(bool launchInApp)
            => _setLaunchURLsInApp(launchInApp);

        public override void Initialize(string appId) {
            _initialize(appId);

            if (_inAppMessages == null) {
                _inAppMessages = new iOSInAppMessagesManager();
                _inAppMessages.Initialize();
            }

            if (_notifications == null) {
                _notifications = new iOSNotificationsManager();
                _notifications.Initialize();
            }

            if (_user == null) {
                _user = new iOSUserManager();
                _user.Initialize();
            }

            if (_location == null) {
                _location = new iOSLocationManager();
            }

            if (_session == null) {
                _session = new iOSSessionManager();
            }

            _completedInit(appId);
        }

        public override void Login(string externalId, string jwtBearerToken = null) {
            if (jwtBearerToken == null) {
                _login(externalId);
            } else {
                _loginWithJwtBearerToken(externalId, jwtBearerToken);
            }
        }

        public override void Logout() {
            _logout();
        }

        public override async Task<bool> EnterLiveActivityAsync(string activityId, string token) {
            var (proxy, hashCode) = WaitingProxy._setupProxy<bool>();
            _enterLiveActivity(activityId, token, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public override async Task<bool> ExitLiveActivityAsync(string activityId) {
            var (proxy, hashCode) = WaitingProxy._setupProxy<bool>();
            _exitLiveActivity(activityId, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        [AOT.MonoPInvokeCallback(typeof(BooleanResponseDelegate))]
        private static void BooleanCallbackProxy(int hashCode, bool response)
            => WaitingProxy.ResolveCallbackProxy(hashCode, response);
    }
}