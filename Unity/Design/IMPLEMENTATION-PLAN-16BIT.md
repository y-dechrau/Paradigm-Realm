# Paradigm Realm — implementation plan for the 16-bit core design (planned)

Status: **PLAN ONLY. No stage below is implemented.** Date: 28 October 2026. Companion to CORE-DESIGN-16BIT.md. Findings about the current code come from reading it. On 28 October 2026 the owner reported that the project opens in Unity with no compile errors and the Earth scene works; performance and large-map behaviour are untested.

Rule for every stage: bump the save version with a migration, update the matching harness in Unity/Tests, update IMPLEMENTATION-STATUS.md, commit and push, and say plainly what was and was not verified.

## Decisions

- The draft balance numbers in CORE-DESIGN-16BIT.md are accepted as the starting point, to be tuned in play.
- Map size must range from small to massively expansive, chosen when starting a new game.
- Earth first; realms and portals later.

## Map size options (proposed)

| Size | Grid (W x H) | Tiles | Civilisations |
|---|---|---|---|
| Tiny | 32 x 20 | 640 | 2 |
| Small (current) | 48 x 30 | 1,440 | 3 |
| Medium | 80 x 50 | 4,000 | 4 |
| Large | 120 x 75 | 9,000 | 6 |
| Huge | 180 x 110 | 19,800 | 10 |
| Massive | 320 x 200 | 64,000 | 16 |

Small stays the default so existing saves and behaviour are unchanged. A Custom option (width and height within set limits) can come after the presets work. Civilisation count should follow map size but be adjustable.

## What the current code does that blocks large maps

1. **Fixed map size.** `World.Width=48, Height=30` are compile-time constants used in about 35 places across 13 files. Saves are only valid if the tile count equals 48 x 30.
2. **Fixed three civilisations.** `nations` is created with exactly 3 entries, loops use `owner<3`, and diplomacy assumes the three pairs.
3. **No drawing cull.** The map renderer loops over every tile every frame, drawing with IMGUI.
4. **Terrain texture cache and snapshot.** The per-tile art cache is already capped at 2,048 entries (about 32 MB), so memory is bounded. The real costs were rebuilding the whole-map terrain snapshot every frame and drawing every tile. (An earlier version of this plan overstated the memory risk.)
5. **Fog is recomputed for every tile.** `Reveal()` loops over all tiles for each of 3 owners. This will not scale to Massive.
6. **AI and turn processing** scan whole maps and all cities; budgets are unmeasured.

## Stages

### Stage 0 — Get it compiling and running in Unity (done, owner-reported 28 Oct 2026)
The owner opened the Unity folder in Unity 6000.3.25f1: no compile errors, Earth scene working. This is the baseline for later stages.

### Stage 1 — Variable map size foundation (save v8) — source written 28 Oct 2026, awaiting Unity testing
Done in source: variable size and v8 migration, scaled starts and terrain features, visible-tile drawing, incremental fog, windowed terrain snapshot, scalable Known World screen, New Game size picker, delete saved games, autosave throttle on large maps. Not done: a separate minimap overlay, per-size camera limits beyond the existing scroll clamp, faster pathfinding, and a measured performance budget.
- Make width and height saved fields of `World` instead of constants. Old saves load as 48 x 30.
- Parameterise terrain generation and starting positions; keep land connected and starts fair at every size.
- Size `nations[].known` and all per-tile arrays from the saved size.
- New Game screen: map size picker.
- Camera limits, zoom range and a minimap that scale with map size.
- Draw only visible tiles; replace the per-tile texture cache with a small cache of shared terrain textures or an atlas with deterministic variation.
- Make fog update incrementally, only around units and cities that moved or were created.
- Harness: size round trip at every preset, v7 to v8 migration, determinism by seed, performance smoke test on Massive.

### Stage 2 — Variable civilisation count (save v10) — source written 29 Oct 2026, awaiting Unity testing
Dynamic `nations`, diplomacy pairs for every pair of civilisations, AI and contact rules at 2 to 16, UI lists that scroll. Start positions spread by map size.

### Stage 3 — Rules alignment: economy and cities (save v12) — complete in source 29 Oct 2026, awaiting Unity testing
Done in source: growth formula, buying, production costs and building tiers, Settler population cost, unit and building upkeep, Trade yield with the science-rate split (save v11, with a bridging +2 Trade per citizen until Stage 5), content/unhappy citizens with disorder, healing rates, and Veteran/Elite ranks from victories (save v12). Open tuning item: gold builds up about three times faster than before.
Growth formula, production costs, buying, tax and science split, upkeep, happiness model, experience. Replace the existing formulas one at a time with a harness check each. Map the 21 existing terrain types onto the simple Food/Shield/Trade table through base yield plus modifiers so artwork and names stay.

### Stage 4 — Combat rules — complete in source 29 Oct 2026, awaiting Unity testing (no AI wars occurred in test runs, so balance is unplayed)
Deterministic damage formula with exact preview, stack limit of 3, best-defender rule, ranged without retaliation, defence bonuses. Rescale the current Power table (currently 0-13) to the new strengths.

### Stage 5 — Technology compression (save v13) — complete in source 29 Oct 2026 (36 keystones, eras, edicts; later eras still run ahead of the pacing targets)
Map the 124 technologies to about 36 keystone technologies in 6 eras. Migrate known technologies and in-progress research. Add era advancement and the one-of-three edict choice.

### Stage 6 — Victory (save v14) — complete in source 29 Oct 2026 (rivals still win around turn 145; pacing is Stage 8)
Conquest by capitals, Transcendence in three stages, Concord. One status screen with three bars.

### Stage 7 — Interface (save v15) — complete in source 29 Oct 2026 (three-panel city screen, Governor, tile locks, clearer New Game screen; two production lines kept)
Three-panel city screen with Governor presets, the victory status screen, a clearer New Game screen.

### Stage 8 — AI and balance
Headless multi-turn runs at each map size to check pacing targets and turn time; tune numbers.

## Risks

- Stage 1 touches the most code and is where Unity-only problems (IMGUI speed, memory, layout) are most likely. It needs real Unity feedback.
- Changing saves four times needs careful migration tests; keep a backup of the owner's saves.
- Massive maps with 16 civilisations may need simpler AI behaviour per turn. Budget to be set after measuring in Unity.
