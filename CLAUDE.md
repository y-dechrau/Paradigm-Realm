# Paradigm Realm — instructions for Claude

Read `AGENTS.md` first. Its workflow rules apply to Claude exactly as written (GitHub backup after every authorised change, no force-pushes, verify the remote commit, report failed pushes, keep implemented and planned features separate).

Canonical repository: https://github.com/y-dechrau/Paradigm-Realm (the lower-case `paradigm-realm` URL redirects here).

## Where things are

- `Unity/` is the active project (Unity 6000.3.25f1, C#). Source is in `Unity/Assets/Scripts`.
- The repo root (`index.html`, `game.js`, `engine.js`, `style.css`, `test.cjs`) is the old JavaScript prototype. Historical only; do not develop it.
- `Unity/Design/MASTER-DESIGN.md` is the design; `Unity/Design/IMPLEMENTATION-STATUS.md` says what is built versus planned.
- `Unity/Tests/` holds standalone .NET harnesses that use stand-ins for Unity APIs. `Unity/Documentation/` holds validation reports.
- Start with `Unity/START-HERE-DIPLOMACY.md` and `Unity/README.md` for the latest release notes.

## Current state

- Latest release: Diplomacy Stage 3. Stage 1 of the 16-bit plan (map sizes Tiny to Massive, save version 8, delete saved games) is written in source but NOT yet tested in Unity; versions 1-7 migrate.
- Implemented: foundation systems (124 technologies, 11 units, 9 buildings, cities, borders, upkeep, fog, AI), Stage 2 settlement adviser, Stage 3 diplomacy.
- Not implemented: naval play, aircraft, espionage, barbarians, world congress, city-states, vassalage, multiplayer.
- **Unity check (28 Oct 2026, owner-reported):** the project opens in Unity 6000.3.25f1 with no compile errors, and the owner reports the Earth scene works in Play Mode. Engine Checks output, save/load, performance, balance and large maps are not yet verified. Never describe harness or API-double results as Unity playtesting.

## Working rules

- Preserve existing artwork, scenes, textures and `.meta` files byte-for-byte unless the owner asks otherwise. Missing `.meta` files break Unity references.
- Keep saves backwards compatible: bump the save version and add a migration rather than breaking old saves.
- When a change touches game logic, update the relevant harness in `Unity/Tests/` and the status docs in the same commit.
- Be honest about what was and was not verified. If something cannot be run here, say so.
- Ask the owner to test in Unity and report compile errors or console output; use those to drive the next fix.
