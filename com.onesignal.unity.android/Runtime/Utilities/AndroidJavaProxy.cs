using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Laters;
using OneSignalSDK.User;
using UnityEngine;

namespace OneSignalSDK.Android.Utilities
{
    public abstract class OneSignalAndroidJavaProxy : AndroidJavaProxy
    {
        private const string SDKPackage = "com.onesignal";

        protected OneSignalAndroidJavaProxy(string listenerClassName)
            : base(SDKPackage + "." + listenerClassName) { }
    }

    public abstract class OneSignalAwaitableAndroidJavaProxy<TResult>
        : AwaitableAndroidJavaProxy<TResult>
    {
        private const string SDKPackage = "com.onesignal";

        protected OneSignalAwaitableAndroidJavaProxy(string listenerClassName)
            : base(SDKPackage + "." + listenerClassName) { }
    }

    public sealed class Continuation
    {
        public AndroidJavaObject Proxy { get; }
        private AndroidConsumer _consumer;

        public Continuation()
        {
            var continuation = new AndroidJavaClass("com.onesignal.Continue");
            _consumer = new AndroidConsumer();
            Proxy = continuation.CallStatic<AndroidJavaObject>("with", _consumer);
        }

        public TaskAwaiter<object> GetAwaiter() => _consumer.GetAwaiter();
    }

    public sealed class AndroidConsumer : AwaitableAndroidJavaProxy<object>
    {
        public AndroidConsumer()
            : base("java.util.function.Consumer") { }

        public void accept(AndroidJavaObject obj)
        {
            var result = obj.Call<bool>("isSuccess");

            if (result)
            {
                _complete(null);
            }
            else
            {
                var throwable = obj.Call<AndroidJavaObject>("getThrowable");
                if (throwable != null)
                {
                    _fail(throwable.Call<string>("getMessage"));
                }
                else
                {
                    _fail("error with async method");
                }
            }
        }
    }

    /// <summary>
    /// Completes with the native OneSignalResult, whether it arrives through the continuation or
    /// is returned synchronously by the suspend call. Completes exactly once and never throws.
    /// </summary>
    internal sealed class LoginResultContinuation : AndroidJavaProxy
    {
        private readonly TaskCompletionSource<LoginResult> _completion =
            new TaskCompletionSource<LoginResult>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

        public Task<LoginResult> Result => _completion.Task;

        public LoginResultContinuation()
            : base("java.util.function.Consumer") { }

        public AndroidJavaObject CreateProxy()
        {
            using var continuation = new AndroidJavaClass("com.onesignal.Continue");
            return continuation.CallStatic<AndroidJavaObject>("with", this);
        }

        public void accept(AndroidJavaObject continueResult)
        {
            try
            {
                if (continueResult.Call<bool>("isSuccess"))
                {
                    using var result = continueResult.Call<AndroidJavaObject>("getData");
                    Resolve(result);
                }
                else
                {
                    using var throwable = continueResult.Call<AndroidJavaObject>("getThrowable");
                    Fail(throwable?.Call<string>("toString") ?? "loginWithProfile failed");
                }
            }
            catch (Exception e)
            {
                Fail(e.Message);
            }
        }

        public void Resolve(AndroidJavaObject oneSignalResult)
        {
            try
            {
                using var map = oneSignalResult.Call<AndroidJavaObject>("toMap");
                using var json = new AndroidJavaObject("org.json.JSONObject", map);
                _completion.TrySetResult(LoginResult.FromJson(json.Call<string>("toString")));
            }
            catch (Exception e)
            {
                Fail(e.Message);
            }
        }

        public void Fail(string message) =>
            _completion.TrySetResult(LoginResult.Failure("UNKNOWN", message));
    }

    public sealed class BoolContinuation
    {
        public AndroidJavaObject Proxy { get; }
        private AndroidBoolConsumer _consumer;

        public BoolContinuation()
        {
            var continuation = new AndroidJavaClass("com.onesignal.Continue");
            _consumer = new AndroidBoolConsumer();
            Proxy = continuation.CallStatic<AndroidJavaObject>("with", _consumer);
        }

        public TaskAwaiter<bool> GetAwaiter() => _consumer.GetAwaiter();
    }

    public sealed class AndroidBoolConsumer : AwaitableAndroidJavaProxy<bool>
    {
        public AndroidBoolConsumer()
            : base("java.util.function.Consumer") { }

        public void accept(AndroidJavaObject obj)
        {
            var result = obj.Call<bool>("isSuccess");

            if (result)
            {
                var data = obj.Call<AndroidJavaObject>("getData");
                var value = data.Call<bool>("booleanValue");
                _complete(value);
            }
            else
            {
                var throwable = obj.Call<AndroidJavaObject>("getThrowable");
                if (throwable != null)
                    _fail(throwable.Call<string>("getMessage"));
                else
                    _fail("error with async method");
            }
        }
    }
}
