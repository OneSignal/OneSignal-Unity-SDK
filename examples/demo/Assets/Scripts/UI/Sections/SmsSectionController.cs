using System;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class SmsSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;
        private VisualElement _listContainer;
        private bool _expanded;
        private const int CollapseThreshold = 5;

        public Action OnInfoTap;
        public Action OnAddTap;

        public SmsSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection("SMS", "sms_section",
                () => OnInfoTap?.Invoke());

            var card = SectionBuilder.CreateCard("sms_card");
            _listContainer = new VisualElement();
            _listContainer.name = "sms_list";
            card.Add(_listContainer);
            section.Add(card);

            section.Add(SectionBuilder.CreatePrimaryButton("ADD SMS", "add_sms_button",
                () => OnAddTap?.Invoke()));

            RefreshList();
            return section;
        }

        public void Refresh() => RefreshList();

        private void RefreshList()
        {
            _listContainer.Clear();
            var numbers = _viewModel.SmsNumbers;

            if (numbers.Count == 0)
            {
                _listContainer.Add(SectionBuilder.CreateEmptyState("No SMS Added"));
                return;
            }

            int showCount = _expanded || numbers.Count <= CollapseThreshold
                ? numbers.Count : CollapseThreshold;

            for (int i = 0; i < showCount; i++)
            {
                if (i > 0) _listContainer.Add(SectionBuilder.CreateDivider(tight: true));
                var sms = numbers[i];
                _listContainer.Add(SectionBuilder.CreateSingleItem(sms, $"sms_{i}",
                    () => _viewModel.RemoveSms(sms)));
            }

            if (!_expanded && numbers.Count > CollapseThreshold)
            {
                var more = new Label($"{numbers.Count - CollapseThreshold} more");
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
