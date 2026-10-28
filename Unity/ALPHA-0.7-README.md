**Latest: Diplomacy Stage 3.** Read [START-HERE-DIPLOMACY.md](START-HERE-DIPLOMACY.md) first. The notes below describe earlier development.

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
