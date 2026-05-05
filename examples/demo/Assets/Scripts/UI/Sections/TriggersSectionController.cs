using System;
using System.Linq;
using OneSignalDemo.Services;
using OneSignalDemo.ViewModels;
using UnityEngine;
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
        private double _lastAddTapMs = -1.0;
        private double _lastAddMultipleTapMs = -1.0;
        private double _lastRemoveSelectedTapMs = -1.0;
        private double _lastClearAllTapMs = -1.0;
        private const float E2ETapMoveTolerance = 12f;
        private const float EmptyCardTapMoveTolerance = -1f;
        private const int E2ETapDelayMs = 120;
        private const double E2ETapDedupeMs = 500.0;

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
                RegisterDelayedTapFallback(
                    card,
                    () => _viewModel.Triggers.Count == 0,
                    InvokeAddMultiple
                );
                AccessibilityBridge.RegisterE2ETapFallback(
                    card,
                    () => _viewModel.Triggers.Count == 0,
                    InvokeAddMultiple,
                    EmptyCardTapMoveTolerance
                );
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
                RegisterDelayedTapFallback(addButton, () => true, InvokeAdd);
                AccessibilityBridge.RegisterE2ETapFallback(addButton, () => true, InvokeAdd);
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
                RegisterDelayedTapFallback(addMultipleButton, () => true, InvokeAddMultiple);
                AccessibilityBridge.RegisterE2ETapFallback(
                    addMultipleButton,
                    () => true,
                    InvokeAddMultiple
                );
            }
            section.Add(addMultipleButton);

            _removeSelectedButton = SectionBuilder.CreateDestructiveButton(
                "REMOVE TRIGGERS",
                "remove_triggers_button",
                InvokeRemoveSelected
            );
            if (AppViewModel.IsE2EMode)
            {
                RegisterDelayedTapFallback(_removeSelectedButton, () => true, InvokeRemoveSelected);
                AccessibilityBridge.RegisterE2ETapFallback(
                    _removeSelectedButton,
                    () => true,
                    InvokeRemoveSelected
                );
            }
            section.Add(_removeSelectedButton);

            _clearAllButton = SectionBuilder.CreateDestructiveButton(
                "CLEAR ALL TRIGGERS",
                "clear_triggers_button",
                InvokeClearAll
            );
            if (AppViewModel.IsE2EMode)
            {
                RegisterDelayedTapFallback(_clearAllButton, () => true, InvokeClearAll);
                AccessibilityBridge.RegisterE2ETapFallback(
                    _clearAllButton,
                    () => true,
                    InvokeClearAll
                );
            }
            section.Add(_clearAllButton);

            RefreshList();
            return section;
        }

        private void RegisterDelayedTapFallback(
            VisualElement element,
            Func<bool> isEnabled,
            Action action
        )
        {
            bool pending = false;
            int pointerId = 0;
            Vector2 startPosition = default;

            element.RegisterCallback<PointerDownEvent>(e =>
            {
                if (!isEnabled())
                    return;

                pending = true;
                pointerId = e.pointerId;
                startPosition = new Vector2(e.position.x, e.position.y);
                element.schedule.Execute(() =>
                {
                    if (!pending || !isEnabled())
                        return;
                    pending = false;
                    action();
                }).StartingIn(E2ETapDelayMs);
            });

            element.RegisterCallback<PointerMoveEvent>(e =>
            {
                if (!pending || pointerId != e.pointerId)
                    return;

                var position = new Vector2(e.position.x, e.position.y);
                if (Vector2.Distance(position, startPosition) > E2ETapMoveTolerance)
                    pending = false;
            });

            element.RegisterCallback<PointerCancelEvent>(_ => pending = false);
        }

        private void InvokeAddMultiple()
        {
            double now = Time.realtimeSinceStartupAsDouble * 1000.0;
            if (_lastAddMultipleTapMs >= 0.0 && now - _lastAddMultipleTapMs < E2ETapDedupeMs)
                return;

            _lastAddMultipleTapMs = now;
            OnAddMultipleTap?.Invoke();
        }

        private void InvokeAdd()
        {
            double now = Time.realtimeSinceStartupAsDouble * 1000.0;
            if (_lastAddTapMs >= 0.0 && now - _lastAddTapMs < E2ETapDedupeMs)
                return;

            _lastAddTapMs = now;
            OnAddTap?.Invoke();
        }

        private void InvokeRemoveSelected()
        {
            double now = Time.realtimeSinceStartupAsDouble * 1000.0;
            if (_lastRemoveSelectedTapMs >= 0.0 && now - _lastRemoveSelectedTapMs < E2ETapDedupeMs)
                return;

            _lastRemoveSelectedTapMs = now;
            OnRemoveSelectedTap?.Invoke();
        }

        private void InvokeClearAll()
        {
            double now = Time.realtimeSinceStartupAsDouble * 1000.0;
            if (_lastClearAllTapMs >= 0.0 && now - _lastClearAllTapMs < E2ETapDedupeMs)
                return;

            _lastClearAllTapMs = now;
            _viewModel.ClearAllTriggers();
        }

        public void Refresh() => RefreshList();

        private void RefreshList()
        {
            _listContainer.Clear();
            var triggers = _viewModel.Triggers;

            bool hasTriggers = triggers.Count > 0;
            _removeSelectedButton.style.display = hasTriggers
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            _clearAllButton.style.display = hasTriggers ? DisplayStyle.Flex : DisplayStyle.None;

            if (!hasTriggers)
            {
                _listContainer.Add(SectionBuilder.CreateEmptyState("No triggers added", "triggers"));
                return;
            }

            for (int i = 0; i < triggers.Count; i++)
            {
                if (i > 0)
                    _listContainer.Add(SectionBuilder.CreateDivider(tight: true));
                var kvp = triggers[i];
                _listContainer.Add(
                    SectionBuilder.CreateKeyValueItem(
                        kvp.Key,
                        kvp.Value,
                        "triggers",
                        kvp.Key,
                        () => _viewModel.RemoveTrigger(kvp.Key)
                    )
                );
            }
        }
    }
}
