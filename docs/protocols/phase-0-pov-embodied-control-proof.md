# Phase 0 — POV Embodied Control Proof Protocol

## 1. Purpose

This protocol exists to determine whether the core Boxer control model is viable **before** production implementation begins.

The experiment must answer one question:

> Can a player use mobile touch plus physical device motion in first-person POV to produce readable, comfortable, boxing-like behavior?

This phase is a proof exercise, not a game-development milestone.

---

## 2. Frozen Phase 0 Control Hypothesis

The baseline hypothesis for the first test cycle is:

- **Left thumb gesture → footwork**
- **Right thumb gesture → punch intent**
- **Physical phone orientation/motion → head movement / evasion**
- **No active action → return to high guard**

Internal mnemonic:

> **Phone = Head · Left Thumb = Feet · Right Thumb = Fists**

The important claim is not merely that these inputs can be detected. The claim is that the mapping can become intuitive enough for the player to stop thinking about commands and start reacting to boxing situations.

---

## 3. What Is Already Plausible vs What Remains Unproven

### Supported enough to justify experimentation

- Modern mobile devices expose orientation/motion data suitable for real-time interaction.
- Touch input and device-motion input can be sampled concurrently.
- A motion detector can, in principle, distinguish intentional movement from small sensor/hand noise if filtering, dead zones, and thresholds are appropriate.

### Not proven

- Players will interpret device movement as movement of their own head.
- Motion will remain stable while both thumbs are active.
- The POV view will preserve sufficient spatial awareness.
- Incoming punches will remain readable without intrusive warning UI.
- Guard, evasion, movement, and punching will combine into a boxing loop rather than gesture spam.
- Repeated phone movement will remain physically comfortable.

No document may mark Phase 0 complete until these unproven items are tested on real devices with human participants.

---

## 4. Experimental Artifact: Sensor Combat Harness

### 4.1 Purpose

The harness is a measurement instrument used to test the control hypothesis.

It is explicitly **not** the production combat engine.

### 4.2 Required components

The minimum harness should include:

- first-person camera,
- placeholder player gloves,
- one opponent mannequin,
- head hitbox,
- minimal torso/body hit regions,
- scripted opponent punch trajectories,
- device attitude/orientation capture,
- angular velocity capture if available,
- left and right touch regions,
- neutral device calibration,
- head-position visualization/debug overlay,
- touch trace visualization/debug overlay,
- event log,
- timestamped sensor/input log,
- outcome log for HIT / MISS / BLOCK / COUNTER.

### 4.3 Explicit exclusions

Do not implement during this proof:

- polished characters,
- production arena art,
- crowd systems,
- career mode,
- fighter customization,
- equipment inventory,
- shop,
- hospital,
- coach system,
- progression economy,
- networking,
- multiplayer,
- backend,
- monetization.

---

## 5. Device-Motion Model

### 5.1 Core behavior

The default model should be **continuous relative head movement**, not a discrete dodge command.

Desired behavior:

```text
neutral device orientation
        ↓
virtual head at centerline

rotate/lean device left
        ↓
virtual head shifts left within bounded range

return device to calibrated neutral
        ↓
virtual head returns toward centerline
```

The first proof should prioritize left/right head movement. Pitch-based duck/weave motion is not part of the mandatory baseline and should only be introduced after left/right control is stable.

### 5.2 Relative calibration

At the beginning of a test:

1. Player adopts a comfortable normal phone-holding posture.
2. Harness records this orientation as neutral.
3. Subsequent motion is measured relative to this neutral state.

Do not assume a universal absolute phone angle.

### 5.3 Dead zone

A small dead zone is required to ignore natural hand instability.

Initial candidate range for experimentation:

- neutral dead zone: approximately `±2°`
- active range: approximately `2°–12°`
- saturation: approximately `12°–15°`

These are starting hypotheses, not product constants.

### 5.4 Detection philosophy

Intentional movement may be inferred from a combination of:

- relative orientation displacement,
- angular velocity,
- direction consistency,
- duration,
- return-to-neutral behavior.

The detector should not interpret every accelerometer spike as a dodge.

### 5.5 Required noise classes

Tests must include:

1. normal hold jitter,
2. posture drift,
3. left-thumb drag,
4. right-thumb punch swipe,
5. simultaneous two-thumb input,
6. intentional left movement,
7. intentional right movement.

---

## 6. Touch Model

The exact production gesture grammar is not frozen in Phase 0. The harness needs only enough touch vocabulary to test simultaneous embodied control.

### 6.1 Left region — movement

Minimum required test actions:

- advance,
- retreat,
- lateral left,
- lateral right.

### 6.2 Right region — attack

Minimum required test actions:

- fast straight attack,
- power straight or hook-like attack.

The harness does not need the final jab/cross/hook/uppercut taxonomy unless that distinction is required by a specific experiment.

### 6.3 Idle state

When no conflicting action is active, the gloves return to high guard.

This guard must not equal invulnerability.

For Phase 0, the simplest valid model is:

- head-level straight attacks can be blocked by a stable guard,
- body attacks can bypass a purely high guard,
- repeated block impact may create stamina/guard pressure if necessary for the experiment.

---

## 7. Head and Punch Geometry

To test embodied evasion honestly, hit/miss outcomes should come from geometry rather than from a discrete `dodged=true` flag.

Minimum model:

- opponent punch follows a defined trajectory,
- player's head has a collision region,
- device motion changes the position of that region,
- collision → HIT,
- no collision → MISS,
- guard intersection may produce BLOCK.

This is central to the hypothesis. If device motion merely triggers an animation with invulnerability frames, Phase 0 does not prove that the phone functions as the player's head.

---

## 8. Required Test Scenarios

# Test A — Calibration and Noise Baseline

### Goal
Measure natural sensor and grip noise.

### Procedure

Player holds the device normally for 60 seconds while:

1. doing nothing,
2. moving only the left thumb,
3. moving only the right thumb,
4. using both thumbs.

### Collect

- relative roll distribution,
- angular velocity distribution,
- drift over time,
- false evade events,
- touch-induced motion.

### PASS target

False intentional evade activation: **≤ 5%**.

---

# Test B — Intentional Left/Right Head Movement

### Goal
Determine whether intentional phone movement is reliably identified.

### Procedure

Random prompts request LEFT or RIGHT movements while the participant maintains normal grip.

Minimum suggested sample per participant:

- 20 LEFT,
- 20 RIGHT.

### Collect

- detection rate,
- direction accuracy,
- initiation latency,
- peak angle,
- peak angular velocity,
- overshoot,
- return-to-center time.

### PASS targets

- intentional evade detection: **≥ 90%**
- direction correctness: **≥ 95%**

---

# Test C — Pure Dodge

### Goal
Determine whether physical device motion can make incoming attacks miss in POV.

### Procedure

Opponent throws telegraphed straight attacks.

Player is not allowed to punch or move with the left thumb. The only active defense is device head movement.

### Collect

- successful misses,
- hits,
- wrong-direction movements,
- reaction latency,
- head displacement at impact time.

### Observation question

Does the player report:

> "I moved my head out of the punch"

or instead:

> "I triggered a dodge"?

The first response supports the embodied-control thesis more strongly.

---

# Test D — Touch Interference

### Goal
Determine whether active touch corrupts head control.

### Procedure

Player continuously performs movement and attack gestures while the opponent remains passive.

### Collect

- false head movement,
- camera displacement caused by thumb activity,
- grip instability,
- accidental saturation,
- neutral drift.

### PASS target

False evade activation: **≤ 5%**.

---

# Test E — Slip → Counter

### Goal
Test the first meaningful boxing sequence.

### Procedure

Opponent telegraphs a predictable straight power punch.

Player must:

1. read attack,
2. physically move device/head out of trajectory,
3. counter using right-thumb input,
4. return to defensive state.

### Collect

- successful evade,
- successful counter,
- total sequence success,
- latency between miss and counter,
- accidental attack before evade completion.

### PASS target

After short tutorial, successful intentional slip-counter sequence: **≥ 70%**.

---

# Test F — Move + Evade + Counter

### Goal
Prove simultaneous lower-body, head, and hand control.

### Target sequence

```text
left-thumb retreat
+ physical device slip
+ right-thumb counter
```

### Collect

- input overlap,
- conflicts,
- missed gestures,
- accidental device motion,
- user-reported cognitive load.

This test is particularly important because the design thesis depends on different physical channels operating concurrently.

---

# Test G — Unknown Attack Sequence

### Goal
Determine whether the player is reading the opponent rather than memorizing the experiment.

### Opponent actions

Randomized mixture of:

- jab-like straight,
- cross-like straight,
- hook-like head attack,
- body attack,
- pauses/feints if required.

### Collect

- correct defensive choice,
- reaction time,
- unnecessary evades,
- attack-reading accuracy,
- body-vs-head defensive mistakes.

If the player needs large explicit directional warning arrows to succeed, POV attack readability should be considered unresolved.

---

# Test H — Three-Minute Motor Learning Trial

### Goal
Determine whether the control vocabulary becomes behavior rather than conscious command recall.

### Procedure

After a tutorial of no more than two minutes, participant plays a three-minute unscripted bout.

Do not provide new control instruction during the bout.

### Observe

Whether the player naturally produces combinations such as:

```text
move
→ guard
→ read
→ evade
→ counter
→ reset
```

and whether panic behavior collapses into uncontrolled repeated swiping.

---

# Test I — Comfort Trial

### Goal
Measure physical and visual usability.

### Minimum duration

10 minutes of repeated use after the participant understands the controls.

### Ask about

- wrist fatigue,
- grip fatigue,
- difficulty seeing screen during device motion,
- nausea/dizziness,
- visual instability,
- fear of dropping device,
- whether movement must be exaggerated to register.

### PASS target

Significant motion discomfort: **≤ 10% of participants**.

