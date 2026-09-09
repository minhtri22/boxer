# 14 — Agent Working Protocol

## Operating principle

> **PROVE ONLY WHAT IS STILL UNCERTAIN → DECIDE → IMPLEMENT**

Do not spend time re-proving established platform technology or rewriting already-accepted mechanics without a new reason.

## Required workflow

For every substantial task:

`SYNC → READ SOURCE OF TRUTH → STATE VERIFIED BASELINE → IDENTIFY UNCERTAINTY → FREEZE HYPOTHESIS → MINIMAL CHANGE → TEST → EVIDENCE → SOURCE COMMIT → CLEAN BUILD → ARTIFACT COMMIT → REPORT`

## Mandatory pre-implementation statement

Before code changes, write:

1. current branch and expected HEAD;
2. verified baseline;
3. exact unresolved question;
4. hypothesis;
5. single causal variable or tightly scoped change;
6. acceptance gate;
7. regression suite;
8. explicitly out-of-scope items.

If repository evidence contradicts memory/handoff text, report the discrepancy and resolve it before implementation.

## Scope discipline

Do not:

- start the next roadmap phase inside the current task;
- combine several uncertain mechanics into one patch;
- convert diagnostic metrics into gameplay without an experiment;
- change combat authority to hide presentation bugs;
- change presentation to claim a combat mechanic exists;
- silently retune frozen constants;
- declare HUMAN PASS from deterministic tests or local smoke;
- rebase after provenance-locked build;
- force push unless explicitly authorized;
- reopen Safari optimization work unless delivery strategy changes.

## Research patch pattern

A preferred patch looks like:

- hypothesis file/spec frozen first;
- one small mechanics or presentation change;
- dedicated deterministic tests;
- all prior regressions pass;
- build with exact provenance;
- real-device UAT question limited to the hypothesis;
- result doc with PASS/FAIL classification.

## Integrated UAT policy — product-owner override

As of 2026-09-09, standalone real-device UAT for intermediate whole-body milestones is deferred. Intermediate milestones may advance when their frozen deterministic/regression gates pass. Do not label them `HUMAN_PASS`. Build and presentation convergence may continue autonomously. The next required human gate is one final integrated UAT candidate containing the approved UI/UX and visual package.

## Source ownership

When ChatGPT/GitHub connector implements source and local agent builds:

- connector-side source edits must be committed/pushed first;
- local agent syncs exact branch HEAD;
- local agent may make only minimal compile/meta corrections if genuinely necessary;
- any such correction becomes a new source commit before build;
- local agent then tests/builds/commits artifacts.

## Reporting language

Use precise labels:

- `IMPLEMENTATION_PASS`
- `DETERMINISTIC_PASS`
- `BUILD_PASS`
- `LOCAL_SMOKE_PASS`
- `HUMAN_PASS`

Do not collapse them into a single vague PASS.

## Decision rule

If a human-visible problem remains but tests pass, trust the human-visible failure for presentation/readability claims. Add observability or a new test only if it helps isolate the next uncertainty; do not use more tests to argue against UAT.
