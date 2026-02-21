using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    private const string DefaultBundleId = "com.onesignal.example";

    [MenuItem("Build/Android APK")]
    public static void BuildAndroid()
    {
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, DefaultBundleId);
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;

        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogWarning("No scenes in build settings, searching for scenes...");
            scenes = System.IO.Directory.GetFiles("Assets", "*.unity", System.IO.SearchOption.AllDirectories);
        }

        var outputPath = GetArg("-outputPath") ?? "Build/OneSignalDemo.apk";

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"Build failed: {report.summary.totalErrors} error(s)");
            EditorApplication.Exit(1);
        }
        else
        {
            Debug.Log($"Build succeeded: {report.summary.outputPath}");
            EditorApplication.Exit(0);
        }
    }

    private static string GetArg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }
        return null;
    }
}
