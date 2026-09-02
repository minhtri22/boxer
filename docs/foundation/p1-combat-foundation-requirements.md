# Boxer — P1 Combat Foundation Requirements

## Status

**LOCKED UNTIL PHASE 0 PASS**

This document records requirements that must be addressed when P1 begins. It is not authorization to implement P1 during Phase 0.

---

# 1. P1 Purpose

P1 is the core design and simulation foundation of Boxer.

Its job is not merely to add more punches or stamina bars. P1 must define one coherent combat model connecting:

1. player input,
2. boxer movement,
3. boxing biomechanics,
4. spatial/physical interaction,
5. attack and defense resolution,
6. stamina, balance and recovery,
7. anatomy/body consequences,
8. deterministic combat events,
9. replay reconstruction,
10. automatic shareable highlight generation.

The central requirement is:

> A combat result should emerge from the interaction of position, movement, timing, anatomy, energy and technique — not from arbitrary animation states or unexplained RNG.

---

# 2. Mandatory P1 Master Mechanics Matrix

Before P1 production implementation, create a detailed **P1 Master Mechanics Matrix**.

This matrix is a mandatory design artifact and should become the reference table for combat implementation, testing, balancing and replay.

At minimum, every meaningful action/state must be traceable across the following columns:

| Dimension | Required content |
| --- | --- |
| Player input | phone/head input, left-thumb feet, right-thumb fists, guard/no-input |
| Boxer action | step, retreat, lateral move, slip, pull-back, guard, jab, cross, hook, uppercut, body shot, reset |
| Body mechanics | head, neck, shoulders, torso, hips, legs, stance, center of mass |
| Spatial effect | position, range, angle, head offset, body orientation |
| Physical/biomechanical effect | momentum, balance, weight transfer, leverage, recovery |
| Energy cost | short-term action cost, long-term stamina cost, recovery effect |
| Offensive effect | reach, speed, power potential, opening created, vulnerability |
| Defensive effect | evade line, guard coverage, range change, exposure |
| Hit resolution | HIT / MISS / BLOCK / GLANCE / COUNTER if supported |
| Anatomy target | head region, jaw/chin, temple/side, body/torso, ribs, liver region or simplified gameplay equivalent |
| Damage/consequence | impact, stun, stamina effect, balance effect, guard effect, knockdown contribution |
| Recovery | action lock/recovery window, return to guard, balance recovery |
| Counter relationship | what openings this action creates or closes |
| Log event | authoritative event(s) emitted by combat simulation |
| Replay data | minimum state required to reconstruct the action |
| Highlight value | whether/how the event may become a replay/highlight candidate |

The final matrix may add more columns, but it must not remove these relationships without explicit justification.

---

# 3. Movement Foundation

P1 must define how foot movement, head movement, guard and attack coexist.

Baseline embodied mapping inherited from P0:

- **Phone = Head**
- **Left Thumb = Feet**
- **Right Thumb = Fists**
- **No active conflicting action = Return to Guard**

## Head movement

Mandatory baseline:

- left,
- right.

Candidates to evaluate:

- pull-back / rearward head movement,
- lean-in / forward head movement.

Forward/back head movement must not be confused with whole-body forward/back footwork.

## Footwork

P1 must model at least:

- advance,
- retreat,
- lateral left,
- lateral right,
- stance/balance consequences,
- range consequences,
- ring-position consequences where relevant.

---

# 4. Energy and Recovery Model

P1 must not use a single arbitrary energy bar without examining what it represents.

At minimum evaluate a two-timescale model:

## Long-term stamina

Represents accumulated fatigue across the round/fight.

Potential effects:

- reduced punch output,
- slower recovery,
- reduced movement quality,
- reduced guard resilience,
- reduced ability to sustain pressure.

## Short-term action/recovery capacity

Represents immediate ability to chain actions effectively.

Potential effects:

