using System;
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
        private const float TapMarkerSize = 28f;
        private const int MaxTapMarkers = 40;
        private const float ScrollOffsetEpsilon = 0.5f;
        private const double ScrollSettleLayoutChangeDelayMs = 150.0;
        // Backstop structural poll. The hot path is DetachFromPanelEvent
        // (per node, see WalkAndAdd) which schedules an immediate resync the
        // moment a tracked element leaves the tree. The timer catches
        // additions, since a freshly-created VisualElement has nothing for
        // us to subscribe to until BuildHierarchy walks it.
        private const float StructurePollIntervalSeconds = 0.1f;
        private const string GameObjectName = "OneSignalAccessibilityBridge";

        private static AccessibilityBridge _instance;
#if UNITY_ANDROID && !UNITY_EDITOR
        // Named-element click registry. UiAutomator2 only sees a flat
        // accessibility tree, so a "click" arriving from the OS for element
        // foo_button must be routed to the C# Action that the foo button's
        // builder wired up. The dict is keyed by element instance and grows
        // for the life of the app; named-element churn is bounded by the
        // number of distinct UI builders, not test iterations.
        private static readonly Dictionary<VisualElement, AndroidClickTarget> AndroidClickTargets
            = new();
        private static AndroidJavaClass _androidAccessibilityBridge;
        private static bool _androidSyncErrorLogged;
#endif
#if UNITY_IOS && !UNITY_EDITOR
        // iOS-only name-keyed dispatch table for "*_info_icon" Labels.
        // Plain Label has no Clickable manipulator, so XCUITest taps reach
        // the target via UI Toolkit's hit-test but no AtTarget callback runs.
        // Worse, after iOS Appium injects a mobile:scroll before the tap,
        // AtTarget dispatch on the Label is observed to drop entirely — the
        // panel root sees the PointerDown with the correct target, but the
        // target's own callback never fires. We bypass AtTarget by dispatching
        // from a TrickleDown handler installed once on the panel root (see
        // BuildHierarchy), keyed by element name so modal overlays can't
        // bleed through.
        private static readonly Dictionary<string, IosInfoTap> _iosInfoTapByName
            = new();
#endif

        private VisualElement _root;
        private AccessibilityHierarchy _hierarchy;
        private readonly Dictionary<VisualElement, AccessibilityNode> _map = new();
        private bool _resyncScheduled;
        private bool _frameRefreshScheduled;
        private float _frameRefreshTimer;
        private float _structurePollTimer;
        private int _lastStructureSignature = -1;
        private int _tapMarkerCount;
        private VisualElement _tapMarkerOverlay;
        private Vector2 _lastScrollOffset;
        private double _lastScrollChangeMs = -1.0;
        private bool _scrollLayoutChangePending;
        private readonly EventCallback<GeometryChangedEvent> _onGeometryChanged;
        private readonly EventCallback<DetachFromPanelEvent> _onDetachFromPanel;
        private readonly EventCallback<PointerDownEvent> _onTapMarkerPointerDown;
        // Set true after the one-shot ScrollView taming hooks (WheelEvent
        // block + clamp settings) are installed; BuildHierarchy can fire many
        // times per session and we only want one subscription.
        private bool _scrollViewHooksInstalled;
#if UNITY_IOS && !UNITY_EDITOR
        private readonly EventCallback<PointerDownEvent> _onIosInfoIconPointerDown;
