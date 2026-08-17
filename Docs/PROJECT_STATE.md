# PROJECT_STATE

Keep this short. Reread it.

## Current milestone

**M0 complete. M1/M2 code compiled and tested. Waiting on a human play-mode pass.**

Editor `6000.3.22f1` Personal is installed at `C:\Program Files\Unity\Hub\Editor\6000.3.22f1`. First batchmode open compiled every GummyDynasty assembly. Bootstrap created URP + Boot/Main. EditMode tests: **9/9 passed**.

## Working

- Unity 6.3.22 Personal (ULF + somapptic assigned seat)
- Windows IL2CPP / standalone support
- Assembly walls: Core, Cognition, Simulation, Presentation, Input, UI, Editor, Tests
- `BeliefField` + 7 cognition tests
- Session clock + 2 tests
- M1/M2 toy code compiled into Main: gummies, crate wall, projectiles, host HUD, orbit camera
- URP asset + Boot/Main scenes on disk

## Not working yet

- Play-mode evidence (Benchmark A numbers). Batchmode cannot flop gummies.
- Hierarchical factions, ladder, phones, machines, castle, train

## Immediate objectives

1. Open Main in the editor GUI and play the toy.
2. Record Benchmark A (F5–F8) on this laptop.
3. Tune default jelly if the first pile is boring or explodes.

## Known failures

- First batchmode quit reported compiler errors because the EditMode asmdef listed TestRunner twice. Fixed.
- Bootstrap `DontDestroyOnLoad` threw in the editor. Removed; `AppBoot.Awake` still does it in play mode.

## Blockers

- None for opening the project. Play-mode needs you (or an editor GUI session) because agent GUI launches have died at the licensing handshake before.

## Major decisions

- Hybrid sim: PhysX only for active bodies; logical/aggregate are C# data.
- Umwelt adapted in C#, not embedded Python.
- α-blend default. Belavkin/Bloch compact state is an M7 experiment.
- Phones are browsers against an authoritative host.
- No day-one Entities/DOTS.

## Next actions

- Install 6000.3.22f1 + IL2CPP module.
- First-open GummyDynasty.
- Play the toy. Write the first bench row.
