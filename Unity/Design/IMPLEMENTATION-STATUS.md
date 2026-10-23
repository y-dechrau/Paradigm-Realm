# Paradigm Realm Alpha 0.7 — Empire expansion

This release expands the original Earth game in the direction of classic Civilization II gameplay. It is not a complete Test of Time remake.

## Added and playable

- Five governments: Tribal rule, Monarchy, Republic, Democracy and Collectivism. Technology gates, military support and economic bonuses differ. Revolution reduces output for two processed turns.
- Scientists (+3 science), merchants (+3 gold) and entertainers (+2 happiness). Every specialist replaces a tile worker and still consumes food. Open the city and choose Citizens / specialists.
- Luxury spending: 0, 1 or 2 gold per city per turn, granting 0, 2 or 4 happiness. Controlled in Economy.
- Eight new buildings: Temple, Aqueduct, University, Bank, Factory, Sewer system, Colosseum and Harbour. Buildings have distinct effects and research requirements. Aqueduct/Sewer growth ceilings are 12/20; without them new growth stops at 6. Existing large cities are not reduced.
- Four original world wonders: Grand Archive (+3 science in each owned city), Sun Gardens (+2 happiness in each owned city), Merchant Exchange (+3 gold in each owned city), War Academy (5 experience for new units). Only one of each may finish worldwide. Losing competitors receive half their invested production as gold. Capturing the wonder city transfers its effects.
- Caravans, unlocked by Currency, with original wagon sprites. Move beside another city and select Trade. Requires a surviving home city, a different destination at least four tiles from home, and peaceful open borders for foreign delivery. Each city can host three routes. Each active endpoint earns 3 gold/turn; delivery grants a distance-based lump sum. Wars suspend income; captured endpoints invalidate a route. Roads are not required for these abstract caravan routes.
- Caravans can instead supply 40 production to a friendly active wonder and are consumed.
- Diplomatic gifts (25 gold), visible relation scores, technology purchases with prerequisite checks, and peace offers that hostile stronger rivals can refuse.
- Rush either production line for three gold per missing production. Completion occurs on the next production tick; unit stack capacity still applies.
- Exploration sites on new maps provide a one-time reward. Existing maps retain their geography and do not receive new sites.
- Scout auto-exploration of the nearby frontier; it stops when blocked or threatened. It does not plan expeditions around distant obstacles. X cancels it.
- Sentry orders wake when a visible enemy approaches. X cancels sentry duty.
- World atlas and military adviser, accessible from Cities; atlas clicks centre the map and unit entries select their unit.
- Adjacent melee combat previews in tile tooltips, including damage and effective strengths.
- AI retains a defender in cities and uses monarchy, luxury spending, caravans and the expanded construction roster when eligible.
- Restored soft terrain joins, stepped 16-bit-inspired relief shading, and removal of the dark map grid. Rendering remains 2D, with point filtering. Neighbour blending uses only known terrain.

## Preserved

Both production queues, Ctrl-click queue editing, Enter/Space/X shortcuts, worker cooperation, mouse zoom, city double-click, 124 technologies, all 21 terrain images and existing save/load controls remain.

## Saves and installation

Save format 6 migrates older saves through the existing technology conversion and adds the empire fields. Keep a backup of older saves; do not load new saves into older code.

Stop Play mode. Extract this ZIP. Copy all three folders — Assets/Scripts, Assets/Editor and Assets/Resources — into your existing Assets folder, merging and replacing files. Include .meta files: terrain textures now require Read/Write Enabled. Do not replace Packages, ProjectSettings or your scene. Run Paradigm Realm > Run Engine Checks, then Play. Start a new game for exploration sites, or load an older game for the other additions.

## Verified and outstanding

Passed outside Unity: runtime script compilation against API doubles; existing foundation and content regressions; new government, specialist, wonder, trade, diplomacy, rush, exploration and save-validation checks; three 50-turn campaigns; simulated old/new interface controls and serialization; fog-safe terrain cache checks.

These checks do not establish Unity playtesting. Unity Editor import, actual mouse/layout behaviour, JsonUtility round-trips, terrain-generation performance, Windows builds and campaign balance remain to be tested in Unity. The Editor checks command now includes the new regression suite.

Naval movement/transport, aircraft, spies, barbarian factions, spaceship construction, linked worlds, scenario scripting, expanded civilisation counts and multiplayer remain unimplemented. Terrain motifs still repeat; edge blending is not directional river/coast autotiling. Diplomacy and AI remain simplified. Wonders have no obsolescence mechanic. These are deliberate limits of this release, not finished systems.


---
Historical foundation status (superseded where listed above):

# Implementation coverage — Foundation Alpha 0.5

