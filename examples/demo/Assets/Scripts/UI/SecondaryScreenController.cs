using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI
{
    public class SecondaryScreenController : MonoBehaviour
    {
        [SerializeField]
        private UIDocument _uiDocument;

        private VisualElement _root;

        private void OnEnable()
        {
            _root = _uiDocument.rootVisualElement;
            _root.Clear();

            var themeSheet = Resources.Load<StyleSheet>("Theme");
            if (themeSheet != null)
                _root.styleSheets.Add(themeSheet);

            var screenRoot = new VisualElement();
            screenRoot.AddToClassList("screen-root");

            var statusSpacer = new VisualElement();
            statusSpacer.name = "status_bar_spacer";
            statusSpacer.AddToClassList("status-bar-spacer");
            statusSpacer.style.height = 0;
            screenRoot.Add(statusSpacer);

            var appBar = new VisualElement();
            appBar.AddToClassList("app-bar");
            appBar.AddToClassList("app-bar-left");

            void GoBack() => SceneManager.LoadScene("Main");
            var backButton = new Button(GoBack);
            backButton.name = "back_button";
            backButton.text = MaterialIcons.ArrowBack;
            backButton.AddToClassList("back-button");
#if UNITY_ANDROID && !UNITY_EDITOR
            OneSignalDemo.Services.AccessibilityBridge.RegisterE2ETapTarget(
                backButton,
                () => backButton.enabledInHierarchy,
                GoBack
            );
#endif
            appBar.Add(backButton);

            var title = new Label("Secondary Screen");
            title.AddToClassList("app-bar-title");
            appBar.Add(title);

            screenRoot.Add(appBar);

            var appBarShadow = new VisualElement();
            appBarShadow.AddToClassList("app-bar-shadow");
            screenRoot.Add(appBarShadow);

            var content = new VisualElement();
            content.AddToClassList("centered-content");

            var heading = new Label("Secondary Screen");
            heading.name = "secondary_activity_label";
            heading.AddToClassList("page-heading");
            content.Add(heading);

            screenRoot.Add(content);
            _root.Add(screenRoot);

#if UNITY_IOS || UNITY_ANDROID
            OneSignalDemo.Services.AccessibilityBridge.EnableForE2E(_root);
#endif
        }

        private void Update()
        {
            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            if (_root == null)
                return;

            float rootHeight = _root.resolvedStyle.height;
            if (float.IsNaN(rootHeight) || rootHeight <= 0 || Screen.height <= 0)
                return;

            var safe = Screen.safeArea;
            float scale = rootHeight / Screen.height;
            float top = (Screen.height - safe.yMax) * scale;

            var statusSpacer = _root.Q("status_bar_spacer");
            if (statusSpacer != null)
                statusSpacer.style.height = top;
        }
    }
}
