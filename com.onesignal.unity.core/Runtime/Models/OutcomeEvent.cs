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
using System.Collections.Generic;

namespace OneSignalSDK {
    /// <summary>
    /// todo - desc
    /// </summary>
    [Serializable] public class OutcomeEvent {

        public enum OSSession {
            DIRECT,
            INDIRECT,
            UNATTRIBUTED,
            DISABLED
        }

        /// <summary>todo</summary>
        public OSSession session = OSSession.DISABLED;

        /// <summary>todo</summary>
        public List<string> notificationIds = new List<string>();

        /// <summary>todo</summary>
        public string name = "";

        /// <summary>todo</summary>
        public long timestamp = 0;

        /// <summary>todo</summary>
        public double weight = 0.0;

        /// <summary>todo</summary>
        internal OutcomeEvent() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outcomeDict"></param>
        internal OutcomeEvent(IReadOnlyDictionary<string, object> outcomeDict) {
            // session
            if (outcomeDict.ContainsKey("session") && outcomeDict["session"] != null)
                session = _sessionFromString(outcomeDict["session"] as string);

            // notificationIds
            if (outcomeDict.ContainsKey("notification_ids") && outcomeDict["notification_ids"] != null) {
                var notifications = new List<string>();

                if (outcomeDict["notification_ids"] is string) {
                    // notificationIds come over as a string of comma seperated string ids
                    notifications = new List<string> { Convert.ToString(outcomeDict["notification_ids"] as string) };
                }
                else {
                    // notificationIds come over as a List<object> and should be parsed and appended to the List<string>
                    if (outcomeDict["notification_ids"] is List<object> idObjects) {
                        foreach (var id in idObjects)
                            notifications.Add(id.ToString());
                    }
                }

                notificationIds = notifications;
            }

            // id
            if (outcomeDict.ContainsKey("id") && outcomeDict["id"] != null)
                name = outcomeDict["id"] as string;

            // timestamp
            if (outcomeDict.ContainsKey("timestamp") && outcomeDict["timestamp"] != null)
                timestamp = (long)outcomeDict["timestamp"];

            // weight
            if (outcomeDict.ContainsKey("weight") && outcomeDict["weight"] != null)
                weight = double.Parse(Convert.ToString(outcomeDict["weight"]));
        }

        // Used by onSendOutcomeSuccess() to convert session string to OSSession
        private OSSession _sessionFromString(string session)
        {
            session = session.ToLower();

            switch (session)
            {
                case "direct":
                    return OSSession.DIRECT;
                case "indirect":
                    return OSSession.INDIRECT;
                case "unattributed":
                    return OSSession.UNATTRIBUTED;
                default:
                    return OSSession.DISABLED;
            }
        }
    }
}