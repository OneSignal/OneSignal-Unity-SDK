using UnityEngine;
using System;
using Laters;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OneSignalSDK.Android.Utilities {
    public abstract class OneSignalAndroidJavaProxy : AndroidJavaProxy {
        private const string SDKPackage = "com.onesignal";

        protected OneSignalAndroidJavaProxy(string listenerClassName)
            : base(SDKPackage + "." + listenerClassName) { }
    }
    
    public abstract class OneSignalAwaitableAndroidJavaProxy<TResult> : AwaitableAndroidJavaProxy<TResult> {
        private const string SDKPackage = "com.onesignal";

        protected OneSignalAwaitableAndroidJavaProxy(string listenerClassName)
            : base(SDKPackage + "." + listenerClassName) { }
    }

    public sealed class Continuation {
        public AndroidJavaObject Proxy { get; }
        private AndroidConsumer _consumer;

        public Continuation() {
            var continuation = new AndroidJavaClass("com.onesignal.Continue");
            _consumer = new AndroidConsumer();
            Proxy = continuation.CallStatic<AndroidJavaObject>("with", _consumer);
        }

        public TaskAwaiter<object> GetAwaiter() => _consumer.GetAwaiter();
    }

    public sealed class AndroidConsumer : AwaitableAndroidJavaProxy<object> {
        public AndroidConsumer() : base("java.util.function.Consumer") { }

        public void accept(AndroidJavaObject obj) {
            var result = obj.Call<bool>("isSuccess");

            if (result) {
                _complete(null);
            } else {
                var throwable = obj.Call<AndroidJavaObject>("getThrowable");
                if (throwable != null) {
                    _fail(throwable.Call<string>("getMessage"));
                } else {
                    _fail("error with async method");
                }
            }
        }
    }

    public sealed class BoolContinuation {
        public AndroidJavaObject Proxy { get; }
        private AndroidBoolConsumer _consumer;

        public BoolContinuation() {
            var continuation = new AndroidJavaClass("com.onesignal.Continue");
            _consumer = new AndroidBoolConsumer();
            Proxy = continuation.CallStatic<AndroidJavaObject>("with", _consumer);
        }

        public TaskAwaiter<bool> GetAwaiter() => _consumer.GetAwaiter();
    }

    public sealed class AndroidBoolConsumer : AwaitableAndroidJavaProxy<bool> {
        public AndroidBoolConsumer() : base("java.util.function.Consumer") { }

        public void accept(AndroidJavaObject obj) {
            var result = obj.Call<bool>("isSuccess");

            if (result) {
                var data = obj.Call<AndroidJavaObject>("getData");
                var value = data.Call<bool>("booleanValue");
                _complete(value);
            } else {
                var throwable = obj.Call<AndroidJavaObject>("getThrowable");
                if (throwable != null)
                    _fail(throwable.Call<string>("getMessage"));
                else
                    _fail("error with async method");
            }
        }
    }
}
