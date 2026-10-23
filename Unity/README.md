# Current release: Alpha 0.7

Read **ALPHA-0.7-README.md** for the current feature list, controls, installation and limitations. The sections below document the earlier foundation.

# Paradigm Realm — Foundation Alpha 0.5

This is an updated Unity project based on the previous Paradigm-Realm-Unity.zip (version 5). It preserves the existing opening menu, civilisation naming, drag movement, persistent orders, first contact, connected roads and placeholder unit artwork. Terrain now uses your supplied textures.

**This is not the completed 36-system game.** All 36 design topics are incorporated into Design/MASTER-DESIGN.md. This release implements a broader Earth foundation and labels remaining features in Design/IMPLEMENTATION-STATUS.md.

## Open the project

1. Extract the ZIP.
2. In Unity Hub choose Add/Open project and select the inner **Paradigm-Realm-Unity** folder (the one containing Assets, Packages and ProjectSettings).
3. Use your existing Unity 6 installation. The project records 6000.3.0f1; do not change your working editor just to match this file without a reason.
4. Let scripts compile. Open **Assets/Scenes/Earth.unity**, or use **Paradigm Realm > Open Earth Scene**.
5. Press Play and choose New Game.

## Update an existing copy

Stop Play mode. Copy the **Scripts**, **Editor** and **Resources** folders from the downloaded project's **Assets** folder into your existing project's **Assets** folder. Merge folders and replace matching files. Copy all three: Resources contains the textures; Scripts includes the new content and research files. Do not paste inside the old Scripts folder. Keep your existing ProjectSettings and Packages. See UPDATE-INSTRUCTIONS.txt for the exact sequence.

Start a **New Game** to generate the new terrain types. Existing saves load with refreshed artwork but retain their original geography. Version 5 upgrades versions 1–4; keep a backup if you need to return to older code.

## Controls

- Click a unit/city; repeated clicks cycle a stack.
- Drag a selected unit to a destination. It continues on later turns.
- Arrow keys move the selected unit one tile; WASD and middle drag pan.
- Mouse wheel zooms toward the pointer; scroll bars and middle drag pan. Zoom +/- buttons also change tile scale.
- Tab finds the next idle unit; B founds a city; Enter or keypad Enter ends a turn; Space skips only the selected unit; F1 opens help. Gameplay shortcuts are blocked while menus are open or text is being edited.
- Double-click your city tile (even with units on it) to open its own management screen. Back to map closes it.
- Escape opens Quick Save, Save As, Continue, Settings and Quit. Ctrl+S saves and briefly shows a fading "Game Saved" message. Quit asks Yes/No.
- Settings control wheel sensitivity and tooltips.
- Select a worker to see its job and remaining turns. X cancels the selected unit or co-located worker team activity/route.
- Multiple workers can occupy a tile. Group all workers here shares construction: each worker contributes one work per turn. Ungroup stops the shared job. Grouping/reassigning/cancelling discards unfinished job progress.
- Roads cost 2 work in your borders, 3 outside (4 on hills). Forests outside use the 3-work rule. Foreign border permission still applies. Farms/mines/forts remain restricted to your territory and cost 3 work.
- Worker teams receive shared movement destinations; members with different movement allowances may arrive on different turns. Construction uses only members on the same tile.
- Game details are in the top bar: Research, Economy, Diplomacy, Identity, Cities and Chronicle. The right panel contains the current selection and unit actions.
- Research opens a connected tree of labelled technology cards, each with a unique pixel icon, benefits, prerequisites and a research button.
- Upgrades require a city owned by the unit’s civilisation, as well as the existing technology, resource and gold requirements.
- Map labels show a lone unit’s type, Stack for matching types, or nothing for mixed types belonging to the same civilisation.
- Load Game on the opening menu lists manual saves and autosaves. Invalid saves leave the current session intact.

## New working foundation systems

- 124 technologies across 14 eras, with original prerequisite links, descriptions, unlock descriptions and research costs; independent progress retained when switching research. Active effects and planned content are explicitly separated.
- Eleven unit types, including spearman, horseman, catapult and medic.
- Nine city buildings with gameplay effects.
- Separate simultaneous unit and building queues; focus-based tile assignment, city growth and happiness.
- City territory and expanding borders; peace/war confirmation and open borders.
- Multi-turn roads, farms/pastures, mines and forts; pillaging.
- Improved resources connected to the capital via owned roads; trade income for connected cities.
- Tax policy, unit/city/building upkeep, treasury forecasts and unpaid-upkeep combat penalties.
- Limited tile stacking, fortification, ranged fire, promotions and paid upgrades.
- Four-turn occupation penalties after capture; conquest, Transcendence science victory and defeat.
- AI research, legal recruitment, upkeep, settlement and land development.
- Last-seen player map snapshots and hidden enemy units.
- Custom civilisation/city names, symbolic flags, traits, map preset, seed and speed.
- Shared calendar, manual backup saves, three rotating autosaves and migration from saves v1–4.

Resource icons and new units use placeholder procedural art. Naval transport, aircraft, active portals, advanced diplomacy, national stability, interactive tutorials and diplomatic/realm victories remain planned. Map size and civilisation count remain fixed. See status document for details.

