using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public class MultiPairInputDialog : DialogBase
    {
        private readonly string _title;
        private readonly string _keyLabel;
        private readonly string _valueLabel;
        private readonly string _confirmText;
        private readonly Action<Dictionary<string, string>> _onConfirm;
        private readonly List<(TextField key, TextField value, VisualElement row)> _rows = new();
        private VisualElement _rowsContainer;
        private Button _confirmButton;

        public MultiPairInputDialog(string title, string keyLabel, string valueLabel,
            string confirmText, Action<Dictionary<string, string>> onConfirm)
        {
            _title = title;
            _keyLabel = keyLabel;
            _valueLabel = valueLabel;
            _confirmText = confirmText;
            _onConfirm = onConfirm;
        }

        protected override void BuildContent(VisualElement container)
        {
            var title = new Label(_title);
            title.AddToClassList("dialog-title");
            container.Add(title);

            _rowsContainer = new VisualElement();
            container.Add(_rowsContainer);

            AddRow();

            var addRowButton = new Button(AddRow);
            addRowButton.text = "+ Add Row";
            addRowButton.AddToClassList("dialog-add-row-button");
            container.Add(addRowButton);

            var actions = new VisualElement();
            actions.AddToClassList("dialog-actions");

            actions.Add(CreateCancelButton());

            _confirmButton = CreateConfirmButton(_confirmText, OnConfirm);
            _confirmButton.SetEnabled(false);
            actions.Add(_confirmButton);

            container.Add(actions);
        }

        private void AddRow()
        {
            if (_rows.Count > 0)
            {
                var divider = new VisualElement();
                divider.AddToClassList("divider");
                _rowsContainer.Add(divider);
            }

            var row = new VisualElement();
            row.AddToClassList("dialog-row");

            var keyField = new TextField();
            keyField.name = $"multi_key_{_rows.Count}";
            keyField.AddToClassList("input-field");
            keyField.AddToClassList("dialog-field-group-left");
            keyField.textEdition.placeholder = _keyLabel;
            keyField.RegisterValueChangedCallback(_ => ValidateAll());

            var valueField = new TextField();
            valueField.name = $"multi_value_{_rows.Count}";
            valueField.AddToClassList("input-field");
            valueField.AddToClassList("dialog-field-group");
            valueField.textEdition.placeholder = _valueLabel;
            valueField.RegisterValueChangedCallback(_ => ValidateAll());

            row.Add(keyField);
            row.Add(valueField);

            var entry = (keyField, valueField, row);

            var deleteBtn = new Button(() => RemoveRow(entry));
            deleteBtn.text = "\uE5CD";
            deleteBtn.AddToClassList("dialog-row-delete");
            row.Add(deleteBtn);

            _rows.Add(entry);
            _rowsContainer.Add(row);

            UpdateDeleteVisibility();
            ValidateAll();
        }

        private void RemoveRow((TextField key, TextField value, VisualElement row) entry)
        {
            _rows.Remove(entry);
            _rowsContainer.Clear();

            for (int i = 0; i < _rows.Count; i++)
            {
                if (i > 0)
                {
                    var divider = new VisualElement();
                    divider.AddToClassList("divider");
                    _rowsContainer.Add(divider);
                }
                _rowsContainer.Add(_rows[i].row);
            }

            UpdateDeleteVisibility();
            ValidateAll();
        }

        private void UpdateDeleteVisibility()
        {
            foreach (var (_, _, row) in _rows)
            {
                var del = row.Q<Button>(className: "dialog-row-delete");
                if (del != null)
                    del.style.display = _rows.Count > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void ValidateAll()
        {
            bool allValid = _rows.Count > 0;
            foreach (var (key, value, _) in _rows)
            {
                if (string.IsNullOrEmpty(key.value) || string.IsNullOrEmpty(value.value))
                {
                    allValid = false;
                    break;
                }
            }
            _confirmButton?.SetEnabled(allValid);
        }

        private void OnConfirm()
        {
            var dict = new Dictionary<string, string>();
            foreach (var (key, value, _) in _rows)
            {
                if (!string.IsNullOrEmpty(key.value) && !string.IsNullOrEmpty(value.value))
                    dict[key.value] = value.value;
            }

            if (dict.Count > 0)
            {
                _onConfirm?.Invoke(dict);
                Dismiss();
            }
        }
    }
}
