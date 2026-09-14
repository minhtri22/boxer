# Boxer — P1-EV Takeover Review

STATUS: `PLAN_REVIEW_REQUIRED_BEFORE_CODE_CONTINUES`

Review date: 2026-09-14

This document records the handoff state recovered from `task.md`, the current
`p1/whole-body-mechanics` checkout, existing Round 2 evidence, P1-EV evidence,
and the current uncommitted implementation. No new gameplay or visual code was
changed as part of this review.

## 1. Task boundary

The active task is P1-EV: apply the approved Boxer/Ramirez visual reference to
the already-working Round 2 articulated fighter while preserving the verified
combat model.

The following Round 2 behavior remains frozen unless a contradiction is proven:

- contact architecture and M05 tolerance;
- distance bands and AI spacing;
- player gesture vocabulary and punch-family semantics;
- A1 advance / neutral / retreat ordering;
- A3.1 hook close-range relationship;
- no-homing opponent commitment rule;
- HP, stamina, counter, round and winner semantics;
- articulated arm/leg/head/body motion and foot planting.

P1-EV is therefore a presentation integration over the Round 2 rig. It must not
silently become a new combat-mechanics phase.

## 2. Verified lineage

Current branch: `p1/whole-body-mechanics`.

Current committed HEAD and remote branch: `b0eeed2`.

Relevant lineage:

| SHA | Meaning |
|---|---|
| `795981c` | Round 2 baseline and contact-coherence blocker recorded |
| `d26dab5` | shared anatomical motion + relative swept contact integrated |
| `2931ec4` | opponent reach-profile behavior retained |
| `d735703` | committed-pose and planted-foot invariants added |
| `00f9c90` | redundant HUD removed; browser input status preserved |
| `725a215` | final Round 2 motion/contact source used for qualified build |
| `337116c` | verified Round 2 WebGL candidate packaged |
| `b0eeed2` | Round 2 experiments/evidence closed and deployed UAT documented |

Round 2 at `b0eeed2` is the protected baseline for P1-EV.

## 3. Round 2 baseline already established

The existing Round 2 final report records:

- `182/182` deterministic assertions PASS;
- candidate contact experiment: 1,860 cases;
- production contact matrix: `288/288` agreements;
- shared anatomical pose + relative swept-sphere contact selected;
- contact verification allowance: `0.001 m`;
- no-homing committed root/rotation drift: zero in the qualified runtime;
- planted-foot drift within the established invariant;
- WebGL smoke and deployment verification completed;
- status: `IMPLEMENTATION_PASS_PENDING_HUMAN_UAT`.

That qualification belongs to the Round 2 artifact. It does not automatically
qualify the later uncommitted P1-EV visual changes.

## 4. P1-EV implementation currently present

P1-EV starts from `b0eeed2` and is still an uncommitted working tree.

The current design is intentionally presentation-only:

- `EVVisualShell` reads existing anatomical anchors and does not own combat
  transforms;
- player/opponent arm, leg, head and glove meshes are replaced or decorated on
  their existing rig anchors;
- opponent head uses `Resources/EV/RamirezHead.png`;
- player gloves use black/gold styling; opponent gloves use red styling;
- opponent uses red/gold trunks and white/red boots attached to the existing
  thigh/shin/foot chain;
- the arena uses physical ring geometry plus a depth-tested distant backdrop;
- the HUD is made compact;
- rendered bottom control buttons are removed while the invisible input zones
  retain the existing control semantics;
- old duplicate punch-guide rendering is suppressed while the P1-V/P1-EV HUD
  is active.

No current dirty source file changes `Round2CombatRig`, `Round2Motion`,
`Round2Footwork`, `OpponentBoxer`, `PlayerBoxer`, or `P1PunchMechanics`.
That is the correct boundary to preserve.

## 5. P1-EV candidate experiment recovered

Two visual candidates were evaluated:

### Candidate A — segmented shaped shell

Reuse the existing torso contact volumes as individually rendered shaped pieces,
with the new materials, face, gloves, trunks and boots.

Observed limitation: visible boundaries between torso pieces continue to read as
an assembly of primitive volumes.

### Candidate B — continuous torso shell

Keep the same articulated limbs, face, garments and authoritative rig, but render
the torso as one triangulated surface clipped from the original contact-sphere
union and anchored to the existing chest transform.

