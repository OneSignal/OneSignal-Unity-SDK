using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
#if UNITY_2023_2_OR_NEWER
using UnityEngine.Accessibility;
#endif
using UnityEngine.UIElements;

namespace OneSignalDemo.Services
{
    /// <summary>
    /// Mirrors UI Toolkit's VisualElement tree into Unity's AccessibilityHierarchy so XCUITest
    /// (iOS) and UiAutomator2 (Android) can find elements by name. E2E mode only.
    /// </summary>
    public class AccessibilityBridge : MonoBehaviour
    {
#if UNITY_2023_2_OR_NEWER
        // GeometryChangedEvent is the hot path; this tick catches drift from sources that
        // don't raise it (animation curves, opacity tweens, value mutations).
        private const float FrameRefreshIntervalSeconds = 0.05f;
        private const float ScrollOffsetEpsilon = 0.5f;
        private const double ScrollSettleLayoutChangeDelayMs = 150.0;
        // DetachFromPanelEvent covers removals; this timer catches additions since a fresh
        // VisualElement has nothing to subscribe to until BuildHierarchy walks it.
        private const float StructurePollIntervalSeconds = 0.1f;
        private const string GameObjectName = "OneSignalAccessibilityBridge";
        private const double StalePointerCaptureWindowMs = 200.0;

        private static AccessibilityBridge _instance;
#if UNITY_ANDROID && !UNITY_EDITOR
        // UiAutomator2 sees a flat tree, so a "click" arriving by name must be routed to the
        // C# Action wired up by the named element's builder. Keyed by element instance.
        private static readonly Dictionary<VisualElement, AndroidClickTarget> AndroidClickTargets
            = new();
        private static AndroidJavaClass _androidAccessibilityBridge;
        private static bool _androidSyncErrorLogged;
#endif
#if UNITY_IOS && !UNITY_EDITOR
        // "*_info_icon" Labels have no Clickable manipulator and Appium's mobile:scroll drops
        // their AtTarget dispatch; we route via panel-root TrickleDown keyed by name.
        private static readonly Dictionary<string, IosInfoTap> _iosInfoTapByName = new();
#endif

        private VisualElement _root;
        private AccessibilityHierarchy _hierarchy;
        private readonly Dictionary<VisualElement, AccessibilityNode> _map = new();
        // Reused per BuildHierarchy to avoid per-rebuild allocation.
        private readonly Dictionary<string, AccessibilityNode> _existingByName = new();
        private readonly HashSet<string> _seenNames = new();
        private bool _resyncScheduled;
        private bool _frameRefreshScheduled;
        private float _frameRefreshTimer;
        private float _structurePollTimer;
        private int _lastStructureSignature = -1;
        private Vector2 _lastScrollOffset;
        private double _lastScrollChangeMs = -1.0;
        private bool _scrollLayoutChangePending;
        private readonly EventCallback<GeometryChangedEvent> _onGeometryChanged;
        private readonly EventCallback<DetachFromPanelEvent> _onDetachFromPanel;
        // BuildHierarchy can fire many times per session; this one-shot guards ScrollView taming.
        private bool _scrollViewHooksInstalled;
#if UNITY_IOS && !UNITY_EDITOR
        private readonly EventCallback<PointerDownEvent> _onIosInfoIconPointerDown;
#endif
        // Cached main ScrollView + stale-capture watchdog. A self-destroying child can leave
        // ScrollView's pending-capture latch set; we reset via reflection on idle.
        private ScrollView _mainSv;
        private double _lastTouchActivityMs = -1.0;
        private static FieldInfo _svPointerCaptureScheduledField;
        private static FieldInfo _svPressedField;
        private static bool _svReflectionResolved;

        // Unity's iOS a11y plugin treats AccessibilityNode.frameGetter Rect as physical screen
        // pixels; multiply panel-local worldBound by this to match.
        private float _panelToScreenScale = 1f;

