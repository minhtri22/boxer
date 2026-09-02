# Boxer — Master Proof-First Project Plan

## 0. Project Operating Rule

Boxer follows one non-negotiable development rule:

> **PROVE → DECIDE → IMPLEMENT**

No production feature is authorized merely because it is attractive, marketable, technically possible, or part of the long-term vision.

Every phase must define:

1. The hypothesis being tested.
2. Why the hypothesis matters.
3. The smallest experiment capable of testing it.
4. What evidence must be collected.
5. Explicit PASS / CONDITIONAL PASS / FAIL criteria.
6. The decision unlocked by a PASS.
7. What remains explicitly locked.

A phase is not complete because a prototype looks impressive. It is complete only when the evidence supports the hypothesis strongly enough to unlock the next phase.

---

# 1. Product Thesis

Boxer is a **first-person boxing career simulator** built around an embodied-control thesis:

> **Do not control a boxer. Be the boxer.**

Frozen Phase 0 control hypothesis:

- **Phone = Head** — physical device movement controls head movement/evasion.
- **Left Thumb = Feet** — left-thumb gestures control footwork.
- **Right Thumb = Fists** — right-thumb gestures express punch intent.
- **No active action = Return to Guard** — the boxer naturally returns to defensive stance.

The target boxing loop is:

```text
SEE → READ → MOVE / GUARD / EVADE → CREATE OPENING → COUNTER → RESET
```

The anti-goal is:

```text
SWIPE → SWIPE → SWIPE → SPAM → WIN
```

Reference documents:

- `docs/foundation/product-thesis.md`
- `docs/protocols/phase-0-pov-embodied-control-proof.md`
- `docs/protocols/phase-0-feedback-energy-proof.md`

---

# 2. Evidence and Repository Rules

## 2.1 Evidence-first repository structure

All proof work should eventually follow a structure similar to:

```text
docs/
  foundation/
  protocols/
  result/

experiments/
  phase0/
  phase1/
  ...

evidence/
  phase0/
  phase1/
  ...
```

Exact folders may evolve, but experimental outputs must remain reproducible and attributable to a build/commit.

## 2.2 Required experiment provenance

Every proof report must record at minimum:

```text
Experiment name
Protocol version
Git commit/build
Device
OS/version
Refresh rate
Participant/test ID
Frozen thresholds
Raw evidence path
Result
PASS / CONDITIONAL PASS / FAIL
Reason
Next authorized action
```

## 2.3 Threshold discipline

Thresholds must be frozen before the experiment whenever practical.

Do not tune a failing threshold merely to obtain PASS.

If a criterion is changed, record:

1. original criterion,
2. observed problem,
3. why the metric was invalid or misleading,
4. revised criterion,
5. whether revision occurred before rerun.

## 2.4 Commit discipline

Preferred workflow for each phase:

1. Commit protocol/specification.
2. Implement only the authorized experimental artifact.
3. Run QA and collect evidence.
4. Write result report.
5. Decide PASS / CONDITIONAL PASS / FAIL.
6. Commit evidence/result documentation.
7. Only after PASS, update `plan.md` to unlock the next phase.

Do not mix speculative next-phase production work into a proof commit.

---

# Phase 0 — POV Embodied Control Proof

## Status

**ACTIVE**

Production game implementation remains **BLOCKED**.

## Goal

Prove that a mobile player can use touch plus physical device motion in first-person POV to produce readable, comfortable, boxing-like behavior.

This phase is not intended to prove the entire combat system. It proves the embodied interface on which later combat depends.

---

## P0.1 — Sensor and Motion Feasibility

### Question

Can iOS/Android device-motion sensors provide stable, low-latency relative orientation suitable for continuous head movement?

### Required proof

- neutral calibration,
- relative roll/orientation tracking,
- angular velocity capture,
- dead-zone behavior,
- saturation behavior,
- frame-by-frame sensor update stability.

### Status

Technical feasibility is supported strongly enough to build the experimental harness, but real-device validation is still required.

---

## P0.2 — Signal Separation

### Question

Can intentional head movement be separated from:

