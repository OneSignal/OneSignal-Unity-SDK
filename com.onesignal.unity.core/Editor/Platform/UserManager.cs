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
using OneSignalSDK.User.Models;

namespace OneSignalSDK.User {
    internal sealed class UserManager : IUserManager {
        public string OneSignalId {
            get => null;
        }

        public string ExternalId {
            get => null;
        }

        public event EventHandler<UserStateChangedEventArgs> Changed;

        private PushSubscription _subscription = new PushSubscription();

        public IPushSubscription PushSubscription {
            get => _subscription;
        }

        public string Language {
            set {

            }
        }

        public Dictionary<string, string> GetTags() {
            return new Dictionary<string, string>();
        }

        public void AddTag(string key, string value){

        }

        public void AddTags(Dictionary<string, string> tags) {

        }

        public void RemoveTag(string key) {

        }

        public void RemoveTags(params string[] keys) {

        }

        public void AddAlias(string label, string id) {

        }

        public void AddAliases(Dictionary<string, string> aliases) {

        }

        public void RemoveAlias(string label) {

        }

        public void RemoveAliases(params string[] labels) {

        }

        public void AddEmail(string email) {

        }

        public void RemoveEmail(string email) {

        }

        public void AddSms(string sms) {

        }

        public void RemoveSms(string sms) {

        }
    }
}