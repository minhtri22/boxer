# Phase 0 — Boxer Micro-Prototype Spec

## Status

**FROZEN P0 IMPLEMENTATION SPEC**

This document defines the smallest Unity artifact needed to answer the current
Phase 0 product question:

> Does Phone = Head, Left Thumb = Feet, Right Thumb = Fists, and automatic
> return-to-guard create a readable first-person boxing exchange that makes a
> player want another bout?

`plan.md` is authoritative for Phase 0 scope.

## Scope boundary

Phase 0 builds one boxing-specific Unity micro-prototype. It does not extract a
reusable engine and does not implement the P1 intent-to-anatomy, biomechanics,
energy, replay, career, networking, or production-content systems.

P1's rule is retained only as a boundary condition:

> Player controls intent; simulation resolves anatomy.

For P0 this is represented by a deliberately small deterministic mapping rather
than the future full resolver.

## Product mapping

| Player channel | P0 meaning | P0 implementation |
| --- | --- | --- |
| Phone | Head | Relative device attitude drives continuous lateral head/camera offset. |
| Left thumb | Feet / movement intent | Four-direction virtual stick drives boxer root translation. |
| Right thumb | Fists / punch intent | Gesture direction/length/curvature resolves a minimal jab/cross/hook set. |
| No conflicting active action | Return to guard | Both gloves continuously return toward high-guard poses after recovery. |

Left thumb never means left leg. Right thumb never means right hand. P0 chooses
the executing glove from a small deterministic alternating/availability rule so
the control can be tested without implementing P1 anatomy resolution.

## Authoritative conflict resolution

The older Phase 0 protocol files contain laboratory-style gates that conflict
with the newer `plan.md`, including generic sensor false-activation thresholds,
direction-accuracy thresholds, haptic interference experiments, battery/thermal
A/B tests, and language limiting P0 to a measurement harness.

For this task:

- generic gyroscope feasibility is accepted prior art;
- touch + device motion coexistence is accepted prior art;
- generic haptic/audio feasibility is accepted prior art;
- geometry collision capability is accepted prior art;
- battery/thermal testing is triggered only by an observed Boxer-specific
  problem;
- quantitative telemetry is diagnostic rather than an arbitrary product gate;
- the authorized artifact is the Unity Boxer Micro-Prototype in `plan.md` and
  the current Codex handoff.

No older threshold is silently promoted into a P0 PASS requirement.

## Runtime scene

Create one first-person scene containing:

- neutral floor/ring boundary;
- player root with POV camera/head collider;
- left and right placeholder gloves visible from first person;
- one simple opponent with head/body/guard colliders;
- opponent attack telegraph and punch geometry;
- compact debug HUD;
- short instruction text that disappears automatically.

No production art is required.

## Phone = Head baseline

### Input

Use device attitude when available. Capture a neutral reference on startup and
allow recalibration.

Editor fallback is explicitly synthetic and exists only for implementation QA:

- keyboard `Q/E` or equivalent simulates lateral head input;
- reset/recalibrate is available from keyboard/UI;
- synthetic evidence must be labelled `SYNTHETIC`.

### Mapping

The authoritative chain is:

```text
relative device roll/yaw component
→ dead zone
→ normalized signed head input
→ bounded lateral target offset
→ smooth head collider/camera displacement
```

There is no authoritative `Dodged = true` input flag.

Minimum behavior:

- continuous left/right displacement;
- neutral calibration;
- dead zone;
- maximum lateral bound;
- smoothing;
- return toward neutral when input returns to neutral;
- debug display of raw/normalized input and resolved offset.

Forward/backward head motion is deferred unless it is nearly free and isolated.

## Left-thumb footwork

The left half of the screen is a four-direction movement-intent surface.

Resolve normalized drag displacement to:

- forward;
- backward;
- left;
- right;
- diagonals as blended root velocity.

Movement is boxing-root translation on the ring plane with a bounded playable
area and stable facing toward the opponent. Individual foot sequencing is out of
scope.

An editor keyboard fallback may use WASD and must be labelled synthetic.

## Right-thumb punch grammar

The right half of the screen captures one gesture from pointer-down through
pointer-up. Features required for P0:

- displacement vector;
- path length;
- duration;
- approximate curvature / lateral deviation;
- average speed.

Small gestures under a minimum travel threshold are ignored.

Minimal classification:

```text
short + fast + mostly straight     → jab intent
longer + mostly straight           → cross intent
meaningfully curved/lateral path   → hook intent
```

The gesture specifies punch intent only. A small deterministic hand-selection
rule resolves the physical glove:

- jab prefers lead/left glove;
- cross prefers rear/right glove;
- hook uses the next available glove appropriate to the prototype animation;
- a glove in recovery cannot immediately recommit.

This mapping is a P0 test fixture, not P1 anatomy design.

Editor fallback keys may trigger jab/cross/hook for geometry/state testing and
must be labelled synthetic.

## Punch motion and anti-spam

Each punch has:

