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
|  | B | logical only | 1000 |  |  |  | M4 |
|  | C | mixed fidelity |  |  |  |  | M5/M6 |
|  | D | refinement storm |  |  |  |  | M6 |
|  | E | autonomous fight |  |  |  |  | M3+ |
|  | F | wall collapse |  |  |  |  | M8 |
|  | G | fake phone clients |  |  |  |  | M10 |

How to run A (once the editor is open): play Main, press the bench buttons in the toy HUD, wait 8 seconds per step, copy the overlay numbers here.
