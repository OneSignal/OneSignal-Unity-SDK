using System;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public class SingleInputDialog : DialogBase
    {
        private readonly string _title;
        private readonly string _fieldLabel;
        private readonly string _fieldName;
        private readonly string _confirmText;
        private readonly Action<string> _onConfirm;
        private TextField _inputField;
        private Button _confirmButton;

        public SingleInputDialog(
            string title,
            string fieldLabel,
            string fieldName,
            string confirmText,
            Action<string> onConfirm
        )
        {
            _title = title;
            _fieldLabel = fieldLabel;
            _fieldName = fieldName;
            _confirmText = confirmText;
            _onConfirm = onConfirm;
        }

        protected override void BuildContent(VisualElement container)
        {
            var title = new Label(_title);
            title.AddToClassList("dialog-title");
            title.AddToClassList("text-dialog-title");
            container.Add(title);

            var label = new Label(_fieldLabel);
            label.AddToClassList("input-label");
            label.AddToClassList("text-body-small");
            container.Add(label);

            _inputField = new TextField();
            _inputField.name = _fieldName;
            _inputField.AddToClassList("input-field");
            _inputField.RegisterValueChangedCallback(_ => ValidateInput());
            container.Add(_inputField);

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
            _confirmButton?.SetEnabled(!string.IsNullOrEmpty(_inputField?.value));
        }

        private void OnConfirm()
        {
            var value = _inputField?.value;
            if (!string.IsNullOrEmpty(value))
            {
                _onConfirm?.Invoke(value);
                Dismiss();
            }
        }
    }
}
