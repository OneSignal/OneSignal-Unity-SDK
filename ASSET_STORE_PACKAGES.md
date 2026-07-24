# Unity Asset Store packages

The multi-package Unity Asset Store product starts at SDK version `6.0.0`.
This major-version cutover changes the package identifiers:

- `com.onesignal.unitysdk.core`
- `com.onesignal.unitysdk.android`
- `com.onesignal.unitysdk.ios`

Android and iOS depend on the exact same version of Core. All packages require
Unity 2022.3 or newer.

## Release verification

Before uploading a release:

1. Run `python3 .github/scripts/validate-unity-distribution.py`.
2. Pack and sign each package with the OneSignal Unity organization through
   Asset Store Publishing Tools.
3. Confirm each signed tarball contains `.attestation.p7m`.
4. Run Unity package validation for every tarball.
5. Upload each tarball to the matching technical name in the existing
   multi-package draft.
6. Install the draft product in clean Unity 2022.3 and current Unity 6
   projects. Verify dependency resolution, valid signatures, and the absence
   of duplicate assemblies or missing scripts.

Migration and deprecation of the legacy `com.onesignal.unity.*` npm packages
are handled separately from this package cutover.
