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
using System.Linq;
using System.Collections.Generic;
using OneSignalSDK.Notifications.Models;

namespace OneSignalSDK.Notifications.Internal {
    [Serializable] public sealed class ActionButton : IActionButton {
        public string Id => id;
        public string Text => text;
        public string Icon => icon;

        #region Native Field Handling
            public string id;
            public string text;
            public string icon;
        #endregion
    }

    /// <summary>
    /// If a background image was set, this object will be available
    /// </summary>
    /// <remarks>Android only</remarks>
    [Serializable] public sealed class BackgroundImageLayout : IBackgroundImageLayout {
        public string Image => image;
        public string TitleTextColor => titleTextColor;
        public string BodyTextColor => bodyTextColor;

        #region Native Field Handling
            public string image;
            public string titleTextColor;
            public string bodyTextColor;
        #endregion
    }
    
    /// <summary>
    /// See full documentation at
    /// https://documentation.onesignal.com/docs/mobile-push-notifications-guide#notification-payload-and-methods
    /// </summary>
    [Serializable] public class NotificationBase : INotificationBase {
        /// <summary>
        /// OneSignal notification UUID
        /// </summary>
        public string NotificationId => notificationId;
        
        /// <summary>
        /// Name of Template from <a href="https://documentation.onesignal.com/docs/templates">Templates</a>
        /// </summary>
        public string TemplateName => templateName;
        
        /// <summary>
        /// Unique Template Identifier from <a href="https://documentation.onesignal.com/docs/templates">Templates</a>
        /// </summary>
        public string TemplateId => templateId;
        
        /// <summary>
        /// Title text of the notification
        /// </summary>
        public string Title => title;
        
        /// <summary>
        /// Body text of the notification
        /// </summary>
        public string Body => body;

        /// <summary>
        /// URL opened when opening the notification
        /// </summary>
        public string LaunchURL => launchURL;
        
        /// <summary>
        /// Sound resource played when the notification is shown
        /// https://documentation.onesignal.com/docs/customize-notification-sounds
        /// </summary>
        public string Sound => sound;

        /// <summary>
        /// Collapse id for the notification
        /// </summary>
        public string CollapseId => collapseId;
        
        /// <summary>
        /// Gets custom additional data that was sent with the notification. Set on the dashboard under
        /// Options > Additional Data or with the data field on the REST API.
        /// </summary>
        public IDictionary<string, object> AdditionalData => additionalData;

        /// <summary>
        /// List of action buttons on the notification
        /// </summary>
        public List<IActionButton> ActionButtons => actionButtons.ToList<IActionButton>();

        /// <summary>
        /// Raw JSON payload string received from OneSignal
        /// </summary>
        public string RawPayload => rawPayload;

    #region Android
        /// <summary>
        /// Unique Android Native API identifier
        /// </summary>
        /// <remarks>Android only</remarks>
        public int AndroidNotificationId => androidNotificationId;

        /// <summary>
        /// Small icon resource name set on the notification
        /// </summary>
        /// <remarks>Android only</remarks>
        public string SmallIcon => smallIcon;
        
        /// <summary>
        /// Large icon resource name set on the notification
        /// </summary>
        /// <remarks>Android only</remarks>
        public string LargeIcon => largeIcon;
        
        /// <summary>
        /// Big picture image set on the notification
        /// </summary>
        /// <remarks>Android only</remarks>
        public string BigPicture => bigPicture;
        
        /// <summary>
        /// Accent color shown around small notification icon on Android 5+ devices. ARGB format.
        /// </summary>
        /// <remarks>Android 5+ only</remarks>
        public string SmallIconAccentColor => smallIconAccentColor;
        
        /// <summary>
        /// LED string. Devices that have a notification LED will blink in this color. ARGB format.
        /// </summary>
        /// <remarks>Android only</remarks>
        public string LedColor => ledColor;
        
        /// <summary>
        /// Privacy setting for how the notification should be shown on the lockscreen of Android 5+ devices
        /// </summary>
        /// <remarks>Android 5+ only</remarks>
        public LockScreenVisibility LockScreenVisibility => lockScreenVisibility;
        
        /// <summary>
        /// Notifications with this same key will be grouped together as a single summary notification
        /// </summary>
        /// <remarks>Android only</remarks>
        public string GroupKey => groupKey;
        
        /// <summary>
        /// Summary text displayed in the summary notification
        /// </summary>
        /// <remarks>Android only</remarks>
        public string GroupMessage => groupMessage;
        
        /// <summary>
        /// Google project number the notification was sent under
        /// </summary>
        /// <remarks>Android only</remarks>
        public string FromProjectNumber => fromProjectNumber;
        
        /// <summary>
        /// Priority of the notification. Values range from -2 to 2 (see
        /// https://developer.android.com/reference/androidx/core/app/NotificationCompat for more info)
        /// </summary>
        /// <remarks>Android only</remarks>
        public int Priority => priority;

        /// </summary>
        /// If a background image was set, this object will be available
        /// </summary>
        /// <remarks>Android only</remarks>
        public IBackgroundImageLayout BackgroundImageLayout => backgroundImageLayout;
    #endregion

    #region iOS
        /// <summary>
        /// Message Subtitle, iOS only
        /// </summary>
        /// <remarks>iOS only</remarks>
        public string Subtitle => subtitle;

        /// <summary>
        /// True when the key content-available is set to 1 in the APNS payload.
        /// Used to wake your app when the payload is received.
        /// See Apple's documentation for more details.
        /// https://developer.apple.com/documentation/uikit/uiapplicationdelegate/1623013-application
        /// </summary>
        /// <remarks>iOS only</remarks>
        public bool ContentAvailable => contentAvailable;

        /// <summary>
        /// True when the key content-available is set to 1 in the APNS payload.
        /// Used to wake your Notification Service Extension to modify a notification.
        /// See Apple's documentation for more details.
        /// https://developer.apple.com/documentation/usernotifications/unnotificationserviceextension
        /// </summary>
        /// <remarks>iOS only</remarks>
        public bool MutableContent => mutableContent;

        /// <summary>
        /// iOS Notification category key previously registered to display with
        /// </summary>
        /// <remarks>iOS only</remarks>
        public string Category => category;

        /// <summary>
        /// The badge number assigned to the application icon
        /// </summary>
        /// <remarks>iOS only</remarks>
        public int Badge => badge;

        /// <summary>
        /// The amount to increment the badge icon number
        /// </summary>
        /// <remarks>iOS only</remarks>
        public int BadgeIncrement => badgeIncrement;

        /// <summary>
        /// Groups notifications into threads
        /// </summary>
        /// <remarks>iOS 10+ only</remarks>
        public string ThreadId => threadId;

        /// <summary>
        /// Relevance Score for notification summary
        /// </summary>
        /// <remarks>iOS 15+ only</remarks>
        public double RelevanceScore => relevanceScore;

        /// <summary>
        /// Interruption Level of the notification
        /// </summary>
        /// <remarks>iOS 15+ only</remarks>
        public string InterruptionLevel => interruptionLevel;

        /// <summary>
        /// Attachments sent as part of the rich notification
        /// </summary>
        /// <remarks>iOS 10+ only</remarks>
        public IDictionary<string, object> Attachments => attachments;
    #endregion

        #region Native Field Handling
            public string notificationId;
            public string templateName;
            public string templateId;
            public string title;
            public string body;
            public string launchURL;
            public string sound;
            public string collapseId;
            public IDictionary<string, object> additionalData;
            public List<ActionButton> actionButtons;
            public string rawPayload;
            public int androidNotificationId;
            public string smallIcon;
            public string largeIcon;
            public string bigPicture;
            public string smallIconAccentColor;
            public string ledColor;
            public LockScreenVisibility lockScreenVisibility;
            public string groupKey;
            public string groupMessage;
            public string fromProjectNumber;
            public int priority;
            public BackgroundImageLayout backgroundImageLayout;
            public string subtitle;
            public bool contentAvailable;
            public bool mutableContent;
            public string category;
            public int badge;
            public int badgeIncrement;
            public string threadId;
            public double relevanceScore;
            public string interruptionLevel;
            public IDictionary<string, object> attachments;
        #endregion
    }

    [Serializable] public class Notification : NotificationBase, INotification {
        /// <summary>
        /// Gets the notification payloads a summary notification was created from
        /// </summary>
        /// <remarks>Android only</remarks>
        public List<INotificationBase> GroupedNotifications => groupedNotifications.ToList<INotificationBase>();

        #region Native Field Handling
            public List<NotificationBase> groupedNotifications;
        #endregion
    }
}
