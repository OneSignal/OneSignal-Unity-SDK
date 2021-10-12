/*
 * Modified MIT License
 *
 * Copyright 2021 OneSignal
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * 1. The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * 2. All copies of substantial portions of the Software may only be used in connection
 * with services provided by OneSignal.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.Collections.Generic;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace OneSignalSDK {
    /// <summary>
    /// Conversion methods for common Java types wrapped by <see cref="AndroidJavaObject"/>
    /// </summary>
    internal static class AndroidJavaObjectExtensions {
        /// <summary>
        /// Converts from a Java class which implements a toString method which returns a JSON representation of that
        /// object to a Serializable class in Unity
        /// </summary>
        public static TModel ToSerializable<TModel>(this AndroidJavaObject source)
            => JsonUtility.FromJson<TModel>(source.Call<string>("toString"));
        
        /*
         * JSONObject
         */
        
        /// <summary>
        /// Converts from a Java org.json.JSONObject to a <see cref="Dictionary{TKey,TValue}"/>
        /// </summary>
        public static Dictionary<string, object> JSONObjectToDictionary(this AndroidJavaObject source)
            => Json.Deserialize(source.Call<string>("toString")) as Dictionary<string, object>;

        /// <summary>
        /// Converts from a <see cref="Dictionary{TKey,TValue}"/> to a Java org.json.JSONObject
        /// </summary>
        public static AndroidJavaObject ToJSONObject<TKey, TValue>(this Dictionary<TKey, TValue> source)
            => new AndroidJavaObject("org.json.JSONObject", Json.Serialize(source));
        
        /*
         * Map
         */
        
        /// <summary>
        /// Converts from a Java java.util.Map to a <see cref="Dictionary{TKey,TValue}"/>
        /// </summary>
        public static Dictionary<string, object> MapToDictionary(this AndroidJavaObject source) {
            return null; // todo
        }

        /// <summary>
        /// Converts from a <see cref="Dictionary{TKey,TValue}"/> to a Java java.util.Map
        /// </summary>
        public static AndroidJavaObject ToMap(this Dictionary<string, object> source) {
            return null;
        }

    }
}