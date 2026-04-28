using System;
using OneSignalDemo.UI;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class LocationSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;
        private SwitchToggle _locationToggle;

        public Action OnInfoTap;

        public LocationSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection(
                "Location",
                "location_section",
                () => OnInfoTap?.Invoke()
            );

            var card = SectionBuilder.CreateCard("location_card");
            var toggleRow = SectionBuilder.CreateToggleRow(
                "Location Shared",
                "Share device location with OneSignal",
                "location_shared_toggle",
                _viewModel.LocationShared,
                OnLocationChanged
            );
            _locationToggle = toggleRow.Q<SwitchToggle>();
            card.Add(toggleRow);
            section.Add(card);

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "PROMPT LOCATION",
                    "prompt_location_button",
                    () => _viewModel.PromptLocation()
                )
            );

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "CHECK LOCATION SHARED",
                    "check_location_button",
                    () => _viewModel.CheckLocationShared()
                )
            );

            return section;
        }

        public void Refresh()
        {
            _locationToggle.SetValueWithoutNotify(_viewModel.LocationShared);
        }

        private void OnLocationChanged(bool value) => _viewModel.SetLocationShared(value);
    }
}
