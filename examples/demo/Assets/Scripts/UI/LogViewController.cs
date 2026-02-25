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
        private readonly Button _clearButton;
        private readonly Button _chevronButton;
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

            var title = new Label("LOGS");
            title.AddToClassList("log-header-title");
            title.AddToClassList("text-label-small");
            headerLeft.Add(title);

            _countLabel = new Label("(0)");
            _countLabel.name = "log_view_count";
            _countLabel.AddToClassList("log-header-count");
            _countLabel.AddToClassList("text-label-small");
            headerLeft.Add(_countLabel);

            header.Add(headerLeft);

            var headerRight = new VisualElement();
            headerRight.AddToClassList("log-header-right");

            _clearButton = new Button(ClearLogs);
            _clearButton.name = "log_view_clear_button";
            _clearButton.text = MaterialIcons.Delete;
            _clearButton.AddToClassList("log-clear-button");
            headerRight.Add(_clearButton);

            _chevronButton = new Button(ToggleExpand);
            _chevronButton.name = "log_view_toggle";
            _chevronButton.text = MaterialIcons.ExpandLess;
            _chevronButton.AddToClassList("log-clear-button");
            headerRight.Add(_chevronButton);

            header.Add(headerRight);

            _container.Add(header);

            _scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
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
            _chevronButton.text = _expanded ? MaterialIcons.ExpandLess : MaterialIcons.ExpandMore;
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
            _clearButton.style.display = entries.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;

            if (entries.Count == 0)
            {
                var empty = new Label("No logs yet");
                empty.name = "log_view_empty";
                empty.AddToClassList("log-empty");
                _scrollContent.Add(empty);
                return;
            }

            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var entry = entries[i];
                var displayIndex = entries.Count - 1 - i;
                var row = new VisualElement();
                row.name = $"log_entry_{displayIndex}";
                row.AddToClassList("log-entry");

                var ts = new Label(entry.Timestamp.ToString("HH:mm:ss"));
                ts.name = $"log_entry_{displayIndex}_timestamp";
                ts.AddToClassList("log-timestamp");
                ts.AddToClassList("text-label-small");

                var level = new Label(entry.LevelChar);
                level.name = $"log_entry_{displayIndex}_level";
                level.AddToClassList("log-level");
                level.AddToClassList("text-label-small");
                level.AddToClassList($"log-level-{entry.LevelChar.ToLower()}");

                var msg = new Label($"{entry.Tag}: {entry.Message}");
                msg.name = $"log_entry_{displayIndex}_message";
                msg.AddToClassList("log-message");
                msg.AddToClassList("text-label-small");

                row.Add(ts);
                row.Add(level);
                row.Add(msg);
                _scrollContent.Add(row);
            }
        }

        public void Destroy()
        {
            LogManager.Instance.OnLogAdded -= OnLogAdded;
        }
    }
}
