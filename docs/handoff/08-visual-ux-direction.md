# 08 — Visual & UX Direction

## Authority status

The project now has a **human-approved visual reference set** in `docs/handoff/reference-ui/`. These assets are **binding UAT visual authority**, not optional moodboard material.

See:
- `reference-ui/README-VISUAL-REFERENCE.md`
- `reference-ui/ui-reference-notes.md`
- `reference-ui/visual-lock.yaml`

For current combat UAT, `reference-ui/03-pov-combat-hud.jpg` is the primary reference.

## Current vs target

Current implementation uses procedural primitive geometry to prove interaction, anatomy readability and combat mechanics. It is not intended final fidelity.

Target direction is realistic/stylized high-fidelity 3D boxing from first person, with the opponent as the primary readable threat and the player's gloves/arms in foreground.

## Approved visual language

- premium black / warm gold UI;
- controlled red accents;
- cinematic fight-night or gym lighting;
- bold condensed sports typography;
- dark, high-contrast cards and HUD panels;
- strong fighter identity;
- realistic/stylized 3D body/material direction;
- aspirational themes of discipline, growth and championship progression.

Exact generated slogans/copy are not immutable product text. Composition, hierarchy, interaction clarity and brand language are authoritative.

## Body topology readability

Upper body:
`shoulder → upper arm → elbow joint → forearm → glove`

Opponent lower body:
`pelvis → thigh → knee joint → shin → foot`

Production 3D must preserve these semantics.

## Combat POV composition lock

Use `reference-ui/03-pov-combat-hud.jpg` as the current UAT target:
- player identity/HP/stamina top-left;
- round/timer top-center;
- opponent identity/HP/stamina top-right;
- opponent centered and large;
- player gloves frame foreground;
- movement bottom-left;
- guard bottom-center;
- punch control zone bottom-right;
- arena/ring provides depth and range cues;
- diagnostic overlays minimized in user-facing UAT.

The verified control thesis remains authoritative; UI must represent it truthfully rather than replacing it with concept-only button semantics.

## Approved future surfaces

These lock visual direction but do not unlock implementation ahead of roadmap:
- `01-character-customization.jpg`
- `02-home-main-menu.jpg`
- `04-venues-career-selection.jpg`
- `05-training-coach.jpg`

## Approved opponent production direction

`06-opponent-ramirez-turnaround.jpg` is the approved production character authority for the current Ramirez archetype once full 3D production is unlocked.

## Mechanics / art boundary

Verified mechanics remain authoritative. Never change hit geometry simply to imitate the image, fake contact with projectile-like effects, alter input semantics to match a generated UI label, or hide unresolved mechanics under animation polish.

## UAT reference match report

Every UAT-facing build report must include:
- POV framing: PASS/PARTIAL/FAIL
- top HUD hierarchy: PASS/PARTIAL/FAIL
- bottom control hierarchy: PASS/PARTIAL/FAIL
- player glove presentation: PASS/PARTIAL/FAIL
- opponent readability: PASS/PARTIAL/FAIL
- black/gold/red visual language: PASS/PARTIAL/FAIL
- premium championship atmosphere: PASS/PARTIAL/FAIL
- mobile text readability: PASS/PARTIAL/FAIL
- debug intrusiveness: PASS/PARTIAL/FAIL

## UX anti-goals

- projectile-like visual for punches;
- hidden combat effect contradicting glove contact;
- decorative animation that makes attack family unreadable;
- oversized/debug-first HUD;
- unreadable onboarding/training text;
- drifting from approved layout hierarchy without a mechanics reason;
- implementing future screens merely because a reference exists.
