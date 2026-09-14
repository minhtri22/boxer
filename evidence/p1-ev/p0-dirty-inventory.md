# P1-EV P0 Candidate Inventory

Baseline: `b0eeed2fcb60006f05956ebca9f211de09608d9d`

Branch/worktree: `p1/ev-completion` / `boxer-p1-ev-completion`

## Included tracked changes

- `tools/verify_round2_artifact.py` — P1-EV version-prefix verification.
- `Round2Build.cs` — P1-EV productVersion/presentation provenance.
- `Round2RuntimeAudit.cs` — EV ownership audit hooks only.
- `BoxerVisualShell.cs` — attach EV presentation shell.
- `P1PunchGuide.cs` — suppress duplicate guide when P1-V/P1-EV HUD owns it.
- `P1VCombatPresentation.cs` — compact HUD and hide rendered bottom controls.
- WebGL template `index.html` — developer metrics become explicit opt-in.

After line-ending cleanup, normalized contents of all seven files match the
source WIP. `git diff --check` reports no whitespace errors.

## Included untracked source/resources/tests/evidence

- `EVVisualShell.cs` + `.meta`;
- `EVEvaluation.cs` + `.meta`;
- `EVSurface.shader`, `EVBackdrop.shader`, `EVText.shader` + `.meta`;
- `Resources/EV/RamirezHead.png` + `.meta`;
- `Resources/EV/GloveLeather.png` + `.meta`;
- candidate A/B and iteration visual/runtime evidence;
- P1-EV handoff documents 24, 25 and approved plan 26.

Key source/destination SHA-256 checks were identical for the takeover/plan docs,
`EVEvaluation.cs`, `EVVisualShell.cs`, `RamirezHead.png` and
`GloveLeather.png`.

## Excluded generated/local state

- `Phase0Boxer.unity` regenerated object-ID churn;
- `ProjectSettings.asset` local `bundleVersion: local-web`;
- legacy generated web build metadata;
- `.agentloop/` state;
- compile/build logs from the original checkout.

## Frozen authority check

No P1-EV diff exists in `Round2CombatRig`, `Round2Motion`, `Round2Footwork`,
`OpponentBoxer`, `PlayerBoxer` or `P1PunchMechanics`.

P0 verdict: `PASS`.
