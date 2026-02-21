using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class SceneSetup
{
    [MenuItem("Build/Setup Scenes")]
    public static void SetupScenes()
    {
        CreateMainScene();
        CreateSecondaryScene();
        SetBuildScenes();
        Debug.Log("Scenes created and configured.");
    }

    private static void CreateMainScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cameraGo = new GameObject("Main Camera");
        var camera = cameraGo.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.97f, 0.97f, 0.98f);
        camera.orthographic = true;
        cameraGo.tag = "MainCamera";

        var managerGo = new GameObject("AppManager");
        var bootstrapper = managerGo.AddComponent<OneSignalDemo.AppBootstrapper>();
        var viewModel = managerGo.AddComponent<OneSignalDemo.ViewModels.AppViewModel>();

        var bootstrapperSo = new SerializedObject(bootstrapper);
        bootstrapperSo.FindProperty("_viewModel").objectReferenceValue = viewModel;
        bootstrapperSo.ApplyModifiedProperties();

        var uiGo = new GameObject("UIDocument");
        var uiDoc = uiGo.AddComponent<UIDocument>();
        uiDoc.panelSettings = GetOrCreatePanelSettings();
        var homeController = uiGo.AddComponent<OneSignalDemo.UI.HomeScreenController>();

        var homeSo = new SerializedObject(homeController);
        homeSo.FindProperty("_uiDocument").objectReferenceValue = uiDoc;
        homeSo.FindProperty("_viewModel").objectReferenceValue = viewModel;
        homeSo.ApplyModifiedProperties();

        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
    }

    private static void CreateSecondaryScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cameraGo = new GameObject("Main Camera");
        var camera = cameraGo.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.97f, 0.97f, 0.98f);
        camera.orthographic = true;
        cameraGo.tag = "MainCamera";

        var uiGo = new GameObject("UIDocument");
        var uiDoc = uiGo.AddComponent<UIDocument>();
        uiDoc.panelSettings = GetOrCreatePanelSettings();
        var secondaryController = uiGo.AddComponent<OneSignalDemo.UI.SecondaryScreenController>();

        var secSo = new SerializedObject(secondaryController);
        secSo.FindProperty("_uiDocument").objectReferenceValue = uiDoc;
        secSo.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Secondary.unity");
    }

    private static PanelSettings GetOrCreatePanelSettings()
    {
        var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI/PanelSettings.asset");
        if (existing != null)
        {
            existing.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            existing.referenceResolution = new Vector2Int(412, 892);
            existing.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            existing.match = 0f;
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            return existing;
        }

        var settings = ScriptableObject.CreateInstance<PanelSettings>();
        settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        settings.referenceResolution = new Vector2Int(412, 892);
        settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        settings.match = 0f;

        System.IO.Directory.CreateDirectory("Assets/UI");
        AssetDatabase.CreateAsset(settings, "Assets/UI/PanelSettings.asset");
        AssetDatabase.SaveAssets();
        return settings;
    }

    private static void SetBuildScenes()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Secondary.unity", true),
        };
    }
}
