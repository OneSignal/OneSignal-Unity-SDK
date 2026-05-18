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
                    "send_simple_button",
                    () => _viewModel.SendNotification(NotificationType.Simple)
                )
            );

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "WITH IMAGE",
                    "send_image_button",
                    () => _viewModel.SendNotification(NotificationType.WithImage)
                )
            );

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "WITH SOUND",
                    "send_sound_button",
                    () => _viewModel.SendNotification(NotificationType.WithSound)
                )
            );

            section.Add(
                SectionBuilder.CreatePrimaryButton(
                    "CUSTOM",
                    "send_custom_button",
                    () => OnCustomTap?.Invoke()
                )
            );

            section.Add(
                SectionBuilder.CreateDestructiveButton(
                    "CLEAR ALL",
                    "clear_all_button",
                    () => _viewModel.ClearAllNotifications()
                )
            );

            return section;
        }
    }
}