- repeated punches cost progressively more,
- heavy actions create recovery windows,
- missed power shots create vulnerability,
- repeated defensive movement may reduce immediate response quality,
- spam becomes self-defeating.

The P1 matrix must explicitly state energy cost and recovery consequences for each action.

---

# 5. Range, Trajectory and Hit Resolution

Whenever practical, outcomes should be determined geometrically rather than by abstract dodge probability.

Conceptual rule:

```text
attack trajectory + active window
        ×
current fighter geometry/state
        ↓
HIT / MISS / BLOCK / other resolved outcome
```

Relevant state may include:

- fighter position,
- head offset,
- body orientation,
- guard geometry,
- punch reach,
- punch trajectory,
- timing,
- stance/balance.

Avoid using RNG as a substitute for geometry/timing when the outcome can be computed directly.

Stats may modify capability but should not replace player action.

---

# 6. Attack Model

Every punch should eventually be described by a coherent set of variables rather than only a damage number.

Candidate dimensions:

- punch type,
- hand,
- start state,
- range envelope,
- trajectory,
- speed,
- active window,
- power potential,
- weight transfer,
- balance requirement,
- stamina cost,
- recovery duration,
- exposed defensive regions,
- target regions,
- counter vulnerability.

Conceptually, effective impact may depend on factors such as:

```text
Base technique
× timing
× range quality
× balance
× stamina
× opening
× contact quality
```

The exact formula must be researched/designed in P1; this expression is not a frozen production equation.

---

# 7. Defense Model

P1 should preserve three distinct defensive families:

## Footwork defense

Changes whole-body position/range.

## Head-movement defense

Moves the head out of the attack line while preserving more of the fighting position.

## Guard defense

Allows contact with defensive structure rather than clean anatomical target contact.

Each defense must have different tactical costs and benefits.

Automatic return-to-guard must not make inactivity optimal.

---

# 8. Anatomy and Biomechanics Requirement

P1 must perform a focused research/design pass on boxing-relevant anatomy and biomechanics before freezing formulas.

The goal is not medical simulation for its own sake. The goal is to identify which real physical relationships materially improve believable gameplay.

Research/design topics should include at least:

- stance and center of mass,
- foot placement and balance,
- kinetic chain from legs/hips/torso/shoulder/arm,
- weight transfer,
- head/neck movement,
- jaw/chin vulnerability as a gameplay concept,
- body-shot consequences,
- guard structure,
- effects of fatigue on movement and output,
- knockdown-relevant balance/impact concepts,
- recovery after missed/heavy punches.

Any anatomical simplification must be explicit and gameplay-motivated.

P1 should avoid pseudo-medical precision unsupported by evidence.

---

# 9. Combat State Consequences

Actions must affect subsequent possibilities.

Examples of relationships P1 must model or explicitly reject:

```text
heavy cross
→ greater recovery exposure

successful slip
→ counter opportunity

continuous retreat
→ gives up ring position

heavy blocked strike
→ defender still pays some stamina/guard cost

missed hook
→ balance/recovery penalty

body hit
→ possible stamina/recovery consequence
```

The combat system should therefore behave as a state transition system, not as independent attack animations.

---

# 10. Deterministic Combat Timeline

P1 must treat combat logging as foundation architecture, not as a later replay hack.

Every meaningful outcome should emit an authoritative combat event sufficient for diagnosis, analytics and later replay.

The event model should support at least:

- tick/time,
- actor,
- target where relevant,
- action,
- fighter position,
- head offset,
- guard state,
- relevant energy state,
- attack phase,
- trajectory/contact information where necessary,
- resolved outcome,
- damage/state consequence,
- counter/opening state,
- deterministic seed/state if randomness exists.

P1 should evaluate a hybrid replay model:

> **authoritative event stream + periodic state snapshots**

This is preferred over relying only on raw input replay, because combat code may evolve over time and old replays should not silently change outcome.

---

# 11. Replay Reconstruction Requirement

