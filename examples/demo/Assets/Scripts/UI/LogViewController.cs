using System;
using OneSignalDemo.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI
{
    public class LogViewController
    {
        private readonly VisualElement _container;
        private readonly VisualElement _scrollContent;
        private readonly ScrollView _scrollView;
        private readonly Label _countLabel;
        private readonly Label _emptyLabel;
        private bool _expanded = true;

        public LogViewController(VisualElement root)
        {
            _container = new VisualElement();
            _container.name = "log_view_container";
            _container.AddToClassList("log-container");

            var header = new VisualElement();
            header.name = "log_view_header";
            header.AddToClassList("log-header");
            header.RegisterCallback<ClickEvent>(_ => ToggleExpand());

            var headerLeft = new VisualElement();
            headerLeft.AddToClassList("log-header-left");

            var title = new Label("Logs");
            title.AddToClassList("log-header-title");
            headerLeft.Add(title);

            _countLabel = new Label("(0)");
            _countLabel.name = "log_view_count";
            _countLabel.AddToClassList("log-header-count");
            headerLeft.Add(_countLabel);

            header.Add(headerLeft);

            var clearButton = new Button(ClearLogs);
            clearButton.name = "log_view_clear_button";
            clearButton.text = "\uE5CD";
            clearButton.AddToClassList("log-clear-button");
            header.Add(clearButton);

            _container.Add(header);

            _scrollView = new ScrollView(ScrollViewMode.Horizontal);
            _scrollView.name = "log_view_list";
            _scrollView.AddToClassList("log-scroll");

            _scrollContent = new VisualElement();
            _scrollView.Add(_scrollContent);

            _emptyLabel = new Label("No logs yet");
            _emptyLabel.name = "log_view_empty";
            _emptyLabel.AddToClassList("log-empty");
            _scrollContent.Add(_emptyLabel);

            _container.Add(_scrollView);

            LogManager.Instance.OnLogAdded += OnLogAdded;
            Refresh();

            root.Add(_container);
        }

        private void ToggleExpand()
        {
            _expanded = !_expanded;
            _scrollView.style.display = _expanded ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ClearLogs()
        {
            LogManager.Instance.Clear();
            Refresh();
        }

        private void OnLogAdded(LogEntry entry)
        {
            Refresh();
        }

        private void Refresh()
        {
            _scrollContent.Clear();
            var entries = LogManager.Instance.Entries;
            _countLabel.text = $"({entries.Count})";

            if (entries.Count == 0)
            {
                var empty = new Label("No logs yet");
                empty.name = "log_view_empty";
                empty.AddToClassList("log-empty");
                _scrollContent.Add(empty);
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var row = new VisualElement();
                row.name = $"log_entry_{i}";
                row.AddToClassList("log-entry");

                var ts = new Label(entry.Timestamp.ToString("HH:mm:ss"));
                ts.name = $"log_entry_{i}_timestamp";
                ts.AddToClassList("log-timestamp");

                var level = new Label(entry.LevelChar);
                level.name = $"log_entry_{i}_level";
                level.AddToClassList("log-level");
                level.AddToClassList($"log-level-{entry.LevelChar.ToLower()}");

                var msg = new Label($"[{entry.Tag}] {entry.Message}");
                msg.name = $"log_entry_{i}_message";
                msg.AddToClassList("log-message");

                row.Add(ts);
                row.Add(level);
                row.Add(msg);
                _scrollContent.Add(row);
            }

            _scrollView.schedule.Execute(() =>
            {
                _scrollView.scrollOffset = new Vector2(0, float.MaxValue);
            });
        }

        public void Destroy()
        {
            LogManager.Instance.OnLogAdded -= OnLogAdded;
        }
    }
}
