using System;
using OneSignalDemo.UI;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public static class SectionBuilder
    {
        public static VisualElement CreateSection(string title, string name, Action onInfoTap = null)
        {
            var section = new VisualElement();
            section.name = name;
            section.AddToClassList("section-container");

            var header = new VisualElement();
            header.AddToClassList("section-header");

            var titleLabel = new Label(title.ToUpperInvariant());
            titleLabel.AddToClassList("section-title");
            titleLabel.AddToClassList("text-section-header");
            header.Add(titleLabel);

            if (onInfoTap != null)
            {
                var infoBtn = new Button(onInfoTap);
                infoBtn.name = $"{name}_info";
                infoBtn.text = MaterialIcons.Info;
                infoBtn.AddToClassList("info-button");
                header.Add(infoBtn);
            }

            section.Add(header);
            return section;
        }

        public static VisualElement CreateCard(string name = null)
        {
            var card = new VisualElement();
            if (!string.IsNullOrEmpty(name))
                card.name = name;
            card.AddToClassList("card");
            return card;
        }

        public static VisualElement CreateToggleRow(string label, string description,
            string name, bool initialValue, Action<bool> onChanged)
        {
            var row = new VisualElement();
            row.AddToClassList("toggle-row");

            var labelContainer = new VisualElement();
            labelContainer.AddToClassList("toggle-label-container");

            var labelElement = new Label(label);
            labelElement.AddToClassList("toggle-label");
            labelElement.AddToClassList("text-toggle-label");
            labelContainer.Add(labelElement);

            if (!string.IsNullOrEmpty(description))
            {
                var desc = new Label(description);
                desc.AddToClassList("toggle-description");
                desc.AddToClassList("text-toggle-desc");
                labelContainer.Add(desc);
            }

            row.Add(labelContainer);

            var toggle = new SwitchToggle();
            toggle.name = name;
            toggle.SetValueWithoutNotify(initialValue);
            toggle.ValueChanged += v => onChanged?.Invoke(v);
            row.Add(toggle);

            return row;
        }

        public static Button CreatePrimaryButton(string text, string name, Action onClick)
        {
            var btn = new Button(onClick);
            btn.name = name;
            btn.text = text;
            btn.AddToClassList("primary-button");
            return btn;
        }

        public static Button CreateDestructiveButton(string text, string name, Action onClick)
        {
            var btn = new Button(onClick);
            btn.name = name;
            btn.text = text;
            btn.AddToClassList("destructive-button");
            return btn;
        }

        public static VisualElement CreateDivider(bool tight = false)
        {
            var divider = new VisualElement();
            divider.AddToClassList("divider");
            if (tight)
                divider.AddToClassList("divider--tight");
            return divider;
        }

        public static VisualElement CreateKeyValueItem(string key, string value,
            string name = null, Action onDelete = null)
        {
            var item = new VisualElement();
            item.AddToClassList("key-value-item");
            if (!string.IsNullOrEmpty(name))
                item.name = name;

            var texts = new VisualElement();
            texts.AddToClassList("key-value-texts");

            var keyLabel = new Label(key);
            keyLabel.AddToClassList("key-value-key");
            keyLabel.AddToClassList("text-card-label");
            texts.Add(keyLabel);

            var valueLabel = new Label(value);
            valueLabel.AddToClassList("key-value-value");
            valueLabel.AddToClassList("text-toggle-desc");
            texts.Add(valueLabel);

            item.Add(texts);

            if (onDelete != null)
            {
                var deleteBtn = new Button(onDelete);
                deleteBtn.text = MaterialIcons.Close;
                deleteBtn.AddToClassList("delete-button");
                item.Add(deleteBtn);
            }

            return item;
        }

        public static VisualElement CreateInlineKeyValue(string key, string value, string name = null)
        {
            var row = new VisualElement();
            row.AddToClassList("key-value-inline");
            if (!string.IsNullOrEmpty(name))
                row.name = name;

            var keyLabel = new Label(key);
            keyLabel.AddToClassList("key-value-inline-key");
            keyLabel.AddToClassList("text-body-medium");
            row.Add(keyLabel);

            var valueLabel = new Label(value);
            valueLabel.name = name != null ? $"{name}_value" : null;
            valueLabel.AddToClassList("key-value-inline-value");
            valueLabel.AddToClassList("text-card-value");
            row.Add(valueLabel);

            return row;
        }

        public static VisualElement CreateSingleItem(string value, string name = null, Action onDelete = null)
        {
            var item = new VisualElement();
            item.AddToClassList("key-value-item");
            if (!string.IsNullOrEmpty(name))
                item.name = name;

            var label = new Label(value);
            label.AddToClassList("key-value-key");
            label.AddToClassList("text-card-label");
            label.AddToClassList("flex-grow");
            item.Add(label);

            if (onDelete != null)
            {
                var deleteBtn = new Button(onDelete);
                deleteBtn.text = MaterialIcons.Close;
                deleteBtn.AddToClassList("delete-button");
                item.Add(deleteBtn);
            }

            return item;
        }

        public static Label CreateEmptyState(string text)
        {
            var label = new Label(text);
            label.AddToClassList("empty-state");
            label.AddToClassList("text-empty-state");
            return label;
        }
    }
}
