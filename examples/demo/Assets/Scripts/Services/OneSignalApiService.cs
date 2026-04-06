using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OneSignalDemo.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace OneSignalDemo.Services
{
    public class OneSignalApiService
    {
        private string _appId;
        private string _apiKey;

        private const string NotificationImageUrl =
            "https://media.onesignal.com/automated_push_templates/ratings_template.png";

        private const string PlaceholderApiKey = "your_rest_api_key";

        public void SetAppId(string appId) => _appId = appId;

        public string GetAppId() => _appId;

        public void LoadApiKey()
        {
            var envPath = Path.Combine(Application.dataPath, "..", ".env");
            if (!File.Exists(envPath))
                return;

            foreach (var line in File.ReadAllLines(envPath))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("#") || !trimmed.Contains("="))
                    continue;

                var eqIndex = trimmed.IndexOf('=');
                var key = trimmed.Substring(0, eqIndex).Trim();
                var value = trimmed.Substring(eqIndex + 1).Trim();

                if (key == "ONESIGNAL_API_KEY")
                    _apiKey = value;
            }
        }

        public bool HasApiKey() =>
            !string.IsNullOrEmpty(_apiKey) && _apiKey != PlaceholderApiKey;

        public async Task<bool> SendNotification(NotificationType type, string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId) || string.IsNullOrEmpty(_appId))
                return false;

            string title,
                body;
            switch (type)
            {
                case NotificationType.Simple:
                    title = "Simple Notification";
                    body = "This is a simple push notification";
                    break;
                case NotificationType.WithImage:
                    title = "Image Notification";
                    body = "This notification includes an image";
                    break;
                default:
                    return false;
            }

            var payload = new JObject
            {
                ["app_id"] = _appId,
                ["include_subscription_ids"] = new JArray(subscriptionId),
                ["headings"] = new JObject { ["en"] = title },
                ["contents"] = new JObject { ["en"] = body },
            };

            if (type == NotificationType.WithImage)
            {
                payload["big_picture"] = NotificationImageUrl;
                payload["ios_attachments"] = new JObject { ["image"] = NotificationImageUrl };
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

            var payload = new JObject
            {
                ["app_id"] = _appId,
                ["include_subscription_ids"] = new JArray(subscriptionId),
                ["headings"] = new JObject { ["en"] = title },
                ["contents"] = new JObject { ["en"] = body },
            };

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

            var url =
                $"https://api.onesignal.com/apps/{_appId}/live_activities/{activityId}/notifications";

            var payload = new JObject
            {
                ["event"] = eventType,
                ["name"] = "Unity Demo Update",
                ["priority"] = 10,
            };

            if (eventUpdates != null)
                payload["event_updates"] = eventUpdates;

            if (eventType == "end")
            {
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                payload["dismissal_date"] = unixTimestamp;
            }

            var jsonPayload = payload.ToString();
            var request = new UnityWebRequest(url, "POST");
            var bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Key {_apiKey}");

            var tcs = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();
            operation.completed += _ => tcs.TrySetResult(true);
            await tcs.Task;

            bool success = request.responseCode >= 200 && request.responseCode < 300;
            request.Dispose();
            return success;
        }

        private async Task<bool> PostNotification(string jsonPayload)
        {
            var request = new UnityWebRequest("https://onesignal.com/api/v1/notifications", "POST");
            var bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
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