#endif
        // E2E only: cached ScrollView ref + stale-capture watchdog. A self-
        // destroying child (e.g. triggers_remove) can leave ScrollView's
        // pending-capture latch set because the matching PointerUp is dropped
        // when its target detaches mid-touch. The next unrelated pointer event
        // is then interpreted as a continuation of the prior drag, scrolling
        // the content out from under the test's tap. We reset the latch via
        // reflection if no pointer activity is observed for a short window.
        private ScrollView _mainSv;
        private double _lastTouchActivityMs = -1.0;
        private const double StalePointerCaptureWindowMs = 200.0;
        private static System.Reflection.FieldInfo _svPointerCaptureScheduledField;
        private static System.Reflection.FieldInfo _svPressedField;
        private static bool _svReflectionResolved;

        public AccessibilityBridge()
        {
            _onGeometryChanged = _ => ScheduleFrameRefresh();
            _onDetachFromPanel = _ => ScheduleResync();
            _onTapMarkerPointerDown = e => AddTapMarker(new Vector2(e.position.x, e.position.y));
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
        /// Registers a named element as a click target. On Android, reports
        /// the element as role=button to UiAutomator2 and routes incoming
        /// "click" actions by name. On iOS, only "*_info_icon" Labels need
        /// special handling — they have no Clickable manipulator, so their
        /// action is dispatched via the bridge's panel-root PointerDown
        /// handler. All other iOS taps ride UI Toolkit's standard Clickable.
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

        // After returnToApp() the test runner's first query can read a stale
        // accessibility snapshot: WDA on iOS (and to a lesser extent
        // UiAutomator2 on Android) caches the tree, and neither
        // BuildHierarchy's "structure changed" check nor the OS's own
        // notifications reliably invalidate that cache when the tree is
        // identical to before backgrounding. Rebuild and broadcast a
        // screen-changed notification unconditionally on every foreground so
        // the next XCUITest/UiAutomator2 query returns fresh data without the
        // Appium side having to spin on a fixed sleep.
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
                // Disabled: purple tap-marker overlay (debug visual for E2E taps).
                // _root.UnregisterCallback(_onTapMarkerPointerDown, TrickleDown.TrickleDown);
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

            // Cheap structural-change check: count named descendants and
            // rebuild on diff. Catches dialog open/close, scene swap, section
            // refresh — every test-relevant mutation creates or removes named
            // VisualElements. Avoids hot event hooks during layout.
            _structurePollTimer += Time.unscaledDeltaTime;
            // Stale-pointer-capture watchdog: see field comment for context.
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
                // Only push to the platform a11y service when something actually
                // changed. Unconditional RefreshNodeFrames + Android sync every
                // 50ms emits a continuous stream of TYPE_WINDOW_CONTENT_CHANGED
                // AccessibilityEvents that prevent UiAutomator2's wait-for-idle
                // from observing 500ms of quiet, stalling every click for the
                // full waitForIdleTimeout (default 10s).
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
                bool valueChanged = node.value != newValue;
                bool activeChanged = node.isActive != newActive;
                if (!valueChanged && !activeChanged)
                    continue;

                node.value = newValue;
                node.isActive = newActive;
                anyChanged = true;
            }
            return anyChanged;
        }

        // Order-sensitive FNV-1a hash over named descendants. A bare count missed
        // dialog→section transitions where the closing dialog and the freshly
        // revealed section exposed the same number of named descendants — the
        // bridge would skip BuildHierarchy and XCUITest would keep reading a
        // stale tree (e.g. add_multiple_tags_button stuck at active=0, rect=0x0)
        // until the next unrelated mutation. Hashing names catches structural
        // identity changes even when the cardinality is unchanged.
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

            // Disabled: purple tap-marker overlay (debug visual for E2E taps).
            // EnsureTapMarkerOverlay();
            UnregisterTreeCallbacks();

            _hierarchy ??= new AccessibilityHierarchy();

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
            // Disabled: purple tap-marker overlay (debug visual for E2E taps).
            // Drops a circular marker at every tap location so the demo
            // overlay matches XCUITest's reported coordinates. Does not
            // dispatch any action; UI Toolkit's hit-test handles the real
            // tap routing.
            // _root.RegisterCallback(_onTapMarkerPointerDown, TrickleDown.TrickleDown);
#if UNITY_IOS && !UNITY_EDITOR
            // Register on the panel's visualTree (the absolute topmost
            // dispatch point), not _root. Anything in TrickleDown above _root
            // — including ScrollView's own pan-capture handlers — could
            // StopPropagation before the event reaches a descendant
            // registration. AtTarget drops on the info Label (observed after
            // iOS Appium injects mobile:scroll mid-test) would then take the
            // tooltip tap with them. See _iosInfoTapByName field comment.
            var visualTree = _root.panel?.visualTree;
            if (visualTree != null)
                visualTree.RegisterCallback(_onIosInfoIconPointerDown, TrickleDown.TrickleDown);
