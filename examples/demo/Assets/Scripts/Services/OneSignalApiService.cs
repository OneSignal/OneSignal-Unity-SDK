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

        private const string DefaultAndroidChannelId = "b3b015d9-c050-4042-8548-dcc34aa44aa4";

        public void SetAppId(string appId) => _appId = appId;

        public string GetAppId() => _appId;

        public bool HasApiKey()
        {
            var key = DotEnv.Get("ONESIGNAL_API_KEY");
            return !string.IsNullOrWhiteSpace(key) && key != PlaceholderApiKey;
        }

        private static string GetApiKey() => DotEnv.Get("ONESIGNAL_API_KEY");

        private static string GetAndroidChannelId()
        {
            var value = DotEnv.Get("ONESIGNAL_ANDROID_CHANNEL_ID")?.Trim();
            return string.IsNullOrEmpty(value) ? DefaultAndroidChannelId : value;
        }

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
                        ["android_channel_id"] = GetAndroidChannelId(),
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

        // Retry while the OneSignal backend hasn't yet indexed the freshly
        // created subscription. The /notifications endpoint reports this race
        // in a few different shapes, all of which return HTTP 200:
        //   {"id":"...","recipients":0}                       (user just switched, push token not yet attached)
        //   {"id":"...","errors":{"invalid_player_ids":[...]}}
        //   {"id":"","errors":["All included players are not subscribed"]}
        //   {"id":"","errors":[...]}
        // Treat any 200 response with no real id, populated errors, or recipients=0 as transient.
        private async Task<bool> PostNotification(string jsonPayload)
        {
            const int maxAttempts = 3;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var request = new UnityWebRequest(
                    "https://onesignal.com/api/v1/notifications",
                    "POST"
                );
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/vnd.onesignal.v1+json");

                var tcs = new TaskCompletionSource<bool>();
                var operation = request.SendWebRequest();
                operation.completed += _ => tcs.TrySetResult(true);
                await tcs.Task;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    request.Dispose();
                    return false;
                }

                var responseText = request.downloadHandler.text;
                request.Dispose();

                if (IsTransientSendFailure(responseText))
                {
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(3000 * attempt);
                        continue;
                    }
                    return false;
                }

                return true;
            }

            return false;
        }

        private static bool IsTransientSendFailure(string responseJson)
        {
            if (string.IsNullOrWhiteSpace(responseJson))
                return false;
            try
            {
                var parsed = JObject.Parse(responseJson);
                var id = parsed["id"]?.ToString();
                var errors = parsed["errors"];
                bool hasErrors =
                    (errors is JArray arr && arr.Count > 0) ||
                    (errors is JObject obj && obj.Count > 0);
                bool missingId = string.IsNullOrEmpty(id);
                var recipientsToken = parsed["recipients"];
                bool zeroRecipients =
                    recipientsToken != null
                    && recipientsToken.Type == JTokenType.Integer
                    && recipientsToken.Value<long>() == 0;
                return hasErrors || missingId || zeroRecipients;
            }
            catch
            {
                return false;
            }
        }
    }
}
