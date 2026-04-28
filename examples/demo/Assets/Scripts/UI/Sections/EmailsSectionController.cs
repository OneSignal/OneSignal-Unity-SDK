using System;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class EmailsSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;
        private VisualElement _listContainer;
        private bool _expanded;
        private const int CollapseThreshold = 5;

        public Action OnInfoTap;
        public Action OnAddTap;

        public EmailsSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection(
                "Emails",
                "emails_section",
                () => OnInfoTap?.Invoke()
            );

            var card = SectionBuilder.CreateCard("emails_card");
            _listContainer = new VisualElement();
            _listContainer.name = "emails_list";
            card.Add(_listContainer);
            section.Add(card);

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "ADD EMAIL",
                    "add_email_button",
                    () => OnAddTap?.Invoke()
                )
            );

            RefreshList();
            return section;
        }

        public void Refresh() => RefreshList();

        private void RefreshList()
        {
            _listContainer.Clear();
            var emails = _viewModel.Emails;

            if (emails.Count == 0)
            {
                _listContainer.Add(
                    _viewModel.IsLoading
                        ? SectionBuilder.CreateLoadingState("emails_loading")
                        : SectionBuilder.CreateEmptyState("No Emails Added")
                );
                return;
            }

            int showCount =
                _expanded || emails.Count <= CollapseThreshold ? emails.Count : CollapseThreshold;

            for (int i = 0; i < showCount; i++)
            {
                if (i > 0)
                    _listContainer.Add(SectionBuilder.CreateDivider(tight: true));
                var email = emails[i];
                _listContainer.Add(
                    SectionBuilder.CreateSingleItem(
                        email,
                        $"email_{i}",
                        () => _viewModel.RemoveEmail(email)
                    )
                );
            }

            if (!_expanded && emails.Count > CollapseThreshold)
            {
                var more = new Label($"{emails.Count - CollapseThreshold} more");
                more.AddToClassList("collapsible-more");
                more.AddToClassList("text-collapsible-more");
                more.RegisterCallback<ClickEvent>(_ =>
                {
                    _expanded = true;
                    RefreshList();
                });
                _listContainer.Add(more);
            }
        }
    }
}
