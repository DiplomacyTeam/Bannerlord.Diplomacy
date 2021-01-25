# Diplomacy: To-Do


## Higher Priority


#### Compatibility with BL e1.5.7

- ~~GUI compatibility~~

- Compile-time compatibility

  + End usage of `StatExplainer`

- Harmony patch compatibility

  + Full review of all patched methods for semantic correctness


## Normal Priority


- Convert all Harmony patches from annotated/attribute form to declarative form (actually using `Harmony.Patch` for each patch, except with very convenient instrumentation)

  + Why? To make it safe to build Diplomacy with full compiler and JIT optimizations enabled as well as potentially detect patching conflicts on the fly, providing at the very least better diagnostics


- Create a special `CampaignLog` logger which outputs a plaintext log of campaign events/actions/outcomes as well as some light statistics.

  + Since the goal is to be as readable as possible, all entries will be timestamped with campaign time, and we don't want to impose performance overhead from it (i.e., avoiding "autoflush" behavior), this will mostly likely use a custom asynchronous file I/O backend for logging instead of the Serilog from ButterLib used in the rest of the code.

  + However, since sometimes we'll want to log something to both the Serilog provider and to the `CampaignLog`, it should provide an extension for logging to both and handling the different formatting appropriately.


#### Deactivate 'Usurp Throne' feature

- Deactivation itself

- Rearrangement of UI buttons to again look decent without it


## Lower Priority

- Add logging throughout the codebase for user diagnostics and our own debugging.

- Address the hundreds of nullability warnings inherited from DiplomacyFixes.

- Convert complete overrides of vanilla GUI Prefabs to UIExtenderEx patches for better compatibility and more robustness to vanilla changes.

- Wrap MCM settings in a larger config class
