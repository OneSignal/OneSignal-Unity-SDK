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
    /// <remarks>Android 5+ only</remarks>
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
    /// If a background image was set, this object will be available
    /// </summary>
    /// <remarks>Android only</remarks>
    [Serializable] public sealed class BackgroundImageLayout {
        public string image;
        public string titleTextColor;
        public string bodyTextColor;
    }
    
    /// <summary>
    /// See full documentation at
    /// https://documentation.onesignal.com/docs/sdk-notification-event-handlers#osnotification-class
    /// </summary>
    [Serializable] public class NotificationBase {
        /// <summary>
        /// OneSignal notification UUID
        /// </summary>
        public string notificationId;
        
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
        /// URL opened when opening the notification
        /// </summary>
        public string launchURL;
        
        /// <summary>
        /// Sound resource played when the notification is shown
        /// https://documentation.onesignal.com/docs/customize-notification-sounds
        /// </summary>
        public string sound;

        /// <summary>
        /// Collapse id for the notification
        /// </summary>
        public string collapseId;
        
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

    #region Android
        /// <summary>
        /// Unique Android Native API identifier
        /// </summary>
        /// <remarks>Android only</remarks>
        public int androidNotificationId;

        /// <summary>
        /// Small icon resource name set on the notification
        /// </summary>
        /// <remarks>Android only</remarks>
        public string smallIcon;
        
        /// <summary>
        /// Large icon resource name set on the notification
        /// </summary>
        /// <remarks>Android only</remarks>
        public string largeIcon;
        
        /// <summary>
        /// Big picture image set on the notification
        /// </summary>
        /// <remarks>Android only</remarks>
        public string bigPicture;
        
        /// <summary>
        /// Accent color shown around small notification icon on Android 5+ devices. ARGB format.
        /// </summary>
        /// <remarks>Android 5+ only</remarks>
        public string smallIconAccentColor;
        
        /// <summary>
        /// LED string. Devices that have a notification LED will blink in this color. ARGB format.
        /// </summary>
        /// <remarks>Android only</remarks>
        public string ledColor;
        
        /// <summary>
        /// Privacy setting for how the notification should be shown on the lockscreen of Android 5+ devices
        /// </summary>
        /// <remarks>Android 5+ only</remarks>
        public LockScreenVisibility lockScreenVisibility;
        
        /// <summary>
        /// Notifications with this same key will be grouped together as a single summary notification
        /// </summary>
        /// <remarks>Android only</remarks>
        public string groupKey;
        
        /// <summary>
        /// Summary text displayed in the summary notification
        /// </summary>
        /// <remarks>Android only</remarks>
        public string groupMessage;
        
        /// <summary>
        /// Google project number the notification was sent under
        /// </summary>
        /// <remarks>Android only</remarks>
        public string fromProjectNumber;
        
        /// <summary>
        /// Priority of the notification. Values range from -2 to 2 (see
        /// https://developer.android.com/reference/androidx/core/app/NotificationCompat for more info)
        /// </summary>
        /// <remarks>Android only</remarks>
        public int priority;

        /// </summary>
        /// If a background image was set, this object will be available
        /// </summary>
        /// <remarks>Android only</remarks>
        public BackgroundImageLayout backgroundImageLayout;
    #endregion

    #region iOS
        /// <summary>
        /// Message Subtitle, iOS only
        /// </summary>
        /// <remarks>iOS only</remarks>
        public string subtitle;

        /// <summary>
        /// True when the key content-available is set to 1 in the APNS payload.
        /// Used to wake your app when the payload is received.
        /// See Apple's documentation for more details.
        /// https://developer.apple.com/documentation/uikit/uiapplicationdelegate/1623013-application
        /// </summary>
        /// <remarks>iOS only</remarks>
        public bool contentAvailable;

        /// <summary>
        /// True when the key content-available is set to 1 in the APNS payload.
        /// Used to wake your Notification Service Extension to modify a notification.
        /// See Apple's documentation for more details.
        /// https://developer.apple.com/documentation/usernotifications/unnotificationserviceextension
        /// </summary>
        /// <remarks>iOS only</remarks>
        public bool mutableContent;

        /// <summary>
        /// iOS Notification category key previously registered to display with
        /// </summary>
        /// <remarks>iOS only</remarks>
        public string category;

        /// <summary>
        /// The badge number assigned to the application icon
        /// </summary>
        /// <remarks>iOS only</remarks>
        public int badge;

        /// <summary>
        /// The amount to increment the badge icon number
        /// </summary>
        /// <remarks>iOS only</remarks>
        public int badgeIncrement;

        /// <summary>
        /// Groups notifications into threads
        /// </summary>
        /// <remarks>iOS 10+ only</remarks>
        public string threadId;

        /// <summary>
        /// Relevance Score for notification summary
        /// </summary>
        /// <remarks>iOS 15+ only</remarks>
        public double relevanceScore;

        /// <summary>
        /// Interruption Level of the notification
        /// </summary>
        /// <remarks>iOS 15+ only</remarks>
        public string interruptionLevel;

        /// <summary>
        /// Attachments sent as part of the rich notification
        /// </summary>
        /// <remarks>iOS 10+ only</remarks>
        public Dictionary<string, object> attachments;
    #endregion
    }

    [Serializable] public sealed class Notification : NotificationBase {
        /// <summary>
        /// Gets the notification payloads a summary notification was created from
        /// </summary>
        /// <remarks>Android only</remarks>
        public List<NotificationBase> groupedNotifications;
    }
}
