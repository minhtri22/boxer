# P1-A3.3 — Forward-Commitment → Overhand Recovery Exposure

## Question

What is the smallest player-controlled commitment consequence that makes an overhand tactically distinct?

## Frozen hypothesis

An overhand started while the player is advancing represents a forward-committed attack. It keeps the existing trajectory and resolution geometry, but takes longer to return to guard.

Frozen intervention:

- family: OVERHAND only;
- punch-start `StepState=ADVANCING` → `FORWARD_COMMITTED`;
- forward-committed recovery multiplier = `1.20`;
- neutral or retreating overhand recovery multiplier = `1.00`;
- base player recovery = `0.280s`;
- forward-committed overhand recovery = `0.336s`.

Only the Recover phase duration changes. Commit/Extend timing, punch endpoint, hit radius, resolution threshold, damage, stamina, guard geometry, counter logic, AI, A1, A3.1 and A3.2 remain unchanged.

## Why this variable

The existing design requires overhand to reward commitment while exposing recovery. Forward left-thumb input already closes distance and is frozen into `StepState` when the punch begins. Extending only the recovery phase turns that existing player-controlled commitment into an inspectable tactical risk without inventing force, damage or balance simulation.

## Presentation agreement

The authoritative action state and visible arm return use the same effective recovery duration. The visible glove therefore cannot reach guard before the action state does.

## Observability

The semantic P1 punch event records:

- `A33_MODE=OVERHAND_RECOVERY` for overhands;
- `A33_COMMIT=FORWARD_COMMITTED|UNCOMMITTED`;
- `A33_RECOVERY=<multiplier>`.

The F3 inspector displays the active recovery duration.

## Acceptance

1. Non-overhands keep recovery multiplier `1.00`.
2. Neutral overhands keep recovery multiplier `1.00`.
3. Retreating overhands keep recovery multiplier `1.00`.
4. Advancing overhands use recovery multiplier `1.20`.
5. Effective forward-committed recovery is exactly `0.336s` from the frozen base.
6. Commit and Extend durations remain unchanged.
7. Semantic event mode/commit/factor fields are deterministic.
8. All prior 108 deterministic gates remain PASS.

Standalone human UAT is deferred by product-owner direction. This milestone may close as `DETERMINISTIC_PASS_UAT_DEFERRED`; final feel/readability is judged in the single integrated UAT candidate with approved UI/UX and visuals.
