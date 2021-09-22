/*
 * Modified MIT License
 *
 * Copyright 2021 OneSignal
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
using Laters;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedParameter.Local
namespace OneSignalSDK {
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class OneSignalAndroid : OneSignal {
        private abstract class OneSignalAndroidJavaProxy : AndroidJavaProxy {
            protected OneSignalAndroidJavaProxy(string listenerClassName)
                : base(QualifiedSDKClass + "$" + listenerClassName) { }
        }

        private abstract class OneSignalAwaitableAndroidJavaProxy<TResult> : AwaitableAndroidJavaProxy<TResult> {
            protected OneSignalAwaitableAndroidJavaProxy(string listenerClassName)
                : base(QualifiedSDKClass + "$" + listenerClassName) { }
        }

        /*
         * Global Callbacks
         */

        private static OneSignalAndroid _instance;

        /// <summary>
        /// Used to provide a reference for and sets up the global callbacks
        /// </summary>
        internal OneSignalAndroid() {
            if (_instance != null)
                SDKDebug.Error("Additional instance of OneSignalAndroid created.");
            
            // states
            _sdkClass.CallStatic("addPermissionObserver", new OSPermissionObserver());
            _sdkClass.CallStatic("addSubscriptionObserver", new OSSubscriptionObserver());
            _sdkClass.CallStatic("addEmailSubscriptionObserver", new OSEmailSubscriptionObserver());
            _sdkClass.CallStatic("addSMSSubscriptionObserver", new OSSMSSubscriptionObserver());
            
            // notifications
            _sdkClass.CallStatic("setRemoteNotificationReceivedHandler", new OSRemoteNotificationReceivedHandler());
            _sdkClass.CallStatic("setNotificationWillShowInForegroundHandler", new OSNotificationWillShowInForegroundHandler());
            _sdkClass.CallStatic("setNotificationOpenedHandler", new OSNotificationOpenedHandler());

            // iams
            _sdkClass.CallStatic("setInAppMessageLifecycleHandler", new OSInAppMessageLifecycleHandler());
            _sdkClass.CallStatic("setInAppMessageClickHandler", new OSInAppMessageClickHandler());

            _instance = this;
        }
        
        private sealed class OSPermissionObserver : OneSignalAndroidJavaProxy {
            public OSPermissionObserver() : base("OSPermissionObserver") { }

            /// <param name="stateChanges">OSPermissionStateChanges</param>
            public void onOSPermissionChanged(AndroidJavaObject stateChanges) {
                _instance.PermissionStateChanged?.Invoke(null, null);
            }
        }

        private sealed class OSSubscriptionObserver : OneSignalAndroidJavaProxy {
            public OSSubscriptionObserver() : base("OSSubscriptionObserver") { }

            /// <param name="stateChanges">OSSubscriptionStateChanges</param>
            public void onOSSubscriptionChanged(AndroidJavaObject stateChanges) { 
                _instance.SubscriptionStateChanged?.Invoke(null, null);
            }
        }

        private sealed class OSEmailSubscriptionObserver : OneSignalAndroidJavaProxy {
            public OSEmailSubscriptionObserver() : base("OSEmailSubscriptionObserver") { }

            /// <param name="stateChanges">OSEmailSubscriptionStateChanges</param>
            public void onOSEmailSubscriptionChanged(AndroidJavaObject stateChanges) { 
                _instance.EmailSubscriptionStateChanged?.Invoke(null, null);
            }
        }

        private sealed class OSSMSSubscriptionObserver : OneSignalAndroidJavaProxy {
            public OSSMSSubscriptionObserver() : base("OSSMSSubscriptionObserver") { }

            /// <param name="stateChanges">OSSMSSubscriptionStateChanges</param>
            public void onSMSSubscriptionChanged(AndroidJavaObject stateChanges) { 
                _instance.SMSSubscriptionStateChanged?.Invoke(null, null);
            }
        }

        private sealed class OSRemoteNotificationReceivedHandler : OneSignalAndroidJavaProxy {
            public OSRemoteNotificationReceivedHandler() : base("OSRemoteNotificationReceivedHandler") { }

            /// <param name="context">Context</param>
            /// <param name="notificationReceivedEvent">OSNotificationReceivedEvent</param>
            public void remoteNotificationReceived(AndroidJavaObject context, AndroidJavaObject notificationReceivedEvent) {
                _instance.NotificationReceived?.Invoke(null); // todo
            }
        }

        private sealed class OSNotificationWillShowInForegroundHandler : OneSignalAndroidJavaProxy {
            public OSNotificationWillShowInForegroundHandler() : base("OSNotificationWillShowInForegroundHandler") { }

            /// <param name="notificationReceivedEvent">OSNotificationReceivedEvent</param>
            public void notificationWillShowInForeground(AndroidJavaObject notificationReceivedEvent) {
                // todo
            }
        }

        private sealed class OSNotificationOpenedHandler : OneSignalAndroidJavaProxy {
            public OSNotificationOpenedHandler() : base("OSNotificationOpenedHandler") { }

            /// <param name="result">OSNotificationOpenedResult</param>
            public void notificationOpened(AndroidJavaObject result) {
                _instance.NotificationOpened?.Invoke(null); // todo
            }
        }
        
        private sealed class OSInAppMessageLifecycleHandler : OneSignalAndroidJavaProxy {
            public OSInAppMessageLifecycleHandler() : base("OSInAppMessageLifecycleHandler") { }
            
            /// <param name="message">OSInAppMessage</param>
            public void onWillDisplayInAppMessage(AndroidJavaObject message) {
                // todo
            }
            
            /// <param name="message">OSInAppMessage</param>
            public void onDidDisplayInAppMessage(AndroidJavaObject message) {
                // todo
            }
            
            /// <param name="message">OSInAppMessage</param>
            public void onWillDismissInAppMessage(AndroidJavaObject message) {
                // todo
            }
            
            /// <param name="message">OSInAppMessage</param>
            public void onDidDismissInAppMessage(AndroidJavaObject message) {
                // todo
            }
        }

        private sealed class OSInAppMessageClickHandler : OneSignalAndroidJavaProxy {
            public OSInAppMessageClickHandler() : base("OSInAppMessageClickHandler") { }

            /// <param name="result">OSInAppMessageAction</param>
            public void inAppMessageClicked(AndroidJavaObject result) {
                _instance.InAppMessageClicked?.Invoke(null); // todo
            }
        }

        /*
         * Direct Callbacks
         */

        private sealed class OSGetTagsHandler : OneSignalAwaitableAndroidJavaProxy<Dictionary<string, object>> {
            public OSGetTagsHandler() : base("OSGetTagsHandler") { }

            /// <param name="tags">JSONObject</param>
            public void tagsAvailable(AndroidJavaObject tags) // this is coming back from another thread
                => UnityMainThreadDispatch.Post(state => _complete(tags.JSONObjectToDictionary()));
        }

        private sealed class ChangeTagsUpdateHandler : OneSignalAwaitableAndroidJavaProxy<Dictionary<string, object>> {
            public ChangeTagsUpdateHandler() : base("ChangeTagsUpdateHandler") { }

            /// <param name="tags">JSONObject</param>
            public void onSuccess(AndroidJavaObject tags)
                => _complete(tags.JSONObjectToDictionary());

            /// <param name="error">SendTagsError</param>
            public void onFailure(AndroidJavaObject error) {
                SDKDebug.Error(error.Call<string>("toString"));
                // todo
            }
        }

        private sealed class EmailUpdateHandler : AwaitableVoidAndroidJavaProxy {
            public EmailUpdateHandler() : base("EmailUpdateHandler") { }

            public void onSuccess() { } // completes itself

            /// <param name="error">EmailUpdateError</param>
            public void onFailure(AndroidJavaObject error) {
                SDKDebug.Error(error.Call<string>("toString"));
                // todo
            }
        }

        private sealed class OSExternalUserIdUpdateCompletionHandler : OneSignalAwaitableAndroidJavaProxy<Dictionary<string, object>> {
            public OSExternalUserIdUpdateCompletionHandler() : base("OSExternalUserIdUpdateCompletionHandler") { }

            /// <param name="results">JSONObject</param>
            public void onSuccess(AndroidJavaObject results)
                => _complete(results.JSONObjectToDictionary());

            /// <param name="error">ExternalIdError</param>
            public void onFailure(AndroidJavaObject error) {
                SDKDebug.Error(error.Call<string>("toString"));
                // todo
            }
        }

        private sealed class OSSMSUpdateHandler : OneSignalAwaitableAndroidJavaProxy<Dictionary<string, object>> {
            public OSSMSUpdateHandler() : base("OSSMSUpdateHandler") { }

            /// <param name="result">JSONObject</param>
            public void onSuccess(AndroidJavaObject result)
                => _complete(result.JSONObjectToDictionary());

            /// <param name="error">OSSMSUpdateError</param>
            public void onFailure(AndroidJavaObject error) {
                SDKDebug.Error(error.Call<string>("toString"));
                // todo
            }
        }

        private sealed class OutcomeCallback : OneSignalAwaitableAndroidJavaProxy<OutcomeEvent> {
            public OutcomeCallback() : base("OutcomeCallback") { }

            /// <param name="outcomeEvent">OSOutcomeEvent</param>
            public void onSuccess(AndroidJavaObject outcomeEvent) { }
        }

        private sealed class PostNotificationResponseHandler : OneSignalAwaitableAndroidJavaProxy<Dictionary<string, object>> {
            public PostNotificationResponseHandler() : base("PostNotificationResponseHandler") { }

            /// <param name="response">JSONObject</param>
            public void onSuccess(AndroidJavaObject response)
                => _complete(response.JSONObjectToDictionary());

            /// <param name="response">JSONObject</param>
            public void onFailure(AndroidJavaObject response) {
                SDKDebug.Error(response.Call<string>("toString"));
                // todo
            }
        }
    }
}