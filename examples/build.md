# OneSignal Unity Sample App - Build Guide

This document contains all the prompts and requirements needed to build the OneSignal Unity Sample App from scratch. Give these prompts to an AI assistant or follow them manually to recreate the app.

---

## Phase 0: Reference Screenshots (REQUIRED)

### Prompt 0.1 - Capture Reference UI

```
Before building anything, an Android emulator MUST be running with the
reference OneSignal demo app installed. These screenshots are the source
of truth for the UI you are building. Do NOT proceed to Phase 1 without them.

Check for connected emulators:
  adb devices

If no device is listed, stop and ask the user to start one.

Identify which emulator has com.onesignal.sdktest installed by checking each listed device, e.g.:
  adb -s emulator-5554 shell pm list packages 2>/dev/null | grep -i onesignal
  adb -s emulator-5556 shell pm list packages 2>/dev/null | grep -i onesignal

Use that emulator's serial (e.g. emulator-5556) for all subsequent adb commands via the -s flag.

Launch the reference app:
  adb -s <emulator-serial> shell am start -n com.onesignal.sdktest/.ui.main.MainActivity

Dismiss any in-app messages that appear on launch. Tap the X or
click-through button on each IAM until the main UI is fully visible
with no overlays.

Create an output directory:
  mkdir -p /tmp/onesignal_reference

Capture screenshots by scrolling through the full UI:
1. Take a screenshot from the top of the screen:
     adb shell screencap -p /sdcard/ref_01.png && adb pull /sdcard/ref_01.png /tmp/onesignal_reference/ref_01.png
2. Scroll down by roughly one viewport height:
     adb shell input swipe 500 1500 500 500
3. Take the next screenshot (ref_02.png, ref_03.png, etc.)
4. Repeat until you've reached the bottom of the scrollable content

You MUST read each captured screenshot image so you can see the actual UI.
These images define the visual target for every section you build later.
Pay close attention to:
  - Section header style and casing
  - Card vs non-card content grouping
  - Button placement (inside vs outside cards)
  - List item layout (stacked vs inline key-value)
  - Icon choices (delete, close, info, etc.)
  - Typography, spacing, and colors
  - Spacing: 12px gap between sections, 8px gap between cards/buttons within a section

You can also interact with the reference app to observe specific flows:

Dump the UI hierarchy to find elements by resource-id, text, or content-desc:
  adb shell uiautomator dump /sdcard/ui.xml && adb pull /sdcard/ui.xml /tmp/onesignal_reference/ui.xml

Parse the XML to find an element's bounds, then tap it:
  adb shell input tap <centerX> <centerY>

Type into a focused text field:
  adb shell input text "test"

Example flow to observe "Add Tag" behavior:
  1. Dump UI -> find the ADD button bounds -> tap it
  2. Dump UI -> find the Key and Value fields -> tap and type into them
  3. Tap the confirm button -> screenshot the result
  4. Compare the tag list state before and after

Also capture screenshots of key dialogs to match their layout:
  - Add Alias (single pair input)
  - Add Multiple Aliases/Tags (dynamic rows with add/remove)
  - Remove Selected Tags (checkbox multi-select)
  - Login User
  - Send Outcome (radio options)
  - Track Event (with JSON properties field)
  - Custom Notification (title + body)
These dialog screenshots are important for matching field layout,
button placement, spacing, and validation behavior.

Refer back to these screenshots throughout all remaining phases whenever
you need to decide on layout, spacing, section order, dialog flows, or
overall look and feel.
```

---

## Phase 1: Initial Setup

### Prompt 1.1 - Project Foundation

```
Create a new Unity project at examples/demo/ (relative to the SDK repo root).

Build the app with:
- Clean architecture: repository pattern with a central ViewModel MonoBehaviour for state
- C# 9+ features where supported by the target Unity version (pattern matching, records where useful)
- Unity UI Toolkit (UI Documents + USS) for the interface
- App name: "OneSignal Demo"
- Top bar: centered title with OneSignal logo SVG/PNG + "Sample App" text
- Support for both Android and iOS
- Android package name: com.onesignal.example
- iOS bundle identifier: com.onesignal.example
- All dialogs should have EMPTY input fields (for Appium testing - test framework enters values)
- Separate UXML/USS per section to keep files focused and readable
- Use SerializeField for Inspector-configurable references; avoid public fields

Download the app bar logo from:
  https://raw.githubusercontent.com/OneSignal/sdk-shared/refs/heads/main/assets/onesignal_logo.svg
Save it to the demo project at Assets/Resources/onesignal_logo (import as Sprite or
use Unity's Vector Graphics package for SVG rendering in UI Toolkit).

Download the padded app icon PNG from:
  https://raw.githubusercontent.com/OneSignal/sdk-shared/refs/heads/main/assets/onesignal_logo_icon_padded.png
Save it to Assets/Resources/ temporarily, set as the default icon in
Project Settings > Player for both Android and iOS, then remove the
source file from version control.

Reference the OneSignal Unity SDK from the parent repo using a local path dependency
in Packages/manifest.json:
  "com.onesignal.unity.core": "file:../../../com.onesignal.unity.core",
  "com.onesignal.unity.android": "file:../../../com.onesignal.unity.android",
  "com.onesignal.unity.ios": "file:../../../com.onesignal.unity.ios"
```

### Prompt 1.2 - Dependencies (Packages/manifest.json)

```
Add these dependencies to Packages/manifest.json:

Required packages:
  "com.onesignal.unity.core": "file:../../../com.onesignal.unity.core"
  "com.onesignal.unity.android": "file:../../../com.onesignal.unity.android"
  "com.onesignal.unity.ios": "file:../../../com.onesignal.unity.ios"
  "com.unity.nuget.newtonsoft-json": "3.2.1"   # JSON serialization

Unity built-in (no manifest entry needed):
  UnityEngine.Networking  (UnityWebRequest for REST API calls)
  PlayerPrefs             (Local persistence)

Minimum Unity version: 2021.3 LTS
Target Android API: 33+
iOS deployment target: 13.0+
```

