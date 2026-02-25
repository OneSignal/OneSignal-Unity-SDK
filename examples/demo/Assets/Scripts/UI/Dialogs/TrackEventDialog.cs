using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public class TrackEventDialog : DialogBase
    {
        private readonly Action<string, Dictionary<string, object>> _onConfirm;
        private TextField _nameField;
        private TextField _propertiesField;
        private Label _jsonError;
        private Button _confirmButton;
        private bool _jsonValid = true;

        public TrackEventDialog(Action<string, Dictionary<string, object>> onConfirm)
        {
            _onConfirm = onConfirm;
        }

        protected override void BuildContent(VisualElement container)
        {
            var title = new Label("Track Event");
            title.AddToClassList("dialog-title");
            title.AddToClassList("text-dialog-title");
            container.Add(title);

            var nameLabel = new Label("Event Name");
            nameLabel.AddToClassList("input-label");
            nameLabel.AddToClassList("text-body-small");
            container.Add(nameLabel);

            _nameField = new TextField();
            _nameField.name = "event_name";
            _nameField.AddToClassList("input-field");
            _nameField.RegisterValueChangedCallback(_ => ValidateInput());
            container.Add(_nameField);

            var propsLabel = new Label("Properties (optional, JSON)");
            propsLabel.AddToClassList("input-label");
            propsLabel.AddToClassList("text-body-small");
            container.Add(propsLabel);

            _propertiesField = new TextField();
            _propertiesField.name = "event_properties";
            _propertiesField.AddToClassList("input-field");
            _propertiesField.textEdition.placeholder = "{\"key\": \"value\"}";
            _propertiesField.RegisterValueChangedCallback(_ => ValidateInput());
            container.Add(_propertiesField);

            _jsonError = new Label("Invalid JSON format");
            _jsonError.AddToClassList("input-error");
            _jsonError.style.display = DisplayStyle.None;
            container.Add(_jsonError);

            var actions = new VisualElement();
            actions.AddToClassList("dialog-actions");

            actions.Add(CreateCancelButton());

            _confirmButton = CreateConfirmButton("Track", OnConfirm);
            _confirmButton.name = "event_confirm_button";
            _confirmButton.SetEnabled(false);
            actions.Add(_confirmButton);

            container.Add(actions);
        }

        private void ValidateInput()
        {
            bool nameValid = !string.IsNullOrEmpty(_nameField?.value);
            _jsonValid = true;

            var propsText = _propertiesField?.value;
            if (!string.IsNullOrEmpty(propsText))
            {
                try
                {
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(propsText);
                }
                catch
                {
                    _jsonValid = false;
                }
            }

            _jsonError.style.display = _jsonValid ? DisplayStyle.None : DisplayStyle.Flex;
            _confirmButton?.SetEnabled(nameValid && _jsonValid);
        }

        private void OnConfirm()
        {
            var name = _nameField?.value;
            if (string.IsNullOrEmpty(name))
                return;

            Dictionary<string, object> props = null;
            var propsText = _propertiesField?.value;
            if (!string.IsNullOrEmpty(propsText))
            {
                try
                {
                    props = JsonConvert.DeserializeObject<Dictionary<string, object>>(propsText);
                }
                catch
                {
                    return;
                }
            }

            _onConfirm?.Invoke(name, props);
            Dismiss();
        }
    }
}
