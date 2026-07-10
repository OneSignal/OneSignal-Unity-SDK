#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MANIFEST="$PROJECT_DIR/Packages/manifest.json"
LOCK_FILE="$PROJECT_DIR/Packages/packages-lock.json"
UNITY_PATH="${UNITY_PATH:-/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity}"
BACKUP_DIR="$(mktemp -d)"

restore_packages() {
  cp "$BACKUP_DIR/manifest.json" "$MANIFEST"
  if [[ -f "$BACKUP_DIR/packages-lock.json" ]]; then
    cp "$BACKUP_DIR/packages-lock.json" "$LOCK_FILE"
  else
    rm -f "$LOCK_FILE"
  fi
  rm -rf "$BACKUP_DIR"
}
trap restore_packages EXIT

cp "$MANIFEST" "$BACKUP_DIR/manifest.json"
if [[ -f "$LOCK_FILE" ]]; then
  cp "$LOCK_FILE" "$BACKUP_DIR/packages-lock.json"
fi

python3 - "$MANIFEST" <<'PY'
import json
import sys

path = sys.argv[1]
with open(path, encoding="utf-8") as file:
    manifest = json.load(file)
manifest["dependencies"].pop("com.unity.modules.accessibility", None)
with open(path, "w", encoding="utf-8") as file:
    json.dump(manifest, file, indent=2)
    file.write("\n")
PY
rm -f "$LOCK_FILE"

"$UNITY_PATH" -projectPath "$PROJECT_DIR" "$@"
