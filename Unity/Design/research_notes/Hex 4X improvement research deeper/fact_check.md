# Fact check of "Hex 4X improvement research" (checked 6 Nov 2026)

Method and limits: every "read" below went through WebFetch, which summarises pages and extracts quotes, except XAG 102 and 103, which came back as full text. So "read" means the summariser's extract, and quotes should be checked before reprinting. No page was labelled "snippet" because no claim here rests on a search result: WebSearch was never used. I went over the 60-call limit: 68 calls had been made before this notes file was written, because parallel batches were larger than I tracked. I stopped research at that point, so 11 links cited in the report and the GameRant link in the notes were not opened; they are listed as "not opened (budget)", which is different from "could not open". Saves were only read; nothing but this file was written in the project.

## Do the cited links exist and say what the report claims?

### Takeaway
The numbers named for checking mostly hold, several of them exactly. The problems are in how the report uses them: paraphrases that drop limits, and strong conclusions drawn from sources that do not reach that far. No opened link was missing or flatly contradicted its claim; 4 could not be opened and 11 were not opened.

### Cited Findings

**Named numbers**

| Claim in report | Verdict | Check | What the source says |
|---|---|---|---|
| Beach: under 50% of Civ VI players finished | Partly supported | read | "It was less than 50 percent." The bracketed metric is the "number [of people who completed a game of Civ VI]". The article gives no telemetry, sample, period or definition of "completed". Calling it "the only solid figure" overstates it — [Game Developer](https://www.gamedeveloper.com/design/firaxis-big-swing-with-civilization-vii-convincing-players-to-actually-finish-their-games) |
| Steam 37.4% "first-win achievement" | Partly supported | read | Page now shows 37.3% for "Game, Settler, Match": "Win a regular game at Settler difficulty or harder". It means any win of a regular game on Settler or harder, not a first-win measure of finishing. The 0.1 gap is normal drift — [Steam](https://steamcommunity.com/stats/289070/achievements) |
| Civ VII 1.4.0 score-lead win: five-turn countdown, rivals can pause, 6x/4x/3x/2x/1.5x/1.25x | Confirmed | read | All six multipliers, the countdown and the pause match. Page body is dated 5/18/2026, slug says May 19 — [2K](https://civilization.2k.com/civ-vii/game-update-notes/2026-may-19-patch-1-4-0/) |
| 1.4.0 made civ swapping optional | Confirmed | read | "you can choose to stay or swap at each Age Transition". Age Transitions remain, so this supports "forced civ change" only, not "era resets" — same page |
| Take-Two's chief executive said the changes went too far | Partly supported | read | Zelnick: "what we tried to do was a bridge too far, from the consumer's perspective" and "we got it wrong with Civ VII". A fair paraphrase of reception, but he names no mechanic, so it does not show era resets are the fault — [Aftermath](https://aftermath.site/civilization-vii-civ-firaxis-update.md) |
| Old World "over 3,000 events" | Confirmed, dated | read | "our 3,000+ events", post dated 30 Sep 2021. Whether the count has grown since was not checked — [Notes #9](https://www.designer-notes.com/old-world-designer-notes-9-events/) |
| Age of Wonders 4 vassal shares (30%, top tier 50%) | Not checked | not opened (budget) | Appear in `mechanics.md` line 102 only, marked secondary (GameRant). They are not in the report, so no recommendation rests on them |
| Unciv camps: unseen land, 50% roll, 7 and 4 tiles | Confirmed, simplified | read | Code matches. Omitted: turn 1 places a third of all allowed camps, the number of camps is capped by fog tiles, and destroyed camps block only 4 tiles — [BarbarianManager](https://github.com/yairm210/Unciv/blob/master/core/src/com/unciv/logic/automation/civilization/BarbarianManager.kt) |
| Unciv spawn every 8-12 turns | Partly supported | read | 8-12 is the base only: `countdown = 8 + rng.nextInt(5)`, halved on raging, plus a difficulty delay, minus up to 3 for units already spawned, times a speed modifier. So a veteran camp can run below 8 — [BarbarianEncampment](https://github.com/yairm210/Unciv/blob/master/core/src/com/unciv/logic/automation/civilization/BarbarianEncampment.kt) |
| Unciv none wandering before turn 10, no ships before turn 30 | Confirmed | read | `if (gameInfo.turns < 10) return null`; `canSpawnNaval = gameInfo.turns > 30`. These live in BarbarianManager, not the Encampment file — BarbarianManager link above |
| Unciv friend 30, ally 60, decay 1 | Confirmed, simplified | read | Ally also requires being that city-state's chosen ally. Decay is 1 by default, 1.5 or 2 in some cases, and only above the resting point — [DiplomacyManager](https://github.com/yairm210/Unciv/blob/master/core/src/com/unciv/logic/civilization/diplomacy/DiplomacyManager.kt) |
| Unciv great person 100, doubling | Confirmed | read | Per pool, with a speed modifier on top — [GreatPersonManager](https://github.com/yairm210/Unciv/blob/master/core/src/com/unciv/logic/civilization/managers/GreatPersonManager.kt) |
| XAG: "at least 3:1 on large elements in maps" | Partly supported | read | "Large-scale text and large-scale visual elements" need 3:1 against their background; standard-size elements need 4.5:1, which the report omits. Maps get a separate passage asking for "strong contrast" with no number. Elements "part of a picture that contains significant other visual content" have no requirement, which may exempt terrain — [XAG 102](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/102) |
| XAG: a signal other than colour | Confirmed | read | "at least one additional signifier such as shape, pattern, iconography, or text labels" — [XAG 103](https://learn.microsoft.com/en-gb/gaming/accessibility/xbox-accessibility-guidelines/103) |
| Outlined markers hold on light and dark | Confirmed | read | For Honor example in XAG 102 |
| Unity: IMGUI not intended for normal player interfaces | Confirmed, softened | read | "not generally intended to be used for normal in-game user interfaces". The word "generally" was dropped, and the page speaks of UI, not map drawing — [Unity manual](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/ui-imgui/guiscripting-guide) |
| OnGUI may run several times a frame | Confirmed | read | "several times per frame (one call per event)" — [Unity](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/monobehaviour/ongui) |
| 8-10% of males; free colour choice most requested | Confirmed as what the page says | read | Both on the page, rated Basic, with no underlying data shown — [GAG](https://gameaccessibilityguidelines.com/ensure-no-essential-information-is-conveyed-by-a-colour-alone/) |
| Basic: volume, colour, font, remap, difficulty. Intermediate: reminders, contextual help | Confirmed | read | Mid-game difficulty change is Intermediate — [GAG full list](https://gameaccessibilityguidelines.com/full-list/) |
| 84:1 save compression | Confirmed for that save | run locally | See the save question below |

**Other links**

| Link | Verdict | Check | Note |
|---|---|---|---|
| [Notes #7](https://www.designer-notes.com/old-world-designer-notes-7-characters/) | Confirmed | read | "Eternal China Syndrome"; pressure is mostly external |
| [Notes #8](https://www.designer-notes.com/old-world-designer-notes-8-opinion/) | Confirmed, but see below | read | "the game slowly slides into auto-pilot"; the fix named is "adding internal pressure from families and religions" |
| [Notes #9](https://www.designer-notes.com/old-world-designer-notes-9-events/) | Confirmed | read | Deck, trigger, requirements, weights, the Drunk trait example, discomfort with opaque triggers. Johnson allows that transparency "could be violated for effect occasionally" |
| [Notes #11](https://www.designer-notes.com/old-world-designer-notes-11-the-end/) | Confirmed | read | Hardest part, "no one has ever done this well", "push End Turn fifty more times", surprise ending, ambitions scale |
| [Johnson on undo](https://www.designer-notes.com/my-elephant-in-the-room-part-1/) | Confirmed | read | "one of our most popular features" |
| [Stellaris Anomaly](https://stellaris.paradoxwikis.com/Anomaly) | Partly supported | read, first 100,000 of 227,870 characters | "Each anomaly outcome can only take place once" is stated. Chains appear only as listed examples, not as a stated rule |
| [Stellaris Crisis](https://stellaris.paradoxwikis.com/Crisis) | Confirmed | read | 120 empty against 8+10+10+10 gives 38/158 = 24%, matching the report's arithmetic |
| [Civilopedia copy](https://www.civilopedia.net/en-US/gathering-storm/concepts/world_6/) | Confirmed | read | Scout, return, raid. Site is unofficial in appearance; the report's "copy" label is fair |
| [CivFanatics barbarians](https://forums.civfanatics.com/threads/barbarian-spawn-rate-remains-insane.628150/) | Confirmed | read | Complaints in both directions, and posters ask for options |
| [EL GDD 5](https://community.amplitude-studios.com/amplitude-studios/endless-legend/forums/7-archives/threads/3590-el-gdd-5-minor-factions?page=1) | Partly supported | read | Force against slower, better-paying peace is there, dated 5 Dec 2013. "Trade" was not confirmed in the extract |
| [2K diplomacy diary](https://civilization.2k.com/civ-vii/game-guide/dev-diary/diplomacy-influence-trade/) | Confirmed | read | Influence, Suzerain lock-out, levy, incorporate |
| [Slashdot Q&A](https://forums.civfanatics.com/threads/slashdot-interview-of-the-civ4-team.134142) | Confirmed | read | Penalties low, bonuses high, Noble and Prince closest, no wonder bonuses |
| [Cheatin' Hearts](https://www.gamedeveloper.com/game-platforms/analysis-game-ai-our-cheatin-hearts) | Confirmed | read | Quote and fog, wonder and discount passage. The examples concern the original Civilization |
| [Mohawk 116](https://mohawkgames.com/2023/11/22/old-world-update-116/) | Confirmed | read | The tutorial change is "less prone to cluster on the same turns", a small tweak |
| [Firaxis Ages diary](https://civilization.2k.com/civ-vii/game-guide/dev-diary/ages) | Confirmed, overstated | read | "around 150 to 200 turns" and "roughly three to four hours" are expectations from the developer's diary. The report calls them "official figures"; no measured turn data is cited |
| [PCGamesN](https://www.pcgamesn.com/old-world/civilization-too-long) | Confirmed | read | "capped at 200 turns" |
| [Shafer on Civ 5](https://www.gamedeveloper.com/design/revisiting-the-design-of-civ-5) | Confirmed | read | Exact quote, about global happiness |
| [Firaxis message](https://civilization.2k.com/en-GB/civ-vii/news/message-from-firaxis-games/) | Confirmed | read | "Our top priority is improving the UI." |
| [Two Average Gamers](https://www.twoaveragegamers.com/game-completion-rates/) | Confirmed | read | 36% median, 19 games, Steam achievements, no strategy games |
| [Incompetech FAQ](https://incompetech.com/music/royalty-free/faq.html) | Partly supported | read | Credit screen and visibility rule confirmed. The extract did not show "default licence" wording |
| [PC Gamer, Old World review](https://www.pcgamer.com/uk/old-world-review/) | Could not open | could not open | Title only, on this URL and the non-UK one. Both uses (clashing text, poor victory conditions) are unchecked |
| [Game Watch](https://game.watch.impress.co.jp/docs/20080222/ai.htm) | Could not open | timed out | 40% at Deity unchecked |
| [MDPI sound study](https://mdpi.com/2076-3417/14/2/583) | Could not open | HTTP 403 | The only evidence for sound's effect is unchecked; the report itself saw only the abstract |
| [MTV LittleBigPlanet](https://www.mtv.com/news/wntrxa/littlebigplanet-release-delayed-over-quotes-from-quran) | Could not open | 301 to mtv.co.uk home | The cited link no longer reaches the article. A replacement citation is needed |
| [Metacritic Millennia](https://legacy.prod.metacritic.com/game/pc/millennia/critic-reviews), [Digital Trends Ara](https://digitaltrends.com/gaming/ara-history-untold-review-pc), [Pixelblog 20](https://slynyrd.com/blog/2019/8/27/pixelblog-20-top-down-tiles), [Pixelblog 1](https://www.slynyrd.com/blog/2018/1/10/pixelblog-1-color-palettes), [Pixel Parmesan](https://pixelparmesan.com/blog/dithering-for-pixel-artists), [Civ VI art](https://www.gamedeveloper.com/art/making-i-civilization-vi-i-both-playable-and-beautiful), [Wesnoth](https://wiki.wesnoth.org/Tiles_Tutorial), [Terrain from Noise](https://www.redblobgames.com/maps/terrain-from-noise/), [River growing](https://www.redblobgames.com/x/1723-procedural-river-growing/), [PC Gamer Smite](https://www.pcgamer.com/hindu-leader-requests-removal-of-dieties-from-smite-hi-rez-declines/), [GameSpot Eidos](https://www.gamespot.com/articles/eidos-responds-to-sikh-controversy/1100-2897279) | Not checked | not opened (budget) | Item 8 rests on the three art links, so its sources remain unverified by me |

### Inferences
- Item 6 is the most stretched citation. The autopilot passages I read name families, religions and characters as the answer. Linking autopilot to event cards alone is the report's inference, and I did not ask the summariser whether Notes #9 makes that link itself. Old World's events act largely on characters and families, which Paradigm Realm does not have, so a port may lose much of what made them work.
- "Do not add era resets" cites evidence that bears on forced civ change and general reception. `pacing.md` line 100 records a snippet (GamesRadar, not opened by me) that Civ VII's director says Ages fix players not reaching the end, with a "much higher completion rate" and no number. The report's main goal is finishing, yet that counter-evidence never reaches the text. It may be weak, but it should be stated.

### Gaps
- No replacement sources were sought for the four unopenable links. Old World's current event count and Aftermath's underlying Game File interview were not checked.

## Are the project-based claims true?

### Takeaway
All the core code claims hold, and two are understated: there is no player-visible difficulty text at all, and the autosave limit affects ordinary maps, not only big ones. Parts of the report are also out of date because the fifty peoples were built afterwards.

### Cited Findings
All paths are under `Unity/Assets/Scripts` or `Unity/Design`, read locally on 6 Nov 2026.

- **`difficulty` unread: confirmed, and understated.** A case-insensitive search of all scripts finds it only in `World.cs` lines 110, 207 and 215 (field, parameter, assignment). No script contains the word in any other form, including interface text, so New Game has no labelled difficulty control. "Its difficulty setting" and "the setting is stored" suggest a player-facing option. Item 2's job must include building the control. Unresolved: `RealmGame.cs` line 72 calls `World.New`, and I did not read whether it passes an unnamed value in that position.
- **No audio: confirmed.** No `AudioSource`, `AudioClip`, `AudioListener`, `PlayOneShot`, `AudioMixer` or `PlayClipAtPoint` in scripts, and no audio files under `Assets`.
- **End Turn with no check: confirmed.** `HudTheme.cs` line 94 calls `EndTurn()` from the button, `RealmGame.cs` line 67 from Enter, and `RealmGame.cs` line 31 goes straight to `world.EndTurn()`. Item 9 lists "no research" as a blocker; I did not check whether research or empty queues are filled automatically, which would shrink that item.
- **Wonder race: confirmed.** `EmpireSystems.cs` line 34 cancels rivals and pays half of invested production as gold; `IsWonder` covers ids 60-63, so 4 wonders; line 39 gives caravans 40 production.
- **Transcendence warnings: confirmed, with a gap.** `Victory.cs` line 52 warns when a rival completes a stage, only with `HasContact`. Line 101 shows the leader as "An unknown rival" otherwise. So an unmet rival gives no notification, only a line on the victory screen. "Already addressed" is too strong for rivals you have not met.
- **Others confirmed:** every-fifth-turn autosave above 9,000 tiles (`FoundationUI.cs` line 82); "Era 5 is not completed before Transcendence" (`IMPLEMENTATION-STATUS.md` line 202); hex era ranges (line 18, ranges recomputed and correct); 92-101 technologies at turn 240 and 4-6 cities (line 26); 80-84 at turn 200 (line 18); no rival wars in six test games (line 405).
- **Missing systems:** searches for barbarian, raider, encampment, great person, undo, remap and colour-blind find only flavour text. My event search used only `RandomEvent` and `EventCard`, and I ran no search for an early-finish rule, so those two rest on the report.
- **Stale: "by an amount nobody has measured" is now false.** `IMPLEMENTATION-STATUS.md` line 10 measures 54 nations at turn 165: Tiny 1.6 s, Small 1.5 s, Medium 1.5 s, saves about 6, 12 and 20 MB. The peoples the report treats as hypothetical now exist. The 217 ms figure is also from turn 30 (line 31), which the report omits.
- **Omission in the era comparison:** line 18 shows square maps at similar late figures (28/87/147/191, 47/100/157/202), so "drifted" should not read as a hex effect. The report does say hex and natives are both newer, which is fair.
- **Item 12 may partly exist.** Line 111 says flags fly on army banners. My search found `DrawFlag` in `FlagUI.cs` and `InterfaceV4.cs` but no "banner" in `IsoRenderer.cs` or `Flags.cs`. Unresolved; check before scheduling "flags at unit bases".
- **Fog history:** `IsoRenderer.cs` line 270 reads "Fog of war as dark mist (owner request): a thin drifting veil". The report says "dark drifting veil" and "darkened heavily" without noting that the darkness was requested. I did not find or check the "reads dark" complaint, so this may be a reversal the owner already wants.

### Inferences
- The fifty-peoples entry in `CLAUDE.md` postdates the report's section on the sealed worlds, so that section should be revised rather than read as current.

### Gaps
- I did not re-measure the fog contrast table. Its figures come from hand-picked areas and remain the report's own.

## Does the save compression claim survive a rerun?

### Takeaway
The 84:1 figure reproduces to the byte, but only on early saves: no late save exists on this machine. Compression would shrink files greatly, yet the claim that it "could end" the every-fifth-turn limit is unproven, because the unmeasured cost may be producing the text rather than writing it.

### Cited Findings
Run with .NET `GZipStream` in Windows PowerShell, in memory, on `C:/Users/Matth/AppData/LocalLow/DefaultCompany/Paradigm-Realm-Unity`. Save content is not reproduced.

| File | Turn | Bytes | Optimal | Ratio | Fastest ratio |
|---|---|---|---|---|---|
| earth-auto-2.json.bak | 5 | 7,676,466 | 86,863 | 88.4:1 | 55.2:1 |
| earth-auto-1.json.bak | 10 | 7,679,961 | 87,592 | 87.7:1 | 54.6:1 |
| earth-auto-0.json.bak | 15 | 7,679,595 | 88,060 | 87.2:1 | 54.2:1 |
| earth-auto-2.json | 20 | 7,683,145 | 88,634 | 86.7:1 | 53.8:1 |
| earth-auto-1.json | 25 | 7,693,268 | 89,831 | 85.6:1 | 53.3:1 |
| earth-auto-0.json | 30 | 7,700,321 | 91,463 | 84.2:1 | 52.4:1 |

- Time at Optimal was 39-73 ms (the report's 57 ms is in range), and decompression 20-26 ms. All six are save version 25, hex, 13 civilisations, 15,778 tiles on a 49 x 52 home map.
- Tiles are 82.7-83.0% of each file, and field names are 294 of the first tile's 400 characters (74%), so both report claims hold. Each file starts with byte 0x7B.
- **Latest save is turn 30.** A search of the work folder for `earth-*.json`, `earth-*.bak`, `*.sav` and `*autosave*` found nothing else.
- **Synthetic stress on the turn-30 file** (random tile integers, leaving x, y and terrain alone): 5% of values gives 60.7:1, 25% gives 32.9:1, 100% gives 18.0:1. This is a rough floor for tile data, not a late-game measurement. Non-tile data was left unchanged, and that data may grow in the late game.
- `FoundationUI.cs` line 77 is `File.WriteAllText(temporary,JsonUtility.ToJson(world,false))`. Compression adds a step after `ToJson` and removes none of its work.
- The limit applies above 9,000 tiles (line 82), and this Small game has 15,778. "On big maps" is therefore wrong: it applies at Small, and these saves' turn numbers show it in force.

### Inferences
- File size gains look safe even late, since the stress floor is still 18:1. Item 10's stated payoff, more frequent autosaves, needs a Unity timing of `ToJson` against the disk write first. If `ToJson` dominates, compression does not lift the limit.
- The six saves' timestamps run from 22:23:55 to 22:25:46, so 25 turns in 111 seconds, about 4.4 s a turn. That looks like a rapid or automated run rather than played turns, though the files do not show which. The 84:1 figure therefore also comes from an atypical game.
- The save has 13 civilisations, while the current default adds far more, and line 10 gives about 12 MB on Small late on. A current late save is the sample still needed. A first byte of 0x7B against gzip's 0x1F makes the report's "first byte" detection workable.

### Gaps
- No late or current-version 54-nation save was available, and the harness was not run to make one. Unity's `ToJson` time is unmeasured.

## Are any rankings not justified by the evidence given?

### Takeaway
The top three are defensible, but the report's grades do not follow its own legend in places, and several "High" payoffs rest on judgement. The opening's reasoning from Civ VI's completion rate to "finishing matters more than breadth" is the largest overreach.

### Cited Findings
- **Opening argument.** The report concludes "finishing matters more than adding breadth" from Beach's figure. Its last section admits the 36% median suggests Civ VI "may not be unusual" — [Two Average Gamers](https://www.twoaveragegamers.com/game-completion-rates/). Using the same metric, 37.3% against 36% is about average — [Steam](https://steamcommunity.com/stats/289070/achievements). The caveat belongs beside the claim, and its sample excludes strategy games.
- **Item 3 grade conflicts with the legend.** "Strong" rests on one pair of screenshots with hand-picked areas, while the legend puts judgement from one screenshot under Weak. Its own targets (1.5:1, 2:1) sit below the 3:1 it cites, and visible grass to sea is only 1.8:1, so 3:1 is not a useful bar here — [XAG 102](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/102).
- **Item 6 grade conflicts with the legend.** "Strong, but from one game" fits the legend's "Moderate (one good source on one game)", and the source is the game's own designer with no outcome data. "Medium" also looks low: rival handling and save memory are named, a few dozen cards must be written, and the job scale excludes tuning — [Notes #9](https://www.designer-notes.com/old-world-designer-notes-9-events/).
- **Item 9 rationale.** The article blames an "exponentially growing list of tasks", in the reporter's words rather than a Beach quote — [Game Developer](https://www.gamedeveloper.com/design/firaxis-big-swing-with-civilization-vii-convincing-players-to-actually-finish-their-games). A reminder list does not shrink that list. The report's "Weak on this fix" is honest, but "Medium" payoff is judgement.

### Inferences
- **Item 2:** "Strong" is true of the fact that it is missing. No source measures the payoff. "Small to Medium" fits flat bonuses, but "harder levels that improve rival play" is AI work and likely larger, and the missing control adds work.
- **Item 4:** "High" payoff with "Weak" evidence ranked above Moderate items is pure judgement. It is reasonable, but "the cheapest large change" is unsupported, and its one study could not be opened.
- **Item 5:** "Small to Medium" sits awkwardly with needing a new overall score first, including how natives and sealed peoples count. Its Civ VII model is too new to show results, as the report says.
- **Item 8:** "likely the largest visual gain per hour" has no measurement behind it, and it ranks above items with direct evidence. It may be right, but it is a guess.
- **Item 10:** see the save section; "Medium" payoff depends on an unmeasured bottleneck.
- **"At about 230 turns, the game's length is not the problem"** is asserted in bold. The same paragraph says real turn time is unknown, and the next says 222-250 needs re-measuring. It should read "probably not".
- **"Designers across the genre"** rests mainly on Johnson and Firaxis in what I checked.
- **Item 1 holds up:** every other judgement depends on it, and it is cheap.

### Gaps
- I did not assess items 11-22 individually beyond the citations above. "8 colours of a standard colour-safe palette" has no citation in the report and was not checked.