#endif

            // One-shot per root ScrollView taming: BuildHierarchy can run
            // dozens of times per scene (every dialog open/close, every
            // section refresh), but a scene/root swap needs fresh hooks.
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

            // Incremental rebuild: preserve AccessibilityNode identities by
            // name across rebuilds. _hierarchy.Clear() + re-add invalidates
            // every WDA-cached element ref on iOS, producing a burst of
            // "stale element - terminating request" warnings whenever a
            // dialog opens/closes — even when most nodes are unchanged.
            // We diff by name: reuse existing nodes (just re-bind their
            // frameGetter to the current VisualElement instance), add new
            // ones, remove ones whose names disappeared.
            var existingByName = new Dictionary<string, AccessibilityNode>(_map.Count);
            foreach (var kvp in _map)
            {
                var n = kvp.Value;
                if (n != null && !string.IsNullOrEmpty(kvp.Key?.name))
                    existingByName[kvp.Key.name] = n;
            }

            _map.Clear();
            var seenNames = new HashSet<string>();
            bool addedAny = false;
            WalkAndUpsert(_root, existingByName, seenNames, ref addedAny);

            bool removedAny = false;
            foreach (var kvp in existingByName)
            {
                if (seenNames.Contains(kvp.Key))
                    continue;
                try { _hierarchy.RemoveNode(kvp.Value); }
                catch { /* node may already be detached */ }
                removedAny = true;
            }

            _lastStructureSignature = ComputeStructureSignature(_root);

            if (AssistiveSupport.activeHierarchy != _hierarchy)
                AssistiveSupport.activeHierarchy = _hierarchy;
            // SendScreenChanged tells VoiceOver/XCUITest to refresh its
            // element snapshot, which also invalidates cached refs. Skip it
            // when we only re-bound existing nodes — frame/value updates
            // ride out via the normal RefreshNodeFrames path.
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

            if (action == "setValue")
            {
                var field = _root.Q<TextField>(id);
                if (field != null && field.value != value)
                    field.value = value;
                return;
            }

            if (action == "click")
            {
                InvokeAndroidNativeAction(id);
                return;
            }

            if (action == "scroll")
                InvokeAndroidNativeScroll(value);
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

            // BaseBoolField is the common base of Toggle AND RadioButton in
            // UI Toolkit. Q<Toggle> would miss radios entirely, leaving the
            // overlay click as a no-op (clicking the unique/with-value radio
            // wouldn't actually select that outcome type).
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
            _mainSv.scrollOffset = new Vector2(current.x, Mathf.Clamp(nextY, 0f, maxY));

            RefreshNodeValuesAndActive();
            _hierarchy?.RefreshNodeFrames();
            SyncAndroidNativeAccessibility();
        }

        private static string AndroidNativeRole(VisualElement el)
        {
            var role = el switch
            {
                TextField => "input",
                BaseBoolField => "toggle",
                OneSignalDemo.UI.SwitchToggle => "toggle",
                Button => "button",
                _ => AndroidClickTargets.ContainsKey(el) ? "button" : "text",
            };
            return role;
        }
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
                // See settle branch below for rationale. Also ping during the
                // scroll so a getLocation that races with the gesture's tail
                // sees fresh data — the settle ping fires 150ms after the
                // last change, which can land after the test's refetch.
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
            // RefreshNodeFrames updates Unity's a11y node positions, but WDA
            // keeps a cached element-frame snapshot until the OS reports an
            // a11y notification. Without this ping, scrollToEl's refetch
            // reads pre-scroll coordinates and openModal's click() lands on
            // the icon's old off-screen spot — the modal never opens. Firing
            // only on settle (not on every intermediate scroll frame) keeps
            // the notification rate sane and gives WDA one clean signal that
            // frames are stable and re-readable.
            AssistiveSupport.notificationDispatcher?.SendScreenChanged();
        }

        private void EnsureTapMarkerOverlay()
        {
            if (_tapMarkerOverlay?.panel != null)
                return;

            var mainSv = _root.Q<ScrollView>("main_scroll_view");
            var parent = mainSv?.contentContainer ?? _root;

            _tapMarkerOverlay = new VisualElement();
            _tapMarkerOverlay.pickingMode = PickingMode.Ignore;
            _tapMarkerOverlay.style.position = Position.Absolute;
            _tapMarkerOverlay.style.left = 0;
            _tapMarkerOverlay.style.top = 0;
            _tapMarkerOverlay.style.right = 0;
            _tapMarkerOverlay.style.bottom = 0;
            parent.Add(_tapMarkerOverlay);
        }

        private void AddTapMarker(Vector2 position)
        {
            EnsureTapMarkerOverlay();
            var localPosition = _tapMarkerOverlay.WorldToLocal(position);

            var marker = new VisualElement();
            marker.pickingMode = PickingMode.Ignore;
            marker.style.position = Position.Absolute;
            marker.style.width = TapMarkerSize;
            marker.style.height = TapMarkerSize;
            marker.style.left = localPosition.x - TapMarkerSize / 2f;
            marker.style.top = localPosition.y - TapMarkerSize / 2f;
            marker.style.borderTopLeftRadius = TapMarkerSize / 2f;
            marker.style.borderTopRightRadius = TapMarkerSize / 2f;
            marker.style.borderBottomLeftRadius = TapMarkerSize / 2f;
            marker.style.borderBottomRightRadius = TapMarkerSize / 2f;
            marker.style.borderTopWidth = 2;
            marker.style.borderRightWidth = 2;
            marker.style.borderBottomWidth = 2;
            marker.style.borderLeftWidth = 2;
            var markerColor = new Color(0.55f, 0f, 1f, 1f);
            marker.style.borderTopColor = markerColor;
            marker.style.borderRightColor = markerColor;
            marker.style.borderBottomColor = markerColor;
            marker.style.borderLeftColor = markerColor;
            marker.style.backgroundColor = new Color(0.55f, 0f, 1f, 0.18f);

            var label = new Label((++_tapMarkerCount).ToString());
            label.pickingMode = PickingMode.Ignore;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.color = markerColor;
            label.style.fontSize = 10;
            marker.Add(label);

            _tapMarkerOverlay.Add(marker);
            while (_tapMarkerOverlay.childCount > MaxTapMarkers)
                _tapMarkerOverlay.RemoveAt(0);
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
        private void InstallScrollViewE2EHooks(ScrollView mainSv)
        {
            mainSv.RegisterCallback<WheelEvent>(
                e =>
                {
                    e.StopImmediatePropagation();
                },
                TrickleDown.TrickleDown
            );
            // Touch-activity watchdog: any real Down/Move/Up resets the timer.
            // The Update tick clears ScrollView's stuck pan-capture latch if no
            // touch event arrives within StalePointerCaptureWindowMs after a
            // Down — that's the only way the latch can outlive a self-
            // destroying child whose PointerUp is dropped by the dispatcher.
            mainSv.RegisterCallback<PointerDownEvent>(
                e =>
                {
                    _lastTouchActivityMs = Time.realtimeSinceStartupAsDouble * 1000.0;
                },
                TrickleDown.TrickleDown
            );
            mainSv.RegisterCallback<PointerMoveEvent>(
                e =>
                {
                    _lastTouchActivityMs = Time.realtimeSinceStartupAsDouble * 1000.0;
                },
                TrickleDown.TrickleDown
            );
            mainSv.RegisterCallback<PointerUpEvent>(
                e =>
                {
                    _lastTouchActivityMs = -1.0;
                },
                TrickleDown.TrickleDown
            );
            mainSv.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
            mainSv.scrollDecelerationRate = 0f;
            mainSv.elasticity = 0f;
        }

        // Reflection-based reset for ScrollView's stuck pan-capture latches.
        // We deliberately only clear m_PointerCaptureScheduled and m_Pressed —
        // those represent "a touch is currently captured / pending capture",
        // which is the state we need to release when a child detaches mid-
        // touch and PointerUp never arrives. m_TouchPointerMoveAllowed is per-
        // gesture state owned by PointerDown/PointerUp; clearing it here
        // disables touch scrolling for the *next* swipe, since iOS XCUITest
        // sometimes drops PointerUp on the ScrollView and the watchdog ends
        // up running mid-swipe with that flag set true. Leave it alone so the
        // next swipe's PointerDown can manage it normally.
        // Field names match Unity 2022+ UI Toolkit; resolve once and cache.
        private static void ResetScrollViewPanState(ScrollView sv)
        {
            if (sv == null) return;
            if (!_svReflectionResolved)
            {
                var t = typeof(ScrollView);
                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
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

        private void WalkAndUpsert(
            VisualElement el,
            Dictionary<string, AccessibilityNode> existingByName,
            HashSet<string> seenNames,
            ref bool addedAny
        )
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
            if (!string.IsNullOrEmpty(el.name) && !_map.ContainsKey(el))
            {
                var name = el.name;
                // Two distinct VisualElements with the same name (e.g.
                // freshly-rebuilt dialog) — first one wins; only one
                // accessibility node per name to keep ids unambiguous.
                if (seenNames.Add(name))
                {
                    AccessibilityNode node;
                    if (!existingByName.TryGetValue(name, out node))
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
                    // Detach fires the moment a tracked element leaves the
                    // panel — Dismiss(), section refresh, scene swap. Drives
                    // an immediate ScheduleResync so XCUITest can't read a
                    // node whose owner is already gone.
                    el.RegisterCallback(_onDetachFromPanel);
                }
            }

            for (int i = 0; i < el.childCount; i++)
                WalkAndUpsert(el[i], existingByName, seenNames, ref addedAny);
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
        }

        private static string ExtractValue(VisualElement el) => el switch
        {
            TextField tf => tf.value ?? string.Empty,
            BaseBoolField b => b.value ? "1" : "0",
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

            if (IsSectionAnchor(el))
                return true;

            // An element whose worldBound has been clipped away by an ancestor
            // ScrollView/clip container is visually invisible. Other SDKs get
            // this for free because they render through native UIScrollView /
            // android.widget.ScrollView whose accessibilityFrame already
            // excludes clipped children. UI Toolkit reports children's panel
            // coordinates regardless of clip, so we have to intersect here.
            var visible = ComputeVisibleWorldBound(GetFrameElement(el));
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
    }
}
