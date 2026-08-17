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
- **RESULT:** Yes. 6000.3.22f1 Personal installed. Batchmode first-open compiled all assemblies. License groups: old ULF `F4-TNUX-…` and somapptic `20067637073452-UnityPersonal`. EditMode 9/9. Boot/Main exist. ~16 GB RAM, no OOM on import.
- **DECISION:** Stay on 6000.3.22f1 Personal. Do not use 2022.3.65f1.
- **FOLLOW-UP:** Human play-mode for E1.

## E1 — PhysX ragdoll budget (Benchmark A)

- **QUESTION:** How many active 5-body gummies until p95 frame time is unusable?
- **HYPOTHESIS:** 32–80 on this GPU/CPU.
- **IMPLEMENTATION:** `ToySandboxDirector` spawn + `FrameTimeSampler`.
- **COMPARISON:** 1 / 8 / 16 / 32 / 64 / 96
- **RESULT:** pending instrumented p50/p95. Human play 2026-08-17: "handled 100s with ease" on this laptop.
- **DECISION:** envelope is at least ~100 active ragdolls. Do not shrink the toy. Ladder still matters for thousands.
- **FOLLOW-UP:** Fill F5–F8 numbers when someone watches the overlay. Envelope for M6, not a product cap.

## E2 — α-blend field vs tiny FSM

- **QUESTION:** Does the C# belief field change behavior usefully vs a 20-line FSM on 2–8 gummies?
- **HYPOTHESIS:** History-dependent knockdown recovery and self-tag will be visible in the inspector; cost will be negligible at this N.
- **IMPLEMENTATION:** `AgentMemory` ring (8) + `MemorySense` writes pain/threat from `Flattened`/`Hit`. Resolve reads `Get` for objective and `GetLocal` for threat/pain/congestion (formation Max-threat was making the neighbor dodge). `IntentResolver` maps pain ≥ 0.7 to `down`. Same two actors, one remembered flattened, one not.
- **COMPARISON:** same field, same west order, 8 ticks at 0.05 s. Harness + EditMode `AgentMemoryTests.E2_FlattenedHistoryChangesIntent`.
- **RESULT:** harness 2026-08-17: flattened resolves `down`, neighbor resolves `march-west`, `LastHitter` is `candy`. Field is not only vocabulary — history changes the action.
- **DECISION:** keep the field as the inspectable bus. Policy stays the small resolver table. No FSM fork.
- **FOLLOW-UP:** Human Play of the right-hand inspector. If timid lasts too long, lower `AgentMemory.FadePerSecond` wait — raise it (0.12 now). Do not add a second brain.

## E3 — Hierarchical inherit vs local override

- **QUESTION:** Can a faction order survive the crowd, while an individual still dodges an incoming crate?
- **HYPOTHESIS:** Unitary command roles must skip upward reduce. Local override on `objective` keeps the dodge from being blended back into the march.
- **IMPLEMENTATION:** `BattleHierarchy` + `IntentResolver` + west-march sandbox (`4`).
- **COMPARISON:** inherited `march-west` vs local `dodge` on the same selected gummy, shown in the HUD.
- **RESULT:** EditMode tests cover inherit, override, unitary survival, cost reduce, ancestry, and the three intents. Play-mode scripted proof pending human `4`.
- **DECISION:** keep this as the M3 policy. Do not add Belavkin.
- **FOLLOW-UP:** If the march never stands up, tune `WalkForce` / recover — do not invent a second brain.

## E4 — Candy shot vs X-button ice rink

- **QUESTION:** Why did FIRE turn the pit into an ice rink, and what budget is “modestly less dramatic than X”?
- **HYPOTHESIS:** The old shot (mass 3.4 at 18 m/s, KE ≈ 551) was ~18× one crate blast and then kept rolling on a frictionless floor. The foam dart (0.92 @ 8, KE ≈ 29) then did nothing and dropped short of the target.
- **IMPLEMENTATION:** Midpoint KE ≈ 290: mass 5 / 10.77 m/s, scale 0.9. Ballistic loft so it actually hits. Punches through gummies; settles (×0.22, damping 4.2) only on kinematic ground. High-friction ground. Gummy bounce combine Average.
- **COMPARISON:** smash wall implied energy ≈ 626. Candy is ~46% of the wall, ~53% of the old ball, ~10× the foam dart.
- **RESULT:** numbers locked in `CandyShot`. Play rejected the foam dart (could not hit, no punch). Physics.Simulate pile tests are in `CandyShotFeelTests` (batchmode blocked by Hub license handshake on this machine).
- **DECISION:** FIRE is a planted catapult stone. X is still the boom. Do not go back to 18 m/s.
- **FOLLOW-UP:** Human pile-fire in Play. If it still slides, settle harder on ground — do not cut mass first.

## E5 — Anguished caterpillar gait

