using System;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public class LoginWithProfileDialog : DialogBase
    {
        private readonly Action<string, string, string, string> _onConfirm;
        private TextField _externalIdField;
        private TextField _emailField;
        private TextField _smsField;
        private TextField _jwtField;
        private Button _confirmButton;

        public LoginWithProfileDialog(Action<string, string, string, string> onConfirm)
        {
            _onConfirm = onConfirm;
        }

        protected override void BuildContent(VisualElement container)
        {
            var title = new Label("Login With Profile");
            title.AddToClassList("dialog-title");
            title.AddToClassList("text-dialog-title");
            container.Add(title);

            _externalIdField = CreateTextField("External User Id", "composite_login_user_id_input");
            _externalIdField.RegisterValueChangedCallback(_ => ValidateInput());
            container.Add(_externalIdField);

            _emailField = CreateTextField("Email (optional)", "composite_login_email_input");
            container.Add(_emailField);

            _smsField = CreateTextField("SMS (optional)", "composite_login_sms_input");
            container.Add(_smsField);

            _jwtField = CreateTextField("JWT Token (optional)", "composite_login_jwt_input");
            container.Add(_jwtField);

            var actions = new VisualElement();
            actions.AddToClassList("dialog-actions");

            actions.Add(CreateCancelButton());

            _confirmButton = CreateConfirmButton("Login", OnConfirm);
            _confirmButton.name = "composite_login_confirm_button";
            _confirmButton.SetEnabled(false);
            actions.Add(_confirmButton);

            container.Add(actions);
        }

        private void ValidateInput()
        {
            _confirmButton?.SetEnabled(!string.IsNullOrWhiteSpace(_externalIdField?.value));
        }

        private void OnConfirm()
        {
            var externalId = _externalIdField?.value?.Trim();
            if (string.IsNullOrEmpty(externalId))
                return;

            _externalIdField?.Blur();
            _onConfirm?.Invoke(
                externalId,
                _emailField?.value?.Trim(),
                _smsField?.value?.Trim(),
                _jwtField?.value?.Trim()
            );
            Dismiss();
        }
    }
}
