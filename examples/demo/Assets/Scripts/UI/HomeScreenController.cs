using OneSignalDemo.Services;
using OneSignalDemo.UI.Dialogs;
using OneSignalDemo.UI.Sections;
using OneSignalDemo.ViewModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI
{
    public class HomeScreenController : MonoBehaviour
    {
        [SerializeField]
        private UIDocument _uiDocument;

        [SerializeField]
        private AppViewModel _viewModel;

        private VisualElement _root;
        private VisualElement _contentRoot;

        private AppSectionController _appSection;
        private UserSectionController _userSection;
        private PushSectionController _pushSection;
        private SendPushSectionController _sendPushSection;
        private InAppSectionController _inAppSection;
        private SendIamSectionController _sendIamSection;
        private AliasesSectionController _aliasesSection;
        private EmailsSectionController _emailsSection;
        private SmsSectionController _smsSection;
        private TagsSectionController _tagsSection;
        private OutcomesSectionController _outcomesSection;
        private TriggersSectionController _triggersSection;
        private CustomEventsSectionController _customEventsSection;
        private LocationSectionController _locationSection;
        private LiveActivitiesSectionController _liveActivitiesSection;

        private void OnEnable()
        {
            ConfigureAndroidStatusBar();

            if (_viewModel == null)
            {
                _viewModel = FindAnyObjectByType<AppViewModel>();
            }

            _root = _uiDocument.rootVisualElement;
            _root.Clear();

            var themeSheet = Resources.Load<StyleSheet>("Theme");
            if (themeSheet != null)
                _root.styleSheets.Add(themeSheet);

            BuildScreen();
            WireEvents();

#if UNITY_IOS || UNITY_ANDROID
            // E2E only: publish the VisualElement tree to platform a11y so
            // Appium (XCUITest / UiAutomator2) can locate elements by name.
            // No-op outside E2E_MODE.
            OneSignalDemo.Services.AccessibilityBridge.EnableForE2E(_root);
#endif
        }

        private void OnDisable()
        {
            if (_viewModel != null)
            {
                _viewModel.OnStateChanged -= RefreshAll;
            }
        }

        private void Update()
        {
            ApplySafeArea();
        }

        private static void ConfigureAndroidStatusBar()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Screen.fullScreen = false;
            try
            {
                var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                activity.Call(
                    "runOnUiThread",
                    new AndroidJavaRunnable(() =>
                    {
                        var window = activity.Call<AndroidJavaObject>("getWindow");
                        window.Call("addFlags", unchecked((int)0x80000000));
                        window.Call("clearFlags", 0x04000000);
                        window.Call("setStatusBarColor", unchecked((int)0xFFE54B4D));
                    })
                );
            }
            catch (System.Exception) { }
#endif
        }

        private void ApplySafeArea()
        {
            if (_root == null)
                return;

            float rootHeight = _root.resolvedStyle.height;
            if (float.IsNaN(rootHeight) || rootHeight <= 0 || Screen.height <= 0)
                return;

            var safe = Screen.safeArea;
            float scale = rootHeight / Screen.height;
            float top = (Screen.height - safe.yMax) * scale;

            var statusSpacer = _root.Q("status_bar_spacer");
            if (statusSpacer != null)
                statusSpacer.style.height = top;
        }

        private void BuildScreen()
        {
            var screenRoot = new VisualElement();
            screenRoot.name = "screen_root";
            screenRoot.AddToClassList("screen-root");

            var statusSpacer = new VisualElement();
            statusSpacer.name = "status_bar_spacer";
            statusSpacer.AddToClassList("status-bar-spacer");
            statusSpacer.style.height = 0;
            screenRoot.Add(statusSpacer);

            var appBar = new VisualElement();
            appBar.AddToClassList("app-bar");

            var logo = new VisualElement();
            logo.AddToClassList("app-bar-logo");
            var logoTexture = Resources.Load<Texture2D>("onesignal_logo");
            if (logoTexture != null)
                logo.style.backgroundImage = new StyleBackground(logoTexture);
            appBar.Add(logo);

            var appBarTitle = new Label("Unity");
            appBarTitle.AddToClassList("app-bar-title");
            appBar.Add(appBarTitle);

            screenRoot.Add(appBar);

            var appBarShadow = new VisualElement();
            appBarShadow.AddToClassList("app-bar-shadow");
            screenRoot.Add(appBarShadow);

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.name = "main_scroll_view";
            scrollView.AddToClassList("flex-grow");

            _contentRoot = new VisualElement();
            _contentRoot.AddToClassList("scroll-content");

            BuildSections();

            scrollView.Add(_contentRoot);
            screenRoot.Add(scrollView);

            DemoToast.Initialize(screenRoot);

            _root.Add(screenRoot);
        }

        private void BuildSections()
        {
            _appSection = new AppSectionController(_viewModel);
            _contentRoot.Add(_appSection.Root);

            _userSection = new UserSectionController(_viewModel, _root);
            _contentRoot.Add(_userSection.Root);

            _pushSection = new PushSectionController(_viewModel);
            _pushSection.OnInfoTap = () => ShowTooltip("push");
            _contentRoot.Add(_pushSection.Root);

            _sendPushSection = new SendPushSectionController(_viewModel, _root);
            _sendPushSection.OnInfoTap = () => ShowTooltip("sendPushNotification");
            _contentRoot.Add(_sendPushSection.Root);

            _inAppSection = new InAppSectionController(_viewModel);
            _inAppSection.OnInfoTap = () => ShowTooltip("inAppMessaging");
            _contentRoot.Add(_inAppSection.Root);

            _sendIamSection = new SendIamSectionController(_viewModel);
            _sendIamSection.OnInfoTap = () => ShowTooltip("sendInAppMessage");
            _contentRoot.Add(_sendIamSection.Root);

            _aliasesSection = new AliasesSectionController(_viewModel, _root);
            _aliasesSection.OnInfoTap = () => ShowTooltip("aliases");
            _contentRoot.Add(_aliasesSection.Root);

            _emailsSection = new EmailsSectionController(_viewModel, _root);
            _emailsSection.OnInfoTap = () => ShowTooltip("emails");
            _contentRoot.Add(_emailsSection.Root);

            _smsSection = new SmsSectionController(_viewModel, _root);
            _smsSection.OnInfoTap = () => ShowTooltip("sms");
            _contentRoot.Add(_smsSection.Root);

            _tagsSection = new TagsSectionController(_viewModel, _root);
            _tagsSection.OnInfoTap = () => ShowTooltip("tags");
            _contentRoot.Add(_tagsSection.Root);

            _outcomesSection = new OutcomesSectionController(_viewModel, _root);
            _outcomesSection.OnInfoTap = () => ShowTooltip("outcomes");
            _contentRoot.Add(_outcomesSection.Root);

            _triggersSection = new TriggersSectionController(_viewModel, _root);
            _triggersSection.OnInfoTap = () => ShowTooltip("triggers");
            _contentRoot.Add(_triggersSection.Root);

            _customEventsSection = new CustomEventsSectionController(_viewModel, _root);
            _customEventsSection.OnInfoTap = () => ShowTooltip("customEvents");
            _contentRoot.Add(_customEventsSection.Root);

            _locationSection = new LocationSectionController(_viewModel);
            _locationSection.OnInfoTap = () => ShowTooltip("location");
            _contentRoot.Add(_locationSection.Root);

#if UNITY_IOS
            _liveActivitiesSection = new LiveActivitiesSectionController(_viewModel);
            _liveActivitiesSection.OnInfoTap = () => ShowTooltip("liveActivities");
            _contentRoot.Add(_liveActivitiesSection.Root);
#endif

            var nextButton = SectionBuilder.CreatePrimaryButton(
                "NEXT SCREEN",
                "next_screen_button",
                () => SceneManager.LoadScene("Secondary")
            );
            nextButton.style.marginTop = 24;
            _contentRoot.Add(nextButton);
        }

        private void WireEvents()
        {
            _viewModel.OnStateChanged += RefreshAll;
        }

        private void RefreshAll()
        {
            _appSection?.Refresh();
            _userSection?.Refresh();
            _pushSection?.Refresh();
            _inAppSection?.Refresh();
            _aliasesSection?.Refresh();
            _emailsSection?.Refresh();
            _smsSection?.Refresh();
            _tagsSection?.Refresh();
            _triggersSection?.Refresh();
            _locationSection?.Refresh();
            _liveActivitiesSection?.Refresh();
        }

        private void ShowTooltip(string key)
        {
            var tooltip = TooltipHelper.Instance.GetTooltip(key);
            if (tooltip != null)
            {
                var dialog = new TooltipDialog(tooltip);
                dialog.Show(_root);
            }
        }
    }
}
