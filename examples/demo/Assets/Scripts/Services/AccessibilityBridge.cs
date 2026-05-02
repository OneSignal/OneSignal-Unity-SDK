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
        // Throttle frame refreshes so scroll/resize don't spam the OS. 50ms
        // (20 Hz) is short enough that mid-scroll-animation taps land on the
        // correct element (Appium tap latency is ~80-150ms, so the frame the
        // test framework reads is at most one tick stale), and long enough
        // not to overwhelm the platform plugin. At the original 250ms,
        // frames lagged the live UI badly enough that taps for
        // send_push_info_icon landed on send_simple_button.
        private const float FrameRefreshIntervalSeconds = 0.05f;
        // Throttle structural-change polls. Faster than frame refresh because
        // dialog open/close should reach the test framework quickly.
        private const float StructurePollIntervalSeconds = 0.1f;

        private static AccessibilityBridge _instance;

        private VisualElement _root;
        private AccessibilityHierarchy _hierarchy;
        private readonly Dictionary<VisualElement, AccessibilityNode> _map = new();
        private bool _resyncScheduled;
        private float _frameRefreshTimer;
        private float _structurePollTimer;
        private int _lastNamedDescendantCount = -1;
        // Empirically, Unity's iOS accessibility plugin treats the Rect we
        // hand to AccessibilityNode.frameGetter as if it were already in
        // physical screen pixels, then divides by the device scale to get
        // UIKit points. UI Toolkit, however, reports VisualElement.worldBound
        // in panel-local coordinates (reference resolution scaled to fit), so
        // unscaled frames render ~1/scale of where the UI is actually drawn.
        // This factor brings panel coords into the same space the plugin
        // expects; recomputed on every BuildHierarchy in case the panel size
        // changes (rotation, safe-area shift, multi-display).
        private float _panelToScreenScale = 1f;

        /// <summary>
        /// Idempotent entry point. Safe to call multiple times; only the first
        /// call wires the bridge. No-ops outside E2E_MODE.
        /// </summary>
        public static void EnableForE2E(VisualElement root)
        {
            if (root == null)
            {
                Debug.Log("[A11yBridge] EnableForE2E: root is null, skipping");
                return;
            }
            if (!DotEnv.IsE2EMode)
            {
                Debug.Log("[A11yBridge] EnableForE2E: E2E_MODE not set, skipping");
                return;
            }

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
            _root = root;
            _lastNamedDescendantCount = -1; // force first poll to rebuild

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
            AssistiveSupport.screenReaderStatusChanged -= OnScreenReaderStatusChanged;
            AssistiveSupport.activeHierarchy = null;
            AssistiveSupport.screenReaderStatusOverride =
                AssistiveSupport.ScreenReaderStatusOverride.OSDriven;
            if (_instance == this)
                _instance = null;
        }

        private void Update()
        {
            if (_hierarchy == null || _root == null)
                return;

            // Cheap structural-change check: count named descendants and
            // rebuild on diff. Catches dialog open/close, scene swap, section
            // refresh — every test-relevant mutation creates or removes named
            // VisualElements. Avoids hot event hooks during layout.
            _structurePollTimer += Time.unscaledDeltaTime;
            if (_structurePollTimer >= StructurePollIntervalSeconds)
            {
                _structurePollTimer = 0f;
                int count = CountNamedDescendants(_root);
                if (count != _lastNamedDescendantCount)
                {
                    _lastNamedDescendantCount = count;
                    ScheduleResync();
                }
            }

            // Live frames + values: scroll/animation moves elements and
            // toggles/labels mutate text without altering tree structure.
            // RefreshNodeFrames covers geometry; AccessibilityNode.value is
            // a plain field with no per-node "value changed" notification
            // (the dispatcher only exposes Layout/Screen/Page/Announcement),
            // so writes don't push to the platform unless we re-publish the
            // whole hierarchy. We compare against last-known values per tick
            // and rebuild only when something actually changed — avoids the
            // every-frame churn the Unity perf docs warn against.
            _frameRefreshTimer += Time.unscaledDeltaTime;
            if (_frameRefreshTimer >= FrameRefreshIntervalSeconds)
            {
                _frameRefreshTimer = 0f;
                bool dirty = false;
                foreach (var kvp in _map)
                {
                    var el = kvp.Key;
                    var node = kvp.Value;
                    var newValue = ExtractValue(el);
                    var newActive = IsVisible(el);
                    if (node.value != newValue || node.isActive != newActive)
                    {
                        node.value = newValue;
                        node.isActive = newActive;
                        dirty = true;
                    }
                }
                _hierarchy.RefreshNodeFrames();
                if (dirty)
                    ScheduleResync();
                else
                    AssistiveSupport.notificationDispatcher?.SendLayoutChanged();
            }
        }

        private static int CountNamedDescendants(VisualElement el)
        {
            if (el == null)
                return 0;
            int count = string.IsNullOrEmpty(el.name) ? 0 : 1;
            for (int i = 0; i < el.childCount; i++)
                count += CountNamedDescendants(el[i]);
            return count;
        }

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

            // Recompute the panel→screen scale before walking the tree so each
            // node's frameGetter sees the correct ratio.
            var rootBound = _root.worldBound;
            _panelToScreenScale =
                rootBound.width > 0 ? Screen.width / rootBound.width : 1f;

            WalkAndAdd(_root, parent: null);

            AssistiveSupport.activeHierarchy = _hierarchy;
            AssistiveSupport.notificationDispatcher?.SendScreenChanged();

            Debug.Log(
                $"[A11yBridge] Built hierarchy with {_map.Count} named nodes. "
                    + $"Active={AssistiveSupport.activeHierarchy != null}, "
                    + $"ScreenReader={AssistiveSupport.isScreenReaderEnabled}"
            );
            // Log a sample so we can confirm specific test ids reached the OS.
            var sample = new System.Text.StringBuilder("[A11yBridge] Names: ");
            int i = 0;
            foreach (var kvp in _map)
            {
                if (i++ > 0) sample.Append(", ");
                sample.Append(kvp.Key.name);
            }
            Debug.Log(sample.ToString());
        }

        private void WalkAndAdd(VisualElement el, AccessibilityNode parent)
        {
            if (el == null)
                return;

            // Publish every named element as a direct child of the hierarchy
            // root (parent: null) — iOS UIAccessibility requires container
            // elements to themselves be non-elements. Nesting our nodes makes
            // ancestors opaque hit targets that swallow visibility tests for
            // descendants (XCUITest reports visible="false" even when the
            // child's frame is on screen). The test framework only ever looks
            // up by name, so containment doesn't affect reachability.
            if (!string.IsNullOrEmpty(el.name))
            {
                var node = _hierarchy.AddNode(el.name, parent: null);
                var captured = el;
                node.frameGetter = () => GetScreenRect(captured);
                node.role = MapRole(el);
                node.value = ExtractValue(el);
                node.isActive = IsVisible(el);
                _map[el] = node;
            }

            for (int i = 0; i < el.childCount; i++)
                WalkAndAdd(el[i], parent: null);
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

        private Rect GetScreenRect(VisualElement el)
        {
            if (el?.panel == null)
                return Rect.zero;

            var wb = el.worldBound;
            if (float.IsNaN(wb.x) || float.IsNaN(wb.y) || wb.width <= 0 || wb.height <= 0)
                return Rect.zero;

            float s = _panelToScreenScale;
            return new Rect(wb.x * s, wb.y * s, wb.width * s, wb.height * s);
        }
    }
}
#endif
