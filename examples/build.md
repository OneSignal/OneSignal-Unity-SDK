# OneSignal Unity Sample App - Build Guide

This document extends the shared build guide with Unity-specific details.

**Read the shared guide first:**
https://raw.githubusercontent.com/OneSignal/sdk-shared/refs/heads/main/demo/build.md

Replace `{{PLATFORM}}` with `Unity` everywhere in that guide. Everything below either overrides or supplements sections from the shared guide.

---

## Project Setup

Create a new Unity project at `examples/demo/` (relative to the SDK repo root).

- Unity UI Toolkit for the interface. Section UXML files DO NOT exist -- sections are assembled in C# via `Assets/Scripts/UI/Sections/SectionBuilder.cs` + `*SectionController.cs`. `HomeScreen.uxml` and `SecondaryScreen.uxml` are empty shells; controllers `_root.Clear()` and rebuild from C#.
- C# 9+ features where supported
- Editor: `6000.4.6f1` (per `ProjectSettings/ProjectVersion.txt`).
- Target Android API: 33+, iOS deployment target: 13.0+
- Use `SerializeField` for Inspector-configurable references; avoid public fields

App bar logo: Unity cannot load SVGs as Texture2D natively. Convert to PNG:
```bash
rsvg-convert -w 632 onesignal_logo.svg -o onesignal_logo.png
```
Save to `Assets/Resources/onesignal_logo.png` with meta settings: `nPOTScale: 0`, `enableMipMap: 0`, `alphaIsTransparency: 1`.

App icon generation: run `generate-icons.sh` which downloads the padded icon and produces platform icons.
- Android: creates an androidlib at `Assets/Plugins/Android/AppIcon.androidlib/`
- iOS: generates icons into `Assets/AppIcons/iOS/`, a build post-processor (`Assets/App/Editor/iOS/IconSetter.cs`) copies them into the Xcode project at build time

Android status bar:
- Project settings: `androidStartInFullscreen: 0`, `androidRenderOutsideSafeArea: 0`
- At runtime, set `Screen.fullScreen = false` and call `Window.addFlags(FLAG_DRAWS_SYSTEM_BAR_BACKGROUNDS)`, `Window.clearFlags(FLAG_TRANSLUCENT_STATUS)`, and `Window.setStatusBarColor()` via `AndroidJavaObject`

SDK reference via local path in `Packages/manifest.json`:
```json
"com.onesignal.unity.core": "file:../../../com.onesignal.unity.core",
"com.onesignal.unity.android": "file:../../../com.onesignal.unity.android",
"com.onesignal.unity.ios": "file:../../../com.onesignal.unity.ios"
```

### Dependencies (Packages/manifest.json)

```json
"com.onesignal.unity.core": "file:../../../com.onesignal.unity.core",
"com.onesignal.unity.android": "file:../../../com.onesignal.unity.android",
"com.onesignal.unity.ios": "file:../../../com.onesignal.unity.ios",
"com.unity.nuget.newtonsoft-json": "3.2.2"
```

The manifest also declares Unity modules `com.unity.modules.accessibility`, `com.unity.modules.androidjni`, `com.unity.modules.uielements`, `com.unity.modules.unitywebrequest`, plus a `scopedRegistries` entry for `npmjs` scoping `com.onesignal`.

Built-in (no manifest entry): `UnityEngine.Networking` (UnityWebRequest), `PlayerPrefs`.

---

## OneSignal Repository (SDK API Mapping)

Using directives:
```csharp
using OneSignalSDK;
using OneSignalSDK.User.Models;
using OneSignalSDK.Notifications;
using OneSignalSDK.InAppMessages;
using OneSignalSDK.Debug.Models;
```

