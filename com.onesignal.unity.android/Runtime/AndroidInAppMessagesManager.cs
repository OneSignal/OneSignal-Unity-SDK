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
using OneSignalSDK.InAppMessages;
using OneSignalSDK.InAppMessages.Models;
using OneSignalSDK.InAppMessages.Internal;
using OneSignalSDK.Android.Utilities;

namespace OneSignalSDK.Android.InAppMessages {
    internal sealed class AndroidInAppMessagesManager : IInAppMessagesManager {
        private readonly AndroidJavaObject _inAppMessages;

        public AndroidInAppMessagesManager(AndroidJavaClass sdkClass) {
            _inAppMessages = sdkClass.CallStatic<AndroidJavaObject>("getInAppMessages");
        }

        public event EventHandler<InAppMessageWillDisplayEventArgs> WillDisplay;
        public event EventHandler<InAppMessageDidDisplayEventArgs> DidDisplay;
        public event EventHandler<InAppMessageWillDismissEventArgs> WillDismiss;
        public event EventHandler<InAppMessageDidDismissEventArgs> DidDismiss;
        public event EventHandler<InAppMessageClickEventArgs> Clicked;

        public bool Paused {
            get => _inAppMessages.Call<bool>("getPaused");
            set => _inAppMessages.Call("setPaused", value);
        }

        public void AddTrigger(string key, string value)
            => _inAppMessages.Call("addTrigger", key, value);

        public void AddTriggers(Dictionary<string, string> triggers)
            => _inAppMessages.Call("addTriggers", triggers.ToMap());

        public void RemoveTrigger(string key)
            => _inAppMessages.Call("removeTrigger", key);

        public void RemoveTriggers(params string[] keys)
            => _inAppMessages.Call("removeTriggers", keys.ToArrayList());

        public void ClearTriggers()
            => _inAppMessages.Call("clearTriggers");

        public void Initialize() {
            _inAppMessages.Call("addClickListener", new IInAppMessageClickListener(this));
            _inAppMessages.Call("addLifecycleListener", new IInAppMessageLifecycleHandler(this));
        }

        private sealed class IInAppMessageLifecycleHandler : OneSignalAndroidJavaProxy {
            private AndroidInAppMessagesManager _parent;

            public IInAppMessageLifecycleHandler(AndroidInAppMessagesManager inAppMessagesManager) : base("inAppMessages.IInAppMessageLifecycleListener") {
                _parent = inAppMessagesManager;
            }

            /// <param name="willDisplayEvent">IInAppMessageWillDisplayEvent</param>
            public void onWillDisplay(AndroidJavaObject willDisplayEvent) {
                var messageJO = willDisplayEvent.Call<AndroidJavaObject>("getMessage");
                var message = messageJO.ToSerializable<InAppMessage>();

                InAppMessageWillDisplayEventArgs args = new InAppMessageWillDisplayEventArgs(message);

                EventHandler<InAppMessageWillDisplayEventArgs> handler = _parent.WillDisplay;
                if (handler != null)
                {
                    UnityMainThreadDispatch.Post(state => handler(_parent, args));
                }
            }

            /// <param name="didDisplayEvent">IInAppMessageDidDisplayEvent</param>
            public void onDidDisplay(AndroidJavaObject didDisplayEvent) {
                var messageJO = didDisplayEvent.Call<AndroidJavaObject>("getMessage");
                var message = messageJO.ToSerializable<InAppMessage>();

                InAppMessageDidDisplayEventArgs args = new InAppMessageDidDisplayEventArgs(message);

                EventHandler<InAppMessageDidDisplayEventArgs> handler = _parent.DidDisplay;
                if (handler != null)
                {
                    UnityMainThreadDispatch.Post(state => handler(_parent, args));
                }
            }

            /// <param name="willDismissEvent">IInAppMessageWillDismissEvent</param>
            public void onWillDismiss(AndroidJavaObject willDismissEvent) {
                var messageJO = willDismissEvent.Call<AndroidJavaObject>("getMessage");
                var message = messageJO.ToSerializable<InAppMessage>();

                InAppMessageWillDismissEventArgs args = new InAppMessageWillDismissEventArgs(message);

                EventHandler<InAppMessageWillDismissEventArgs> handler = _parent.WillDismiss;
                if (handler != null)
                {
                    UnityMainThreadDispatch.Post(state => handler(_parent, args));
                }
            }

            /// <param name="didDismissEvent">IInAppMessageDidDismissEvent</param>
            public void onDidDismiss(AndroidJavaObject didDismissEvent) {
                var messageJO = didDismissEvent.Call<AndroidJavaObject>("getMessage");
                var message = messageJO.ToSerializable<InAppMessage>();

                InAppMessageDidDismissEventArgs args = new InAppMessageDidDismissEventArgs(message);

                EventHandler<InAppMessageDidDismissEventArgs> handler = _parent.DidDismiss;
                if (handler != null)
                {
                    UnityMainThreadDispatch.Post(state => handler(_parent, args));
                }
            }
        }

        private sealed class IInAppMessageClickListener : OneSignalAndroidJavaProxy {
            private AndroidInAppMessagesManager _parent;

            public IInAppMessageClickListener(AndroidInAppMessagesManager inAppMessagesManager) : base("inAppMessages.IInAppMessageClickListener") {
                _parent = inAppMessagesManager;
            }

            /// <param name="clickEvent">IInAppMessageClickEvent</param>
            public void onClick(AndroidJavaObject clickEvent) {
                var messageJO = clickEvent.Call<AndroidJavaObject>("getMessage");
                var message = messageJO.ToSerializable<InAppMessage>();

                var resultJO = clickEvent.Call<AndroidJavaObject>("getResult");
                
                // Not having a 1:1 serializable class with matching primative types will result in ToSerialization being empty
                var actionId = resultJO.Call<string>("getActionId");
                
                var urlTypeJO = resultJO.Call<AndroidJavaObject>("getUrlTarget");
                var urlType = urlTypeJO.Call<string>("toString");
                var urlTarget = StringToInAppMessageActionUrlType(urlType);
                
                var url = resultJO.Call<string>("getUrl");
                var closingMessage = resultJO.Call<bool>("getClosingMessage");

                var result = new InAppMessageClickResult(actionId, urlTarget, url, closingMessage);

                InAppMessageClickEventArgs args = new InAppMessageClickEventArgs(message, result);

                EventHandler<InAppMessageClickEventArgs> handler = _parent.Clicked;
                if (handler != null)
                {
                    UnityMainThreadDispatch.Post(state => handler(_parent, args));
                }
            }

            public static InAppMessageActionUrlType StringToInAppMessageActionUrlType(string urlType) {
                switch (urlType)
                {
                    case "webview":
                        return InAppMessageActionUrlType.InAppWebview;
                    case "browser":
                        return InAppMessageActionUrlType.Browser;
                    case "replacement":
                        return InAppMessageActionUrlType.RepalceContent;
                    default:
                        return null;
                }
            }
        }
    }
}