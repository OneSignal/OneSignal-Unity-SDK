using Com.OneSignal.Editor.IMGUI;
using UnityEditor;
using UnityEngine;

namespace Com.OneSignal.Editor
{
    class AboutTab : WindowTabElement
    {
        public override void OnGUI()
        {
            using (new WindowBlockWithIndent(new GUIContent("Who we are")))
            {
                EditorGUILayout.LabelField("OneSignal is the market leader in customer engagement, " +
                    "powering mobile push, web push, email, and in-app messages.", OneSignalImguiStyles.DescriptionLabelStyle);
                EditorGUILayout.Space();
            }

            using (new WindowBlockWithIndent(new GUIContent("Mobile Push")))
            {
                EditorGUILayout.LabelField("Be the first message customers see when they pick up their phones. " +
                    "Notifications are the primary traffic source for most mobile apps.", OneSignalImguiStyles.DescriptionLabelStyle);
                EditorGUILayout.Space();
            }

            using (new WindowBlockWithIndent(new GUIContent("In-App")))
            {
                EditorGUILayout.LabelField("Deliver messages that create delight. " +
                    "Design banners, pop-ups, and interstitials; implement without a single line of code.", OneSignalImguiStyles.DescriptionLabelStyle);
                EditorGUILayout.Space();
            }

            using (new WindowBlockWithIndent(new GUIContent("Email")))
            {
                EditorGUILayout.LabelField("Design emails that look great on every device with the drag-and-drop composer. " +
                    "Customize our free templates to match your brand.with the drag-and-drop composer. Customize our free", OneSignalImguiStyles.DescriptionLabelStyle);
                EditorGUILayout.Space();
            }
        }
    }
}