        public AccessibilityBridge()
        {
            _onGeometryChanged = _ => ScheduleFrameRefresh();
            _onDetachFromPanel = _ => ScheduleResync();
#if UNITY_IOS && !UNITY_EDITOR
            _onIosInfoIconPointerDown = OnIosInfoIconPointerDown;
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        private static void OnIosInfoIconPointerDown(PointerDownEvent e)
        {
            if (!(e.target is VisualElement t)
                || string.IsNullOrEmpty(t.name)
                || !t.name.EndsWith("_info_icon"))
                return;
            if (!_iosInfoTapByName.TryGetValue(t.name, out var entry))
                return;
            if (!entry.IsEnabled())
                return;
            entry.Action();
            e.StopPropagation();
        }
#endif

        /// <summary>
        /// Idempotent entry point. Safe to call multiple times; no-ops outside E2E_MODE.
        /// </summary>
        public static void EnableForE2E(VisualElement root)
        {
            if (root == null)
                return;
            if (!DotEnv.IsE2EMode)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                Debug.LogWarning("[OneSignalDemo] E2E accessibility bridge disabled");
#endif
                return;
            }

            if (_instance == null)
            {
                var go = new GameObject(GameObjectName);
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<AccessibilityBridge>();
            }

            _instance.Initialize(root);
#if UNITY_ANDROID && !UNITY_EDITOR
            Debug.Log("[OneSignalDemo] E2E accessibility bridge enabled");
#endif
        }

        /// <summary>
        /// Android: reports the element as role=button and routes "click" by name. iOS: only
        /// "*_info_icon" Labels need this; other taps use UI Toolkit's Clickable.
        /// </summary>
        public static void RegisterE2ETapTarget(
            VisualElement target,
            Func<bool> isEnabled,
            Action action
        )
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (target == null || action == null)
                return;
            AndroidClickTargets[target] = new AndroidClickTarget(
                isEnabled ?? (() => true),
                action
            );
            target.RegisterCallback<DetachFromPanelEvent>(OnAndroidClickTargetDetached);
#elif UNITY_IOS && !UNITY_EDITOR
            if (target == null || action == null || string.IsNullOrEmpty(target.name))
                return;
            if (!target.name.EndsWith("_info_icon"))
                return;
            _iosInfoTapByName[target.name] = new IosInfoTap(
                isEnabled ?? (() => true),
                action
            );
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void OnAndroidClickTargetDetached(DetachFromPanelEvent e)
        {
            if (e.target is VisualElement target)
                AndroidClickTargets.Remove(target);
        }
#endif

        public static void RequestResync()
        {
            _instance?.ScheduleResync();
        }

        public static void RequestImmediateResync()
        {
            _instance?.BuildHierarchy();
        }

        private void Initialize(VisualElement root)
        {
            if (_root != root)
            {
                UnregisterTreeCallbacks();
                _mainSv = null;
                _lastTouchActivityMs = -1.0;
                _scrollViewHooksInstalled = false;
            }

            _root = root;
            _lastStructureSignature = -1; // force first poll to rebuild

            // AssistiveSupport.activeHierarchy is auto-cleared when the OS reports screen
            // reader off; XCUITest queries without enabling VoiceOver, so we have to lie.
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

        // Forces a rebuild + screen-changed ping on every foreground because WDA/UiAutomator2
        // cache the tree and don't reliably invalidate when the post-resume tree is identical.
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus || _hierarchy == null || _root == null)
                return;
            BuildHierarchy();
            AssistiveSupport.notificationDispatcher?.SendScreenChanged();
        }

        private void UnregisterTreeCallbacks()
        {
            if (_root != null)
            {
                _root.UnregisterCallback(_onGeometryChanged, TrickleDown.TrickleDown);
#if UNITY_IOS && !UNITY_EDITOR
                var visualTree = _root.panel?.visualTree;
                if (visualTree != null)
                    visualTree.UnregisterCallback(_onIosInfoIconPointerDown, TrickleDown.TrickleDown);
#endif
            }
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
            {
                _hierarchy.RefreshNodeFrames();
                SyncAndroidNativeAccessibility();
            }
        }

