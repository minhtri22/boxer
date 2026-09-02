# P1 Research — Boxing Biomechanics, Physiology, Anatomy, and Simulation Implications

## Status

**DOCUMENTATION / RESEARCH ONLY**

P1 implementation remains locked until Phase 0 PASS.

This document exists so P1 can proceed immediately after P0 without repeating foundation research.

---

# 1. Research Question

How should Boxer translate player inputs into a combat model that is physically believable, tactically interesting, computationally manageable on mobile, and rich enough to support deterministic replay/highlight reconstruction?

The model must connect:

```text
INPUT
→ BODY ACTION
→ BIOMECHANICS
→ SPACE / RANGE
→ CONTACT GEOMETRY
→ HIT / MISS / BLOCK / EVADE
→ IMPACT
→ STAMINA / BALANCE / RECOVERY / DAMAGE
→ NEXT TACTICAL STATE
→ AUTHORITATIVE EVENT LOG
→ REPLAY / HIGHLIGHT
```

The objective is not biomedical simulation. The objective is a **gameplay-grade biomechanical model** grounded in real boxing principles.

---

# 2. Key Research Findings

## 2.1 Punches are whole-body kinetic-chain actions

Research on elite and junior boxers shows that punch performance depends on coordinated contribution from pelvis, trunk, shoulder, elbow and wrist rather than arm strength alone. Elite boxers generally produce higher force and velocity with more effective segment coordination.

Gameplay implication:

> Punch effectiveness must depend on stance, balance, lower-body support, trunk/weight-transfer state and timing — not only a punch-type damage constant.

A useful abstraction is therefore:

```text
EffectivePunch
= BaseTechnique
× FighterCapability
× RangeQuality
× BalanceQuality
× WeightTransferQuality
× StaminaQuality
× TimingQuality
× TargetExposure
```

This is a design structure, not a literal biomechanical equation.

Sources:

- Dinu et al., biomechanical analysis of cross/hook/uppercut in junior vs elite boxers: https://pmc.ncbi.nlm.nih.gov/articles/PMC7739747/
- Current review of limb biomechanics and wearable-sensor evidence: https://pmc.ncbi.nlm.nih.gov/articles/PMC12714896/

---

## 2.2 Punch types require different mechanics

Research distinguishes straight punches from hooks/uppercuts in segment contribution and trajectory.

Important gameplay abstractions:

### Jab

- short preparation,
- high speed / low commitment,
- lower energy cost,
- useful for probing/range interruption,
- relatively small recovery exposure.

### Cross

- stronger contribution from lower-body drive and trunk transfer,
- more commitment than jab,
- high straight-line effectiveness,
- meaningful recovery/counter vulnerability if missed.

### Hook

- rotational trajectory,
- stronger dependence on trunk/shoulder rotation,
- strong close/mid-range threat,
- larger angular exposure and miss vulnerability.

### Uppercut

- rising/curved trajectory,
- primarily close-range application,
- should be poor when range is wrong,
- potentially strong against lowered/forward head position.

Gameplay consequence:

Punch selection must interact with **range + opponent head/body position + current balance**, otherwise the punch taxonomy becomes cosmetic.

Sources:

- https://pmc.ncbi.nlm.nih.gov/articles/PMC7739747/
- https://pmc.ncbi.nlm.nih.gov/articles/PMC10414587/

---

## 2.3 Lower-body mechanics materially affect punch output

Recent force-platform work reports clear ground-reaction-force contributions and changes in punch output under fatigue. Rear-leg drive and transfer through the lower body are important to whole-body punching mechanics.

Gameplay implication:

Footwork and punching cannot be treated as independent systems.

Examples:

- throwing a committed cross while poorly planted should reduce effective impact and/or balance,
- punching during a retreat may be possible but should alter force, timing and recovery,
- successful weight transfer can increase commitment and therefore both reward and risk,
- repeated movement + heavy punches should degrade short-term action capacity.

Source:

- https://pmc.ncbi.nlm.nih.gov/articles/PMC12729554/

---

## 2.4 Fatigue should alter capability, not simply drain an RPG bar

Boxing is a high-intensity intermittent sport with substantial physiological demand. Reviews show high cardiorespiratory requirements and strong acute physiological responses. More recent biomechanical work also reports punch-output reductions after fatigue, with effects differing by punch type.

Gameplay implication:

P1 should distinguish at least two energy timescales:

### Long-term stamina

Represents accumulated round/fight fatigue.

Influences:

