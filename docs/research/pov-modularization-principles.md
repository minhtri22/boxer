# POV Modularization Principles

## Requirement

All new Boxer systems should be implemented as modular, reusable building blocks where practical so the same foundations can later be cloned or adapted into other first-person / POV games.

Target future examples include:

- racing from inside the vehicle;
- aircraft combat from the cockpit;
- ninja / melee combat with threats approaching from front and sides;
- tactical assault / room-entry combat with enemies approaching from front and lateral sectors;
- other embodied POV games sharing head/camera, locomotion, action, opponent-awareness, HUD, progression, replay, and semantic-event needs.

The goal is NOT to build a generic engine prematurely. The goal is to avoid Boxer-specific coupling when a clean domain boundary already exists.

## Architectural rule

> GENERALIZE STABLE PRIMITIVES, SPECIALIZE GAMEPLAY RULES.

Do not generalize unproven mechanics. First prove a mechanic inside Boxer; once its interface and semantics are stable, isolate it behind a reusable module boundary.

## Proposed module families

### 1. POV Input / Embodiment

Reusable responsibilities:

- device orientation / head-look input;
- touch/gesture input;
- left/right control zones;
- calibration / neutral pose;
- input gating and onboarding hooks;
- action-intent events.

Boxer specialization:

- head movement = boxing evade/read;
- left thumb = boxing footwork;
- right thumb = punch family intent.

Future reuse:

- racing: steering / throttle / look;
- cockpit: pitch/yaw/fire / look;
- ninja: dodge / move / strike;
- tactical assault: lean / move / shoot / melee.

### 2. Actor State / Body State

Reusable responsibilities:

- transform / facing;
- movement state;
- action commitment state;
- recovery state;
- health/stamina capability slots;
- semantic actor state snapshots.

Boxer specialization:

- stance, guard, punch hand, punch family, balance, counter window.

### 3. Action Execution

Reusable responsibilities:

- intent → action request;
- windup / active / recovery phases;
- locked target / endpoint where appropriate;
- action cancellation rules;
- action semantic events.

Boxer specialization:

- jab/cross/hook/uppercut/overhand;
- range and biomechanics rules.

Future reuse:

- weapon fire;
- sword slash;
- aircraft missile launch;
- boost / brake / drift action.

### 4. Spatial Resolution / Contact

Reusable responsibilities:

- world-space segments/rays/arcs;
- target zones;
- range checks;
- hit / miss / block / intercept-style resolution;
- debug geometry and reason codes.

Boxer specialization:

- glove endpoint;
- head/body/guard target spheres;
- punch radius;
- counter geometry.

### 5. Opponent / Threat Model

Reusable responsibilities:

- target acquisition;
- threat direction sectors;
- commitment and telegraph state;
- attack intent;
- difficulty parameters;
- semantic opponent profile.

Important future requirement:

Support not only frontal threats but also lateral sectors, so a future ninja/tactical game can represent enemies approaching from:

- front;
- front-left / front-right;
- left / right;
- potentially rear sectors later.

Boxer can remain front-biased while the core threat interface should not hard-code a single forward attacker assumption if avoidable.

### 6. Readability / Feedback

Reusable responsibilities:

- action telegraph;
- hit / miss / block feedback channels;
- camera reaction;
- haptics;
- sound hooks;
- debug overlays;
- semantic outcome visualization.

Boxer specialization:

- arm/glove embodiment;
- punch impact readability.

### 7. Semantic Event Stream

Reusable responsibilities:

- timestamped action events;
- actor / target IDs;
- action type;
- start state;
- target state;
- result;
- reason code;
- important geometry values;
- replay/debug metadata.

This should be treated as a reusable infrastructure layer because it can later feed:

- replay/highlights;
- analytics;
- coach/AI-agent analysis;
- telemetry;
- scoring;
- progression systems.

Game-specific event payloads may extend a stable core schema.

### 8. Scoring / Outcome Adapter

Reusable responsibilities:

- consume authoritative semantic events;
- compute game-specific score/result through adapters;
- expose explainable score reasons.

Do not hard-code boxing scoring into the generic event system.

### 9. Progression / Career Shell

Potentially reusable later:

- player profile;
- rank / ladder;
- next challenge;
- result history;
- unlock state;
- lightweight customization state.

Game-specific layers:

- Boxer ranking / coach report;
- racing championship standings;
- aircraft mission ladder;
- ninja campaign / challenge progression.

Do not extract this until Boxer launch loop is proven.

### 10. Presentation Adapter

Keep authoritative mechanics separate from visual representation.

Examples:

- current procedural primitives;
- later Blender 3D boxer;
- cockpit model;
- car interior;
- sword/hand models.

A presentation swap should not require rewriting core combat/action resolution.

## Dependency direction

Preferred direction:

`Input → Intent → Actor/Action State → Spatial Resolution → Semantic Events → Score/Feedback/Replay`

Presentation observes these systems but should not own authoritative mechanics.

Avoid circular dependencies such as:

`visual glove transform → authoritative combat decision`

when the visual object exists only for presentation.

## What to modularize now

During current Boxer research, favor clear boundaries for:

- input and gesture classification;
- actor/action state;
- opponent attack commitment;
- spatial hit resolution;
- semantic combat events;
- debug/readability interfaces;
- onboarding state hooks.

## What NOT to modularize prematurely

Do not build generic frameworks yet for:

- universal vehicle physics;
- generic weapons systems;
- generic AI behavior trees;
- universal RPG stats;
- economy/shop framework;
- hospital/injury framework;
- multiplayer/networking;
- generic 3D character system.

These should be extracted only after at least one real second use case proves that the abstraction is useful.

## Extraction rule

A Boxer subsystem becomes a reusable POV module only when:

1. its Boxer behavior is proven;
2. its inputs/outputs are explicit;
3. game-specific rules can be separated from stable infrastructure;
4. a second use case can consume it without copying internal Boxer assumptions.

If these conditions are not met, keep it local and well-encapsulated rather than creating a false generic engine.

## Long-term direction

The intended result is not merely a Boxer codebase. Boxer should gradually act as the first proven vertical slice of a reusable POV-game foundation.

Possible future structure:

- `POV.Core`
- `POV.Input`
- `POV.Actor`
- `POV.Action`
- `POV.Spatial`
- `POV.Threats`
- `POV.Feedback`
- `POV.Events`
- `POV.Replay`
- game-specific adapters such as `Boxer.Gameplay`

Do not physically split assemblies/packages until the boundaries are stable enough to justify it.

## Rule

> PROVE LOCALLY → DEFINE THE INTERFACE → EXTRACT ONLY STABLE PRIMITIVES.

This keeps Boxer fast to iterate while preserving a path toward reusable POV foundations for future games.