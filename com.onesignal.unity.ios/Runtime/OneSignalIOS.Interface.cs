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

using System.Runtime.InteropServices;

namespace OneSignalSDK {
    public sealed partial class OneSignalIOS : OneSignal {
        /*
         * Global callbacks
         */
        
        [DllImport("__Internal")] private static extern void _setNotificationReceivedCallback(NotificationWillShowInForegroundDelegate callback);
        [DllImport("__Internal")] private static extern void _setNotificationOpenedCallback(StringListenerDelegate callback);
        
        [DllImport("__Internal")] private static extern void _setInAppMessageWillDisplayCallback(StringListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _setInAppMessageDidDisplayCallback(StringListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _setInAppMessageWillDismissCallback(StringListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _setInAppMessageDidDismissCallback(StringListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _setInAppMessageClickedCallback(StringListenerDelegate callback);
        
        [DllImport("__Internal")] private static extern void _setPermissionStateChangedCallback(StateListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _setSubscriptionStateChangedCallback(StateListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _setEmailSubscriptionStateChangedCallback(StateListenerDelegate callback);
        [DllImport("__Internal")] private static extern void _setSMSSubscriptionStateChangedCallback(StateListenerDelegate callback);
        
        /*
         * Direct methods
         */
        
        [DllImport("__Internal")] private static extern string _getDeviceState();
        
        [DllImport("__Internal")] private static extern void _setLogLevel(int logLevel, int alertLevel);
        [DllImport("__Internal")] private static extern void _setPrivacyConsent(bool consent);
        [DllImport("__Internal")] private static extern bool _getPrivacyConsent();
        [DllImport("__Internal")] private static extern void _setRequiresPrivacyConsent(bool required);
        [DllImport("__Internal")] private static extern bool _getRequiresPrivacyConsent();
        [DllImport("__Internal")] private static extern void _setLaunchURLsInApp(bool launchInApp);
        [DllImport("__Internal")] private static extern void _initialize(string appId);
        [DllImport("__Internal")] private static extern void _promptForPushNotificationsWithUserResponse(int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _disablePush(bool disable);
        [DllImport("__Internal")] private static extern void _postNotification(string optionsJson, int hashCode, StringResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _setTrigger(string key, string value);
        [DllImport("__Internal")] private static extern void _setTriggers(string triggersJson);
        [DllImport("__Internal")] private static extern void _removeTrigger(string key);
        [DllImport("__Internal")] private static extern void _removeTriggers(string triggersJson);
        [DllImport("__Internal")] private static extern string _getTrigger(string key);
        [DllImport("__Internal")] private static extern string _getTriggers();
        [DllImport("__Internal")] private static extern void _setInAppMessagesArePaused(bool paused);
        [DllImport("__Internal")] private static extern bool _getInAppMessagesArePaused();

        [DllImport("__Internal")] private static extern void _sendTag(string name, string value, int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _sendTags(string tagsJson, int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _getTags(int hashCode, StringResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _deleteTag(string name, int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _deleteTags(string tagsJson, int hashCode, BooleanResponseDelegate callback);
        
        [DllImport("__Internal")] private static extern void _setExternalUserId(string externalId, string authHash, int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _setEmail(string email, string authHash, int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _setSMSNumber(string smsNumber, string authHash, int hashCode, BooleanResponseDelegate callback);

        [DllImport("__Internal")] private static extern void _removeExternalUserId(int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _logoutEmail(int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _logoutSMSNumber(int hashCode, BooleanResponseDelegate callback);
        
        [DllImport("__Internal")] private static extern void _setLanguage(string languageCode, int hashCode, BooleanResponseDelegate callback);

        [DllImport("__Internal")] private static extern void _promptLocation();
        [DllImport("__Internal")] private static extern void _setShareLocation(bool share);
        [DllImport("__Internal")] private static extern bool _getShareLocation();
        
        [DllImport("__Internal")] private static extern void _sendOutcome(string name, int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _sendUniqueOutcome(string name, int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _sendOutcomeWithValue(string name, float value, int hashCode, BooleanResponseDelegate callback);
    }
}