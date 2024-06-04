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

namespace OneSignalSDK.LiveActivities {
    public interface ILiveActivitiesManager {
        /// <summary>
        /// Associates a customer defined activityId with a live activity temporary push token on OneSignal's server
        /// </summary>
        /// <remarks>iOS Only</remarks>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        Task<bool> EnterAsync(string activityId, string token);

        /// <summary>
        /// Deletes the association between a customer defined activityId with a Live Activity temporary push token on
        /// OneSignal's server
        /// </summary>
        /// <remarks>iOS Only</remarks>
        /// <returns>Awaitable boolean of whether the operation succeeded or failed</returns>
        Task<bool> ExitAsync(string activityId);

        /// <summary>
        /// Enable the OneSignalSDK to setup the default`DefaultLiveActivityAttributes` structure,
        /// which conforms to the `OneSignalLiveActivityAttributes`. When using this function, the
        /// widget attributes are owned by the OneSignal SDK, which will allow the SDK to handle the
        /// entire lifecycle of the live activity.  All that is needed from an app-perspective is to
        /// create a Live Activity widget in a widget extension, with a `ActivityConfiguration` for
        /// `DefaultLiveActivityAttributes`. This is most useful for users that (1) only have one Live
        /// Activity widget and (2) are using a cross-platform framework and do not want to create the
        /// cross-platform <-> iOS native bindings to manage ActivityKit.
        /// 
        /// Only applies to iOS.
        /// </summary>
        /// <param name="options">An optional structure to provide for more granular setup options.</param>
        void SetupDefault(LiveActivitySetupOptions options = null);

        /// <summary>
        /// Start a new LiveActivity that is modelled by the default`DefaultLiveActivityAttributes`
        /// structure. The `DefaultLiveActivityAttributes` is initialized with the dynamic `attributes`
        /// and `content` passed in.
        /// 
        /// Only applies to iOS.
        /// </summary>
        /// <param name="activityId">The activity identifier the live activity on this device will be started
        /// and eligible to receive updates for.</param>
        /// <param name="attributes">A dynamic type containing the static attributes passed into `DefaultLiveActivityAttributes`.</param>
        /// <param name="content">A dynamic type containing the content attributes passed into `DefaultLiveActivityAttributes`.</param>
        void StartDefault(string activityId, IDictionary<string, object> attributes, IDictionary<string, object> content);

        /// <summary>
        /// Indicate this device is capable of receiving pushToStart live activities for the `activityType`.
        /// The `activityType` **must** be the name of the struct conforming to `ActivityAttributes` that will be used
        /// to start the live activity.
        /// 
        /// Only applies to iOS.
        /// </summary>
        /// <param name="activityType">The name of the specific `ActivityAttributes` structure tied to the live activity.</param>
        /// <param name="token">The (OS-provided) token that will be used to start a live activity of this `activityType` on this device.</param>
        void SetPushToStartToken(string activityType, string token);

        /// <summary>
        /// Indicate this device is no longer capable of receiving pushToStart live activities for the `activityType`.
        /// The `activityType` **must** be the name of the struct conforming to `ActivityAttributes` that will be used
        /// to start the live activity.
        /// 
        /// Only applies to iOS.
        /// </summary>
        /// <param name="activityType">The name of the specific `ActivityAttributes` structure tied to the live activity.</param>
        void RemovePushToStartToken(string activityType);
    }
}