        private void Update()
        {
            if (_hierarchy == null || _root == null)
                return;

            _structurePollTimer += Time.unscaledDeltaTime;
            // Stale-pointer-capture watchdog: see _mainSv field comment.
            if (_lastTouchActivityMs > 0.0)
            {
                double now = Time.realtimeSinceStartupAsDouble * 1000.0;
                if (now - _lastTouchActivityMs > StalePointerCaptureWindowMs)
                {
                    ResetScrollViewPanState(_mainSv);
                    _lastTouchActivityMs = -1.0;
                }
            }
            RefreshScrollAccessibilityState();

            if (_structurePollTimer >= StructurePollIntervalSeconds)
            {
                _structurePollTimer = 0f;
                int signature = ComputeStructureSignature(_root);
                if (signature != _lastStructureSignature)
                {
                    _lastStructureSignature = signature;
                    ScheduleResync();
                }
            }

            // AccessibilityNode.value has no per-node change notification; diff in-tick and
            // re-publish only when something changed to avoid every-frame churn.
            _frameRefreshTimer += Time.unscaledDeltaTime;
            if (_frameRefreshTimer >= FrameRefreshIntervalSeconds)
            {
                _frameRefreshTimer = 0f;
                // Skip the sync when nothing changed — unconditional sync floods UiAutomator2
                // with TYPE_WINDOW_CONTENT_CHANGED events and stalls every click.
                if (RefreshNodeValuesAndActive())
                {
                    _hierarchy.RefreshNodeFrames();
                    SyncAndroidNativeAccessibility();
                }
            }
        }

        private bool RefreshNodeValuesAndActive()
        {
            bool anyChanged = false;
            foreach (var kvp in _map)
            {
                var el = kvp.Key;
                var node = kvp.Value;
                var newValue = ExtractValue(el);
                var newActive = IsVisible(el);
                if (node.value == newValue && node.isActive == newActive)
                    continue;

                node.value = newValue;
                node.isActive = newActive;
                anyChanged = true;
            }
            return anyChanged;
        }

        // Order-sensitive FNV-1a hash over named descendants. Hashing (not counting) names
        // catches dialog→section swaps where cardinality stays the same.
        private static int ComputeStructureSignature(VisualElement el)
        {
            unchecked
            {
                int hash = (int)2166136261u;
                AccumulateNameHash(el, ref hash);
                return hash;
            }
        }

        private static void AccumulateNameHash(VisualElement el, ref int hash)
        {
            if (el == null)
                return;
            unchecked
            {
                if (!string.IsNullOrEmpty(el.name))
                {
                    var name = el.name;
                    for (int i = 0; i < name.Length; i++)
                    {
                        hash ^= name[i];
                        hash *= 16777619;
                    }
                    hash ^= '|';
                    hash *= 16777619;
                }
                for (int i = 0; i < el.childCount; i++)
                    AccumulateNameHash(el[i], ref hash);
            }
        }

