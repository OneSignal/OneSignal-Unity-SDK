using OneSignalDemo.Services;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI
{
    public static class DemoToast
    {
        public const int DurationMs = 3000;

        private static VisualElement _root;
        private static VisualElement _container;
        private static IVisualElementScheduledItem _hideSchedule;

        public static void Initialize(VisualElement root)
        {
            _root = root;
        }

        public static void Show(string message)
        {
            if (_root == null)
                return;

            _hideSchedule?.Pause();
            _container?.RemoveFromHierarchy();

            _container = new VisualElement();
            _container.AddToClassList("toast-container");
            // The container and label must be Ignore so UI Toolkit picking
            // does not see them — the bottom-of-screen toast overlaps the
            // action buttons most tests tap right after the toast appears,
            // and without Ignore the empty horizontal slack of the centered
            // container swallowed the next tap.
            _container.pickingMode = PickingMode.Ignore;

            var label = new Label(message);
            label.name = "toast_message";
            label.AddToClassList("toast-label");
            label.pickingMode = PickingMode.Ignore;
            _container.Add(label);

            _root.Add(_container);
            AccessibilityBridge.RequestImmediateResync();

            _hideSchedule = _container.schedule.Execute(Hide).StartingIn(DurationMs);
        }

        private static void Hide()
        {
            _container?.RemoveFromHierarchy();
            _container = null;
            AccessibilityBridge.RequestImmediateResync();
        }
    }
}
