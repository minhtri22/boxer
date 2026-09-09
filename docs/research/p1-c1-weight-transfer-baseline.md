# P1-C1 — Weight Transfer Baseline

## Status

`FROZEN_FOR_IMPLEMENTATION`

## Unresolved question

What is the smallest useful, deterministic state variable for weight transfer that can be observed, replayed, and later coupled to one combat consequence without introducing force/COM simulation?

## Hypothesis

A single normalized `forward_load` scalar is sufficient as the first weight-transfer state. It can describe a small preload → drive → recovery cycle while leaving authoritative combat unchanged.

## Frozen state

`forward_load` is clamped to `[0, 1]`:

- `0.50` = neutral balanced stance.
- Commit moves smoothly from `0.50` to `0.42` (small backward preload).
- Extend moves smoothly from `0.42` to `0.62` (forward drive).
- Recover moves smoothly from `0.62` to `0.50`.
- Guard is exactly `0.50`.
- Punch family and hand are intentionally ignored in C1.

For presentation only, the state may produce a tiny forward/back shoulder/waist offset using a fixed `0.25 m` scale per unit load delta. This yields at most `-0.02 m` preload and `+0.03 m` forward shift under the frozen values.

## Acceptance

1. Guard is exactly neutral.
2. Commit ends rearward of neutral at `0.42`.
3. Extend begins continuously at the commit endpoint and ends forward at `0.62`.
4. Recover begins continuously at the extend endpoint and returns exactly to `0.50`.
5. State is family- and hand-independent in this baseline.
6. State and derived presentation offset are finite and bounded.
7. Prior deterministic regressions plus C0 remain PASS.

## Authority boundary

C1 is diagnostic/presentation state only. It must not change:

- hit-test geometry;
- punch endpoints/reach;
- damage/power;
- punch timing;
- stamina;
- opponent AI;
- guard/counter rules.

## UAT policy

Standalone C1 UAT is deferred by product-owner direction. C1 may advance as `DETERMINISTIC_PASS`; visual/feel judgment is part of the single final integrated UAT candidate.