- natural hand jitter,
- grip drift,
- left-thumb movement,
- right-thumb punch gestures,
- simultaneous two-thumb activity,
- haptic-generated vibration?

### PASS targets

- intentional evade detection: **≥ 90%**
- false evade activation: **≤ 5%**
- left/right direction correctness: **≥ 95%**

Synthetic evidence is useful for tuning candidates but cannot substitute for human-on-device data.

---

## P0.3 — Continuous Head Geometry

### Question

Does device movement physically move the virtual head out of punch trajectories rather than merely triggering a dodge animation?

### Required implementation principle

```text
punch trajectory ∩ head hitbox = HIT
punch trajectory ∩ guard = BLOCK
no intersection = MISS
```

The harness must not use a discrete `dodged=true` flag as the source of truth.

### Candidate motion range

Initial experimental values only:

- neutral dead zone: approximately `±2°`
- active range: approximately `2°–12°`
- saturation: approximately `12°–15°`

These are not production constants.

---

## P0.4 — Simultaneous Embodied Control

### Question

Can footwork, head movement, and attack input operate concurrently without unacceptable conflict?

Target sequence:

```text
left-thumb retreat
+ physical device slip
+ right-thumb counter
```

### Required observation

The player should remain focused on the opponent rather than on remembering input syntax.

---

## P0.5 — POV Readability

### Question

Can the player perceive:

- incoming punch direction,
- timing,
- distance/range,
- head vs body threat,
- guard state,
- opponent pressure,
- return-to-center state,

without depending on large directional warning UI?

If the game only works with intrusive arrows or command prompts, this proof remains unresolved.

---

## P0.6 — Embodied Boxing Feel

### Question

Does the player think:

> "I moved my head and made the punch miss"

rather than:

> "I triggered the dodge command"?

### PASS targets

- controls understood within 2 minutes: **≥ 80% of participants**
- slip-counter success after tutorial: **≥ 70%**
- embodied-control rating: **≥ 4/5**
- voluntary immediate replay: **≥ 70%**

---

## P0.7 — Comfort and Physical Usability

### Question

Does repeated device motion remain comfortable and visually usable?

Measure:

- wrist fatigue,
- grip fatigue,
- difficulty seeing the opponent while moving the phone,
- nausea/dizziness,
- exaggerated-motion requirement,
- fear of dropping the phone,
- neutral drift over time.

### PASS target

Significant motion discomfort: **≤ 10% of participants**.

---

## P0.8 — Haptic Impact Proof

### Hypothesis

Haptic feedback improves impact perception and punch-weight readability without creating excessive noise, fatigue, energy cost, or motion-sensor corruption.

### Required behavior candidates

- light contact / jab → short light haptic,
- clean cross → stronger short haptic,
- heavy hook / major impact → strongest bounded haptic,
- guard impact → distinct reduced feedback,
- knockdown → exceptional impact profile.

Principle:

> **Feedback must be proportional but sparse.**

Haptics must never become continuous vibration during active exchanges.

### Required sensor rule

Every game-generated haptic event must be timestamped so sensor analysis can distinguish self-generated vibration from intentional player motion.

### Required settings

At minimum:

- Haptics: `Off / Low / Normal / Strong`

The proof must confirm that `Off` removes haptic cost without breaking gameplay readability.

---

## P0.9 — Audio Feedback Proof

### Hypothesis

Audio increases punch readability and immersion but must remain optional.

Candidate layers:

1. **Impact** — glove, guard, body and head contact.
2. **Body state** — breathing, heartbeat, muffled hearing after major hits.
3. **Environment** — crowd, coach, announcer, venue ambience.

### Constraint

Audio must not be the only channel communicating essential combat information.

### Required settings

At minimum:

- Sound Effects: `On / Off`
- Crowd/Ambience: `Off / Low / Full`

---

## P0.10 — Battery and Thermal Proof

### Question

What is the real battery and thermal cost of the immersive feedback stack on representative devices?

### Required configurations

At minimum compare:

