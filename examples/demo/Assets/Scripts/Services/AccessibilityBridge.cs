#if UNITY_IOS || UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.UIElements;

namespace OneSignalDemo.Services
{
    /// <summary>
    /// Mirrors the demo's UI Toolkit VisualElement tree into Unity's
    /// AccessibilityHierarchy so that XCUITest (iOS) and UiAutomator2
    /// (Android) can locate elements by their VisualElement.name. UI
    /// Toolkit renders to a single texture-backed view and is invisible to
    /// platform accessibility services unless we publish a parallel a11y
    /// tree via this bridge. Active only in E2E mode.
    /// </summary>
    public class AccessibilityBridge : MonoBehaviour
    {
        // Throttle frame refreshes so scroll/resize don't spam the OS.
        private const float FrameRefreshIntervalSeconds = 0.25f;

        private static AccessibilityBridge _instance;

        private VisualElement _root;
        private AccessibilityHierarchy _hierarchy;
        private readonly Dictionary<VisualElement, AccessibilityNode> _map = new();
        private bool _resyncScheduled;
        private float _frameRefreshTimer;

        /// <summary>
        /// Idempotent entry point. Safe to call multiple times; only the first
        /// call wires the bridge. No-ops outside E2E_MODE.
        /// </summary>
        public static void EnableForE2E(VisualElement root)
        {
            if (!DotEnv.IsE2EMode || root == null)
                return;

            if (_instance == null)
            {
                var go = new GameObject("[AccessibilityBridge]");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<AccessibilityBridge>();
            }

            _instance.Initialize(root);
        }

        private void Initialize(VisualElement root)
        {
            // Detach any prior root's listener (scene reload, dialog re-parent, etc.).
            if (_root != null && _root != root)
                _root.UnregisterCallback<ChildHierarchyChangedEvent>(OnChildHierarchyChanged);

            _root = root;
            _root.RegisterCallback<ChildHierarchyChangedEvent>(OnChildHierarchyChanged);

            // Without this override, AssistiveSupport.activeHierarchy is auto-cleared
            // whenever the OS reports VoiceOver/TalkBack as off. XCUITest reads the
            // a11y tree without enabling VoiceOver, so we have to lie.
            AssistiveSupport.screenReaderStatusOverride =
                AssistiveSupport.ScreenReaderStatusOverride.ForceEnabled;
            AssistiveSupport.screenReaderStatusChanged -= OnScreenReaderStatusChanged;
            AssistiveSupport.screenReaderStatusChanged += OnScreenReaderStatusChanged;

            BuildHierarchy();
        }

        private void OnDestroy()
        {
            if (_root != null)
                _root.UnregisterCallback<ChildHierarchyChangedEvent>(OnChildHierarchyChanged);
            AssistiveSupport.screenReaderStatusChanged -= OnScreenReaderStatusChanged;
            AssistiveSupport.activeHierarchy = null;
            AssistiveSupport.screenReaderStatusOverride =
                AssistiveSupport.ScreenReaderStatusOverride.OSDriven;
            if (_instance == this)
                _instance = null;
        }

        private void Update()
        {
            // Live frames: scroll position changes don't fire ChildHierarchyChanged
            // but elements move on screen. Refresh frames + notify the OS so cached
            // hit-testing in XCUITest stays accurate.
            _frameRefreshTimer += Time.unscaledDeltaTime;
            if (_frameRefreshTimer < FrameRefreshIntervalSeconds || _hierarchy == null)
                return;
            _frameRefreshTimer = 0f;
            _hierarchy.RefreshNodeFrames();
            AssistiveSupport.notificationDispatcher?.SendLayoutChanged();
        }

        private void OnChildHierarchyChanged(ChildHierarchyChangedEvent evt) => ScheduleResync();

        private void OnScreenReaderStatusChanged(bool _)
        {
            // Unity nulls activeHierarchy when the OS reports screen reader off;
            // reassert immediately since our override should have prevented it.
            if (_hierarchy != null)
                AssistiveSupport.activeHierarchy = _hierarchy;
        }

