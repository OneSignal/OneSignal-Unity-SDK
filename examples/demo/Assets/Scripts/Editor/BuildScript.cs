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
    private const string AabName = "onesignal-demo.aab";

    /// <summary>
    /// Builds an Android APK using the architecture and scripting backend
    /// pre-configured in ProjectSettings.asset by build_android.sh.
    ///
    /// In Unity 6, only ARMv7 supports Mono; ARM64 requires IL2CPP.
    /// The shell script patches the correct values before launching Unity.
    /// </summary>
    public static void BuildAndroidEmulator()
    {
        var outputPath = Path.Combine(OutputDir, ApkName);
        Directory.CreateDirectory(OutputDir);

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(
            NamedBuildTarget.Android,
            ScriptingImplementation.IL2CPP
        );

        Debug.Log(
            $"[BuildScript] arch={PlayerSettings.Android.targetArchitectures} backend={PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)}"
        );

        PlayerSettings.SetIl2CppCodeGeneration(
            NamedBuildTarget.Android,
            Il2CppCodeGeneration.OptimizeSize
        );
        PlayerSettings.SetManagedStrippingLevel(
            NamedBuildTarget.Android,
            ManagedStrippingLevel.High
        );
        PlayerSettings.Android.minifyRelease = true;
        PlayerSettings.Android.minifyWithR8 = true;
        PlayerSettings.stripEngineCode = true;

        PlayerSettings.SetIl2CppCompilerConfiguration(
            NamedBuildTarget.Android,
            Il2CppCompilerConfiguration.Release
        );
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        EditorUserBuildSettings.buildAppBundle = false;

        var options = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = outputPath,
            target = BuildTarget.Android,
            subtarget = 0,
            options = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);
        HandleReport(report, outputPath);
    }

    public static void BuildAndroidAAB()
    {
        var outputPath = Path.Combine(OutputDir, AabName);
        Directory.CreateDirectory(OutputDir);

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(
            NamedBuildTarget.Android,
            ScriptingImplementation.IL2CPP
        );

        Debug.Log(
            $"[BuildScript] arch={PlayerSettings.Android.targetArchitectures} backend={PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)}"
        );

        PlayerSettings.SetIl2CppCodeGeneration(
            NamedBuildTarget.Android,
            Il2CppCodeGeneration.OptimizeSize
        );
        PlayerSettings.SetManagedStrippingLevel(
            NamedBuildTarget.Android,
            ManagedStrippingLevel.High
        );
        PlayerSettings.Android.minifyRelease = true;
        PlayerSettings.Android.minifyWithR8 = true;
        PlayerSettings.stripEngineCode = true;

        PlayerSettings.SetIl2CppCompilerConfiguration(
            NamedBuildTarget.Android,
            Il2CppCompilerConfiguration.Release
        );
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        EditorUserBuildSettings.buildAppBundle = true;

        var options = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = outputPath,
            target = BuildTarget.Android,
            subtarget = 0,
            options = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);
        HandleReport(report, outputPath);
    }

    private static string[] GetScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        if (scenes.Length == 0)
        {
            Debug.LogWarning(
                "[BuildScript] No scenes in build settings; falling back to Assets/Scenes/Main.unity"
            );
            return new[] { "Assets/Scenes/Main.unity" };
        }
        return Array.ConvertAll(scenes, s => s.path);
    }

    private static void HandleReport(BuildReport report, string outputPath)
    {
        if (report.summary.result == BuildResult.Succeeded)
        {
            var sizeMb = report.summary.totalSize / 1024.0 / 1024.0;
            Debug.Log(
                $"[BuildScript] Build succeeded: {outputPath} ({sizeMb:F1} MB) in {report.summary.totalTime.TotalSeconds:F1}s"
            );
        }
        else
        {
            Debug.LogError(
                $"[BuildScript] Build failed: {report.summary.result} — {report.summary.totalErrors} error(s)"
            );
            EditorApplication.Exit(1);
        }
    }
}
