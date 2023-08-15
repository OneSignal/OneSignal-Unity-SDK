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


using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OneSignalSDK.User;
using OneSignalSDK.User.Models;
using OneSignalSDK.iOS.User.Models;

namespace OneSignalSDK.iOS.User {
    internal sealed class iOSUserManager : IUserManager {
        [DllImport("__Internal")] private static extern void _userSetLanguage(string languageCode);
        [DllImport("__Internal")] private static extern void _userAddAlias(string aliasLabel, string aliasId);
        [DllImport("__Internal")] private static extern void _userAddAliases(string aliasesJson);
        [DllImport("__Internal")] private static extern void _userRemoveAlias(string aliasLabel);
        [DllImport("__Internal")] private static extern void _userRemoveAliases(string aliasesJson);
        [DllImport("__Internal")] private static extern void _userAddEmail(string email);
        [DllImport("__Internal")] private static extern void _userRemoveEmail(string email);
        [DllImport("__Internal")] private static extern void _userAddSms(string smsNumber);
        [DllImport("__Internal")] private static extern void _userRemoveSms(string smsNumber);
        [DllImport("__Internal")] private static extern void _userAddTag(string key, string value);
        [DllImport("__Internal")] private static extern void _userAddTags(string tagsJson);
        [DllImport("__Internal")] private static extern void _userRemoveTag(string key);
        [DllImport("__Internal")] private static extern void _userRemoveTags(string tagsJson);

        private iOSPushSubscription _pushSubscription;
        
        public iOSUserManager() {
            _pushSubscription = new iOSPushSubscription();
        }

        public IPushSubscription PushSubscription {
            get => _pushSubscription;
        }

        public string Language {
            set => _userSetLanguage(value);
        }

        public void AddTag(string key, string value)
            =>_userAddTag(key, value);

        public void AddTags(Dictionary<string, string> tags)
            => _userAddTags(Json.Serialize(tags));

        public void RemoveTag(string key)
            => _userRemoveTag(key);

        public void RemoveTags(params string[] keys)
            => _userRemoveTags(Json.Serialize(keys));

        public void AddAlias(string label, string id)
            => _userAddAlias(label, id);

        public void AddAliases(Dictionary<string, string> aliases)
            => _userAddAliases(Json.Serialize(aliases));

        public void RemoveAlias(string label)
            => _userRemoveAlias(label);

        public void RemoveAliases(params string[] labels)
            => _userRemoveAliases(Json.Serialize(labels));

        public void AddEmail(string email)
            => _userAddEmail(email);

        public void RemoveEmail(string email)
            => _userRemoveEmail(email);

        public void AddSms(string sms)
            => _userAddSms(sms);

        public void RemoveSms(string sms)
            => _userRemoveSms(sms);

        public void Initialize() {
            _pushSubscription.Initialize();
        }
    }
}