﻿/**
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

using System.Collections.Generic;

// Shared interface so OneSignal.cs can use each mobile platform in a generic way
public interface OneSignalPlatform {
   void SetLogLevel(OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel);
   void RegisterForPushNotifications();
   void promptForPushNotificationsWithUserResponse();
   void SendTag(string tagName, string tagValue);
   void SendTags(IDictionary<string, string> tags);
   void GetTags();
   void DeleteTag(string key);
   void DeleteTags(IList<string> keys);
   void IdsAvailable();
   void SetSubscription(bool enable);
   void PostNotification(Dictionary<string, object> data);
   void SyncHashedEmail(string email);
   void PromptLocation();
   void SetLocationShared(bool shared);

   void SetEmail (string email);
   void SetEmail(string email, string emailAuthToken);
   void LogoutEmail();

   void SetInFocusDisplaying(OneSignal.OSInFocusDisplayOption display);

   void UserDidProvideConsent(bool consent);
   bool UserProvidedConsent();
   void SetRequiresUserPrivacyConsent(bool required);


   void addPermissionObserver();
   void removePermissionObserver();
   void addSubscriptionObserver();
   void removeSubscriptionObserver();
   void addEmailSubscriptionObserver();
   void removeEmailSubscriptionObserver();

   OSPermissionSubscriptionState getPermissionSubscriptionState();

   OSPermissionState parseOSPermissionState(object stateDict);
   OSSubscriptionState parseOSSubscriptionState(object stateDict);
   OSEmailSubscriptionState parseOSEmailSubscriptionState (object stateDict);

   OSPermissionStateChanges parseOSPermissionStateChanges(string stateChangesJSONString);
   OSSubscriptionStateChanges parseOSSubscriptionStateChanges(string stateChangesJSONString);
   OSEmailSubscriptionStateChanges parseOSEmailSubscriptionStateChanges(string stateChangesJSONString);
}