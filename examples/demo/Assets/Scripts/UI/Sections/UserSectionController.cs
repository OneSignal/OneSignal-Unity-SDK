using System;
using System.Collections.Generic;
using OneSignalDemo.Services;
using OneSignalDemo.UI.Dialogs;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class UserSectionController
    {
        private static readonly KeyValuePair<string, string>[] LanguageOptions =
        {
            new("Device Default", ""),
            new("English", "en"),
            new("Arabic", "ar"),
            new("Azerbaijani", "az"),
            new("Bosnian", "bs"),
            new("Catalan", "ca"),
            new("Chinese (Simplified)", "zh-Hans"),
            new("Chinese (Traditional)", "zh-Hant"),
            new("Croatian", "hr"),
            new("Czech", "cs"),
            new("Danish", "da"),
            new("Dutch", "nl"),
            new("Estonian", "et"),
            new("Finnish", "fi"),
            new("French", "fr"),
            new("Georgian", "ka"),
            new("Bulgarian", "bg"),
            new("German", "de"),
            new("Greek", "el"),
            new("Hindi", "hi"),
            new("Hebrew", "he"),
            new("Hungarian", "hu"),
            new("Indonesian", "id"),
            new("Italian", "it"),
            new("Japanese", "ja"),
            new("Korean", "ko"),
            new("Latvian", "lv"),
            new("Lithuanian", "lt"),
            new("Malay", "ms"),
            new("Norwegian", "nb"),
            new("Persian", "fa"),
            new("Polish", "pl"),
            new("Portuguese", "pt"),
            new("Punjabi", "pa"),
            new("Romanian", "ro"),
            new("Russian", "ru"),
            new("Serbian", "sr"),
            new("Slovak", "sk"),
            new("Spanish", "es"),
            new("Swedish", "sv"),
            new("Thai", "th"),
            new("Turkish", "tr"),
            new("Ukrainian", "uk"),
            new("Vietnamese", "vi"),
        };

        private readonly AppViewModel _viewModel;
        private readonly VisualElement _dialogRoot;
        private readonly VisualElement _root;
        private Label _statusValue;
        private Label _externalIdValue;
        private Label _languageValue;
        private VisualElement _languageRow;
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

            statusCard.Add(SectionBuilder.CreateDivider());

            _languageRow = SectionBuilder.CreateInlineKeyValue(
                "Language",
                LanguageName(_viewModel.Language),
                "user_language_dropdown"
            );
            _languageValue = _languageRow.Q<Label>("user_language_dropdown_value");
            var chevron = new Label(MaterialIcons.ExpandMore);
            chevron.AddToClassList("material-icon");
            chevron.AddToClassList("language-dropdown-icon");
            _languageRow.Add(chevron);
            _languageRow.AddManipulator(new Clickable(ShowLanguagePicker));
            AccessibilityBridge.RegisterE2ETapTarget(
                _languageRow,
                () => _languageRow.enabledInHierarchy,
                ShowLanguagePicker
            );
            statusCard.Add(_languageRow);
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

        private void ShowLanguagePicker()
        {
            var menu = new DropdownMenuDialog(
                _languageRow,
                LanguageOptions,
                _viewModel.Language,
                "language_option",
                code =>
                {
                    if (code != (_viewModel.Language ?? ""))
                        _viewModel.SetLanguage(code);
                }
            );
            menu.Show(_dialogRoot);
        }

        private static string LanguageName(string code)
        {
            code ??= "";
            foreach (var option in LanguageOptions)
            {
                if (option.Value == code)
                    return option.Key;
            }
            return code;
        }

        public void Refresh()
        {
            _statusValue.text = _viewModel.IsLoggedIn ? "Logged In" : "Anonymous";
            _statusValue.EnableInClassList("status-value-green", _viewModel.IsLoggedIn);

            _externalIdValue.text = _viewModel.IsLoggedIn ? _viewModel.ExternalUserId : "\u2014";
            _languageValue.text = LanguageName(_viewModel.Language);

            _loginButton.text = _viewModel.IsLoggedIn ? "SWITCH USER" : "LOGIN USER";
            _logoutButton.style.display = _viewModel.IsLoggedIn
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }
}
