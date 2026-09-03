# P1 — Input-Derived Biomechanics Mapping

## Status

**NORMATIVE P1 DESIGN REQUIREMENT — IMPLEMENTATION LOCKED UNTIL P0 PASS**

This document refines the P1 combat model so that important biomechanical variables are derived from player input coordination rather than generated as hidden animation-only state.

---

# 1. Core Principle

For player-controlled Boxer combat, variables such as **hip angular velocity / trunk rotational contribution** should be inferred from the relationship between:

- left-thumb footwork input,
- right-thumb punch input,
- their directions,
- their velocities,
- their timing overlap,
- stance / lead side,
- current balance and movement state.

Therefore `HipAngularVelocity` must not be a free arbitrary stat that appears only because a punch animation is playing.

Conceptually:

```text
LEFT THUMB
(direction + speed + timing)
        \
         → lower-body / COM contribution
          \
           → inferred hip/trunk rotation
          /
         → upper-limb punch intent
        /
RIGHT THUMB
(direction + speed + timing)
```

The resulting kinetic-chain quality influences punch effectiveness, balance, recovery and replay animation semantics.

---

# 2. Player Controls Intent; Simulation Resolves Anatomy

A critical complexity constraint for P1 is:

> **Player controls intent; simulation resolves anatomy.**

The left thumb does **not** mean "left leg" and the right thumb does **not** mean "right hand".

Instead:

- **Left Thumb = footwork / lower-body intent**
- **Right Thumb = punch / upper-body intent**
- **Phone = head intent**

The player should not be required to explicitly encode which anatomical limb moves first for every action.

The combat model must contain an `IntentToAnatomyResolver` that maps intent into stance-correct virtual-body actions.

Conceptually:

```text
PLAYER INPUT
→ MOVEMENT / PUNCH INTENT
→ STANCE + CURRENT BODY STATE
→ INTENT-TO-ANATOMY RESOLVER
→ RESOLVED FOOT / ARM / HIP / TRUNK ACTION
→ BIOMECHANICAL STATE
```

Examples:

```text
left-thumb advance intent
+ orthodox stance
→ lead foot initiates
→ rear foot follows
→ stance width preserved
```

```text
right-thumb fast straight intent
+ orthodox stance
+ neutral attack state
→ lead-hand jab candidate
```

```text
right-thumb committed straight intent
+ rear-side kinetic setup
+ viable range
→ rear-hand cross candidate
```

The exact gesture grammar remains subject to P1 calibration, but anatomy resolution must not require the player to micromanage each limb.

This constraint is intended to keep Boxer from becoming a gesture-language simulator.

---

# 3. Complexity Guardrail

Do not require the player to simultaneously encode all of the following directly:

- left/right hand identity,
- left/right foot identity,
- gesture direction,
- gesture speed,
- timing,
- stance,
- pivot,
- weight transfer.

Those variables may exist in the simulation, but only the smallest tactically meaningful subset should be exposed as player input.

Recommended abstraction:

```text
Player specifies:
- where the boxer should move,
- what kind of punch intent is being expressed,
- when and how strongly/quickly the action is expressed,
- where the head should move.

Simulation resolves:
- which foot initiates,
- which foot follows,
- which arm executes,
- pivot amount,
- hip/trunk rotation,
- weight transfer,
- stance preservation,
- balance consequence.
```

---

# 4. Left-Thumb Input Features

For each left-thumb gesture, derive at minimum:

```text
L.direction_angle
L.speed
L.acceleration (candidate)
L.magnitude
L.start_tick
L.peak_tick
L.end_tick
```

Interpretation depends on stance and current facing.

Examples:

- forward component → advance / drive,
- backward component → retreat,
- lateral component → sidestep / angle change,
- diagonal component → combined translation and rotational setup.

The left-thumb gesture should first express a `FootworkIntent`, then the resolver derives stance-correct anatomical action.

Conceptually:

