using System;
using System.Collections.Generic;
using OneSignalDemo.Services;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public class LanguagePickerDialog : DialogBase
    {
        private readonly IReadOnlyList<KeyValuePair<string, string>> _options;
        private readonly string _selectedCode;
        private readonly Action<string> _onSelected;

        /// <param name="options">Display name to language code pairs.</param>
        public LanguagePickerDialog(
            IReadOnlyList<KeyValuePair<string, string>> options,
            string selectedCode,
            Action<string> onSelected
        )
        {
            _options = options;
            _selectedCode = selectedCode ?? "";
            _onSelected = onSelected;
        }

        protected override void BuildContent(VisualElement container)
        {
            var title = new Label("Language");
            title.AddToClassList("dialog-title");
            title.AddToClassList("text-dialog-title");
            container.Add(title);

            var list = new ScrollView(ScrollViewMode.Vertical);
            list.name = "language_option_list";
            list.AddToClassList("language-option-list");

            foreach (var option in _options)
            {
                var code = option.Value;
                var row = new VisualElement();
                row.name = $"language_option_{(code.Length == 0 ? "default" : code)}";
                row.AddToClassList("language-option");

                var label = new Label(option.Key);
                label.AddToClassList("language-option-label");
                label.AddToClassList("text-card-label");
                row.Add(label);

                if (code == _selectedCode)
                {
                    row.AddToClassList("language-option--selected");
                    var check = new Label(MaterialIcons.Check);
                    check.AddToClassList("material-icon");
                    check.AddToClassList("language-option-check");
                    row.Add(check);
                }

                void Select() => OnSelected(code);
                row.AddManipulator(new Clickable(Select));
                AccessibilityBridge.RegisterE2ETapTarget(row, () => true, Select);
                list.Add(row);
            }

            container.Add(list);

            var actions = new VisualElement();
            actions.AddToClassList("dialog-actions");
            actions.Add(CreateCancelButton());
            container.Add(actions);
        }

        private void OnSelected(string code)
        {
            _onSelected?.Invoke(code);
            Dismiss();
        }
    }
}
