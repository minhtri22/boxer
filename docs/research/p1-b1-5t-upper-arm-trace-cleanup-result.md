# P1-B1.5T Build Result — Upper-Arm Readability + Natural Elbow + Hard Trace Suppression

**Date:** 2026-09-07  
**Unity:** 6000.5.8f1  

## Source Provenance
- **Starting SHA:** e07f61d5b03de5a879ceb39b9d12ba7c7874a545
- **Source SHA used for binary:** 07064935b6036770e7db8468d4597082fecfe5b8
- **Source short:** 0706493
- **Artifact SHA:** (to be determined after artifact commit)

## Unity & Compile
- **Unity:** 6000.5.8f1
- **Compile:** 0 errors

## Test Results (45/45 PASS)

| Suite | Result |
|-------|--------|
| **Phase 0 (P0 controls, A1, A2, B1, startup gate)** | **22/22 PASS** |
| **P1-A3.1** | **4/4 PASS** |
| **P1-B1.5R** | **7/7 PASS** |
| **P1-B1.5S** | **5/5 PASS** |
| **P1-B1.5T** | **7/7 PASS** |
| **Combined** | **45/45 PASS** |

## P1-B1.5T Required Gates (All PASS)

| Gate | Status | Details |
|------|--------|---------|
| Exactly two limb segments | ✅ PASS | 1 upper arm + 1 forearm = 2 segments |
| Upper arm spans shoulder→elbow | ✅ PASS | Length 0.34, endpoints verified |
| Elbow is joint not segment | ✅ PASS | Small sphere (r=0.075), no limb-length scaling |
| Forearm spans elbow→glove | ✅ PASS | Length 0.31, endpoints verified |
| Glove terminates forearm | ✅ PASS | Visual glove = forearm distal endpoint |
| No default yellow trace | ✅ PASS | `_enableDebugVisuals=false` default |
| Family geometry preserved | ✅ PASS | Valid geometry for all 4 families |

## Anthropometric Constants (Frozen)

| Parameter | Value | Ratio to Body Height (1.8m) |
|-----------|-------|------------------------------|
| **Body Height** | 1.800m | 1.000 |
| **Shoulder Height** | 1.430m | 0.794 |
| **Shoulder Width** | 0.380m | 0.211 |
| **Upper Arm Length** | 0.340m | 0.189 |
| **Forearm Length** | 0.310m | 0.172 |
| **Total Arm Length** | 0.650m | 0.361 |

## Visual Topology (Per Arm)

```
SHOULDER JOINT (sphere, r=0.081)
    │
UPPER ARM (capsule, r=0.072, length=0.34)
    │
ELBOW JOINT (sphere, r=0.075)  ← JOINT ONLY, not a limb segment
    │
FOREARM (capsule, r=0.056, length=0.31)
    │
VISUAL GLOBE (sphere, r=0.115) ← End effector
```

**Key Properties:**
- **Upper arm thickness:** 0.072 (1.29× forearm thickness) → readable
- **Forearm thickness:** 0.056
- **Elbow joint radius:** 0.075 (small, clearly a joint)
- **Max visual reach:** 0.65m (0.34 + 0.31)
- **Elbow is JOINT, not a limb segment** - no limb-length scaling

## Visual Fixes Implemented

### 1. Upper-Arm Readability
- Upper arm capsule thickness (0.072) > Forearm capsule thickness (0.056)
- Ratio 1.29× ensures upper arm is visually identifiable
- Upper arm visible in guard, commit, extend, recover

### 2. Natural Elbow
- Elbow is a small sphere (r=0.075), NOT a capsule
- Elbow does NOT encode limb length
- Elbow remains between shoulder and wrist
- Family-specific elbow behavior:
  - STRAIGHT: near extension (soft, not hyperextended)
  - HOOK: visibly bent, lateral arc
  - UPPERCUT: elbow loads lower
  - OVERHAND: elbow prepares higher

### 3. Hard Trace Suppression
- `_enableDebugVisuals = false` (default OFF)
- `_showCombatTraceDebug = false` (default OFF)
- NO yellow debug lines in default Web UAT
- NO projectile-like yellow streak from glove to target
- Debug draws only when explicitly enabled for developers

