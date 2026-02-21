using UnityEngine;

namespace OneSignalDemo.Services
{
    public class PreferencesService
    {
        private const string KeyAppId = "onesignal_app_id";
        private const string KeyConsentRequired = "consent_required";
        private const string KeyPrivacyConsent = "privacy_consent";
        private const string KeyExternalUserId = "external_user_id";
        private const string KeyLocationShared = "location_shared";
        private const string KeyIamPaused = "iam_paused";

        public string AppId
        {
            get => GetString(KeyAppId, "");
            set => SetString(KeyAppId, value);
        }

        public bool ConsentRequired
        {
            get => GetBool(KeyConsentRequired, false);
            set => SetBool(KeyConsentRequired, value);
        }

        public bool PrivacyConsent
        {
            get => GetBool(KeyPrivacyConsent, false);
            set => SetBool(KeyPrivacyConsent, value);
        }

        public string ExternalUserId
        {
            get => GetString(KeyExternalUserId, "");
            set => SetString(KeyExternalUserId, value);
        }

        public bool LocationShared
        {
            get => GetBool(KeyLocationShared, false);
            set => SetBool(KeyLocationShared, value);
        }

        public bool IamPaused
        {
            get => GetBool(KeyIamPaused, false);
            set => SetBool(KeyIamPaused, value);
        }

        private bool GetBool(string key, bool defaultValue) =>
            PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;

        private void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        private string GetString(string key, string defaultValue) =>
            PlayerPrefs.GetString(key, defaultValue);

        private void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }
    }
}
