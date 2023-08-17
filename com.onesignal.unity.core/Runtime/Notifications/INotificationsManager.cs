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

using System;
using System.Threading.Tasks;
using OneSignalSDK.Notifications.Models;

namespace OneSignalSDK.Notifications {
    /// <summary>
    /// When a push notification has been received and is about to be displayed
    /// </summary>
    public class NotificationWillDisplayEventArgs : EventArgs
    {
        public IDisplayableNotification Notification { get; }

        public NotificationWillDisplayEventArgs(IDisplayableNotification notification) {
            Notification = notification;
        }

        /// <summary>
        /// Prevents OneSignal from displaying the notification automatically.
        /// </summary>
        public virtual void PreventDefault() {

        }
    }

    /// <summary>
    /// When a push notification was acted on by the user.
    /// </summary>
    public class NotificationClickEventArgs : EventArgs
    {
        public INotification Notification { get; }
        public INotificationClickResult Result { get; }

        public NotificationClickEventArgs(INotification notification, INotificationClickResult result) {
            Notification = notification;
            Result = result;
        }
    }

    /// <summary>
    /// When the user enables or disables notifications for your app from the system settings outside of your app. 
    /// </summary>
    public class NotificationPermissionChangedEventArgs : EventArgs
    {
        public bool Permission { get; }

        public NotificationPermissionChangedEventArgs(bool permission) {
            Permission = permission;
        }
    }

    /// <summary>
    /// The entry point to the notification SDK for OneSignal.
    /// </summary>
    public interface INotificationsManager {
        /// <summary>
        /// When a push notification has been received while app is in the foreground
        /// </summary>
        event EventHandler<NotificationWillDisplayEventArgs> ForegroundWillDisplay;

        /// <summary>
        /// When a push notification has been clicked by the user
        /// </summary>
        event EventHandler<NotificationClickEventArgs> Clicked;

        /// <summary>
        /// When this device's permissions for authorization of push notifications have changed.
        /// </summary>
        event EventHandler<NotificationPermissionChangedEventArgs> PermissionChanged;

        /// <summary>
        /// Current status of permissions granted by this device for push notifications
        /// </summary>
        bool Permission { get; }

        /// <summary>
        /// Whether this app can request push notification permission.
        /// </summary>
        bool CanRequestPermission { get; }

        /// <summary>
        /// Native permission of the device
        /// </summary>
        NotificationPermission PermissionNative { get; }

        /// <summary>
        /// Prompt the user for notification permissions.
        /// </summary>
        /// <param name="fallbackToSettings">
        /// Whether to direct the user to this app's settings to drive nabling of notifications,
        /// when the in-app prompting is not possible.
        /// </param>
        /// <returns>
        /// Awaitable boolean of true if the user opted in to push notifications or
        /// false if the user opted out of push notifications.
        /// </returns>
        Task<bool> RequestPermissionAsync(bool fallbackToSettings);

        /// <summary>
        /// Removes all OneSignal app notifications
        /// </summary>
        void ClearAllNotifications();
    }
}