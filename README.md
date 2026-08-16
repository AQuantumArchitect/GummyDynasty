# GummyDynasty

Unity **6.3 LTS** (`6000.3.22f1`, Personal) warfare toy box: gummy-bear armies in large physical battles. Hierarchical abstraction + Umwelt-derived cognition. Castle siege and a train fight are later evidence, not the product.

Read `Docs/` before changing architecture.

| Doc | Role |
|---|---|
| `Docs/PROJECT_STATE.md` | what works, what's next |
| `Docs/ARCHITECTURE.md` | what the code actually is |
| `Docs/UMWELT_MAPPING.md` | what we took from `ws-win/umwelt` |
| `Docs/RESEARCH_LEDGER.md` | experiments |
| `Docs/BENCHMARKS.md` | frame-time tables |
| `Docs/HANDOFF.md` | how a new Unity dev pokes the toy |

## Open

1. Unity Hub as `somapptic@gmail.com`. Editor **6000.3.22f1** only (never 2022.3.65f1).
2. Project path: `C:\Users\Luke Spooner\wkspaces\GummyDynasty`.
3. First open imports packages and runs bootstrap (URP + Boot/Main + toy arena).

## Play the toy

`1/2/3` spawn · click select · space launch · `K` knock · `F` fire · `B` smash wall · `R` reset · `F5–F8` bench · RMB orbit.

## Layout

| Folder | Role |
|---|---|
| `Runtime/Core` | boot, services, events |
| `Runtime/Cognition` | BeliefField (η, dissipative decay, self-tag) |
| `Runtime/Simulation` | bodies, arena, session — no cameras |
| `Runtime/Presentation` | orbit camera |
| `Runtime/Input` | host-debug keys |
| `Runtime/UI` | host HUD (not the player HUD) |
| `Docs` | ledger |

Cloud: org `somapptic`, project `GummyDynasty` (`f39c2cd0-2bad-4ac0-902e-63f48b75baa8`).
