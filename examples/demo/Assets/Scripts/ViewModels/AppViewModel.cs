using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OneSignalSDK;
using OneSignalSDK.Notifications;
using OneSignalSDK.User;
using OneSignalSDK.User.Models;
using OneSignalDemo.Models;
using OneSignalDemo.Repositories;
using OneSignalDemo.Services;
using UnityEngine;

namespace OneSignalDemo.ViewModels
{
    public class AppViewModel : MonoBehaviour
    {
        private OneSignalRepository _repository;
        private PreferencesService _prefs;
        private const string Tag = "AppViewModel";

        private string _appId;
        private bool _consentRequired;
        private bool _privacyConsentGiven;
        private bool _inAppMessagesPaused;
        private bool _locationShared;
        private string _externalUserId;
        private string _pushSubscriptionId;
        private bool _pushOptedIn;
        private bool _hasPermission;
        private bool _isLoading;

        private List<KeyValuePair<string, string>> _aliasesList = new();
        private List<string> _emailsList = new();
        private List<string> _smsNumbersList = new();
        private List<KeyValuePair<string, string>> _tagsList = new();
        private List<KeyValuePair<string, string>> _triggersList = new();

        public string AppId => _appId;
        public bool ConsentRequired => _consentRequired;
        public bool PrivacyConsentGiven => _privacyConsentGiven;
        public bool InAppMessagesPaused => _inAppMessagesPaused;
        public bool LocationShared => _locationShared;
        public string ExternalUserId => _externalUserId;
        public string PushSubscriptionId => _pushSubscriptionId;
        public bool PushOptedIn => _pushOptedIn;
        public bool HasPermission => _hasPermission;
        public bool IsLoading => _isLoading;
        public bool IsLoggedIn => !string.IsNullOrEmpty(_externalUserId);

        public IReadOnlyList<KeyValuePair<string, string>> Aliases => _aliasesList;
        public IReadOnlyList<string> Emails => _emailsList;
        public IReadOnlyList<string> SmsNumbers => _smsNumbersList;
        public IReadOnlyList<KeyValuePair<string, string>> Tags => _tagsList;
        public IReadOnlyList<KeyValuePair<string, string>> Triggers => _triggersList;

        public event Action OnStateChanged;
        public event Action<string> OnToastMessage;

        public void Init(OneSignalRepository repository, PreferencesService prefs)
        {
            _repository = repository;
            _prefs = prefs;

            OneSignal.User.PushSubscription.Changed += OnPushSubscriptionChanged;
            OneSignal.Notifications.PermissionChanged += OnPermissionChanged;
            OneSignal.User.Changed += OnUserChanged;
        }

        private void OnDestroy()
        {
            if (OneSignal.Default != null)
            {
                OneSignal.User.PushSubscription.Changed -= OnPushSubscriptionChanged;
                OneSignal.Notifications.PermissionChanged -= OnPermissionChanged;
                OneSignal.User.Changed -= OnUserChanged;
            }
        }

        public void LoadInitialState()
        {
            _appId = _prefs.AppId;
            _consentRequired = _prefs.ConsentRequired;
            _privacyConsentGiven = _prefs.PrivacyConsent;
            _inAppMessagesPaused = _prefs.IamPaused;
            _locationShared = _repository.IsLocationShared();
            _externalUserId = _prefs.ExternalUserId;
            _pushSubscriptionId = _repository.GetPushSubscriptionId();
            _pushOptedIn = _repository.IsPushOptedIn();
            _hasPermission = _repository.HasPermission();

            NotifyStateChanged();
        }

        public async Task LoadInitialDataAsync()
        {
            var onesignalId = _repository.GetOnesignalId();
            if (!string.IsNullOrEmpty(onesignalId))
            {
                SetLoading(true);
                await FetchUserDataFromApi();
                await Task.Yield();
                SetLoading(false);
            }
        }

        public void LoginUser(string externalUserId)
        {
            if (string.IsNullOrEmpty(externalUserId)) return;

            SetLoading(true);
            _externalUserId = externalUserId;
            _prefs.ExternalUserId = externalUserId;

            ClearUserData();
            _repository.LoginUser(externalUserId);

            LogManager.Instance.Info(Tag, $"Logged in as: {externalUserId}");
            ShowToast($"Logged in as: {externalUserId}");
            NotifyStateChanged();
        }

