# Gummy Warfare Engine — Grok Build Execution Directive

## Purpose of This Document

This document is an execution companion to:

**Gummy Warfare Engine — Build Objective**

Read the Build Objective first.

Treat that document as the authoritative description of **what the finished system should become**.

Treat this document as the authoritative description of **how to conduct the development program**.

You are responsible for architecture, implementation strategy, experimentation, testing, benchmarking, integration, documentation, and delivery.

Do not wait for the user to decompose the project into coding tasks.

Do that yourself.

---

# 1. Primary Directive

Build the Gummy Warfare Engine described in the Build Objective into a functioning Unity project on this Windows development machine.

The finished system should be a reusable Unity warfare sandbox capable of:

1. large populations of logical agents;
2. hundreds of visible physical characters where hardware permits;
3. hierarchical simulation abstraction;
4. hierarchical Umwelt-based autonomous behavior;
5. dynamic allocation of computational detail according to relevance;
6. entertaining ragdoll-heavy physical interaction;
7. rigid-component structural destruction;
8. reusable physical machines and weapons;
9. browser/phone tactical interfaces;
10. authoritative host simulation;
11. game modes assembled from reusable systems;
12. cinematic or strategic visualization independent of player UI;
13. extensive debugging and simulation-observability tools;
14. straightforward Unity authoring for a comparatively inexperienced game developer.

The primary research hypothesis is:

> A large physical battle can be made computationally inexpensive by representing entities at the lowest-dimensional level sufficient for their current relevance, while Umwelt-derived hierarchical state models preserve enough intent, history, uncertainty, and aggregate information to reconstruct richer behavior when detail becomes necessary.

Test this hypothesis through implementation.

---

# 2. Operating Principle

Do not interpret the project as a request to implement every entity at maximum fidelity and optimize afterward.

Build **adaptive representation** into the architecture.

The fundamental computational pattern is:

```text
cheap abstract representation
        ↕
richer aggregate representation
        ↕
individual logical representation
        ↕
active physical representation
```

Entities move between representations according to relevance.

Relevance may be determined by combinations of:

- interaction;
- visibility;
- player attention;
- tactical importance;
- collision probability;
- proximity to active physics;
- unusual events;
- combat intensity;
- uncertainty;
- requested observation fidelity;
- available computational budget.

The engine should spend computation where events require it.

---

# 3. Authority to Make Engineering Decisions

You are expected to make technical decisions independently.

You may:

- choose Unity packages;
- choose networking libraries;
- choose serialization approaches;
- choose data structures;
- write custom Unity tooling;
- implement Burst/Jobs/ECS/DOTS where advantageous;
- use conventional GameObjects where advantageous;
- write custom simulation systems;
- use compute shaders where justified;
- create browser applications;
- create local servers;
- benchmark alternative architectures;
- discard failed prototypes;
- refactor aggressively;
- create internal tools;
- inspect and adapt code from the user's repositories where appropriate;
- introduce third-party open-source dependencies when their licenses and maintenance characteristics are appropriate.

Prefer measured engineering decisions over ideological commitment to a particular Unity architecture.

---

# 4. Repository Investigation Is Mandatory

Before committing to the primary architecture:

1. inspect the available Unity environment;
2. inspect the local `AQuantumArchitect/umwelt` repository;
3. locate its theory, state representation, belief-update, graph, observation/action, hierarchy, memory/history, and experimental machinery;
4. identify reusable concepts and reusable implementation;
5. inspect other local `AQuantumArchitect` repositories when they contain directly relevant work;
6. record what was discovered;
7. state which concepts will be incorporated into the Unity architecture;
8. distinguish direct reuse, conceptual adaptation, and new implementation.

Do not create a nominal class called `UmweltAgent` and consider this requirement satisfied.

Understand the repository first.

---

# 5. Maintain a Development Ledger

Create and continuously maintain project-local engineering documents.

At minimum maintain:

```text
PROJECT_STATE.md
ARCHITECTURE.md
RESEARCH_LEDGER.md
BENCHMARKS.md
HANDOFF.md
```

### PROJECT_STATE.md

Current authoritative state of development.

Include:

- currently working systems;
- current milestone;
- immediate objectives;
- known failures;
- blockers;
- major decisions;
- next actions.

Keep it concise enough to reread frequently.

### ARCHITECTURE.md

Describe the architecture that actually exists.

Update it when implementation changes.

Do not allow it to become an aspirational document disconnected from the code.

### RESEARCH_LEDGER.md

Record significant experiments.

For each experiment record:

```text
QUESTION
HYPOTHESIS
IMPLEMENTATION
COMPARISON
RESULT
DECISION
FOLLOW-UP
```

Negative results are useful results.

### BENCHMARKS.md

Maintain reproducible performance measurements.

### HANDOFF.md

Develop this progressively rather than writing it only at the end.

It should eventually allow the recipient to begin making game content without understanding the internal engine.

---

# 6. Work in Executable Milestones

Plan the entire project before major implementation.

Then execute it in milestones.

Each milestone must end in a **working, inspectable state**.

A milestone is complete when:

1. its functionality runs;
2. its important behavior can be observed;
3. relevant automated tests pass;
4. relevant performance measurements have been recorded;
5. regressions against previous milestones have been checked;
6. project documentation reflects reality.

Do not spend long periods building disconnected infrastructure without producing executable evidence that the architecture works.

---

# 7. Recommended Development Program

You may modify the internal ordering when technical dependencies require it, but preserve the capabilities and validation gates.

## Milestone 0 — Environment and Repository Reconnaissance

Establish:

- Unity version;
- render pipeline;
- project structure;
- available Windows tooling;
- source-control state;
- build pipeline;
- test framework;
- profiling workflow;
- relevant local repositories;
- Umwelt architecture.

Produce the initial architecture proposal.

Identify major technical risks.

Establish baseline performance measurement.

Then begin implementation.

---

## Milestone 1 — Physical Toy

Create the first convincing gummy battlefield.

Deliver:

- physical gummy character;
- animated movement;
- physical reactions;
- knockdown;
- ragdoll;
- recovery;
- impulses;
- throwing/launching;
- collisions between characters;
- collisions with environment;
- configurable physical personality;
- basic weapon/projectile interaction;
- simple rigid-component structure.

The goal is an immediately entertaining physical toy.

Establish a visual and physical quality bar that subsequent abstraction systems must preserve.

---

## Milestone 2 — Autonomous Gummy

Implement an individual autonomous character using the Umwelt-derived cognitive architecture.

The character must possess inspectable:

- observations;
- beliefs/state;
- goals;
- intent;
- actions;
- memory/history;
- relationships;
- physical state.

Demonstrate multiple agents responding differently because their internal state or history differs.

Provide a live debugging inspector.

---

## Milestone 3 — Hierarchical Cognition

Implement:

```text
Faction
→ Army / strategic grouping
→ Formation
→ Individual
```

Demonstrate information propagating downward and upward.

Example test:

A faction intends to capture an objective.

A formation inherits that objective.

Local observations reveal an obstruction.

The formation changes its tactical intent.

Individuals receive appropriate local intentions.

Individual observations alter their immediate behavior.

Relevant aggregate observations return upward.

This behavior must be visible in debugging tools.

---

## Milestone 4 — Large Logical Population

Separate **logical existence** from **physical embodiment**.

Create a battle containing substantially more logical agents than fully physical agents.

Logical agents must retain enough information to support later embodiment.

Measure cost per logical entity.

Demonstrate populations large enough that naïve full simulation would be undesirable.

---

## Milestone 5 — Representation Ladder

Implement multiple simulation representations.

At minimum establish useful distinctions between:

```text
aggregate formation
logical individual
lightweight visible individual
active physical individual
```

Exact implementation is your engineering decision.

Entities must move both directions through this ladder.

Create explicit tests for continuity during refinement and aggregation.

Measure:

- position error;
- population conservation;
- momentum continuity where relevant;
- state continuity;
- intent continuity;
- visible artifacts;
- transition cost.

---

## Milestone 6 — Relevance / Attention Scheduler

Create the system that allocates simulation budget.

It should decide where detailed computation is currently valuable.

Demonstrate a battlefield in which computational attention migrates as events migrate.

Example:

