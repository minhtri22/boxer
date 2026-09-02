# Phase 0 Supplement — Haptic, Audio, Energy and Thermal Proof

## 1. Purpose

This supplement extends the Phase 0 POV Embodied Control Proof with one additional question:

> Can Boxer use haptic and audio feedback to increase perceived punch impact without creating unacceptable battery drain, thermal load, fatigue, or motion-sensor interference?

This is a proof protocol. It does not authorize production feedback implementation.

---

## 2. Hypotheses

### H-F1 — Haptic impact readability

Short, event-based haptic feedback helps players distinguish light, medium, heavy, blocked, and knockdown-class impacts.

### H-F2 — Haptic embodiment

Haptic feedback increases the player's sense that the impact happened to their own boxer rather than merely appearing on screen.

### H-F3 — Sensor coexistence

Game-generated haptic impulses can coexist with device-motion head control without causing unacceptable false evade/head-motion events.

### H-F4 — Audio contribution

Impact and body-state audio improve hit readability and perceived weight, but the game remains playable when audio is disabled.

### H-F5 — Energy proportionality

The incremental battery and thermal cost of haptics/audio is small enough relative to the experiential value they add, or can be controlled through player settings and lower-power presets.

---

## 3. Feedback Model Under Test

### 3.1 Haptic classes

Candidate classes:

| Event | Candidate feedback |
| --- | --- |
| Light jab / glancing contact | very short, light |
| Guard impact | short, muted |
| Clean straight | short, medium |
| Heavy hook / power shot | short, strong |
| Body shot | distinct short profile if platform supports it |
| Knockdown-class impact | strongest bounded profile |

The mapping must remain **proportional but sparse**.

Do not use continuous vibration.

Do not assume numerical damage maps linearly to haptic strength.

### 3.2 Audio layers

Minimum test layers:

1. impact sound,
2. guard impact sound,
3. body-state cue such as breathing or short muffled-hearing effect,
4. optional minimal ambience.

Audio must not be the only channel that communicates a gameplay-critical event.

---

## 4. Required Runtime Controls

The experimental harness should expose independent toggles or levels for:

### Haptics

- Off
- Low
- Normal
- Strong

### Audio

- Sound Effects On / Off
- Ambience Off / Low / Full

### Experimental operating presets

- **Full Immersion** — haptic + audio + normal visual impact feedback
- **Balanced** — reduced haptic / reduced impact FX + audio
- **Battery Saver** — haptic off, reduced visual/frame load, audio optional

Preset names are provisional.

---

## 5. Instrumentation Requirements

For each session log, at minimum:

- device model,
- OS/version,
- display refresh / target FPS,
- session start/end timestamp,
- feedback configuration,
- haptic event timestamp,
- haptic event class,
- audio event timestamp,
- motion-sensor samples around each haptic event,
- false head-motion / evade detections,
- frame time / dropped-frame counters,
- thermal state if the platform exposes it,
- battery level at start/end where reliable,
- session duration.

Where battery percentage is too coarse for short runs, use longer repeated trials or platform energy instrumentation rather than inventing precision.

---

## 6. Test A — Impact Recognition

### Goal

Determine whether players can distinguish impact strength/classes more accurately with haptics.

### Conditions

A. visual only

B. visual + haptic

Use randomized light / medium / heavy impacts.

### Collect

- correct impact-strength classification,
- confidence,
- perceived impact rating.

### Evidence sought

Haptic should materially improve classification and/or impact perception without creating confusion between classes.

---

## 7. Test B — Haptic Sensor Interference

### Goal

Determine whether haptic output corrupts the `Phone = Head` motion channel.

### Procedure

1. Hold neutral while receiving randomized light/medium/heavy haptic events.
2. Repeat while both thumbs are active.
3. Repeat while intentionally slipping left/right immediately before or after impact.

### Collect

- false head displacement,
- false evade activation,
- orientation spikes,
- angular-velocity spikes,
- missed intentional evades,
- calibration drift.

### PASS target

Haptic-induced false evade activation must remain within the Phase 0 false-activation budget: **≤ 5%**.

If haptic events cause identifiable sensor artifacts, the pipeline may timestamp/mask/filter only the artifact window; it must not suppress legitimate player motion for an excessive interval.

