# P1-D — Lightweight Opponent Attributes Verification Status

Date: 2026-09-10

## Current verdict

- `DETERMINISTIC_PASS_UAT_DEFERRED`
- `STATIC_COMPILE_PASS`
- `PURE_LOGIC_PASS_8_OF_8`
- `UNITY_P1_D_PASS_8_OF_8`
- `UNITY_COMBINED_PASS_131_OF_131`

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

## Unity combined regression

The combined suite executed under Unity 6000.5.8f1 through:

`BoxerP0.Editor.P1DOpponentAttributesSelfTests.RunWithRegressions`

Result:

- P1-D: `8/8 PASS`
- prior suites through P1-CG: `123/123 PASS`
- combined: `131/131 PASS`

Evidence:

- `evidence/phase1/SYNTHETIC/p1-d-opponent-attributes-deterministic-self-tests.txt`
- `evidence/phase1/SYNTHETIC/p1-d-opponent-attributes-combined-regression.txt`

Earlier sandboxed attempts were blocked by local Package Manager/licensing IPC. The successful local Unity execution supersedes those incomplete attempts.

## Closure

Source commit: `96c47cadfe9d7f37ac02a29aa908ca130d76c8e7`.

P1-D is closed at deterministic level. Per product-owner direction, no standalone intermediate UAT is required; opponent-profile readability and tactical feel will be judged once in the final integrated candidate after the approved visual/UI package is complete.
