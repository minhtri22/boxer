# Local Agent Execution Prompt — Boxer UAT Round 2

You are implementing **Boxer UAT Round 2**.

Your task is to read the handoff documents, inspect the existing implementation, implement the required changes, build and validate the game, and leave a complete UAT candidate. The human should only need to perform final UAT after you finish all implementation and automated validation.

## 0. Read These Documents First — Mandatory

Read all of the following before changing code:

- `docs/handoff/README-HANDOFF.md`
- `docs/handoff/18-uat-round2-combat-readability-plan.md`
- `docs/handoff/19-pvp-architecture-direction.md`
- `docs/handoff/21-boxing-motion-reference-analysis.md`
- `docs/handoff/22-combat-animation-quality-gates.md`
- `docs/handoff/reference-ui/README-VISUAL-REFERENCE.md`
- `docs/handoff/reference-ui/ui-reference-notes.md`
- `docs/handoff/reference-ui/visual-lock.yaml`
- all approved reference images under `docs/handoff/reference-ui/`

Also inspect the current Unity implementation and existing evidence/tests before deciding how to implement anything.

## 1. Critical Scope Lock

### This round still uses AI opponent

The opponent remains the existing AI-controlled fighter.

**DO NOT implement PvP networking in this task.**

Do not add:

- matchmaking;
- networking transport;
- server authority;
- client prediction;
- network interpolation;
- network replication;
- player-to-player session management.

PvP is the future architecture direction only. Keep the opponent-control boundary modular so that AI intent can later be replaced by player/network intent.

### Do not start unrelated work

Do NOT start:

- career/progression systems;
- shop systems;
- full character customization;
- replay/clip-generation system;
- new monetization systems;
- new game modes;
- large HUD redesign;
- player-leg embodiment unless it is required to fix a direct regression from this task.

## 2. Primary Goal

The current prototype must stop looking like:

> two characters independently playing punch animations.

It must begin to read as:

> two boxers positioning themselves, maintaining distance, moving their feet, defending and throwing punches at each other.

The current AI opponent is a **training/sparring opponent**, not the final PvP player representation.

## 3. Reference Priority

Use the approved reference images as the visual target for:

- apparent combat distance;
- opponent scale;
- stance;
- glove relationship to the player;
- first-person composition;
- ring atmosphere;
- overall visual hierarchy.

Use the boxing-motion reference analysis in:

`docs/handoff/21-boxing-motion-reference-analysis.md`

as the movement/animation target.

Use:

`docs/handoff/22-combat-animation-quality-gates.md`

as the acceptance contract.

Do not replace the approved visual language with generic fighting-game visuals.

## 4. Objective A — Combat Distance

Fix the current feeling that fighters are punching empty space.

Inspect the existing combat geometry before changing it.

Implement a coherent combat-distance model that keeps:

- player and opponent spatially connected;
- normal punches visually within believable reach;
- opponent positioning consistent with hit geometry;
- close/mid/long combat states where useful;
- target position and visual embodiment aligned.

Do not blindly invent world-space values. Reuse or derive from existing authoritative combat geometry where possible.

### Acceptance

When a normal punch is thrown, the viewer should understand:

`attacker → glove → target → impact`

There must not be a visually disconnected projectile/effect that explains a hit while the glove appears to miss.

## 5. Objective B — Opponent Footwork

The opponent currently punches but can feel static.

Add subtle, stance-preserving:

- advance;
- retreat;
- small lateral repositioning;
- distance adjustment;
- stance reset after actions.

Movement must be economical and readable.

Do NOT make the opponent continuously strafe.

Do NOT make the opponent slide like a rigid object.

The opponent's feet must visually support its body position.

### Required relationship

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

A movement must not be implemented as a root translation that leaves the visual feet/body relationship implausible.

## 6. Objective C — Natural Boxing Stance

The opponent must no longer look like a mannequin.

Maintain:

- compact guard;
- slight stance asymmetry;
- natural lead/rear side;
- slightly flexed knees;
- stable foot base;
- slight torso rotation;
- shoulders integrated with the guard;
- head contained inside the defensive structure;
- believable weight distribution.

Avoid:

- arms spread laterally;
- perfectly symmetrical guard;
- locked legs;
- square mannequin stance;
- rigid torso;
- impossible recovery poses.

## 7. Objective D — Punch / Body Connection

Existing punch families must remain readable:

- STRAIGHT
- HOOK
- UPPERCUT
- OVERHAND

Do not replace the existing arm-chain embodiment.

Preserve:

`shoulder → upper arm → elbow joint → forearm → glove`

A punch should communicate body contribution where practical:

`feet → legs → hips → torso/shoulder → arm → glove`

Do not build a full physics animation system for this task.

Use lightweight procedural/interpolated motion suitable for iPhone/WebGL.

## 8. Objective E — Recovery

Every attack must return to a plausible combat state:

```text
commit
  ↓
extend / attack
  ↓
impact or miss
  ↓
recover
  ↓
guard / reposition
```

No snapping to an unrelated pose.

