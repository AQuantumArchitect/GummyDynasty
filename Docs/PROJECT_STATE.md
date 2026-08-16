# PROJECT_STATE

Keep this short. Reread it.

## Current milestone

**M0 in progress → M1 implementation started.**

Unity 6.3.22 (`6000.3.22f1`) Personal is the editor. Installer was still downloading when this file was written. Project has not had a first editor open yet, so URP assets and Boot/Main scenes are created by `GummyDynastyBootstrap` on first open.

## Working

- Unity 6.3 LTS project shell, Personal / somapptic Cloud
- Assembly walls: Core, Cognition, Simulation, Presentation, Input, UI, Editor, Tests
- Session clock (`SessionState` / `SessionDirector`)
- C# Umwelt-host contract (`BeliefField`: observe η, dissipative decay, parent reduce, self-tag)
- M1 toy systems in code: `PhysicalPersonality`, `GummyFactory`, `GummyBody`, `ToyArena`, `ToySandboxDirector`, breakable parts, projectile
- EditMode tests for session + belief field

## Not working yet

- Editor binary `C:\Program Files\Unity\Hub\Editor\6000.3.22f1` (download/install)
- First play-mode run, URP assets, scenes on disk
- Measured ragdoll budget (Benchmark A)
- Hierarchical factions, ladder, phones, machines, castle, train

## Immediate objectives

1. Finish editor install + first Hub open.
2. Play `Main` and confirm gummies spawn, flop, launch, break a wall.
3. Record Benchmark A on this laptop (i7-5700HQ, 16 GB, GTX 960M).
4. Wire M2 inspector onto a selected gummy.

## Known failures

- None measured. No play-mode evidence yet.

## Blockers

- Editor download/install. No other product blockers.

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
