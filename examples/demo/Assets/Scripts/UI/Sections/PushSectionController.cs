using System;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class PushSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;
        private Label _pushIdLabel;
        private Toggle _enabledToggle;
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
            var section = SectionBuilder.CreateSection("Push", "push_section",
                () => OnInfoTap?.Invoke());

            var card = SectionBuilder.CreateCard("push_card");

            var pushIdRow = SectionBuilder.CreateInlineKeyValue("Push ID",
                _viewModel.PushSubscriptionId ?? "\u2013", "push_subscription_id");
            _pushIdLabel = pushIdRow.Q<Label>("push_subscription_id_value");
            card.Add(pushIdRow);

            card.Add(SectionBuilder.CreateDivider());

            var toggleRow = SectionBuilder.CreateToggleRow(
                "Enabled", null, "push_enabled_toggle",
                _viewModel.PushOptedIn, OnEnabledChanged);
            _enabledToggle = toggleRow.Q<Toggle>();
            _enabledToggle.SetEnabled(_viewModel.HasPermission);
            card.Add(toggleRow);

            section.Add(card);

            _promptButton = SectionBuilder.CreatePrimaryButton(
                "PROMPT PUSH", "prompt_push_button",
                () => _viewModel.PromptPush());
            _promptButton.style.display = _viewModel.HasPermission ? DisplayStyle.None : DisplayStyle.Flex;
            section.Add(_promptButton);

            return section;
        }

        public void Refresh()
        {
            _pushIdLabel.text = _viewModel.PushSubscriptionId ?? "\u2013";
            _enabledToggle.SetValueWithoutNotify(_viewModel.PushOptedIn);
            _enabledToggle.SetEnabled(_viewModel.HasPermission);
            _promptButton.style.display = _viewModel.HasPermission ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void OnEnabledChanged(bool value) => _viewModel.SetPushEnabled(value);
    }
}
