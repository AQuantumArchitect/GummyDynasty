# HANDOFF

A new Unity developer should be able to play and poke without reading engine internals.

## Open the project

1. Clone this repo. Unity Hub, signed into **your** account (Personal is fine).
2. Editor **6000.3.22f1** only. Do not use 2022.3.65f1. Install it from Hub → Installs if you do not have it.
3. Hub → Projects → Add → Add project from disk → the `GummyDynasty` folder you cloned.
4. First open imports packages and runs `GummyDynasty/Bootstrap Project` if needed.

## 60-second toy

1. Menu **GummyDynasty > Open Toy (Main)** if the viewport is empty/grey. That opens `Assets/_Project/Scenes/Main.unity`.
2. Click the **Game** tab (not Scene). Press Play. Mint checker pit, pink rails, WEST flag, a smashable crate wall **across the west road**, a smaller crate pile, a knockable tower, a wood catapult **and an iron cannon** on the east, pink gumdrops, one purple jawbreaker, and 3 gummies dropping in. The title should read `GUMMY PIT   n   2026-08-17  next R`. A join URL and QR sit on the HUD.
3. Grey void usually means an untitled scene was playing. Stop Play, use the menu above, Play again.
4. If the wall still looks like the old glued pile, press `R` once. Stacks are jointed now.

Do this:

| Do | How |
|---|---|
| Spawn | `1` gummy · `2` knight · `3` scout |
| Select | click a gummy, a candy, or a **wall crate** (it highlights) |
| Yeet | `space` / `V` / **YEET** — selected gummy, candy, or crate |
| Knock | `K` — they get back up, but stay timid for a few seconds |
| Fire | `G` / middle-mouse / **FIRE** — camera gun, at the selection |
| Lob | `C` / **LOB** — east catapult throws a stone **at** the selection |
| Drop | `T` / **DROP** — one crate falls from the sky onto the selection |
| Smash | `X` / **SMASH** — the big wall. That is enough rubble; do not add more walls. |
| March | `4` / **MARCH** — they keep ranks, **spread** at the wall, pile through a hole, **plant** at WEST. The title should flip to **HELD** when the file arrives. |
| Phone | scan the HUD QR, or open the printed URL on the same LAN. **COMMANDER** = west / hold. **ARTILLERY** = tap map + FIRE (catapult or cannon). |
| Showcase | **PIT** / **CASTLE** / **TRAIN** on the HUD, or `7` / `8` / `9`. On TRAIN the WEST flag rides the engine; hoppers should stay on the cars. |
| Reset | `R` |
| Orbit | right mouse |

Click a flattened gummy. The inspector should show `down`, a `Flattened` memory, and `mad at candy` (or `host`). Its untouched neighbor should still be marching.

Press `4`, then click a ranked orange hopper. The inspector `file` line should say `spread` at the wall, then `through-breach` after you open a **passable** hole (two crate columns, LOB, or X). One yanked crate is a slit, not a door.

## Things you should edit

Do **not** open `GummyFactory`. Change an asset, press Play.

1. `Assets/_Project/Content/Resources/Personalities/Gummy.asset` — mass / color / recovery. Press `1`.
2. `Assets/_Project/Content/Resources/Projectiles/Candy.asset` — mass / color / speed. **FIRE** and **LOB** both use it.
3. `Assets/_Project/Content/Resources/Units/Levy.asset` — personality + faction + default intent. The first gummies in the pit spawn from this unit.

Menus: **GummyDynasty > Create Personality / Projectile / Faction / Unit / Objective / Mode**.

Hold WEST lives in `Assets/_Project/Content/Resources/Modes/HoldWest.asset`. If a phone cannot connect, allow Windows through on port **8787**, or open the URL in a browser on this PC first.

## What you should not have to touch

`BeliefField`, factory joint setup, session clock, `MachineControls`. Those are engine.

## Agent lab (no Play click)

`Tools/run-harness.ps1` is how the agent tests **behavior**: wall blocking, gap air vs marcher belly, through-breach slots, candy hits, stacks, DROP, moving decks, incoming dodge, hold-WEST. It does not test hop feel.

If the Unity editor is already open: menu **GummyDynasty > Run Behavior Lab (PhysX)**, or the agent writes `Tools/Lab/inbox.json` and reads `Logs/lab-result.json`. That uses a local PhysicsScene. It still cannot launch the editor (Personal Hub handshake).

A one-crate slit is not a door. Open two columns, LOB a stone, or press X.

## Research hatch (ignore unless asked)

Backtick or `5` opens the research hatch (ghost army / embody / bench B). Ghosts have no collision. They are not the toy.

`F5–F8` still run physical bench A. **SMASH** also writes Benchmark F into `Logs/play-bench.jsonl`. `5` / `6` / `F9` do nothing until the hatch is open.
