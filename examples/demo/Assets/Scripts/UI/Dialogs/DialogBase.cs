using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public abstract class DialogBase
    {
        // Hooks a panel-root PointerDown handler that dispatches taps on
        // section info icons (elements named "*_info_icon") through a
        // name->Action lookup registered by SectionBuilder. UIToolkit's
        // normal AtTarget dispatch was observed to drop PointerDown on the
        // info Label after iOS Appium injected a mobile:scroll gesture in
        // the same test (the panel root sees the event with the correct
        // target, but the target's own callback never fires). Dispatching
        // from the panel root sidesteps whatever state breaks normal
        // dispatch and keeps E2E taps reliable.
        private static bool _infoFallbackHooked;
        private static readonly Dictionary<string, Action> TapByName =
            new Dictionary<string, Action>();

        public static bool TryGetNamedTapAction(string name, out Action action)
        {
            action = null;
            if (string.IsNullOrEmpty(name))
                return false;
            return TapByName.TryGetValue(name, out action);
        }

        protected static void RegisterNamedTap(string name, Action action)
        {
            if (!string.IsNullOrEmpty(name) && action != null)
                TapByName[name] = action;
        }

        private static void RegisterNamedTap(Button button, Action action)
        {
            button.RegisterCallback<AttachToPanelEvent>(_ => RegisterNamedTap(button.name, action));
        }

        private static void HookInfoIconFallback(VisualElement parent)
        {
            if (_infoFallbackHooked) return;
            var root = parent?.panel?.visualTree;
            if (root == null) return;
            _infoFallbackHooked = true;
            root.RegisterCallback<PointerDownEvent>(e =>
            {
                var t = e.target as VisualElement;
                if (t?.name == null || !t.name.EndsWith("_info_icon")) return;
                if (!OneSignalDemo.UI.Sections.SectionBuilder.InfoTapByName
                    .TryGetValue(t.name, out var action) || action == null) return;
                action();
                e.StopPropagation();
            }, TrickleDown.TrickleDown);
        }

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
            HookInfoIconFallback(parent);

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
            var textField = Container?.Q<TextField>();
            textField?.Blur();

#if UNITY_IOS && !UNITY_EDITOR
            // Force UIKit to resign first responder so the keyboard view tears
            // down immediately. Without this the keyboard stays floating over
            // the app after the modal closes and blocks taps on controls
            // behind it (e.g. the logout button after login).
            OneSignalDemoEndEditing();
#endif

            // Release any pointer capture the dismissed Overlay still holds
            // before removing it. Without this UIToolkit can keep delivering
            // the in-flight pointer sequence to the captured target after
            // RemoveFromHierarchy, which swallows the next synthetic tap.
            var overlay = Overlay;
            if (overlay != null)
            {
                var panel = overlay.panel;
                if (panel != null)
                {
                    for (int id = 0; id < PointerId.maxPointers; id++)
                    {
                        if (panel.GetCapturingElement(id) == overlay)
                            overlay.ReleasePointer(id);
                    }
                }
                overlay.RemoveFromHierarchy();
            }
            Overlay = null;
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
            RegisterNamedTap(btn, onClick);
            return btn;
        }

        protected Button CreateCancelButton(string text = "Cancel")
        {
            Action dismiss = Dismiss;
            var btn = new Button(dismiss);
            btn.text = text;
            btn.AddToClassList("dialog-cancel-button");
            btn.AddToClassList("text-dialog-action");
            RegisterNamedTap(btn, dismiss);
            return btn;
        }
    }
}
