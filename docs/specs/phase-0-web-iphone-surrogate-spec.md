# Phase 0 — Web iPhone Surrogate Spec

## Purpose

Provide the existing Boxer P0 interaction through HTTPS to iPhone 12 Safari:

`Windows → Unity WebGL → HTTPS → Safari → real motion + real touch → human P0 validation`.

This is a delivery surrogate. It does not establish native iOS performance and does not unlock P1.

## Control mapping

- Phone orientation remains the P0 head channel.
- Left thumb remains footwork.
- Right thumb remains punch gesture input.
- Existing geometry, guard, opponent states, HIT / MISS / BLOCK / COUNTER resolution and counter window remain authoritative.

Browser flow:

1. User taps `ENABLE MOTION & START`.
2. Safari motion/orientation permission is requested from that user gesture.
3. Permission state is shown explicitly.
4. Browser orientation events are forwarded to `P0 Systems/BoxerInput`.
5. User holds a comfortable neutral portrait position and taps `CALIBRATE & START BOUT`.
6. Neutral `gamma` is recorded; the existing head-angle path continues from the relative angle.

No silent mouse/keyboard fallback counts as valid iPhone evidence. Required failure states include `DENIED`, `UNAVAILABLE`, `HTTPS REQUIRED`, and `DEVICE ORIENTATION NOT RECEIVED`.

## Web presentation

- portrait/mobile viewport;
- no page scrolling/rubber-band during combat;
- canvas uses the dynamic viewport;
- touch actions and Safari gesture defaults are suppressed where practical;
- fullscreen is optional because Safari support is limited.

## P0-D — Immediate Control Comprehension & Agency

Ask after the short bout:

> “Bạn có cảm thấy chính thao tác đầu/chân/tay của mình tạo ra kết quả vừa xảy ra không?”

Then:

> “Hãy mô tả một tình huống vừa rồi: tại sao đòn đó trúng / trượt / bị đỡ / phản đòn được?”

PASS requires the tester to connect the observed result to their own control actions.

## Result separation

- `P0 Interaction`: PASS / CONDITIONAL PASS / FAIL / BLOCKED
- `Web Delivery Quality`: GOOD / DEGRADED / POOR
- `Native iOS Performance`: NOT TESTED

WebGL/Safari visual or performance degradation alone does not fail P0 interaction.
