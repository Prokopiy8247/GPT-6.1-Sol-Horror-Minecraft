# GPT-6.1 Sol — Unity Minecraft Horror Expansion + Blender MCP

## Mission: extend the existing game, do not recreate it

You are the developer, game designer and technical artist responsible for a **substantial, original horror expansion for the existing Unity Minecraft-style game previously created by Claude Opus 5.5**.

Work in the currently opened project folder. This is an in-project expansion of that game, not a new game, not a replacement project, and not a Java/Fabric/Forge Minecraft mod.

**Preserve the existing playable Minecraft systems and content. Add an integrated horror layer with multiple genuinely frightening monsters, an original flagship monster, exploration, useful new items, learnable survival rules, and a complete route to defeating the flagship monster and completing the expansion.**

The user has deliberately left the story, names, lore, supporting monster designs, attack mechanics and exact victory method to you. **Design those elements yourself, then implement them.** Do not respond with only a concept, a list of suggestions or instructions for the user.

The user will record the result for a YouTube comparison across three engines. This prompt is **Unity only**. Do not modify the Godot or Unreal projects.

---

## Revision 2 — ten integrated horror-design refinements

This revision retains the original expansion brief and adds the ten agreed refinements below. They are **new design and implementation requirements for this expansion**, not claims that the base game already implements them, and not instructions to copy another mod's content.

| Refinement | Detailed requirements |
| --- | --- |
| 1. Persistent flagship stalker with bounded memory | Section 4A |
| 2. Long-term entity attention separate from short-term tension | Section 6A |
| 3. Encounter compatibility and anti-repetition | Section 6B |
| 4. Horror tied to ordinary Minecraft actions | Section 6C |
| 5. Reliable player-built refuges and defensive rules | Section 8A |
| 6. Investigation, journal and learned boss counterplay | Section 7A |
| 7. Local threat sources and visible world recovery | Section 9A |
| 8. Optional, bounded perception distortion | Section 14A |
| 9. Creature movement, collision and in-game presentation | Section 12A |
| 10. Reproducible behavior tests and a showcase route | Section 16A |

Keep the story, names, monster designs and exact boss-killing method your own. These refinements specify how the experience must function; examples are not a prescribed plot or mandatory creature roster. Do not increase the existing content counts just to accommodate them. Reuse the required creatures, items and location archetypes. Perception distortion remains optional and follows completion of the core progression.

---

# 0. Sources, instruction scope and facts you must verify

Read this entire prompt before making substantive changes.

The original `Unity_Minecraft_Prompt.md`, or its BlenderMCP-named equivalent if present, describes the intended base game. Read it as historical design context, not as a command to build the game again. Also read the project's applicable `AGENTS.md`, developer notes, `FEATURE_MATRIX.md`, and `OPUS_5.5_FINAL_REPORT.md` or equivalent previous report when available.

**A historical prompt and a completion report are not proof of implementation.** Inspect actual source files, scenes, registries, assets, saves and runtime behavior. Record what actually works before changing it. Do not invent file names, classes, APIs, entry points or implemented features.

The intended base-game systems include voxel terrain, mining/building, first-person controls, Creative and Survival, inventories, crafting, mobs, combat, world generation, saves, Nether/End portals and dimensions, existing bosses, and other systems listed in the original prompt. Preserve what is actually present and working; do not silently remove imperfect features.

This expansion brief supersedes historical game-generation instructions only for:
- the addition of horror content and its integration;
- the appearance and behavior of the **new** horror creatures;
- the new expansion's documentation and metrics.

It does not authorize rebuilding the base game, replacing its renderer, resetting its world, changing its engine, or erasing previous work. Old instructions to generate every base-game asset again are not active instructions for this task.

If a base-game dependency is missing, implement the smallest compatible addition needed for the horror expansion and document it separately. Do not spend the session attempting to finish every unmet requirement from the historical Minecraft prompt.

---

# 1. Protect the base project before implementation

## Baseline inspection

Record the start timestamp immediately, then inspect:
- the actual Unity root (`Assets`, `Packages`, `ProjectSettings`);
- `ProjectSettings/ProjectVersion.txt`, installed editor, current render pipeline and packages;
- startup scene, runtime bootstrap, player controller, input and UI flow;
- block/item/entity identifiers and registration paths;
- Creative inventory, spawn-egg implementation and Survival restrictions;
- damage, loot, recipes, containers, lighting/time, chunk/world queries and dimensions;
- save location, serialization format and existing world data;
- imported Blender assets and their source/export relationships;
- baseline compiler errors, runtime problems and known omissions.

Run a baseline smoke test when the environment permits. Separate **pre-existing problems**, **new regressions**, and **unverified behavior** in `HORROR_BASELINE.md`.

## Recovery and limited changes

Before modifying existing content:
1. Inspect Git status. Preserve all existing local work. Make an identifiable baseline snapshot/commit when safe; do not change global Git settings to force a commit.
2. Back up the relevant source assets, scenes, settings, master `.blend`, and existing save data before their first modification. Saves may be outside the repository: a Git commit alone is not their backup.
3. Use a dedicated expansion branch if appropriate. No `reset --hard`, destructive clean, force-push, or deletion of unrelated files. Do not publish anything.
4. Preserve Unity `.meta` files and existing GUID references. Prefer Unity asset operations for moves; do not recreate established assets under new identities unnecessarily.[1]
5. Do not replace the original texture atlas layout, renumber existing IDs, rename core classes indiscriminately, or break serialized Prefab/scene references.
6. Do not upgrade Unity, migrate the render pipeline, update unrelated packages, or change the existing main control scheme just for the expansion.

Add new content under a dedicated directory such as `Assets/_Game/Horror/`, adapting to the real project layout. Use a separate namespace and a stable ID prefix such as `horror:` if the registry supports it. If it uses numeric IDs, allocate stable unused IDs without renumbering the originals.

Do not overwrite the old prompt, old Opus report, old benchmark timings, or historical feature matrix. Keep expansion reports separate and link them from existing documentation only where useful.

---

# 2. Mandatory Blender connection — exact routing

Use the existing Unity Minecraft Blender session:

```text
MCP server: blender_unity
Host: localhost
Port: 9876
Master source file: UnityMinecraft.blend
```

**Use only `blender_unity`.** Do not use `blender_godot`, `blender_unreal`, any `*_gpt6sol` GTA server, or any other project's Blender instance.

Before writing to Blender, inspect the connected session and verify the currently opened file's full path belongs to this Unity Minecraft project, not merely that its basename looks right. Confirm through available MCP scene inspection or a permitted read-only Blender query. If a different file is open, do not overwrite it or switch away from unsaved work. Report the concrete connection/file blocker instead of guessing.

Create a new top-level Blender collection, for example `HORROR_EXPANSION`, alongside the existing assets. Keep the original collections, meshes, UVs, materials, armatures and animations intact. Do not clear the scene, select-all/delete, or rebuild the base asset library.

Use this live MCP connection to author the new monsters, boss, equipment and important props, save the source, and export only the intended new assets. Reusable `bpy` generation scripts executed **through the connected MCP** are allowed when its permissions permit them. Merely creating a script without executing and checking it is not asset creation.

Respect the installed MCP's permissions and Safe Mode. Adapt to permitted tools and native Blender operations; do not disable protections, modify the add-on, bypass validation, or silently switch to another unrestricted Blender process. The project's documented Safe Mode checks and local connection options are described in its README; detect the actual installed capabilities rather than assuming every tool exists.[2]

Keep external model libraries and 3D-generation services disabled. Do not obtain the boss or supporting monsters from asset packs or another game's files. Do not require paid tools or user-supplied modeling work.

---

# 3. Design authority and intended scale

You must invent and commit to a coherent horror concept, not ask the user to write it for you.

