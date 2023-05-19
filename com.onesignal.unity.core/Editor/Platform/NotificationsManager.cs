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
using System.Threading.Tasks;
using OneSignalSDK.Notifications.Models;

#pragma warning disable 0067 // the event 'x' is never used
namespace OneSignalSDK.Notifications {
    internal sealed class NotificationsManager : INotificationsManager {
        public event EventHandler<NotificationWillDisplayEventArgs> ForegroundWillDisplay;
        public event EventHandler<NotificationClickEventArgs> Clicked;
        public event EventHandler<NotificationPermissionChangedEventArgs> PermissionChanged;

        public bool Permission { get; }

        public NotificationPermission PermissionNative { get; }

        public Task<bool> RequestPermissionAsync(bool fallbackToSettings){
            return Task.FromResult(false);
        }

        public void ClearAllNotifications() {

        }
    }
}