| Operation | SDK Call |
|---|---|
| LoginUser(externalUserId) | `OneSignal.Login(externalUserId)` |
| LogoutUser() | `OneSignal.Logout()` |
| AddAlias(label, id) | `OneSignal.User.AddAlias(label, id)` |
| AddAliases(aliases) | `OneSignal.User.AddAliases(aliases)` |
| AddEmail(email) | `OneSignal.User.AddEmail(email)` |
| RemoveEmail(email) | `OneSignal.User.RemoveEmail(email)` |
| AddSms(number) | `OneSignal.User.AddSms(number)` |
| RemoveSms(number) | `OneSignal.User.RemoveSms(number)` |
| AddTag(key, value) | `OneSignal.User.AddTag(key, value)` |
| AddTags(tags) | `OneSignal.User.AddTags(tags)` |
| RemoveTag(key) | `OneSignal.User.RemoveTag(key)` |
| RemoveTags(keys) | `OneSignal.User.RemoveTags(keys)` |
| GetTags() | `OneSignal.User.GetTags()` |
| AddTrigger(key, value) | `OneSignal.InAppMessages.AddTrigger(key, value)` |
| AddTriggers(triggers) | `OneSignal.InAppMessages.AddTriggers(triggers)` |
| RemoveTrigger(key) | `OneSignal.InAppMessages.RemoveTrigger(key)` |
| RemoveTriggers(keys) | `OneSignal.InAppMessages.RemoveTriggers(keys)` |
| ClearTriggers() | `OneSignal.InAppMessages.ClearTriggers()` |
| SendOutcome(name) | `OneSignal.Session.AddOutcome(name)` |
| SendUniqueOutcome(name) | `OneSignal.Session.AddUniqueOutcome(name)` |
| SendOutcomeWithValue(name, value) | `OneSignal.Session.AddOutcomeWithValue(name, value)` |
| GetPushSubscriptionId() | `OneSignal.User.PushSubscription.Id` |
| IsPushOptedIn() | `OneSignal.User.PushSubscription.OptedIn` |
| OptInPush() | `OneSignal.User.PushSubscription.OptIn()` |
| OptOutPush() | `OneSignal.User.PushSubscription.OptOut()` |
| ClearAllNotifications() | `OneSignal.Notifications.ClearAllNotifications()` |
| HasPermission() | `OneSignal.Notifications.Permission` |
| RequestPermissionAsync(fallback) | `OneSignal.Notifications.RequestPermissionAsync(fallback)` |
| SetInAppMessagesPaused(paused) | `OneSignal.InAppMessages.Paused = paused` |
| IsInAppMessagesPaused() | `OneSignal.InAppMessages.Paused` |
| SetLocationShared(shared) | `OneSignal.Location.IsShared = shared` |
| IsLocationShared() | `OneSignal.Location.IsShared` |
| RequestLocationPermission() | `OneSignal.Location.RequestPermission()` |
| SetConsentRequired(required) | `OneSignal.ConsentRequired = required` |
| SetConsentGiven(granted) | `OneSignal.ConsentGiven = granted` |
| GetExternalId() | `OneSignal.User.ExternalId` |
| GetOnesignalId() | `OneSignal.User.OneSignalId` |

REST API client uses `UnityWebRequest`. Wrap `SendWebRequest()` in a `TaskCompletionSource` or use `completed` callback for async/await. JSON parsing via `Newtonsoft.Json`.

---

## SDK Initialization & Observers

In `AppBootstrapper.cs` (on a `DontDestroyOnLoad` GameObject), in `Start()`:

```csharp
OneSignal.Debug.LogLevel = LogLevel.Verbose;
OneSignal.ConsentRequired = cachedConsentRequired;
OneSignal.ConsentGiven = cachedPrivacyConsent;
OneSignal.Initialize(appId);
```

