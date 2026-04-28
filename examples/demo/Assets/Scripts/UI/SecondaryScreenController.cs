using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI
{
    public class SecondaryScreenController : MonoBehaviour
    {
        [SerializeField]
        private UIDocument _uiDocument;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;
            root.Clear();

            var themeSheet = Resources.Load<StyleSheet>("Theme");
            if (themeSheet != null)
                root.styleSheets.Add(themeSheet);

            var screenRoot = new VisualElement();
            screenRoot.AddToClassList("screen-root");

            var appBar = new VisualElement();
            appBar.AddToClassList("app-bar");

            var backButton = new Button(() => SceneManager.LoadScene("Main"));
            backButton.name = "back_button";
            backButton.text = MaterialIcons.ArrowBack;
            backButton.AddToClassList("back-button");
            appBar.Add(backButton);

            var title = new Label("Secondary Screen");
            title.AddToClassList("app-bar-title");
            appBar.Add(title);

            screenRoot.Add(appBar);

            var content = new VisualElement();
            content.AddToClassList("centered-content");

            var heading = new Label("Secondary Screen");
            heading.name = "secondary_activity_label";
            heading.AddToClassList("page-heading");
            content.Add(heading);

            screenRoot.Add(content);
            root.Add(screenRoot);

#if UNITY_IOS || UNITY_ANDROID
            OneSignalDemo.Services.AccessibilityBridge.EnableForE2E(root);
#endif
        }
    }
}
