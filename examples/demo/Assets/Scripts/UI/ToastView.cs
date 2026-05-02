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

            var label = new Label(message);
            // Surface the message text to iOS accessibility so Appium's
            // `label/name/value == "..."` predicate can locate the toast.
            label.name = message;
            label.AddToClassList("toast-label");
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
