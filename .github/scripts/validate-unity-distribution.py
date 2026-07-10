#!/usr/bin/env python3

import json
import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
DEMO = ROOT / "examples/demo"
BOOTSTRAP = ROOT / "examples/demo/Assets/OneSignal"
SAMPLE = ROOT / "com.onesignal.unity.core/Samples~"
INVENTORY = BOOTSTRAP / "Editor/Resources/OneSignalFileInventory.asset"

EXCLUDED_PREFIXES = (
    "Assets/OneSignal/Attribution",
    "Assets/OneSignal/Example",
)
EXCLUDED_FILES = {
    "Assets/OneSignal/Editor/OneSignalAndroidDependencies.xml",
    "Assets/OneSignal/Editor/OneSignalAndroidDependencies.xml.meta",
    "Assets/OneSignal/Editor/OneSignaliOSDependencies.xml",
    "Assets/OneSignal/Editor/OneSignaliOSDependencies.xml.meta",
}
REQUIRED_SAMPLE_GUIDS = {
    "OneSignal.UnityPackage.Example.asmdef.meta": "a28dab59edddfb3448f1fd9318f85c32",
    "OneSignalExampleBehaviour.cs.meta": "1a40a711031e4cb8b9b3f674dda19d55",
    "OneSignalExampleScene.unity.meta": "54284d014d2241544a24b57b13c09ac8",
    "INCONSOLATA-VARIABLEFONT_WDTH,WGHT.TTF.meta": "792110c4f5f19e64196eb25f41c0b783",
}


def fail(message: str) -> None:
    print(f"Unity distribution validation failed: {message}", file=sys.stderr)
    raise SystemExit(1)


def is_distributed(path: str) -> bool:
    if path in EXCLUDED_FILES or path.endswith("/.DS_Store"):
        return False
    return not any(
        path == prefix
        or path == f"{prefix}.meta"
        or path.startswith(f"{prefix}/")
        for prefix in EXCLUDED_PREFIXES
    )


def validate_inventory() -> None:
    actual = sorted(
        path.relative_to(DEMO).as_posix()
        for path in BOOTSTRAP.rglob("*")
        if path.is_file() and is_distributed(path.relative_to(DEMO).as_posix())
    )
    recorded = re.findall(r"^\s*-\s+(Assets/OneSignal/.+)$", INVENTORY.read_text(), re.MULTILINE)

    if actual != recorded:
        missing = sorted(set(actual) - set(recorded))
        stale = sorted(set(recorded) - set(actual))
        fail(
            "file inventory is out of date"
            + (f"; missing: {', '.join(missing)}" if missing else "")
            + (f"; stale: {', '.join(stale)}" if stale else "")
        )


def validate_sample() -> None:
    for filename, expected_guid in REQUIRED_SAMPLE_GUIDS.items():
        path = SAMPLE / filename
        if not path.is_file():
            fail(f"sample is missing {path.relative_to(ROOT)}")
        match = re.search(r"^guid:\s*(\w+)$", path.read_text(), re.MULTILINE)
        if match is None or match.group(1) != expected_guid:
            fail(f"{path.relative_to(ROOT)} does not preserve GUID {expected_guid}")

    scene = (SAMPLE / "OneSignalExampleScene.unity").read_text()
    if "OneSignalSDK.OneSignalExampleBehaviour" in scene:
        fail("sample scene contains stale OneSignalExampleBehaviour type references")
    if "OneSignalExampleBehaviour, OneSignal.UnityPackage.Example" not in scene:
        fail("sample scene does not contain OneSignalExampleBehaviour button bindings")

    asmdef = json.loads((SAMPLE / "OneSignal.UnityPackage.Example.asmdef").read_text())
    required_references = {
        "OneSignal.Core",
        "UnityEngine.UI",
        "UnityEngine.JSONSerializeModule",
    }
    if not required_references.issubset(asmdef["references"]):
        fail("sample assembly is missing required references")

    package = json.loads((ROOT / "com.onesignal.unity.core/package.json").read_text())
    if not any(sample.get("path") == "Samples~" for sample in package.get("samples", [])):
        fail("core package does not register Samples~")
    if not any(
        version.get("name") == "com.onesignal.unity.core"
        and version.get("expression") == package["version"]
        and version.get("define") == "ONE_SIGNAL_INSTALLED"
        for version in asmdef.get("versionDefines", [])
    ):
        fail("sample version define does not match the core package version")


def main() -> None:
    if (BOOTSTRAP / "Example").exists() or (BOOTSTRAP / "Example.meta").exists():
        fail("empty legacy Asset Store Example path still exists")
    if (BOOTSTRAP / "Documentation~").exists():
        fail("Documentation~ is hidden from Unity and cannot be exported")
    if not (BOOTSTRAP / "Documentation").is_dir():
        fail("exportable bootstrap documentation is missing")
    validate_inventory()
    validate_sample()
    print("Unity distribution layout is valid.")


if __name__ == "__main__":
    main()
