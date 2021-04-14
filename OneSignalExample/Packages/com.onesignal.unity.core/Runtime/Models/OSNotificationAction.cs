public class OSNotificationAction
{
    public enum ActionType
    {
        // Notification was tapped on.
        Opened,

        // User tapped on an action from the notification.
        ActionTaken
    }

    public string actionID;
    public ActionType type;
}
