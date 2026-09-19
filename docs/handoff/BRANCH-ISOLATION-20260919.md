# Branch isolation and WIP recovery — 2026-09-19

Primary: `art/ramirez-realistic-blender` in `boxer-ramirez-realistic`.
Backup/experiments: `experiment/other-agent-backup` in `boxer-other-agent-experiments`.

## Verified lineage

The shared checkout `boxer` remained at `93c0ef59b80ba74599cb18898b4a07f3ac0aa0a4`.
The remote `p1/whole-body-mechanics` was verified at
`bc0b2129dae47deeb3ce29d548624cf5571d0f36`.
Its history includes the other EV/Blender line. Neither ref is rewritten here.

Primary WIP was recovered from stash `2997526b41869e98d1dbd1bbdc4514b1207e3cf2`,
whose first parent is `b0eeed2fcb60006f05956ebca9f211de09608d9d`.
105 source/evidence files were recovered byte-for-byte. Another 14 task-owned
runtime evidence files were copied separately from `boxer-round2-final`; their
source paths and hashes are in the recovery manifest. The old stash is retained.

Three pre-existing unrelated tracked modifications (generated scene, project
settings and build metadata) remain in the stash and were not promoted into the
primary branch. The other agent's takeover/plan documents, transient service
state and loose logs were likewise not mixed into the primary source snapshot.

The backup branch descends from `8c2d590484a8dfc67438c3359994f601b4ec08bd`, keeping
all six later Blender commits. Its 330 modified/untracked source/evidence files
were copied and verified by SHA-256. Python bytecode caches are not source and
are excluded from the working snapshot. Untracked leftovers in the shared
checkout are preserved separately as a hashed zip. Original files are untouched.

## Next work

Finish the realistic Blender character before continuing gameplay/presentation
code. The recovered experimental mesh and historical successful numerical tests
do not qualify the current art as realistic or human-approved. No tests/build
were rerun merely to copy branches, and no public UAT deployment is part of this
isolation operation. The handoff defines the other agent's allowed branch/root.

## Evidence

- `evidence/branch-isolation-20260919/recovery-manifest.json`
- Backup branch: `evidence/branch-isolation-20260919/backup-manifest.json`
- `docs/handoff/HANDOFF-OTHER-AGENT-EXPERIMENTS-20260919.md`
