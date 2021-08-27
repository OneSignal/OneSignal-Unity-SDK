using System;
using System.Collections.Generic;
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
        
        private async Task<TReturn> _callAsync<TReturn, TCallback>(string methodName, Action<TReturn> delegateEvent = null) 
            where TCallback : AwaitableAndroidJavaProxy<TReturn>, new() {
            
            var callback = new TCallback();
            
            _sdkClass.CallStatic(methodName, callback);
            
            var result = await callback;
            delegateEvent?.Invoke(result);
            return result;
        }
    }
}