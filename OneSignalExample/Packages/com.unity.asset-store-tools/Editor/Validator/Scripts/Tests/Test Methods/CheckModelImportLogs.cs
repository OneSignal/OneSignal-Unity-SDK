using AssetStoreTools.Validator.Data;
using AssetStoreTools.Validator.TestDefinitions;
using AssetStoreTools.Validator.TestMethods.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityObject = UnityEngine.Object;

namespace AssetStoreTools.Validator.TestMethods
{
    internal class CheckModelImportLogs : ITestScript
    {
        public TestResult Run(ValidationTestConfig config)
        {
            var result = new TestResult() { Result = TestResult.ResultStatus.Undefined };

            var models = AssetUtility.GetObjectsFromAssets<UnityObject>(config.ValidationPaths, AssetType.Model);
            var importLogs = ModelUtility.GetImportLogs(models.ToArray());

            var warningModels = new List<UnityObject>();
            var errorModels = new List<UnityObject>();

            foreach (var kvp in importLogs)
            {
                if (kvp.Value.Any(x => x.Severity == UnityEngine.LogType.Error))
                    errorModels.Add(kvp.Key);
                if (kvp.Value.Any(x => x.Severity == UnityEngine.LogType.Warning))
                    warningModels.Add(kvp.Key);
            }

            if (warningModels.Count > 0 || errorModels.Count > 0)
            {
                if (warningModels.Count > 0)
                {
                    result.Result = TestResult.ResultStatus.VariableSeverityIssue;
                    result.AddMessage("The following models contain import warnings:", null, warningModels.ToArray());
                }

                if (errorModels.Count > 0)
                {
                    result.Result = TestResult.ResultStatus.Fail;
                    result.AddMessage("The following models contain import errors:", null, errorModels.ToArray());
                }
            }
            else
            {
                result.Result = TestResult.ResultStatus.Pass;
                result.AddMessage("No issues were detected when importing your models!");
            }

            return result;
        }
    }
}