The combat timeline should be designed so that a completed fight can later be reconstructed independently from the original POV presentation.

Potential replay viewpoints include:

- original first-person POV,
- opponent POV,
- ringside/broadcast camera,
- overhead/tactical camera,
- cinematic knockout camera,
- slow-motion detail camera.

Replay is not required to be fully implemented in early P1, but the event/state architecture must preserve enough information to make faithful reconstruction feasible later.

---

# 12. Shareable Highlight Foundation

A later product feature should be able to identify and render short shareable fight highlights from the combat timeline.

Examples of highlight candidates:

- clean knockout,
- knockdown,
- slip → counter,
- very close evade,
- strong combination finish,
- comeback finish,
- late-round decisive hit.

Potential derived metrics/events include:

- evade clearance / near-miss distance,
- counter-response time,
- impact quality,
- sequence/combo quality,
- current stamina disadvantage/advantage,
- round/time context,
- knockdown/KO state.

Future flow:

```text
Fight
→ authoritative action/event log
→ highlight candidate detection
→ replay reconstruction
→ cinematic camera selection
→ short clip render
→ Share Highlight
```

This capability should eventually support sharing to common social/messaging platforms through standard mobile share mechanisms.

P1 does **not** need to build the full social/export feature, but it must prevent combat architecture choices that would make this difficult later.

---

# 13. Why Replay/Highlight Matters to Core Architecture

Replay is valuable beyond presentation.

The same deterministic timeline may later support:

- fight replay,
- automated highlights,
- post-fight statistics,
- coaching/tactical analysis,
- debugging,
- balance analysis,
- AI evaluation,
- ghost/replay opponents,
- future PvP dispute/anti-cheat investigation,
- content generation and organic sharing.

Therefore combat events are first-class data, not debugging leftovers.

---

# 14. P1 Research Requirement

Before freezing the P1 Master Mechanics Matrix and formulas, perform a dedicated research pass covering:

1. boxing biomechanics,
2. boxing tactics and distance/range,
3. sports/boxing-relevant anatomy at an appropriate gameplay abstraction,
4. fatigue/stamina modeling,
5. punch trajectory/contact mechanics,
6. game combat simulation patterns,
7. deterministic simulation/event sourcing/replay approaches,
8. replay camera reconstruction,
9. automated highlight detection/rendering where useful.

Use authoritative papers, technical references and relevant game-engine implementations where available.

Separate:

- established external evidence,
- Boxer design assumptions,
- implementation approximations,
- hypotheses requiring playtesting.

---

# 15. Mandatory P1 Deliverables

When P1 is unlocked, it must produce at least:

1. **P1 research review with citations**
2. **P1 Master Mechanics Matrix**
3. **Combat state model**
4. **Energy/stamina/recovery model**
5. **Movement/head/guard interaction model**
6. **Attack model**
7. **Defense model**
8. **Anatomy/body-region abstraction**
9. **Hit/miss/block/counter resolution model**
10. **Deterministic combat event schema**
11. **State snapshot/replay reconstruction design**
12. **Highlight-candidate event/metric design**
13. **QA/simulation plan**
14. **Explicit PASS / FAIL criteria before deeper production work**

The Master Mechanics Matrix is the central P1 artifact and must link these subsystems rather than documenting them independently.

---

# 16. P1 Design Principle

The P1 foundation should make the following chain explainable for any important exchange:

```text
PLAYER INPUT
→ BODY ACTION
→ BIOMECHANICS / POSITION
→ RANGE / TRAJECTORY / CONTACT
→ HIT / MISS / BLOCK / COUNTER
→ ENERGY / BALANCE / DAMAGE / RECOVERY
→ NEW COMBAT STATE
→ AUTHORITATIVE EVENTS
→ REPLAY / ANALYTICS / HIGHLIGHT DATA
```

If a major gameplay outcome cannot be explained through this chain, the model is incomplete or hiding arbitrary behavior.
