using System;
using System.Collections.Generic;
using System.Linq;
using OneSignalDemo.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public class MultiSelectRemoveDialog : DialogBase
    {
        private readonly string _title;
        private readonly List<KeyValuePair<string, string>> _items;
        private readonly Action<List<string>> _onConfirm;
        private readonly Dictionary<string, Toggle> _toggles = new();
        private readonly Dictionary<string, double> _lastToggleChangeMs = new();
        private Button _confirmButton;
        private const double ToggleDedupeMs = 500.0;

        public MultiSelectRemoveDialog(
            string title,
            List<KeyValuePair<string, string>> items,
            Action<List<string>> onConfirm
        )
        {
            _title = title;
            _items = items;
            _onConfirm = onConfirm;
        }

        protected override void BuildContent(VisualElement container)
        {
            var title = new Label(_title);
            title.AddToClassList("dialog-title");
            title.AddToClassList("text-dialog-title");
            container.Add(title);

            foreach (var item in _items)
            {
                var row = new VisualElement();
                row.AddToClassList("checkbox-row");

                var toggle = new Toggle();
                toggle.name = $"remove_checkbox_{item.Key}";
                toggle.RegisterValueChangedCallback(evt =>
                {
                    _lastToggleChangeMs[item.Key] =
                        Time.realtimeSinceStartupAsDouble * 1000.0;
                    toggle.EnableInClassList("checkbox--checked", evt.newValue);
                    UpdateCount();
                });
                row.Add(toggle);

                var label = new Label(item.Key);
                label.AddToClassList("checkbox-label");
                label.AddToClassList("text-card-label");
                row.Add(label);

                _toggles[item.Key] = toggle;
                AccessibilityBridge.RegisterE2ETapFallback(
                    row,
                    () => toggle.enabledInHierarchy,
                    () => ToggleSelection(item.Key, toggle)
                );
                AccessibilityBridge.RegisterE2ETapFallback(
                    toggle,
                    () => toggle.enabledInHierarchy,
                    () => ToggleSelection(item.Key, toggle)
                );
                container.Add(row);
            }

            var actions = new VisualElement();
            actions.AddToClassList("dialog-actions");

            actions.Add(CreateCancelButton());

            _confirmButton = CreateConfirmButton("Remove (0)", OnConfirm);
            _confirmButton.name = "multiselect_confirm_button";
            _confirmButton.SetEnabled(false);
            actions.Add(_confirmButton);

            container.Add(actions);
        }

        private void UpdateCount()
        {
            int count = _toggles.Values.Count(t => t.value);
            _confirmButton.text = $"Remove ({count})";
            _confirmButton.SetEnabled(count > 0);
        }

        private void ToggleSelection(string key, Toggle toggle)
        {
            double now = Time.realtimeSinceStartupAsDouble * 1000.0;
            if (_lastToggleChangeMs.TryGetValue(key, out var last) && now - last < ToggleDedupeMs)
                return;

            _lastToggleChangeMs[key] = now;
            toggle.value = !toggle.value;
            toggle.EnableInClassList("checkbox--checked", toggle.value);
            UpdateCount();
        }

        private void OnConfirm()
        {
            var selected = _toggles.Where(kvp => kvp.Value.value).Select(kvp => kvp.Key).ToList();

            if (selected.Count > 0)
            {
                _onConfirm?.Invoke(selected);
                Dismiss();
            }
        }
    }
}
