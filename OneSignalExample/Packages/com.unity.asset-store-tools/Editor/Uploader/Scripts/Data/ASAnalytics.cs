using System;
using UnityEditor;
using UnityEngine.Analytics;

namespace AssetStoreTools.Uploader.Data
{
    internal static class ASAnalytics
    {
        private const int VersionId = 3;
        private const int MaxEventsPerHour = 20;
        private const int MaxNumberOfElements = 1000;

        private const string VendorKey = "unity.assetStoreTools";
        private const string EventName = "assetStoreTools";
        
        static bool EnableAnalytics()
        {
#if UNITY_2023_2_OR_NEWER
            return true;
#else
            var result = EditorAnalytics.RegisterEventWithLimit(EventName, MaxEventsPerHour, MaxNumberOfElements, VendorKey, VersionId);
            return result == AnalyticsResult.Ok;
#endif
        }

        [System.Serializable]
        public struct AnalyticsData
#if UNITY_2023_2_OR_NEWER
            : IAnalytic.IData
#endif
        {
            public string ToolVersion;
            public string PackageId;
            public string Category;
            public bool UsedValidator;
            public string ValidatorResults;
            public string UploadFinishedReason;
            public double TimeTaken;
            public long PackageSize;
            public string Workflow;
            public string EndpointUrl;
        }

#if UNITY_2023_2_OR_NEWER
        [AnalyticInfo(eventName: EventName, vendorKey: VendorKey, version: VersionId, maxEventsPerHour: MaxEventsPerHour, maxNumberOfElements: MaxNumberOfElements)]
        private class AssetStoreToolsAnalytic : IAnalytic
        {
            private AnalyticsData _data;

            public AssetStoreToolsAnalytic(AnalyticsData data)
            {
                _data = data;
            }

            public bool TryGatherData(out IAnalytic.IData data, out Exception error)
            {
                error = null;
                data = _data;
                return data != null;
            }
        }
#endif

        public static void SendUploadingEvent(AnalyticsData data)
        {
            if (!EditorAnalytics.enabled)
                return;

            if (!EnableAnalytics())
                return;

#if UNITY_2023_2_OR_NEWER
            var analytic = new AssetStoreToolsAnalytic(data);
            EditorAnalytics.SendAnalytic(analytic);
#else
            EditorAnalytics.SendEventWithLimit(EventName, data, VersionId);
#endif
        }
    }
}