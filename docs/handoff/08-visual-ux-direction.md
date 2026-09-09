# 08 — Visual & UX Direction

## Authority status
The project has a **human-approved visual reference set** in `docs/handoff/reference-ui/`. These assets are **binding UAT visual authority**, not optional moodboards. Current combat UAT primary reference: `reference-ui/03-pov-combat-hud.jpg`.

## Target
Realistic/stylized high-fidelity 3D boxing from first person; opponent as primary readable threat; player gloves/arms in foreground. Premium black/warm-gold UI, controlled red accents, cinematic arena/gym lighting, bold condensed sports typography.

## Body topology
Upper: `shoulder → upper arm → elbow joint → forearm → glove`
Lower opponent: `pelvis → thigh → knee joint → shin → foot`
Production 3D must preserve these semantics.

## Combat POV composition lock
- player identity/HP/stamina top-left;
- round/timer top-center;
- opponent identity/HP/stamina top-right;
- opponent centered and large;
- player gloves foreground;
- movement bottom-left;
- guard bottom-center;
- punch control zone bottom-right;
- arena depth/range cues;
- diagnostic overlays minimized in user-facing UAT.

Verified controls remain authoritative; UI represents them truthfully rather than replacing them with concept-only semantics.

## Future approved surfaces
`01-character-customization.jpg`, `02-home-main-menu.jpg`, `04-venues-career-selection.jpg`, `05-training-coach.jpg` lock visual direction but do not unlock implementation ahead of roadmap.

## Opponent production direction
`06-opponent-ramirez-turnaround.jpg` is approved production character authority for current Ramirez archetype once full 3D production is unlocked.

## UAT reference match report
Every UAT-facing build must classify POV framing, top HUD, bottom controls, glove presentation, opponent readability, black/gold/red language, championship atmosphere, mobile text readability and debug intrusiveness as PASS/PARTIAL/FAIL.

## Anti-goals
No projectile-like punch visual, hidden combat effect contradicting glove contact, debug-first HUD, unreadable onboarding text, art-driven mechanics changes, or premature future-screen implementation.
