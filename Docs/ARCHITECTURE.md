# ARCHITECTURE

This file describes the architecture **that actually exists**. Aspirations belong in the build plan, not here.

## Host

- Unity 6000.3.22f1, URP, Input System, Cinemachine, Burst/Collections/Mathematics (declared).
- Company Somapptic, product GummyDynasty.
- Authoritative simulation is the Windows editor/player. Phones join a local HTTP host (`PhoneHost` on port 8787) and send intentions. The host validates and applies them.

## Assemblies

| Assembly | Allowed to know | Exists |
|---|---|---|
| `GummyDynasty.Core` | services, events, no domain | yes |
| `GummyDynasty.Cognition` | belief field, observations, intents, hierarchy ids | yes |
| `GummyDynasty.Simulation` | session, bodies, arena, sandbox, logical population | yes |
| `GummyDynasty.Presentation` | camera | yes |
| `GummyDynasty.Input` | host Input System facade | yes |
| `GummyDynasty.UI` | IMGUI host HUD | yes |
| `GummyDynasty.Editor` | first-open bootstrap | yes |

Simulation does not reference UI or Presentation.

## Runtime objects

```
Boot
  AppBoot → loads Main
Main
  SessionDirector
  ToyArena
  ToySandboxDirector
  LogicalDirector
  Hud
  Input
  Camera + MainCameraRig
```

## Cognition

`BeliefField` is a graph of named nodes and roles.

- `Observe(η)`: η ≤ 0 is a no-op. Writes local + effective value.
- Dissipative roles decay toward 0.5 (unknown) and lose confidence.
- Unitary roles do not decay and are **not** reduced from children (faction orders survive).
- Parent reduce (`Mean` / `Max` / `Or`) synthesizes report roles from children.
- Child inherit pulls effective value toward the parent prior, scaled by `(1 - override)`.
- `Get` / `GetLocal` / `GetInherited` are all inspectable.
- Self-tagged observations do not update `NodeKind.World` nodes.

Installed tree:

```
world
  faction-red
    army-west
      formation-red
        gummy-*
```

`FormationTactics` is the file's policy: `march-west` / `spread` / `through-breach` / `hold`. The formation observes a cheap `SmashWallQuery` (wall ahead, hole, breach point). Children inherit that **tactic**. `IntentResolver` is still shadow-first: local threat or pain can `dodge` or `down`. Congestion is the file's problem, not another per-hopper sidestep. One flattened hopper does not scare the file.

`AgentMemory` is a Unity-free ring of the last 8 events (hit, flattened, saw-ally-down, incoming). `MemorySense` writes pain/threat from that history. A just-flattened actor stays `down` after the impulse dies; an untouched neighbor keeps marching. That is E2. `LastHitter` / `LastDownedAlly` are the only relationships.

This is the Umwelt *host* contract (value + confidence), not the Python substrate.

## Physics

`GummyFactory` builds a 5-body gummy (hips, belly, head, two arms) with springy `ConfigurableJoint`s. `GummyBody` stands with an Octodad-style one-way marionette string on the whole axis, and walks as a **bouncing-ball hop**: same-phase `VelocityChange` on head/belly/hips, then gravity. Joints stiffen while standing so they stay one blob. A traveling wave down the spine was a caterpillar — do not bring it back. Slack the string after a real hit; after `RecoverySeconds` they stand and take two up-only hops. `ToyArena` ground uses high-friction mats so landings die. `SmashWall` blasts only the named SmashWall group. A second `CratePile` and `LooseToys` (gumdrops + jawbreaker with `Tossable`) sit in the pit. `CandyShot` is a catapult stone (mass 5 at 10.77 m/s, KE ≈ 290) that punches through gummies and plants on the ground.

Shipping assets live under `Assets/_Project/Content/Resources/`:

- `Personalities/` — Gummy, Knight, Scout, Marcher
- `Projectiles/` — Candy, Gumdrop, Jawbreaker (`FIRE` / `LOB` / loose toys)
- `Factions/Red` and `Units/Levy` — color + personality + default intent stubs
- `Objectives/West` and `Modes/HoldWest` — capture / hold WEST (bootstrap writes them)

