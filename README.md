# Paradigm Realm

An original, 16-bit-inspired, square-tile civilisation strategy game. Early playable alpha inspired by the exploration, city-building and connected-world ideas of Civilization II: Test of Time.

## Play on Windows

1. Click **Code → Download ZIP** on this repository.
2. Right-click the ZIP and choose **Extract All**.
3. Open **index.html** in Edge, Chrome or Firefox.
4. Read the in-game Guide, then select **Found settlement**.

No installation, account, build step or internet connection is needed to play. Keep index.html, engine.js, game.js and style.css in the same folder.

## Included

- Original pixel-drawn square terrain, settlement and unit sprites.
- Seeded 48 × 30 Earth-like world and a parallel realm, with exploration fog.
- Settlers, workers, scouts and military units; roads and farms; terrain movement costs.
- City growth, production, resources, granaries, libraries and walls.
- Seven research advances from Agriculture to Gate Theory.
- Two basic AI rivals that explore, settle and build armies; peace and war controls.
- Combat, city capture, conquest and parallel-realm settlement victories.
- Browser save/load, end-turn autosave and portable JSON export/import.

## Controls

Click units to select; click adjacent tiles or use arrow keys to move. Click a selected unit's city tile to open the city panel, and click it again to select its unit. Tab selects the next unit with movement. B founds a settlement. Enter ends the turn. Use map scrollbars, overview or zoom controls to navigate.

## Development

Plain JavaScript and Canvas 2D, without dependencies. `engine.js` holds rules and state; `game.js` draws the world and handles input; `style.css` styles the interface. Run `node test.cjs` for the engine regression checks. Open index.html to run locally.

## Scope and next steps

This is an early alpha, not a feature-complete recreation. Geography is generated rather than an accurate map of Earth. AI and diplomacy are deliberately simple; rivals use basic units. Naval travel, pathfinding, trade routes, religion, a full historical technology tree, multiplayer, custom scenarios and richer animated artwork remain future work. Maps are currently bounded without wraparound. Pixel artwork is original procedural artwork, with room for a richer sprite atlas.

## References

Design research used the original Civilization II: Test of Time player's manual (MicroProse/Hasbro, 1999), especially its city, movement, technology and multiple-map concepts: https://www.mogelpower.de/manuals/Civilization_2_Test_of_Time_Manual.pdf

Manual index consulted: https://manualzz.com/doc/o/sgfb1/microprose-test-of-time-civilization-ii-user-manual-terrain-and-movement

All implementation and graphics in this alpha were created for Paradigm Realm. No Civilization game code, graphics, music or text are bundled. This project is not affiliated with the Civilization rights holders.
