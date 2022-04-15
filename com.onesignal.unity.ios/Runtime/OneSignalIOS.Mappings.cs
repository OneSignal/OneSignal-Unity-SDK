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
using System.Collections.Generic;
using UnityEngine;

namespace OneSignalSDK {
    public sealed partial class OneSignalIOS : OneSignal {
        [Serializable] private sealed class DeviceState {
            public long notificationPermissionStatus;
            
            public string userId;
            public string pushToken;
            public bool isSubscribed;
            public bool isPushDisabled;
            
            public string emailUserId;
            public string emailAddress;
            public bool isEmailSubscribed;
            
            public string smsUserId;
            public string smsNumber;
            public bool isSMSSubscribed;

            public static implicit operator PushSubscriptionState(DeviceState source)
                => new PushSubscriptionState {
                    userId = source.userId,
                    pushToken = source.pushToken,
                    isSubscribed = source.isSubscribed,
                    isPushDisabled = source.isPushDisabled,
                };

            public static implicit operator EmailSubscriptionState(DeviceState source)
                => new EmailSubscriptionState {
                    emailUserId = source.emailUserId,
                    emailAddress = source.emailAddress,
                    isSubscribed = source.isEmailSubscribed,
                };

            public static implicit operator SMSSubscriptionState(DeviceState source)
                => new SMSSubscriptionState {
                    smsUserId = source.smsUserId,
                    smsNumber = source.smsNumber,
                    isSubscribed = source.isSMSSubscribed,
                };
        }
        
        [Serializable] private sealed class NotificationPermissionState {
            public long status;
            public bool provisional;
            public bool hasPrompted;

            public static implicit operator NotificationPermission(NotificationPermissionState source)
                => (NotificationPermission)source.status;
        }

        private static void _fillNotifFromObj(ref Notification notif, object notifObj) {
            if (!(notifObj is Dictionary<string, object> notifDict)) 
                return;
            
            if (notifDict.ContainsKey("additionalData"))
                notif.additionalData = notifDict["additionalData"] as Dictionary<string, object>;
            
            if (notifDict.ContainsKey("attachments"))
                notif.attachments = notifDict["attachments"] as Dictionary<string, object>;

            if (notifDict.ContainsKey("rawPayload") && notifDict["rawPayload"] is Dictionary<string, object> payloadDict)
                notif.rawPayload = Json.Serialize(payloadDict);
        }
    }
}