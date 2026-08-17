# UMWELT_MAPPING

Local repo: `C:\Users\Luke Spooner\ws-win\umwelt`  
Remote: `AQuantumArchitect/umwelt`

This is what we take, what we adapt, and what we refuse. It is not a class named `UmweltAgent`.

## What Umwelt actually is

A **belief-field engine**: a world is a `DomainSpec` (nodes, roles, bridges, bindings, outputs, drivers). Observations update a graph of beliefs with confidence η. Actions are shadow-first and self-tagged so the engine does not learn its own footprint as world signal. Default estimator is a classical **α-blend**. Full Belavkin filter was measured and **denied** as default.

Game-facing contract (`umwelt.host`): `Observation`, `Intent`, `Belief(value, confidence)`, `step`. Internals (Bloch, density matrices) stay behind that boundary.

Game-shaped proof already in-repo: `examples/fledgeling_fog`, `examples/gridworld`. Nested LOD / scale kit is explicitly unscheduled in Fledgeling core — that is *our* research problem, not something we can import.

## Direct reuse (concepts + contract)

| Umwelt | GummyDynasty |
|---|---|
| `Observation.confidence` η, 0 = no-op | `Cognition.Observation.Eta` |
| `Belief.value` + `Belief.confidence` | `Cognition.Belief` |
| Parent tree + `reduce` | `BeliefField.SetParent` + `ReduceOp` |
| Dissipative vs unitary roles | `RoleMode` — dissipative default |
| Self-tag / `actor_id` | `Observation.SelfTagged` + `ActorId` skips `NodeKind.World` |
| Shadow-first outputs | later machine/AI tendrils (not built) |
| Attention as belief + hysteresis actuate | later relevance scheduler (M6) |
| CLAIMS.md honesty | `Docs/RESEARCH_LEDGER.md` |

## Adapt (C#, realtime, Unity)

- Port the host contract, not `umwelt.substrate`.
- Tick on the sim clock, not Python `datetime`.
- One `BeliefField` per battle (shared world) plus per-faction / per-agent nodes.
- Logical soldiers (M4) do **not** get a belief node each. They inherit a formation prior on the struct. A node is minted only when the soldier is embodied.
- Compact manifolds (Bloch, ρ, cumulants) only as **M7 comparison** if scalar/statistical aggregates fail reconstruction.

## Reject for the game process

| Thing | Why |
|---|---|
| `umweltd` / Python engine in-frame | not a 60 Hz battle kernel |
| `substrate.population` | genetic Hamiltonian search, not soldiers |
| BPU `ComputeScheduler` | accelerator routing, not ragdoll LOD |
| Belavkin default | measured worse than α-blend |
| Forge, Berry live authority, dream loop | unproven / wrong layer |
| Domain words inside a generic engine | keep battle vocabulary in Simulation/Authoring |

## First usefulness test (E2)

The field earns a permanent job only if, on 2–8 gummies, inspectable history (just-flattened vs untouched) produces different action, *or* self-tag prevents a measurable poison, *or* hierarchical reduce in M3 is clearly cheaper than N independent brains.

Harness + EditMode 2026-08-17: flattened history → `down`, neighbor → `march-west`. The field stays the inspectable bus. Policy stays the small resolver table.
