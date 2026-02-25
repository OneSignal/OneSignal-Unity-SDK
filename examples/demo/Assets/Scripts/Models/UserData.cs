using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace OneSignalDemo.Models
{
    [Serializable]
    public class UserData
    {
        public Dictionary<string, string> Aliases { get; }
        public Dictionary<string, string> Tags { get; }
        public List<string> Emails { get; }
        public List<string> SmsNumbers { get; }
        public string ExternalId { get; }

        public UserData(
            Dictionary<string, string> aliases,
            Dictionary<string, string> tags,
            List<string> emails,
            List<string> smsNumbers,
            string externalId = null
        )
        {
            Aliases = aliases ?? new Dictionary<string, string>();
            Tags = tags ?? new Dictionary<string, string>();
            Emails = emails ?? new List<string>();
            SmsNumbers = smsNumbers ?? new List<string>();
            ExternalId = externalId;
        }

        private static readonly HashSet<string> FilteredAliasKeys = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            "external_id",
            "onesignal_id",
        };

        public static UserData FromJson(string json)
        {
            var root = JObject.Parse(json);

            var identity = root["identity"] as JObject;
            var properties = root["properties"] as JObject;
            var subscriptions = root["subscriptions"] as JArray;

            string externalId = identity?["external_id"]?.ToString();

            var aliases = new Dictionary<string, string>();
            if (identity != null)
            {
                foreach (var prop in identity.Properties())
                {
                    if (!FilteredAliasKeys.Contains(prop.Name))
                        aliases[prop.Name] = prop.Value.ToString();
                }
            }

            var tags = new Dictionary<string, string>();
            var tagsObj = properties?["tags"] as JObject;
            if (tagsObj != null)
            {
                foreach (var prop in tagsObj.Properties())
                    tags[prop.Name] = prop.Value.ToString();
            }

            var emails = new List<string>();
            var smsNumbers = new List<string>();
            if (subscriptions != null)
            {
                foreach (var sub in subscriptions)
                {
                    var type = sub["type"]?.ToString();
                    var token = sub["token"]?.ToString();
                    if (string.IsNullOrEmpty(token))
                        continue;

                    if (string.Equals(type, "Email", StringComparison.OrdinalIgnoreCase))
                        emails.Add(token);
                    else if (string.Equals(type, "SMS", StringComparison.OrdinalIgnoreCase))
                        smsNumbers.Add(token);
                }
            }

            return new UserData(aliases, tags, emails, smsNumbers, externalId);
        }
    }
}
