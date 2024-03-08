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
using System.Collections.Generic;
using OneSignalSDK.InAppMessages.Models;

namespace OneSignalSDK.InAppMessages {
    /// <summary>
    /// When any client side will display event in an In-App Message's occurs there will be a corresponding event with
    /// this arguement.
    /// </summary>
    public class InAppMessageWillDisplayEventArgs : EventArgs
    {
        public IInAppMessage Message { get; }

        public InAppMessageWillDisplayEventArgs(IInAppMessage message) {
            Message = message;
        }
    }

    /// <summary>
    /// When any client side did display event in an In-App Message's occurs there will be a corresponding event with
    /// this arguement.
    /// </summary>
    public class InAppMessageDidDisplayEventArgs : EventArgs
    {
        public IInAppMessage Message { get; }

        public InAppMessageDidDisplayEventArgs(IInAppMessage message) {
            Message = message;
        }
    }

    /// <summary>
    /// When any client side will dismiss event in an In-App Message's occurs there will be a corresponding event with
    /// this arguement.
    /// </summary>
    public class InAppMessageWillDismissEventArgs : EventArgs
    {
        public IInAppMessage Message { get; }

        public InAppMessageWillDismissEventArgs(IInAppMessage message) {
            Message = message;
        }
    }

    /// <summary>
    /// When any client side did dismiss event in an In-App Message's occurs there will be a corresponding event with
    /// this arguement.
    /// </summary>
    public class InAppMessageDidDismissEventArgs : EventArgs
    {
        public IInAppMessage Message { get; }

        public InAppMessageDidDismissEventArgs(IInAppMessage message) {
            Message = message;
        }
    }

    /// <summary>
    /// When an In-App Message action is tapped on.
    /// </summary>
    public class InAppMessageClickEventArgs : EventArgs
    {
        public IInAppMessage Message { get; }
        public IInAppMessageClickResult Result { get; }

        public InAppMessageClickEventArgs(IInAppMessage message, IInAppMessageClickResult result) {
            Message = message;
            Result = result;
        }
    }

    /// <summary>
    /// The In-App Message (IAM) Manager is a *device-scoped* manager for controlling the IAM
    /// functionality within your application.  By default IAMs are enabled and will present
    /// if the current user qualifies for any IAMs sent down by the OneSignal backend. To
    /// blanket disable IAMs, set [paused] to `true` on startup.
    /// </summary>
    public interface IInAppMessagesManager {
        /// <summary>
        /// When an In-App Message is ready to be displayed to the screen
        /// </summary>
        event EventHandler<InAppMessageWillDisplayEventArgs> WillDisplay;

        /// <summary>
        /// When an In-App Message is has been displayed to the screen
        /// </summary>
        event EventHandler<InAppMessageDidDisplayEventArgs> DidDisplay;

        /// <summary>
        /// When a user has chosen to dismiss an In-App Message
        /// </summary>
        event EventHandler<InAppMessageWillDismissEventArgs> WillDismiss;

        /// <summary>
        /// When an In-App Message has finished being dismissed
        /// </summary>
        event EventHandler<InAppMessageDidDismissEventArgs> DidDismiss;

        /// <summary>
        /// When a user has clicked on a clickable element in the In-App Message
        /// </summary>
        event EventHandler<InAppMessageClickEventArgs> Clicked;

        /// <summary>
        /// Allows you to temporarily pause all In-App Messages. You may want to do this while the user is engaged in
        /// an activity that you don't want a message to interrupt (such as watching a video).
        /// </summary>
        bool Paused { get; set; }

        /// <summary>
        /// Add a local trigger. May show an In-App Message if its triggers conditions were met.
        /// </summary>
        /// <param name="key">Key for the trigger</param>
        /// <param name="value">Value for the trigger</param>
        void AddTrigger(string key, string value);

        /// <summary>
        /// Allows you to set multiple local trigger key/value pairs simultaneously. May show an In-App Message if its
        /// triggers conditions were met.
        /// </summary>
        /// <param name="triggers">The dictionary of triggers that are to be added to the current user.</param>
        void AddTriggers(Dictionary<string, string> triggers);

        /// <summary>
        /// Removes a single local trigger for the given key.
        /// </summary>
        /// <param name="key">Key for the trigger.</param>
        void RemoveTrigger(string key);

        /// <summary>
        /// Removes a list of local triggers based on a collection of keys.
        /// </summary>
        /// <param name="keys">Removes a collection of triggers from their keys.</param>
        void RemoveTriggers(params string[] keys);

        /// <summary>
        /// Clear all triggers from the current user.
        /// </summary>
        void ClearTriggers();
    }
}