# Local Agent Execution Prompt — Boxer UAT Round 2

You are implementing Boxer UAT Round 2.

Read first:
- docs/handoff/README-HANDOFF.md
- docs/handoff/18-uat-round2-combat-readability-plan.md
- docs/handoff/19-pvp-architecture-direction.md
- docs/handoff/reference-ui/

## Important Scope Rule

This phase still uses AI opponent.

DO NOT implement PvP networking.

DO NOT redesign the game architecture.

Improve the current opponent into a believable boxing training opponent.

## Objectives

### 1. Combat Distance

Fix the feeling that fighters are punching empty space.

Implement:
- believable boxing range
- opponent positioning
- distance maintenance
- close/mid/long range states if needed

Use approved combat POV reference as visual target.

### 2. Opponent Movement

Opponent currently punches but feels static.

Add:
- advance
- retreat
- small lateral movement
- stance adjustment

Movement should be subtle and readable.

Avoid unrealistic sliding.

### 3. Boxing Pose

Opponent must maintain:
- compact guard
- slight torso rotation
- natural boxing stance
- weight distribution
- readable feet placement

### 4. Preserve Existing Systems

Do not break:
- arm chain
- shoulder/elbow/forearm/glove embodiment
- hit detection
- existing tests
- iPhone WebGL build

## Validation

Before reporting completion:

1. Build successfully.
2. Run deterministic tests.
3. Provide evidence.
4. Produce WebGL UAT build.
5. Only then request human UAT.

## Future Direction

Keep opponent controller modular because future PvP will replace AI input with network player input.
