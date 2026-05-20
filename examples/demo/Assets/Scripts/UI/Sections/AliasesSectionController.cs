using System;
using OneSignalDemo.UI.Dialogs;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class AliasesSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _dialogRoot;
        private readonly VisualElement _root;
        private VisualElement _listContainer;

        public Action OnInfoTap;

        public AliasesSectionController(AppViewModel viewModel, VisualElement dialogRoot)
        {
            _viewModel = viewModel;
            _dialogRoot = dialogRoot;
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
                    ShowAddAliasDialog
                )
            );
            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "ADD MULTIPLE ALIASES",
                    "add_multiple_aliases_button",
                    ShowAddMultipleAliasesDialog
                )
            );

            RefreshList();
            return section;
        }

        private void ShowAddAliasDialog()
        {
            var dialog = new PairInputDialog(
                "Add Alias",
                "Label",
                "ID",
                "alias_label_input",
                "alias_id_input",
                "Add",
                (key, value) => _viewModel.AddAlias(key, value)
            );
            dialog.Show(_dialogRoot);
        }

        private void ShowAddMultipleAliasesDialog()
        {
            var dialog = new MultiPairInputDialog(
                "Add Multiple Aliases",
                "Label",
                "ID",
                "Add all",
                pairs => _viewModel.AddAliases(pairs)
            );
            dialog.Show(_dialogRoot);
        }

        public void Refresh() => RefreshList();

        private void RefreshList()
        {
            SectionBuilder.RenderPairList(
                _listContainer,
                _viewModel.Aliases,
                "No aliases added",
                "aliases",
                loading: _viewModel.IsLoading
            );
        }
    }
}
