using UnityEditor;
using UnityEngine;

namespace OneSignalPush.Editor
{
    class OneSignalSettingsWindow :  OneSignalBaseSettingsWindow<OneSignalSettingsWindow>
    {
        const string k_SdkSetupUrl = "https://documentation.onesignal.com/docs/unity-sdk-setup";
        const string k_HeaderText = "OneSignal is the market leader in customer engagement, powering mobile push, web push, email, and in-app messages.";

        protected override void OnAwake()
        {
            SetHeaderTitle(OneSignalSettings.ProductName);
            SetHeaderDescription(k_HeaderText);
            SetDocumentationUrl(k_SdkSetupUrl);
            AddMenuItem("SETTINGS", CreateInstance<SettingsTab>());
            AddMenuItem("ABOUT", CreateInstance<AboutTab>());
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent(OneSignalSettings.ProductName);
        }

        protected override void BeforeGUI()
        {
            EditorGUI.BeginChangeCheck();
        }

        protected override void AfterGUI()
        {
            if(EditorGUI.EndChangeCheck())
            {
                OneSignalSettings.Save();
            }
        }
    }
}
