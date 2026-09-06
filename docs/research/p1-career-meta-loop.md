# Boxer — Career / Meta-Game Loop

## Purpose

Core combat proof is necessary but not sufficient for the intended Boxer product. After the combat model is trustworthy, the game also needs a persistent career/meta loop that gives fights meaning outside the ring.

This layer is distinct from the current P1 mechanics proof and should not be allowed to obscure unresolved combat causality.

## Core meta-game systems

1. **Shop / Equipment**
   - buy gloves, apparel, protective gear, training items, and later cosmetic items;
   - equipment effects must be explicit and bounded;
   - do not let shop stats silently override or hide the proven combat model;
   - economic progression should create choices, not pay-to-win mechanics by default.

2. **Gym / Coach Meeting**
   - training hub for practice, preparation, and tactical planning;
   - may eventually host coach conversations and fight preparation;
   - future AI-coach concept: an AI agent may read semantic fight logs/replays, analyze the player's recent bouts, profile an upcoming opponent, identify tendencies, and suggest a fight plan;
   - AI tactical advice must be grounded in authoritative combat data rather than invented narrative explanations.

3. **Hospital / Recovery**
   - recovery from accumulated injuries or fight damage;
   - should become meaningful only after health, damage, injury, and recovery formulas are authoritative;
   - may create time/cost/risk trade-offs in career mode.

4. **Rankings / Career Ladder**
   - persistent ranking, opponent progression, and bout eligibility;
   - ranking changes must derive from explicit results/scoring rules;
   - later career state may include records, titles, divisions, streaks, and matchmaking constraints.

5. **Mini-games / Supporting Activities**
   - optional activities that reinforce boxer skills or career identity;
   - examples may include bag work, reflex drills, roadwork, mitt work, recovery activities, weigh-in/media events, or other lightweight career interactions;
   - mini-games should reuse or teach core mechanics where possible instead of becoming unrelated filler.

## Recommended product order

The product now has three layers:

### Layer A — Combat Truth

Prove first:

1. body / whole-body mechanics;
2. punches + range + HIT/MISS/BLOCK;
3. scoring;
4. health / force / stamina.

### Layer B — Career / Meta Loop

After the required combat state is authoritative enough to support it, implement the persistent loop:

`PREPARE → TRAIN / COACH → FIGHT → SCORE / RANK → RECOVER → SHOP / UPGRADE → NEXT FIGHT`

The five systems in this document belong to this layer.

### Layer C — 3D Production / Presentation

Production-quality Blender assets, character rigs, environments, animation polish, replay cameras, and visual identity come after the important combat rules are proven. Meta-game prototypes may begin before final 3D, but they should remain lightweight until their underlying combat/economy dependencies are stable.

## AI Coach design principle

Future AI coach architecture should prefer:

`semantic fight events + opponent profile + player history → analysis → tactical recommendation`

rather than:

`video/game state → free-form guess`

Potential inputs:

- punch-family frequency;
- lead/rear hand patterns;
- range distribution;
- HIT/MISS/BLOCK rates;
- counter success;
- movement direction;
- defensive habits;
- round scoring;
- fatigue/injury state once authoritative;
- opponent tendencies from prior data.

Potential outputs:

- preferred engagement range;
- punches to prioritize/avoid;
- defensive pattern;
- counter opportunities;
- pace/fatigue plan;
- round-by-round tactical adjustment.

This is a later system, not part of the current P1 implementation slice.

## Rule

> PROVE THE FIGHT → BUILD THE CAREER LOOP → POLISH THE WORLD.

Do not let shop, rankings, hospital, coach AI, or mini-games become a substitute for unresolved core boxing mechanics.