### Prompt 1.3 - OneSignalRepository

```
Create a OneSignalRepository class that centralizes all OneSignal SDK calls.
This is a plain C# class (not a MonoBehaviour) injected into the ViewModel.

Using directives:
  using OneSignalSDK;
  using OneSignalSDK.User.Models;
  using OneSignalSDK.Notifications;
  using OneSignalSDK.InAppMessages;
  using OneSignalSDK.Debug.Models;

User operations:
- LoginUser(string externalUserId) -> void  (calls OneSignal.Login)
- LogoutUser() -> void                      (calls OneSignal.Logout)

Alias operations:
- AddAlias(string label, string id) -> void           (calls OneSignal.User.AddAlias)
- AddAliases(Dictionary<string, string> aliases) -> void (calls OneSignal.User.AddAliases)

Email operations:
- AddEmail(string email) -> void     (calls OneSignal.User.AddEmail)
- RemoveEmail(string email) -> void  (calls OneSignal.User.RemoveEmail)

SMS operations:
- AddSms(string smsNumber) -> void     (calls OneSignal.User.AddSms)
- RemoveSms(string smsNumber) -> void  (calls OneSignal.User.RemoveSms)

Tag operations:
- AddTag(string key, string value) -> void            (calls OneSignal.User.AddTag)
- AddTags(Dictionary<string, string> tags) -> void    (calls OneSignal.User.AddTags)
- RemoveTag(string key) -> void                       (calls OneSignal.User.RemoveTag)
- RemoveTags(List<string> keys) -> void               (calls OneSignal.User.RemoveTags)
- GetTags() -> Dictionary<string, string>             (calls OneSignal.User.GetTags)

Trigger operations (via OneSignal.InAppMessages):
- AddTrigger(string key, string value) -> void          (calls OneSignal.InAppMessages.AddTrigger)
- AddTriggers(Dictionary<string, string> triggers) -> void (calls OneSignal.InAppMessages.AddTriggers)
- RemoveTrigger(string key) -> void                     (calls OneSignal.InAppMessages.RemoveTrigger)
- RemoveTriggers(List<string> keys) -> void             (calls OneSignal.InAppMessages.RemoveTriggers)
- ClearTriggers() -> void                               (calls OneSignal.InAppMessages.ClearTriggers)

Outcome operations (via OneSignal.Session):
- SendOutcome(string name) -> void                  (calls OneSignal.Session.AddOutcome)
- SendUniqueOutcome(string name) -> void            (calls OneSignal.Session.AddUniqueOutcome)
- SendOutcomeWithValue(string name, float value) -> void (calls OneSignal.Session.AddOutcomeWithValue)

Push subscription:
- GetPushSubscriptionId() -> string?   (reads OneSignal.User.PushSubscription.Id)
- IsPushOptedIn() -> bool              (reads OneSignal.User.PushSubscription.OptedIn)
- OptInPush() -> void                  (calls OneSignal.User.PushSubscription.OptIn)
- OptOutPush() -> void                 (calls OneSignal.User.PushSubscription.OptOut)

Notifications:
- HasPermission() -> bool                          (reads OneSignal.Notifications.Permission)
- RequestPermissionAsync(bool fallbackToSettings) -> Task<bool>
    (calls OneSignal.Notifications.RequestPermissionAsync)

In-App Messages:
- SetInAppMessagesPaused(bool paused) -> void   (sets OneSignal.InAppMessages.Paused)
- IsInAppMessagesPaused() -> bool               (reads OneSignal.InAppMessages.Paused)

Location:
- SetLocationShared(bool shared) -> void  (sets OneSignal.Location.IsShared)
- IsLocationShared() -> bool              (reads OneSignal.Location.IsShared)
- RequestLocationPermission() -> void     (calls OneSignal.Location.RequestPermission)

Privacy consent:
- SetConsentRequired(bool required) -> void  (sets OneSignal.ConsentRequired)
- SetConsentGiven(bool granted) -> void      (sets OneSignal.ConsentGiven)

User IDs:
- GetExternalId() -> string?    (reads OneSignal.User.ExternalId)
- GetOnesignalId() -> string?   (reads OneSignal.User.OneSignalId)

Notification sending (via REST API, delegated to OneSignalApiService):
- SendNotification(NotificationType type) -> Task<bool>
- SendCustomNotification(string title, string body) -> Task<bool>
- FetchUser(string onesignalId) -> Task<UserData>
```

### Prompt 1.4 - OneSignalApiService (REST API Client)

```
Create OneSignalApiService class for REST API calls using UnityWebRequest:

Properties:
- _appId: string (set during initialization)

Methods:
- SetAppId(string appId)
- GetAppId() -> string
- SendNotification(NotificationType type, string subscriptionId) -> Task<bool>
- SendCustomNotification(string title, string body, string subscriptionId) -> Task<bool>
- FetchUser(string onesignalId) -> Task<UserData>

Use async/await with UnityWebRequest by wrapping SendWebRequest() in a
TaskCompletionSource or using UnityWebRequestAsyncOperation.completed callback.
Alternatively, use a coroutine-to-task bridge utility.

sendNotification endpoint:
- POST https://onesignal.com/api/v1/notifications
- Accept header: "application/vnd.onesignal.v1+json"
- Uses include_subscription_ids (not include_player_ids)
- Includes big_picture for Android image notifications
- Includes ios_attachments for iOS image notifications (needed for the NSE to download and attach images)

fetchUser endpoint:
- GET https://api.onesignal.com/apps/{app_id}/users/by/onesignal_id/{onesignal_id}
- NO Authorization header needed (public endpoint)
- Returns UserData with aliases, tags, emails, smsNumbers, externalId
- Parse JSON response with JsonUtility or Newtonsoft.Json
```

### Prompt 1.5 - SDK Observers

