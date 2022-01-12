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

using System;
using System.Linq;
using UnityEngine;

namespace OneSignalSDK {
    public abstract partial class OneSignal {
        internal static event Action<string> OnInitialize;
        protected static void _completedInit(string appId) => OnInitialize?.Invoke(appId);
        
        protected LogLevel _logLevel = LogLevel.Fatal;
        protected LogLevel _alertLevel = LogLevel.None;
        
        private static OneSignal _default;

        private static OneSignal _getDefaultInstance() {
            if (_default != null)
                return _default;

            // only 1 sdk should be available for any given supported platform
            var availableSDKs = ReflectionHelpers.FindAllAssignableTypes<OneSignal>("OneSignal");
            if (Activator.CreateInstance(availableSDKs.First()) is OneSignal sdk) {
                _default = sdk;
                SDKDebug.Info($"OneSignal.Default set to platform SDK {sdk.GetType()}. Current version is {Version}");
            }
            else {
                Debug.LogError("[OneSignal] Could not find an implementation of OneSignal SDK to use!");
            }
            
            return _default;
        }
    }
}

