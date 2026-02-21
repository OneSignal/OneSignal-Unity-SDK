using System;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public class PairInputDialog : DialogBase
    {
        private readonly string _title;
        private readonly string _keyLabel;
        private readonly string _valueLabel;
        private readonly string _keyName;
        private readonly string _valueName;
        private readonly string _confirmText;
        private readonly Action<string, string> _onConfirm;
        private TextField _keyField;
        private TextField _valueField;
        private Button _confirmButton;

        public PairInputDialog(string title, string keyLabel, string valueLabel,
            string keyName, string valueName, string confirmText,
            Action<string, string> onConfirm)
        {
            _title = title;
            _keyLabel = keyLabel;
            _valueLabel = valueLabel;
            _keyName = keyName;
            _valueName = valueName;
            _confirmText = confirmText;
            _onConfirm = onConfirm;
        }

        protected override void BuildContent(VisualElement container)
        {
            var title = new Label(_title);
            title.AddToClassList("dialog-title");
            container.Add(title);

            var row = new VisualElement();
            row.AddToClassList("dialog-row");

            var keyContainer = new VisualElement();
            keyContainer.style.flexGrow = 1;
            keyContainer.style.marginRight = 8;
            var keyLabel = new Label(_keyLabel);
            keyLabel.AddToClassList("input-label");
            keyContainer.Add(keyLabel);
            _keyField = new TextField();
            _keyField.name = _keyName;
            _keyField.AddToClassList("input-field");
            _keyField.RegisterValueChangedCallback(_ => ValidateInput());
            keyContainer.Add(_keyField);

            var valueContainer = new VisualElement();
            valueContainer.style.flexGrow = 1;
            var valueLabel = new Label(_valueLabel);
            valueLabel.AddToClassList("input-label");
            valueContainer.Add(valueLabel);
            _valueField = new TextField();
            _valueField.name = _valueName;
            _valueField.AddToClassList("input-field");
            _valueField.RegisterValueChangedCallback(_ => ValidateInput());
            valueContainer.Add(_valueField);

            row.Add(keyContainer);
            row.Add(valueContainer);
            container.Add(row);

            var actions = new VisualElement();
            actions.AddToClassList("dialog-actions");

            actions.Add(CreateCancelButton());

            _confirmButton = CreateConfirmButton(_confirmText, OnConfirm);
            _confirmButton.SetEnabled(false);
            actions.Add(_confirmButton);

            container.Add(actions);
        }

        private void ValidateInput()
        {
            bool valid = !string.IsNullOrEmpty(_keyField?.value) &&
                         !string.IsNullOrEmpty(_valueField?.value);
            _confirmButton?.SetEnabled(valid);
        }

        private void OnConfirm()
        {
            var key = _keyField?.value;
            var value = _valueField?.value;
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                _onConfirm?.Invoke(key, value);
                Dismiss();
            }
        }
    }
}
