#!/bin/sh
# Downloads the OneSignal padded logo and generates Android launcher icons
# at all mipmap densities, stored in an androidlib that Unity merges into
# the Gradle build to replace the default icon.
#
# Requires: curl, sips (macOS), python3
set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ICON_URL="https://raw.githubusercontent.com/OneSignal/sdk-shared/refs/heads/main/assets/onesignal_logo_icon_padded.png"
TMP_ICON="$(mktemp).png"
LIB_DIR="$SCRIPT_DIR/Assets/Plugins/Android/AppIcon.androidlib"
RES_DIR="$LIB_DIR/src/main/res"

echo "Downloading source icon..."
curl -fsSL -o "$TMP_ICON" "$ICON_URL"

for density in mdpi hdpi xhdpi xxhdpi xxxhdpi; do
  mkdir -p "$RES_DIR/mipmap-$density"
done
mkdir -p "$RES_DIR/mipmap-anydpi-v26"

echo "Generating icons..."

for pair in "mdpi 48" "hdpi 72" "xhdpi 96" "xxhdpi 144" "xxxhdpi 192"; do
  density="${pair%% *}"
  size="${pair##* }"
  sips -z "$size" "$size" "$TMP_ICON" --out "$RES_DIR/mipmap-$density/app_icon.png" >/dev/null 2>&1
done

TMP_RESIZED="$(mktemp).png"
for entry in "mdpi 108 66" "hdpi 162 99" "xhdpi 216 132" "xxhdpi 324 198" "xxxhdpi 432 264"; do
  set -- $entry
  density="$1"; canvas="$2"; safe="$3"
  sips -z "$safe" "$safe" "$TMP_ICON" --out "$TMP_RESIZED" >/dev/null 2>&1
  sips -p "$canvas" "$canvas" "$TMP_RESIZED" --out "$RES_DIR/mipmap-$density/ic_launcher_foreground.png" >/dev/null 2>&1
done
rm -f "$TMP_RESIZED"

python3 -c "
import struct, zlib, sys, os

def solid_png(size, r, g, b, path):
    ihdr = struct.pack('>IIBBBBB', size, size, 8, 2, 0, 0, 0)
    pixel = bytes([r, g, b])
    raw = b''.join(b'\x00' + pixel * size for _ in range(size))
    idat = zlib.compress(raw, 9)
    def chunk(t, d):
        c = t + d
        return struct.pack('>I', len(d)) + c + struct.pack('>I', zlib.crc32(c) & 0xFFFFFFFF)
    with open(path, 'wb') as f:
        f.write(b'\x89PNG\r\n\x1a\n')
        f.write(chunk(b'IHDR', ihdr))
        f.write(chunk(b'IDAT', idat))
        f.write(chunk(b'IEND', b''))

res = sys.argv[1]
for density, size in [('mdpi',108),('hdpi',162),('xhdpi',216),('xxhdpi',324),('xxxhdpi',432)]:
    solid_png(size, 0xFF, 0xFF, 0xFF, os.path.join(res, f'mipmap-{density}', 'ic_launcher_background.png'))
" "$RES_DIR"

cat > "$RES_DIR/mipmap-anydpi-v26/app_icon.xml" << 'XML'
<?xml version="1.0" encoding="utf-8"?>
<adaptive-icon xmlns:android="http://schemas.android.com/apk/res/android">
    <background android:drawable="@mipmap/ic_launcher_background"/>
    <foreground android:drawable="@mipmap/ic_launcher_foreground"/>
</adaptive-icon>
XML

cp "$RES_DIR/mipmap-anydpi-v26/app_icon.xml" "$RES_DIR/mipmap-anydpi-v26/app_icon_round.xml"

cat > "$LIB_DIR/build.gradle" << 'GRADLE'
apply plugin: 'com.android.library'

android {
    namespace 'com.onesignal.example.icon'

    def unityLib = project(':unityLibrary').extensions.getByName('android')

    defaultConfig {
        minSdkVersion unityLib.defaultConfig.minSdkVersion.mApiLevel
        targetSdkVersion unityLib.defaultConfig.targetSdkVersion.mApiLevel
    }

    compileSdkVersion unityLib.compileSdkVersion
    buildToolsVersion unityLib.buildToolsVersion
}
GRADLE

cat > "$LIB_DIR/AndroidManifest.xml" << 'MANIFEST'
<manifest xmlns:android="http://schemas.android.com/apk/res/android"
    package="com.onesignal.example.icon">
</manifest>
MANIFEST

rm -f "$TMP_ICON"

echo "Done — icons written to Assets/Plugins/Android/AppIcon.androidlib/"
