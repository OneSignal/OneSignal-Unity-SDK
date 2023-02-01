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
using System.Runtime.InteropServices;
using OneSignalSDKNew.InAppMessages;
using OneSignalSDKNew.InAppMessages.Models;

namespace OneSignalSDKNew.iOS.InAppMessages {
    internal sealed class iOSInAppMessagesManager : IInAppMessagesManager {
        [DllImport("__Internal")] private static extern void _inAppMessagesSetWillDisplayCallback(StringListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _inAppMessagesSetDidDisplayCallback(StringListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _inAppMessagesSetWillDismissCallback(StringListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _inAppMessagesSetDidDismissCallback(StringListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _inAppMessagesSetClickedCallback(StringListenerDelegate callback);

        [DllImport("__Internal")] private static extern void _inAppMessagesSetPaused(bool paused);
        [DllImport("__Internal")] private static extern bool _inAppMessagesGetPaused();
        [DllImport("__Internal")] private static extern void _inAppMessagesAddTrigger(string key, string value);
        [DllImport("__Internal")] private static extern void _inAppMessagesAddTriggers(string triggersJson);
        [DllImport("__Internal")] private static extern void _inAppMessagesRemoveTrigger(string key);
        [DllImport("__Internal")] private static extern void _inAppMessagesRemoveTriggers(string triggersJson);
        [DllImport("__Internal")] private static extern void _inAppMessagesClearTriggers();

        private delegate void StringListenerDelegate(string response);

        public event InAppMessageLifecycleDelegate WillDisplay;
        public event InAppMessageLifecycleDelegate DidDisplay;
        public event InAppMessageLifecycleDelegate WillDismiss;
        public event InAppMessageLifecycleDelegate DidDismiss;

        public event InAppMessageClickedDelegate Clicked;

        private static iOSInAppMessagesManager _instance;

        public iOSInAppMessagesManager() {
            _instance = this;
        }

        public bool Paused {
            get => _inAppMessagesGetPaused();
            set => _inAppMessagesSetPaused(value);
        }

        public void AddTrigger(string key, object value)
            => _inAppMessagesAddTrigger(key, value.ToString()); // test

        public void AddTriggers(Dictionary<string, object> triggers) // <string, string> to <string, object>
            => _inAppMessagesAddTriggers(Json.Serialize(triggers));

        public void RemoveTrigger(string key)
            => _inAppMessagesRemoveTrigger(key);

        public void RemoveTriggers(params string[] keys)
            => _inAppMessagesRemoveTriggers(Json.Serialize(keys));

        public void ClearTriggers()
            => _inAppMessagesClearTriggers();

        public void Initialize() {
            _inAppMessagesSetWillDisplayCallback(_onWillDisplay);
            _inAppMessagesSetDidDisplayCallback(_onDidDisplay);
            _inAppMessagesSetWillDismissCallback(_onWillDismiss);
            _inAppMessagesSetDidDismissCallback(_onDidDismiss);
            _inAppMessagesSetClickedCallback(_onClicked);
        }

        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onWillDisplay(string response)
            =>  UnityMainThreadDispatch.Post(state => _instance.WillDisplay?.Invoke(new InAppMessage { messageId = response }));

        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onDidDisplay(string response)
            =>  UnityMainThreadDispatch.Post(state => _instance.DidDisplay?.Invoke(new InAppMessage { messageId = response }));

        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onWillDismiss(string response)
            =>  UnityMainThreadDispatch.Post(state => _instance.WillDismiss?.Invoke(new InAppMessage { messageId = response }));

        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onDidDismiss(string response)
            =>  UnityMainThreadDispatch.Post(state => _instance.DidDismiss?.Invoke(new InAppMessage { messageId = response }));

        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))] // iOS returns InAppMessageAction. Android returns InAppMessageClickedResult
        private static void _onClicked(string response) {
            InAppMessageClickedResult temp = new InAppMessageClickedResult();
            temp.action = JsonUtility.FromJson<InAppMessageAction>(response);
            //temp.message = new InAppMessage { messageId = "messageIdTemp" };
            UnityMainThreadDispatch.Post(state => _instance.Clicked?.Invoke(temp));
        }
    }
}