```
In a boot MonoBehaviour (e.g. AppBootstrapper.cs on a DontDestroyOnLoad GameObject),
set up OneSignal initialization and listeners in Start():

OneSignal.Debug.LogLevel = LogLevel.Verbose;
OneSignal.ConsentRequired = cachedConsentRequired;
OneSignal.ConsentGiven = cachedPrivacyConsent;
OneSignal.Initialize(appId);

Then register event handlers:
- OneSignal.InAppMessages.WillDisplay += handler;
- OneSignal.InAppMessages.DidDisplay += handler;
- OneSignal.InAppMessages.WillDismiss += handler;
- OneSignal.InAppMessages.DidDismiss += handler;
- OneSignal.InAppMessages.Clicked += handler;
- OneSignal.Notifications.Clicked += handler;
- OneSignal.Notifications.ForegroundWillDisplay += handler;

After initialization, restore cached SDK states from PlayerPrefs:
- OneSignal.InAppMessages.Paused = cachedPausedStatus;
- OneSignal.Location.IsShared = cachedLocationShared;

In AppViewModel (MonoBehaviour), register observers:
- OneSignal.User.PushSubscription.Changed += handler;   // react to push subscription changes
- OneSignal.Notifications.PermissionChanged += handler;  // react to permission changes
- OneSignal.User.Changed += handler;                     // call FetchUserDataFromApi() when user changes

Unsubscribe from all events in OnDestroy() to prevent memory leaks.
```

---

## Phase 2: UI Sections

### Section Order (top to bottom)

1. **App Section** (App ID, Guidance Banner, Consent Toggle, Logged-in-as display, Login/Logout)
2. **Push Section** (Push ID, Enabled Toggle, Auto-prompts permission on load)
3. **Send Push Notification Section** (Simple, With Image, Custom buttons)
4. **In-App Messaging Section** (Pause toggle)
5. **Send In-App Message Section** (Top Banner, Bottom Banner, Center Modal, Full Screen - with icons)
6. **Aliases Section** (Add/Add Multiple, read-only list)
7. **Emails Section** (Collapsible list >5 items)
8. **SMS Section** (Collapsible list >5 items)
9. **Tags Section** (Add/Add Multiple/Remove Selected)
10. **Outcome Events Section** (Send Outcome dialog with type selection)
11. **Triggers Section** (Add/Add Multiple/Remove Selected/Clear All - IN MEMORY ONLY)
12. **Track Event Section** (Track Event with JSON validation)
13. **Location Section** (Location Shared toggle, Prompt Location button)
14. **Next Page Button**

### Prompt 2.1 - App Section

```
App Section layout:

1. App ID display (readonly Label showing the OneSignal App ID)

2. Sticky guidance banner below App ID:
   - Text: "Add your own App ID, then rebuild to fully test all functionality."
   - Link text: "Get your keys at onesignal.com" (clickable, opens browser via Application.OpenURL)
   - Light background color to stand out

3. Consent card with up to two toggles:
   a. "Consent Required" toggle (always visible):
      - Label: "Consent Required"
      - Description: "Require consent before SDK processes data"
      - Sets OneSignal.ConsentRequired = value
   b. "Privacy Consent" toggle (only visible when Consent Required is ON):
      - Label: "Privacy Consent"
      - Description: "Consent given for data collection"
      - Sets OneSignal.ConsentGiven = value
      - Separated from the above toggle by a horizontal divider
   - NOT a blocking overlay - user can interact with app regardless of state

4. User status card (always visible, ABOVE the login/logout buttons):
   - Card with two rows separated by a divider
   - Row 1: "Status" label on the left, value on the right
   - Row 2: "External ID" label on the left, value on the right
   - When logged out:
     - Status shows "Anonymous"
     - External ID shows "–" (dash)
   - When logged in:
     - Status shows "Logged In" with green styling (hex #2E7D32)
     - External ID shows the actual external user ID

5. LOGIN USER button:
   - Shows "LOGIN USER" when no user is logged in
   - Shows "SWITCH USER" when a user is logged in
   - Opens dialog with empty "External User Id" field

6. LOGOUT USER button (only visible when a user is logged in)
```

### Prompt 2.2 - Push Section

```
Push Section:
- Section title: "Push" with info icon for tooltip
- Push Subscription ID display (readonly)
- Enabled toggle switch (controls OptIn/OptOut)
  - Disabled when notification permission is NOT granted
- Notification permission is automatically requested when home screen loads
- PROMPT PUSH button:
  - Only visible when notification permission is NOT granted (fallback if user denied)
  - Calls OneSignal.Notifications.RequestPermissionAsync(true) when clicked
  - Hidden once permission is granted
```

### Prompt 2.3 - Send Push Notification Section

```
Send Push Notification Section (placed right after Push Section):
- Section title: "Send Push Notification" with info icon for tooltip
- Three buttons:
  1. SIMPLE - title: "Simple Notification", body: "This is a simple push notification"
  2. WITH IMAGE - title: "Image Notification", body: "This notification includes an image"
     big_picture (Android): https://media.onesignal.com/automated_push_templates/ratings_template.png
     ios_attachments (iOS): {"image": "https://media.onesignal.com/automated_push_templates/ratings_template.png"}
  3. CUSTOM - opens dialog for custom title and body

Tooltip should explain each button type.
```

### Prompt 2.4 - In-App Messaging Section

```
In-App Messaging Section (placed right after Send Push):
- Section title: "In-App Messaging" with info icon for tooltip
- Pause In-App Messages toggle switch:
  - Label: "Pause In-App Messages"
  - Description: "Toggle in-app message display"
  - Sets OneSignal.InAppMessages.Paused property
```

### Prompt 2.5 - Send In-App Message Section

