# Testing on the Demo App

First try to use a more recent Unity version for testing on the emulators.

![Unity Version](docs/unity-version.png)

## Command Line

You can build and install without opening the Unity editor by using the provided scripts. Have an emulator/simulator booted first, then run:

```sh
# Android — builds APK and installs on a running emulator
./run-android.sh

# iOS — generates Xcode project, builds, and installs on a booted simulator
./run-ios.sh
```

Both scripts accept `--no-install` to build only, `--install-only` to skip rebuilding, and `run-ios.sh` also supports `--open` to open the generated Xcode workspace. Set `UNITY_PATH` if Unity is not at the default location.

## Unity Editor

### Android

Check your build profiles and make sure Android platform is selected.  
Then click build and select a location for the apk bundle.

![Android Example 1](docs/android-example-1.png)

Then have an emulator running and drag the apk to the emulator to install it. Then test what OneSignal functionality you want.
![Android Example 2](docs/android-example-2.png)

### iOS

Check your build profiles and make sure iOS platform is selected.  
Then configure your play settings and set `Target SDK` to `Simulator SDK`
![iOS Example 1](docs/ios-example-1.png)

Then click `Build and Run` and select a location for the apk bundle. Or click `Build` to use own simulator.

![iOS Example 1](docs/ios-example-2.png)
