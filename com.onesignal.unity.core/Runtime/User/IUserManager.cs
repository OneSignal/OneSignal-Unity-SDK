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
using OneSignalSDKNew.User.Models;

namespace OneSignalSDKNew.User {
    /// <summary>
    /// The OneSignal user manager is responsible for managing the current user state.  When
    /// an app starts up for the first time, it is defaulted to having a guest user that is only
    /// accessible by the current device.  Once the application knows the identity of the user using their
    /// app, they should call [OneSignal.login] providing that identity to OneSignal, at which
    /// point all state in here will reflect the state of that known user.
    /// 
    /// The current user is persisted across the application lifecycle, even when the application
    /// is restarted.  It is up to the application developer to call [OneSignal.login] when
    /// the user of the application switches, or logs out, to ensure the identity tracked by OneSignal
    /// remains in sync.
    ///
    /// When you should call [OneSignal.login]:
    ///   1. When the identity of the user changes (i.e. a login or a context switch)
    ///   2. When the identity of the user is lost (i.e. a logout)
    /// </summary>
    public interface IUserManager {
        /// <summary>
        /// The 2-character language either as a detected language or explicitly set for this user. See
        /// https://documentation.onesignal.com/docs/language-localization#what-languages-are-supported
        /// for supported languages.
        /// </summary>
        string Language { set; }

        /// <summary>
        /// Current user's subscription to push notifications
        /// </summary>
        IPushSubscription PushSubscription { get; }

        /// <summary>
        /// Tag player with a key value pair to later create segments on them at onesignal.com
        /// </summary>
        /// <param name="key">The key of the data tag.</param>
        /// <param name="value">The new value of the data tag.</param>
        void AddTag(string key, string value); // TODO: Switched value from object to string

        /// <summary>
        /// Tag player with a key value pairs to later create segments on them at onesignal.com
        /// </summary>
        /// <param name="tags">A dictionary of tags, all of which will be added/updated for the current user.</param>
        void AddTags(Dictionary<string, string> tags);

        /// <summary>
        /// Delete a Tag from current device record
        /// </summary>
        /// <param name="key">The key of the data tag.</param>
        void RemoveTag(string key);

        /// <summary>
        /// Delete multiple Tags from current device record
        /// </summary>
        /// <param name="keys">The array of keys, all of which will be removed from the current user.</param>
        void RemoveTags(params string[] keys);

        /// <summary>
        /// Set an alias for the current user.  If this alias already exists it will be overwritten.
        /// </summary>
        /// <param name="label">The alias label that you want to set against the current user.</param>
        /// <param name="id">
        /// The alias id that should be set against the current user. This must be a unique value
        /// within the alias label across your entire user base so it can uniquely identify this user.
        /// </param>
        void AddAlias(string label, string id);

        /// <summary>
        /// Add/set aliases for the current user. If any alias already exists it will be overwritten.
        /// </summary>
        /// <param name="aliases">
        /// A dictionary of the alias label -> alias id that should be set against the user. Each alias id 
        /// must be a unique value within the alias label across your entire user base so it can
        /// uniquely identify this user.
        /// </param>
        void AddAliases(Dictionary<string, string> aliases);

        /// <summary>
        /// Remove an alias from the current user.
        /// </summary>
        /// <param name="label">The alias label that should no longer be set for the current user.</param>
        void RemoveAlias(string label);

        /// <summary>
        /// Remove multiple aliases from the current user.
        /// </summary>
        /// <param name="labels">The collection of alias labels, all of which will be removed from the current user.</param>
        void RemoveAliases(params string[] keys);

        /// <summary>
        /// Add a new email subscription to the current user.
        /// </summary>
        /// <param name="email">The email that you want to subscribe and associate with the user</param>
        void AddEmail(string email);

        /// <summary>
        /// Remove an email subscription from the current user.
        /// </summary>
        /// <param name="email">The email address that the current user was subscribed for, and should no longer be.</param>
        void RemoveEmail(string email);

        /// <summary>
        /// Add a new SMS subscription to the current user.
        /// </summary>
        /// <param name="sms">The sms number that you want subscribe and associate with the user</param>
        void AddSms(string sms);

        /// <summary>
        /// Remove an SMS subscription from the current user.
        /// </summary>
        /// <param name="sms">The sms address that the current user was subscribed for, and should no longer be.</param>
        void RemoveSms(string sms);
    }
}