using System;
using System.Linq;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class TriggersSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;
        private VisualElement _listContainer;
        private Button _removeSelectedButton;
        private Button _clearAllButton;

        public Action OnInfoTap;
        public Action OnAddTap;
        public Action OnAddMultipleTap;
        public Action OnRemoveSelectedTap;

        public TriggersSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection("Triggers", "triggers_section",
                () => OnInfoTap?.Invoke());

            var card = SectionBuilder.CreateCard("triggers_card");
            _listContainer = new VisualElement();
            _listContainer.name = "triggers_list";
            card.Add(_listContainer);
            section.Add(card);

            section.Add(SectionBuilder.CreatePrimaryButton("ADD", "add_trigger_button",
                () => OnAddTap?.Invoke()));
            section.Add(SectionBuilder.CreatePrimaryButton("ADD MULTIPLE", "add_multiple_triggers_button",
                () => OnAddMultipleTap?.Invoke()));

            _removeSelectedButton = SectionBuilder.CreateDestructiveButton(
                "REMOVE SELECTED", "remove_selected_triggers_button",
                () => OnRemoveSelectedTap?.Invoke());
            section.Add(_removeSelectedButton);

            _clearAllButton = SectionBuilder.CreateDestructiveButton(
                "CLEAR ALL", "clear_all_triggers_button",
                () => _viewModel.ClearAllTriggers());
            section.Add(_clearAllButton);

            RefreshList();
            return section;
        }

        public void Refresh() => RefreshList();

        private void RefreshList()
        {
            _listContainer.Clear();
            var triggers = _viewModel.Triggers;

            bool hasTriggers = triggers.Count > 0;
            _removeSelectedButton.style.display = hasTriggers ? DisplayStyle.Flex : DisplayStyle.None;
            _clearAllButton.style.display = hasTriggers ? DisplayStyle.Flex : DisplayStyle.None;

            if (!hasTriggers)
            {
                _listContainer.Add(SectionBuilder.CreateEmptyState("No Triggers Added"));
                return;
            }

            for (int i = 0; i < triggers.Count; i++)
            {
                if (i > 0) _listContainer.Add(SectionBuilder.CreateDivider());
                var kvp = triggers[i];
                _listContainer.Add(SectionBuilder.CreateKeyValueItem(
                    kvp.Key, kvp.Value, $"trigger_{i}",
                    () => _viewModel.RemoveTrigger(kvp.Key)));
            }
        }
    }
}
