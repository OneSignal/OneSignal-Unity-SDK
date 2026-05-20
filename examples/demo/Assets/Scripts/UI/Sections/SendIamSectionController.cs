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
            var section = SectionBuilder.CreateSection(
                "Send In-App Message",
                "send_iam_section",
                () => OnInfoTap?.Invoke()
            );

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "TOP BANNER",
                    "send_iam_top_banner_button",
                    () => _viewModel.SendInAppMessage(InAppMessageType.TopBanner)
                )
            );
            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "BOTTOM BANNER",
                    "send_iam_bottom_banner_button",
                    () => _viewModel.SendInAppMessage(InAppMessageType.BottomBanner)
                )
            );
            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "CENTER MODAL",
                    "send_iam_center_modal_button",
                    () => _viewModel.SendInAppMessage(InAppMessageType.CenterModal)
                )
            );
            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "FULL SCREEN",
                    "send_iam_full_screen_button",
                    () => _viewModel.SendInAppMessage(InAppMessageType.FullScreen)
                )
            );

            return section;
        }
    }
}
