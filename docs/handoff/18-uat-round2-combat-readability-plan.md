# Boxer UAT Round 2 Combat Readability Plan

## Purpose

This document defines the work required before the next human UAT. The current AI opponent remains the opponent for this phase. PvP architecture is documented separately but is NOT implemented in this round.

## Current Problem Observed

The fighter body model is improved, but combat still lacks boxing spatial readability:

- fighters can feel separated
- punches can look like hitting empty space
- opponent lacks natural repositioning
- stance and distance do not communicate a real boxing exchange

## UAT Round 2 Goals

### 1. Combat Distance

Use approved POV boxing reference as visual target.

Target:
- default boxing range
- opponent readable in center combat zone
- player gloves remain visible
- punches should visually approach opponent body/head area

Distance states:
- Long range
- Boxing range (default)
- Close range

Do not change hit logic without validation. Improve visual and positioning coherence first.

### 2. Opponent Positioning

Opponent must no longer behave as a static target.

Add:
- small advance
- small retreat
- lateral adjustment
- distance maintenance

Goal:
The opponent should appear to fight the player, not stand in place.

### 3. Boxing Pose

Reference:
- compact guard
- slight body rotation
- natural shoulder position
- weight distributed between legs
- readable stance

Avoid:
- flat mannequin pose
- open arms
- floating feeling

### 4. Movement Foundation

This round does NOT require full boxing AI.

Only implement enough movement to support visual realism:
- stance movement
- body shift
- foot placement
- recovery to guard

## Future PvP Direction

The final architecture targets player vs player.

However:
- do not implement networking now
- do not replace AI opponent now
- keep current opponent controller reusable

Future:
Opponent input source can be replaced by network player input.

## Acceptance Criteria

PASS when:
- two fighters visually occupy a believable boxing distance
- punches look like they target the opponent
- opponent no longer feels like a floating stationary object
- stance and movement communicate boxing
- no regression of arm embodiment
