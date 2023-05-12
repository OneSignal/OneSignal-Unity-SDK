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
using OneSignalSDK.InAppMessages.Models;

namespace OneSignalSDK.InAppMessages.Internal {
    [Serializable] public sealed class InAppMessageClickResult : IInAppMessageClickResult {
        /// <summary>
        /// Action the user took on the in-app message
        /// </summary>
        public string ActionId => actionId;

        /// <summary>
        /// Where the URL will be opened
        /// </summary>
        // public InAppMessageActionUrlType UrlTarget => urlTarget; // TODO

        /// <summary>
        /// URL opened from the action
        /// </summary>
        public string Url => url;

        /// <summary>
        /// If tapping on the element closes the in-app message
        /// </summary>
        public bool ClosingMessage => closingMessage;

        public InAppMessageClickResult() {}

        public InAppMessageClickResult(string actionId, string url, bool closingMessage) {
            this.actionId = actionId;
            this.url = url;
            this.closingMessage = closingMessage;
        }

        #region Native Field Handling
            public string actionId;
            // public InAppMessageActionUrlType urlTarget;
            public string url;
            public bool closingMessage;
        #endregion
    }
}