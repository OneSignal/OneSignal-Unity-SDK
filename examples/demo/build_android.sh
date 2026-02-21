#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="$SCRIPT_DIR"
OUTPUT_PATH="${1:-$PROJECT_PATH/Build/OneSignalDemo.apk}"

# --- Device selection (before build) ---
SELECTED=""
if command -v adb &>/dev/null; then
    adb start-server 2>/dev/null || true
    sleep 1

    DEVICE_LIST=()
    while IFS= read -r line; do
        dev=$(echo "$line" | awk '{print $1}')
        [ -n "$dev" ] && DEVICE_LIST+=("$dev")
    done < <(adb devices | tail -n +2 | grep -w "device")

    DEVICE_COUNT=${#DEVICE_LIST[@]}

    get_device_name() {
        local name
        name=$(adb -s "$1" emu avd name 2>/dev/null | head -1 | tr -d '\r' | sed 's/_/ /g')
        [ -z "$name" ] && name=$(adb -s "$1" shell getprop ro.product.model 2>/dev/null | tr -d '\r')
        echo "${name:-unknown}"
    }

    if [ "$DEVICE_COUNT" -eq 1 ]; then
        SELECTED="${DEVICE_LIST[0]}"
        echo "Target device: $SELECTED ($(get_device_name "$SELECTED"))"
    elif [ "$DEVICE_COUNT" -gt 1 ]; then
        echo "Available devices:"
        for i in "${!DEVICE_LIST[@]}"; do
            echo "  $((i + 1))) ${DEVICE_LIST[$i]} ($(get_device_name "${DEVICE_LIST[$i]}"))"
        done

        echo ""
        read -rp "Select device [1-$DEVICE_COUNT]: " CHOICE

        if ! [[ "$CHOICE" =~ ^[0-9]+$ ]] || [ "$CHOICE" -lt 1 ] || [ "$CHOICE" -gt "$DEVICE_COUNT" ]; then
            echo "Invalid selection, will build without installing."
        else
            SELECTED="${DEVICE_LIST[$((CHOICE - 1))]}"
            echo "Target device: $SELECTED"
        fi
    else
        echo "No devices connected, will build without installing."
    fi
    echo ""
fi

# --- Unity build ---
UNITY_VERSION=$(grep 'm_EditorVersion:' "$PROJECT_PATH/ProjectSettings/ProjectVersion.txt" | awk '{print $2}')

case "$(uname)" in
    Darwin)
        UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
        ;;
    Linux)
        UNITY_PATH="$HOME/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity"
        ;;
    MINGW*|MSYS*|CYGWIN*)
        UNITY_PATH="C:/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity.exe"
        ;;
esac

if [ ! -f "$UNITY_PATH" ]; then
    echo "Error: Unity $UNITY_VERSION not found at $UNITY_PATH"
    echo "Install it via Unity Hub or set UNITY_PATH env var."
    exit 1
fi

mkdir -p "$(dirname "$OUTPUT_PATH")"

echo "Building Android APK..."
echo "  Unity:   $UNITY_VERSION"
echo "  Project: $PROJECT_PATH"
echo "  Output:  $OUTPUT_PATH"
echo ""

"$UNITY_PATH" \
    -batchmode \
    -nographics \
    -quit \
    -projectPath "$PROJECT_PATH" \
    -executeMethod BuildScript.BuildAndroid \
    -outputPath "$OUTPUT_PATH" \
    -logFile -

GRADLE_APK="$PROJECT_PATH/Library/Bee/Android/Prj/IL2CPP/Gradle/launcher/build/outputs/apk/release/launcher-release.apk"

if [ ! -f "$OUTPUT_PATH" ] && [ -f "$GRADLE_APK" ]; then
    cp "$GRADLE_APK" "$OUTPUT_PATH"
fi

if [ ! -f "$OUTPUT_PATH" ]; then
    echo ""
    echo "Error: APK not found at $OUTPUT_PATH"
    exit 1
fi

echo ""
echo "Build complete: $OUTPUT_PATH"

# --- Install and launch ---
if [ -z "$SELECTED" ]; then
    exit 0
fi

echo ""
echo "Waiting for adb..."
adb start-server 2>/dev/null || true
sleep 3
adb -s "$SELECTED" wait-for-device

echo "Installing on $SELECTED..."
adb -s "$SELECTED" install -r "$OUTPUT_PATH" || {
    echo "Signature mismatch, uninstalling first..."
    adb -s "$SELECTED" uninstall com.onesignal.example || true
    adb -s "$SELECTED" install "$OUTPUT_PATH"
}

echo "Launching app..."
adb -s "$SELECTED" shell am start -n com.onesignal.example/com.unity3d.player.UnityPlayerActivity
