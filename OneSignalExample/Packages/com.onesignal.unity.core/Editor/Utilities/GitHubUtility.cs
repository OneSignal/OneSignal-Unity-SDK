using System;
using System.Collections.Generic;
using Com.OneSignal.MiniJSON;
using UnityEngine.Networking;

namespace Com.OneSignal.Editor
{
    static class GitHubUtility
    {
        const string k_APIAccessPoint = "https://api.github.com";
        const string k_GitHubWebUrl = "https://github.com";

        public static string GetRawFileUrl(string owner, string repo, string tag, string relativeFilePath)
        {
            return $"{k_GitHubWebUrl}/{owner}/{repo}/blob/{tag}/{relativeFilePath}?raw=true";
        }

        public static void ListRepositoryTags(string owner, string repo, Action<List<GitHubTag>> callback)
        {
            var url = $"{k_APIAccessPoint}/repos/{owner}/{repo}/tags";

            var tagsList = new List<GitHubTag>();
            var rq = UnityWebRequest.Get(url);
            rq.SetRequestHeader("application", "vnd.github.v3+json");
            rq.SendWebRequest().completed += obj =>
            {
                var tags = (List<object>)Json.Deserialize(rq.downloadHandler.text);
                foreach (Dictionary<string, object> tag in tags)
                {
                    var gitHubTag = new GitHubTag
                    {
                        Name = (string)tag["name"],
                        ZipballUrl = (string)tag["zipball_url"],
                        TarballUrl = (string)tag["tarball_url"],
                        NodeId = (string)tag["node_id"]
                    };

                    tagsList.Add(gitHubTag);
                }

                callback.Invoke(tagsList);
            };
        }
    }
}
