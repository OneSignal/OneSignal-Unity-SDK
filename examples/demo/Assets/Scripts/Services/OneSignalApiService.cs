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

        public void SetAppId(string appId) => _appId = appId;

        public string GetAppId() => _appId;

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
