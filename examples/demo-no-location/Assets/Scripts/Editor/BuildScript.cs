using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildScript
{
    private const string AndroidOutputDir = "Build/Android";
    private const string ApkName = "onesignal-demo-no-location.apk";
    private const string IOSOutputDir = "Build/iOS";
    private const string AppIdEnvironmentVariable = "ONESIGNAL_APP_ID";
    private const string MainScenePath = "Assets/Scenes/Main.unity";
    private const string OneSignalAppIdPropertyName = "_oneSignalAppId";
    private const string NoLocationDemoTypeName = "NoLocationDemo";

    public static void BuildAndroidEmulator()
    {
        EnforceNoLocationBuild();

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

        BuildReport report;
        using (ApplyConfiguredAppId())
            report = BuildPipeline.BuildPlayer(
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
        EnforceNoLocationBuild();

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

        BuildReport report;
        using (ApplyConfiguredAppId())
            report = BuildPipeline.BuildPlayer(
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

    private static void EnforceNoLocationBuild()
    {
        // Scripted builds should stay no-location even if the Editor menu setting was toggled.
        OneSignalSDK.OneSignalSDKSettings.DisableLocation = true;
    }

    private static IDisposable ApplyConfiguredAppId()
    {
        var appId = GetConfiguredAppId();
        if (string.IsNullOrWhiteSpace(appId))
            return AppIdOverride.Noop;

        var scenePath = GetMainScenePath();
        if (!File.Exists(scenePath))
        {
            Debug.LogWarning($"[BuildScript] Could not find scene at {scenePath}");
            return AppIdOverride.Noop;
        }

        var originalSceneContents = File.ReadAllText(scenePath);
        var scene = EditorSceneManager.OpenScene(scenePath);

        foreach (var rootGameObject in scene.GetRootGameObjects())
        {
            foreach (var component in rootGameObject.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null || component.GetType().Name != NoLocationDemoTypeName)
                    continue;

                var serializedObject = new SerializedObject(component);
                var appIdProperty = serializedObject.FindProperty(OneSignalAppIdPropertyName);
                if (appIdProperty == null)
                    continue;

                appIdProperty.stringValue = appId;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(component);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log(
                    $"[BuildScript] Applied {AppIdEnvironmentVariable} from environment or .env for this build."
                );
                return new AppIdOverride(scenePath, originalSceneContents);
            }
        }

        Debug.LogWarning($"[BuildScript] Could not find {NoLocationDemoTypeName} in {scenePath}");
        return new AppIdOverride(scenePath, originalSceneContents);
    }

    private static string GetConfiguredAppId()
    {
        var appId = Environment.GetEnvironmentVariable(AppIdEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(appId))
            return Unquote(appId.Trim());

        var envPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ".env");
        if (!File.Exists(envPath))
            return null;

        foreach (var line in File.ReadAllLines(envPath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                continue;

            if (trimmed.StartsWith("export "))
                trimmed = trimmed.Substring("export ".Length).TrimStart();

            var separator = trimmed.IndexOf('=');
            if (separator <= 0)
                continue;

            var key = trimmed.Substring(0, separator).Trim();
            if (key != AppIdEnvironmentVariable)
                continue;

            return Unquote(trimmed.Substring(separator + 1).Trim());
        }

        return null;
    }

    private static string GetMainScenePath()
    {
        var scenes = GetScenes();
        foreach (var scenePath in scenes)
        {
            if (scenePath == MainScenePath)
                return scenePath;
        }

        return scenes.Length > 0 ? scenes[0] : MainScenePath;
    }

    private static string Unquote(string value)
    {
        if (value.Length < 2)
            return value;

        var first = value[0];
        var last = value[value.Length - 1];
        if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
            return value.Substring(1, value.Length - 2);

        return value;
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

    private sealed class AppIdOverride : IDisposable
    {
        internal static readonly AppIdOverride Noop = new AppIdOverride(null, null);

        private readonly string _scenePath;
        private readonly string _originalSceneContents;

        internal AppIdOverride(string scenePath, string originalSceneContents)
        {
            _scenePath = scenePath;
            _originalSceneContents = originalSceneContents;
        }

        public void Dispose()
        {
            if (string.IsNullOrEmpty(_scenePath) || _originalSceneContents == null)
                return;

            File.WriteAllText(_scenePath, _originalSceneContents);
            AssetDatabase.ImportAsset(_scenePath);
        }
    }
}
