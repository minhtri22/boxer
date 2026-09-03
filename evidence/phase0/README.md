# Phase 0 Evidence Index

Evidence is separated by provenance so synthetic/editor results cannot be
mistaken for real-device or human observations.

## EDITOR

- `windows-build/build-metadata.txt` — concise successful build metadata.

The generated Windows build binaries are intentionally ignored by Git; the local
build can be reproduced from the Unity project. Raw Unity import/build logs are
also retained locally but ignored because they are machine-generated and noisy.

## SYNTHETIC

- `deterministic-self-tests.txt` — latest deterministic test summary.
- `runtime-smoke-summary.txt` — concise headless runtime smoke result.
- `runtime-telemetry-excerpt.csv` — selected concurrent input/action rows showing
  guard, opponent recovery and `PLAYER_COUNTER_HIT`.

Raw Unity runtime/test logs and the full telemetry CSV are retained locally but
ignored. Synthetic evidence verifies implementation behavior only.

## REAL_DEVICE

- `blocker.md` — why iPhone 12 deployment was not available in this environment.

## MANUAL_OBSERVATION

- `not-tested.md` — records that no human game-feel/replay-intent observation was
  performed during this task.
