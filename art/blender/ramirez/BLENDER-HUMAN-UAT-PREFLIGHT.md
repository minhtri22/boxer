# RAMIREZ — BLENDER VISUAL QA CHECKLIST
## Mandatory preflight before Human UAT

Purpose:

Agent MUST complete this checklist before asking for Human UAT.

This is a BLENDER-ONLY visual gate.

Unity integration is NOT allowed until:
- all HARD gates PASS,
- evidence is produced,
- final status is `READY_FOR_HUMAN_BLENDER_UAT`.

The agent's own PASS does NOT equal Human PASS.

---

# 0. QA RULES

For every item use exactly one:

- [ ] PASS
- [ ] FAIL
- [ ] N/A — only when explicitly justified

Every PASS must include:

- evidence file path
- short factual observation
- reference used

Example:

PASS  
Evidence: `renders/comparisons/front_v3.png`  
Observation: shoulder width and waist taper visually align with approved reference; no barrel torso remains.

Not acceptable:

"looks good"

"close enough"

"seems correct"

---

# 1. REFERENCE AUTHORITY — HARD GATE

## R01 — Correct approved references loaded

- [ ] PASS
- [ ] FAIL

Verify that the newest approved Ramirez references are being used.

Required:
- approved turnaround
- approved 3D target
- approved clay/sculpt target
- approved boxing/combat pose references

Evidence:
`____________________________`

---

## R02 — No obsolete reference overrides newer reference

- [ ] PASS
- [ ] FAIL

No old prototype/mannequin image may override the latest approved visual target.

Evidence:
`____________________________`

---

## R03 — Comparison uses equivalent camera/view

- [ ] PASS
- [ ] FAIL

Reference vs Blender comparison must use approximately matching:

- camera angle
- body orientation
- framing
- scale
- view direction

Do not compare front reference to A-pose or mismatched perspective.

Evidence:
`____________________________`

---

# 2. STATIC BODY PROPORTIONS — HARD GATE

## S01 — Head size

- [ ] PASS
- [ ] FAIL

Check:
- not oversized
- not undersized
- compatible with approved Ramirez silhouette

Evidence:
`____________________________`

---

## S02 — Head position

- [ ] PASS
- [ ] FAIL

Head must sit naturally above neck/shoulders.

Reject:
- head buried in torso
- floating head
- excessive neck gap

Evidence:
`____________________________`

---

## S03 — Neck anatomy

- [ ] PASS
- [ ] FAIL

Must visibly have:

head
→ neck
→ trapezius
→ clavicle
→ shoulder

Reject:
- cylinder neck
- neck disappearing into chest
- head sitting directly on torso

Evidence:
`____________________________`

---

## S04 — Shoulder width

- [ ] PASS
- [ ] FAIL

Must match athletic boxer reference.

Reject:
- shoulders too narrow
- extreme bodybuilder width
- square mannequin shoulder line

Evidence:
`____________________________`

---

## S05 — Shoulder slope / trapezius

- [ ] PASS
- [ ] FAIL

Shoulder-neck transition must look organic.

Evidence:
`____________________________`

---

## S06 — Chest / rib cage

- [ ] PASS
- [ ] FAIL

Must look athletic and anatomically continuous.

Reject:
- sphere chest
- stacked blocks
- overly inflated rib cage

Evidence:
`____________________________`

---

## S07 — V-taper

- [ ] PASS
- [ ] FAIL

Required silhouette:

shoulders > chest > waist

Must clearly read as athletic boxer.

Evidence:
`____________________________`

---

## S08 — Waist

- [ ] PASS
- [ ] FAIL

Reject:
- barrel torso
- fat-looking midsection
- rectangular waist
- waist too narrow to look plausible

Evidence:
`____________________________`

---

## S09 — Torso length

- [ ] PASS
- [ ] FAIL

Check reference alignment between:
- chest
- abdomen
- waist
- pelvis

Reject short compressed torso.

Evidence:
`____________________________`

---

## S10 — Pelvis / hips

- [ ] PASS
- [ ] FAIL

Must transition naturally from waist to legs.

Reject:
- oversized pelvis
- block pelvis
- disconnected leg sockets

Evidence:
`____________________________`

---

# 3. ARM ANATOMY — HARD GATE

## A01 — Deltoid

- [ ] PASS
- [ ] FAIL

Shoulder cap must visibly connect:
chest/back → upper arm

Reject:
- ball shoulder
- no deltoid
- sharp mechanical seam

Evidence:
`____________________________`

---

## A02 — Upper-arm mass

- [ ] PASS
- [ ] FAIL

Biceps/triceps volume must be readable.

Reject:
- stick arm
- constant-diameter tube

Evidence:
`____________________________`

---

## A03 — Elbow narrowing

- [ ] PASS
- [ ] FAIL

