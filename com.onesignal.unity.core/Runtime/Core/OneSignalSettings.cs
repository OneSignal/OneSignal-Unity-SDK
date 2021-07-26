using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// OneSignal Project Config.
/// </summary>
public class OneSignalSettings : ScriptableObject
{
    static OneSignalSettings s_Instance;

    /// <summary>
    /// OneSignal Application Id.
    /// </summary>
    public string ApplicationId;

    /// <summary>
    /// Project folder related settings ScriptableObject location path
    /// </summary>
    public static readonly string SettingsLocation = Path.Combine("Assets", "Plugins", "OneSignal", "Resources");

    internal const string ProductName = "OneSignal";

    /// <summary>
    /// Returns a singleton class instance.
    /// If current instance is not assigned it will try to find an object of the instance type,
    /// in case instance already exists in a project. If not, new instance will be created,
    /// and saved under the <see cref="SettingsLocation"/>.
    /// </summary>
    public static OneSignalSettings Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = Resources.Load<OneSignalSettings>(nameof(OneSignalSettings));
                if (s_Instance == null)
                {
                    s_Instance = CreateInstance<OneSignalSettings>();
                    s_Instance.SaveToAssetDatabase();
                }
            }

            return s_Instance;
        }
    }

#if UNITY_EDITOR
    internal static void Save() => EditorUtility.SetDirty(Instance);
#endif

    void SaveToAssetDatabase()
    {
#if UNITY_EDITOR
        if (!Directory.Exists(SettingsLocation))
        {
            Directory.CreateDirectory(SettingsLocation);
        }

        AssetDatabase.CreateAsset(this, $"{SettingsLocation}/{nameof(OneSignalSettings)}.asset");
#endif
    }
}