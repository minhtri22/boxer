# Phase 0 — QA

## Scope

QA covers the Unity Boxer Micro-Prototype defined by
`docs/specs/phase-0-boxer-micro-prototype-spec.md`.

Automated and synthetic checks verify deterministic implementation behavior only.
They do not establish boxing feel, visual readability, comfort, learnability, or
replay intent.

## Environment

| Item | Value |
| --- | --- |
| Unity | 6000.5.8f1 (5cb7df797b7d) |
| Development OS | Windows 11 64-bit |
| Editor GPU observed | Intel Arc 140V |
| Windows player target | StandaloneWindows64, Development build |
| Primary real-device target | iPhone 12 |
| iPhone deployment | BLOCKED / NOT TESTED |

## Build and run methods

Scene generation:

```text
Unity.exe -batchmode -quit
  -projectPath unity/BoxerP0
  -executeMethod BoxerP0.Editor.Phase0SceneBuilder.Build
```

Deterministic QA:

```text
Unity.exe -batchmode -quit
  -projectPath unity/BoxerP0
  -executeMethod BoxerP0.Editor.Phase0SelfTests.RunBatch
```

Windows build:

```text
Unity.exe -batchmode -quit
  -projectPath unity/BoxerP0
  -executeMethod BoxerP0.Editor.Phase0SceneBuilder.BuildWindowsPlayer
```

Synthetic runtime smoke:

```text
BoxerP0.exe -batchmode -nographics -p0SyntheticDemo -p0SmokeSeconds=6
```

The synthetic demo drives movement, head offset and punch requests together so
runtime state and telemetry can be inspected without representing the run as a
human playtest.

## Deterministic tests

Latest result: **9 PASS**.

| Test | Expected | Actual | Status |
| --- | --- | --- | --- |
| Head dead zone | small input resolves to zero offset | PASS | PASS |
| Head sign and bound | left/right sign retained and max offset bounded | PASS | PASS |
| Gesture jab | short fast straight gesture resolves to jab | PASS | PASS |
| Gesture cross | longer straight gesture resolves to cross | PASS | PASS |
| Gesture hook | lateral/curved gesture resolves to hook | PASS | PASS |
| Geometry hit/miss | segment/sphere helper separates intersection from miss | PASS | PASS |
| Default fight range | default player punch trajectory can geometrically reach opponent head | PASS | PASS |
| Anti-spam state | second punch during commitment is rejected; action returns to guard | PASS | PASS |
| Counter window | window is closed during extension and open during recovery | PASS | PASS |

Evidence: `evidence/phase0/SYNTHETIC/deterministic-self-tests.txt`.

## Windows build QA

Latest development build:

```text
result=Succeeded
size_bytes=152925748
```

The build contains the P0 scene and runtime scripts and completed without a
compile/build error. Unity cloud configuration endpoints were unreachable during
some build steps, but the local player build still completed successfully.

Evidence: `evidence/phase0/EDITOR/windows-build/build-metadata.txt`.

## Runtime smoke QA

The rebuilt Windows player launched using NullGfx, initialized the P0 scene,
executed the synthetic concurrent input demo, and exited intentionally with:

```text
P0_SMOKE_COMPLETE
```

The latest telemetry demonstrates implementation-level coexistence of the three
channels. One observed synthetic exchange includes:

```text
opponent straight attack
→ player BLOCK
→ opponent recovery / counter window OPEN
→ non-zero movement intent
→ non-zero head offset
→ player cross
→ PLAYER_COUNTER_HIT
```

This verifies that a geometry/result path exists for the intended exchange. It
does not prove that a human can read or enjoy that sequence.

Evidence:

- `evidence/phase0/SYNTHETIC/runtime-smoke-summary.txt`
- `evidence/phase0/SYNTHETIC/runtime-telemetry-excerpt.csv`

## Defects found during QA

### P0-001 — runtime material creation failed under NullGfx

Initial smoke testing exposed an `ArgumentNullException` because runtime material
creation depended on `Shader.Find` returning a shader in a headless player.

Resolution: reuse each primitive renderer's existing material and set supported
color properties. Rebuilt runtime smoke completed successfully afterward.

Status: **FIXED**.

### P0-002 — default player attack geometry could not reach opponent

Initial synthetic telemetry showed player attacks resolving to MISS because the
opponent started outside the actual punch endpoint. The opponent gloves were also
placed on the wrong local-Z side for a root rotated 180 degrees.

Resolution:

- move opponent to the intended P0 fighting distance;
- place opponent gloves toward the player;
- prevent the player root from passing through the opponent;
- keep the player facing the opponent during footwork;
- expose the opponent vulnerable target during recovery so commitment creates a
  real counter opening.

The deterministic range check now passes and runtime telemetry records
`PLAYER_COUNTER_HIT` during an opponent recovery window.

Status: **FIXED**.

## Required P0 scenarios

| Scenario | Current evidence | Status |
| --- | --- | --- |
| First Contact | instruction exists; no human first-use observation | NOT TESTED |
| Slip → Counter | implementation and synthetic geometry/state path exists | BLOCKED for human/device validation |
| Move + Evade + Counter | synthetic telemetry shows all channels active during an exchange | BLOCKED for human/device validation |
| 60–90 s unscripted bout | 75 s mode implemented; no human bout completed | NOT TESTED |
| Replay intent question | prompt exists; no participant answer | NOT TESTED |

## Real-device status

The installed Unity 6000.5.8f1 playback engines are:

```text
WebGLSupport
windowsstandalonesupport
```

`iOSSupport` is not installed. The Windows environment also has no `xcodebuild`,
`ios-deploy`, or `ideviceinstaller` command and no discovered macOS/Xcode path
hint.

Therefore this environment cannot produce, sign and install the required iPhone
12 build. No real-device evidence is fabricated.

Real-device status: **BLOCKED / NOT TESTED**.

Evidence: `evidence/phase0/REAL_DEVICE/blocker.md`.

## QA conclusion

The Unity implementation compiles, builds and survives a synthetic concurrent
runtime exchange with geometry-derived outcomes and a working counter opening.
The decisive Phase 0 product gate remains open because the required iPhone/human
interaction has not been performed.