1. **Full Immersion** — haptic + audio + normal visual impact.
2. **No Haptic** — audio + normal visual impact.
3. **No Audio** — haptic + normal visual impact.
4. **Battery Saver** — haptic off or reduced, reduced visual/FPS budget, optional reduced ambience.

### Collect

- test duration,
- starting/ending battery level where measurable,
- OS thermal state where exposed,
- frame-time stability,
- FPS behavior,
- device surface-temperature observation where practical,
- haptic event count,
- audio mode,
- visual/FPS mode,
- sensor false-evade rate,
- participant immersion rating.

### Decision principle

Do not assume haptic or audio is the dominant energy cost. Measure the complete rendering + feedback stack on device.

Battery Saver must be a real operating mode, not a cosmetic toggle.

---

# Phase 0 Authorized Artifact — Sensor Combat Harness

Only this experimental artifact is authorized.

Minimum required components:

- first-person camera,
- two placeholder player gloves,
- one opponent mannequin,
- player head hitbox,
- minimal body hit regions,
- scripted opponent punch trajectories,
- device attitude/orientation input,
- angular velocity where available,
- left/right touch zones,
- neutral calibration,
- bounded continuous head displacement,
- minimal high-guard state,
- HIT / MISS / BLOCK / COUNTER outcomes,
- haptic-event timestamps,
- selectable haptic intensity,
- selectable audio mode,
- optional battery-saver mode,
- debug overlays,
- timestamped sensor/input/event logs,
- frame-time logging.

### Explicitly forbidden during Phase 0

Do not build:

- production characters,
- polished arenas,
- crowd simulation,
- career mode,
- fighter creator,
- inventory/shop,
- coach system,
- hospital system,
- economy,
- live-service systems,
- networking,
- multiplayer,
- backend,
- monetization.

---

# Phase 0 Required Experiment Set

## Test A — Calibration and Noise Baseline

Measure natural hold/touch noise and neutral drift.

## Test B — Intentional Left/Right Motion

Measure detection, direction accuracy, latency, overshoot and return-to-neutral.

## Test C — Pure Dodge

Player may only move the phone/head against scripted attacks.

## Test D — Touch Interference

Player attacks and moves while opponent is passive; measure accidental head movement.

## Test E — Slip → Counter

Measure the first meaningful boxing exchange.

## Test F — Move + Evade + Counter

Prove simultaneous feet/head/fist control.

## Test G — Unknown Attack Sequence

Determine whether player reads the opponent instead of memorizing sequence.

## Test H — Three-Minute Motor Learning Trial

Observe whether boxing-like behavior emerges after a short tutorial.

## Test I — Comfort Trial

Minimum repeated-use session after control understanding.

## Test J — Haptic A/B

Compare impact recognition and perceived punch weight with haptic ON vs OFF.

## Test K — Haptic Sensor Interference

Determine whether generated vibration causes false dodge/head displacement.

## Test L — Audio A/B

Compare impact/readability/immersion with audio ON vs OFF.

## Test M — Energy/Thermal Matrix

Compare Full Immersion / No Haptic / No Audio / Battery Saver under the same workload.

---

# Phase 0 Hard Gate

Phase 0 cannot PASS unless the central embodied-control targets are satisfied.

| Metric | PASS target |
| --- | ---: |
| Intentional evade detection | ≥ 90% |
| False evade activation | ≤ 5% |
| Median motion-to-visual latency | ≤ 60 ms |
| Correct left/right mapping | ≥ 95% |
| Slip-counter success after tutorial | ≥ 70% |
| Controls understood within 2 min | ≥ 80% |
| Immediate replay intent | ≥ 70% |
| Embodied-control rating | ≥ 4/5 |
| Significant motion discomfort | ≤ 10% |
| Mandatory additional dodge button | Not required |

Haptic/audio/energy tests do not need to prove that every immersive feature must be enabled by default. They must determine which modes are safe, useful, efficient, and optional.

## Phase 0 decision outcomes

### PASS

Embodied control is proven strongly enough to begin Combat Foundation.

Unlock: **Phase 1 only**.

### CONDITIONAL PASS

Core embodied mapping is supported but an isolated technical problem remains.