Upper arm must transition naturally through elbow into forearm.

Evidence:
`____________________________`

---

## A04 — Forearm anatomy

- [ ] PASS
- [ ] FAIL

Forearm must:
- carry visible muscle volume
- taper toward wrist

Reject cylindrical forearm.

Evidence:
`____________________________`

---

## A05 — Wrist-to-glove transition

- [ ] PASS
- [ ] FAIL

Glove must look attached to a real wrist/forearm.

Reject:
- floating glove
- swollen wrist
- abrupt cylinder-to-glove joint

Evidence:
`____________________________`

---

## A06 — Arm silhouette at guard

- [ ] PASS
- [ ] FAIL

Muscle volume must remain believable with elbows bent.

Evidence:
`____________________________`

---

## A07 — Arm silhouette at extension

- [ ] PASS
- [ ] FAIL

During jab/cross:
- shoulder remains anatomical
- biceps/triceps volume remains plausible
- elbow does not collapse

Evidence:
`____________________________`

---

# 4. LOWER BODY — HARD GATE

## L01 — Thigh volume

- [ ] PASS
- [ ] FAIL

Must look capable of boxing movement.

Evidence:
`____________________________`

---

## L02 — Knee definition

- [ ] PASS
- [ ] FAIL

Reject:
- tube legs
- missing knee transition

Evidence:
`____________________________`

---

## L03 — Calf volume

- [ ] PASS
- [ ] FAIL

Must taper naturally toward ankle.

Evidence:
`____________________________`

---

## L04 — Ankle

- [ ] PASS
- [ ] FAIL

Must not be same thickness as calf.

Evidence:
`____________________________`

---

## L05 — Foot proportions

- [ ] PASS
- [ ] FAIL

Feet must support correct boxing stance.

Evidence:
`____________________________`

---

## L06 — Whole lower-body silhouette

- [ ] PASS
- [ ] FAIL

Must read as an athletic fighter, not mannequin legs.

Evidence:
`____________________________`

---

# 5. BOXING TRUNKS — HARD GATE

## T01 — Correct boxing trunks silhouette

- [ ] PASS
- [ ] FAIL

Reject current failure shape:

rigid trapezoid / skirt / solid block

Must resemble approved boxing trunks.

Evidence:
`____________________________`

---

## T02 — Waistband

- [ ] PASS
- [ ] FAIL

Must have:
- proper height
- correct thickness
- Ramirez visual treatment
- correct relationship to waist

Evidence:
`____________________________`

---

## T03 — Two-leg construction

- [ ] PASS
- [ ] FAIL

Must visually read as shorts with left/right leg openings.

Reject continuous skirt geometry.

Evidence:
`____________________________`

---

## T04 — Fabric behavior

- [ ] PASS
- [ ] FAIL

Need believable:
- drape
- folds
- looseness
- leg opening

Do not require final cloth simulation, but geometry must read as fabric.

Evidence:
`____________________________`

---

## T05 — Length

- [ ] PASS
- [ ] FAIL

Compare directly with approved reference.

Evidence:
`____________________________`

---

## T06 — No severe clipping in poses

- [ ] PASS
- [ ] FAIL

Check:
- guard
- jab
- cross
- hook
- slip
- steps

Evidence:
`____________________________`

---

# 6. GLOVES / BOOTS / HAIR — HARD VISUAL GATE

## G01 — Glove size

- [ ] PASS
- [ ] FAIL

Must match boxing glove proportions.

Evidence:
`____________________________`

---

## G02 — Glove shape

- [ ] PASS
- [ ] FAIL

Reject primitive blob/capsule glove.

Evidence:
`____________________________`

---

## B01 — Boxing boots silhouette

- [ ] PASS
- [ ] FAIL

Must resemble approved high boxing boots.

Reject:
- rectangular blocks
- oversized cartoon boots
- debug footwear

Evidence:
`____________________________`

---

## H01 — Hair silhouette

- [ ] PASS
- [ ] FAIL

Must approximately match approved Ramirez:
- dark
- short curly / wet-looking character

Reject:
- hemisphere cap
- beads/dots obvious as placeholder
- helmet-like hair

Evidence:
`____________________________`

---

# 7. STATIC POSE QA — HARD GATE

Important:

Static comparison pose must match the corresponding reference purpose.

Do NOT use A-pose as proof of boxing stance.

## P01 — Front reference pose

- [ ] PASS
- [ ] FAIL

Check:
- guard
- torso
- chin
- legs
- stance

Evidence:
`____________________________`

---

## P02 — 3/4 front

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## P03 — Side

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## P04 — Back

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## P05 — 3/4 back

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

# 8. BOXING GUARD — HARD GATE

## C01 — Chin tucked

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## C02 — Slight forward/crouched boxing posture

- [ ] PASS
- [ ] FAIL

