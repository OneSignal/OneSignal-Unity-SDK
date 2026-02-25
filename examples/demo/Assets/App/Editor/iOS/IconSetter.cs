#if UNITY_IOS

using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace App.Editor.iOS
{
    public class IconSetter : IPostprocessBuildWithReport
    {
        private const string SourceDirectory = "Assets/AppIcons/iOS";

        public int callbackOrder => 46;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != UnityEditor.BuildTarget.iOS)
                return;

            var appiconsetPath = Path.Combine(
                report.summary.outputPath,
                "Unity-iPhone",
                "Images.xcassets",
                "AppIcon.appiconset"
            );

            if (!Directory.Exists(appiconsetPath))
            {
                Debug.LogWarning($"IconSetter: {appiconsetPath} not found");
                return;
            }

            var sourceDir = Path.Combine(Directory.GetCurrentDirectory(), SourceDirectory);
            if (!Directory.Exists(sourceDir))
            {
                Debug.LogWarning($"IconSetter: {sourceDir} not found, run generate-icons.sh first");
                return;
            }

            foreach (var src in Directory.GetFiles(sourceDir, "icon_*.png"))
            {
                var dest = Path.Combine(appiconsetPath, Path.GetFileName(src));
                File.Copy(src, dest, true);
            }

            var contentsJson =
                @"{
  ""images"" : [
    { ""filename"" : ""icon_120.png"", ""idiom"" : ""iphone"", ""scale"" : ""2x"", ""size"" : ""60x60"" },
    { ""filename"" : ""icon_180.png"", ""idiom"" : ""iphone"", ""scale"" : ""3x"", ""size"" : ""60x60"" },
    { ""filename"" : ""icon_80.png"",  ""idiom"" : ""iphone"", ""scale"" : ""2x"", ""size"" : ""40x40"" },
    { ""filename"" : ""icon_120.png"", ""idiom"" : ""iphone"", ""scale"" : ""3x"", ""size"" : ""40x40"" },
    { ""filename"" : ""icon_58.png"",  ""idiom"" : ""iphone"", ""scale"" : ""2x"", ""size"" : ""29x29"" },
    { ""filename"" : ""icon_87.png"",  ""idiom"" : ""iphone"", ""scale"" : ""3x"", ""size"" : ""29x29"" },
    { ""filename"" : ""icon_40.png"",  ""idiom"" : ""iphone"", ""scale"" : ""2x"", ""size"" : ""20x20"" },
    { ""filename"" : ""icon_60.png"",  ""idiom"" : ""iphone"", ""scale"" : ""3x"", ""size"" : ""20x20"" },
    { ""filename"" : ""icon_76.png"",  ""idiom"" : ""ipad"",   ""scale"" : ""1x"", ""size"" : ""76x76"" },
    { ""filename"" : ""icon_152.png"", ""idiom"" : ""ipad"",   ""scale"" : ""2x"", ""size"" : ""76x76"" },
    { ""filename"" : ""icon_167.png"", ""idiom"" : ""ipad"",   ""scale"" : ""2x"", ""size"" : ""83.5x83.5"" },
    { ""filename"" : ""icon_40.png"",  ""idiom"" : ""ipad"",   ""scale"" : ""1x"", ""size"" : ""40x40"" },
    { ""filename"" : ""icon_80.png"",  ""idiom"" : ""ipad"",   ""scale"" : ""2x"", ""size"" : ""40x40"" },
    { ""filename"" : ""icon_29.png"",  ""idiom"" : ""ipad"",   ""scale"" : ""1x"", ""size"" : ""29x29"" },
    { ""filename"" : ""icon_58.png"",  ""idiom"" : ""ipad"",   ""scale"" : ""2x"", ""size"" : ""29x29"" },
    { ""filename"" : ""icon_20.png"",  ""idiom"" : ""ipad"",   ""scale"" : ""1x"", ""size"" : ""20x20"" },
    { ""filename"" : ""icon_40.png"",  ""idiom"" : ""ipad"",   ""scale"" : ""2x"", ""size"" : ""20x20"" },
    { ""filename"" : ""icon_1024.png"",""idiom"" : ""ios-marketing"", ""scale"" : ""1x"", ""size"" : ""1024x1024"" }
  ],
  ""info"" : { ""author"" : ""xcode"", ""version"" : 1 }
}";

            File.WriteAllText(Path.Combine(appiconsetPath, "Contents.json"), contentsJson);
            Debug.Log("IconSetter: iOS app icons written to AppIcon.appiconset");
        }
    }
}

#endif
