# BENCHMARKS

Hardware line is mandatory on every row.

**Host:** Intel Core i7-5700HQ @ 2.70 GHz (4c/8t), 16 GB DDR3, NVIDIA GeForce GTX 960M + Intel HD 5600, Windows 10, Unity 6000.3.22f1, URP.

Prefer p50 / p95 / p99 frame time (ms) over a single FPS number.

| Date | Bench | Setup | N | p50 ms | p95 ms | p99 ms | Notes |
|---|---|---|---|---|---|---|---|
|  | A | empty Main | 0 |  |  |  | not run |
|  | A | active gummies | 1 |  |  |  |  |
|  | A | active gummies | 8 |  |  |  |  |
|  | A | active gummies | 16 |  |  |  |  |
|  | A | active gummies | 32 |  |  |  |  |
|  | A | active gummies | 64 |  |  |  |  |
| 2026-08-17 | A | human play | ~100 |  |  |  | Play report: "handled 100s with ease." No p50/p95 yet. |
| 2026-08-17 | B | harness logical tick | 1000 | 0.046 |  |  | `Tools/run-harness.ps1` — ms **per Tick()**, not frame time. No PhysX. |
| 2026-08-17 | B | harness logical tick | 3000 | 0.141 |  |  | same harness |
|  | C | mixed fidelity |  |  |  |  | M5/M6 |
|  | D | refinement storm |  |  |  |  | M6 |
|  | E | autonomous fight |  |  |  |  | M3+ |
|  | F | wall collapse |  |  |  |  | Play **SMASH** (or LOB the tower). Sampler writes this row to `Logs/play-bench.jsonl`. |
|  | G | two phones on LAN |  |  |  |  | Play: commander + artillery. Harness E9 covers validate only. |

How to run A (once the editor is open): play Main, press F5–F8 (or the bench is still key-only for A), wait 8 seconds per step, copy the overlay numbers here.

How to run B: play Main, press **5** if you want to see the army first, then **F9** / **BENCH B**. First 5s ticks logicals only (status: "ticking N logicals"). Next 5s spawns N cubes and marches them the same way. Copy both p50/p95 lines. Destroyed cubes should vanish when it finishes.

How to run F: play Main, press **X** / **SMASH** (or knock the north tower). Wait 5 seconds. Copy the overlay line into this table.
