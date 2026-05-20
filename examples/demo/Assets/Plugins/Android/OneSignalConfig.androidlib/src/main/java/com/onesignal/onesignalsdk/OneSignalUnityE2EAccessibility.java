package com.onesignal.onesignalsdk;

import android.app.Activity;
import android.graphics.Color;
import android.text.Editable;
import android.text.InputType;
import android.text.TextWatcher;
import android.util.Log;
import android.view.Gravity;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.view.ViewConfiguration;
import android.view.accessibility.AccessibilityNodeInfo;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.FrameLayout;
import android.widget.TextView;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;

public final class OneSignalUnityE2EAccessibility {
  private static final String UNITY_OBJECT = "OneSignalAccessibilityBridge";
  private static final String TAG = "OneSignalUnityE2E";
  private static final Map<String, Entry> entries = new HashMap<>();
  private static FrameLayout overlay;
  private static int generation;
  private static boolean loggedOverlayReady;

  private OneSignalUnityE2EAccessibility() {}

  public static void beginSync() {
    Activity activity = getActivity();
    if (activity == null) return;
    activity.runOnUiThread(
        () -> {
          ensureOverlay(activity);
          generation++;
        });
  }

  public static void syncElement(
      String id,
      String text,
      boolean active,
      int x,
      int y,
      int width,
      int height,
      String role,
      boolean enabled) {
    Activity activity = getActivity();
    if (activity == null || id == null || id.length() == 0) return;
    activity.runOnUiThread(
        () -> {
          ensureOverlay(activity);
          Entry entry = entries.get(id);
          if (entry == null || !entry.role.equals(role)) {
            if (entry != null) overlay.removeView(entry.view);
            entry = new Entry(createView(activity, id, role), role);
            entries.put(id, entry);
            overlay.addView(entry.view);
          }

          entry.generation = generation;
          entry.view.setVisibility(active ? View.VISIBLE : View.GONE);
          entry.view.setEnabled(enabled);
          entry.view.setContentDescription(text == null ? "" : text);
          applyText(entry.view, text == null ? "" : text);

          FrameLayout.LayoutParams params =
              new FrameLayout.LayoutParams(Math.max(1, width), Math.max(1, height));
          params.leftMargin = x;
          params.topMargin = y;
          entry.view.setLayoutParams(params);
        });
  }

  public static void endSync() {
    Activity activity = getActivity();
    if (activity == null) return;
    activity.runOnUiThread(
        () -> {
          if (overlay == null) return;
          Iterator<Map.Entry<String, Entry>> it = entries.entrySet().iterator();
          while (it.hasNext()) {
            Entry entry = it.next().getValue();
            if (entry.generation == generation) continue;
            overlay.removeView(entry.view);
            it.remove();
          }
        });
  }