Early in the task create a compact `HORROR_DESIGN.md` describing:
- the expansion's original title and central horror premise;
- why these beings have appeared in the familiar world;
- the flagship monster's original name, design and role;
- the supporting roster and the different decisions each creature forces;
- how the player first encounters the threat;
- a dependency graph from early clues to the boss's actual death;
- new items, structures, clues and countermeasures;
- escalation, quiet periods, safety opportunities and the post-victory state;
- how to demonstrate the expansion in Creative without spoiling a Survival save.

Then implement that design. Keep this document useful and concise; writing lore is not a substitute for gameplay.

## Scope target

Aim for a substantial expansion containing:
- **one original flagship monster/boss**, with both stalking encounters and a complete final confrontation;
- **at least six additional distinct horror creature types**; recolors, size variants and extra boss phases do not count as separate types;
- **at least four distinct horror location/event-site types**, including the final encounter site;
- **at least ten useful new items or interactive blocks**, excluding spawn eggs and mere recolors;
- new recipes/loot, environmental storytelling, audio, encounter scheduling and persistent progression;
- all new items in the existing Creative catalog and a spawn egg for every new mob;
- a complete, understandable Survival route from discovery to defeating the boss.

These are content targets, not permission to create nonfunctional catalog entries. Build a complete playable path early, then broaden the roster and locations. Do not stop after a single monster and a dark sky. If a genuine blocker prevents a target, document the exact missing part rather than claiming completeness.

Choose reasonable playthrough pacing yourself. Avoid excessive grinding, hour-long empty waits, arbitrary AFK timers, and one-in-a-million prerequisite drops. The expansion should provide substantial new play while remaining practical to demonstrate on video.

---

# 4. The flagship monster: frightening, original and fully three-dimensional

## Reference interpretation

The user supplied two screenshots from another creator's video as a **mood and threat reference**, not a model to reproduce.

The relevant high-level impression is a very large, unsettling creature in a familiar block forest: a low, stalking posture, elongated anatomy, dark bodily mass, a pale bone-like facial focal point, and a threatening close-range presence.

**Do not recreate that creature one-for-one.** Do not reproduce its particular grinning skull, face layout, body silhouette, limb arrangement, spikes, texture pattern, name, signature pose or shot composition. Do not download it, identify it for asset extraction, or place the screenshot on a mesh.

Invent your own creature with an unmistakably different head/face design, anatomy, silhouette, gait and signature behavior. Changing only color, size or a few accessories is not enough. Explain the original design decisions briefly in `HORROR_DESIGN.md`; do not present this explanation as a legal guarantee.

The screenshots may need to be attached separately to the coding session or placed in a folder such as `References/Horror/`. If they are not accessible, say so and use the written mood description above. Do not pretend to have inspected missing images. Do not block the project on obtaining them.

## Preserve the game; allow the new monsters to be disturbing

Keep the existing Minecraft world, blocks, base mobs and UI recognizable. Do not reskin the entire game.

For **new horror creatures only**, you may depart from the old prompt's requirement that every mob use vanilla cuboid anatomy. The user's monster references call for unsettling authored creatures, not simply a black zombie, a bigger Creeper or a recolored Enderman. Choose geometry and surface detail that make the new beings genuinely threatening while integrating them into the existing Unity renderer and world scale.

No renderer migration or photorealistic replacement of the base world is required. The deliberate contrast between the familiar world and unfamiliar creatures can carry the horror.

## Required boss presentation

Author a substantial Blender model with:
- a distinctive silhouette readable through trees and at distance;
- unsettling close-up head/face geometry, not a single flat face texture;
- articulated body sections and a movement rig suited to the anatomy you invent;
- a coherent relationship between body, reach, collision and attack hit regions;
- idle/stalk, search, pursuit, wind-up, strike, stagger, retreat and death behaviors;
- at least one striking change of posture or movement state;
- audio motifs and environmental cues that make it recognizable before it appears.

Inspect it at both player distance and close range. Revise proportions, movement and materials if it looks like a collection of default primitives or becomes unintentionally comedic. Do not hide a poor model behind total darkness.

Make the boss exceptional through a combination of scale, anticipation, movement, intelligence, sound and mechanics—not only high damage or loud full-screen jumpscares. Threatening imagery is welcome, but graphic gore is not a substitute for design.

---

# 4A. Persistent flagship stalker with bounded memory

Make recurring encounters feel like encounters with **the same entity**, not unrelated copies of one enemy Prefab. Its persistence and apparent intelligence must come from concrete game state, not omniscient tracking.

## What the entity may remember

Maintain a bounded, serializable memory associated with this world's flagship identity. Useful evidence records include:
- the last position where it actually saw the player;
- the location and approximate character of a sound it actually detected;
- a trail or trace generated by a relevant player action;
- an entrance or shelter the player was observed using;
- a failed search, interrupted encounter or previous successful distraction.

For each record store its evidence source, location, dimension, in-game timestamp and confidence or expiry. Limit the number of records and let stale evidence lose value. Choose the precise schema to fit the existing architecture.

**Do not give the monster the player's live position, bed location, home designation or complete travel history as hidden hunting knowledge.** World data can be used for collision/navigation and protection validation, but must not become a shortcut around the creature's detection rules. Returning to a base that the monster never discovered does not automatically tell it where the player lives.

## Behavior driven by evidence

Use an inspectable state machine or equivalent behavior model for observing, investigating, stalking, chasing, searching and disengaging. After losing contact, investigate last-known evidence and plausible nearby routes; do not continue steering toward an unseen target's current coordinates.

A lure must cause a real investigation decision. Remaining quiet, breaking sight and leaving by an unobserved route must be useful. A false lead can lead to an unsuccessful search. Revisiting an observed area is allowed, but respect cooldowns, refuges and spawn protection instead of camping the player indefinitely.

This does not require machine learning or an external AI service. A finite set of well-tested rules and evidence records is sufficient. Do not secretly invalidate a learned countermeasure because the player has used it successfully.

Persist memory and campaign identity across normal saves and chunk transitions. A normal world must not accumulate duplicate flagship actors with independent memories. Manifestations, if the design uses them, must have an explicit relationship to the real entity. Creative showcase copies are isolated test actors. Unloading must not resurrect a defeated boss, forget a completed encounter, or cause an instant attack on reload.

---

# 5. Supporting monsters with genuinely different attack rules

Invent the names, silhouettes and lore yourself. Their behavior must be mechanically distinct, not the same chase script with different speed and health.

Cover **at least five different encounter rule families** across the supporting roster. Possible families include:
- a creature that tracks in-game noise and can be misdirected by a thrown sound lure;
- an ambusher using ceilings, foliage, corners or vertical spaces;
- a deceiver that imitates a familiar **in-game** sound or silhouette and reveals itself at a risky distance;
- a light-reactive hunter whose behavior changes around torches or a new protective device;
- a stalker controlled by visibility/gaze rules that are clearly taught;
- a pack encounter requiring spacing, cover or separating enemies;
- a territorial guardian with readable boundaries and a valuable resource/site;
- a ranged or area-control threat that makes remaining stationary dangerous.

These are design possibilities, not prescribed monster names. Select, combine or invent rules that fit the expansion's premise. Do not clone an existing horror video's signature creature or encounter.

For each monster, define and implement:

```text
Stable ID and display name
Original visual identity and source asset
Spawn context, limits and progression gate
Detection channels: sight / game-generated sound / light / other
Warning and attack telegraph
Attack rule, range, cooldown and damage/effect
What makes it different from the other monsters
At least one learnable countermeasure
Search / loss-of-contact / disengagement behavior
Death, loot and progression role
Creative spawn-egg ID and preview behavior
Save/persistence policy and verification result
```

