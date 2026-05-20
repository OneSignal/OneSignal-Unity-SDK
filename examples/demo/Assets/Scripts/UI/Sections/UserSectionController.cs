using System;
using OneSignalDemo.UI.Dialogs;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class UserSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _dialogRoot;
        private readonly VisualElement _root;
        private Label _statusValue;
        private Label _externalIdValue;
        private Button _loginButton;
        private Button _logoutButton;

        public UserSectionController(AppViewModel viewModel, VisualElement dialogRoot)
        {
            _viewModel = viewModel;
            _dialogRoot = dialogRoot;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection("User", "user_section");

            var statusCard = SectionBuilder.CreateCard("user_status_card");
            var statusRow = SectionBuilder.CreateInlineKeyValue(
                "Status",
                _viewModel.IsLoggedIn ? "Logged In" : "Anonymous",
                "user_status"
            );
            _statusValue = statusRow.Q<Label>("user_status_value");
            if (_viewModel.IsLoggedIn)
                _statusValue.AddToClassList("status-value-green");
            statusCard.Add(statusRow);

            statusCard.Add(SectionBuilder.CreateDivider());

            var extIdRow = SectionBuilder.CreateInlineKeyValue(
                "External ID",
                _viewModel.IsLoggedIn ? _viewModel.ExternalUserId : "\u2014",
                "user_external_id"
            );
            _externalIdValue = extIdRow.Q<Label>("user_external_id_value");
            statusCard.Add(extIdRow);
            section.Add(statusCard);

            _loginButton = SectionBuilder.CreatePrimaryButton(
                _viewModel.IsLoggedIn ? "SWITCH USER" : "LOGIN USER",
                "login_user_button",
                ShowLoginDialog
            );
            section.Add(_loginButton);

            _logoutButton = SectionBuilder.CreateDestructiveButton(
                "LOGOUT USER",
                "logout_user_button",
                () => _viewModel.LogoutUser()
            );
            _logoutButton.style.display = _viewModel.IsLoggedIn
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            section.Add(_logoutButton);

            return section;
        }

        private void ShowLoginDialog()
        {
            var dialog = new LoginDialog(
                externalId => _viewModel.LoginUser(externalId),
                _viewModel.IsLoggedIn
            );
            dialog.Show(_dialogRoot);
        }

        public void Refresh()
        {
            _statusValue.text = _viewModel.IsLoggedIn ? "Logged In" : "Anonymous";
            _statusValue.EnableInClassList("status-value-green", _viewModel.IsLoggedIn);

            _externalIdValue.text = _viewModel.IsLoggedIn ? _viewModel.ExternalUserId : "\u2014";

            _loginButton.text = _viewModel.IsLoggedIn ? "SWITCH USER" : "LOGIN USER";
            _logoutButton.style.display = _viewModel.IsLoggedIn
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }
}