- sustained movement,
- recovery rate,
- repeated high-output combinations,
- late-round output,
- body-shot consequences.

### Short-term action capacity

Represents immediate readiness / local fatigue / recovery after actions.

Influences:

- spam resistance,
- combination length,
- recovery windows,
- explosive evasions,
- ability to throw a hard counter immediately after another committed action.

This avoids the unrealistic pattern:

```text
100 stamina → 30 identical punches → 0 stamina
```

and supports a richer pattern:

```text
burst
→ local recovery
→ reposition
→ burst
```

Sources:

- Amateur boxing physical/physiological attributes: https://pubmed.ncbi.nlm.nih.gov/25358529/
- Acute physiological systematic review/meta-analysis: https://pubmed.ncbi.nlm.nih.gov/35380916/
- Competition performance/physiology review: https://pubmed.ncbi.nlm.nih.gov/28081033/
- Lower-limb kinetics/fatigue study: https://pmc.ncbi.nlm.nih.gov/articles/PMC12729554/

---

## 2.5 Defensive movement involves head motion, centre of mass and support base

Recent work examining slips, ducks and sidesteps reports differences in centre-of-mass displacement/speed, hip angular velocity, muscular coordination and movement stability between competitive levels.

Gameplay implication:

Defense should not be represented by one `DODGE` state.

Maintain distinct state variables for:

- `head_offset`,
- `body/COM_offset`,
- `foot_position`,
- `support/balance`,
- `guard_geometry`,
- `movement_velocity`.

This supports meaningful distinctions:

### Slip

Head leaves trajectory while feet/body largely retain range.

### Pull-back

Head/upper body retreats without necessarily moving the feet.

### Step-back

Entire boxer changes distance.

### Duck/weave

Head and COM move vertically/laterally with stronger lower-body involvement.

### Sidestep

Feet and COM change lateral position.

These moves can then have distinct counter opportunities and energy/balance costs.

Source:

- https://pmc.ncbi.nlm.nih.gov/articles/PMC13220430/

---

# 3. Anatomy Abstraction for Gameplay

The game should use **anatomical zones**, not an injury simulator.

P1 should define a compact target model such as:

## Head zones

- frontal face/head,
- left jaw/temple side,
- right jaw/temple side,
- chin/jawline.

## Torso zones

- upper torso/guarded chest,
- left body,
- right body,
- central body/solar-plexus region abstraction.

The exact number of zones should remain minimal unless more detail creates observable gameplay value.

### Why zones matter

A strike result may depend on:

```text
punch type
+ impact direction
+ target zone
+ guard coverage
+ target motion
+ impact quality
```

For example, head impacts that generate rotational acceleration are biomechanically different from purely translational loading. Boxing literature consistently identifies rotational head dynamics as relevant to concussion/head-trauma mechanisms.

Gameplay should use this only as inspiration for differentiated reactions and knockdown likelihood — **not as a medical predictor**.

Sources:

- Olympic boxing head biomechanics: https://pmc.ncbi.nlm.nih.gov/articles/PMC1725037/
- Rotational head acceleration review: https://pubmed.ncbi.nlm.nih.gov/35107134/
- Punches with/without loss of consciousness: https://pubmed.ncbi.nlm.nih.gov/31082637/
- Boxing head-trauma systematic review: https://pubmed.ncbi.nlm.nih.gov/37862081/

---

# 4. P1 Modeling Principles

## 4.1 Geometry before probability

When possible:

```text
trajectory intersects target → CONTACT
trajectory misses target → MISS
trajectory intersects guard first → BLOCK/PARRY-LIKE CONTACT
```

Do not replace spatial outcomes with arbitrary dodge percentages.

RNG may later be appropriate for uncertain secondary consequences, but not for whether a geometrically missed punch magically hits.

---

## 4.2 Stats modify capability; they do not play the game

Future fighter stats may modify:

- max movement speed,
- recovery rate,
- effective reach,
- punch velocity envelope,
- stamina capacity,
- balance recovery,
- damage tolerance.

But player timing/range/defense should remain decisive.

---

## 4.3 Commitment must create vulnerability

High-value actions should create meaningful opportunity cost.

Examples:

```text
heavy cross
→ higher impact ceiling
→ higher stamina/action cost
→ larger recovery exposure if missed
```

```text
wide hook
→ strong close-range threat
→ larger rotational commitment
→ punishable on whiff
```

```text
aggressive forward step + punch
→ closes range
→ increases pressure
→ may reduce lateral defensive freedom during commitment
```

