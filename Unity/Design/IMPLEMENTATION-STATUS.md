# Implementation coverage — Foundation Alpha 0.4

"Implemented" below means code exists with engine regression checks where applicable, not that the Unity user experience has been playtested. No full-game completion claim is made.

| Topic | Coverage in this release | Still planned |
|---|---|---|
| 1 Setting | Grounded Earth | Speculative-era content and final magic/physics decision |
| 2 Realms | Full design recorded | World identifiers, portal simulation, other maps |
| 3 Maps | Seeded continents and landmass presets | Layered terrain, rivers, archipelago, realistic Earth |
| 4 Navigation | Pan, pointer-centred wheel zoom, zoom buttons, selection and worker-team orders | Minimap, route preview, military group navigation |
| 5 Cities | Dedicated city screen with enlarged tile yields, icon building list, separate unit/building queues, population, food and focus | Manual locks, specialists, project-specific suspended progress |
| 6 Resources | Wheat, iron, gold, horses, spices; resource prerequisites | Material inventories, fuel, wider resource roster |
| 7 Economy | Taxes, upkeep, treasury, deficits, connected-city income | Foreign sales, city budget screen, escalating bankruptcy |
| 8 Units | Eleven land units | Ships, aircraft, spies, diplomats, advanced support |
| 9 Movement | Costs, roads, persistent orders, unknown-terrain assumptions | Rivers, bridges, zones of control, advanced routing |
| 10 Stacking | Three military plus one non-worker civilian/support; unlimited workers; worker grouping | Military groups, ships, amphibious landings |
| 11 Buildings | Nine functional buildings | Full building tree, ports, power, mothballing |
| 12 Infrastructure | Shared worker jobs, roads beyond borders at extra cost, farms/pastures, mines, forts, pillaging | Rail, watchtowers, airfields, depots, clearing, repairs with retained progress |
| 13 Territory | Ownership, age-based growth, border permission | Contested borders, cultural influence, formal withdrawal timers |
| 14 Networks | Capital road network and resource disconnection | Local independent supply networks, rivers, ports, contracts |
| 15 Research | Twelve technologies in a connected card tree with icons/benefits; per-tech progress | Full historical/future tree, distant-tech path preview |
| 16 Unlock effects | Units, buildings, jobs, upgrades, electricity science | Power/fuel dependencies, resource discoveries |
| 17 Diplomacy | Contact, confirmed war, auto-accepted peace/open borders | Negotiations, attitudes, alliances, truces, AI war strategy |
| 18 Capture | Occupation, population loss, wall damage, cleared queue | Return, liberation, razing, garrison integration |
| 19 Victory | Conquest and defeat | Science, diplomacy, realm, score, sandbox continuation |
| 20 Happiness | Local modifiers, output/growth penalties | National stability, war weariness, revolt, autonomy |
| 21 Identity | Name, city names, symbolic flag, fixed trait | Premade historical roster, leaders, imported flags, reforms |
| 22 Experience | Three strength ranks and two upgrade chains restricted to owned city tiles | Branching promotions, non-combat XP, unit histories |
| 23 Setup | Seed, two maps, three speeds, four traits | Size, rival count, difficulty, resources, victory selection |
| 24 Time | Shared calendar; research/construction speed | Era-specific presentation and growth speed balancing |
| 25 Accessibility | Top game-details bar, selection sidebar, activity/ETA tooltips, main-menu Load, keyboard controls, Escape menu, quick/named saves and settings | Font scale, rebinding, colour-independent full interface |
| 26 Balance | Provisional numbers and regression checks | Human campaigns and broader balancing |
| 27 Graphics | Procedural pixel art, ownership tint, markers | Finished sprites, era cities, animation and audio |
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
