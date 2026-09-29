using System.Collections.Generic;
using UnityEngine;

namespace OneSignalSDK
{
    internal static class InputGuard
    {
        public static bool IsMissing(string value, string api)
        {
            if (!string.IsNullOrEmpty(value))
                return false;
            Debug.LogError("OneSignal: " + api + " is required");
            return true;
        }

        public static bool IsMissingAny(string[] values, string api)
        {
            if (values == null)
                return IsMissing(null, api);
            foreach (var value in values)
            {
                if (IsMissing(value, api))
                    return true;
            }
            return false;
        }

        public static bool HasMissingEntries(
            IDictionary<string, string> values,
            string api,
            bool allowEmptyValue
        )
        {
            if (values == null)
                return IsMissing(null, api);
            foreach (var pair in values)
            {
                if (IsMissing(pair.Key, api + ": key"))
                    return true;
                if (allowEmptyValue)
                {
                    if (pair.Value != null)
                        continue;
                    Debug.LogError("OneSignal: " + api + ": value is required");
                    return true;
                }
                if (IsMissing(pair.Value, api + ": value"))
                    return true;
            }
            return false;
        }
    }
}