```
Send In-App Message Section (placed right after In-App Messaging):
- Section title: "Send In-App Message" with info icon for tooltip
- Four FULL-WIDTH buttons (not a grid):
  1. TOP BANNER - up arrow icon, trigger: "iam_type" = "top_banner"
  2. BOTTOM BANNER - down arrow icon, trigger: "iam_type" = "bottom_banner"
  3. CENTER MODAL - square icon, trigger: "iam_type" = "center_modal"
  4. FULL SCREEN - expand icon, trigger: "iam_type" = "full_screen"
- Button styling:
  - RED background color (#E9444E)
  - WHITE text
  - Type-specific icon on LEFT side only (no right side icon)
  - Full width of the card
  - Left-aligned text and icon content (not centered)
  - UPPERCASE button text
- On tap: adds trigger via OneSignal.InAppMessages.AddTrigger and shows toast "Sent In-App Message: {type}"
  - Also upserts `iam_type` in the Triggers list immediately so UI reflects the sent IAM type

Tooltip should explain each IAM type.
```

### Prompt 2.6 - Aliases Section

```
Aliases Section (placed after Send In-App Message):
- Section title: "Aliases" with info icon for tooltip
- List showing key-value pairs (read-only, no delete icons)
- Each item shows: Label | ID
- Filter out "external_id" and "onesignal_id" from display (these are special)
- "No Aliases Added" text when empty
- ADD button -> PairInputDialog with empty Label and ID fields on the same row (single add)
- ADD MULTIPLE button -> MultiPairInputDialog (dynamic rows, add/remove)
- No remove/delete functionality (aliases are add-only from the UI)
```

### Prompt 2.7 - Emails Section

```
Emails Section:
- Section title: "Emails" with info icon for tooltip
- List showing email addresses
- Each item shows email with an X icon (remove action)
- "No Emails Added" text when empty
- ADD EMAIL button -> dialog with empty email field
- Collapse behavior when >5 items:
  - Show first 5 items
  - Show "X more" text (tappable)
  - Expand to show all when tapped
```

### Prompt 2.8 - SMS Section

```
SMS Section:
- Section title: "SMS" with info icon for tooltip
- List showing phone numbers
- Each item shows phone number with an X icon (remove action)
- "No SMS Added" text when empty
- ADD SMS button -> dialog with empty SMS field
- Collapse behavior when >5 items (same as Emails)
```

### Prompt 2.9 - Tags Section

```
Tags Section:
- Section title: "Tags" with info icon for tooltip
- List showing key-value pairs
- Each item shows key above value (stacked layout) with an X icon on the right (remove action)
- "No Tags Added" text when empty
- ADD button -> PairInputDialog with empty Key and Value fields (single add)
- ADD MULTIPLE button -> MultiPairInputDialog (dynamic rows)
- REMOVE SELECTED button:
  - Only visible when at least one tag exists
  - Opens MultiSelectRemoveDialog with checkboxes (Toggle elements)
```

### Prompt 2.10 - Outcome Events Section

```
Outcome Events Section:
- Section title: "Outcome Events" with info icon for tooltip
- SEND OUTCOME button -> opens dialog with 3 radio options (Toggle group):
  1. Normal Outcome -> shows name input field
  2. Unique Outcome -> shows name input field
  3. Outcome with Value -> shows name and value (float) input fields
```

### Prompt 2.11 - Triggers Section (IN MEMORY ONLY)

```
Triggers Section:
- Section title: "Triggers" with info icon for tooltip
- List showing key-value pairs
- Each item shows key above value (stacked layout) with an X icon on the right (remove action)
- "No Triggers Added" text when empty
- ADD button -> PairInputDialog with empty Key and Value fields (single add)
- ADD MULTIPLE button -> MultiPairInputDialog (dynamic rows)
- Two action buttons (only visible when triggers exist):
  - REMOVE SELECTED -> MultiSelectRemoveDialog with checkboxes
  - CLEAR ALL -> Removes all triggers at once

IMPORTANT: Triggers are stored IN MEMORY ONLY during the app session.
- triggersList is a List<KeyValuePair<string, string>> in AppViewModel
- Sending an IAM button also updates the same list by setting `iam_type`
- Triggers are NOT persisted to PlayerPrefs
- Triggers are cleared when the app is killed/restarted
- This is intentional - triggers are transient test data for IAM testing
```

### Prompt 2.12 - Track Event Section

```
Track Event Section:
- Section title: "Track Event" with info icon for tooltip
- TRACK EVENT button -> opens TrackEventDialog with:
  - "Event Name" label + empty input field (required, shows error if empty on submit)
  - "Properties (optional, JSON)" label + input field with placeholder hint {"key": "value"}
    - If non-empty and not valid JSON, shows "Invalid JSON format" error on the field
    - If valid JSON, parsed via JsonConvert.DeserializeObject and converted to Dictionary<string, object>
    - If empty, passes null
  - TRACK button disabled until name is filled AND JSON is valid (or empty)
- Calls the SDK's track event method
```

### Prompt 2.13 - Location Section

```
Location Section:
- Section title: "Location" with info icon for tooltip
- Location Shared toggle switch:
  - Label: "Location Shared"
  - Description: "Share device location with OneSignal"
  - Sets OneSignal.Location.IsShared property
- PROMPT LOCATION button (calls OneSignal.Location.RequestPermission)
```

### Prompt 2.14 - Secondary Activity

```
Secondary Activity (launched by "Next Activity" button at bottom of main screen):
- Load a second scene or show a second panel titled "Secondary Activity"
- Page content: centered text "Secondary Activity" using headline style
- Simple screen, no additional functionality needed
- Back navigation returns to the main screen
```

---

## Phase 3: View User API Integration

### Prompt 3.1 - Data Loading Flow

