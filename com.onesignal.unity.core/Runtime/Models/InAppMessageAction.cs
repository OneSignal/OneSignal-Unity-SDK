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
using UnityEngine;

namespace OneSignalSDK {
    /// <summary>
    /// Action associated with clicking a button in an In-App Message
    /// </summary>
    [Serializable] public sealed class InAppMessageAction : ISerializationCallbackReceiver {
        /// <summary>
        /// An optional click name defined for the action element
        /// </summary>
        public string clickName;

        /// <summary>
        /// An optional URL that opens when the action takes place
        /// </summary>
        public string clickUrl;

        /// <summary>
        /// Whether this is the first time the user has clicked any action on the In-App Message
        /// </summary>
        public bool firstClick;

        /// <summary>
        /// Whether this action will close the In-App Message
        /// </summary>
        public bool closesMessage;

    #region Native Field Handling
        public string click_name;
        public string click_url;
        public bool first_click;
        public bool closes_message;

        public void OnBeforeSerialize() {
            // no operation
        }

        public void OnAfterDeserialize() {
            SDKDebug.Info("DESERIALIZING IAM ACTION");
            clickName     = click_name;
            clickUrl      = click_url;
            firstClick    = first_click;
            closesMessage = closes_message;
        }
    #endregion

    }
}