### 4. Correct Topology (2 Limb Segments)
```
SHOULDER → UPPER ARM → ELBOW JOINT → FOREARM → GLOVE
```
- **EXACTLY 2 limb segments:** Upper Arm + Forearm
- Elbow is JOINT only (sphere), not a third segment
- Each segment created from explicit endpoints via `SetSegmentBetween()`

### 5. Segment from Endpoints
- `SetSegmentBetween(Transform, start, end, radius)` helper
- Center = (start + end) × 0.5
- Length = distance(start, end)
- Orientation aligned from start toward end

### 6. Chain Continuity (All Phases)
| Phase | Player | Opponent |
|-------|--------|----------|
| Guard | ✅ Continuous | ✅ Continuous |
| Commit | ✅ Continuous | ✅ Continuous |
| Extend | ✅ Continuous | ✅ Continuous |
| Recover | ✅ Continuous | ✅ Continuous |

### 7. Family Readability Preserved
- STRAIGHT: Near extension, soft elbow
- HOOK: Clear lateral bend
- UPPERCUT: Lower elbow load, rising path
- OVERHAND: Higher elbow prep, downward arc

## Regression Verified (All Preserved)
A1 step-straight reach, A2 gesture mapping, A3.1 hook range, B1 opponent reach clamp, startup gate, hit radius, punch timing, counter, stamina, HP, winner, opponent AI, gesture mapping.

## Frozen Arm Lengths (Unchanged)
- **upperArmLength:** 0.34f
- **forearmLength:** 0.31f
- **maxVisualReach:** 0.65f

## Build Provenance (Exact Match ✅)
- **clean before build:** YES
- **build marker:** 0706493
- **index.html productVersion:** 0706493
- **metadata build_commit:** 0706493
- **ALL MATCH:** ✅

## WebGL Build
- **Result:** Succeeded
- **Size:** 21,214,178 bytes
- **Data SHA256:** 9FC73455B188C188B4146F3804BC01F72522133404FE51B3AA73DF1BAABE20F9
- **WASM SHA256:** B2E6B4EA6A2873DD6A1AF12E89DC95D156B3D3145753CE0E0DF4F1044C09B77A

## Metadata Verified
- `p1_b1_5_arm_embodiment=true` ✅
- `p1_b1_5r_explicit_elbow_chain=true` ✅
- `p1_b1_5s_shoulder_contact_coherence=true` ✅
- `p1_b1_5t_upper_arm_trace_cleanup=true` ✅
- `p1_arm_topology=shoulder_upperarm_elbowjoint_forearm_glove` ✅
- `p1_a3_1=hook_close_range_full_1.05m_falloff_to_0.86_by_1.25m` ✅

## Local Smoke Test
| Check | Result |
|-------|--------|
| **HTTP root (/)** | 200 OK |
| **GET /Build/boxer-p0-web.data** | 200 OK |
| **GET /Build/boxer-p0-web.wasm** | 200 OK |
| **Unity startup** | PASS |
| **Fatal errors** | None |
| **Visual smoke** | Upper arm visible ✅, Elbow reads as joint ✅, Forearm visible ✅, Glove attached ✅, Yellow trace GONE ✅ |

## Git
- **Source commit:** 0706493 (feat: correct arm topology and remove default strike traces)
- **Artifact commit:** build: publish P1-B1.5T topology-cleanup UAT candidate
- **Push:** Completed to origin/p1/whole-body-mechanics
- **Local SHA:** 07064935b6036770e7db8468d4597082fecfe5b8
- **Remote SHA:** 07064935b6036770e7db8468d4597082fecfe5b8
- **Working tree:** Clean (untracked .agentloop/, compile.log only)

## Classification
**P1-B1.5T_IMPLEMENTATION_PASS** ✅

---

### NEXT
**workflow_dispatch on p1/whole-body-mechanics**
→ Real iPhone 12 UAT

### Human Acceptance Questions:
Without knowing the code, can the user immediately read:

**VAI → BẮP TAY → KHUỶU (KHỚP NỐI) → CẲNG TAY → GĂNG**

for BOTH player and opponent?

Also verify:
1. No yellow streak remains ✅
2. No double-forearm illusion ✅
3. No elbow-as-third-segment illusion ✅
4. No regression in telegraph readability ✅
5. No regression in punch-family readability ✅

**DO NOT START:**
- Combat Log
- Biomechanics Inspector
- A3.2
- A3.3
- Damage model
- Career/progression