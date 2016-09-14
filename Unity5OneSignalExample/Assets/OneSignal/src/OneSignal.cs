<<<<<<< HEAD
﻿/**
 * Modified MIT License
 * 
 * Copyright 2016 OneSignal
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

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8 || UNITY_WP_8_1)
#define ONESIGNAL_PLATFORM
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
#define ANDROID_ONLY
#endif

#if ONESIGNAL_PLATFORM && !UNITY_WP8 && !UNITY_WP_8_1
#define SUPPORTS_LOGGING
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OneSignalPush.MiniJSON;

public class OneSignal : MonoBehaviour {

   // NotificationReceived - Delegate is called when a push notification is opened or one is received when the user is in your game.
   // message        = The message text the use seen in the push notification.
   // additionalData = Dictionary of key value pairs sent with the push notification.
   // isActive       = True when the user was currently in your game when a notification was received.
   public delegate void NotificationReceived(string message, Dictionary<string, object> additionalData, bool isActive);
   
   public delegate void IdsAvailable(string playerID, string pushToken);
   public delegate void TagsReceived(Dictionary<string, object> tags);

   public delegate void OnPostNotificationSuccess(Dictionary<string, object> response);
   public delegate void OnPostNotificationFailure(Dictionary<string, object> response);

   public static IdsAvailable idsAvailableDelegate = null;
   public static TagsReceived tagsReceivedDelegate = null;

    public enum LOG_LEVEL {
        NONE, FATAL, ERROR, WARN, INFO, DEBUG, VERBOSE
    }

#if ONESIGNAL_PLATFORM
   #if SUPPORTS_LOGGING
      private static LOG_LEVEL logLevel = LOG_LEVEL.INFO, visualLogLevel = LOG_LEVEL.NONE;
   #endif

   private static OneSignalPlatform oneSignalPlatform = null;
   private static bool initialized = false;

   internal static NotificationReceived notificationDelegate = null;
   internal static OnPostNotificationSuccess postNotificationSuccessDelegate = null;
   internal static OnPostNotificationFailure postNotificationFailureDelegate = null;

   // Name of the GameObject that gets automaticly created in your game scene.
   private const string gameObjectName = "OneSignalRuntimeObject_KEEP";
#endif

   // Init - Only required method you call to setup OneSignal to recieve push notifications.
   //        Call this on the first scene that is loaded.
   // appId                  = Your OneSignal AppId from onesignal.com
   // googleProjectNumber    = Your Google Project Number that is only required for Android GCM pushes.
   // inNotificationDelegate = Calls this delegate when a notification is opened or one is received when the user is in your game.
   // autoRegister           = Set false to delay the iOS accept notification system prompt. Defaults true.
   //                          You can then call RegisterForPushNotifications at a better point in your game to prompt them.
   public static void Init(string appId, string googleProjectNumber, NotificationReceived inNotificationDelegate, bool autoRegister) {
      #if !UNITY_EDITOR
         #if ONESIGNAL_PLATFORM
            if (initialized) return;
            #if UNITY_ANDROID
               oneSignalPlatform = new OneSignalAndroid(gameObjectName, googleProjectNumber, appId, logLevel, visualLogLevel);
            #elif UNITY_IPHONE
               oneSignalPlatform = new OneSignalIOS(gameObjectName, appId, autoRegister, logLevel, visualLogLevel);
            #elif UNITY_WP8
               oneSignalPlatform = new OneSignalWP80(appId);
            #elif UNITY_WP_8_1
            oneSignalPlatform = new OneSignalWPWNS(appId);
            #endif
            notificationDelegate = inNotificationDelegate;
            
            #if !UNITY_WP8 && !UNITY_WP_8_1
               GameObject go = new GameObject(gameObjectName);
               go.AddComponent<OneSignal>();
               DontDestroyOnLoad(go);
            #endif
            
            initialized = true;
        #endif
      #else
         print("Please run OneSignal on a device to see push notifications.");
      #endif
   }

   // Parameter defaulting split out into different methods so they are compatible with UnityScript (AKA Unity Javascript).
   public static void Init(string appId, string googleProjectNumber, NotificationReceived inNotificationDelegate) {
      Init(appId, googleProjectNumber, inNotificationDelegate, true);
   }
   public static void Init(string appId, string googleProjectNumber) {
      Init(appId, googleProjectNumber, null, true);
   }
   public static void Init(string appId) {
      Init(appId, null, null, true);
   }

    public static void SetLogLevel(LOG_LEVEL inLogLevel, LOG_LEVEL inVisualLevel) {
      #if SUPPORTS_LOGGING
         logLevel = inLogLevel; visualLogLevel = inVisualLevel;
      #endif
    }

   // Tag player with a key value pair to later create segments on them at onesignal.com.
   public static void SendTag(string tagName, string tagValue) {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.SendTag(tagName, tagValue);
      #endif
   }

   // Tag player with a key value pairs to later create segments on them at onesignal.com.
   public static void SendTags(IDictionary<string, string> tags) {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.SendTags(tags);
      #endif
   }

   // Makes a request to onesignal.com to get current tags set on the player and then run the callback passed in.
   public static void GetTags(TagsReceived inTagsReceivedDelegate) {
      #if ONESIGNAL_PLATFORM
         tagsReceivedDelegate = inTagsReceivedDelegate;
         oneSignalPlatform.GetTags();
      #endif
   }

   // Set OneSignal.inTagsReceivedDelegate before calling this method or use the method above.
   public static void GetTags() {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.GetTags();
      #endif
   }

   public static void DeleteTag(string key) {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.DeleteTag(key);
      #endif
   }

   public static void DeleteTags(IList<string> keys) {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.DeleteTags(keys);
      #endif
   }

   // Call when the player has made an IAP purchase in your game so you can later send push notifications based on free or paid users.
   public static void SendPurchase(double amount) {
      #if UNITY_WP8 && !UNITY_EDITOR
         ((OneSignalWP80)oneSignalPlatform).SendPurchase(amount);
      #endif
   }

   // Call this when you would like to prompt an iOS user accept push notifications with the default system prompt.
   // Only use if you passed false to autoRegister when calling Init.
   public static void RegisterForPushNotifications() {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.RegisterForPushNotifications();
      #endif
   }

   // Call this if you need the playerId and/or pushToken
   // NOTE: pushToken maybe null if notifications are not accepted or there is connectivity issues. 
   public static void GetIdsAvailable(IdsAvailable inIdsAvailableDelegate) {
      #if ONESIGNAL_PLATFORM
         idsAvailableDelegate = inIdsAvailableDelegate;
         oneSignalPlatform.IdsAvailable();
      #endif
   }

   // Set OneSignal.idsAvailableDelegate before calling this method or use the method above.
   public static void GetIdsAvailable() {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.IdsAvailable();
      #endif
   }

   public static void EnableVibrate(bool enable) {
      #if ANDROID_ONLY
         ((OneSignalAndroid)oneSignalPlatform).EnableVibrate(enable);
      #endif
   }

   public static void EnableSound(bool enable) {
      #if ANDROID_ONLY
         ((OneSignalAndroid)oneSignalPlatform).EnableSound(enable);
      #endif
   }

   public static void EnableNotificationsWhenActive(bool enable) {
      #if ANDROID_ONLY
         ((OneSignalAndroid)oneSignalPlatform).EnableNotificationsWhenActive(enable);
      #endif
   }
   
   public static void ClearOneSignalNotifications() {
      #if ANDROID_ONLY
         ((OneSignalAndroid)oneSignalPlatform).ClearOneSignalNotifications();
      #endif
   }

   public static void EnableInAppAlertNotification(bool enable) {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.EnableInAppAlertNotification(enable);
      #endif
   }

   public static void SetSubscription(bool enable) {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.SetSubscription(enable);
      #endif
   }

   public static void PostNotification(Dictionary<string, object> data) {
      #if ONESIGNAL_PLATFORM
         PostNotification(data, null, null);
      #endif
   }

   public static void PostNotification(Dictionary<string, object> data, OnPostNotificationSuccess inOnPostNotificationSuccess, OnPostNotificationFailure inOnPostNotificationFailure) {
      #if ONESIGNAL_PLATFORM
         postNotificationSuccessDelegate = inOnPostNotificationSuccess;
         postNotificationFailureDelegate = inOnPostNotificationFailure;
         oneSignalPlatform.PostNotification(data);
      #endif
   }
   
   public static void SetEmail(string email) {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.SetEmail(email);
      #endif
   }

    public static void PromptLocation() {
        #if ONESIGNAL_PLATFORM
             oneSignalPlatform.PromptLocation();
        #endif
    }


    /*** protected and private methods ****/
