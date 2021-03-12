using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace StansAssets.AMM
{
    public static class Skin
    {
        static readonly Dictionary<string, Texture2D> s_Icons = new Dictionary<string, Texture2D>();
        
        public static string IconsPath => "Packages/com.onesignal-test.unity.core/Editor/Icons/" 
                                        + (EditorGUIUtility.isProSkin 
                                               ? "icon_pro.png" 
                                               : "icon_default.png");

        public static Texture2D SettingsWindowIcon => GetTextureAtPath(IconsPath);
        
        public static Texture2D GetTextureAtPath(string path)
        {
            var tx = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
            
            if (s_Icons.ContainsKey(path))
            {
                return s_Icons[path];
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) {
                return new Texture2D(0, 0);
            }

            var importRequired = false;

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                importRequired = true;
            }

            if (importer.alphaIsTransparency != true)
            {
                importer.alphaIsTransparency = true;
                importRequired = true;
            }

            if (importer.wrapMode != TextureWrapMode.Clamp)
            {
                importer.wrapMode = TextureWrapMode.Clamp;
                importRequired = true;
            }

            if (importer.textureType != TextureImporterType.GUI)
            {
                importer.textureType = TextureImporterType.GUI;
                importRequired = true;
            }

            if (importer.npotScale != TextureImporterNPOTScale.None)
            {
                importer.npotScale = TextureImporterNPOTScale.None;
                importRequired = true;
            }

            if (importer.alphaSource != TextureImporterAlphaSource.FromInput)
            {
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importRequired = true;
            }

            //Should we make additional option for this?
            if (importer.isReadable != true)
            {
                importer.isReadable = true;
                importRequired = true;
            }

            var settings = importer.GetPlatformTextureSettings(EditorUserBuildSettings.activeBuildTarget.ToString());
            if (settings.overridden)
            {
                settings.overridden = false;
                importer.SetPlatformTextureSettings(settings);
            }

            settings = importer.GetDefaultPlatformTextureSettings();
            if (!settings.textureCompression.Equals(TextureImporterCompression.Uncompressed))
            {
                settings.textureCompression = TextureImporterCompression.Uncompressed;
                importRequired = true;
            }

            if (importRequired) importer.SetPlatformTextureSettings(settings);

            var tex = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
            s_Icons.Add(path, tex);

            return GetTextureAtPath(path);
        }
    }
}
