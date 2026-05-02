// #region agent log
// Temporary instrumentation helper for the d01c6d debug session.
// Sends NDJSON entries via HTTP POST to the host's debug ingestion server.
// iOS Simulator and Android emulator both share the host network stack,
// so 127.0.0.1 resolves to the host machine. Real devices would need the
// host's LAN IP. Fire-and-forget; never throws into caller.
#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OneSignalDemo.Services
{
    public static class E2EDebugLog
    {
        private const string Endpoint =
            "http://127.0.0.1:7742/ingest/4a8fa49e-f409-48a0-b447-5877c2b74311";
        private const string SessionId = "d01c6d";

        private static readonly HttpClient _client = CreateClient();

        private static HttpClient CreateClient()
        {
            var c = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            c.DefaultRequestHeaders.Add("X-Debug-Session-Id", SessionId);
            return c;
        }

        public static void Send(string location, string message, string dataJson = "null")
        {
            string body =
                "{\"sessionId\":\"" + SessionId + "\","
                + "\"location\":\"" + Escape(location) + "\","
                + "\"message\":\"" + Escape(message) + "\","
                + "\"data\":" + (string.IsNullOrEmpty(dataJson) ? "null" : dataJson) + ","
                + "\"timestamp\":" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}";

            _ = Task.Run(async () =>
            {
                try
                {
                    using var content = new StringContent(body, Encoding.UTF8, "application/json");
                    await _client.PostAsync(Endpoint, content);
                }
                catch
                {
                    // best-effort
                }
            });
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
        }
    }
}
#endif
// #endregion
