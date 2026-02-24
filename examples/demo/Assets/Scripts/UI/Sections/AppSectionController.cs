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
        private SwitchToggle _consentToggle;
        private SwitchToggle _privacyToggle;
        private VisualElement _privacyRow;
        private VisualElement _privacyDivider;

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
            banner.AddToClassList("card");
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

            return section;
        }

        public void Refresh()
        {
            _appIdLabel.text = _viewModel.AppId;
            _consentToggle.SetValueWithoutNotify(_viewModel.ConsentRequired);
            _privacyToggle.SetValueWithoutNotify(_viewModel.PrivacyConsentGiven);
            _privacyDivider.style.display = _viewModel.ConsentRequired ? DisplayStyle.Flex : DisplayStyle.None;
            _privacyRow.style.display = _viewModel.ConsentRequired ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnConsentRequiredChanged(bool value) => _viewModel.SetConsentRequired(value);
        private void OnPrivacyConsentChanged(bool value) => _viewModel.SetPrivacyConsent(value);
    }
}
