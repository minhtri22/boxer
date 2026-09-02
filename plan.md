# Boxer — Proof-First Project Plan

## 0. Operating Rule

Boxer follows one non-negotiable development rule:

> **PROVE → DECIDE → IMPLEMENT**

Every phase must define:

1. The hypothesis being tested.
2. Why the hypothesis matters.
3. The smallest experiment capable of testing it.
4. What evidence must be collected.
5. Explicit PASS / FAIL criteria.
6. The decision unlocked by a PASS.

No later-stage feature is allowed to justify skipping an earlier proof gate.

---

# Phase 0 — POV Embodied Control Proof

## Goal

Prove that a mobile player can experience first-person boxing using a compact embodied control model:

- **Left thumb = footwork**
- **Right thumb = punches**
- **Device movement = head movement / evasive movement**
- **No active input = return to guard**

The desired outcome is not merely that commands can be recognized. The desired outcome is that the player learns a boxing-like motor loop:

```text
SEE → READ → MOVE / GUARD / EVADE → CREATE OPENING → COUNTER → RESET
```

## Phase 0 Questions

### P0-A — Sensor Feasibility
Can current iOS/Android motion sensors produce stable, low-latency relative orientation suitable for head movement?

**Status:** Foundation evidence supports feasibility. Device validation still required.

### P0-B — Signal Separation
Can intentional head movement be distinguished from:

- natural hand jitter,
- touch-induced device motion,
- posture drift,
- accidental rotation?

**Status:** Synthetic feasibility is promising; real-device proof required.

### P0-C — Simultaneous Control
Can a player move, evade, and punch concurrently without gesture conflict?

Example target sequence:

```text
left-thumb retreat
+ device slip left
+ right-thumb counter
```

### P0-D — POV Perception
Can the player correctly perceive:

- punch direction,
- attack timing,
- distance/range,
- defensive openings,
- ring pressure,
- return-to-center state?

### P0-E — Embodied Boxing Feel
Does device motion feel like moving the fighter's head rather than triggering a dodge command?

### P0-F — Comfort
Does repeated physical phone movement remain usable without excessive:

- motion sickness,
- fatigue,
- loss of screen visibility,
- accidental input,
- grip instability?

## Authorized Implementation

Only a **Sensor Combat Harness** is authorized during Phase 0.

The harness is an experimental measurement tool, not the production combat engine.

It should contain only what is necessary to test the hypotheses:

- first-person camera,
- two placeholder player gloves,
- one opponent mannequin,
- basic punch telegraphs and trajectories,
- head hitbox,
- device orientation input,
- left/right touch input zones,
- neutral calibration,
- minimal guard state,
- event logging,
- metric capture.

No production art, economy, career progression, character creation, online multiplayer, or content pipeline should be built during this phase.

## Phase 0 Required Experiments

1. **Pure Dodge Test**
   - Opponent throws scripted jabs/crosses.
   - Player may only use device movement.
   - Measure direction accuracy, reaction time, miss/hit result, overshoot, return-to-center.

2. **Touch Interference Test**
   - Player performs continuous movement and punch gestures.
   - Opponent does not attack.
   - Measure false head-movement activations and camera instability.

3. **Slip-Counter Test**
   - Opponent telegraphs a known cross.
   - Player evades with device movement and counters with right-thumb input.
   - Measure intentional sequence success.

4. **Unknown Sequence Test**
   - Opponent mixes jab, cross, hook, and body attack.
   - Player is not told the sequence.
   - Measure whether behavior is based on visual reading rather than memorization.

5. **Three-Minute Motor Learning Test**
   - After a short tutorial, player completes an unscripted three-minute session.
   - Observe whether `move → guard → evade → counter → reset` emerges naturally.

## Phase 0 Hard Gate

Target acceptance criteria:

| Metric | PASS target |
| --- | ---: |
| Intentional evade detection | ≥ 90% |
| False evade activation | ≤ 5% |
| Median motion-to-visual latency | ≤ 60 ms |
| Correct left/right evade mapping | ≥ 95% |
| Slip-counter success after tutorial | ≥ 70% |
| Players understanding controls within 2 min | ≥ 80% |
| Players choosing to replay another bout | ≥ 70% |
| Embodied-control rating | ≥ 4/5 |
| Significant motion discomfort | ≤ 10% |
| Additional mandatory dodge button | Not required |

Thresholds may only be changed if the protocol documents why the original threshold was invalid. They must not be tuned merely to turn a failing experiment into a passing result.

## Phase 0 Decision

Possible outcomes:

### PASS
The embodied control architecture is feasible and produces convincing boxing behavior.

Unlock: Phase 1.

### CONDITIONAL PASS
The central model works but one isolated subsystem requires redesign, for example head-motion filtering or guard behavior.

Unlock: only the minimum follow-up proof required to resolve the identified issue.

### FAIL
The interaction does not become readable, comfortable, or boxing-like.

Action: redesign or abandon the control hypothesis before production work begins.

---

# Phase 1 — Combat Foundation

**Locked until Phase 0 PASS.**

Candidate proof areas:

- distance and range model,
- punch trajectories,
- hit detection,
- guard geometry,
- body vs head defense,
- stamina,
- recovery frames,
- counter windows,
- knockdown,
- basic opponent behavior.

Primary question:

> Can the proven controls support a deep boxing exchange rather than gesture spam?

---

# Phase 2 — Opponent Intelligence & Boxing Styles

**Locked until Phase 1 PASS.**

Candidate proof areas:

- opponent telegraph readability,
- adaptive behavior,
- pressure fighter,
- out-boxer,
- counter puncher,
- power puncher,
- difficulty without stat inflation.

Primary question:

> Can different opponents force genuinely different boxing decisions?

---

# Phase 3 — Career Loop

**Locked until Phase 2 PASS.**

Candidate systems:

- fight selection,
- rankings,
- purses,
- training camps,
- recovery,
- injuries,
- coaches,
- gyms,
- equipment,
- progression through fight tiers.

Primary question:

> Does the career layer strengthen the next fight rather than distract from boxing?

---

# Phase 4 — Fighter Identity & World Progression

**Locked until Phase 3 PASS.**

Candidate systems:

- nationality,
- appearance,
- hair and facial features,
- height,
- weight,
- reach,
- stance,
- weight classes,
- walkouts,
- fight posters,
- championship presentation,
- arena ladder from street to world-title venues.

Primary question:

> Does player identity remain meaningful in a game whose primary gameplay camera is first person?

---

# Phase 5 — Online PvP Feasibility

**Locked until the offline game is proven.**

Required proofs before production multiplayer:

- acceptable latency envelope,
- authoritative hit validation,
- prediction/rollback strategy if needed,
- fair matchmaking,
- stat normalization,
- anti-cheat assumptions,
- disconnect handling,
- pay-to-win prevention.

Primary question:

> Can timing-based POV boxing remain fair and readable under real mobile network conditions?

---

# Scope Guard

The following are explicitly **out of scope before their proof gate**:

- large-scale character creator,
- large arena catalog,
- cosmetic store,
- hospital system,
- lifestyle simulation,
- sponsor system,
- live-service economy,
- PvP matchmaking,
- guild/social systems,
- production backend,
- monetization implementation.

These ideas remain part of the product vision, not current implementation scope.

---

# Current Project State

**Current phase:** Phase 0  
**Current decision:** PROVE  
**Production implementation:** BLOCKED  
**Authorized next artifact:** Sensor Combat Harness specification and experimental implementation  
**Next gate:** Human-on-device validation of embodied POV control
