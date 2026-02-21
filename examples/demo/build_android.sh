#!/usr/bin/env bash
# Build the OneSignal Unity demo as an APK for Android emulator (x86_64 / Mono).
# Usage:
#   ./build_android.sh [--install]   # --install deploys to the first running emulator
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
UNITY="${UNITY_PATH:-/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity}"
PROJECT_SETTINGS="$SCRIPT_DIR/ProjectSettings/ProjectSettings.asset"
OUTPUT_APK="$SCRIPT_DIR/Build/Android/onesignal-demo.apk"
LOG_FILE="$SCRIPT_DIR/Build/build.log"
INSTALL=false

# AndroidArchitecture flags: ARMv7=1, X86=2, ARM64=4, X86_64=8
ARCH_EMULATOR=8    # X86_64 only — fastest emulator build
ARCH_ORIGINAL=""

for arg in "$@"; do
  [[ "$arg" == "--install" ]] && INSTALL=true
done

if [[ ! -x "$UNITY" ]]; then
  echo "Unity not found at: $UNITY"
  echo "Set UNITY_PATH to the correct Unity binary, e.g.:"
  echo "  UNITY_PATH=/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity ./build_android.sh"
  exit 1
fi

mkdir -p "$SCRIPT_DIR/Build/Android"

# --- Patch ProjectSettings to use X86_64 for fast emulator build ---
ARCH_ORIGINAL=$(grep "AndroidTargetArchitectures:" "$PROJECT_SETTINGS" | awk '{print $2}')
sed -i '' "s/AndroidTargetArchitectures: ${ARCH_ORIGINAL}/AndroidTargetArchitectures: ${ARCH_EMULATOR}/" "$PROJECT_SETTINGS"

restore_settings() {
  sed -i '' "s/AndroidTargetArchitectures: ${ARCH_EMULATOR}/AndroidTargetArchitectures: ${ARCH_ORIGINAL}/" "$PROJECT_SETTINGS"
}
trap restore_settings EXIT

echo "Building Android APK (emulator / x86_64 / Mono)..."
echo "Log: $LOG_FILE"
echo ""

"$UNITY" \
  -batchmode \
  -quit \
  -buildTarget Android \
  -projectPath "$SCRIPT_DIR" \
  -executeMethod BuildScript.BuildAndroidEmulator \
  -logFile "$LOG_FILE"

BUILD_EXIT=$?

if [[ $BUILD_EXIT -ne 0 ]] || [[ ! -f "$OUTPUT_APK" ]]; then
  echo "Build failed (exit $BUILD_EXIT). Check $LOG_FILE for details."
  exit 1
fi

echo "APK: $OUTPUT_APK"

if [[ "$INSTALL" == true ]]; then
  EMULATOR=$(adb devices | awk '/emulator-[0-9]+\s+device/{print $1; exit}')
  if [[ -z "$EMULATOR" ]]; then
    echo "No running emulator found. Start one and re-run with --install."
    exit 1
  fi
  echo "Installing on $EMULATOR..."
  adb -s "$EMULATOR" install -r "$OUTPUT_APK"
  echo "Done. Launch with:"
  echo "  adb -s $EMULATOR shell am start -n com.onesignal.example/.UnityPlayerActivity"
fi
