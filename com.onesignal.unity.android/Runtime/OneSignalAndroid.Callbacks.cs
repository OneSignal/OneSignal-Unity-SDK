using System.Collections.Generic;
using Laters;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedParameter.Local
namespace OneSignalSDK {
    public partial class OneSignalAndroid : OneSignal {
        private abstract class OneSignalAndroidJavaProxy: AndroidJavaProxy {
            protected OneSignalAndroidJavaProxy(string listenerClassName)
                : base(QualifiedSDKClass + "$" + listenerClassName) { }
        }
        
        private abstract class OneSignalAwaitableAndroidJavaProxy<TResult> : AwaitableAndroidJavaProxy<TResult> {
            protected OneSignalAwaitableAndroidJavaProxy(string listenerClassName)
                : base(QualifiedSDKClass + "$" + listenerClassName) { }
        }
        
        /*
         * Observers
         */
        
        private sealed class OSPermissionObserver : OneSignalAndroidJavaProxy {
            public OSPermissionObserver() : base("OSPermissionObserver") { }
            
            /// <param name="stateChanges">OSPermissionStateChanges</param>
            public void onOSPermissionChanged(AndroidJavaObject stateChanges) { }
        }
        
        private sealed class OSSubscriptionObserver : OneSignalAndroidJavaProxy {
            public OSSubscriptionObserver() : base("OSSubscriptionObserver") { }
            
            /// <param name="stateChanges">OSSubscriptionStateChanges</param>
            public void onOSSubscriptionChanged(AndroidJavaObject stateChanges) { }
        }
        
        private sealed class OSEmailSubscriptionObserver : OneSignalAndroidJavaProxy {
            public OSEmailSubscriptionObserver() : base("OSEmailSubscriptionObserver") { }
            
            /// <param name="stateChanges">OSEmailSubscriptionStateChanges</param>
            public void onOSEmailSubscriptionChanged(AndroidJavaObject stateChanges) { }
        }
        
        private sealed class OSSMSSubscriptionObserver : OneSignalAndroidJavaProxy {
            public OSSMSSubscriptionObserver() : base("OSSMSSubscriptionObserver") { }
            
            /// <param name="stateChanges">OSPermissionStateChanges</param>
            public void onSMSSubscriptionChanged(AndroidJavaObject stateChanges) { }
        }
        
        /*
         * Callbacks
         */

        private sealed class OSGetTagsHandler : OneSignalAwaitableAndroidJavaProxy<Dictionary<string, object>> {
            public OSGetTagsHandler() : base("OSGetTagsHandler") { }

            /// <param name="tags">JSONObject</param>
            public void tagsAvailable(AndroidJavaObject tags) { }
        }

        private sealed class ChangeTagsUpdateHandler : OneSignalAwaitableAndroidJavaProxy<Dictionary<string, object>>  {
            public ChangeTagsUpdateHandler() : base("ChangeTagsUpdateHandler") { }

            /// <param name="tags">JSONObject</param>
            public void onSuccess(AndroidJavaObject tags) { }

            /// <param name="error">SendTagsError</param>
            public void onFailure(AndroidJavaObject error) { }
        }

        private sealed class EmailUpdateHandler : AwaitableVoidAndroidJavaProxy {
            public EmailUpdateHandler() : base("EmailUpdateHandler") { }

            public void onSuccess() { }

            /// <param name="error">EmailUpdateError</param>
            public void onFailure(AndroidJavaObject error) { }
        }

        private sealed class OSExternalUserIdUpdateCompletionHandler : OneSignalAwaitableAndroidJavaProxy<Dictionary<string, object>> {
            public OSExternalUserIdUpdateCompletionHandler() : base("OSExternalUserIdUpdateCompletionHandler") { }

            /// <param name="results">JSONObject</param>
            public void onSuccess(AndroidJavaObject results) { }

            /// <param name="error">ExternalIdError</param>
            public void onFailure(AndroidJavaObject error) { }
        }

        private sealed class OSInAppMessageClickHandler : OneSignalAwaitableAndroidJavaProxy<InAppMessageAction> {
            public OSInAppMessageClickHandler() : base("OSInAppMessageClickHandler") { }

            /// <param name="result">OSInAppMessageAction</param>
            public void inAppMessageClicked(AndroidJavaObject result) { }
        }

        private sealed class OSNotificationOpenedHandler : OneSignalAwaitableAndroidJavaProxy<NotificationOpenedResult> {
            public OSNotificationOpenedHandler() : base("OSNotificationOpenedHandler") { }

            /// <param name="result">OSNotificationOpenedResult</param>
            public void notificationOpened(AndroidJavaObject result) { }
        }

        // todo - received result
        private sealed class OSNotificationWillShowInForegroundHandler : OneSignalAwaitableAndroidJavaProxy<NotificationOpenedResult> {
            public OSNotificationWillShowInForegroundHandler() : base("OSNotificationWillShowInForegroundHandler") { }

            /// <param name="notificationReceivedEvent">OSNotificationOpenedResult</param>
            public void notificationWillShowInForeground(AndroidJavaObject notificationReceivedEvent) { }
        }

        // todo - received result
        private sealed class OSRemoteNotificationReceivedHandler : OneSignalAwaitableAndroidJavaProxy<NotificationOpenedResult> {
            public OSRemoteNotificationReceivedHandler() : base("OSRemoteNotificationReceivedHandler") { }

            /// <param name="context">Context</param>
            /// <param name="notificationReceivedEvent">OSNotificationReceivedEvent</param>
            public void remoteNotificationReceived(AndroidJavaObject context, AndroidJavaObject notificationReceivedEvent) { }
        }

        private sealed class OSSMSUpdateHandler : OneSignalAwaitableAndroidJavaProxy<Dictionary<string, object>> {
            public OSSMSUpdateHandler() : base("OSSMSUpdateHandler") { }

            /// <param name="result">JSONObject</param>
            public void onSuccess(AndroidJavaObject result) { }

            /// <param name="error">OSSMSUpdateError</param>
            public void onFailure(AndroidJavaObject error) { }
        }

        private sealed class OutcomeCallback : OneSignalAwaitableAndroidJavaProxy<OutcomeEvent> {
            public OutcomeCallback() : base("OutcomeCallback") { }

            /// <param name="outcomeEvent">OSOutcomeEvent</param>
            public void onSuccess(AndroidJavaObject outcomeEvent) { }
        }

        private sealed class PostNotificationResponseHandler : OneSignalAwaitableAndroidJavaProxy<Dictionary<string, object>> {
            public PostNotificationResponseHandler() : base("PostNotificationResponseHandler") { }

            /// <param name="response">JSONObject</param>
            public void onSuccess(AndroidJavaObject response) { }

            /// <param name="response">JSONObject</param>
            public void onFailure(AndroidJavaObject response) { }
        }
    }
}