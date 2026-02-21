using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public class MultiSelectRemoveDialog : DialogBase
    {
        private readonly string _title;
        private readonly List<KeyValuePair<string, string>> _items;
        private readonly Action<List<string>> _onConfirm;
        private readonly Dictionary<string, Toggle> _toggles = new();
        private Button _confirmButton;

        public MultiSelectRemoveDialog(string title, List<KeyValuePair<string, string>> items,
            Action<List<string>> onConfirm)
        {
            _title = title;
            _items = items;
            _onConfirm = onConfirm;
        }

        protected override void BuildContent(VisualElement container)
        {
            var title = new Label(_title);
            title.AddToClassList("dialog-title");
            container.Add(title);

            foreach (var item in _items)
            {
                var row = new VisualElement();
                row.AddToClassList("checkbox-row");

                var toggle = new Toggle();
                toggle.name = $"select_{item.Key}";
                toggle.RegisterValueChangedCallback(_ => UpdateCount());
                row.Add(toggle);

                var label = new Label(item.Key);
                label.AddToClassList("checkbox-label");
                row.Add(label);

                _toggles[item.Key] = toggle;
                container.Add(row);
            }

            var actions = new VisualElement();
            actions.AddToClassList("dialog-actions");

            actions.Add(CreateCancelButton());

            _confirmButton = CreateConfirmButton("REMOVE (0)", OnConfirm);
            _confirmButton.SetEnabled(false);
            actions.Add(_confirmButton);

            container.Add(actions);
        }

        private void UpdateCount()
        {
            int count = _toggles.Values.Count(t => t.value);
            _confirmButton.text = $"REMOVE ({count})";
            _confirmButton.SetEnabled(count > 0);
        }

        private void OnConfirm()
        {
            var selected = _toggles
                .Where(kvp => kvp.Value.value)
                .Select(kvp => kvp.Key)
                .ToList();

            if (selected.Count > 0)
            {
                _onConfirm?.Invoke(selected);
                Dismiss();
            }
        }
    }
}
