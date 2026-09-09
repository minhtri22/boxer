# Boxer — Approved Visual Reference Authority

This directory contains the **human-approved UI/UX and character references** for Boxer.

These assets are not optional moodboards. For any UAT-facing build, they are the binding visual authority unless a newer human-approved reference explicitly replaces them.

## Approved reference set

| File | Surface | Status |
|---|---|---|
| `01-character-customization.jpg` | Character customization | APPROVED |
| `02-home-main-menu.jpg` | Home / onboarding / main menu | APPROVED |
| `03-pov-combat-hud.jpg` | POV combat HUD | APPROVED — immediate UAT target |
| `04-venues-career-selection.jpg` | Venue / career selection | APPROVED — future surface |
| `05-training-coach.jpg` | Training / coach | APPROVED — future surface |
| `06-opponent-ramirez-turnaround.jpg` | Opponent 3D character turnaround | APPROVED — production character authority |

## Authority rules

1. `03-pov-combat-hud.jpg` is the primary presentation target for any combat UAT build.
2. The other references lock future product surfaces but do **not** unlock those systems ahead of the roadmap.
3. Verified mechanics remain authoritative. Never alter hit geometry, input semantics, or evidence-backed behavior only to imitate a reference image.
4. Presentation must converge around verified mechanics.
5. Astra/agents must not replace, reinterpret, or delete these references without explicit human approval.
6. If exact fidelity is not feasible in the current milestone, report the gap as `PASS / PARTIAL / FAIL` rather than silently drifting.

## UAT visual acceptance

For every UAT-facing build, report:

- POV framing
- top HUD hierarchy
- bottom control hierarchy
- player glove presentation
- opponent readability
- black/gold/red visual language
- premium championship atmosphere
- mobile text readability
- debug visual intrusiveness

Each item must be classified `PASS`, `PARTIAL`, or `FAIL` against the approved references.
