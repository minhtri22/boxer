# Boxer — Approved UI/UX Reference Notes

## Shared art direction

The six approved images define a single Boxer visual language:

- realistic/stylized high-fidelity 3D boxing;
- premium black / warm gold / controlled red accents;
- cinematic arena or gym lighting;
- strong contrast with readable mobile UI;
- bold condensed sports typography;
- subtle distressed boxing-poster texture;
- first-person gloves used as a framing device where appropriate;
- the opponent remains the primary readable threat in combat;
- aspirational tone: discipline, growth, identity, championship progression.

The slogans visible in the concept art express tone, not immutable copy. Layout hierarchy and interaction clarity are more authoritative than exact generated wording.

## 01 — Character customization

Approved structure: fighter visible behind/above customization card; player name; nationality; skin tone; hair; facial hair; glove style/color; trunks; wraps; fighting style; strong gold `CONFIRM` CTA; premium black card with gold border/highlight states.

This is a future product surface. Do not implement it before roadmap unlock.

## 02 — Home / main menu

Approved structure: prominent Boxer logo + `Fight from your own eyes`; control thesis taught visually (Phone = Head, Left Thumb = Feet, Right Thumb = Punch, No Touch = Guard); strong `START` CTA; secondary `TRAINING` and `CAREER`; POV gloves in foreground; arena/ring environment.

The control meanings must match the verified input model, not generated concept labels when they differ in detail.

## 03 — POV combat HUD — immediate UAT authority

This is the most important current reference.

Approved hierarchy:
- top-left: player identity + level + HP + stamina;
- top-center: round / timer;
- top-right: opponent identity + portrait + HP + stamina;
- opponent centered, large, fully readable;
- player's black/gold gloves frame foreground;
- bottom-left: movement control;
- bottom-center: guard;
- bottom-right: punch-family control zone;
- combat environment uses premium arena lighting and depth cues.

Current verified controls remain authoritative. UI may visually group punch families, but must not redefine gesture semantics.

## 04 — Venues / career selection

Approved future hierarchy: main navigation; venue cards progress from smaller/rougher competition toward championship stage; rewards/rank visible; strong `NEXT FIGHT` CTA; black/gold card system.

This image is binding visual direction only; it does not authorize early career implementation.

## 05 — Training / coach

Approved future hierarchy: coach as dominant anchor; Head Movement, Footwork, Punch Mechanics, Guard, Conditioning; each module communicates improvement outcome; strong `TRAIN NOW` CTA; gym lighting distinct from fight-night arena while preserving Boxer brand language.

## 06 — Ramirez opponent turnaround

Approved production opponent direction: muscular middleweight silhouette; compact boxing guard; dark hair/beard; red gloves; red/gold trunks; white/red boots; readable front/3⁄4/side/back silhouette; realistic proportions and muscle groups.

Production rigging must preserve validated semantic chains:
`shoulder → upper arm → elbow joint → forearm → glove`
and
`pelvis → thigh → knee joint → shin → foot`.

## Debug policy for UAT

Diagnostic data can remain available through a toggle or dedicated dev build, but a user-facing UAT candidate must not be visually dominated by debug text, traces, gizmos or placeholder blocks.
