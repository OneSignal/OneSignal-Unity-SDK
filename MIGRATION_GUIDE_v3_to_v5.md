# Unity v5.0.0-beta.3 Migration Guide
In this release, we are making a significant shift from a device-centered model to a user-centered model.  A user-centered model allows for more powerful omni-channel integrations within the OneSignal platform.

This migration guide will walk you through the Unity SDK v5.0.0 changes as a result of this shift.

# Overview

Under the user-centered model, the concept of a "player" is being replaced with three new concepts: users, subscriptions, and aliases.


## Users

A user is a new concept which is meant to represent your end-user.   A user has zero or more subscriptions and can be uniquely identified by one or more aliases.  In addition to subscriptions a user can have **data tags** which allows for user attribution.


## Subscription

A subscription refers to the method in which an end-user can receive various communications sent by OneSignal, including push notifications, in app messages, SMS, and email.  In previous versions of the OneSignal platform, this was referred to as a “player”. A subscription is in fact identical to the legacy “player” concept.  Each subscription has a **subscription_id** (previously, player_id) to uniquely identify that communication channel.


## Aliases

Aliases are a concept evolved from [external user ids](https://documentation.onesignal.com/docs/external-user-ids) which allows the unique identification of a user within a OneSignal application.  Aliases are a key-value pair made up of an **alias label** (the key) and an **alias id** (the value).   The **alias label** can be thought of as a consistent keyword across all users, while the **alias id** is a value specific to each user for that particular label. The combined **alias label** and **alias id** provide uniqueness to successfully identify a user.

OneSignal uses a built-in **alias label** called `external_id` which supports existing use of [external user ids](https://documentation.onesignal.com/docs/external-user-ids). `external_id` is also used as the identification method when a user identifies themselves to the OneSignal SDK via the `OneSignal.login` method.  Multiple aliases can be created for each user to allow for your own application's unique identifier as well as identifiers from other integrated applications.


# Migration (v3 to v5)

## Code Modularization

The OneSignal SDK has been updated to be more modular in nature.  The SDK has been split into namespaces and functionality previously in the static `OneSignal.Default` class has been moved to the appropriate namespace.  The namespaces, their containing modules, and how to access them in code are as follows:

| Namespace      | C#                         |
| -------------- | ---------------------------|
| User           | `OneSignal.User`           |
| Session        | `OneSignal.Session`        |
| Notifications  | `OneSignal.Notifications`  |
| Location       | `OneSignal.Location`       |
| InAppMessages  | `OneSignal.InAppMessages`  |
| LiveActivities | `OneSignal.LiveActivities` |
| Debug          | `OneSignal.Debug`          |



## Initialization

Initialization of the OneSignal SDK, although similar to past versions, has changed.  The target OneSignal application (`appId`) is now provided as part of initialization and cannot be changed post-initialization.  A typical initialization now looks similar to below

    OneSignal.Initialize(ONESIGNAL_APP_ID);
    // RequestPermissionAsync will show the native platform notification permission prompt.
    // We recommend removing the following code and instead using an In-App Message to prompt for notification permission.
    var result = await OneSignal.Notifications.RequestPermissionAsync(true);

If your integration is not user-centric, there is no additional startup code required.  A user is automatically created as part of the push subscription creation, both of which are only accessible from the current device and the OneSignal dashboard.

If your integration is user-centric, or you want the ability to identify as the same user on multiple devices, the OneSignal SDK should be called once the user has been identified:

    OneSignal.Login("USER_EXTERNAL_ID");

The `login` method will associate the device’s push subscription to the user that can be identified via alias  `externalId=USER_EXTERNAL_ID`.  If a user with the provided `externalId` does not exist, one will be created.  If a user does already exist, the user will be updated to include the current device’s push subscription.  Note that a device's push subscription will always be transferred to the currently logged in user, as they represent the current owners of that push subscription.

Once (or if) the user is no longer identifiable in your app (i.e. they logged out), the OneSignal SDK should be called:

    OneSignal.Logout();

Logging out has the affect of reverting to a “device-scoped” user, which is the new owner of the device’s push subscription.


## Subscriptions

In previous versions of the SDK there was a player that could have zero or one email address, and zero or one phone number for SMS.  In the user-centered model there is a user with the current device’s **Push Subscription** along with the ability to have zero or **more** email subscriptions and zero or **more** SMS subscriptions.  A user can also have zero or more push subscriptions, one push subscription for each device the user is logged into via the OneSignal SDK.

**Push Subscription**
The current device’s push subscription can be retrieved via:

    var pushSubscription = OneSignal.User.PushSubscription;


If at any point you want the user to stop receiving push notifications on the current device (regardless of permission status) you can use the push subscription to opt out:

    pushSubscription.OptOut();


To resume receiving of push notifications (driving the native permission prompt if OS permissions are not available), you can opt back in:

    pushSubscription.OptIn();


**Email/SMS Subscriptions**
Email and/or SMS subscriptions can be added or removed via:

    // Add email subscription
    OneSignal.User.AddEmail("customer@company.com")
    // Remove previously added email subscription
    OneSignal.User.RemoveEmail("customer@company.com")
    
    // Add SMS subscription
    OneSignal.User.AddSms("+15558675309")
    // Remove previously added SMS subscription
    OneSignal.User.RemoveSms("+15558675309")



## API Reference

Below is a comprehensive reference to the v5.0.0 OneSignal SDK.

**OneSignal**
The SDK is still accessible via a `OneSignal` static class, it provides access to higher level functionality and is a gateway to each subspace of the SDK.

| **C#**                                                          | **Description**                                                                                                                                                                                                                                                                                                     |
| --------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `bool ConsentRequired`                                          | *Determines whether a user must consent to privacy prior to their user data being sent up to OneSignal.  This should be set to `true` prior to the invocation of [Initialize] to ensure compliance.*                                                                                                                |
| `bool ConsentGiven`                                             | *Indicates whether privacy consent has been granted. This field is only relevant when the application has opted into data privacy protections. See [ConsentRequired].*                                                                                                                                              |
| `void Initialize(string appId)`                                 | *Initialize the OneSignal SDK.  This should be called during startup of the application.*                                                                                                                                                                                                                           |
| `void Login(string externalId, string jwtBearerToken = null)`   | *Login to OneSignal under the user identified by the [externalId] provided. The act of logging a user into the OneSignal SDK will switch the [user] context to that specific user.*<br><br>- *If the [externalId] exists the user will be retrieved and the context set from that user information. If operations have already been performed under a guest user, they* ***will not*** *be applied to the now logged in user (they will be lost).*<br>- *If the [externalId] does not exist the user will be created and the context set from the current local state. If operations have already been performed under a guest user those operations* ***will*** *be applied to the newly created user.*<br><br>***Push Notifications and In App Messaging***<br>*Logging in a new user will automatically transfer push notification and in app messaging subscriptions from the current user (if there is one) to the newly logged in user.  This is because both Push and IAM are owned by the device.* |
| `void Logout()`                                                 | *Logout the user previously logged in via [login]. The [user] property now references a new device-scoped user. A device-scoped user has no user identity that can later be retrieved, except through this device as long as the app remains installed and the app data is not cleared.*                            |
| `void SetLaunchURLsInApp(bool launchInApp)`                     | ***Note:*** *This method is for iOS only<br>This method can be used to set if launch URLs should be opened in Safari or within the application. Set to true to launch all notifications with a URL in the app instead of the default web browser. Make sure to call SetLaunchURLsInApp before the initialize call.* |


**User Namespace**
The user name space is accessible via `OneSignal.User` and provides access to user-scoped functionality.

| **C#**                                                | **Description**                                                                                                                                                                                                                          |
| ----------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `IPushSubscription PushSubscription`                  | *The push subscription associated to the current user.*                                                                                                                                                                                  |
| `string Language`                                     | *Set the 2-character language either as a detected language or explicitly set for this user.*                                                                                                                                            |
| `PushSubsription.Changed` <br><br> `event EventHandler<PushSubscriptionChangedEventArgs> Changed` <br><br> `PushSubscriptionChangedEventArgs { PushSubscriptionChangedState State }` | *Adds a change event that will run whenever the push subscription has been changed.*                      |
| `void AddAlias(string label, string id)`              | *Set an alias for the current user.  If this alias already exists it will be overwritten.*                                                                                                                                               |
| `void AddAliases(Dictionary<string, string> aliases)` | S*et aliases for the current user. If any alias already exists it will be overwritten.*                                                                                                                                                  |
| `void RemoveAlias(string label)`                      | *Remove an alias from the current user.*                                                                                                                                                                                                 |
| `void RemoveAliases(params string[] labels)`          | *Remove multiple aliases from the current user.*                                                                                                                                                                                         |
| `void AddEmail(string email)`                         | *Add a new email subscription to the current user.*                                                                                                                                                                                      |
| `void RemoveEmail(string email)`                      | *Remove an email subscription from the current user.*                                                                                                                                                                                    |
| `void AddSms(string sms)`                             | *Add a new SMS subscription to the current user.*                                                                                                                                                                                        |
| `void RemoveSms(string sms)`                          | *Remove an SMS subscription from the current user.*                                                                                                                                                                                      |
| `void AddTag(string key, string value)`               | *Add a tag for the current user.  Tags are key:value pairs used as building blocks for targeting specific users and/or personalizing messages. If the tag key already exists, it will be replaced with the value provided here.*         |
| `void AddTags(Dictionary<string, string> tags)`       | *Add multiple tags for the current user.  Tags are key:value pairs used as building blocks for targeting specific users and/or personalizing messages. If the tag key already exists, it will be replaced with the value provided here.* |
| `void RemoveTag(string key)`                          | *Remove the data tag with the provided key from the current user.*                                                                                                                                                                       |
| `void RemoveTags(params string[] keys)`               | *Remove multiple tags from the current user.*                                                                                                                                                                                            |


**Session Namespace**
The session namespace is accessible via `OneSignal.Session` and provides access to session-scoped functionality.

| **C#**                                               | **Description**                                                                          |
| ---------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| `void AddOutcome(string name)`                       | *Add an outcome with the provided name, captured against the current session.*           |
| `void AddUniqueOutcome(string name)`                 | *Add a unique outcome with the provided name, captured against the current session.*     |
| `void AddOutcomeWithValue(string name, float value)` | *Add an outcome with the provided name and value, captured against the current session.* |


**Notifications Namespace**
The notification namespace is accessible via `OneSignal.Notifications` and provides access to notification-scoped functionality.

| **C#**                                                                                                                                                       | **Description**                                                                                                                                                                                                                            |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `bool Permission`                                                                                                                                            | *Whether this app has push notification permission.*                                                                                                                                                                                       |
| `bool CanRequestPermission`                                                                                                                                  | *Whether this app can request push notification permission.*                                                                                                                                                                               |
| `NotificationPermission PermissionNative`                                                                                                                    | *Native permission of the device. The enum NotificationPermission can be NotDetermined, Denied, Authorized, Provisional, or Ephemeral.*                                                                                                    |
| `Task<bool> RequestPermissionAsync(bool fallbackToSettings)`                                                                                                 | *Prompt the user for permission to push notifications.  This will display the native OS prompt to request push notification permission.  If the user enables, a push subscription to this device will be automatically added to the user.* |
| `void ClearAllNotifications()`                                                                                                                               | *Removes all OneSignal notifications.*                                                                                                                                                                                                     |
| `event EventHandler<NotificationPermissionChangedEventArgs> PermissionChanged` <br><br> `NotificationPermissionChangedEventArgs { bool Permission }`         | *The [PermissionChanged] event will be fired when a notification permission setting changes. This happens when the user enables or disables notifications for your app from the system settings outside of your app.*                      |
| `event EventHandler<NotificationWillDisplayEventArgs> ForegroundWillDisplay` <br><br> `NotificationWillDisplayEventArgs { IDisplayableNotification Notification, void PreventDefault() }` | *Set an event to fire before displaying a notification while the app is in focus. Use this event to read notification data or decide if the notification should show or not.*                                 |
| `event EventHandler<NotificationClickEventArgs> Clicked` <br><br> `NotificationClickEventArgs { INotification Notification, INotificationClickResult Result }` | *Set an event that will fire whenever a notification is clicked on by the user.*                                                                                                                                                         |


**Location Namespace**
The location namespace is accessible via `OneSignal.Location` and provides access to location-scoped functionality.

| **C#**                     | **Description**                                                                                                                                          |
| ---------------------------| -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `bool IsShared`            | *Whether location is currently shared with OneSignal.*                                                                                                   |
| `Task<bool> RequestPermissionAsync()` | *Use this method to manually prompt the user for location permissions. This allows for geotagging so you send notifications to users based on location.* |


**InAppMessages Namespace**
The In App Messages namespace is accessible via `OneSignal.InAppMessages` and provides access to in app messages-scoped functionality.

| **C#**                                                                                                                             | **Description**                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| ---------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `bool Paused`                                                                                                                      | *Whether the in app messaging is currently paused.  When set to `true` no IAM will be presented to the user regardless of whether they qualify for them. When set to 'false` any IAMs the user qualifies for will be presented to the user at the appropriate time.*                                                                                                                                                                                              |
| `void AddTrigger(string key, string value)`                                                                                        | *Add a trigger for the current user.  Triggers are currently explicitly used to determine whether a specific IAM should be displayed to the user. See [Triggers](https://documentation.onesignal.com/docs/iam-triggers).*<br><br>*If the trigger key already exists, it will be replaced with the value provided here. Note that triggers are not persisted to the backend. They only exist on the local device and are applicable to the current user.*          |
| `void AddTriggers(Dictionary<string, string> triggers)`                                                                            | *Add multiple triggers for the current user.  Triggers are currently explicitly used to determine whether a specific IAM should be displayed to the user. See [Triggers](https://documentation.onesignal.com/docs/iam-triggers).*<br><br>*If the trigger key already exists, it will be replaced with the value provided here.  Note that triggers are not persisted to the backend. They only exist on the local device and are applicable to the current user.* |
| `void RemoveTrigger(string key)`                                                                                                   | *Remove the trigger with the provided key from the current user.*                                                                                                                                                                                                                                                                                                                                                                                                 |
| `void RemoveTriggers(params string[] keys)`                                                                                        | *Remove multiple triggers from the current user.*                                                                                                                                                                                                                                                                                                                                                                                                                 |
| `void ClearTriggers()`                                                                                                             | *Clear all triggers from the current user.*                                                                                                                                                                                                                                                                                                                                                                                                                       |
| `event EventHandler<InAppMessageWillDisplayEventArgs> WillDisplay` <br><br>  `event EventHandler<InAppMessageDidDisplayEventArgs> DidDisplay` <br><br> `event EventHandler<InAppMessageWillDismissEventArgs> WillDismiss` <br><br> `event EventHandler<InAppMessageDidDismissEventArgs> DidDismiss` <br><br><br> `InAppMessageWillDisplayEventArgs { IInAppMessage Message }` <br><br> `InAppMessageDidDisplayEventArgs { IInAppMessage Message }` <br><br> `InAppMessageWillDismissEventArgs { IInAppMessage Message }` <br><br> `InAppMessageDidDismissEventArgs { IInAppMessage Message }`| *Set the IAM lifecycle events.* |
| `event EventHandler<InAppMessageClickEventArgs> Clicked` <br><br> `InAppMessageClickEventArgs { IInAppMessage Message, IInAppMessageClickResult Result }` | *Set the IAM click events.*                                                                                                                                                                                                                                                                                                                                                                                                                |


**LiveActivities Namespace**
The Live Activities namespace is accessible via `OneSignal.LiveActivities` and provides access to live activities-scoped functionality.

| **C#**                                                   | **Description**                                                                                                                                                                                                                                                                 |
| -------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Task<bool> EnterAsync(string activityId, string token)` | ***Note:*** *This method is for iOS only<br>Entering a Live Activity associates an activityId with a live activity temporary push token on OneSignal's server. The activityId is then used with the OneSignal REST API to update one or multiple Live Activities at one time.*  |
| `Task<bool> ExitAsync(string activityId)`                | ***Note:*** *This method is for iOS only<br>Exiting a Live activity deletes the association between a customer defined activityId with a Live Activity temporary push token on OneSignal's server.*                                                                             |


**Debug Namespace**
The debug namespace is accessible via `OneSignal.Debug` and provides access to debug-scoped functionality.

| **C#**                | **Description**                                                                                    |
| --------------------- | -------------------------------------------------------------------------------------------------- |
| `LogLevel LogLevel`   | *The log level the OneSignal SDK should be writing to the Unity log. Defaults to [LogLevel.Warn].* |
| `LogLevel AlertLevel` | *The log level the OneSignal SDK should be showing as a modal. Defaults to [LogLevel.None].*       |


# Limitations 
- Recommend using only in development and staging environments for Beta releases.
- Outcomes will be available in a future release

# Known issues
- Identity Verification
    - We will be introducing JWT in follow up Beta release

# Troubleshooting
`Assets/OneSignal/Example/OneSignalExampleBehaviou.cs: error CS0246: The type or namespace name '...' cound not be found (are you missing a using directive or an assembly reference?)`
- Delete the directory at `Assets/OneSignal` and the xml file at `Assets/Plugins/Android/OneSignalConfig.plugin/AndroidManifest.xml`