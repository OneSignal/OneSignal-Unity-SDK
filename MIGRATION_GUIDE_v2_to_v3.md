# OneSignal Unity SDK 2.x.x to 3.x.x Migration Guide

## Requirements
This guide assumes you are upgrading from a 2.x.x version of the OneSignal Unity SDK to the 3.x.x version. Additionally please ensure you are using:
* Unity 2018.4 or newer
* For iOS builds: CocoaPods 1.11.3 or newer

## Adding 3.x.x to your project
Follow one of the following sections based on your previous install method of the OneSignal SDK.

### Unity Package Manager
1. If you have them delete the directory at `Assets/OneSignal` and the xml file at `Assets/Plugins/Android/OneSignalConfig.plugin/AndroidManifest.xml`
2. In Unity open **Window > Package Manager**
3. From the **Package Manager** window select **Packages:** in the top left and click on **In Project**
4. Select the OneSignal Unity SDK(s) and press the **Upgrade to 3.x.x** button (make sure to update both Android and iOS packages)
5. Follow the guides below for adding a [Namespace](#namespace) and [Updating method calls](#updating-method-calls) to fix any compilation errors
6. Check the menu at **Window > OneSignal SDK Setup** to see if there are any remaining steps to run

### Unity Asset Store
1. Delete the directory at `Assets/OneSignal` and the xml file at `Assets/Plugins/Android/OneSignalConfig.plugin/AndroidManifest.xml`
2. In Unity open **Window > Package Manager**
3. From the **Package Manager** window select **Packages:** in the top left and click on **My Assets**
4. Select the **OneSignal SDK** from the list and press the **Update** button.
5. Once the update has completed click the **Import** button
6. Navigate to **Window > OneSignal SDK Setup**
7. Click **Run All Steps**
8. Follow the guides below for adding a [Namespace](#namespace) and [Updating method calls](#updating-method-calls) to fix any compilation errors
9. Navigate back to the menu at **Window > OneSignal SDK Setup** to see if there are any remaining steps to run

### Unitypackage distributable
1. Delete the directory at `Assets/OneSignal` and the xml file at `Assets/Plugins/Android/OneSignalConfig.plugin/AndroidManifest.xml`
2. Download the latest release from our [releases page](https://github.com/OneSignal/OneSignal-Unity-SDK/releases)
3. In Unity navigate to **Assets > Import Package > Custom Package...** and select the newly downloaded `*.unitypackage` file
4. Navigate to **Window > OneSignal SDK Setup**
7. Click **Run All Steps**
8. Follow the guides below for adding a [Namespace](#namespace) and [Updating method calls](#updating-method-calls) to fix any compilation errors
9. Navigate back to the menu at **Window > OneSignal SDK Setup** to see if there are any remaining steps to run

## Namespace
You will notice that previous uses of OneSignal no longer can be found. In any file you are using the OneSignal Unity SDK please add to the top of the file:
```c#
using OneSignalSDK;
```

## Updating method calls
- [Added](#added)
  - [SMS](#sms)
- [Updated](#updated)
  - [Initialization](#initialization)
  - [Debugging](#debugging)
  - [Privacy](#privacy)
  - [User Id](#user-id)
  - [Push Notifications](#push-notifications)
  - [In App Messages](#in-app-messages)
  - [Email](#email)
  - [Location](#location)
  - [Tags](#tags)
  - [Outcomes](#outcomes)
  - [Lifecycle](#lifecycle)
  - [Other](#other)
- [Removed](#removed)

### Added
#### SMS
A new feature of 3.x.x is the ability to manage a SMS subscription.

Set the user's SMS Number
<table>
<td>

```c#
OneSignal.Default.SetSMSNumber("+12345556789");
```
</td>
</table>

Set the user's SMS Number and get the result of the call
<table>
<td>

```c#
var result = await OneSignal.Default.SetSMSNumber("+12345556789");
if (result) {
    Debug.Log("success");
}
else {
    Debug.Log("error");
}
```
</td>
</table>

Unlink the SMS subscription from the device so that it will no longer be updated
<table>
<td>

```c#
OneSignal.Default.LogOutSMS();
```
</td>
</table>

Unlink the SMS subscription from the device so that it will no longer be updated and get the result of the call
<table>
<td>

```c#
var result = await OneSignal.Default.LogOutSMS();
if (result) {
    Debug.Log("success");
}
else {
    Debug.Log("error");
}
```
</td>
</table>

Get the current SMS subscription status
<table>
<td>

```c#
var smsState  = OneSignal.Default.SMSSubscriptionState;
var smsUserId = smsState.smsUserId;
var smsNumber = smsState.smsNumber;
```
</td>
</table>

Listen for SMS subscription status changes
<table>
<td>

```c#
OneSignal.Default.SMSSubscriptionStateChanged += (current, previous) => {
    var smsSubscribed = current.isSubscribed;
};
```
</td>
</table>

### Updated
#### Initialization
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.StartInit("your_app_id")
   .EndInit();
```
</td>
<td>

```c#
OneSignal.Default.Initialize("your_app_id");
```
</td>
</table>

We also now include a prefab for codeless initialization!

Located in the `com.onesignal.unity.core` package we've include a simple prefab which initializes OneSignal. You can easily find it using the Asset search bar to find `OneSignalController.prefab` and making sure to select All** or **In Packages** for your search option. Drag the prefab into your very first scene, fill in the **App Id, and you are immediately ready to go!

#### Debugging
Set the log and alert levels
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SetLogLevel(OneSignal.LOG_LEVEL.INFO, OneSignal.LOG_LEVEL.ERROR);
```
</td>
<td>

```c#
OneSignal.Default.LogLevel   = LogLevel.Info;
OneSignal.Default.AlertLevel = LogLevel.Error;
```
</td>
</table>

#### Privacy
Set whether user consent is required to start the SDK
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SetRequiresUserPrivacyConsent(true); // before init

// or

OneSignal.StartInit("your_app_id")
   .SetRequiresUserPrivacyConsent(true)
   .EndInit();
```
</td>
<td>

```c#
OneSignal.Default.RequiresPrivacyConsent = true; // before init
```
</td>
</table>

Set the status of the user's consent
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.UserDidProvideConsent(true);
```
</td>
<td>

```c#
OneSignal.Default.PrivacyConsent = true;
```
</td>
</table>

Get the status of the user's consent
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
if (OneSignal.UserProvidedConsent()) {
    // user provided consent
}
```
</td>
<td>

```c#
if (OneSignal.Default.PrivacyConsent) {
    // user provided consent
}
```
</td>
</table>

#### User Id
Set the external user id
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SetExternalUserId("3983ad1b-e31d-4df8-b063-85785ee34aa4");
```
</td>
<td>

```c#
OneSignal.Default.SetExternalUserId("3983ad1b-e31d-4df8-b063-85785ee34aa4");
```
</td>
</table>

Set the external user id and get the result of the call
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SetExternalUserId("3983ad1b-e31d-4df8-b063-85785ee34aa4",
    result => Debug.Log("success")
);
```
</td>
<td>

```c#
var result = await OneSignal.Default.SetExternalUserId("3983ad1b-e31d-4df8-b063-85785ee34aa4");
if (result) {
    Debug.Log("success");
}
```
</td>
</table>

Remove the external user id
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.RemoveExternalUserId();
```
</td>
<td>

```c#
OneSignal.Default.RemoveExternalUserId();
```
</td>
</table>

Remove the external user id and get the result of the call
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.RemoveExternalUserId(
    result => Debug.Log("success")
);
```
</td>
<td>

```c#
var result = await OneSignal.Default.RemoveExternalUserId();
if (result) {
    Debug.Log("success");
}
```
</td>
</table>

#### Push Notifications
Prompt the user for permission to send push notifications
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.PromptForPushNotificationsWithUserResponse(response => {
    if (response) {
        // user accepted
    }
});
```
</td>
<td>

```c#
var response = await OneSignal.Default.PromptForPushNotificationsWithUserResponse();
if (response == NotificationPermission.Authorized) {
    // user accepted
}
```
</td>
</table>

Getting the current push notification permission status
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
var currentDeviceState = OneSignal.GetPermissionSubscriptionState();
var currentStatus      = currentDeviceState.permissionStatus.status;

if (currentDeviceState.permissionStatus.hasPrompted == false) {
    // do if user was not prompted
}
```
</td>
<td>

```c#
var currentStatus = OneSignal.Default.NotificationPermission;
if (currentStatus == NotificationPermission.NotDetermined) {
    // do if user was not prompted
}
```
</td>
</table>

Listen for push notification permission status changes
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.permissionObserver += changes => {
    var previousStatus = changes.from.status;
    var currentStatus  = changes.to.status;

    if (changes.to.hasPrompted) {
        // do if user was prompted
    }
};
```
</td>
<td>

```c#
OneSignal.Default.NotificationPermissionChanged += (current, previous) => {
    if (current != NotificationPermission.NotDetermined) {
        // do if user was prompted
    }
};
```
</td>
</table>

Get the current push notification subscription status
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
var currentDeviceState = OneSignal.GetPermissionSubscriptionState();
var pushUserId         = currentDeviceState.subscriptionStatus.userId;
var pushIsSubscribed   = currentDeviceState.subscriptionStatus.subscribed;
```
</td>
<td>

```c#
var pushState        = OneSignal.Default.PushSubscriptionState;
var pushUserId       = pushState.userId;
var pushIsSubscribed = pushState.isSubscribed;
```
</td>
</table>

Listen for push notification subscription status changes
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.subscriptionObserver += changes => {
    var prevPushState = changes.from;
    var currPushState = changes.to;

    var pushToken   = currPushState.pushToken;
    var pushEnabled = currPushState.userSubscriptionSetting;
};
```
</td>
<td>

```c#
OneSignal.Default.PushSubscriptionStateChanged += (current, previous) => {
    var pushToken   = current.pushToken;
    var pushEnabled = !current.isPushDisabled;
};
```
</td>
</table>

Disabling push notifications without removing the push subscription
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SetSubscription(false);
```
</td>
<td>

```c#
OneSignal.Default.PushEnabled = false;
```
</td>
</table>

Clear all OneSignal notifications from the notification shade
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.ClearOneSignalNotifications();
```
</td>
<td>

```c#
OneSignal.Default.ClearOneSignalNotifications();
```
</td>
</table>

Get the push notification subscription status' ids
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.IdsAvailable((pushUserId, pushToken) => {
    // perform action with push info
});
```
</td>
<td>

```c#
var pushUserId = OneSignal.Default.PushSubscriptionState.userId;
var pushToken  = OneSignal.Default.PushSubscriptionState.pushToken;
```
</td>
</table>

Sending a push to the user
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.IdsAvailable((pushUserId, pushToken) => {
    var notification = new Dictionary<string, object> {
        ["contents"]           = new Dictionary<string, string> { ["en"] = "Test Message" },
        ["include_player_ids"] = new List<string> { pushUserId },
        ["send_after"]         = DateTime.Now.ToUniversalTime().AddSeconds(30).ToString("U")
    };

    OneSignal.PostNotification(notification,
        response => Debug.Log("success"),
        error => Debug.Log("error")
    );
});
```
</td>
<td>

```c#
var pushUserId = OneSignal.Default.PushSubscriptionState.userId;
var pushOptions = new Dictionary<string, object> {
    ["contents"]           = new Dictionary<string, string> { ["en"] = "Test Message" },
    ["include_player_ids"] = new List<string> { pushUserId },
    ["send_after"]         = DateTime.Now.ToUniversalTime().AddSeconds(30).ToString("U")
};

var result = await OneSignal.Default.PostNotification(pushOptions);
if (result != null) {
    Debug.Log("success");
}
else {
    Debug.Log("error");
}
```
</td>
</table>

#### In App Messages
Set a trigger value
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.AddTrigger("triggerKey", 123);
```
</td>
<td>

```c#
OneSignal.Default.SetTrigger("triggerKey", 123);
```
</td>
</table>

Set several trigger values
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.AddTriggers(new Dictionary<string, object> {
    ["trigger1"] = "abc",
    ["trigger2"] = 456
});
```
</td>
<td>

```c#
OneSignal.Default.SetTriggers(new Dictionary<string, object> {
    ["trigger1"] = "abc",
    ["trigger2"] = 456
});
```
</td>
</table>

Removing a trigger value
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.RemoveTriggerForKey("trigger3");
```
</td>
<td>

```c#
OneSignal.Default.RemoveTrigger("trigger3");
```
</td>
</table>

Removing several trigger values
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.RemoveTriggersForKeys(new[] { "trigger4", "trigger5" });
```
</td>
<td>

```c#
OneSignal.Default.RemoveTriggers(new[] { "trigger4", "trigger5" });
```
</td>
</table>

Pause In-App Messages
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.PauseInAppMessages(true);
```
</td>
<td>

```c#
OneSignal.Default.InAppMessagesArePaused = true;
```
</td>
</table>

#### Email
Set the user's Email
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SetEmail("user@email.com");
```
</td>
<td>

```c#
OneSignal.Default.SetEmail("user@email.com");
```
</td>
</table>

Set the user's Email and get the result of the call
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SetEmail("user@email.com",
    () => Debug.Log("success"),
    error => Debug.Log("error")
);
```
</td>
<td>

```c#
var result = await OneSignal.Default.SetEmail("user@email.com");
if (result) {
    Debug.Log("success");
}
else {
    Debug.Log("error");
}
```
</td>
</table>

Unlink the Email subscription from the device so that it will no longer be updated
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.LogoutEmail();
```
</td>
<td>

```c#
OneSignal.Default.LogoutEmail();
```
</td>
</table>

Unlink the Email subscription from the device so that it will no longer be updated and get the result of the call
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.LogoutEmail(
    () => Debug.Log("success"),
    error => Debug.Log("error")
);
```
</td>
<td>

```c#
var result = await OneSignal.Default.LogoutEmail();
if (result) {
    Debug.Log("success");
}
else {
    Debug.Log("error");
}
```
</td>
</table>

Get the current Email subscription status
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
var currentDeviceState = OneSignal.GetPermissionSubscriptionState();
var emailUserId        = currentDeviceState.emailSubscriptionStatus.emailUserId;
var emailAddress       = currentDeviceState.emailSubscriptionStatus.emailAddress;
```
</td>
<td>

```c#
var emailState = OneSignal.Default.EmailSubscriptionState;
var emailUserId  = emailState.emailUserId;
var emailAddress = emailState.emailAddress;
```
</td>
</table>

Listen for Email subscription status changes
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.emailSubscriptionObserver += changes => {
    var prevEmailState = changes.from;
    var currEmailState = changes.to;

    var emailSubscribed = currEmailState.subscribed;
};
```
</td>
<td>

```c#
OneSignal.Default.EmailSubscriptionStateChanged += (current, previous) => {
    var emailSubscribed = current.isSubscribed;
};
```
</td>
</table>

Sync a hashed email
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SyncHashedEmail("user@email.com");
```
</td>
<td>

```c#
<b>REMOVED</b> - Please refer to our new Email methods/functionality such as setEmail()
```
</td>
</table>

#### Location
Set whether location sharing is enabled
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SetLocationShared(true);
```
</td>
<td>

```c#
OneSignal.Default.ShareLocation = true;
```
</td>
</table>

Prompt the user if they would like to share their location
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.PromptLocation();
```
</td>
<td>

```c#
OneSignal.Default.PromptLocation();
```
</td>
</table>

#### Tags
Send a tag with a value
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SendTag("tagName", "tagValue");
```
</td>
<td>

```c#
OneSignal.Default.SendTag("tagName", "tagValue");
```
</td>
</table>

Send multiple tags with values
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SendTags(new Dictionary<string, string> {
    ["tag1"] = "123",
    ["tag2"] = "abc"
});
```
</td>
<td>

```c#
OneSignal.Default.SendTags(new Dictionary<string, string> {
    ["tag1"] = "123",
    ["tag2"] = "abc"
});
```
</td>
</table>

Get all tags
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.GetTags(tags => {
    var tag3Value = tags["tag3"];
});
```
</td>
<td>

```c#
var tags = await OneSignal.Default.GetTags();
var tag3Value = tags["tag3"];
```
</td>
</table>

Delete a tag
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.DeleteTag("tag4");
```
</td>
<td>

```c#
OneSignal.Default.DeleteTag("tag4");
```
</td>
</table>

Delete multiple tags
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.DeleteTags(new[] { "tag5", "tag6" });
```
</td>
<td>

```c#
OneSignal.Default.DeleteTags(new[] { "tag5", "tag6" });
```
</td>
</table>

#### Outcomes
Send an outcome
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SendOutcome("outcomeName");
```
</td>
<td>

```c#
OneSignal.Default.SendOutcome("outcomeName");
```
</td>
</table>

Send a unique outcome
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SendUniqueOutcome("uniqueOutcomeName");
```
</td>
<td>

```c#
OneSignal.Default.SendUniqueOutcome("uniqueOutcomeName");
```
</td>
</table>

Send an outcome with a float value
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.SendOutcomeWithValue("outcomeWithVal", 4.2f);
```
</td>
<td>

```c#
OneSignal.Default.SendOutcomeWithValue("outcomeWithVal", 4.2f);
```
</td>
</table>

#### Lifecycle
Previously setting up the callbacks for checking the 3 supported lifecycle methods had to be done exclusively during initialization. These are now available to be subscribed to at any time and several new lifecycle methods have been added.

Listen for when a push notification opened the app
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.StartInit("your_app_id")
   .HandleNotificationOpened(onNotificationOpened)
   .EndInit();
   
void onNotificationOpened(OSNotificationOpenedResult result) {
    var notif = result.notification;
    var action = result.action;
}
```
</td>
<td>

```c#
OneSignal.Default.NotificationOpened += onNotificationOpened;

void onNotificationOpened(NotificationOpenedResult result) {
    var notif = result.notification;
    var action = result.action;
}
```
</td>
</table>

Listen for when a push notification is about to display while the app is in focus
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.StartInit("your_app_id")
   .HandleNotificationReceived(onNotificationReceived)
   .EndInit();

void onNotificationReceived(OSNotification notification) {
    var notifID = notification.payload.notificationID;
}
```
</td>
<td>

```c#
OneSignal.Default.NotificationWillShow += onNotificationWillShow;

Notification onNotificationWillShow(Notification notification) {
    if (someCheck) {
        return null; // don't show the notificaiton
    }
    // COMING SOON - make modifications to the notification before showing
    return notification; // show the notification
}
```
</td>
</table>

Listen for when an action of an In-App Message was triggered by a button click
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.StartInit("your_app_id")
   .HandleInAppMessageClicked(onIAMClicked)
   .EndInit();

void onIAMClicked(OSInAppMessageAction action) {
    var clickName = action.clickName;
}
```
</td>
<td>

```c#
OneSignal.Default.InAppMessageTriggeredAction += onIAMTriggedAction;

void onIAMTriggedAction(InAppMessageAction action) {
    var clickName  = action.clickName;
    var firstClick = action.firstClick;
}
```
</td>
<tr>
</table>
  
#### Android - Background Notification Control
If you added native Android code to your app to handle notifications with the Notification Extender Service (NES) make sure to follow the 
[Background Notification Control part of the native Android migration guide](https://documentation.onesignal.com/docs/40-api-android-native#background-notification-control).
  * Search for `com.onesignal.NotificationExtender` in your `AndroidManifest.xml` as an indicator if you set this up in the 2.x.x SDK.

#### Other

Set whether URLs embedded in push notification open within the app or in a browser
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.StartInit("your_app_id")
   .Settings(new Dictionary<string, bool> { { OneSignal.kOSSettingsInAppLaunchURL, true } })
   .EndInit();
```
</td>
<td>

```c#
OneSignal.Default.SetLaunchURLsInApp(true);
```
</td>
</table>

### Removed
Set whether or not push notifications show while the app is in focus
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.inFocusDisplayType = OneSignal.OSInFocusDisplayOption.InAppAlert;

// or

OneSignal.StartInit("your_app_id")
   .InFocusDisplaying(OneSignal.OSInFocusDisplayOption.Notification)
   .EndInit();
```
</td>
<td>
Replaced by the feature of the <b>NotificationWillShow</b> event to determine if a notification will show or not.
</td>
</table>

Enable vibrate
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.EnableVibrate(true);
```
</td>
<td>
<b>REMOVED</b> - Stopped working in Android 8+ due to a breaking change. To customize going forward, use 
<a href="https://documentation.onesignal.com/docs/android-notification-categories">Notification Categories (Channels)</a>.
</td>
</table>

Enable sound
<table><td>2.x.x</td><td>3.x.x</td><tr>
<td>

```c#
OneSignal.EnableSound(true);
```
</td>
<td>
<b>REMOVED</b> - Stopped working in Android 8+ due to a breaking change. To customize going forward, use 
<a href="https://documentation.onesignal.com/docs/android-notification-categories">Notification Categories (Channels)</a>.
</td>
</table>
