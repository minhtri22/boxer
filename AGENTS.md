# Branch ownership — primary Blender work

User-approved separation, 2026-09-19.

- Work only in `D:/WORK/RESEARCH/POVGame/boxer-ramirez-realistic` on
  `art/ramirez-realistic-blender`. Verify both before mutations.
- This branch preserves the primary agent's recovered WIP from stash
  `2997526b41869e98d1dbd1bbdc4514b1207e3cf2`, based on `b0eeed2`.
- Recovered code/evidence is unfinished historical work, not a new PASS or build.
- Next implementation priority: realistic Ramirez in Blender, faithful anatomy,
  face, equipment, materials and deformation. Finish the Blender visual gate
  before changing other game code or producing another UAT build.
- Do not pull/merge/cherry-pick the other agent's work implicitly. Do not write
  other worktrees or push `p1/whole-body-mechanics`/`main` without a new instruction.
- Push only `HEAD:refs/heads/art/ramirez-realistic-blender`; no push --all/mirror/force.
- The other agent is confined to `experiment/other-agent-backup`. See
  `docs/handoff/HANDOFF-OTHER-AGENT-EXPERIMENTS-20260919.md`.
