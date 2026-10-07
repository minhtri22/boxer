# Ramirez Round 2 Pages deployment

Date: 2026-10-07. Deployment: `PASS`. Integration acceptance remains
`IMPLEMENTATION_BLOCKED`; Human/device UAT has not been performed.

This is an additive deployment record. Handoff 25b accurately records the earlier
pre-deployment state and is preserved unchanged. The owner subsequently requested
GitHub Pages deployment and explicitly approved adding the isolated integration
branch to the Pages environment allowlist.

## Exact candidate

- Branch: `integration/ramirez-unity-round2`.
- Workflow checkout SHA: `e3c1208ccaaeea0b19e0995e3e4f86173f50e789`.
- Unity SOURCE_SHA: `4940f8061eaf1ed58bcf5356a1bce4817de6eab0`.
- ART_SOURCE_SHA: `3e4e13c4b4451cb542e068652508ec12e248225f`.
- Artifact: `builds/web/boxer-round2`, already committed; no rebuild or payload edit.
- URL: <https://minhtri22.github.io/boxer/>.
- Cache-busting device URL: <https://minhtri22.github.io/boxer/?uat=4940f806>.
- Explicit synthetic desktop URL:
  <https://minhtri22.github.io/boxer/?desktop=1&uat=4940f806>.

## Authorization and workflow evidence

First dispatch [37608392227](https://github.com/minhtri22/boxer/actions/runs/37608392227)
failed before runner steps because `github-pages` rejected the integration branch.
Following explicit owner permission, only one exact branch policy was added:
`integration/ramirez-unity-round2`, type `branch`, policy ID `62241161`.
Read-after-write checks confirmed all pre-existing policies and environment
protection settings remained unchanged. Existing policy IDs:
`59076861` (main), `59150555` (p0/control-legibility-bout-closure),
`59160047` (p1/whole-body-mechanics).
No main/p0/p1 push, merge, protection removal, or environment substitution occurred.

New workflow_dispatch run
[37608813127](https://github.com/minhtri22/boxer/actions/runs/37608813127)
was created at `2026-10-07T10:39:09Z`, checked out the exact SHA above, and completed
`success`. Deploy job `112750783597` and every step succeeded. The existing
`p0-web-deploy.yml` uploads the committed build folder; it does not build Unity.

## Deployed verification

`evidence/ramirez-unity-round2/pages-smoke.json` records HTTP 200 and exact SHA-256
matches for all seven canonical files: provenance, HTML, data, framework, loader,
wasm and CSS. These were fetched directly over deployed HTTPS and compared with
local immutable provenance. The deployed provenance identifies SOURCE_SHA above.

A fresh isolated headless Edge desktop session loaded the deployed URL using
the explicit synthetic desktop flag, clicked the startup button, reached the
Unity canvas, and exercised Q/E/W. Startup `PASS`: zero page errors, zero engine
console errors, zero failed requests. Screenshot `pages-candidate.png` shows
the initial tutorial (1/5 HEAD MOVEMENT), not a completed tutorial or bout.
No target-device gesture, sensor, combat acceptance, visual-reference acceptance,
or performance PASS is inferred from this smoke test.

Known 25b blockers remain: native-mesh/contact-envelope mismatch, historical
triangle budget, POV reference fidelity, and unverified target-device performance.
This deployment makes the unchanged diagnostic candidate available for owner UAT;
it does not resolve those blockers.

On a phone, open the normal URL, press ENABLE MOTION & START, and allow motion
access. The desktop query bypasses sensor testing intentionally and must not be
used as phone-motion acceptance evidence. Refresh or use the cache-busting URL
if an old page remains cached.
