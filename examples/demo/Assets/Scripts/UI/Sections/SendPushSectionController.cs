using System;
using OneSignalDemo.Models;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class SendPushSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _root;

        public Action OnInfoTap;
        public Action OnCustomTap;

        public SendPushSectionController(AppViewModel viewModel)
        {
            _viewModel = viewModel;
            _root = BuildSection();
        }

        public VisualElement Root => _root;

        private VisualElement BuildSection()
        {
            var section = SectionBuilder.CreateSection(
                "Send Push Notification",
                "send_push_section",
                () => OnInfoTap?.Invoke()
            );

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "SIMPLE",
                    "send_push_simple",
                    () => _viewModel.SendNotification(NotificationType.Simple)
                )
            );

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "WITH IMAGE",
                    "send_push_image",
                    () => _viewModel.SendNotification(NotificationType.WithImage)
                )
            );

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "CUSTOM",
                    "send_push_custom",
                    () => OnCustomTap?.Invoke()
                )
            );

            section.Add(
                SectionBuilder.CreateDestructiveButton(
                    "CLEAR ALL",
                    "clear_all_notifications",
                    () => _viewModel.ClearAllNotifications()
                )
            );

            return section;
        }
    }
}
