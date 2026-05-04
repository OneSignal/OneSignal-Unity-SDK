using UnityEngine;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI
{
    public class ToastView
    {
        private readonly VisualElement _root;
        private VisualElement _container;
        private IVisualElementScheduledItem _hideSchedule;

        public ToastView(VisualElement root)
        {
            _root = root;
        }

        public void Show(string message)
        {
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
            label.AddToClassList("toast-label");
            label.pickingMode = PickingMode.Ignore;
            _container.Add(label);

            _root.Add(_container);

            _hideSchedule = _container.schedule.Execute(Hide).StartingIn(2500);
        }

        private void Hide()
        {
            _container?.RemoveFromHierarchy();
            _container = null;
        }
    }
}
