#!/bin/sh
# Remove generated Unity/build state so Editor setup can be re-tested from a clean project.
set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

remove_path() {
  path="$1"
  if [ -e "$path" ] || [ -L "$path" ]; then
    printf 'Removing %s\n' "${path#$SCRIPT_DIR/}"
    rm -rf "$path"
  fi
}

remove_path "$SCRIPT_DIR/Assets/OneSignal/Editor/OneSignalAndroidDependencies.xml"
remove_path "$SCRIPT_DIR/Assets/OneSignal/Editor/OneSignalAndroidDependencies.xml.meta"
remove_path "$SCRIPT_DIR/Assets/OneSignal/Editor/OneSignaliOSDependencies.xml"
remove_path "$SCRIPT_DIR/Assets/OneSignal/Editor/OneSignaliOSDependencies.xml.meta"

for path in \
  "$SCRIPT_DIR/.utmp" \
  "$SCRIPT_DIR/.gradle" \
  "$SCRIPT_DIR/Build" \
  "$SCRIPT_DIR/Builds" \
  "$SCRIPT_DIR/Library" \
  "$SCRIPT_DIR/Logs" \
  "$SCRIPT_DIR/MemoryCaptures" \
  "$SCRIPT_DIR/Obj" \
  "$SCRIPT_DIR/Temp" \
  "$SCRIPT_DIR/UserSettings"
do
  remove_path "$path"
done

find "$SCRIPT_DIR" -maxdepth 1 \( \
  -name "*.csproj" -o \
  -name "*.sln" -o \
  -name "*.suo" -o \
  -name "*.tmp" -o \
  -name "*.unityproj" -o \
  -name "*.user" -o \
  -name "*.userprefs" \
\) -exec sh -c 'root="$1"; shift; for path do printf "Removing %s\n" "${path#$root/}"; rm -rf "$path"; done' sh "$SCRIPT_DIR" {} +

printf 'Clean complete. Reopen this project in Unity to regenerate OneSignal dependency manifests.\n'
