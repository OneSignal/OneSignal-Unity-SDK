# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
## [5.1.14]
### Changed
- [Fix] Missing .meta Android Migration files Unity editor error
## [5.1.13]
### Changed
- Updated included Android SDK from 5.1.26 to [5.1.31](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.31)
  - [Compatibility] Unity 6 build error with Android
  - [Fix] Incorrect activity path for NotificationOpenedActivityHMS
  - [Fix] Anonymous Login request not cleared if app is forced close within 5 seconds on a new install
  - [Fix] V4 to v5 upgrade will migrate app ID
  - [Fix] Notification click not foreground the app in the first click if app is closed and no clickListener is added
  - [Fix] Subscription/IAM not updated after upgrading from 5.2.0-beta or between 5.1.9 to 5.1.27
  - [Fix] ANR caused by model.add(), model.initializeFromJson(), or modelstore.load()
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-Android-SDK/releases)
- Updated included iOS SDK from 5.2.9 to [5.2.10](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.2.10)
  - [Fix] Requiring privacy consent blocks confirmed deliveries indefinitely
  - [Fix] Detect for timezone changes and update the user
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-iOS-SDK/releases)
## [5.1.12]
### Changed
- Updated included Android SDK from 5.1.25 to [5.1.26](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.26)
  - [Fix] ANR caused by operationRepo.enqueue while loading is in progress
  - [Fix] Check subscription Id before executing delete and update subscription operations
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-Android-SDK/releases)
- Updated included iOS SDK from 5.2.8 to [5.2.9](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.2.9)
  - [Fix] Use new OneSignalClientError type for callbacks which fixes crash report of NSInvalidArgumentException
  - [Fix] Don’t evaluate in app messages when paused which fixes issues with duration-since-last In-App Messages when pausing and unpausing
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-iOS-SDK/releases)

## [5.1.11]
### Changed
- Updated included Android SDK from 5.1.24 to [5.1.25](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.25)
  - Fix NullPointerException from getScheduleBackgroundRunIn
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-Android-SDK/releases)
- Updated included iOS SDK from 5.2.7 to [5.2.8](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.2.8)
  - Fix [__NSPlaceholderDictionary initWithObjects:forKeys:count:] crashes caused by nil HTTPResponse headers
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-iOS-SDK/releases)

## [5.1.10]
### Changed
- Updated included Android SDK from 5.1.21 to [5.1.24](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.24)
  - Pausing in-app messages now dismisses any currently displaying in-app messages
  - Fix setting consentGiven throwing if called before initWithContext
  - Fix BadTokenException and WindowLeaked exception caused by showing a dialog on a finishing or destroyed activity
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-Android-SDK/releases)
- Updated included iOS SDK from 5.2.4 to [5.2.7](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.2.7)
  - Pausing in-app messages now dismisses any currently displaying in-app messages
  - Don't use cached in-app messages if the SDK encounters an error fetching them or when the server returns none
  - Improve segment membership calculation that allows for fetching more accurate and updated in-app messages for a user
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-iOS-SDK/releases)

## [5.1.9]
### Changed
- Updated included Android SDK from 5.1.20 to [5.1.21](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.21)
  - Fix ending an already ended session
  - Fix permissions returned by onRequestPermissionResult when the list is empty
  - Fix keyboard input in HTML in-app messages
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-Android-SDK/releases)

## [5.1.8]
### Changed
- Updated included Android SDK from 5.1.17 to [5.1.20](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.20)
  - Optimized the initialization process by moving some service initialization to a background thread
  - Recover null onesignal ID crashes for Operations
  - Add option to default to HMS over FCM
  - Prevent retrying IAM display if 410 is received from backend
  - Fix dynamic triggers showing IAM repeatedly after being dismissed
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-Android-SDK/releases)
- Updated included iOS SDK from 5.2.2 to [5.2.4](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.2.4)
  - Handle incorrect `404` by delaying making updates to new users or subscriptions
  - The user executor needs to uncache first which fixes some cached requests being dropped for past users
  - Omit misleading fatal-level log for cross-platform SDKs
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-iOS-SDK/releases)