```
Loading indicator overlay:
- Full-screen semi-transparent overlay with centered spinner (Unity UI Toolkit or a rotating image)
- isLoading flag in AppViewModel
- Show/hide via VisualElement display style (DisplayStyle.Flex / DisplayStyle.None)
- IMPORTANT: Add a short delay after populating data before dismissing loading indicator
  - This gives the UI Toolkit layout a frame to rebuild
  - Use await Task.Yield() or await Task.Delay(100) after setting state

On cold start:
- Check if OneSignal.User.OneSignalId is not null
- If exists: show loading -> call FetchUserDataFromApi() -> populate UI -> short delay -> hide loading
- If null: just show empty state (no loading indicator)

On login (LOGIN USER / SWITCH USER):
- Show loading indicator immediately
- Call OneSignal.Login(externalUserId)
- Clear old user data (aliases, emails, sms, triggers)
- Wait for User.Changed callback
- User.Changed calls FetchUserDataFromApi()
- FetchUserDataFromApi() populates UI, delays, then hides loading

On logout:
- Show loading indicator
- Call OneSignal.Logout()
- Clear local lists (aliases, emails, sms, triggers)
- Hide loading indicator

On User.Changed callback:
- Call FetchUserDataFromApi() to sync with server state
- Update UI with new data (aliases, tags, emails, sms)

Note: REST API key is NOT required for fetchUser endpoint.
```

### Prompt 3.2 - UserData Model

```
[Serializable]
public class UserData
{
    public Dictionary<string, string> Aliases { get; }    // From identity object (filter out external_id, onesignal_id)
    public Dictionary<string, string> Tags { get; }       // From properties.tags object
    public List<string> Emails { get; }                   // From subscriptions where type=="Email" -> token
    public List<string> SmsNumbers { get; }               // From subscriptions where type=="SMS" -> token
    public string ExternalId { get; }                     // From identity.external_id

    public UserData(
        Dictionary<string, string> aliases,
        Dictionary<string, string> tags,
        List<string> emails,
        List<string> smsNumbers,
        string externalId = null)
    {
        Aliases = aliases;
        Tags = tags;
        Emails = emails;
        SmsNumbers = smsNumbers;
        ExternalId = externalId;
    }

    public static UserData FromJson(string json) { ... }
}
```

---

## Phase 4: Info Tooltips

### Prompt 4.1 - Tooltip Content (Remote)

```
Tooltip content is fetched at runtime from the sdk-shared repo. Do NOT bundle a local copy.

URL:
https://raw.githubusercontent.com/OneSignal/sdk-shared/main/demo/tooltip_content.json

This file is maintained in the sdk-shared repo and shared across all platform demo apps.
```

### Prompt 4.2 - Tooltip Helper

```
Create TooltipHelper as a singleton:

public class TooltipHelper
{
    private static readonly TooltipHelper _instance = new TooltipHelper();
    public static TooltipHelper Instance => _instance;

    private Dictionary<string, TooltipData> _tooltips = new Dictionary<string, TooltipData>();
    private bool _initialized;

    private const string TooltipUrl =
        "https://raw.githubusercontent.com/OneSignal/sdk-shared/main/demo/tooltip_content.json";

    public async Task InitAsync()
    {
        if (_initialized) return;

        try
        {
            // Fetch tooltip_content.json from TooltipUrl using UnityWebRequest.Get
            // Parse JSON into _tooltips dictionary
            // On failure (no network, etc.), leave _tooltips empty — tooltips are non-critical
        }
        catch (System.Exception) { }

        _initialized = true;
    }

    public TooltipData GetTooltip(string key)
    {
        _tooltips.TryGetValue(key, out var data);
        return data;
    }
}

public class TooltipData
{
    public string Title { get; set; }
    public string Description { get; set; }
    public List<TooltipOption> Options { get; set; }
}

public class TooltipOption
{
    public string Name { get; set; }
    public string Description { get; set; }
}
```

### Prompt 4.3 - Tooltip UI Integration

```
For each section, pass an onInfoTap callback to SectionCard:
- SectionCard has an optional info icon Button that invokes onInfoTap when clicked
- In HomeScreen, wire onInfoTap to show a TooltipDialog
- TooltipDialog displays title, description, and options (if present)

Example in HomeScreen:

aliasesSection.OnInfoTap = () => ShowTooltipDialog("aliases");

private void ShowTooltipDialog(string key)
{
    var tooltip = TooltipHelper.Instance.GetTooltip(key);
    if (tooltip != null)
    {
        var dialog = new TooltipDialog(tooltip);
        dialog.Show(rootVisualElement);
    }
}
```

---

## Phase 5: Data Persistence & Initialization

### What IS Persisted (PlayerPrefs)

```
PreferencesService stores via PlayerPrefs:
- OneSignal App ID                  (PlayerPrefs string key: "onesignal_app_id")
- Consent required status           (PlayerPrefs int key: "consent_required", 0/1)
- Privacy consent status            (PlayerPrefs int key: "privacy_consent", 0/1)
- External user ID                  (PlayerPrefs string key: "external_user_id")
- Location shared status            (PlayerPrefs int key: "location_shared", 0/1)
- In-app messaging paused status    (PlayerPrefs int key: "iam_paused", 0/1)

Wrap PlayerPrefs access in a PreferencesService class with typed getters/setters
(GetBool, SetBool, GetString, SetString) to avoid scattered PlayerPrefs calls
and bare string keys throughout the codebase.
```

### Initialization Flow

```
On app startup, state is restored in two layers:

1. AppBootstrapper.Start() restores SDK state from PlayerPrefs cache BEFORE Initialize:
   - OneSignal.ConsentRequired = cachedConsentRequired;
   - OneSignal.ConsentGiven = cachedPrivacyConsent;
   - OneSignal.Initialize(appId);
   Then AFTER Initialize, restores remaining SDK state:
   - OneSignal.InAppMessages.Paused = cachedPausedStatus;
   - OneSignal.Location.IsShared = cachedLocationShared;
   This ensures consent settings are in place before the SDK initializes.

2. AppViewModel.LoadInitialState() reads UI state from the SDK (not PlayerPrefs):
   - consentRequired from cached prefs (no SDK getter)
   - privacyConsentGiven from cached prefs (no SDK getter)
   - inAppMessagesPaused from OneSignal.InAppMessages.Paused
   - locationShared from OneSignal.Location.IsShared
   - externalUserId from OneSignal.User.ExternalId
   - appId from PreferencesService (app-level config)

This two-layer approach ensures:
- The SDK is configured with the user's last preferences before anything else runs
- The ViewModel reads the SDK's actual state as the source of truth for the UI
- The UI always reflects what the SDK reports, not stale cache values
```

