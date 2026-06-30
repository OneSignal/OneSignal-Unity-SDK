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

using System;
using OneSignalSDK.Android.Utilities;
using OneSignalSDK.Debug.Utilities;
using OneSignalSDK.Location;
using UnityEngine;

namespace OneSignalSDK.Android.Location
{
    internal sealed class AndroidLocationManager : ILocationManager
    {
        private const string LocationModuleNotAvailable =
            "OneSignal location module is not available. Add the location dependency to use OneSignal.Location.";

        private readonly AndroidJavaObject _location;

        public AndroidLocationManager(AndroidJavaClass sdkClass)
        {
            try
            {
                _location = sdkClass.CallStatic<AndroidJavaObject>("getLocation");
            }
            catch (Exception)
            {
                SDKDebug.Error(LocationModuleNotAvailable);
            }
        }

        public bool IsShared
        {
            get
            {
                try
                {
                    return _location != null && _location.Call<bool>("isShared");
                }
                catch (Exception)
                {
                    SDKDebug.Error(LocationModuleNotAvailable);
                    return false;
                }
            }
            set
            {
                try
                {
                    if (_location != null)
                        _location.Call("setShared", value);
                }
                catch (Exception)
                {
                    SDKDebug.Error(LocationModuleNotAvailable);
                }
            }
        }

        public void RequestPermission()
        {
            try
            {
                var continuation = new BoolContinuation();
                _location.Call<AndroidJavaObject>("requestPermission", continuation.Proxy);
            }
            catch (Exception)
            {
                SDKDebug.Error(LocationModuleNotAvailable);
            }
        }
    }
}
