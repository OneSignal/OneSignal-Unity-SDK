using System;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class AliasesSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;
        private VisualElement _listContainer;

        public Action OnInfoTap;
        public Action OnAddTap;
        public Action OnAddMultipleTap;

        public AliasesSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection(
                "Aliases",
                "aliases_section",
                () => OnInfoTap?.Invoke()
            );

            var card = SectionBuilder.CreateCard("aliases_card");
            _listContainer = new VisualElement();
            _listContainer.name = "aliases_list";
            card.Add(_listContainer);
            section.Add(card);

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "ADD ALIAS",
                    "add_alias_button",
                    () => OnAddTap?.Invoke()
                )
            );
            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "ADD MULTIPLE ALIASES",
                    "add_multiple_aliases_button",
                    () => OnAddMultipleTap?.Invoke()
                )
            );

            RefreshList();
            return section;
        }

        public void Refresh() => RefreshList();

        private void RefreshList()
        {
            SectionBuilder.RenderPairList(
                _listContainer,
                _viewModel.Aliases,
                "No Aliases Added",
                "aliases",
                loading: _viewModel.IsLoading
            );
        }
    }
}