### What is NOT Persisted (In-Memory Only)

```
AppViewModel holds in memory:
- triggersList: List<KeyValuePair<string, string>>
  - Triggers are session-only
  - Cleared on app restart
  - Used for testing IAM trigger conditions

- aliasesList:
  - Populated from REST API on each session start
  - When user adds alias locally, added to list immediately (SDK syncs async)
  - Fetched fresh via FetchUserDataFromApi() on login/app start

- emailsList, smsNumbersList:
  - Populated from REST API on each session
  - Not cached locally
  - Fetched fresh via FetchUserDataFromApi()

- tagsList:
  - Can be read from SDK via GetTags()
  - Also fetched from API for consistency
```

---

## Phase 6: Testing Values (Appium Compatibility)

```
All dialog input fields should be EMPTY by default.
The test automation framework (Appium) will enter these values:

- Login Dialog: External User Id = "test"
- Add Alias Dialog: Key = "Test", Value = "Value"
- Add Multiple Aliases Dialog: Key = "Test", Value = "Value" (first row; supports multiple rows)
- Add Email Dialog: Email = "test@onesignal.com"
- Add SMS Dialog: SMS = "123-456-5678"
- Add Tag Dialog: Key = "Test", Value = "Value"
- Add Multiple Tags Dialog: Key = "Test", Value = "Value" (first row; supports multiple rows)
- Add Trigger Dialog: Key = "trigger_key", Value = "trigger_value"
- Add Multiple Triggers Dialog: Key = "trigger_key", Value = "trigger_value" (first row; supports multiple rows)
- Outcome Dialog: Name = "test_outcome", Value = "1.5"
- Track Event Dialog: Name = "test_event", Properties = "{\"key\": \"value\"}"
- Custom Notification Dialog: Title = "Test Title", Body = "Test Body"

All interactive elements should have a unique name in the UXML for Appium
accessibility automation (set via the name attribute or VisualElement.name).
```

---

## Phase 7: Important Implementation Details

### Alias Management

```
Aliases are managed with a hybrid approach:

1. On app start/login: Fetched from REST API via FetchUserDataFromApi()
2. When user adds alias locally:
   - Call OneSignal.User.AddAlias(label, id) - syncs to server async
   - Immediately add to local aliasesList (don't wait for API)
   - This ensures instant UI feedback while SDK syncs in background
3. On next app launch: Fresh data from API includes the synced alias
```

### Notification Permission

```
Notification permission is automatically requested when the home screen loads:
- Call viewModel.PromptPush() in the HomeScreen's OnEnable or Start
- This ensures prompt appears after user sees the app UI
- PROMPT PUSH button remains as fallback if user initially denied
- Button hidden once permission is granted
- Keep Push "Enabled" toggle disabled until permission is granted
```

---

## Phase 8: Unity Architecture

### Prompt 8.1 - State Management with MonoBehaviour

```
Use a central AppViewModel MonoBehaviour for state management with C# events
for reactive UI updates.

AppBootstrapper.cs (on a DontDestroyOnLoad GameObject):
- Initializes OneSignal SDK in Start()
- Creates and injects dependencies (OneSignalRepository, PreferencesService, OneSignalApiService)
- Fetches tooltips in the background (non-blocking)

AppViewModel extends MonoBehaviour:
- Holds all UI state as private fields with public properties
- Fires C# events (System.Action or System.EventHandler) when state changes
- Receives OneSignalRepository and PreferencesService via Init() method
- Lives on the same DontDestroyOnLoad GameObject as the bootstrapper

HomeScreenController extends MonoBehaviour:
- Subscribes to AppViewModel events to refresh UI bindings
- Owns the root VisualElement reference via UIDocument
- Delegates user actions to AppViewModel methods
```

### Prompt 8.2 - Reusable UI Components

```
Create reusable UI components using UI Toolkit (UXML + USS + C# controllers):

Assets/UI/Components/:

SectionCard.uxml + SectionCard.uss:
- Card container with title Label and optional info IconButton
- Content slot (VisualElement) for child content
- OnInfoTap callback for tooltips
- Consistent padding and styling

ToggleRow.uxml:
- Label, optional description, Toggle (switch-style)
- Row layout with space-between justification

ActionButton.uss:
- PrimaryButton style (filled, primary color background)
- DestructiveButton style (outlined, red accent)
- Full-width via flex-grow or width: 100%

ListWidgets/:
- PairItem (key-value with optional delete IconButton)
- SingleItem (single value with delete IconButton)
- EmptyState (centered "No items" Label)
- CollapsibleList (shows 5 items, expandable)
- PairList (simple list of key-value pairs)

LoadingOverlay.uxml:
- Semi-transparent full-screen overlay using position: absolute + stretch
- Centered spinner element
- Shown via DisplayStyle.Flex / DisplayStyle.None based on isLoading state

Dialogs/:
- All dialogs use full-width layout within a modal overlay
- SingleInputDialog (one TextField)
- PairInputDialog (key-value TextFields on the same row, single pair)
- MultiPairInputDialog (dynamic rows with dividers, X button to delete a row, batch submit)
- MultiSelectRemoveDialog (Toggle checkboxes for batch remove)
- LoginDialog, OutcomeDialog, TrackEventDialog
- CustomNotificationDialog, TooltipDialog

Dialogs are VisualElements added to the root overlay, not separate scenes.
Use a shared DialogBase class that handles modal backdrop, close on backdrop tap,
and basic layout. Each dialog extends DialogBase.
```

### Prompt 8.3 - Reusable Multi-Pair Dialog

