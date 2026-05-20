using System;
using OneSignalDemo.UI;
using OneSignalDemo.UI.Dialogs;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class CustomEventsSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _dialogRoot;
        private readonly VisualElement _root;

        public Action OnInfoTap;

        public CustomEventsSectionController(AppViewModel viewModel, VisualElement dialogRoot)
        {
            _viewModel = viewModel;
            _dialogRoot = dialogRoot;
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
                    ShowTrackEventDialog
                )
            );

            return section;
        }

        private void ShowTrackEventDialog()
        {
            var dialog = new TrackEventDialog(
                (name, props) =>
                {
                    _viewModel.TrackEvent(name, props);
                    DemoToast.Show($"Event tracked: {name}");
                }
            );
            dialog.Show(_dialogRoot);
        }
    }
}
