using System;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public class CustomNotificationDialog : DialogBase
    {
        private readonly Action<string, string> _onConfirm;
        private TextField _titleField;
        private TextField _bodyField;
        private Button _confirmButton;

        public CustomNotificationDialog(Action<string, string> onConfirm)
        {
            _onConfirm = onConfirm;
        }

        protected override void BuildContent(VisualElement container)
        {
            var title = new Label("Custom Notification");
            title.AddToClassList("dialog-title");
            title.AddToClassList("text-dialog-title");
            container.Add(title);

            var titleLabel = new Label("Title");
            titleLabel.AddToClassList("input-label");
            titleLabel.AddToClassList("text-body-small");
            container.Add(titleLabel);

            _titleField = new TextField();
            _titleField.name = "custom_notif_title";
            _titleField.AddToClassList("input-field");
            _titleField.RegisterValueChangedCallback(_ => ValidateInput());
            container.Add(_titleField);

            var bodyLabel = new Label("Body");
            bodyLabel.AddToClassList("input-label");
            bodyLabel.AddToClassList("text-body-small");
            container.Add(bodyLabel);

            _bodyField = new TextField();
            _bodyField.name = "custom_notif_body";
            _bodyField.AddToClassList("input-field");
            _bodyField.RegisterValueChangedCallback(_ => ValidateInput());
            container.Add(_bodyField);

            var actions = new VisualElement();
            actions.AddToClassList("dialog-actions");

            actions.Add(CreateCancelButton());

            _confirmButton = CreateConfirmButton("Send", OnConfirm);
            _confirmButton.name = "custom_notif_confirm";
            _confirmButton.SetEnabled(false);
            actions.Add(_confirmButton);

            container.Add(actions);
        }

        private void ValidateInput()
        {
            bool valid = !string.IsNullOrEmpty(_titleField?.value) &&
                         !string.IsNullOrEmpty(_bodyField?.value);
            _confirmButton?.SetEnabled(valid);
        }

        private void OnConfirm()
        {
            var t = _titleField?.value;
            var b = _bodyField?.value;
            if (!string.IsNullOrEmpty(t) && !string.IsNullOrEmpty(b))
            {
                _onConfirm?.Invoke(t, b);
                Dismiss();
            }
        }
    }
}
