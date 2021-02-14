# Diplomacy: To-Do


## Higher Priority


#### Compatibility with BL e1.5.8

- ~~GUI~~

- ~~Compiles~~

  + ~~Wrapper for `HintViewModel` constructor~~

- Harmony patch compatibility

  + Figure out the best way to restore the patch for when a settlement is captured by the player

  + Full review of all patched methods' differences for semantic correctness

- Go over FIXME tags related to compatch and verify the associated behavior is WAD


## Normal Priority




- Create a special `CampaignLog` logger which outputs a plaintext log of campaign events/actions/outcomes as well as some light statistics.

  + Since the goal is to be as readable as possible, all entries will be timestamped with campaign time, and we don't want to impose performance overhead from it (i.e., avoiding "autoflush" behavior), this will mostly likely use a custom asynchronous file I/O backend for logging instead of the Serilog from ButterLib used in the rest of the code.

  + However, since sometimes we'll want to log something to both the Serilog provider and to the `CampaignLog`, it should provide an extension for logging to both and handling the different formatting appropriately.


- Evaluate whether to apply our `StanceType` fix for `FactionManager.DeclareAlliance` globally

  + Currently we just workaround the faulty method with reflection rather than patching `FactionManager`

  + Is it used anywhere in the game's code that could result in unwanted behavior if the `StanceType == Allied` rather than `StanceType == Neutral`?


- Fix improper negative scoring of common allies (current code lumps them in with neutrals allied to the other kingdom) for scoring models related to forming alliances and NAPs


#### Deactivate 'Usurp Throne' feature

- Deactivation itself

- Rearrangement of UI buttons to again look decent without it


## Lower Priority


- Convert usage of `ChangeRelationAction.ApplyPlayerRelation` to `ChangeRelationAction.ApplyRelationChangeBetweenHeroes` or a more advanced action which properly affects relatives where necessary.

  + Even though these are only found for currently player-only interactions, we should never assume that anything is player-only, and we should prepare for an eventual overhaul of the relations system.

- Add logging throughout the codebase for user diagnostics and our own debugging.

- Address the hundreds of nullability warnings inherited from DiplomacyFixes.

- Access modifiers from DiplomacyFixes are over-accessible and inconsistent. Standardize these, and export as few types as possible from the assembly.

- Convert complete overrides of vanilla GUI Prefabs to UIExtenderEx patches for better compatibility and more robustness to vanilla changes.

- Wrap MCM settings in a larger config class


## Done


- Convert all Harmony patches from annotated/attribute form to declarative form (actually using `Harmony.Patch` for each patch, except with very convenient instrumentation)

  + Why? To make it safe to build Diplomacy with full compiler and JIT optimizations enabled as well as potentially detect patching conflicts on the fly, providing at the very least better diagnostics


## Major Fixes


- War/peace kingdom decisions will now correctly no longer be considered if Diplomacy's war/peace conditions do not permit the proposal