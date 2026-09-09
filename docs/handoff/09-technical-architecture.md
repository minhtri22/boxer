# 09 — Technical Architecture

## System layers

Astra must preserve the distinction between input, intent, authoritative combat, telemetry and presentation.

```text
Physical Input
├─ Device orientation
├─ Left-thumb movement
└─ Right-thumb punch gesture
        ↓
Intent Layer
├─ Head/evasion intent
├─ Footwork intent
└─ Punch family/hand intent
        ↓
Combat State / Snapshot
        ↓
Punch Mechanics
        ↓
Authoritative Hit Resolution
        ↓
Outcome / Counter / State
        ↓
Telemetry / Semantic Events
        ↓
Visual Presentation
```

## Important code responsibilities

Representative current files:

- `PlayerBoxer.cs` — player combat/action state and visual-access properties.
- `OpponentBoxer.cs` — opponent combat/action state, attack target/reach/state.
- `P1PunchMechanics.cs` — P1 snapshot and isolated A1/A3.1 coupling functions.
- `ArmVisualEmbodiment.cs` — presentation-only upper-body arm chain and guard/punch poses.
- `OpponentLegEmbodiment.cs` — presentation-only opponent pelvis/leg chain.
- `BoxerBootstrap.cs` — runtime composition/bootstrap.
- `BoxerVisualShell.cs` — procedural visual shell/decorations; note prior execution-order conflict with glove visuals.
- `Phase0SceneBuilder.cs` — deterministic scene/build entry points and WebGL build metadata.
- `P1*SelfTests.cs` — deterministic research/regression suites.

## Authoritative vs presentation contracts

### Authoritative

- combat state;
- punch intent/family;
- timing phases;
- authoritative target/path/endpoint;
- hit/block/miss resolution;
- counter state;
- HP/stamina/winner test semantics;
- A1/A3.1 coupling.

### Presentation-only

- rendered arm segments/joints;
- visual glove proxy;
- guard pose aesthetics;
- opponent leg/pelvis geometry;
- decorative shell;
- debug lines/traces;
- eventual production rig/animation.

A presentation task must not mutate authoritative geometry merely to make visuals appear to connect.

## Execution-order lesson

A prior yellow/orange punch artifact was caused by `BoxerVisualShell` adding gold glove/cuff children after arm embodiment had hidden original glove renderers. The correction re-hides original glove child renderers each frame.

Lesson: when visual systems create late children, inspect execution order and ownership before assuming combat-trace bugs.

## Build architecture

`Phase0SceneBuilder.BuildWebPlayer`:

- rebuilds deterministic scene;
- sets WebGL productVersion from `BOXER_BUILD_MARKER`;
- builds static-pages-compatible WebGL;
- computes data/wasm SHA256;
- writes build metadata/evidence;
- embeds research feature flags.

## Architectural rule for new mechanics

Prefer pure/static functions for causal mechanics where possible, with deterministic tests that isolate one variable. Keep visual interpolation and GameObject creation outside authoritative rules.
