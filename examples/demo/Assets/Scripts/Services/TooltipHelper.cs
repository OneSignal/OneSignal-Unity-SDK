using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace OneSignalDemo.Services
{
    public class TooltipData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<TooltipOption> Options { get; set; }
    }

    public class TooltipOption
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class TooltipHelper
    {
        private static readonly TooltipHelper _instance = new();
        public static TooltipHelper Instance => _instance;

        private readonly Dictionary<string, TooltipData> _tooltips = new();
        private bool _initialized;

        private const string TooltipUrl =
            "https://raw.githubusercontent.com/OneSignal/sdk-shared/main/demo/tooltip_content.json";

        private TooltipHelper() { }

        public async Task InitAsync()
        {
            if (_initialized) return;

            try
            {
                var request = UnityWebRequest.Get(TooltipUrl);
                var operation = request.SendWebRequest();
                var tcs = new TaskCompletionSource<bool>();
                operation.completed += _ => tcs.TrySetResult(true);
                await tcs.Task;

                if (request.result == UnityWebRequest.Result.Success)
                {
                    ParseTooltips(request.downloadHandler.text);
                }

                request.Dispose();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Failed to fetch tooltips: {ex.Message}");
            }

            _initialized = true;
        }

        public TooltipData GetTooltip(string key)
        {
            _tooltips.TryGetValue(key, out var data);
            return data;
        }

        private void ParseTooltips(string json)
        {
            var root = JObject.Parse(json);
            foreach (var prop in root.Properties())
            {
                var obj = prop.Value as JObject;
                if (obj == null) continue;

                var tooltip = new TooltipData
                {
                    Title = obj["title"]?.ToString(),
                    Description = obj["description"]?.ToString(),
                    Options = new List<TooltipOption>()
                };

                var options = obj["options"] as JArray;
                if (options != null)
                {
                    foreach (var opt in options)
                    {
                        tooltip.Options.Add(new TooltipOption
                        {
                            Name = opt["name"]?.ToString(),
                            Description = opt["description"]?.ToString()
                        });
                    }
                }

                _tooltips[prop.Name] = tooltip;
            }
        }
    }
}
