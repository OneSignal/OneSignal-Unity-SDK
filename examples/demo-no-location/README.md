# OneSignal No-Location Demo

Minimal Unity project for apps that use OneSignal without the native location module.

This demo opts out of location two ways, so it stays no-location whether you build from the Editor or the command line:

- The `run-ios.sh` and `run-android.sh` scripts export `ONESIGNAL_DISABLE_LOCATION=true` before launching Unity. The environment variable takes precedence over the Editor toggle.
- The Editor toggle is checked in for interactive use (`ProjectSettings/OneSignalSettings.json`):

  ```json
  {
      "disableLocation": true
  }
  ```

If you open the project in the Editor without the environment variable, confirm **OneSignal > Disable Location Module** is checked before resolving Android dependencies or building iOS.

## Configure App ID

For scripted builds, `run-android.sh` and `run-ios.sh` read `ONESIGNAL_APP_ID` from the shell environment or from `.env` in this folder. The environment variable wins if both are set.

```sh
cp .env.example .env
```

Then edit `.env`:

```sh
ONESIGNAL_APP_ID=YOUR-ONESIGNAL-APP-ID
```

The build injects that value into the generated app without saving it into the tracked scene file.

For Play Mode in the Unity Editor:

1. Open `Assets/Scenes/Main.unity`.
2. Select the `OneSignal No Location Demo` GameObject.
3. Set `One Signal App Id` on the `NoLocationDemo` component.

If the value is left as `YOUR-ONESIGNAL-APP-ID`, the app starts but does not initialize OneSignal.

## Run On Android

Have an Android emulator running, then run:

```sh
./run-android.sh
```

Useful options:

```sh
./run-android.sh --no-install
./run-android.sh --install-only
```

Set `UNITY_PATH` if you want to use a specific Unity executable. Set `ADB` or `ANDROID_HOME` if the script cannot find `adb`.

## Run On iOS

Have an iOS simulator booted, then run:

```sh
./run-ios.sh
```

Recent macOS ships `openrsync` as `/usr/bin/rsync`, which fails the CocoaPods "Copy XCFrameworks" build phase (`renameat: No such file or directory` on the OneSignal dSYM). Install a real rsync once and `run-ios.sh` will prefer it automatically:

```sh
brew install rsync
```

Useful options:

```sh
./run-ios.sh --no-install
./run-ios.sh --install-only
./run-ios.sh --open
```

Set `UNITY_PATH` if you want to use a specific Unity executable.

The iOS script generates a minimal Podfile when External Dependency Manager does not create one, then runs `pod install` and builds the generated Xcode workspace.

## Clean Generated State

Use `clean.sh` when you want to retest setup from the Unity Editor or clear build artifacts before switching between dependency configurations:

```sh
./clean.sh
```

The script removes Unity-generated folders such as `Build/`, `Library/`, `Temp/`, `Obj/`, `Logs/`, and `UserSettings/`. It also removes the generated OneSignal dependency manifest XML files while preserving their `.meta` files so Unity GUIDs remain stable:

```text
Assets/OneSignal/Editor/OneSignalAndroidDependencies.xml
Assets/OneSignal/Editor/OneSignaliOSDependencies.xml
```

After running it, reopen the project in Unity. The OneSignal Editor setup code should regenerate those manifests from `ProjectSettings/OneSignalSettings.json`, where `disableLocation` is checked in as `true`. Confirm the regenerated files contain the granular no-location dependencies listed below.

## Native Dependencies

The SDK generates the EDM4U manifests per project at `Assets/OneSignal/Editor/OneSignal{Android,iOS}Dependencies.xml` based on the location flag, so the granular (no-location) dependency set below is what EDM4U resolves for this demo.

### Android

The resolver snapshot uses OneSignal native modules without the location artifact:

```xml
<package>com.onesignal:core:5.9.5</package>
<package>com.onesignal:notifications:5.9.5</package>
<package>com.onesignal:in-app-messages:5.9.5</package>
```

If you force-resolve again, keep the location module disabled.

### iOS

When location is disabled, iOS uses OneSignal pods without `OneSignalLocation`:

```xml
<iosPod name="OneSignalXCFramework/OneSignal" version="5.5.3" addToAllTargets="true" />
<iosPod name="OneSignalXCFramework/OneSignalInAppMessages" version="5.5.3" addToAllTargets="true" />
```

The notification service extension still uses `OneSignalXCFramework/OneSignalExtension`.

## App Code

`NoLocationDemo.cs` initializes OneSignal, requests push permission, sends a test push to the current subscription via the OneSignal REST API, and includes a test button for `OneSignal.Location.RequestPermission()`. With the native location module excluded, location calls no-op and `OneSignal.Location.IsShared` returns `false`.

The test push is sent with the app ID only (no REST API key), targeting the current `include_subscription_ids`, matching the other OneSignal no-location demos.

## Code Stripping (`link.xml`)

`Assets/OneSignal/link.xml` is required. Do not delete it.

The scripted builds use `ManagedStrippingLevel.High`, and the OneSignal platform implementations (`OneSignaliOS`, `OneSignalAndroid`) are resolved by reflection at runtime. Without this file, IL2CPP strips those types, so `OneSignal.Default` throws on first use and the SDK never initializes (the app shows `Not initialized` and no permission prompt appears).

In a normal SDK integration the OneSignal Setup window generates this file automatically; this demo is built headlessly via `BuildScript`, so the file is checked in instead.

## Generated Files

Unity build output is generated under `Build/`, `Library/`, `Logs/`, and `UserSettings/`. These folders are ignored.

Unity may also generate extra files under `ProjectSettings/`; this demo tracks only the minimal settings files needed to keep the project reproducible.