Examples:

- filtering,
- calibration,
- head-range scaling,
- haptic interference,
- feedback intensity,
- energy mode configuration.

Unlock: only the targeted follow-up proof.

### FAIL — REDESIGN

Players understand the idea but the mapping causes unacceptable conflict, readability, comfort, or control problems.

Action: redesign Phase 0 control hypothesis and rerun.

### FAIL — STOP

Physical-device head control provides insufficient embodied value or creates unacceptable usability cost.

Action: do not compensate by building more game systems.

---

# Phase 1 — Combat Foundation

## Status

**LOCKED UNTIL PHASE 0 PASS**

## Goal

Prove that the embodied controls support a deep boxing exchange rather than gesture spam.

## Proof areas

- distance/range,
- jab/cross/hook/uppercut taxonomy,
- punch trajectories,
- head/body hit geometry,
- high/low guard logic,
- stamina,
- recovery,
- balance,
- counter windows,
- body vs head damage,
- knockdown,
- get-up interaction,
- basic round structure.

## Core question

> Can timing, range, defense and counters matter more than raw swipe frequency?

## Exit evidence

A short fight should repeatedly produce intentional sequences such as:

```text
probe with jab
→ manage distance
→ read attack
→ evade/block
→ counter
→ reset
```

without requiring production graphics.

---

# Phase 2 — Opponent Intelligence and Boxing Styles

## Status

**LOCKED UNTIL PHASE 1 PASS**

## Goal

Prove that different opponents create meaningfully different boxing decisions.

## Candidate styles

- pressure fighter,
- out-boxer,
- counter puncher,
- power puncher,
- defensive specialist,
- orthodox/southpaw variants where justified.

## Proof areas

- readable telegraphs,
- adaptive attack selection,
- distance preference,
- counter behavior,
- fatigue behavior,
- difficulty without simple HP/stat inflation,
- exploitable strengths and weaknesses.

## Core question

> Can the player identify an opponent's style and adapt tactically?

---

# Phase 3 — Minimal Career Loop

## Status

**LOCKED UNTIL PHASE 2 PASS**

## Goal

Prove that a lightweight career layer strengthens the meaning of each fight.

## Candidate minimal loop

```text
choose fight
→ prepare/train
→ fight
→ win/loss/injury
→ money/ranking change
→ recovery
→ next decision
```

## Candidate systems

- fight selection,
- rankings,
- purses,
- basic expenses,
- training camp choices,
- recovery,
- injuries,
- first coach effects,
- first gym effects,
- limited equipment choices.

## Constraint

Every system must answer:

> How does this change the next fight or the meaning of the career?

If it cannot answer this, defer or remove it.

## Core question

> Does the career layer make the player care more about the next bout?

---

# Phase 4 — Fighter Identity and World Progression

## Status

**LOCKED UNTIL PHASE 3 PASS**

## Goal

Prove that player identity remains valuable despite POV gameplay.

## Candidate systems

- nationality,
- fighter name,
- face/hair features,
- height,
- weight,
- reach,
- stance,
- weight class,
- clothing/gloves,
- walkout presentation,
- mirrors,
- weigh-ins,
- profile cards,
- fight posters,
- replay/highlight presentation.

## World ladder candidate

1. Street / underground
2. Amateur
3. Regional professional
4. National contender
5. International contender
6. Championship level

Possible environments may include:

- street/parking areas,
- improvised underground venues,
- cages/club spaces,
- local halls,
- casinos/regional arenas,
- national stadiums,
- championship arenas.

## Core question

> Does identity and venue progression create an earned rise-from-nobody fantasy?

---

# Phase 5 — Career Depth

## Status

**LOCKED UNTIL PHASE 4 PASS**

## Candidate proof areas

- deeper coaches,
- gym progression,
- injury consequence,
- medical treatment,
- hospital presentation,
- nutrition/weight management,
- contracts,
- promoters/managers,
- rivals/rematches,
- sponsors,
- travel,
- optional lifestyle presentation.

## Constraint

Avoid turning Boxer into a generic life simulator.

Career depth exists to strengthen:

