using OneSignalDemo.Services;
using OneSignalDemo.ViewModels;
using OneSignalSDK;
using OneSignalSDK.Debug.Models;
using OneSignalSDK.InAppMessages;
using OneSignalSDK.LiveActivities;
using OneSignalSDK.Notifications;
using OneSignalSDK.Notifications.Models;
using UnityEngine;

namespace OneSignalDemo
{
    public class AppBootstrapper : MonoBehaviour
    {
        private const string DefaultAppId = "77e32082-ea27-42e3-a898-c72e141824ef";
        private const string PlaceholderAppId = "your-onesignal-app-id";
        private const string Tag = "AppBootstrapper";

        [SerializeField]
        private AppViewModel _viewModel;

        private PreferencesService _prefs;
        private OneSignalApiService _apiService;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            DotEnv.Load();

            // iOS cancels in-progress touches when the OS suspends the app, and
            // Unity's Input System raises "Touch was already deallocated" on the
            // next frame. In dev builds, that pops the engine's Development
            // Console overlay on top of the UI and occludes subsequent test taps.
            // Appium's live-activity spec deliberately locks the screen mid-tap,
            // so suppress the overlay when E2E mode is on; the exception itself
            // still logs to stdout for debugging. developerConsoleEnabled
            // prevents future pops; developerConsoleVisible hides one that's
            // already showing.
            if (DotEnv.IsE2EMode)
            {
                Debug.developerConsoleEnabled = false;
                Debug.developerConsoleVisible = false;
            }

            _prefs = new PreferencesService();
            _apiService = new OneSignalApiService();

            var envAppId = DotEnv.Get("ONESIGNAL_APP_ID");
            var appId =
                string.IsNullOrWhiteSpace(envAppId) || envAppId == PlaceholderAppId
                    ? DefaultAppId
                    : envAppId;

            _apiService.SetAppId(appId);

            OneSignal.Debug.LogLevel = LogLevel.Verbose;
#if UNITY_ANDROID && !UNITY_EDITOR
            SetAndroidWebViewDebugging(false);
#endif
            OneSignal.ConsentRequired = _prefs.ConsentRequired;
            OneSignal.ConsentGiven = _prefs.PrivacyConsent;
            OneSignal.Initialize(appId);

#if UNITY_IOS
            OneSignal.LiveActivities.SetupDefault(
                new LiveActivitySetupOptions
                {
                    EnablePushToStart = true,
                    EnablePushToUpdate = true,
                }
            );
#endif

            RegisterSdkListeners();

            OneSignal.InAppMessages.Paused = _prefs.IamPaused;
            OneSignal.Location.IsShared = _prefs.LocationShared;

            _viewModel.Init(_prefs, _apiService);
            _viewModel.LoadInitialState();
            await _viewModel.LoadInitialDataAsync();

            PromptPushForStartup();

            _ = TooltipHelper.Instance.InitAsync();
            Debug.Log($"[{Tag}] App initialized");
        }

        private void PromptPushForStartup()
        {
            // Route both runtime and E2E flows through the SDK on every
            // platform. The earlier Android E2E path called raw
            // `activity.requestPermissions(POST_NOTIFICATIONS)`, which shows
            // the OS dialog but never tells the OneSignal SDK about the
            // grant. The SDK then enqueues two competing update-subscription
            // ops (one SUBSCRIBED, one NO_PERMISSION) and which one wins is a
            // race — pushes silently never deliver when NO_PERMISSION wins.
            _viewModel.PromptPush();
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void SetAndroidWebViewDebugging(bool enabled)
        {
            if (!DotEnv.IsE2EMode)
                return;

            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                if (activity == null)
                    return;

                activity.Call(
                    "runOnUiThread",
                    new AndroidJavaRunnable(() =>
                    {
                        try
                        {
                            using var webView = new AndroidJavaClass("android.webkit.WebView");
                            webView.CallStatic("setWebContentsDebuggingEnabled", enabled);
                        }
                        catch (AndroidJavaException ex)
                        {
                            Debug.LogWarning(
                                $"[{Tag}] Could not set WebView debugging: {ex.Message}"
                            );
                        }
                    })
                );
            }
            catch (AndroidJavaException ex)
            {
                Debug.LogWarning($"[{Tag}] Could not set WebView debugging: {ex.Message}");
            }
        }
#endif

        private void RegisterSdkListeners()
        {
            OneSignal.InAppMessages.WillDisplay += OnIamWillDisplay;
            OneSignal.InAppMessages.DidDisplay += OnIamDidDisplay;
            OneSignal.InAppMessages.WillDismiss += OnIamWillDismiss;
            OneSignal.InAppMessages.DidDismiss += OnIamDidDismiss;
            OneSignal.InAppMessages.Clicked += OnIamClicked;
            OneSignal.Notifications.Clicked += OnNotificationClicked;
            OneSignal.Notifications.ForegroundWillDisplay += OnNotificationForegroundWillDisplay;
        }

        private void OnDestroy()
        {
            if (OneSignal.Default == null)
                return;

            OneSignal.InAppMessages.WillDisplay -= OnIamWillDisplay;
            OneSignal.InAppMessages.DidDisplay -= OnIamDidDisplay;
            OneSignal.InAppMessages.WillDismiss -= OnIamWillDismiss;
            OneSignal.InAppMessages.DidDismiss -= OnIamDidDismiss;
            OneSignal.InAppMessages.Clicked -= OnIamClicked;
            OneSignal.Notifications.Clicked -= OnNotificationClicked;
            OneSignal.Notifications.ForegroundWillDisplay -= OnNotificationForegroundWillDisplay;
        }

        private void OnIamWillDisplay(object sender, InAppMessageWillDisplayEventArgs e)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            SetAndroidWebViewDebugging(true);
#endif
            Debug.Log($"[{Tag}] IAM will display: {e.Message.MessageId}");
        }

        private void OnIamDidDisplay(object sender, InAppMessageDidDisplayEventArgs e) =>
            Debug.Log($"[{Tag}] IAM did display: {e.Message.MessageId}");

        private void OnIamWillDismiss(object sender, InAppMessageWillDismissEventArgs e) =>
            Debug.Log($"[{Tag}] IAM will dismiss: {e.Message.MessageId}");

        private void OnIamDidDismiss(object sender, InAppMessageDidDismissEventArgs e)
        {
            Debug.Log($"[{Tag}] IAM did dismiss: {e.Message.MessageId}");
#if UNITY_ANDROID && !UNITY_EDITOR
            SetAndroidWebViewDebugging(false);
#endif
        }

        private void OnIamClicked(object sender, InAppMessageClickEventArgs e) =>
            Debug.Log($"[{Tag}] IAM clicked: {e.Result.ActionId}");

        private void OnNotificationClicked(object sender, NotificationClickEventArgs e) =>
            Debug.Log($"[{Tag}] Notification clicked: {e.Result.ActionId}");

        private void OnNotificationForegroundWillDisplay(
            object sender,
            NotificationWillDisplayEventArgs e
        )
        {
            Debug.Log($"[{Tag}] Notification received in foreground");
            e.Notification.Display();
        }
    }
}
