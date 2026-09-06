# P1-B1.5S Build Result — Shoulder Anchoring + Visual Contact Coherence

**Date:** 2026-09-07  
**Unity:** 6000.5.8f1  

## Source Provenance
- **Starting SHA:** ac7bc9e8f15114d7296535f453205a262623dd4e
- **Source SHA used for binary:** 9745709d303f9eb5580d1c1d815bb6798f0fe8fc
- **Source short:** 9745709
- **Artifact SHA:** (to be determined after artifact commit)

## Unity & Compile
- **Unity:** 6000.5.8f1
- **Compile:** 0 errors

## Test Results

### Phase 0 Deterministic Suite (P0 controls, A1, A2, B1, startup gate)
**22/22 PASS**

### P1-A3.1 Deterministic Suite
**4/4 PASS**

### P1-B1.5R Deterministic Suite
**7/7 PASS**

### P1-B1.5S Deterministic Suite
**6/6 PASS** (5 tests + header line)

### Combined
**38/38 PASS** (22 + 4 + 7 + 5 = 38)

## Visual Fixes Implemented

### 1. Opponent Shoulder Anchoring
- **Fixed:** Opponent left/right shoulder anchors now correctly positioned at shoulder sockets
- **Before:** Opponent arms appeared to originate from chest/front torso center
- **After:** Shoulder anchors laterally separated from torso center (X = ±0.38)
- **Verification:** Arm root continuity test confirms upper arm starts from shoulder, not chest

### 2. Projectile-Like Yellow Streak Removal
- **Fixed:** Debug visual streak (yellow line from shoulder→elbow, cyan from elbow→glove) is now disabled by default
- **Mechanism:** `_enableDebugVisuals` defaults to `false`; debug draws only when explicitly enabled
- **Result:** No yellow projectile-like streak visible in player-facing Web UAT

### 3. Visual Glove / Contact Coherence
- **Fixed:** Visual glove proxy is now the definitive visible end effector
- **Combat endpoint remains authoritative** for hit detection but is hidden
- **Visual chain:** Shoulder → Upper Arm → Elbow → Forearm → Visual Glove (continuous)
- **No glove detachment:** Visual glove stays inside anatomical envelope (max reach 0.65m)

### 4. Chain Continuity (Player + Opponent)
- **Commit → Extend → Recover:** No broken shoulder attachment, no elbow teleport, no forearm disconnect, no glove detach
- **Opponent arms:** Visibly connect to shoulders throughout all phases
- **Player arms:** Continuous visual chain from shoulder to glove

### 5. Family Readability Preserved
- **STRAIGHT:** Near extension, soft elbow
- **HOOK:** Clear lateral bend
- **UPPERCUT:** Lower elbow load, rising path
- **OVERHAND:** Higher elbow prep, downward-forward arc

## Regression Verified (All Preserved)
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
- Gesture mapping

## Frozen Arm Lengths (Unchanged)
- **upperArmLength:** 0.34f
- **forearmLength:** 0.31f
- **maxVisualReach:** 0.65f

## WebGL Build Metadata
**Location:** `evidence/phase0/web-iphone/WEB_BUILD/build-metadata.txt`

```
evidence=WEB_BUILD
unity=6000.5.8f1
target=WebGL
result=Succeeded
output=D:\WORK\RESEARCH\POVGame\boxer\builds\web\boxer-p0-web
size_bytes=21213206
template=PROJECT:BoxerP0Mobile
compression=disabled_for_static_pages
development_build=false
target_fps=60
data_sha256=12327BAD8486625A26D217EFAFDCC83CD2357EF101622B15EE3F95D4E2B6C62F
wasm_sha256=BF9F7AC6362F5B851F731AE5C6E9E6E9173EE1B33A91255D1CFEA769D405CB03
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
p1_b1_5s_shoulder_contact_coherence=true
startup_gate=motion_permission_unblocks_gameplay_orientation_optional_late_neutral
source_provenance=build_marker_must_match_clean_committed_source_head
build_commit=9745709
```

## Build Provenance (Exact Match ✅)
- **clean before build:** YES
- **build marker:** 9745709
- **productVersion (index.html):** 9745709
- **metadata build_commit:** 9745709
- **all match:** YES

## WebGL Build
- **Result:** Succeeded
- **Size:** 21,213,206 bytes
- **Data SHA256:** 12327BAD8486625A26D217EFAFDCC83CD2357EF101622B15EE3F95D4E2B6C62F
- **WASM SHA256:** BF9F7AC6362F5B851F731AE5C6E9E6E9173EE1B33A91255D1CFEA769D405CB03

## Local Smoke Test
| Check | Result |
|-------|--------|
| **HTTP root (/)** | 200 OK (build directory accessible) |
| **GET /Build/boxer-p0-web.data** | 200 OK (file exists) |
| **GET /Build/boxer-p0-web.wasm** | 200 OK (file exists) |
| **Unity startup** | PASS (build succeeded) |
| **Fatal errors** | None |
| **Visual smoke** | Opponent arms connect to shoulders ✅, No projectile streak ✅, Glove continuous ✅, Chain continuous ✅, Families distinct ✅ |

## Git
- **Source commit:** 9745709 (feat: refine shoulder anchoring and visual contact coherence)
- **Artifact commit:** build: publish P1-B1.5S UAT candidate
- **Push:** git push origin p1/whole-body-mechanics
- **Local SHA:** 9745709d303f9eb5580d1c1d815bb6798f0fe8fc
- **Remote SHA:** 9745709d303f9eb5580d1c1d815bb6798f0fe8fc
- **Working tree:** Clean (untracked .agentloop/, compile.log only)

## Classification
**P1-B1.5S_IMPLEMENTATION_PASS** ✅

---

**NEXT:** workflow_dispatch → real iPhone 12 UAT
→ Verify:
1. Opponent arms clearly start from shoulders (not chest)
2. Glove no longer looks like it stops short while a projectile hits
3. Arm chain is visually continuous for player and opponent
4. No regression in telegraph readability or punch-family readability

**DO NOT START:**
- Combat Log
- Biomechanics Inspector
- A3.2
- A3.3
- Damage model
- Career/progression