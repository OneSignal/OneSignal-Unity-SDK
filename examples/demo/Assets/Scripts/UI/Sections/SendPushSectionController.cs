using System;
using OneSignalDemo.Models;
using OneSignalDemo.UI.Dialogs;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class SendPushSectionController
    {
        private readonly AppViewModel _viewModel;
        private readonly VisualElement _dialogRoot;
        private readonly VisualElement _root;

        public Action OnInfoTap;

        public SendPushSectionController(AppViewModel viewModel, VisualElement dialogRoot)
        {
            _viewModel = viewModel;
            _dialogRoot = dialogRoot;
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
                    ShowCustomNotificationDialog
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

        private void ShowCustomNotificationDialog()
        {
            var dialog = new CustomNotificationDialog(
                (title, body) => _viewModel.SendCustomNotification(title, body)
            );
            dialog.Show(_dialogRoot);
        }
    }
}
