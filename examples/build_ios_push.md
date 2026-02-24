# iOS Setup: Push Notifications

Configure the demo Unity iOS project for OneSignal push notifications.

> For Live Activities setup, see [build_ios_la.md](build_ios_la.md).

---

## What the SDK handles automatically

The OneSignal Unity SDK includes a build post-processor (`com.onesignal.unity.ios/Editor/BuildPostProcessor.cs`) that runs automatically when you build for iOS. It takes care of:

- Adding push notification and background mode (remote-notification) capabilities
- Creating an App Group (`group.{bundleId}.onesignal`) shared between the app and extensions
- Creating the `OneSignalNotificationServiceExtension` target with its Swift source, Info.plist, and entitlements
- Appending the NSE target to the generated Podfile with the correct `OneSignalXCFramework` version
- Disabling bitcode on all targets

The post-processor runs at callback order 45, between EDM4U's Podfile generation (40) and `pod install` (50).

---

## 1. Prerequisites

- Install the [External Dependency Manager for Unity (EDM4U)](https://github.com/googlesamples/unity-jar-resolver) if it is not already included in your project. The OneSignal SDK relies on it to generate the Podfile and run `pod install`.
- Ensure your iOS build target is set in **File > Build Settings**.
- Set your **Bundle Identifier** and **Apple Developer Team ID** in **Edit > Project Settings > Player > iOS**.

---

## 2. Build the Xcode project

From Unity, build for iOS via **File > Build Settings > Build**. Choose an output directory (e.g. `Build/iOS`).

After the build completes, the output Xcode project will already contain:

- `Unity-iPhone` main target with push notification capabilities, background modes, and App Group entitlements
- `OneSignalNotificationServiceExtension` target with its own App Group entitlements
- A Podfile with both targets referencing `OneSignalXCFramework`

Open the `.xcworkspace` (not `.xcodeproj`) after running `pod install`.

---

## 3. Notification Service Extension (automated)

The SDK's post-processor creates this target automatically. For reference, these are the files it generates in the Xcode project.

### NotificationService.swift

Copied from `com.onesignal.unity.ios/Runtime/Plugins/iOS/NotificationService.swift`:

```swift
import UserNotifications
import OneSignalFramework

class NotificationService: UNNotificationServiceExtension {
    var contentHandler: ((UNNotificationContent) -> Void)?
    var receivedRequest: UNNotificationRequest!
    var bestAttemptContent: UNMutableNotificationContent?

    override func didReceive(_ request: UNNotificationRequest, withContentHandler contentHandler: @escaping (UNNotificationContent) -> Void) {
        self.receivedRequest = request
        self.contentHandler = contentHandler
        self.bestAttemptContent = (request.content.mutableCopy() as? UNMutableNotificationContent)

        if let bestAttemptContent = bestAttemptContent {
            OneSignal.didReceiveNotificationExtensionRequest(self.receivedRequest, with: bestAttemptContent, withContentHandler: self.contentHandler)
        }
    }

    override func serviceExtensionTimeWillExpire() {
        if let contentHandler = contentHandler, let bestAttemptContent = bestAttemptContent {
            OneSignal.serviceExtensionTimeWillExpireRequest(self.receivedRequest, with: self.bestAttemptContent)
            contentHandler(bestAttemptContent)
        }
    }
}
```

### Build properties set on the NSE target

| Property | Value |
|---|---|
| `TARGETED_DEVICE_FAMILY` | `1,2` |
| `IPHONEOS_DEPLOYMENT_TARGET` | `10.0` |
| `SWIFT_VERSION` | `5.0` |
| `ENABLE_BITCODE` | `NO` |

---

## 4. Install dependencies and open

After building from Unity, run `pod install` in the output directory if EDM4U did not run it automatically:

```sh
cd Build/iOS && pod install
```

Open the `.xcworkspace` file (not `.xcodeproj`) in Xcode, then build and run.

---

## Troubleshooting

- **Provisioning errors for the NSE**: Ensure your Apple Developer account has an App ID and provisioning profile for `{bundleId}.OneSignalNotificationServiceExtension`. The App Group `group.{bundleId}.onesignal` must be registered as well.
- **Pod install fails**: Make sure CocoaPods 1.11.3+ is installed. If using EDM4U, verify it is enabled under **Assets > External Dependency Manager > iOS Resolver > Settings**.
- **`CLANG_WARN_QUOTED_INCLUDE_IN_FRAMEWORK_HEADER` warnings**: Avoid setting this in extension build configurations since CocoaPods provides it via xcconfig.
- **Appending builds**: The SDK's post-processor checks for existing targets before creating them, so building to the same output directory multiple times is safe.
