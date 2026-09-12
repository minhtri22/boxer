# Boxer — Boxing Motion Reference Analysis

## Purpose

This document converts the boxing-motion references supplied for UAT Round 2 into implementation constraints for the current AI training opponent.

The references are used to guide **movement readability**, not to justify adding a larger combat system in this round.

## Scope Lock

UAT Round 2 still uses the existing AI opponent.

- Do NOT implement PvP networking in this round.
- Do NOT replace the AI with a network player.
- Do NOT start replay/clip-generation work.
- Do NOT redesign hit/damage rules unless a movement change demonstrably requires a minimal compatibility fix.

The long-term product direction is PvP, but this round is specifically about making the current AI opponent visually read as a boxer.

## Core Observation to Reproduce

A convincing boxing exchange is not just two arm animations. The viewer reads a relationship between:

`stance → distance → feet → weight → torso → guard → punch → recovery`

The current prototype must therefore stop looking like a stationary character receiving isolated punch animations.

## 1. Stance

The opponent should continuously read as a boxer before, during and after an action.

Required visual characteristics:

- feet separated enough to establish a stable base;
- one side naturally leading rather than a perfectly square mannequin stance;
- knees slightly flexed;
- hips below the upper torso rather than locked upright;
- torso slightly rotated rather than perfectly flat to the camera;
- shoulders and guard consistent with the stance;
- head contained inside the defensive structure;
- weight visibly supported by the feet.

Avoid:

- feet placed like a static rectangular pedestal;
- fully upright locked knees;
- perfectly symmetrical arms;
- arms held away from the torso;
- body facing the opponent with zero rotation at all times.

## 2. Combat Distance

The most important readability problem identified in the current prototype is the feeling that both fighters can punch without being spatially connected.

The AI must therefore maintain a meaningful combat distance.

Conceptual states:

```text
LONG
A                 B

MID
A           B

PUNCH
A       B

TOO CLOSE
A    B
```

The exact world-space values remain implementation-owned. Do not invent arbitrary values without inspecting the current geometry and hit envelope.

The important invariant is:

> When a boxer throws a normal punch, the target should visually occupy the reachable combat space of that punch.

The camera must not make the fighters appear to be in different worlds or punching past one another.

## 3. Footwork

Movement must preserve stance.

### Advance

Use a small lead-foot/rear-foot sequence or an equivalent stance-preserving interpolation.

The body should not simply translate as one rigid object while the feet remain visually disconnected.

### Retreat

The fighter should move away while retaining guard and base.

Avoid a backwards slide with feet apparently glued to the floor.

### Lateral movement

Use small lateral repositioning when useful for combat spacing.

Do not make the opponent continuously strafe. Boxing movement should be economical.

### Reset

After an action the opponent should return to a stable fighting base rather than snapping to an unrelated pose.

## 4. Weight Transfer

Punches and movement should communicate that force originates from the body, not only from the arm.

For the current implementation this does not require a full physics simulation.

The minimum readable chain is:

```text
feet
 ↓
legs
 ↓
hips
 ↓
torso / shoulder
 ↓
arm
 ↓
glove
```

A punch should include a small, controlled body contribution where the existing implementation permits it.

Do not introduce exaggerated animation that damages the first-person combat readability.

## 5. Punch Readability

### Jab / straight

Should read as:

- compact preparation;
- direct extension;
- target-oriented glove path;
- immediate recovery to guard.

### Cross / rear straight

Should read as:

- rear-side contribution;
- torso/shoulder rotation;
- direct target line;
- recovery to stance.

### Hook

The glove must travel on a visibly curved path around the target line.

The elbow should remain structurally readable.

### Uppercut

The body should support a compact upward movement rather than an arm-only vertical extension.

## 6. Defensive Movement

The current round does not need a complete defensive combat system, but the architecture and animation code must not prevent it.

The following are future-ready concepts:

### Guard

- hands near face;
- elbows protect torso;
- shoulders compact;
- return state after attacks.

### Slip

A small lateral/head displacement with the body remaining balanced.

Avoid teleporting the head or translating the whole fighter sideways as a single rigid object.

### Roll

A controlled head/torso path under a hook, followed by recovery to guard.

### Retreat / angle exit

Use feet and torso rather than only translating the root transform.

## 7. Attack / Defense Relationship

The opponent should not continuously attack on a timer regardless of distance.

At minimum the current AI should respect:

```text
distance
   ↓
can attack?
   ↓
choose attack
   ↓
commit
   ↓
impact / miss
   ↓
recover
   ↓
reposition
```

This creates the visual relationship required for boxing without requiring PvP networking.

## 8. First-Person Reference Target

The approved POV combat reference is the primary visual target for:

- apparent opponent scale;
- apparent distance;
- opponent pose;
- glove-to-target relationship;
- camera-relative composition.

Reference assets are in:

`docs/handoff/reference-ui/`

Do not replace the approved visual direction with a generic fighting-game HUD or generic third-person fighting-game composition.

## 9. Animation Failure Modes to Eliminate

The agent must actively inspect for these failure modes:

1. Opponent appears to float.
2. Feet move independently of body weight.
3. Opponent stands frozen while player moves.
4. Punch lands visually short of the opponent.
5. Punch travels through empty space before a detached effect reaches the opponent.
6. Guard opens into a mannequin pose.
7. Body snaps between animation states.
8. Shoulder appears duplicated during fast movement.
9. Elbow/forearm chain disconnects during punch.
10. Fighter returns to an impossible stance after recovery.

## 10. Performance Constraint

The implementation must remain suitable for the current iPhone/WebGL target.

Prefer:

- deterministic procedural movement;
- small numbers of transforms;
- simple interpolation;
- no expensive runtime IK solver unless already present and justified;
- no per-frame allocation;
- no unnecessary physics simulation.

## 11. Future PvP Compatibility

The opponent controller should keep the following separation:

```text
combat intent
      ↓
movement / action command
      ↓
character embodiment
```

The current AI supplies the intent.

Future PvP will supply player/network intent.

The embodiment layer should not know whether the intent came from AI or a future network player.

## 12. Acceptance Principle

The implementation is successful when a reviewer can watch a short exchange and immediately understand:

> two boxers are positioning themselves, maintaining distance, moving their feet, defending and throwing punches at each other.

It is not successful merely because unit tests report valid coordinates or because the arm reaches the hit target.
