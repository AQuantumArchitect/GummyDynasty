# RESEARCH_LEDGER

Negative results stay here.

Format:

```
QUESTION
HYPOTHESIS
IMPLEMENTATION
COMPARISON
RESULT
DECISION
FOLLOW-UP
```

---

## E0 — Can this laptop run the Unity 6.3 project?

- **QUESTION:** Does 6000.3.22f1 Personal open GummyDynasty and enter play mode on the i7-5700HQ / 16 GB / GTX 960M host?
- **HYPOTHESIS:** Yes, after the editor installer finishes.
- **IMPLEMENTATION:** Hub/editor first open + bootstrap.
- **COMPARISON:** n/a
- **RESULT:** pending (editor not installed at time of writing)
- **DECISION:** pending
- **FOLLOW-UP:** If first open OOMs or license-fails, stop and record. Do not install 2022.3.65f1.

## E1 — PhysX ragdoll budget (Benchmark A)

- **QUESTION:** How many active 5-body gummies until p95 frame time is unusable?
- **HYPOTHESIS:** 32–80 on this GPU/CPU.
- **IMPLEMENTATION:** `ToySandboxDirector` spawn + `FrameTimeSampler`.
- **COMPARISON:** 1 / 8 / 16 / 32 / 64 / 96
- **RESULT:** pending play mode
- **DECISION:** pending
- **FOLLOW-UP:** That number is the attention-scheduler envelope, not the product cap.

## E2 — α-blend field vs tiny FSM

- **QUESTION:** Does the C# belief field change behavior usefully vs a 20-line FSM on 2–8 gummies?
- **HYPOTHESIS:** History-dependent knockdown recovery and self-tag will be visible in the inspector; cost will be negligible at this N.
- **IMPLEMENTATION:** `GummyAgent` + `BeliefField` vs a later `FsmAgent` baseline.
- **COMPARISON:** same toy scene, both brains
- **RESULT:** pending M2 play
- **DECISION:** pending
- **FOLLOW-UP:** If the field is only vocabulary, keep it as the inspectable bus and put decisions in a simpler policy until M3.