- **QUESTION:** Why did marchers look like writhing worms instead of peppy gummy warriors / VeggieTales hops?
- **HYPOTHESIS:** Phase-lagged forces down a soft spine (head, then belly, then hips) is peristalsis. Continuous sine acceleration shears floppy joints. A peppy hop is one bouncing ball, not a wave.
- **IMPLEMENTATION:** Same-phase `VelocityChange` kick on the whole axis (`HopUp` 2.25 / `HopForward` 1.55 / `HopHz` 1.45). One-way marionette string (Octodad / Nylund: lift head+torso only when below hang; slack above so they can leave the ground). Joints stiffen while standing. Wait for first landing before the motor starts. Animation Mentor: vertical bounce and forward travel are independent.
- **COMPARISON:** Prior cascade (shares 0.50 / 0.28 / 0.22 with 1.05 rad lag) vs coherent hop. Research: squash-stretch jump cycle (squash → stretch-launch → land-squash); PuppetMaster/active-ragdoll pin-to-pose not sequential tugs; 640Lab marionette hover; do not apply traveling waves down a joint chain.
- **RESULT:** pending human `4` play. EditMode `Hop_MovesUprightAxisWest` covers west travel + upright axis.
- **DECISION:** one blob bounce. Do not restore the segment wave.
- **FOLLOW-UP:** If they still fold, raise hop stiffness before adding more force. If they don't travel, raise `HopForward` before `HopHz`.

## E6 — Per-logical belief nodes vs a formation prior

- **QUESTION:** Should each of 1,000 logical soldiers own a `BeliefField` node?
- **HYPOTHESIS:** No. The field is for inspectable minds. 1k nodes would measure the graph, not the soldier, and fail the M4 cheapness gate.
- **IMPLEMENTATION:** `LogicalSoldier` stores objective/threat/pain/intent. Tick reads `Marching` + incoming point. Embodied soldiers keep their existing actor node and write back.
- **COMPARISON:** formation prior on a struct vs `BattleHierarchy.AttachActor` × 1000.
- **RESULT:** EditMode covers tick, dodge, JSON/blob, embody skip, and SoA vs Transform. Play Benchmark B pending human F9.
- **DECISION:** no per-logical belief node until refine (M5) promotes one.
- **FOLLOW-UP:** Parked. Friend toybox first. See `Docs/WISHLIST.md`. Ghost sheet stays a hatch (`5`), not the product.

## E8 — Formation replan vs individual dodge

- **QUESTION:** When the west road is blocked, does the *formation* change tactic, while one incoming threat and one flattened hopper still act locally?
- **HYPOTHESIS:** A tiny formation table (spread / through-breach / hold) plus inherited tactic beats another per-hopper congestion dodge. Faction west order stays unitary.
- **IMPLEMENTATION:** `FormationTactics` + `SmashWallQuery` + `BattleHierarchy.ObserveRoad`. `IntentResolver` no longer sidesteps on congestion. Individuals still dodge threat and go `down` on pain.
- **COMPARISON:** wall+no hole vs hole vs arrived; same two actors, one threatened / one flattened / one calm+crowded.
- **RESULT:** harness 2026-08-17: spread / through-breach / hold; faction objective stays west; incoming = dodge; flattened = down; crowded neighbor inherits spread.
- **DECISION:** keep the table. Do not add a second brain. Do not add another wall.
- **FOLLOW-UP:** Human Play of `4` → wad → open a hole → pile through. If they wander instead of funnel, tighten the breach slots before changing the table.

## E9 — Phone commands stay intentions

- **QUESTION:** Can a phone join and change the pit without owning sim, and can garbage / the wrong role be rejected without Unity?
- **HYPOTHESIS:** A Unity-free command + session table is enough. HTTP is just a pipe. Two roles: commander (west/hold) and artillery (aim/load/fire).
- **IMPLEMENTATION:** `PhoneCommand` + `PhoneSession` + `GameModeRules` + `PhoneHost` (`TcpListener` :8787). QR via `QrEncode`. Cannon is a second `Machine` on the same tendrils.
- **COMPARISON:** malformed / second commander / commander-fires vs valid west + artillery fire + hold-WEST victory band.
- **RESULT:** harness 2026-08-17 E9: garbage rejected; commander+artillery join; second commander blocked; commander cannot fire; artillery fire validates; hold-WEST wins in band; ordered hold beats wall; QR 25×25 with finders. Human Play is a real phone on the LAN.
- **DECISION:** local HTTP, not Unity Netcode. Host stays authoritative.
- **FOLLOW-UP:** If Windows firewall blocks 8787, document allow-through. Do not add a cloud relay.

## E7 — Agent harness without a Unity license

- **QUESTION:** Can the agent run benches when Hub owns the seat and batchmode dies at handshake 505?
- **HYPOTHESIS:** Yes, if cognition + logical tick compile with `csc` against tiny `UnityEngine` stubs, and Play writes `Logs/play-bench.jsonl` for PhysX numbers.
- **IMPLEMENTATION:** `Tools/run-harness.ps1` + `Tools/Harness/*` + `BenchSink`.
- **COMPARISON:** Unity batchmode (blocked) vs VS2022 `csc` (ran).
- **RESULT:** Harness OK. Candy midpoint, inherit, march, blob, 1000 logicals at 0.046 ms/tick, 3000 at 0.141 ms/tick.
- **DECISION:** Harness is the agent bench path. Do not fight Hub for GUI Unity.
- **FOLLOW-UP:** Grow the harness when toybox logic is Unity-free enough to share. Hop/PhysX stay human Play.