Counterplay must work in code: crouching actually affects noise, a lure changes investigation, cover blocks appropriate attacks, and a ward has measurable limits. Every hostile attack needs a readable warning and a feasible response for its intended progression stage.

Do not give all enemies perfect knowledge of the player's position. Do not spawn a damaging creature inside the player or trigger an unavoidable instant kill immediately after loading, respawning or changing dimensions.

---

# 6. Horror pacing and encounter director

Create an encounter coordinator that schedules events around the existing world state. It must support tension and quiet, not nonstop random attacks.

Use available signals such as game mode, time, biome/location, light, health, equipment, progression, recent encounters, spawn protection and player activity. Reuse real project data rather than assuming a feature exists.

A useful pacing shape is:

```text
Ordinary play → subtle anomaly → evidence/sighting → pressure → encounter → recovery
```

Implement:
- separate ambient-scare and combat-event budgets;
- per-creature cooldowns and a global limit on simultaneous threats;
- an early grace period without a guaranteed fatal ambush;
- recovery after major encounters and death;
- progression-based escalation with meaningful rest windows;
- spatial spawn validation against loaded terrain, collision, player visibility and safe areas;
- behavior that respects pause, save/load, unloaded chunks and dimension changes;
- a deterministic seed or logged decisions for reproducing problematic encounters.

Ambient events may include distant silhouettes, unusual footsteps, new tracks, altered local ambience, a temporary fog pocket, an unfamiliar mark or a sound from an apparently empty location. Mix actual evidence with uncertainty, but do not make every clue meaningless.

Do not permanently make the whole world night, blanket the map in opaque fog, erase vanilla populations, or continuously disable the player's lights. Any light disruption or environmental effect must be local, bounded and cleaned up correctly.

Use actual game audio events for hearing. Do not access the user's microphone, webcam, personal files, desktop or notifications for horror effects. Do not simulate malware or claim real-world access.

---

# 6A. Separate entity attention from moment-to-moment tension

Extend the existing horror director rather than creating a second competing scheduler. Track two distinct concepts, with tunable values and clear update rules:

| State | Role | Typical inputs |
| --- | --- | --- |
| Entity attention | Long-term interest and progression-dependent eligibility for encounters | Disturbing a threat source, activating a dangerous artifact, being detected, or a meaningful progression action |
| Current tension | Short-term pressure and readiness for another encounter | Recent sightings, pursuit/combat, damage, recovery time and the current safety context |

High attention can unlock more elaborate encounters. **High current tension should suppress additional pressure and allow recovery**, not automatically summon more enemies. The two values must not collapse into one difficulty meter.

Escalation must have design reasons and traceable triggers. Ordinary building, inventory management, long world age, idle time or simply playing for a long time must not by themselves force maximum attention. Peaceful preparation remains possible. Explain discoverable attention-raising actions through the game without requiring a permanent numerical HUD.

Start the expansion's progression clock at its activation in the relevant save. A legacy world with many elapsed Minecraft days must not immediately begin at the maximum horror stage. Preserve the initial grace period and use the existing difficulty and Creative/Survival rules.

Persist long-term attention and bounded cooldown state. Pause gameplay timers when appropriate; do not accumulate offline attacks or emit a backlog after a load, bed time-skip or dimension transition. Saving/reloading should neither reset earned progression nor produce an unavoidable ambush. Disabling horror and recording boss victory must override encounter admission appropriately.

Expose the two values, recent trigger reasons, active cooldowns and encounter decisions to authorized diagnostics/Showcase controls. Select and document initial tuning, then adjust it from real playtests. Reuse available world signals; do not assume the baseline implements additional stats.

---

# 6B. Encounter compatibility and anti-repetition

A global enemy-count cap alone does not guarantee a fair encounter. Describe the response demands of each threat and validate combinations before scheduling them.

Examples of response-demand tags include maintaining gaze, looking away, moving quietly, remaining still, escaping an area, reaching shelter, keeping a light active, or using a particular defensive tool. These are design examples; derive the actual tags from your own roster.

Create a compact compatibility table or equivalent rule set. Reject or defer combinations that leave **no reachable, learnable response**, such as mutually exclusive gaze demands in the same space without cover or an escape route. Account for attack timing, terrain, available exits, usable counters and nearby base-game enemies, not just the names of the horror mobs.

Recheck compatibility as enemies approach or change states. If a fair situation becomes unsolvable, delay or cancel an appropriate horror attack, stagger its timing, or disengage a threat. Do not solve this by deleting or rewriting ordinary Minecraft mobs. Stronger combined encounters are allowed when a feasible response has been designed and tested.

Keep a bounded history of recent event families, places, reveal patterns and outcomes. Use cooldowns and weighted selection to avoid repeatedly presenting the same window apparition, sound or ambush in quick succession. Do not schedule the next scare the instant the previous cooldown expires every time.

**Vary timing, location and presentation; keep learned survival rules dependable.** Do not secretly randomize whether a previously established counter works. Preview controls can deliberately select events, but normal play must still pass the director's checks.

Log admitted/rejected events and reasons in bounded development diagnostics. Seeded decisions should be reproducible for testing; do not claim frame-perfect deterministic physics merely because the scheduler has a seed.

---

# 6C. Contextual horror inside ordinary Minecraft play

Implement multiple reusable event families triggered by real player actions and local world context. A random sound timer with no connection to the player's activity is not sufficient.

Possible contexts include mining, placing blocks, passing a familiar landmark, approaching a shelter, interacting with an existing door or light, exploring a cave, or receiving a reaction from an already implemented pet. Invent the specific events yourself. Do not add a whole missing baseline subsystem merely to reproduce one example.

For example, an event could answer recent mining sounds from behind a nearby wall, leave a creature-specific trace on a return route, or make an existing companion react toward an unseen presence. Treat these as functional inspiration, not fixed scenes to copy.

Each event definition should identify:
- the triggering game action and eligible world conditions;
- a valid local origin, visibility and collision constraints;
- its sound/visual evidence, escalation possibilities and player responses;
- whether it teaches a rule, supports progression, or is purely ambient;
- cooldown, ownership, persistence and cleanup behavior.

Connect some events into short evidence chains: an anomaly leads to a trace, a distant sighting or a reproducible observation, and eventually a meaningful encounter or clue. Not every anomaly needs combat, and not every clue should be a fake. Critical information must remain obtainable if a player misses an incidental event.

Use existing action callbacks or narrow adapters instead of scanning every world block each frame. Ground sounds and traces in plausible local positions and the creature's actual capabilities. A deception may mislead the player, but it must not falsify the implementation's test logs.

Keep environmental interference bounded and owned by the expansion. Use reversible overlays, temporary source-level effects or separate clue objects. Do not steal stored resources, burn builds, erase pets, or replace world terrain for a scare. Cleanup must not restore an old block/light snapshot over a legitimate change made by the player during the event.

---

# 7. A complete Survival progression and actual victory

**Completion means the player really defeats and kills the original flagship monster in gameplay.** A timer, debug button, ending text, or an endlessly retreating monster is not the final objective.

You decide the exact method. Build a connected progression with these functions:

1. **Discovery:** a discoverable anomaly/clue introduces the threat without replacing normal Minecraft play.
2. **Learning:** different encounters teach rules about creatures and the flagship monster.
3. **Preparation:** exploration, combat, crafting and/or interaction provide useful defensive tools and information.
4. **Access or vulnerability:** the player learns how to reach, lure, expose or weaken the real boss.
5. **Confrontation:** a playable fight tests the learned rules and equipment.
6. **Death and resolution:** the boss dies, the world records victory, a reward appears, and free play continues.

Do not require one exact solution from this prompt. Invent an original combination of preparation, mechanics and fight structure. Avoid making the entire expansion a repetitive hunt for identical tokens followed by a normal melee damage sponge.

## Progression integrity

