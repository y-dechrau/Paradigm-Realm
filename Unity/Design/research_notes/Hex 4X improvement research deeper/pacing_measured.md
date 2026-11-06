# Late game on hex maps, measured, and exact game-speed data from inspectable 4X sources

Date: 6 Nov 2026. No file under `Unity/` other than this note was touched.

**What the measurements are.** Headless .NET harness runs of the game's engine source, driven only by the rival AI. They are not Unity playtesting, nobody played them, and they show timing, not fun. Harness: `C:/Users/Matth/AppData/Local/Temp/claude/37cdc78b320e14b6/a3b6113e-5299-463a-9abe-c0038f5399dd/scratchpad/pace2/Pace.cs` (raw logs in `results`, `results2` and `results3` beside it; the scratchpad is temporary).

**Labels for outside sources.**
- **[Read]:** the file or page itself was fetched. The fetch tool passes pages through a small model, so check exact figures against the file before reprinting.
- **[Forum]:** a forum post read directly; the poster's claim about a game file was not checked.
- **[Snippet]:** seen only in search results.

## Part 1: what happens late in a hex game

### Takeaway
**Rivals now win by Transcendence at about turn 300 (269-325 across 14 distinct games), not 222-250, and no rival completed era 5 or era 6 in any of those games.** The delay does not come from the hex grid. It comes from rivals researching nearly all minor technologies first. Rival activity also stops long before the end: no wars, no captures, no new cities after about turn 100, and armies that are still mostly Warriors at turn 300.

### Cited findings

