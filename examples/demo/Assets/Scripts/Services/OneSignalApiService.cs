using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OneSignalDemo.Models;
using UnityEngine.Networking;

namespace OneSignalDemo.Services
{
    public class OneSignalApiService
    {
        private string _appId;

        private const string NotificationImageUrl =
            "https://media.onesignal.com/automated_push_templates/ratings_template.png";

        private const string PlaceholderApiKey = "your_rest_api_key";

        public void SetAppId(string appId) => _appId = appId;

        public string GetAppId() => _appId;

        public bool HasApiKey()
        {
            var key = DotEnv.Get("ONESIGNAL_API_KEY");
            return !string.IsNullOrWhiteSpace(key) && key != PlaceholderApiKey;
        }

        private static string GetApiKey() => DotEnv.Get("ONESIGNAL_API_KEY");

        public async Task<bool> SendNotification(NotificationType type, string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId) || string.IsNullOrEmpty(_appId))
                return false;

            string title,
                body;
            JObject extra = null;

            switch (type)
            {
                case NotificationType.Simple:
                    title = "Simple Notification";
                    body = "This is a simple push notification";
                    break;
                case NotificationType.WithImage:
                    title = "Image Notification";
                    body = "This notification includes an image";
                    extra = new JObject
                    {
                        ["big_picture"] = NotificationImageUrl,
                        ["ios_attachments"] = new JObject { ["image"] = NotificationImageUrl },
                        ["mutable_content"] = true,
                    };
                    break;
                case NotificationType.WithSound:
                    title = "Sound Notification";
                    body = "This notification plays a custom sound";
                    extra = new JObject
                    {
                        ["ios_sound"] = "vine_boom.wav",
                        ["android_channel_id"] = "b3b015d9-c050-4042-8548-dcc34aa44aa4",
                    };
                    break;
                default:
                    return false;
            }

            var payload = BuildBasePayload(title, body, subscriptionId);
            if (extra != null)
            {
                foreach (var prop in extra.Properties())
                    payload[prop.Name] = prop.Value;
            }

            return await PostNotification(payload.ToString());
        }

        public async Task<bool> SendCustomNotification(
            string title,
            string body,
            string subscriptionId
        )
        {
            if (string.IsNullOrEmpty(subscriptionId) || string.IsNullOrEmpty(_appId))
                return false;

            var payload = BuildBasePayload(title, body, subscriptionId);
            return await PostNotification(payload.ToString());
        }

        public async Task<UserData> FetchUser(string onesignalId)
        {
            if (string.IsNullOrEmpty(onesignalId) || string.IsNullOrEmpty(_appId))
                return null;

            var url =
                $"https://api.onesignal.com/apps/{_appId}/users/by/onesignal_id/{onesignalId}";
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Accept", "application/json");

            var tcs = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();
            operation.completed += _ => tcs.TrySetResult(true);
            await tcs.Task;

            UserData result = null;
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    result = UserData.FromJson(request.downloadHandler.text);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Error parsing user data: {ex.Message}");
                }
            }

            request.Dispose();
            return result;
        }

        public async Task<bool> UpdateLiveActivity(
            string activityId,
            string eventType,
            JObject eventUpdates = null
        )
        {
            if (string.IsNullOrEmpty(activityId) || string.IsNullOrEmpty(_appId) || !HasApiKey())
                return false;

            var encodedId = Uri.EscapeDataString(activityId);
            var url =
                $"https://api.onesignal.com/apps/{_appId}/live_activities/{encodedId}/notifications";

            var payload = new JObject
            {
                ["event"] = eventType,
                ["name"] = "Unity Demo Update",
                ["priority"] = 10,
            };

            if (eventUpdates != null)
                payload["event_updates"] = eventUpdates;

            if (eventType == "end")
                payload["dismissal_date"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload.ToString()));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Key {GetApiKey()}");

            var tcs = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();
            operation.completed += _ => tcs.TrySetResult(true);
            await tcs.Task;

            bool success = request.responseCode >= 200 && request.responseCode < 300;
            request.Dispose();
            return success;
        }

        private JObject BuildBasePayload(string title, string body, string subscriptionId) =>
            new()
            {
                ["app_id"] = _appId,
                ["include_subscription_ids"] = new JArray(subscriptionId),
                ["headings"] = new JObject { ["en"] = title },
                ["contents"] = new JObject { ["en"] = body },
            };

        private async Task<bool> PostNotification(string jsonPayload)
        {
            var request = new UnityWebRequest("https://onesignal.com/api/v1/notifications", "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/vnd.onesignal.v1+json");

            var tcs = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();
            operation.completed += _ => tcs.TrySetResult(true);
            await tcs.Task;

            bool success = request.result == UnityWebRequest.Result.Success;
            request.Dispose();
            return success;
        }
    }
}