        private void OnScreenReaderStatusChanged(bool _)
        {
            // Unity nulls activeHierarchy when the OS reports screen reader off; reassert.
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
            // Batch child mutations into a single end-of-frame rebuild.
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

            // Recompute panel→screen scale before walking so each frameGetter sees the ratio.
            var rootBound = _root.worldBound;
            _panelToScreenScale =
                rootBound.width > 0 ? Screen.width / rootBound.width : 1f;

            // TrickleDown so we get geometry events from every descendant without per-element
            // registration. Drives a coalesced end-of-frame RefreshNodeFrames.
            _root.RegisterCallback(_onGeometryChanged, TrickleDown.TrickleDown);
#if UNITY_IOS && !UNITY_EDITOR
            // Register on panel.visualTree (the topmost dispatch point), not _root, so nothing
            // above _root can StopPropagation before our handler runs.
            var visualTree = _root.panel?.visualTree;
            if (visualTree != null)
                visualTree.RegisterCallback(_onIosInfoIconPointerDown, TrickleDown.TrickleDown);
#endif

            // One-shot per root: BuildHierarchy fires many times but a scene/root swap is
            // detected in Initialize, which resets this flag.
            if (!_scrollViewHooksInstalled)
            {
                var mainSv = _root.Q<ScrollView>("main_scroll_view");
                if (mainSv != null)
                {
                    _mainSv = mainSv;
                    _lastScrollOffset = mainSv.scrollOffset;
                    _lastScrollChangeMs = -1.0;
                    _scrollLayoutChangePending = false;
                    InstallScrollViewE2EHooks(mainSv);
                    _scrollViewHooksInstalled = true;
                }
            }

            // Incremental rebuild: reuse AccessibilityNodes by name so WDA's cached element
            // refs survive across rebuilds (Clear() + re-add invalidates them all).
            _existingByName.Clear();
            foreach (var kvp in _map)
            {
                var n = kvp.Value;
                if (n != null && !string.IsNullOrEmpty(kvp.Key?.name))
                    _existingByName[kvp.Key.name] = n;
            }

            _map.Clear();
            _seenNames.Clear();
            bool addedAny = false;
            WalkAndUpsert(_root, ref addedAny);

            bool removedAny = false;
            foreach (var kvp in _existingByName)
            {
                if (_seenNames.Contains(kvp.Key))
                    continue;
                try { _hierarchy.RemoveNode(kvp.Value); }
                catch { /* node may already be detached */ }
                removedAny = true;
            }

            _lastStructureSignature = ComputeStructureSignature(_root);

            if (AssistiveSupport.activeHierarchy != _hierarchy)
                AssistiveSupport.activeHierarchy = _hierarchy;
            // SendScreenChanged invalidates cached element refs; skip when only re-binding
            // existing nodes — frame/value updates ride out via RefreshNodeFrames.
            if (addedAny || removedAny)
                AssistiveSupport.notificationDispatcher?.SendScreenChanged();
            _hierarchy.RefreshNodeFrames();
            SyncAndroidNativeAccessibility();
        }

        public void HandleAndroidAccessibilityAction(string payload)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (string.IsNullOrEmpty(payload) || _root == null)
                return;

            var firstBreak = payload.IndexOf('\n');
            if (firstBreak < 0)
                return;
            var secondBreak = payload.IndexOf('\n', firstBreak + 1);
            if (secondBreak < 0)
                return;

            var id = payload.Substring(0, firstBreak);
            var action = payload.Substring(firstBreak + 1, secondBreak - firstBreak - 1);
            var value = payload.Substring(secondBreak + 1);

            switch (action)
            {
                case "setValue":
                    var field = _root.Q<TextField>(id);
                    if (field != null && field.value != value)
                        field.value = value;
                    break;
                case "click":
                    InvokeAndroidNativeAction(id);
                    break;
                case "scroll":
                    InvokeAndroidNativeScroll(value);
                    break;
                case "scrollDelta":
                    InvokeAndroidNativeScrollDelta(value);
                    break;
            }
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaClass AndroidAccessibilityBridge
        {
            get
            {
                _androidAccessibilityBridge ??= new AndroidJavaClass(
                    "com.onesignal.onesignalsdk.OneSignalUnityE2EAccessibility"
                );
                return _androidAccessibilityBridge;
            }
        }

        private void SyncAndroidNativeAccessibility()
        {
            try
            {
                AndroidAccessibilityBridge.CallStatic("beginSync");
                foreach (var kvp in _map)
                {
                    var el = kvp.Key;
                    if (el == null || string.IsNullOrEmpty(el.name))
                        continue;

                    var rect = GetScreenRect(el);
                    AndroidAccessibilityBridge.CallStatic(
                        "syncElement",
                        el.name,
                        ExtractValue(el),
                        IsVisible(el) && rect.width > 0f && rect.height > 0f,
                        Mathf.RoundToInt(rect.x),
                        Mathf.RoundToInt(rect.y),
                        Mathf.RoundToInt(rect.width),
                        Mathf.RoundToInt(rect.height),
                        AndroidNativeRole(el),
                        el.enabledInHierarchy
                    );
                }
                AndroidAccessibilityBridge.CallStatic("endSync");
            }
            catch (Exception ex)
            {
                if (!_androidSyncErrorLogged)
                {
                    _androidSyncErrorLogged = true;
                    Debug.LogWarning(
                        $"[OneSignalDemo] Android E2E accessibility sync failed: {ex.Message}"
                    );
                }
            }
        }

