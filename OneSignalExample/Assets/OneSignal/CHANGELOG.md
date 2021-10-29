# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
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

[Unreleased]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.5...HEAD
[2.14.5]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.4...2.14.5
[2.14.4]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.3...2.14.4
[2.14.3]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.2...2.14.3
[2.14.2]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.1...2.14.2
[2.14.1]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.0...2.14.1
[2.14.0]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.13.6...2.14.0
