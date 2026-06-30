#!/bin/sh
# Build the OneSignal Unity no-location demo for iOS simulator.
#
# Usage:
#   ./run-ios.sh [--no-install] [--install-only] [--open]
set -eu

# Opt out of the OneSignal location module for this build. The SDK reads this at
# dependency-resolution time and links the granular pods without OneSignalLocation.
export ONESIGNAL_DISABLE_LOCATION=true

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

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

XCODE_DIR="$SCRIPT_DIR/Build/iOS"
LOG="$SCRIPT_DIR/Build/build-ios.log"
SCHEME="Unity-iPhone"
DERIVED="$SCRIPT_DIR/Build/iOS-DerivedData"
APP_BUNDLE_ID="com.onesignal.example"
INSTALL=true
SKIP_BUILD=false
OPEN_XCODE=false

for arg in "$@"; do
  case "$arg" in
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

  echo "Multiple simulators booted - pick one:"
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

# Fallback Podfile generator: EDM4U's iOS resolver normally emits the Podfile
# during the Unity build from the generated per-project dependency manifest. This
# only runs if that didn't happen, so the no-location build still works offline.
ensure_podfile() {
  [ -f "$XCODE_DIR/Podfile" ] && return

  DEPS="$SCRIPT_DIR/Assets/OneSignal/Editor/OneSignaliOSDependencies.xml"
  [ ! -f "$DEPS" ] && return

  python3 - "$DEPS" "$XCODE_DIR/Podfile" <<'PY'
import os
import sys
import xml.etree.ElementTree as ET

deps_path, podfile_path = sys.argv[1:3]
root = ET.parse(deps_path).getroot()
pods = []

for pod in root.findall("./iosPods/iosPod"):
    name = pod.get("name")
    version = pod.get("version")
    if name and version:
        pods.append((name, version))

if not pods:
    sys.exit(0)

onesignal_version = next(
    (version for name, version in pods if name.startswith("OneSignalXCFramework")),
    None,
)

lines = [
    "source 'https://cdn.cocoapods.org/'",
    "install! 'cocoapods', :disable_input_output_paths => true",
    "platform :ios, '13.0'",
    "use_frameworks! :linkage => :static",
    "",
]

for target in ("UnityFramework", "Unity-iPhone"):
    lines.append(f"target '{target}' do")
    for name, version in pods:
        lines.append(f"  pod '{name}', '{version}'")
    lines.extend(["end", ""])

if onesignal_version:
    lines.extend([
        "target 'OneSignalNotificationServiceExtension' do",
        f"  pod 'OneSignalXCFramework/OneSignalExtension', '{onesignal_version}'",
        "end",
        "",
    ])

os.makedirs(os.path.dirname(podfile_path), exist_ok=True)
with open(podfile_path, "w", encoding="utf-8") as podfile:
    podfile.write("\n".join(lines))
PY
}

SIM_UDID=""
SIM_NAME=""
[ "$INSTALL" = true ] && pick_simulator && echo "Target: $SIM_NAME ($SIM_UDID)" && echo ""

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
  [ ! -x "$UNITY" ] && echo "Unity not found at $UNITY - set UNITY_PATH" && exit 1
  mkdir -p "$XCODE_DIR"
  echo "Generating no-location Xcode project (IL2CPP / Simulator)..."
  echo "Log: $LOG"
  echo ""

  START=$(date +%s)
  "$UNITY" -batchmode -nographics -quit -buildTarget iOS \
    -projectPath "$SCRIPT_DIR" -executeMethod BuildScript.BuildiOSSimulator \
    -logFile "$LOG"
  ELAPSED=$(( $(date +%s) - START ))

  [ ! -d "$XCODE_DIR/Unity-iPhone.xcodeproj" ] && echo "Build failed after $((ELAPSED/60))m $((ELAPSED%60))s. Check $LOG" && exit 1
  echo "Xcode project generated in $((ELAPSED/60))m $((ELAPSED%60))s"
fi

ensure_podfile

if [ -f "$XCODE_DIR/Podfile" ]; then
  echo "Running pod install..."
  (cd "$XCODE_DIR" && pod install --repo-update)
fi

if [ "$INSTALL" = true ] && [ -n "$SIM_UDID" ]; then
  echo ""
  echo "Building with xcodebuild for simulator..."
  WS="$XCODE_DIR/Unity-iPhone.xcworkspace"
  [ ! -d "$WS" ] && WS=""

  # macOS ships openrsync as /usr/bin/rsync, which fails the CocoaPods "Copy
  # XCFrameworks" phase ("renameat: No such file or directory" on the dSYM
  # DWARF). Prefer a real rsync (brew install rsync) on PATH if present.
  if [ -x /opt/homebrew/bin/rsync ]; then
    PATH="/opt/homebrew/bin:$PATH"
    export PATH
  elif [ -x /usr/local/bin/rsync ]; then
    PATH="/usr/local/bin:$PATH"
    export PATH
  else
    echo "Warning: only macOS openrsync found; the XCFramework copy phase may fail."
    echo "         Install a real rsync with: brew install rsync"
  fi

  BUILD_START=$(date +%s)
  if [ -n "$WS" ]; then
    xcodebuild -workspace "$WS" -scheme "$SCHEME" \
      -destination "id=$SIM_UDID" \
      -derivedDataPath "$DERIVED" \
      -quiet \
      build
  else
    xcodebuild -project "$XCODE_DIR/Unity-iPhone.xcodeproj" -scheme "$SCHEME" \
      -destination "id=$SIM_UDID" \
      -derivedDataPath "$DERIVED" \
      -quiet \
      build
  fi
  BUILD_ELAPSED=$(( $(date +%s) - BUILD_START ))
  echo "xcodebuild finished in $((BUILD_ELAPSED/60))m $((BUILD_ELAPSED%60))s"

  APP_PATH=$(find "$DERIVED" -name "*.app" -path "*/Build/Products/*" | sed -n '1p')
  [ -z "$APP_PATH" ] && echo "Could not find .app bundle in derived data." && exit 1

  echo "Installing on $SIM_NAME..."
  xcrun simctl install "$SIM_UDID" "$APP_PATH"
  xcrun simctl launch "$SIM_UDID" "$APP_BUNDLE_ID"
fi
