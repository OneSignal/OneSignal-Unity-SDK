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

using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OneSignalSDK.Notifications;
using OneSignalSDK.InAppMessages;
using OneSignalSDK.Debug;
using OneSignalSDK.Debug.Utilities;
using OneSignalSDK.Location;
using OneSignalSDK.Session;
using OneSignalSDK.User;
using OneSignalSDK.LiveActivities;
using OneSignalSDK.iOS.Notifications;
using OneSignalSDK.iOS.InAppMessages;
using OneSignalSDK.iOS.Debug;
using OneSignalSDK.iOS.Location;
using OneSignalSDK.iOS.Session;
using OneSignalSDK.iOS.User;
using OneSignalSDK.iOS.LiveActivities;

namespace OneSignalSDK.iOS {
    public sealed partial class OneSignaliOS : OneSignalPlatform {
        [DllImport("__Internal")] private static extern void _oneSignalSetConsentGiven(bool consent);
        [DllImport("__Internal")] private static extern void _oneSignalSetConsentRequired(bool required);
        [DllImport("__Internal")] private static extern void _oneSignalInitialize(string appId);
        [DllImport("__Internal")] private static extern void _oneSignalLogin(string externalId);
        [DllImport("__Internal")] private static extern void _oneSignalLoginWithJwtBearerToken(string externalId, string jwtBearerToken);
        [DllImport("__Internal")] private static extern void _oneSignalLogout();

        private iOSUserManager _user;
        private iOSSessionManager _session;
        private iOSNotificationsManager _notifications;
        private iOSLocationManager _location;
        private iOSInAppMessagesManager _inAppMessages;
        private iOSDebugManager _debug;
        private iOSLiveActivitiesManager _liveActivities;

        private static OneSignaliOS _instance;

        /// <summary>
        /// Used to provide a reference for and sets up the global callbacks
        /// </summary>
        public OneSignaliOS() {
            if (_instance != null) {
                SDKDebug.Error("Additional instance of OneSignaliOS created.");
                return;
            }

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

        public override ILiveActivitiesManager LiveActivities {
            get => _liveActivities;
        }

        public override bool ConsentGiven {
            set => _oneSignalSetConsentGiven(value);
        }

        public override bool ConsentRequired {
            set => _oneSignalSetConsentRequired(value);
        }

        public override void Initialize(string appId) {
            _oneSignalInitialize(appId);

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

            if (_liveActivities == null) {
                _liveActivities = new iOSLiveActivitiesManager();
            }

            _completedInit(appId);
        }

        public override void Login(string externalId, string jwtBearerToken = null) {
            if (jwtBearerToken == null) {
                _oneSignalLogin(externalId);
            } else {
                _oneSignalLoginWithJwtBearerToken(externalId, jwtBearerToken);
            }
        }

        public override void Logout() {
            _oneSignalLogout();
        }
    }
}