- Every mandatory object has a known obtainable source or recipe before it is needed.
- No item required to kill the boss drops only from that boss.
- Every mandatory site can be located using in-game clues, tools or guidance—not by reading source code.
- Critical drops destroyed by lava, lost on death, or unloaded with a chunk must have a recovery or replacement path.
- A missed ambient event cannot permanently lock progression.
- Repeated site generation, reload or respawn cannot duplicate unique rewards or reset completed milestones accidentally.
- Do not require Nether/End systems absent from the actual baseline. If they work, they may enrich the story without replacing their original progression or bosses.
- Use existing crafting, mining, building, light, bows/weapons, armor, containers and travel where useful; this should feel like an expansion of Minecraft rather than a disconnected horror corridor.

## Boss stages and fairness

Give the boss at least three meaningfully different combat stages or tactical states, with at least three distinct offensive actions overall and a clear vulnerability/counterplay loop. A health threshold with only a faster movement speed is not enough.

Early stalking manifestations may be retreating, protected or non-corporeal if the fiction explains this, but teach why normal attacks are ineffective and how the final vulnerability will work. In the final encounter, the real boss must become genuinely damageable and killable.

Support a practical response to player building, digging, hiding, towering and tunneling. Special breaching or phasing must be limited, telegraphed and creature-specific. Do not make every monster pass through every wall or flatten the user's base. Budget any terrain damage through existing block APIs and respect protected areas.

If the arena is damaged, the player leaves, or the game is reloaded, recover safely without trapping the player or discarding all preparation. Document boss health/checkpoint/reset rules and implement them consistently.

After victory, persist a completed flag and reward. Major stalking/escalation should end or clearly reduce. The player remains able to build, explore, use other dimensions and fight base-game bosses. Rematches are deliberate, not an involuntary restart of the whole storyline.

---

# 7A. Investigation, evidence and preparation for the boss

Add an in-game journal or equivalent accessible investigation interface. It should develop from the player's observations, encounters, discoveries and experiments, not reveal a complete bestiary and solution at the start.

Keep **observed facts**, **unconfirmed hypotheses** and **confirmed countermeasures** distinguishable. Flavor text may be uncertain, but confirmed gameplay guidance must match implemented behavior. Do not display invented findings merely because a monster exists somewhere in an unloaded area.

For every rule essential to finishing the expansion, provide **at least two independent, reachable ways to learn it**. Examples include a survivable encounter plus a discoverable clue, or a repeatable experiment plus a recoverable record. Two copies of the same easily missed one-time event are not independent paths. Neither path should require reading source code or the external spoiler guide.

Tie the supporting encounters to the final fight. They should teach useful investigation, distraction, defense, revealing, binding or other mechanics selected by your design. The final confrontation must test **at least two previously learnable mechanics** rather than introduce its whole rule set in a lethal first attack. Preserve the requirement for distinct boss stages and the boss's actual death.

Preparation and acquired knowledge should matter. Avoid a progression made only of identical collectible keys. Document a rule-to-clue-to-countermeasure-to-boss mapping in `HORROR_DESIGN.md` and implement the links, not only the document.

Store discoveries independently of an easily lost physical journal. If you add a journal item, make it obtainable in Survival, immediately available in Creative, and safely replaceable without erasing knowledge. Reuse existing UI/input conventions; reading must not create cursor-lock or pause bugs. Clearly document whether the game pauses while this interface is open.

Use recoverable clues, nearby fallback opportunities and optional in-game guidance to avoid silent soft locks. Guidance should help the player form the next useful hypothesis without automatically performing the investigation or killing the boss. Keep the full spoiler walkthrough separate from the normal journal.

---

# 8. New items, recipes, blocks and equipment

Choose names and exact functions that serve your design. Ensure the content includes useful tools for several different purposes: investigation, defense, distraction, revealing/weakening threats, crafting/progression, and a meaningful victory reward.

Examples of functions—not mandatory names—are a locating instrument, a limited protective light/ward, a throwable lure, a revealing consumable, a temporary binding device, a ritual component, or equipment that changes a particular monster interaction.

For every implemented item/block:
- allocate a stable ID without disturbing base IDs;
- implement acquisition, icon, tooltip, stack/durability/consumption rules;
- implement held/placed appearance where relevant;
- connect actual use behavior, sound/VFX and cooldown;
- register recipes/loot and persistence;
- expose it in Creative immediately;
- make Survival costs and availability consistent with the progression graph.

Do not silently make normal weapons, armor, torches or tools useless. Specialized horror tools should create new decisions rather than arbitrarily invalidate the whole base game.

Store unique progression in world/player data, not solely in an easily lost physical item. Inventory objects can remain meaningful without being a single unrecoverable point of failure.

---

# 8A. Reliable player-built refuges and defensive rules

Let the player build or establish a protected place for preparation, recovery and investigation using existing Minecraft systems plus selected new horror equipment. Reuse the expansion's item/block budget; a separate safehouse game mode is not required.

Define a **protection contract** for each applicable threat. It should state what provides protection, which boundaries it covers, what can invalidate it, any resource/cooldown limits, and the warning before danger resumes. Different creatures may have different counters; do not make one unexplained universal lamp solve the entire roster.

Depending on the chosen design, protection may use real solid barriers, closed openings, light, a ward, a sound-management device or a combination. Validate the actual built geometry and device state through the existing world APIs. Re-evaluate after relevant block, door, power or device changes; do not rely solely on a baked safe-zone marker that ignores player edits.

Communicate valid protection with a dependable device state, sound, light or UI signal. Display no false assurance when protection conditions are not met. Costs, expiry and recharge rules must be learnable, with a warning window that allows a response. Ordinary preparation must not demand constant tedious upkeep.

**While the documented conditions hold, protection works.** The boss may remain outside, investigate or attempt an explicitly allowed and telegraphed approach, but cannot arbitrarily spawn inside, phase through every wall, or disable protection only because the player is doing well. Any creature-specific breach must respect the original limits on damage to existing builds and be taught before it can be fatal.

A refuge is a meaningful rest opportunity, not automatic victory. The player still needs to explore, prepare and confront the real boss to finish. Do not force a fatal event or permanent base destruction to make the player leave shelter.

Test both a correctly protected structure and one whose conditions have genuinely changed. Protection state, memory and timing must remain coherent after save/load, disabling the mod and switching Creative/Survival. Showcase demonstrations must not silently alter the user's real defenses or supplies.

---

# 9. Structures and integration with an existing voxel world

Invent at least four distinct encounter/location archetypes, with different purposes such as teaching, resource acquisition, investigation, defense or the final confrontation. Use original architecture and props rather than copying structures from the reference video.

Keep the world seed, chunk layout, original structures, dimensions and player builds intact.

For existing saves:
- do not regenerate loaded/explored terrain to insert the mod;
- prefer new unexplored chunks, validated empty spaces or non-destructive overlays;
- never overwrite containers, player buildings, portals or critical base structures;
- validate sufficient space and collision before placing a site;
- maintain a persistent structure/event ledger to prevent duplicate placement;
- provide discoverable nearby/fallback opportunities so a mature save is not blocked from starting the expansion;
- if a site becomes unusable, supply a safe alternative or recovery action without resetting the save.

For new saves, integrate the same content into normal world generation using compatible hooks. Do not create an unrelated replacement world as the only way to play the expansion.

Existing world size remains unchanged. There is no 600×600 m GTA map requirement in this task.

---

# 9A. Local threat sources and visible world recovery

After the complete core route is playable, integrate localized sources of horror into the existing required location archetypes. The sources may be rifts, marked objects, nests or another original concept; choose the explanation yourself rather than importing another mod's infection system.

Give each source a stable identity, dimension/location, limited influence area, discoverable cues, activation/suppression conditions and a saved lifecycle. Use lightweight data for distant sources and activate presentation only around relevant loaded areas. Do not create an unbounded simulation across every chunk.