        private void InvokeAndroidNativeAction(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            var switchToggle = _root.Q<OneSignalDemo.UI.SwitchToggle>(id);
            if (switchToggle != null)
            {
                switchToggle.SetValueAndNotify(!switchToggle.Value);
                return;
            }

            // BaseBoolField is the common base of Toggle AND RadioButton — Q<Toggle> would
            // miss radios entirely, making the overlay click a no-op for radio selections.
            var boolField = _root.Q<BaseBoolField>(id);
            if (boolField != null)
            {
                if (boolField is RadioButton && boolField.value)
                    return;

                boolField.value = !boolField.value;
                return;
            }

            Action actionToInvoke = null;
            List<VisualElement> detachedTargets = null;
            foreach (var kvp in AndroidClickTargets)
            {
                var el = kvp.Key;
                if (el == null || el.panel == null || !IsDescendantOfRoot(el))
                {
                    if (el != null)
                    {
                        detachedTargets ??= new List<VisualElement>();
                        detachedTargets.Add(el);
                    }
                    continue;
                }
                if (el.name != id || !IsVisible(el) || !kvp.Value.IsEnabled())
                    continue;
                actionToInvoke = kvp.Value.Action;
                break;
            }
            if (detachedTargets != null)
            {
                foreach (var target in detachedTargets)
                    AndroidClickTargets.Remove(target);
            }
            actionToInvoke?.Invoke();
        }

        private bool IsDescendantOfRoot(VisualElement el)
        {
            for (var current = el; current != null; current = current.hierarchy.parent)
            {
                if (current == _root)
                    return true;
            }
            return false;
        }

        private void InvokeAndroidNativeScroll(string direction)
        {
            if (_mainSv == null)
                return;

            var viewportHeight = _mainSv.contentViewport?.layout.height ?? _mainSv.layout.height;
            var contentHeight = _mainSv.contentContainer?.layout.height ?? 0f;
            if (viewportHeight <= 0f || contentHeight <= viewportHeight)
                return;

            var current = _mainSv.scrollOffset;
            var delta = Mathf.Max(80f, viewportHeight * 0.6f);
            var maxY = Mathf.Max(0f, contentHeight - viewportHeight);
            var nextY = direction == "up" ? current.y - delta : current.y + delta;
            ApplyScrollOffset(current.x, nextY, maxY);
        }

        // Pixel-accurate scroll from the native overlay's swipe intercept. Positive value =
        // scroll content forward; convert screen px → panel units before applying.
        private void InvokeAndroidNativeScrollDelta(string deltaStr)
        {
            if (_mainSv == null || string.IsNullOrEmpty(deltaStr))
                return;
            if (!float.TryParse(
                    deltaStr,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var screenDelta))
                return;

            var viewportHeight = _mainSv.contentViewport?.layout.height ?? _mainSv.layout.height;
            var contentHeight = _mainSv.contentContainer?.layout.height ?? 0f;
            if (viewportHeight <= 0f || contentHeight <= viewportHeight)
                return;

            var scale = _panelToScreenScale > 0f ? _panelToScreenScale : 1f;
            var panelDelta = screenDelta / scale;
            var current = _mainSv.scrollOffset;
            var maxY = Mathf.Max(0f, contentHeight - viewportHeight);
            ApplyScrollOffset(current.x, current.y + panelDelta, maxY);
        }

        private void ApplyScrollOffset(float x, float y, float maxY)
        {
            if (_mainSv == null)
                return;
            _mainSv.scrollOffset = new Vector2(x, Mathf.Clamp(y, 0f, maxY));

            RefreshNodeValuesAndActive();
            _hierarchy?.RefreshNodeFrames();
            SyncAndroidNativeAccessibility();
        }

        private static string AndroidNativeRole(VisualElement el) => el switch
        {
            TextField => "input",
            BaseBoolField => "toggle",
            OneSignalDemo.UI.SwitchToggle => "toggle",
            Button => "button",
            _ => AndroidClickTargets.ContainsKey(el) ? "button" : "text",
        };
#else
        private void SyncAndroidNativeAccessibility() { }
#endif

