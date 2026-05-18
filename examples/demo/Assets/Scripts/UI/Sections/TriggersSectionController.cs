using System;
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
            var section = SectionBuilder.CreateSection(
                "Triggers",
                "triggers_section",
                () => OnInfoTap?.Invoke()
            );

            var card = SectionBuilder.CreateCard("triggers_card");
            if (AppViewModel.IsE2EMode)
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
                InvokeAdd
            );
            if (AppViewModel.IsE2EMode)
            {
                addButton.style.minHeight = 40;
                addButton.style.marginBottom = 4;
            }
            section.Add(addButton);
            var addMultipleButton = SectionBuilder.CreatePrimaryButton(
                "ADD MULTIPLE TRIGGERS",
                "add_multiple_triggers_button",
                InvokeAddMultiple
            );
            if (AppViewModel.IsE2EMode)
            {
                addMultipleButton.style.minHeight = 40;
                addMultipleButton.style.marginBottom = 4;
            }
            section.Add(addMultipleButton);

            _removeSelectedButton = SectionBuilder.CreateDestructiveButton(
                "REMOVE TRIGGERS",
                "remove_triggers_button",
                InvokeRemoveSelected
            );
            section.Add(_removeSelectedButton);

            _clearAllButton = SectionBuilder.CreateDestructiveButton(
                "CLEAR ALL TRIGGERS",
                "clear_triggers_button",
                InvokeClearAll
            );
            section.Add(_clearAllButton);

            RefreshList();
            return section;
        }

        private void InvokeAddMultiple() => OnAddMultipleTap?.Invoke();

        private void InvokeAdd() => OnAddTap?.Invoke();

        private void InvokeRemoveSelected() => OnRemoveSelectedTap?.Invoke();

        private void InvokeClearAll() => _viewModel.ClearAllTriggers();

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