---

## 4.4 Defense must alter future offense

Successful defense should do more than avoid HP loss.

Examples:

- clean slip may create counter timing advantage,
- heavy block may preserve health but cost guard/action capacity,
- pull-back may create a long counter opportunity but lose pressure/position,
- sidestep may alter angle and opponent alignment,
- repeated retreat may approach boundary/ring-pressure disadvantage.

---

# 5. Candidate Combat State Vector

P1 should evaluate a compact fighter state such as:

```text
FighterState
- world_position
- facing
- foot_state / stance
- head_offset
- COM_offset
- guard_state
- active_action
- action_phase
- balance
- short_term_capacity
- long_term_stamina
- head_condition
- body_condition
- current_recovery
- counter_advantage
- ring_pressure
```

Not all of these must survive implementation. Every retained variable must create visible gameplay consequence.

---

# 6. Candidate Impact Model

P1 should not use raw Newtons as direct gameplay damage.

A gameplay impact score can be derived from normalized factors:

```text
ImpactQuality =
  technique_base
× kinetic_chain_quality
× range_quality
× velocity_quality
× balance_quality
× stamina_quality
× contact_quality
× target_exposure
```

Then resolve consequences separately:

```text
ImpactQuality
→ damage / condition
→ balance disruption
→ guard disruption
→ recovery/stun window
→ knockdown pressure
```

This allows two punches with similar nominal power to have different tactical consequences.

---

# 7. Candidate Energy Model

Use multiple related states rather than one universal mana bar.

## LongTermStamina [0..1]

Slow-changing accumulated endurance state.

## ActionCapacity [0..1]

Fast-changing capacity for explosive actions.

## Balance [0..1]

Mechanical readiness and stable support.

Possible conceptual update:

```text
ActionCost = BaseActionCost × Commitment × FatigueModifier × InstabilityModifier

ActionCapacity(t+1)
= clamp(ActionCapacity(t) - ActionCost + ShortRecovery)

LongTermStamina(t+1)
= clamp(LongTermStamina(t) - SustainedWork - ImpactFatigue + RoundRecovery)
```

Exact functions/constants must be calibrated experimentally; these formulas are architecture placeholders.

---

# 8. Research Implications for Head Controls

P0 baseline:

- phone left → head left,
- phone right → head right.

P1 may evaluate:

- phone backward → pull-back,
- phone forward → lean-in / inside-position head movement.

Forward/back must remain optional until usability is validated because it can overlap semantically with footwork and affect screen viewing distance.

P1 should preserve the distinction:

```text
left-thumb retreat = whole boxer changes distance
phone pull-back = head/upper body changes relative position without equivalent foot displacement
```

This distinction can become tactically valuable.

---

# 9. Simulation / Replay Research Finding

Unity's physics APIs support fixed-timestep simulation, and Unity documentation explicitly warns that variable frame-dependent simulation steps produce non-deterministic results. Fixed-step simulation improves reproducibility, but Unity/PhysX should not be assumed to guarantee perfect cross-platform determinism.

Therefore Boxer replay should **not depend solely on replaying raw user input through current physics code**.

Recommended architecture:

```text
INPUT LOG
+ AUTHORITATIVE COMBAT EVENTS
+ PERIODIC STATE SNAPSHOTS
+ VERSION / SEED METADATA
```

This provides resilience if:

- engine version changes,
- floating-point behavior differs by platform,
- animation/physics implementation changes,
- old fights need to remain replayable.

Sources:

- Unity `Physics.Simulate`: https://docs.unity3d.com/ScriptReference/Physics.Simulate.html
- Unity fixed timestep/time management: https://docs.unity3d.com/Manual/TimeFrameManagement.html
- Unity manual simulation/reproducibility note: https://docs.unity3d.com/6000.0/Documentation/Manual/physics-optimization-cpu-manual-simulation.html

---

# 10. What P1 Must Produce Before Implementation

P1 documentation phase must output at minimum:

1. `p1-master-mechanics-matrix.md`
2. `p1-combat-state-and-energy-model.md`
3. `p1-hit-defense-resolution.md`
4. `p1-combat-event-timeline-and-replay.md`
5. a frozen minimal combat-state schema
6. a frozen minimal event taxonomy
7. candidate formulas with explicit tunable parameters
8. validation scenarios and invariants
9. a list of what remains intentionally arcade/gameplay-oriented rather than physiologically simulated

P1 implementation remains blocked until P0 PASS.