Must not stand perfectly upright.

Expected:
- knees slightly bent
- torso slightly compressed/forward
- head protected

Evidence:
`____________________________`

---

## C03 — Gloves protect face

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## C04 — Elbows protect torso

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## C05 — Lead/rear hand asymmetry

- [ ] PASS
- [ ] FAIL

Do not use perfectly symmetric mannequin guard.

Evidence:
`____________________________`

---

## C06 — Boxing stance feet

- [ ] PASS
- [ ] FAIL

Required:
- split stance
- one foot forward
- one rear
- not side-by-side mannequin stance

Evidence:
`____________________________`

---

## C07 — Balanced center of mass

- [ ] PASS
- [ ] FAIL

Pose must visually look stable.

Evidence:
`____________________________`

---

# 9. JAB — HARD GATE

## J01 — Correct lead hand

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## J02 — Rear hand remains guard

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## J03 — Shoulder protects chin

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## J04 — Torso remains boxing posture

- [ ] PASS
- [ ] FAIL

Reject fully upright mannequin.

Evidence:
`____________________________`

---

## J05 — Legs remain grounded

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## J06 — Jab reads as punch, not arm reach

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

# 10. CROSS — HARD GATE

## X01 — Rear hand punches

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## X02 — Lead hand guards

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## X03 — Hip rotation

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## X04 — Shoulder rotation

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## X05 — Rear foot pivot

- [ ] PASS
- [ ] FAIL

This must be visible in pose.

Evidence:
`____________________________`

---

## X06 — Weight transfer

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## X07 — Head remains protected

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

# 11. HOOK — HARD GATE

## K01 — Elbow angle believable

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## K02 — Hip/torso rotation

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## K03 — Feet participate

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## K04 — Hook is compact

- [ ] PASS
- [ ] FAIL

Reject wide slow swinging-arm pose.

Evidence:
`____________________________`

---

# 12. UPPERCUT — HARD GATE

## U01 — Knee compression

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## U02 — Hip drive

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## U03 — Compact upward path

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## U04 — Other hand remains useful guard

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

# 13. SLIP / EVADE — HARD GATE

## E01 — Head leaves centerline

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## E02 — Knees participate

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## E03 — Torso participates

- [ ] PASS
- [ ] FAIL

Reject neck-only tilt.

Evidence:
`____________________________`

---

## E04 — Feet remain grounded

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## E05 — Guard remains meaningful

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

# 14. SLIP + COUNTER — HARD GATE

This is a critical gate based on Human feedback.

## SC01 — Slip happens before or naturally into counter

- [ ] PASS
- [ ] FAIL

Do not create a pose that looks like:
"leaning sideways while randomly extending arm."

Evidence:
`____________________________`

---

## SC02 — Head remains off centerline

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## SC03 — Counter generated from stable base

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## SC04 — Legs / hips / torso coordinate with punch

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## SC05 — Pose remains physically plausible

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

# 15. STEP FORWARD — HARD GATE

## F01 — Lead foot initiates

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## F02 — Rear foot gathers

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## F03 — Boxing stance preserved

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## F04 — No crossed feet

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

# 16. STEP BACKWARD — HARD GATE

## BK01 — Rear foot initiates

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## BK02 — Lead foot follows

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

## BK03 — Guard and balance preserved

- [ ] PASS
- [ ] FAIL

Evidence:
`____________________________`

---

# 17. DEFORMATION QA — HARD GATE

Check all major poses.

## D01 — Neck

- [ ] PASS
- [ ] FAIL

No collapse / penetration.

---

## D02 — Shoulder

- [ ] PASS
- [ ] FAIL

No collapse / extreme stretching.

---

## D03 — Armpit

- [ ] PASS
- [ ] FAIL

No tearing / impossible fold.

---

## D04 — Elbow

- [ ] PASS
- [ ] FAIL

No pinching/candy-wrapper deformation.

---

## D05 — Forearm

- [ ] PASS
- [ ] FAIL

Volume preserved.

---

## D06 — Torso

- [ ] PASS
- [ ] FAIL

No rubber stretching.

---

## D07 — Hip

- [ ] PASS
- [ ] FAIL

Natural transition.

---

## D08 — Knee

- [ ] PASS
- [ ] FAIL

No collapse.

Evidence package:
`____________________________`

---

# 18. SILHOUETTE TEST — HARD GATE

Create black silhouette renders.

## SI01 — Front silhouette reads as boxer

- [ ] PASS
- [ ] FAIL

---

## SI02 — Side silhouette reads as boxer

- [ ] PASS
- [ ] FAIL

---

## SI03 — Back silhouette reads as boxer

- [ ] PASS
- [ ] FAIL

---

## SI04 — Guard silhouette reads as boxer

- [ ] PASS
- [ ] FAIL