Within an active source's influence, the director may alter eligible sightings, encounters, audio motifs or local atmospheric effects. Players should be able to investigate and weaken or neutralize sources through implemented actions. Clearing one must produce a measurable local reduction or change in the relevant threat, with an observable payoff, not just a changed quest label.

Keep spread, spawned objects and effects explicitly bounded. Prefer expansion-owned objects and reversible overlays. Do not automatically convert player builds, replace existing containers, spread across the whole save, or remove original villagers/animals to create an atmosphere.

Tie local successes to progression without creating circular dependencies. Neutralizing a site cannot erase the only remaining source of a required item or clue. Supply recoverable, repeatable or alternative acquisition routes where needed. Do not require clearing every randomly generated site in an effectively unbounded world.

Source suppression is preparation or intermediate progress, **not a substitute for killing the flagship boss**. The final objective remains the real boss's defeat. On victory, make the resolution visible and audible: selected sources deactivate or settle, the signature pursuit motif ceases or changes, and major escalation ends or clearly reduces according to the design. Let ordinary Minecraft play continue.

Persist the result and make subsequent rematches intentional. Remove only expansion-owned effects during recovery; never restore an obsolete world snapshot over player construction. This system does not require a new dimension or an enlarged world.

---

# 10. Creative inventory and spawn eggs — mandatory acceptance gate

Extend the **existing Creative inventory**, not merely a separate debug console.

A player in Creative must be able to obtain immediately:
- every horror item, block, ingredient, device, piece of equipment and reward;
- all progression items even before discovery or boss defeat;
- a clearly named, functional spawn egg for **every new horror mob**;
- a spawn egg for the flagship boss, regardless of normal Survival summon conditions;
- spawn access for independently spawnable minions/forms, if your design uses them.

Boss combat phases need not all be separate species, but they must be directly previewable through the optional showcase controls.

Integrate with existing search, categories/tabs, hotbar, icons, tooltips, drag/click behavior and stack rules. Add a Horror category/filter if practical while preserving the existing catalog and controls.

**No new item or spawn egg may be available only through a developer command.** A debug panel is additional convenience, not a substitute for the Creative inventory.

Spawn eggs must instantiate the actual playable entity, not a decorative model. Validate terrain, space, loaded chunks and the creature's movement needs. Explain placement failures without consuming the egg or breaking the UI. Flying/aquatic/large creatures require suitable spawn handling.

Preserve normal Creative privileges: invulnerability, flight, unlimited use and no forced Survival restrictions. Horror mobs do not damage or forcibly kill the Creative player by default.

## Non-destructive showcase controls

Provide a Creative/admin-only **Horror Showcase** panel or command extension that can:
- preview each creature's attacks against an invulnerable player or a test target;
- trigger one selected encounter without random interference;
- preview boss phases, vulnerability mechanics and death effects;
- give the preparation kit and teleport to a validated horror site;
- pause/resume the horror director;
- reset a test encounter, not the user's whole world;
- restore the previous presentation settings and stop temporary effects.

Use an explicit test flag to ensure preview summons/kills do not grant Survival completion, consume unique progression, duplicate final rewards or corrupt a live boss encounter. Preview mode must still use real creature behavior.

Switching to Survival must immediately remove Creative item/egg privileges and showcase-only protections. Respect any existing legitimate admin/cheat authorization.

---

# 11. Save compatibility, difficulty and a reversible horror layer

Do not require deleting saves to install or test the expansion.

Extend the existing save schema safely, or add versioned sidecar horror data keyed to the stable world/save identifier. Back up before migration. Preserve unknown/base fields and do not reuse historical IDs.

Persist at least:
- expansion enabled/configuration state;
- discovered clues and durable progression milestones;
- required site identities and completed objectives;
- unique creature/boss lifecycle state or explicit recovery checkpoint;
- unique rewards already granted;
- mod items and placed blocks using compatible registration;
- a safe representation of director cooldowns/state where needed.

Use atomic or recoverable writes appropriate to the project's persistence system. Missing horror data in a legacy save should initialize cleanly. Corrupt expansion data should not destroy valid base-world data.

Provide a reversible in-game **Horror Enabled** setting. Turning it off should stop new horror encounters, safely dismiss expansion threats, cancel damage/effects and restore temporary lighting/audio/camera changes. Keep earned progression and mod item definitions readable. Turning it off is not the same as uninstalling code: do not promise that an unmodified old executable can load new mod IDs.

Keep the existing default game-mode behavior. Do not silently convert Creative worlds to Survival. Use a clear setting/help entry to explain how to begin the Survival horror route. New worlds can have horror enabled with a grace period; legacy saves should not suffer an attack immediately on first load.

Respect Peaceful and difficulty settings if they exist. Do not silently override them. Avoid spawn-camping, repeated unavoidable deaths and permanent loss of the only progression source. Bed use, portals and respawn must not create unlimited encounter loops.

---

# 12. Blender-to-Unity production requirements

Use `UnityMinecraft.blend` as the existing master and add new collections only. Make a separate safe backup before substantial changes.

Organize new content, for example:

```text
HORROR_EXPANSION
  FlagshipBoss
  SupportingCreatures
  Equipment
  InteractiveProps
  Structures
  RigsAndAnimation
  PreviewSetup
```

For each major new asset:
1. Create/refine geometry through `blender_unity`.
2. Establish UVs/material slots, sensible scale, transforms and pivots.
3. Rig or separate moving parts as appropriate; provide convincing movement.
4. Inspect a viewport/render preview, then revise visible defects.
5. Save the source without modifying existing base assets.
6. Export only the new selected asset/collection, with stable names and paths.
7. Import automatically into Unity, assign compatible materials and configure Prefabs, collision, damage regions and animation.
8. Inspect the actual in-game result in day/low light and during movement.

Prefer explicit FBX exports for this Unity pipeline. Use GLB only if a suitable importer already exists and is validated. Do not assume that a Blender material node graph will reproduce automatically in Unity: bake or recreate required surface information using the established material pipeline. Keep a runtime build independent of a running Blender session.

Avoid blindly re-exporting the complete `.blend` over existing models. Do not rebuild the base atlas or all base Prefabs to add a monster. Model simplification/LOD is allowed for performance but must preserve the new creature's identity and animation.

Maintain a lightweight `HORROR_ASSET_MANIFEST.json` with stable asset IDs, Blender collection/object names, source file, exports, Unity asset paths and verification state. Repeated export/import must update the intended asset rather than produce uncontrolled duplicates.

---

# 12A. Creature movement, collision and in-game visual validation

Do not judge a new creature solely by a still render from Blender. Its rig, animation, controller, collision and attack behavior must work together in the actual Unity game.

Validate the flagship monster in representative situations:
- a slow approach between trees or other common obstacles;
- stopping, turning, searching and transitioning into pursuit;
- uneven ground, steps and a change of body posture/height;
- approach to a doorway or passage that is too small for its current body;
- an attack next to cover or a player-built barrier;
- stagger, disengagement and death.

Unusual movement should look intentional. Choose a distinctive gait, head/body timing and posture suited to the original anatomy. Do not mistake sliding feet, uncontrolled limb clipping, broken bone transforms or a huge body passing through a tiny tunnel for frightening animation. Use procedural adjustment or IK only where it fits the actual rig and installed project capabilities.

Give movement a clear authority: root motion, a physics/controller path, or a documented hybrid. Prevent the animated body from drifting away from its collider. A posture change that enables a smaller opening must update collision and movement constraints consistently; it must not be a visual-only excuse for arbitrary wall traversal.

