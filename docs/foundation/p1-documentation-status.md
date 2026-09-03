# P1 Documentation Status and Concurrency Rule

## Status

**DOCUMENTATION MAY PROCEED IN PARALLEL WITH P0. P1 IMPLEMENTATION REMAINS LOCKED UNTIL P0 PASS.**

This file exists to prevent parallel research/design work from being mistaken for authorization to build P1 systems while Phase 0 is active.

## Current rule

- Codex or another implementation agent may work on **Phase 0 — Boxer Interaction Integration Proof**.
- A separate research/design agent may continue P1 documentation in parallel.
- No P1 gameplay implementation, engine extraction, career implementation, or production-system work is authorized before P0 PASS.

## P1 control complexity principle

> **Player controls intent; simulation resolves anatomy.**

The left thumb does not map to the left leg. The right thumb does not map to the right hand.

- Left Thumb = footwork / lower-body intent.
- Right Thumb = punch / upper-body intent.
- Phone = head intent.

The simulation must resolve stance-correct anatomical actions, including which foot initiates/follows, which arm executes, pivot, hip/trunk rotation, weight transfer, stance preservation, and balance consequences.

Detailed normative design is in:

- `docs/foundation/p1-input-derived-biomechanics.md`
- `docs/foundation/p1-master-mechanics-matrix.md`
- `docs/foundation/p1-combat-event-timeline-and-replay.md`
- `docs/foundation/p1-combat-foundation-requirements.md`
- `docs/research/p1-boxing-biomechanics-and-physiology.md`

## Current P1 documentation completeness

Existing:

1. research review,
2. combat foundation requirements,
3. master mechanics matrix,
4. deterministic event/replay/highlight foundation,
5. input-derived biomechanics and intent-to-anatomy resolver.

Still to complete before P1 design is called complete:

1. `p1-combat-state-and-energy-model.md`,
2. `p1-hit-defense-resolution.md`,
3. consistency review across all P1 documents,
4. final QA/simulation plan and explicit P1 PASS/FAIL gates.

These documentation tasks may continue during P0, but implementation remains locked.