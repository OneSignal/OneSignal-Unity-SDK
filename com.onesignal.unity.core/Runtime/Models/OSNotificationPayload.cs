using System.Collections.Generic;

public class OSNotificationPayload
{
    public string notificationID;
    public string sound;
    public string title;
    public string body;
    public string subtitle;
    public string launchURL;
    public Dictionary<string, object> additionalData;
    public Dictionary<string, object> actionButtons;
    public bool contentAvailable;
    public int badge;
    public string smallIcon;
    public string largeIcon;
    public string bigPicture;
    public string smallIconAccentColor;
    public string ledColor;
    public int lockScreenVisibility = 1;
    public string groupKey;
    public string groupMessage;
    public string fromProjectNumber;
}

