# GummyDynasty

New Unity **6.3 LTS** project (`6000.3.22f1`) on the `somapptic` account.

This is a production-shaped shell, not a vertical slice. Domain systems go in after the first design pass.

## Editor

- Unity **6000.3.22f1** (Personal). Do not use 2022.3.65f1 (Enterprise XLTS).
- URP + Input System + Cinemachine + Timeline.
- Company: Somapptic. Product: GummyDynasty.
- Cloud: org `somapptic`, project `GummyDynasty` (`f39c2cd0-2bad-4ac0-902e-63f48b75baa8`).

## Open

1. Unity Hub, signed in as `somapptic@gmail.com`.
2. Add project → `C:\Users\Luke Spooner\wkspaces\GummyDynasty`.
3. First open will import packages and run `GummyDynastyBootstrap` (creates URP assets + Boot/Main scenes if missing).

## Layout

| Folder | Role |
|---|---|
| `Assets/_Project/Runtime/Core` | Boot, clock, events, service registry. No presentation. |
| `Assets/_Project/Runtime/Simulation` | Authoritative state and commands. No cameras/UI. |
| `Assets/_Project/Runtime/Presentation` | Scene views, cameras, VFX. |
| `Assets/_Project/Runtime/Input` | Input System wrapper. |
| `Assets/_Project/Runtime/UI` | UI Toolkit / HUD. |
| `Assets/_Project/Editor` | First-run bootstrap and editor tools. |
| `Assets/_Project/Content` | Art, audio, prefabs, ScriptableObjects, pipeline settings. |
| `Assets/_Project/Scenes` | `Boot` then `Main`. |
| `Assets/Tests` | EditMode + PlayMode. |

Assemblies keep Simulation free of UI so we can grow systems without a spaghetti `GameManager`.

## Play

Boot scene loads Main. ESC in play mode does nothing until we wire pause.

## Next

Need the design brief (genre, 2D/3D, single vs multiplayer, scope) before the intense build starts.