`PersonalityCatalog` loads them and falls back to the same preset numbers if an asset is missing. Menu **GummyDynasty > Create Personality / Projectile / Faction / Unit / Objective / Mode**.

`Machine` + `MachineControls` expose named tendrils (`aim`, `draw`, `release`, `load`). The pit catapult and pit cannon are instances. Host HUD `LOB` / `C` calls the catapult. Phone artillery can fire either. `G` is still the camera gun.

`PhoneCommand` / `PhoneSession` are Unity-free. `PhoneHost` is a `TcpListener` (no URL ACL). Commander and artillery are HTML pages. `GameModeRules` is hold-WEST. `QrEncode` draws the join URL.

Showcases (`ShowcaseKind`) rebuild the same arena: pit, castle keep (SmashWall is the east face; X still only that group), train of three kinematic cars. The WEST flag is parented to the engine. Cars carry a `MovingDeck`; still-attached wall crates are `MovePosition`'d with the mid car so the wall rides. Hoppers inherit that deck. The HUD QR is large enough to scan.

`BreakableStack` builds a jointed crate assembly. Smash wall, crate pile, and the north tower are the same primitive. Detached parts damp, then go kinematic once they rest.

## Logical population (M4)

`LogicalPopulation` is a growable array of `LogicalSoldier` (id, faction, pose, intent, pain/threat/objective, embodied flag). Tick is a plain loop: march west, dodge an incoming point, or idle. Embodied soldiers are skipped and written back from their `GummyBody`. No per-logical belief node — they read the formation prior as a bool/`Objective` on the struct. Snapshots: JSON (`ToJson`/`LoadJson`) and a `GDLS` binary blob (`ToBlob`/`LoadBlob`). Load always clears the embodied flag (bodies are not in the snapshot).

`LogicalCrowdView` draws disembodied soldiers as instanced orange dots **only when `LogicalDirector.ShowGhosts` is on** (backtick hatch, then `5` / bench B). Off by default — the sheet has no collision and is not the toy. `5` / `6` / `F9` do nothing until the hatch is open.

Physical marchers take `MarchFormation` slots (3-wide ranks). Spread widens the files; through-breach funnels on the hole, **single-file when the air is narrower than three bellies**. A hole is air a marcher can fit (`WallMeasure.PassableAir`), not crate-center spacing. Travel heading is always west. Arrival latches Hold; slots sit inside the 1.6 m victory band (`HoldEast` 0.9, or in-place on Hold) so the file can actually HELD. `Plant()` kills spin and matches any `MovingDeck` they are standing on (train cars). Idle grounded gummies brake so sphere colliders do not roll the pit into an ice rink. Incoming candy and sky crates share the same threat point — hoppers flinch before the stone lands.

`SmashWall` sits on the west corridor (about `x = -2`, thick in Z). Same named group, same X button. Castle keep walls are other `BreakableStack` groups; X does not blow them.

## Harness

`Tools/run-harness.ps1` compiles Cognition + `LogicalPopulation` + phone host + march math + `WallMeasure` + a tiny `ToyWorld` contact solver with `UnityEngine` stubs (VS2022 `csc`). No Hub, no license. It live-hits `PhoneHost`, walks march/artillery math, and runs the behavior lab (solid wall, slit vs door, candy hit, stack, DROP, deck carry). Writes `Logs/harness-report.md`. Play-mode benches append `Logs/play-bench.jsonl` via `BenchSink`.

If the editor is already open, `LabProbe` (`GummyDynasty > Run Behavior Lab (PhysX)`, or `Tools/Lab/inbox.json`) steps a local `PhysicsScene` and writes `Logs/lab-result.json`. That is real PhysX without launching Unity or clicking Play. Feel is still a human job.

Founding product spec: `Docs/Founding/Build-Objective.md` and `Docs/Founding/Execution-Directive.md`.

## Not present

Representation ladder (refine/collapse), attention scheduler, Burst jobs in use. First-pass castle/train do not refine. See `Docs/WISHLIST.md`.
