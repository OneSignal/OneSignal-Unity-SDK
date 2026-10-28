using System;
using System.Collections.Generic;
using OneSignalDemo.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    /// <summary>
    /// Menu anchored under the element that opened it, dismissed by tapping anywhere outside it.
    /// </summary>
    public class DropdownMenuDialog : DialogBase
    {
        private const float MenuMaxHeight = 320f;
        private const float ScreenMargin = 16f;
        private const float ItemHeight = 48f;
        private const float ListPadding = 8f;
        private const float MouseDragThreshold = 8f;

        private readonly VisualElement _anchor;
        private readonly IReadOnlyList<KeyValuePair<string, string>> _options;
        private readonly string _selectedValue;
        private readonly string _namePrefix;
        private readonly Action<string> _onSelected;

        /// <param name="options">Display label to value pairs.</param>
        public DropdownMenuDialog(
            VisualElement anchor,
            IReadOnlyList<KeyValuePair<string, string>> options,
            string selectedValue,
            string namePrefix,
            Action<string> onSelected
        )
        {
            _anchor = anchor;
            _options = options;
            _selectedValue = selectedValue ?? "";
            _namePrefix = namePrefix;
            _onSelected = onSelected;
        }

        protected override void BuildContent(VisualElement container)
        {
            Overlay.AddToClassList("dropdown-overlay");
            container.AddToClassList("dropdown-menu");

            var list = new ScrollView(ScrollViewMode.Vertical);
            list.name = $"{_namePrefix}_list";
            list.style.maxHeight = MenuMaxHeight - ListPadding * 2;

            VisualElement selectedItem = null;
            foreach (var option in _options)
            {
                var value = option.Value;
                var item = new Label(option.Key);
                item.name = $"{_namePrefix}_{(value.Length == 0 ? "default" : value)}";
                item.AddToClassList("dropdown-item");
                item.AddToClassList("text-card-label");
                if (value == _selectedValue)
                {
                    item.AddToClassList("dropdown-item--selected");
                    selectedItem = item;
                }

                void Select() => OnSelected(value);
                item.AddManipulator(new Clickable(Select));
                AccessibilityBridge.RegisterE2ETapTarget(item, () => true, Select);
                list.Add(item);
            }
            container.Add(list);
            EnableMouseDragScroll(list);

            if (selectedItem != null)
                CenterOnceLaidOut(list, selectedItem);

            Position(container);
        }

        private void Position(VisualElement container)
        {
            var bounds = Parent.worldBound;
            var anchor = _anchor.worldBound;
            float anchorTop = anchor.yMin - bounds.yMin;
            float anchorBottom = anchor.yMax - bounds.yMin;
            float menuHeight = Mathf.Min(
                MenuMaxHeight,
                _options.Count * ItemHeight + ListPadding * 2
            );

            container.style.right = Mathf.Max(ScreenMargin, bounds.xMax - anchor.xMax);
            if (anchorBottom + menuHeight > bounds.height - ScreenMargin)
                container.style.bottom = bounds.height - anchorTop;
            else
                container.style.top = anchorBottom;
        }

        private static void CenterOnceLaidOut(ScrollView list, VisualElement item)
        {
            void Center(GeometryChangedEvent _)
            {
                list.UnregisterCallback<GeometryChangedEvent>(Center);
                float viewport = list.contentViewport.layout.height;
                float maxOffset = Mathf.Max(0, list.contentContainer.layout.height - viewport);
                float offset = item.layout.y - (viewport - item.layout.height) / 2;
                list.scrollOffset = new Vector2(0, Mathf.Clamp(offset, 0, maxOffset));
            }
            list.RegisterCallback<GeometryChangedEvent>(Center);
        }

        // ScrollView only drag-scrolls touch and pen, and Android emulators report clicks as mouse.
        private static void EnableMouseDragScroll(ScrollView list)
        {
            int pointerId = PointerId.invalidPointerId;
            float startY = 0;
            float startOffset = 0;
            bool dragging = false;

            void Reset()
            {
                pointerId = PointerId.invalidPointerId;
                dragging = false;
            }

            list.RegisterCallback<PointerDownEvent>(
                e =>
                {
                    if (e.pointerType != UnityEngine.UIElements.PointerType.mouse || e.button != 0)
                        return;
                    pointerId = e.pointerId;
                    startY = e.position.y;
                    startOffset = list.scrollOffset.y;
                    dragging = false;
                },
                TrickleDown.TrickleDown
            );
            list.RegisterCallback<PointerMoveEvent>(
                e =>
                {
                    if (e.pointerId != pointerId)
                        return;
                    float delta = e.position.y - startY;
                    if (!dragging)
                    {
                        if (Mathf.Abs(delta) < MouseDragThreshold)
                            return;
                        dragging = true;
                        // Taking capture from the item's Clickable cancels its pending selection.
                        list.CapturePointer(pointerId);
                    }
                    float maxOffset = Mathf.Max(
                        0,
                        list.contentContainer.layout.height - list.contentViewport.layout.height
                    );
                    list.scrollOffset = new Vector2(
                        0,
                        Mathf.Clamp(startOffset - delta, 0, maxOffset)
                    );
                    e.StopPropagation();
                },
                TrickleDown.TrickleDown
            );
            list.RegisterCallback<PointerUpEvent>(
                e =>
                {
                    if (e.pointerId != pointerId)
                        return;
                    if (dragging)
                    {
                        list.ReleasePointer(pointerId);
                        e.StopPropagation();
                    }
                    Reset();
                },
                TrickleDown.TrickleDown
            );
            list.RegisterCallback<PointerCaptureOutEvent>(e =>
            {
                if (e.target == list)
                    Reset();
            });
        }

        private void OnSelected(string value)
        {
            Dismiss();
            _onSelected?.Invoke(value);
        }
    }
}
