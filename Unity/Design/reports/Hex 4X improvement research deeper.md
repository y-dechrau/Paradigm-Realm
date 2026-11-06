# Make the Late Game Actually Happen

The most useful finding is about your own game: in 14 measured hex games, a rival won by Transcendence at about turn 300 (269-325), no rival completed era 5 or 6, and the 8 games logged for war had no wars at all. Empires stop changing long before the end: no new cities after about turn 100, armies still mostly Warriors at turn 300, and thousands of unspent gold. So the best next steps are cheap repairs to rival behaviour, a decision from you on what the last two eras are for, and one full game played by hand, not more big systems. After that come the gaps a new player meets first: difficulty that does nothing, remembered land that merges with the sea, a silent game and tutorial text with mistakes in it. Event cards are still the first large addition I would make, but the case for them is weaker than the last report claimed, because it rests mainly on one designer and on a game built around characters and families you do not have. This report replaces the first one and applies its fact-check. Nothing here has been played by a human, every proposal is an untested suggestion, and research ran well short of the three hours you asked for: ten tasks on 6 Nov 2026, each about 6 to 17 minutes, side by side.

## The ranked list, in plain language

The order weighs how much better the game gets against the size of the job. **Job** is my estimate of building work, and it leaves out your play-testing and tuning: Small is under half a day, Medium is 1-3 days, Large is 1-2 weeks or more. **Evidence** has two grades, because the fact-check found the first report blurred them. The first says how sure we are the problem is real, and the second how sure we are this fix helps. Strong means a direct measurement of this game, a read of its code, or an official guideline. Moderate means one good source on one game, or several weaker ones. Weak means opinion, forum posts or my judgement. No source anywhere shows that one of these mechanics makes players happier or keeps them longer, so even a Moderate fix grade means "documented", not "proven".

### Do these first

| # | What to do | Why it helps | Payoff | Job | Evidence (problem / fix) |
|---|---|---|---|---|---|
| 1 | **Play one full hex game by hand**, with an automatic diary (seconds and clicks per turn, when each era ends, first technology, second city, first contact). Have someone else try the first hour too | Every balance number so far comes from computer rivals, which can time a game but cannot feel bored. Developers also stop seeing their own blind spots | High | Small, plus your time | Strong / Moderate |
| 2 | **Modernise rival armies**: count only soldiers towards the unit limit, scale it with cities, let rivals upgrade and spend gold | Rivals sit on 4,427-19,256 gold behind Warriors at turn 300, so no late threat is believable | High | Small | Strong / Moderate |
| 3 | **Make rival war real**: first log who has met whom and why war was not declared; then attack only when the exact combat preview favours it, retreat when hurt, and stop peace coming on a timer | No war in any logged default game. War is the clearest weakness in the rival's code | High | Small to log, Medium to fix | Strong / Moderate |
| 4 | **Warn about any rival's Transcendence, met or not** | Today an unmet rival's progress shows only as a line on the Victory screen, so a 300-turn game can end in a near-surprise | Medium | Small | Strong / Moderate |
| 5 | **Correct tutorial and help text** (the wrong F key, an out-of-date F1 page, "0 queued" above a queued item) | These are simple errors at the moment a new player most needs trust | Medium | Small | Strong / Weak |
| 6 | **Stop rivals researching nearly every old minor technology first** (after decision 1 below) | Rivals focused on keystones won about 52 turns sooner and came close to the era targets. This is the measured cause of the delay | High | Small | Strong / Moderate |
| 7 | **Try a lighter remembered land**: faded and cooler, with a thinner mist (decision 3) | Remembered land, sea and cloud nearly merge on screen. My one look at the test picture supports this change most of all the art proposals | High | Small to try, Medium to finish | Moderate / Weak |
| 8 | **Make difficulty real**, starting with a control on New Game and three rows of the table below | Nothing reads the stored setting, and New Game has no control for it | High | Medium | Strong / Moderate |
| 9 | **Give the ending a route through the eras**: tie project stages to eras 5 and 6, and cut research cost there (decision 2) | Eras 5 and 6 hold about 70% of all research cost and have no effect on who wins | High | Small to change, Medium to tune | Strong / Weak |
| 10 | **Add sound effects and volume sliders**, using free CC0 packs | The game is silent, which anyone trying it will notice at once. Size of the benefit is unknown | High | Small to Medium | Strong / Weak |

### Do these next

| # | What to do | Why it helps | Payoff | Job | Evidence (problem / fix) |
|---|---|---|---|---|---|
| 11 | **End Turn shows what needs you**, one item at a time, with Shift+Enter always ending the turn | End Turn checks nothing today. One public game (Unciv) shows a clean way to do this | Medium | Small to Medium | Strong / Moderate |
| 12 | **Tutorial steps fire when they apply**, the guide stays visible on city and research screens, and a one-line current goal stays up | Six text steps can all be clicked through on turn 2, then nothing guides turns 3-20 | Medium | Small to Medium | Strong / Moderate |
| 13 | **Fill eras 5 and 6**: late units and a building or two per era, or merge them into one short era (decision 2) | Neither era adds a unit; five of their 12 keystones show no benefit | Medium to High | Medium to Large | Strong / Weak |
| 14 | **Calm the sea and foam** | Open sea changes 13-17% of its pixels at each step, and foam re-rolls about a third of its pixels | Medium | Small to Medium | Moderate / Weak |
| 15 | **Event cards**: 15 cards as a measured prototype, for the human player only | The first big addition worth trying. Its cost was understated last time, and much of Old World's version acts on characters you lack | High if it works | Medium for the system; Large with writing and tuning | Moderate / Moderate |
| 16 | **Offer an early finish** when one side is far ahead | Avoids pressing End Turn long after a game is settled. Needs a new overall score first | Medium | Medium | Moderate / Moderate |
| 17 | **Add 6-10 music tracks** | Same gap as effects, but you must listen and choose, and credits are needed | Medium | Medium, plus your time | Strong / Weak |
| 18 | **Remove pixel jitter from ground and adopt new colour ramps** | About a third of ground pixels stand alone, whichever palette is used. But the first prototype looks blocky to me, so it needs retuning | Medium | Medium | Moderate / Weak |
| 19 | **Give rivals a small per-game personality** | Agendas change opinions but not what rivals build or research. This needs no save change | Medium | Small to Medium | Strong / Weak |
| 20 | **Explain losses and rival attitudes on screen**, with named reasons | Designers repeatedly report that opaque behaviour reads as noise or cheating | Medium | Small to Medium | Moderate / Moderate |
| 21 | **Compress save files**, after timing the save in Unity | An early save shrinks about 84 times. Shorter autosave gaps remain unproven, because the slow part may be producing the text | Medium for disk, unknown for speed | Small | Strong / Weak |
| 22 | **Restart in the first five turns; later, restart this turn** | Cheap protection against a bad start or a misclick | Medium | Small, then Small to Medium | Moderate / Moderate |

### Only after a hand-played game