```
Tags, Aliases, and Triggers all share a reusable MultiPairInputDialog component
for adding multiple key-value pairs at once.

Behavior:
- Dialog opens full-width with modal backdrop
- Starts with one empty key-value row (Key and Value TextFields side by side)
- "Add Row" Button below the rows adds another empty row
- Dividers separate each row for visual clarity
- Each row shows an X (close icon) delete button on the right (hidden when only one row)
- "Add All" button is disabled until ALL key and value fields in every row are filled
- Validation runs on every text change and after row add/remove (via RegisterValueChangedCallback)
- On "Add All" press, all rows are collected and submitted as a batch
- Batch operations use SDK bulk APIs (AddAliases, AddTags, AddTriggers)

Used by:
- ADD MULTIPLE button (Aliases section) -> calls viewModel.AddAliases(pairs)
- ADD MULTIPLE button (Tags section) -> calls viewModel.AddTags(pairs)
- ADD MULTIPLE button (Triggers section) -> calls viewModel.AddTriggers(pairs)
```

### Prompt 8.4 - Reusable Remove Multi Dialog

```
Tags and Triggers share a reusable MultiSelectRemoveDialog component
for selectively removing items from the current list.

Behavior:
- Accepts the current list of items as List<KeyValuePair<string, string>>
- Renders one Toggle (checkbox) per item on the left with just the key as the label (not "key: value")
- User can check 0, 1, or more items
- "Remove (N)" button shows count of selected items, disabled when none selected
- On confirm, checked items' keys are collected as List<string> and passed to the callback

Used by:
- REMOVE SELECTED button (Tags section) -> calls viewModel.RemoveSelectedTags(keys)
- REMOVE SELECTED button (Triggers section) -> calls viewModel.RemoveSelectedTriggers(keys)
```

### Prompt 8.5 - Theme

```
Create OneSignal theme in Assets/UI/Theme/:

Colors (defined in USS variables at :root):
- --os-red: #E54B4D           (primary)
- --os-green: #34A853          (success)
- --os-green-light: #E6F4EA    (success background)
- --os-bg-light: #F8F9FA       (light background)
- --os-card-bg: #FFFFFF         (card background)
- --os-divider: #E8EAED         (divider color)
- --os-warning-bg: #FFF8E1      (warning background)

Spacing constants (USS variables):
- --card-gap: 8px     // gap between a card/banner and its action buttons within a section
- --section-gap: 12px // gap between sections

Theme.uss (imported by all UXML files):
- Card style with rounded corners (12px border-radius), white background, subtle shadow
- Button styles with rounded corners (8px border-radius)
- Input field styles with outline border
- Consistent font sizes and spacing
```

### Prompt 8.6 - Log View (Appium-Ready)

```
Add collapsible log view at top of screen for debugging and Appium testing.

Files:
- Assets/Scripts/Services/LogManager.cs  - Singleton logger
- Assets/UI/Components/LogView.uxml      - Log viewer layout
- Assets/UI/Components/LogView.uss       - Log viewer styling
- Assets/Scripts/UI/LogViewController.cs - Log viewer controller

LogManager Features:
- Singleton pattern (private constructor, static Instance property)
- C# event Action<LogEntry> OnLogAdded for reactive UI updates
- API: LogManager.Instance.Debug(tag, message), .Info(), .Warn(), .Error()
- Also prints to console via UnityEngine.Debug.Log for development

LogView Features:
- STICKY at the top of the screen (always visible while scrolling content below)
- Full width, no horizontal margin, no rounded corners, no top margin (touches app bar area)
- Background color: #1A1B1E
- Single horizontal scroll on the entire log list (not per-row), no text truncation
- Use ScrollView with horizontal scrolling enabled
- Fixed 100px height
- Default expanded
- Trash icon button for clearing logs, not a text button
- Auto-scroll to newest entry

Appium Element Names (set via VisualElement.name):
| Name | Description |
|------|-------------|
| log_view_container | Main container |
| log_view_header | Tappable expand/collapse |
| log_view_count | Shows "(N)" log count |
| log_view_clear_button | Clear all logs |
| log_view_list | Scrollable log list |
| log_view_empty | "No logs yet" state |
| log_entry_N | Each log row (N=index) |
| log_entry_N_timestamp | Timestamp text |
| log_entry_N_level | D/I/W/E indicator |
| log_entry_N_message | Log message content |
```

### Prompt 8.7 - Toast Messages

```
All user actions should display toast messages (brief on-screen notifications):

- Login: "Logged in as: {userId}"
- Logout: "Logged out"
- Add alias: "Alias added: {label}"
- Add multiple aliases: "{count} alias(es) added"
- Similar patterns for tags, triggers, emails, SMS
- Notifications: "Notification sent: {type}" or "Failed to send notification"
- In-App Messages: "Sent In-App Message: {type}"
- Outcomes: "Outcome sent: {name}"
- Events: "Event tracked: {name}"
- Location: "Location sharing enabled/disabled"
- Push: "Push enabled/disabled"

Implementation:
- AppViewModel exposes an event Action<string> OnToastMessage
- HomeScreenController subscribes and shows a ToastView overlay element
- ToastView is a Label styled at the bottom of the screen, auto-hides after ~2 seconds
- All toast messages are also logged via LogManager.Instance.Info()
- Remove previous toast before showing new one (hide existing, show new)
```

---

## Key Files Structure

