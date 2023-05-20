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
using UnityEngine;
using OneSignalSDK.Notifications;
using OneSignalSDK.InAppMessages;
using OneSignalSDK.Debug;
using OneSignalSDK.Debug.Utilities;
using OneSignalSDK.Location;
using OneSignalSDK.Session;
using OneSignalSDK.User;
using OneSignalSDK.LiveActivities;
using OneSignalSDK.Android.Notifications;
using OneSignalSDK.Android.InAppMessages;
using OneSignalSDK.Android.Debug;
using OneSignalSDK.Android.Location;
using OneSignalSDK.Android.Session;
using OneSignalSDK.Android.User;
using OneSignalSDK.Android.LiveActivities;

namespace OneSignalSDK.Android {
    public sealed partial class OneSignalAndroid : OneSignalPlatform {
        private const string SDKPackage = "com.onesignal";
        private const string SDKClassName = "OneSignal";
        private const string QualifiedSDKClass = SDKPackage + "." + SDKClassName;

        private readonly AndroidJavaClass _sdkClass = new AndroidJavaClass(QualifiedSDKClass);

        private readonly AndroidJavaClass _sdkWrapperClass = new AndroidJavaClass(SDKPackage + ".common.OneSignalWrapper");

        private static OneSignalAndroid _instance;

        private AndroidUserManager _user;
        private AndroidSessionManager _session;
        private AndroidNotificationsManager _notifications;
        private AndroidLocationManager _location;
        private AndroidInAppMessagesManager _inAppMessages;
        private AndroidDebugManager _debug;
        private AndroidLiveActivitiesManager _liveActivities;

        /// <summary>
        /// Used to provide a reference for the global callbacks
        /// </summary>
        public OneSignalAndroid() {
            if (_instance != null)
                SDKDebug.Error("Additional instance of OneSignalAndroid created.");

            _instance = this;
            _debug = new AndroidDebugManager(_sdkClass);
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
            set => _sdkClass.CallStatic("setConsentGiven", value);
        }

        public override bool ConsentRequired {
            set => _sdkClass.CallStatic("setConsentRequired", value);
        }

        public override void SetLaunchURLsInApp(bool launchInApp)
            => SDKDebug.Warn("This feature is only available for iOS.");

        public override void Initialize(string appId) {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            _sdkWrapperClass.CallStatic("setSdkType", "unity");
            _sdkWrapperClass.CallStatic("setSdkVersion", VersionHeader);

            _sdkClass.CallStatic("initWithContext", activity, appId);

            if (_inAppMessages == null) {
                _inAppMessages = new AndroidInAppMessagesManager(_sdkClass);
                _inAppMessages.Initialize();
            }

            if (_notifications == null) {
                _notifications = new AndroidNotificationsManager(_sdkClass);
                _notifications.Initialize();
            }

            if (_user == null) {
                _user = new AndroidUserManager(_sdkClass);
                _user.Initialize();
            }

            if (_location == null) {
                _location = new AndroidLocationManager(_sdkClass);
            }

            if (_session == null) {
                _session = new AndroidSessionManager(_sdkClass);
            }

            if (_liveActivities == null) {
                _liveActivities = new AndroidLiveActivitiesManager();
            }

            _completedInit(appId);
        }

        public override void Login(string externalId, string jwtBearerToken = null) {
            _sdkClass.CallStatic("login", externalId, jwtBearerToken);
        }

        public override void Logout() {
            _sdkClass.CallStatic("logout");
        }
    }
}