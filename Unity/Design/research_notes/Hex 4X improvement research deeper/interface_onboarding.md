# Turn flow, automation, onboarding and accessibility for Paradigm Realm (second round)

Evidence labels:
- **[read]**: I fetched the page. The fetch tool passes pages through a summarising model, so wording and lists came through that step. Short quotes are as it returned them.
- **[read, full]**: full page text returned (XAG 101 and the Xbox guidelines intro only).
- **[snippet]**: search-result summary only. Check before relying on it.
- **[weak]**: forum or player opinion.
- **[code]**: this repository, read on 6 Nov 2026. **[screenshot]**: `Unity/Documentation/Unity-Screenshots-2026-11-05-HexGrid`.
- **Judgement**: my reasoning. Nothing here comes from playtesting, and no project file except these notes was changed.

Reading limits: the 60-call budget ran out before three checks could be repeated. They are marked VERIFY where they matter. Everything about Paradigm Realm is from reading code and four screenshots, not from running the game.

## End-turn flow: how 4X games handle "what needs me", blocking prompts and automation

### Takeaway
Unciv has the clearest model to copy, because its source is public: one button shows the first thing that needs you, and only then "Next turn". Humankind shows the risk: blockers that cannot be skipped, or that misfire, stop the game, so a force-end key and a setting are essential. Published designer reasoning on this topic is scarce; most evidence is source code, patch notes and player reports.

