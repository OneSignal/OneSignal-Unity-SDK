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


using UnityEngine;
using System.Collections.Generic;
using OneSignalSDKNew.User.Models;

namespace OneSignalSDKNew.User {
    internal sealed class AndroidUserManager : IUserManager {
        private readonly AndroidJavaObject _user;
        private AndroidPushSubscription _pushSubscription;
        
        public AndroidUserManager(AndroidJavaClass sdkClass) {
            _user = sdkClass.CallStatic<AndroidJavaObject>("getUser");
            _pushSubscription = new AndroidPushSubscription(_user);
        }

        public IPushSubscription PushSubscription {
            get => _pushSubscription;
        }

        public string Language {
            get => _user.Call<string>("getLanguage");
            set => _user.Call("setLanguage", value);
        }

        public void AddTag(string key, string value)
            => _user.Call("addTag", key, value);

        public void AddTags(Dictionary<string, string> tags)
            => _user.Call("addTags", tags.ToMap());

        public void RemoveTag(string key)
            => _user.Call("removeTag", key);

        public void RemoveTags(params string[] keys)
            => _user.Call("removeTags", keys.ToArrayList());

        public void AddAlias(string label, string id)
            => _user.Call("addAlias", label, id);

        public void AddAliases(Dictionary<string, string> aliases)
            => _user.Call("addAliases", aliases.ToMap());

        public void RemoveAlias(string label)
            => _user.Call("removeAlias", label);

        public void AddEmailSubscription(string email)
            => _user.Call("addEmailSubscription", email);

        public void RemoveEmailSubscription(string email)
            => _user.Call("removeEmailSubscription", email);

        public void AddSmsSubscription(string sms)
            => _user.Call("addSmsSubscription", sms);

        public void RemoveSmsSubscription(string sms)
            => _user.Call("removeSmsSubscription", sms);

        public void Initialize() {
            _pushSubscription.Initialize();
        }
    }
}