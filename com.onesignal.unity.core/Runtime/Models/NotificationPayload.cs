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

using System.Collections.Generic;

namespace OneSignalSDK {
    /// <summary>
    /// todo - struct?
    /// </summary>
    public class NotificationPayload {
        /// <summary>todo</summary>
        public string notificationID;

        /// <summary>todo</summary>
        public string sound;

        /// <summary>todo</summary>
        public string title;

        /// <summary>todo</summary>
        public string body;

        /// <summary>todo</summary>
        public string subtitle;

        /// <summary>todo</summary>
        public string launchURL;

        /// <summary>todo</summary>
        public Dictionary<string, object> additionalData;

        /// <summary>todo</summary>
        public Dictionary<string, object> actionButtons;

        /// <summary>todo</summary>
        public bool contentAvailable;

        /// <summary>todo</summary>
        public int badge;

        /// <summary>todo</summary>
        public string smallIcon;

        /// <summary>todo</summary>
        public string largeIcon;

        /// <summary>todo</summary>
        public string bigPicture;

        /// <summary>todo</summary>
        public string smallIconAccentColor;

        /// <summary>todo</summary>
        public string ledColor;

        /// <summary>todo</summary>
        public int lockScreenVisibility = 1;

        /// <summary>todo</summary>
        public string groupKey;

        /// <summary>todo</summary>
        public string groupMessage;

        /// <summary>todo</summary>
        public string fromProjectNumber;
    }
}

