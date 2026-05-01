using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OneSignalDemo.Models;
using OneSignalDemo.Services;
using OneSignalSDK;
using OneSignalSDK.Notifications;
using OneSignalSDK.User;
using OneSignalSDK.User.Models;
using UnityEngine;

namespace OneSignalDemo.ViewModels
{
    public class AppViewModel : MonoBehaviour
    {
        private PreferencesService _prefs;
        private OneSignalApiService _apiService;
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

        // Drops stale FetchUserDataFromApi responses (mirrors requestSequenceRef in RN useOneSignal).
        private int _requestSequence;

        private List<KeyValuePair<string, string>> _aliasesList = new();
        private List<string> _emailsList = new();
        private List<string> _smsNumbersList = new();
        private List<KeyValuePair<string, string>> _tagsList = new();
        private List<KeyValuePair<string, string>> _triggersList = new();

        private int _liveActivityStatusIndex;
        private bool _isLiveActivityUpdating;

        private static readonly string[] LiveActivityStatuses = { "preparing", "on_the_way", "delivered" };
        private static readonly string[] LiveActivityMessages =
        {
            "Your order is being prepared",
            "Driver is heading your way",
            "Order delivered!",
        };
        private static readonly string[] LiveActivityETAs = { "15 min", "10 min", "" };
        private static readonly string[] LiveActivityStatusLabels = { "PREPARING", "ON THE WAY", "DELIVERED" };

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

        public int LiveActivityStatusIndex => _liveActivityStatusIndex;
        public bool IsLiveActivityUpdating => _isLiveActivityUpdating;
        public bool HasApiKey => _apiService?.HasApiKey() ?? false;
        public static bool IsE2EMode => DotEnv.IsE2EMode;

        public string NextStatusLabel
        {
            get
            {
                int nextIndex = (_liveActivityStatusIndex + 1) % LiveActivityStatuses.Length;
                return LiveActivityStatusLabels[nextIndex];
            }
        }

        public event Action OnStateChanged;
        public event Action<string> OnToastMessage;

        public void Init(PreferencesService prefs, OneSignalApiService apiService)
        {
            _prefs = prefs;
            _apiService = apiService;

            OneSignal.User.PushSubscription.Changed += OnPushSubscriptionChanged;
            OneSignal.Notifications.PermissionChanged += OnPermissionChanged;
            OneSignal.User.Changed += OnUserChanged;
        }

        private static string MaskValue(string value) =>
            string.IsNullOrEmpty(value) ? value : new string('\u2022', value.Length);

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
            var rawAppId = _apiService?.GetAppId() ?? "";
            _appId = IsE2EMode ? MaskValue(rawAppId) : rawAppId;
            _consentRequired = _prefs.ConsentRequired;
            _privacyConsentGiven = _prefs.PrivacyConsent;
            _inAppMessagesPaused = OneSignal.InAppMessages.Paused;
            _locationShared = OneSignal.Location.IsShared;
            _externalUserId = _prefs.ExternalUserId;

            var rawPushId = OneSignal.User.PushSubscription.Id ?? "";
            _pushSubscriptionId = IsE2EMode ? MaskValue(rawPushId) : rawPushId;
            _pushOptedIn = OneSignal.User.PushSubscription.OptedIn;
            _hasPermission = OneSignal.Notifications.Permission;

            NotifyStateChanged();
        }

        public async Task LoadInitialDataAsync()
        {
            var onesignalId = OneSignal.User.OneSignalId;
            if (!string.IsNullOrEmpty(onesignalId))
            {
                await FetchUserDataFromApi();
            }
        }

        public void LoginUser(string externalUserId)
        {
            if (string.IsNullOrEmpty(externalUserId))
                return;

            ClearUserData();
            SetLoading(true);

            try
            {
                OneSignal.Login(externalUserId);
                _prefs.ExternalUserId = externalUserId;
                _externalUserId = externalUserId;

                Debug.Log($"[{Tag}] Logged in as: {externalUserId}");
                ShowToast($"Logged in as: {externalUserId}");
                // The user 'change' listener runs FetchUserDataFromApi once the new
                // onesignalId is assigned; that call clears isLoading in its finally.
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{Tag}] Login error: {ex.Message}");
                SetLoading(false);
            }