        public void LogoutUser()
        {
            SetLoading(true);
            _repository.LogoutUser();

            _externalUserId = "";
            _prefs.ExternalUserId = "";
            ClearUserData();

            SetLoading(false);
            LogManager.Instance.Info(Tag, "Logged out");
            ShowToast("Logged out");
            NotifyStateChanged();
        }

        public void AddAlias(string label, string id)
        {
            _repository.AddAlias(label, id);
            _aliasesList.Add(new KeyValuePair<string, string>(label, id));
            LogManager.Instance.Info(Tag, $"Alias added: {label}");
            ShowToast($"Alias added: {label}");
            NotifyStateChanged();
        }

        public void AddAliases(Dictionary<string, string> aliases)
        {
            _repository.AddAliases(aliases);
            foreach (var kvp in aliases)
                _aliasesList.Add(new KeyValuePair<string, string>(kvp.Key, kvp.Value));
            LogManager.Instance.Info(Tag, $"{aliases.Count} alias(es) added");
            ShowToast($"{aliases.Count} alias(es) added");
            NotifyStateChanged();
        }

        public void AddEmail(string email)
        {
            _repository.AddEmail(email);
            if (!_emailsList.Contains(email))
                _emailsList.Add(email);
            LogManager.Instance.Info(Tag, $"Email added: {email}");
            ShowToast($"Email added: {email}");
            NotifyStateChanged();
        }

        public void RemoveEmail(string email)
        {
            _repository.RemoveEmail(email);
            _emailsList.Remove(email);
            LogManager.Instance.Info(Tag, $"Email removed: {email}");
            ShowToast($"Email removed: {email}");
            NotifyStateChanged();
        }

        public void AddSms(string smsNumber)
        {
            _repository.AddSms(smsNumber);
            if (!_smsNumbersList.Contains(smsNumber))
                _smsNumbersList.Add(smsNumber);
            LogManager.Instance.Info(Tag, $"SMS added: {smsNumber}");
            ShowToast($"SMS added: {smsNumber}");
            NotifyStateChanged();
        }

        public void RemoveSms(string smsNumber)
        {
            _repository.RemoveSms(smsNumber);
            _smsNumbersList.Remove(smsNumber);
            LogManager.Instance.Info(Tag, $"SMS removed: {smsNumber}");
            ShowToast($"SMS removed: {smsNumber}");
            NotifyStateChanged();
        }

        public void AddTag(string key, string value)
        {
            _repository.AddTag(key, value);
            UpsertInList(_tagsList, key, value);
            LogManager.Instance.Info(Tag, $"Tag added: {key}={value}");
            ShowToast($"Tag added: {key}");
            NotifyStateChanged();
        }

        public void AddTags(Dictionary<string, string> tags)
        {
            _repository.AddTags(tags);
            foreach (var kvp in tags)
                UpsertInList(_tagsList, kvp.Key, kvp.Value);
            LogManager.Instance.Info(Tag, $"{tags.Count} tag(s) added");
            ShowToast($"{tags.Count} tag(s) added");
            NotifyStateChanged();
        }

        public void RemoveTag(string key)
        {
            _repository.RemoveTag(key);
            _tagsList.RemoveAll(kvp => kvp.Key == key);
            LogManager.Instance.Info(Tag, $"Tag removed: {key}");
            ShowToast($"Tag removed: {key}");
            NotifyStateChanged();
        }

        public void RemoveSelectedTags(List<string> keys)
        {
            _repository.RemoveTags(keys);
            _tagsList.RemoveAll(kvp => keys.Contains(kvp.Key));
            LogManager.Instance.Info(Tag, $"{keys.Count} tag(s) removed");
            ShowToast($"{keys.Count} tag(s) removed");
            NotifyStateChanged();
        }

        public void AddTrigger(string key, string value)
        {
            _repository.AddTrigger(key, value);
            UpsertInList(_triggersList, key, value);
            LogManager.Instance.Info(Tag, $"Trigger added: {key}={value}");
            ShowToast($"Trigger added: {key}");
            NotifyStateChanged();
        }

        public void AddTriggers(Dictionary<string, string> triggers)
        {
            _repository.AddTriggers(triggers);
            foreach (var kvp in triggers)
                UpsertInList(_triggersList, kvp.Key, kvp.Value);
            LogManager.Instance.Info(Tag, $"{triggers.Count} trigger(s) added");
            ShowToast($"{triggers.Count} trigger(s) added");
            NotifyStateChanged();
        }

