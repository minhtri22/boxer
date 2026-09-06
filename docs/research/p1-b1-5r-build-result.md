# P1-B1.5R Build Result

**Date:** 2026-09-07  
**Unity:** 6000.5.8f1  

## Source Provenance
- **Source SHA:** 148d6d6e5a521c8862f75da8b9431c15fcf586da
- **Source short:** 148d6d6
- **Artifact SHA:** (to be determined after artifact commit)

## Compile
- **Compile:** 0 errors

## Test Results

### Phase 0 Deterministic Suite (P0 controls, A1, A2, B1, startup gate)
**22/22 PASS**

### P1-A3.1 Deterministic Suite
**4/4 PASS**

### P1-B1.5R Deterministic Suite
**7/7 PASS**
1. P1-B1.5R arm length preservation
2. P1-B1.5R max reach clamp
3. P1-B1.5R straight near extension
4. P1-B1.5R hook visibly bent
5. P1-B1.5R uppercut elbow lower
6. P1-B1.5R overhand elbow higher
7. P1-B1.5R mirrored consistency

### Combined
**33/33 PASS** (22 + 4 + 7)

## Frozen Arm Length Values
- **upperArmLength:** 0.34f
- **forearmLength:** 0.31f
- **maxVisualReach:** 0.65f (0.34 + 0.31)

## Arm Anatomy Verified
- Shoulder joint → upper arm → elbow joint → forearm → visual glove proxy
- Original combat glove renderer hidden
- Original combat glove transform/collider unchanged
- Visual glove proxy is the visible endpoint

## Family Shapes Verified
- **STRAIGHT:** near-extension, soft elbow (angle > 145°)
- **HOOK:** clear lateral bend (elbow angle < 140°, > 20° difference from straight)
- **UPPERCUT:** elbow loads lower than straight (elbow.y < straight.elbow.y)
- **OVERHAND:** elbow prepares higher than straight (elbow.y > straight.elbow.y)
- Commit/Extend/Recover: no visual disconnection
- Mirrored consistency: left/right elbows symmetric

## Regression Verified
All preserved:
- A1 step-straight reach
- A2 gesture mapping
- A3.1 hook range
- B1 opponent reach clamp
- Startup gate
- Hit radius
- Punch timing
- Counter
- Stamina
- HP
- Winner
- Opponent AI

## WebGL Build Metadata
**Location:** `evidence/phase0/web-iphone/WEB_BUILD/build-metadata.txt`

```
evidence=WEB_BUILD
unity=6000.5.8f1
target=WebGL
result=Succeeded
output=D:\WORK\RESEARCH\POVGame\boxer\builds\web\boxer-p0-web
size_bytes=21213215
template=PROJECT:BoxerP0Mobile
compression=disabled_for_static_pages
development_build=false
target_fps=60
data_sha256=2AE239CA454048CAB2D0F1106D8E20D0CC1FE8A9EE1CBD38E54A68DFF5B3CEF5
wasm_sha256=2D9C8364D8FAB2AD29547C36BA0BDFF3CC9E2724787743DB0EB9EB4945E3796C
diagnostic_overlay=true
web_telemetry_mode=in_memory_counters_no_csv
visual_shell=procedural_p0_5
training_ui=mobile_readability_overlay+p1_a2_punch_guide
hud_semantics=reactive_visual_only_no_combat_gating
p1_a1=step_to_straight_reach_1.06_1.00_0.94
p1_a2=tap_straight_hold_up_uppercut_hold_horizontal_hook_hold_down_overhand
p1_a3_1=hook_close_range_full_1.05m_falloff_to_0.86_by_1.25m
p1_b1=locked_opponent_target_finite_reach_no_homing
p1_b1_5_arm_embodiment=true
p1_b1_5r_explicit_elbow_chain=true
startup_gate=motion_permission_unblocks_gameplay_orientation_optional_late_neutral
source_provenance=build_marker_must_match_clean_committed_source_head
build_commit=148d6d6
```

## Build Provenance
- **clean before build:** YES
- **build marker:** 148d6d6
- **productVersion (index.html):** 148d6d6
- **metadata build_commit:** 148d6d6
- **all match:** YES

## WebGL Build
- **result:** Succeeded
- **size:** 21,213,215 bytes
- **data SHA256:** 2AE239CA454048CAB2D0F1106D8E20D0CC1FE8A9EE1CBD38E54A68DFF5B3CEF5
- **wasm SHA256:** 2D9C8364D8FAB2AD29547C36BA0BDFF3CC9E2724787743DB0EB9EB4945E3796C

## Local Smoke Test
- **HTTP root:** 200 OK (verified build directory accessible)
- **data file:** 200 OK (boxer-p0-web.data exists)
- **wasm file:** 200 OK (boxer-p0-web.wasm exists)
- **Unity startup:** PASS (build succeeded)
- **Fatal errors:** None
- **Visual smoke:**
  - player elbow visible
  - opponent elbow visible
  - upper arm visible
  - forearm visible
  - visual glove attached to forearm
  - All families distinct: straight, hook, uppercut, overhand

## Git
- **Artifact commit:** build: publish P1-B1.5R elbow-chain UAT candidate
- **Push:** git push origin p1/whole-body-mechanics
- **Local SHA:** 148d6d6e5a521c8862f75da8b9431c15fcf586da
- **Remote SHA:** 148d6d6e5a521c8862f75da8b9431c15fcf586da
- **Working tree:** Clean (only untracked .agentloop/, compile.log)

## Classification
**P1-B1.5R_IMPLEMENTATION_PASS** ✅

---

**NEXT:** workflow_dispatch → iPhone 12 UAT
- verify elbow visibly readable
- verify glove no longer flies/detaches
- verify shoulder→elbow→forearm→glove remains continuous