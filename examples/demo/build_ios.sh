#!/bin/sh
# Build the OneSignal Unity demo for iOS simulator or device.
#
# Usage:
#   ./build_ios.sh [--device] [--no-install] [--install-only] [--open]
#
# By default targets the iOS Simulator. Pass --device to build for a
# connected physical device instead.
set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
UNITY="${UNITY_PATH:-/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity}"
XCODE_DIR="$SCRIPT_DIR/Build/iOS"
LOG="$SCRIPT_DIR/Build/build.log"
SCHEME="Unity-iPhone"
DERIVED="$SCRIPT_DIR/Build/iOS-DerivedData"
APP_BUNDLE_ID="com.onesignal.example"
INSTALL=true
SKIP_BUILD=false
OPEN_XCODE=false
DEVICE_BUILD=false

for arg in "$@"; do
  case "$arg" in
    --device)       DEVICE_BUILD=true ;;
    --no-install)   INSTALL=false ;;
    --install-only) SKIP_BUILD=true ;;
    --open)         OPEN_XCODE=true; INSTALL=false; SKIP_BUILD=true ;;
  esac
done

pick_simulator() {
  LIST=$(xcrun simctl list devices booted -j \
    | python3 -c "
import json,sys
d=json.load(sys.stdin)
for r,devs in d['devices'].items():
    for dev in devs:
        if dev['state']=='Booted':
            print(dev['udid'] + '|' + dev['name'])
" 2>/dev/null || true)
  COUNT=$(printf '%s\n' "$LIST" | grep -c . || true)

  [ "$COUNT" -eq 0 ] && echo "No booted simulators found. Boot one with: xcrun simctl boot <device>" && exit 1
  [ "$COUNT" -eq 1 ] && SIM_UDID=$(echo "$LIST" | cut -d'|' -f1) && SIM_NAME=$(echo "$LIST" | cut -d'|' -f2) && return

  echo "Multiple simulators booted — pick one:"
  i=1
  printf '%s\n' "$LIST" | while IFS='|' read -r UDID NAME; do
    printf "  [%d] %s  (%s)\n" "$i" "$NAME" "$UDID"
    i=$((i + 1))
  done
  printf "Choice [1-%d]: " "$COUNT"
  read -r CHOICE
  LINE=$(printf '%s\n' "$LIST" | sed -n "${CHOICE}p")
  [ -z "$LINE" ] && echo "Invalid choice." && exit 1
  SIM_UDID=$(echo "$LINE" | cut -d'|' -f1)
  SIM_NAME=$(echo "$LINE" | cut -d'|' -f2)
}

pick_device() {
  LIST=$(python3 -c "
import subprocess,json,sys,tempfile,os
tmp = tempfile.mktemp(suffix='.json')
subprocess.run(['xcrun','devicectl','list','devices','--json-output',tmp],
               capture_output=True, text=True)
try:
    with open(tmp) as f: data = json.load(f)
finally:
    try: os.unlink(tmp)
    except: pass
for d in data.get('result',{}).get('devices',[]):
    identifier = d.get('identifier','')
    name = d.get('deviceProperties',{}).get('name','')
    platform = d.get('hardwareProperties',{}).get('platform','')
    if identifier and name and platform in ('iOS','iPadOS'):
        print(identifier + '|' + name)
" 2>/dev/null || true)
  COUNT=$(printf '%s\n' "$LIST" | grep -c . || true)

  [ "$COUNT" -eq 0 ] && echo "No connected devices found. Plug in a device or check Xcode." && exit 1
  [ "$COUNT" -eq 1 ] && DEV_UDID=$(echo "$LIST" | cut -d'|' -f1) && DEV_NAME=$(echo "$LIST" | cut -d'|' -f2) && return

  echo "Multiple devices found — pick one:"
  i=1
  printf '%s\n' "$LIST" | while IFS='|' read -r UDID NAME; do
    printf "  [%d] %s  (%s)\n" "$i" "$NAME" "$UDID"
    i=$((i + 1))
  done
  printf "Choice [1-%d]: " "$COUNT"
  read -r CHOICE
  LINE=$(printf '%s\n' "$LIST" | sed -n "${CHOICE}p")
  [ -z "$LINE" ] && echo "Invalid choice." && exit 1
  DEV_UDID=$(echo "$LINE" | cut -d'|' -f1)
  DEV_NAME=$(echo "$LINE" | cut -d'|' -f2)
}

TARGET_UDID=""
TARGET_NAME=""

if [ "$INSTALL" = true ]; then
  if [ "$DEVICE_BUILD" = true ]; then
    DEV_UDID=""
    DEV_NAME=""
    pick_device
    TARGET_UDID="$DEV_UDID"
    TARGET_NAME="$DEV_NAME"
  else
    SIM_UDID=""
    SIM_NAME=""
    pick_simulator
    TARGET_UDID="$SIM_UDID"
    TARGET_NAME="$SIM_NAME"
  fi
  echo "Target: $TARGET_NAME ($TARGET_UDID)"
  echo ""
fi

if [ "$OPEN_XCODE" = true ]; then
  WS="$XCODE_DIR/Unity-iPhone.xcworkspace"
  [ ! -d "$WS" ] && echo "No workspace found. Run without --open first." && exit 1
  echo "Opening $WS..."
  open "$WS"
  exit 0
fi

if [ "$SKIP_BUILD" = true ]; then
  [ ! -d "$XCODE_DIR/Unity-iPhone.xcodeproj" ] && echo "No existing build. Run without --install-only first." && exit 1
  echo "Skipping Unity build, using existing Xcode project"
else
  [ ! -x "$UNITY" ] && echo "Unity not found at $UNITY — set UNITY_PATH" && exit 1
  mkdir -p "$XCODE_DIR"

  if [ "$DEVICE_BUILD" = true ]; then
    BUILD_METHOD="BuildScript.BuildiOSDevice"
    echo "Generating Xcode project (IL2CPP / Device)..."
  else
    BUILD_METHOD="BuildScript.BuildiOSSimulator"
    echo "Generating Xcode project (IL2CPP / Simulator)..."
  fi
  echo "Log: $LOG"
  echo ""

  START=$(date +%s)
  "$UNITY" -batchmode -nographics -quit -buildTarget iOS \
    -projectPath "$SCRIPT_DIR" -executeMethod "$BUILD_METHOD" \
    -logFile "$LOG"
  ELAPSED=$(( $(date +%s) - START ))

  [ ! -d "$XCODE_DIR/Unity-iPhone.xcodeproj" ] && echo "Build failed after $((ELAPSED/60))m $((ELAPSED%60))s. Check $LOG" && exit 1
  echo "Xcode project generated in $((ELAPSED/60))m $((ELAPSED%60))s"
fi

if [ -f "$XCODE_DIR/Podfile" ]; then
  echo "Running pod install..."
  (cd "$XCODE_DIR" && pod install --repo-update)
fi

if [ "$INSTALL" = true ] && [ -n "$TARGET_UDID" ]; then
  echo ""
  echo "Building with xcodebuild..."
  WS="$XCODE_DIR/Unity-iPhone.xcworkspace"
  [ ! -d "$WS" ] && WS=""

  if [ "$DEVICE_BUILD" = true ]; then
    DESTINATION="generic/platform=iOS"
  else
    DESTINATION="id=$TARGET_UDID"
  fi

  BUILD_START=$(date +%s)
  if [ -n "$WS" ]; then
    xcodebuild -workspace "$WS" -scheme "$SCHEME" \
      -destination "$DESTINATION" \
      -derivedDataPath "$DERIVED" \
      -quiet \
      build
  else
    xcodebuild -project "$XCODE_DIR/Unity-iPhone.xcodeproj" -scheme "$SCHEME" \
      -destination "$DESTINATION" \
      -derivedDataPath "$DERIVED" \
      -quiet \
      build
  fi
  BUILD_ELAPSED=$(( $(date +%s) - BUILD_START ))
  echo "xcodebuild finished in $((BUILD_ELAPSED/60))m $((BUILD_ELAPSED%60))s"

  APP_PATH=$(find "$DERIVED" -name "*.app" -path "*/Build/Products/*" | head -1)
  [ -z "$APP_PATH" ] && echo "Could not find .app bundle in derived data." && exit 1

  if [ "$DEVICE_BUILD" = true ]; then
    echo "Installing on $TARGET_NAME..."
    xcrun devicectl device install app --device "$TARGET_UDID" "$APP_PATH"
    xcrun devicectl device process launch --device "$TARGET_UDID" "$APP_BUNDLE_ID"
  else
    echo "Installing on $TARGET_NAME..."
    xcrun simctl install "$TARGET_UDID" "$APP_PATH"
    xcrun simctl launch "$TARGET_UDID" "$APP_BUNDLE_ID"
  fi
fi