        public void RemoveTrigger(string key)
        {
            _repository.RemoveTrigger(key);
            _triggersList.RemoveAll(kvp => kvp.Key == key);
            LogManager.Instance.Info(Tag, $"Trigger removed: {key}");
            ShowToast($"Trigger removed: {key}");
            NotifyStateChanged();
        }

        public void RemoveSelectedTriggers(List<string> keys)
        {
            _repository.RemoveTriggers(keys);
            _triggersList.RemoveAll(kvp => keys.Contains(kvp.Key));
            LogManager.Instance.Info(Tag, $"{keys.Count} trigger(s) removed");
            ShowToast($"{keys.Count} trigger(s) removed");
            NotifyStateChanged();
        }

        public void ClearAllTriggers()
        {
            _repository.ClearTriggers();
            _triggersList.Clear();
            LogManager.Instance.Info(Tag, "All triggers cleared");
            ShowToast("All triggers cleared");
            NotifyStateChanged();
        }

        public void SendInAppMessage(InAppMessageType type)
        {
            var triggerValue = type.TriggerValue();
            _repository.AddTrigger("iam_type", triggerValue);
            UpsertInList(_triggersList, "iam_type", triggerValue);
            LogManager.Instance.Info(Tag, $"Sent In-App Message: {type.DisplayName()}");
            ShowToast($"Sent In-App Message: {type.DisplayName()}");
            NotifyStateChanged();
        }

        public void SendOutcome(string name)
        {
            _repository.SendOutcome(name);
            LogManager.Instance.Info(Tag, $"Outcome sent: {name}");
            ShowToast($"Outcome sent: {name}");
        }

        public void SendUniqueOutcome(string name)
        {
            _repository.SendUniqueOutcome(name);
            LogManager.Instance.Info(Tag, $"Unique outcome sent: {name}");
            ShowToast($"Unique outcome sent: {name}");
        }

        public void SendOutcomeWithValue(string name, float value)
        {
            _repository.SendOutcomeWithValue(name, value);
            LogManager.Instance.Info(Tag, $"Outcome sent: {name} = {value}");
            ShowToast($"Outcome sent: {name}");
        }

        public void TrackEvent(string name, Dictionary<string, object> properties = null)
        {
            _repository.TrackEvent(name, properties);
            LogManager.Instance.Info(Tag, $"Event tracked: {name}");
            ShowToast($"Event tracked: {name}");
        }