1. marching formation is cheap;
2. projectile approaches;
3. relevant region refines;
4. projectile impacts;
5. detailed physics produces local chaos;
6. survivors reorganize;
7. debris settles;
8. region becomes inexpensive again.

Create visualization showing the active simulation fidelity across the battlefield.

This visualization is a core development tool.

---

## Milestone 7 — Crowd Abstraction Research

Investigate aggregate representations of groups and formations.

Test useful aggregate variables such as:

- density;
- center of mass;
- velocity;
- momentum;
- occupied volume;
- cohesion;
- pressure;
- engagement;
- casualties;
- formation shape;
- tactical intent.

Determine which quantities permit convincing reconstruction of individual activity.

Investigate Umwelt-derived compact state representations where appropriate.

Benchmark alternative approaches.

Select implementations based on measured performance and reconstruction quality.

Record failures as well as successes.

---

## Milestone 8 — Structural Destruction

Build reusable rigid-component structural systems.

Deliver:

- connected structural assemblies;
- damage/stress;
- component detachment;
- collapse;
- debris;
- settled-debris simplification;
- interaction between crowds and structural failure.

Demonstrate a formation creating or exploiting a breach.

---

## Milestone 9 — Machines

Build reusable physical-machine architecture.

Implement representative examples:

- catapult or trebuchet;
- cannon or equivalent projectile weapon;
- gate or mechanical defense.

Machines must expose semantic controls that can be operated by either AI or human interfaces.

Examples:

```text
aim
draw
release
load
fire
rotate
open
close
```

Avoid coupling machine implementation directly to one input device.

---

## Milestone 10 — Player Session Infrastructure

Create local player joining.

Target flow:

```text
host starts session
→ join address / QR available
→ phone opens browser
→ player joins
→ player selects or receives role
→ tactical interface appears
→ commands affect authoritative simulation
```

The browser interface must receive player-specific state.

The simulation host remains authoritative.

Support multiple simultaneous players.

---

## Milestone 11 — Tactical Surfaces

Create a modular phone-interface system.

Demonstrate substantially different interfaces using the same networking/session architecture.

Examples:

### Commander

- tactical map;
- formation selection;
- objectives;
- reinforcement control.

### Artillery Operator

- aim;
- ammunition;
- loading state;
- firing controls.

### Champion

- appropriate direct or semi-direct control.

Player information required for decision-making should be available through the player's device.

---

## Milestone 12 — Game Mode Framework

Implement reusable data-driven:

- factions;
- alliances;
- objectives;
- victory conditions;
- scoring;
- spawning;
- reinforcements;
- resources;
- roles;
- round lifecycle;
- scripted events.

Game modes should primarily compose existing systems.

---

## Milestone 13 — Castle Siege

Construct the first integrated showcase.

It must exercise:

- hierarchical AI;
- formations;
- large logical populations;
- adaptive simulation;
- physical gummy characters;
- projectiles;
- machines;
- rigid structural destruction;
- tactical objectives;
- multiple phone players;
- autonomous battle behavior;
- simulation observability;
- cinematic/strategic viewing.

The battle should remain interesting without player input.

Human intervention should alter an already functioning battle.

---

## Milestone 14 — Generality Test

Create a substantially different scenario using the existing engine.

Preferred reference:

**battle on a moving train.**

Use existing primitives wherever possible.

Record any engine modifications required.

Treat excessive scenario-specific engine changes as evidence of an abstraction failure and refactor accordingly.

---

## Milestone 15 — Creator Experience

Turn the engineering project into a toy box.

Provide polished Unity authoring workflows for:

- gummy units;
- physical personalities;
- factions;
- formations;
- AI personalities;
- weapons;
- projectiles;
- machines;
- structures;
- objectives;
- maps;
- game modes;
- player roles;
- tactical surfaces.

Create templates and examples.

Optimize this milestone for someone learning game development through experimentation.

---

# 8. Benchmark Suite

Create reproducible benchmark scenes.

At minimum include:

### Benchmark A — Physical Population

Increase simultaneously active physical gummies until performance limits become clear.

### Benchmark B — Logical Population

Measure large populations without full physical embodiment.

### Benchmark C — Mixed Fidelity

Measure representative combinations of aggregate, logical, visible and physical entities.

