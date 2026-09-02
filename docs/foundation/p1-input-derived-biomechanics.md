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

# 2. Left-Thumb Input Features

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

The left-thumb gesture should contribute to an inferred lower-body vector:

```text
FootDriveVector = f(L.direction_angle, L.speed, stance, current_velocity)
```

---

# 3. Right-Thumb Punch Features

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

Gesture classification may later map these features to:

```text
jab
cross
hook_left/right
uppercut
body variant
```

But the biomechanical model should retain the continuous input features, not only the discrete punch label.

---

# 4. Coordination Window

The relationship between foot and punch input should be evaluated over a short temporal window around punch commitment.

Define conceptually:

```text
Δt_coord = R.peak_tick - L.peak_tick
```

or another calibrated measure of phase alignment.

A coordinated punch should receive stronger kinetic-chain contribution when the relevant foot/COM drive occurs in the mechanically useful direction and timing window.

A poorly synchronized punch should still execute, but with reduced kinetic efficiency and/or greater balance/recovery cost.

---

# 5. Derived Hip Angular Velocity

`HipAngularVelocity` should be a gameplay-derived biomechanical variable.

Candidate normalized structure:

```text
FootAngularContribution =
    g(L.direction_angle,
      L.speed,
      stance,
      facing,
      current_foot_state)

PunchAngularDemand =
    h(R.direction_angle,
      R.speed,
      R.curvature,
      punch_type,
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

The point is to make virtual hip rotation causally reflect the player's coordinated foot + hand input.

---

# 6. Directional Coupling

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

# 7. Kinetic Chain Quality

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

# 8. Examples

## Example A — Strong coordinated cross

```text
left thumb:
forward/right-biased drive appropriate to stance
high but controlled velocity

right thumb:
rear-hand straight trajectory
high velocity

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
backward vector

right thumb:
rear-hand straight

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
abrupt contradictory lateral input

right thumb:
fast curved hook

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
fast short straight gesture

result:
low required hip contribution
jab remains effective as probe
low action cost
low commitment
```

This prevents the system from demanding full-body input for every punch.

---

# 9. Head Movement Interaction

Phone/head input is a third concurrent channel:

```text
Phone = Head
Left Thumb = Feet / lower-body drive
Right Thumb = Fists / upper-body attack intent
```

Head movement may affect kinetic-chain quality and balance if the player attempts a punch while far outside stable head/COM alignment.

Example:

```text
extreme slip left
+ immediate opposite-side heavy hook
```

may be mechanically strong or weak depending on stance, target angle and recovery timing; P1 should model this as coordination rather than forbid it categorically.

The head channel must therefore contribute to:

- `COMAlignmentQuality`,
- `BalanceQuality`,
- `TargetExposure`,
- counter geometry.

---

# 10. Replay Requirement

The action log must preserve enough data to reconstruct the derived biomechanics.

For meaningful attacks, store or make reproducible:

```text
left_input_direction
left_input_speed
left_input_peak_tick
right_input_direction
right_input_speed
right_input_path/curvature
right_input_peak_tick
head_offset
stance
balance_before
HipAngularVelocity_est
KineticChainQuality
ImpactQuality
```

This allows cinematic replay to animate not merely "a cross happened", but the **specific quality and coordination of that cross**.

For example, a high-quality finishing cross can later drive:

- stronger hip/trunk animation,
- foot pivot visualization,
- appropriate body follow-through,
- impact reaction,
- camera selection.

---

# 11. Validation Requirement

P1 must test these invariants:

1. Faster right-thumb input alone does not automatically maximize punch power.
2. Faster left-thumb input alone does not automatically maximize punch power.
3. Correct direction + timing coordination increases kinetic-chain quality.
4. Contradictory foot/hand input reduces kinetic efficiency and/or balance.
5. Jab remains viable without large lower-body gesture demand.
6. Cross/hook reward stronger coordinated foot/hip contribution.
7. Player can intentionally learn and reproduce a better coordinated punch.
8. Replay can reconstruct the distinction between poorly and well coordinated strikes.

---

# 12. Design Constraint

Do not pretend the phone/thumbs directly measure real human biomechanics.

The system models the **virtual boxer's biomechanics from intentional control signals**.

That distinction is fundamental:

> The player's inputs specify movement intent and coordination; the Boxer simulation converts that intent into a biomechanically plausible virtual body action.