```text
guard → windup/commit → extension → recovery → guard
```

Use short boxing-specific action timers. Endless immediate swiping is limited by
commitment/recovery and temporary guard exposure. There is no P1 stamina or
energy model.

Repeated invalid gestures during commitment may be logged but do not queue an
unbounded attack chain.

## Automatic guard

When neither glove is committed to an attack, both gloves interpolate toward
high guard. Guard uses actual colliders.

Guard is not invulnerability:

- coverage is spatially limited;
- opponent body attacks may bypass a high guard;
- an attacking glove creates temporary exposure;
- opponent punch resolution evaluates guard geometry before vulnerable target
  geometry for the same trajectory.

## HIT / MISS / BLOCK

Outcomes are geometry-derived.

Opponent punch:

```text
swept punch path intersects player guard → BLOCK
swept punch path intersects player head/body → HIT
otherwise → MISS
```

Player punch follows the same geometry principle against opponent guard and
vulnerable targets.

The player's head collider is a child of the continuous head-offset transform;
therefore a slip can turn an expected head contact into a MISS without a dodge
state flag.

## Opponent

Use one intentionally simple opponent. Required behavior:

- maintains facing toward player;
- periodically selects from a small attack set;
- attack timing has bounded random variation;
- attack side/type has enough variation to prevent memorizing one fixed script;
- each attack has a visible windup/telegraph through glove/body motion;
- at least one straight attack creates a reliable slip → counter test;
- opponent has commitment/recovery that creates the counter opening.

Sophisticated AI is out of scope.

## Counter interaction

The required sequence is:

```text
READ
→ EVADE or BLOCK
→ opponent committed/recovering
→ OPENING
→ player punch lands
→ RESET
```

Counter state is primarily a vulnerability/recovery timing window. P0 may log a
counter outcome, but does not require a raw damage multiplier.

## Feedback

Use lightweight impact audio and mobile vibration/haptic calls only when they are
available without creating a separate research program.

Feedback must remain optional and must not be required to understand the bout.

## Debug and telemetry

Keep one compact runtime log and HUD with at least:

```text
time/tick
head input
resolved head offset
movement intent
punch intent
resolved player action
opponent action
guard state
last HIT/MISS/BLOCK outcome
counter window/state
```

Logs are diagnostic P0 evidence. They are not the P1 deterministic event/replay
architecture.

## Test modes

### Test 1 — First Contact

Show only:

```text
Left thumb moves.
Right thumb punches.
Move the phone to move your head.
Release actions to return to guard.
```

Then hide persistent instruction.

### Test 2 — Slip → Counter

Provide a repeatable opponent straight-punch mode so implementation/device QA
can test:

```text
see punch → move head → geometric MISS → counter → reset
```

### Test 3 — Move + Evade + Counter

Allow simultaneous footwork, continuous head displacement and right-thumb
attack input during the same exchange.

### Test 4 — Short unscripted bout

Default bout duration is 75 seconds, within the requested 60–90 second range.
Opponent uses mixed attacks and variable intervals.

## Automated QA targets

Automated EditMode tests should cover deterministic code where practical:

- head input dead-zone/bounds mapping;
- gesture classification;
- guard/hit geometry helper logic;
- action/recovery state transitions;
- counter-window state transitions.

Automated tests do not establish game feel, comfort, learnability, or replay
intent.

## Evidence taxonomy

Evidence written under `evidence/phase0/` must identify its source as one of:

- `EDITOR`;
- `REAL_DEVICE`;
- `SYNTHETIC`;
- `MANUAL_OBSERVATION`.

Editor keyboard simulations are `SYNTHETIC`, even when executed inside Unity
Editor.

## iPhone deployment boundary

The target is iPhone 12. On Windows, Unity may prepare an iOS/Xcode project when
iOS Build Support is installed, but native compilation, signing and installation
require a macOS/Xcode/signing path.

The task must inspect the installed Unity modules and available Apple deployment
infrastructure. If no valid path exists, real-device observations remain
`BLOCKED / NOT TESTED` and overall P0 cannot be PASS.

## P0 product gate

Overall PASS requires actual player/device evidence for all of the following:

- phone motion clearly feels connected to head evasion;
- no mandatory dodge button;
- feet + head + fists can be used in the same exchange;
- intentional slip → counter works;
- attacks are readable without intrusive arrows;
- swipe spam is not obviously dominant;
- automatic guard does not make inactivity optimal;
- short instruction is sufficient;
- no severe short-session discomfort;
- player can connect a representative HIT / MISS / BLOCK / COUNTER result to their own head, footwork and punch actions. Replay intent is not a decisive P0 gate for the placeholder-visual prototype.

Until actual human/device interaction is recorded, these product criteria remain
NOT TESTED even if editor implementation and deterministic tests pass.

## Stop rule

When sufficient evidence exists to answer:

> Can I move, read, evade, counter, reset — and understand how my own inputs caused the outcome?

stop P0 work and report the evidence. Do not begin P1 or engine extraction.
