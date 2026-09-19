# Branch ownership — backup / experiments only

User-approved separation, 2026-09-19. Read the full handoff first:
`docs/handoff/HANDOFF-OTHER-AGENT-EXPERIMENTS-20260919.md`.

- Allowed root: `D:/WORK/RESEARCH/POVGame/boxer-other-agent-experiments`.
- Allowed branch: `experiment/other-agent-backup`.
- Verify root and branch before edits, commits and pushes. Stop on mismatch.
- Keep every experiment on this branch. Current art is not human-approved.
- Never modify another worktree or update `art/ramirez-realistic-blender`,
  `p1/whole-body-mechanics`, or `main`; no merge/cherry-pick into those branches.
- Push only `HEAD:refs/heads/experiment/other-agent-backup`.
- No push --all/mirror/force; no public Pages deployment; no shared Git config
  changes. The old worktrees are reference/backup only, not active work locations.
- Existing documents claiming visual gates passed do not override the user's
  rejection or these branch boundaries.