For melee, synchronize the active damage window with the attack animation and relevant body/weapon hit region. Use suitable overlap, ray or swept-volume checks and world obstruction tests. Do not apply contact-strike damage every frame merely because the player is within a large radius. Define per-attack hit limits and cooldowns. Ranged and area attacks may use their own clearly telegraphed, accurately represented damage volumes.

Verify model readability from the real player camera at ordinary encounter distances, in daylight and intended night conditions, while moving. Check surface/rig correctness, attack reach, shadows and silhouette without hiding defects in opaque fog or extreme darkness. This requirement concerns **new horror creatures**, not a redesign of existing Minecraft content.

Keep representative in-game screenshots or short captures when available, and record the actual tests performed. If capture or graphical testing is unavailable, mark that evidence as `NOT TESTED`; a Blender preview or headless compile is not a substitute. Reuse these scenes/conditions for iteration and the tests in Section 16A.

---

# 13. Unity integration and runtime behavior

Reuse the real project's architecture. Prefer a small extension layer with narrow adapters/hooks over replacing its entity, inventory or world systems.

Possible components, to adapt rather than force on the project:

```text
HorrorDirector
HorrorProgression
HorrorRegistryExtension
HorrorCreatureController
HorrorEncounterSite
HorrorPresentationController
HorrorSaveAdapter
HorrorShowcaseController
```

- Reuse the existing player, camera, interaction raycast, damage flow, inventory, loot and crafting.
- Route horror item/egg additions through the same runtime registries and UI path as base content.
- Add fields/defaults compatibly; avoid fragile assumptions about enumeration ordering.
- Make initialization idempotent: no duplicate callbacks, recipes, inventory entries, spawns or save handlers after reload/domain reload.
- Do not duplicate the player, audio listener, EventSystem or core bootstrap.
- Keep editor automation in Editor-only code and runtime logic usable in a player build.
- New hidden states must clean up on death, disconnect/load, disabling horror, leaving a dimension and exiting Play Mode.

## Navigation on editable terrain

Monsters operate in a world the player can mine and build. Inspect existing navigation first. Use local voxel occupancy/line-of-sight queries, chunk-aware steering, or compatible navigation updates rather than assuming an immutable baked level.

Handle stairs, roofs, caves, doorways, water, falling, player-built obstacles and chunk borders. Update/replan after terrain changes. Large creatures must have a movement strategy consistent with their size; do not solve every navigation problem by teleporting through the player.

Schedule AI queries, line-of-sight checks and path replanning. Pool transient effects; cap enemies, sounds, decals and expensive animated meshes. Do not rebuild global navigation or the entire voxel world every frame.

---

# 14. Sound, atmosphere and readable fear

Create new original sounds and effects that communicate different creatures and attack rules. Reuse project-owned sound sources only when appropriate; do not import audio from the reference video or commercial games.

Prioritize spatial warning cues, distance variation, silence, occlusion where feasible, creature movement sounds and distinct attack wind-ups. Hearing-based enemies react to game events, not the user's actual room audio.

Use local fog, light, particles, scene objects and reversible post effects compatible with the current renderer. Make monster silhouettes, essential terrain, held tools, clues and UI readable at ordinary display brightness. Do not achieve horror by making the screen essentially black.

Avoid repeated full-screen face images, excessively loud audio spikes and rapid strobing. Offer controls for scare intensity, camera shake and flashing effects. Escape/pause, inventory and mouse release must remain usable; a scare must not lock the user out of control indefinitely.

Temporary possession, hallucination or perception effects are allowed only with clear boundaries, counterplay and reliable restoration. Do not silently discard inventory or rewrite the user's base to simulate fear.

---

# 14A. Optional, bounded perception distortion

Treat this as a **secondary enhancement after the complete core route works**, not a prerequisite for implementing the boss, Creative integration, refuges or progression. Include it only when it improves the chosen premise without undermining the higher-priority systems. If deferred, document it honestly as optional and not implemented.

If implemented, use a lightweight temporary influence/exposure state, triggered by specific horror interactions, to introduce restrained false sounds, uncertain distant silhouettes or other clearly bounded perception effects. It is separate from both long-term entity attention and short-term tension. Do not add a mandatory maintenance-heavy stat merely to increase system count.

Provide understandable recovery through a working refuge, an appropriate item or a suitable already implemented resting activity. The player must be able to recover without first completing an action made impossible by the impairment.

Rules:
- early deceptive effects are non-damaging; they are not unavoidable invisible attacks;
- do not take away basic movement, invent input reversals, force prolonged loss of camera control, or falsify essential inventory/health/resource facts;
- preserve readable terrain, critical clues, real attack warnings and UI;
- confirmed journal facts and protection-state indicators remain reliable;
- if a later manifestation is genuinely hostile, it must be an explicitly designed threat with normal warning, counterplay and damage rules, not an arbitrary damaging hallucination;
- any actual independently spawnable new creature still needs its own Creative egg; a purely audiovisual effect does not pretend to be a new species;
- do not create a feedback loop where a scare causes impairment, impairment causes unavoidable damage, and damage makes recovery impossible;
- provide a separate effects setting and bounded intensity; turning it off must not make completion impossible;
- restore audio, camera and visual state reliably on recovery, death, load, dimension change, mode change, mod disable and exit from Play Mode.

Keep these effects entirely inside the game. The prohibitions on microphone access, personal-file inspection, desktop tricks, deliberate crashes and simulated malware remain in force. Do not make loudness spikes or strobing the main source of fear.

---

# 15. Implementation sequence

Proceed autonomously through these milestones without asking for approval at every phase:

1. Capture start time, inspect and back up the actual baseline; verify the Unity Blender session.
2. Choose the horror premise, roster and dependency graph. Define integration hooks and a regression checklist.
3. Add the extension infrastructure, save handling, horror toggle and Creative registration early.
4. Produce an original boss visual prototype through Blender and one fully integrated supporting creature. Validate the entire Blender-to-game path.
5. Implement a playable progression slice: discovery, preparation, a real boss encounter, death, reward and post-victory persistence.
6. Broaden to the full supporting roster, locations and items. Add distinctive attacks and counterplay rather than superficial variants.
7. Refine the boss, animation, director, scares, sound, presentation and world interactions.
8. Test on copies of an existing save and a fresh save, then run the full Survival route without cheats and Creative catalog tests.
9. Fix regressions, optimize, save assets, produce a runnable project/build, and calculate final metrics.

A milestone is not permission to stop. If a subsystem is blocked, document why, keep the game working and continue useful independent work. Do not label the expansion complete merely because the code compiles or the boss Prefab exists.

## Integration priorities for the ten refinements

Keep the milestone sequence above. Extend the existing systems rather than building parallel replacements:
- introduce memory, attention/tension and encounter admission rules with the director/controller infrastructure;
- include an initial contextual clue, a working refuge and a learned countermeasure in the early complete progression slice;
- broaden those same systems across the required roster, items and locations;
- implement local threat sources after the core path works, reusing the required sites;
- validate movement and behavioral tests incrementally, not only after the art pass;
- attempt optional perception distortion only after the core route, save compatibility and Creative behavior are stable.

The content targets remain unchanged. Do not trade the existing completion requirements for an impressive scheduler with no playable path, and do not treat a deferred optional effect as a reason to halt higher-priority work.

Ask the user only for an actually unresolved permission, missing project/MCP connection, unsaved-file conflict or destructive action that cannot safely be avoided. Do not ask the user to invent the story, monster names or boss solution.

---

# 16. Verification and acceptance tests

## Baseline regression tests

Compare against observed baseline results, not historical promises:
- startup, main scene, player controls, pause and inventory;
- movement, mining, placing blocks and chunk-border behavior;
- Creative flight, unlimited items and invulnerability;
- Survival health, hunger, damage, crafting, loot and respawn;
- original mobs/items/recipes/containers and original content counts/IDs;
- existing Nether/End travel and base bosses where verified previously;
- old world seed, builds, containers, inventory and save/load behavior;
- original assets/import references and ordinary day/night presentation.