```text
FootworkIntent = f(L.direction_angle, L.speed, L.magnitude)

ResolvedFootAction =
resolve(FootworkIntent, stance, facing, current_foot_state, balance)

FootDriveVector =
g(ResolvedFootAction, current_velocity)
```

---

# 5. Right-Thumb Punch Features

For each right-thumb punch gesture, derive at minimum:

```text
R.direction_angle
R.speed
R.acceleration (candidate)
R.path_length
R.curvature
R.start_tick
R.peak_tick
R.end_tick
```

Gesture classification may later map these features to intent classes such as:

```text
fast straight
committed straight
hook-like curved attack
uppercut-like rising attack
body-target variant
```

The simulation then resolves the actual arm/punch based on stance and current state.

Conceptually:

```text
PunchIntent = classify(R.features)

ResolvedPunchAction =
resolve(PunchIntent, stance, lead/rear availability, guard, range, recovery)
```

The biomechanical model should retain continuous input features, not only the final discrete punch label.

---

# 6. Coordination Window

The relationship between resolved foot action and resolved punch action should be evaluated over a short temporal window around punch commitment.

Define conceptually:

```text
Δt_coord = PunchPeakTick - FootDrivePeakTick
```

or another calibrated measure of phase alignment.

A coordinated punch should receive stronger kinetic-chain contribution when the relevant foot/COM drive occurs in the mechanically useful direction and timing window.

A poorly synchronized punch should still execute, but with reduced kinetic efficiency and/or greater balance/recovery cost.

---

# 7. Derived Hip Angular Velocity

`HipAngularVelocity` should be a gameplay-derived biomechanical variable.

It must be derived **after** intent has been resolved into stance-correct lower- and upper-body actions.

Candidate normalized structure:

```text
FootAngularContribution =
    g(ResolvedFootAction,
      L.direction_angle,
      L.speed,
      stance,
      facing,
      current_foot_state)

PunchAngularDemand =
    h(ResolvedPunchAction,
      R.direction_angle,
      R.speed,
      R.curvature,
      lead/rear side)

Coordination =
    c(Δt_coord,
      directional_alignment,
      current_balance)

HipAngularVelocity_est =
    FighterHipCapability
  × FootAngularContribution
  × PunchAngularDemand
  × Coordination
```

This is a **gameplay estimation model**, not a claim that touchscreen input directly measures the player's biological hip velocity.

The point is to make virtual hip rotation causally reflect the player's coordinated lower-body + upper-body intent while preserving stance-correct anatomy.

---

# 8. Directional Coupling

Direction must matter.

Example for an orthodox rear-hand cross:

- useful lower-body drive / rotation direction should reinforce the rear-hand punch,
- contradictory or abrupt opposite foot input should reduce `Coordination`,
- retreating while throwing the cross may still work but should yield a different kinetic profile,
- lateral movement in the correct direction may support angle creation but alter power transfer.

For hooks, rotational coupling should be more important than for a light jab.

For a jab, hip contribution should exist but be weighted lower so the player is not forced to exaggerate foot input for every fast lead-hand strike.

Therefore punch types use different coefficients:

```text
HipWeight_jab       = low
HipWeight_cross     = high
HipWeight_hook      = high
HipWeight_uppercut  = medium/high, calibrated by stance/range
```

Exact values remain tunable P1 parameters.

---

# 9. Kinetic Chain Quality

`HipAngularVelocity_est` is one contributor to a broader `KineticChainQuality`.

Candidate structure:

```text
KineticChainQuality =
weighted(
    FootDriveQuality,
    HipRotationQuality,
    TrunkTransferQuality,
    PunchGestureQuality,
    TimingCoordination,
    BalanceQuality
)
```

Then:

```text
ImpactQuality =
BaseTechnique
× FighterCapability
× RangeQuality
× KineticChainQuality
× BalanceQuality
× StaminaQuality
× ContactQuality
× TargetExposure
```

This makes a powerful strike emerge from coordinated player input plus fighter capability rather than from the punch label alone.

---

# 10. Examples

## Example A — Strong coordinated cross

