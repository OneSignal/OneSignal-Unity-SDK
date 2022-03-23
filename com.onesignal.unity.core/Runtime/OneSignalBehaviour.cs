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

namespace OneSignalSDK {
    /// <summary>
    /// Behaviour attached to the OneSignalController.prefab which can be dragged into your scene fora codeless init
    /// </summary>
    public class OneSignalBehaviour : MonoBehaviour {
        /// <summary>
        /// The unique identifier for your application from the OneSignal dashboard
        /// https://documentation.onesignal.com/docs/accounts-and-keys#app-id
        /// </summary>
        public string AppId;

        /// <summary>
        /// The minimum level of logs which will be logged to the console
        /// </summary>
        public LogLevel LogLevel = LogLevel.Warn;
        
        /// <summary>
        /// The minimum level of log events which will be converted into foreground alerts
        /// </summary>
        public LogLevel AlertLevel = LogLevel.None;

        /// <summary>
        /// Allows you to delay the initialization of the SDK until <see cref="OneSignal.PrivacyConsent"/> is set to
        /// true. Must be set before <see cref="OneSignal.Initialize"/> is called.
        /// </summary>
        public bool RequireUserConsent = false;

        /// <summary>
        /// Disable or enable location collection by OneSignal (defaults to enabled if your app has location permission).
        /// </summary>
        /// <remarks>This method must be called before <see cref="OneSignal.Initialize"/> on iOS.</remarks>
        public bool ShareLocation = false;

        private void Start() {
            OneSignal.Default.LogLevel               = LogLevel;
            OneSignal.Default.AlertLevel             = AlertLevel;
            OneSignal.Default.RequiresPrivacyConsent = RequireUserConsent;
            OneSignal.Default.ShareLocation          = ShareLocation;
            
            OneSignal.Default.Initialize(AppId);
        }
    }
}