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

using System.Threading.Tasks;
using System.Runtime.InteropServices;
using OneSignalSDK.LiveActivities;
using OneSignalSDK.iOS.Utilities;

namespace OneSignalSDK.iOS.LiveActivities {
    internal sealed class iOSLiveActivitiesManager : ILiveActivitiesManager {
        [DllImport("__Internal")] private static extern void _enterLiveActivity(string activityId, string token, int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _exitLiveActivity(string activityId, int hashCode, BooleanResponseDelegate callback);

        private delegate void BooleanResponseDelegate(int hashCode, bool response);

        public async Task<bool> EnterAsync(string activityId, string token) {
            var (proxy, hashCode) = WaitingProxy._setupProxy<bool>();
            _enterLiveActivity(activityId, token, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public async Task<bool> ExitAsync(string activityId) {
            var (proxy, hashCode) = WaitingProxy._setupProxy<bool>();
            _exitLiveActivity(activityId, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        [AOT.MonoPInvokeCallback(typeof(BooleanResponseDelegate))]
        private static void BooleanCallbackProxy(int hashCode, bool response)
            => WaitingProxy.ResolveCallbackProxy(hashCode, response);
    }
}