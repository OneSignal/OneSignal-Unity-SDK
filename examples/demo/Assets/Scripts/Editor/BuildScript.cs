using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    private const string OutputDir = "Build/Android";
    private const string ApkName = "onesignal-demo.apk";

    /// <summary>
    /// Fast build targeting x86_64 Android emulators with Mono scripting backend.
    /// Invoked by build_android.sh via -executeMethod BuildScript.BuildAndroidEmulator
    /// </summary>
    public static void BuildAndroidEmulator()
    {
        var outputPath = Path.Combine(OutputDir, ApkName);
        Directory.CreateDirectory(OutputDir);

        Debug.Log($"[BuildScript] targetArchitectures before: {PlayerSettings.Android.targetArchitectures}");
        Debug.Log($"[BuildScript] scriptingBackend before: {PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)}");

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        var options = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = outputPath,
            target = BuildTarget.Android,
            subtarget = 0,
            options = BuildOptions.Development | BuildOptions.AllowDebugging,
        };

        Debug.Log($"[BuildScript] Starting build → {outputPath}");
        var report = BuildPipeline.BuildPlayer(options);
        HandleReport(report, outputPath);
    }

    private static string[] GetScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        if (scenes.Length == 0)
        {
            Debug.LogWarning("[BuildScript] No scenes in build settings; falling back to Assets/Scenes/Main.unity");
            return new[] { "Assets/Scenes/Main.unity" };
        }
        return Array.ConvertAll(scenes, s => s.path);
    }

    private static void HandleReport(BuildReport report, string outputPath)
    {
        if (report.summary.result == BuildResult.Succeeded)
        {
            var sizeMb = report.summary.totalSize / 1024.0 / 1024.0;
            Debug.Log($"[BuildScript] Build succeeded: {outputPath} ({sizeMb:F1} MB) in {report.summary.totalTime.TotalSeconds:F1}s");
        }
        else
        {
            Debug.LogError($"[BuildScript] Build failed: {report.summary.result} — {report.summary.totalErrors} error(s)");
            EditorApplication.Exit(1);
        }
    }
}
