using System.Collections.Generic;
using System.Linq;
using OneSignalDemo.Models;
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
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private AppViewModel _viewModel;

        private VisualElement _root;
        private VisualElement _contentRoot;
        private VisualElement _loadingOverlay;
        private LogViewController _logView;
        private ToastView _toastView;

        private AppSectionController _appSection;
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
        private TrackEventSectionController _trackEventSection;
        private LocationSectionController _locationSection;

        private void OnEnable()
        {
            if (_viewModel == null)
            {
                _viewModel = FindAnyObjectByType<AppViewModel>();
            }

            _root = _uiDocument.rootVisualElement;
            _root.Clear();

            var themeSheet = Resources.Load<StyleSheet>("Theme");
            if (themeSheet != null) _root.styleSheets.Add(themeSheet);

            var logViewSheet = Resources.Load<StyleSheet>("LogView");
            if (logViewSheet != null) _root.styleSheets.Add(logViewSheet);

            BuildScreen();
            WireEvents();

            _viewModel.PromptPush();
        }

        private void OnDisable()
        {
            if (_viewModel != null)
            {
                _viewModel.OnStateChanged -= RefreshAll;
                _viewModel.OnToastMessage -= ShowToast;
            }
            _logView?.Destroy();
        }

        private void Update()
        {
            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            if (_root == null) return;
            var safe = Screen.safeArea;
            float scale = _root.resolvedStyle.height / Screen.height;
            float top = (Screen.height - safe.yMax) * scale;
            float bottom = safe.y * scale;
            var screenRoot = _root.Q("screen_root");
            if (screenRoot != null)
                screenRoot.style.paddingBottom = bottom;
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

            var appBarTitle = new Label("Sample App");
            appBarTitle.AddToClassList("app-bar-title");
            appBar.Add(appBarTitle);

            screenRoot.Add(appBar);

            _logView = new LogViewController(screenRoot);

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;

            _contentRoot = new VisualElement();
            _contentRoot.AddToClassList("scroll-content");

            BuildSections();

            scrollView.Add(_contentRoot);
            screenRoot.Add(scrollView);

            _loadingOverlay = new VisualElement();
            _loadingOverlay.name = "loading_overlay";
            _loadingOverlay.AddToClassList("loading-overlay");
            _loadingOverlay.style.display = DisplayStyle.None;

            var spinner = new VisualElement();
            spinner.AddToClassList("loading-spinner");
            _loadingOverlay.Add(spinner);

            screenRoot.Add(_loadingOverlay);

            _toastView = new ToastView(screenRoot);

            _root.Add(screenRoot);
        }

        private void BuildSections()
        {
            _appSection = new AppSectionController(_viewModel);
            _appSection.OnLoginTap = ShowLoginDialog;
            _appSection.OnLogoutTap = () => _viewModel.LogoutUser();
            _contentRoot.Add(_appSection.Root);

            _pushSection = new PushSectionController(_viewModel);
            _pushSection.OnInfoTap = () => ShowTooltip("push");
            _contentRoot.Add(_pushSection.Root);

            _sendPushSection = new SendPushSectionController(_viewModel);
            _sendPushSection.OnInfoTap = () => ShowTooltip("send_push_notification");
            _sendPushSection.OnCustomTap = ShowCustomNotificationDialog;
            _contentRoot.Add(_sendPushSection.Root);

            _inAppSection = new InAppSectionController(_viewModel);
            _inAppSection.OnInfoTap = () => ShowTooltip("in_app_messaging");
            _contentRoot.Add(_inAppSection.Root);

            _sendIamSection = new SendIamSectionController(_viewModel);
            _sendIamSection.OnInfoTap = () => ShowTooltip("send_in_app_message");
            _contentRoot.Add(_sendIamSection.Root);

            _aliasesSection = new AliasesSectionController(_viewModel);
            _aliasesSection.OnInfoTap = () => ShowTooltip("aliases");
            _aliasesSection.OnAddTap = ShowAddAliasDialog;
            _aliasesSection.OnAddMultipleTap = ShowAddMultipleAliasesDialog;
            _contentRoot.Add(_aliasesSection.Root);

            _emailsSection = new EmailsSectionController(_viewModel);
            _emailsSection.OnInfoTap = () => ShowTooltip("emails");
            _emailsSection.OnAddTap = ShowAddEmailDialog;
            _contentRoot.Add(_emailsSection.Root);

            _smsSection = new SmsSectionController(_viewModel);
            _smsSection.OnInfoTap = () => ShowTooltip("sms");
            _smsSection.OnAddTap = ShowAddSmsDialog;
            _contentRoot.Add(_smsSection.Root);

            _tagsSection = new TagsSectionController(_viewModel);
            _tagsSection.OnInfoTap = () => ShowTooltip("tags");
            _tagsSection.OnAddTap = ShowAddTagDialog;
            _tagsSection.OnAddMultipleTap = ShowAddMultipleTagsDialog;
            _tagsSection.OnRemoveSelectedTap = ShowRemoveSelectedTagsDialog;
            _contentRoot.Add(_tagsSection.Root);

            _outcomesSection = new OutcomesSectionController(_viewModel);
            _outcomesSection.OnInfoTap = () => ShowTooltip("outcome_events");
            _outcomesSection.OnSendOutcomeTap = ShowOutcomeDialog;
            _contentRoot.Add(_outcomesSection.Root);

            _triggersSection = new TriggersSectionController(_viewModel);
            _triggersSection.OnInfoTap = () => ShowTooltip("triggers");
            _triggersSection.OnAddTap = ShowAddTriggerDialog;
            _triggersSection.OnAddMultipleTap = ShowAddMultipleTriggersDialog;
            _triggersSection.OnRemoveSelectedTap = ShowRemoveSelectedTriggersDialog;
            _contentRoot.Add(_triggersSection.Root);

            _trackEventSection = new TrackEventSectionController(_viewModel);
            _trackEventSection.OnInfoTap = () => ShowTooltip("track_event");
            _trackEventSection.OnTrackEventTap = ShowTrackEventDialog;
            _contentRoot.Add(_trackEventSection.Root);

            _locationSection = new LocationSectionController(_viewModel);
            _locationSection.OnInfoTap = () => ShowTooltip("location");
            _contentRoot.Add(_locationSection.Root);

            var nextButton = SectionBuilder.CreatePrimaryButton(
                "NEXT ACTIVITY", "next_activity_button",
                () => SceneManager.LoadScene("Secondary"));
            _contentRoot.Add(nextButton);
        }

        private void WireEvents()
        {
            _viewModel.OnStateChanged += RefreshAll;
            _viewModel.OnToastMessage += ShowToast;
        }

        private void RefreshAll()
        {
            _appSection?.Refresh();
            _pushSection?.Refresh();
            _inAppSection?.Refresh();
            _aliasesSection?.Refresh();
            _emailsSection?.Refresh();
            _smsSection?.Refresh();
            _tagsSection?.Refresh();
            _triggersSection?.Refresh();
            _locationSection?.Refresh();

            _loadingOverlay.style.display = _viewModel.IsLoading
                ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ShowToast(string message) => _toastView?.Show(message);

        private void ShowLoginDialog()
        {
            var dialog = new LoginDialog(
                externalId => _viewModel.LoginUser(externalId),
                _viewModel.IsLoggedIn);
            dialog.Show(_root);
        }

        private void ShowAddAliasDialog()
        {
            var dialog = new PairInputDialog(
                "Add Alias", "Label", "ID",
                "alias_key", "alias_value", "ADD",
                (key, value) => _viewModel.AddAlias(key, value));
            dialog.Show(_root);
        }

        private void ShowAddMultipleAliasesDialog()
        {
            var dialog = new MultiPairInputDialog(
                "Add Multiple Aliases", "Label", "ID", "ADD ALL",
                pairs => _viewModel.AddAliases(pairs));
            dialog.Show(_root);
        }

        private void ShowAddEmailDialog()
        {
            var dialog = new SingleInputDialog(
                "Add Email", "Email", "email_input", "ADD",
                email => _viewModel.AddEmail(email));
            dialog.Show(_root);
        }

        private void ShowAddSmsDialog()
        {
            var dialog = new SingleInputDialog(
                "Add SMS", "SMS", "sms_input", "ADD",
                sms => _viewModel.AddSms(sms));
            dialog.Show(_root);
        }

        private void ShowAddTagDialog()
        {
            var dialog = new PairInputDialog(
                "Add Tag", "Key", "Value",
                "tag_key", "tag_value", "ADD",
                (key, value) => _viewModel.AddTag(key, value));
            dialog.Show(_root);
        }

        private void ShowAddMultipleTagsDialog()
        {
            var dialog = new MultiPairInputDialog(
                "Add Multiple Tags", "Key", "Value", "ADD ALL",
                pairs => _viewModel.AddTags(pairs));
            dialog.Show(_root);
        }

        private void ShowRemoveSelectedTagsDialog()
        {
            var items = _viewModel.Tags.ToList();
            if (items.Count == 0) return;

            var dialog = new MultiSelectRemoveDialog(
                "Remove Selected Tags", items,
                keys => _viewModel.RemoveSelectedTags(keys));
            dialog.Show(_root);
        }

        private void ShowAddTriggerDialog()
        {
            var dialog = new PairInputDialog(
                "Add Trigger", "Key", "Value",
                "trigger_key", "trigger_value", "ADD",
                (key, value) => _viewModel.AddTrigger(key, value));
            dialog.Show(_root);
        }

        private void ShowAddMultipleTriggersDialog()
        {
            var dialog = new MultiPairInputDialog(
                "Add Multiple Triggers", "Key", "Value", "ADD ALL",
                pairs => _viewModel.AddTriggers(pairs));
            dialog.Show(_root);
        }

        private void ShowRemoveSelectedTriggersDialog()
        {
            var items = _viewModel.Triggers.ToList();
            if (items.Count == 0) return;

            var dialog = new MultiSelectRemoveDialog(
                "Remove Selected Triggers", items,
                keys => _viewModel.RemoveSelectedTriggers(keys));
            dialog.Show(_root);
        }

        private void ShowOutcomeDialog()
        {
            var dialog = new OutcomeDialog((type, name, value) =>
            {
                switch (type)
                {
                    case OutcomeType.Normal:
                        _viewModel.SendOutcome(name);
                        break;
                    case OutcomeType.Unique:
                        _viewModel.SendUniqueOutcome(name);
                        break;
                    case OutcomeType.WithValue:
                        _viewModel.SendOutcomeWithValue(name, value);
                        break;
                }
            });
            dialog.Show(_root);
        }

        private void ShowTrackEventDialog()
        {
            var dialog = new TrackEventDialog(
                (name, props) => _viewModel.TrackEvent(name, props));
            dialog.Show(_root);
        }

        private void ShowCustomNotificationDialog()
        {
            var dialog = new CustomNotificationDialog(
                (title, body) => _viewModel.SendCustomNotification(title, body));
            dialog.Show(_root);
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
