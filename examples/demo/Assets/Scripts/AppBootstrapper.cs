using OneSignalDemo.Repositories;
using OneSignalDemo.Services;
using OneSignalDemo.ViewModels;
using OneSignalSDK;
using OneSignalSDK.Debug.Models;
using OneSignalSDK.InAppMessages;
using OneSignalSDK.LiveActivities;
using OneSignalSDK.Notifications;
using UnityEngine;

namespace OneSignalDemo
{
    public class AppBootstrapper : MonoBehaviour
    {
        private const string OneSignalAppId = "77e32082-ea27-42e3-a898-c72e141824ef";
        private const string Tag = "AppBootstrapper";

        [SerializeField]
        private AppViewModel _viewModel;

        private PreferencesService _prefs;
        private OneSignalApiService _apiService;
        private OneSignalRepository _repository;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            _prefs = new PreferencesService();
            _apiService = new OneSignalApiService();
            _repository = new OneSignalRepository(_apiService);

            var appId = _prefs.AppId;
            if (string.IsNullOrEmpty(appId))
            {
                appId = OneSignalAppId;
                _prefs.AppId = appId;
            }

            _apiService.SetAppId(appId);
            _apiService.LoadApiKey();

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

            _viewModel.Init(_repository, _prefs);
            _viewModel.LoadInitialState();
            await _viewModel.LoadInitialDataAsync();

            if (!_viewModel.HasPermission)
                _viewModel.PromptPush();

            _ = TooltipHelper.Instance.InitAsync();
            LogManager.Instance.Info(Tag, "App initialized");
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
            LogManager.Instance.Info(Tag, $"IAM will display: {e.Message.MessageId}");

        private void OnIamDidDisplay(object sender, InAppMessageDidDisplayEventArgs e) =>
            LogManager.Instance.Info(Tag, $"IAM did display: {e.Message.MessageId}");

        private void OnIamWillDismiss(object sender, InAppMessageWillDismissEventArgs e) =>
            LogManager.Instance.Info(Tag, $"IAM will dismiss: {e.Message.MessageId}");

        private void OnIamDidDismiss(object sender, InAppMessageDidDismissEventArgs e) =>
            LogManager.Instance.Info(Tag, $"IAM did dismiss: {e.Message.MessageId}");

        private void OnIamClicked(object sender, InAppMessageClickEventArgs e) =>
            LogManager.Instance.Info(Tag, $"IAM clicked: {e.Result.ActionId}");

        private void OnNotificationClicked(object sender, NotificationClickEventArgs e) =>
            LogManager.Instance.Info(Tag, $"Notification clicked: {e.Result.ActionId}");

        private void OnNotificationForegroundWillDisplay(
            object sender,
            NotificationWillDisplayEventArgs e
        )
        {
            LogManager.Instance.Info(Tag, "Notification received in foreground");
            e.Notification.Display();
        }
    }
}
