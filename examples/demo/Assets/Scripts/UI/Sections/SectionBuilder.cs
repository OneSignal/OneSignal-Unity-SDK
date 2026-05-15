using System;
using OneSignalDemo.Services;
using OneSignalDemo.UI;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public static class SectionBuilder
    {
        public static VisualElement CreateSection(
            string title,
            string name,
            Action onInfoTap = null
        )
        {
            var section = new VisualElement();
            section.AddToClassList("section-container");

            var header = new VisualElement();
            header.AddToClassList("section-header");

            var titleLabel = new Label(title.ToUpperInvariant());
            titleLabel.name = name;
            titleLabel.AddToClassList("section-title");
            titleLabel.AddToClassList("text-section-header");
            header.Add(titleLabel);

            if (onInfoTap != null)
            {
                // Plain Label (no Button/Clickable manipulator). On iOS, UI
                // Toolkit's AtTarget dispatch on this Label is observed to
                // drop the PointerDown whenever Appium injects a mobile:scroll
                // just before the tap (every spec's `before` hook scrolls to
                // its section first). The bridge installs a panel-root
                // TrickleDown handler that dispatches by name — bypasses the
                // dropped AtTarget. On Android, the same registration drives
                // the UiAutomator2 click route.
                var infoBtn = new Label(MaterialIcons.Info);
                infoBtn.name = $"{SectionKeyFromName(name)}_info_icon";
                infoBtn.AddToClassList("info-button");
                infoBtn.pickingMode = PickingMode.Position;
                AccessibilityBridge.RegisterE2ETapTarget(
                    infoBtn,
                    () => infoBtn.panel?.visualTree.Q("tooltip_title") == null,
                    onInfoTap
                );
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

        public static VisualElement CreateToggleRow(
            string label,
            string description,
            string name,
            bool initialValue,
            Action<bool> onChanged
        )
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
            AccessibilityBridge.RegisterE2ETapTarget(btn, () => btn.enabledInHierarchy, onClick);
            return btn;
        }

        public static Button CreateDestructiveButton(string text, string name, Action onClick)
        {
            var btn = new Button(onClick);
            btn.name = name;
            btn.text = text;
            btn.AddToClassList("destructive-button");
            AccessibilityBridge.RegisterE2ETapTarget(btn, () => btn.enabledInHierarchy, onClick);
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

        public static VisualElement CreateKeyValueItem(
            string key,
            string value,
            string sectionKey = null,
            string itemKey = null,
            Action onDelete = null
        )
        {
            var item = new VisualElement();
            item.AddToClassList("key-value-item");

            var texts = new VisualElement();
            texts.AddToClassList("key-value-texts");

            var keyLabel = new Label(key);
            if (sectionKey != null && itemKey != null)
                keyLabel.name = $"{sectionKey}_pair_key_{itemKey}";
            keyLabel.AddToClassList("key-value-key");
            keyLabel.AddToClassList("text-card-label");
            texts.Add(keyLabel);

            var valueLabel = new Label(value);
            if (sectionKey != null && itemKey != null)
                valueLabel.name = $"{sectionKey}_pair_value_{itemKey}";
            valueLabel.AddToClassList("key-value-value");
            valueLabel.AddToClassList("text-toggle-desc");
            texts.Add(valueLabel);

            item.Add(texts);

            if (onDelete != null)
            {
                var deleteBtn = new Button(onDelete);
                if (sectionKey != null && itemKey != null)
                    deleteBtn.name = $"{sectionKey}_remove_{itemKey}";
                deleteBtn.text = MaterialIcons.Close;
                deleteBtn.AddToClassList("delete-button");
            AccessibilityBridge.RegisterE2ETapTarget(deleteBtn, () => true, onDelete);
                item.Add(deleteBtn);
            }

            return item;
        }

        public static VisualElement CreateInlineKeyValue(
            string key,
            string value,
            string name = null
        )
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

        public static VisualElement CreateSingleItem(
            string value,
            string sectionKey = null,
            Action onDelete = null
        )
        {
            var item = new VisualElement();
            item.AddToClassList("key-value-item");

            var label = new Label(value);
            if (sectionKey != null)
                label.name = $"{sectionKey}_value_{value}";
            label.AddToClassList("key-value-key");
            label.AddToClassList("text-card-label");
            label.AddToClassList("flex-grow");
            item.Add(label);

            if (onDelete != null)
            {
                var deleteBtn = new Button(onDelete);
                if (sectionKey != null)
                    deleteBtn.name = $"{sectionKey}_remove_{value}";
                deleteBtn.text = MaterialIcons.Close;
                deleteBtn.AddToClassList("delete-button");
            AccessibilityBridge.RegisterE2ETapTarget(deleteBtn, () => true, onDelete);
                item.Add(deleteBtn);
            }

            return item;
        }

        public static Label CreateEmptyState(string text, string sectionKey = null)
        {
            var label = new Label(text);
            if (sectionKey != null)
                label.name = $"{sectionKey}_empty";
            label.AddToClassList("empty-state");
            label.AddToClassList("text-empty-state");
            return label;
        }

        public static Label CreateLoadingState(string sectionKey)
        {
            var label = new Label("Loading…");
            label.name = $"{sectionKey}_loading";
            label.AddToClassList("empty-state");
            label.AddToClassList("text-empty-state");
            return label;
        }

        private static string SectionKeyFromName(string name) =>
            name != null && name.EndsWith("_section")
                ? name.Substring(0, name.Length - "_section".Length)
                : name;
    }
}
