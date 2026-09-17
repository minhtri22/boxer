# Ramirez MPFB Unity integration gate

Date: 2026-09-17

Result: `UNITY_INTEGRATION_GATE: PASS_PENDING_CLEAN_WEBGL_AND_HUMAN_UAT`

## Imported asset

- Unity 6000.5.8f1 imported `Resources/Boxer3D/Ramirez_UAT3.fbx` successfully.
- Imported Ramirez triangles: `29,386` (budget: `<= 30,000`).
- Imported hierarchy: `91` transforms, `35` MeshFilters, `1` SkinnedMeshRenderer.
- MPFB bone aliases used by the presentation adapter are present.
- `BLENDER_3D_ASSET_AUDIT=PASS`.

## Deterministic regression

- Round2 deterministic mechanics/contact suite: `43/43 PASS`.
- Blender/EV shell and asset checks: `21/21 PASS`.
- Existing Round2 mechanics remain authoritative; the MPFB rig is presentation-only.

## Runtime ownership / deformation

- Audited frames: `1,200`.
- Failed frames: `0`.
- Visual path: `blender_fbx_follow_round2`.
- Maximum Blender hand-to-authoritative-glove anchor error: `8.371217e-07 m`.
- `RESULT=PASS` for runtime ownership.
- `RUNTIME_INVARIANTS=PASS`.
- Committed root drift: `0 m`.
- Committed rotation drift: `0 degrees`.
- Editor rig timing: mean `0.06249 ms`, max `3.65960 ms`; this is Editor evidence, not WebGL/device performance evidence.

## Visual runtime evidence

Ten portrait runtime captures are frozen under `motion/motion-00.png` through `motion/motion-09.png`.
The MPFB body, gloves, trunks, boots and hair remain attached across the sampled guard/punch/footwork sequence; no gross shoulder/elbow separation or glove double-transform regression is visible in the sampled frames.

This is not a substitute for final human UAT on the target device.

## Not claimed yet

Final WebGL qualification is intentionally not claimed from this dirty working tree. Repository provenance rules require a committed candidate SHA and a clean tracked source tree before the authoritative WebGL build. Human visual acceptance also remains pending.

## Evidence

- `unity-asset-audit.log`
- `unity-tests.log`
- `unity-runtime.log`
- `geometry-tests.txt`
- `round2-tests.txt`
- `contact-matrix.csv`
- `ownership-runtime.txt`
- `runtime.txt`
- `motion/motion-00.png` ... `motion/motion-09.png`
