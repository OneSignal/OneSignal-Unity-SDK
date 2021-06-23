using UnityEngine;
using UnityEditor;

static class Skin
{
    public static Texture2D SettingsWindowIcon =>
        AssetDatabase.LoadAssetAtPath(IconsPath, typeof(Texture2D)) as Texture2D;

    private static string IconsPath => $"Packages/{ScopeRegistriesConfig.OneSignalScope}.core/Editor/Icons/"
                                       + (EditorGUIUtility.isProSkin
                                           ? "icon_pro.png"
                                           : "icon_default.png");
}