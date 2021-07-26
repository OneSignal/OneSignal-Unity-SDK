using System;
using System.Collections.Generic;
using OneSignalPush.Utilities;
using UnityEngine.Networking;

static class GitHubUtility
{
    internal static void GetLatestRelease(string url, Action<string> callback)
    {
        var rq = UnityWebRequest.Get(GetReleaseInfoFromURL(url));
        rq.SendWebRequest().completed += obj =>
        {
            var jsonObject = (Dictionary<string, object>) Json.Deserialize(rq.downloadHandler.text);
            string releaseName = jsonObject.TryGetValue("name", out var name) ? (string) name : string.Empty;
            callback(releaseName);
        };
    }

    static string GetReleaseInfoFromURL(string repositoryURL)
    {
        if (repositoryURL.Contains("github.com"))
        {
            return repositoryURL.Replace(@".git", @"/releases/latest")
                .Replace(@"ssh://git@github.com:", @"https://api.github.com/repos/");
        }

        throw new InvalidOperationException($"The provided URL {repositoryURL} is not a GitHub repository URL.");
    }
}