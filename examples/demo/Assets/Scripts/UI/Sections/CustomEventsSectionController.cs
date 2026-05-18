using System;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class CustomEventsSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;

        public Action OnInfoTap;
        public Action OnTrackEventTap;

        public CustomEventsSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection(
                "Custom Events",
                "custom_events_section",
                () => OnInfoTap?.Invoke()
            );

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "TRACK EVENT",
                    "track_event_button",
                    () => OnTrackEventTap?.Invoke()
                )
            );

            return section;
        }
    }
}
