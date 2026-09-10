# Boxer Future PvP Architecture Direction

## Vision

The final Boxer experience is player vs player boxing.

The current AI opponent is a training/sparring foundation only.

## Not Implemented In UAT Round 2

Do not implement:
- multiplayer networking
- matchmaking
- server infrastructure
- player synchronization

## Future Hybrid Networking Model

Preferred direction:

Player input / intent
        |
Network layer
        |
Combat resolution
        |
State synchronization

Avoid synchronizing every bone transform every frame.

Prefer synchronizing intent:
- movement input
- punch intent
- guard state
- head movement intent

Each device can render local animation.

## Replay Consideration

Combat history recording may be used later for:
- highlights
- replay clips
- training analysis

Not part of current phase.
