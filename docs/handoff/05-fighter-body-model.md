# 05 — Fighter Body Model

## Purpose

This document freezes the current semantic body topology and anthropometric baseline used by the procedural prototype. It is not a final art-rig specification.

## Upper body topology

Each visible arm has exactly two limb segments:

`SHOULDER JOINT → UPPER ARM → ELBOW JOINT → FOREARM → GLOVE`

Hard rule:

> **The elbow is a joint/pivot, not a limb segment.**

Per-arm topology must never become shoulder → segment → segment → segment → glove.

### Frozen upper-body constants

- Body height: `1.80m`
- Shoulder height: `1.43m`
- Shoulder width: `0.38m`
- Upper arm length: `0.34m`
- Forearm length: `0.31m`
- Total shoulder-to-wrist visual arm length: `0.65m`

Visual hierarchy currently uses upper arm slightly thicker than forearm to improve mobile readability.

### Guard presentation

Current opponent guard has been iteratively tightened so elbows sit closer to ribs and gloves are raised/inset with mild lead/rear asymmetry. These are presentation parameters, not authoritative combat variables.

## Lower body topology — opponent in POV combat

Current opponent lower body:

`PELVIS → THIGH → KNEE JOINT → SHIN → FOOT`

Each leg has exactly two limb segments:

1. thigh: hip/pelvis → knee
2. shin: knee → ankle/foot

Hard rule:

> **The knee is a joint/pivot, not a limb segment.**

### Frozen lower-body constants

- Pelvis height: `0.92m`
- Hip width: `0.26m`
- Thigh length: `0.46m`
- Shin length: `0.44m`
- Foot length: `0.22m`
- Knee radius: `0.055m`
- Thigh radius: `0.075m`
- Shin radius: `0.055m`

The old visible rectangular lower-body pedestal is no longer the intended body representation.

## Scope policy

### Current POV combat

- Opponent upper body: REQUIRED
- Opponent lower body: REQUIRED
- Player visible arms/gloves: REQUIRED
- Player legs/full lower body: OUT OF SCOPE

### Future replay / cinematic / KO clips

A replay/cinematic system will eventually require a complete player body as well as opponent body. Do not add player legs to current POV gameplay solely for future replay needs.

## Current limitation

The current body is readable and accepted for the milestone, but motion is still intentionally primitive. Full softness, shoulder/scapula articulation, pelvis-driven movement, stance mechanics and weight transfer are next-stage biomechanics work.

## Next body-mechanics questions

- How much hip rotation should accompany each punch family?
- How should pelvis/torso rotation lag or lead arm extension?
- How should weight shift affect reach/power/recovery?
- How should stance recover after commit?
- How can body state remain semantically replayable?
