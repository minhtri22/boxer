# REAL_DEVICE — iPhone 12 Blocker

Status: **BLOCKED / NOT TESTED**

Observed on the P0 development machine with Unity 6000.5.8f1:

```text
Installed PlaybackEngines:
- WebGLSupport
- windowsstandalonesupport

iOSSupport: NOT INSTALLED
xcodebuild: NOT FOUND
ios-deploy: NOT FOUND
ideviceinstaller: NOT FOUND
macOS/Xcode environment hints: NONE
```

The current Windows environment therefore cannot complete the Unity → Xcode →
Apple signing → iPhone installation path required for real-device P0 validation.

No iPhone observation is claimed from Editor, Windows player, or synthetic
sensor input.
