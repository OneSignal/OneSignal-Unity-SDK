using System;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI
{
    /// <summary>
    /// Material-style pill switch (52x32) with red track when on.
    /// Drop-in replacement for Unity Toggle in toggle rows.
    /// </summary>
    public class SwitchToggle : VisualElement
    {
        private bool _value;
        private bool _enabled = true;
        private readonly VisualElement _track;
        private readonly VisualElement _thumb;

        public event Action<bool> ValueChanged;

        public bool Value
        {
            get => _value;
            set => SetValueWithoutNotify(value);
        }

        public SwitchToggle()
        {
            _track = new VisualElement();
            _track.AddToClassList("switch-track");

            _thumb = new VisualElement();
            _thumb.AddToClassList("switch-thumb");
            _track.Add(_thumb);

            Add(_track);
            _track.RegisterCallback<ClickEvent>(OnClick);
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            _value = newValue;
            _track.EnableInClassList("switch-track--on", _value);
            _thumb.EnableInClassList("switch-thumb--on", _value);
        }

        public void SetValueAndNotify(bool newValue)
        {
            if (_value == newValue) return;
            SetValueWithoutNotify(newValue);
            ValueChanged?.Invoke(_value);
        }

        public new void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            _track.EnableInClassList("switch-track--disabled", !_enabled);
            base.SetEnabled(enabled);
        }

        private void OnClick(ClickEvent evt)
        {
            if (!_enabled) return;
            SetValueAndNotify(!_value);
            evt.StopPropagation();
        }
    }
}
