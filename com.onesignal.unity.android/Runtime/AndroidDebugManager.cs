/*
 * Modified MIT License
 *
 * Copyright 2023 OneSignal
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

using UnityEngine;
using OneSignalSDK.Debug;
using OneSignalSDK.Debug.Models;

namespace OneSignalSDK.Android.Debug {
    internal sealed class AndroidDebugManager : IDebugManager {
        private readonly AndroidJavaObject _debug;

        private LogLevel _logLevel = LogLevel.Warn;
        private LogLevel _alertLevel = LogLevel.None;

        public AndroidDebugManager(AndroidJavaClass sdkClass) {
            _debug = sdkClass.CallStatic<AndroidJavaObject>("getDebug");
        }

        public LogLevel LogLevel {
            get => _logLevel;
            set {
                _logLevel = value;
                _debug.Call("setLogLevel", ToLogLevel(value));
            }
        }

        public LogLevel AlertLevel {
            get => _alertLevel;
            set {
                _alertLevel = value;
                _debug.Call("setAlertLevel", ToLogLevel(value));
            }
        }

        private AndroidJavaObject ToLogLevel(LogLevel value) {
            var logLevelClass = new AndroidJavaClass("com.onesignal.debug.LogLevel");
            var logLevelValue = logLevelClass.CallStatic<AndroidJavaObject>("valueOf", logLevelClass, value.ToString().ToUpper());
            return logLevelValue;
        }
    }
}