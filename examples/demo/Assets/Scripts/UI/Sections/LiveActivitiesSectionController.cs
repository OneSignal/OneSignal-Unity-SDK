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
            inputCard.Add(activityIdRow);

            inputCard.Add(SectionBuilder.CreateDivider(true));

            var orderNumberRow = CreateInlineInputRow(
                "Order #",
                "ORD-1234",
                "live_activity_order_input"
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

            bool canUpdate = hasActivityId && hasApiKey && !_viewModel.IsLiveActivityUpdating;
            _updateButton?.SetEnabled(canUpdate);
            if (_updateButton != null)
                _updateButton.text = $"UPDATE \u2192 {_viewModel.NextStatusLabel}";

            _endButton?.SetEnabled(hasActivityId && hasApiKey);
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
