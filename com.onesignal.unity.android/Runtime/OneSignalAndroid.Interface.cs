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

using System.Threading.Tasks;
using Laters;
using UnityEngine;

namespace OneSignalSDK {
    /// <summary>
    /// 
    /// </summary>
    public partial class OneSignalAndroid : OneSignal {

        private const string SDKPackage = "com.onesignal";
        private const string SDKClassName = "OneSignal";
        private const string QualifiedSDKClass = SDKPackage + "." + SDKClassName;

        private readonly AndroidJavaClass _sdkClass = new AndroidJavaClass(QualifiedSDKClass);
        
        /// <summary>
        /// helper, assumes the callback interface is the last parameter
        /// </summary>
        private async Task<TReturn> _callAsync<TReturn, TCallback>(string methodName, params object[] args) 
            where TCallback : AwaitableAndroidJavaProxy<TReturn>, new() {
            
            var callback = new TCallback();

            if (args == null || args.Length == 0)
                _sdkClass.CallStatic(methodName, callback);
            else {
                var newArgs = new object[args.Length + 1];
                args.CopyTo(newArgs, 0);
                newArgs[args.Length] = callback;
                _sdkClass.CallStatic(methodName, newArgs);
            }

            return await callback;
        }
    }
}