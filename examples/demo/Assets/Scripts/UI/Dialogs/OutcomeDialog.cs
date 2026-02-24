using System;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public enum OutcomeType { Normal, Unique, WithValue }

    public class OutcomeDialog : DialogBase
    {
        private readonly Action<OutcomeType, string, float> _onConfirm;
        private OutcomeType _selectedType = OutcomeType.Normal;
        private TextField _nameField;
        private TextField _valueField;
        private VisualElement _valueContainer;
        private Button _confirmButton;
        private RadioButton _normalRadio;
        private RadioButton _uniqueRadio;
        private RadioButton _withValueRadio;

        public OutcomeDialog(Action<OutcomeType, string, float> onConfirm)
        {
            _onConfirm = onConfirm;
        }

        protected override void BuildContent(VisualElement container)
        {
            var title = new Label("Send Outcome");
            title.AddToClassList("dialog-title");
            title.AddToClassList("text-dialog-title");
            container.Add(title);

            var radioGroup = new VisualElement();

            _normalRadio = CreateRadio("Normal Outcome", "outcome_normal", true);
            _uniqueRadio = CreateRadio("Unique Outcome", "outcome_unique", false);
            _withValueRadio = CreateRadio("Outcome with Value", "outcome_with_value", false);

            _normalRadio.RegisterValueChangedCallback(e => { if (e.newValue) SelectType(OutcomeType.Normal); });
            _uniqueRadio.RegisterValueChangedCallback(e => { if (e.newValue) SelectType(OutcomeType.Unique); });
            _withValueRadio.RegisterValueChangedCallback(e => { if (e.newValue) SelectType(OutcomeType.WithValue); });

            radioGroup.Add(_normalRadio);
            radioGroup.Add(_uniqueRadio);
            radioGroup.Add(_withValueRadio);
            container.Add(radioGroup);

            _nameField = new TextField();
            _nameField.name = "outcome_name";
            _nameField.AddToClassList("input-field");
            _nameField.textEdition.placeholder = "Outcome Name";
            _nameField.RegisterValueChangedCallback(_ => ValidateInput());
            container.Add(_nameField);

            _valueContainer = new VisualElement();
            _valueContainer.style.display = DisplayStyle.None;

            _valueField = new TextField();
            _valueField.name = "outcome_value";
            _valueField.AddToClassList("input-field");
            _valueField.textEdition.placeholder = "Value";
            _valueField.RegisterValueChangedCallback(_ => ValidateInput());
            _valueContainer.Add(_valueField);

            container.Add(_valueContainer);

            var actions = new VisualElement();
            actions.AddToClassList("dialog-actions");

            actions.Add(CreateCancelButton());

            _confirmButton = CreateConfirmButton("Send", OnConfirm);
            _confirmButton.name = "outcome_confirm_button";
            _confirmButton.SetEnabled(false);
            actions.Add(_confirmButton);

            container.Add(actions);
        }

        private RadioButton CreateRadio(string label, string name, bool selected)
        {
            var radio = new RadioButton(label);
            radio.name = name;
            radio.value = selected;
            radio.AddToClassList("radio-row");
            radio.AddToClassList("text-body-large");
            return radio;
        }

        private void SelectType(OutcomeType type)
        {
            _selectedType = type;
            _normalRadio.SetValueWithoutNotify(type == OutcomeType.Normal);
            _uniqueRadio.SetValueWithoutNotify(type == OutcomeType.Unique);
            _withValueRadio.SetValueWithoutNotify(type == OutcomeType.WithValue);
            _valueContainer.style.display = type == OutcomeType.WithValue ? DisplayStyle.Flex : DisplayStyle.None;
            ValidateInput();
        }

        private void ValidateInput()
        {
            bool valid = !string.IsNullOrEmpty(_nameField?.value);
            if (_selectedType == OutcomeType.WithValue)
                valid = valid && float.TryParse(_valueField?.value, out _);
            _confirmButton?.SetEnabled(valid);
        }

        private void OnConfirm()
        {
            var name = _nameField?.value;
            if (string.IsNullOrEmpty(name)) return;

            float value = 0;
            if (_selectedType == OutcomeType.WithValue)
                float.TryParse(_valueField?.value, out value);

            _onConfirm?.Invoke(_selectedType, name, value);
            Dismiss();
        }
    }
}
