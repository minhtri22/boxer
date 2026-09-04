# WEB_BUILD blocker — 2026-09-04

Environment: Windows 11, Unity 6000.5.8f1, WebGLSupport installed.

Two batch verification attempts were made from the Codex sandbox.

1. Normal batch mode failed before compilation because Unity Package Manager could not establish its local IPC stream.
2. Retrying with `-noUpm` loaded the project assemblies and WebGL support module, then stalled at `Licensing is not yet initialized`. The sandbox also emitted `attempt to write a readonly database`, consistent with Unity licensing state requiring write access outside the permitted workspace.

The stalled Unity process was terminated after the failure mode was established.

Result: **WEB_BUILD = BLOCKED IN CODEX SANDBOX**. This is a tooling/licensing-sandbox blocker, not P0 interaction evidence and not a native iOS result.

Logs:

- `../WINDOWS_EDITOR/self-tests.log`
- `../WINDOWS_EDITOR/self-tests-noupm.log`
