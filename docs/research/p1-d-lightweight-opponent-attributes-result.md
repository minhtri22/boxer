# P1-D — Lightweight Opponent Attributes Verification Status

Date: 2026-09-10

## Current verdict

- `IMPLEMENTATION_READY`
- `STATIC_COMPILE_PASS`
- `PURE_LOGIC_PASS_8_OF_8`
- `UNITY_BATCH_BLOCKED_BY_LOCAL_LICENSING`
- `DETERMINISTIC_PASS_PENDING`

## What is implemented

Four opponent profiles are available through the frozen P1-D attribute layer:

- `BALANCED`: exact pre-P1-D baseline.
- `LONG_REACH`: reach only, `1.08x`.
- `PRESSURE`: post-attack gap only, `0.80x`.
- `FAST_HANDS`: Commit / Extend / Recover duration only, `0.90x`.

The default runtime profile is `BALANCED`. Optional runtime selection is exposed through `-opponentProfile=<profile>`. F3 diagnostics and semantic telemetry expose the active profile and factors.

Presentation now reads the opponent's live action-phase duration so `FAST_HANDS` cannot visually drift from combat authority.

## Verification completed

### Static Unity-source compile

All C# files under `Assets/Scripts` and `Assets/Editor` were compiled with the Roslyn compiler bundled with Unity 6000.5.8f1 plus Unity managed assemblies and Unity's NetStandard 2.1 reference set.

Result: `CSC_EXIT=0`.

Only pre-existing obsolete/unused warnings were emitted; no compile errors were reported.

### Pure P1-D logic harness

The P1-D profile source was also executed through the .NET SDK bundled with Unity.

Result: `8/8 PASS`:

1. balanced baseline;
2. long-reach isolation;
3. pressure isolation;
4. fast-hands isolation;
5. monotonic effects;
6. bounds;
7. parser;
8. inspector schema.

## Unity batch blocker

The intended Unity command is:

`BoxerP0.Editor.P1DOpponentAttributesSelfTests.RunWithRegressions`

The first batch attempt stopped before compilation because Unity Package Manager could not establish its local IPC connection. A second attempt using `-noUpm` progressed further but Unity Licensing repeatedly lost its local client channel and never reached the test method.

Therefore the repository must not record `131/131 PASS` yet. The 123 prior deterministic checks remain the latest executed Unity regression baseline until this batch suite can run in a normal local Unity environment.

## Required closure

Run the P1-D combined regression under a normal local Unity session. If it emits `COMBINED=131/131 PASS`, P1-D can be promoted to `DETERMINISTIC_PASS_UAT_DEFERRED` and the handoff/evidence index can advance to the next roadmap item.