No detached limb.

No duplicated shoulder.

No frozen limb after attack.

## 9. Objective F — Defensive Readiness

A full defensive system is NOT required in this round.

However, the current implementation must remain compatible with future:

- guard;
- slip;
- roll;
- retreat;
- angle exit.

Do not lock the head, torso or root into an architecture that makes those actions impossible later.

If an existing dodge/guard behavior already exists, preserve it.

## 10. Objective G — Remove Visual Artifacts

The previous UAT identified visual problems such as:

- unexplained yellow/orange flying object during player punch;
- duplicated/lagging-looking opponent shoulder during fast punch movement.

Inspect the actual rendering/transform pipeline and remove the root cause.

Do not simply hide a legitimate combat element without proving that it is a debug/visual artifact.

The authoritative combat endpoint and the visible glove must remain coherent.

## 11. Performance

Target platform remains:

- iPhone/WebGL;
- current Unity version;
- current project architecture.

Prefer:

- deterministic procedural movement;
- lightweight interpolation;
- no per-frame allocations;
- no unnecessary physics simulation;
- no expensive runtime IK unless already present and clearly justified.

Do not trade visual polish for a major performance regression.

Record frame-time/FPS evidence for the UAT candidate.

## 12. Architecture Boundary for Future PvP

Maintain this conceptual boundary:

```text
AI / future player / future network input
                  ↓
             combat intent
                  ↓
        movement/action controller
                  ↓
        fighter visual embodiment
                  ↓
              rendering
```

The current AI supplies intent.

Future PvP can replace the source of intent without rewriting fighter embodiment.

Do not implement the future network layer now.

## 13. Implementation Procedure

Follow this order:

### Step 1 — Inspect

Read the handoff documents and inspect:

- current combat-distance code;
- opponent movement/controller;
- current arm/leg embodiment;
- punch timing;
- hit detection;
- visual/debug effects;
- existing deterministic tests;
- build pipeline.

### Step 2 — Establish baseline

Run the current deterministic tests before modification.

Record baseline result.

### Step 3 — Implement distance foundation

Fix combat spacing and target relationship first.

### Step 4 — Implement opponent footwork

Add advance/retreat/lateral repositioning while preserving stance.

### Step 5 — Refine stance and recovery

Ensure the opponent looks like a boxer before, during and after attacks.

### Step 6 — Fix punch visual relationship/artifacts

Verify glove-to-target coherence and remove unexplained visual artifacts at their source.

### Step 7 — Regression

Run all relevant deterministic tests.

Add focused tests where the new behavior is deterministic and testable.

### Step 8 — Build

Produce the WebGL/iPhone UAT candidate.

### Step 9 — Smoke test

Verify:

- build loads;
- required files return successfully;
- no fatal startup errors;
- no console/runtime errors introduced by this task;
- performance remains acceptable.

### Step 10 — Evidence

Produce a concise result report with:

- source SHA;
- artifact SHA if applicable;
- Unity version;
- baseline tests;
- final tests;
- build marker/productVersion;
- WebGL hashes if available;
- performance evidence;
- M01–M12 gate status;
- exact UAT artifact/location.

## 14. Quality Gates

Use `docs/handoff/22-combat-animation-quality-gates.md`.

At minimum, verify:

- M01 Combat Distance
- M02 Stable Boxing Stance
- M03 Footwork
- M04 Weight / Body Connection
- M05 Punch-to-Target Relationship
- M06 Punch Family Readability
- M07 Recovery
- M08 Defensive Readiness
- M09 Visual Stability
- M10 First-Person Composition
- M11 Performance
- M12 Regression

The agent may declare **implementation/build PASS** only when the automated/build requirements are satisfied.

The agent must NOT declare human UAT PASS.

## 15. Human UAT Is the Final Gate

After implementation, provide the UAT candidate for human inspection.

The human reviewer will judge:

1. Do the two fighters look like they are actually boxing each other?
2. Is the combat distance believable?
3. Does the opponent move its feet rather than float?
4. Does the opponent maintain a natural boxing pose?
5. Do punches visually connect to the opponent?
6. Is there any unexplained yellow/orange projectile-like effect?
7. Is there any duplicated/jittering shoulder or limb?
8. Did the previous arm and opponent-leg embodiment regress?
9. Does the game remain responsive on iPhone?

## 16. Stop Conditions

STOP and report instead of expanding scope if:

- a required change would alter authoritative hit/damage geometry unexpectedly;
- a change would require PvP/networking;
- a change would require a major architecture rewrite;
- a visual issue cannot be diagnosed confidently;
- performance becomes materially worse;
- existing accepted systems regress.

Do not solve a scope problem by adding unrelated systems.

## 17. Final Rule

Do not optimize for passing a test suite while leaving the game visually unbelievable.

The purpose of this task is **boxing readability**.

The final UAT candidate must make a human observer immediately understand:

> two fighters are standing in a believable boxing range, moving their feet, maintaining guard, exchanging punches and recovering naturally.

Only after that condition is met should this task be considered ready for human UAT.