| # | What to do | Why it helps | Payoff | Job | Evidence (problem / fix) |
|---|---|---|---|---|---|
| 23 | **Raiders from camps on unclaimed land**, with an off setting. Needs item 3 first, or rivals cannot cope | No early danger today | High early | Medium, more with rival handling | Strong / Moderate |
| 24 | **A ladder for tribal villages**: gift or quest, then ally, levy or absorb | Villages only join and pay tax today | Medium to High | Medium | Strong / Moderate |
| 25 | **Larger text, key remapping, owners readable without colour** | Likely below Xbox's text size at 1080p; no remapping; more civilisations than safe colours. Raising the smallest fonts alone is Small | Medium | Medium each | Strong / Moderate |
| 26 | **Personal goals, great people, undo for moves** | Each gives something to chase or forgives a mistake. Support is thin or from one game | Medium | Small to Medium each | Weak / Weak |
| 27 | **Rival aggression dial, drawn-card minor technologies, charging for citizen changes** | Ideas from Firaxis and Johnson. Try only if the diary shows long calm or tedious micromanagement | Unknown | Medium | Weak / Weak |
| 28 | **Internal factions; a mid-game crisis** | Deepest fixes, largest cost, mixed reception elsewhere | Unknown | Large | Moderate / Weak |

### Do not do these

