# GummyDynasty

Unity **6.3 LTS** (`6000.3.22f1`, Personal) warfare toy box: gummy-bear armies in large physical battles. Hierarchical abstraction + Umwelt-derived cognition. Castle and train are first-pass showcases (phones + machines + stacks). The representation ladder is still later.

Product spec (do not rewrite lightly):

- `Docs/Founding/Build-Objective.md` — what the finished engine is
- `Docs/Founding/Execution-Directive.md` — how to run the program

Read `Docs/PROJECT_STATE.md` before changing architecture.

| Doc | Role |
|---|---|
| `Docs/PROJECT_STATE.md` | what works, what's next |
| `Docs/ARCHITECTURE.md` | what the code actually is |
| `Docs/UMWELT_MAPPING.md` | what we took from `ws-win/umwelt` |
| `Docs/RESEARCH_LEDGER.md` | experiments |
| `Docs/BENCHMARKS.md` | frame-time tables |
| `Docs/HANDOFF.md` | 60-second toy for a friend |
| `Docs/WISHLIST.md` | scale research after the friend drop |

## Open

1. Clone this repo. Unity Hub, your own account. Editor **6000.3.22f1** only (never 2022.3.65f1).
2. Hub → Projects → Add → Add project from disk → the cloned `GummyDynasty` folder.
3. First open imports packages and runs bootstrap (URP + Boot/Main + toy arena). Then read `Docs/HANDOFF.md`.

## Play the toy

`1/2/3` spawn · click a gummy (inspector) · `space`/`V` yeet · `K` knock · `G`/FIRE · `C`/LOB · `T`/DROP · `X`/SMASH · `4` march west · phone QR / URL (commander + artillery) · `7`/`8`/`9` pit/castle/train · `R` reset · RMB orbit.

Agent benches (no Unity license): `Tools/run-harness.ps1` → `Logs/harness-report.md`. Play benches write `Logs/play-bench.jsonl`.

Edit these, not the factory: `Personalities/Gummy`, `Projectiles/Candy`, `Units/Levy`. Large-scale ghosts (`5`/`6`/`F9`) stay behind the HUD RESEARCH toggle. See `Docs/WISHLIST.md`.

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
