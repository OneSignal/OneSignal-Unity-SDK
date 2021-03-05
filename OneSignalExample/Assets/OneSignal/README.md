## OneSignal Push Notifications
[OneSignal](https://www.onesignal.com) is a free push notification service for Android and iOS apps. This SDK makes it easy to integrate your Unity app with OneSignal.

### Supported Platforms
#### Devices
* iOS 9 - 14
* Android 4.0.3 (API Level 15) - Android 11 (API Level 30)
* Amazon FireOS 3 - 6

#### Unity Versions
* Unity 2018.4, 2019.4, and 2020.2

### Quick setup
Add the following code to a script that starts with your game or app.
```C#
  // Replace 'YOUR_ONESIGNAL_APP_ID' with your OneSignal App ID from app.onesignal.com
  OneSignal.StartInit("YOUR_ONESIGNAL_APP_ID").EndInit();
```

### Demo Project
Try out the OneSignalExampleScene scene on a device (iOS or Android) and check out the code for it in the GameControllerExample.cs file under the Examples folder.

### Installation and Setup
See the [Setup Guide](https://documentation.onesignal.com/docs/unity-sdk-setup) for installation and setup instructions.

### API
See OneSignal's [Unity SDK API](https://documentation.onesignal.com/docs/unity-sdk) page for a list of all available methods.

### Change Log
See this repository's [release tags](https://github.com/OneSignal/OneSignal-Unity-SDK/releases) for a complete change log of every released version.

### Support
Please visit this repository's [Github issue tracker](https://github.com/OneSignal/OneSignal-Unity-SDK/issues) for feature requests and bug reports related specifically to the SDK.

For account issues and support please contact OneSignal support from the [OneSignal.com](https://onesignal.com) dashboard.
