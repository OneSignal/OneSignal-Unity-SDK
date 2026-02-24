using OneSignalDemo.Services;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Dialogs
{
    public class TooltipDialog : DialogBase
    {
        private readonly TooltipData _data;

        public TooltipDialog(TooltipData data)
        {
            _data = data;
        }

        protected override void BuildContent(VisualElement container)
        {
            var title = new Label(_data.Title ?? "Info");
            title.AddToClassList("dialog-title");
            title.AddToClassList("text-dialog-title");
            container.Add(title);

            if (!string.IsNullOrEmpty(_data.Description))
            {
                var desc = new Label(_data.Description);
                desc.AddToClassList("tooltip-description");
                desc.AddToClassList("text-body-medium");
                container.Add(desc);
            }

            if (_data.Options != null)
            {
                foreach (var opt in _data.Options)
                {
                    var optContainer = new VisualElement();
                    optContainer.AddToClassList("tooltip-option");

                    if (!string.IsNullOrEmpty(opt.Name))
                    {
                        var name = new Label(opt.Name);
                        name.AddToClassList("tooltip-option-name");
                        name.AddToClassList("text-body-medium");
                        optContainer.Add(name);
                    }

                    if (!string.IsNullOrEmpty(opt.Description))
                    {
                        var desc = new Label(opt.Description);
                        desc.AddToClassList("tooltip-option-desc");
                        desc.AddToClassList("text-body-small");
                        optContainer.Add(desc);
                    }

                    container.Add(optContainer);
                }
            }

            var actions = new VisualElement();
            actions.AddToClassList("dialog-actions");
            actions.Add(CreateCancelButton("OK"));
            container.Add(actions);
        }
    }
}