#if ONESIGNAL_PLATFORM
      // Called from the native SDK - Called when a push notification is open or app is running when one comes in.
      private void onPushNotificationReceived(string jsonString) {
         if (notificationDelegate != null)
            oneSignalPlatform.FireNotificationReceivedEvent(jsonString, notificationDelegate);
      }
      
      // Called from the native SDK - Called when device is registered with onesignal.com service or right after GetIdsAvailable
      //                          if already registered.
      private void onIdsAvailable(string jsonString) {
         if (idsAvailableDelegate != null) {
            var ids = Json.Deserialize(jsonString) as Dictionary<string, object>;
            idsAvailableDelegate((string)ids["userId"], (string)ids["pushToken"]);
         }
      }

      // Called from the native SDK - Called After calling GetTags(...)
      private void onTagsReceived(string jsonString) {
         tagsReceivedDelegate(Json.Deserialize(jsonString) as Dictionary<string, object>);
      }

      // Called from the native SDK
      private void onPostNotificationSuccess(string response) {
         if (postNotificationSuccessDelegate != null) {
            OnPostNotificationSuccess tempPostNotificationSuccessDelegate = postNotificationSuccessDelegate;
            postNotificationFailureDelegate = null;
            postNotificationSuccessDelegate = null;
            tempPostNotificationSuccessDelegate(Json.Deserialize(response) as Dictionary<string, object>);
         }
      }
      
      // Called from the native SDK
      private void onPostNotificationFailed(string response) {
         if (postNotificationFailureDelegate != null) {
            OnPostNotificationFailure tempPostNotificationFailureDelegate = postNotificationFailureDelegate;
            postNotificationFailureDelegate = null;
            postNotificationSuccessDelegate = null;
            tempPostNotificationFailureDelegate(Json.Deserialize(response) as Dictionary<string, object>);
         }
      }
