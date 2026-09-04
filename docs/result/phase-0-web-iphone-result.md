# Phase 0 — Web iPhone Surrogate Result

## Current decision

- **P0 Interaction:** BLOCKED pending real iPhone 12 Safari human validation
- **Web Delivery Quality:** NOT YET RATED
- **Native iOS Performance:** NOT TESTED

## Implementation status

The Web surrogate routes Safari `deviceorientation` events through the existing `BoxerInput` head-control path, provides explicit permission/calibration UI, preserves the existing left/right touch controls, exposes Web debug state, and prepares a local WebGL build for static HTTPS hosting.

Static repository checks pass. After Unity Hub sign-in, the normal interactive Unity 6000.5.8f1 Editor initialized a Unity Personal license and produced a successful WebGL Development build at builds/web/boxer-p0-web using the PROJECT:BoxerP0Mobile template. The build is 36,431,792 bytes (34.74 MiB) and contains index.html, loader, framework, data, and wasm outputs. Local HTTP/headless Edge smoke loaded the page and Unity instance successfully; desktop correctly reported DEVICE ORIENTATION NOT RECEIVED, which is not an iPhone motion result. The earlier sandbox licensing blocker is retained as historical tooling evidence in evidence/phase0/web-iphone/WEB_BUILD/blocker.md.

## Required real-device evidence

| Test | Status |
| --- | --- |
| Motion permission + calibration | NOT TESTED ON IPHONE |
| Head-only evade | NOT TESTED ON IPHONE |
| Touch coexistence | NOT TESTED ON IPHONE |
| Slip → Counter | NOT TESTED ON IPHONE |
| Move + Slip + Counter | NOT TESTED ON IPHONE |
| 60–90 s bout | NOT TESTED ON IPHONE |
| Control Comprehension & Agency | NOT TESTED WITH HUMAN |

No native iOS conclusion may be inferred from this surrogate.

## Next authorized action

`NEXT AUTHORIZED ACTION = user runs real iPhone 12 Safari human validation.`
