_see a more detailed version of this guide at https://documentation.onesignal.com/docs/customize-notification-icons#how-to-add-default-icons_

# Android Notification Icons

Icons are a way to provide a more unique, branded experience for your Android and Amazon app.

You may add a default icon that appears with every notification you send, or you may add icons to just certain types of 
notifications. The below tutorial shows you how to do both.

## About Notification Icons
Android supports both Small and Large Notification Icons.

### Small Notification Icons
The small icon is displayed on the top status bar as well as the notification itself. By default OneSignal will show a bell 
icon, however we recommend you customize this so users recognize it's a notification from your app. Note that Android only 
uses the alpha channel for the icon. It will display monochrome in the status bar but an accent color can be applied to the 
left side the notification itself.

### Large Notification Icons
The large notification icon will show up to the left of the notification text on Android 4.0.3 - 6.0 devices, and shows on 
the right for Android 7.0+ devices. If you do not set a large icon, the small icon will be used instead. OneSignal will auto 
scale large notification icons for you to prevent the icon from being cropped. The recommended size of the large icon is 
256x256 pixels.

## How to Add Default Icons
We **strongly** recommend adding default icons to every Android and Amazon app.

### Step 1 - Generate Icons
#### Option A: Using Android Asset Studio _(Recommended)_
To quickly and easily generate small icons with the correct settings, we recommend using the Android Asset Studio. Use 
**ic_stat_onesignal_default** as the name.

#### Option B: Manually Create Icons
If you prefer to create your own icons, you must make your icons the following sizes and make the small ones in white with a transparent background.

| Name                                | Density (dp) | Size (px) |
|-------------------------------------|--------------|-----------|
| ic_stat_onesignal_default.png       | MDPI         | 24x24     |
| ic_stat_onesignal_default.png       | HDPI         | 36x36     |
| ic_stat_onesignal_default.png       | XHDPI        | 48x48     |
| ic_stat_onesignal_default.png       | XXHDPI       | 72x72     |
| ic_stat_onesignal_default.png       | XXXHDPI      | 96x96     |
| ic_onesignal_large_icon_default.png | XXXHDPI      | 256x256   |

> Required: Each name and pixel size must be present in the app.

You must be sure the icon filenames are correct as per the above table. If you used Android Asset Studio for your small icon then this step may 
have already been done for you.

### Step 2 - Add icons to project
If you have already run our SDK setup wizard (found in the Unity Editor via the `Window > OneSignal` menu) then you will already have the 
`OneSignalConfig.plugin` imported into your project at the path `/Assets/Plugins/Android/OneSignalConfig.plugin`. If this is not the case 
please run the step **"Copy Android plugin to Assets"** to do so. Once this is ready you can simply copy the files you made in step 1 to 
the correct paths:

| Path                                                                                                       | Density (dp) | Size (px) |
|------------------------------------------------------------------------------------------------------------|--------------|-----------|
| Assets/Plugins/Android/OneSignalConfig.plugin/res/**drawable-mdpi**/ic_stat_onesignal_default.png          | MDPI         | 24x24     |
| Assets/Plugins/Android/OneSignalConfig.plugin/res/**drawable-hdpi**/ic_stat_onesignal_default.png          | HDPI         | 36x36     |
| Assets/Plugins/Android/OneSignalConfig.plugin/res/**drawable-xhdpi**/ic_stat_onesignal_default.png         | XHDPI        | 48x48     |
| Assets/Plugins/Android/OneSignalConfig.plugin/res/**drawable-xxhdpi**/ic_stat_onesignal_default.png        | XXHDPI       | 72x72     |
| Assets/Plugins/Android/OneSignalConfig.plugin/res/**drawable-xxxhdpi**/ic_stat_onesignal_default.png       | XXXHDPI      | 96x96     |
| Assets/Plugins/Android/OneSignalConfig.plugin/res/**drawable-xxxhdpi**/ic_onesignal_large_icon_default.png | XXXHDPI      | 256x256   |