        private void RefreshScrollAccessibilityState()
        {
            if (_mainSv == null || _hierarchy == null)
                return;

            var scrollOffset = _mainSv.scrollOffset;
            if ((scrollOffset - _lastScrollOffset).sqrMagnitude > ScrollOffsetEpsilon * ScrollOffsetEpsilon)
            {
                _lastScrollOffset = scrollOffset;
                _lastScrollChangeMs = Time.realtimeSinceStartupAsDouble * 1000.0;
                _scrollLayoutChangePending = true;
                RefreshNodeValuesAndActive();
                _hierarchy.RefreshNodeFrames();
                // Ping during scroll too — the settle ping fires 150ms after the last change
                // and can land after the test's refetch, leaving the queued tap on stale coords.
                AssistiveSupport.notificationDispatcher?.SendScreenChanged();
                return;
            }

            if (!_scrollLayoutChangePending || _lastScrollChangeMs < 0.0)
                return;

            double now = Time.realtimeSinceStartupAsDouble * 1000.0;
            if (now - _lastScrollChangeMs < ScrollSettleLayoutChangeDelayMs)
                return;

            _scrollLayoutChangePending = false;
            _hierarchy.RefreshNodeFrames();
            // WDA caches element frames until an a11y notification arrives. One ping on
            // settle keeps the rate sane and signals frames are stable and re-readable.
            AssistiveSupport.notificationDispatcher?.SendScreenChanged();
        }

        /// <summary>
        /// Tames the main ScrollView so XCUITest taps land deterministically: block synthetic
        /// WheelEvents (mobile:scroll injected mid-tap) and disable post-pointer-up animation
        /// so queued taps don't land on shifted content. Real swipes still work via PointerDown/Move/Up.
        /// </summary>
        private void InstallScrollViewE2EHooks(ScrollView mainSv)
        {
            mainSv.RegisterCallback<WheelEvent>(
                e => e.StopImmediatePropagation(),
                TrickleDown.TrickleDown
            );
            // Touch-activity watchdog: real Down/Move resets the timer; the Update tick clears
            // ScrollView's pan-capture latch if no touch arrives within the window.
            mainSv.RegisterCallback<PointerDownEvent>(
                _ => _lastTouchActivityMs = Time.realtimeSinceStartupAsDouble * 1000.0,
                TrickleDown.TrickleDown
            );
            mainSv.RegisterCallback<PointerMoveEvent>(
                _ => _lastTouchActivityMs = Time.realtimeSinceStartupAsDouble * 1000.0,
                TrickleDown.TrickleDown
            );
            mainSv.RegisterCallback<PointerUpEvent>(
                _ => _lastTouchActivityMs = -1.0,
                TrickleDown.TrickleDown
            );
            mainSv.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
            mainSv.scrollDecelerationRate = 0f;
            mainSv.elasticity = 0f;
        }

        // Reset ScrollView's pan-capture latches via reflection. Only the "captured / pending
        // capture" fields are cleared; m_TouchPointerMoveAllowed is owned by Down/Up.
        private static void ResetScrollViewPanState(ScrollView sv)
        {
            if (sv == null) return;
            if (!_svReflectionResolved)
            {
                var t = typeof(ScrollView);
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                _svPointerCaptureScheduledField = t.GetField("m_PointerCaptureScheduled", flags);
                _svPressedField = t.GetField("m_Pressed", flags);
                _svReflectionResolved = true;
            }
            try
            {
                _svPointerCaptureScheduledField?.SetValue(sv, false);
                _svPressedField?.SetValue(sv, false);
            }
            catch
            {
            }
        }

