# Gummy Warfare Engine — Build Objective

## Mission

Build a Unity-based game engine and authoring environment for large, chaotic, highly physical battles between autonomous toy-like characters.

The engine should make it inexpensive—computationally and developmentally—to create scenarios involving large numbers of characters, destructible structures, vehicles, siege machinery, environmental hazards, factions, formations, objectives, and player interventions.

The primary technical ambition is to achieve scale through **hierarchical abstraction**: simulate only as much detail as is currently meaningful, while preserving convincing continuity as entities move between levels of representation.

Use the local `AQuantumArchitect/umwelt` repository as an important research and architectural resource. Investigate whether Umwelt's compact state representations, belief dynamics, hierarchical models, history dependence, and observation/action structure can serve as a general mechanism for compressing both simulation state and autonomous behavior.

This is intended to become an engine that can be handed to a new Unity developer as a **physics warfare toy box**. The difficult simulation, AI, networking, and orchestration machinery should already exist. The recipient should be able to spend their effort inventing units, maps, weapons, machines, factions, game modes, parameters, art, sounds, and ridiculous situations.

Build toward that complete target. Intermediate prototypes, benchmarks, experiments, rewrites, and throwaway implementations are expected steps toward the destination rather than definitions of scope.

---

# Product Vision

The central fantasy is:

**Gummy Bears in Dynasty Warriors.**

Large armies of charming, physical, ragdoll-like creatures collide in ridiculous battles across interactive environments.

Characters should feel weighty, floppy, throwable, crushable, launchable, pileable, and funny. Their apparent softness is primarily an experiential and visual quality: articulated physical characters, good collision reactions, animation, ragdoll behavior, stretching/squashing where useful, expressive rendering, and convincing interactions with crowds and objects.

A battle might involve:

- hundreds of gummy soldiers assaulting a castle;
- catapults throwing soldiers or ammunition over walls;
- defenders dropping objects into packed formations;
- walls breaking into rigid chunks;
- troops piling into breaches;
- champions smashing through crowds;
- moving vehicles carrying battles through a level;
- trees, rocks, machinery, debris, traps, bridges and structures participating physically;
- armies dynamically reorganizing as the environment changes;
- players influencing all of this through personal tactical interfaces.

The physics should create stories.

---

# The Engine Is the Product

Design the project so that individual scenarios are compositions of reusable systems rather than bespoke levels.

A content creator should eventually be able to construct things such as:

- castle assaults;
- bridge battles;
- hill control;
- convoy attacks;
- train battles;
- ship boarding;
- moving fortresses;
- giant-monster encounters;
- artillery duels;
- capture objectives;
- escort missions;
- siege defense;
- multi-faction battles;
- king-of-the-hill;
- asymmetric survival;
- arena battles;
- battles on unstable structures;
- battles involving vehicles or moving platforms;
- ridiculous experimental modes nobody anticipated during engine development.

The value of the engine is that these become inexpensive combinations of existing capabilities.

---

# World Model

Represent the simulated world hierarchically.

Useful levels may include concepts such as:

```text
WORLD
  └── FACTION
       └── ARMY
            └── FORMATION / GROUP
                 └── UNIT
                      └── PHYSICAL BODY
```

These levels should be composable rather than rigidly hardcoded.

Each level may have:

- state;
- beliefs;
- goals;
- intent;
- history;
- relationships;
- observations;
- uncertainty;
- physical properties;
- simulation fidelity;
- ownership;
- parent context;
- child context.

A lower-level entity should inherit meaningful context from the systems containing it while retaining its own local state.

A soldier can therefore behave as the intersection of:

```text
faction intent
+ army intent
+ formation intent
+ local observations
+ individual history
+ current physical situation
```

The same conceptual machinery should work at multiple scales.

---

# Umwelt as the Cognitive Substrate

Inspect the local Umwelt repository deeply rather than implementing a superficial imitation of its vocabulary.

Develop a Unity-appropriate implementation or bridge that lets Umwelt concepts participate directly in simulation.

The desired conceptual model is:

```text
observations
      ↓
belief / internal state
      ↓
intent
      ↓
action
      ↓
world consequences
      ↓
new observations
```

Apply this recursively.

A faction can maintain beliefs about the battle.

A formation can maintain beliefs about its local tactical situation.

An individual can maintain beliefs about nearby threats, allies, objectives, terrain and its own recent experience.

Parent state should influence child priors and intentions without eliminating local autonomy.

Child observations should be able to aggregate upward.

Examples:

A faction believes:

> breach the western wall and enter the fortress.

A formation inherits:

