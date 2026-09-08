# 06 — Opponent AI

## Separation of concerns

Keep these systems distinct:

- **AI decision** — what the opponent chooses to do.
- **Combat authority** — whether an action hits/blocks/misses/counters.
- **Presentation** — how stance, telegraph, arm/leg motion and impact look.

Do not fix one layer by mutating another without an explicit experiment.

## Current opponent behavior contract

The opponent currently supports readable attack/guard behavior sufficient for UAT:

- chooses/executes attack intents;
- presents a preparation/telegraph before attack;
- faces the player before commit;
- locks attack target at start;
- does not home toward the live player during committed punch;
- has finite punch reach;
- recovers after attack;
- maintains a readable guard state;
- moves relative to the player.

## Human-readability requirements

An opponent attack should read as:

`STANCE/GUARD → PREPARE → COMMIT → EXTEND → RESOLVE → RECOVER`

The player should be able to perceive the threat before resolution. Telegraph readability is a regression-protected property.

## Current body embodiment

Opponent arms and legs are now visually present and accepted for the current milestone. Existing movement can therefore be read against a body rather than a sliding torso/pedestal.

This does **not** yet mean the opponent has validated boxing footwork or weight transfer.

## Next AI/body boundary

When adding hip/weight mechanics, avoid making the AI itself more complicated in the same experiment. Prefer:

1. keep AI intent selection frozen;
2. change one body-coupling variable;
3. compare readability/outcomes;
4. only later change tactical AI.

## Future opponent dimensions — planned, not active

Possible later attributes:

- aggressiveness;
- preferred range;
- punch-family bias;
- counter tendency;
- guard discipline;
- stamina conservation;
- pressure vs outside fighting;
- recovery vulnerability;
- personality/career identity.

These are not current implementation requirements.
