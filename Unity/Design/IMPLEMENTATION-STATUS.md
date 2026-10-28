# Current release: Advanced diplomacy — Stage 3

> **Planned design, not implemented:** see [CORE-DESIGN-16BIT.md](CORE-DESIGN-16BIT.md) for the agreed 16-bit core design and draft balance formulas.

See [Diplomacy implementation](DIPLOMACY-STAGE-3.md) and [Play instructions](../START-HERE-DIPLOMACY.md).

Implemented: bilateral relationships for all three civilisation pairs; negotiated timed borders, friendship, defensive alliances, research, trade, peace and border promises; joint war; gold/resource contracts; grievances, denunciation, embargoes and war weariness; AI offers/counteroffers; inbox, acceptance previews, save v7 migration and regression checks.

Not implemented: world congress, espionage, city-states, vassalage, diplomatic victory, city transfers, advanced war objectives or naval systems. Unity playtesting remains outstanding. Existing artwork and terrain are preserved.

## Previous stage retained below

# Settlement strategy — Stage 2

This stage adds gameplay to the existing terrain/city update. Artwork and terrain
rendering are unchanged. No ships or naval gameplay are added.

## What the player can do

Select a Settler and choose **Compare settlement sites**. The adviser lists up to
five reachable, safe suggestions within 12 tiles. It explains estimated output
at three citizens, nearby known resources, early population limit, city upkeep,
travel distance and how many surrounding tiles remain unexplored. Hovering a tile
with a Settler selected also shows settlement benefits or a specific blocker.

**Travel to...** issues ordinary movement orders and returns to the map. It never
founds a player city automatically. Found the city with B or the Found city button.

## Rules and trade-offs

- River city centre: 3 food / 2 production, instead of 2 / 2.
- Hill city centre: 2 food / 3 production, instead of 2 / 2.
- A lake or river on or adjacent to the centre gives an early population limit
  of 8; other sites have 6. Ocean and reef water do not count as fresh water.
- Aqueducts set the cap to 12 and sewers to 20; water bonuses do not stack on top.
- Existing food consumption, happiness, specialists, tile ownership, resource
  connection requirements and upkeep still apply. Nearby resources are potential
  access; they do not become connected automatically when a city is founded.
- Existing cities also receive the derived site bonuses. No existing population,
  terrain, researched technology or orders are deleted, and no city cap is reduced.

Decision: choose food and population capacity, productive terrain, resource access
and defensibility while considering travel and distance upkeep. Better sites may
require longer travel, exploration and roads. A higher cap still requires food and
happiness. These are site-dependent advantages, not free policy bonuses.

## AI and information boundaries

AI settlers score known food, production, resources, fresh water, upkeep and
travel. They keep valid routes, avoid visible military threats and competing
settler destinations, and explore a local frontier when no suitable site is known.
Settler training requires population 2+, positive food, Agriculture and a treasury
reserve, and retains the current four-city target. AI uses the same founding,
movement, production and city bonus rules as the player.

Player suggestions use visible or remembered terrain and ownership, not hidden
live changes. Rival nations have no per-tile memory structures in save v6, so
this planner conservatively uses only their currently visible tiles. The planner
tries the best 24 scored candidates for reachability; it is a bounded heuristic,
not a guarantee of the globally best site. Resource and output estimates omit
future improvements, buildings, specialists and policy modifiers.

## Compatibility and tests

Save schema remains version 6; this stage adds no serialized fields. Existing
movement orders and saves round-trip through the supplementary test harness.
Foundation, Content, Empire, Strategy and new Settlement checks pass under .NET.
Coverage includes site yield/cap rules, cold founding, hidden geography/resources/
foreign cities, sustainable site ranking, stable ties, threats, treasury constraints
and an AI founding a fresh-water colony. Existing campaign simulations also pass.

API-double UI tests cover adviser opening, displayed explanations, travel without
automatic founding, no state mutation by recommendations, and saved travel orders.
Existing UI and renderer harnesses pass. Actual Unity compilation, JsonUtility,
Play Mode layout and frame-time checks remain unverified in this environment.

Next gameplay priorities: richer land-combat decisions and AI, diplomatic
consequences and objectives, and additional victory progress/counterplay.

---

The previous-stage audit and implementation notes follow for history. Current
rules above supersede the earlier description of the base 6/12/20 growth caps.

# Terrain and city strategy — stage 1

## Audit of the supplied graphics-update project

The project has working code for movement orders, fog, cities, separate unit and
building production, improvements, resource networks, research, economy,
diplomacy, trade, combat, AI, conquest/science victory and save version 6.
Foundation, Content and Empire test suites exercise those systems. The supplied
project had no AGENTS.md or Design directory; its START-HERE instructions and
artwork notes were read. Unity target: 6000.3.25f1.

The renderer used straight rectangular crossfades, sampled river tiles without
connections and repeatedly queried world visibility during texture generation.
The city allocator ranked base yields, ignored improvements unlocked by research
while choosing tiles and could select low-food production tiles during famine.
Growth blockers were poorly explained, food could accumulate without a bound at
the cap, and AI buildings were selected in catalog order. AI research selected
the cheapest available technology and workers heavily favoured roads.

