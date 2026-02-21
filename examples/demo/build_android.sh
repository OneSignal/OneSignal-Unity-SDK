#!/bin/sh
# Build the OneSignal Unity demo APK for Android emulators and install it.
# Uses ARM64 + IL2CPP (Unity 6 dropped Mono support for 64-bit architectures).
#
# Usage:
#   ./build_android.sh [--no-install]
set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
UNITY="${UNITY_PATH:-/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity}"
OUTPUT_APK="$SCRIPT_DIR/Build/Android/onesignal-demo.apk"
LOG_FILE="$SCRIPT_DIR/Build/build.log"
INSTALL=true
EMULATOR=""

for arg in "$@"; do
  [ "$arg" = "--no-install" ] && INSTALL=false
done

if [ ! -x "$UNITY" ]; then
  echo "Unity not found at: $UNITY"
  echo "Set UNITY_PATH, e.g.:"
  echo "  UNITY_PATH=/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity ./build_android.sh"
  exit 1
fi

# --- Select emulator before building ---
if [ "$INSTALL" = true ]; then
  EMULATOR_LIST=$(adb devices | awk '/emulator-[0-9]+[[:space:]]+device/{print $1}')
  EMULATOR_COUNT=$(echo "$EMULATOR_LIST" | grep -c . || true)

  if [ "$EMULATOR_COUNT" -eq 0 ]; then
    echo "No running emulators found. Start one and re-run."
    exit 1
  fi

  if [ "$EMULATOR_COUNT" -eq 1 ]; then
    EMULATOR="$EMULATOR_LIST"
  else
    echo "Multiple emulators running — pick one:"
    i=1
    OLDIFS="$IFS"
    IFS='
'
    for SERIAL in $EMULATOR_LIST; do
      NAME=$(adb -s "$SERIAL" emu avd name 2>/dev/null | head -1 | tr -d '\r')
      printf "  [%d] %s  (%s)\n" "$i" "$SERIAL" "$NAME"
      i=$((i + 1))
    done
    IFS="$OLDIFS"
    printf "Choice [1-%d]: " "$EMULATOR_COUNT"
    read -r CHOICE
    case "$CHOICE" in
      ''|*[!0-9]*) echo "Invalid choice."; exit 1 ;;
    esac
    if [ "$CHOICE" -lt 1 ] || [ "$CHOICE" -gt "$EMULATOR_COUNT" ]; then
      echo "Invalid choice."
      exit 1
    fi
    EMULATOR=$(echo "$EMULATOR_LIST" | sed -n "${CHOICE}p")
  fi
  echo "Target: $EMULATOR"
  echo ""
fi

# --- Build ---
mkdir -p "$SCRIPT_DIR/Build/Android"

echo "Building Android APK (ARM64 / IL2CPP)..."
echo "Log: $LOG_FILE"
echo ""

BUILD_START=$(date +%s)

"$UNITY" -batchmode -quit -buildTarget Android -projectPath "$SCRIPT_DIR" -executeMethod BuildScript.BuildAndroidEmulator -logFile "$LOG_FILE"

BUILD_ELAPSED=$(( $(date +%s) - BUILD_START ))
BUILD_MIN=$(( BUILD_ELAPSED / 60 ))
BUILD_SEC=$(( BUILD_ELAPSED % 60 ))

if [ ! -f "$OUTPUT_APK" ]; then
  echo "Build failed after ${BUILD_MIN}m ${BUILD_SEC}s. Check $LOG_FILE for details."
  exit 1
fi

APK_SIZE=$(du -sh "$OUTPUT_APK" | awk '{print $1}')
echo "Build complete in ${BUILD_MIN}m ${BUILD_SEC}s — ${APK_SIZE}  $OUTPUT_APK"

# --- Install & launch ---
if [ "$INSTALL" = true ] && [ -n "$EMULATOR" ]; then
  # Unity kills the ADB server during builds; wait for it to reconnect.
  adb start-server >/dev/null 2>&1
  adb -s "$EMULATOR" wait-for-device
  echo "Installing on $EMULATOR..."
  adb -s "$EMULATOR" install -r "$OUTPUT_APK"
  adb -s "$EMULATOR" shell am start -n com.onesignal.example/com.unity3d.player.UnityPlayerActivity
fi
