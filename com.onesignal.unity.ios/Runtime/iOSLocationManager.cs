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

using UnityEngine;
using System.Threading.Tasks;
using OneSignalSDKNew.Location;
using System.Runtime.InteropServices;

namespace OneSignalSDKNew.iOS.Location {
    internal sealed class iOSLocationManager : ILocationManager {
        [DllImport("__Internal")] private static extern bool _locationGetIsShared();
        [DllImport("__Internal")] private static extern void _locationSetIsShared(bool shared);
        [DllImport("__Internal")] private static extern void _locationRequestPermission();

        public bool IsShared {
            get => _locationGetIsShared();
            set => _locationSetIsShared(value);
        }

        public Task<bool> RequestPermissionAsync() { // iOS make async
            _locationRequestPermission();
            return Task.FromResult(true);
        }
    }
}