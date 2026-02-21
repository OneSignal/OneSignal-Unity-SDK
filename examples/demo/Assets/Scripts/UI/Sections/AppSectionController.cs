using System;
using OneSignalDemo.UI;
using OneSignalDemo.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class AppSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;
        private Label _appIdLabel;
        private Label _statusValue;
        private Label _externalIdValue;
        private SwitchToggle _consentToggle;
        private SwitchToggle _privacyToggle;
        private VisualElement _privacyRow;
        private VisualElement _privacyDivider;
        private Button _loginButton;
        private Button _logoutButton;

        public Action OnLoginTap;
        public Action OnLogoutTap;

        public AppSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection("App", "app_section");

            var appIdCard = SectionBuilder.CreateCard("app_id_card");
            var appIdRow = SectionBuilder.CreateInlineKeyValue("App ID", _viewModel.AppId, "app_id");
            _appIdLabel = appIdRow.Q<Label>("app_id_value");
            appIdCard.Add(appIdRow);
            section.Add(appIdCard);

            var banner = new VisualElement();
            banner.AddToClassList("guidance-banner");
            var bannerText = new Label("Add your own App ID, then rebuild to fully test all functionality.");
            bannerText.AddToClassList("guidance-text");
            banner.Add(bannerText);
            var bannerLink = new Label("Get your keys at onesignal.com");
            bannerLink.AddToClassList("guidance-link");
            bannerLink.RegisterCallback<ClickEvent>(_ =>
                Application.OpenURL("https://onesignal.com"));
            banner.Add(bannerLink);
            section.Add(banner);

            var consentCard = SectionBuilder.CreateCard("consent_card");
            var consentRow = SectionBuilder.CreateToggleRow(
                "Consent Required",
                "Require consent before SDK processes data",
                "consent_required_toggle",
                _viewModel.ConsentRequired,
                OnConsentRequiredChanged);
            _consentToggle = consentRow.Q<SwitchToggle>();
            consentCard.Add(consentRow);

            _privacyDivider = SectionBuilder.CreateDivider();
            _privacyDivider.style.display = _viewModel.ConsentRequired ? DisplayStyle.Flex : DisplayStyle.None;
            consentCard.Add(_privacyDivider);

            _privacyRow = SectionBuilder.CreateToggleRow(
                "Privacy Consent",
                "Consent given for data collection",
                "privacy_consent_toggle",
                _viewModel.PrivacyConsentGiven,
                OnPrivacyConsentChanged);
            _privacyToggle = _privacyRow.Q<SwitchToggle>();
            _privacyRow.style.display = _viewModel.ConsentRequired ? DisplayStyle.Flex : DisplayStyle.None;
            consentCard.Add(_privacyRow);
            section.Add(consentCard);

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
            _appIdLabel.text = _viewModel.AppId;
            _consentToggle.SetValueWithoutNotify(_viewModel.ConsentRequired);
            _privacyToggle.SetValueWithoutNotify(_viewModel.PrivacyConsentGiven);
            _privacyDivider.style.display = _viewModel.ConsentRequired ? DisplayStyle.Flex : DisplayStyle.None;
            _privacyRow.style.display = _viewModel.ConsentRequired ? DisplayStyle.Flex : DisplayStyle.None;

            _statusValue.text = _viewModel.IsLoggedIn ? "Logged In" : "Anonymous";
            _statusValue.EnableInClassList("status-value-green", _viewModel.IsLoggedIn);

            _externalIdValue.text = _viewModel.IsLoggedIn ? _viewModel.ExternalUserId : "\u2013";

            _loginButton.text = _viewModel.IsLoggedIn ? "SWITCH USER" : "LOGIN USER";
            _logoutButton.style.display = _viewModel.IsLoggedIn ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnConsentRequiredChanged(bool value) => _viewModel.SetConsentRequired(value);
        private void OnPrivacyConsentChanged(bool value) => _viewModel.SetPrivacyConsent(value);
    }
}
