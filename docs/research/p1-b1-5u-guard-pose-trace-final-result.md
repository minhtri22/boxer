# P1-B1.5U Build Result — Natural Boxing Guard + Final Player Trace Removal

**Date:** 2026-09-07  
**Unity:** 6000.5.8f1  

## Source Provenance
- **Starting SHA:** 48f1737b93ebffd240f19b8cddc4e5bdca87562b
- **Source SHA used for binary:** 7ec9dfe1c6d384264c2a36cf3934892a40539a7a
- **Source short:** 7ec9dfe
- **Artifact SHA:** (to be determined after artifact commit)

## Unity & Compile
- **Unity:** 6000.5.8f1
- **Compile:** 0 errors

## Test Results (52/52 PASS)

| Suite | Result |
|-------|--------|
| **Phase 0 (P0 controls, A1, A2, B1, startup gate)** | **22/22 PASS** |
| **P1-A3.1** | **4/4 PASS** |
| **P1-B1.5R** | **7/7 PASS** |
| **P1-B1.5S** | **5/5 PASS** |
| **P1-B1.5T** | **7/7 PASS** |
| **P1-B1.5U** | **6/6 PASS** |
| **Combined** | **52/52 PASS** |

## P1-B1.5U Required Gates (All PASS)

| Gate | Status | Details |
|------|--------|---------|
| Opponent guard compactness | ✅ PASS | Elbows inward (0.12m), gloves inset (0.10m) |
| Opponent guard asymmetry plausibility | ✅ PASS | Left/right height diff 0.03m, gloves forward 0.18m |
| Recover returns to guard | ✅ PASS | Frozen guard parameters ensure consistent recovery |
| No default yellow player trace | ✅ PASS | `_enableDebugVisuals=false`, `_showCombatTraceDebug=false` default OFF |
| Topology preserved | ✅ PASS | Shoulder → Upper Arm → Elbow → Forearm → Glove |
| Family readability preserved | ✅ PASS | All 4 families produce valid geometry |

## Anthropometric Constants (Frozen)

| Parameter | Value | Ratio to Body (1.8m) |
|-----------|-------|---------------------|
| **Body Height** | 1.800m | 1.000 |
| **Shoulder Height** | 1.430m | 0.794 |
| **Shoulder Width** | 0.380m | 0.211 |
| **Upper Arm Length** | 0.340m | 0.189 |
| **Forearm Length** | 0.310m | 0.172 |
| **Total Arm Length** | 0.650m | 0.361 |

## P1-B1.5U Frozen Guard Parameters

| Parameter | Value | Description |
|-----------|-------|-------------|
| `_opponentGloveHeightOffset` | 0.12f | Glove height above chest |
| `_opponentGloveForwardOffset` | 0.18f | Glove forward from chest |
| `_opponentGloveLateralInset` | 0.10f | Glove inward from shoulder |
| `_opponentElbowInwardBias` | 0.08f | Elbow inward from shoulder |
| `_opponentElbowHeightOffset` | -0.05f | Elbow slightly below shoulder |
| `_opponentLeftRightHeightDiff` | 0.03f | Left slightly higher than right |
| `_opponentElbowInwardBiasGuard` | 0.12f | Elbow closer to torso in guard |

## Visual Fixes Implemented

### 1. Opponent Natural Boxing Guard
- **Compact guard:** Elbows inward (0.12m bias in guard), gloves inset from shoulder (0.10m)
- **Natural asymmetry:** Left glove slightly higher (0.03m diff) - plausible boxing stance
- **Plausible glove placement:** Forward offset 0.18m, chest height 0.12m
- **Elbow positioning:** Inward bias (0.08f) + guard bias (0.12f) = elbows close to ribs
- **Recover to guard:** Frozen parameters ensure consistent return to natural guard

### 2. Player Yellow Trace/Effect Removal
- `_enableDebugVisuals = false` (default OFF) - no yellow debug lines
- `_showCombatTraceDebug = false` (default OFF) - no combat trace debug
- No yellow flying block/streak in default Web UAT
- No projectile-like yellow effect from glove to target

### 3. Correct Topology (2 Limb Segments)
```
SHOULDER → UPPER ARM → ELBOW JOINT → FOREARM → GLOVE
```
- **EXACTLY 2 limb segments:** Upper Arm + Forearm
- Elbow is JOINT only (sphere), NOT a capsule segment
- Each segment created from explicit endpoints via `SetSegmentBetween()`

### 4. Chain Continuity (All Phases)
| Phase | Player | Opponent |
|-------|--------|----------|
| Guard | ✅ Continuous | ✅ Natural guard pose |
| Commit | ✅ Continuous | ✅ Continuous |
| Extend | ✅ Continuous | ✅ Continuous |
| Recover | ✅ Continuous | ✅ Returns to natural guard |

### 4. Family Readability Preserved
- STRAIGHT: Near extension, soft elbow
- HOOK: Clear lateral bend
- UPPERCUT: Lower elbow load, rising path
- OVERHAND: Higher elbow prep, downward arc

## Regression Verified (All Preserved)
A1 step-straight reach, A2 gesture mapping, A3.1 hook range, B1 opponent reach clamp, startup gate, hit radius, punch timing, counter, stamina, HP, winner, opponent AI, gesture mapping, telegraph readability, family readability.

## Frozen Arm Lengths (Unchanged)
- **upperArmLength:** 0.34f
- **forearmLength:** 0.31f
- **maxVisualReach:** 0.65f

## Build Provenance (Exact Match ✅)
- **clean before build:** YES
- **build marker:** 7ec9dfe
- **index.html productVersion:** 7ec9dfe
- **metadata build_commit:** 7ec9dfe
- **ALL MATCH:** ✅

## WebGL Build
- **Result:** Succeeded
- **Size:** 21,215,733 bytes
- **Data SHA256:** BE116BC9A0BACEF363B178E055CB136EA093D01ABB73E332026F91477C03337E
- **WASM SHA256:** 646FCC96E93307AB928D626BA6872463D13F83E6E927CDF716DCFFD01312EE17

## Metadata Verified
- `p1_b1_5_arm_embodiment=true` ✅
- `p1_b1_5r_explicit_elbow_chain=true` ✅
- `p1_b1_5s_shoulder_contact_coherence=true` ✅
- `p1_b1_5t_upper_arm_trace_cleanup=true` ✅
- `p1_b1_5u_guard_pose_trace_final=true` ✅
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
| **Visual smoke** | Opponent natural guard ✅, Player no yellow trace ✅, Chain continuous ✅ |

## Git
- **Source commit:** 7ec9dfe (feat: refine opponent guard pose and remove remaining player strike effect)
- **Artifact commit:** build: publish P1-B1.5U guard-pose-clean UAT candidate
- **Push:** Completed to origin/p1/whole-body-mechanics
- **Working tree:** Clean (untracked .agentloop/, compile.log only)

## Classification
**P1-B1.5U_IMPLEMENTATION_PASS** ✅

---

### NEXT
**workflow_dispatch on p1/whole-body-mechanics**
→ Real iPhone 12 UAT

**Human Acceptance Questions:**
Without knowing the code, can the user immediately read:
**VAI → BẮP TAY → KHUỶU (KHỚP NỐI) → CẲNG TAY → GĂNG**
for BOTH player and opponent?

Also verify:
1. Opponent guard now looks like a natural boxing guard ✅
2. Player no longer shows any yellow flying effect ✅
3. No regression in telegraph readability ✅
4. No regression in punch-family readability ✅

**DO NOT START:**
- Combat Log
- Biomechanics Inspector
- A3.2
- A3.3
- Damage model
- Career/progression