        public async void SendNotification(NotificationType type)
        {
            try
            {
                bool success = await _repository.SendNotification(type);
                var label = type.ToString();
                if (success)
                {
                    LogManager.Instance.Info(Tag, $"Notification sent: {label}");
                    ShowToast($"Notification sent: {label}");
                }
                else
                {
                    LogManager.Instance.Error(Tag, $"Failed to send notification: {label}");
                    ShowToast("Failed to send notification");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(Tag, $"Notification error: {ex.Message}");
                ShowToast("Failed to send notification");
            }
        }

        public async void SendCustomNotification(string title, string body)
        {
            try
            {
                bool success = await _repository.SendCustomNotification(title, body);
                if (success)
                {
                    LogManager.Instance.Info(Tag, $"Custom notification sent: {title}");
                    ShowToast($"Notification sent: Custom");
                }
                else
                {
                    LogManager.Instance.Error(Tag, "Failed to send custom notification");
                    ShowToast("Failed to send notification");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(Tag, $"Custom notification error: {ex.Message}");
                ShowToast("Failed to send notification");
            }
        }

        public void SetConsentRequired(bool required)
        {
            _consentRequired = required;
            _prefs.ConsentRequired = required;
            _repository.SetConsentRequired(required);
            if (!required)
            {
                _privacyConsentGiven = false;
                _prefs.PrivacyConsent = false;
            }
            LogManager.Instance.Info(Tag, $"Consent required: {required}");
            NotifyStateChanged();
        }

        public void SetPrivacyConsent(bool granted)
        {
            _privacyConsentGiven = granted;
            _prefs.PrivacyConsent = granted;
            _repository.SetConsentGiven(granted);
            LogManager.Instance.Info(Tag, $"Privacy consent: {granted}");
            NotifyStateChanged();
        }

        public void SetInAppMessagesPaused(bool paused)
        {
            _inAppMessagesPaused = paused;
            _prefs.IamPaused = paused;
            _repository.SetInAppMessagesPaused(paused);
            LogManager.Instance.Info(Tag, $"IAM paused: {paused}");
            NotifyStateChanged();
        }

        public void SetLocationShared(bool shared)
        {
            _locationShared = shared;
            _prefs.LocationShared = shared;
            _repository.SetLocationShared(shared);
            LogManager.Instance.Info(Tag, $"Location sharing: {(shared ? "enabled" : "disabled")}");
            ShowToast($"Location sharing {(shared ? "enabled" : "disabled")}");
            NotifyStateChanged();
        }

        public void PromptLocation()
        {
            _repository.RequestLocationPermission();
            LogManager.Instance.Info(Tag, "Location permission requested");
        }

        public async void PromptPush()
        {
            try
            {
                bool granted = await _repository.RequestPermissionAsync(true);
                _hasPermission = granted;
                LogManager.Instance.Info(Tag, $"Push permission: {(granted ? "granted" : "denied")}");
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(Tag, $"Push permission error: {ex.Message}");
            }
        }

        public void SetPushEnabled(bool enabled)
        {
            if (enabled)
                _repository.OptInPush();
            else
                _repository.OptOutPush();

            _pushOptedIn = enabled;
            LogManager.Instance.Info(Tag, $"Push {(enabled ? "enabled" : "disabled")}");
            ShowToast($"Push {(enabled ? "enabled" : "disabled")}");
            NotifyStateChanged();
        }

        public async Task FetchUserDataFromApi()
        {
            try
            {
                var onesignalId = _repository.GetOnesignalId();
                if (string.IsNullOrEmpty(onesignalId))
                {
                    SetLoading(false);
                    return;
                }

                var userData = await _repository.FetchUser(onesignalId);
                if (userData != null)
                {
                    _aliasesList = userData.Aliases.Select(kvp =>
                        new KeyValuePair<string, string>(kvp.Key, kvp.Value)).ToList();
                    _tagsList = userData.Tags.Select(kvp =>
                        new KeyValuePair<string, string>(kvp.Key, kvp.Value)).ToList();
                    _emailsList = new List<string>(userData.Emails);
                    _smsNumbersList = new List<string>(userData.SmsNumbers);

                    if (!string.IsNullOrEmpty(userData.ExternalId))
                    {
                        _externalUserId = userData.ExternalId;
                        _prefs.ExternalUserId = userData.ExternalId;
                    }

                    LogManager.Instance.Info(Tag, "User data fetched from API");
                }

                _pushSubscriptionId = _repository.GetPushSubscriptionId();
                _pushOptedIn = _repository.IsPushOptedIn();
                _hasPermission = _repository.HasPermission();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(Tag, $"Fetch user error: {ex.Message}");
            }

            await Task.Yield();
            SetLoading(false);
            NotifyStateChanged();
        }

        private void ClearUserData()
        {
            _aliasesList.Clear();
            _emailsList.Clear();
            _smsNumbersList.Clear();
            _triggersList.Clear();
        }

        private void SetLoading(bool loading)
        {
            _isLoading = loading;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnStateChanged?.Invoke();
        private void ShowToast(string message) => OnToastMessage?.Invoke(message);

        private static void UpsertInList(List<KeyValuePair<string, string>> list, string key, string value)
        {
            var index = list.FindIndex(kvp => kvp.Key == key);
            if (index >= 0)
                list[index] = new KeyValuePair<string, string>(key, value);
            else
                list.Add(new KeyValuePair<string, string>(key, value));
        }

        private void OnPushSubscriptionChanged(object sender, PushSubscriptionChangedEventArgs e)
        {
            _pushSubscriptionId = _repository.GetPushSubscriptionId();
            _pushOptedIn = _repository.IsPushOptedIn();
            LogManager.Instance.Info(Tag, $"Push subscription changed: {_pushSubscriptionId}");
            NotifyStateChanged();
        }

        private void OnPermissionChanged(object sender, NotificationPermissionChangedEventArgs e)
        {
            _hasPermission = e.Permission;
            LogManager.Instance.Info(Tag, $"Permission changed: {e.Permission}");
            NotifyStateChanged();
        }

        private async void OnUserChanged(object sender, UserStateChangedEventArgs e)
        {
            LogManager.Instance.Info(Tag, "User changed, fetching data...");
            await FetchUserDataFromApi();
        }
    }
}
