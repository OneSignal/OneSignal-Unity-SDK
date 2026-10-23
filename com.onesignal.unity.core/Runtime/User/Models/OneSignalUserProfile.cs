using System.Collections.Generic;

namespace OneSignalSDK.User
{
    /// <summary>
    /// User data applied together with a login. Blank email and SMS values are treated as absent.
    /// </summary>
    public sealed class OneSignalUserProfile
    {
        public string Email { get; set; }
        public string Sms { get; set; }
        public Dictionary<string, string> Tags { get; set; }
        public Dictionary<string, string> Aliases { get; set; }
    }
}
