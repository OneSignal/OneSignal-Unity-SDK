using AssetStoreTools.Validator.Data;
using AssetStoreTools.Validator.TestDefinitions;
using AssetStoreTools.Validator.TestMethods.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace AssetStoreTools.Validator.TestMethods
{
    internal class CheckAnimationClips : ITestScript
    {
        private static readonly string[] InvalidNames = new[] { "Take 001" };

        public TestResult Run(ValidationTestConfig config)
        {
            var result = new TestResult() { Result = TestResult.ResultStatus.Undefined };
            var badModels = new Dictionary<UnityObject, List<UnityObject>>();
            var models = AssetUtility.GetObjectsFromAssets<UnityObject>(config.ValidationPaths, AssetType.Model);

            foreach(var model in models)
            {
                var badClips = new List<UnityObject>();
                var clips = AssetDatabase.LoadAllAssetsAtPath(AssetUtility.ObjectToAssetPath(model));
                foreach(var clip in clips)
                {
                    if (InvalidNames.Any(x => x.ToLower().Equals(clip.name.ToLower())))
                    {
                        badClips.Add(clip);
                    }
                }

                if (badClips.Count > 0)
                    badModels.Add(model, badClips);
            }

            if(badModels.Count > 0)
            {
                result.Result = TestResult.ResultStatus.VariableSeverityIssue;
                result.AddMessage("The following models have animation clips with invalid names. Animation clip names should be unique and reflective of the animation itself");
                foreach(var kvp in badModels)
                {
                    result.AddMessage(AssetUtility.ObjectToAssetPath(kvp.Key), null, kvp.Value.ToArray());
                }
            }
            else
            {
                result.AddMessage("No animation clips with invalid names were found!");
                result.Result = TestResult.ResultStatus.Pass;
            }

            return result;
        }
    }
}