Do not mark a pre-existing absent feature as a newly introduced regression, but do not hide a real new failure as an old limitation.

## Expansion tests

Verify and record evidence for:
- every new item is searchable and obtainable from the existing Creative inventory;
- every new mob, including the boss, has a working spawn egg;
- each creature's distinct attack and at least one countermeasure work;
- Creative preview shows behavior without damaging the player or committing Survival victory;
- ordinary Survival cannot obtain Creative-only items/privileges through the new UI;
- the director obeys limits, cooldowns, pause and safe-spawn rules;
- a legacy save can begin the expansion without terrain regeneration or erased buildings;
- a fresh Survival playthrough can reach and kill the real boss without admin commands;
- clues, prerequisite items, vulnerability windows and boss phases are actually connected;
- death, leaving/re-entering, saving/reloading and lost critical items do not soft-lock completion;
- victory and reward persist without duplicate grants;
- disabling the horror layer cleans up effects while preserving ordinary play;
- no mismatched MCP writes or unwanted modifications to original Blender assets;
- reasonable performance in regular play and the busiest new encounter.

Test critical pure-data rules with EditMode tests and runtime interactions with PlayMode or practical automated/manual-input tests when available. Report `NOT TESTED` where visual/runtime verification could not be performed; do not confuse source inspection with a successful playtest.

Use the detected Unity editor and existing automation. Command-line `-batchmode`, `-projectPath`, `-executeMethod` and logs may help, but do not open a second editor against the same project while one is active.[3] Never force-kill unsaved work. A `-nographics` pass is not visual evidence.

If a Windows development build can be created with the installed modules, put it in a new expansion-specific output directory, not over the old release. Otherwise leave the Editor project directly runnable and state the concrete packaging blocker.

---

# 16A. Reproducible behavior tests and a real-mechanics showcase route

Extend the acceptance tests above with repeatable scenarios. For each, record the initial state, relevant seed/tuning, actions performed, expected result, observed result, evidence location and `PASS / FAIL / NOT TESTED`. Use test copies of saves and do not fabricate passes from source inspection alone.

| Test | Setup/action | Required result |
| --- | --- | --- |
| H01 — Lost contact and bounded memory | Let the boss detect the player, break sight, stop making noise and leave by an unseen route. | It investigates evidence/last-known position, can search unsuccessfully, and does not steer toward hidden live coordinates. |
| H02 — Deception and discovered shelter | Test a sound lure and separately return to an observed versus unobserved shelter. | The lure changes investigation. Home knowledge is supported by actual evidence, not a global home/bed lookup. |
| H03 — Attention versus tension | Trigger a documented attention-raising action, then complete a dangerous encounter. | Long-term eligibility changes, but recovery suppresses an immediate new pressure spike. Routine building alone does not force maximum threat. |
| H04 — Legacy save and time changes | Enable horror on an old world; test pause, bed time-skip, reload and a working dimension transition. | The expansion starts with its own grace period and no accumulated offline/time-skip attack backlog. |
| H05 — Incompatible encounters | Force candidate threats with contradictory response demands into the same relevant area. | The director rejects, defers or safely staggers the conflict. A permitted combination has a demonstrated reachable response. |
| H06 — Context and repetition | Perform a real triggering action, then repeat nearby actions and revisit the area. | Events use the correct context; cooldowns/history prevent identical scare spam; missed events do not erase critical clues. |
| H07 — Editable obstacles | Build/remove cover or close/open a passage while a creature searches or attacks. | Detection, navigation and damage respect the changed terrain and the creature's documented abilities. |
| H08 — Refuge contract | Enter a correctly protected refuge, then deliberately change one protection condition. | Valid protection holds. Invalid protection is communicated with the designed warning/response window, not an arbitrary inside spawn. |
| H09 — Investigation and loss recovery | Learn a critical rule by each of its two paths; lose the physical record or a required item. | Journal state survives; the alternate clue/replacement route is real and completion remains reachable. |
| H10 — Threat source | Investigate and neutralize a source, leave/reload and revisit it. | Local eligibility/presentation changes persist without duplicate sites, lost critical resources or overwritten base blocks. |
| H11 — Perception effects, if implemented | Apply exposure, use recovery, disable the effect and change scenes/modes. | Effects remain bounded, essential information stays readable, and restoration works. If omitted, record the optional omission rather than a pass. |
| H12 — Rig, movement and attack contact | Test turns, slope/step movement, posture changes and melee beside cover at representative frame rates. | Body/collision remain aligned; attack damage matches the active hit region/window; cover and hit limits work. |
| H13 — Persistent encounter identity | Save/reload during stalking and during an allowed boss checkpoint; stream relevant chunks. | No duplicate real boss, unsafe immediate attack, lost preparation, double reward or accidental resurrection. |
| H14 — Creative coverage and preview isolation | Obtain every new item/egg in the existing inventory; preview encounters/attacks and stop the showcase. | Real entities/mechanics are demonstrated without damaging the Creative player, granting Survival victory or leaking test state. |
| H15 — Complete Survival route | On a fresh or safely copied test save, use normal obtainable resources and learned counterplay from discovery through the final battle. | The real boss is killed without debug victory/kill commands; learned mechanics matter, reward and world recovery persist, and ordinary play continues. |
| H16 — Reversible expansion | Disable horror during an encounter/effect, then continue baseline play and reload. | Threats and temporary overrides stop safely, original world content remains intact, and saved mod items/progression remain readable. |

Use lightweight automated tests for state transitions, compatibility, IDs and persistence where feasible, plus real runtime/visual checks for movement, contact and presentation. Log only necessary game/test data. Failure reasons must be reproducible and useful for a fix.

## Showcase route for the YouTube recording

Extend the existing Creative/admin-only Horror Showcase rather than replacing it. Provide a short documented route covering:

```text
Ordinary baseline play
→ first contextual anomaly
→ an identifiable stalk/search encounter
→ a successful countermeasure and refuge
→ an investigation/source interaction
→ a boss phase and vulnerability demonstration
→ death/resolution preview
```

Use a copied or disposable test save with explicit preview state. Selected events may be scheduled directly and setup may grant a test kit, but **the entity AI, movement, detection, attacks, countermeasures and boss vulnerability must use the real gameplay implementation**. Do not substitute a cinematic, stationary display model or debug health removal and present it as working gameplay.

Keep normal Survival pacing separate from demonstration scheduling. A showcase route is not proof of an unassisted Survival completion test. Label staged setup, preview defeat and any skipped prerequisites honestly. Preserve the requirement that genuine completion is demonstrated independently in H15.

Allow event selection, pause/resume, inspection of memory/attention/tension and compatibility rejection reasons, and safe reset of only the test encounter. Stop random interference during an isolated preview without globally rewriting the normal difficulty settings. Leaving preview must remove test entities/effects, restore the recorded presentation settings and not overwrite live campaign data. Respect the existing rule that switching to Survival removes showcase-only privileges.

---

# 17. Deliverables and final presentation

Maintain separate expansion outputs:

- `HORROR_BASELINE.md`: actual baseline, backup/rollback references, integration points, pre-existing issues.
- `HORROR_DESIGN.md`: original concept, creature rules, progression graph and original boss design rationale.
- `HORROR_FEATURE_MATRIX.md`: each new requirement, `WORKING / PARTIAL / NOT IMPLEMENTED`, verification method and `PASS / FAIL / NOT TESTED`; include base regression results separately.
- `HORROR_PLAY_GUIDE.md`: how to begin, basic survival hints, controls and an explicitly spoiler-marked complete route to boss defeat; include a short Creative video-demo route.
- `HORROR_ASSET_MANIFEST.json`: source/export/runtime links for the new assets.
- `HORROR_FINAL_REPORT.md`: implemented content, test/build results, known limitations, changed integration points and the metrics required below.

