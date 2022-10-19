using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;

namespace OneSignalSDK
{
    public class LinkerProcessor : IUnityLinkerProcessor
    {
        private const string XmlGuid = "b7ef312023e648bebcd07161e965b0d6";

        public int callbackOrder => 0;

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(XmlGuid);
            return Path.GetFullPath(assetPath);
        }

#if !UNITY_2021_1_OR_NEWER
        public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }

        public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }
#endif
    }
}
