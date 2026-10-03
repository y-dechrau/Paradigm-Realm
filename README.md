# Paradigm Realm — Earth alpha

Original 16-bit-inspired square-tile civilisation strategy game, inspired by Civilization II: Test of Time. This iteration focuses exclusively on Earth; additional dimensions are deferred.

## Play

Download ZIP from GitHub's Code menu, extract it, and open index.html in Edge, Chrome or Firefox. Keep engine.js, game.js and style.css alongside it. No installation or internet is required.

## Features

- Original pixel terrain, settlements and unit sprites on a generated 48 × 30 Earth-like map.
- Settlers, workers, scouts and military units; exploration fog, terrain movement costs, roads and farms.
- City growth, production, resources, granaries, libraries and walls.
- Six technologies from Agriculture through Electricity.
- Two simple AI rivals, peace and war, combat, city capture and conquest victory.
- Save/load, autosave and portable JSON saves.

Click a unit, then an adjacent tile to move. Arrow keys also move. Tab selects the next unit, B founds a settlement and Enter ends the turn. Click a city to choose its production. The in-game Guide explains the rules.

## Save compatibility

This Earth-only release uses version 2 saves. Earlier two-world saves are incompatible: start a new game. Export old saves separately if you want to retain them for the previous release.

## Development and validation

Plain JavaScript and Canvas 2D without dependencies. engine.js contains game rules, game.js handles rendering and input, and style.css styles the interface. Run `node test.cjs` for rule checks, a 130-turn simulation, save validation and conquest victory checks. Browser visual verification remains outstanding.

## Scope

Early alpha with generated geography, not an accurate Earth map. AI and diplomacy are basic. Naval travel, pathfinding, trade routes, religion, a full historical technology tree, multiplayer and richer sprites are future work. The map is bounded without wraparound. Parallel worlds and gates are removed from this release and can be developed later.

## References

Design reference: Civilization II: Test of Time player's manual (MicroProse/Hasbro, 1999), for city, movement and technology concepts:
https://www.mogelpower.de/manuals/Civilization_2_Test_of_Time_Manual.pdf

Manual index consulted:
https://manualzz.com/doc/o/sgfb1/microprose-test-of-time-civilization-ii-user-manual-terrain-and-movement

All game code and pixel artwork are original. No Civilization code, graphics, music or text are bundled. This project is not affiliated with its rights holders.
