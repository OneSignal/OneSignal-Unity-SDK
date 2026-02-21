using System;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class OutcomesSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;

        public Action OnInfoTap;
        public Action OnSendOutcomeTap;

        public OutcomesSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection("Outcome Events", "outcomes_section",
                () => OnInfoTap?.Invoke());

            section.Add(SectionBuilder.CreatePrimaryButton("SEND OUTCOME", "send_outcome_button",
                () => OnSendOutcomeTap?.Invoke()));

            return section;
        }
    }
}
