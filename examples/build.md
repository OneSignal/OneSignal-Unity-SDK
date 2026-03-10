# OneSignal Unity Sample App - Build Guide

This document extends the shared build guide with Unity-specific details.

**Read the shared guide first:**
https://raw.githubusercontent.com/OneSignal/sdk-shared/refs/heads/main/demo/build.md

Replace `{{PLATFORM}}` with `Unity` everywhere in that guide. Everything below either overrides or supplements sections from the shared guide.

---

## Project Setup

Create a new Unity project at `examples/demo/` (relative to the SDK repo root).

- Unity UI Toolkit (UXML + USS) for the interface
- C# 9+ features where supported
- Minimum Unity version: 2021.3 LTS
- Target Android API: 33+, iOS deployment target: 13.0+
- Use `SerializeField` for Inspector-configurable references; avoid public fields
- Separate UXML/USS per section

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
"com.unity.nuget.newtonsoft-json": "3.2.1"
```

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
| ClearAllNotifications() | `OneSignal.Notifications.ClearAll()` |
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

- `AppBootstrapper.cs` on a `DontDestroyOnLoad` GameObject: initializes SDK, creates and injects dependencies, fetches tooltips in background
- `AppViewModel` extends `MonoBehaviour`: holds all UI state as private fields with public properties, fires C# events (`Action`, `EventHandler`) on state changes, receives `OneSignalRepository` and `PreferencesService` via `Init()`
- `HomeScreenController` extends `MonoBehaviour`: subscribes to ViewModel events, owns root `VisualElement` via `UIDocument`, delegates user actions to ViewModel

### Persistence

- `PreferencesService` wraps `PlayerPrefs` with typed getters/setters (`GetBool`, `SetBool`, `GetString`, `SetString`)
- Triggers list (`triggersList`) is NOT persisted
- In-memory lists use `List<KeyValuePair<string, string>>`

### SDK State Restoration

In `AppBootstrapper.Start()`, restore from `PlayerPrefs` BEFORE `Initialize`:
```csharp
OneSignal.ConsentRequired = cachedConsentRequired;
OneSignal.ConsentGiven = cachedPrivacyConsent;
OneSignal.Initialize(appId);
```

Then AFTER initialize:
```csharp
OneSignal.InAppMessages.Paused = cachedPausedStatus;
OneSignal.Location.IsShared = cachedLocationShared;
```

In `AppViewModel.LoadInitialState()`, read UI state from SDK (not cache):
- `OneSignal.InAppMessages.Paused` for IAM paused state
- `OneSignal.Location.IsShared` for location state
- `OneSignal.User.ExternalId` for external user ID

---

## Unity-Specific UI Details

### Notification Permission
- Call `viewModel.PromptPush()` in `HomeScreenController.OnEnable` or `Start`

### Loading Overlay
- `VisualElement` with `position: absolute` + stretch, centered spinner
- Show/hide via `DisplayStyle.Flex` / `DisplayStyle.None`
- Animate spinner: `VisualElement.schedule.Execute().Every(16)` to rotate ~12 degrees per tick
- Use `await Task.Yield()` or `await Task.Delay(100)` after setting state for render delay

### Toast Messages
- `AppViewModel` exposes `Action<string> OnToastMessage` event
- `HomeScreenController` subscribes and shows a `ToastView` overlay Label at the bottom, auto-hides after ~2 seconds

### Send In-App Message Icons
- TOP BANNER: up arrow icon
- BOTTOM BANNER: down arrow icon
- CENTER MODAL: square icon
- FULL SCREEN: expand icon

### Secondary Screen
- Load a second scene or show a second panel
- Back navigation returns to the main screen

### Dialogs
- `VisualElements` added to the root overlay, not separate scenes
- Shared `DialogBase` class handles modal backdrop, close on backdrop tap
- `MultiSelectRemoveDialog` uses `Toggle` elements for checkboxes
- Outcome dialog uses `Toggle` group for radio options
- Track Event JSON parsing via `JsonConvert.DeserializeObject` into `Dictionary<string, object>`

### Accessibility (Appium)
- Use `VisualElement.name` for all interactive elements

### Log Manager
- Singleton with `Action<LogEntry> OnLogAdded` event for reactive UI
- `.Debug(tag, message)`, `.Info()`, `.Warn()`, `.Error()` with `UnityEngine.Debug.Log` forwarding

---

## Theme

Create `Assets/UI/Theme/Theme.uss` with USS variables at `:root` mapped from the shared style reference.

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

- `HomeScreenController` reads `Screen.safeArea` every frame in `Update()`
- Top inset: sets height of a `status_bar_spacer` element above the app bar
- Bottom inset: applies as `paddingBottom` on the screen root container
- Guards against NaN/zero values from `resolvedStyle.height` before computing scale
- On Android, `androidRenderOutsideSafeArea` is disabled so the spacer calculates to 0
- On iOS, the spacer handles the notch/Dynamic Island inset

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
│   │   │   ├── TooltipHelper.cs
│   │   │   └── LogManager.cs
│   │   ├── Repositories/
│   │   │   └── OneSignalRepository.cs
│   │   ├── ViewModels/
│   │   │   └── AppViewModel.cs
│   │   └── UI/
│   │       ├── HomeScreenController.cs
│   │       ├── SecondaryScreenController.cs
│   │       ├── LogViewController.cs
│   │       ├── ToastView.cs
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
│   │           ├── TrackEventSectionController.cs
│   │           └── LocationSectionController.cs
│   ├── UI/
│   │   ├── Theme/
│   │   │   └── Theme.uss
│   │   ├── Screens/
│   │   │   ├── HomeScreen.uxml
│   │   │   └── SecondaryScreen.uxml
│   │   ├── Components/
│   │   │   ├── SectionCard.uxml
│   │   │   ├── ToggleRow.uxml
│   │   │   ├── LogView.uxml / LogView.uss
│   │   │   ├── LoadingOverlay.uxml
│   │   │   └── ToastView.uxml
│   │   ├── Dialogs/
│   │   │   ├── DialogBase.uxml
│   │   │   ├── SingleInputDialog.uxml
│   │   │   ├── PairInputDialog.uxml
│   │   │   ├── MultiPairInputDialog.uxml
│   │   │   ├── MultiSelectRemoveDialog.uxml
│   │   │   ├── LoginDialog.uxml
│   │   │   ├── OutcomeDialog.uxml
│   │   │   ├── TrackEventDialog.uxml
│   │   │   ├── CustomNotificationDialog.uxml
│   │   │   └── TooltipDialog.uxml
│   │   └── Sections/
│   │       ├── AppSection.uxml
│   │       ├── PushSection.uxml
│   │       ├── SendPushSection.uxml
│   │       ├── InAppSection.uxml
│   │       ├── SendIamSection.uxml
│   │       ├── AliasesSection.uxml
│   │       ├── EmailsSection.uxml
│   │       ├── SmsSection.uxml
│   │       ├── TagsSection.uxml
│   │       ├── OutcomesSection.uxml
│   │       ├── TriggersSection.uxml
│   │       ├── TrackEventSection.uxml
│   │       └── LocationSection.uxml
│   └── Resources/
│       └── onesignal_logo.png
├── Packages/
│   └── manifest.json
└── ProjectSettings/
    └── ProjectSettings.asset
```

---

## Unity Best Practices

- **DontDestroyOnLoad** for persistent managers (AppBootstrapper, AppViewModel) across scene loads
- **C# events** (`Action`, `EventHandler`) for decoupled state-to-UI communication
- **SerializeField** over public fields for Inspector-exposed references
- **Singleton pattern** for services (LogManager, TooltipHelper) that survive scene transitions
- **async/await** with Task-based APIs where the SDK supports it
- **UI Toolkit** (UXML + USS) for resolution-independent, stylesheet-driven UI
- **PanelSettings** with ScaleWithScreenSize for dp-accurate scaling on mobile
- **Safe area** insets via `Screen.safeArea` for notches and system bars
- **Unsubscribe in OnDestroy** from all events to prevent memory leaks
- **Assembly definitions** (.asmdef) for faster compilation
- **Coroutine-to-Task bridge** for UnityWebRequest async/await
- **VisualElement.name** on interactive elements for Appium automation
- **Newtonsoft.Json** for JSON parsing