The technology catalog has 124 entries but many future unlocks remain design text.
Ships, aircraft, other realms, difficulty presets, additional victory routes and
advanced diplomacy have not been implemented in this stage.

## Implemented: terrain

- Pure C# TerrainPainter samples a shared world-space field. Edge and diagonal
  contributions aggregate repeated biomes correctly, including multi-biome corners.
- Warped boundaries, limited dithering, 5-bit colour channels and intermediate dry
  colours replace straight soft rectangular crossfades.
- Shared water fields shape land banks, shallow water, lake outlines and ocean coast.
- River tiles draw connected channels with rounded bends, junctions and mouths.
  Their movement costs, yields and gameplay terrain types are unchanged.
- TerrainPalette prepares periodic samples from existing textures, keeping their
  orientation and lighting rather than mirroring them. The ocean artwork supplies
  lake interiors; coast artwork is sampled from its land portion so baked shorelines
  do not appear twice. All original PNG files remain unchanged.
- Seeded variations remain stable between frames and save/load.
- A per-draw remembered-terrain snapshot prevents unseen live terrain from
  influencing edges. Unknown neighbours use neutral grass, never their hidden type.
- Cache keys contain seed, coordinates and the complete influencing 5x5 neighbourhood.
  The 2048-entry cache fits the fixed 1440-tile map with headroom for changed edges.
  Textures and source caches are released on world changes and destruction.
- Raised scenery, resources and existing connected roads remain. Unseen terrain
  receives a final opaque mask so raised sprites cannot protrude into unexplored land.
- Terrain Transition Review is an independent Editor diagnostic window with all
  21 terrains, islands, narrow land, bends, junctions and lake/ocean mouths. It uses
  the exact new rasteriser and can export PNG without editing the current world.

## Implemented: city strategy

Decision: choose a city focus, specialist mix and infrastructure appropriate to
local food, production, science and treasury needs.

Connections: population consumes food; specialists replace tile workers;
improvements and research affect usable tile yields; happiness and infrastructure
control growth; buildings incur the existing upkeep; production remains split
between the existing two concurrent production lines.

Trade-offs: automatic workers try to cover food needs before their focus, but
specialists can still cause a shortage. Science focus retains its existing +2
science / up-to-2-production cost. Commerce values gold deposits without a free
gold bonus. Growth focus sacrifices other output for food. Higher caps require
research, production and upkeep. No new free AI yields are added.

- EffectiveTileYield is now shared by allocation, city totals and tile labels.
  Irrigation, chemistry, steam and harbour effects are counted exactly once.
- Workers swap low-food tiles for higher-food alternatives only as necessary to
  cover consumption. Deterministic ties and existing shared-city tile ownership
  are retained. The allocator is a heuristic, not a global optimiser.
- Citizens screen explains food sources/consumption, workers, specialists and
  output. City screen names growth blockers and estimates growth/starvation turns.
- Growth, starvation and newly reached growth blocks have event messages.
- Blocked storage is bounded to the next growth threshold on a turn; granaries
  retain half that threshold after growth. Existing population is not reduced by
  a growth cap. Starvation still reduces population and normalises specialists.
- AI manages its own focus and specialists using the same yield rules, prioritises
  relevant growth/happiness/economic buildings and uses prerequisite-aware research
  scoring toward usable improvements and current city needs.
- AI workers score worked tiles, hunger, resource improvements, connected roads,
  distance, other workers' jobs and visible threats, using normal job restrictions.

## Save and art compatibility

Existing v6 worlds load without regeneration. No new serialized fields are needed;
focus 4 is Commerce. All existing asset bytes/GUIDs, Packages and ProjectSettings
are retained. Original units, cities, resources, theme and gallery remain available.
Back up saves: turn processing now changes stored food and worker assignments.

## Validation

See Documentation/Validation.txt. Actual Unity compilation and Play Mode have not
been run. CPU-rendered comparison uses the supplied artwork at 56 pixels/tile;
it does not include raised sprites or the game UI and is not a Unity screenshot.

## Remaining priorities, in order

1. Unity visual/performance acceptance: play through the diagnostic cases and
   actual campaign fog, inspect overlays and zoom at the user's screen resolution.
   IMGUI scaling remains fractional on some windows; point filtering avoids blur
   but this is not an integer-only pixel-perfect camera. Dense source art can still
   show repetition; shore and relief palettes need further artistic tuning.
2. Coherent new-world geography and stronger settlement-site scoring. Existing
   generated worlds remain unchanged; landform generation is still simple.
3. Extend city roles with site-dependent building choices and full production,
   science, happiness and treasury modifier breakdowns. Current breakdown is most
   detailed for food. Housing remains the existing 6/12/20 population cap system.
4. Land combat: tactical AI, defensive fronts and clearer strategic war objectives.
5. Diplomacy: persistent grievances, treaties and readable rival intentions.
6. Additional victory paths with progress and counterplay. Keep future technology
   unlocks explicitly planned until their complete gameplay and AI support exist.

No claims of full Civilisation VI-equivalent depth or finished balancing are made.