---

## 8. Test C — Audio Contribution

### Conditions

A. visual only

B. visual + audio

C. visual + haptic

D. visual + haptic + audio

### Collect

- hit recognition,
- heavy/light distinction,
- reaction/counter behavior,
- perceived punch weight,
- immersion rating.

### Critical rule

The visual-only or visual+haptic conditions must remain playable. Audio may improve performance but must not be required for basic fight comprehension.

---

## 9. Test D — Feedback Fatigue / Annoyance

### Minimum duration

10 minutes after the participant understands controls.

### Collect

- hand discomfort,
- vibration annoyance,
- desire to reduce/disable haptics,
- audio fatigue,
- perceived repetition,
- grip instability caused by feedback.

Record whether the preferred setting changes from the first minute to the tenth minute.

---

## 10. Test E — Battery / Thermal A-B

Use the same device, brightness target, arena/harness scene, FPS target, and scripted workload.

Run matched sessions under at least:

1. Full Immersion
2. No Haptic
3. No Audio
4. Battery Saver / reduced-feedback configuration

### Minimum comparison data

- elapsed time,
- battery delta or platform energy estimate,
- thermal state / temperature proxy,
- average frame time,
- frame-time variance,
- dropped frames,
- haptic event count,
- audio state.

### Interpretation rule

Do not attribute all battery or heat cost to haptics/audio. Rendering, frame rate, display, physics, sensor polling, and later networking may dominate total consumption.

The proof question is the **incremental cost** of each feedback channel under matched workload.

---

## 11. Test F — Full Embodied Exchange

Run the core Phase 0 sequence:

```text
READ
→ MOVE / GUARD
→ DEVICE SLIP
→ OPPONENT MISS OR HIT
→ HAPTIC/AUDIO FEEDBACK IF HIT
→ COUNTER
→ RESET
```

Compare Full Feedback against feedback-reduced conditions.

### Collect

- embodied-control rating,
- perceived hit weight,
- counter timing,
- false motion events,
- desire to replay,
- comfort.

The desired result is stronger impact perception without damaging the head-control channel.

---

## 12. Acceptance Logic

This supplement does not require haptics or audio to PASS Boxer Phase 0.

Instead it decides which feedback features are authorized for the next phase.

### Haptic — PASS

Authorize haptic impact feedback if:

- users meaningfully distinguish or perceive impact better,
- false evade/head-motion remains within the accepted Phase 0 budget,
- fatigue/annoyance is acceptable,
- measured incremental energy/thermal cost is acceptable or can be controlled with settings.

### Haptic — CONDITIONAL

Reduce frequency/intensity or adjust filtering if impact value is clear but sensor interference, fatigue, or energy cost is excessive.

### Haptic — FAIL

Do not make haptics part of core combat if they add little experiential value or materially damage sensor stability, comfort, energy behavior, or accessibility.

### Audio — PASS

Authorize audio as an enhancement if it materially improves feedback while gameplay remains understandable when audio is disabled.

### Energy modes — PASS

At least one user-selectable lower-power path must be feasible if Full Immersion materially increases battery/thermal cost.

---

## 13. Product Constraints Frozen by This Supplement

Regardless of test outcome:

1. Haptics must be user-disableable.
2. Audio must be user-disableable.
3. Gameplay-critical information must not depend solely on audio.
4. Haptic events must be timestamped for motion-sensor analysis.
5. Continuous vibration is not allowed as the default combat feedback model.
6. Battery and thermal behavior must be measured on real devices before production defaults are frozen.
7. Energy-saving options must be considered part of product quality, not an afterthought.

---

## 14. Reporting Template

```text
Experiment:
Date:
Build/commit:
Device:
OS/version:
Target FPS:
Brightness target:
Session duration:

Feedback condition:
Haptic level:
Audio state:
Ambience state:

Haptic events:
False evade events around haptics:
Impact-recognition accuracy:
Perceived impact rating:
Embodied rating:
Fatigue/annoyance:

Battery start/end or energy estimate:
Thermal start/end:
Average frame time:
Dropped frames:

PASS / CONDITIONAL / FAIL per feature:
Reason:
Next authorized action:
```
