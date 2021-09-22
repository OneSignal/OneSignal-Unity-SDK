/*
 * Modified MIT License
 *
 * Copyright 2017 OneSignal
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
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class OneSignalIOS : OneSignal {
        [DllImport("__Internal")] private static extern void _setNotificationReceivedCallback();
        [DllImport("__Internal")] private static extern void _setNotificationOpenedCallback();
        [DllImport("__Internal")] private static extern void _setInAppMessageClickedCallback();
        
        [DllImport("__Internal")] private static extern void _setPermissionStateChangedCallback();
        [DllImport("__Internal")] private static extern void _setSubscriptionStateChangedCallback();
        [DllImport("__Internal")] private static extern void _setEmailSubscriptionStateChangedCallback();
        [DllImport("__Internal")] private static extern void _setSMSSubscriptionStateChangedCallback();
        
        [DllImport("__Internal")] private static extern void _setPrivacyConsent(bool consent);
        [DllImport("__Internal")] private static extern bool _getPrivacyConsent();
        [DllImport("__Internal")] private static extern void _setRequiresPrivacyConsent(bool required);
        [DllImport("__Internal")] private static extern bool _getRequiresPrivacyConsent();
        [DllImport("__Internal")] private static extern void _initialize(string appId);
        [DllImport("__Internal")] private static extern void _registerForPushNotifications();
        [DllImport("__Internal")] private static extern void _promptForPushNotificationsWithUserResponse(BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _clearOneSignalNotifications();
        
        // post notif
        
        [DllImport("__Internal")] private static extern void _setTrigger(string key, string value);
        [DllImport("__Internal")] private static extern void _setTriggers(); // todo
        [DllImport("__Internal")] private static extern void _removeTrigger(string key);
        [DllImport("__Internal")] private static extern void _removeTriggers(); // todo
        [DllImport("__Internal")] private static extern void _getTrigger(string key); // todo
        [DllImport("__Internal")] private static extern void _getTriggers(); // todo
        [DllImport("__Internal")] private static extern void _setInAppMessagesArePaused(bool paused);
        [DllImport("__Internal")] private static extern bool _getInAppMessagesArePaused();

        // todo - tasky things
        
        [DllImport("__Internal")] private static extern void _promptLocation();
        [DllImport("__Internal")] private static extern void _setShareLocation(bool share);
        [DllImport("__Internal")] private static extern bool _getShareLocation();
        
        
    }
}