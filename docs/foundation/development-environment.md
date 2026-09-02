# Boxer — Development & Test Environment

## 1. Purpose

This document records the currently available development and real-device test environment for Boxer so that implementation agents (including Codex) choose tools, build targets, and proof procedures that match the actual hardware available to the project owner.

The environment is a project constraint, not a suggestion.

---

## 2. Primary Development Machine

Current development machine:

- **Operating system:** Windows 11 Home Single Language, 64-bit
- **CPU:** Intel Core Ultra 7 258V
- **CPU topology:** 8 cores / 8 logical processors
- **Memory:** 32 GB RAM
- **GPU:** Intel Arc 140V integrated GPU
- **Reported graphics memory:** up to 16 GB shared/available graphics memory depending on workload and system allocation

This Windows machine is the primary local development environment.

Implementation work should assume:

- development happens primarily on Windows,
- local Unity Editor workflows must run acceptably on this machine,
- experimental tooling and automated/synthetic tests should be runnable locally where practical,
- heavyweight production graphics are not required during Phase 0.

---

## 3. Game Development Stack

The project has selected **Unity** as the implementation environment for the Boxer Phase 0 experimental harness and subsequent game development unless future evidence justifies a documented change.

Phase 0 therefore should use:

- Unity,
- C#,
- Unity-supported mobile input/device-motion APIs,
- minimal rendering and physics sufficient for proof,
- deterministic/loggable experimental code where practical.

### Phase 0 constraint

Unity is being used to build the **Sensor Combat Harness**, not a production Boxer game and not the reusable Embodied POV Engine.

The reusable engine remains locked until Phase 0 PASS.

---

## 4. Available iOS Test Device

Primary real-device test target currently available:

- **Device:** iPhone 12
- **Role:** primary Phase 0 human-on-device sensor, touch, POV, haptic, audio, comfort, battery, and thermal test device

The Phase 0 harness should therefore be designed so that it can ultimately run on this iPhone 12.

The iPhone 12 should be treated as the first real reference device, not as proof of the entire iOS device range.

Any Phase 0 report must clearly distinguish:

- results measured on the iPhone 12,
- results from Unity Editor simulation,
- results from synthetic sensor traces,
- results inferred from platform documentation,
- results not yet tested on real hardware.

Never convert Editor or synthetic results into iPhone PASS results.

---

## 5. Important Windows + iOS Build Constraint

The primary development machine runs Windows.

Native iOS application signing/build deployment normally requires Apple's iOS build toolchain/Xcode on macOS at the final native-build stage.

Therefore Codex must not assume that a Windows-only local Unity installation can directly produce and sign a deployable iPhone build without an available macOS/Xcode path.

During Phase 0, Codex must:

1. inspect the actual available environment before choosing the iPhone deployment path,
2. document any missing iOS build/signing dependency,
3. still implement all Unity-side harness code that can be built/tested on Windows,
4. create clear iPhone build/test instructions,
5. mark real-device tests as `BLOCKED — IOS BUILD/DEVICE RUN REQUIRED` if the environment cannot deploy to the iPhone,
6. never fabricate device results.

A future macOS/Xcode build path, CI service, remote Mac, or other legitimate iOS deployment route may be introduced later, but Phase 0 must not assume one exists unless it is actually available and documented.

---

## 6. Phase 0 Device-Test Priorities

When a real iPhone 12 build is available, the following tests have priority:

1. neutral motion calibration,
2. left/right head-motion responsiveness,
3. touch + motion interference,
4. motion-to-visual latency,
5. slip/counter interaction,
6. haptic-generated sensor interference,
7. audio/haptic perception,
8. 10-minute comfort test,
9. battery consumption comparison,
10. thermal behavior comparison.

The iPhone 12 is particularly important because these cannot be credibly proven in the Unity Editor alone:

- actual gyroscope/attitude behavior,
- actual grip jitter,
- actual haptic feedback,
- physical comfort,
- screen visibility while tilting the phone,
- real battery drain,
- real thermal behavior,
- subjective embodied-control feel.

---

## 7. Performance Philosophy

Phase 0 should optimize for **measurement quality and stable interaction**, not visual fidelity.

The harness should remain lightweight enough that poor frame rate does not contaminate sensor/interaction findings.

Record at minimum:

- Unity version,
- render pipeline if relevant,
- target frame rate,
- observed frame time/FPS,
- device and OS,
- build configuration.

Do not attribute poor control feel to the interaction model until performance-induced latency has been separated from sensor/control latency.

---

## 8. Codex Environment Rule

Any Codex task working on Boxer must read this document before selecting the implementation or test strategy.

Codex must not:

- switch away from Unity merely for convenience,
- assume Android hardware is currently available,
- assume a Mac/Xcode machine is currently available,
- claim real-device iPhone measurements from Editor simulations,
- build production systems to compensate for unavailable device testing.

If a required Phase 0 proof cannot be performed with the currently available environment, implement the minimum measurement support, document the manual procedure, and mark the proof as blocked rather than inventing evidence.
