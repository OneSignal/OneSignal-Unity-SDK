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