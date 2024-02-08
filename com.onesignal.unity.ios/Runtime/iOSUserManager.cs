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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OneSignalSDK.User;
using OneSignalSDK.User.Internal;
using OneSignalSDK.User.Models;
using OneSignalSDK.iOS.User.Models;

namespace OneSignalSDK.iOS.User {
    internal sealed class iOSUserManager : IUserManager {
        [DllImport("__Internal")] private static extern string _oneSignalUserGetOneSignalId();
        [DllImport("__Internal")] private static extern string _oneSignalUserGetExternalId();
        [DllImport("__Internal")] private static extern void _oneSignalUserSetLanguage(string languageCode);
        [DllImport("__Internal")] private static extern void _oneSignalUserAddAlias(string aliasLabel, string aliasId);
        [DllImport("__Internal")] private static extern void _oneSignalUserAddAliases(string aliasesJson);
        [DllImport("__Internal")] private static extern void _oneSignalUserRemoveAlias(string aliasLabel);
        [DllImport("__Internal")] private static extern void _oneSignalUserRemoveAliases(string aliasesJson);
        [DllImport("__Internal")] private static extern void _oneSignalUserAddEmail(string email);
        [DllImport("__Internal")] private static extern void _oneSignalUserRemoveEmail(string email);
        [DllImport("__Internal")] private static extern void _oneSignalUserAddSms(string smsNumber);
        [DllImport("__Internal")] private static extern void _oneSignalUserRemoveSms(string smsNumber);
        [DllImport("__Internal")] private static extern string _oneSignalUserGetTags();
        [DllImport("__Internal")] private static extern void _oneSignalUserAddTag(string key, string value);
        [DllImport("__Internal")] private static extern void _oneSignalUserAddTags(string tagsJson);
        [DllImport("__Internal")] private static extern void _oneSignalUserRemoveTag(string key);
        [DllImport("__Internal")] private static extern void _oneSignalUserRemoveTags(string tagsJson);
        [DllImport("__Internal")] private static extern void _oneSignalUserAddStateChangedCallback(UserStateListenerDelegate callback);

        public delegate void UserStateListenerDelegate(string current);

        public event EventHandler<UserStateChangedEventArgs> Changed;

        private iOSPushSubscription _pushSubscription;

        private static iOSUserManager _instance;
        
        public iOSUserManager() {
            _instance = this;
            _pushSubscription = new iOSPushSubscription();
        }

        public string OneSignalId {
            get => _oneSignalUserGetOneSignalId();
        }

        public string ExternalId {
            get => _oneSignalUserGetExternalId();
        }

        public IPushSubscription PushSubscription {
            get => _pushSubscription;
        }

        public string Language {
            set => _oneSignalUserSetLanguage(value);
        }

        public Dictionary<string, string> GetTags() {
            Dictionary<string, object> raw = (Dictionary<string, object>)Json.Deserialize(_oneSignalUserGetTags());
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (KeyValuePair<string, object> kvp in raw) dict.Add(kvp.Key, kvp.Value.ToString());
            return dict;
        }

        public void AddTag(string key, string value)
            =>_oneSignalUserAddTag(key, value);

        public void AddTags(Dictionary<string, string> tags)
            => _oneSignalUserAddTags(Json.Serialize(tags));

        public void RemoveTag(string key)
            => _oneSignalUserRemoveTag(key);

        public void RemoveTags(params string[] keys)
            => _oneSignalUserRemoveTags(Json.Serialize(keys));

        public void AddAlias(string label, string id)
            => _oneSignalUserAddAlias(label, id);

        public void AddAliases(Dictionary<string, string> aliases)
            => _oneSignalUserAddAliases(Json.Serialize(aliases));

        public void RemoveAlias(string label)
            => _oneSignalUserRemoveAlias(label);

        public void RemoveAliases(params string[] labels)
            => _oneSignalUserRemoveAliases(Json.Serialize(labels));

        public void AddEmail(string email)
            => _oneSignalUserAddEmail(email);

        public void RemoveEmail(string email)
            => _oneSignalUserRemoveEmail(email);

        public void AddSms(string sms)
            => _oneSignalUserAddSms(sms);

        public void RemoveSms(string sms)
            => _oneSignalUserRemoveSms(sms);

        public void Initialize() {
            _pushSubscription.Initialize();
            _oneSignalUserAddStateChangedCallback(_onUserStateChanged);
        }

        [AOT.MonoPInvokeCallback(typeof(UserStateListenerDelegate))]
        private static void _onUserStateChanged(string current) {
            var curr = JsonUtility.FromJson<UserState>(current);

            UserChangedState userChangedState = new UserChangedState(curr);
            UserStateChangedEventArgs args = new UserStateChangedEventArgs(userChangedState);

            EventHandler<UserStateChangedEventArgs> handler = _instance.Changed;
            if (handler != null)
            {
                UnityMainThreadDispatch.Post(state => handler(_instance, args));
            }
        }
    }
}