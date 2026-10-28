using System;
using OneSignalDemo.UI;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class PushSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;
        private Label _pushIdLabel;
        private Label _statusLabel;
        private SwitchToggle _enabledToggle;
        private Button _promptButton;

        public Action OnInfoTap;

        public PushSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection(
                "Push",
                "push_section",
                () => OnInfoTap?.Invoke()
            );

            var card = SectionBuilder.CreateCard("push_card");

            var pushIdRow = SectionBuilder.CreateInlineKeyValue(
                "Push ID",
                FormatIdValue(_viewModel.PushSubscriptionId),
                "push_id"
            );
            _pushIdLabel = pushIdRow.Q<Label>("push_id_value");
            card.Add(pushIdRow);

            card.Add(SectionBuilder.CreateDivider());

            var statusRow = SectionBuilder.CreateInlineKeyValue(
                "Status",
                FormatStatus(_viewModel.IsPushSubscribed)
            );
            // The shared E2E ID names the status text itself, not the row.
            _statusLabel = statusRow.Q<Label>(className: "key-value-inline-value");
            _statusLabel.name = "push_subscription_status";
            card.Add(statusRow);

            card.Add(SectionBuilder.CreateDivider());

            var toggleRow = SectionBuilder.CreateToggleRow(
                "Opted In",
                null,
                "push_enabled_toggle",
                _viewModel.PushOptedIn,
                OnEnabledChanged
            );
            _enabledToggle = toggleRow.Q<SwitchToggle>();
            _enabledToggle.SetEnabled(_viewModel.HasPermission);
            card.Add(toggleRow);

            section.Add(card);

            _promptButton = SectionBuilder.CreatePrimaryButton(
                "PROMPT PUSH",
                "prompt_push_button",
                () => _viewModel.PromptPush()
            );
            _promptButton.style.display = _viewModel.HasPermission
                ? DisplayStyle.None
                : DisplayStyle.Flex;
            section.Add(_promptButton);

            return section;
        }

        public void Refresh()
        {
            _pushIdLabel.text = FormatIdValue(_viewModel.PushSubscriptionId);
            _statusLabel.text = FormatStatus(_viewModel.IsPushSubscribed);
            _enabledToggle.SetValueWithoutNotify(_viewModel.PushOptedIn);
            _enabledToggle.SetEnabled(_viewModel.HasPermission);
            _promptButton.style.display = _viewModel.HasPermission
                ? DisplayStyle.None
                : DisplayStyle.Flex;
        }

        private static string FormatIdValue(string value) =>
            string.IsNullOrEmpty(value) ? "—" : value;

        private static string FormatStatus(bool subscribed) =>
            subscribed ? "Subscribed" : "Unsubscribed";

        private void OnEnabledChanged(bool value) => _viewModel.SetPushEnabled(value);
    }
}
