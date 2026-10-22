# Verification record — 17 October 2026

## Executed successfully

- Compiled World.cs and FoundationChecks.cs with Roslyn C# compiler 4.0.1, then ran on an isolated .NET runtime.
- Compiled all runtime scripts together against minimal Unity API contract doubles. This verifies C# integration only, not Unity runtime behaviour.
- Parsed all C# sources for syntax errors.
- Executed FoundationChecks.Run():
  - New world validity.
  - Persistent multi-turn movement and replacement destinations.
  - Founding a city.
  - Two-turn road construction.
  - Connected iron access and road disconnection.
  - Construction queue advancement and duplicate-building rejection.
  - Tax/happiness trade-off.
  - Experience consumption on promotion.
  - Peace prevents combat; confirmed war allows it.
  - Separate progress retained when switching technology.
  - State validity after every turn of a 45-turn campaign.
  - Malformed unit rejection.
  - Last-seen terrain memory.
  - Ranged units cannot capture or loop on empty enemy cities.
  - Military stacking limit.
  - Conquest and defeat detection.

Result: PASS.

## Not executed here

- Actual Unity Editor compilation/play mode.
- UnityEngine.JsonUtility round-trip and legacy migration checks.
- GUI rendering, mouse gestures, scaling and modal interaction.
- Windows build and real filesystem save recovery.
- Full campaign balancing, large-world performance and multiplayer (not implemented).

## Unity validation

1. Open the project and wait for compilation.
2. Select Paradigm Realm > Run Engine Checks. The editor command runs the engine suite plus real JsonUtility round-trip and legacy migration checks.
3. Start a new game. Found a city, queue a granary after Agriculture, and issue a multi-turn movement order.
4. Press End Turn several times. Confirm turn/calendar changes, jobs progress and orders continue.
5. Meet a rival, issue a hostile order and cancel the war prompt. Confirm peace is preserved; repeat and confirm to test war.
6. Save, change orders, then load. Check treasury, orders, research, queue and worker job state.
7. Test zoom/pan, city selection, scrolling, identity editing and tutorial closure at your screen resolution.

## Alpha 0.3 — 21 October 2026

Executed successfully outside Unity:
- Entire FoundationChecks suite, including the existing 45-turn campaign.
- Foreign/unclaimed flat and hill road duration, worker stacking, grouping, shared progress counted exactly once per worker per turn, team cancellation, ungrouping, foreign access restrictions.
- All runtime scripts compile against minimal Unity API doubles.
- Simulated input routing: wheel direction, pointer anchoring, zoom limits, panel wheel exclusion, Escape modal blocking/resume, city double-click with a worker on the tile.
- Simulated Ctrl+S writes a real temporary save file, starts the two-second toast, safely confines named paths, and reports failure for an invalid save destination. The JSON serializer in these doubles is NOT Unity JsonUtility.

Required Unity manual checks (not executed here):
1. Run Paradigm Realm > Run Engine Checks, including actual JSON round trips with active grouped workers.
2. Wheel over map zooms, wheel over panels scrolls; zoom remains anchored and bounded.
3. Double-click own city (with and without units), edit queue/name/focus, then return.
4. Escape menu prevents movement, ending turns and map interaction underneath; Continue returns to the previous screen.
5. Ctrl+S from map/city/menu shows Game Saved then fades; Save As creates/replaces and loads named saves. Restart and continue the newest manual save.
6. Quit: No stays in game, Yes exits play mode/build.
7. Settings save wheel sensitivity and tooltip visibility across restart.
8. Group workers, build outside borders on flat/hills, inspect ETA and hover details, cancel with X, ungroup and reload a grouped job.
9. Confirm layout and End Turn visibility at the user's display resolution.

## Alpha 0.4 — 22 October 2026

Passed outside Unity:
- Foundation engine suite including the existing campaign and worker regressions.
- Simultaneous unit/building progress and completion, blocked unit stack not blocking buildings, duplicate building prevention, normal selection and last-first queue removal.
- Version-3 mixed queue migration preserves active progress, each category's order and remains idempotent.
- Upgrade fails on owned land outside a city and succeeds inside an eligible owned city.
- Unit labels and selected-unit-only skip semantics.
- All runtime scripts compile against Unity API doubles.
- Simulated Return/keypad Enter, Space with and without selection, modal keyboard blocking, normal/Ctrl-left/Ctrl-right production input.
- Drawing calls cover all technology names/prerequisites, 21 distinct valid pixel icons, city tile contributions and constructed building list. These checks do not validate rendered appearance.
- Main menu Load entry, valid dual-production load and malformed-load preservation using a JSON serializer double (not Unity JsonUtility).

Required real Unity verification (not run here):
1. Run Paradigm Realm > Run Engine Checks for actual Unity JSON round trips including both production lines and worker groups.
2. Load an Alpha 0.3 save, check split queues/progress, save and reopen.
3. Test Return/keypad Enter/Space with and without selected units, and when typing names or opening menus.
4. Inspect unit labels at all zoom levels and on mixed stacks.
5. Inspect the city inset, scrolling, worked/unworked values, constructed building icons and both production queues.
6. Ctrl-click repeated/mixed units, distinct buildings, remove last orders, and verify simultaneous construction.
7. Open each top-bar details menu; scroll and select available research cards; verify locked prerequisites.
8. Inspect actual game layout at the user's resolution and test Save, Load, Escape and Quit.