> move toward the western breach.

Its local state observes:

> the breach is congested and under artillery fire.

It forms the intent:

> spread around the obstruction and seek adjacent entrances.

A particular gummy observes:

> nearby ally knocked down, projectile approaching from right, wall opening ahead.

Its immediate behavior emerges from the composition of all those states.

Make these systems inspectable during development.

We should be able to watch what entities believe, what they intend, what information contributed to that state, and how those quantities change over time.

---

# Umwelt as a Simulation-Compression Substrate

The most ambitious research objective is to determine whether the same general machinery can reduce the cost of physical world simulation.

Do not assume every visible object requires equivalent simulation detail at all times.

The engine should understand the world at several resolutions.

For example, a distant formation might be represented primarily through quantities such as:

```text
population
center of mass
spatial extent
density
momentum
cohesion
formation
pressure
terrain relationship
combat engagement
aggregate health
intent
```

Its constituent soldiers can still exist logically without every soldier requiring full physical processing.

As relevance increases, the representation can refine.

Conceptually:

```text
aggregate state
      ↓ refinement
groups / clusters
      ↓ refinement
individual agents
      ↓ refinement
full physical interaction
```

And in the opposite direction:

```text
physical individuals
      ↓ aggregation
group state
      ↓ aggregation
formation state
```

Changes of representation should conserve the properties necessary to make transitions convincing.

Important quantities may include:

- population;
- location;
- occupied volume;
- momentum;
- velocity distributions;
- formation;
- casualties;
- energy;
- direction of travel;
- relationships;
- combat state;
- intent;
- history.

Investigate whether Umwelt-style compact manifolds or density-matrix-inspired representations provide useful ways of storing distributions, correlations, uncertainty, or latent state at these reduced resolutions.

Treat this as an empirical engineering and research problem.

Build benchmarks and comparison cases.

If a representation allows 1,000 logical soldiers to behave convincingly while only a strategically selected subset requires detailed physical processing, that is a major success.

---

# Continuous Simulation Refinement

The transition between abstraction levels is one of the central engine problems.

An entity should be able to become more detailed when:

- a player interacts with it;
- a projectile approaches;
- a collision becomes important;
- a formation enters a contested area;
- it becomes visually important;
- unusual behavior occurs;
- tactical decisions require local information;
- the camera focuses on it.

Likewise, detailed state should be able to collapse back into cheaper representations when appropriate.

The transition should preserve continuity.

A group represented abstractly as fifty soldiers moving downhill should refine into approximately fifty soldiers already occupying plausible positions, possessing plausible velocities, carrying appropriate history, and pursuing compatible intentions.

When those soldiers later leave relevance, their detailed activity should aggregate back into an appropriate group state.

Make refinement a first-class engine concept rather than an isolated optimization.

---

# Physical Characters

Characters should provide the interaction feel associated with toy-like physics brawlers.

Support combinations of:

- animated locomotion;
- articulated bodies;
- ragdolls;
- active ragdolls;
- impacts;
- knockdowns;
- grabbing;
- pushing;
- pulling;
- carrying;
- throwing;
- launching;
- stumbling;
- recovery;
- pileups;
- crushing;
- environmental forces;
- exaggerated reactions;
- procedural squash/stretch where visually useful;
- configurable body proportions and physical personality.

Physical character parameters should be exposed as authoring tools.

A developer should be able to make:

- a tiny springy gummy scout;
- a massive heavy gummy knight;
- an unstable explosive gummy;
- a sticky gummy;
- a nearly indestructible champion;
- a floppy wizard;
- a cannonball-shaped infantry unit;

primarily through assets and parameters rather than engine modifications.

---

# Large-Scale Crowd Physics

Crowds should have meaningful physical properties at both microscopic and macroscopic scales.

Dense crowds produce phenomena such as:

- compression;
- directional flow;
- congestion;
- pileups;
- spreading;
- bottlenecks;
- waves of force;
- formation breakup;
- cascading knockdowns;
- pressure against structures;
- sudden release through an opening.

Investigate representations in which the **crowd itself has useful aggregate dynamics**.

The simulation should be able to reason about a formation as a physical object-like phenomenon while still rendering or instantiating individual characters.

This macro/micro relationship is a major opportunity for abstraction.

---

# Rigid-Body Destruction

Structures and terrain props should support destruction through composable rigid elements.

Examples include:

- castle walls;
- towers;
- gates;
- bridges;
- houses;
- trees;
- barricades;
- carts;
- siege engines;
- platforms;
- vehicles;
- environmental machinery.

Support structural relationships between components so forces can cause meaningful failure.

