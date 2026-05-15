using System;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class LiveActivitiesSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;
        private TextField _activityIdField;
        private TextField _orderNumberField;
        private Button _startButton;
        private Button _updateButton;
        private Button _endButton;
        private Label _apiKeyHint;

        public Action OnInfoTap;

        public LiveActivitiesSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection(
                "Live Activities",
                "live_activities_section",
                () => OnInfoTap?.Invoke()
            );

            var inputCard = SectionBuilder.CreateCard("live_activities_input_card");

            var activityIdRow = CreateInlineInputRow(
                "Activity ID",
                "order-1",
                "live_activity_id_input"
            );
            _activityIdField = activityIdRow.Q<TextField>();
            _activityIdField.RegisterValueChangedCallback(_ => RefreshButtonStates());
            inputCard.Add(activityIdRow);

            inputCard.Add(SectionBuilder.CreateDivider(true));

            var orderNumberRow = CreateInlineInputRow(
                "Order #",
                "ORD-1234",
                "live_activity_order_number_input"
            );
            _orderNumberField = orderNumberRow.Q<TextField>();
            inputCard.Add(orderNumberRow);

            section.Add(inputCard);

            _startButton = SectionBuilder.CreatePrimaryButton(
                "START LIVE ACTIVITY",
                "start_live_activity_button",
                OnStartTap
            );
            section.Add(_startButton);

            _updateButton = SectionBuilder.CreatePrimaryButton(
                $"UPDATE \u2192 {_viewModel.NextStatusLabel}",
                "update_live_activity_button",
                OnUpdateTap
            );
            section.Add(_updateButton);

            _endButton = SectionBuilder.CreateDestructiveButton(
                "END LIVE ACTIVITY",
                "end_live_activity_button",
                OnEndTap
            );
            section.Add(_endButton);

            _apiKeyHint = new Label("Set ONESIGNAL_API_KEY in .env to enable update & end");
            _apiKeyHint.name = "live_activity_api_key_hint";
            _apiKeyHint.AddToClassList("hint-text");
            section.Add(_apiKeyHint);

            RefreshButtonStates();
            return section;
        }

        public void Refresh()
        {
            RefreshButtonStates();
        }

        private void RefreshButtonStates()
        {
            bool hasActivityId = !string.IsNullOrEmpty(_activityIdField?.value);
            bool hasApiKey = _viewModel.HasApiKey;

            _startButton?.SetEnabled(hasActivityId);

            _updateButton?.SetEnabled(
                hasActivityId && hasApiKey && !_viewModel.IsLiveActivityUpdating
            );
            if (_updateButton != null)
                _updateButton.text = $"UPDATE \u2192 {_viewModel.NextStatusLabel}";

            _endButton?.SetEnabled(hasActivityId && hasApiKey);

            if (_apiKeyHint != null)
                _apiKeyHint.style.display = hasApiKey ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void OnStartTap()
        {
            var activityId = _activityIdField?.value;
            var orderNumber = string.IsNullOrEmpty(_orderNumberField?.value) ? "ORD-1234" : _orderNumberField.value;
            _viewModel.StartLiveActivity(activityId, orderNumber);
        }

        private void OnUpdateTap()
        {
            var activityId = _activityIdField?.value;
            _viewModel.UpdateLiveActivity(activityId);
        }

        private void OnEndTap()
        {
            var activityId = _activityIdField?.value;
            _viewModel.EndLiveActivity(activityId);
        }

        private static VisualElement CreateInlineInputRow(
            string label,
            string defaultValue,
            string name
        )
        {
            var row = new VisualElement();
            row.AddToClassList("toggle-row");

            var labelElement = new Label(label);
            labelElement.AddToClassList("toggle-label");
            labelElement.AddToClassList("text-toggle-label");
            row.Add(labelElement);

            var field = new TextField();
            field.name = name;
            field.value = defaultValue;
            field.AddToClassList("inline-input-field");
            row.Add(field);

            return row;
        }
    }
}