## Save files

Unity's Application.persistentDataPath contains earth-save.json, its .bak backup, and earth-auto-0/1/2.json. Manual saves use temporary-file replacement. The in-game button restores the newest valid autosave. A new game will rotate autosaves from a previous game, so use a manual save if you wish to retain it. Save As creates named earth-named-*.json files with replacement confirmation. Quick Save updates the active manual file. Named saves can be loaded from the Save As screen; Continue Saved Game chooses the most recently modified manual save after restarting. This is an in-game name dialog, not an operating-system file picker.

## Verification

The pure C# engine and its regression checks were compiled and executed outside Unity. A 45-turn campaign and targeted rules checks passed. Runtime scripts were also compiled against minimal Unity API contract doubles, which catch C# integration errors but do not emulate Unity. Unity Editor, actual GUI interaction, platform builds and Unity JsonUtility migration/round-trip checks were **not run here**.

In Unity, choose **Paradigm Realm > Run Engine Checks**. This runs engine checks plus real Unity JSON round-trip and legacy migration tests. Then manually test End Turn, queued movement, the war confirmation, save/load and right-panel layout before relying on this alpha.

## Windows build

After a successful Unity test, choose **Paradigm Realm > Build Windows Game**. Unity's Windows Build Support module must be installed. This ZIP contains source, not a tested Windows executable.

## City screen and production (Alpha 0.4)

Double-click an owned city. The inset map enlarges its neighbouring tiles and can be scrolled as borders grow. F/P/G show actual raw food, production and gold contributed by worked tiles; unworked tiles contribute zero. The central city contributes its base 2 food and 2 production. Food consumption, city buildings and other modifiers are reflected in the city totals. Constructed buildings have a dedicated list with unique icons.

Units and buildings have separate production lines. **Each receives the city's full production output per turn**; an occupied unit tile can delay unit completion without delaying a building.

- Left-click a unit/building: select one project and clear that line's waiting queue. Switching the active project discards its unfinished progress.
- Ctrl+left-click: add one project to that line (up to eight waiting projects). Repeated unit types are allowed.
- Ctrl+right-click inside either production line: remove its last waiting project. If none are waiting, cancel its active project.
- Only one copy of each building type can be present in a city; duplicate active/queued buildings are blocked. Different building types can coexist.

Save format 5 migrates versions 1–4. Old mixed queues split into unit and building lines while retaining each category's order and the old active project's progress. Keep the older save backup if you need to return to Alpha 0.3.


## Attached terrain and technology content (Alpha 0.5)

All 21 distinct images from the texture archive are integrated into map and city rendering. Two duplicate images were omitted. Runtime textures are 128×128 PNGs sampled with nearest-neighbour scaling, point-filtered, without mipmaps or lossy compression. Original upload filenames and SHA-256 hashes are recorded in Design/TEXTURE-MANIFEST.json. The uploaded archive remains the full-resolution source.

Terrain includes the six original types plus lakes, rivers, tundra, bogs, desolate wasteland, jungle, coral reefs, arctic forest, ice sheets, glaciers, savanna, volcanic terrain, rocky coast and mangroves/steppe. Climate bands and seeded patches place the new biomes. Rivers use short north–south segments matching the supplied art. Shoreline art is fixed-orientation; autotiling, seamless river junctions and directional shores remain future work. The starting areas retain guaranteed land access.

The supplied HTML is preserved at Design/From-flint-to-the-stars.html. TechnologyCatalog.cs contains its complete 124-node dependency graph, 14 eras, descriptions, unlock text and original cost formula. Game-speed multipliers still apply. The Research screen has era shortcuts, inspectable cards, highlighted prerequisite/successor connections, navigation buttons and retained per-technology progress. New games begin researching Toolmaking; the starting settler and warrior are bootstrap units. Training later settlers requires Agriculture.

Existing unit/building/job unlocks now follow the imported tree where matching content exists: Pottery → Granary; Masonry → Walls; Horseback Riding → Horseman; Chivalry → Knight; Rifling → Rifleman; Mathematics → Catapult; The Wheel → roads; Mining → mines. Existing Workshop/Barracks/Gardens/Medic bonuses remain foundation additions and appear in Active game effects.

Additional active effects: Fire allows cold settlement; Alphabet adds library science; Irrigation adds river-adjacent farm food; Chemistry adds farm food; Steam adds mine production; Printing/Computers/Quantum add science percentages; Nanotechnology adds production; Satellites reveal the map; Longevity adds happiness; Mind Uploading removes population food consumption; Transcendence wins a science victory. Percentage yields round down to whole numbers.

**Importing the tree does not implement every listed future unit, wonder, building, improvement, government or space project.** Their original descriptions are preserved in the detail panel, alongside an explicit distinction from playable effects. All technologies can be researched, and prerequisite-only technologies advance towards later discoveries. The game still plays on Earth; other worlds and spacecraft remain planned.

Older saves keep their maps, discoveries, queues and stored research progress. Migration adds prerequisites for older discoveries, preserves previously unlocked military/building options and grants The Wheel so existing road workers remain usable. Research whose new prerequisites are still missing is stored until it becomes available. The new tree's long-term pacing needs human playtesting.
