#!/bin/sh
# Build the OneSignal Unity demo for iOS simulator.
#
# Usage:
#   ./build_ios.sh [--no-install] [--install-only] [--open]
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

# Unity batchmode fails with misleading errors when these are missing
# (Rosetta: instant death with an unrelated dialog; iOS Build Support:
# 'build target was unsupported' buried in the log), so check up front.
preflight_unity() {
  if [ "$(uname -m)" = "arm64" ]; then
    if [ ! -f /usr/libexec/rosetta/runtime ] && ! pgrep -q oahd 2>/dev/null; then
      echo "Rosetta 2 is not installed — Unity's iOS build tooling requires it on Apple Silicon."
      echo "Install it with: softwareupdate --install-rosetta --agree-to-license"
      exit 1
    fi
  fi

  # $UNITY is .../Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity;
  # the iOS module lives at .../Hub/Editor/<version>/PlaybackEngines/iOSSupport.
  UNITY_ROOT="$(cd "$(dirname "$UNITY")/../../.." && pwd)"
  if [ ! -d "$UNITY_ROOT/PlaybackEngines/iOSSupport" ]; then
    echo "Unity iOS Build Support module not found at $UNITY_ROOT/PlaybackEngines/iOSSupport."
    echo "Install 'iOS Build Support' via Unity Hub for $(basename "$UNITY_ROOT"), then re-run."
    exit 1
  fi
}

# Boot the newest available iPhone when nothing is booted yet.
boot_newest_iphone() {
  AVAIL=$(xcrun simctl list devices available -j \
    | python3 -c "
import json,re,sys
d=json.load(sys.stdin)
rows=[]
for rt,devs in d['devices'].items():
    m=re.search(r'iOS-(\d+)-(\d+)$',rt)
    if not m: continue
    ver=(int(m.group(1)),int(m.group(2)))
    for dev in devs:
        if dev.get('isAvailable') and dev['name'].startswith('iPhone'):
            rows.append((ver,dev['name'],dev['udid']))
rows.sort()
for ver,name,udid in rows:
    print(udid + '|' + name + '|' + '%d.%d' % ver)
" 2>/dev/null || true)

  if [ -z "$AVAIL" ]; then
    echo "No booted simulators and no available iPhone simulators found."
    echo "Install an iOS runtime via Xcode > Settings > Components, or boot a device with: xcrun simctl boot <device>"
    exit 1
  fi

  echo "No booted simulators found. Available iPhones:"
  printf '%s\n' "$AVAIL" | while IFS='|' read -r UDID NAME VER; do
    printf '  %s (iOS %s)\n' "$NAME" "$VER"
  done

  LINE=$(printf '%s\n' "$AVAIL" | tail -1)
  SIM_UDID=$(echo "$LINE" | cut -d'|' -f1)
  SIM_NAME=$(echo "$LINE" | cut -d'|' -f2)
  echo "Booting newest: $SIM_NAME (iOS $(echo "$LINE" | cut -d'|' -f3))..."
  xcrun simctl boot "$SIM_UDID" 2>/dev/null || true
  open -a Simulator
  xcrun simctl bootstatus "$SIM_UDID" -b >/dev/null
}

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

  [ "$COUNT" -eq 0 ] && boot_newest_iphone && return
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
  [ ! -x "$UNITY" ] && echo "Unity not found at $UNITY — set UNITY_PATH" && exit 1
  preflight_unity
  mkdir -p "$XCODE_DIR"
  echo "Generating Xcode project (IL2CPP / Simulator)..."
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

if [ -f "$XCODE_DIR/Podfile" ]; then
  echo "Running pod install..."
  (cd "$XCODE_DIR" && pod install --repo-update)
fi

if [ "$INSTALL" = true ] && [ -n "$SIM_UDID" ]; then
  echo ""
  echo "Building with xcodebuild for simulator..."
  WS="$XCODE_DIR/Unity-iPhone.xcworkspace"
  [ ! -d "$WS" ] && WS=""

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

  APP_PATH=$(find "$DERIVED" -name "*.app" -path "*/Build/Products/*" | head -1)
  [ -z "$APP_PATH" ] && echo "Could not find .app bundle in derived data." && exit 1

  echo "Installing on $SIM_NAME..."
  xcrun simctl install "$SIM_UDID" "$APP_PATH"
  xcrun simctl launch "$SIM_UDID" "$APP_BUNDLE_ID"
fi
