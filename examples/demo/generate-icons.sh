#!/bin/sh
# Downloads the OneSignal padded logo and generates platform app icons.
# Android: launcher icons at all mipmap densities in an androidlib that
# Unity merges into the Gradle build, replacing the default icon.
# iOS: icons at all required sizes into Assets/AppIcons/iOS/, assigned
# to PlayerSettings automatically by Assets/App/Editor/iOS/IconSetter.cs.
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

IOS_ICON_DIR="$SCRIPT_DIR/Assets/AppIcons/iOS"
mkdir -p "$IOS_ICON_DIR"

echo "Generating iOS icons..."
for size in 20 29 40 58 60 76 80 87 120 152 167 180 1024; do
  sips -z "$size" "$size" "$TMP_ICON" --out "$IOS_ICON_DIR/icon_${size}.png" >/dev/null 2>&1
done

echo "Flattening iOS icons onto white background..."
python3 -c "
import struct, zlib, os, sys

def flatten(path):
    with open(path, 'rb') as f:
        f.read(8)
        chunks = []
        while True:
            hdr = f.read(8)
            if len(hdr) < 8:
                break
            length, ctype = struct.unpack('>I4s', hdr)
            data = f.read(length)
            f.read(4)
            chunks.append((ctype, data))

    ihdr = [d for t, d in chunks if t == b'IHDR'][0]
    w, h, bd, ct = struct.unpack('>IIBB', ihdr[:10])
    if bd != 8 or ct != 6:
        return

    raw = zlib.decompress(b''.join(d for t, d in chunks if t == b'IDAT'))
    bpp = 4
    stride = w * bpp
    prev = bytearray(stride)
    out = bytearray()

    pos = 0
    for y in range(h):
        fb = raw[pos]; pos += 1
        row = bytearray(raw[pos:pos + stride]); pos += stride
        for x in range(stride):
            a = row[x - bpp] if x >= bpp else 0
            b = prev[x]
            c = prev[x - bpp] if x >= bpp else 0
            if fb == 1: row[x] = (row[x] + a) & 0xFF
            elif fb == 2: row[x] = (row[x] + b) & 0xFF
            elif fb == 3: row[x] = (row[x] + (a + b) // 2) & 0xFF
            elif fb == 4:
                p = a + b - c
                pa, pb, pc = abs(p - a), abs(p - b), abs(p - c)
                row[x] = (row[x] + (a if pa <= pb and pa <= pc else b if pb <= pc else c)) & 0xFF
        out.append(0)
        for x in range(w):
            r, g, bl, al = row[x*4], row[x*4+1], row[x*4+2], row[x*4+3]
            af = al / 255.0
            out.extend([int(r * af + 255 * (1 - af)), int(g * af + 255 * (1 - af)), int(bl * af + 255 * (1 - af))])
        prev = row

    new_ihdr = struct.pack('>IIBBBBB', w, h, 8, 2, 0, 0, 0)
    idat = zlib.compress(bytes(out), 9)
    def chunk(t, d):
        c = t + d
        return struct.pack('>I', len(d)) + c + struct.pack('>I', zlib.crc32(c) & 0xFFFFFFFF)
    with open(path, 'wb') as f:
        f.write(b'\x89PNG\r\n\x1a\n')
        f.write(chunk(b'IHDR', new_ihdr))
        f.write(chunk(b'IDAT', idat))
        f.write(chunk(b'IEND', b''))

d = sys.argv[1]
for name in os.listdir(d):
    if name.startswith('icon_') and name.endswith('.png'):
        flatten(os.path.join(d, name))
" "$IOS_ICON_DIR"

rm -f "$TMP_ICON"

echo "Done — Android icons written to Assets/Plugins/Android/AppIcon.androidlib/"
echo "Done — iOS icons written to Assets/AppIcons/iOS/"
