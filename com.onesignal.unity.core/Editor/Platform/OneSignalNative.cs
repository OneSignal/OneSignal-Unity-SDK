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
using OneSignalSDKNew.Notifications;
using OneSignalSDKNew.InAppMessages;
using OneSignalSDKNew.Debug;
using OneSignalSDKNew.Debug.Utilities;
using OneSignalSDKNew.Location;
using OneSignalSDKNew.Session;
using OneSignalSDKNew.User;

namespace OneSignalSDKNew { // TODO: Change namespace to OneSignalSDK and delete OneSignalNativeOld, OneSignalNativeInitOld
    /// <summary>
    /// Implementationless variation of the OneSignal SDK so that it "runs" in the Editor
    /// </summary>
    internal sealed class OneSignalNative : OneSignal {
        private UserManager _user = new UserManager();
        private SessionManager _session = new SessionManager();
        private NotificationsManager _notifications = new NotificationsManager();
        private LocationManager _location = new LocationManager();
        private InAppMessagesManager _iams = new InAppMessagesManager();
        private DebugManager _debug = new DebugManager();

        public override IUserManager User {
            get => _user;
        }
        
        public override ISessionManager Session {
            get => _session;
        }
        
        public override INotificationsManager Notifications{
            get => _notifications;
        }
        
        public override ILocationManager Location {
            get => _location;
        }

        public override IInAppMessagesManager InAppMessages {
            get => _iams;
        }

        public override IDebugManager Debug {
            get => _debug;
        }

        public override bool PrivacyConsent { get; set; }

        public override bool RequiresPrivacyConsent { get; set; }

        public override void SetLaunchURLsInApp(bool launchInApp) {

        }

        public override void Initialize(string appId) {
            if (string.IsNullOrEmpty(appId)) {
                SDKDebug.Error("appId is null or empty");
                return;
            }
            
            SDKDebug.Warn("Native SDK is placeholder. Please run on supported platform (iOS or Android).");
        }

        public override Task LoginAsync(string externalId, string jwtBearerToken = null) {
            return Task.FromResult(false);
        }

        public override void LogoutAsync() {

        }
    }
}