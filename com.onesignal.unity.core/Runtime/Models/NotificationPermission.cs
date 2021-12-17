/*
 * Modified MIT License
 *
 * Copyright 2021 OneSignal
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

namespace OneSignalSDK {
    /// <summary>
    /// Status of ability to send push notifications to the current device as determined by its user
    /// </summary>
    public enum NotificationPermission {
        /// <summary>The user has not yet made a choice regarding whether your app can show notifications.</summary>
        NotDetermined = 0,

        /// <summary>The application is not authorized to post user notifications.</summary>
        Denied,

        /// <summary>The application is authorized to post user notifications.</summary>
        Authorized,

        /// <summary>The application is only authorized to post Provisional notifications (direct to history)</summary>
        Provisional,

        /// <summary>The application is authorized to send notifications for 8 hours. Only used by App Clips.</summary>
        Ephemeral
    }
}