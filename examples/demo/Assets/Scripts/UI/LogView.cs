using OneSignalDemo.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI
{
    public class LogView
    {
        public VisualElement Root { get; }

        private readonly Label _headerLabel;
        private readonly Button _toggleButton;
        private readonly ScrollView _body;
        private bool _collapsed;

        public LogView()
        {
            Root = new VisualElement();
            Root.name = "log_view";
            Root.AddToClassList("log-view");

            var header = new VisualElement();
            header.AddToClassList("log-header");

            _headerLabel = new Label();
            _headerLabel.AddToClassList("log-header-label");
            header.Add(_headerLabel);

            var clearButton = new Button(LogService.Clear);
            clearButton.name = "log_clear_button";
            clearButton.text = MaterialIcons.Delete;
            clearButton.AddToClassList("log-header-button");
            clearButton.AddToClassList("log-clear-button");
            AccessibilityBridge.RegisterE2ETapTarget(clearButton, () => true, LogService.Clear);
            header.Add(clearButton);

            _toggleButton = new Button(ToggleCollapsed);
            _toggleButton.name = "log_toggle_button";
            _toggleButton.AddToClassList("log-header-button");
            _toggleButton.AddToClassList("log-toggle-button");
            AccessibilityBridge.RegisterE2ETapTarget(_toggleButton, () => true, ToggleCollapsed);
            header.Add(_toggleButton);

            Root.Add(header);

            _body = new ScrollView(ScrollViewMode.Vertical);
            _body.AddToClassList("log-body");
            _body.contentContainer.AddToClassList("log-body-content");
            _body.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            _body.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _body.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
            _body.contentContainer.RegisterCallback<GeometryChangedEvent>(_ => ScrollToEnd());
            Root.Add(_body);

            // Entries is static, so only stay subscribed while attached or rebuilt screens leak.
            Root.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                LogService.Changed += Render;
                Render();
            });
            Root.RegisterCallback<DetachFromPanelEvent>(_ => LogService.Changed -= Render);

            ApplyCollapsed();
        }

        private void ToggleCollapsed()
        {
            _collapsed = !_collapsed;
            ApplyCollapsed();
            AccessibilityBridge.RequestResync();
        }

        private void ApplyCollapsed()
        {
            _body.style.display = _collapsed ? DisplayStyle.None : DisplayStyle.Flex;
            _toggleButton.text = _collapsed ? MaterialIcons.ExpandMore : MaterialIcons.ExpandLess;
            if (!_collapsed)
                ScrollToEnd();
        }

        // Rebuilt in full so `log_entry_{index}` stays 0-based after the oldest entries are dropped.
        private void Render()
        {
            var entries = LogService.Entries;
            _headerLabel.text = $"LOGS ({entries.Count})";
            _body.Clear();

            if (entries.Count == 0)
            {
                var empty = new Label("No logs yet");
                empty.AddToClassList("log-empty");
                _body.Add(empty);
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var row = new VisualElement();
                row.AddToClassList("log-entry");

                var time = new Label(entries[i].Time);
                time.AddToClassList("log-entry-time");
                row.Add(time);

                // Plain text (no rich-text spans) so the a11y bridge reports the message verbatim.
                var message = new Label(entries[i].Message);
                message.name = $"log_entry_{i}";
                message.enableRichText = false;
                message.AddToClassList("log-entry-message");
                row.Add(message);

                _body.Add(row);
            }

            AccessibilityBridge.RequestResync();
        }

        // Deferred a tick so the ScrollView has picked up the new content height.
        private void ScrollToEnd()
        {
            if (_collapsed)
                return;
            _body.schedule.Execute(() =>
            {
                var overflow =
                    _body.contentContainer.layout.height - _body.contentViewport.layout.height;
                if (overflow > 0)
                    _body.scrollOffset = new Vector2(0, overflow);
            });
        }
    }
}