Any severe discomfort event should be recorded separately and investigated even if the aggregate target passes.

---

## 9. Required Metrics

At minimum, log:

- sensor timestamp,
- input timestamp,
- calibrated orientation,
- relative orientation,
- angular velocity,
- touch region,
- touch gesture classification,
- virtual head position,
- opponent punch phase,
- impact timestamp,
- HIT / MISS / BLOCK result,
- counter timestamp,
- frame time,
- estimated motion-to-visual latency.

Participant-level summary should include:

- learning time,
- detection accuracy,
- false activation rate,
- direction accuracy,
- evade success,
- slip-counter success,
- replay intent,
- embodied-control rating,
- comfort rating.

---

## 10. Phase 0 Hard Acceptance Criteria

Phase 0 should not PASS unless the following targets are met or a documented review establishes that a metric itself was invalid before results were known.

| Metric | Required result |
| --- | ---: |
| Intentional evade detection | ≥ 90% |
| False evade activation | ≤ 5% |
| Median motion-to-visual latency | ≤ 60 ms |
| Left/right direction correctness | ≥ 95% |
| Slip-counter success after tutorial | ≥ 70% |
| Players learning controls in ≤2 min | ≥ 80% |
| Players voluntarily choosing another bout | ≥ 70% |
| Embodied-control rating | ≥ 4/5 |
| Significant motion discomfort | ≤ 10% |
| Mandatory extra dodge button | Not required |

### Anti-goal

Do not modify thresholds after seeing bad results simply to obtain a PASS.

If a threshold is changed, the report must state:

1. original threshold,
2. observed problem with the metric,
3. reason the metric was invalid or misleading,
4. revised criterion,
5. whether the revision was chosen before rerunning the experiment.

---

## 11. Qualitative Questions

After the test, ask participants without leading them toward expected answers:

1. What did moving the phone feel like it controlled?
2. How did you know when a punch was coming?
3. Did you think about gestures or about avoiding the opponent's punch?
4. Did you understand how far away the opponent was?
5. Did you ever move the phone accidentally because of thumb input?
6. Did any action feel physically awkward?
7. Did you feel more like you were operating a game or standing in a fight?
8. Would you immediately play another bout?
9. Which action felt least natural?
10. Did you ever stop looking at the opponent because you were thinking about controls?

Do not reduce these observations to a single satisfaction score.

---

## 12. Failure Modes to Watch

Phase 0 should explicitly look for these failure modes:

### F1 — Gesture Game
Player focuses on remembering swipes rather than opponent behavior.

### F2 — Free Guard
Doing nothing is safer than active boxing.

### F3 — False Dodge
Normal touch/grip motion repeatedly moves the virtual head.

### F4 — Exaggerated Motion
Player must swing or shake the whole phone excessively for reliable evasion.

### F5 — Lost Screen
Device motion causes the player to lose useful visual contact with the opponent.

### F6 — Camera Turret
Footwork does not create a sense of body position or range; the view feels like a stationary camera with hands.

### F7 — Telegraph UI Dependency
Player can only defend when directional warning graphics are displayed.

### F8 — Swipe Spam
Fast repeated attacks dominate reading, movement, and counterplay.

### F9 — Physical Fatigue
The interaction is novel for one minute but tiring over a normal bout.

### F10 — Motion Discomfort
Camera/head behavior produces nausea, dizziness, or excessive visual instability.

---

## 13. Decision Rules

### PASS

All central control assumptions are supported strongly enough to begin a separate Combat Foundation phase.

### CONDITIONAL PASS

The embodied mapping is supported, but one isolated technical issue remains, such as filtering, calibration, or head-range scaling.

Only a targeted follow-up proof is authorized.

### FAIL — REDESIGN

Players understand the core idea but the current mapping causes unacceptable conflict, comfort, or readability problems.

Return to control design and run a new Phase 0 variant.

### FAIL — STOP

Evidence shows that physical-device head control does not provide meaningful embodied value or creates unacceptable usability cost.

Do not compensate by building more game systems.

---

## 14. Reporting Template

Every Phase 0 experiment report should record:

```text
Experiment:
Date:
Build/commit:
Device:
OS/version:
Refresh rate:
Participant ID:

Hypothesis:
Protocol version:
Frozen thresholds:

Raw evidence paths:

Results:
- detection accuracy:
- false activation:
- direction accuracy:
- motion-to-visual latency:
- evade success:
- counter success:
- discomfort:
- replay intent:
- embodied rating:

Qualitative notes:

Unexpected observations:

PASS / CONDITIONAL PASS / FAIL:

Reason:

Next authorized action:
```

---

## 15. Phase 0 Exit Condition

Phase 0 ends only when evidence supports or rejects the embodied control thesis.

It does **not** end because:

- the simulator looks good,
- a single developer likes the controls,
- a championship arena has been modeled,
- punch animations have been completed,
- a feature roadmap is ready.

The exit question remains:

> **Does this control model make real players behave and feel like they are boxing in first person?**