Event handlers (C# events, not callbacks):
```csharp
OneSignal.InAppMessages.WillDisplay += handler;
OneSignal.InAppMessages.DidDisplay += handler;
OneSignal.InAppMessages.WillDismiss += handler;
OneSignal.InAppMessages.DidDismiss += handler;
OneSignal.InAppMessages.Clicked += handler;
OneSignal.Notifications.Clicked += handler;
OneSignal.Notifications.ForegroundWillDisplay += handler;
```

After initialization, restore cached state:
```csharp
OneSignal.InAppMessages.Paused = cachedPausedStatus;
OneSignal.Location.IsShared = cachedLocationShared;
```

ViewModel observers (unsubscribe in `OnDestroy()`):
```csharp
OneSignal.User.PushSubscription.Changed += handler;
OneSignal.Notifications.PermissionChanged += handler;
OneSignal.User.Changed += handler;
```

---

## State Management (MonoBehaviour + Events)

- `AppBootstrapper.cs` -- `Awake()` calls `DontDestroyOnLoad(gameObject)`; `Start()` initializes SDK, creates and injects dependencies, fetches tooltips in background. `AppViewModel` is a child `MonoBehaviour` on the same GameObject and follows that lifetime by attachment.
- `AppViewModel` extends `MonoBehaviour`: holds all UI state as private fields with public properties; single `public event Action OnStateChanged` (`Assets/Scripts/ViewModels/AppViewModel.cs` line ~88). Initialized via `Init(PreferencesService prefs, OneSignalApiService apiService)` (line ~90).
- `HomeScreenController` extends `MonoBehaviour`: subscribes to `OnStateChanged`, owns root `VisualElement` via `UIDocument`, delegates user actions to ViewModel.
- `OneSignalApiService` (`Assets/Scripts/Services/OneSignalApiService.cs`) is the REST client. Handles push send, custom push send, user fetch, and Live Activity update/end using `ONESIGNAL_API_KEY`.

### Persistence

- `PreferencesService` (`Assets/Scripts/Services/PreferencesService.cs`) wraps `PlayerPrefs`. Public API is properties: `ConsentRequired`, `PrivacyConsent`, `ExternalUserId`, `LocationShared`, `IamPaused`. The underlying `GetBool` / `SetBool` / `GetString` / `SetString` helpers are private.
- Triggers list (`triggersList`) is NOT persisted.
- In-memory lists use `List<KeyValuePair<string, string>>`.

### SDK State Restoration

In `AppBootstrapper.Start()`, restore from `PlayerPrefs` BEFORE `Initialize`:
```csharp
OneSignal.ConsentRequired = _prefs.ConsentRequired;
OneSignal.ConsentGiven = _prefs.PrivacyConsent;
OneSignal.Initialize(appId);
```

Then AFTER initialize:
```csharp
OneSignal.InAppMessages.Paused = _prefs.IamPaused;
OneSignal.Location.IsShared = _prefs.LocationShared;
```

In `AppViewModel.LoadInitialState()` (`AppViewModel.cs` lines ~110-122), UI state is read primarily from cached preferences, with the SDK queried only for live push/identity fields:
- `_prefs.IamPaused`, `_prefs.LocationShared`, `_prefs.ExternalUserId`, `_prefs.ConsentRequired`, `_prefs.PrivacyConsent` for cached state
- `OneSignal.User.PushSubscription.Id`, `OneSignal.User.PushSubscription.OptedIn`, `OneSignal.Notifications.Permission`, `OneSignal.User.OneSignalId` for live SDK state

### AppBootstrapper extras

- E2E dev-console suppression: when `DotEnv.IsE2EMode` is true, `Debug.developerConsoleEnabled` and `Debug.developerConsoleVisible` are disabled so the iOS development-console overlay never occludes test taps.
- Android WebView debugging is toggled around IAM display: enabled in `OnIamWillDisplay`, disabled in `OnIamDidDismiss` (E2E mode only, via `WebView.setWebContentsDebuggingEnabled` through `AndroidJavaClass`).

---

## Unity-Specific UI Details

### Notification Permission
- `AppBootstrapper.Start()` calls `_viewModel.PromptPush()` right after `Init`/`LoadInitialState` (`Assets/Scripts/AppBootstrapper.cs` line ~86). `HomeScreenController` does NOT call it.
- `PushSectionController` also exposes a PROMPT PUSH button that re-invokes `_viewModel.PromptPush()`.

### Loading Indicator
- No full-screen overlay. Sections that fetch data render an inline `"Loading..."` label via `SectionBuilder.CreateLoadingState(sectionKey)`.
- `SectionBuilder.RenderPairList(..., loading: _viewModel.IsLoading)` swaps the list body for the loading label while `AppViewModel.IsLoading` is true (used by alias, email, SMS, tag sections).

### Toast Messages
- `DemoToast` static class (`Assets/Scripts/UI/DemoToast.cs`) owns the lone toast `VisualElement` overlay.
- Initialize-once pattern: `DemoToast.Initialize(VisualElement root)` is called exactly once on `screenRoot` from `HomeScreenController.BuildScreen()`. The static class caches that root for subsequent `Show` calls.
- `DemoToast.Show(string message)` takes ONLY a message -- no root argument. Section controllers call it directly, e.g. `DemoToast.Show($"Outcome sent: {name}")` (`OutcomesSectionController.cs`, `CustomEventsSectionController.cs`, `LocationSectionController.cs`).
- Replace-on-show: `Show(...)` calls `_hideSchedule?.Pause()` and `_container?.RemoveFromHierarchy()` before creating a new container, then schedules `Hide()` after `DurationMs`.
- Duration is the static constant `public const int DurationMs = 3000;` (milliseconds).
- `AppViewModel` does not expose toast events or hold toast state. `HomeScreenController` does not subscribe to a toast event -- sections call `DemoToast` directly.

### Secondary Screen
- Load a second scene or show a second panel
- Back navigation returns to the main screen

### Dialogs
- `HomeScreenController` owns layout + tooltip dialogs only (`ShowTooltip(key)`). Each section's `OnInfoTap` event is wired to the matching tooltip key.
- Section constructor signatures vary by responsibility:
    - Six sections take `AppViewModel` only: `AppSectionController`, `PushSectionController`, `InAppSectionController`, `SendIamSectionController`, `LocationSectionController`, `LiveActivitiesSectionController`.
    - The remaining (dialog-presenting) sections take `(AppViewModel viewModel, VisualElement dialogRoot)`: `UserSectionController`, `SendPushSectionController`, `AliasesSectionController`, `EmailsSectionController`, `SmsSectionController`, `TagsSectionController`, `OutcomesSectionController`, `TriggersSectionController`, `CustomEventsSectionController`.
- Dialog-presenting sections call shared dialog classes from `Assets/Scripts/UI/Dialogs/` inside private `Show*Dialog` methods bound to section buttons. They do not expose `OnAddTap` / `OnLoginTap` / similar action callbacks back to `HomeScreenController`.
- `AppViewModel` does not hold any action-dialog visibility flags or dialog input drafts.
- All dialogs add `VisualElement`s to the dialog root overlay (not separate scenes). Shared `DialogBase` class handles the modal backdrop and close-on-backdrop-tap. `MultiSelectRemoveDialog` uses `Toggle` elements for checkboxes. The outcome dialog uses `RadioButton` elements (`_normalRadio`, `_uniqueRadio`, `_withValueRadio`) for its three modes. Track Event JSON parsing uses `OneSignalSDK.Json.Deserialize(propsText) as Dictionary<string, object>` (`TrackEventDialog.cs` lines ~102-116) -- the comment in code explicitly avoids Newtonsoft so the native bridges receive plain `Dictionary<string, object>` / `List<object>` rather than `JObject`/`JArray`.
- Shared dialog classes live in `Assets/Scripts/UI/Dialogs/` (`DialogBase`, `SingleInputDialog`, `PairInputDialog`, `MultiPairInputDialog`, `MultiSelectRemoveDialog`, `LoginDialog`, `OutcomeDialog`, `TrackEventDialog`, `CustomNotificationDialog`, `TooltipDialog`).

### SectionBuilder
- `Assets/Scripts/UI/Sections/SectionBuilder.cs` is the central UI-construction helper. There is no per-section UXML; section controllers compose layouts in C#.
- Factory methods include `CreateSection`, `CreateCard`, `CreateToggleRow`, `CreatePrimaryButton`, `CreateDestructiveButton`, `CreateDivider`, `CreateKeyValueItem`, `CreateInlineKeyValue`, `CreateSingleItem`, `CreateEmptyState`, `CreateLoadingState`, and `RenderPairList`.
- Each section controller exposes a `BuildSection()` / `BuildContent()` method that calls these factories and adds the result to its `Root` `VisualElement`. `HomeScreenController.BuildSections()` instantiates the controllers and adds each `.Root` to the scroll content.

### Environment / secrets
- `Assets/StreamingAssets/.env` is loaded at startup via `DotEnv.Load()` (`Assets/Scripts/Services/DotEnv.cs`). In the Editor, `DotEnv` reads `<project>/.env` directly; on Android it reads via `AssetManager.open(".env")`.
- Recognized keys: `ONESIGNAL_APP_ID`, `ONESIGNAL_API_KEY`, `ONESIGNAL_ANDROID_CHANNEL_ID`, `E2E_MODE`.
- `AppBootstrapper.Start()` calls `DotEnv.Load()` BEFORE `OneSignal.Initialize`. App ID falls back to the hard-coded default when the env value is empty or the `your-onesignal-app-id` placeholder.

### Accessibility / Appium bridge
- `Assets/Scripts/Services/AccessibilityBridge.cs` mirrors the UI Toolkit `VisualElement` tree into Unity's `AccessibilityHierarchy` so Appium (XCUITest on iOS, UiAutomator2 on Android) can find elements by `VisualElement.name`.
- `EnableForE2E(_root)` is invoked from `HomeScreenController.OnEnable` and `SecondaryScreenController.OnEnable` and is a no-op outside E2E mode.
- E2E mode (`E2E_MODE=true`) disables the iOS dev console, registers per-element tap targets so Android UiAutomator2 can drive C# `Action`s by element name, and adds an iOS panel-root `TrickleDown` handler for info-icon labels whose AtTarget dispatch is dropped after a mobile:scroll.

### Live Activities (iOS only)
- `LiveActivitiesSectionController` is added to `HomeScreenController` under `#if UNITY_IOS`.
- `AppBootstrapper.Start()` calls `OneSignal.LiveActivities.SetupDefault(new LiveActivitySetupOptions { EnablePushToStart = true, EnablePushToUpdate = true })` inside an `#if UNITY_IOS` block.
- `AppViewModel.StartLiveActivity` uses `OneSignal.LiveActivities.StartDefault`. `UpdateLiveActivity` / `EndLiveActivity` call `OneSignalApiService.UpdateLiveActivity(activityId, "update"|"end", eventUpdates)`, which posts to the OneSignal REST API using `ONESIGNAL_API_KEY`.
- The section shows a hint label ("Set ONESIGNAL_API_KEY in .env to enable update & end") and disables the update/end buttons when `AppViewModel.HasApiKey` is false.

### Editor build tooling
- `Assets/Scripts/Editor/BuildScript.cs` -- CI entry points for headless Unity builds.
- `Assets/Scripts/Editor/SceneSetup.cs` -- scene bootstrap helper.
- `Assets/Scripts/Editor/CopyEnvPreBuild.cs` -- `IPreprocessBuildWithReport` that copies `<project>/.env` into `Assets/StreamingAssets/.env` before each build (and deletes the stale copy if no source `.env` exists).

---

## Theme

Create `Assets/Resources/Theme.uss` with USS variables at `:root` mapped from the shared style reference. The stylesheet lives under `Resources/` because `HomeScreenController` and `SecondaryScreenController` load it via `Resources.Load<StyleSheet>("Theme")` and add it to `_root.styleSheets`.

Unity-specific considerations:
- USS does not support CSS `gap`; use `margin` on child elements instead
- Override `.unity-base-text-field__input` on `.input-field` to remove default background/border
- Hide scrollbars by collapsing `Scroller` elements to 0 width/height (touch scrolling still works)

PanelSettings (`Assets/UI/PanelSettings.asset`):
- ScaleMode: `ScaleWithScreenSize`
- Reference Resolution: `412 x 892` (standard Android dp)
- ScreenMatchMode: `MatchWidthOrHeight`, Match: `0` (width only)
- 1 USS pixel ≈ 1 dp on device

### Safe Area

- `HomeScreenController.ApplySafeArea()` and `SecondaryScreenController.ApplySafeArea()` read `Screen.safeArea` every frame in `Update()`.
- Only the top inset is applied: sets the height of the `status_bar_spacer` element above the app bar. No bottom padding is added.
- Guards against NaN/zero values from `resolvedStyle.height` before computing scale.
- On Android, `androidRenderOutsideSafeArea` is disabled so the spacer calculates to 0.
- On iOS, the spacer handles the notch/Dynamic Island inset.

---

## UserData Model

```csharp
[Serializable]
public class UserData
{
    public Dictionary<string, string> Aliases { get; }
    public Dictionary<string, string> Tags { get; }
    public List<string> Emails { get; }
    public List<string> SmsNumbers { get; }
    public string ExternalId { get; }

    public static UserData FromJson(string json) { ... }
}
```

---

## Key Files Structure

```
examples/demo/
├── Assets/
│   ├── Scenes/
│   │   ├── Main.unity
│   │   └── Secondary.unity
│   ├── Scripts/
│   │   ├── AppBootstrapper.cs
│   │   ├── Models/
│   │   │   ├── UserData.cs
│   │   │   ├── NotificationType.cs
│   │   │   └── InAppMessageType.cs
│   │   ├── Services/
│   │   │   ├── OneSignalApiService.cs
│   │   │   ├── PreferencesService.cs
│   │   │   ├── DotEnv.cs
│   │   │   ├── AccessibilityBridge.cs
│   │   │   └── TooltipHelper.cs
│   │   ├── ViewModels/
│   │   │   └── AppViewModel.cs
│   │   ├── Editor/
│   │   │   ├── BuildScript.cs
│   │   │   ├── SceneSetup.cs
│   │   │   └── CopyEnvPreBuild.cs
│   │   └── UI/
│   │       ├── HomeScreenController.cs
│   │       ├── SecondaryScreenController.cs
│   │       ├── DemoToast.cs
│   │       ├── SwitchToggle.cs
│   │       ├── MaterialIcons.cs
│   │       ├── Dialogs/
│   │       │   ├── DialogBase.cs
│   │       │   ├── SingleInputDialog.cs
│   │       │   ├── PairInputDialog.cs
│   │       │   ├── MultiPairInputDialog.cs
│   │       │   ├── MultiSelectRemoveDialog.cs
│   │       │   ├── LoginDialog.cs
│   │       │   ├── OutcomeDialog.cs
│   │       │   ├── TrackEventDialog.cs
│   │       │   ├── CustomNotificationDialog.cs
│   │       │   └── TooltipDialog.cs
│   │       └── Sections/
│   │           ├── SectionBuilder.cs
│   │           ├── AppSectionController.cs
│   │           ├── UserSectionController.cs
│   │           ├── PushSectionController.cs
│   │           ├── SendPushSectionController.cs
│   │           ├── InAppSectionController.cs
│   │           ├── SendIamSectionController.cs
│   │           ├── AliasesSectionController.cs
│   │           ├── EmailsSectionController.cs
│   │           ├── SmsSectionController.cs
│   │           ├── TagsSectionController.cs
│   │           ├── OutcomesSectionController.cs
│   │           ├── TriggersSectionController.cs
│   │           ├── CustomEventsSectionController.cs
│   │           ├── LocationSectionController.cs
│   │           └── LiveActivitiesSectionController.cs
│   ├── UI/
│   │   ├── PanelSettings.asset
│   │   ├── Screens/
│   │   │   ├── HomeScreen.uxml
│   │   │   └── SecondaryScreen.uxml
│   │   └── Components/
│   │       └── LogView.uss
│   ├── Resources/
│   │   ├── Theme.uss
│   │   └── onesignal_logo.png
│   └── StreamingAssets/
│       └── .env
├── Packages/
│   └── manifest.json
└── ProjectSettings/
    ├── ProjectSettings.asset
    └── ProjectVersion.txt
```

Note: section UI is built in C# via `SectionBuilder.cs`. There are no per-section UXML files and no per-dialog UXML files. The two UXML shells under `UI/Screens/` are empty containers cleared and rebuilt by their screen controllers.

---

## Unity Best Practices

- **DontDestroyOnLoad** on `AppBootstrapper` so the bootstrapper GameObject (and the `AppViewModel` attached to it) survives scene loads
- **Single `OnStateChanged` event** on `AppViewModel` for decoupled state-to-UI communication
- **SerializeField** over public fields for Inspector-exposed references
- **Singleton pattern** for `TooltipHelper` that survives scene transitions
- **async/await** with Task-based APIs where the SDK supports it
- **UI Toolkit** with C#-built sections (no per-section UXML) for resolution-independent UI
- **PanelSettings** with ScaleWithScreenSize for dp-accurate scaling on mobile
- **Safe area** insets via `Screen.safeArea` for notches and system bars
- **Unsubscribe in OnDestroy** from all events to prevent memory leaks
- **Assembly definitions** (.asmdef) for faster compilation
- **Coroutine-to-Task bridge** for UnityWebRequest async/await
- **VisualElement.name** on interactive elements for Appium automation (mirrored to platform a11y via `AccessibilityBridge`)
- **`OneSignalSDK.Json.Deserialize`** for any payload handed to the native bridges; **Newtonsoft.Json** for REST payloads built in `OneSignalApiService`