Candidate B was selected because it improves silhouette continuity while keeping
the generated surface on the original contact union.

The first radial-envelope prototype failed the 1 mm surface requirement and was
rejected. The selected clipped-union version did not relax the threshold.

## 6. Evidence that is already usable

The selected B geometry evidence currently records `6/6` PASS:

- Ramirez head texture is packaged as a Unity resource;
- glove/head replacement sphere preserves the exact contact radius;
- torso vertices remain within the existing 1 mm M05 allowance;
- torso triangle centers remain within the same allowance;
- bottom control visuals are disabled independently of input;
- the WebGL-compatible EV surface shader resource is present.

Measured B torso errors:

- maximum vertex error: `0.2811849 mm`;
- maximum triangle-center error: `0.199913979 mm`;
- mesh: `15,818` vertices / `30,380` triangles.

Existing A/B runtime logs also show the underlying Round 2 runtime invariants
passing during the captured motion sequences: real contacts, planted feet and
zero committed root/facing drift.

## 7. Evidence that is NOT yet closed

The following items prevent a final P1-EV implementation PASS today.

### 7.1 Final visual ownership audit has not completed

`EVEvaluation` now contains explicit per-frame checks for:

- torso anchor ownership;
- glove cuff parentage and forearm alignment;
- exactly one visible shoulder per side;
- trunk-leg attachment to the thigh;
- boot-upper attachment to the shin;
- redundant torso renderers disabled for candidate B.

The expected evidence file is `evidence/p1-ev/ownership-runtime.txt`.
It does not currently exist.

Therefore the new ownership gate is not closed, even though earlier Round 2
runtime evidence passes.

### 7.2 Latest `RunVisual` attempts did not reach normal audit completion

The latest visual-run logs show the six shell tests passing, scene creation and
EV startup, then an Editor `UnityEditor.Search.SearchDatabase` indexing
`ArgumentOutOfRangeException` before the new ownership report is written.

This looks like an Editor/search-index environment failure rather than a combat
failure, but it cannot be treated as a successful runtime audit.

### 7.3 Final deterministic suite has to be rerun on the final P1-EV source

Round 2's `182/182` result is qualified evidence for the committed baseline.
P1-EV has since changed presentation/HUD/build code and added new resources and
tests. The final P1-EV source must rerun the complete deterministic suite plus
the EV-specific checks before commit/build qualification.

### 7.4 No final committed P1-EV source exists

HEAD remains `b0eeed2`; P1-EV is uncommitted. A committed source SHA is required
before a provenance-valid WebGL build can be accepted.

### 7.5 No committed-source P1-EV WebGL qualification exists

The existing deployed URL and artifact prove Round 2. They do not prove the
current P1-EV working tree.

P1-EV still needs:

- clean build from its committed source;
- `ev-<full SHA>` productVersion/provenance marker;
- artifact hashes;
- browser startup/runtime smoke;
- matched performance observation;
- deployment-byte verification;
- one integrated Human UAT handoff.

## 8. Working-tree hygiene findings

The checkout currently contains both intended P1-EV changes and generated local
noise. These must be separated before any commit.

Examples of generated/local state currently dirty:

- `Phase0Boxer.unity` differs only by regenerated object file IDs;
- `ProjectSettings.asset` currently contains `bundleVersion: local-web`;
- legacy web build metadata currently records local build output;
- compile/build logs and `.agentloop/` are untracked.

Large apparent diffs in some existing C# files are mostly line-ending noise.
With end-of-line differences ignored, the meaningful existing-file changes are
substantially smaller. Final staging must therefore be explicit and reviewed so
that P1-EV does not accidentally commit regenerated scene/settings files or
wholesale line-ending rewrites.

## 9. Current assessment

The recovered architecture is coherent with the user's requirement: the visual
reference is being attached to the verified articulated skeleton without moving
contact, distance, punch, footwork or no-homing authority into the visual layer.

Candidate B is a technically defensible selected visual architecture and its
surface-coherence evidence is good. The task is not ready for final build or UAT
handoff because the final ownership audit, full final-source regression run,
committed-source build provenance and WebGL validation remain open.

Current truthful status:

`P1_EV_IMPLEMENTATION_IN_PROGRESS__PLAN_REVIEW_REQUIRED`

No Human UAT PASS is claimed.

