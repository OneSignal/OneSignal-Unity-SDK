# OneSignal No-Location Demo

Minimal Unity project for apps that use OneSignal without the native location module.

Open this folder in Unity. Location is disabled with the same project setting used by the root README:

```csharp
OneSignalSDK.OneSignalSDKSettings.DisableLocation = true;
```

The saved project setting is checked in as:

```json
{
    "disableLocation": true
}
```

Before resolving Android dependencies or building iOS, confirm **OneSignal > Disable Location Module** is checked.

## Configure

Open `Assets/Scenes/Main.unity`, select the `OneSignal No Location Demo` GameObject, and set your OneSignal App ID on the `NoLocationDemo` component.

## Android

The resolver snapshot uses OneSignal native modules without the location artifact:

```xml
<package>com.onesignal:core:5.9.3</package>
<package>com.onesignal:notifications:5.9.3</package>
<package>com.onesignal:in-app-messages:5.9.3</package>
```

If you force-resolve again, keep the location module disabled.

## App Code

`NoLocationDemo.cs` initializes OneSignal, requests push permission, and includes a test button for `OneSignal.Location.RequestPermission()`. With the native location module excluded, location calls no-op and `OneSignal.Location.IsShared` returns `false`.
