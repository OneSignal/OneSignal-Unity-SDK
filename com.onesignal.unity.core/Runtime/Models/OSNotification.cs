public class OSNotification
{
    public enum DisplayType
    {
        // Notification shown in the notification shade.
        Notification,

        // Notification shown as an in app alert.
        InAppAlert,

        // Notification was silent and not displayed.
        None
    }

    public bool isAppInFocus;
    public bool shown;
    public bool silentNotification;
    public int androidNotificationId;
    public DisplayType displayType;
    public OSNotificationPayload payload;
}