A structure should be able to transition from:

```text
stable structure
→ stressed structure
→ partial structural failure
→ detached physical components
→ debris
→ simplified settled debris
```

This transition is another candidate for simulation-level abstraction.

Terrain systems should support authored static geometry combined with embedded physical/destructible objects and components.

---

# Machines and Weapons

Create a general framework for physical machines.

Initial examples should include siege equipment such as:

- catapults;
- trebuchets;
- cannons;
- ballistae;
- gates;
- lifts;
- traps.

The framework should naturally extend to:

- vehicles;
- ships;
- trains;
- cranes;
- moving fortresses;
- mechanical monsters;
- absurd player-created contraptions.

Machines should be constructed from understandable components and expose their meaningful controls to both AI and human interfaces.

---

# Human Players

Human players primarily operate through phones or other browser-capable personal devices.

The central computer is the authoritative simulation host.

A player device is a **private tactical surface**.

The player's complete decision interface should be capable of living on that device.

Examples of player interaction include:

- selecting tactical objectives;
- ordering formations;
- choosing reinforcements;
- controlling siege machinery;
- aiming artillery;
- selecting ammunition;
- triggering abilities;
- managing resources;
- inspecting battlefield information;
- viewing maps;
- selecting units;
- changing priorities;
- commanding champions;
- temporarily possessing particular entities or machines.

Different roles or game modes may expose completely different phone interfaces.

A siege operator might receive a physical-looking catapult interface.

A commander might receive a tactical map.

A champion player might receive direct controls.

A defender might receive castle systems.

A game mode can mix these roles.

---

# Display Independence

Treat the simulation host and the shared display as separate concepts.

A television or monitor may present a cinematic shared view of the battle, but gameplay should not fundamentally depend upon players reading HUD information from that display.

The central display should therefore be capable of functioning largely as a window into the world.

Support:

- strategic overview;
- cinematic battle cameras;
- automatic camera direction;
- following important events;
- player-selected viewpoints;
- spectator mode.

It should also be possible for players to operate the game using their phones while the host runs without a conventional central gameplay display.

This makes the architecture useful for parties, installations, LAN environments, streamed simulations, and unusual physical setups.

---

# Networking Model

The authoritative host owns:

- canonical world state;
- physics;
- AI;
- simulation refinement;
- game rules;
- player identity;
- command validation.

Personal devices communicate intentions and receive relevant private state.

Think in terms of:

```text
PLAYER INTENTION
      ↓
authoritative host
      ↓
simulation
      ↓
player-specific observations
```

rather than distributing authoritative physical simulation among player devices.

Joining should be extremely easy.

A target experience is:

```text
launch game
→ session appears
→ scan QR / open local URL
→ choose identity/team/role
→ play
```

---

# Game Rules as Data

Game modes should be inexpensive to create.

Provide reusable concepts for:

- factions;
- teams;
- alliances;
- objectives;
- spawn rules;
- victory conditions;
- scoring;
- resources;
- territory;
- reinforcements;
- role assignment;
- round structure;
- scripted events;
- environmental events.

A content creator should be able to combine these without restructuring the engine.

---

# Authoring Experience

The eventual handoff recipient should experience the system as an unusually powerful Unity toy box.

Prioritize excellent editor tooling and comprehensible assets.

Expose things such as:

```text
Gummy Unit
Formation
Faction
Weapon
Projectile
Machine
Structure
Objective
Spawn Rule
AI Personality
Umwelt Configuration
Physical Personality
Game Mode
Map
Player Role
Phone Interface
```

as reusable authoring concepts.

Provide presets and examples.

Changes should produce immediate visible consequences whenever practical.

The recipient should be able to ask:

> What happens if there are 400 tiny bears attacking 20 enormous bears on a train?

and spend most of their time configuring and playing with that question.

---

# Observability

Because abstraction is central to this engine, create unusually good simulation inspection tools.

Developers should be able to inspect:

- current abstraction level;
- physical activity;
- simulation cost;
- aggregate representations;
- refinement/collapse events;
- entity ancestry;
- faction state;
- formation state;
- individual state;
- Umwelt beliefs;
- observations;
- intentions;
- decision history;
- active objectives;
- relationships;
- crowd fields;
- structural state.

Provide overlays and debugging visualizations.

Make invisible systems legible.

---

# Performance Philosophy

The goal is not simply to make individual agents inexpensive.

The goal is for computational effort to follow **meaning**.

A quiet army far from interaction should cost very little.

A catapult projectile about to hit twenty soldiers should cause computation to concentrate around that event.

