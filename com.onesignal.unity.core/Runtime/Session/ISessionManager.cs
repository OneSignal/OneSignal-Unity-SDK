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

namespace OneSignalSDK.Session {
    /// <summary>
    /// The OneSignal session manager is responsible for managing the current session state.
    /// </summary>
    public interface ISessionManager {
        /// <summary>
        /// Send a trackable custom event which is tied to push notification campaigns
        /// </summary>
        /// <param name="name">The name of the outcome that has occurred.</param>
        void AddOutcome(string name);

        /// <summary>
        /// Send a trackable custom event which can only happen once and is tied to push notification campaigns
        /// </summary>
        /// <param name="name">The name of the unique outcome that has occurred.</param>
        void AddUniqueOutcome(string name);

        /// <summary>
        /// Send a trackable custom event with an attached value which is tied to push notification campaigns
        /// </summary>
        /// <param name="name">The name of the outcome that has occurred.</param>
        /// <param name="value">The value tied to the outcome.</param>
        void AddOutcomeWithValue(string name, float value);
    }
}