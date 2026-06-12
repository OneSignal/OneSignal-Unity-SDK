using OneSignalSDK;
using OneSignalSDK.Debug.Models;
using UnityEngine;

public sealed class NoLocationDemo : MonoBehaviour
{
    [SerializeField]
    private string _oneSignalAppId = "YOUR-ONESIGNAL-APP-ID";

    private string _status = "Set your OneSignal App ID in the Inspector.";
    private bool _initialized;

    private void Start()
    {
        OneSignal.Debug.LogLevel = LogLevel.Verbose;

        if (!IsConfigured)
            return;

        OneSignal.Initialize(_oneSignalAppId);
        _initialized = true;
        _status = "OneSignal initialized without native location module.";
    }

    private void OnGUI()
    {
        const int margin = 24;
        const int buttonHeight = 56;
        var width = Screen.width - margin * 2;

        GUILayout.BeginArea(new Rect(margin, margin, width, Screen.height - margin * 2));
        GUILayout.Label("OneSignal No-Location Demo");
        GUILayout.Space(12);
        GUILayout.Label(_status);
        GUILayout.Space(12);

        if (GUILayout.Button("Request Push Permission", GUILayout.Height(buttonHeight)))
            RequestPushPermission();

        if (GUILayout.Button("Test Location Request No-Op", GUILayout.Height(buttonHeight)))
            TestLocationRequest();

        GUILayout.Space(12);
        GUILayout.Label(
            $"Location IsShared: {(_initialized ? OneSignal.Location.IsShared.ToString() : "Not initialized")}"
        );
        GUILayout.EndArea();
    }

    private async void RequestPushPermission()
    {
        if (!_initialized)
        {
            _status = "Initialize OneSignal before requesting push permission.";
            return;
        }

        var granted = await OneSignal.Notifications.RequestPermissionAsync(false);
        _status = $"Push permission: {(granted ? "granted" : "not granted")}";
    }

    private void TestLocationRequest()
    {
        if (!_initialized)
        {
            _status = "Initialize OneSignal before testing location.";
            return;
        }

        OneSignal.Location.RequestPermission();
        _status = "Location request no-op completed.";
    }

    private bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_oneSignalAppId)
        && !_oneSignalAppId.StartsWith("YOUR-");
}