```
examples/demo/
├── Assets/
│   ├── Scenes/
│   │   ├── Main.unity                          # Main scene with HomeScreen
│   │   └── Secondary.unity                     # Secondary activity scene
│   ├── Scripts/
│   │   ├── AppBootstrapper.cs                  # SDK init, DI setup, DontDestroyOnLoad
│   │   ├── Models/
│   │   │   ├── UserData.cs                     # UserData model from API
│   │   │   ├── NotificationType.cs             # Enum with bigPicture and iosAttachments
│   │   │   └── InAppMessageType.cs             # Enum with icon names
│   │   ├── Services/
│   │   │   ├── OneSignalApiService.cs          # REST API client (UnityWebRequest)
│   │   │   ├── PreferencesService.cs           # PlayerPrefs wrapper
│   │   │   ├── TooltipHelper.cs                # Fetches tooltips from remote URL
│   │   │   └── LogManager.cs                   # Singleton logger with events
│   │   ├── Repositories/
│   │   │   └── OneSignalRepository.cs          # Centralized SDK calls
│   │   ├── ViewModels/
│   │   │   └── AppViewModel.cs                 # MonoBehaviour with all UI state + events
│   │   └── UI/
│   │       ├── HomeScreenController.cs         # Main screen UI bindings
│   │       ├── SecondaryScreenController.cs    # Secondary screen controller
│   │       ├── LogViewController.cs            # Log view controller
│   │       ├── ToastView.cs                    # Toast notification overlay
│   │       ├── Dialogs/
│   │       │   ├── DialogBase.cs               # Base class for modal dialogs
│   │       │   ├── SingleInputDialog.cs        # One input field dialog
│   │       │   ├── PairInputDialog.cs          # Key-value input dialog
│   │       │   ├── MultiPairInputDialog.cs     # Dynamic rows dialog
│   │       │   ├── MultiSelectRemoveDialog.cs  # Checkbox multi-select dialog
│   │       │   ├── LoginDialog.cs
│   │       │   ├── OutcomeDialog.cs
│   │       │   ├── TrackEventDialog.cs
│   │       │   ├── CustomNotificationDialog.cs
│   │       │   └── TooltipDialog.cs
│   │       └── Sections/
│   │           ├── AppSectionController.cs
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
│   │   │   └── Theme.uss                      # Global USS theme with variables
│   │   ├── Screens/
│   │   │   ├── HomeScreen.uxml                # Main screen layout
│   │   │   └── SecondaryScreen.uxml           # Secondary screen layout
│   │   ├── Components/
│   │   │   ├── SectionCard.uxml
│   │   │   ├── ToggleRow.uxml
│   │   │   ├── LogView.uxml
│   │   │   ├── LogView.uss
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
│       └── onesignal_logo.png                  # App bar logo
├── Packages/
│   └── manifest.json                           # Unity Package Manager dependencies
├── ProjectSettings/
│   └── ProjectSettings.asset                   # Player settings (bundle ID, icons, etc.)
├── google-services.json                        # Firebase config (Android)
└── agconnect-services.json                     # Huawei config (Android, if needed)
```

Note:
- All UI is built with Unity UI Toolkit (UXML + USS) for modern, resolution-independent layouts
- Tooltip content is fetched from remote URL (not bundled locally)
- LogView at top of screen displays SDK and app logs for debugging/Appium testing
- MonoBehaviour + C# events are used for state management (no third-party DI framework required)

---

## Configuration

### App ID Placeholder

```csharp
// In AppBootstrapper.cs or a Constants.cs file
private const string OneSignalAppId = "77e32082-ea27-42e3-a898-c72e141824ef";
```

Note: REST API key is NOT required for the fetchUser endpoint.

### Package / Bundle Identifier

The identifiers MUST be `com.onesignal.example` to work with the existing:
- `google-services.json` (Firebase configuration)
- `agconnect-services.json` (Huawei configuration)

If you change the identifier, you must also update these files with your own Firebase/Huawei project configuration.

---

## Unity / C# Best Practices Applied

- **DontDestroyOnLoad** for persistent managers (AppBootstrapper, AppViewModel) across scene loads
- **C# events** (Action, EventHandler) for decoupled state-to-UI communication instead of tight coupling
- **SerializeField** over public fields for Inspector-exposed references; keeps the API surface clean
- **Singleton pattern** for services (LogManager, TooltipHelper) that must survive scene transitions
- **async/await** with Task-based APIs where the SDK supports it (RequestPermissionAsync, EnterAsync)
- **PlayerPrefs wrapped** in a typed PreferencesService to avoid scattered string keys
- **UI Toolkit** (UXML + USS) for resolution-independent, stylesheet-driven UI over legacy Canvas
- **Separation of concerns**: Repository wraps SDK, ViewModel holds state, Controllers bind UI
- **Unsubscribe in OnDestroy** from all SDK events and C# events to prevent memory leaks
- **Assembly definitions** (.asmdef) to organize scripts into compiled units for faster iteration
- **Coroutine-to-Task bridge** for UnityWebRequest calls so async/await can be used consistently
- **USS variables** for theme tokens (colors, spacing) to enable consistent styling across components
- **VisualElement.name** on all interactive elements for Appium automation and debugging
- **Null-conditional operators** (?.) and null-coalescing (??) for safe SDK property access
- **try/catch** on all async SDK and network calls with errors logged via LogManager
- **JsonUtility or Newtonsoft.Json** for JSON parsing (avoid manual string parsing)
- **ScriptableObject** as an option for shared configuration data (app ID, endpoints) that persists across scenes

---

## Summary

This app demonstrates all OneSignal Unity SDK features:
- User management (login/logout, aliases with batch add)
- Push notifications (subscription, sending with images, auto-permission prompt)
- Email and SMS subscriptions
- Tags for segmentation (batch add/remove support)
- Triggers for in-app message targeting (in-memory only, batch operations)
- Outcomes for conversion tracking
- Event tracking with JSON properties validation
- In-app messages (display testing with type-specific icons)
- Location sharing
- Privacy consent management

The app is designed to be:
1. **Testable** - Empty dialogs with named elements for Appium automation
2. **Comprehensive** - All SDK features demonstrated
3. **Clean** - Repository pattern with event-driven state management
4. **Cross-platform** - Single codebase for Android and iOS via Unity
5. **Session-based triggers** - Triggers stored in memory only, cleared on restart
6. **Responsive UI** - Loading indicator with delay to ensure UI populates before dismissing
7. **Performant** - Tooltip JSON loaded asynchronously, assembly definitions for fast compilation
8. **Modern UI** - UI Toolkit with USS theming and reusable components
9. **Batch Operations** - Add multiple items at once, select and remove multiple items
