# Phase 0 — Web iPhone Surrogate Result

## Current decision

- **P0 Interaction:** BLOCKED pending real iPhone 12 Safari human validation
- **Web Delivery Quality:** NOT YET RATED
- **Native iOS Performance:** NOT TESTED

## Implementation status

The Web surrogate routes Safari `deviceorientation` events through the existing `BoxerInput` head-control path, provides explicit permission/calibration UI, preserves the existing left/right touch controls, exposes Web debug state, and prepares a local WebGL build for static HTTPS hosting.

Static repository checks pass (`git diff --check`; Unity custom-template tokens match the installed Unity 6000.5.8f1 default template contract). A real Unity WebGL build could not be produced inside the Codex sandbox because Unity licensing cannot initialize with the sandbox's read-only external licensing database. See `evidence/phase0/web-iphone/WEB_BUILD/blocker.md`.

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
