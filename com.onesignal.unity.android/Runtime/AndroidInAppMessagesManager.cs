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
using OneSignalSDK.InAppMessages;
using OneSignalSDK.InAppMessages.Models;
using OneSignalSDK.Android.Utilities;

namespace OneSignalSDK.Android.InAppMessages {
    internal sealed class AndroidInAppMessagesManager : IInAppMessagesManager {
        private readonly AndroidJavaObject _inAppMessages;

        public AndroidInAppMessagesManager(AndroidJavaClass sdkClass) {
            _inAppMessages = sdkClass.CallStatic<AndroidJavaObject>("getInAppMessages");
        }
        public event InAppMessageLifecycleDelegate WillDisplay;
        public event InAppMessageLifecycleDelegate DidDisplay;
        public event InAppMessageLifecycleDelegate WillDismiss;
        public event InAppMessageLifecycleDelegate DidDismiss;

        public event InAppMessageClickedDelegate Clicked;

        public bool Paused {
            get => _inAppMessages.Call<bool>("getPaused");
            set => _inAppMessages.Call("setPaused", value);
        }

        public void AddTrigger(string key, object value)
            => _inAppMessages.Call("addTrigger", key, value);

        public void AddTriggers(Dictionary<string, object> triggers)
            => _inAppMessages.Call("addTriggers", triggers.ToMap());

        public void RemoveTrigger(string key)
            => _inAppMessages.Call("removeTrigger", key);

        public void RemoveTriggers(params string[] keys)
            => _inAppMessages.Call("removeTriggers", keys.ToArrayList());

        public void ClearTriggers()
            => _inAppMessages.Call("clearTriggers");

        public void Initialize() {
            _inAppMessages.Call("setInAppMessageClickHandler", new IInAppMessageClickHandler(this));
            _inAppMessages.Call("setInAppMessageLifecycleHandler", new IInAppMessageLifecycleHandler(this));
        }

        private sealed class IInAppMessageLifecycleHandler : OneSignalAndroidJavaProxy {
            private AndroidInAppMessagesManager _parent;

            public IInAppMessageLifecycleHandler(AndroidInAppMessagesManager inAppMessagesManager) : base("inAppMessages.IInAppMessageLifecycleHandler") {
                _parent = inAppMessagesManager;
            }

            /// <param name="message">IInAppMessage</param>
            public void onWillDisplayInAppMessage(AndroidJavaObject message)
                => UnityMainThreadDispatch.Post(state => _parent.WillDisplay?.Invoke(message.ToSerializable<InAppMessage>()));

            /// <param name="message">IInAppMessage</param>
            public void onDidDisplayInAppMessage(AndroidJavaObject message)
                => UnityMainThreadDispatch.Post(state => _parent.DidDisplay?.Invoke(message.ToSerializable<InAppMessage>()));

            /// <param name="message">IInAppMessage</param>
            public void onWillDismissInAppMessage(AndroidJavaObject message)
                => UnityMainThreadDispatch.Post(state => _parent.WillDismiss?.Invoke(message.ToSerializable<InAppMessage>()));

            /// <param name="message">IInAppMessage</param>
            public void onDidDismissInAppMessage(AndroidJavaObject message)
                => UnityMainThreadDispatch.Post(state => _parent.DidDismiss?.Invoke(message.ToSerializable<InAppMessage>()));
        }

        private sealed class IInAppMessageClickHandler : OneSignalAndroidJavaProxy {
            private AndroidInAppMessagesManager _parent;

            public IInAppMessageClickHandler(AndroidInAppMessagesManager inAppMessagesManager) : base("inAppMessages.IInAppMessageClickHandler") {
                _parent = inAppMessagesManager;
            }

            /// <param name="result">IInAppMessageClickResult</param>
            public void inAppMessageClicked(AndroidJavaObject result) {
                var actionResult = result.Call<AndroidJavaObject>("getAction").ToSerializable<InAppMessageAction>(); // temp
                //var messageResult = result.Call<AndroidJavaObject>("getMessage").ToSerializable<InAppMessage>();
                var clickResult = new InAppMessageClickedResult() {
                    action = actionResult,
                    //message = messageResult
                };
                UnityMainThreadDispatch.Post(state => _parent.Clicked?.Invoke(clickResult));
            }
        }
    }
}