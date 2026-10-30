using System;
using OneSignalDemo.Models;
using OneSignalDemo.ViewModels;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI.Sections
{
    public class SendIamSectionController
    {
        private static readonly InAppMessageType[] Types =
        {
            InAppMessageType.TopBanner,
            InAppMessageType.BottomBanner,
            InAppMessageType.CenterModal,
            InAppMessageType.Html,
            InAppMessageType.FullScreen,
            InAppMessageType.FullBleed,
            InAppMessageType.Carousel,
            InAppMessageType.CarouselBleed,
        };

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

            VisualElement row = null;
            for (int i = 0; i < Types.Length; i++)
            {
                if (i % 2 == 0)
                {
                    row = new VisualElement();
                    row.AddToClassList("button-grid-row");
                    section.Add(row);
                }

                var type = Types[i];
                var btn = SectionBuilder.CreatePrimaryButton(
                    type.DisplayName().ToUpperInvariant(),
                    $"send_iam_{type.TriggerValue()}_button",
                    () => _viewModel.SendInAppMessage(type)
                );
                btn.AddToClassList("button-grid-item");
                btn.AddToClassList(
                    i % 2 == 0 ? "button-grid-item--leading" : "button-grid-item--trailing"
                );
                row.Add(btn);
            }

            return section;
        }
    }
}
