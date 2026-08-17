# PROJECT_STATE

Keep this short. Reread it.

## Current milestone

**Phones + showcases (Waves P and S) are in the pit.** Founding spec lives in `Docs/Founding/`. Wave **R (representation research)** is next, and still parked until you ask for it.

Editor `6000.3.22f1` Personal.

## Working

- Hopping gummies, camera candy (G), pit catapult (C / LOB), pit cannon (phone artillery), smash wall
- Phone join: LAN URL + QR on the HUD. Commander issues west/hold. Artillery aims and fires catapult or cannon.
- One mode asset: Hold WEST. Victory when the ranked file plants in the flag band.
- Showcases: **PIT** / **CASTLE** / **TRAIN** (`7`/`8`/`9` or HUD). Same hoppers, stacks, machines, phones.
- `4` west march: formation **spreads** at the wall, **through-breach** when a hole opens, **hold** at WEST
- Friend HUD: toys + showcase + join URL. Research is backtick / `5`.
- Agent harness: `Tools/run-harness.ps1` now owns **behavior / contact** (wall gap, collisions, stacks, decks), not just cognition math. Feel stays Play.

## Not now

- M5–M7 representation ladder / crowd abstraction — after you ask (see `Docs/WISHLIST.md`)
- Extra smash walls or a gate
- Hop or candy retune unless Play reports a regression

## Immediate objectives

1. Human play of feel only (hop, slide, QR scan, train ride in the room). Agent iterates wall-gap / collision / hold / artillery math via `Tools/run-harness.ps1`. If the editor is already open, `GummyDynasty > Run Behavior Lab (PhysX)` or drop `Tools/Lab/inbox.json` for a real PhysX pass.
2. Wave R stays parked. Cases for it are at the bottom of `Docs/WISHLIST.md`.

## Known failures

- Agent Unity launches still die at the Hub license handshake. The harness is the autonomous loop. A one-column crate slit is **not** a hole anymore (a marcher cannot fit); two columns or rubble is.
- `5` / backtick still exist. Ghost sheet has no collision. Not the toy.
- First-pass castle/train have **no** refine/collapse. They are phones + machines + destruction + formation intent, not the representation hypothesis.
- Leftover slide on grass is still a Play follow-up, not a wave. Train hoppers now inherit the deck so they should not be rug-pulled.
- Wave R stays parked. Playtest cases for it live at the bottom of `Docs/WISHLIST.md`.

## Major decisions

- Phones before research.
- Local HTTP, not Unity Netcode. Host is authoritative.
- Cannon is the second machine. No gate.
- Castle/train compose existing primitives. A train-specific brain is an abstraction failure.
- Formation owns the road. Individuals only override for incoming threat or pain. X is enough rubble.
