using System;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public class LoginDialog : DialogBase
    {
        private readonly Action<string> _onConfirm;
        private readonly bool _isSwitchUser;
        private TextField _externalIdField;
        private Button _confirmButton;

        public LoginDialog(Action<string> onConfirm, bool isSwitchUser = false)
        {
            _onConfirm = onConfirm;
            _isSwitchUser = isSwitchUser;
        }

        protected override void BuildContent(VisualElement container)
        {
            var title = new Label(_isSwitchUser ? "Switch User" : "Login User");
            title.AddToClassList("dialog-title");
            container.Add(title);

            var label = new Label("External User Id");
            label.AddToClassList("input-label");
            container.Add(label);

            _externalIdField = new TextField();
            _externalIdField.name = "login_external_id";
            _externalIdField.AddToClassList("input-field");
            _externalIdField.RegisterValueChangedCallback(_ => ValidateInput());
            container.Add(_externalIdField);

            var actions = new VisualElement();
            actions.AddToClassList("dialog-actions");

            actions.Add(CreateCancelButton());

            _confirmButton = CreateConfirmButton(_isSwitchUser ? "SWITCH" : "LOGIN", OnConfirm);
            _confirmButton.name = "login_confirm_button";
            _confirmButton.SetEnabled(false);
            actions.Add(_confirmButton);

            container.Add(actions);
        }

        private void ValidateInput()
        {
            _confirmButton?.SetEnabled(!string.IsNullOrEmpty(_externalIdField?.value));
        }

        private void OnConfirm()
        {
            var value = _externalIdField?.value;
            if (!string.IsNullOrEmpty(value))
            {
                _onConfirm?.Invoke(value);
                Dismiss();
            }
        }
    }
}
