# P1-A2-B1 Build Result

- **Source SHA**: 464257ffc1cee7c986b72646ec35c9f7e167df77
- **Unity Version**: 6000.5.8f1
- **Compile**: 0 errors (after fixing BoxerBootstrap property names)
- **Self-tests**: 22/22 PASS

## Gesture Mappings (P1-A2)
- TAP → STRAIGHT
- HOLD + UP → UPPERCUT
- HOLD + HORIZONTAL → HOOK
- HOLD + DOWN → OVERHAND
- All tested and passed via deterministic tests.

## A1 Regression
- Straight reach ordering applies.
- Neutral baseline matches.
- Hooks/Uppercuts/Overhands remain unchanged (Reach factor 1.000).

## B1 Finite Reach
- Opponent target locks upon commit.
- Reach is clamped to finite constants (head 1.02m, body 0.96m).
- No arbitrary homing on retreating targets.

## WebGL Metadata
- result=Succeeded
- compression=disabled_for_static_pages
- development_build=false
- target_fps=60
- size_bytes=21194365
- data_sha256=0F51C1CAADDC6B76406CF1290EDA7D65FA985CC742FC408D11A7EC70520209A4
- wasm_sha256=78DCA7CC7A854F8A11D285DA30AF90EFFDB13E5A5643FB520EE600B814F4FC36
- build_commit=464257f
- training_ui=mobile_readability_overlay+p1_a2_punch_guide
- p1_a1=step_to_straight_reach_1.06_1.00_0.94
- p1_a2=tap_straight_hold_up_uppercut_hold_horizontal_hook_hold_down_overhand
- p1_b1=locked_opponent_target_finite_reach_no_homing

## Local Smoke
- HTTP 200 OK: PASS
- Unity/WASM startup: PASS
- UI / Training Stages: PASS (Punch guide visible)
- Fatal errors: None

**Classification**: P1_A2_B1_BUILD_PASS
