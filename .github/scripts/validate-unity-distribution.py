#!/usr/bin/env python3

import json
import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
DEMO = ROOT / "examples/demo"
DEMO_PROJECTS = (DEMO, ROOT / "examples/demo-no-location")
BOOTSTRAP = ROOT / "examples/demo/Assets/OneSignal"
SAMPLE = ROOT / "com.onesignal.unitysdk.core/Samples~"
INVENTORY = BOOTSTRAP / "Editor/Resources/OneSignalFileInventory.asset"
PACKAGE_NAMES = (
    "com.onesignal.unitysdk.core",
    "com.onesignal.unitysdk.android",
    "com.onesignal.unitysdk.ios",
)
CORE_PACKAGE = PACKAGE_NAMES[0]
PLATFORM_PACKAGES = PACKAGE_NAMES[1:]
EDM_PACKAGE = "com.unity.external-dependency-manager"
EDM_VERSION = "2.0.0"
MINIMUM_UNITY_VERSION = "2022.3"
MINIMUM_UNITY_RELEASE = "0f1"

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


def validate_packages() -> None:
    packages = {
        name: json.loads((ROOT / name / "package.json").read_text())
        for name in PACKAGE_NAMES
    }
    core_version = packages[CORE_PACKAGE]["version"]

    for name, package in packages.items():
        if package.get("name") != name:
            fail(f"{name}/package.json name does not match its folder")
        if package.get("version") != core_version:
            fail(f"{name} version does not match {CORE_PACKAGE}")
        if package.get("unity") != MINIMUM_UNITY_VERSION:
            fail(f"{name} does not support Unity {MINIMUM_UNITY_VERSION}")
        if package.get("unityRelease") != MINIMUM_UNITY_RELEASE:
            fail(
                f"{name} does not support Unity "
                f"{MINIMUM_UNITY_VERSION}.{MINIMUM_UNITY_RELEASE}"
            )
        if package.get("license") != "SEE LICENSE IN LICENSE.md":
            fail(f"{name} does not reference its package license")
        if not package.get("documentationUrl", "").startswith("https://"):
            fail(f"{name} does not have an HTTPS documentation URL")
        if not package.get("repository", {}).get("url", "").startswith("https://"):
            fail(f"{name} does not have an HTTPS repository URL")
        if name != CORE_PACKAGE:
            if package.get("dependencies", {}).get(CORE_PACKAGE) != core_version:
                fail(f"{name} does not depend on {CORE_PACKAGE}@{core_version}")
            if package.get("samples"):
                fail(f"{name} must not contain samples")

    for name in PLATFORM_PACKAGES:
        if packages[name].get("dependencies", {}).get(EDM_PACKAGE) != EDM_VERSION:
            fail(f"{name} does not depend on {EDM_PACKAGE}@{EDM_VERSION}")

    legacy_installer = ROOT / CORE_PACKAGE / "Editor/SetupSteps/InstallEdm4uStep.cs"
    if legacy_installer.exists():
        fail(f"{legacy_installer.relative_to(ROOT)} installs unsupported Google EDM4U")

    for project in DEMO_PROJECTS:
        project_name = project.relative_to(ROOT)
        manifest = json.loads((project / "Packages/manifest.json").read_text())
        if manifest.get("scopedRegistries"):
            fail(f"{project_name} must not declare a scoped registry")
        for name in PACKAGE_NAMES:
            expected_path = f"file:../../../{name}"
            if manifest["dependencies"].get(name) != expected_path:
                fail(f"{project_name} does not reference {name} at {expected_path}")


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
    behaviour = (SAMPLE / "OneSignalExampleBehaviour.cs").read_text()
    if "ExitLiveActivityAsync" in scene or "LiveActivities.ExitAsync" in behaviour:
        fail("sample exposes the deprecated Live Activities exit API")

    asmdef = json.loads((SAMPLE / "OneSignal.UnityPackage.Example.asmdef").read_text())
    required_references = {
        "OneSignal.Core",
        "UnityEngine.UI",
        "UnityEngine.JSONSerializeModule",
    }
    if not required_references.issubset(asmdef["references"]):
        fail("sample assembly is missing required references")

    package = json.loads((ROOT / CORE_PACKAGE / "package.json").read_text())
    if not any(sample.get("path") == "Samples~" for sample in package.get("samples", [])):
        fail("core package does not register Samples~")
    if not any(
        version.get("name") == "com.onesignal.unitysdk.core"
        and version.get("expression") == package["version"]
        and version.get("define") == "ONE_SIGNAL_INSTALLED"
        for version in asmdef.get("versionDefines", [])
    ):
        fail("sample version define does not match the core package version")


def main() -> None:
    for project in DEMO_PROJECTS:
        legacy_edm = project / "Assets/ExternalDependencyManager"
        if legacy_edm.exists():
            fail(f"{legacy_edm.relative_to(ROOT)} bundles unsupported Google EDM4U")
        settings_path = project / "ProjectSettings/ExternalDependencyManagerSettings.json"
        if not settings_path.is_file():
            fail(f"{settings_path.relative_to(ROOT)} is missing")
        settings = json.loads(settings_path.read_text())
        if not settings.get("edmEnabled"):
            fail(f"{settings_path.relative_to(ROOT)} does not enable Unity EDM")
    if (BOOTSTRAP / "Example").exists() or (BOOTSTRAP / "Example.meta").exists():
        fail("empty legacy Asset Store Example path still exists")
    if (BOOTSTRAP / "Documentation~").exists():
        fail("Documentation~ is hidden from Unity and cannot be exported")
    if not (BOOTSTRAP / "Documentation").is_dir():
        fail("exportable bootstrap documentation is missing")
    validate_inventory()
    validate_packages()
    validate_sample()
    print("Unity distribution layout is valid.")


if __name__ == "__main__":
    main()