        private void WalkAndUpsert(VisualElement el, ref bool addedAny)
        {
            if (el == null)
                return;

            // Publish every named element as a direct child of the hierarchy root (parent:
            // null) — iOS UIAccessibility containers must themselves be non-elements.
            if (!string.IsNullOrEmpty(el.name) && !_map.ContainsKey(el))
            {
                var name = el.name;
                // Duplicate names (e.g. freshly-rebuilt dialog) — first wins to keep ids unique.
                if (_seenNames.Add(name))
                {
                    if (!_existingByName.TryGetValue(name, out var node))
                    {
                        node = _hierarchy.AddNode(name, parent: null);
                        addedAny = true;
                    }
                    var captured = el;
                    node.frameGetter = () => GetScreenRect(captured);
                    node.role = MapRole(el);
                    node.value = ExtractValue(el);
                    node.isActive = IsVisible(el);
                    _map[el] = node;
                    // Detach schedules a resync so XCUITest can't read a node whose owner is gone.
                    el.RegisterCallback(_onDetachFromPanel);
                }
            }

            for (int i = 0; i < el.childCount; i++)
                WalkAndUpsert(el[i], ref addedAny);
        }

        private static AccessibilityRole MapRole(VisualElement el)
        {
            if (IsSectionAnchor(el))
                return AccessibilityRole.StaticText;

            return el switch
            {
                Button => AccessibilityRole.Button,
                BaseBoolField => AccessibilityRole.Toggle,
                OneSignalDemo.UI.SwitchToggle => AccessibilityRole.Toggle,
                // ScrollView intentionally NOT exposed: XCUIElement.tap()'s implicit
                // scrollToVisible would shift content mid-tap onto empty space.
                Slider => AccessibilityRole.Slider,
                TextField => AccessibilityRole.SearchField,
                Label => AccessibilityRole.StaticText,
                Image => AccessibilityRole.Image,
                _ => AccessibilityRole.None,
            };
        }

        private static string ExtractValue(VisualElement el) => el switch
        {
            TextField tf => tf.value ?? string.Empty,
            BaseBoolField b => b.value ? "1" : "0",
            // Demo's custom SwitchToggle isn't a UI Toolkit Toggle — selectors expect "1"/"0".
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

            if (IsSectionAnchor(el))
                return true;

            // Clipped children aren't visible; native SDKs get this for free via their
            // backing scroll views, but UI Toolkit reports panel coords regardless of clip.
            var visible = ComputeVisibleWorldBound(GetFrameElement(el));
            return visible.width > 0f && visible.height > 0f;
        }

        /// <summary>
        /// Intersects an element's worldBound with every clipping ancestor (ScrollView's
        /// contentViewport). Returns Rect.zero when fully clipped.
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

        private static VisualElement GetFrameElement(VisualElement el)
        {
            if (IsSectionAnchor(el) && el.childCount > 0)
                return el[0];
            return el;
        }

        private static bool IsSectionAnchor(VisualElement el) =>
            !string.IsNullOrEmpty(el?.name)
            && el.name.EndsWith("_section", StringComparison.Ordinal);

        private Rect GetScreenRect(VisualElement el)
        {
            if (el?.panel == null)
                return Rect.zero;

            var wb = ComputeVisibleWorldBound(GetFrameElement(el));
            if (wb.width <= 0f || wb.height <= 0f)
                return Rect.zero;

            float s = _panelToScreenScale;
            return new Rect(wb.x * s, wb.y * s, wb.width * s, wb.height * s);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private sealed class AndroidClickTarget
        {
            public readonly Func<bool> IsEnabled;
            public readonly Action Action;

            public AndroidClickTarget(Func<bool> isEnabled, Action action)
            {
                IsEnabled = isEnabled;
                Action = action;
            }
        }
#endif
#if UNITY_IOS && !UNITY_EDITOR
        private sealed class IosInfoTap
        {
            public readonly Func<bool> IsEnabled;
            public readonly Action Action;

            public IosInfoTap(Func<bool> isEnabled, Action action)
            {
                IsEnabled = isEnabled;
                Action = action;
            }
        }
#endif
#else
        public static void EnableForE2E(VisualElement root) { }

        public static void RegisterE2ETapTarget(
            VisualElement target,
            Func<bool> isEnabled,
            Action action
        ) { }

        public static void RequestResync() { }

        public static void RequestImmediateResync() { }
#endif
    }
}
