# OneSignal No-Location Demo

Minimal Unity project for apps that use OneSignal without the native location module.

The saved OneSignal project setting is checked in as:

```json
{
    "disableLocation": true
}
```

Before resolving Android dependencies or building iOS, confirm **OneSignal > Disable Location Module** is checked.

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

Useful options:

```sh
./run-ios.sh --no-install
./run-ios.sh --install-only
./run-ios.sh --open
```

Set `UNITY_PATH` if you want to use a specific Unity executable.

The iOS script generates a minimal Podfile when External Dependency Manager does not create one, then runs `pod install` and builds the generated Xcode workspace.

## Native Dependencies

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

`NoLocationDemo.cs` initializes OneSignal, requests push permission, and includes a test button for `OneSignal.Location.RequestPermission()`. With the native location module excluded, location calls no-op and `OneSignal.Location.IsShared` returns `false`.

## Generated Files

Unity build output is generated under `Build/`, `Library/`, `Logs/`, and `UserSettings/`. These folders are ignored.

Unity may also generate extra files under `ProjectSettings/`; this demo tracks only the minimal settings files needed to keep the project reproducible.