"Implemented" below means code exists with engine regression checks where applicable, not that the Unity user experience has been playtested. No full-game completion claim is made.

| Topic | Coverage in this release | Still planned |
|---|---|---|
| 1 Setting | Grounded Earth | Speculative-era content and final magic/physics decision |
| 2 Realms | Full design recorded | World identifiers, portal simulation, other maps |
| 3 Maps | 21 textured terrain types, seeded climate bands, rivers and lakes | Layered terrain, directional shoreline autotiling, archipelago, realistic Earth |
| 4 Navigation | Pan, pointer-centred wheel zoom, zoom buttons, selection and worker-team orders | Minimap, route preview, military group navigation |
| 5 Cities | Dedicated city screen with enlarged tile yields, icon building list, separate unit/building queues, population, food and focus | Manual locks, specialists, project-specific suspended progress |
| 6 Resources | Wheat, iron, gold, horses, spices; resource prerequisites | Material inventories, fuel, wider resource roster |
| 7 Economy | Taxes, upkeep, treasury, deficits, connected-city income | Foreign sales, city budget screen, escalating bankruptcy |
| 8 Units | Eleven land units | Ships, aircraft, spies, diplomats, advanced support |
| 9 Movement | Terrain-specific costs (including rivers), roads, persistent orders, unknown-terrain assumptions | Bridges, zones of control, advanced routing |
| 10 Stacking | Three military plus one non-worker civilian/support; unlimited workers; worker grouping | Military groups, ships, amphibious landings |
| 11 Buildings | Nine functional buildings | Full building tree, ports, power, mothballing |
| 12 Infrastructure | Shared worker jobs, roads beyond borders at extra cost, farms/pastures, mines, forts, pillaging | Rail, watchtowers, airfields, depots, clearing, repairs with retained progress |
| 13 Territory | Ownership, age-based growth, border permission | Contested borders, cultural influence, formal withdrawal timers |
| 14 Networks | Capital road network and resource disconnection | Local independent supply networks, rivers, ports, contracts |
| 15 Research | 124 imported technologies across 14 eras, prerequisite graph, era navigation, descriptions, per-tech progress | Research queues and full future unlock implementation |
| 16 Unlock effects | Existing units/buildings/jobs, science/food/production modifiers, map reveal, cold settlement, food-free population | Full imported unlock roster, power/fuel dependencies, resource discoveries |
| 17 Diplomacy | Contact, confirmed war, auto-accepted peace/open borders | Negotiations, attitudes, alliances, truces, AI war strategy |
| 18 Capture | Occupation, population loss, wall damage, cleared queue | Return, liberation, razing, garrison integration |
| 19 Victory | Conquest, Transcendence science victory and defeat | Diplomacy, realm, score, sandbox continuation |
| 20 Happiness | Local modifiers, output/growth penalties | National stability, war weariness, revolt, autonomy |
| 21 Identity | Name, city names, symbolic flag, fixed trait | Premade historical roster, leaders, imported flags, reforms |
| 22 Experience | Three strength ranks and two upgrade chains restricted to owned city tiles | Branching promotions, non-combat XP, unit histories |
| 23 Setup | Seed, two maps, three speeds, four traits | Size, rival count, difficulty, resources, victory selection |
| 24 Time | Shared calendar; research/construction speed | Era-specific presentation and growth speed balancing |
| 25 Accessibility | Top game-details bar, selection sidebar, activity/ETA tooltips, main-menu Load, keyboard controls, Escape menu, quick/named saves and settings | Font scale, rebinding, colour-independent full interface |
| 26 Balance | Provisional numbers and regression checks | Human campaigns and broader balancing |
| 27 Graphics | 21 imported terrain textures, procedural unit art, ownership tint, markers | Terrain transition tiles, finished sprites, era cities, animation and audio |
| 28 Tutorial | Revised help panel and status messages | Interactive tutorial scenario and searchable guide |
| 29 Turns | Ordered processing and re-entrancy guard | Asynchronous AI progress UI and double-click debounce |
| 30 Combat | Deterministic melee/ranged, defence, counters and healing | Damage preview, detailed LOS and advanced combat |
| 31 Fog | Player last-seen snapshots and current unit visibility | Shared intelligence, AI belief/history model |
| 32 AI | Research, economy, legal recruitment, settlement, improvements, visible-target combat | Coordinated strategy, escorts, retreat, diplomacy personalities |
| 33 Persistence | Versioned migration, backup manual save, rotating autosaves | Campaign browser and verified Unity/platform execution |
| 34 Notifications | 150-entry log with recent display | Clickable events, categories, grouping |
| 35 Automation | Queued production/movement and city focuses | Worker queues, rally points, locks, auto-explore |
| 36 Architecture | Pure C# engine, cached network, culling | External data, multiworld architecture, large-map optimisation |