### Cited Findings
**Unciv (best evidence, source code)**
- `NextTurnAction` declares, in order: free great person, pick construction, pick a tech, pick a policy, move spies, pantheon and religion choices, world-leader vote, next unit, move automated units, then next turn as the fallback. — [NextTurnAction.kt](https://raw.githubusercontent.com/yairm210/Unciv/master/core/src/com/unciv/ui/screens/worldscreen/status/NextTurnAction.kt) [read; priority inferred from declaration order, plus the next item]
- The button takes the first entry whose condition is true, and clicking performs that action. It also has a context menu and shows a sub-label of units due. — [NextTurnButton.kt](https://raw.githubusercontent.com/yairm210/Unciv/master/core/src/com/unciv/ui/screens/worldscreen/status/NextTurnButton.kt) [read]
- Popups are queued one at a time, only when none is open and it is the player's turn: votes, victory, free great person, alerts, trade requests. — [WorldScreen.kt](https://raw.githubusercontent.com/yairm210/Unciv/master/core/src/com/unciv/ui/screens/worldscreen/WorldScreen.kt) [read]
- A 2018 commit added a "check for idle units" setting, off by default. — [commit d2541e6, a mirror](https://git.wbrawner.com/wbrawner/Unciv/commit/d2541e6) [snippet: page later returned 404, so the current default is unknown]
- Release 4.3.9 added "Auto Unit Cycle" and "Automated units move on turn start", said to follow Civ 5. — [newreleases.io](https://newreleases.io/project/github/yairm210/Unciv/release/4.3.9) [snippet]

**Civilization VI**
- Clicking End Turn while it shows "!" zooms to the next unit needing orders, and Shift+Enter forces the turn. An auto-end-turn option is mentioned. — [Steam discussion listing](https://steamcommunity.com/app/289070/discussions/0/312265327165650851); [CivFanatics, "Stuck on Unit Needs Orders"](https://forums.civfanatics.com/threads/stuck-on-unit-needs-orders.604642) [snippet, weak: which post said which was not separated]
- Reported failure cases: a unit detached from a sleeping support unit, or a spy that does not appear in the cycle. Players also asked to turn off auto-cycling. — same search set [snippet, weak]

**Civilization VII**
- Government choices can be picked from the "Turn Advancer" even with a unit selected, so the end-turn control also carries pending choices. — [Patch 1.1.1](https://civilization.2k.com/civ-vii/game-update-notes/2025-mar-25-patch-1-1-1/) [read]
- Advisors have an "Only Warnings" setting; Firaxis's internal team plays with those warnings on. — [Dev Diary 8](https://civilization.2k.com/civ-vii/archive/dev-diary/victories-post-launch/) [read]

**Humankind**
- A setting (Settings > UI > Enable Mandatories, as reported by players) makes ending the turn skippable. One player found a hint to convert an outpost still blocked the turn, even with the setting off and Shift-Enter, and asked for such popups to say that they block. — [Steam thread](https://steamcommunity.com/app/1124300/discussions/0/3044984779773227020) [read, weak, about 2022]
- Amplitude's release notes list a fix for an Idle Army notification that stopped the end turn. — [release notes thread](https://community.amplitude-studios.com/amplitude-studios/humankind/forums/168-humankind/threads/41945-humankind-release-notes) [snippet]

**Endless Legend 2**
- The Nov 2025 post says the main screen was redone on feedback; the end-turn button was shrunk and upper-left elements consolidated. A switchable indicator shows which players have finished. The result is called "a much cleaner design" taking less space. — [State of the Game, Nov 2025](https://community.amplitude-studios.com/amplitude-studios/endless-legend-2/blogs/1015-%E2%80%8Bstate-of-the-game-%E2%80%93-november-2025) [read]

**Old World**
- Johnson's account centres on Orders, which ration actions per turn, plus undo and linked tooltips. I found nothing from him about an end-turn list. — [My Elephant in the Room, Part 1](https://www.designer-notes.com/my-elephant-in-the-room-part-1/) [read]

**Paradigm Realm today [code]**
- `EndTurn()` runs the world turn and autosaves with no check (`RealmGame.cs` line 31). The button (`HudTheme.cs` line 94) and Enter (`RealmGame.cs` line 67) both call it directly.
- `NextUnit()` (`RealmGame.cs` line 30) already defines "idle": owned, has moves, and not fortified, sentry, exploring, under orders or on a job. It does not auto-advance after an order.
- Clicking a notice always opens the Chronicle, not the thing it describes (`HudTheme.cs` line 168). Only the status line and up to three log lines from this or the last turn appear (`HudTheme.cs` lines 35 to 36). The log holds plain text, so notices carry no location.
- Only Diplomacy has a count badge, for offers (`HudTheme.cs` line 132). An empty research slot shows "Choose research" on the card and blocks nothing.
- Existing automation: auto-explore, worker Auto develop, R road routes, Governor focus, Sentry, Fortify.

### Inferences
**Specific changes (ranked by judgement, none built):**
1. **End Turn becomes a ladder**, Unciv-style. Suggested order: Choose research, Answer offer (offers that expire), City has nothing to build, Next unit (n), End Turn. Button label changes, and Enter or click goes to the item. Small to Medium.
   - **Shift+Enter always ends the turn**, and the button's context area offers "End anyway". Humankind's reports show why a blocker must never trap the player.
   - **Settings:** "Stop at idle units" and "Stop at empty cities", each on by default in tutorial games. For veterans, idle units could default to off, as Unciv's did in 2018. Evidence is thin, so playtest this.
   - **Two production lines:** treat a city as empty only if the unit line and building line are both empty. Otherwise the ladder nags every turn, since `c.build` and `c.buildingProject` are separate.
   - **Tests:** add a harness test that no ladder state can block forever.
2. **Auto-advance to the next idle unit after an order**, as a setting. Players asked to turn this off in Civ VI, so it must be optional. Small.
3. **Notices go to their subject.** Store a tile or city id with each note and call the existing `FocusTile`; keep Chronicle as a separate button. Small to Medium, because log lines are strings today.
4. **A warnings-only advisor** in Divus Maximus's voice, after the tutorial: city about to riot, gold going negative, enemy beside a city, city starving. It follows Firaxis's Only Warnings and PvZ's hint rule. Small to Medium.
5. **Do not add auto-end-turn yet.** Players ask for it, but the early game needs attention. Revisit after the hand-played game in item 1 of the earlier report.

### Gaps
- No primary or designer source for Civ IV or Civ V end-turn design. A Humankind forum poster says Civ IV to VI have an auto-end option; I did not confirm it for IV or V. No Old World end-turn source.
- No data anywhere on whether blockers help or hurt.

## Civ VII interface criticism and fixes, in Firaxis's words

### Takeaway
Firaxis said "Our top priority is improving the UI" before launch, then shipped many small fixes rather than a single redesign. Most are basics that players assume: readable health bars, tooltips on screen, notifications that fire, tutorial pop-ups in order. Paradigm Realm can treat these as a checklist.

### Cited Findings
- Firaxis named more intuitive interactions, map readability and formatting polish, with "quality-of-life UI adjustments" in 1.1.0. — [Firaxis message](https://civilization.2k.com/en-GB/civ-vii/news/message-from-firaxis-games/) [read]; quote relayed by [CGMagazine](https://www.cgmagonline.com/news/civilization-vii-quality-of-life-updates) [read]
- Before launch, Beach's diary said the team would look to "elevate our user interface" and "provide better tutorialization". — [Dev Diary 8](https://civilization.2k.com/civ-vii/archive/dev-diary/victories-post-launch/) [read]
- **Patch 1.1.0:** unit tooltips kept on screen; tree connector lines aligned; scrollbars answer the mouse wheel; "Not enough gold" message fixed; Dawn of Man tutorial no longer closes itself; Social and Crisis Policies slotted separately. — [Patch 1.1.0](https://civilization.2k.com/civ-vii/game-update-notes/2025-mar-04-patch-1-1-0/) [read]
- **Patch 1.1.1, input and flow:** optional Quick Move (off by default); notifications appear faster and play at turn start; unit action tray expanded by default; restart button in the first few turns. — [Patch 1.1.1](https://civilization.2k.com/civ-vii/game-update-notes/2025-mar-25-patch-1-1-1/) [read]
- **Patch 1.1.1, information:** "District Under Attack" notification; advisor warnings for Treasure Fleets, including a missing technology; Trade Lens; more readable health bars; plot tooltips for specialists; "Improved the alignment and ordering of tutorial pop-ups". — same page [read]
- **Patch 1.2.0:** late-game slowdown fixed when opening the production menu; research icons fixed when queueing with tutorials on; adjacency arrows shown each time, not only the first. — [Patch 1.2.0.1](https://civilization.2k.com/civ-vii/game-update-notes/2025-apr-29-patch-1-2-0-1/) [read]
- These three patch pages list no dedicated end-turn or automation item. — same pages [read]
- A critic saw the UI as overhauled late and noted auto-explore, quick move and renaming as things players simply expected. — [Aftermath](https://aftermath.site/civilization-vii-civ-7-update-ui/) [from first-round notes, journalist opinion]

### Inferences
Checklist against Paradigm Realm, from code and screenshots:
- **Restart in the first few turns:** absent. It is cheap and answers bad starts. Worth adding; see the undo section.
- **Quick Move, or skipping unit animation:** not audited.
- **Yields lens:** the earlier report's item 11 proposes a held key. A toggle should exist as well, per the hold guidance below.
- **Action tray open by default:** Paradigm's unit card shows 4 or 5 actions plus More, and Auto develop lives inside More (`HudTheme.cs`). Consider promoting it, since the tutorial names it.
- **Warnings that name the missing thing:** `JobTechMissing` already does this for workers. Extend the pattern to notices.
- Firaxis's sequence shows that interface repair after launch is slower and more public. This game has no public-release pressure yet, so polish costs less now. Judgement.

### Gaps
- I read three of many patch notes. Later ones, including 1.3 and 1.4, were not itemised for interface changes.
- Firaxis gave no numbers on the effect of any fix.

## Onboarding: data, tutorials, advisors and progressive disclosure

### Takeaway
There is now partial PC 4X data, which the first round lacked: Steam achievements show Old World's "Learn to Play" series falling from 53% to 40% to 25%. Firaxis, Mohawk and PvZ's designer all point the same way: teach at the moment of need, one thing at a time. Paradigm's tutorial does the reverse after turn 2.

### Cited Findings
**Steam global achievements** (percent of Steam players, as summarised on 6 Nov 2026; pages showed only part of the list; my attempt to parse raw pages failed, so figures are not re-checked)
- **Old World:** Play a Single Player Game 65.8%; Learn to Play 1 53.2%; Learn to Play 2 39.7%; Learn to Play 3 25.3%; "Learned" 22.9%. — [Steam stats](https://steamcommunity.com/stats/597180/achievements) [read; 20 of 240 entries shown; I infer these are the tutorial scenarios, and did not see the descriptions]
- **Civ VI:** win on Settler or higher 37.3%; win from Ancient Era 36.8%; win on Chieftain or higher 33.7%. — [Steam stats](https://steamcommunity.com/stats/289070/achievements) [read; 20 of 320 shown]
- **Humankind:** "Play 10 turns" 90.5%. Win-a-game items seen: finish a game while refusing all demands 26.5%; win without Unrest 26.0%. — [Steam stats](https://steamcommunity.com/stats/1124300/achievements) [read; these are conditional wins, so a floor at best]
- **Civ VII:** 3 cultural triumphs in Antiquity 27.9%; win the Modern Age as Confucius 10.0%. No general first-win item seen. — [Steam stats](https://steamcommunity.com/stats/1295660/achievements) [read]
- **Outside 4X, as scale only:** tutorial or first-win achievements range from 31.2% (Lethis) to 76.4% (Elaine) to 80.5% (Steam Tactics). — [Lethis](https://steamcommunity.com/stats/359230/achievements), [Elaine](https://steamcommunity.com/stats/587480/achievements), [Steam Tactics](https://steamcommunity.com/stats/708950/achievements) [snippet]
- Beach: under half of Civ VI players finished a game. — [Game Developer](https://gamedeveloper.com/design/firaxis-big-swing-with-civilization-vii-convincing-players-to-actually-finish-their-games) [first-round read]. The 37% above is the same order, though the measures differ and neither states its denominator in what I read.
- Johnson: many players never finish Civ, so Old World is capped at 200 turns. — [PCGamesN](https://www.pcgamesn.com/old-world/civilization-too-long) [read]

**How others teach**
- Civ VII's tutorial is on for a first game and gives tips "exactly when you first encounter something new". Four advisors lead quests, and players overwhelmed can follow "only a single Advisor". — [Dev Diary 8](https://civilization.2k.com/civ-vii/archive/dev-diary/victories-post-launch/) [read]
- Advisors offer advice rather than requirements, each tied to a play style. — [Civ VII beginner's guide](https://civilization.2k.com/en-GB/civ-vii/news/civilization-vii-beginners-guide/) [snippet]
- Old World: tutorial events were "less prone to cluster on the same turns" after Update 116, which also added the easier level The New. There is a "dynamic" tutorial event that triggers from game state. — [Update 116](https://mohawkgames.com/2023/11/22/old-world-update-116/) [read]
- Old World also has a Tutorial: Undo event, later given an example video. — [Mohawk notes on GitHub](https://gist.github.com/MohawkGames/0f5ff6751c38d094a883c920804d7aa9) [snippet: exact gist for each change not confirmed]
- Old World's "infinite tooltips" let players open a tooltip from a tooltip instead of consulting a wiki; Johnson credits Jon Shafer's Civ 5 work as first. — [Designer Notes](https://www.designer-notes.com/my-elephant-in-the-room-part-1/) [read]
- George Fan, GDC 2012 (Plants vs. Zombies, not a 4X): do rather than read; spread mechanics out; keep text brief; give mid-game messages passively; show hints only to players making mistakes; cut messages that neither inform nor entertain. — [Game Developer](https://gamedeveloper.com/design/gdc-2012-10-tutorial-tips-from-i-plants-vs-zombies-i-creator-george-fan) [read]
- A strategy-game commentator: explaining what a GUI element does does not teach how to play; teach when it becomes relevant. — [Game-Wisdom](https://game-wisdom.com/critical/onboarding) [snippet, weak]

**Progressive disclosure**
- Show core options first and defer advanced ones to a secondary screen. Benefits include learnability, efficiency and error rate. — [NN/g](https://www.nngroup.com/articles/progressive-disclosure/) [read]
- Criteria: frequent features up front, an obvious labelled way to the second level, and selection by task analysis and usage data, tested with users. — same page [read]
- Two levels is the practical limit; more tends to hurt. Staged disclosure (a wizard) suits distinct steps, and becomes a problem when steps depend on each other. — same page [read]

### Inferences
- **The Old World fall** suggests each further tutorial lesson costs a large share of players, so short lessons in play seem safer than long separate scenarios. This is weak: achievement data cannot show why people stopped, and owners may simply have started a normal game.
- **Separate scenarios:** Paradigm should keep teaching inside the real game, which it already does.

**Specific changes (full reasoning in the walkthrough):**
1. Make steps 6 to 11 fire on events, at most one per turn (Small).
2. Show the guide on the city and research screens (Small).
3. Add a one-line current goal below the research card, kept for the whole first game (Small). See the guideline "reminders of current objectives".
4. Dim, in a tutorial game only, rail buttons that have nothing to do yet (Realms, Religion, Victory, Identity), keeping them clickable and showing why (Small). NN/g's two-level limit applies, so do not nest them further. Judgement; playtest it.
5. A "First game" preset: small map, few rivals, peoples slider low, an easy level once difficulty is real (Small to Medium). Earlier report item 15.
6. Local funnel: log tutorial step, turn reached and session count to a local file, shareable by choice. It costs little and yields this game's only real data.

### Gaps
- Still no developer-published tutorial funnel for any PC 4X. No GDC talk on Civ's tutorial or advisors was found. GDC talks found concern other genres: [Rainbow Six Siege onboarding](https://gdcvault.com/play/1034331/Teaching-Complex-Games-Onboarding-Redesign) and [The Gamer's Brain, Part 2](https://gdcvault.com/play/1022951/The-Gamer-s-Brain-Part) [snippet, not read].
- Old World's Learn to Play achievement descriptions were not visible. Steam percentages depend on who owns the game (free weekends and bundles would lower them). Compare within a game, not across games.
- Civ IV and V advisor design: no source read.

## Undo: Old World and others around revealed information

### Takeaway
Old World accepts the information leak: its undo acts like reloading an earlier state, and I found no rule against undoing a reveal. Wargames such as Panzer Corps instead lock undo once new enemy information appears. For Paradigm Realm, "restart this turn" from the existing autosave costs little. Per-move undo should refuse moves that reveal anything.

### Cited Findings
- Undo was "initially intended just to help with misclicks" and became a pillar, letting players try attack plans. It was among the most popular features and often led reviews. Johnson notes deterministic combat is what makes it practical. — [Designer Notes](https://www.designer-notes.com/my-elephant-in-the-room-part-1/) [read]
- Mohawk's Solver says undo works like reloading an earlier save and can go back several turns, even past an AI's elimination; "we discard undo data after a few turns to keep save size manageable". — [CivFanatics thread](https://forums.civfanatics.com/threads/civ-7-undo-or-backup-turn-game-mechanic.685920/latest) [read, forum post by a developer; I relied on the summariser for his role]
- In the same thread a player objects that scouting, reverting and moving differently gives the human an edge over the AI. — same [read, weak]
- Multiplayer limits: undo disabled when several players are connected, later allowed in co-op with strict turns, and a game option added for Cloud and Hotseat. — [Mohawk notes](https://gist.github.com/MohawkGames/595020c183105fb5491cb3d78a8a433b); [Mohawk site](https://mohawkgames.com/?p=3252) [snippet]
- Panzer Corps reportedly allows undo unless the move reveals new enemy information, and a Slitherine thread describes scouting abuse of undo. — [Slitherine forum](https://forum.slitherine.com/viewtopic.php?p=38844) [snippet, weak]
- A Freeciv poster says single-move undo would be very hard there, because actions change game data in ways that cannot be rolled back. — [Freeciv forum](https://forum.freeciv.org/f/viewtopic.php?p=202) [snippet, weak]
- Civ VII added a restart button for the first few turns, not after an Age transition or in network multiplayer. — [Patch 1.1.1](https://civilization.2k.com/civ-vii/game-update-notes/2025-mar-25-patch-1-1-1/) [read]
- Xbox guidance: allow cancelling before completion, and "if cancelling isn't possible, provide undo". — [XAG 107](https://devdocs.xbox.com/build/game-principles/accessibility/xag-deep-dives/xag-107-input.md) [read]
- [PC Gamer's undo article](https://www.pcgamer.com/put-an-undo-button-in-every-strategy-game) came back truncated, so its body was not read. Its search summary says any action or a whole turn can be undone in Old World. [snippet]

**Paradigm Realm [code]**
- `AutoSave()` runs straight after `EndTurn` (`RealmGame.cs` line 31), so a start-of-turn snapshot already exists. On maps over 9,000 tiles it runs only every fifth turn (first-round notes, `IMPLEMENTATION-STATUS.md` line 495).
- Saves are large: 7.7 million characters at turn 30 (first-round measurement). An Old World-style snapshot per action would be too expensive on the main thread unless packed. Judgement.
- Combat has an exact preview, and damage is a formula per `CLAUDE.md`. That favours undo, though I did not check every outcome for randomness.

### Inferences
Three steps, smallest first:
1. **Restart game** in the first 5 turns, with same-seed or new-seed choices. Small. Precedent: Civ VII.
2. **Restart this turn** from the turn-start autosave. Small to Medium. It leaks information as Old World does, which is acceptable in single player. On large maps, either take a turn-start snapshot in memory or wait for smaller saves (earlier report item 10). Should not count as cheating; disclose it as an option.
3. **Undo last move**, by recording the unit's position, moves and order. Refuse it, with a stated reason, if the move revealed any tile, entered combat, met a civilisation, or took a village or ruin. Medium. This takes the Panzer Corps rule rather than Old World's, because a small inverse record is much cheaper than state snapshots, and hidden-map reveals are the only hard case.

Priority stays as the earlier report says (item 19, after playtesting), but step 1 could move up because it is so cheap. Also add a one-line confirmation of accidental End Turn only if playtesting shows it happens: Enter fires instantly today.

### Gaps
- I found no Mohawk statement that a reveal is deliberately allowed; that is inferred from Solver's description of undo as a reload. Tested behaviour was not available.
- Unciv's undo was not examined. Humankind and Endless Legend undo: nothing found.

## Accessibility: Game Accessibility Guidelines, Xbox guidelines, and IMGUI

### Takeaway
IMGUI is not the barrier to key remapping, which is a small refactor. For UI scale, the barrier is fixed 1280 x 800 rectangles rather than IMGUI itself: scaling the entire canvas already works, but enlarging text inside fixed boxes does not. The best-fitting items are text size, remapping, colour independence, hold alternatives and objective reminders.

### Cited Findings
**Sources**
- Full list read for motor, cognitive, vision and general. — [Game Accessibility Guidelines](https://gameaccessibilityguidelines.com/full-list/) [read]
- Xbox guidelines are version 3.2 (8 June 2023), explicitly best practices and not a compliance list. — [Xbox guidelines](https://learn.microsoft.com/en-us/gaming/accessibility/guidelines) [read, full]
- XAG 112 asks for a non-scrolling way around zoomed maps, such as a list of points of interest. XAG 114 says critical text should not exceed lower secondary reading level. — [XAG index](https://devdocs.xbox.com/build/game-principles/accessibility/xbox-accessibility-guidelines.md) [snippet]. I did not read XAG 102 (contrast), 112, 113 or 114 in full.

**XAG 101 text** — [XAG 101](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/101) [read, full]
- PC minimum default: 18 px at 1080p, 36 px at 4K, measured as body height from lowest descender to highest ascender.
- Players should be able to resize text up to 200% of the minimums without losing content; icons should scale with it.
- At least one sans serif option; line width no more than 80 characters; line spacing 1.5 in blocks over two lines; sentence case rather than full caps for lines of text; left or right alignment rather than centred.
- Platform magnifiers do not count as a mitigation.

**XAG 107 input** — [XAG 107](https://devdocs.xbox.com/build/game-principles/accessibility/xag-deep-dives/xag-107-input.md) [read]
- In-game remapping of all mouse and keyboard controls, including `Esc` on PC. Any action to any input; a key combination replaceable by one key.
- On-screen hints, tutorials and diagrams must update when bindings change.
- Menus navigable by single key presses; play and exit by keyboard alone.
- Avoid long holds and simultaneous presses, or offer alternatives.
- Mouse sensitivity at least ±50% of default. Activate on mouse-up.

**Items that best suit this game, with status [code]**

| Source and level | Item | Status |
|---|---|---|
| GAG Basic, vision | No essential information by colour alone | Open; earlier report item 12 |
| GAG Basic; XAG 101 | Readable default size | **Likely fails.** See below |
| GAG Advanced; XAG 101 | Adjustable font size, 200% | Absent |
| GAG Basic, motor; XAG 107 | Remappable controls | Absent; `KeyCode` in 4 files |
| GAG Intermediate, motor | Alternatives to holding | Partial. Full tile details need Alt held (`HudTheme.cs` line 99); add a toggle |
| GAG Intermediate, motor | Multi-action input optional | Partial. Move button replaces right-drag, but queue appending needs Ctrl+click and road routes use R then right-drag |
| GAG Intermediate, cognitive | Reminders of objectives and controls | Absent, apart from the End Turn key cap |
| GAG Intermediate, cognitive | Contextual help | Static F1 page, and it is out of date |
| GAG Basic, cognitive | Simple clear language | Mixed; see walkthrough |
| GAG Intermediate, general | Change difficulty mid-game | Absent; difficulty does nothing yet |
| GAG Basic, general | Describe features; ask for feedback | Absent |
| GAG Intermediate, general | Autosave, manual save | Present |
| GAG Basic, cognitive | Text at own pace | Present (turn-based, Next button) |

**How the code handles input and scale [code]**
- `ProjectSettings.asset` has `activeInputHandler: 0`, the old Input Manager. Keys are read as `Event.current.keyCode` in `OnGUI`, on one long line each in `FoundationUI.cs` (3 lines), `InterfaceV4.cs`, `RealmGame.cs` and `TilePrompts.cs`.
- `GUI.matrix` scales a fixed 1280 x 800 canvas uniformly and centres it (`RealmGame.cs` line 37). On 16:9 this leaves side bars: at 1920 x 1080 the scale is 1.35 and the canvas 1728 wide. The screenshots show these bars.
- HUD positions are `static readonly Rect` constants (`HudTheme.cs` lines 16 to 19).
- `ContentUI.cs` line 54 records that Unity clips scaled content wrongly in a scroll view under `GUI.matrix`, so the tech tree scales its own rectangles and fonts.
- `FitLabel` shrinks text to fit, down to 9 or 8 points (`HudTheme.cs`), which works against any larger-text setting.
- Font sizes: tutorial and `hudSmall` 12, `hudLabel` 14, plates 11, tile labels and key caps 10.
- Forum advice on `GUI.matrix` is the same pattern, with warnings on aspect ratio and touch. — [Unity discussions](https://discussions.unity.com/t/bulletproof-way-to-do-resolution-independant-gui-scaling/28808) [snippet, weak, old]
- Unity describes IMGUI as mainly a programmer tool, not generally for player-facing UI. — [Unity manual](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/ui-imgui/guiscripting-guide) [first-round read]

### Inferences
**Remapping: IMGUI is no obstacle.**
- Add an action enum, a `Dictionary<Action,KeyCode>` stored in PlayerPrefs, and a helper replacing `keyCode==KeyCode.Tab`-style tests. A settings row captures the next `KeyDown` event. Conflict checks are needed.
- Hard-coded names must then read the binding: the "Enter" key cap, the F1 page, tutorial texts and button labels. This is the larger part of the job. Medium. I did not read Unity's `Event` page this round; `keyCode` use is taken from the project's own working code.
- Moving to the new Input System is not needed for this.

**Scale: a limit of layout, not of IMGUI.**
- At 1080p, 12-point text renders at a nominal 16.2 px and 14-point at 18.9 px. XAG measures body height, which is usually close to or below nominal size, so 12-point body text is probably under 18 px, and 14-point borderline. Measure with Microsoft's screenshot method. This is an estimate, not a measurement.
- **Cheap first step:** raise the smallest sizes (10 and 12) by 1 to 2 points where boxes allow, and stop `FitLabel` below about 11.
- **Use the side bars:** let the virtual canvas widen to the screen's aspect, about 1422 x 800 on 16:9, and anchor right-side widgets to the right edge. The map view constant `MapW` would need to follow. Small to Medium. This gains no text size but gives room for larger panels.
- **UI scale 100 to 150%:** a global multiplier on the matrix alone would push fixed rectangles off the canvas. Instead, keep a smaller virtual canvas and anchor by edge: the rail and card left, End Turn right. Screens that fill 1280 x 800 (city, research, diplomacy) need scroll or redesign. Large, screen by screen. Do it for the HUD first, not everywhere.
- **Rewrite to uGUI or UI Toolkit:** no evidence justifies it for scale or remapping alone. It would become justified only if screen-reader support became a goal, since IMGUI exposes no accessibility tree to my knowledge (not sourced).

**Other cheap items**
- A toggle for tile details, in addition to Alt.
- A "List accessibility options" line in Settings and a feedback line.
- XAG 112's points-of-interest idea largely exists already as the Cities and Military screens; add click-to-focus where missing.
- Reading level: Divus's joke-heavy lines are charming but carry instructions. Put the instruction first and the joke second, so tired readers still get it.
- Sentence case in tutorial titles is already used; Cinzel headings are short labels, which XAG exempts.

### Gaps
- Contrast ratios not measured; XAG 102 not read.
- No data on how many 4X players use these options.

## Walkthrough: what a new player meets in the first 20 turns

### Takeaway
This is **judgement from reading code, tutorial text and screenshots, not playtesting**. The biggest finding: the tutorial's event-driven part ends on turn 2, six text cards can be clicked through at once, and then nothing guides turns 3 to 20, during which little may happen. Several small errors are cheap to fix now.

### Cited Findings
All [code] or [screenshot]. Numbers are defaults seen in the code and the auto-shot game, and they may differ with map or speed settings.

**Before turn 1**
- New Game defaults include tutorial on (`RealmTutorialOn` defaults to 1) and `setupPeoples=10`. Difficulty is not functional (first-round finding). I did not audit the New Game screen.
- The player starts with a Settler, a Warrior to the east and a Worker to the south (`World.cs` line 247).

**Turn 1, step by step**
1. **Welcome card (step 0).** Whole screen dimmed; keys blocked by `PromptOpen`. Two clear choices: "Show me how to play" and "I know what I'm doing (skip)".
2. **Found your first city (step 1).** Compact panel at top centre (`TutorialRect`, 640 x 126).
   - **Confusing 1:** it says press Found city "(or F)". The button reads "Found", and `RealmGame.cs` line 67 binds **B**; the help page also says B. VERIFY: one line in `FoundationUI.cs` line 160 contains `KeyCode.F` or `F1`, and my output was cut before showing which.
   - **Confusing 2:** it says "Left-click to select it", but the map screenshot shows the Settler already selected. VERIFY in a real new game, since that shot is auto-driven.
   - **Confusing 3:** "Good ground has fresh water, food and a hill" gives no way to judge. Settlement advice exists but sits behind holding Alt (`HudTheme.cs` line 99), and the tutorial never mentions it.
   - **Confusing 4:** three voices say similar things at once: the panel, the top notice "Select your Settler, then Found city", and a second notice "Found a city, choose production and research. F1 opens the guide". They give different levels of detail.
   - Founding takes three confirmations: Found, "Yes, settle here", then name and "Found city" (`TilePrompts.cs`). Fine for a first city; check it is not tiresome for the tenth.
3. **Your city (step 2).** "Double-click it, or press Open city".
   - **Confusing 5:** the panel explains the left and right sides of the city screen, but `OnGUI` returns before `DrawTutorial` when the city screen is open (`RealmGame.cs` lines 49 and 52). The guide vanishes exactly as the player arrives, and opening the screen also completes the step.
   - **Confusing 6:** it says "Start with a Warrior or a Worker", but a new city already builds a Warrior (`City.build=2`), and the player already owns one of each. A Scout (14) is the obvious exploring choice and is not mentioned.
   - **Confusing 7:** the city screen shows "UNIT TRAINING — 0 QUEUED" directly above "1. Warrior 0/10 — 3 turns". The zero counts the waiting list, but it reads as a contradiction.
   - **Confusing 8:** a plain click replaces the current item and discards its progress (`World.cs` line 407). Appending needs Ctrl+click, which only the F1 page explains. Two parallel production lines (Units, Buildings) are unusual and untaught in the tutorial.
   - **Confusing 9:** "Worker → Entertainer" means a citizen, yet Worker is also a unit. "Shields" and "production" are used for the same thing on one screen. "Trade at 50% science" is unexplained, and nothing shows where to change the rate.
   - **Confusing 10:** "Settler: Requires city size 2" is a good message, but growth is 14 turns away and the Settler costs 90, so the next city is far off. Nothing says so.
4. **Research (step 3).** "Open Research from the left bar and pick".
   - **Confusing 11:** the step completes merely by opening the screen (`tutorialResearchSeen`). Agriculture is already selected, so the player may not know whether a choice was needed. The guide is again hidden on that screen.
   - **Confusing 12:** the card showed "45 turns" with Science +0 before the city existed, and the city screen shows Science +2. So about 23 turns at first, assuming a cost of 45 worked out from that display. The research screen also opens onto a tree that is part of 180 technologies; "four of its six keystone technologies" has no prior explanation.
5. **Explore (step 4).** "Select a Warrior or Scout".
   - **Confusing 13:** there is no Scout yet. Three ways to move are present: right-click, right-drag and the Move button. The F1 page still says "Drag units to destinations", from before the left-selects, right-directs change.
   - The step finishes as soon as any unit has an order, or at turn 4. Good.
6. **End the turn (step 5).** Enter or the button ends at once with no check, so the unmoved Worker and idle units pass silently.

**Turn 2: the rest of the tutorial**
- Step 5 completes when `turn>=2`. Steps 6 to 11 use a Next button and do not wait for anything (`TutorialWaits` covers only 1 to 5). So Workers, Growth and happiness, Borders and the wider world, Faith, Other civilisations, and Ages and victory can all be read in one sitting on turn 2, before any of them applies.
- **Confusing 14:** this is the cluster Mohawk tweaked away and the opposite of Firaxis's and Fan's advice. The texts run up to about 330 characters at 12 points in a box 64 high, so step 8 may be a tight fit. VERIFY visually.
- **Confusing 15:** step 6 says Workers build roads, farms and mines, and that "Anything greyed out needs a technology". The F1 page says Agriculture gives farms, Mining gives mines, The Wheel gives roads; clearing needs Bronze Working (first-round status notes and `CLAUDE.md`). If no starting technology unlocks these, **all four Worker buttons are grey for about the first 20 turns**, and the Worker has nothing to do except, perhaps, Auto develop in the More panel. VERIFY: I did not read which technologies a new game starts with, and that F1 text is partly stale.
- **Confusing 16:** step 8 mentions the sealed gateways, while the Realms button stays on the rail with nothing to do. Step 9's Religion and step 11's Victory are likewise far off.

**Turns 3 to 20 (no guide, judgement from the numbers)**
- **What can happen:** Warrior done about turn 4 at 4 shields (cost 10), then perhaps a Scout (14), Worker (20) or nothing queued. Growth to size 2 about turn 15 at +1 food on 14 needed. Then a Settler at 90 shields, about 20 turns at 4 to 5 shields. So a **second city around turn 35 or later** if the player builds nothing else, and the first technology around turn 23.
- **This conflicts with a harness figure:** rival games complete eras around turn 36, meaning four technologies by then, so science must rise faster than +2 at some point. Measure it in the hand-played game. Until then treat my turn counts as rough.
- **What the player does:** moves two or three units around and presses End Turn. Explore and village or ruin finds are the main events. There is no early danger today (earlier report item 13), so the Warrior has no urgent job.
- **Confusing 17:** no statement anywhere of a current goal. The top bar's Order, Science and Gold have no visible targets. "Order —" before a city exists and "Strength 0 / 1" on the card (attack and defence) are unlabelled.
- **Confusing 18:** a notice cannot show the place it describes; clicking opens the Chronicle.
- **Confusing 19:** first contact opens Diplomacy unprompted (`Civilisations.cs` line 158), with trade, friendship and war options the player read about many turns earlier, if at all. Good that it is a moment of drama, but it is also the moment to teach.
- **Confusing 20:** the F1 guide, which notices recommend, ends "Foundation alpha: Earth, 3 civilisations, 124 technologies across 14 eras", says naval units remain planned, and refers to "Economy" where the rail says "Empire" (`RealmGame.cs` line 374). A new player who turns to help for reassurance gets wrong facts.
- **Confusing 21:** the rail shows ten destinations from turn 1; Identity (flags) and Chronicle are not obviously game actions. No hover explanation was seen in the code I read. Not audited.

**What works well (also judgement)**
- Tutorial panel is clearly part of the HUD and can be hidden or ended.
- Actions finish steps by doing, as Fan advises.
- Grey jobs name the missing technology.
- Divus's voice is distinctive.
- End Turn is large and shows its key.
- Tile yields appear on hover without a hold.

### Inferences
**Fix list, smallest first:**
1. **Correct the text:** F versus B; rewrite the F1 page; "0 queued" to "Waiting: 0"; "Worker → Entertainer" to "Citizen → Entertainer"; pick shields or production; label Attack and Defence. Half a day. Update `RealmTools.Checks` only if a harness asserts strings.
2. **Guide on the city and research screens.** A slim strip along the existing bottom line, at steps 2 and 3, listing what to look at. Complete step 3 when a choice is made or confirmed. Small.
3. **Event-driven steps 6 to 11, at most one per turn:**
   - Workers: first turn the Worker is selected, or turn 3, with honest text about what is available now, pointing to Auto develop.
   - Growth: first city growth or first unhappy citizen.
   - Borders: first village or ruin seen, or border contact.
   - Faith: when faith could buy a pantheon.
   - Other civilisations: on first contact, before or inside Diplomacy.
   - Ages and victory: first era score event, or about turn 30, and link the Victory screen.
   
   `TutorialStepDone` and `TutorialWaits` already support this shape. Small. Progress in PlayerPrefs needs a look, because out-of-order firing no longer fits one step number; a bit mask would do. Do not save this to the game file without a version bump.
4. **Goal line**, for the first game: "Grow Azurehold to size 2 (9 turns)", "Train a Settler", "Found a second city", then "Research Writing". Each one derived from game state, not scripted by turn. Small. It answers Confusing 10, 12 and 17 at once.
5. **Settler guidance:** while a Settler is selected, show site advice on the tile card without Alt. Small.
6. **End Turn ladder** from the first section; in a tutorial game it also fills the Worker and idle-unit gap.
7. **First 20 turns as a design question for the owner:** if the hand-played game confirms long gaps, consider a cheaper early Settler or Agriculture, faster first growth, or a starting technology that gives the Worker a job. These are balance choices, so measure first. Note that the Settler cost of 90 was the owner's own earlier decision, per `CLAUDE.md`.
8. **Measure:** add the walkthrough's confusing points as a tester checklist, and include turn of first tech, second city and first contact in the earlier report's automatic diary.

### Gaps
- No human observed. Any of these points may be trivial, or worse, in play. Real new players may also hit problems invisible in code, such as map reading on the first screen, where cloud bank and hex land dominate.
- Marked VERIFY: the F key, Settler auto-selection, tutorial text fit, and starting technologies with their effect on Worker jobs.
- Screenshots came from the auto-shot run, which does not show the tutorial panel, so overlap with other widgets was judged from rectangles only: panel spans x 392 to 1032, the research card ends at 380, and notices start at 1040, so none overlap.