## [5.1.7]
### Changed
- Updated SDK to support Live Activities PushToStart and added a concept of a "Default" Live Activity to facilitate easier adoption. Please check out https://documentation.onesignal.com/docs/push-to-start-live-activities for more information and our [example app](https://github.com/OneSignal/OneSignal-Unity-SDK/tree/main/OneSignalExample) for an example implementation.
- Updated included Android SDK from 5.1.13 to [5.1.17](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.17)
  - Fixed Xiaomi notification click not foregrounding app
  - Fixed FCM push token not being refreshed
  - Poll for notification permission changes to detect permission change when prompting outside of OneSignal
  - Cold start creates new session and refreshes the user from the server
  - Immediately process pending operations when privacy consent goes from false to true
  - Fixed OneSignal.Notifications.RequestPermissionAsync() not firing when permission was already granted
  - Fixed Operation Model Store adding duplicate operations when the same ones that were previously added to the store and persisted, are re-read from cache
  - Fixed a bug causing clicking an unexpanded group notification results in only registering the click result for the final notification in the group
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-Android-SDK/releases)
- Updated included iOS SDK from 5.2.0 to [5.2.2](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.2.2)
  - Prevent In-App Message request crashes by making null values safe
  - Added Dispatch Queues to all executors to prevent concurrency crashes
  - Fixed clearing notifications incorrectly such as when pulling down the notification center
  - Fixed a purchases bug for the amount spent
  - Fixed a build issue for mac catalyst
  - Fixed crash when IAM window fails to load by using the main thread
  - Network call optimizations: Combine user property updates for network call improvements
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-iOS-SDK/releases)
### Fixed
- Additional instance of OneSignal error when calling OneSignal methods in Awake()
- iOS Mac Catalyst build error: Use of undeclared identifier 'OneSignalLiveActivitiesManagerImpl'

## [5.1.6]
### Fixed
- iOS build error: No type or protocol named OSLiveActivities

## [5.1.5]
### Changed
- Updated included Android SDK from 5.1.10 to [5.1.13](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.13)
  - Fixed the ANR issue caused by prolonged loading of OperationRepo and potentially by extended holding of the model lock during disk I/O read operations
  - Fixed IndexOutOfBounds exception thrown from OperationRepo.loadSavedOperations if app was opened offline, some operations done, and then the app is opened again
  - Targets JDK11 instead of JDK21 to address build errors encountered on certain development environments using JDK versions below 21
  - Fixed grouping skipping opRepoPostCreateDelay, causing operations being applied out of order when multiple login operations are pending
  - Fixed cancelling permission request dialog not firing continuation
  - Fixed RecoverFromDroppedLoginBug not running in very rare cases
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-Android-SDK/releases)
- Updated included iOS SDK from 5.1.6 to [5.2.0](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.2.0)
  - Added additional 6 privacy manifests to the 6 sub-targets that are included in the primary targets clients import
  - Updated User Defaults API reason to include app groups for appropriate modules
  - Fixed rare scenario of dropping data when multiple logins are called
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-iOS-SDK/releases)

## [5.1.4]
### Changed
- Updated included Android SDK from 5.1.9 to [5.1.10](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.10)
  - Handle incorrect 404 responses; add a delay after creates and retries on 404 of new ids
  - Added network call optimizations
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-Android-SDK/releases)
- Updated included iOS SDK from 5.1.5 to [5.1.6](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.1.6)
  - Fixed pending properties from being sent to the incorrect user when quickly changing users
  - Fixed crashes when encoding user models
  - Fixed crash in OneSignalAttachmentHandler trimURLSpacing method
  - Fixed crash when handling a dialog result
  - Removed IAM window when an in app message is inactive
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-iOS-SDK/releases)

## [5.1.3]
### Changed
- Updated included Android SDK from 5.1.8 to [5.1.9](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.9)
  - Added AndroidManifest option to override In-App Messages gray overlay and dropshadow
    - \<meta-data android:name="com.onesignal.inAppMessageHideGrayOverlay" android:value="true"/>
    - \<meta-data android:name="com.onesignal.inAppMessageHideDropShadow" android:value="true"/>
  - Fixed WorkManager not initialized crash
  - Fixed don't re-create user on failed remove alias
  - Added network call optimizations 
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-Android-SDK/releases)
- Updated included iOS SDK from 5.1.4 to [5.1.5](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.1.5)
  - Added plist option to hide gray overlay and disable dropshadow for In-App Messages
    - OneSignal_in_app_message_hide_gray_overlay
    - OneSignal_in_app_message_hide_drop_shadow
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-iOS-SDK/releases)

