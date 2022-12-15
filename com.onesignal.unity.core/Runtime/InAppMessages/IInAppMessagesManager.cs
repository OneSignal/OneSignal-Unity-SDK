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

using System.Collections.Generic;
using OneSignalSDKNew.InAppMessages.Models;

namespace OneSignalSDKNew.InAppMessages {
    /// <summary>
    /// When any client side event in an In-App Message's occurs there will be a corresponding event with this
    /// delegate signature.
    /// </summary>
    /// <param name="message">In-app message to be displayed and dismissed</param>
    public delegate void InAppMessageLifecycleDelegate(InAppMessage message);
    
    /// <summary>
    /// Sets a In-App Message opened handler. The instance will be called when an In-App Message action is tapped on.
    /// </summary>
    /// <param name="result">The In-App Message clicked result describing:
    ///     1. The in-app message opened
    ///     2. The action taken by the user.
    /// </param>
    public delegate void InAppMessageClickedDelegate(InAppMessageClickedResult result);

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
        event InAppMessageLifecycleDelegate WillDisplay;

        /// <summary>
        /// When an In-App Message is has been displayed to the screen
        /// </summary>
        event InAppMessageLifecycleDelegate DidDisplay;

        /// <summary>
        /// When a user has chosen to dismiss an In-App Message
        /// </summary>
        event InAppMessageLifecycleDelegate WillDismiss;

        /// <summary>
        /// When an In-App Message has finished being dismissed
        /// </summary>
        event InAppMessageLifecycleDelegate DidDismiss;

        /// <summary>
        /// When a user has clicked on a clickable element in the In-App Message
        /// </summary>
        event InAppMessageClickedDelegate Clicked;

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
        /// <remarks>Android Only</remarks> // TODO: Android Only
        void ClearTriggers();
    }
}