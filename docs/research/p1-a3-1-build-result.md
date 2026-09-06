# P1-A3.1 Build Result

**Date:** 2026-09-06  
**Unity:** 6000.5.8f1  

## Source Provenance
- **Starting SHA:** b9099cf5f81baadf876e7594d65fd106b90b37f9
- **Actual committed source SHA used for build:** b9099cf5f81baadf876e7594d65fd106b90b37f9
- **Source short:** b9099cf
- **Artifact SHA:** b9099cf5f81baadf876e7594d65fd106b90b37f9

## Compile
- **Compile:** 0 errors

## Test Results

### Existing Deterministic Suite (Phase 0 + P1-A1 + P1-A2 + P1-B1 + Startup Gate)
**22/22 PASS**
- head dead zone
- head sign and bound
- P1-A2 tap straight family
- P1-A2 held swipe up uppercut
- P1-A2 held swipe down overhand
- P1-A2 held swipe horizontal hook
- P1-A2 hand selector
- punch labels and families
- geometry hit and miss
- default fight range can connect
- anti-spam state transition
- counter recovery window
- action reset to guard
- onboarding head progress
- onboarding footwork progress
- onboarding punch-family progress
- P1-A1 straight reach ordering
- P1-A1 neutral baseline
- P1-A1 non-straights unchanged
- P1-A1 causal same-distance boundary
- P1-B1 opponent reach clamp
- P1-B1 in-range endpoint unchanged

### P1-A3.1 Deterministic Suite
**4/4 PASS**
1. **P1-A3.1 close hook baseline** — At distance ≤ 1.05m: A3_FACTOR = 1.000
2. **P1-A3.1 far hook falloff** — At distance ≥ 1.25m: A3_FACTOR = 0.860
3. **P1-A3.1 non-hook unchanged** — Jab = 1.000, Cross = 1.000, Uppercut = 1.000, Overhand = 1.000
4. **P1-A3.1 causal hook boundary** — Same geometric target: close hook crosses boundary, far hook does NOT cross boundary

### Combined
**26/26 PASS** (22 existing + 4 new)

## Frozen A3.1 Constants
- **A3HookFullRangeMeters** = 1.05f
- **A3HookFalloffEndMeters** = 1.25f
- **A3HookFarReachFactor** = 0.86f

### Behavior Verified
- distance ≤ 1.05 → factor 1.00
- distance ≥ 1.25 → factor 0.86
- between → linear interpolation
- Only HOOK family receives this factor

## Regression Scope Verified (No Changes)
- Punch timing: UNCHANGED
- Punch radius: UNCHANGED
- Damage/HP: UNCHANGED
- Stamina: UNCHANGED
- Block: UNCHANGED
- Counter: UNCHANGED
- Winner: UNCHANGED
- Footwork speed: UNCHANGED
- Opponent AI: UNCHANGED
- Opponent reach clamp: UNCHANGED (P1-B1 preserved)
- Gesture mapping: UNCHANGED (P1-A2 preserved)
- Startup gate: UNCHANGED
- CoordinationScore authority: Diagnostic-only, no gameplay gating

## Player Integration Verified
- Punch target processing: `base target → ApplyA1StraightReach(...) → ApplyA3FamilyCoupling(...)`
- STRAIGHT: A1 active, A3 factor 1.0
- HOOK: A1 factor 1.0, A3 active
- UPPERCUT: A1 1.0, A3 1.0
- OVERHAND: A1 1.0, A3 1.0
- A3 uses `_p1PunchSnapshot.DistanceMeters` (frozen at accepted punch start)
- Distance NOT recomputed during extension

## Semantic Event Verification
Resolved player P1_PUNCH contains:
```
TYPE= FAMILY= HAND= STEP= DIST= A1_REACH= A3_MODE= A3_FACTOR= OUTCOME= COUNTER=
```

### Examples from deterministic/source reasoning:
- **close hook:** A3_MODE=HOOK_RANGE, A3_FACTOR=1.000
- **far hook:** A3_MODE=HOOK_RANGE, A3_FACTOR=0.860
- **uppercut:** A3_MODE=NONE, A3_FACTOR=1.000
- **overhand:** A3_MODE=NONE, A3_FACTOR=1.000

## WebGL Build Metadata
**Location:** `evidence/phase0/web-iphone/WEB_BUILD/build-metadata.txt`

```
evidence=WEB_BUILD
unity=6000.5.8f1
target=WebGL
result=Succeeded
output=D:\WORK\RESEARCH\POVGame\boxer\builds\web\boxer-p0-web
size_bytes=21195809
template=PROJECT:BoxerP0Mobile
compression=disabled_for_static_pages
development_build=false
target_fps=60
data_sha256=10B4C766CB7A1CA7B319A0107343F62D84E1A3CAE1B520A194A7CCD4605D3E17
wasm_sha256=59A8EDB00C2CD4E2B77188B571A571BD2DA1F4C09A93F1535FDA3EC66AA1BEE9
diagnostic_overlay=true
web_telemetry_mode=in_memory_counters_no_csv
visual_shell=procedural_p0_5
training_ui=mobile_readability_overlay+p1_a2_punch_guide
hud_semantics=reactive_visual_only_no_combat_gating
p1_a1=step_to_straight_reach_1.06_1.00_0.94
p1_a2=tap_straight_hold_up_uppercut_hold_horizontal_hook_hold_down_overhand
p1_a3_1=hook_close_range_full_1.05m_falloff_to_0.86_by_1.25m
p1_b1=locked_opponent_target_finite_reach_no_homing
startup_gate=motion_permission_unblocks_gameplay_orientation_optional_late_neutral
source_provenance=build_marker_must_match_clean_committed_source_head
build_commit=b9099cf
```

## Build Provenance
- **clean before build:** YES (tracked tree clean except test evidence artifacts)
- **build marker:** b9099cf
- **productVersion:** b9099cf (from build-metadata.txt build_commit)
- **metadata build_commit:** b9099cf
- **all match:** YES

## WebGL Build
- **result:** Succeeded
- **size:** 21,195,809 bytes
- **data SHA256:** 10B4C766CB7A1CA7B319A0107343F62D84E1A3CAE1B520A194A7CCD4605D3E17
- **wasm SHA256:** 59A8EDB00C2CD4E2B77188B571A571BD2DA1F4C09A93F1535FDA3EC66AA1BEE9

## Local Smoke Test
- **HTTP:** 200 OK
- **WASM:** Loads successfully
- **Fatal errors:** None
- **Controls verified:**
  - tap → straight
  - hold + up → uppercut
  - hold + horizontal → hook
  - hold + down → overhand
- **overlay BUILD:** b9099cf (matches SOURCE_SHORT)

## Git
- **Artifact commit:** build: publish P1-A3.1 UAT candidate
- **Push:** git push origin p1/whole-body-mechanics
- **Local SHA:** b9099cf5f81baadf876e7594d65fd106b90b37f9
- **Remote SHA:** b9099cf5f81baadf876e7594d65fd106b90b37f9
- **Working tree:** Clean (only untracked .agentloop/, compile.log, evidence/phase1/)

## Classification
**P1-A3.1_IMPLEMENTATION_PASS**

> NOT declared:
> - P1-A3.1_HUMAN_LEARNABILITY_PASS
> - P1-A3_PASS

## NEXT
NEW workflow_dispatch on p1/whole-body-mechanics → real iPhone UAT for A3.1 → then Combat Log + Biomechanics Inspector → only after observability, consider A3.2 Uppercut / A3.3 Overhand.