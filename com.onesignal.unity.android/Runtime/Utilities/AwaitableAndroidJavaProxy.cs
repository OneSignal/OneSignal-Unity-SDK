using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Laters {
    public abstract class AwaitableAndroidJavaProxy<TResult> : AndroidJavaProxy {
        public TaskAwaiter<TResult> GetAwaiter() {
            if (_completionSource != null)
                return _completionSource.Task.GetAwaiter();
                
            _completionSource = new TaskCompletionSource<TResult>();
            
            if (_isComplete)
                _setResult();
            else
                _onComplete += result => _setResult();

            return _completionSource.Task.GetAwaiter();
        }
        
        protected bool _isComplete = false;
        
        protected AwaitableAndroidJavaProxy(string javaInterface) : base(javaInterface) {}
        protected AwaitableAndroidJavaProxy(AndroidJavaClass javaInterface) : base(javaInterface) {}

        protected void _complete(TResult result) {
            if (_isComplete)
                return;

            _result = result;

            var toComplete = _onComplete;
            _onComplete = null;

            _isComplete = true;
            toComplete?.Invoke(_result);
        }

        protected void _fail(string error, bool except = false) {
            if (except) {
                var exception = new Exception(error);
                Debug.LogException(exception);
                _completionSource.TrySetException(exception);
            }
            else {
                Debug.LogError(error);
                _completionSource.TrySetCanceled();
            }
        }

        private void _setResult() {
            try {
                _completionSource.TrySetResult(_result);
            }
            catch (ObjectDisposedException e) {
                Debug.Log(e.Message);
            }
        }
            
        private TaskCompletionSource<TResult> _completionSource;
        private event Action<TResult> _onComplete;
        private TResult _result;
    }

    /// <summary>
    /// Helper which will autocomplete the awaited task when a callback is invoked
    /// </summary>
    public abstract class AwaitableVoidAndroidJavaProxy : AwaitableAndroidJavaProxy<object> {
        protected AwaitableVoidAndroidJavaProxy(string javaInterface) : base(javaInterface) {}
        protected AwaitableVoidAndroidJavaProxy(AndroidJavaClass javaInterface) : base(javaInterface) {}

        public override AndroidJavaObject Invoke(string methodName, object[] args) {
            var invokeResult = base.Invoke(methodName, args);
            
            if (_isComplete == false)
                _complete(null);
            
            return invokeResult;
        }
    }
}