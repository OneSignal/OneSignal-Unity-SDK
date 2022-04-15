# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
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

[Unreleased]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/3.0.1...HEAD
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
