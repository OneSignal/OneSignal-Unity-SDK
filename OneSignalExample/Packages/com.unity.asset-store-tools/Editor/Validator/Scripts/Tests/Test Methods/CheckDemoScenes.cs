using AssetStoreTools.Utility.Json;
using AssetStoreTools.Validator.Data;
using AssetStoreTools.Validator.TestDefinitions;
using AssetStoreTools.Validator.TestMethods.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace AssetStoreTools.Validator.TestMethods
{
    internal class CheckDemoScenes : ITestScript
    {
        private class DemoSceneScanResult
        {
            public List<UnityObject> ValidAdbScenes;
            public List<string> HybridScenePaths;
            public List<UnityObject> NestedUnityPackages;
        }

        public TestResult Run(ValidationTestConfig config)
        {
            var result = new TestResult();
            var demoSceneScanResult = CheckForDemoScenes(config);

            // Valid demo scenes were found in ADB
            if (demoSceneScanResult.ValidAdbScenes.Count > 0)
            {
                result.Result = TestResult.ResultStatus.Pass;
                result.AddMessage("Demo scenes found", null, demoSceneScanResult.ValidAdbScenes.ToArray());
                return result;
            }

            // Valid demo scenes found in UPM package.json
            if (demoSceneScanResult.HybridScenePaths.Count > 0)
            {
                result.Result = TestResult.ResultStatus.Pass;

                var upmSampleSceneList = string.Join("\n-", demoSceneScanResult.HybridScenePaths);
                upmSampleSceneList = upmSampleSceneList.Insert(0, "-");

                result.AddMessage($"Demo scenes found:\n{upmSampleSceneList}");
                return result;
            }

            // No valid scenes found, but package contains nested .unitypackages
            if (demoSceneScanResult.NestedUnityPackages.Count > 0)
            {
                result.Result = TestResult.ResultStatus.Warning;
                result.AddMessage("Could not find any valid Demo scenes in the selected validation paths.");
                result.AddMessage("The following nested .unitypackage files were found. " +
                    "If they contain any demo scenes, you can ignore this warning.", null, demoSceneScanResult.NestedUnityPackages.ToArray());
                return result;
            }

            // No valid scenes were found and there is nothing pointing to their inclusion in the package
            result.Result = TestResult.ResultStatus.VariableSeverityIssue;
            result.AddMessage("Could not find any valid Demo Scenes in the selected validation paths.");
            return result;
        }

        private DemoSceneScanResult CheckForDemoScenes(ValidationTestConfig config)
        {
            var scanResult = new DemoSceneScanResult();
            scanResult.ValidAdbScenes = CheckForDemoScenesInAssetDatabase(config);
            scanResult.HybridScenePaths = CheckForDemoScenesInUpmSamples(config);
            scanResult.NestedUnityPackages = CheckForNestedUnityPackages(config);

            return scanResult;
        }

        private List<UnityObject> CheckForDemoScenesInAssetDatabase(ValidationTestConfig config)
        {
            var scenePaths = AssetUtility.GetAssetPathsFromAssets(config.ValidationPaths, AssetType.Scene).ToArray();
            if (scenePaths.Length == 0)
                return new List<UnityObject>();

            var originalScenePath = SceneUtility.CurrentScenePath;
            var validScenePaths = scenePaths.Where(CanBeDemoScene).ToArray();
            SceneUtility.OpenScene(originalScenePath);

            if (validScenePaths.Length == 0)
                return new List<UnityObject>();

            return validScenePaths.Select(x => AssetDatabase.LoadAssetAtPath<UnityObject>(x)).ToList();
        }

        private bool CanBeDemoScene(string scenePath)
        {
            // Check skybox
            var sceneSkyboxPath = AssetUtility.ObjectToAssetPath(RenderSettings.skybox).Replace("\\", "").Replace("/", "");
            var defaultSkyboxPath = "Resources/unity_builtin_extra".Replace("\\", "").Replace("/", "");

            if (!sceneSkyboxPath.Equals(defaultSkyboxPath, StringComparison.OrdinalIgnoreCase))
                return true;

            // Check GameObjects
            SceneUtility.OpenScene(scenePath);
            var rootObjects = SceneUtility.GetRootGameObjects();
            var count = rootObjects.Length;

            if (count == 0)
                return false;

            if (count != 2)
                return true;

            var cameraGOUnchanged = rootObjects.Any(o => o.TryGetComponent<Camera>(out _) && o.GetComponents(typeof(Component)).Length == 3);
            var lightGOUnchanged = rootObjects.Any(o => o.TryGetComponent<Light>(out _) && o.GetComponents(typeof(Component)).Length == 2);

            return !cameraGOUnchanged || !lightGOUnchanged;
        }

        private List<string> CheckForDemoScenesInUpmSamples(ValidationTestConfig config)
        {
            var scenePaths = new List<string>();

            foreach (var path in config.ValidationPaths)
            {
                if (!File.Exists($"{path}/package.json"))
                    continue;

                var packageJsonText = File.ReadAllText($"{path}/package.json");
                var json = JSONParser.SimpleParse(packageJsonText);

                if (!json.ContainsKey("samples") || !json["samples"].IsList() || json["samples"].AsList().Count == 0)
                    continue;

                foreach (var sample in json["samples"].AsList())
                {
                    var samplePath = sample["path"].AsString();
                    samplePath = $"{path}/{samplePath}";
                    if (!Directory.Exists(samplePath))
                        continue;

                    var sampleScenePaths = Directory.GetFiles(samplePath, "*.unity", SearchOption.AllDirectories);
                    foreach (var scenePath in sampleScenePaths)
                    {
                        // If meta file is not found, the sample will not be included with the exported .unitypackage
                        if (!File.Exists($"{scenePath}.meta"))
                            continue;

                        if (!scenePaths.Contains(scenePath.Replace("\\", "/")))
                            scenePaths.Add(scenePath.Replace("\\", "/"));
                    }
                }
            }

            return scenePaths;
        }

        private List<UnityObject> CheckForNestedUnityPackages(ValidationTestConfig config)
        {
            var unityPackages = AssetUtility.GetObjectsFromAssets(config.ValidationPaths, AssetType.UnityPackage).ToArray();
            return unityPackages.ToList();
        }
    }
}