- fight preparation,
- identity,
- consequence,
- progression,
- emergent story.

---

# Phase 6 — MVP Production Proof

## Status

**LOCKED UNTIL CORE CAREER IS PROVEN**

## Goal

Convert proven systems into the smallest production-quality vertical slice that can validate product appeal.

## Candidate MVP slice

- one player boxer,
- limited identity customization,
- small opponent roster,
- small arena ladder,
- proven combat controls,
- one lightweight career loop,
- training/recovery subset,
- haptic/audio/battery settings,
- production-grade telemetry,
- stable performance on target devices.

## MVP success questions

- Do players want another fight?
- Do they continue the career?
- Does POV remain the defining feature after novelty wears off?
- Does the game run acceptably on the target mobile hardware envelope?

---

# Phase 7 — Online PvP Feasibility

## Status

**LOCKED UNTIL OFFLINE MVP IS PROVEN**

## Required proofs

- acceptable network-latency envelope,
- authoritative hit validation,
- prediction/rollback strategy if needed,
- fair matchmaking,
- stat normalization,
- anti-cheat assumptions,
- disconnect handling,
- device-performance variance,
- pay-to-win prevention.

## Core question

> Can timing-based POV boxing remain fair and readable under real mobile-network conditions?

PvP is an end-state candidate, not a prerequisite for proving Boxer.

---

# Phase 8 — Live Product and Expansion

## Status

**NOT AUTHORIZED**

Only considered after MVP and, if pursued, PvP feasibility are proven.

Potential areas:

- larger career world,
- new cities/arenas,
- deeper fighter customization,
- additional boxing styles,
- live events,
- seasonal competition,
- cosmetics,
- social systems,
- monetization.

All monetization must preserve boxing skill as the primary determinant of competitive performance.

---

# 3. Energy / Accessibility / Settings Baseline

The product must support users who prioritize battery life, thermal comfort, accessibility, or quiet play.

Candidate settings baseline:

```text
Haptics: Off / Low / Normal / Strong
Sound Effects: Off / On
Crowd/Ambience: Off / Low / Full
Camera Impact: Reduced / Normal
Motion Blur: Off / On
Frame Rate: 30 / 60 / High where supported
Power Mode: Full Immersion / Balanced / Battery Saver
```

Exact production settings remain subject to proof.

Gameplay must remain understandable with haptic and audio disabled.

---

# 4. Scope Guard

The following remain explicitly out of scope before their proof gate:

- large-scale character creator,
- large arena catalog,
- cosmetic store,
- deep hospital system,
- lifestyle simulation,
- sponsor ecosystem,
- live-service economy,
- PvP matchmaking,
- guild/social systems,
- production backend,
- monetization implementation.

These remain product possibilities, not implementation commitments.

---

# 5. Current Project State

**Repository:** `minhtri22/boxer`  
**Current phase:** `Phase 0 — POV Embodied Control Proof`  
**Current decision state:** `PROVE`  
**Production implementation:** `BLOCKED`  
**Authorized implementation:** `Sensor Combat Harness only`  
**Primary protocol:** `docs/protocols/phase-0-pov-embodied-control-proof.md`  
**Feedback/energy protocol:** `docs/protocols/phase-0-feedback-energy-proof.md`  
**Next engineering artifact:** `Phase 0 Sensor Combat Harness Specification`  
**Next evidence gate:** `Human-on-device validation`  

---

# 6. Immediate Next Actions

Only the following sequence is currently authorized:

1. Write the **Sensor Combat Harness Specification**.
2. Freeze the first device/orientation/touch/logging design.
3. Choose the smallest mobile technical stack capable of collecting valid sensor data.
4. Implement the harness without production art or career systems.
5. Run developer/device QA.
6. Run Phase 0 human-on-device tests.
7. Run haptic/audio/energy experiments.
8. Produce raw evidence and Phase 0 result report.
9. Decide `PASS / CONDITIONAL PASS / FAIL`.
10. Update this plan only after evidence determines what is unlocked.

Until step 9 produces PASS, **Phase 1 remains locked**.