A breach containing several interacting formations should receive significantly more simulation attention.

Settled debris should become cheap.

An off-screen formation marching uneventfully should become cheap.

A champion being directly controlled by a human should become expensive.

The simulation should behave as though it has an attention budget.

Investigate whether Umwelt can help represent that changing boundary of relevance.

---

# Scale Target

Build the architecture around battles containing **hundreds of visibly represented characters and potentially thousands of logical entities**.

The exact achievable number will depend on the representations discovered during development.

Do not encode an architectural assumption that every logical entity must continuously exist as a fully simulated Unity GameObject with full-rate physics and AI.

Scale should emerge from representation choice.

---

# Reference Scenario

Create a polished reference scenario that exercises the engine:

## Gummy Castle Siege

Two autonomous gummy factions contest a destructible fortress.

The battlefield contains:

- a castle assembled from physical structural components;
- gates and walls that can fail;
- attacking formations;
- defending formations;
- siege engines;
- projectiles;
- debris;
- environmental objects;
- multiple tactical routes;
- player-controllable systems;
- autonomous tactical adaptation.

Several humans join from phones.

Possible player roles include:

- attacker commander;
- defender commander;
- artillery operator;
- reinforcement controller;
- champion controller.

The armies should continue functioning without human commands.

Players perturb, guide and exploit the simulation rather than manually puppeteering every soldier.

The battle should generate moments worth watching even when nobody is touching a control.

---

# Additional Demonstration Scenario

After the castle scenario demonstrates the system, create at least one radically different scenario using the same engine primitives.

A strong example is a **battle on a moving train**.

The purpose is to prove that the architecture produced a general warfare sandbox rather than a castle-specific implementation.

Reuse existing systems rather than building a second bespoke game.

---

# Friend-Facing Handoff

The final project should include a curated starting point for the recipient.

They should be able to open Unity and quickly discover:

- playable example scenes;
- editable units;
- editable physical parameters;
- factions;
- weapons;
- machines;
- objectives;
- maps;
- game modes;
- AI behavior settings;
- phone interfaces;
- instructions for creating new content.

Give them obvious places to experiment.

Seed the project with playful content ideas and unfinished extension points that invite modification.

The desired reaction is:

> “I wonder what happens if I change this.”

followed shortly by:

> “Oh wow.”

---

# Research Discipline

Treat the abstraction system as an empirical research project embedded inside a videogame project.

Maintain comparable benchmarks.

Measure:

- CPU cost;
- physics cost;
- AI cost;
- memory;
- entity count;
- frame time;
- refinement cost;
- behavioral continuity;
- physical continuity;
- visible artifacts;
- scalability.

Compare promising representations rather than assuming the most sophisticated representation is automatically superior.

Use the Umwelt repository as source material, but adapt its ideas to the needs of a realtime game rather than preserving implementation details for their own sake.

When an Umwelt-derived technique wins, integrate it.

When a simpler representation wins, retain the simpler representation.

Preserve the experimental results so later work can build on what was learned.

---

# Architectural Principle

The common thread across the engine should be:

> **Represent a thing according to what currently matters about it.**

A faction does not need to be simulated as thousands of decisions when a handful of beliefs describe its strategic state.

A formation does not need hundreds of independent pathfinding solutions when aggregate flow describes most of its motion.

An individual does not need expensive cognition when its behavior follows obviously from its formation.

A physical body does not need full attention while nothing interesting is happening to it.

A destroyed wall does not need complex structural reasoning after it has become a settled pile.

But all of these representations should be capable of becoming richer when events demand it.

The world should continuously move between:

**abstraction and embodiment.**

That is the central engine experiment.

---

# Definition of Success

The project succeeds when all of the following are true:

A developer can create a new battle primarily by composing and tuning existing engine systems.

Large numbers of gummy characters produce entertaining physical interactions.

Autonomous armies pursue goals coherently at faction, formation and individual scales.

Umwelt-derived hierarchical state meaningfully participates in that autonomy.

The simulation changes representation according to relevance and demonstrates measurable computational savings.

Characters and formations can move between cheap and detailed representations without destroying the perceived continuity of the battle.

Rigid structures can meaningfully break apart and become part of the physical battlefield.

Players can join easily from phones and interact entirely through their personal tactical surfaces.

The authoritative host can run with or without a shared central gameplay display.

The same architecture supports substantially different settings and game modes.

A new Unity developer can modify content without understanding the internals of the simulation engine.

And, most importantly:

**putting a ridiculous number of gummy creatures into a ridiculous physical situation reliably produces something funny, surprising, understandable, and worth playing with.**