Do not make Old World's Orders system, edge rivers or a rewrite of map drawing your next job: each touches rules, rivals, saves or art, and no measurement yet justifies it. Do not add auto-end-turn, real-time music layers or any assets with non-commercial or share-alike licences. Do not give rivals hidden cheats, such as sight through fog, free units during play or discounts on wonders and Transcendence. I would also avoid forced changes of civilisation, since Civ VII's update 1.4.0 made swapping optional ([2K](https://civilization.2k.com/civ-vii/game-update-notes/2026-may-19-patch-1-4-0/)) and Take-Two's chief executive conceded it had pushed too far, though he named no mechanic ([Aftermath](https://aftermath.site/civilization-vii-civ-firaxis-update.md)). Era resets themselves are less clear than the last report said: a GamesRadar headline has Firaxis's director saying Ages do address players not finishing, but only the headline could be read ([GamesRadar](https://www.gamesradar.com/games/strategy/civ-7-director-thought-the-new-ages-system-might-not-work-but-says-it-does-fix-the-number-one-issue-players-not-getting-to-the-end-of-their-games)).

## Six decisions only you can make

No research can settle these, because they depend on the game you want. The "my lean" column is an opinion to argue with.

| # | Decision | Why it matters | Options | My lean |
|---|---|---|---|---|
| 1 | **Target game length** | The design says about 230 turns, while rivals now reach about 300. Items 6 and 9 pull in opposite directions until this is set | About 230: brisk, fewer eras seen. About 280-300: fits the calendar and the 180-technology tree. Much shorter: needs chores removed, not just a cap | 280-300, because calendar and tree already agree there and the early game need not be re-paced. Nothing shows it is more fun |
| 2 | **What eras 5 and 6 are for** | They hold about 70% of research cost, and nobody needs them to win | (a) The ending passes through them, with new content. (b) Merge them into one short final era built around the project. (c) Keep them as optional late depth for players who stay | (a) if you can fund the art and writing; otherwise (b). Option (c) is what you have now |
| 3 | **Should remembered land stay dark?** | The dark mist was your own request (30 Oct; the code comment says so). The first report's notes also mention a "reads dark" complaint, but the fact-check could not find its source, so please say which you want | Keep as is. Keep a dark mist but thin it and fade the land (proposed). A different look, such as Civ VI's parchment map | Thin and fade. It keeps the mood, and you judge it side by side |
| 4 | **Do the gates ever open?** | Fifty sealed peoples exist, and no player can meet them. They also cost time: about 1.5 s a turn with 54 nations late on, and larger saves (about 6, 12 and 20 MB on Tiny, Small and Medium) | Open them late. Open them as a special ending. Never open them, and lower the default peoples per world | Try a scrappy switch before building anything else on them. Press accounts say a late feature forced a major redesign of Shafer's At the Gates ([Wccftech](https://wccftech.com/jon-shafer-reveals-troubles-at-the-gates/amp/)) |
| 5 | **Should rivals start wars with a peaceful player?** | Today war needs a grudge (opinion below -20) and a seen 1.5x strength lead, so a peaceful player may never be attacked | War by grudge only. Rivals also fight for land and to stop a project. Vary by difficulty | Vary by difficulty, and always show the reason |
| 6 | **Art and sound taste, and how public this stays** | Calm ground risks your "painted" look. A darker sea runs against your bright gallery seas. Keeping the repository public rules out several audio sources | Calm versus painted, by side-by-side pictures. Chip sound or a small acoustic group, not both. Public or private repository | Choose the music palette first, then fill it in. Keep only CC0 and CC BY audio while the repository is public |

Two smaller calls also come to you later. One is whether a one-line rule should govern new features; I suggest "nothing new until the last feature has been played by hand", which echoes Unciv's blunt inclusion rule ([Unciv](https://www.github.com/yairm210/Unciv)). The other is the early pace, since a Settler cost of 90 was your own earlier decision and the walkthrough suggests a second city might not arrive until about turn 35.

## Measured games show a fixed countdown over a frozen world

**What was measured.** These are headless runs of the game's own engine source in a .NET harness on 6 Nov 2026, driven by the rival AI, not by Unity. The player is an idle stand-in that founds one city and is given the second-best rival's technology. Repeating the games without that gift, and with the stand-in kept out of war, gave the same end turns, so it did not move these results. Settings were Normal speed, difficulty 1, 3 home rivals, 10 sealed natives, gates locked. All 64 games ran without a crash. The harness and its raw logs are in a temporary folder, `C:/Users/Matth/AppData/Local/Temp/claude/37cdc78b320e14b6/a3b6113e-5299-463a-9abe-c0038f5399dd/scratchpad/pace2/`, so copy them if they should be kept.

**Every game ended the same way.** All 14 distinct hex games ended in rival Transcendence: Small maps on Continents at a mean of turn 304, Medium on Continents at 287, other land types at 301. There was no conquest or Concord ending. The earlier "222-250" figure is out of date for current rules.

| Set | Rivals | Era 1 | Era 2 | Era 3 | Era 4 | Era 5 | Era 6 | Game end |
|---|---|---|---|---|---|---|---|---|
| Design targets | | 40 | 80 | 125 | 170 | 210 | not stated | about 230 |
| Hex Small, Continents | 12 | 45 | 101 | 161 | 229 | **none** | **none** | 304 (295-308) |
| Hex Medium, Continents | 12 | 42 | 100 | 162 | 228 | **none** | **none** | 287 (269-297) |
| Hex, other land types | 18 | 39 | 96 | 158 | 225 | **none** | **none** | 301 (269-325) |
| Square grid, same seeds | 18 | 46 | 102 | 166 | 231 | **none** | **none** | 297 (270-339) |
| Experiment: keystones first | 24 | 44 | 75 | 117 | 181 | 240, 7 of 24 rivals | **none** | 244 (215-263) |
| Experiment: stage 3 held back | 18 | 42 | 99 | 159 | 227 | 358 (301-424) | 381 (341-429), 13 of 18 | ran to 450 |
| Experiment: both of the above | 24 | 44 | 75 | 117 | 181 | 259 (212-299) | 350 (304-409) | ran to 450 |

The two "held back" rows are experiments that stop the last stage finishing, to show when late eras would otherwise arrive.

**The delay comes from minor technologies, not hexes.** On the same seeds, square maps were earlier on Small and later on Medium, so there is no consistent hex effect. Rival winners know all 24 minors of each of eras 1-4 before going far into era 5, because the research choice counts a minor at three times its cost, and an old cheap minor still beats Computing at a cost of 2,205. Making rivals research keystones only brought the win forward by 34-68 turns (mean 52) on the same 8 seeds and put eras close to design targets. That variant is an extreme, not a tuned AI. The likely link to the 144 minors added on 31 Oct is an inference, since old code was not re-run.

**The race ignores the late eras.** Transcendence needs only Computing, whose only prerequisite is Electricity, plus a size 8 city. Each default win came 32-38 turns after Computing, made up of 30 turns of activation and 1-3 turns per stage. The winner's best city was making 124-494 shields a turn against stage costs of 300/400/500, so **those costs hardly matter and the ending is a fixed countdown**. Winners still had 45-52 of 180 technologies unresearched.

| Era | Cost of all 6 keystones + 24 minors | New units | New buildings | Keystones with no listed benefit |
|---|---|---|---|---|
| 4 Industrial | 9,360 + 14,592 | Rifleman, Medic | Hospital, Factory, Sewer system | Flight |
| 5 Modern | 14,805 + 23,184 | **none** | Transcendence stages only | Nuclear Fission, Robotics, Genetic Engineering, Globalisation |
| 6 Dimensional | 20,790 + 32,640 | **none** | **none** | Fusion Power |

**The world stops well before the clock does.** In the 8 games logged for it, every home rival had zero turns at war, and no city changed hands in any of the 36 logged games. The last new city came at turn 43-102 and the last new building at turn 176-265. Rivals ended with 9-16 Warriors, a few Spearmen or Horsemen at most, and no Knights or Riflemen. In 52-79% of city-turns no building was under way. Reading the code shows why armies stall: rivals stop training at 14 units of any kind, workers and boats included, they cannot buy production, and the AI never calls the upgrade order. Whether "no war" reflects friendly diplomacy or rivals who never met was not separated, which is why item 3 starts with a log. An earlier note inferred from code that wars between rivals would last only 6-10 turns because peace is offered on a timer. These games show no wars to measure, so that remains untested.

**Against other games, turn 300 is unremarkable and calendar-true.** Civ VI's Standard limit is 500 turns, with costs scaling alongside turn totals across speeds ([third-party copy of GameSpeeds.xml](https://raw.githubusercontent.com/ernsnl/Civilization6Mods/master/Lightning%20Snail%20Fast%20Mod/Original/Base/Assets/Gameplay/Data/GameSpeeds.xml)). That is a ceiling, not a typical finish. Firaxis expects a Civ VII Age to take about 150-200 turns ([2K](https://civilization.2k.com/civ-vii/game-guide/dev-diary/ages)). **The first report's "Old World is capped at 200 turns" is wrong:** a Mohawk developer says the soft limit was turned off and that games average around 120 turns, with most ending before 150 ([Steam](https://steamcommunity.com/app/597180/discussions/0/4625855867810482986)). By your own calendar, turn 300 lands at about 1990 CE and turn 240 at about 1390 CE, so the present finish suits the calendar. The cost curve is normal too: the last two eras in Unciv's Civ V-derived data take 67% of research cost by my sum ([Techs.json](https://raw.githubusercontent.com/yairm210/Unciv/master/android/assets/jsons/Civ%20V%20-%20Gods%20%26%20Kings/Techs.json)), against your 70%. At Civ VII's implied 1-1.5 minutes per turn, 300 turns is about 5-7.5 hours, but that arithmetic is mine and human late-game turns will run longer. "Probably not too long" is as far as the evidence goes.

**What this cannot tell you.** No competent player was simulated, so a human with Knights may well take a Warrior-held project city, and a focused human may finish far sooner than 215-263. Only Small and Medium maps at 2 peoples per world were run, not Large maps or the default of 10. These games measure timing, not whether the last 30 turns feel tense or dull.

## Suggestion, untested: a late-game package in five steps

Do these in order and re-run the same 14 seeds after each. First, once decision 1 is made, raise the research weight on minors so rivals reach keystones sooner. This moves the end by up to about 52 turns earlier at most. Second, if you choose route (a), tie stage 1 to Computing, stage 2 to completing era 5 and stage 3 to an era 6 keystone such as Mind Uploading. Third, since measured era 5 and 6 dates (259 and 350 even with focused research) are too slow for this, cut the era 5 and 6 research pace (420 now) by a third to a half. That is a rough estimate from gaps in the tables, so it must be measured and not trusted. Meier reportedly tunes by doubling or halving rather than small nudges, which suits this kind of test ([Brikman on the memoir](https://www.ybrikman.com/blog/2026/06/18/sid-meier-memoir/)). Fourth, add content: late units so armies change, and a building or two per era. Fifth, keep or replace the plain countdown after a human has played it; a fixed 30 turns with warnings may be the better ending, and scaling stage costs with an empire's output is the alternative.

Suggested pass marks, all mine: game end within 15 turns of your target; every winner passes through the era gates by construction; rivals' armies include units from their latest known era; at least one city changes hands in most games. Any change to rival settling or research will disturb earlier pacing, so old era targets should be re-measured with each step. CLAUDE.md's turn figures also need updating, since they still quote 222-250.

## Suggestion, untested: an open difficulty table with six levels

This now rests on real data, which the first round lacked. Johnson's own slide gives Civ IV's AI production modifiers as +50, +20, +10, even, -5, -10, -15, -20 and -40%, and warns that cheats should not be linear ([Playing to Lose slides](http://www.designer-notes.com/PlayingToLose.zip)). A base-game copy of the Civ IV data file holds slightly different values at the bottom end (160 and 130 where the slide implies 150 and 120), so treat that level as approximate ([copy in a mod repository](https://github.com/rigiechang/Broken-BTS/blob/master/sid-meiers-civilization-iv/Assets/XML/GameInfo/CIV4HandicapInfo.xml)). Unciv's open data runs AI unit cost from 1.75 down to 0.5 and leaves wonders undiscounted on all upper levels ([Difficulties.json](https://github.com/yairm210/Unciv/blob/master/android/assets/jsons/Civ%20V%20-%20Gods%20%26%20Kings/Difficulties.json)). Freeciv instead weakens low levels with deliberate mistakes: 400 in 1,000 decisions at Novice ([difficulty.c](https://github.com/freeciv/freeciv/blob/main/ai/difficulty.c)). Old World varies aggression and starting development by level, with an explicit advantage only at the top two ([Mohawk](https://mohawkgames.com/?p=3363)), and Johnson says its higher levels mainly give the AI more starting cities under the same rules ([Part 3](https://www.designer-notes.com/my-elephant-in-the-room-part-3/)). Copy methods only: Freeciv is GPL, and the Firaxis files are third-party uploads.

Fairness is a matter of perception. Johnson's line is "transparency is the antidote to suspicion and distrust", and he treats a chosen, readable difficulty level as a different thing from cheating ([Game Developer](https://www.gamedeveloper.com/game-platforms/analysis-game-ai-our-cheatin-hearts)). Your code already gives rivals no extra yields or hidden terrain knowledge, which can truthfully be advertised. All figures below are my starting proposal, not measurements, and are unsimulated. Level names are placeholders.

| Setting | Novice | Easy | **Even** (today's rules) | Hard | Very Hard | Paragon |
|---|---|---|---|---|---|---|
| Rival unit and building cost % | 150 | 125 | **100** | 90 | 80 | 65 |
| Rival wonder and Transcendence stage cost % | 150 | 125 | **100** | 100 | 100 | 100 |
| Rival food to grow % | 140 | 120 | **100** | 95 | 90 | 80 |
| Rival research cost % | 130 | 115 | **100** | 95 | 90 | 85 |
| Rival unit upkeep % | 100 | 100 | **100** | 90 | 80 | 65 |
| Rival extras at start | none | none | **none** | 1 soldier | 1 soldier, 1 Worker | 2 soldiers, 1 Worker, 1 Settler, 1 first-era technology |
| Rival target cities | 3 | 3 | **4** | 6 | 8 | 10, by land available |
| Rival total unit limit | 8 | 10 | **14** | 18 | 24 | 30 |
| Chance a build or war choice is deliberately poor | 30% | 15% | **0** | 0 | 0 | 0 |
| Strength lead needed to declare war | 3.0 | 2.0 | **1.5** | 1.5 | 1.25 | 1.25 |
| Rival attitude to you | +15 | +5 | **0** | 0 | -5 | -5 |
| Rivals see through fog | never | never | **never** | never | never | never |
| Your research cost % | 90 | 95 | **100** | 100 | 100 | 100 |
| Your unit upkeep % | 75 | 90 | **100** | 100 | 100 | 100 |

**Build only three rows first** (costs, research, target cities), plus the New Game control, an in-game panel that reads the same numbers, and the fog promise. Apply it to home rivals only, not natives. The save plan: levels 0-5 with Even as 2, and save version 26 moving old games to Even, so they play on as now. Allow changing level in play, and record the lowest level used on the victory screen. Unciv's idea is worth copying: when judging a war, it discounts the strength a difficulty bonus gave the AI, so bonuses alone do not make it reckless ([MotivationToAttackAutomation.kt](https://github.com/yairm210/Unciv/blob/master/core/src/com/unciv/logic/automation/civilization/MotivationToAttackAutomation.kt)).

**Three corrections to the notes that proposed this table.** First, its pass marks assumed Even finishes at 222-250, which the measurements overturn. Keep the gaps, not the turns: Hard about 10-17 turns before Even, Very Hard 25-32, Paragon 40-47, Easy at least 20 after Even's latest game and Novice at least 50 (gaps taken from the note's guesses, re-measured Even first). Second, "Even" is not today's game once items 2, 3 and 6 land, since those change rival strength, so settle them first. Third, the code's target of 4 cities does not match the 5-6 cities measured at turn 100. I did not trace why, so check this before relying on that row.

**The war rules, in order** (code reading only, not run). Spare units today head for the nearest visible enemy with no strength check, never retreat, and wander if they have no target, yet the exact outcome of a fight is available to them. So: hurt units go home; attack only when the exact exchange favours it; pick targets slowly, as Johnson did every 8 or 12 turns by a secondary account ([Rosetta](https://rosetta.to/u/googletechtalks/playing-to-lose-ai-and-civilization-soren-johnson)); offer peace from a list of named reasons, as Unciv does, including an "about to win" reason that can start a war on a racing leader. A first patch sketch is in `research_notes/Hex 4X improvement research deeper/difficulty_ai.md`. Add a test mode where old and new rivals share a game, as Freeciv does when testing new AI code ([README.AI](https://github.com/freeciv/freeciv/blob/main/doc/README.AI)).

**Per-game personality** can be derived from the world seed, so it needs no save change: six values from 0 to 10 (Expansion, Military, Science, Wonders, Boldness, Loyalty), a roll of -2 to +2, mild weights on existing choices, and one visible line such as "Rome this game: expansive, cautious". Freeciv rolls traits between per-nation limits ([aitraits.c](https://github.com/freeciv/freeciv/blob/main/ai/aitraits.c)). No evidence measures how much players notice this.

## Suggestion, untested: event cards as a bag, a budget and 15 cards

**The design.** Reigns' method is the clearest statement of what all these systems do: remove what does not fit or was just played, size the rest by weight, and draw one ([Game Developer](https://gamedeveloper.com/design/game-design-deep-dive-creating-an-adaptive-narrative-in-i-reigns-i-)). Firing from game moments, with one extra pacing roll, avoids scanning every card each turn, a method Paradox is moving away from for speed ([Stellaris wiki](https://stellaris.paradoxwikis.com/Event_modding)). A card holds: id, trigger or Pacing, tier, weight, share of games it appears in, earliest turn and era, overall and per-subject cooldowns, once-a-game, subjects and their tests, required memories, text and options. Each option holds a gate, effects, memories written, an optional roll with odds shown, and an optional follow-up. Selection runs: trigger fires; budget check; collect cards dealt into this game; bind a subject; keep the highest tier; draw by weight with an explicit "nothing" weight; show; apply; log to the Chronicle.

**What to borrow, and what not.** Old World's memories are named, timed and scoped, and a community reference shows durations such as 5, 20 and 100 turns, with costs that scale with cities and turn number ([Old World Reference](https://alcaras.github.io/owreference/events/civil-war)). Keep chains to about one card in ten: about 5% of Old World entries and 11% of King of Dragon Pass scenes are follow-ups ([index](https://alcaras.github.io/owreference/events/); [Dunham](https://kingofdragonpass.blogspot.com/2012/09/how-many-scenes.html)). Follow Failbetter's rule that each stored value has one meaning, and show how long a penalty lasts ([Failbetter](https://www.failbettergames.com/dont-poke-that-narrative-engineering-principles-part-1)). Make cards human-only: Johnson's AI gets no events and cannot win through ambitions, so losses never come as surprises ([Part 3](https://www.designer-notes.com/my-elephant-in-the-room-part-3/)). Write mid-game cards first, as Stellaris's own developers admit its scripted mid-game was too thin after focusing on early and late ([postmortem](https://gamedeveloper.com/design/postmortem-paradox-development-studio-s-i-stellaris-i-)). Amplitude reworked its narrative events to show art and need fewer clicks ([Amplitude](https://community.amplitude-studios.com/amplitude-studios/endless-legend-2/blogs/1084-road-to-1-0-early-access-retrospective)).

**The firing budget.** The notes wrote this for a 230-turn game, so I give the rates and the totals for a 300-turn one, and the totals move with decision 1. Hard rules at every size: no card in the first 10-15 turns; at least 6 turns between cards; one a turn at most; only a crisis tier may break the gap; a token bucket holding 2, so quiet spells do not end in a flood. Old World's reference suggests similar spacing for its slower pools, though it does not define its spacing field precisely.

| Deck size | Earn one card per | Cards in a 300-turn game | Writing, about 100 words a card |
|---|---|---|---|
| 15 (prototype) | 25 turns | 10-13 | 1,500 words |
| 60 (credible first release) | 13 turns | 20-26 | 6,000 words |
| 150 (mature) | 10 turns | 26-37 | 15,000 words |

**Repetition.** No source states when repetition is felt, and Old World players with thousands of events still report it ([Steam](https://steamcommunity.com/app/597180/discussions/1/4625855423768032153/)). CK3's reported spam came largely from one missing cooldown ([Dev Diary 147, excerpt only](https://admin-forum.paradoxplaza.com/forum/developer-diary/dev-diary-147-some-words-from-our-game-director.1673686/latest)), so make cooldown a required field and have a test fail on overfiring. My simple model of the share already seen, if a game draws k from N eligible cards:

| Cards per game | Eligible cards | Game 2 | Game 3 | Game 4 |
|---|---|---|---|---|
| 10 | 15 | 67% | 89% | 96% |
| 25 | 60 | 42% | 66% | 80% |
| 25 | 150 | 17% | 31% | 42% |

**Text.** Players split on reading: complaints centre on being interrupted by walls of text, while simple decision prompts draw fewer ([CivFanatics, weak](https://forums.civfanatics.com/threads/events.691598/latest)). So: title up to 5 words, body 25-50, options up to about 12 with exact effects shown, and the subject named at once. Ordinary cards wait in a side tray rather than interrupting, and a one-line report tier goes straight to the Chronicle. Add a setup setting of Off, Rare, Normal or Frequent, since Johnson says some players find events excessive while others want more ([PCGamesN](https://www.pcgamesn.com/old-world/civilization-too-long)). Log option choices in debug builds; a card with one option taken over 85% of the time is a false choice. These limits are judgement, not benchmarks.

**The first 15 cards (my outlines; effects are placeholders; hooks marked new need a small addition).**

| # | Card | Trigger | The choice | Remembers |
|---|---|---|---|---|
| 1 | The Felled Grove | Woodland cleared beside a city (new) | Pay the woodsfolk, replant, or ignore for production | Felled grove |
| 2 | Water Where the Trees Were | 15-30 turns after ignoring card 1 | Raise banks, move the low ward, or pray with a stated risk | Reads card 1 |
| 3 | Two Wards, One Well | City of 6+ grows with no fresh water (new) | Cistern, ration, or favour the old ward | Slighted ward |
| 4 | The Old Shrine | City's majority faith changes | Protect, seize, or endow both | Shrine seized or kept |
| 5 | Preachers at the Gate | Rival sends missionaries | Expel, debate with a Priest, or tax | Expelled faith |
| 6 | The Pilgrim Road | Pacing, holy city with road | Open to all, or restrict to the faithful | Open road |
| 7 | The Far Province Writes | Pacing, farthest city | Charter, road, or garrison; Rome, Persia and Aztec get an extra option | Chartered or ignored |
| 8 | A Tax Upon the Village | Pacing, owned village | Lower tax, demand levy, or send a Priest | Village wronged or friend |
| 9 | Land for the Old Soldiers | Elite unit (new) or war ends | Retire to land, parade, or keep in service | Veterans settled |
| 10 | The Insult at Table | Pacing, disliking rival at peace | Gifts, answer in kind, or ignore | Insult answered |
| 11 | The Gilded Commission | Golden Age starts | Monument, ease dues, or save | Monument raised |
| 12 | The Lean-Year Prophet | Dark Age starts | Hear him, silence him, or give grain | Reads shrine seized |
| 13 | What the Ruin Kept | Ruin found, era 2+ | Study, sell, or reseal | Opened tomb |
| 14 | Lights Off the Harbour | Pacing, road-linked port | License, raid, or look away | Smugglers |
| 15 | The Doubters at the Work | Project stage starts (new) | Guards or a pause, with a third option set by earlier memories | Reads 3-4 memories |

Build order: data and JSON cards; hooks; save version with used, cooldown and memory tables; tray and card screen; then a test of 200 simulated games reporting firings per card, cards that never fire, and memories nobody reads. Card effects have not been checked against real gold, faith or era-score rates. The fact-check's warning stands: Johnson's own answer to autopilot names families and religions, and tying it to event cards alone is an inference. Old World's events work partly through people you do not have, so a port may lose some of what made them work.

## Suggestion, untested: fix fog first, then water, then ground

**The fault is jitter more than palette.** Measuring the project's own ground generator shows most terrains already use 5-7 colours, yet about a third of their pixels have no same-coloured neighbour: 34% on grass and forest floor, 36% on wasteland, 11% on desert. So "limit each ground to a small colour set", the first report's advice, misses the cause. Alpha layers then multiply colours on screen: a remembered grass patch of 1,500 pixels shows 878 colours. Practitioners warn against exactly this: lone pixels mean little, and random dithering reads as noise ([Pixel Joint](https://pixeljoint.com/forum/forum_posts.asp?TID=11299)). Reef and ocean also share a ramp, so they have a contrast of 1.00. Published palettes step harder between tones than the ramps below, which is reasonable for ground rather than sprites ([Apollo](https://lospec.com/palette-list/apollo.json); [Endesga 32](https://lospec.com/palette-list/endesga-32.json)). All targets below are mine, since no source sets numbers for terrain contrast. The fact-check also dropped the 3:1 guideline as a target: it concerns large elements, with an exemption for elements inside a complex picture ([XAG 102](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/102)), and even visible grass against visible sea is only 1.8:1.

**Fog numbers.** Unciv blends unseen terrain 60% towards a fog colour and adds a faint crosshatch ([TileLayerTerrain.kt](https://raw.githubusercontent.com/yairm210/Unciv/master/core/src/com/unciv/ui/components/tilegroups/layers/TileLayerTerrain.kt)). Wesnoth draws terrain under a fog image and hides most overlays ([display.cpp](https://raw.githubusercontent.com/wesnoth/wesnoth/master/src/display.cpp)). The candidate below is the generator's output without scenery, ownership wash or cloud shadows, so on-screen values will come in lower.

| Median contrast | Screen today (hand-picked areas) | One veil layer, current colours | **Candidate F: saturation .55, brightness ×.80, cool tint, current colours** | Candidate A: same, proposed ramps |
|---|---|---|---|---|
| Remembered grass / ocean (target 1.5) | about 1.2 | 1.30 | **1.62** | 1.93 |
| Remembered forest / ocean | about 1.0 | 1.08 | 1.14 | 1.20, by hand |
| Remembered grass / cloud (target 2) | about 1.6 | 1.56 | **2.72** | 2.58 |
| Remembered ocean / cloud | about 1.04 | 1.20 | 1.67, or 2.05 with cloud ×.7 | 1.33, or 1.64 with cloud ×.7 |

The fog steps are: F, with no art change; thin the mist to alpha about .10-.12, or draw a puff on every third tile, in two or three flat steps; multiply cloud by .7; and freeze remembered water at one frame, which gives a second signal besides colour and saves churn. Forest stays close to sea at any setting, so colour has to carry that pair. A brighter fog must not read as live land, because units are hidden there. Unity's GUI tint only multiplies, so desaturating needs a small material or a baked second picture. The cheap fallback, a flat overlay, covers ground only and has not been checked in Unity.

**Water numbers.** Three frames at 400 ms change 13-17% of open-sea pixels per step, with a hitch at wrap-around. Foam re-rolls 32% of its band. Factorio aims for slow water that does not grab attention, with a freeze option ([FFF-323](https://cf-www.factorio.com/blog/post/fff-323)). A prototype of short ripple dashes fixed to the world changes 1.5-1.7% per step over eight frames with no hitch, and a breathing foam edge flips 7%. Whether 1.6% looks too quiet is unknown. Eight frames could multiply the tile cache, so profile it. Add a freeze option.

**Ramps (my colours; RGB).** Noise picks among three core tones, with shadow and highlight used only as short dashes. In this table, forest was recalculated by hand after the first pass tied it with ocean, and it was not re-run through the harness.

| Terrain | Shadow | Dark | Base | Light | Highlight |
|---|---|---|---|---|---|
| Grass, hill, river meadow | 52,104,68 | 74,134,62 | 98,156,66 | 126,176,76 | 170,202,98 |
| Forest floor (revised) | 34,68,58 | 46,88,58 | 62,110,60 | 82,128,66 | 112,150,76 |
| Jungle floor | 18,52,50 | 26,70,52 | 38,88,52 | 56,108,54 | 88,132,62 |
| Mangrove | 28,62,60 | 40,84,64 | 58,106,68 | 82,128,76 | 60,120,120 |
| Bog | 38,50,44 | 54,68,46 | 74,88,52 | 96,106,62 | 62,96,92 |
| Savanna | 140,116,62 | 172,146,68 | 198,172,82 | 220,196,106 | 238,218,140 |
| Steppe | 112,120,70 | 142,148,80 | 168,172,94 | 192,194,116 | 216,212,144 |
| Desert | 176,134,92 | 204,164,108 | 224,190,128 | 238,210,150 | 248,230,182 |
| Mountain base | 72,66,72 | 98,88,84 | 124,112,98 | 150,136,114 | 182,168,142 |
| Rock | 64,62,78 | 90,86,96 | 118,112,114 | 148,140,132 | 184,174,158 |
| Wasteland | 72,56,60 | 96,78,68 | 122,102,82 | 148,128,100 | 178,156,124 |
| Volcanic | 28,24,36 | 44,36,46 | 62,48,52 | 86,62,58 | 190,70,30 |
| Tundra | 92,108,104 | 118,134,118 | 146,160,136 | 176,186,162 | 214,222,206 |
| Snow forest | 116,138,160 | 146,168,184 | 178,198,208 | 208,222,228 | 238,244,246 |
| Ice | 146,182,214 | 174,204,226 | 202,224,238 | 226,240,246 | 248,252,252 |
| Glacier | 104,144,190 | 136,174,212 | 172,206,232 | 206,228,242 | 236,248,252 |
| Ocean | 18,50,116 | 22,66,140 | 28,84,164 | 44,112,186 | 120,180,216 |
| Coral reef, shallows | 26,92,160 | 34,116,180 | 48,140,194 | 84,170,206 | 170,222,232 |
| Lake | 24,74,100 | 32,96,120 | 44,116,138 | 66,142,156 | 126,190,190 |

Greyscale results: grass/ocean moves from 1.77 to 2.23, reef/ocean from 1.00 to 1.98, lake/ocean from 1.10 to 1.39. Steppe, savanna and tundra stay close in value and rely on hue. The ocean median falls from .143 to .093, against your liking for bright seas; a gentler base of 30,94,176 is offered if you object.

**My one look at the prototype.** The researcher never viewed the side-by-side sheet, so I did, once, at sheet size, and with no scenery or real map around it. The faded fog column reads clearly better than the current dark one, with land and sea plainly distinct. The calmer ground is less noisy but looks blocky, like camouflage, and its small dashes read as regular stitches. Water looks flat with hard bands. That matches the note's measurements: clusters averaged 36-110 pixels against its own target of 8-25, so **the ground prototype overshoots and could cost you the painted look**. Treat fog as ready to try and the ground as ready to retune. Keep the old code behind a switch until you choose. An AI's glance at a contact sheet is not a verdict, so please judge it yourself: `C:/Users/Matth/AppData/Local/Temp/claude/37cdc78b320e14b6/a3b6113e-5299-463a-9abe-c0038f5399dd/scratchpad/m/sheet.png`. Copy it soon, as this folder is temporary.

Pass tests, all mine: orphans under 5% (now 11-36%); neighbouring pixels differing 20-40% (now 48-71%); 3-5 colours covering 90% of a ground; mean cluster 8-25 pixels on land; water change under 3% per step; foam under 10%; reef/ocean 1.5 or more. The note's working code is in `research_notes/Hex 4X improvement research deeper/terrain_art.md`. Also test whether fractional zoom breaks pixels, which is a suspected and unconfirmed cause of on-screen noise. None of this spends SpriteCook credits, and it needs no save change. Whether it looks better needs a Unity build, screenshots and you.

## Suggestion, untested: CC0 and CC BY audio only, effects first

Text searches of scripts and scenes find no audio at all, not even a listener, so the first audio code must add one. **Your standing rule of pushing everything to a public repository decides the licences.** CC0 allows any use with no credit ([CC0](https://creativecommons.org/publicdomain/zero/1.0/legalcode)), and CC BY allows it with credit and notices ([CC BY 4.0](https://creativecommons.org/licenses/by/4.0/legalcode)). Sonniss bars supplying its sounds to others, so loose files in a public repository look like a breach by my reading, though the licence does not name repositories ([Sonniss](https://sonniss.com/gdc-bundle-license/)). Pixabay bars distributing unmodified files on their own ([Pixabay](https://pixabay.com/service/terms/)). The Asset Store limits distribution to files embedded in a product ([Unity](https://unity.com/legal/as-terms)). Avoid non-commercial and share-alike tracks, including Wesnoth's ([Wesnoth](https://wiki.wesnoth.org/Wesnoth:Copyrights)). This is not legal advice. CC0 uploads carry no warranty, so keep a dated record of each file's page, author and licence, plus a Credits screen.

**I have not heard any of these tracks, and neither has any researcher.** Licences on OpenGameArt are search-summary level, because the site blocked automated reading, so open each page in a browser first.

| Use | Candidate | Licence as seen | Confidence |
|---|---|---|---|
| Clicks, tabs, errors, end turn | [Kenney Interface Sounds](https://kenney.nl/assets/interface-sounds), 100 files | CC0 | Pack page read |
| New turn, city founded, research, era change, first contact | [Kenney Music Jingles](https://kenney.nl/assets/music-jingles), 85 files | CC0 | Pack page read |
| Battle: hit, kill, loss | [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds), [RPG Audio](https://kenney.nl/assets/rpg-audio) | CC0 | Pack page read |
| Menu | Once Upon a Time (loop), TAD, on OpenGameArt; find by title | CC0 | Summary only |
| Peace, early eras | [Treasure Hunter](https://opengameart.org/content/treasure-hunter); [Folk Round, Kevin MacLeod](https://incompetech.filmmusic.io/song/3770-folk-round) | CC0; CC BY | Summary only |
| City screen or peace | [Minstrel Guild, Kevin MacLeod](https://incompetech.filmmusic.io/song/4056-minstrel-guild) | CC BY, version unclear | Summary only |
| War only | [Determined Pursuit, Emma_MA](https://opengameart.org/content/determined-pursuit-epic-orchestra-loop); Boss Battle Music, SubspaceAudio | CC0 | Summary only |
| If you choose chip sound | [Chiptune Adventures, Juhani Junkala](https://opengameart.org/comment/70103) | CC0 | Summary only |
| Effects and music, one source | [Ninja Adventure pack](https://pixel-boy.itch.io/ninja-adventure-asset-pack), 37 tracks | CC0 | Page read; theme fit unproven |
| Hold until confirmed | [Alkakrab medieval packs](https://alkakrab.itch.io/free-fantasy-medieval-ambient-music-pack) | No named licence | Ask the author in writing |
| Backup with credit | [Incompetech](https://incompetech.com/music/royalty-free/faq.html) | CC BY, on a visible Credits screen | FAQ read |

**How to use them.** Minimum set: menu, three peace tracks by era band and two war tracks. Ten tracks are about 36 MB by my estimate. Pick one palette, since mixing chip and orchestral will likely sound wrong. Civ IV used era playlists and Civ V war and peace ones, with war pieces more aggressive ([interview](https://squareenixmusic.com/features/interviews/michaelcurran.shtml)). So build shuffled era-band playlists with silent gaps first, then a peace/war switch and crossfade. Skip layered stems, as free packs rarely supply them. I found **no composer statement on fatigue in long sessions**. Moderate tempos and 30-90 second gaps are my judgement, loosely supported by Phillips's slow-tempo choices for a strategy game, whose cited studies did not test strategy games ([Part 2](https://gamedeveloper.com/audio/video-game-composers-can-make-you-smarter-the-music-of-dragon-front-part-2)).

**Traps specific to this game.** With up to 66 nations, "at war with someone" could become near-constant if rivals start fighting, so trigger war music only on recent combat involving you or an enemy near your city, with a few turns' delay. Play only events you can see, with a cooldown per cue and a cap on simultaneous sounds, and stay silent for other worlds. Play the end-turn sound on the click, before rival turns run. Keep audio out of engine logic so headless tests still run.

**Settings from day one:** Master, Music, Effects, Interface and Ambient sliders, mute-all and a mono toggle, stored in PlayerPrefs so saves are untouched. Separate volume controls and no essential information by sound alone are basic guidelines ([GAG](https://gameaccessibilityguidelines.com/full-list/); [XAG 105](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/105)). Import effects as ADPCM with Decompress On Load, and music as Vorbis with Streaming ([Unity](https://docs.unity.com/en-us/engine/6000.3/manual/audio/reference/class-audio-clip)).

## Suggestion, untested: an End Turn button that leads, never traps

Unciv is the clearest model because its source is public: one button shows the first thing needing you, and clicking does it, falling through to Next turn ([NextTurnAction.kt](https://raw.githubusercontent.com/yairm210/Unciv/master/core/src/com/unciv/ui/screens/worldscreen/status/NextTurnAction.kt)). Humankind shows the danger, as players report a hint that blocked the turn with no way past ([Steam, weak](https://steamcommunity.com/app/1124300/discussions/0/3044984779773227020)). Firaxis offers an advisor mode limited to warnings ([Dev Diary 8](https://civilization.2k.com/civ-vii/archive/dev-diary/victories-post-launch/)). No data shows whether such blockers help or hurt.

The proposed order is **Choose research, Answer an expiring offer, City has nothing to build, Next unit (n), End Turn**. Shift+Enter always ends the turn, and the button also offers "End anyway". A city counts as empty only when both its unit and building lines are empty; otherwise the button nags every turn. Settings cover "Stop at idle units" and "Stop at empty cities", both on in tutorial games. Add a test that no state can block forever. Beside it: optional auto-advance to the next idle unit; notices that jump to their subject instead of always opening the Chronicle (log lines are plain text today, hence Small to Medium); and a warnings-only advisor in Divus Maximus's voice for riots, negative gold, enemies beside cities and starvation. Not yet: auto-end-turn.

For mistakes, **restart in the first five turns** follows Civ VII's patch ([1.1.1](https://civilization.2k.com/civ-vii/game-update-notes/2025-mar-25-patch-1-1-1/)). Restart this turn can use the autosave, except that on maps over 9,000 tiles autosave runs only every fifth turn, and that already includes your Small maps at 15,778 tiles. Per-move undo should refuse any move that revealed a tile, fought, met someone or took a village. Old World instead treats undo as reloading, and one player objects that this lets a human scout for free ([CivFanatics, forum post](https://forums.civfanatics.com/threads/civ-7-undo-or-backup-turn-game-mechanic.685920/latest)).

## Suggestion, untested: onboarding fixes, smallest first

This comes from reading code, tutorial text and screenshots, not from watching anyone play. The nearest thing to data is weak: Old World's Learn to Play achievements fall from 53% to 40% to 25% ([Steam](https://steamcommunity.com/stats/597180/achievements)), which may only mean owners started a normal game. Firaxis, Mohawk and Plants vs. Zombies' designer all point to teaching at the moment of need, one thing at a time ([Dev Diary 8](https://civilization.2k.com/civ-vii/archive/dev-diary/victories-post-launch/); [Update 116](https://mohawkgames.com/2023/11/22/old-world-update-116/); [George Fan](https://gamedeveloper.com/design/gdc-2012-10-tutorial-tips-from-i-plants-vs-zombies-i-creator-george-fan)).

| Fix | Problem seen | Job |
|---|---|---|
| Change "(or F)" to B | **Checked for this report:** `Tutorial.cs` says F; only B is bound in the scripts | Small |
| Rewrite the F1 page | It still says 124 technologies, 14 eras, naval units planned, "Drag units", "Economy" | Small |
| Clear labels | "0 queued" above a queued item; "Worker → Entertainer" means a citizen; shields versus production; unlabelled "Strength 0 / 1" | Small |
| Correct the production advice | "Start with a Warrior or Worker" when a Warrior is already queued and you own both; a plain click discards progress | Small |
| Show the guide on city and research screens | The panel disappears exactly where it explains things | Small |
| Finish the research step on choosing | It completes on merely opening the screen | Small |
| Steps 6-11 fire on events, one a turn at most | They can all be read on turn 2 | Small to Medium |
| One-line goal, derived from game state | "Grow Azurehold to size 2 (9 turns)", then "Train a Settler" | Small |
| Show site advice while a Settler is selected | It needs Alt held today, which the tutorial never mentions | Small |
| Quick game preset | Small map, few rivals, peoples low, easy level. Also speeds your own hand tests | Small to Medium |
| Local funnel log | Tutorial step, turn reached, sessions; shared only by choice. This would be the only real data | Small |

Three things remain to check in a real game: whether the Settler is already selected, whether long tutorial text fits its box, and how long the Worker has nothing to do. On the last: code shows roads need The Wheel, farms Agriculture and mines Mining, while a project check implies clearing is available from Stone Tools at the start, so "all four buttons grey" from the notes looks overstated. The note's turn counts for a first game (first technology about turn 23, second city about turn 35) conflict with rivals finishing era 1 near turn 42, so measure them in the diary.

Items for accessibility, when you reach them: Xbox asks for 18 px minimum at 1080p and resizing to 200%, and your 12-point text is probably under that, an estimate rather than a measurement ([XAG 101](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/101)). Key remapping needs no interface rewrite, just one lookup table plus updating every on-screen key name. A real UI scale means anchoring layouts screen by screen, and is Large.

## Designers' admissions and 2025-26 releases point to clarity over breadth

**Admitted failures cluster in a few places**: free repeated actions, opaque AI, hollow mid-games and mechanics with knock-on effects. Those make a useful checklist for your diary. Shafer says human-like AI behaviour read as noise and that a visible relations number works better, and that a visible resource that does not matter is a flaw ([Civ 5 retrospective](https://gamedeveloper.com/design/revisiting-the-design-of-civ-5)). Audit your 16 resources against that. Johnson calls free per-turn citizen moves for tiny gains a tedious trap ([Part 2](https://www.designer-notes.com/my-elephant-in-the-room-part-2/)), which is relevant to your click-to-lock citizens. He also classes the Civ 3 bargaining table as a mistake because humans re-contact every civilisation each turn ([Part 3](https://www.designer-notes.com/my-elephant-in-the-room-part-3/)), so check your deals for that loophole. Fischer says a team-wide blind spot hid a bug because everyone placed things the same way ([After the Demo](https://www.paradoxinteractive.com/games/millennia/news/after-the-demo)), which is why item 1 asks for someone else as well. Designers also disagree on unit stacking, automation, exploits and how clever an AI should be, so no rule here is universal. Civ VI's team named pacing as its biggest challenge and added an AI "mayhem" level so games would not stay calm ([Game Developer](https://www.gamedeveloper.com/design/city-management-mayhem-and-sid-meier-s-wisdom-making-i-civilization-vi-i-)), a producer's second-hand account of an idea that fits your frozen late game. Meier's reported advice to explain every setback comes from coverage, not from his own text ([VentureBeat](https://venturebeat.com/2010/03/12/quotes-from-sid-meiers-keynote-gdc-speech)).

**Recent patches are mostly clarity work.** Civ VII's 2026 updates added a Radial Ruler showing distances from the cursor, a panel naming who caused each change in a relationship, the ability to close a peace screen to see the map, and on-off switches for each victory ([1.4.2](https://civilization.2k.com/civ-vii/game-update-notes/2026-jul-28-patch-1-4-2/); [1.5.0 copy](https://patchbot.io/games/sid-meiers-civilization-vii/articles/1434-update-150-notes-september-15-2026)). I have not checked which you already have. A ruler would suit your city reach of 2-5 tiles. Its largest update doubled concurrent players to over 16,000 for a time ([CivFanatics](https://civfanatics.com/2026/05/23/pc-gamer-civ7s-test-of-time-update-brings-players-back/)), while Steam's rating stayed near 48% ([Raijin, weak](https://raijin.gg/app/1295660/Sid_Meiers_Civilization_VII)), which hints that first impressions are hard to repair. Endless Legend 2, a hex game, added zone of control and melee retaliation late, in about 59 early-access updates ([Amplitude](https://community.amplitude-studios.com/amplitude-studios/endless-legend-2/blogs/1084-road-to-1-0-early-access-retrospective)). Check how units slip past each other in your hexes before raiders arrive. Big system updates are a reputational risk: Stellaris 4.0 and EU5's 1.2 both reportedly drew backlash for instability ([PCGamesN](https://www.pcgamesn.com/stellaris/4-0-fixes-free-weekend); [MeinMMO, weak](https://mein-mmo.de/en/the-swedish-strategy-nerds-are-now-even-exaggerating-with-the-updates-delivering-72-pages-of-patch-notes,1567059/)). A sensible local rule is to measure late-game turn time and load an old save before calling any systems change finished.

**The solo warning is scope.** Victor Reijkersz says he was far too ambitious on Shadow Empire, which ran years over and nearly burned him out, and he built systems a learner can safely ignore ([Matchstick Eyes](https://matchstickeyes.com/2022/04/03/shadow-empire-interview-with-victor-reijkersz)). Songs of Syx, a one-person pixel-art game rated 94% on Steam, still names onboarding as its largest problem ([Steam](https://store.steampowered.com/app/1162750/Songs_of_Syx/)). Short games such as Hexarchy and Ozymandias removed whole chores to get short ([Hexarchy](https://store.steampowered.com/app/1356810/Hexarchy/); [Ozymandias](https://store.steampowered.com/app/1768280)), but long Terra Invicta also rates well, so nothing shows short is better. These ratings measure satisfaction against expectations and come from self-selected fans. A project with 180 technologies, 22 civilisations, 50 sealed peoples, 23 faiths and five locked worlds, none played by hand on hexes, is exposed to exactly this risk.

## The sealed peoples passed review, with a few text fixes under way

Text fixes are being applied separately, so this section is only a summary. A desk review of the 40 sealed peoples numbered 39-78 in the catalogue found no figure sacred to a living faith, no people openly mapped to a real religious or ethnic group, and nothing sexualised. The unit name "Ravener" clashes closely with a Warhammer 40,000 product ([Wayland Games](https://www.waylandgames.co.uk/tyranid-ravener-brood-99120106016)). The Stiltwind Factors' story stacks plague, debt and expulsion in a way that echoes medieval slander, and The Manifest and the Flensers of Ummoth need lighter edits. Several source comments overstate their origins, and "Ummoth" is shared across two worlds. This was not legal clearance: no trademark registry was searched, most leader, unit, building and city names were not searched at all, and no sensitivity readers were consulted. A real clearance is needed before any sale.

## What the fact-check changed, and how far to trust any of this

**How much research ran.** Ten tasks on 6 Nov 2026, each about 6 to 17 minutes, side by side. That is **well under the three hours you asked for**. Several tasks stopped at a 60-call budget: the fact-check went over it (68 calls) and left 11 links unopened, while the terrain, interface and designer tasks ran out too. Some reading therefore stopped early, not because the question was finished.

**Limits that apply to everything.** Nothing has been played by a human. Harness results are not Unity results, and Unity turn times were not measured here. Web pages were read through a summarising tool, so numbers and wording need checking before reprinting; only a couple of pages came back as full text. Quotations were not checked against originals. Job sizes, payoff grades and all proposed numbers are my judgement.

**Sites that blocked or failed:** OpenGameArt, Musopen, Paradox forums, weplayciv's Old World modding guide, Old World Reference's rare-events page, Fandom pages, Wesnoth's forums, MarketBeat, SteamPulse, the Internet Archive, GameSpot's Reynolds interview, the GDC Vault's members-only material, Lucas Pope's devlog and individual posts on Jon Shafer's blog. None was bypassed. Some PC Gamer and GamesRadar pages returned headlines only.

**Things that could not be verified.** Old World's actual firing rates and how its selection values combine. Stock Civ V and Civ IV Beyond the Sword data files, for which forum posts and mod copies stand in. Civ VI's difficulty zero point. Any studio's tutorial funnel, or any link between a mechanic and retention. The book "At the Helm" by Jon Shafer could not be confirmed to exist, and his At the Gates postmortem went unread; only press coverage of it was seen. Sid Meier's memoir was read only through reviews. No composer quote on long-session fatigue exists in what was read.

**First-report claims now corrected:**

| First report said | Now |
|---|---|
| Rivals win at 222-250; game length "is not the problem" | About 300 by measurement. "Probably not", and the human pace is unknown |
| Old World is capped at 200 turns | A developer says there is no limit; games average about 120 turns |
| Under half of Civ VI players finished, so finishing matters more than breadth | The figure has no stated sample or definition ([Game Developer](https://www.gamedeveloper.com/design/firaxis-big-swing-with-civilization-vii-convincing-players-to-actually-finish-their-games)). The nearest Steam achievement is 37.3% for any win on Settler or higher, about the 36% median in a benchmark that excludes strategy games ([Steam](https://steamcommunity.com/stats/289070/achievements); [Two Average Gamers](https://www.twoaveragegamers.com/game-completion-rates/)). So Civ VI may not be unusual, and this report no longer rests on the claim |
| "Designers across the genre" name the late game as unsolved | Mainly Johnson and Firaxis, plus Stellaris's mid-game admission |
| Event cards: Strong evidence, Medium job | One designer with no outcome data, so Moderate; larger job; a port may lose its character-driven effect |
| Sound is "the cheapest large change" | Unsupported. The one cited study could not be opened ([MDPI, HTTP 403](https://mdpi.com/2076-3417/14/2/583)). The gap is certain, its size is not |
| Colour limits are "likely the largest visual gain per hour" | A guess. Measurements now put the fault in jitter, and the prototype needs retuning |
| Fog evidence "Strong"; 3:1 target from Xbox | One pair of hand-picked screenshots, so Moderate; 3:1 is not a useful bar here |
| Difficulty is Small to Medium | No control exists at all, so at least Medium |
| Compression "could end" the autosave limit on big maps | 84:1 reproduces to the byte, but only on turn-30 saves from what looks like a rapid run. A synthetic stress test still gives 18:1. The limit already applies on Small maps. Compression adds a step after producing the text, so it may not help speed |
| Civ VII's Age figures are official | They are developer-diary expectations, not measured turn data |
| Transcendence warnings are "already addressed" | Only for rivals you have met |
| LittleBigPlanet's recall as a citation | The cited MTV link no longer reaches the article, so verify any use of it |

## Conclusion

The measurements change the shape of the problem. The first report asked how to make an ending better; the data says most of the ending is not being reached, by rivals at least, and the empires arriving there have stopped being interesting long before. That turns the largest late-game question from a content problem into a behaviour problem: rivals that fight, modernise and spend may do more for turn 250 than any new system, at a fraction of the cost, and they also determine whether raiders, crises, Transcendence warnings and harder difficulty mean anything at all.

The evidence cannot choose your game's length or the fate of the gates, and it should not try. It does suggest an order: decide length and era purpose, make rivals behave, then play. A single hand-played game with rivals who can threaten you would test more of this report than another round of reading would, and it is the one kind of evidence nobody has yet.