            NotifyStateChanged();
        }

        public void LogoutUser()
        {
            OneSignal.Logout();

            _externalUserId = "";
            _prefs.ExternalUserId = "";
            ClearUserData();

            Debug.Log($"[{Tag}] Logged out");
            ShowToast("Logged out");
            NotifyStateChanged();
        }

        public void AddAlias(string label, string id)
        {
            OneSignal.User.AddAlias(label, id);
            _aliasesList.Add(new KeyValuePair<string, string>(label, id));
            Debug.Log($"[{Tag}] Alias added: {label}");
            ShowToast($"Alias added: {label}");
            NotifyStateChanged();
        }

        public void AddAliases(Dictionary<string, string> aliases)
        {
            OneSignal.User.AddAliases(aliases);
            foreach (var kvp in aliases)
                _aliasesList.Add(new KeyValuePair<string, string>(kvp.Key, kvp.Value));
            Debug.Log($"[{Tag}] {aliases.Count} alias(es) added");
            ShowToast($"{aliases.Count} alias(es) added");
            NotifyStateChanged();
        }

        public void AddEmail(string email)
        {
            OneSignal.User.AddEmail(email);
            if (!_emailsList.Contains(email))
                _emailsList.Add(email);
            Debug.Log($"[{Tag}] Email added: {email}");
            ShowToast($"Email added: {email}");
            NotifyStateChanged();
        }

        public void RemoveEmail(string email)
        {
            OneSignal.User.RemoveEmail(email);
            _emailsList.Remove(email);
            Debug.Log($"[{Tag}] Email removed: {email}");
            ShowToast($"Email removed: {email}");
            NotifyStateChanged();
        }

        public void AddSms(string smsNumber)
        {
            OneSignal.User.AddSms(smsNumber);
            if (!_smsNumbersList.Contains(smsNumber))
                _smsNumbersList.Add(smsNumber);
            Debug.Log($"[{Tag}] SMS added: {smsNumber}");
            ShowToast($"SMS added: {smsNumber}");
            NotifyStateChanged();
        }

        public void RemoveSms(string smsNumber)
        {
            OneSignal.User.RemoveSms(smsNumber);
            _smsNumbersList.Remove(smsNumber);
            Debug.Log($"[{Tag}] SMS removed: {smsNumber}");
            ShowToast($"SMS removed: {smsNumber}");
            NotifyStateChanged();
        }

        public void AddTag(string key, string value)
        {
            OneSignal.User.AddTag(key, value);
            UpsertInList(_tagsList, key, value);
            Debug.Log($"[{Tag}] Tag added: {key}={value}");
            ShowToast($"Tag added: {key}");
            NotifyStateChanged();
        }

        public void AddTags(Dictionary<string, string> tags)
        {
            OneSignal.User.AddTags(tags);
            foreach (var kvp in tags)
                UpsertInList(_tagsList, kvp.Key, kvp.Value);
            Debug.Log($"[{Tag}] {tags.Count} tag(s) added");
            ShowToast($"{tags.Count} tag(s) added");
            NotifyStateChanged();
        }

        public void RemoveTag(string key)
        {
            OneSignal.User.RemoveTag(key);
            _tagsList.RemoveAll(kvp => kvp.Key == key);
            Debug.Log($"[{Tag}] Tag removed: {key}");
            ShowToast($"Tag removed: {key}");
            NotifyStateChanged();
        }

        public void RemoveSelectedTags(List<string> keys)
        {
            OneSignal.User.RemoveTags(keys.ToArray());
            _tagsList.RemoveAll(kvp => keys.Contains(kvp.Key));
            Debug.Log($"[{Tag}] {keys.Count} tag(s) removed");
            ShowToast($"{keys.Count} tag(s) removed");
            NotifyStateChanged();
        }

        public void AddTrigger(string key, string value)
        {
            OneSignal.InAppMessages.AddTrigger(key, value);
            UpsertInList(_triggersList, key, value);
            Debug.Log($"[{Tag}] Trigger added: {key}={value}");
            ShowToast($"Trigger added: {key}");
            NotifyStateChanged();
        }

        public void AddTriggers(Dictionary<string, string> triggers)
        {
            OneSignal.InAppMessages.AddTriggers(triggers);
            foreach (var kvp in triggers)
                UpsertInList(_triggersList, kvp.Key, kvp.Value);
            Debug.Log($"[{Tag}] {triggers.Count} trigger(s) added");
            ShowToast($"{triggers.Count} trigger(s) added");
            NotifyStateChanged();
        }

        public void RemoveTrigger(string key)
        {
            OneSignal.InAppMessages.RemoveTrigger(key);
            _triggersList.RemoveAll(kvp => kvp.Key == key);
            Debug.Log($"[{Tag}] Trigger removed: {key}");
            ShowToast($"Trigger removed: {key}");
            NotifyStateChanged();
        }

        public void RemoveSelectedTriggers(List<string> keys)
        {
            OneSignal.InAppMessages.RemoveTriggers(keys.ToArray());
            _triggersList.RemoveAll(kvp => keys.Contains(kvp.Key));
            Debug.Log($"[{Tag}] {keys.Count} trigger(s) removed");
            ShowToast($"{keys.Count} trigger(s) removed");
            NotifyStateChanged();
        }

        public void ClearAllTriggers()
        {
            OneSignal.InAppMessages.ClearTriggers();
            _triggersList.Clear();
            Debug.Log($"[{Tag}] All triggers cleared");
            ShowToast("All triggers cleared");
            NotifyStateChanged();
        }

        public void SendInAppMessage(InAppMessageType type)
        {
            var triggerValue = type.TriggerValue();
            OneSignal.InAppMessages.AddTrigger("iam_type", triggerValue);
            UpsertInList(_triggersList, "iam_type", triggerValue);
            Debug.Log($"[{Tag}] Sent In-App Message: {type.DisplayName()}");
            ShowToast($"Sent In-App Message: {type.DisplayName()}");
            NotifyStateChanged();
        }

        public void SendOutcome(string name)
        {
            OneSignal.Session.AddOutcome(name);
            Debug.Log($"[{Tag}] Outcome sent: {name}");
            ShowToast($"Outcome sent: {name}");
        }

        public void SendUniqueOutcome(string name)
        {
            OneSignal.Session.AddUniqueOutcome(name);
            Debug.Log($"[{Tag}] Unique outcome sent: {name}");
            ShowToast($"Unique outcome sent: {name}");
        }

        public void SendOutcomeWithValue(string name, float value)
        {
            OneSignal.Session.AddOutcomeWithValue(name, value);
            Debug.Log($"[{Tag}] Outcome sent: {name} = {value}");
            ShowToast($"Outcome sent: {name}");
        }

        public void TrackEvent(string name, Dictionary<string, object> properties = null)
        {
            OneSignal.User.TrackEvent(name, properties);
            Debug.Log($"[{Tag}] Event tracked: {name}");
            ShowToast($"Event tracked: {name}");
        }

        public async void SendNotification(NotificationType type)
        {
            try
            {
                var pushId = OneSignal.User.PushSubscription.Id;
                bool success = await _apiService.SendNotification(type, pushId);
                var label = type.ToString();
                if (success)
                {
                    Debug.Log($"[{Tag}] Notification sent: {label}");
                    ShowToast($"Notification sent: {label}");
                }
                else
                {
                    Debug.LogError($"[{Tag}] Failed to send notification: {label}");
                    ShowToast("Failed to send notification");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{Tag}] Notification error: {ex.Message}");
                ShowToast("Failed to send notification");
            }
        }

        public void ClearAllNotifications()
        {
            OneSignal.Notifications.ClearAllNotifications();
            Debug.Log($"[{Tag}] All notifications cleared");
            ShowToast("All notifications cleared");
        }

        public async void SendCustomNotification(string title, string body)
        {
            try
            {
                var pushId = OneSignal.User.PushSubscription.Id;
                bool success = await _apiService.SendCustomNotification(title, body, pushId);
                if (success)
                {
                    Debug.Log($"[{Tag}] Custom notification sent: {title}");
                    ShowToast($"Notification sent: Custom");
                }
                else
                {
                    Debug.LogError($"[{Tag}] Failed to send custom notification");
                    ShowToast("Failed to send notification");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{Tag}] Custom notification error: {ex.Message}");
                ShowToast("Failed to send notification");
            }
        }

        public void StartLiveActivity(string activityId, string orderNumber)
        {
            if (string.IsNullOrEmpty(activityId))
                return;

            var attributes = new Dictionary<string, object> { { "orderNumber", orderNumber } };
            var content = new Dictionary<string, object>
            {
                { "status", LiveActivityStatuses[0] },
                { "message", LiveActivityMessages[0] },
                { "estimatedTime", LiveActivityETAs[0] },
            };

            OneSignal.LiveActivities.StartDefault(activityId, attributes, content);
            _liveActivityStatusIndex = 0;

            Debug.Log($"[{Tag}] Started Live Activity: {activityId}");
            ShowToast($"Started Live Activity: {activityId}");
            NotifyStateChanged();
        }

        public async void UpdateLiveActivity(string activityId)
        {
            if (string.IsNullOrEmpty(activityId) || _isLiveActivityUpdating)
                return;

            _isLiveActivityUpdating = true;
            NotifyStateChanged();

            try
            {
                int nextIndex = (_liveActivityStatusIndex + 1) % LiveActivityStatuses.Length;
                var eventUpdates = new JObject
                {
                    ["data"] = new JObject
                    {
                        ["status"] = LiveActivityStatuses[nextIndex],
                        ["message"] = LiveActivityMessages[nextIndex],
                        ["estimatedTime"] = LiveActivityETAs[nextIndex],
                    },
                };

                bool success = await _apiService.UpdateLiveActivity(activityId, "update", eventUpdates);
                if (success)
                {
                    _liveActivityStatusIndex = nextIndex;
                    Debug.Log($"[{Tag}] Updated Live Activity: {activityId}");
                    ShowToast($"Updated Live Activity: {activityId}");
                }
                else
                {
                    Debug.LogError($"[{Tag}] Failed to update Live Activity");
                    ShowToast("Failed to update Live Activity");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{Tag}] Live Activity update error: {ex.Message}");
                ShowToast("Failed to update Live Activity");
            }

            _isLiveActivityUpdating = false;
            NotifyStateChanged();
        }

        public async void EndLiveActivity(string activityId)
        {
            if (string.IsNullOrEmpty(activityId) || _isLiveActivityUpdating)
                return;

            _isLiveActivityUpdating = true;
            NotifyStateChanged();

            try
            {
                var eventUpdates = new JObject
                {
                    ["data"] = new JObject
                    {
                        ["status"] = "delivered",
                        ["message"] = "Ended",
                        ["estimatedTime"] = "",
                    },
                };

                bool success = await _apiService.UpdateLiveActivity(activityId, "end", eventUpdates);
                if (success)
                {
                    _liveActivityStatusIndex = 0;
                    Debug.Log($"[{Tag}] Ended Live Activity: {activityId}");
                    ShowToast($"Ended Live Activity: {activityId}");
                }
                else
                {
                    Debug.LogError($"[{Tag}] Failed to end Live Activity");
                    ShowToast("Failed to end Live Activity");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{Tag}] Live Activity end error: {ex.Message}");
                ShowToast("Failed to end Live Activity");
            }

            _isLiveActivityUpdating = false;
            NotifyStateChanged();
        }

        public void SetConsentRequired(bool required)
        {
            _consentRequired = required;
            _prefs.ConsentRequired = required;
            OneSignal.ConsentRequired = required;
            if (!required)
            {
                _privacyConsentGiven = false;
                _prefs.PrivacyConsent = false;
            }
            Debug.Log($"[{Tag}] Consent required: {required}");
            NotifyStateChanged();
        }

        public void SetPrivacyConsent(bool granted)
        {
            _privacyConsentGiven = granted;
            _prefs.PrivacyConsent = granted;
            OneSignal.ConsentGiven = granted;
            Debug.Log($"[{Tag}] Privacy consent: {granted}");
            NotifyStateChanged();
        }

        public void SetInAppMessagesPaused(bool paused)
        {
            _inAppMessagesPaused = paused;
            _prefs.IamPaused = paused;
            OneSignal.InAppMessages.Paused = paused;
            Debug.Log($"[{Tag}] IAM paused: {paused}");
            NotifyStateChanged();
        }

        public void SetLocationShared(bool shared)
        {
            _locationShared = shared;
            _prefs.LocationShared = shared;
            OneSignal.Location.IsShared = shared;
            Debug.Log($"[{Tag}] Location sharing: {(shared ? "enabled" : "disabled")}");
            ShowToast($"Location sharing {(shared ? "enabled" : "disabled")}");
            NotifyStateChanged();
        }

        public void PromptLocation()
        {
            OneSignal.Location.RequestPermission();
            Debug.Log($"[{Tag}] Location permission requested");
        }

        public void CheckLocationShared()
        {
            var shared = OneSignal.Location.IsShared;
            ShowToast($"Location shared: {shared.ToString().ToLowerInvariant()}");
        }

        public async void PromptPush()
        {
            try
            {
                bool granted = await OneSignal.Notifications.RequestPermissionAsync(true);
                _hasPermission = granted;
                Debug.Log($"[{Tag}] Push permission: {(granted ? "granted" : "denied")}");
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{Tag}] Push permission error: {ex.Message}");
            }
        }

        public void SetPushEnabled(bool enabled)
        {
            if (enabled)
                OneSignal.User.PushSubscription.OptIn();
            else
                OneSignal.User.PushSubscription.OptOut();

            _pushOptedIn = enabled;
            Debug.Log($"[{Tag}] Push {(enabled ? "enabled" : "disabled")}");
            ShowToast($"Push {(enabled ? "enabled" : "disabled")}");
            NotifyStateChanged();
        }

        public async Task FetchUserDataFromApi()
        {
            var requestId = ++_requestSequence;
            SetLoading(true);

            try
            {
                var onesignalId = OneSignal.User.OneSignalId;
                if (string.IsNullOrEmpty(onesignalId))
                    return;

                var userData = await _apiService.FetchUser(onesignalId);
                if (userData == null)
                    return;

                if (_requestSequence != requestId)
                    return;

                MergePairs(_aliasesList, userData.Aliases);
                MergePairs(_tagsList, userData.Tags);
                MergeUnique(_emailsList, userData.Emails);
                MergeUnique(_smsNumbersList, userData.SmsNumbers);

                if (!string.IsNullOrEmpty(userData.ExternalId))
                {
                    _externalUserId = userData.ExternalId;
                    _prefs.ExternalUserId = userData.ExternalId;
                }

                var rawPushId = OneSignal.User.PushSubscription.Id ?? "";
                _pushSubscriptionId = IsE2EMode ? MaskValue(rawPushId) : rawPushId;
                _pushOptedIn = OneSignal.User.PushSubscription.OptedIn;
                _hasPermission = OneSignal.Notifications.Permission;

                Debug.Log($"[{Tag}] User data fetched from API");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{Tag}] Fetch user error: {ex.Message}");
            }
            finally
            {
                if (_requestSequence == requestId)
                    SetLoading(false);
                NotifyStateChanged();
            }
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

        private static void UpsertInList(
            List<KeyValuePair<string, string>> list,
            string key,
            string value
        )
        {
            var index = list.FindIndex(kvp => kvp.Key == key);
            if (index >= 0)
                list[index] = new KeyValuePair<string, string>(key, value);
            else
                list.Add(new KeyValuePair<string, string>(key, value));
        }

        private static void MergePairs(
            List<KeyValuePair<string, string>> target,
            IDictionary<string, string> source
        )
        {
            foreach (var kv in source)
            {
                var idx = target.FindIndex(p => p.Key == kv.Key);
                if (idx >= 0)
                {
                    if (!string.Equals(target[idx].Value, kv.Value, StringComparison.Ordinal))
                        target[idx] = new KeyValuePair<string, string>(kv.Key, kv.Value);
                }
                else
                {
                    target.Add(new KeyValuePair<string, string>(kv.Key, kv.Value));
                }
            }
        }

        private static void MergeUnique(List<string> target, IEnumerable<string> source)
        {
            foreach (var item in source)
            {
                if (!target.Contains(item))
                    target.Add(item);
            }
        }

        private void OnPushSubscriptionChanged(object sender, PushSubscriptionChangedEventArgs e)
        {
            var rawPushId = OneSignal.User.PushSubscription.Id ?? "";
            _pushSubscriptionId = IsE2EMode ? MaskValue(rawPushId) : rawPushId;
            _pushOptedIn = OneSignal.User.PushSubscription.OptedIn;
            Debug.Log($"[{Tag}] Push subscription changed: {rawPushId}");
            NotifyStateChanged();
        }

        private void OnPermissionChanged(object sender, NotificationPermissionChangedEventArgs e)
        {
            _hasPermission = e.Permission;
            Debug.Log($"[{Tag}] Permission changed: {e.Permission}");
            NotifyStateChanged();
        }

        private async void OnUserChanged(object sender, UserStateChangedEventArgs e)
        {
            Debug.Log($"[{Tag}] User changed, fetching data...");
            await FetchUserDataFromApi();
        }
    }
}
