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

        public static bool IsMissing(object value, string api)
        {
            if (value != null)
                return false;
            Debug.LogError("OneSignal: " + api + " is required");
            return true;
        }

        public static bool IsNotFinite(float value, string api)
        {
            if (!float.IsNaN(value) && !float.IsInfinity(value))
                return false;
            Debug.LogError("OneSignal: " + api + " must be a finite number");
            return true;
        }

        // Native JSON serializers reject NaN and Infinity (Android throws, iOS drops the payload).
        public static Dictionary<string, object> ReplaceNonFiniteNumbers(
            IDictionary<string, object> values
        )
        {
            if (values == null)
                return null;
            var result = new Dictionary<string, object>(values.Count);
            foreach (var pair in values)
                result[pair.Key] = ReplaceNonFiniteNumber(pair.Value);
            return result;
        }

        private static object ReplaceNonFiniteNumber(object value)
        {
            switch (value)
            {
                case double d when double.IsNaN(d) || double.IsInfinity(d):
                case float f when float.IsNaN(f) || float.IsInfinity(f):
                    return null;
                case IDictionary<string, object> dict:
                    return ReplaceNonFiniteNumbers(dict);
                case IList<object> list:
                    var items = new List<object>(list.Count);
                    foreach (var item in list)
                        items.Add(ReplaceNonFiniteNumber(item));
                    return items;
                default:
                    return value;
            }
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
