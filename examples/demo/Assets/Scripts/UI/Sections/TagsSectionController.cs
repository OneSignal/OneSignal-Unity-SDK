using System;
using System.Linq;
using OneSignalDemo.UI.Dialogs;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class TagsSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _dialogRoot;
        private readonly VisualElement _root;
        private VisualElement _listContainer;
        private Button _removeSelectedButton;

        public Action OnInfoTap;

        public TagsSectionController(AppViewModel viewModel, VisualElement dialogRoot)
        {
            _viewModel = viewModel;
            _dialogRoot = dialogRoot;
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
                    ShowAddTagDialog
                )
            );
            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "ADD MULTIPLE TAGS",
                    "add_multiple_tags_button",
                    ShowAddMultipleTagsDialog
                )
            );

            _removeSelectedButton = SectionBuilder.CreateDestructiveButton(
                "REMOVE TAGS",
                "remove_tags_button",
                ShowRemoveSelectedTagsDialog
            );
            section.Add(_removeSelectedButton);

            RefreshList();
            return section;
        }

        private void ShowAddTagDialog()
        {
            var dialog = new PairInputDialog(
                "Add Tag",
                "Key",
                "Value",
                "tag_key_input",
                "tag_value_input",
                "Add",
                (key, value) => _viewModel.AddTag(key, value)
            );
            dialog.Show(_dialogRoot);
        }

        private void ShowAddMultipleTagsDialog()
        {
            var dialog = new MultiPairInputDialog(
                "Add Multiple Tags",
                "Key",
                "Value",
                "Add all",
                pairs => _viewModel.AddTags(pairs)
            );
            dialog.Show(_dialogRoot);
        }

        private void ShowRemoveSelectedTagsDialog()
        {
            var items = _viewModel.Tags.ToList();
            if (items.Count == 0)
                return;

            var dialog = new MultiSelectRemoveDialog(
                "Remove Tags",
                items,
                keys => _viewModel.RemoveSelectedTags(keys)
            );
            dialog.Show(_dialogRoot);
        }

        public void Refresh() => RefreshList();

        private void RefreshList()
        {
            SectionBuilder.RenderPairList(
                _listContainer,
                _viewModel.Tags,
                "No tags added",
                "tags",
                loading: _viewModel.IsLoading,
                onRemove: key => _viewModel.RemoveTag(key)
            );

            _removeSelectedButton.style.display =
                _viewModel.Tags.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
