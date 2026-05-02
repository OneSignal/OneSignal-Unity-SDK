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
        // Backstop frame refresh tick. The hot path is GeometryChangedEvent
        // (registered in BuildHierarchy) which dispatches a same-frame
        // refresh whenever anything in the tree resizes, scrolls, or
        // re-layouts. The timer only catches drift from sources that don't
        // raise GeometryChangedEvent — animation curves, opacity tweens, and
        // value mutations on TextField/Toggle/Label.
        private const float FrameRefreshIntervalSeconds = 0.05f;
        // Backstop structural poll. The hot path is DetachFromPanelEvent
        // (per node, see WalkAndAdd) which schedules an immediate resync the
        // moment a tracked element leaves the tree. The timer catches
        // additions, since a freshly-created VisualElement has nothing for
        // us to subscribe to until BuildHierarchy walks it.
        private const float StructurePollIntervalSeconds = 0.1f;

        private static AccessibilityBridge _instance;

        private VisualElement _root;
        private AccessibilityHierarchy _hierarchy;
        private readonly Dictionary<VisualElement, AccessibilityNode> _map = new();
        private bool _resyncScheduled;
        private bool _frameRefreshScheduled;
        private float _frameRefreshTimer;
        private float _structurePollTimer;
        private int _lastNamedDescendantCount = -1;
        private readonly EventCallback<GeometryChangedEvent> _onGeometryChanged;
        private readonly EventCallback<DetachFromPanelEvent> _onDetachFromPanel;
        // Set true after the one-shot ScrollView taming hooks (WheelEvent
        // block + clamp settings) are installed; BuildHierarchy can fire many
        // times per session and we only want one subscription.
        private bool _scrollViewHooksInstalled;

        public AccessibilityBridge()
        {
            _onGeometryChanged = _ => ScheduleFrameRefresh();
            _onDetachFromPanel = _ => ScheduleResync();
        }

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
            UnregisterTreeCallbacks();
            if (_instance == this)
                _instance = null;
        }

        private void UnregisterTreeCallbacks()
        {
            if (_root != null)
                _root.UnregisterCallback(_onGeometryChanged, TrickleDown.TrickleDown);
            foreach (var el in _map.Keys)
                el.UnregisterCallback(_onDetachFromPanel);
        }

        private void ScheduleFrameRefresh()
        {
            if (_frameRefreshScheduled || !isActiveAndEnabled)
                return;
            _frameRefreshScheduled = true;
            StartCoroutine(RefreshFramesEndOfFrame());
        }

        private IEnumerator RefreshFramesEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            _frameRefreshScheduled = false;
            if (_hierarchy != null)
                _hierarchy.RefreshNodeFrames();
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
                bool valueDirty = false;
                foreach (var kvp in _map)
                {
                    var el = kvp.Key;
                    var node = kvp.Value;
                    var newValue = ExtractValue(el);
                    var newActive = IsVisible(el);
                    bool valueChanged = node.value != newValue;
                    bool activeChanged = node.isActive != newActive;
                    if (valueChanged || activeChanged)
                    {
                        // In-place mutation. The visibility flip is enough for
                        // XCUITest because GetScreenRect already returns
                        // (0,0,0,0) for clipped elements, which iOS reports as
                        // hittable=false. We MUST NOT call ScheduleResync on
                        // active-only flips: every scroll moves elements into
                        // and out of the ScrollView viewport, and a republish
                        // (SendScreenChanged) issued mid-tap makes iOS drop
                        // the pending touch — that was the root cause of the
                        // "no PointerDown, button moved 120pt" failure.
                        node.value = newValue;
                        node.isActive = newActive;
                        if (valueChanged)
                            valueDirty = true;
                    }
                }
                _hierarchy.RefreshNodeFrames();
                if (valueDirty)
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

            UnregisterTreeCallbacks();

            _hierarchy ??= new AccessibilityHierarchy();
            _hierarchy.Clear();
            _map.Clear();

            // Recompute the panel→screen scale before walking the tree so each
            // node's frameGetter sees the correct ratio.
            var rootBound = _root.worldBound;
            _panelToScreenScale =
                rootBound.width > 0 ? Screen.width / rootBound.width : 1f;

            // TrickleDown so we receive geometry events bubbling up from
            // every descendant without having to register per-element. One
            // callback drives a coalesced end-of-frame RefreshNodeFrames so
            // a tap fired immediately after a scroll sees fresh frames
            // instead of a 50ms-stale snapshot.
            _root.RegisterCallback(_onGeometryChanged, TrickleDown.TrickleDown);

            // One-shot ScrollView taming: BuildHierarchy can run dozens of
            // times per session (every dialog open/close, every section
            // refresh), but the main ScrollView only needs these hooks once.
            // The flag is reset only by OnDestroy — a fresh bridge instance
            // re-applies them.
            if (!_scrollViewHooksInstalled)
            {
                var mainSv = _root.Q<ScrollView>("main_scroll_view");
                if (mainSv != null)
                {
                    InstallScrollViewE2EHooks(mainSv);
                    _scrollViewHooksInstalled = true;
                }
            }

            WalkAndAdd(_root);

            AssistiveSupport.activeHierarchy = _hierarchy;
            AssistiveSupport.notificationDispatcher?.SendScreenChanged();
            _hierarchy.RefreshNodeFrames();

            Debug.Log(
                $"[A11yBridge] Built hierarchy with {_map.Count} named nodes. "
                    + $"Active={AssistiveSupport.activeHierarchy != null}, "
                    + $"ScreenReader={AssistiveSupport.isScreenReaderEnabled}"
            );

            var sample = new System.Text.StringBuilder("[A11yBridge] Names: ");
            int i = 0;
            foreach (var kvp in _map)
            {
                if (i++ > 0) sample.Append(", ");
                sample.Append(kvp.Key.name);
            }
            Debug.Log(sample.ToString());
        }

        /// <summary>
        /// Tames the main ScrollView so XCUITest taps land deterministically
        /// on iOS. Two distinct sources can shift content during a queued tap
        /// and make the click land on the wrong element:
        ///   1. iOS XCUITest performs an implicit "scroll-to-visible" before
        ///      dispatching the tap. The scroll arrives in UI Toolkit as a
        ///      synthetic WheelEvent injected by DefaultEventSystem
        ///      (SendPositionBasedEvent ← InputForUIProcessor.ProcessPointerEvent).
        ///      The ScrollView consumes it and scrolls scrollOffset by ~120pt
        ///      mid-tap, leaving the queued click on empty space.
        ///   2. After a swipe gesture, ScrollView schedules
        ///      `PostPointerUpAnimation` (elasticity bounce + inertia) via
        ///      TimerEventScheduler. That tick fires AFTER the swipe ends and
        ///      can run DURING the next queued tap, scrolling the content a
        ///      few points so the click lands on `unity-content-container`.
        /// Block both: stop WheelEvent at the ScrollView, and clamp the touch
        /// scroll behaviour so there is no post-pointer-up animation. Manual
        /// swipe gestures still work because they go through the
        /// PointerDown/Move/Up path; the content just stops the moment the
        /// finger lifts.
        /// </summary>
        private static void InstallScrollViewE2EHooks(ScrollView mainSv)
        {
            mainSv.RegisterCallback<WheelEvent>(
                e => e.StopImmediatePropagation(),
                TrickleDown.TrickleDown
            );
            mainSv.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
            mainSv.scrollDecelerationRate = 0f;
            mainSv.elasticity = 0f;
        }

        private void WalkAndAdd(VisualElement el)
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
                if (_map.ContainsKey(el))
                {
                    Debug.LogWarning(
                        $"[A11yBridge] Duplicate element registered for name '{el.name}'; skipping"
                    );
                }
                else
                {
                    var node = _hierarchy.AddNode(el.name, parent: null);
                    var captured = el;
                    node.frameGetter = () => GetScreenRect(captured);
                    node.role = MapRole(el);
                    node.value = ExtractValue(el);
                    node.isActive = IsVisible(el);
                    _map[el] = node;
                    // Detach fires the moment a tracked element leaves the
                    // panel — Dismiss(), section refresh, scene swap. Drives
                    // an immediate ScheduleResync so XCUITest can't read a
                    // node whose owner is already gone (the source of the
                    // "stale element" warnings during dialog teardown).
                    el.RegisterCallback(_onDetachFromPanel);
                }
            }

            for (int i = 0; i < el.childCount; i++)
                WalkAndAdd(el[i]);
        }

        private static AccessibilityRole MapRole(VisualElement el) => el switch
        {
            Button => AccessibilityRole.Button,
            Toggle => AccessibilityRole.Toggle,
            OneSignalDemo.UI.SwitchToggle => AccessibilityRole.Toggle,
            // ScrollView intentionally NOT exposed as AccessibilityRole.ScrollView.
            // XCUITest's `XCUIElement.tap()` performs an implicit
            // `scrollToVisible` on the nearest accessibility ancestor that
            // claims a scrollable role, which under our setup invoked Unity's
            // ScrollView mid-tap, shifted the entire content by ~120pt, and
            // made the queued touch land on empty space (root cause of the
            // "no PointerDown, button moved 120pt" failure on the multiple
            // trigger test). Test code drives scrolling via raw swipes, so
            // dropping the role is a no-op for navigation.
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
            // The demo uses a custom SwitchToggle (VisualElement) that does
            // not inherit from UnityEngine.UIElements.Toggle, so the platform
            // a11y value would be empty without this case. The selectors
            // helper reads `value`/`checked` and expects "1"/"0" on iOS.
            OneSignalDemo.UI.SwitchToggle st => st.Value ? "1" : "0",
            Label l => l.text ?? string.Empty,
            Button b => b.text ?? string.Empty,
            _ => string.Empty,
        };

        private static bool IsVisible(VisualElement el)
        {
            var s = el.resolvedStyle;
            if (!el.enabledInHierarchy
                || s.display == DisplayStyle.None
                || s.visibility == Visibility.Hidden
                || s.opacity <= 0f)
                return false;

            // An element whose worldBound has been clipped away by an ancestor
            // ScrollView/clip container is visually invisible. Other SDKs get
            // this for free because they render through native UIScrollView /
            // android.widget.ScrollView whose accessibilityFrame already
            // excludes clipped children. UI Toolkit reports children's panel
            // coordinates regardless of clip, so we have to intersect here.
            var visible = ComputeVisibleWorldBound(el);
            return visible.width > 0f && visible.height > 0f;
        }

        /// <summary>
        /// Intersects an element's worldBound with every ancestor that clips
        /// (ScrollView's contentViewport, or any element with overflow !=
        /// Visible). Returns Rect.zero when the element is fully clipped.
        /// </summary>
        private static Rect ComputeVisibleWorldBound(VisualElement el)
        {
            var r = el.worldBound;
            if (float.IsNaN(r.x) || float.IsNaN(r.y) || r.width <= 0f || r.height <= 0f)
                return Rect.zero;

            var p = el.hierarchy.parent;
            while (p != null)
            {
                if (p is ScrollView sv)
                {
                    var clip = sv.contentViewport.worldBound;
                    float x = Mathf.Max(r.x, clip.x);
                    float y = Mathf.Max(r.y, clip.y);
                    float right = Mathf.Min(r.x + r.width, clip.x + clip.width);
                    float bottom = Mathf.Min(r.y + r.height, clip.y + clip.height);
                    if (right <= x || bottom <= y)
                        return Rect.zero;
                    r = new Rect(x, y, right - x, bottom - y);
                }
                p = p.hierarchy.parent;
            }
            return r;
        }

        private Rect GetScreenRect(VisualElement el)
        {
            if (el?.panel == null)
                return Rect.zero;

            var wb = ComputeVisibleWorldBound(el);
            if (wb.width <= 0f || wb.height <= 0f)
                return Rect.zero;

            float s = _panelToScreenScale;
            return new Rect(wb.x * s, wb.y * s, wb.width * s, wb.height * s);
        }
    }
}
#endif
