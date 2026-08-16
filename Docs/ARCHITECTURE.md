# ARCHITECTURE

This file describes the architecture **that actually exists**. Aspirations belong in the build plan, not here.

## Host

- Unity 6000.3.22f1, URP, Input System, Cinemachine, Burst/Collections/Mathematics (declared).
- Company Somapptic, product GummyDynasty.
- Authoritative simulation is the Windows editor/player. No network session exists yet.

## Assemblies

| Assembly | Allowed to know | Exists |
|---|---|---|
| `GummyDynasty.Core` | services, events, no domain | yes |
| `GummyDynasty.Cognition` | belief field, observations, intents | yes |
| `GummyDynasty.Simulation` | session, bodies, arena, sandbox director | yes |
| `GummyDynasty.Presentation` | camera | yes |
| `GummyDynasty.Input` | host Input System facade | yes |
| `GummyDynasty.UI` | IMGUI host HUD | yes |
| `GummyDynasty.Editor` | first-open bootstrap | yes |

Simulation does not reference UI or Presentation.

## Runtime objects (intended after first bootstrap)

```
Boot
  AppBoot → loads Main
Main
  SessionDirector
  ToyArena
  ToySandboxDirector
  Hud
  Input
  Camera + MainCameraRig
```

## Cognition (exists as C#)

`BeliefField` is a graph of named nodes and roles.

- `Observe(η)`: η ≤ 0 is a no-op.
- Dissipative roles decay toward 0.5 (unknown) and lose confidence.
- Unitary roles do not decay.
- Parent reduce (`Mean` / `Max` / `Or`) synthesizes a parent role from children.
- Child inherit pulls a child toward its parent as a weak prior.
- Self-tagged observations do not update `NodeKind.World` nodes.

This is the Umwelt *host* contract (value + confidence), not the Python substrate.

## Physics (exists as C#, unrun)

`GummyFactory` builds a 5-body gummy (hips, belly, head, two arms) with springy `ConfigurableJoint`s and a `PhysicsMaterial`. `GummyBody` tracks Locomoting / Ragdoll / Recovering. `ToyArena` builds a ground, rails, ramp, and a breakable crate wall.

## Not present

Representation ladder, attention scheduler, machines, destruction graphs, HTTP/phone session, game-mode data, castle/train content, Burst jobs in use.
