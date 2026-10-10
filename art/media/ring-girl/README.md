# Owner-supplied ring introduction

Received 2026-10-09 from `C:\Users\minht\Downloads\2aOboR4CBuiwTV0HVSgmXU4FA6Oy4cVXAyUjiHfU.mp4`.
The original is retained unchanged as `source.mp4` (SHA-256
`1dec4970847aa75bc8ef8d2cf518253554486ef8d2ee4460b3f270c9ca3569ac`).
Source: 6.014989 seconds container / 6 seconds video, 480x854, 24 fps,
H.264 High, AAC stereo 44100 Hz. Owner confirmed bell/crowd content and
authorized splitting its sounds. No generated/substitute sound is used.

Spectral inspection shows stable bell partials starting about 5.17 seconds
(approximately 1320, 2240, 3300 and 4580 Hz). These are inferred cue boundaries,
not human listening approval or isolated source stems. The tail still contains
background audience. Sound quality, loudness and loop acceptance require owner UAT.

Derived `Assets/StreamingAssets/RingMedia` files (FFmpeg 8.1.1):

- `ring-girl.mp4`: video 0–5.166667, H.264 CRF20, yuv420p, faststart, no audio.
- `intro.mp3`: audio 0–5.166667, 128 kbps, 80 ms fade-out at end.
- `bell.mp3`: corrected 2026-10-10: explicitly `atrim=start=5.15:end=6.00,asetpts=PTS-STARTPTS` BEFORE local fades, 160 kbps, 10 ms fade-in / 110 ms fade-out. Reproduce with `tools/repair_ring_bell.ps1` (guards the silent baseline before overwrite).
- `crowd.mp3`: audio 0.25–4.85, 128 kbps, 80 ms fades at both loop edges.

Browser playback: muted inline video and its synchronized intro sound;
start bell at media completion then scored combat and quieter crowd loop;
stop crowd and one end bell at KO or timeout. Sound follows M mute, including
all ring tracks. Original clip and source file in Downloads are untouched.

Correction: the previously released derived bell decoded to exact digital silence (peak/RMS 0), despite positive duration/playback-event tests. The owner's missing-bell report is verified. The corrected 0.85-second decoded mono cue has peak 0.782724, RMS 0.146582 and active-sample fraction 0.977191; SHA256 `21b2cd12f42967f0b2ee0da27b51e80dd3826ca7e530d4bb00fd17bb94a8c5a7`. Original silent file is preserved in `evidence/wave1/bell-repair/bell-silent-before.mp3`. The new PCM and browser-decoder signal gates do not establish listening approval on the target phone.
