using System;
using OneSignalDemo.Models;
using OneSignalDemo.UI;
using OneSignalDemo.UI.Dialogs;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class OutcomesSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _dialogRoot;
        private readonly VisualElement _root;

        public Action OnInfoTap;

        public OutcomesSectionController(AppViewModel viewModel, VisualElement dialogRoot)
        {
            _viewModel = viewModel;
            _dialogRoot = dialogRoot;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection(
                "Outcome Events",
                "outcomes_section",
                () => OnInfoTap?.Invoke()
            );

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "SEND OUTCOME",
                    "send_outcome_button",
                    ShowOutcomeDialog
                )
            );

            return section;
        }

        private void ShowOutcomeDialog()
        {
            var dialog = new OutcomeDialog(
                (type, name, value) =>
                {
                    switch (type)
                    {
                        case OutcomeType.Normal:
                            _viewModel.SendOutcome(name);
                            DemoToast.Show($"Outcome sent: {name}");
                            break;
                        case OutcomeType.Unique:
                            _viewModel.SendUniqueOutcome(name);
                            DemoToast.Show($"Unique outcome sent: {name}");
                            break;
                        case OutcomeType.WithValue:
                            _viewModel.SendOutcomeWithValue(name, value);
                            DemoToast.Show($"Outcome sent: {name} = {value}");
                            break;
                    }
                }
            );
            dialog.Show(_dialogRoot);
        }
    }
}
