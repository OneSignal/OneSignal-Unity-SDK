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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OneSignalSDK.InAppMessages;
using OneSignalSDK.InAppMessages.Models;
using OneSignalSDK.InAppMessages.Internal;

namespace OneSignalSDK.iOS.InAppMessages {
    internal sealed class iOSInAppMessagesManager : IInAppMessagesManager {
        [DllImport("__Internal")] private static extern void _inAppMessagesSetWillDisplayCallback(StringListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _inAppMessagesSetDidDisplayCallback(StringListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _inAppMessagesSetWillDismissCallback(StringListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _inAppMessagesSetDidDismissCallback(StringListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _inAppMessagesSetClickCallback(ClickListenerDelegate callback);

        [DllImport("__Internal")] private static extern void _inAppMessagesSetPaused(bool paused);
        [DllImport("__Internal")] private static extern bool _inAppMessagesGetPaused();
        [DllImport("__Internal")] private static extern void _inAppMessagesAddTrigger(string key, string value);
        [DllImport("__Internal")] private static extern void _inAppMessagesAddTriggers(string triggersJson);
        [DllImport("__Internal")] private static extern void _inAppMessagesRemoveTrigger(string key);
        [DllImport("__Internal")] private static extern void _inAppMessagesRemoveTriggers(string triggersJson);
        [DllImport("__Internal")] private static extern void _inAppMessagesClearTriggers();

        private delegate void StringListenerDelegate(string response);
        private delegate void ClickListenerDelegate(string message, string result, int urlType);

        public event EventHandler<InAppMessageWillDisplayEventArgs> WillDisplay;
        public event EventHandler<InAppMessageDidDisplayEventArgs> DidDisplay;
        public event EventHandler<InAppMessageWillDismissEventArgs> WillDismiss;
        public event EventHandler<InAppMessageDidDismissEventArgs> DidDismiss;
        public event EventHandler<InAppMessageClickEventArgs> Clicked;

        private static iOSInAppMessagesManager _instance;

        public iOSInAppMessagesManager() {
            _instance = this;
        }

        public bool Paused {
            get => _inAppMessagesGetPaused();
            set => _inAppMessagesSetPaused(value);
        }

        public void AddTrigger(string key, string value)
            => _inAppMessagesAddTrigger(key, value.ToString());

        public void AddTriggers(Dictionary<string, string> triggers)
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
            _inAppMessagesSetClickCallback(_onClicked);
        }

        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onWillDisplay(string response) {
            var message = new InAppMessage(response);
            InAppMessageWillDisplayEventArgs args = new InAppMessageWillDisplayEventArgs(message);

            EventHandler<InAppMessageWillDisplayEventArgs> handler = _instance.WillDisplay;
            if (handler != null)
            {
                UnityMainThreadDispatch.Post(state => handler(_instance, args));
            }
        }

        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onDidDisplay(string response) {
            var message = new InAppMessage(response);
            InAppMessageDidDisplayEventArgs args = new InAppMessageDidDisplayEventArgs(message);

            EventHandler<InAppMessageDidDisplayEventArgs> handler = _instance.DidDisplay;
            if (handler != null)
            {
                UnityMainThreadDispatch.Post(state => handler(_instance, args));
            }
        }

        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onWillDismiss(string response) {
            var message = new InAppMessage(response);
            InAppMessageWillDismissEventArgs args = new InAppMessageWillDismissEventArgs(message);

            EventHandler<InAppMessageWillDismissEventArgs> handler = _instance.WillDismiss;
            if (handler != null)
            {
                UnityMainThreadDispatch.Post(state => handler(_instance, args));
            }
        }

        [AOT.MonoPInvokeCallback(typeof(StringListenerDelegate))]
        private static void _onDidDismiss(string response) {
            var message = new InAppMessage(response);
            InAppMessageDidDismissEventArgs args = new InAppMessageDidDismissEventArgs(message);

            EventHandler<InAppMessageDidDismissEventArgs> handler = _instance.DidDismiss;
            if (handler != null)
            {
                UnityMainThreadDispatch.Post(state => handler(_instance, args));
            }
        }

        [AOT.MonoPInvokeCallback(typeof(ClickListenerDelegate))]
        private static void _onClicked(string message, string result, int urlType) {
            var mes = JsonUtility.FromJson<InAppMessage>(message);
            var res = JsonUtility.FromJson<InAppMessageClickResult>(result);

            var urlTarget = IntToInAppMessageActionUrlType(urlType);
            var clickRes = new InAppMessageClickResult(res.ActionId, urlTarget, res.Url, res.ClosingMessage);

            InAppMessageClickEventArgs args = new InAppMessageClickEventArgs(mes, clickRes);

            EventHandler<InAppMessageClickEventArgs> handler = _instance.Clicked;
            if (handler != null)
            {
                UnityMainThreadDispatch.Post(state => handler(_instance, args));
            }
        }

        public static InAppMessageActionUrlType IntToInAppMessageActionUrlType(int urlType) {
            switch (urlType)
            {
                case 0:
                    return InAppMessageActionUrlType.Browser;
                case 1:
                    return InAppMessageActionUrlType.InAppWebview;
                case 2:
                    return InAppMessageActionUrlType.RepalceContent;
                default:
                    return null;
            }
        }
    }
}