namespace OneSignalDemo.Models
{
    public enum InAppMessageType
    {
        TopBanner,
        BottomBanner,
        CenterModal,
        FullScreen,
    }

    public static class InAppMessageTypeExtensions
    {
        public static string TriggerValue(this InAppMessageType type) =>
            type switch
            {
                InAppMessageType.TopBanner => "top_banner",
                InAppMessageType.BottomBanner => "bottom_banner",
                InAppMessageType.CenterModal => "center_modal",
                InAppMessageType.FullScreen => "full_screen",
                _ => "top_banner",
            };

        public static string DisplayName(this InAppMessageType type) =>
            type switch
            {
                InAppMessageType.TopBanner => "Top Banner",
                InAppMessageType.BottomBanner => "Bottom Banner",
                InAppMessageType.CenterModal => "Center Modal",
                InAppMessageType.FullScreen => "Full Screen",
                _ => "Top Banner",
            };
    }
}
