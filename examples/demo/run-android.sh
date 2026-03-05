#!/bin/sh
# Build the OneSignal Unity demo APK and install it on a running emulator.
#
# Usage:
#   ./build_android.sh [--no-install] [--install-only]
set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
UNITY="${UNITY_PATH:-/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity}"
ADB="/Applications/Unity/Hub/Editor/6000.3.6f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb"
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
