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

            _prefs = new PreferencesService();
            _apiService = new OneSignalApiService();

            var envAppId = DotEnv.Get("ONESIGNAL_APP_ID");
            var appId =
                string.IsNullOrWhiteSpace(envAppId) || envAppId == PlaceholderAppId
                    ? DefaultAppId
                    : envAppId;

            _apiService.SetAppId(appId);

            OneSignal.Debug.LogLevel = LogLevel.Verbose;
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

            OneSignal.InAppMessages.Paused = _prefs.IamPaused;
            OneSignal.Location.IsShared = _prefs.LocationShared;

            RegisterSdkListeners();

            _viewModel.Init(_prefs, _apiService);
            _viewModel.LoadInitialState();
            await _viewModel.LoadInitialDataAsync();

            _viewModel.PromptPush();

            _ = TooltipHelper.Instance.InitAsync();
            Debug.Log($"[{Tag}] App initialized");
        }

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

        private void OnIamWillDisplay(object sender, InAppMessageWillDisplayEventArgs e) =>
            Debug.Log($"[{Tag}] IAM will display: {e.Message.MessageId}");

        private void OnIamDidDisplay(object sender, InAppMessageDidDisplayEventArgs e) =>
            Debug.Log($"[{Tag}] IAM did display: {e.Message.MessageId}");

        private void OnIamWillDismiss(object sender, InAppMessageWillDismissEventArgs e) =>
            Debug.Log($"[{Tag}] IAM will dismiss: {e.Message.MessageId}");

        private void OnIamDidDismiss(object sender, InAppMessageDidDismissEventArgs e) =>
            Debug.Log($"[{Tag}] IAM did dismiss: {e.Message.MessageId}");

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
