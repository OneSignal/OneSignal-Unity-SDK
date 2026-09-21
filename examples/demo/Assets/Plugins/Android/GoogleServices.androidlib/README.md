# GoogleServices.androidlib

Unity has no `com.google.gms.google-services` Gradle plugin, so this library ships the
string resources that plugin would generate from `google-services.json`.

Create `src/main/res/values/google-services.xml` (git-ignored) from your Firebase project:

```xml
<?xml version="1.0" encoding="utf-8"?>
<resources>
    <string name="google_app_id" translatable="false">1:PROJECT_NUMBER:android:APP_HASH</string>
    <string name="gcm_defaultSenderId" translatable="false">PROJECT_NUMBER</string>
    <string name="google_api_key" translatable="false">CURRENT_KEY</string>
    <string name="google_crash_reporting_api_key" translatable="false">CURRENT_KEY</string>
    <string name="project_id" translatable="false">PROJECT_ID</string>
    <string name="google_storage_bucket" translatable="false">STORAGE_BUCKET</string>
</resources>
```

Mapping from `google-services.json`:

| Resource | JSON path |
|---|---|
| `google_app_id` | `client[].client_info.mobilesdk_app_id` |
| `gcm_defaultSenderId` | `project_info.project_number` |
| `google_api_key` | `client[].api_key[].current_key` |
| `project_id` | `project_info.project_id` |
| `google_storage_bucket` | `project_info.storage_bucket` |
