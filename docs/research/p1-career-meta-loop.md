# Boxer — Career / Meta-Game Loop

## Purpose

Core combat proof is necessary but not sufficient for the intended Boxer product. After the combat model is trustworthy, the game also needs a persistent career/meta loop that gives fights meaning outside the ring.

This layer is distinct from the current P1 mechanics proof and should not be allowed to obscure unresolved combat causality.

## Launch-loop reviewer decision

The first public/release candidate should stay intentionally small. The product must first prove that players want to repeat the fight loop before expensive meta systems are expanded.

Recommended launch loop:

`HOME → OPPONENT PREVIEW → FIGHT → RESULT / SCORE → COACH REPORT → RANK CHANGE → NEXT OPPONENT`

Product question to prove:

> After winning or losing, does the player want to press NEXT FIGHT?

### Launch-required systems

1. **Fight**
   - authoritative core combat;
   - readable result and scoring;
   - enough opponent variation to create a reason to adapt.

2. **Ranking / Career progression**
   - lightweight persistent ladder;
   - win/loss/result causes an understandable rank change;
   - no need for complex divisions, seasons, belts, matchmaking economy, or weight-class simulation at launch.

3. **Coach Report — lightweight**
   - post-fight analysis from authoritative semantic combat data;
   - initially deterministic/rule-based rather than AI-agent driven;
   - should explain what worked, what failed, and one or two tactical priorities for the next bout.

4. **Character personalization — lightweight identity layer**
   - available before launch so the boxer feels like the player's own character;
   - support both male and female boxer presentation;
   - identity/cosmetic only: no hidden combat-stat consequences;
   - keep each option set deliberately tiny, maximum three choices per category.

   Initial personalization envelope:

   - sex/presentation: male / female;
   - face preset: up to 3;
   - hair: up to 3;
   - eyes / eye appearance: up to 3;
   - facial hair: up to 3 states/styles where applicable, including clean-shaven;
   - other appearance categories should not be added before launch unless they are nearly free to support.

   The purpose is identity, not an avatar editor product.

### Explicit launch simplification: standardized boxer body envelope

The launch build should NOT simulate player-selected:

- weight class;
- body weight;
- height;
- reach / arm span;
- body size advantages;
- short-vs-large fighter matchup geometry.

For the initial release, player and opponents should remain within a deliberately narrow, comparable physical envelope so that combat fairness and mechanics are not confounded by anthropometric variation.

Reason:

A short fighter versus a much taller/heavier/longer-reach fighter immediately creates additional systems that would need separate proof: reach normalization, collision/body geometry, movement speed, power scaling, targeting, camera framing, opponent balance, matchmaking, and scoring interpretation.

Therefore launch principle:

> SAME COMPARABLE PHYSICAL CLASS FIRST → BODY-SIZE DIVERSITY ONLY AFTER COMBAT IS PROVEN.

Later, weight classes, height/reach archetypes, and heterogeneous body types may be introduced as separate controlled gameplay systems if player feedback shows they are valuable.

## Systems deferred until post-launch evidence

1. **Shop / Equipment**
   - buy gloves, apparel, protective gear, training items, and later cosmetic items;
   - equipment effects must be explicit and bounded;
   - do not let shop stats silently override or hide the proven combat model;
   - economic progression should create choices, not pay-to-win mechanics by default;
   - DEFER until players demonstrate demand for progression/economy/customization beyond the lightweight launch identity layer.

2. **Gym / Practice**
   - optional practice/training hub;
   - onboarding remains first-time only, not a forced pre-fight ritual;
   - practice should teach or reinforce actual combat mechanics;
   - DEFER the full gym hub until the repeat-fight loop is proven.

3. **AI Coach Meeting**
   - future AI-coach concept: an AI agent may read semantic fight logs/replays, analyze the player's recent bouts, profile an upcoming opponent, identify tendencies, and suggest a fight plan;
   - AI tactical advice must be grounded in authoritative combat data rather than invented narrative explanations;
   - launch uses a deterministic coach report first; AI agent integration is feedback-gated.

4. **Hospital / Recovery**
   - recovery from accumulated injuries or fight damage;
   - meaningful only after health, damage, injury, and recovery formulas are authoritative;
   - may later create time/cost/risk trade-offs in career mode;
   - DEFER strongly before launch.

5. **Mini-games / Supporting Activities**
   - optional activities only when they reinforce boxer skill, career identity, or a validated retention need;
   - examples may include bag work, reflex drills, roadwork, mitt work, recovery activities, weigh-in/media events;
   - do not add unrelated filler;
   - DEFER until real player behavior shows a clear need.

## Recommended product order

The product has four practical stages:

### Stage A — Combat Truth

Prove first:

1. body / whole-body mechanics;
2. punches + range + HIT/MISS/BLOCK;
3. scoring;
4. health / force / stamina.

### Stage B — Prove the repeat loop

Before broad meta-game scope, ship/test the minimal loop:

`OPPONENT PREVIEW → FIGHT → RESULT / SCORE → COACH REPORT → RANK CHANGE → NEXT FIGHT`

Include lightweight character personalization as an identity layer, but keep physical fighter parameters standardized.

### Stage C — Career / Meta expansion

Only after feedback/retention justifies it, consider:

- Practice Gym;
- opponent scouting;
- career history;
- AI Coach;
- Shop / equipment economy;
- Hospital / injury;
- mini-games;
- broader ranking/division systems;
- body-size / reach / weight-class diversity as separately proven systems.

### Stage D — 3D Production / Presentation

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

## Rules

> PROVE THE FIGHT → PROVE THE REPEAT LOOP → BUILD THE CAREER WORLD → POLISH THE WORLD.

> PERSONALIZE IDENTITY EARLY; STANDARDIZE BODY ADVANTAGE EARLY.

Do not let shop, rankings complexity, hospital, coach AI, body-size simulation, or mini-games become a substitute for unresolved core boxing mechanics.