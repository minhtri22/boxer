# P1 UAT Startup Gate Correction

## Problem

Real iPhone 12 UAT exposed a startup fragility in the WebGL surrogate: Safari motion permission could be granted while no `deviceorientation` event arrived promptly. The prior implementation globally set `Time.timeScale = 0` and kept the game behind a second calibration gate, so footwork, punches, onboarding and combat appeared completely inactive.

This is a surrogate startup issue, not a failure of P1-A1/A2/B1 combat mechanics.

## Corrected rule

Motion/orientation is an input capability, not the master clock for the entire game.

After Safari returns motion permission `GRANTED`:

- Unity gameplay is allowed to run immediately.
- Footwork, punch gestures, onboarding timers and combat are not globally frozen.
- Head input remains neutral while orientation is unavailable.
- The first valid orientation sample establishes the neutral head angle automatically.
- Late orientation delivery must recover head control without reload.

The Web template must not hold the game behind a second calibration button after Unity has loaded.

## Non-goals

This correction does not change:

- P1-A1 reach factors
- P1-A2 gesture thresholds or punch family mappings
- P1-B1 opponent reach constants
- punch timing
- hit radii
- block/counter rules
- stamina or HP semantics
- opponent AI strategy
- Safari performance optimization

## Build provenance rule

The previous UAT artifact was built after an uncommitted minimal compile correction while `BOXER_BUILD_MARKER` still reflected the pre-correction source HEAD. That artifact was usable for functional smoke but its displayed marker was not exact binary source provenance.

From this correction onward:

1. Any compile correction must be committed first.
2. Working tree must be clean before setting the marker.
3. `BOXER_BUILD_MARKER = git rev-parse --short HEAD` only after the committed source is final.
4. The generated `build_commit` metadata and in-game `BUILD` overlay must match that exact committed source HEAD.
5. Generated artifact/evidence is committed only after the build.

## Acceptance

Local build gate:

- Unity compile: 0 errors.
- Existing deterministic suite: 22/22 PASS unless new deterministic startup tests are intentionally added.
- WebGL build succeeds.
- Generated `index.html` contains the non-blocking startup flow.
- Metadata contains:
  - `startup_gate=motion_permission_unblocks_gameplay_orientation_optional_late_neutral`
  - `source_provenance=build_marker_must_match_clean_committed_source_head`
  - `build_commit=<exact source short SHA>`

Real-device gate:

- On iPhone Safari, after granting Motion permission, the Unity scene becomes interactive without waiting indefinitely for orientation.
- If orientation is delayed/missing, footwork and punch controls still work.
- If orientation arrives later, head control activates from a neutral sample without reload.
- A1/A2/B1 UAT can then proceed normally.
