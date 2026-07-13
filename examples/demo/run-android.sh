#!/bin/sh
# Build the OneSignal Unity demo APK and install it on a running emulator.
#
# Usage:
#   ./run-android.sh [--no-install] [--install-only]
set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

# Auto-detect newest Unity 6000.x install under the Hub, unless UNITY_PATH is set.
find_unity() {
  if [ -n "${UNITY_PATH:-}" ]; then
    echo "$UNITY_PATH"
    return
  fi
  for d in $(ls -1 /Applications/Unity/Hub/Editor 2>/dev/null | sort -rV); do
    BIN="/Applications/Unity/Hub/Editor/$d/Unity.app/Contents/MacOS/Unity"
    [ -x "$BIN" ] && echo "$BIN" && return
  done
}
UNITY="$(find_unity)"

# Resolve adb in this order: $ADB, $ANDROID_HOME/platform-tools, ~/Library/Android/sdk,
# `which adb`, then Unity's bundled Android SDK.
find_adb() {
  if [ -n "${ADB:-}" ] && [ -x "$ADB" ]; then echo "$ADB"; return; fi
  for CANDIDATE in \
    "${ANDROID_HOME:-}/platform-tools/adb" \
    "${ANDROID_SDK_ROOT:-}/platform-tools/adb" \
    "$HOME/Library/Android/sdk/platform-tools/adb"; do
    [ -x "$CANDIDATE" ] && echo "$CANDIDATE" && return
  done
  if command -v adb >/dev/null 2>&1; then command -v adb; return; fi
  if [ -n "$UNITY" ]; then
    UNITY_DIR=$(dirname "$(dirname "$(dirname "$(dirname "$UNITY")")")")
    CANDIDATE="$UNITY_DIR/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb"
    [ -x "$CANDIDATE" ] && echo "$CANDIDATE" && return
  fi
}
ADB="$(find_adb)"

OUTPUT="$SCRIPT_DIR/Build/Android/onesignal-demo.apk"
LOG="$SCRIPT_DIR/Build/build-android.log"
INSTALL=true
SKIP_BUILD=false

for arg in "$@"; do
  case "$arg" in
    --no-install)   INSTALL=false ;;
    --install-only) SKIP_BUILD=true ;;
  esac
done

pick_emulator() {
  [ -z "$ADB" ] && echo "adb not found. Set ADB or ANDROID_HOME to your Android SDK." && exit 1
  LIST=$("$ADB" devices | awk '/emulator-[0-9]+[[:space:]]+device/{print $1}')
  COUNT=$(printf '%s\n' "$LIST" | grep -c . || true)

  [ "$COUNT" -eq 0 ] && echo "No running emulators found." && exit 1
  [ "$COUNT" -eq 1 ] && EMULATOR="$LIST" && return

  echo "Multiple emulators running — pick one:"
  i=1
  printf '%s\n' "$LIST" | while IFS= read -r S; do
    NAME=$("$ADB" -s "$S" emu avd name 2>/dev/null | head -1 | tr -d '\r')
    printf "  [%d] %s  (%s)\n" "$i" "$S" "$NAME"
    i=$((i + 1))
  done
  printf "Choice [1-%d]: " "$COUNT"
  read -r CHOICE
  EMULATOR=$(printf '%s\n' "$LIST" | sed -n "${CHOICE}p")
  [ -z "$EMULATOR" ] && echo "Invalid choice." && exit 1
}

EMULATOR=""
[ "$INSTALL" = true ] && pick_emulator && echo "Target: $EMULATOR" && echo ""

if [ "$SKIP_BUILD" = true ]; then
  [ ! -f "$OUTPUT" ] && echo "No existing build. Run without --install-only first." && exit 1
  echo "Skipping build, using existing $(du -sh "$OUTPUT" | awk '{print $1}') APK"
else
  [ ! -x "$UNITY" ] && echo "Unity not found at $UNITY — set UNITY_PATH" && exit 1
  mkdir -p "$SCRIPT_DIR/Build/Android"
  echo "Building APK (ARM64 / IL2CPP)..."
  echo "Log: $LOG"
  echo ""

  START=$(date +%s)
  "$UNITY" -batchmode -nographics -quit -buildTarget Android \
    -projectPath "$SCRIPT_DIR" -executeMethod BuildScript.BuildAndroidEmulator \
    -logFile "$LOG"
  ELAPSED=$(( $(date +%s) - START ))

  [ ! -f "$OUTPUT" ] && echo "Build failed after $((ELAPSED/60))m $((ELAPSED%60))s. Check $LOG" && exit 1
  echo "Build complete in $((ELAPSED/60))m $((ELAPSED%60))s — $(du -sh "$OUTPUT" | awk '{print $1}')  $OUTPUT"
fi

if [ "$INSTALL" = true ] && [ -n "$EMULATOR" ]; then
  "$ADB" start-server >/dev/null 2>&1
  "$ADB" -s "$EMULATOR" wait-for-device
  echo "Installing on $EMULATOR..."
  "$ADB" -s "$EMULATOR" install -r "$OUTPUT"
  "$ADB" -s "$EMULATOR" shell am start -n com.onesignal.example/com.unity3d.player.UnityPlayerActivity
fi
