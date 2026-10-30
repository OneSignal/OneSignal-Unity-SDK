namespace OneSignalDemo.Models
{
    public enum InAppMessageType
    {
        TopBanner,
        BottomBanner,
        CenterModal,
        Html,
        FullScreen,
        FullBleed,
        Carousel,
        CarouselBleed,
    }

    public static class InAppMessageTypeExtensions
    {
        public static string TriggerValue(this InAppMessageType type) =>
            type switch
            {
                InAppMessageType.TopBanner => "top_banner",
                InAppMessageType.BottomBanner => "bottom_banner",
                InAppMessageType.CenterModal => "center_modal",
                InAppMessageType.Html => "html",
                InAppMessageType.FullScreen => "full_screen",
                InAppMessageType.FullBleed => "full_bleed",
                InAppMessageType.Carousel => "carousel",
                InAppMessageType.CarouselBleed => "carousel_bleed",
                _ => "top_banner",
            };

        public static string DisplayName(this InAppMessageType type) =>
            type switch
            {
                InAppMessageType.TopBanner => "Top",
                InAppMessageType.BottomBanner => "Bottom",
                InAppMessageType.CenterModal => "Center",
                InAppMessageType.Html => "HTML",
                InAppMessageType.FullScreen => "Full",
                InAppMessageType.FullBleed => "Full Bleed",
                InAppMessageType.Carousel => "Carousel",
                InAppMessageType.CarouselBleed => "Carousel Bleed",
                _ => "Top",
            };
    }
}
