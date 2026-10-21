# Paradigm Realm — Foundation Alpha 0.3

This is an updated Unity project based on the previous Paradigm-Realm-Unity.zip (version 5). It preserves the existing opening menu, civilisation naming, drag movement, persistent orders, first contact, procedural pixel artwork and connected roads.

**This is not the completed 36-system game.** All 36 design topics are incorporated into Design/MASTER-DESIGN.md. This release implements a broader Earth foundation and labels remaining features in Design/IMPLEMENTATION-STATUS.md.

## Open the project

1. Extract the ZIP.
2. In Unity Hub choose Add/Open project and select the inner **Paradigm-Realm-Unity** folder (the one containing Assets, Packages and ProjectSettings).
3. Use your existing Unity 6 installation. The project records 6000.3.0f1; do not change your working editor just to match this file without a reason.
4. Let scripts compile. Open **Assets/Scenes/Earth.unity**, or use **Paradigm Realm > Open Earth Scene**.
5. Press Play and choose New Game.

## Update an existing copy

Stop Play mode and back up the project first. Copy the entire **Assets/Scripts** folder and **Assets/Editor** folder from this ZIP into the existing project, replacing matching files. World.cs, RealmGame.cs, FoundationUI.cs and FoundationChecks.cs work together. Do not copy only RealmGame.cs. Copy Design and README if you want the specification alongside your project. Keep your existing ProjectSettings and Packages when updating a working project.

## Controls

- Click a unit/city; repeated clicks cycle a stack.
- Drag a selected unit to a destination. It continues on later turns.
- Arrow keys move the selected unit one tile; WASD and middle drag pan.
- Mouse wheel zooms toward the pointer; scroll bars and middle drag pan. Zoom +/- buttons also change tile scale.
- Tab finds the next idle unit; B founds a city; Enter ends a turn; F1 opens help.
- Double-click your city tile (even with units on it) to open its own management screen. Back to map closes it.
- Escape opens Quick Save, Save As, Continue, Settings and Quit. Ctrl+S saves and briefly shows a fading "Game Saved" message. Quit asks Yes/No.
- Settings control wheel sensitivity and tooltips.
- Select a worker to see its job and remaining turns. X cancels the selected unit or co-located worker team activity/route.
- Multiple workers can occupy a tile. Group all workers here shares construction: each worker contributes one work per turn. Ungroup stops the shared job. Grouping/reassigning/cancelling discards unfinished job progress.
- Roads cost 2 work in your borders, 3 outside (4 on hills). Forests outside use the 3-work rule. Foreign border permission still applies. Farms/mines/forts remain restricted to your territory and cost 3 work.
- Worker teams receive shared movement destinations; members with different movement allowances may arrive on different turns. Construction uses only members on the same tile.
- The right panel scrolls: worker jobs, ranged attacks, upgrades, economy, identity, research and diplomacy are there.

## New working foundation systems

- Twelve technologies; independent progress retained when switching research.
- Eleven unit types, including spearman, horseman, catapult and medic.
- Nine city buildings with gameplay effects.
- Construction queues, focus-based tile assignment, city growth and happiness.
- City territory and expanding borders; peace/war confirmation and open borders.
- Multi-turn roads, farms/pastures, mines and forts; pillaging.
- Improved resources connected to the capital via owned roads; trade income for connected cities.
- Tax policy, unit/city/building upkeep, treasury forecasts and unpaid-upkeep combat penalties.
- Limited tile stacking, fortification, ranged fire, promotions and paid upgrades.
- Four-turn occupation penalties after capture; conquest victory and defeat.
- AI research, legal recruitment, upkeep, settlement and land development.
- Last-seen player map snapshots and hidden enemy units.
- Custom civilisation/city names, symbolic flags, traits, map preset, seed and speed.
- Shared calendar, manual backup saves, three rotating autosaves and v1/v2 migration.

Resource icons and new units use placeholder procedural art. Naval transport, aircraft, active portals, advanced diplomacy, national stability, interactive tutorials and additional victories remain planned. Map size and civilisation count remain fixed. See status document for details.

## Save files

Unity's Application.persistentDataPath contains earth-save.json, its .bak backup, and earth-auto-0/1/2.json. Manual saves use temporary-file replacement. The in-game button restores the newest valid autosave. A new game will rotate autosaves from a previous game, so use a manual save if you wish to retain it. Save As creates named earth-named-*.json files with replacement confirmation. Quick Save updates the active manual file. Named saves can be loaded from the Save As screen; Continue Saved Game chooses the most recently modified manual save after restarting. This is an in-game name dialog, not an operating-system file picker.

## Verification

The pure C# engine and its regression checks were compiled and executed outside Unity. A 45-turn campaign and targeted rules checks passed. Runtime scripts were also compiled against minimal Unity API contract doubles, which catch C# integration errors but do not emulate Unity. Unity Editor, actual GUI interaction, platform builds and Unity JsonUtility migration/round-trip checks were **not run here**.

In Unity, choose **Paradigm Realm > Run Engine Checks**. This runs engine checks plus real Unity JSON round-trip and legacy migration tests. Then manually test End Turn, queued movement, the war confirmation, save/load and right-panel layout before relying on this alpha.

## Windows build

After a successful Unity test, choose **Paradigm Realm > Build Windows Game**. Unity's Windows Build Support module must be installed. This ZIP contains source, not a tested Windows executable.
