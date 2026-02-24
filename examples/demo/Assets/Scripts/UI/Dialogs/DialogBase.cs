using System;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public abstract class DialogBase
    {
        protected VisualElement Overlay { get; private set; }
        protected VisualElement Container { get; private set; }
        private VisualElement _parent;

        public void Show(VisualElement parent)
        {
            _parent = parent;

            Overlay = new VisualElement();
            Overlay.AddToClassList("dialog-overlay");
            Overlay.RegisterCallback<ClickEvent>(e =>
            {
                if (e.target == Overlay) Dismiss();
            });

            Container = new VisualElement();
            Container.AddToClassList("dialog-container");

            BuildContent(Container);

            Overlay.Add(Container);
            parent.Add(Overlay);
        }

        public void Dismiss()
        {
            Overlay?.RemoveFromHierarchy();
        }

        protected abstract void BuildContent(VisualElement container);

        protected TextField CreateTextField(string label, string name, string placeholder = "")
        {
            if (!string.IsNullOrEmpty(label))
            {
                var lbl = new Label(label);
                lbl.AddToClassList("input-label");
                lbl.AddToClassList("text-body-small");
                Container.Add(lbl);
            }

            var field = new TextField();
            field.name = name;
            field.AddToClassList("input-field");
            if (!string.IsNullOrEmpty(placeholder))
                field.textEdition.placeholder = placeholder;
            return field;
        }

        protected Button CreateConfirmButton(string text, Action onClick)
        {
            var btn = new Button(onClick);
            btn.text = text;
            btn.AddToClassList("dialog-confirm-button");
            btn.AddToClassList("text-dialog-action");
            return btn;
        }

        protected Button CreateCancelButton(string text = "Cancel")
        {
            var btn = new Button(Dismiss);
            btn.text = text;
            btn.AddToClassList("dialog-cancel-button");
            btn.AddToClassList("text-dialog-action");
            return btn;
        }
    }
}
