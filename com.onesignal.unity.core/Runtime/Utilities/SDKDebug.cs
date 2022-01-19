/*
 * Modified MIT License
 *
 * Copyright 2022 OneSignal
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
using UnityEngine;

namespace OneSignalSDK {
    /// <summary>
    /// Direct mapping of native SDK log levels
    /// </summary> 
    public enum LogLevel {
        None = 0, 
        Fatal, 
        Error, 
        Warn, 
        Info, 
        Debug, 
        Verbose
    }
    
    /// <summary>
    /// Helper for printing Unity logs formatted to specify they are from this SDK
    /// </summary>
    internal static class SDKDebug {
        public static event Action<object> LogIntercept; 
        public static event Action<object> WarnIntercept; 
        public static event Action<object> ErrorIntercept; 

        public static void Info(string message) {
            if (LogIntercept != null)
                LogIntercept(message);
            else if (OneSignal.Default.LogLevel >= LogLevel.Info)
                Debug.Log(_formatMessage(message));
        }
        
        public static void Warn(string message) {
            if (WarnIntercept != null)
                WarnIntercept(message);
            else if (OneSignal.Default.LogLevel >= LogLevel.Warn)
                Debug.LogWarning(_formatMessage(message));
        }
        
        public static void Error(string message) {
            if (ErrorIntercept != null)
                ErrorIntercept(message);
            else  if (OneSignal.Default.LogLevel >= LogLevel.Error)
                Debug.LogError(_formatMessage(message));
        }
        
        private static string _formatMessage(string message) => "[OneSignal] " + message;
    }
}