# HANDOFF

Written as we go. A new Unity developer should be able to play and poke without reading engine internals.

## Open the project

1. Unity Hub, signed in as `somapptic@gmail.com` (Personal).
2. Editor **6000.3.22f1** only. Do not use 2022.3.65f1.
3. Add `C:\Users\Luke Spooner\wkspaces\GummyDynasty` if it is not listed.
4. First open imports packages and runs `GummyDynasty/Bootstrap Project` if needed.

## Play the toy (M1)

1. Open `Assets/_Project/Scenes/Main.unity` (created on first open) or press Play from Boot.
2. Keys:
   - `1` spawn default gummy
   - `2` spawn heavy knight
   - `3` spawn tiny scout
   - `Click` select
   - `Space` launch selected / nearest
   - `K` knock down
   - `F` fire a projectile at the selection
   - `B` smash the crate wall
   - `R` reset arena
3. Change a `PhysicalPersonality` asset and spawn again.

## What you should not have to touch

`BeliefField`, factory joint setup, session clock. Those are engine. You should be editing personalities, later factions, weapons, machines, and maps.

## Not ready for you yet

Authoring inspectors, castle, train, phone join, formations. Those arrive in later milestones.