### Benchmark D — Refinement Storm

Cause many entities to require increased fidelity simultaneously.

Measure transition cost and frame-time spikes.

### Benchmark E — Battle

Run a representative autonomous engagement.

### Benchmark F — Destruction

Cause large structural collapse during crowd interaction.

### Benchmark G — Networking

Run multiple simulated or real phone clients while the battle operates.

Record hardware specifications with results.

Prefer frame-time distributions over a single average FPS number.

Track major regressions.

---

# 9. Comparative Research Requirement

When developing the abstraction system, establish baseline implementations.

Do not evaluate a sophisticated representation without a simpler comparison.

For important experiments compare candidates such as:

```text
simple scalar/vector aggregate
vs.
richer statistical aggregate
vs.
Umwelt-derived representation
```

Measure both:

**computational cost**

and

**behavioral usefulness**.

A more sophisticated system earns adoption by producing useful capabilities or better compression.

---

# 10. Preserve State Across Abstraction Boundaries

When collapsing detailed entities into an aggregate representation, preserve enough information to reconstruct plausible future detail.

Investigate preserving distributions rather than only averages where necessary.

Examples:

A formation containing:

- wounded soldiers;
- frightened soldiers;
- aggressive soldiers;
- scattered soldiers;

should not necessarily collapse into one perfectly average soldier multiplied by population.

Likewise, refinement should produce structured variation rather than identical clones.

Umwelt's state machinery is a candidate mechanism for preserving this latent information compactly.

This is a central research area.

---

# 11. Simulation Invariants

Define and test invariants where appropriate.

Candidates include:

- population conservation;
- faction identity;
- ownership;
- objective state;
- aggregate location;
- aggregate momentum;
- health/casualty totals;
- important inventory;
- relationships;
- intent ancestry;
- persistent history.

Representation changes must not silently destroy strategically important information.

---

# 12. Debugging Is a Product Feature

Build debugging tools concurrently with simulation systems.

Required capabilities should eventually include:

- entity selection;
- entity ancestry;
- current simulation representation;
- representation-transition history;
- current beliefs;
- current observations;
- current intent;
- inherited intent;
- formation visualization;
- crowd density;
- relevance score;
- physics activation;
- structural connections;
- objective visualization;
- CPU/time cost by system.

Prefer visual overlays that make emergent behavior understandable at a glance.

---

# 13. Automated Validation

Create tests for deterministic or bounded behavior wherever practical.

Important systems requiring tests include:

- serialization;
- aggregate/refinement transitions;
- population conservation;
- hierarchy propagation;
- game-mode state;
- session handling;
- network command validation;
- structural state;
- authoring-data integrity.

Maintain executable integration scenes for behavior that is better evaluated visually or statistically.

---

# 14. Failure Handling

When an approach fails:

1. identify the actual failure;
2. create the smallest reproduction if useful;
3. record measurements;
4. update `RESEARCH_LEDGER.md`;
5. select the next approach;
6. continue.

Do not repeatedly patch an architecture whose underlying assumptions have been disproven.

Refactoring and replacement are expected.

---

# 15. Avoid Premature Content Production

Create enough art/content to evaluate the engine and make demonstrations understandable.

Prioritize systems that allow the eventual recipient to create content cheaply.

When choosing between:

```text
manually building ten impressive units
```

and

```text
making the unit-authoring pipeline excellent and supplying three examples
```

prefer the second when it advances the handoff objective.

---

# 16. Make Defaults Fun

Technical correctness is insufficient.

Default parameters should create enjoyable physical behavior.

A developer opening an example scene should immediately be able to:

- spawn gummies;
- launch them;
- knock them over;
- pile them up;
- break something;
- command a group;
- alter parameters;
- observe the result.

Use exaggerated physical responses when they improve readability and comedy.

---

# 17. Do Not Stall on Ambiguity

The Build Objective intentionally leaves implementation choices open.

When encountering an unspecified engineering decision:

1. infer the requirement from the product objective;
2. identify viable alternatives;
3. choose the option with the best combination of simplicity, extensibility, performance and testability;
4. record consequential decisions;
5. implement;
6. measure;
7. revise when evidence warrants it.

