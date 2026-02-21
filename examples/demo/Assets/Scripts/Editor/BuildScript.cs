using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    private const string DefaultBundleId = "com.onesignal.example";

    [MenuItem("Build/Android APK")]
    public static void BuildAndroid()
    {
        SceneSetup.SetupScenes();

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, DefaultBundleId);
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
        PlayerSettings.productName = "OneSignal Demo";

        bool devBuild = HasArg("-devBuild");
        if (devBuild)
        {
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.Mono2x);
            EditorUserBuildSettings.connectProfiler = false;
            EditorUserBuildSettings.allowDebugging = false;
            Debug.Log("Dev build: Mono + Development");
        }
        else
        {
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            Debug.Log("Release build: IL2CPP");
        }

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
        Debug.Log($"Android target architectures: {PlayerSettings.Android.targetArchitectures}");

        var outputPath = GetArg("-outputPath") ?? "Build/OneSignalDemo.apk";

        var buildOptions = BuildOptions.None;
        if (devBuild)
            buildOptions = BuildOptions.Development | BuildOptions.CompressWithLz4;

        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = buildOptions
        };

        var report = BuildPipeline.BuildPlayer(options);
        HandleReport(report);
    }

    private static void HandleReport(BuildReport report)
    {
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

    private static bool HasArg(string name) =>
        Array.Exists(Environment.GetCommandLineArgs(), a => a == name);
}