#endif
=======
﻿/**
 * Modified MIT License
 * 
 * Copyright 2016 OneSignal
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

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8 || UNITY_WP_8_1)
#define ONESIGNAL_PLATFORM
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
#define ANDROID_ONLY
#endif

#if ONESIGNAL_PLATFORM && !UNITY_WP8 && !UNITY_WP_8_1
#define SUPPORTS_LOGGING
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OneSignalPush.MiniJSON;


public class OSNotificationPayload {
   public string notificationID;
   public string sound;
   public string title;
   public string body;
   public string subtitle;
   public string launchURL;
   public Dictionary<string, string> additionalData;
   public Dictionary<string, object> actionButtons;
   public bool contentAvailable;
   public int badge;
   public string smallIcon;
   public string largeIcon;
   public string bigPicture;
   public string smallIconAccentColor;
   public string ledColor;
   public int lockScreenVisibility = 1;
   public string groupKey;
   public string groupMessage;
   public string fromProjectNumber;
}

public class OSNotification {
   public bool isInAppFocus;
   public bool shown;
   public bool silentNotification;
   public int androidNotificationId;
   public int displayType;
   public OSNotificationPayload payload;
}

public class OSNotificationAction {
   public string actionID;
   public int actionType;
   public OSNotification notification;
}

public class OneSignal : MonoBehaviour {

   // NotificationReceived - Delegate is called when a push notification is received when the user is in your game.
   // notification = The Notification dictionary filled from a serialized native OSNotification object
   public delegate void NotificationReceived(OSNotification notification);

   // NotificationOpened - Delegate is called when a push notification is opened.
   // result = The Notification open result describing : 1. The notification opened 2. The action taken by the user
   public delegate void NotificationOpened(OSNotificationAction result);
   
   public delegate void IdsAvailable(string playerID, string pushToken);
   public delegate void TagsReceived(Dictionary<string, object> tags);

   public delegate void OnPostNotificationSuccess(Dictionary<string, object> response);
   public delegate void OnPostNotificationFailure(Dictionary<string, object> response);

   public static IdsAvailable idsAvailableDelegate = null;
   public static TagsReceived tagsReceivedDelegate = null;

   public const string kOSSettingsAutoPrompt = "kOSSettingsAutoPrompt";
   public const string kOSSettingsInAppAlerts = "kOSSettingsInAppAlerts";
   public const string kOSSettingsInAppLaunchURL = "kOSSettingsInAppLaunchURL";

   public enum LOG_LEVEL {
        NONE, FATAL, ERROR, WARN, INFO, DEBUG, VERBOSE
    }

   public class UnityBuilder {
      public string appID = null;
      public string googleProjectNumber = null;
      public Dictionary<string, bool> iOSSettings = null;
      public NotificationReceived notificationReceivedDelegate = null;
      public NotificationOpened notificationOpenedDelegate = null;

      // inNotificationReceivedDelegate   = Calls this delegate when a notification is received.
      public UnityBuilder HandleNotificationReceived(NotificationReceived inNotificationReceivedDelegate) {
         notificationReceivedDelegate = inNotificationReceivedDelegate;
         return this;
      }

      // inNotificationOpenedDelegate     = Calls this delegate when a push notification is opened.
      public UnityBuilder HandleNotificationOpened(NotificationOpened inNotificationOpenedDelegate) {
         notificationOpenedDelegate = inNotificationOpenedDelegate;;
         return this;
      }

      // Pass one if the define kOSSettings strings as keys only. Only affects iOS platform.
      // autoPrompt                       = Set false to delay the iOS accept notification system prompt. Defaults true.
      //                                    You can then call RegisterForPushNotifications at a better point in your game to prompt them.
      // inAppAlerts                      = (iOS) Set false to implement a custom display of a notification when received while the app is in focus.
      // inAppLaunchURL                   = (iOS) Set false to force a ULRL to launch through Safari instead of in-app webview.
      public UnityBuilder Settings(Dictionary<string, bool> settings) {
         //bool autoPrompt, bool inAppAlerts, bool inAppLaunchURL
         #if UNITY_IPHONE
            iOSSettings = settings;
         #endif
         return this;
      }

      public void EndInit() {
         OneSignal.Init();
      }

   }
   internal static UnityBuilder builder = null;


#if ONESIGNAL_PLATFORM
   #if SUPPORTS_LOGGING
      private static LOG_LEVEL logLevel = LOG_LEVEL.INFO, visualLogLevel = LOG_LEVEL.NONE;
   #endif

   private static OneSignalPlatform oneSignalPlatform = null;
   private static bool initialized = false;

   internal static OnPostNotificationSuccess postNotificationSuccessDelegate = null;
   internal static OnPostNotificationFailure postNotificationFailureDelegate = null;

   // Name of the GameObject that gets automaticly created in your game scene.
   private const string gameObjectName = "OneSignalRuntimeObject_KEEP";
#endif

   // Init - Only required method you call to setup OneSignal to recieve push notifications.
   //        Call this on the first scene that is loaded.
   // appId                            = Your OneSignal AppId from onesignal.com
   // googleProjectNumber              = Your Google Project Number that is only required for Android GCM pushes.

   public static OneSignal.UnityBuilder StartInit(string appID, string googleProjectNumber = null) {
      if(builder == null)
            builder = new UnityBuilder();
      #if ONESIGNAL_PLATFORM
         builder.appID = appID;
         builder.googleProjectNumber = googleProjectNumber;
      #endif
      return builder;
   }

   private static void Init() {
      #if !UNITY_EDITOR
         #if ONESIGNAL_PLATFORM
            if (initialized == true || builder == null) return;
            #if UNITY_ANDROID
               oneSignalPlatform = new OneSignalAndroid(gameObjectName, builder.googleProjectNumber, builder.appID, logLevel, visualLogLevel);
            #elif UNITY_IPHONE
               //extract settings
               bool autoPrompt = true, inAppAlerts = true, inAppLaunchURL = true;
               if(builder.iOSSettings != null) {
                  if(builder.iOSSettings.ContainsKey(kOSSettingsAutoPrompt))
                     autoPrompt = builder.iOSSettings[kOSSettingsAutoPrompt];
                  if(builder.iOSSettings.ContainsKey(kOSSettingsInAppAlerts))
                     inAppAlerts = builder.iOSSettings[kOSSettingsInAppAlerts];
                  if(builder.iOSSettings.ContainsKey(kOSSettingsInAppLaunchURL))
                     inAppLaunchURL = builder.iOSSettings[kOSSettingsInAppLaunchURL];
               }
               oneSignalPlatform = new OneSignalIOS(gameObjectName, builder.appID, autoPrompt, inAppAlerts, inAppLaunchURL, logLevel, visualLogLevel);
            #elif UNITY_WP8
               oneSignalPlatform = new OneSignalWP80(builder.appID);
            #elif UNITY_WP_8_1
               oneSignalPlatform = new OneSignalWPWNS(builder.appID);
            #endif

            #if !UNITY_WP8 && !UNITY_WP_8_1
               GameObject go = new GameObject(gameObjectName);
               go.AddComponent<OneSignal>();
               DontDestroyOnLoad(go);
            #endif
            
            initialized = true;
        #endif
      #else
         print("Please run OneSignal on a device to see push notifications.");
      #endif
   }
   
    public static void SetLogLevel(LOG_LEVEL inLogLevel, LOG_LEVEL inVisualLevel) {
      #if SUPPORTS_LOGGING
         logLevel = inLogLevel; visualLogLevel = inVisualLevel;
      #endif
    }

   // Tag player with a key value pair to later create segments on them at onesignal.com.
   public static void SendTag(string tagName, string tagValue) {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.SendTag(tagName, tagValue);
      #endif
   }

   // Tag player with a key value pairs to later create segments on them at onesignal.com.
   public static void SendTags(IDictionary<string, string> tags) {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.SendTags(tags);
      #endif
   }

   // Makes a request to onesignal.com to get current tags set on the player and then run the callback passed in.
   public static void GetTags(TagsReceived inTagsReceivedDelegate) {
      #if ONESIGNAL_PLATFORM
         tagsReceivedDelegate = inTagsReceivedDelegate;
         oneSignalPlatform.GetTags();
      #endif
   }

   // Set OneSignal.inTagsReceivedDelegate before calling this method or use the method above.
   public static void GetTags() {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.GetTags();
      #endif
   }

   public static void DeleteTag(string key) {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.DeleteTag(key);
      #endif
   }

   public static void DeleteTags(IList<string> keys) {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.DeleteTags(keys);
      #endif
   }

   // Call when the player has made an IAP purchase in your game so you can later send push notifications based on free or paid users.
   public static void SendPurchase(double amount) {
      #if UNITY_WP8 && !UNITY_EDITOR
         ((OneSignalWP80)oneSignalPlatform).SendPurchase(amount);
      #endif
   }

   // Call this when you would like to prompt an iOS user accept push notifications with the default system prompt.
   // Only use if you passed false to autoRegister when calling Init.
   public static void RegisterForPushNotifications() {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.RegisterForPushNotifications();
      #endif
   }

   // Call this if you need the playerId and/or pushToken
   // NOTE: pushToken maybe null if notifications are not accepted or there is connectivity issues. 
   public static void GetIdsAvailable(IdsAvailable inIdsAvailableDelegate) {
      #if ONESIGNAL_PLATFORM
         idsAvailableDelegate = inIdsAvailableDelegate;
         oneSignalPlatform.IdsAvailable();
      #endif
   }

   // Set OneSignal.idsAvailableDelegate before calling this method or use the method above.
   public static void GetIdsAvailable() {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.IdsAvailable();
      #endif
   }

   public static void EnableVibrate(bool enable) {
      #if ANDROID_ONLY
         ((OneSignalAndroid)oneSignalPlatform).EnableVibrate(enable);
      #endif
   }

   public static void EnableSound(bool enable) {
      #if ANDROID_ONLY
         ((OneSignalAndroid)oneSignalPlatform).EnableSound(enable);
      #endif
   }

   public static void SetInFocusDisplaying(int display) {
      #if ANDROID_ONLY
         ((OneSignalAndroid)oneSignalPlatform).SetInFocusDisplaying(display);
      #endif
   }
   
   public static void ClearOneSignalNotifications() {
      #if ANDROID_ONLY
         ((OneSignalAndroid)oneSignalPlatform).ClearOneSignalNotifications();
      #endif
   }

   public static void SetSubscription(bool enable) {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.SetSubscription(enable);
      #endif
   }

   public static void PostNotification(Dictionary<string, object> data) {
      #if ONESIGNAL_PLATFORM
         PostNotification(data, null, null);
      #endif
   }

   public static void PostNotification(Dictionary<string, object> data, OnPostNotificationSuccess inOnPostNotificationSuccess, OnPostNotificationFailure inOnPostNotificationFailure) {
      #if ONESIGNAL_PLATFORM
         postNotificationSuccessDelegate = inOnPostNotificationSuccess;
         postNotificationFailureDelegate = inOnPostNotificationFailure;
         oneSignalPlatform.PostNotification(data);
      #endif
   }
   
   public static void SyncHashedEmail(string email) {
      #if ONESIGNAL_PLATFORM
         oneSignalPlatform.SyncHashedEmail(email);
      #endif
   }

    public void PromptLocation() {
        #if ONESIGNAL_PLATFORM
             oneSignalPlatform.PromptLocation();
        #endif
    }

    /*** protected and private methods ****/
