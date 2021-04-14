using System;
using System.Collections.Generic;

public class OSOutcomeEvent
    {
        public enum OSSession
        {
            DIRECT,
            INDIRECT,
            UNATTRIBUTED,
            DISABLED
        }

        public OSSession session = OSSession.DISABLED;
        public List<string> notificationIds = new List<string>();
        public string name = "";
        public long timestamp = 0;
        public double weight = 0.0;

        public OSOutcomeEvent() { }

        public OSOutcomeEvent(Dictionary<string, object> outcomeDict)
        {
            // session
            if (outcomeDict.ContainsKey("session") && outcomeDict["session"] != null)
                session = SessionFromString(outcomeDict["session"] as string);

            // notificationIds
            if (outcomeDict.ContainsKey("notification_ids") && outcomeDict["notification_ids"] != null)
            {
                List<string> notifications = new List<string>();

                if (outcomeDict["notification_ids"].GetType().Equals(typeof(string)))
                {
                    // notificationIds come over as a string of comma seperated string ids
                    notifications = new List<string> { Convert.ToString(outcomeDict["notification_ids"] as string) };
                }
                else
                {
                    // notificationIds come over as a List<object> and should be parsed and appended to the List<string>
                    List<object> idObjects = outcomeDict["notification_ids"] as List<object>;
                    foreach (var id in idObjects)
                        notifications.Add(id.ToString());
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
        OSSession SessionFromString(string session)
        {
            session = session.ToLower();
            if (session.Equals("direct"))
            {
                return OSSession.DIRECT;
            }
            else if (session.Equals("indirect"))
            {
                return OSSession.INDIRECT;
            }
            else if (session.Equals("unattributed"))
            {
                return OSSession.UNATTRIBUTED;
            }

            return OSSession.DISABLED;
        }
    }
