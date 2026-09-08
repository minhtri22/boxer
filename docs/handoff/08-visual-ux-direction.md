# 08 — Visual & UX Direction

## Current vs target

Current implementation uses procedural primitive geometry to prove interaction, anatomy readability and combat mechanics. It is **not** the intended final visual fidelity.

Target direction: realistic/stylized 3D boxing presentation viewed from first person, with the opponent as the primary readable threat and the player's gloves/arms in foreground.

## Current visual responsibilities

The prototype must make these readable before production art:

- opponent guard;
- punch family;
- telegraph/preparation;
- arm topology;
- pelvis/leg topology;
- range/contact/miss;
- player guard/gloves;
- HP/stamina/status HUD.

## Body topology readability

Upper body:

`shoulder → upper arm → elbow joint → forearm → glove`

Opponent lower body:

`pelvis → thigh → knee joint → shin → foot`

Production 3D must preserve these semantics even when primitives are replaced by a rigged character.

## POV composition principles

- opponent occupies central combat focus;
- player's gloves/arms frame foreground without blocking opponent readability;
- ring/environment gives depth/range cues;
- HUD stays legible but secondary to combat body cues;
- telegraphs should be visible without UI dependence.

## North-star art direction

Recent concept art establishes a direction with:

- high-fidelity boxer opponent;
- realistic arena/ring lighting;
- dark/gold premium boxing UI language;
- strong fighter identity;
- POV gloves in foreground;
- concise combat HUD;
- aspirational career presentation.

Treat concept art as **north-star visual reference**, not current implementation acceptance.

## Transition to 3D production

Do not replace primitives with expensive production assets until the body/mechanics contracts are stable enough to avoid repeated rig/animation rework.

3D production should eventually map semantic joints/segments to a rig:

- shoulders, elbows, wrists;
- pelvis, knees, ankles;
- torso/head;
- gloves/feet.

## UX anti-goals

- no projectile-like visual for punches;
- no hidden combat effect that visually contradicts glove contact;
- no decorative animation that makes attack family unreadable;
- no oversized HUD that replaces body reading;
- no animation polish that masks unresolved mechanics.