## Documentation for the integrated refinements

Use the expansion documents above to make the additions inspectable without replacing earlier reports:
- `HORROR_DESIGN.md`: include bounded-memory evidence rules, attention/tension triggers, the encounter compatibility table, contextual event families, refuge contracts, the journal/learning map, source lifecycle and whether optional perception distortion was implemented.
- `HORROR_FEATURE_MATRIX.md`: add all ten refinements and test IDs H01–H16, distinguishing implementation status from test status. A deferred optional perception feature is not a completed test.
- `HORROR_PLAY_GUIDE.md`: explain learned refuge/counterplay rules without front-loading every spoiler, retain the separate full boss walkthrough, and include the reproducible showcase route and reset procedure.
- `HORROR_FINAL_REPORT.md`: summarize which refinements actually work, which tests were run, limitations, visible post-victory changes and any optional omissions. Include the measurements required in Section 18 for this expansion work only.

Keep names, lore, creature identities and the exact victory mechanism original. The added requirements do not authorize importing existing horror mods, copying the reference creature, increasing the world footprint, altering another engine project or rewriting baseline systems.

For later comparison across engines, keep monster concepts, item IDs, progression stages and tuning values documented separately from Unity-specific implementation. This does not authorize modifying or generating the other engine projects now.

Before the final response, save the master Blender source, validate the current project, update the feature matrix and report, and make a final commit if safe and configured. Do not overwrite the old Opus report.

The final response must state how to launch, how to begin the mod, how to find new Creative items/eggs, the new roster, a brief spoiler-marked victory explanation, key remaining limitations, and the metrics. Link the full guide rather than hiding missing functionality.

---

# 18. Mandatory development time, token usage and API-equivalent cost

Measure **only this horror-expansion work**, not the original Opus Minecraft build, old Astra/Sol GTA tasks or other engine sessions.

## 18.1 Time

At the start record an OS timestamp including timezone in a new run directory, for example:

```text
.horror-sol-6.1-runs/<unique-run-id>/start.json
```

At the end, after final implementation/testing/report preparation, capture an end timestamp and elapsed duration in the same directory. Preserve earlier run records; resumed work must append segments rather than overwrite the original start.

Report start, end, wall-clock `HH:MM:SS`, minutes, and any separately identifiable pauses. Include planning, coding, Blender operations, exports/imports, builds, testing and command waits. Do not sum parallel tool durations into wall-clock duration. Do not claim an exact active-work time unless it was measured separately.

## 18.2 Token accounting

Use actual accessible runtime/session telemetry. Identify this session by ID, project directory and timestamps. If local logs are needed, read only the relevant session's usage records; do not copy credentials, unrelated conversations or raw private logs into the repository/report.

Record the baseline cumulative usage when joining a previously used session, or sum correctly identified per-request records for this task. Include task-owned delegated work only if its usage can be identified without double-counting.

Report input, cached input, cache-write tokens if separately exposed, output, reasoning tokens if exposed, and the deduplicated total. Describe the runtime's category semantics. Cumulative snapshots must not be summed as if they were individual requests. Reasoning or cache tokens already included in other totals must not be added twice. Unknown is not zero.

Record the usage observation cutoff. If the final response or reporting work is not yet included in the observed telemetry, state this rather than claiming an exact all-inclusive final count.

If unavailable, report:

```text
Exact token usage unavailable from the accessible runtime.
```

A labeled estimate needs a defensible method. Do not estimate tokens from source-code line counts and present the result as telemetry.

## 18.3 API-equivalent price for the actual model

Calculate the **GPT-6.1 Sol token-only API-equivalent cost in USD** from the verified usage. This is a benchmark equivalent, not the user's actual subscription bill.

First verify the actual model identifier exposed by the session. Use official pricing for that model and state the comparison tier. Do not reuse Claude Opus 5.5, GPT-6 Astra or GPT-6 Sol prices just because they appear in an old prompt.

Official **Standard** pricing snapshot checked on **2026-09-30**, in USD per 1 million tokens:[4]

| GPT-6.1 Sol token category | Short-context rate | Long-context rate |
| --- | ---: | ---: |
| Uncached input | 2.00 | 4.00 |
| Cached input | 0.10 | 0.20 |
| Cache writes | 2.50 | 5.00 |
| Output | 10.00 | 15.00 |

Recheck the official pricing and applicable context/tier rules at execution time. A newer verified rate takes precedence. If live verification is unavailable, this table may be used only as an explicitly dated snapshot estimate for the exact matching model, not as a claim of current billing. Do not apply another service tier's price without identifying it.

Normalize the runtime's token counters into **mutually exclusive billable categories** before calculating:

```text
request_cost_usd = sum(category_tokens × applicable_category_rate) / 1_000_000
session_api_equivalent_usd = sum(request_cost_usd for task-owned requests)
```

When input includes cache-hit/cache-write tokens, subtract only verified disjoint subsets to obtain uncached input. When it already excludes them, do not subtract again. Do not charge an additional reasoning-token line if reasoning is already billed within output.

Apply long-context rules per request using the official threshold and category semantics, not by comparing the entire session's aggregate token count with a request threshold. If the context split cannot be reconstructed, give a clearly labeled estimate or justified range, not invented exactness. If the model or usage cannot be verified well enough to price, state that cost is unavailable.

Report actual separately metered external tool charges only if known, apart from the token estimate. Do not invent a Blender/Unity API fee for local execution. Any model tokens used to invoke/read tools remain part of the measured token usage. State excluded/unknown fees.

## 18.4 Required final metrics block

Put this at the end of `HORROR_FINAL_REPORT.md` and summarize it in the final response:

```text
## GPT-6.1 Sol Horror Expansion — Session Metrics

Project: Unity Minecraft horror expansion
Run/session identifier:
Start timestamp and timezone:
End timestamp and timezone:
Elapsed wall-clock time (HH:MM:SS):
Elapsed minutes:
Pauses / resumed segments:

Actual model identifier:
Input tokens:
Cached input tokens:
Cache-write tokens:
Output tokens:
Reasoning tokens (included/separate/unavailable):
Deduplicated total tokens:
Usage source and observation cutoff:
Work not captured by the observed usage:

Token-only API-equivalent cost (USD):
Pricing source, date and tier:
Live-verified / dated snapshot / unavailable:
Long-context treatment:
Other tool fees (known/excluded/unknown):
Measured / estimated / unavailable, with limitations:

Blender MCP: blender_unity
Blender port: 9876
Master Blender file: UnityMinecraft.blend
New creature count, excluding cosmetic variants:
New functional item/block count, excluding spawn eggs:
Creative spawn-egg coverage:
Survival completion test result:
Base-game regression test result:
```

## Reference notes

These links support technical integration and pricing only. The horror designs and content targets in this document are new requirements for this expansion, not claims about the existing game's implementation.

[1] Unity asset metadata / preserving references: https://docs.unity3d.com/6000.0/Documentation/Manual/AssetMetadata.html

[2] MCP for Blender project documentation (capabilities, ports and Safe Mode; match the installed version): https://github.com/ahujasid/mcp-for-blender

[3] Unity editor command-line reference (match the installed editor version): https://docs.unity3d.com/6000.0/Documentation/Manual/EditorCommandLineArguments.html

[4] OpenAI API pricing and model reference: https://developers.openai.com/api/docs/pricing and https://developers.openai.com/api/docs/models/gpt-6.1-sol

**Begin by capturing the timestamp, inspecting and protecting the existing Unity project, and verifying `blender_unity` → `9876` → `UnityMinecraft.blend`. Then design and build the horror expansion. Preserve the old game; deliver a new, playable horror experience on top of it.**
