using System;
using System.Runtime.InteropServices;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public abstract class DialogBase
    {
#if UNITY_IOS && !UNITY_EDITOR
        // Native bridge in Assets/Plugins/iOS/OneSignalDemoKeyboard.mm.
        // Calls [keyWindow endEditing:YES] to dismiss the iOS keyboard view
        // immediately. UIToolkit's TextField.Blur() and TouchScreenKeyboard.
        // Open("").active = false don't reliably tear down the UIKit keyboard
        // when a UIToolkit-owned modal dismisses, leaving it floating over the
        // app and blocking taps on controls behind it.
        [DllImport("__Internal")]
        private static extern void OneSignalDemoEndEditing();
#endif

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
                if (e.target == Overlay)
                    Dismiss();
            });

            Container = new VisualElement();
            Container.AddToClassList("dialog-container");

            BuildContent(Container);

            Overlay.Add(Container);
            parent.Add(Overlay);
        }

        public void Dismiss()
        {
            // Blur is harmless and keeps UIToolkit's focus state consistent,
            // but does NOT close the iOS keyboard on its own (verified at
            // runtime — UIToolkit owns the TouchScreenKeyboard internally and
            // its focus-out path doesn't propagate to the UIKit first
            // responder).
            var textField = Container?.Q<TextField>();
            textField?.Blur();

#if UNITY_IOS && !UNITY_EDITOR
            // Force UIKit to resign first responder so the keyboard view tears
            // down immediately. Without this the keyboard stays floating over
            // the app after the modal closes and blocks taps on controls
            // behind it (e.g. the logout button after login).
            OneSignalDemoEndEditing();
#endif

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
