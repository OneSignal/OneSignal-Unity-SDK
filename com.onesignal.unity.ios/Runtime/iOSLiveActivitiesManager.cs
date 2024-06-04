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

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using OneSignalSDK.LiveActivities;
using OneSignalSDK.iOS.Utilities;

namespace OneSignalSDK.iOS.LiveActivities {
    internal sealed class iOSLiveActivitiesManager : ILiveActivitiesManager {
        [DllImport("__Internal")] private static extern void _oneSignalSetupDefaultLiveActivity(string optionsJson);
        [DllImport("__Internal")] private static extern void _oneSignalStartDefaultLiveActivity(string activityId, string attributesJson, string contentJson);
        [DllImport("__Internal")] private static extern void _oneSignalEnterLiveActivity(string activityId, string token, int hashCode, BooleanResponseDelegate callback);
        [DllImport("__Internal")] private static extern void _oneSignalExitLiveActivity(string activityId, int hashCode, BooleanResponseDelegate callback);

        [DllImport("__Internal")] private static extern void _oneSignalSetPushToStartToken(string activityType, string token);
        [DllImport("__Internal")] private static extern void _oneSignalRemovePushToStartToken(string activityType);

        private delegate void BooleanResponseDelegate(int hashCode, bool response);

        public async Task<bool> EnterAsync(string activityId, string token) {
            var (proxy, hashCode) = WaitingProxy._setupProxy<bool>();
            _oneSignalEnterLiveActivity(activityId, token, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public async Task<bool> ExitAsync(string activityId) {
            var (proxy, hashCode) = WaitingProxy._setupProxy<bool>();
            _oneSignalExitLiveActivity(activityId, hashCode, BooleanCallbackProxy);
            return await proxy;
        }

        public void RemovePushToStartToken(string activityType)
        {
           _oneSignalRemovePushToStartToken(activityType);
        }

        public void SetPushToStartToken(string activityType, string token)
        {
            _oneSignalSetPushToStartToken(activityType, token);
        }

        public void SetupDefault(LiveActivitySetupOptions options = null)
        {
            string optionsJson = null;
            if (options != null)
            {
                optionsJson = Json.Serialize(new Dictionary<string, object>
                {
                    { "enablePushToStart", options.EnablePushToStart },
                    { "enablePushToUpdate", options.EnablePushToUpdate }
                });
            }

            _oneSignalSetupDefaultLiveActivity(optionsJson);
        }

        public void StartDefault(string activityId, IDictionary<string, object> attributes, IDictionary<string, object> content)
        {
            _oneSignalStartDefaultLiveActivity(activityId, Json.Serialize(attributes), Json.Serialize(content));
        }

        [AOT.MonoPInvokeCallback(typeof(BooleanResponseDelegate))]
        private static void BooleanCallbackProxy(int hashCode, bool response)
            => WaitingProxy.ResolveCallbackProxy(hashCode, response);
    }
}