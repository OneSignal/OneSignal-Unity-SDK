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

namespace OneSignalSDK {
    /// <summary>
    /// Privacy setting for how the notification should be shown on the lockscreen of Android 5+ devices
    /// </summary>
    public enum LockScreenVisibility {
        Secret = -1,    // Not shown
        Private = 0,    // Contents are hidden
        Public = 1      // (default) Fully visible
    }

    [Serializable] public sealed class ActionButton {
        public string id;
        public string text;
        public string icon;
    }
    
    /// <summary>
    /// See full documentation at
    /// https://documentation.onesignal.com/docs/sdk-notification-event-handlers#osnotification-class
    /// </summary>
    [Serializable] public sealed class Notification {
        /// <summary>
        /// OneSignal notification UUID
        /// </summary>
        public string notificationId;
        
        /// <summary>
        /// Unique Android Native API identifier
        /// </summary>
        public int androidNotificationId;

        /// <summary>
        /// Name of Template from <a href="https://documentation.onesignal.com/docs/templates">Templates</a>
        /// </summary>
        public string templateName;
        
        /// <summary>
        /// Unique Template Identifier from <a href="https://documentation.onesignal.com/docs/templates">Templates</a>
        /// </summary>
        public string templateId;
        
        /// <summary>
        /// Title text of the notification
        /// </summary>
        public string title;
        
        /// <summary>
        /// Body text of the notification
        /// </summary>
        public string body;
        
        /// <summary>
        /// Small icon resource name set on the notification
        /// </summary>
        public string smallIcon;
        
        /// <summary>
        /// Large icon resource name set on the notification.
        /// </summary>
        public string largeIcon;
        
        /// <summary>
        /// Big picture image set on the notification
        /// </summary>
        /// <remarks>iOS 10+ only. Attachments sent as part of the rich notification</remarks>
        public string bigPicture;
        
        /// <summary>
        /// Accent color shown around small notification icon on Android 5+ devices. ARGB format.
        /// </summary>
        public string smallIconAccentColor;
        
        /// <summary>
        /// URL opened when opening the notification
        /// </summary>
        public string launchURL;
        
        /// <summary>
        /// Sound resource played when the notification is shown
        /// https://documentation.onesignal.com/docs/customize-notification-sounds
        /// </summary>
        public string sound;
        
        /// <summary>
        /// LED string. Devices that have a notification LED will blink in this color. ARGB format.
        /// </summary>
        public string ledColor;
        
        /// <summary>
        /// Privacy setting for how the notification should be shown on the lockscreen of Android 5+ devices
        /// </summary>
        public LockScreenVisibility lockScreenVisibility;
        
        /// <summary>
        /// Notifications with this same key will be grouped together as a single summary notification
        /// </summary>
        public string groupKey;
        
        /// <summary>
        /// Summary text displayed in the summary notification
        /// </summary>
        public string groupMessage;
        
        /// <summary>
        /// Google project number the notification was sent under
        /// </summary>
        public string fromProjectNumber;
        
        /// <summary>
        /// Collapse id for the notification
        /// </summary>
        public string collapseId;
        
        /// <summary>
        /// Priority of the notification. Values range from -2 to 2 (see
        /// https://developer.android.com/reference/androidx/core/app/NotificationCompat for more info)
        /// </summary>
        /// <remarks>Android only</remarks>
        public int priority;

        /// <summary>
        /// Gets custom additional data that was sent with the notification. Set on the dashboard under
        /// Options > Additional Data or with the data field on the REST API.
        /// </summary>
        public Dictionary<string, object> additionalData;

        /// <summary>
        /// List of action buttons on the notification
        /// </summary>
        public List<ActionButton> actionButtons;

        /// <summary>
        /// Raw JSON payload string received from OneSignal
        /// </summary>
        public string rawPayload;
    }
}
