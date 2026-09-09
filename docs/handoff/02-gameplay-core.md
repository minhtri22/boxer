# 02 — Gameplay Core

## Status language

Every mechanic in this document should be interpreted using one of four statuses:

- **VERIFIED** — implemented and accepted by tests/UAT for the current milestone.
- **PROVISIONAL** — implemented for research/legibility but not final product design.
- **PLANNED** — product direction, not current implementation.
- **LOCKED / NOT STARTED** — explicitly out of scope until opened.

## Current round/combat shell

The current build provides a short playable boxing bout used to validate controls, readability and mechanics. Current HP/stamina/HUD semantics are sufficient for testing but are not the final damage/fatigue/economy model.

### Current player responsibilities

- read opponent telegraph;
- manage range and position;
- move head through phone orientation;
- move feet with left-thumb input;
- select punch family with right-thumb gesture;
- guard by returning to neutral/no active attack;
- exploit opponent commitment/recovery to counter.

## Combat action lifecycle

At a high level:

`GUARD/IDLE → INTENT → COMMIT → EXTEND → RESOLVE → RECOVER → GUARD`

Outcome can be HIT, BLOCK or MISS; a counter state can modify/label context depending on current implementation.

## Current verified mechanics

### Guard
- Idle/no active action returns to high guard.
- Guard is a combat state, not permanent free invulnerability.
- Opponent and player guard visuals are presentation layers over combat state.

### Movement
- Left thumb expresses directional movement/footwork intent.
- Existing movement already changes relative positioning.
- Current opponent legs visually embody movement, but full boxing footwork/weight transfer is not yet verified.

### Punch families
- Straight
- Hook
- Uppercut
- Overhand

Family selection comes from gesture; hand is selected contextually.

### Range
- P1-A1 verifies a small categorical step-direction effect on straight reach.
- P1-A3.1 verifies hook close-range sensitivity.
- P1-A3.2 makes moving uppercuts lose vertical drive while planted uppercuts retain it.
- P1-A3.3 makes advancing overhands recover longer after forward commitment.
- Opponent attack reach is finite and target is locked at commitment; no active homing during punch.

### Counter
Counter opportunity requires a geometric evade: the locked opponent attack path must change from would-hit at commit to miss at resolve due at least `0.08m` of head or root displacement. The opportunity is active only during opponent Recover and is consumed by one successful player hit.

## Provisional systems

### HP
Current HP is adequate for reactive testing/closure. It is not a frozen long-term physiological damage model.

### Stamina
Current stamina drains/recovers for HUD/test semantics and is regression-protected. Do not treat current thresholds as final sports simulation design.

### Bout duration / winner closure
Current closure exists to support UAT. Career rules, round scoring and professional boxing rule fidelity are not finalized here.

## Not currently active

Do not start unless explicitly opened:

- final damage model;
- injury model;
- knockdown/standing-count simulation;
- referee/scoring simulation;
- career economy;
- equipment stats;
- ranking ladder;
- replay/KO clip generator;
- player full-body legs for POV combat.

## Design principle

Whenever a new rule is proposed, ask:

1. Does it help the player understand **why** an outcome happened?
2. Does it strengthen the embodied loop rather than input spam?
3. Is the causal variable isolated enough to test?
4. Can it be instrumented and replayed semantically later?