---

## SI05 — Jab silhouette reads as boxing jab

- [ ] PASS
- [ ] FAIL

---

## SI06 — Cross silhouette reads as boxing cross

- [ ] PASS
- [ ] FAIL

If silhouette fails without materials:
geometry is not ready.

Evidence:
`____________________________`

---

# 19. MATERIAL / IDENTITY QA

These occur only AFTER clay geometry passes.

## M01 — Ramirez facial direction

- [ ] PASS
- [ ] FAIL
- [ ] LIMITED

---

## M02 — Hair

- [ ] PASS
- [ ] FAIL
- [ ] LIMITED

---

## M03 — Skin

- [ ] PASS
- [ ] FAIL
- [ ] LIMITED

---

## M04 — Red gloves

- [ ] PASS
- [ ] FAIL
- [ ] LIMITED

---

## M05 — Maroon/red trunks + gold details

- [ ] PASS
- [ ] FAIL
- [ ] LIMITED

---

## M06 — Boots

- [ ] PASS
- [ ] FAIL
- [ ] LIMITED

---

## M07 — Overall Ramirez identity

- [ ] PASS
- [ ] FAIL
- [ ] LIMITED

Evidence:
`____________________________`

---

# 20. PLACEHOLDER DETECTION — HARD GATE

Before handoff verify that the review renders contain NONE of:

- [ ] hemisphere placeholder hair
- [ ] rectangular boots
- [ ] sphere/capsule gloves
- [ ] skirt/block shorts
- [ ] debug primitive joints
- [ ] mannequin A-pose presented as boxing reference
- [ ] disconnected body pieces
- [ ] obvious unfinished proxy meshes

If ANY is present:

STATUS = FAIL

---

# 21. REFERENCE MATCH SUMMARY — HARD GATE

Complete table:

| Category | PASS/FAIL | Evidence | Largest remaining deviation |
|---|---|---|---|
| Head / neck | | | |
| Shoulders | | | |
| Arms | | | |
| Torso | | | |
| Waist | | | |
| Pelvis | | | |
| Legs | | | |
| Hair | | | |
| Gloves | | | |
| Trunks | | | |
| Boots | | | |
| Guard | | | |
| Jab | | | |
| Cross | | | |
| Hook | | | |
| Uppercut | | | |
| Slip | | | |
| Slip + counter | | | |
| Forward step | | | |
| Backward step | | | |

---

# 22. REQUIRED REVIEW PACKAGE

Before requesting Human UAT, produce:

## STATIC

- front reference vs Blender
- 3/4 front reference vs Blender
- side reference vs Blender
- back reference vs Blender
- 3/4 back reference vs Blender

## CLAY COMBAT

- guard
- jab
- cross
- hook
- uppercut
- slip left
- slip right
- slip + counter
- step forward
- step backward

## SILHOUETTE

- front
- side
- back
- guard
- jab
- cross

## MATERIAL

- front
- 3/4
- guard
- close head
- gloves
- trunks
- boots

---

# 23. AUTOMATIC STOP RULE

If ANY of these fails:

R01–R03
S01–S10
A01–A07
L01–L06
T01–T06
G01–G02
B01
H01
P01–P05
C01–C07
J01–J06
X01–X07
K01–K04
U01–U04
E01–E05
SC01–SC05
F01–F04
BK01–BK03
D01–D08
SI01–SI06
Placeholder Detection

then:

DO NOT REQUEST HUMAN UAT.

Return:

BLENDER_QA_FAIL

Then:
1. identify the largest visual error,
2. fix it,
3. regenerate affected renders,
4. rerun checklist.

---

# 24. HUMAN HANDOFF RULE

Only when ALL HARD gates PASS may the agent report:

BLENDER_QA_PASS_PENDING_HUMAN_UAT

This does NOT authorize Unity integration.

Only the human may say:

BLENDER_VISUAL_HUMAN_PASS

After that, and only after that, Unity integration becomes eligible.

---

# 25. FINAL AGENT REPORT

Report exactly:

BRANCH:
SHA:

STATIC_GATES:
PASS / FAIL

ANATOMY_GATES:
PASS / FAIL

TRUNKS_GATES:
PASS / FAIL

BOXING_POSE_GATES:
PASS / FAIL

DEFORMATION_GATES:
PASS / FAIL

SILHOUETTE_GATES:
PASS / FAIL

MATERIAL_GATES:
PASS / FAIL / LIMITED

PLACEHOLDER_CHECK:
PASS / FAIL

FAILED_ITEMS:
- ...

RENDER_PACKAGE:
- ...

LARGEST_REMAINING_DIFFERENCE_FROM_REFERENCE:
...

FINAL_STATUS:

BLENDER_QA_PASS_PENDING_HUMAN_UAT
or

BLENDER_QA_FAIL