#if ONESIGNAL_PLATFORM

      private OSNotification stringToNotification(string jsonString) {
            OSNotification notification = new OSNotification();
            OSNotificationPayload payload = new OSNotificationPayload();

            //Build OSNotification object from jsonString
            Dictionary<string, object> jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;
            Dictionary<string, object> payloadObj = jsonObject["payload"] as Dictionary<string, object>;
            if(payloadObj.ContainsKey("notificationID")) payload.notificationID = payloadObj["notificationID"] as string;
            if(payloadObj.ContainsKey("sound")) payload.sound = payloadObj["sound"] as string;
            if(payloadObj.ContainsKey("title")) payload.title = payloadObj["title"] as string;
            if(payloadObj.ContainsKey("body")) payload.body = payloadObj["body"] as string;
            if(payloadObj.ContainsKey("subtitle")) payload.subtitle = payloadObj["subtitle"] as string;
            if(payloadObj.ContainsKey("launchURL")) payload.launchURL = payloadObj["launchURL"] as string;
            if(payloadObj.ContainsKey("additionalData")) payload.additionalData = payloadObj["additionalData"] as Dictionary<string, string>;
            if(payloadObj.ContainsKey("actionButtons")) payload.actionButtons = payloadObj["actionButtons"] as Dictionary<string, object>;
            if(payloadObj.ContainsKey("contentAvailable")) payload.contentAvailable = (bool)payloadObj["contentAvailable"];
            if(payloadObj.ContainsKey("badge")) payload.badge = (int)payloadObj["badge"];
            if(payloadObj.ContainsKey("smallIcon")) payload.smallIcon = payloadObj["smallIcon"] as string;
            if(payloadObj.ContainsKey("largeIcon")) payload.largeIcon = payloadObj["largeIcon"] as string;
            if(payloadObj.ContainsKey("bigPicture")) payload.bigPicture = payloadObj["bigPicture"] as string;
            if(payloadObj.ContainsKey("smallIconAccentColor")) payload.smallIconAccentColor = payloadObj["smallIconAccentColor"] as string;
            if(payloadObj.ContainsKey("ledColor")) payload.ledColor = payloadObj["ledColor"] as string;
            if(payloadObj.ContainsKey("lockScreenVisibility")) payload.lockScreenVisibility = (int)payloadObj["lockScreenVisibility"];
            if(payloadObj.ContainsKey("groupKey")) payload.groupKey = payloadObj["groupKey"] as string;
            if(payloadObj.ContainsKey("groupMessage")) payload.groupMessage = payloadObj["groupMessage"] as string;
            if(payloadObj.ContainsKey("fromProjectNumber")) payload.fromProjectNumber = payloadObj["fromProjectNumber"] as string;
            notification.payload = payload;

            if(jsonObject.ContainsKey("isInAppFocus")) notification.isInAppFocus = (bool)jsonObject["isInAppFocus"];
            if(jsonObject.ContainsKey("shown")) notification.shown = (bool)jsonObject["shown"];
            if(jsonObject.ContainsKey("silentNotification")) notification.silentNotification = (bool)jsonObject["silentNotification"];
            if(jsonObject.ContainsKey("androidNotificationId")) notification.androidNotificationId = (int)jsonObject["androidNotificationId"];
            if(jsonObject.ContainsKey("displayType")) notification.displayType = (int)jsonObject["displayType"];

            return notification;
      }

      // Called from the native SDK - Called when a push notification received.
      private void onPushNotificationReceived(string jsonString) {
         if (builder.notificationReceivedDelegate != null) {
            oneSignalPlatform.FireNotificationReceivedEvent(stringToNotification(jsonString), builder.notificationReceivedDelegate);
         }
      }

      // Called from the native SDK - Called when a push notification is opened by the user
      private void onPushNotificationOpened(string jsonString) {
         if (builder.notificationOpenedDelegate != null) {

            OSNotificationAction action = new OSNotificationAction();
            action.notification = stringToNotification(jsonString);

            Dictionary<string, object> jsonObject = Json.Deserialize(jsonString) as Dictionary<string, object>;
            if(jsonObject.ContainsKey("actionID")) action.actionID = jsonObject["actionID"] as string;
            if(jsonObject.ContainsKey("actionType")) action.actionType = (int)jsonObject["actionType"];

            oneSignalPlatform.FireNotificationOpenedEvent(action, builder.notificationOpenedDelegate);
         }
      }
      
      // Called from the native SDK - Called when device is registered with onesignal.com service or right after GetIdsAvailable
      //                          if already registered.
      private void onIdsAvailable(string jsonString) {
         if (idsAvailableDelegate != null) {
            var ids = Json.Deserialize(jsonString) as Dictionary<string, object>;
            idsAvailableDelegate((string)ids["userId"], (string)ids["pushToken"]);
         }
      }

      // Called from the native SDK - Called After calling GetTags(...)
      private void onTagsReceived(string jsonString) {
         tagsReceivedDelegate(Json.Deserialize(jsonString) as Dictionary<string, object>);
      }

      // Called from the native SDK
      private void onPostNotificationSuccess(string response) {
         if (postNotificationSuccessDelegate != null) {
            OnPostNotificationSuccess tempPostNotificationSuccessDelegate = postNotificationSuccessDelegate;
            postNotificationFailureDelegate = null;
            postNotificationSuccessDelegate = null;
            tempPostNotificationSuccessDelegate(Json.Deserialize(response) as Dictionary<string, object>);
         }
      }

      // Called from the native SDK
      private void onPostNotificationFailed(string response) {
         if (postNotificationFailureDelegate != null) {
            OnPostNotificationFailure tempPostNotificationFailureDelegate = postNotificationFailureDelegate;
            postNotificationFailureDelegate = null;
            postNotificationSuccessDelegate = null;
            tempPostNotificationFailureDelegate(Json.Deserialize(response) as Dictionary<string, object>);
         }
      }
#endif
>>>>>>> 2.0.0
}