## [5.1.2]
### Changed
- Updated included Android SDK from 5.1.6 to [5.1.8](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.8)
  - Fixed externalId being skipped and updates to stop if something updates the User (such as addTag) shortly before login is called
  - Fixed optIn() not prompting if called before push subscription is created on backend
  - Fixed crash with EventProducer's fire events
  - Fixed context not being set on all entry points
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-Android-SDK/releases)
- Updated included iOS SDK from 5.1.3 to [5.1.4](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.1.4)
  - Signed XCFrameworks
  - Fixed stuck login requests 
  - For full changes, see the [native release notes](https://github.com/OneSignal/OneSignal-iOS-SDK/releases)

## [5.1.1]
### Changed
- Updated included Android SDK to [5.1.6](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.6)
- Updated included iOS SDK to [5.1.3](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.1.3)

## [5.1.0]
### Fixed
- iOS crash when calling OneSignal.User.PushSubscription.Id and OneSignal.User.PushSubscription.Token when they are null.
### Changed
- Add public getters for OneSignalId and ExternalId in the User namespace
- Add public event handler OneSignal.User.Changed that fires when the OneSignalId or ExternalId changes
- Updated included Android SDK to [5.1.5](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.5)

## [5.0.6]
### Fixed
- Duplicate symbol errors when building with other iOS plugins
- Removed READ_PHONE_STATE permission in Android builds. Delete your OneSignalConfig.androidlib and run the 
"Copy Android plugin to Assets" step in **Window > OneSignal SDK Setup** to apply the fix.
- Fixed lower build-tools versions being needed for Android builds. Delete your OneSignalConfig.androidlib and run the 
"Copy Android plugin to Assets" step in **Window > OneSignal SDK Setup** to apply the fix.
### Changed
- Updated included Android SDK to [5.1.2](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.1.2).
  - Android builds now require the Target API Level to be set to 33 or higher.
- Updated included iOS SDK to [5.1.0](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.1.0)

## [5.0.5]
### Changed
- Add public get tags method
- Updated included Android SDK to [5.0.5](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.0.5)
- Updated included iOS SDK to [5.0.5](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.0.5)
### Fixed
- Included meta files in OneSignalConfig.androidlib to prevent asset from being ignored
- Package download url in the "Sync example code bundle package" step from the OneSignal SDK Setup

## [5.0.4]
### Changed
- Updated included Android SDK to [5.0.3](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.0.3)

## [5.0.3]
### Changed
- `InstallEdm4uStep` now imports version [1.2.177](https://github.com/googlesamples/unity-jar-resolver/releases/tag/v1.2.177) of [EDM4U](https://github.com/googlesamples/unity-jar-resolver)
- Updated included Android SDK to [5.0.2](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.0.2)
- Updated included iOS SDK to [5.0.2](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.0.2)
- `OneSignalConfig.plugin` has been changed to `OneSignalConfig.androidlib`. Run the "Copy Android plugin to Assets" step in **Window > OneSignal SDK Setup** to migrate. Custom notification icons are now located in `Assets/Plugins/Android/OneSignalConfig.androidlib/src/main/res`
### Fixed
- Sending VSAttribution data from the editor
- iOS notifications clicked event firing if the app was cold started from clicking a notification
- ClassNotFoundException: com.onesignal.OneSignal for Android builds with minify enabled. You must run the "Copy Android plugin to Assets" step in **Window > OneSignal SDK Setup**.
- Disabled bitcode to avoid iOS build error

## [5.0.2]
### Fixed
- Stop foreground notifications from displaying after calling prevent default on iOS

## [5.0.1]
### Fixed
- Push subscription Id and Token malloc error on iOS

## [5.0.0]
### Changed
- Removed `SetLaunchURLsInApp`
- Removed async from location request permission and updated method name to `RequestPermission`
- Updated included iOS SDK to [5.0.1](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.0.1)
- Updated included Android SDK to [5.0.0](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.0.0)
- Updated default OneSignal Android notification icons to new logo
### Fixed
- NoSuchMethodError for outcome methods on Android
- Completion check for the Copy Android plugin to Assets setup step

## [5.0.0-beta.3]
### Changed
- Updated `Notifications`, `InAppMessages`, and `User` models to have Pascal Case properties
- Updated public API. Please see the updated [migration guide](https://github.com/OneSignal/OneSignal-Unity-SDK/blob/5.0.0-beta.3/MIGRATION_GUIDE_v3_to_v5.md) for the most up to date signatures. 
- Updated included Android SDK to [5.0.0-beta4](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.0.0-beta4)
- Updated included iOS SDK to [5.0.0-beta-04](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.0.0-beta-04)

## [5.0.0-beta.2]
### Added
- SDK type and version to api headers
### Changed
- Updated included Android SDK to [5.0.0-beta2](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.0.0-beta2)
- Updated included iOS SDK to [5.0.0-beta-02](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.0.0-beta-02)

## [5.0.0-beta.1]
### Added
- [Migration guide](https://github.com/OneSignal/OneSignal-Unity-SDK/blob/5.0.0-beta.1/MIGRATION_GUIDE_v3_to_v5.md
) for updating from 3.x.x to 5.x.x
### Changed
- Overhauled public API of the SDK to a user-centered model. While this release is in beta please see our included [example MonoBehaviour](https://github.com/OneSignal/OneSignal-Unity-SDK/blob/5.0.0-beta.1/OneSignalExample/Assets/OneSignal/Example/OneSignalExampleBehaviour.cs) for usage.
- Updated included Android SDK to [5.0.0-beta1](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/5.0.0-beta1)
- Updated included iOS SDK to [5.0.0-beta-01](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/5.0.0-beta-01)

If you run into any problems, please don’t hesitate to add to this [issue](https://github.com/OneSignal/OneSignal-Unity-SDK/issues/585)!

## [3.0.11]
### Fixed
- Fixed rare Android ANRs on callbacks firing and also when backgrounding the app.

## [3.0.10]
### Changed
- Updated included Android SDK to [4.8.5](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/4.8.5)
- Updated included iOS SDK to [3.12.4](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/3.12.4)
### Fixed
- Fixed InstallEdm4uStep to work with UPM EDM4U installations

## [3.0.9]
### Fixed
- Android - Lock OneSignal version so it doesn't get bumped to the next major version.

## [3.0.8]
### Changed
- Renamed `enterLiveActivity` to `EnterLiveActivity` and `exitLiveActivity` to `ExitLiveActivity`
- Updated Unity Verified Solutions Attribution script from VspAttribution to VSAttribution
### Fixed
- Resolved serialization depth limit 10 exceeded warning log

## [3.0.7]
### Changed
- Updated included iOS SDK to [3.12.3](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/3.12.3)
- Added support for OneSignal iOS functionality `enterLiveActivity` and `exitLiveActivity`

## [3.0.6]
### Fixed
- Android builds failing without the Unity iOS module
- Fixed app group name to be a property. Fixes [#545](https://github.com/OneSignal/OneSignal-Unity-SDK/issues/545)

## [3.0.5]
### Changed
- Updated included Android SDK to [4.8.3](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/4.8.3)
- Updated included iOS SDK to [3.11.5](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/3.11.5)
### Fixed
- Log current version number of the OneSignal SDK

## [3.0.4]
### Fixed
- Android `DeleteTags` and `RemoveTriggers` calls correctly use a Java array list instead of an array

## [3.0.3]
### Changed
- Added support for OneSignal Android functionality `promptForPushNotifications`
- Updated included Android SDK to [4.8.1](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/4.8.1)
- Updated included iOS SDK to [3.11.2](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/3.11.2)
- Added support for OneSignal Android `setLanguage` callbacks

## [3.0.2]
### Changed
- Updated included Android SDK to [4.7.1](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/4.7.1)
- Explicitly check for a diff and handle overwrites for the `AndroidManifest.xml` between the project's and package's `OneSignalConfig.plugin`
- `InstallEdm4uStep` checks for version number to determine if step is completed
### Fixed
- iOS build post processor checks for complete presence of extension
- iOS publishing error 90206 when uploading app to Apple.
- iOS builds on Unity on Windows failing on Entitlements file path. Fixes [#491](https://github.com/OneSignal/OneSignal-Unity-SDK/issues/442)
- `OneSignalXCFramework` pod version of `OneSignalNotificationServiceExtension` target in Podfile of iOS builds will be upgraded if target is present during post processing

## [3.0.1]
### Added
- [Migration guide](../../../MIGRATION.md) for updating from 2.x.x to 3.x.x
### Changed
- Added support for OneSignal iOS functionality `setLaunchURLsInApp`
- Improved included [README](../../../com.onesignal.unity.android/Editor/OneSignalConfig.plugin/README.md) for changing the notification icons in the Android plugin.
- Added inline documentation and Unity idiomatic fields to the `InAppMessageAction`
### Fixed
- Android deserialization of `NotificationAction` type now accounts for `actionID`
- Android deserialization of `Notification` type now accounts for `additionalData` in all cases
- Reverted [#430](https://github.com/OneSignal/OneSignal-Unity-SDK/pull/430) due to a deprecation of where Android resources can be stored in Unity. Notification icons to be changed for Android can again be found at `Assets/Plugins/Android/OneSignalConfig.plugin`. Fixes [#470](https://github.com/OneSignal/OneSignal-Unity-SDK/issues/470)
- Example code for `PostNotification` to show an example that works without the API key
- Reimplemented support for `RemoveExternalUserId`
- Reimplemented `disablePush` as `PushEnabled`
- iOS deserialization of `Notification` type now accounts for `additionalData` and `rawPayload` in all cases
- iOS notifications opened from cold start will be received via `NotificationOpened`
- Added missing `Notification` fields
- Added prefix to the NSExtensionPrincipalClass in the NotificationServiceExtension Info.plist
- Error deserialization for identity methods on Android

## [3.0.0]
### Changed
- Updated VSP Attribution with the latest version of script
- Moved EDM4U installer step to the core package as it is needed for both platforms
### Fixed
- Checks for VERSION file before attempting to read it
- Added podfile amendments to iOS Append builds
- Include utilities necessary for independent use of the initial unitypackage install
- Removed unused helper method in the iOS post processor which used code from after Unity 2018

## [3.0.0-beta.6]
### Fixed
- iOS build post processor will determine extension's imported OneSignalXCFramework from the package's dependencies xml. Fixes [#442](https://github.com/OneSignal/OneSignal-Unity-SDK/issues/442)
- iOS callbacks for the `NotificationPermissionChanged` event will no longer cause an il2cpp exception
### Changed
- Added AndroidManifest with location permissions to the example app to display `PromptLocation`
- `InstallEdm4uStep` now imports version [1.2.169](https://github.com/googlesamples/unity-jar-resolver/releases/tag/v1.2.169) of [EDM4U](https://github.com/googlesamples/unity-jar-resolver)
- Log an error in the example app when `RequiresPrivacyConsent` is attempted to be set to false from true
- Internal state mappings on iOS now rely on class defined objects over dynamic Dictionary types
- Replaced manual manipulation of iOS entitlements in post processing with Unity's [ProjectCapabilityManager](https://docs.unity3d.com/ScriptReference/iOS.Xcode.ProjectCapabilityManager.html)

## [3.0.0-beta.5]
### Changed
- Default export path for notification icons on Android changed to `Assets/Plugins/Android/res`
- Froze imported OneSignal iOS SDK to [3.10.0](https://github.com/OneSignal/OneSignal-iOS-SDK/releases/tag/3.10.0) release
- Froze imported OneSignal Android SDK to [4.6.5](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/4.6.5) release
### Removed
- Legacy AndroidManifest from past version of imported OneSignal Android SDK
- Legacy Android notification icons

## [3.0.0-beta.4]
### Added
- Included a new setup step from the OneSignal Unity Editor menu (**Window > OneSignal**) which syncs the example code bundle with the core package version
### Fixed
- `NotificationPermission` return from native SDK no longer raises a casting exception on iOS
- Resolved infinite loops on logging initialization conditions
- iOS postprocessing will respect existing entitlement files
- Will no longer init SDK again if done before `RuntimeInitializeOnLoadMethod`

## [3.0.0-beta.3]
### Fixed
- Eliminated syntax only supported on Unity 2020 or above
- Global callbacks on Android are now correctly setup post `initWithContext`
- Properly push `LogLevel` and `AlertLevel` settings to native SDKs
- Added missing setter to override the detected language. Fixes [#416](https://github.com/OneSignal/OneSignal-Unity-SDK/issues/416)
- Add missing getters for permission and subscription states
### Changed
- Implemented missing `Notification` properties `additionalData` and `actionButtons`
- `LogLevel` and `AlertLevel` now use a custom enum instead of the Unity `LogType`
- Removed `PermissionState` in favor of `NotificationPermission` enum
  - Renamed `PermissionStateChanged` event to `NotificationPermissionChanged`

## [3.0.0-beta.2]
### Fixed
- Correctly namedspaced the common MiniJSON utility to fix [#404](https://github.com/OneSignal/OneSignal-Unity-SDK/issues/404)
- Ensured code distributed with unitypackage would not reference other packages if missing
- Swapped out code that was only available in Unity 2020 for backwards compatible implementations

## [3.0.0-beta.1]
- Complete overhaul to the public API of the SDK. While this release is in beta please see our included [example MonoBehaviour](Example/OneSignalExampleBehaviour.cs) for usage.
- The included OneSignal Android SDK is now fully imported via EDM4U/gradle and will pull the latest version. Please see [OneSignal-Android-SDK Releases](https://github.com/OneSignal/OneSignal-Android-SDK/releases) for latest changes.
- The included OneSignal iOS SDK is now fully imported via EDM4U/Cocoapods and will pull the latest version. Please see [OneSignal-iOS-SDK Releases](https://github.com/OneSignal/OneSignal-iOS-SDK/releases) for latest changes.

If you run into any problems, please don’t hesitate to [open an issue](https://github.com/OneSignal/OneSignal-Unity-SDK/issues/new)!

## [2.14.6]
### Added
- Included [Unity's VSP Attribution](https://github.com/Unity-Technologies/com.unity.vsp-attribution) code for distribution via the Asset Store.

## [2.14.5]
### Changed
- Updated included Android SDK to [3.16.2](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/3.16.2)
  - Fixes for background image not showing and text not rendering in the RTL direction when a RTL system language is set. [#1475](https://github.com/OneSignal/OneSignal-Android-SDK/pull/1475)
  - Fix IAM preview message returning NPE in a preview case, due to message id being null. [#1463](https://github.com/OneSignal/OneSignal-Android-SDK/pull/1463)

## [2.14.4]
### Fixed
- Removed use of C# 8.0 features to maintain compatibility
### Changed
- Updated included Android SDK to [3.16.1](https://github.com/OneSignal/OneSignal-Android-SDK/releases/tag/3.16.1)
  - Avoid continuing with null IAM message Ids [#1386](https://github.com/OneSignal/OneSignal-Android-SDK/pull/1386)
- Updated included iOS SDK to version [2.16.7](https://github.com/OneSignal/OneSignal-iOS-SDK/tree/2.16.7/)
  - In App Messaging now respects device orientation locks for Unity Applications [#1000](https://github.com/OneSignal/OneSignal-iOS-SDK/pull/1000)

## [2.14.3]
### Fixed
- Added a delayed call when attempting to reshow the `OneSignalSetupWindow` after importing packages.
- Moved the example code to a separate assembly definition so that it may be utilized in place.
- The EDM4U setup step will rename the `Google.IOSResolver_v1.2.165.dll` on import in Unity 2021 and above. See EDM4U issue [#441](https://github.com/googlesamples/unity-jar-resolver/issues/441) for more information.
### Changed
- Updated formatting and documentation within the [OneSignalExampleBehaviour.cs](https://github.com/OneSignal/OneSignal-Unity-SDK/blob/main/OneSignalExample/Assets/OneSignal/Example/Scripts/OneSignalExampleBehaviour.cs) example code for clarity.
- Marked `EnabledVibrate` and `EnableSound` as `Obsolete` with as they do not function on Android 8+. Please check out https://documentation.onesignal.com/docs/android-notification-categories for more information.

## [2.14.2]
### Fixed
- Fixes rare iOS crash with some apps due to a threading issue. From [OneSignal-iOS-SDK PR #979](https://github.com/OneSignal/OneSignal-iOS-SDK/pull/979)

## [2.14.1]
### Fixed
- Corrected directory separators in post processor when building for iOS in a
  Windows environment. From PR [#376](https://github.com/OneSignal/OneSignal-Unity-SDK/pull/376)
  by [@SplenectomY](https://github.com/SplenectomY). Fixes [#375](https://github.com/OneSignal/OneSignal-Unity-SDK/issues/375), [#377](https://github.com/OneSignal/OneSignal-Unity-SDK/issues/377), [#380](https://github.com/OneSignal/OneSignal-Unity-SDK/issues/380)

## [2.14.0]
### Added
- A new Editor window under **Window > OneSignal** can be found which currently includes additional setup steps for installation.
### Changed
- The OneSignal Unity SDK has now transitioned to [Unity Package Manager](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@1.8/manual/index.html) support
  - If you are updating from a previous version of the OneSignal Unity SDK please follow the Unity Asset Store instructions in
      the [README](https://github.com/OneSignal/OneSignal-Unity-SDK/README.md#unity-asset-store) to ensure a smooth transition.

[Unreleased]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.14...HEAD
[5.1.14]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.13...5.1.14
[5.1.13]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.12...5.1.13
[5.1.12]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.11...5.1.12
[5.1.11]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.10...5.1.11
[5.1.10]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.9...5.1.10
[5.1.9]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.8...5.1.9
[5.1.8]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.7...5.1.8
[5.1.7]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.6...5.1.7
[5.1.6]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.5...5.1.6
[5.1.5]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.4...5.1.5
[5.1.4]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.3...5.1.4
[5.1.3]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.2...5.1.3
[5.1.2]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.1...5.1.2
[5.1.1]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.1.0...5.1.1
[5.1.0]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.0.6...5.1.0
[5.0.6]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.0.5...5.0.6
[5.0.5]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.0.4...5.0.5
[5.0.4]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.0.3...5.0.4
[5.0.3]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.0.2...5.0.3
[5.0.2]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.0.1...5.0.2
[5.0.1]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.0.0...5.0.1
[5.0.0]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.0.0-beta.3...5.0.0
[5.0.0-beta.3]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.0.0-beta.2...5.0.0-beta.3
[5.0.0-beta.2]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/5.0.0-beta.1...5.0.0-beta.2
[5.0.0-beta.1]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.9...5.0.0-beta.1
[3.0.9]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.8...3.0.9
[3.0.8]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.7...3.0.8
[3.0.7]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.6...3.0.7
[3.0.6]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.5...3.0.6
[3.0.5]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.4...3.0.5
[3.0.4]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.3...3.0.4
[3.0.3]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.2...3.0.3
[3.0.2]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.1...3.0.2
[3.0.1]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.0...3.0.1
[3.0.0]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.0-beta.6...3.0.0
[3.0.0-beta.6]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.0-beta.5...3.0.0-beta.6
[3.0.0-beta.5]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.0-beta.4...3.0.0-beta.5
[3.0.0-beta.4]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.0-beta.3...3.0.0-beta.4
[3.0.0-beta.3]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.0-beta.2...3.0.0-beta.3
[3.0.0-beta.2]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.0-beta.1...3.0.0-beta.2
[3.0.0-beta.1]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.6...3.0.0-beta.1
[2.14.6]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.5...2.14.6
[2.14.5]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.4...2.14.5
[2.14.4]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.3...2.14.4
[2.14.3]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.2...2.14.3
[2.14.2]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.1...2.14.2
[2.14.1]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.0...2.14.1
[2.14.0]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.13.6...2.14.0