**Set-up.** Every result in this part comes from the harness above, reading [`Unity/Assets/Scripts`](file:///C:/Users/Matth/Desktop/Paradigm%20Realm%20Work%20File/.Versions/Version%200.20%20-%200.40/Paradigm-Realm-main-0.27/Unity/Assets/Scripts).
- Games use `World.New(seed,"Pace",land,1,1,0,size,2,4,hex:true,peoplesPerWorld:2)`, which gives 14 nations: the stand-in, 3 home rivals and 10 sealed natives. Gates are locked. Maximum turn 450.
- **Stand-in player, as requested:** nation 0 founds one city and then does nothing. Each turn it is given the technology of the second-best surviving home rival, and its own research is switched off.
- **Runs in addition to the base set:**
  - No copying of technology: identical end turns and winners in all 6 games. Era turns matched in every rival; one stage date moved by a turn.
  - Stand-in forced out of war each turn: identical end turns in all 8 games.
  - So the stand-in did not move these results.
- Games were deterministic for a seed: repeat runs ended on the same turn with the same winner.

**Outcome by game (default rules, hex).** Every game ended in the same way, with a rival Transcendence and "Defeat" for the player. There were no conquest or Concord endings.

| Map | Land | Seed | End turn | Winner reached Computing | Stage 1 / 2 / 3 built | Turns from Computing to win | Technologies left unresearched by winner |
|---|---|---|---|---|---|---|---|
| Small | Continents | 101 | 308 | 276 | 277 / 288 / 299 | 32 | 49 |
| Small | Continents | 102 | 304 | 272 | 273 / 284 / 295 | 32 | 48 |
| Small | Continents | 103 | 307 | 272 | 274 / 286 / 298 | 35 | 46 |
| Small | Continents | 104 | 295 | 260 | 262 / 274 / 286 | 35 | 46 |
| Medium | Continents | 101 | 269 | 234 | 236 / 248 / 260 | 35 | 49 |
| Medium | Continents | 102 | 289 | 251 | 254 / 267 / 280 | 38 | 46 |
| Medium | Continents | 103 | 294 | 259 | 261 / 273 / 285 | 35 | 45 |
| Medium | Continents | 104 | 297 | 261 | 263 / 275 / 288 | 36 | 46 |
| Small | One landmass | 201 | 322 | 289 | 290 / 301 / 313 | 33 | 49 |
| Small | Archipelago | 201 | 269 | 237 | 238 / 249 / 260 | 32 | 49 |
| Small | Small continents | 201 | 271 | 236 | 238 / 250 / 262 | 35 | 46 |
| Medium | One landmass | 201 | 306 | 270 | 273 / 285 / 297 | 36 | 48 |
| Medium | Archipelago | 201 | 315 | 278 | 280 / 293 / 306 | 37 | 47 |
| Medium | Small continents | 201 | 325 | 292 | 293 / 304 / 316 | 33 | 52 |

Mean end turn: Small on Continents 304, Medium on Continents 287, other land types 301.

**Era completion: mean turn and range (a rival completes an era with 4 of its 6 keystones).**

| Set | Rivals | Era 1 | Era 2 | Era 3 | Era 4 | Era 5 | Era 6 | Game end |
|---|---|---|---|---|---|---|---|---|
| Design targets in the `World.cs` comment | | 40 | 80 | 125 | 170 | 210 | not stated | about 230 |
| Hex Small, Continents | 12 | 45 (39-61) | 101 (92-115) | 161 (151-169) | 229 (207-249) | **none** | **none** | 304 (295-308) |
| Hex Medium, Continents | 12 | 42 (34-57) | 100 (88-111) | 162 (137-179) | 228 (180-273) | **none** | **none** | 287 (269-297) |
| Hex, other land types | 18 | 39 (30-53) | 96 (85-107) | 158 (138-172) | 225 (182-256) | **none** | **none** | 301 (269-325) |
| Square grid, same seeds 101-103 | 18 | 46 (32-99) | 102 (87-143) | 166 (142-228) | 231 (185-329) | **none** | **none** | 297 (270-339) |
| Hex, keystones first (variant) | 24 | 44 (34-61) | 75 (62-89) | 117 (94-133) | 181 (144-203) | 240 (212-259), 7 of 24 rivals | **none** | 244 (215-263) |
| Hex, stage 3 held back, default research | 18 | 42 | 99 | 159 | 227 | 358 (301-424) | 381 (341-429), 13 of 18 | ran to 450 |
| Hex, stage 3 held back, keystones first | 24 | 44 | 75 | 117 | 181 | 259 (212-299) | 350 (304-409) | ran to 450 |

The two "held back" rows are experiments, not the game: the harness stops stage 3 from finishing, to show when eras 5 and 6 would arrive. "Keystones first" means the harness makes all three home rivals research keystones only, cheapest first, as a focused player might.

**Why the game now runs late.**
- **Not the hex grid.** On the same seeds, square-grid games ended at 270/275/310 on Small (hex 308/304/307) and 339/299/288 on Medium (hex 269/289/294). Square was earlier on Small (mean 285 against 306) and later on Medium (309 against 284), so there is no consistent direction. The one Medium square game with a crippled rival (1 city, 33 technologies) shows that seeds vary a lot.
- **Minor technologies.**
  - Winners in the default games know all 24 minors of each of eras 1-4 before going far into era 5 (known at end: `6/24 6/24 6/24 6/24 2-3/9-12`, then `0/0` in era 6).
  - `PreferredResearch` counts a minor at 3 times its cost, but an old cheap minor still beats Computing (cost 2205).
  - On the same 8 seeds, keystones-first ended on average 52 turns earlier: 308 to 240, 304 to 247, 307 to 256, 295 to 254, 269 to 215, 289 to 238, 294 to 236, 297 to 263.
  - The keystones-first eras (44/75/117/181) are close to the design targets (40/80/125/170) and its ending (215-263) overlaps the old 222-250. The old figure dates from 30 Oct and the 144 minors were added on 31 Oct (`CLAUDE.md`).
  - Old code was not re-run, so the link to 31 Oct is an inference. The 52-turn gap is measured.

**The race does not need eras 5 or 6.** The project needs only Computing, whose only prerequisite is Electricity (`TechnologyCatalog.cs`), plus a city of size 8 (`Victory.cs`).
- Every default win came 32-38 turns after Computing: 30 turns of activation plus 1-3 turns to build each stage.
- The first stage was finished 1-3 turns after Computing. The stages cost 300/400/500 shields, and the winner's best city was making 124-494 shields a turn at that moment.
- So **the shield costs hardly matter, and the win is a fixed countdown.**
- Keystones-first winners held 27-28 of 180 technologies when they won.

**Era 5 and 6 content** (dumped from the catalogue with `Pace.dll dump`).

| Era | Cost of all 6 keystones + 24 minors | Cheapest 4 keystones | New units | New buildings | Keystones with no benefit listed |
|---|---|---|---|---|---|
| 4 Industrial | 9,360 + 14,592 | 5,920 | Rifleman, Medic | Hospital, Factory, Sewer system | Flight |
| 5 Modern | 14,805 + 23,184 | 9,450 | **none** | Transcendence stages only | Nuclear Fission, Robotics, Genetic Engineering, Globalisation |
| 6 Dimensional | 20,790 + 32,640 | 13,356 | **none** | **none** | Fusion Power |

The other era 5 and 6 keystones do something: Computing (project, +10% science), Satellites (reveal map), Nanotechnology (+15% production), Quantum Computing (+20% science), Longevity Medicine (+2 content), Mind Uploading (no food use), Transcendence (activation 5 turns instead of 10). Eras 5 and 6 are 70% of all research cost (91,419 of 129,468, a sum made from this table and the earlier eras).

**Per rival, default rules, hex, Continents.** `*` marks the winner. Eras are completion turns for eras 1-4; eras 5 and 6 were never completed, so they are omitted. Technologies are out of 180.

| Game (map, seed, end) | Rival | Eras 1-4 | Technologies at 50 / 100 / 150 / 200 / 250 / end | Cities / army at turn 100 and at end | Science / shields at end | Gold at end | Project stage at end | Share of city-turns with no building under way |
|---|---|---|---|---|---|---|---|---|
| Small 101, 308 | Steppe Horde | 45, 93, 168, 249 | 5 / 34 / 62 / 78 / 94 / 112 | 5 / 13 | 204 / 860 | 5,167 | 0 | 77% |
| | Hellenic League | 39, 92, 167, 245 | 5 / 34 / 62 / 78 / 94 / 113 | 4 / 13 | 246 / 533 | 4,885 | 0 | 76% |
| | Northmarch Kingdom* | 42, 98, 151, 220 | 5 / 35 / 64 / 90 / 107 / 131 | 6 / 16 | 591 / 1,237 | 11,973 | 3 | 69% |
| Small 102, 304 | Lionmarch Kingdom* | 42, 102, 158, 219 | 5 / 34 / 63 / 88 / 108 / 132 | 6 / 14 | 688 / 1,225 | 14,639 | 3 | 61% |
| | Sunspire Emirate | 43, 101, 161, 239 | 5 / 34 / 62 / 82 / 97 / 115 | 5 / 11 | 286 / 914 | 9,323 | 0 | 74% |
| | Steppe Horde | 47, 107, 163, 238 | 4 / 33 / 62 / 82 / 97 / 115 | 5 / 13 | 295 / 864 | 5,413 | 0 | 71% |
| Small 103, 307 | Hellenic League | 40, 95, 157, 239 | 5 / 37 / 63 / 83 / 96 / 113 | 5 / 13 | 218 / 1,120 | 9,159 | 0 | 77% |
| | Oasis Sultanate* | 48, 107, 163, 218 | 4 / 32 / 62 / 88 / 109 / 134 | 6 / 13 | 683 / 868 | 18,133 | 3 | 66% |
| | Rising Isles | 45, 97, 163, 229 | 5 / 34 / 62 / 83 / 103 / 129 | 6 / 12 | 664 / 967 | 8,834 | 2 | 74% |
| Small 104, 295 | Holy Imperium | 52, 105, 169, 226 | 3 / 33 / 62 / 80 / 105 / 126 | 6 / 15 | 747 / 811 | 6,783 | 2 | 68% |
| | Sahel Empire* | 61, 115, 163, 207 | 3 / 31 / 62 / 92 / 114 / 134 | 6 / 14 | 680 / 537 | 19,256 | 3 | 57% |
| | Frostpeak Realm | 42, 101, 158, 219 | 5 / 34 / 63 / 87 / 108 / 128 | 5 / 13 | 695 / 818 | 13,158 | 2 | 65% |
| Medium 101, 269 | Steppe Horde | 57, 111, 173, 246 | 2 / 30 / 58 / 76 / 94 / 104 | 5 / 13 | 261 / 860 | 5,471 | 0 | 69% |
| | Hellenic League* | 36, 88, 137, 180 | 6 / 40 / 74 / 103 / 124 / 131 | 6 / 13 | 546 / 873 | 10,601 | 3 | 61% |
| | Northmarch Kingdom | 37, 93, 153, 228 | 7 / 38 / 64 / 86 / 103 / 113 | 6 / 12 | 365 / 960 | 6,971 | 0 | 75% |
| Medium 102, 289 | Lionmarch Kingdom | 34, 97, 155, 220 | 5 / 36 / 63 / 87 / 107 / 123 | 6 / 13 | 532 / 794 | 11,814 | 1 | 67% |
| | Sunspire Emirate | 40, 101, 165, 240 | 5 / 34 / 62 / 80 / 97 / 117 | 5 / 12 | 389 / 1,035 | 10,134 | 0 | 74% |
| | Steppe Horde* | 43, 98, 156, 198 | 5 / 34 / 63 / 94 / 117 / 134 | 5 / 11 | 690 / 326 | 13,506 | 3 | 54% |
| Medium 103, 294 | Hellenic League | 36, 96, 151, 227 | 5 / 37 / 64 / 87 / 100 / 115 | 5 / 11 at turn 100, 6 / 12 at end | 276 / 1,495 | 9,415 | 0 | 74% |
| | Oasis Sultanate | 43, 108, 175, 252 | 5 / 33 / 62 / 75 / 93 / 112 | 5 / 11 | 321 / 917 | 9,417 | 0 | 79% |
| | Rising Isles* | 49, 107, 162, 207 | 4 / 32 / 62 / 92 / 114 / 135 | 6 / 12 | 912 / 778 | 14,479 | 3 | 62% |
| Medium 104, 297 | Holy Imperium* | 48, 110, 164, 208 | 4 / 33 / 62 / 92 / 114 / 134 | 6 / 13 | 799 / 884 | 10,211 | 3 | 52% |
| | Sahel Empire | 47, 105, 175, 258 | 4 / 33 / 62 / 73 / 92 / 105 | 5 / 11 | 235 / 1,186 | 9,598 | 0 | 77% |
| | Frostpeak Realm | 43, 95, 179, 273 | 5 / 34 / 61 / 69 / 91 / 104 | 5 / 13 | 312 / 680 | 4,427 | 0 | 74% |

"Army" counts every unit with attack strength. Natives in these 8 games complete eras 1-4 at means of 36-40, 98-110, 154-170 and 217-242. All 10 survived in every game, and they cannot build Transcendence while the gates are locked.

**Stalls (the 8 instrumented default games).**
- No war at all: `warTurns=0` for all 24 home rivals. No city changed hands in any of the 36 instrumented games (base, square, no-copy and keystones-first).
- Last new city: turn 43-102. Last new building: turn 176-265.
- Armies do not modernise. At the end each rival has 9-16 Warriors, a few Spearmen or Horsemen at most, and no Knights or Riflemen. The AI stops ordering units once it has 14 of any kind (`own.Count>=14` in `World.AI`), so workers and boats use up the same allowance.
- Gold piles up: 4,427 to 19,256 at the end.
- No rival went without a research choice in the default games, and research never stopped. The longest gap between technologies was 14-33 turns.
- No project was lost, because nobody attacked.
- The turn loop itself did not stall: about 0.2-0.7 s a turn late, with 10-16 games running at once, so this is not a clean timing figure. None of the 64 games run crashed, `World.Valid()` held at the end of all of them, and no nation was eliminated.

**Technologies left at the natural end:** winners 45-52 of 180; losing rivals up to 76 (104 known). With stage 3 held back, 10 of 18 rivals reach all 180, at turns 386-437, and then sit with no research for up to 65 turns. The other 8 hold 149-172 at turn 450.

### Inferences
- **Are eras 5 and 6 ever seen?** Era 5 completes only if the rival goes keystones-first, and then only a few turns before the win (212-259, in 7 of 24 rivals). Era 6 is never completed before a win in any run. Individual era 5 technologies are seen: the winner normally gets Computing and Satellites, and sometimes Nuclear Fission. A focused human could reach further, but this harness cannot say how far.
- **Does the game end on a boring note? For rival behaviour, yes.** From Computing onwards (turns 234-292) the world shows a fixed 30-odd turn countdown by a rival with a Warrior garrison, while nothing else happens. The three warnings exist (`CompleteProjectStage`), and whether a human finds this tense or dull cannot be measured here. A human with Knights could probably take a Warrior-held project city, which would stop the countdown. That is untested.
- **A focused player can get well ahead of the rivals.** A keystones-only route reached the win about 52 turns sooner with 150 technologies skipped, so the 144 minors look optional. A human also manages cities better than this AI, so 215-263 may not be the floor.
- **The late game does more to the clock than to the empire.** Empires are frozen by about turn 100-200 in cities and armies, and the last 100 turns add research and gold only.

### Gaps
- No human or scripted-competent player was measured. An idle stand-in removes every player effect: war with the player, contact, deals, Concord and a player's Transcendence.
- Contact between rivals was not logged, so "no war" could partly reflect rivals not meeting. It also could reflect friendly diplomacy. Not separated.
- Only Normal speed and difficulty 1, Small and Medium, with `peoplesPerWorld:2`. Large maps and the default of 10 peoples per world were not run.
- The old diagonal-map 222-250 games were not re-run.
- This is the harness, not Unity: no rendering, and turn times are not Unity times.

## Part 2: exact speed and era numbers

### Takeaway
Civ IV (Beyond the Sword), Civ V and Civ VI all share a 500-turn Standard game, scaling with speed in about the same ratio as costs. Old World has no turn cap (correcting the earlier note), and Civ VII's three Ages at 150-200 turns are the only official age figures found. Of 500 Standard turns, the last 200 cover under 200 years in those games.

### Cited findings

| Game | Quick | Standard | Epic | Marathon | Other | Label |
|---|---|---|---|---|---|---|
| Civ VI, turns ([GameSpeeds.xml mirror](https://raw.githubusercontent.com/ernsnl/Civilization6Mods/master/Lightning%20Snail%20Fast%20Mod/Original/Base/Assets/Gameplay/Data/GameSpeeds.xml)) | 330 | 500 | 750 | 1500 | Online 250 | [Read], third-party mirror, base file; sums agree with an install reading in [CIVVIS PR 4375](https://github.com/MartinHalvorson/CIVVIS/pull/4375) |
| Civ VI, CostMultiplier (same file) | 67 | 100 | 150 | 300 | Online 50 | [Read] |
| Unciv, Civ V-derived ruleset, turns ([Speeds.json](https://raw.githubusercontent.com/yairm210/Unciv/master/android/assets/jsons/Civ%20V%20-%20Gods%20%26%20Kings/Speeds.json)) | 330 | 500 | 750 | 1500 | | [Read]; open source, not Firaxis data |
| Unciv, modifier on production, gold, science, culture, faith and build time (same file) | 0.67 | 1.0 | 1.5 | 3.0 | | [Read] |
| Civ V stock, turns ([Spatzimaus, CivFanatics](https://forums.civfanatics.com/threads/editing-number-of-max-turns.398860/latest)) | 350 | 500 | 750 | 1500 | Speed names inferred from the order | [Forum]; Quick conflicts with Unciv's 330; the stock XML was not read |
| Civ IV Beyond the Sword, Normal ([post #9, CivFanatics](https://forums.civfanatics.com/threads/number-of-years-per-turn.202516/post-4996799)) | | 500 | | | | [Forum] |
| Civ IV Marathon ([Modding Wiki example](https://modiki.civfanatics.com/index.php/Civ4GameSpeedInfo)) | | | | 1500 by summing | iResearchPercent, iConstructPercent, iCreatePercent, iGrowthPercent 300; iTrainPercent 200; iVictoryDelayPercent 300 | [Read]; the page calls it an example, not stock. Its turn rows match the Civ VI Marathon rows exactly |
| Civ IV percents ([King Jason, CivFanatics](https://forums.civfanatics.com/threads/modding-the-aspects-of-game-speed.134109/)) | "fast" 67 | 100 | 150 | | | [Forum] |
| Civ IV original release, turns ([Ori, post #5](https://forums.civfanatics.com/threads/number-of-years-per-turn.202516/post-4996799)) | 320 | 460 | 660 | 1200 | Sums made here from his tables; later posters say these fit the original release, not Beyond the Sword | [Forum] |

**Calendar shape, Standard speed.**
- Civ VI file, months per turn for that many turns: 480 for 75, 300 for 60, 240 for 25, 120 for 50, 60 for 60, 24 for 50, 12 for 120, 6 for 60 ([Read](https://raw.githubusercontent.com/ernsnl/Civilization6Mods/master/Lightning%20Snail%20Fast%20Mod/Original/Base/Assets/Gameplay/Data/GameSpeeds.xml)).
- Unciv Standard, years per turn until that turn: 40 to 75, 25 to 135, 20 to 160, 10 to 210, 5 to 270, 2 to 320, 1 to 440, 0.5 to 500 ([Read](https://raw.githubusercontent.com/yairm210/Unciv/master/android/assets/jsons/Civ%20V%20-%20Gods%20%26%20Kings/Speeds.json)). This is the same table in years.
- The Civ IV Beyond the Sword forum table has the same breakpoints, with 1800 AD at turn 270 and 1900 AD at turn 320 ([Forum](https://forums.civfanatics.com/threads/number-of-years-per-turn.202516/post-4996799)).

**Era cost shape (Civ V via Unciv).** [Techs.json](https://raw.githubusercontent.com/yairm210/Unciv/master/android/assets/jsons/Civ%20V%20-%20Gods%20%26%20Kings/Techs.json) [Read] gives cost per technology by column: 20 in Ancient, up to 8,800 in Information, and 9,500 for repeatable Future Tech. Multiplying cost by technologies per column gives: Ancient 545, Classical 1,295, Medieval 4,010, Renaissance 9,650, Industrial 19,000, Modern 31,900, Atomic 46,000, Information 90,200. Atomic and Information together are 67% of the 202,600 total (my arithmetic). Paradigm Realm's eras 5 and 6 are 70%, so its cost curve is not unusual.

**Civ VII.**
- "you can expect an Age to last around 150 to 200 turns on standard speed"; each turn adds a small amount of Age Progress, and milestones add more ([2K dev diary, Read](https://civilization.2k.com/civ-vii/game-guide/dev-diary/ages)).
- CostMultiplier 300 / 150 / 100 for Marathon / Epic / Standard, and Marathon Antiquity at 120 months for 280 turns. A poster estimates that a Standard Age ends at about 200 turns if nobody acts, from an Age goal of 200 points, and 240 for Long Ages ([Forum](https://forums.civfanatics.com/threads/deciphering-the-game-speeds-xml-file.694950)).

**Old World.**
- Mohawk_Solver, developer, 1 Dec 2024: "There is no 200 turn limit." A soft limit existed and was turned off. Games "probably averages about 120 turns" and "most games will end before 150." Turn scale changes how long leaders live, not build or research time ([Steam thread, Read](https://steamcommunity.com/app/597180/discussions/0/4625855867810482986)).
- In the same thread a player, not a developer, lists a Time victory for being the leader at 200 turns.
- This corrects the earlier note, which took "capped at 200 turns" from [PCGamesN](https://www.pcgamesn.com/old-world/civilization-too-long).

**Freeciv (civ2civ3).**
- Starts at -4000. Years per turn: 50 base, cut to 25 from -1000, 20 from 0, 10 from 1000, 5 from 1500, 2 from 1750, and 1 from 1900 ([effects.ruleset, Read](https://raw.githubusercontent.com/freeciv/freeciv/main/data/civ2civ3/effects.ruleset); start year in [game.ruleset, Read](https://raw.githubusercontent.com/freeciv/freeciv/main/data/civ2civ3/game.ruleset)). Space-race technologies also slow the calendar (`Slow_Down_Timeline`).
- `tech_cost_style = "Linear"`, base_tech_cost * reqs, base 10, where reqs counts prerequisites recursively ([game.ruleset, Read](https://raw.githubusercontent.com/freeciv/freeciv/main/data/civ2civ3/game.ruleset)).
- Default end turn 5000, sciencebox default 100 ([game.h, Read](https://raw.githubusercontent.com/freeciv/freeciv/main/common/game.h)).

**Paradigm Realm's own calendar** (`World.Calendar`, read): 40 years a turn for 100 turns, then 10 years for 200 turns, then 1 year. Turn 300 therefore falls at about 1990 CE by that formula.

### Inferences
- By that calendar a natural end near turn 300 lands at about the present day, so the current finish fits the calendar, while a turn-240 finish stops at about 1390 CE.
- About 300 turns is 60% of a Civ Standard limit, and 2.5 times Old World's reported 120-turn average. It is also about 1.5-2 Civ VII Ages. The limit figures are ceilings, not typical finishes, so they do not show how long players' games actually run.
- Turn totals and costs scale together across speeds in Civ VI (250/330/500/750/1500 against 50/67/100/150/300). A designer can treat them as one dial.

### Gaps
- **Stock Civ V Civ5GameSpeeds.xml and stock Civ IV Beyond the Sword CIV4GameSpeedInfo.xml were not read directly.** Fandom pages returned HTTP 402 and no plain mirror was found. A local game install would settle it. Civ IV Quick and Epic turn rows for Beyond the Sword remain unverified.
- The Dawn of Civilization mod file [was read](https://raw.githubusercontent.com/dguenms/Dawn-of-Civilization/develop/Assets/XML/GameInfo/CIV4GameSpeedInfo.xml) but is a mod, so it is not used for stock values.
- No official turn-by-turn table of when eras are reached in any Civ game was found. Only limits, costs and calendars are inspectable.
- Civ VI's turn limits are from a third-party copy of the file, not Firaxis pages.
- Whether any of these games' players ever see their last era was not found as a figure.

## Part 3: what the data suggests about eras 5 and 6 (judgement, not measurement)

### Takeaway
**My judgement: keep six eras, and make the ending pass through them, but the first move is smaller. Fix the rival research weighting and give eras 5 and 6 a purpose, then measure again.** As they stand, eras 5 and 6 are 70% of the research cost, unlocked no units and almost no buildings, and do not affect who wins.

### Cited findings
All judgement below rests on the tables in Parts 1 and 2 of this note. No outside source tests any of these options. The earlier round's [pacing notes](file:///C:/Users/Matth/Desktop/Paradigm%20Realm%20Work%20File/.Versions/Version%200.20%20-%200.40/Paradigm-Realm-main-0.27/Unity/Design/research_notes/Hex%204X%20improvement%20research/pacing.md) give the designers' wider arguments.

### Inferences
Each item is a judgement. Effects are predictions unless a measured figure is quoted.

1. **Decide the target turn first.** The owner's design says about 230; rivals now reach about 300. A turn-300 end fits the calendar and a 180-technology tree. A turn-230 end fits Old World-style brevity. Nothing here says which is more fun. I lean to about 280-300 because that is where the calendar and the tree already agree, and it needs no re-pacing of the early game.
2. **Gate the project on the eras (small change, largest effect on the problem).** For example stage 1 at Computing, stage 2 on completing era 5, and stage 3 on an era 6 keystone such as Mind Uploading.
   - Today's pace is too slow for this: measured keystones-first era 5 at about 259 and era 6 at about 350; default research era 5 at about 358 and era 6 at about 381.
   - So `ResearchPace` for those eras (420 now) would need cutting. A rough estimate from those gaps is by a third to a half for an end near 300. This must be measured, not trusted.
3. **Lower the rival's appetite for old minors.** The current weighting makes rivals finish nearly all 96 minors of eras 1-4 before Computing. A higher weight on minors would bring rival eras closer to the 40/80/125/170 targets, about 52 turns earlier at most, as the keystones-first runs imply. This is the cheapest single change, and it is independent of item 2.
4. **Give eras 5 and 6 content, or cut them.** Five of the 12 keystones in those two eras show no benefit, and neither era adds a unit. Even if the race is gated, players would reach two eras with nothing to build. Candidates: late units so that armies change, and a building or two per era.
   - If that content will not be made, the honest alternative is to merge them into one short final era of the project itself, and to drop `ResearchPace` for it. I would choose this only if art and writing time is short.
5. **Fix the Warrior armies regardless (separate from eras).** A unit cap of 14 that includes workers and no upgrade path leaves rivals with Warrior garrisons at turn 300. That undercuts every late-game threat, including defending or attacking the project city, and makes late unit technologies irrelevant to the AI.
6. **Keep shield costs meaningful, or accept the countdown.** The 300/400/500 stages take 1-3 turns once a rival is ready. Scaling cost to the empire's output, or requiring more than one city's work, would add a building phase. A plain 30-turn countdown with warnings may still be the better ending, so test with a human first.
7. **Pair this with the early-finish idea from the earlier report (item 5).** A rival that starts stage 1 has 30-odd known turns left, which is a natural moment for it.

### Gaps
- None of these options was tried. Item 3 is supported by a measured variant, but that variant is an extreme (keystones only), not a re-weighted AI.
- Human late-game pace was not measured. A hand-played late game or a scripted competent player is the next most useful test, and it would show whether a person sees eras 5 and 6 at all.
