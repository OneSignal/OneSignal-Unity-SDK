using System.Collections.Generic;
using System.Threading.Tasks;
using OneSignalDemo.Models;
using OneSignalDemo.Services;
using OneSignalSDK;

namespace OneSignalDemo.Repositories
{
    public class OneSignalRepository
    {
        private readonly OneSignalApiService _apiService;

        public OneSignalRepository(OneSignalApiService apiService)
        {
            _apiService = apiService;
        }

        public void LoginUser(string externalUserId) => OneSignal.Login(externalUserId);

        public void LogoutUser() => OneSignal.Logout();

        public void AddAlias(string label, string id) => OneSignal.User.AddAlias(label, id);

        public void AddAliases(Dictionary<string, string> aliases) =>
            OneSignal.User.AddAliases(aliases);

        public void AddEmail(string email) => OneSignal.User.AddEmail(email);

        public void RemoveEmail(string email) => OneSignal.User.RemoveEmail(email);

        public void AddSms(string smsNumber) => OneSignal.User.AddSms(smsNumber);

        public void RemoveSms(string smsNumber) => OneSignal.User.RemoveSms(smsNumber);

        public void AddTag(string key, string value) => OneSignal.User.AddTag(key, value);

        public void AddTags(Dictionary<string, string> tags) => OneSignal.User.AddTags(tags);

        public void RemoveTag(string key) => OneSignal.User.RemoveTag(key);

        public void RemoveTags(List<string> keys) => OneSignal.User.RemoveTags(keys.ToArray());

        public Dictionary<string, string> GetTags() => OneSignal.User.GetTags();

        public void AddTrigger(string key, string value) =>
            OneSignal.InAppMessages.AddTrigger(key, value);

        public void AddTriggers(Dictionary<string, string> triggers) =>
            OneSignal.InAppMessages.AddTriggers(triggers);

        public void RemoveTrigger(string key) => OneSignal.InAppMessages.RemoveTrigger(key);

        public void RemoveTriggers(List<string> keys) =>
            OneSignal.InAppMessages.RemoveTriggers(keys.ToArray());

        public void ClearTriggers() => OneSignal.InAppMessages.ClearTriggers();

        public void SendOutcome(string name) => OneSignal.Session.AddOutcome(name);

        public void SendUniqueOutcome(string name) => OneSignal.Session.AddUniqueOutcome(name);

        public void SendOutcomeWithValue(string name, float value) =>
            OneSignal.Session.AddOutcomeWithValue(name, value);

        public string GetPushSubscriptionId() => OneSignal.User.PushSubscription.Id;

        public bool IsPushOptedIn() => OneSignal.User.PushSubscription.OptedIn;

        public void OptInPush() => OneSignal.User.PushSubscription.OptIn();

        public void OptOutPush() => OneSignal.User.PushSubscription.OptOut();

        public void ClearAllNotifications() => OneSignal.Notifications.ClearAllNotifications();

        public bool HasPermission() => OneSignal.Notifications.Permission;

        public Task<bool> RequestPermissionAsync(bool fallbackToSettings) =>
            OneSignal.Notifications.RequestPermissionAsync(fallbackToSettings);

        public void SetInAppMessagesPaused(bool paused) => OneSignal.InAppMessages.Paused = paused;

        public bool IsInAppMessagesPaused() => OneSignal.InAppMessages.Paused;

        public void SetLocationShared(bool shared) => OneSignal.Location.IsShared = shared;

        public bool IsLocationShared() => OneSignal.Location.IsShared;

        public void RequestLocationPermission() => OneSignal.Location.RequestPermission();

        public void SetConsentRequired(bool required) => OneSignal.ConsentRequired = required;

        public void SetConsentGiven(bool granted) => OneSignal.ConsentGiven = granted;

        public string GetExternalId() => OneSignal.User.ExternalId;

        public string GetOnesignalId() => OneSignal.User.OneSignalId;

        public void TrackEvent(string name, Dictionary<string, object> properties = null) =>
            OneSignal.User.TrackEvent(name, properties);

        public async Task<bool> SendNotification(NotificationType type)
        {
            var subId = GetPushSubscriptionId();
            return await _apiService.SendNotification(type, subId);
        }

        public async Task<bool> SendCustomNotification(string title, string body)
        {
            var subId = GetPushSubscriptionId();
            return await _apiService.SendCustomNotification(title, body, subId);
        }

        public async Task<UserData> FetchUser(string onesignalId) =>
            await _apiService.FetchUser(onesignalId);
    }
}
