using System;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class InAppSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;
        private Toggle _pauseToggle;

        public Action OnInfoTap;

        public InAppSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection("In-App Messaging", "iam_section",
                () => OnInfoTap?.Invoke());

            var card = SectionBuilder.CreateCard("iam_card");
            var toggleRow = SectionBuilder.CreateToggleRow(
                "Pause In-App Messages",
                "Toggle in-app message display",
                "iam_paused_toggle",
                _viewModel.InAppMessagesPaused,
                OnPauseChanged);
            _pauseToggle = toggleRow.Q<Toggle>();
            card.Add(toggleRow);
            section.Add(card);

            return section;
        }

        public void Refresh()
        {
            _pauseToggle.SetValueWithoutNotify(_viewModel.InAppMessagesPaused);
        }

        private void OnPauseChanged(bool value) => _viewModel.SetInAppMessagesPaused(value);
    }
}