```text
left thumb:
forward/right-biased drive intent appropriate to stance
high but controlled velocity

resolver:
stance-correct rear-side drive / pivot contribution

right thumb:
committed straight intent
high velocity

resolver:
rear-hand cross

coordination:
peaks aligned inside valid timing window

result:
HipAngularVelocity_est ↑
KineticChainQuality ↑
ImpactQuality ↑
commitment/recovery ↑
```

---

## Example B — Cross while retreating

```text
left thumb:
backward movement intent

right thumb:
committed straight intent

resolver:
retreat foot sequence + rear straight

result:
less forward drive
lower kinetic-chain contribution
possibly useful intercept timing
lower impact ceiling
balance/recovery profile differs
```

The punch is not prohibited.

---

## Example C — Hook with poor coordination

```text
left thumb:
abrupt contradictory lateral intent

right thumb:
fast curved hook intent

result:
PunchAngularDemand high
FootAngularContribution poorly aligned
Coordination low
HipAngularVelocity_est reduced/unstable
Balance cost ↑
ImpactQuality ↓
whiff vulnerability ↑
```

---

## Example D — Light jab while stationary

```text
left thumb:
neutral

right thumb:
fast short straight intent

resolver:
lead-hand jab

result:
low required hip contribution
jab remains effective as probe
low action cost
low commitment
```

This prevents the system from demanding full-body input for every punch.

---

# 11. Head Movement Interaction

Phone/head input is a third concurrent channel:

```text
Phone = Head
Left Thumb = Footwork / lower-body intent
Right Thumb = Punch / upper-body intent
```

Head movement may affect kinetic-chain quality and balance if the player attempts a punch while far outside stable head/COM alignment.

Example:

```text
extreme slip left
+ immediate opposite-side heavy hook intent
```

may be mechanically strong or weak depending on stance, target angle and recovery timing; P1 should model this as coordination rather than forbid it categorically.

The head channel must therefore contribute to:

- `COMAlignmentQuality`,
- `BalanceQuality`,
- `TargetExposure`,
- counter geometry.

---

# 12. Replay Requirement

The action log must preserve enough data to reconstruct both intent and resolved anatomy.

For meaningful attacks, store or make reproducible:

```text
left_input_direction
left_input_speed
left_input_peak_tick
footwork_intent
resolved_foot_action
right_input_direction
right_input_speed
right_input_path/curvature
right_input_peak_tick
punch_intent
resolved_punch_action
head_offset
stance
balance_before
HipAngularVelocity_est
KineticChainQuality
ImpactQuality
```

This allows cinematic replay to animate not merely "a cross happened", but the **specific quality, anatomy resolution and coordination of that cross**.

For example, a high-quality finishing cross can later drive:

- stronger hip/trunk animation,
- foot pivot visualization,
- appropriate body follow-through,
- impact reaction,
- camera selection.

---

# 13. Validation Requirement

P1 must test these invariants:

1. Faster right-thumb input alone does not automatically maximize punch power.
2. Faster left-thumb input alone does not automatically maximize punch power.
3. Correct direction + timing coordination increases kinetic-chain quality.
4. Contradictory foot/hand input reduces kinetic efficiency and/or balance.
5. Jab remains viable without large lower-body gesture demand.
6. Cross/hook reward stronger coordinated foot/hip contribution.
7. Player can intentionally learn and reproduce a better coordinated punch.
8. Replay can reconstruct the distinction between poorly and well coordinated strikes.
9. Left thumb never implicitly means left leg.
10. Right thumb never implicitly means right hand.
11. Stance changes can alter resolved limb sequence without changing the player's high-level intent grammar.
12. The control model remains learnable without requiring explicit per-limb micromanagement.

---

# 14. Design Constraint

Do not pretend the phone/thumbs directly measure real human biomechanics.

The system models the **virtual boxer's biomechanics from intentional control signals**.

That distinction is fundamental:

> The player's inputs specify movement intent and coordination; the Boxer simulation resolves stance-correct anatomy and converts that intent into a biomechanically plausible virtual body action.
