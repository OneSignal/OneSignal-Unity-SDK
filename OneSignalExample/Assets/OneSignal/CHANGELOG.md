# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
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

[Unreleased]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.1...HEAD
[2.14.1]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.14.0...2.14.1
[2.14.0]: https://github.com/OneSignal/OneSignal-Unity-SDK/compare/2.13.6...2.14.0
