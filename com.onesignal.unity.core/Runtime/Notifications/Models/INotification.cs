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

namespace OneSignalSDK.Notifications.Models {
    /// <summary>
    /// Privacy setting for how the notification should be shown on the lockscreen of Android 5+ devices
    /// </summary>
    /// <remarks>Android 5+ only</remarks>
    public enum LockScreenVisibility {
        Secret = -1,    // Not shown
        Private = 0,    // Contents are hidden
        Public = 1      // (default) Fully visible
    }

    public interface IActionButton {
        string Id { get; }
        string Text { get; }
        string Icon { get; }
    }

    /// <summary>
    /// If a background image was set, this object will be available
    /// </summary>
    /// <remarks>Android only</remarks>
    public interface IBackgroundImageLayout {
        string Image { get; }
        string TitleTextColor { get; }
        string BodyTextColor { get; }
    }
    
    /// <summary>
    /// See full documentation at
    /// https://documentation.onesignal.com/docs/mobile-push-notifications-guide#notification-payload-and-methods
    /// </summary>
    public interface INotificationBase {
        /// <summary>
        /// OneSignal notification UUID
        /// </summary>
        string NotificationId { get; }
        
        /// <summary>
        /// Name of Template from <a href="https://documentation.onesignal.com/docs/templates">Templates</a>
        /// </summary>
        string TemplateName { get; }
        
        /// <summary>
        /// Unique Template Identifier from <a href="https://documentation.onesignal.com/docs/templates">Templates</a>
        /// </summary>
        string TemplateId { get; }
        
        /// <summary>
        /// Title text of the notification
        /// </summary>
        string Title { get; }
        
        /// <summary>
        /// Body text of the notification
        /// </summary>
        string Body { get; }

        /// <summary>
        /// URL opened when opening the notification
        /// </summary>
        string LaunchURL { get; }
        
        /// <summary>
        /// Sound resource played when the notification is shown
        /// https://documentation.onesignal.com/docs/customize-notification-sounds
        /// </summary>
        string Sound { get; }

        /// <summary>
        /// Collapse id for the notification
        /// </summary>
        string CollapseId { get; }
        
        /// <summary>
        /// Gets custom additional data that was sent with the notification. Set on the dashboard under
        /// Options > Additional Data or with the data field on the REST API.
        /// </summary>
        IDictionary<string, object> AdditionalData { get; }

        /// <summary>
        /// List of action buttons on the notification
        /// </summary>
        List<IActionButton> ActionButtons { get; }

        /// <summary>
        /// Raw JSON payload string received from OneSignal
        /// </summary>
        string RawPayload { get; }

    #region Android
        /// <summary>
        /// Unique Android Native API identifier
        /// </summary>
        /// <remarks>Android only</remarks>
        int AndroidNotificationId { get; }

        /// <summary>
        /// Small icon resource name set on the notification
        /// </summary>
        /// <remarks>Android only</remarks>
        string SmallIcon { get; }
        
        /// <summary>
        /// Large icon resource name set on the notification
        /// </summary>
        /// <remarks>Android only</remarks>
        string LargeIcon { get; }
        
        /// <summary>
        /// Big picture image set on the notification
        /// </summary>
        /// <remarks>Android only</remarks>
        string BigPicture { get; }
        
        /// <summary>
        /// Accent color shown around small notification icon on Android 5+ devices. ARGB format.
        /// </summary>
        /// <remarks>Android 5+ only</remarks>
        string SmallIconAccentColor { get; }
        
        /// <summary>
        /// LED string. Devices that have a notification LED will blink in this color. ARGB format.
        /// </summary>
        /// <remarks>Android only</remarks>
        string LedColor { get; }
        
        /// <summary>
        /// Privacy setting for how the notification should be shown on the lockscreen of Android 5+ devices
        /// </summary>
        /// <remarks>Android 5+ only</remarks>
        LockScreenVisibility LockScreenVisibility { get; }
        
        /// <summary>
        /// Notifications with this same key will be grouped together as a single summary notification
        /// </summary>
        /// <remarks>Android only</remarks>
        string GroupKey { get; }
        
        /// <summary>
        /// Summary text displayed in the summary notification
        /// </summary>
        /// <remarks>Android only</remarks>
        string GroupMessage { get; }
        
        /// <summary>
        /// Google project number the notification was sent under
        /// </summary>
        /// <remarks>Android only</remarks>
        string FromProjectNumber { get; }
        
        /// <summary>
        /// Priority of the notification. Values range from -2 to 2 (see
        /// https://developer.android.com/reference/androidx/core/app/NotificationCompat for more info)
        /// </summary>
        /// <remarks>Android only</remarks>
        int Priority { get; }

        /// </summary>
        /// If a background image was set, this object will be available
        /// </summary>
        /// <remarks>Android only</remarks>
        IBackgroundImageLayout BackgroundImageLayout { get; }
    #endregion

    #region iOS
        /// <summary>
        /// Message Subtitle, iOS only
        /// </summary>
        /// <remarks>iOS only</remarks>
        string Subtitle { get; }

        /// <summary>
        /// True when the key content-available is set to 1 in the APNS payload.
        /// Used to wake your app when the payload is received.
        /// See Apple's documentation for more details.
        /// https://developer.apple.com/documentation/uikit/uiapplicationdelegate/1623013-application
        /// </summary>
        /// <remarks>iOS only</remarks>
        bool ContentAvailable { get; }

        /// <summary>
        /// True when the key content-available is set to 1 in the APNS payload.
        /// Used to wake your Notification Service Extension to modify a notification.
        /// See Apple's documentation for more details.
        /// https://developer.apple.com/documentation/usernotifications/unnotificationserviceextension
        /// </summary>
        /// <remarks>iOS only</remarks>
        bool MutableContent { get; }

        /// <summary>
        /// iOS Notification category key previously registered to display with
        /// </summary>
        /// <remarks>iOS only</remarks>
        string Category { get; }

        /// <summary>
        /// The badge number assigned to the application icon
        /// </summary>
        /// <remarks>iOS only</remarks>
        int Badge { get; }

        /// <summary>
        /// The amount to increment the badge icon number
        /// </summary>
        /// <remarks>iOS only</remarks>
        int BadgeIncrement { get; }

        /// <summary>
        /// Groups notifications into threads
        /// </summary>
        /// <remarks>iOS 10+ only</remarks>
        string ThreadId { get; }

        /// <summary>
        /// Relevance Score for notification summary
        /// </summary>
        /// <remarks>iOS 15+ only</remarks>
        double RelevanceScore { get; }

        /// <summary>
        /// Interruption Level of the notification
        /// </summary>
        /// <remarks>iOS 15+ only</remarks>
        string InterruptionLevel { get; }

        /// <summary>
        /// Attachments sent as part of the rich notification
        /// </summary>
        /// <remarks>iOS 10+ only</remarks>
        IDictionary<string, object> Attachments { get; }
    #endregion
    }

    public interface INotification : INotificationBase {
        /// <summary>
        /// Gets the notification payloads a summary notification was created from
        /// </summary>
        /// <remarks>Android only</remarks>
        List<INotificationBase> GroupedNotifications { get; }
    }
}
