using System;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class UserSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;
        private Label _statusValue;
        private Label _externalIdValue;
        private Button _loginButton;
        private Button _logoutButton;

        public Action OnLoginTap;
        public Action OnLogoutTap;

        public UserSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection("User", "user_section");

            var statusCard = SectionBuilder.CreateCard("user_status_card");
            var statusRow = SectionBuilder.CreateInlineKeyValue("Status",
                _viewModel.IsLoggedIn ? "Logged In" : "Anonymous", "user_status");
            _statusValue = statusRow.Q<Label>("user_status_value");
            if (_viewModel.IsLoggedIn)
                _statusValue.AddToClassList("status-value-green");
            statusCard.Add(statusRow);

            statusCard.Add(SectionBuilder.CreateDivider());

            var extIdRow = SectionBuilder.CreateInlineKeyValue("External ID",
                _viewModel.IsLoggedIn ? _viewModel.ExternalUserId : "\u2013", "external_id");
            _externalIdValue = extIdRow.Q<Label>("external_id_value");
            statusCard.Add(extIdRow);
            section.Add(statusCard);

            _loginButton = SectionBuilder.CreatePrimaryButton(
                _viewModel.IsLoggedIn ? "SWITCH USER" : "LOGIN USER",
                "login_button",
                () => OnLoginTap?.Invoke());
            section.Add(_loginButton);

            _logoutButton = SectionBuilder.CreateDestructiveButton("LOGOUT USER", "logout_button",
                () => OnLogoutTap?.Invoke());
            _logoutButton.style.display = _viewModel.IsLoggedIn ? DisplayStyle.Flex : DisplayStyle.None;
            section.Add(_logoutButton);

            return section;
        }

        public void Refresh()
        {
            _statusValue.text = _viewModel.IsLoggedIn ? "Logged In" : "Anonymous";
            _statusValue.EnableInClassList("status-value-green", _viewModel.IsLoggedIn);

            _externalIdValue.text = _viewModel.IsLoggedIn ? _viewModel.ExternalUserId : "\u2013";

            _loginButton.text = _viewModel.IsLoggedIn ? "SWITCH USER" : "LOGIN USER";
            _logoutButton.style.display = _viewModel.IsLoggedIn ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
