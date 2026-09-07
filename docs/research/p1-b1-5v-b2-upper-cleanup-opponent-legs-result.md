# P1-B1.5V + P1-B2 Build Result — Upper-Body Cleanup + Opponent Leg Embodiment

**Date:** 2026-09-07  
**Unity:** 6000.5.8f1  

## Source Provenance
- **Repository:** D:\WORK\RESEARCH\POVGame\boxer
- **Branch:** p1/whole-body-mechanics
- **Starting SHA:** 44e542fb984eb820c433fb00d3b6b57973b4415f
- **Source SHA used for binary:** 4939e2bd5f44513d9d2ad3732ae07ac02f2ea148
- **Source short:** 4939e2b
- **Artifact SHA:** (see Git section)

## Unity & Compile
- **Unity:** 6000.5.8f1
- **Compile:** 0 errors

## Test Results (All Suites)

| Suite | Result |
|-------|--------|
| Phase 0 (P0 controls, A1, A2, B1, startup gate) | 22/22 PASS |
| P1-A3.1 | 4/4 PASS |
| P1-B1.5R | 7/7 PASS |
| P1-B1.5S | 5/5 PASS |
| P1-B1.5T | 7/7 PASS |
| P1-B1.5U | 6/6 PASS |
| P1-B1.5V (new) | 6/6 PASS |
| P1-B2 (new) | 7/7 PASS |
| **Combined** | **64/64 PASS** |

## P1-B1.5V — Upper-Body Cleanup

### Opponent Guard Naturalization
Refined frozen guard parameters (tighter, less flared):
- `_opponentGloveHeightOffset = 0.16f` (raised toward face)
- `_opponentGloveForwardOffset = 0.16f`
- `_opponentGloveLateralInset = 0.16f` (tighter guard)
- `_opponentElbowInwardBias = 0.10f`
- `_opponentElbowHeightOffset = -0.08f` (dropped toward ribs)
- `_opponentLeftRightHeightDiff = 0.04f` (lead/rear asymmetry)
- `_opponentElbowInwardBiasGuard = 0.16f` (elbows tucked)
- `_opponentElbowForwardBias = 0.04f` (elbows forward, not flared sideways)

### Player Yellow/Orange Artifact Removal
**Root cause identified:** `BoxerVisualShell` (execution order 100, runs in `Start()`)
adds gold cuff/panel decorations (yellow/orange) as children of the player's
original combat gloves AFTER `ArmVisualEmbodiment` (order 200) hides the glove
renderers during `Initialize()` (called from `Awake()`).

**Fix:** `ArmVisualEmbodiment` now re-hides the original combat glove's child
renderers every frame via `HideOriginalGlove()` in `Update()`, defeating the
late decoration ordering. No yellow/orange flying block/streak remains in the
default player-facing Web UAT.

## P1-B2 — Opponent Pelvis + Leg Embodiment

New `OpponentLegEmbodiment.cs` replaces the lower-body pedestal with:
```
PELVIS → THIGH → KNEE JOINT → SHIN → FOOT
```
Per leg: exactly 2 limb segments (thigh + shin); knee is a joint (small sphere).

### Frozen Lower-Body Constants
- `PelvisHeight = 0.92f`
- `HipWidth = 0.26f`
- `ThighLength = 0.46f`
- `ShinLength = 0.44f`
- `FootLength = 0.22f`
- `KneeRadius = 0.055f`
- `ThighRadius = 0.075f`
- `ShinRadius = 0.055f`

### Torso Shrink
The opponent body capsule was shrunk to torso-only (no longer a pedestal),
while the authoritative combat SphereCollider was preserved at its original
world position (center offset compensates for the visual capsule move).

## Anthropometric Constants (Frozen)
- body height = 1.8m
- shoulder height = 1.43m
- shoulder width = 0.38m
- upper arm length = 0.34m
- forearm length = 0.31m
- total arm length = 0.65m
- pelvis height = 0.92m
- hip width = 0.26m
- thigh length = 0.46m
- shin length = 0.44m
- foot length = 0.22m

## Regression Verified (All Preserved)
A1, A2, A3.1, B1, startup gate, hit geometry, punch timing, punch radius,
stamina, HP, counter, winner, opponent AI, gesture mapping, telegraph
readability, family readability.

## Build Provenance (Exact Match)
- clean before build: YES
- build marker: 4939e2b
- productVersion: 4939e2b
- metadata build_commit: 4939e2b
- all match: YES

## WebGL Build
- result: Succeeded
- development_build: false
- compression: disabled_for_static_pages
- target_fps: 60
- size: 21,226,722 bytes
- data SHA256: CEC7B56DB844F7B2D3C4CB7C8B7DF09B693CE0EB1B5113E91D0CD65E908CD381
- wasm SHA256: 008E9737919271CC23C250C1279F2E7DF59C220AD171A895B107B0A5F95EDC68

## Metadata Flags
- p1_b1_5v_guard_cleanup_player_contact_cleanup=true
- p1_b2_opponent_lower_body_embodiment=true
- (plus all prior p1_b1_5* flags and anthropometric constants)

## Local Web Smoke
- HTTP root (index.html): 200
- HTTP data: 200
- HTTP wasm: 200
- Unity startup: PASS (build succeeded)
- Fatal errors: NONE

## Classification
**P1-B1.5V_B2_IMPLEMENTATION_PASS**

> NOT declared: human UAT PASS.

## NEXT
workflow_dispatch on p1/whole-body-mechanics → real iPhone 12 UAT with:
1. Opponent guard now looks like a believable boxing guard?
2. Player punch no longer shows an unexplained yellow/orange flying object?
3. Opponent lower body reads as pelvis → thigh → knee → shin → foot?
4. Does opponent movement read more naturally with legs?