# Ramirez approved-reference measurements

Authority: `docs/handoff/reference-ui/06-opponent-ramirez-turnaround.jpg`.

Context reference: `docs/handoff/reference-ui/03-pov-combat-hud.jpg`.

The turnaround source is 900 x 675 px. The front figure is measured from the
unaltered source pixels using the reproducible crop `(22,72)-(190,486)`.
`scripts/prepare_references.py` creates the review crops without rewriting the
approved source. Measurements below are ratios, so the perspective/render size
does not become a false physical-unit claim.

## Front-view landmark basis

The visible body extends from hair top at about `y=12` to boot sole at about
`y=372`, giving a working visual height of `H = 360 px`. Landmarks are selected
on the approved front figure with an expected reading uncertainty of roughly
`±3 px` for soft tissue and `±5 px` where gloves/shorts occlude anatomy.

| Feature | Source pixels | Ratio to H | Modeling implication |
| --- | ---: | ---: | --- |
| total visible height | 360 | 1.000 | reference normalization |
| head height, hair-to-chin | ~64 | ~0.178 | adult athletic head, not undersized |
| shoulder span | ~108 | ~0.300 | broad but middleweight, not superhero |
| chest width | ~92 | ~0.256 | developed pec/lats |
| waist width | ~66 | ~0.183 | compact athletic waist |
| pelvis/hip width | ~78 | ~0.217 | stable pelvis below V-taper |
| upper-arm thickness | ~24 | ~0.067 | biceps/triceps mass must read clearly |
| forearm thickness | ~21 | ~0.058 | muscular taper into wrist |
| thigh thickness | ~31 | ~0.086 | powerful boxing base |
| calf thickness | ~25 | ~0.069 | calf volume remains visible |
| glove width | ~38 | ~0.106 | full boxing glove, not a small fist cap |
| chin to waistband | ~78 | ~0.217 | torso must not become a short barrel |
| waistband to boot sole | ~216 | ~0.600 | long, stable lower-body read |

Derived silhouette ratios:

- shoulder / waist width ≈ `1.64`
- chest / waist width ≈ `1.39`
- pelvis / waist width ≈ `1.18`
- upper-arm / waist width ≈ `0.36`
- thigh / waist width ≈ `0.47`

The approved figure is posed, so shoulder/chest/arm measurements include some
pose foreshortening. The static Blender neutral gate should stay within the
same visual band rather than mechanically forcing every posed pixel width.

## Side/back observations used as constraints

- Head has clear neck clearance; jaw does not merge into thorax.
- Trapezius rises from clavicle/shoulder into the neck instead of forming a
  vertical cylinder.
- Rib cage is deep enough to read as a trained boxer, while abdomen remains
  flatter and narrower than chest/lats.
- Back view shows strong deltoids, upper arms, lat spread and glute/thigh mass.
- Side view shows a real calf belly, knee narrowing, ankle narrowing and a
  high-support boxing boot.

## Guard/context observations

From `03-pov-combat-hud.jpg`, Ramirez's guard keeps the chin behind the gloves,
uses asymmetric lead/rear hands, flexed knees and a slightly rotated torso.
The gloves are substantial but do not hide the deltoid/upper-arm/forearm chain.

