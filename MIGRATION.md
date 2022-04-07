# OneSignal Unity SDK 2.0.0 to 3.0.0 Migration Guide

## Requirements
This guide assumes you are upgrading from a 2.x.x version of the OneSignal Unity SDK to the 3.x.x version. Additionally please ensure you are using Unity 2018.4 or newer.

## Adding 3.0.0 to your project
### Unity Package Manager
If your previous installation of the OneSignal Unity SDK was via the Unity Package Manager then...
1. In Unity open `Window > Package Manager`
2. From the `Package Manager` window select `Packages:` in the top left and click on `In Project`
3. Select the OneSignal Unity SDK(s) and press the `Upgrade to 3.x.x` button (make sure to update both Android and iOS packages)

### Unity Asset Store
Because a substantial amount of code has been changed in 3.x.x our recommendation is to completely remove the 2.x.x version of the OneSignal Unity SDK from your project.

1. todo
2. todo
3. todo

### Unitypackage distributable
As with the [Unity Asset Store](#Unity Asset Store) directions we recommend completely removing the 2.x.x version of the OneSignal Unity SDK from your project.

1. todo
2. todo
3. todo

## Namespace
You will notice that previous uses of OneSignal no longer can be found. In any file you are using the OneSignal Unity SDK please add to the top of the file:
```c#
using OneSignalSDK;
```

## Updating method calls
- [Initialization](#Initialization)
- [Debugging](#Debugging)
- [Privacy](#Privacy)
- [User Id](#User Id)
- [Push Notifications](#Push Notifications)
- [In App Messages](#In App Messages)
- [Email](#Email)
- [SMS](#SMS)
- [Location](#Location)
- [Tags](#Tags)
- [Outcomes](#Outcomes)
- [Lifecycle](#Lifecycle)
- [Other](#Other)

### Initialization
<table>
<tr><td>2.0.0</td><td>3.0.0</td></tr>
<td> <!-- init -->

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

### Debugging

<table>
<tr><td>2.0.0</td><td>3.0.0</td></tr>
<td> <!-- logging -->

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

### Privacy
<table>
<tr><td>2.0.0</td><td>3.0.0</td></tr>
<td> <!-- set privacy required -->

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
<tr>
<td> <!-- set consent status -->

```c#
OneSignal.UserDidProvideConsent(true);
```
</td>
<td>

```c#
OneSignal.Default.PrivacyConsent = true;
```
</td>
<tr>
<td> <!-- get consent status -->

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

### User Id

<table>
<tr><td>2.0.0</td><td>3.0.0</td></tr>
<td> <!-- todo -->

```c#
OneSignal.SetExternalUserId("3983ad1b-e31d-4df8-b063-85785ee34aa4");
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
OneSignal.SetExternalUserId("3983ad1b-e31d-4df8-b063-85785ee34aa4",
    response => Debug.Log("success")
);
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
OneSignal.RemoveExternalUserId();
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
OneSignal.RemoveExternalUserId(
    response => Debug.Log("success")
);
```
</td>
<td>

```c#
// todo
```
</td>
</table>

### Push Notifications

<table>
<tr><td>2.0.0</td><td>3.0.0</td></tr>
<td> <!-- prompt for push permission -->

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
// todo
```
</td>
<tr>
<td> <!-- get and use current push permission status -->

```c#
var currentDeviceState = OneSignal.GetPermissionSubscriptionState();
var currentStatus      = currentDeviceState.permissionStatus.status;

if (currentDeviceState.permissionStatus.hasPrompted) {
    // do if user was prompted
}
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- listen for and use push permission status changes -->

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
// todo
```
</td>
<tr>
<td> <!-- get current push sub data -->

```c#
var currentDeviceState = OneSignal.GetPermissionSubscriptionState();
var pushUserId         = currentDeviceState.subscriptionStatus.userId;
var pushIsSubscribed   = currentDeviceState.subscriptionStatus.subscribed;
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- listen for push sub changes -->

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
// todo
```
</td>
<tr>
<td> <!-- turn off push subscription -->

```c#
OneSignal.SetSubscription(false);
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- clear all onesignal notifs -->

```c#
OneSignal.ClearOneSignalNotifications();
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- get push sub ids -->

```c#
OneSignal.IdsAvailable((pushUserId, pushToken) => {
    // perform action with push info
});
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

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
// todo
```
</td>
</table>

### In App Messages

<table>
<tr><td>2.0.0</td><td>3.0.0</td></tr>
<td> <!-- todo -->

```c#
OneSignal.AddTrigger("triggerKey", 123);
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
OneSignal.AddTriggers(new Dictionary<string, object> {
    ["trigger1"] = "abc",
    ["trigger2"] = 456
});
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
OneSignal.RemoveTriggerForKey("trigger3");
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
OneSignal.RemoveTriggersForKeys(new[] { "trigger4", "trigger5" });
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
OneSignal.PauseInAppMessages(true);
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
// todo
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
// todo
```
</td>
<td>

```c#
// todo
```
</td>
</table>

### Email

<table>
<tr><td>2.0.0</td><td>3.0.0</td></tr>
<td> <!-- todo -->

```c#
OneSignal.SetEmail("user@email.com");
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
OneSignal.SetEmail("user@email.com",
    () => Debug.Log("success"),
    error => Debug.Log("error")
);
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
OneSignal.LogoutEmail();
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
OneSignal.LogoutEmail(
    () => Debug.Log("success"),
    error => Debug.Log("error")
);
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- get current email subscription data -->

```c#
var currentDeviceState = OneSignal.GetPermissionSubscriptionState();
var emailUserId        = currentDeviceState.emailSubscriptionStatus.emailUserId;
var emailAddress       = currentDeviceState.emailSubscriptionStatus.emailAddress;
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- listen for email sub changes -->

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
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
OneSignal.SyncHashedEmail("user@email.com");
```
</td>
<td>

```c#
// todo
```
</td>
</table>

### SMS
A new feature of 3.0.0 is the ability to manage a SMS subscription
<table>
<tr><td>2.0.0</td><td>3.0.0</td></tr>
<td> <!-- todo -->

```c#
// todo
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
// todo
```
</td>
<td>

```c#
// todo
```
</td>
</table>

### Location

<table>
<tr><td>2.0.0</td><td>3.0.0</td></tr>
<td> <!-- todo -->

```c#
OneSignal.SetLocationShared(true);
```
</td>
<td>

```c#
OneSignal.Default.ShareLocation = true;
```
</td>
<tr>
<td> <!-- todo -->

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

### Tags

<table>
<tr><td>2.0.0</td><td>3.0.0</td></tr>
<td> <!-- send one tag -->

```c#
OneSignal.SendTag("tagName", "tagValue");
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- send multiple tags -->

```c#
OneSignal.SendTags(new Dictionary<string, string> {
    ["tag1"] = "123",
    ["tag2"] = "abc"
});
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- get tags -->

```c#
OneSignal.GetTags(tags => {
    var tag3Value = tags["tag3"];
});
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- delete one tag -->

```c#
OneSignal.DeleteTag("tag4");
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- delete multiple tags -->

```c#
OneSignal.DeleteTags(new[] { "tag5", "tag6" });
```
</td>
<td>

```c#
// todo
```
</td>
</table>

### Outcomes

<table>
<tr><td>2.0.0</td><td>3.0.0</td></tr>
<td> <!-- todo -->

```c#
OneSignal.SendOutcome("outcomeName");
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
OneSignal.SendUniqueOutcome("uniqueOutcomeName");
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- todo -->

```c#
OneSignal.SendOutcomeWithValue("outcomeWithVal", 4.2f);
```
</td>
<td>

```c#
// todo
```
</td>
</table>

### Lifecycle

Previously setting up the callbacks for checking the 3 supported lifecycle methods had to be done exclusively during initialization. These are now available to be
subscribed to at any time and several new lifecycle methods have been added.

<table>
<tr><td>2.0.0</td><td>3.0.0</td></tr>
<td> <!-- notification opened -->

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
    // todo
}
```
</td>
<tr> 
<td> <!-- notification received -->

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
    if (someCheck)
        return null; // don't show the notificaiton

    // you can now make modifications to the notification before showing
    
    return notification; // show the notification
}
```
</td>
<tr>
<td> <!-- iam clicked -->

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
 
 void onIAMTriggedAction() {
    
 }
```
</td>
<tr>
</table>

### Other

<table>
<tr><td>2.0.0</td><td>3.0.0</td></tr>
<td> <!-- display type while focused -->

```c#
OneSignal.inFocusDisplayType = OneSignal.OSInFocusDisplayOption.InAppAlert;

// or

OneSignal.StartInit("your_app_id")
   .InFocusDisplaying(OneSignal.OSInFocusDisplayOption.Notification)
   .EndInit();
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- url launch style -->

```c#
OneSignal.StartInit("your_app_id")
   .Settings(new Dictionary<string, bool> { { OneSignal.kOSSettingsInAppLaunchURL, true } })
   .EndInit();
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- enable vibrate -->

```c#
OneSignal.EnableVibrate(true);
```
</td>
<td>

```c#
// todo
```
</td>
<tr>
<td> <!-- enable sound -->

```c#
OneSignal.EnableSound(true);
```
</td>
<td>

```c#
// todo
```
</td>
</table>
