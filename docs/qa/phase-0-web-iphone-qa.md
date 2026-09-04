# Phase 0 — Web iPhone QA

## Build path

Unity 6000.5.8f1 on Windows builds WebGL with:

```text
Unity.exe -batchmode -nographics -quit
  -projectPath unity/BoxerP0
  -executeMethod BoxerP0.Editor.Phase0SceneBuilder.BuildWebPlayer
```

Output: `builds/web/boxer-p0-web`.

The project uses `PROJECT:BoxerP0Mobile` and disables WebGL compression so a static Pages host does not require custom `Content-Encoding` headers.

## Manual iPhone 12 Safari sequence

1. **Motion Permission + Calibration** — user-triggered permission succeeds; orientation is received; neutral calibration succeeds.
2. **Head-only evade** — phone/head movement moves the actual head collider and an incoming straight can MISS geometrically.
3. **Touch coexistence** — motion remains active while left-thumb footwork and right-thumb punch input work.
4. **Slip → Counter** — READ → HEAD EVADE → MISS → COUNTER → RESET.
5. **Move + Slip + Counter** — feet + phone/head + punch coexist in one exchange.
6. **60–90 s bout** — observe cognitive load, accidental Safari gestures, motion lag, touch conflicts, readability and discomfort.
7. **Control Comprehension & Agency** — ask the two Vietnamese questions in the surrogate spec and record the explanation.

## Debug overlay

The runtime exposes browser permission, alpha/beta/gamma, neutral gamma, resolved head angle/offset, movement intent, last punch intent, guard state, opponent state, last outcome and frame time.

## Evidence provenance

Store evidence under `evidence/phase0/web-iphone/` in separate categories:

- `WEB_BUILD`
- `IPHONE_SAFARI`
- `HUMAN_OBSERVATION`
- `SYNTHETIC`
- `WINDOWS_EDITOR`

Do not infer iPhone/human PASS from Windows or synthetic evidence.
