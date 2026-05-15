using System;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class TagsSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;
        private VisualElement _listContainer;
        private Button _removeSelectedButton;

        public Action OnInfoTap;
        public Action OnAddTap;
        public Action OnAddMultipleTap;
        public Action OnRemoveSelectedTap;

        public TagsSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection(
                "Tags",
                "tags_section",
                () => OnInfoTap?.Invoke()
            );

            var card = SectionBuilder.CreateCard("tags_card");
            _listContainer = new VisualElement();
            _listContainer.name = "tags_list";
            card.Add(_listContainer);
            section.Add(card);

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "ADD TAG",
                    "add_tag_button",
                    () => OnAddTap?.Invoke()
                )
            );
            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "ADD MULTIPLE TAGS",
                    "add_multiple_tags_button",
                    () => OnAddMultipleTap?.Invoke()
                )
            );

            _removeSelectedButton = SectionBuilder.CreateDestructiveButton(
                "REMOVE TAGS",
                "remove_tags_button",
                () => OnRemoveSelectedTap?.Invoke()
            );
            section.Add(_removeSelectedButton);

            RefreshList();
            return section;
        }

        public void Refresh() => RefreshList();

        private void RefreshList()
        {
            SectionBuilder.RenderPairList(
                _listContainer,
                _viewModel.Tags,
                "No Tags Added",
                "tags",
                loading: _viewModel.IsLoading,
                onRemove: key => _viewModel.RemoveTag(key)
            );

            _removeSelectedButton.style.display =
                _viewModel.Tags.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