Request user input only when the decision substantially changes the intended product, requires inaccessible information, incurs meaningful external cost, creates irreversible external consequences, or presents multiple product directions with no defensible technical preference.

Ordinary engineering ambiguity is yours to resolve.

---

# 18. Do Not Confuse a Milestone With the Product

Early prototypes are evidence.

They are not the final scope.

For example:

```text
20 working agents
```

proves a mechanism.

It does not redefine the target as a 20-agent game.

Similarly:

```text
one working catapult
```

proves machine architecture.

It does not redefine the project as a catapult game.

Always maintain the north-star architecture while progressing through smaller executable demonstrations.

---

# 19. Periodic Architecture Review

After major milestones, ask:

1. Does the current architecture still support the Build Objective?
2. Which assumptions have now been tested?
3. Which assumptions were wrong?
4. Where is unnecessary complexity accumulating?
5. Where are abstractions leaking?
6. What currently limits entity count?
7. What currently limits creator flexibility?
8. Which system consumes disproportionate frame time?
9. Can the next scenario be composed rather than specially programmed?
10. Is Umwelt providing measurable value, and where?

Refactor when the answers justify it.

---

# 20. Development Priority Order

When priorities conflict, use this ordering:

### Priority 1 — Preserve the central architecture

Hierarchical adaptive simulation and hierarchical autonomy are the defining technical work.

### Priority 2 — Maintain executable progress

The project should repeatedly return to working demonstrations.

### Priority 3 — Measure

Performance claims and abstraction claims require evidence.

### Priority 4 — Generalize

Systems should support scenarios beyond the current demonstration.

### Priority 5 — Make the creator experience easy

The final recipient should manipulate game concepts rather than engine internals.

### Priority 6 — Polish

Polish demonstrations after the underlying systems deserve to survive.

---

# 21. Definition of Engine-Level Completion

Do not declare the project complete because a visually impressive battle exists.

Engine-level completion requires evidence that:

- hierarchical autonomous behavior works;
- logical and physical entities are separated;
- multiple simulation fidelities work;
- transitions work in both directions;
- adaptive relevance controls simulation effort;
- performance benefits are measurable;
- rigid destruction works;
- machines are reusable;
- phone players work;
- game modes are composable;
- multiple substantially different scenarios use the same architecture;
- debugging tools expose the simulation;
- Unity authoring workflows are usable;
- the handoff recipient can extend the game without modifying core engine systems.

---

# 22. Final Deliverable

Deliver a clean Unity project containing:

```text
ENGINE
    adaptive simulation
    hierarchical Umwelt cognition
    physical character systems
    crowd systems
    destruction
    machines
    networking
    game-mode framework
    observability

CREATOR TOOLKIT
    inspectors
    assets
    templates
    presets
    examples

REFERENCE CONTENT
    castle siege
    second contrasting scenario

PHONE CLIENT
    joining
    sessions
    modular tactical interfaces

RESEARCH
    benchmarks
    experiment ledger
    architectural decisions

DOCUMENTATION
    architecture
    creator handoff
    extension guide
```

The project should build and run on the target Windows machine.

---

# 23. First Action

Before writing substantial implementation code:

1. read the complete Build Objective;
2. inspect the available repository tree;
3. inspect Umwelt;
4. inspect the installed Unity environment;
5. determine the project's existing state, if any;
6. create the development ledger documents;
7. construct a milestone implementation plan based on the actual environment;
8. identify the first experiments that retire the highest architectural risks;
9. verify the plan against the Definition of Success;
10. begin execution.

Do not return a speculative implementation tutorial to the user.

Operate on the project.

The desired loop is:

```text
inspect
→ plan
→ implement
→ run
→ observe
→ measure
→ record
→ decide
→ continue
```

Continue advancing through the development program until blocked by something that genuinely requires user intervention.

---

# Governing Sentence

When uncertain about what to build next, return to this:

> **Build a reusable Unity engine in which large autonomous physical battles become computationally cheap through hierarchical abstraction, behavior remains coherent through Umwelt-derived state and intent, computational detail follows relevance, and a new game developer can create ridiculous new warfare scenarios by manipulating game concepts rather than engine internals.**