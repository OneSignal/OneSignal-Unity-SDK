using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    private const string AndroidOutputDir = "Build/Android";
    private const string ApkName = "onesignal-demo-no-location.apk";
    private const string IOSOutputDir = "Build/iOS";

    public static void BuildAndroidEmulator()
    {
        var outputPath = Path.Combine(AndroidOutputDir, ApkName);
        Directory.CreateDirectory(AndroidOutputDir);

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(
            NamedBuildTarget.Android,
            ScriptingImplementation.IL2CPP
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

        var report = BuildPipeline.BuildPlayer(
            new BuildPlayerOptions
            {
                scenes = GetScenes(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                subtarget = 0,
                options = BuildOptions.None,
            }
        );
        HandleReport(report, outputPath);
    }

    public static void BuildiOSSimulator()
    {
        Directory.CreateDirectory(IOSOutputDir);

        PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
        PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.Universal;
        PlayerSettings.SetIl2CppCodeGeneration(
            NamedBuildTarget.iOS,
            Il2CppCodeGeneration.OptimizeSize
        );
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.iOS, ManagedStrippingLevel.High);
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.SetIl2CppCompilerConfiguration(
            NamedBuildTarget.iOS,
            Il2CppCompilerConfiguration.Release
        );

        var report = BuildPipeline.BuildPlayer(
            new BuildPlayerOptions
            {
                scenes = GetScenes(),
                locationPathName = IOSOutputDir,
                target = BuildTarget.iOS,
                options = BuildOptions.None,
            }
        );
        HandleReport(report, IOSOutputDir);
    }

    private static string[] GetScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        if (scenes.Length == 0)
            return new[] { "Assets/Scenes/Main.unity" };

        return Array.ConvertAll(scenes, scene => scene.path);
    }

    private static void HandleReport(BuildReport report, string outputPath)
    {
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] Build succeeded: {outputPath}");
            return;
        }

        Debug.LogError(
            $"[BuildScript] Build failed: {report.summary.result} - {report.summary.totalErrors} error(s)"
        );
        EditorApplication.Exit(1);
    }
}
