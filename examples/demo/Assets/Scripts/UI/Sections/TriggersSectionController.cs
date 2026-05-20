using System;
using System.Linq;
using OneSignalDemo.Services;
using OneSignalDemo.UI.Dialogs;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class TriggersSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _dialogRoot;
        private readonly VisualElement _root;
        private VisualElement _listContainer;
        private Button _removeSelectedButton;
        private Button _clearAllButton;

        public Action OnInfoTap;

        public TriggersSectionController(AppViewModel viewModel, VisualElement dialogRoot)
        {
            _viewModel = viewModel;
            _dialogRoot = dialogRoot;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection(
                "Triggers",
                "triggers_section",
                () => OnInfoTap?.Invoke()
            );

            var card = SectionBuilder.CreateCard("triggers_card");
            if (DotEnv.IsE2EMode)
            {
                card.style.paddingTop = 6;
                card.style.paddingBottom = 6;
                card.style.marginBottom = 4;
            }
            _listContainer = new VisualElement();
            _listContainer.name = "triggers_list";
            card.Add(_listContainer);
            section.Add(card);

            var addButton = SectionBuilder.CreatePrimaryButton(
                "ADD TRIGGER",
                "add_trigger_button",
                ShowAddTriggerDialog
            );
            if (DotEnv.IsE2EMode)
            {
                addButton.style.minHeight = 40;
                addButton.style.marginBottom = 4;
            }
            section.Add(addButton);
            var addMultipleButton = SectionBuilder.CreatePrimaryButton(
                "ADD MULTIPLE TRIGGERS",
                "add_multiple_triggers_button",
                ShowAddMultipleTriggersDialog
            );
            if (DotEnv.IsE2EMode)
            {
                addMultipleButton.style.minHeight = 40;
                addMultipleButton.style.marginBottom = 4;
            }
            section.Add(addMultipleButton);

            _removeSelectedButton = SectionBuilder.CreateDestructiveButton(
                "REMOVE TRIGGERS",
                "remove_triggers_button",
                ShowRemoveSelectedTriggersDialog
            );
            section.Add(_removeSelectedButton);

            _clearAllButton = SectionBuilder.CreateDestructiveButton(
                "CLEAR ALL TRIGGERS",
                "clear_triggers_button",
                () => _viewModel.ClearAllTriggers()
            );
            section.Add(_clearAllButton);

            RefreshList();
            return section;
        }

        private void ShowAddTriggerDialog()
        {
            var dialog = new PairInputDialog(
                "Add Trigger",
                "Key",
                "Value",
                "trigger_key_input",
                "trigger_value_input",
                "Add",
                (key, value) => _viewModel.AddTrigger(key, value)
            );
            dialog.Show(_dialogRoot);
        }

        private void ShowAddMultipleTriggersDialog()
        {
            var dialog = new MultiPairInputDialog(
                "Add Multiple Triggers",
                "Key",
                "Value",
                "Add all",
                pairs => _viewModel.AddTriggers(pairs)
            );
            dialog.Show(_dialogRoot);
        }

        private void ShowRemoveSelectedTriggersDialog()
        {
            var items = _viewModel.Triggers.ToList();
            if (items.Count == 0)
                return;

            var dialog = new MultiSelectRemoveDialog(
                "Remove Triggers",
                items,
                keys => _viewModel.RemoveSelectedTriggers(keys)
            );
            dialog.Show(_dialogRoot);
        }

        public void Refresh() => RefreshList();

        private void RefreshList()
        {
            SectionBuilder.RenderPairList(
                _listContainer,
                _viewModel.Triggers,
                "No triggers added",
                "triggers",
                onRemove: key => _viewModel.RemoveTrigger(key)
            );

            bool hasTriggers = _viewModel.Triggers.Count > 0;
            _removeSelectedButton.style.display =
                hasTriggers ? DisplayStyle.Flex : DisplayStyle.None;
            _clearAllButton.style.display = hasTriggers ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
