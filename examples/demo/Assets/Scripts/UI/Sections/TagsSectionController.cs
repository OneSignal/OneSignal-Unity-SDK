using System;
using System.Linq;
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
            _listContainer.Clear();
            var tags = _viewModel.Tags;

            if (tags.Count == 0)
            {
                _listContainer.Add(
                    _viewModel.IsLoading
                        ? SectionBuilder.CreateLoadingState("tags")
                        : SectionBuilder.CreateEmptyState("No Tags Added", "tags")
                );
                _removeSelectedButton.style.display = DisplayStyle.None;
                return;
            }

            _removeSelectedButton.style.display = DisplayStyle.Flex;

            for (int i = 0; i < tags.Count; i++)
            {
                if (i > 0)
                    _listContainer.Add(SectionBuilder.CreateDivider(tight: true));
                var kvp = tags[i];
                _listContainer.Add(
                    SectionBuilder.CreateKeyValueItem(
                        kvp.Key,
                        kvp.Value,
                        "tags",
                        kvp.Key,
                        () => _viewModel.RemoveTag(kvp.Key)
                    )
                );
            }
        }
    }
}
