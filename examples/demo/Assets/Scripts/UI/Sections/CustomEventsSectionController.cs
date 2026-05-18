using System;
using System.Collections.Generic;
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

            // ---- TEMP: SDK-4523 native nested-properties sanity check ----
            // Bypasses the dialog/MiniJSON parser to confirm Dictionary<string,object>
            // and List<object> nested data round-trips correctly to the dashboard.
            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "TRACK NATIVE TEST",
                    "track_native_test_button",
                    TrackNativeTest
                )
            );
            // ---- END TEMP ----

            return section;
        }

        // ---- TEMP: SDK-4523 ----
        private void TrackNativeTest()
        {
            var props = new Dictionary<string, object>
            {
                ["someNum"] = 123,
                ["someFloat"] = 3.14159,
                ["someString"] = "abc",
                ["someBool"] = true,
                ["someObject"] = new Dictionary<string, object>
                {
                    ["abc"] = "123",
                    ["nested"] = new Dictionary<string, object> { ["def"] = "456" },
                    ["ghi"] = null,
                },
                ["someArray"] = new List<object> { 1, 2 },
                ["someMixedArray"] = new List<object>
                {
                    1,
                    "2",
                    new Dictionary<string, object> { ["abc"] = "123" },
                    null,
                },
                ["someNull"] = null,
            };

            _viewModel.TrackEvent("props_native", props);
        }
        // ---- END TEMP ----
    }
}
