using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace OneSignalDemo.UI
{
    public class SecondaryScreenController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;
            root.Clear();

            var themeSheet = Resources.Load<StyleSheet>("Theme");
            if (themeSheet != null) root.styleSheets.Add(themeSheet);

            var screenRoot = new VisualElement();
            screenRoot.AddToClassList("screen-root");

            var appBar = new VisualElement();
            appBar.AddToClassList("app-bar");

            var backButton = new Button(() => SceneManager.LoadScene("Main"));
            backButton.name = "back_button";
            backButton.text = "\uE5C4";
            backButton.AddToClassList("material-icon");
            backButton.style.fontSize = 22;
            backButton.style.backgroundColor = new StyleColor(Color.clear);
            backButton.style.borderTopWidth = 0;
            backButton.style.borderBottomWidth = 0;
            backButton.style.borderLeftWidth = 0;
            backButton.style.borderRightWidth = 0;
            backButton.style.marginRight = 8;
            appBar.Add(backButton);

            var title = new Label("Secondary Activity");
            title.AddToClassList("app-bar-title");
            appBar.Add(title);

            screenRoot.Add(appBar);

            var content = new VisualElement();
            content.style.flexGrow = 1;
            content.style.justifyContent = Justify.Center;
            content.style.alignItems = Align.Center;

            var heading = new Label("Secondary Activity");
            heading.name = "secondary_heading";
            heading.style.fontSize = 24;
            heading.style.unityFontStyleAndWeight = FontStyle.Bold;
            content.Add(heading);

            screenRoot.Add(content);
            root.Add(screenRoot);
        }
    }
}
