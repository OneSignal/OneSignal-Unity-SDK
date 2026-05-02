using System;
using System.Collections.Generic;
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
            _listContainer.Clear();
            var aliases = _viewModel.Aliases;

            if (aliases.Count == 0)
            {
                _listContainer.Add(
                    _viewModel.IsLoading
                        ? SectionBuilder.CreateLoadingState("aliases")
                        : SectionBuilder.CreateEmptyState("No Aliases Added", "aliases")
                );
                return;
            }

            for (int i = 0; i < aliases.Count; i++)
            {
                if (i > 0)
                    _listContainer.Add(SectionBuilder.CreateDivider(tight: true));
                var kvp = aliases[i];
                _listContainer.Add(
                    SectionBuilder.CreateKeyValueItem(kvp.Key, kvp.Value, "aliases", kvp.Key)
                );
            }
        }
    }
}