        private void ScheduleResync()
        {
            if (_resyncScheduled || !isActiveAndEnabled)
                return;
            _resyncScheduled = true;
            StartCoroutine(ResyncEndOfFrame());
        }

        private IEnumerator ResyncEndOfFrame()
        {
            // Wait until current frame finishes laying out so child mutations
            // batch into a single rebuild.
            yield return new WaitForEndOfFrame();
            _resyncScheduled = false;
            BuildHierarchy();
        }

        private void BuildHierarchy()
        {
            if (_root == null)
                return;

            _hierarchy ??= new AccessibilityHierarchy();
            _hierarchy.Clear();
            _map.Clear();

            WalkAndAdd(_root, parent: null);

            AssistiveSupport.activeHierarchy = _hierarchy;
            AssistiveSupport.notificationDispatcher?.SendScreenChanged();
        }

        private void WalkAndAdd(VisualElement el, AccessibilityNode parent)
        {
            if (el == null)
                return;

            // Only mirror named elements — the test framework looks up by name
            // (XCUITest `~name` / UiAutomator2 `id`). Unnamed scaffolding adds
            // noise and depth without changing test reachability.
            var nodeForChildren = parent;
            if (!string.IsNullOrEmpty(el.name))
            {
                var node = _hierarchy.AddNode(el.name, parent);
                var captured = el;
                node.frameGetter = () => GetScreenRect(captured);
                node.role = MapRole(el);
                node.value = ExtractValue(el);
                node.isActive = IsVisible(el);
                _map[el] = node;
                nodeForChildren = node;
            }

            for (int i = 0; i < el.childCount; i++)
                WalkAndAdd(el[i], nodeForChildren);
        }

        private static AccessibilityRole MapRole(VisualElement el) => el switch
        {
            Button => AccessibilityRole.Button,
            Toggle => AccessibilityRole.Toggle,
            ScrollView => AccessibilityRole.ScrollView,
            Slider => AccessibilityRole.Slider,
            TextField => AccessibilityRole.SearchField,
            Label => AccessibilityRole.StaticText,
            Image => AccessibilityRole.Image,
            _ => AccessibilityRole.None,
        };

        private static string ExtractValue(VisualElement el) => el switch
        {
            TextField tf => tf.value ?? string.Empty,
            Toggle t => t.value ? "1" : "0",
            Label l => l.text ?? string.Empty,
            Button b => b.text ?? string.Empty,
            _ => string.Empty,
        };

        private static bool IsVisible(VisualElement el)
        {
            var s = el.resolvedStyle;
            return el.enabledInHierarchy
                && s.display != DisplayStyle.None
                && s.visibility != Visibility.Hidden
                && s.opacity > 0f;
        }

        private static Rect GetScreenRect(VisualElement el)
        {
            if (el?.panel == null)
                return Rect.zero;

            var wb = el.worldBound;
            if (float.IsNaN(wb.x) || float.IsNaN(wb.y) || wb.width <= 0 || wb.height <= 0)
                return Rect.zero;

            // Unity returns screen coords with origin at the bottom-left
            // (OpenGL convention). UIKit/Android UIAccessibility expect
            // top-left. Unity's iOS/Android backends do the platform flip
            // internally, so feed bottom-left coords as-is.
            var topLeft = RuntimePanelUtils.PanelToScreenSpace(
                el.panel, new Vector2(wb.xMin, wb.yMin));
            var bottomRight = RuntimePanelUtils.PanelToScreenSpace(
                el.panel, new Vector2(wb.xMax, wb.yMax));

            float x = Mathf.Min(topLeft.x, bottomRight.x);
            float y = Mathf.Min(topLeft.y, bottomRight.y);
            float w = Mathf.Abs(bottomRight.x - topLeft.x);
            float h = Mathf.Abs(bottomRight.y - topLeft.y);
            return new Rect(x, y, w, h);
        }
    }
}
#endif