  private static void ensureOverlay(Activity activity) {
    if (overlay != null) return;
    overlay = new E2EOverlay(activity);
    overlay.setClipChildren(false);
    overlay.setClipToPadding(false);
    overlay.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO);
    activity.addContentView(
        overlay,
        new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT));
    if (!loggedOverlayReady) {
      loggedOverlayReady = true;
      Log.d(TAG, "Native accessibility overlay ready");
    }
  }

  private static View createView(Activity activity, String id, String role) {
    TextView view;
    if ("input".equals(role)) {
      E2EEditText input = new E2EEditText(activity, id);
      // // Hide the blinking caret; the Unity-drawn TextField renders its own.
      // input.setCursorVisible(false);
      // Suppress the spell-checker's red underline span. The Unity TextField
      // is the user-visible input; the overlay only needs to forward
      // characters back to Unity via the TextWatcher.
      input.setInputType(
          InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS);
      input.addTextChangedListener(
          new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {}

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
              if (!input.isApplyingUnityValue()) sendToUnity(id, "setValue", s.toString());
            }

            @Override
            public void afterTextChanged(Editable s) {}
          });
      view = input;
    } else if ("toggle".equals(role)) {
      E2ECheckBox toggle = new E2ECheckBox(activity, id);
      // Hide the default square check indicator; the Unity-drawn pill switch
      // is the visual. CheckBox class, isChecked() state, viewIdResourceName,
      // and click handling are preserved for Appium/UiAutomator2.
      toggle.setButtonDrawable(null);
      toggle.setOnClickListener(v -> sendToUnity(id, "click", ""));
      view = toggle;
    } else {
      view = new E2ETextView(activity, id);
      if ("button".equals(role)) {
        view.setClickable(true);
        view.setOnClickListener(v -> sendToUnity(id, "click", ""));
      } else if ("main_scroll_view".equals(id)) {
        // Claim ACTION_DOWN on the scrollable's empty regions so the parent
        // E2EOverlay sees subsequent ACTION_MOVE events and can intercept
        // vertical drags. Without this, swipes on bare space fall straight
        // through the overlay and Appium scrollIntoView never advances.
        view.setClickable(true);
      }
    }

    view.setGravity(Gravity.CENTER);
    view.setBackgroundColor(Color.TRANSPARENT);
    view.setTextColor(Color.TRANSPARENT);
    view.setHintTextColor(Color.TRANSPARENT);
    view.setIncludeFontPadding(false);
    view.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_YES);
    return view;
  }

  private static void applyText(View view, String text) {
    if (view instanceof E2ECheckBox) {
      ((E2ECheckBox) view).setChecked("1".equals(text) || "true".equalsIgnoreCase(text));
      ((E2ECheckBox) view).setText(text);
      return;
    }

    if (view instanceof E2EEditText) {
      E2EEditText input = (E2EEditText) view;
      if (!text.contentEquals(input.getText())) input.setUnityText(text);
      return;
    }

    if (view instanceof TextView) ((TextView) view).setText(text);
  }

  private static void sendToUnity(String id, String action, String value) {
    try {
      Class<?> unityPlayer = Class.forName("com.unity3d.player.UnityPlayer");
      Method sendMessage =
          unityPlayer.getMethod("UnitySendMessage", String.class, String.class, String.class);
      sendMessage.invoke(
          null, UNITY_OBJECT, "HandleAndroidAccessibilityAction", id + "\n" + action + "\n" + value);
    } catch (Exception ignored) {
    }
  }

  private static Activity getActivity() {
    try {
      Class<?> unityPlayer = Class.forName("com.unity3d.player.UnityPlayer");
      Field currentActivity = unityPlayer.getField("currentActivity");
      Object activity = currentActivity.get(null);
      return activity instanceof Activity ? (Activity) activity : null;
    } catch (Exception ignored) {
      return null;
    }
  }

  private static final class Entry {
    final View view;
    final String role;
    int generation;

    Entry(View view, String role) {
      this.view = view;
      this.role = role;
    }
  }

  private static final class E2EOverlay extends FrameLayout {
    private final int touchSlop;
    // Once a swipe is detected we hijack the gesture: the originally-targeted
    // child (often a button TextView) receives ACTION_CANCEL and every later
    // ACTION_MOVE/UP routes to this overlay's onTouchEvent.
    private boolean interceptingScroll;
    private float startX;
    private float startY;

    E2EOverlay(Activity activity) {
      super(activity);
      touchSlop = ViewConfiguration.get(activity).getScaledTouchSlop();
    }

    @Override
    public boolean onInterceptTouchEvent(MotionEvent ev) {
      // Unity renders into a SurfaceView, so UI Toolkit's ScrollView never
      // sees touches that an overlay accessibility child consumed first.
      // Appium's `scrollIntoView` swipes through the horizontal centre of
      // `main_scroll_view`, often starting on top of a clickable overlay
      // TextView (button) — that child eats ACTION_DOWN and the rest of the
      // gesture, so the ScrollView never scrolls. Catch any vertical drag
      // here and convert it to a real ScrollView offset change. Plain taps
      // stay under touchSlop and pass through unaffected.
      switch (ev.getActionMasked()) {
        case MotionEvent.ACTION_DOWN:
          startX = ev.getX();
          startY = ev.getY();
          interceptingScroll = false;
          return false;
        case MotionEvent.ACTION_MOVE:
          if (interceptingScroll) return true;
          float dx = ev.getX() - startX;
          float dy = ev.getY() - startY;
          if (Math.abs(dy) > touchSlop && Math.abs(dy) > Math.abs(dx)) {
            interceptingScroll = true;
            return true;
          }
          return false;
        default:
          return false;
      }
    }

    @Override
    public boolean onTouchEvent(MotionEvent ev) {
      if (!interceptingScroll) return false;
      switch (ev.getActionMasked()) {
        case MotionEvent.ACTION_UP:
        case MotionEvent.ACTION_CANCEL:
          float deltaY = ev.getY() - startY;
          interceptingScroll = false;
          if (Math.abs(deltaY) > touchSlop) {
            // Positive value = scroll forward (reveal content below),
            // matching a finger that travels upward on screen. Unity converts
            // screen pixels to UI Toolkit panel units before mutating
            // scrollOffset.
            sendToUnity(
                "main_scroll_view", "scrollDelta", String.valueOf(Math.round(-deltaY)));
          }
          return true;
        default:
          return true;
      }
    }
  }

  private static class E2ETextView extends TextView {
    private final String id;

    E2ETextView(Activity activity, String id) {
      super(activity);
      this.id = id;
    }

    @Override
    public void onInitializeAccessibilityNodeInfo(AccessibilityNodeInfo info) {
      super.onInitializeAccessibilityNodeInfo(info);
      info.setViewIdResourceName(id);
    }
  }

  private static final class E2EEditText extends EditText {
    private final String id;
    private boolean applyingUnityValue;

    E2EEditText(Activity activity, String id) {
      super(activity);
      this.id = id;
      setSingleLine(false);
    }

    boolean isApplyingUnityValue() {
      return applyingUnityValue;
    }

    void setUnityText(String text) {
      applyingUnityValue = true;
      setText(text);
      setSelection(getText().length());
      applyingUnityValue = false;
    }

    @Override
    public void onInitializeAccessibilityNodeInfo(AccessibilityNodeInfo info) {
      super.onInitializeAccessibilityNodeInfo(info);
      info.setViewIdResourceName(id);
    }
  }

  private static final class E2ECheckBox extends CheckBox {
    private final String id;

    E2ECheckBox(Activity activity, String id) {
      super(activity);
      this.id = id;
    }

    @Override
    public void onInitializeAccessibilityNodeInfo(AccessibilityNodeInfo info) {
      super.onInitializeAccessibilityNodeInfo(info);
      info.setViewIdResourceName(id);
    }
  }
}
