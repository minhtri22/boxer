# Boxer — Combat Animation Quality Gates

## Purpose

These gates define the visual acceptance criteria for UAT Round 2.

They supplement the implementation plan and must be evaluated in addition to deterministic tests.

## Scope

Current round:

- AI opponent only.
- No PvP networking.
- No replay/clip-generation system.
- No player-leg embodiment yet unless explicitly required by an existing regression fix.
- Opponent lower-body movement is in scope.
- Existing player arm embodiment must remain intact.

## Gate M01 — Combat Distance

PASS when:

- player and opponent occupy a believable boxing distance;
- opponent does not appear several invisible meters away from the player's punch envelope;
- normal punches visually travel toward the opponent's body/head;
- the opponent can move into and out of effective range;
- the camera does not create the impression that both fighters are punching empty space.

FAIL examples:

- glove stops short while a separate effect appears to hit;
- fighters remain visually disconnected despite hit events;
- opponent never adjusts distance.

## Gate M02 — Stable Boxing Stance

PASS when the opponent continuously reads as a boxer:

- stable base;
- knees/legs support body weight;
- one side naturally leads;
- torso has controlled rotation;
- guard is compact;
- feet remain meaningfully connected to the floor.

FAIL:

- mannequin stance;
- straight locked legs;
- arms spread laterally;
- perfectly symmetrical pose during every state;
- body appears to float above feet.

## Gate M03 — Footwork

PASS when the opponent visibly performs controlled:

- advance;
- retreat;
- small lateral repositioning;
- stance reset.

Footwork must preserve the fighter's base.

FAIL:

- root slides while feet remain visually planted;
- feet cross unnaturally;
- opponent remains stationary while the player changes distance;
- movement looks like a character sliding on ice.

## Gate M04 — Weight / Body Connection

PASS when attack movement reads through the body:

`feet → legs → hips → torso/shoulder → arm → glove`

The effect may be subtle. It must not become exaggerated.

FAIL:

- arm-only punch;
- body remains completely rigid while the glove launches;
- recovery produces an impossible weight distribution.

## Gate M05 — Punch-to-Target Relationship

PASS when the viewer can visually connect:

`attacker → glove trajectory → opponent target → impact`

A hit must not depend on a detached projectile-like visual effect to explain contact.

Existing combat geometry remains authoritative.

FAIL:

- unexplained yellow/orange flying object;
- detached impact proxy;
- glove visually misses while the game registers a hit.

## Gate M06 — Punch Family Readability

Existing punch families remain distinct:

- STRAIGHT
- HOOK
- UPPERCUT
- OVERHAND

Do not sacrifice their existing readability while adding movement.

PASS requires that each family remains visibly distinguishable from the others.

## Gate M07 — Recovery

After an attack:

```text
commit → extend / impact → recover → guard
```

The opponent must return to a plausible boxing stance.

FAIL:

- snap to unrelated pose;
- detached limbs;
- duplicated shoulder/arm image;
- frozen limb after punch.

## Gate M08 — Defensive Readiness

Current round does not require a full slip/roll system.

However, the pose system must not make future defensive movement impossible.

At minimum:

- guard remains usable;
- opponent can reposition without breaking guard topology;
- head/torso are not locked to a single rigid transform;
- recovery leaves the fighter ready for another action.

## Gate M09 — Visual Stability

During fast movement and punches:

- no duplicate shoulder image;
- no detached glove;
- no one-frame limb teleport visible to the player;
- no jitter caused by competing transform writers;
- no camera-relative artifact.

This gate is specifically important because the current prototype previously showed a duplicated/lagging shoulder impression during opponent punches.

## Gate M10 — First-Person Composition

Use the approved POV reference assets as the visual target.

Check:

- opponent apparent size;
- head/torso placement;
- glove scale;
- combat distance;
- ring/background relationship;
- player gloves remain visually prominent.

Do not add a new HUD composition in this task.

## Gate M11 — Performance

Target remains iPhone/WebGL.

PASS:

- no meaningful frame-rate regression;
- no visible stutter introduced by movement;
- no per-frame allocation pattern introduced;
- no unnecessary runtime physics/IK cost.

Where measurable, record FPS/frame-time evidence in the result report.

## Gate M12 — Regression

All previously accepted systems must remain functional, including:

- P0 controls;
- A1/A2 behavior;
- A3.1 hook geometry;
- arm embodiment;
- shoulder → upper arm → elbow → forearm → glove topology;
- opponent legs/pelvis embodiment;
- hit detection;
- punch timing;
- stamina/HP;
- counter behavior;
- winner/end state;
- WebGL build.

## Gate M13 — Human UAT

The agent must NOT claim human UAT PASS.

It may claim implementation PASS only after automated/build gates succeed.

Human UAT remains the final acceptance authority.

The human reviewer should answer:

1. Do the two fighters now look like they are actually boxing each other?
2. Is the distance believable?
3. Does the opponent move its feet rather than float?
4. Does the opponent maintain a natural boxing pose?
5. Do punches visually connect to the opponent?
6. Is there any unexplained projectile/effect?
7. Is there any visible duplicate/jittering body part?
8. Did the previous arm/leg embodiment regress?

## Required Evidence

Before handoff, the agent must provide:

- source commit SHA;
- Unity version;
- deterministic test result;
- build result;
- build provenance marker;
- WebGL artifact hash if available;
- performance/frame-time evidence;
- concise explanation of each M01–M12 gate;
- exact UAT URL/artifact location.
