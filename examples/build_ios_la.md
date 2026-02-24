# iOS Setup: Live Activities

Configure the demo Unity iOS project for OneSignal Live Activities via a Widget Extension.

> Push notification setup is covered in [build_ios_push.md](build_ios_push.md). Complete that first — the Widget Extension builds on the same Xcode project.

---

## 1. Widget Extension (demo app)

Live Activities require a Widget Extension target that is **not** created by the core SDK. The demo app includes its own build post-processor (`Assets/App/Editor/iOS/BuildPostProcessor.cs`) that:

1. Sets `NSSupportsLiveActivities = true` in the main app's `Info.plist`
2. Creates the `ExampleWidgetExtension` target
3. Copies the widget Swift files into the Xcode project
4. Appends the widget target to the Podfile

The widget source files live in `iOS/ExampleWidget/` and are copied into the Xcode project at build time.

### ExampleWidgetLiveActivity.swift

Uses `DefaultLiveActivityAttributes` from the OneSignal SDK so no custom `ActivityAttributes` struct is needed:

```swift
#if !targetEnvironment(macCatalyst)
import ActivityKit
import WidgetKit
import SwiftUI
import OneSignalLiveActivities

struct ExampleWidgetLiveActivity: Widget {
    var body: some WidgetConfiguration {
        ActivityConfiguration(for: DefaultLiveActivityAttributes.self) { context in
            VStack {
                Spacer()
                Text("UNITY: " + (context.attributes.data["title"]?.asString() ?? "")).font(.headline)
                Spacer()
                HStack {
                    Spacer()
                    Label {
                        Text(context.state.data["message"]?.asDict()?["en"]?.asString() ?? "")
                    } icon: {
                        Image("onesignaldemo")
                            .resizable()
                            .scaledToFit()
                            .frame(width: 40.0, height: 40.0)
                    }
                    Spacer()
                }
                Text("INT: " + String(context.state.data["intValue"]?.asInt() ?? 0))
                Text("DBL: " + String(context.state.data["doubleValue"]?.asDouble() ?? 0.0))
                Text("BOL: " + String(context.state.data["boolValue"]?.asBool() ?? false))
                Spacer()
            }
            .activitySystemActionForegroundColor(.black)
            .activityBackgroundTint(.white)
        } dynamicIsland: { _ in
            DynamicIsland {
                DynamicIslandExpandedRegion(.leading) {
                    Text("Leading")
                }
                DynamicIslandExpandedRegion(.trailing) {
                    Text("Trailing")
                }
                DynamicIslandExpandedRegion(.bottom) {
                    Text("Bottom")
                }
            } compactLeading: {
                Text("L")
            } compactTrailing: {
                Text("T")
            } minimal: {
                Text("Min")
            }
            .widgetURL(URL(string: "http://www.apple.com"))
            .keylineTint(Color.red)
        }
    }
}
#endif
```

### ExampleWidgetBundle.swift

```swift
#if !targetEnvironment(macCatalyst)
import WidgetKit
import SwiftUI

@main
struct ExampleWidgetBundle: WidgetBundle {
    var body: some Widget {
        ExampleWidgetLiveActivity()
    }
}
#endif
```

### Info.plist

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
	<key>CFBundleDevelopmentRegion</key>
	<string>$(DEVELOPMENT_LANGUAGE)</string>
	<key>CFBundleDisplayName</key>
	<string>ExampleWidget</string>
	<key>CFBundleExecutable</key>
	<string>$(EXECUTABLE_NAME)</string>
	<key>CFBundleIdentifier</key>
	<string>$(PRODUCT_BUNDLE_IDENTIFIER)</string>
	<key>CFBundleInfoDictionaryVersion</key>
	<string>6.0</string>
	<key>CFBundleName</key>
	<string>$(PRODUCT_NAME)</string>
	<key>CFBundlePackageType</key>
	<string>$(PRODUCT_BUNDLE_PACKAGE_TYPE)</string>
	<key>CFBundleShortVersionString</key>
	<string>$(MARKETING_VERSION)</string>
	<key>CFBundleVersion</key>
	<string>$(CURRENT_PROJECT_VERSION)</string>
	<key>NSExtension</key>
	<dict>
		<key>NSExtensionPointIdentifier</key>
		<string>com.apple.widgetkit-extension</string>
	</dict>
</dict>
</plist>
```

### Build properties set on the widget target

| Property | Value |
|---|---|
| `TARGETED_DEVICE_FAMILY` | `1,2` |
| `IPHONEOS_DEPLOYMENT_TARGET` | `17.0` |
| `SWIFT_VERSION` | `5.0` |

---

## 2. Adding a Widget Extension to your own project

If you want Live Activities in your own Unity project (outside the demo), you need to:

1. **Create widget Swift files** similar to the ones in `iOS/ExampleWidget/`. At minimum you need a `WidgetBundle` entry point and a `Widget` using `ActivityConfiguration`.

2. **Create a build post-processor** in an `Editor/` folder that runs at callback order 45. It should:
   - Set `NSSupportsLiveActivities = true` in the main app's `Info.plist`
   - Call `PBXProject.AddAppExtension()` to create the widget target
   - Copy your Swift files into the Xcode project
   - Append the widget target to the Podfile with `OneSignalXCFramework`

   See `examples/demo/Assets/App/Editor/iOS/BuildPostProcessor.cs` for a working reference.

3. **Use the Live Activities API** from C# via `OneSignal.LiveActivities`:
   - `SetupDefault()` — register default Live Activity attributes
   - `StartDefault()` — start a Live Activity with the given attributes and content state
   - `EnterAsync()` / `ExitAsync()` — associate or remove an activity ID with a push token
   - `SetPushToStartToken()` / `RemovePushToStartToken()` — register push-to-start capability (iOS 17.2+)
