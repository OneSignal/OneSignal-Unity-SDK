using System;
using OneSignalDemo.Models;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class SendIamSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;

        public Action OnInfoTap;

        public SendIamSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection("Send In-App Message", "send_iam_section",
                () => OnInfoTap?.Invoke());

            section.Add(CreateIamButton("TOP BANNER", "\u2191", "send_iam_top", InAppMessageType.TopBanner));
            section.Add(CreateIamButton("BOTTOM BANNER", "\u2193", "send_iam_bottom", InAppMessageType.BottomBanner));
            section.Add(CreateIamButton("CENTER MODAL", "\u25A1", "send_iam_center", InAppMessageType.CenterModal));
            section.Add(CreateIamButton("FULL SCREEN", "\u2922", "send_iam_full", InAppMessageType.FullScreen));

            return section;
        }

        private VisualElement CreateIamButton(string text, string icon, string name, InAppMessageType type)
        {
            var btn = new Button(() => _viewModel.SendInAppMessage(type));
            btn.name = name;
            btn.AddToClassList("iam-button");

            var iconLabel = new Label(icon);
            iconLabel.AddToClassList("iam-button-icon");
            btn.Add(iconLabel);

            var textLabel = new Label(text);
            textLabel.AddToClassList("iam-button-label");
            btn.Add(textLabel);

            return btn;
        }
    }
}
