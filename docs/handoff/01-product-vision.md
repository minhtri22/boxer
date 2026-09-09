# 01 — Product Vision

## Product thesis

Boxer is a **POV boxing career simulator** whose defining experience is embodiment rather than external character control.

> **The player is the boxer.**

The long-term product should make a player feel that reading distance, moving the head, managing feet, choosing a punch family, creating an opening and recovering to guard are one coherent embodied action.

## Core fantasy

**Fight from your own eyes.**

The player should experience the opponent as a readable physical threat at human scale: stance, guard, telegraph, range, committed attack, opening, counter opportunity, recovery.

## Desired combat loop

`SEE → READ → MOVE / GUARD / EVADE → CREATE OPENING → COUNTER → RESET`

The system should reward timing, range and body coordination more than input volume.

## Anti-goals

Do not evolve Boxer into:

- a tap/swipe spam fighter;
- an outside-camera boxing avatar game;
- a damage-number game where body mechanics are decorative;
- a cinematic replay product before fights generate meaningful semantic events;
- a progression shell built on shallow combat.

Anti-loop:

`SWIPE → SWIPE → SWIPE → SPAM → WIN`

## Experience pillars

### 1. Embodiment
Phone, thumbs and camera perspective should map onto understandable parts of the boxer body.

### 2. Combat readability
Opponent actions must be legible before outcome resolution. The player should understand why a hit, block, miss or counter occurred.

### 3. Causal body mechanics
Movement and punch families should gradually become connected through range, stance, hip, weight and recovery rather than independent animation channels.

### 4. Learnability
The game may eventually teach boxing intuition through play: range, guard, timing, openings, recovery, countering.

### 5. Replayable semantics
Every meaningful combat action should eventually produce enough structured semantic data to reconstruct/replay a fight and generate highlight/KO clips.

## Long-term product path

Conceptual path, not current implementation scope:

`POV fight → career identity → opponents → training/progression → semantic fight history → replay/highlight/KO clips → shareable career stories`

## Current product boundary

The current project is still validating the embodied combat kernel. Career, ranking, economy, equipment, deep damage simulation and replay generation are future layers and must not drive current mechanics prematurely.

## Success criterion for the combat kernel

A player should be able to say:

> “I won because I read the opponent, moved into the right position, created an opening and countered.”

rather than:

> “I won because I swiped faster.”