## Current limitations worth testing

- Unity IMGUI layout, actual input handling, JsonUtility behaviour and platform save replacement still require Unity verification.
- The player and two AI nations use the same production, research and economic rules, but AI is deliberately simple. AI factions are permanently hostile to each other and do not autonomously declare player wars.
- Terrain representation, network scope, stack rules and ranged LOS are simplified implementations, not the final design.
- Open-border cancellation may strand units until access is restored or war declared; a proper withdrawal grace period is not yet implemented.
- Cancelling production loses its progress, as stated on the button. There is no stored progress per cancelled project.
- Map generation remains 48x30. No naval travel; all starting regions have land connections.
- Unit artwork is placeholder. Flags are three simple symbols, not imported image assets.

## Alpha 0.3 interface and worker update

- Dedicated own-city screen opens by double left-clicking its tile, including when units occupy it.
- Escape menu order: Quick Save, Save As, Continue, Settings, Quit. Ctrl+S quick saves; successful writes show "Game Saved" for two seconds with the final second fading. Failed writes report failure. Quit has Yes/No confirmation.
- Named saves use a safe in-game name and stay within Unity's save directory; existing names require replacement confirmation.
- Tooltips show unit roles, base strength plus promotions, movement, current activity and job/route ETA. Route estimates depend on currently known terrain and may change.
- Unlimited workers may share a tile without using the non-worker support slot. Grouping includes friendly workers on the same tile; each contributes one work per turn. Job ETA rounds up.
- A road requires 2 work inside your borders, 3 outside on non-hill land, 4 outside on hills. Foreign access rules still apply; other improvements require your own land. Outside roads aid movement; capital resource networks still require owned roads.
- Group, ungroup, new jobs and X cancellation discard unfinished work. Movement points already spent are not refunded. Group destinations can split a team when movement allowances differ; only co-located members share work or X cancellation.
- The new workerGroup field defaults to zero for previous version-3 saves. Save format version remains 3. Unity JSON round-trip checks include grouped active workers but require running in the editor.

Validation: engine checks passed; runtime scripts compiled and simulated UI/input/save checks passed against Unity API doubles. Unity editor rendering, real mouse interaction and real JsonUtility checks remain unverified here.

## Alpha 0.4 — city, research and production update

- Main menu Load lists manual and rotating autosaves. Both Enter keys end a turn on the map; Space acts only on a selected unit and cannot activate a focused button when no unit is selected.
- Unit labels: type for one unit; Stack for matching stacked units; blank for mixed types from the same civilisation.
- Top-bar menus replace right-side civilisation details; Research has a scrollable dependency tree with 12 labelled cards, distinct pixel icons, unlock benefits and prerequisite status.
- City inset shows nearby terrain and actual worked-tile F/P/G contributions. Unworked tiles show zero; global city modifiers and consumption are reflected in the top totals. Constructed buildings have their own scrollable icon list.
- Each city has independent unit training and building construction, each receiving full city production each turn. Each line supports one active and eight waiting projects. A normal click selects exactly one and clears only that line; Ctrl+left-click appends; Ctrl+right-click removes the last queued item or cancels the active project if the queue is empty. Changing/cancelling active projects discards progress.
- One copy of each building type per city; multiple different building types allowed. Military tile capacity can delay unit completion independently of buildings.
- Upgrading requires an owned city plus existing technology, resource, movement and money conditions.
- Version-4 saves migrate prior mixed queues, preserving active progress and order within each new line. Worker groups remain supported.

Validation: engine regression checks, runtime compilation and simulated input/drawing/save checks passed with Unity API doubles. These are not Unity editor rendering or real JsonUtility tests; in-editor checks remain required.


## Alpha 0.5 — attached terrain and full research tree

- All 21 distinct textures are runtime assets in Assets/Resources/Terrain. Duplicate ocean/jungle images are omitted. New games generate the added biomes; old maps are not regenerated. Resource textures load for both map and city inset.
- All 124 technologies, 14 eras, dependencies, costs, descriptions and source unlock labels are imported. Research screen distinguishes active effects from planned unlocks. Every node is researchable when its prerequisites are met.
- Existing content is gated by the matching technology. Additional working modifiers and science victory are described in README. No ships, aircraft, governments, wonders or space-world simulation are claimed implemented by this import.
- Save version 5 expands technology arrays, preserves old progress and queues, restores prerequisites and grandfathered unlocks. Existing map terrain IDs 0–5 retain their meanings.
- Original HTML preserved under Design; texture provenance under Design/TEXTURE-MANIFEST.json.
- Engine regression checks and simulated UI/input/save checks passed outside Unity. Actual Unity rendering, asset import and JsonUtility remain to be verified in the editor.
