# Rival AI, interface, onboarding, accessibility, audio, saves and retention for Paradigm Realm

Evidence labels used throughout:
- **[read]**: I fetched the page and worked from its text.
- **[snippet]**: I saw only a search-result summary, not the page. Check it before relying on it.
- **[weak]**: forum, Steam or vendor-blog material, or one person's opinion.
- **[measured here]**: my own check of this repository or the owner's local autosave on 5 Nov 2026.

Almost everything is paraphrased. Exact figures come from the sources named.

## What is already true of Paradigm Realm (baseline for every section)

### Takeaway
There is no audio code, and the stored difficulty value appears to do nothing. Autosaves shrink about 84:1 with plain gzip, measured on a turn-30 save. These are three cheap, high-value openings.

### Cited Findings
- **Audio [measured here]:** a search of `Unity/Assets/Scripts` for `AudioSource`, `AudioClip`, `AudioListener` and `Gamepad` found 0 matches. `Unity/Design/IMPLEMENTATION-STATUS.md` has no line mentioning sound, music or audio. A search of scripts for volume or mute found no audio setting. Scenes were not inspected, so a default `AudioListener` may exist there.
- **Difficulty [measured here]:** `difficulty` appears only in `World.cs`, as a field, a parameter (default 1) and an assignment in `World.New`. A text search found no code that reads or compares it, and no other script mentions it. Saves store `"difficulty":1`. So there is apparently no working difficulty setting. This was a text search, not a run.
- **Accessibility options [measured here]:** a search for colour-blind and rebinding terms found 0 files. Keys are hard-coded with `KeyCode`. Some interface scaling exists (`RealmGame.cs` line 35, comment in `ContentUI.cs` line 54), but I did not audit it.
- **Save size [measured here]:** on `earth-auto-0.json` (turn 30, 7,700,321 characters), the `tiles` array is 6,371,403 characters (83%) and `nations` is 1,253,752 (16%). Units, cities and diplomacy together are about 72,000.
- **Tile cost [measured here]:** one default tile is 404 characters, and 294 of those (73%) are field names, quotes and colons. There are 26 fields per tile (`World.cs` line 7). At this point only 687 tiles have `"seen":true` and 26 have `"road":true`.
- **Gzip [measured here]:** .NET `GZipStream` at default settings took the file to 91,463 bytes in 57 ms, about 84:1. This ran in Windows PowerShell 5.1, not Unity. It is an early save with a mostly default map, so **late-game ratios will be lower and were not measured**.
- **Existing status doc:** turn times in the .NET harness with 14 civilisations at turn 30 are 89 ms on Small, 118 ms on Medium and 217 ms on Large (`IMPLEMENTATION-STATUS.md` line 23). Autosave on maps over 9,000 tiles runs only every fifth turn because saves are large (line 495). AI turn processing and autosave are listed as not profiled (line 477), which may be out of date.

### Inferences
- Items 1 to 3 below (`Save size`, `Audio`, `AI difficulty`) are low-cost, because nothing has to be replaced.

### Gaps
- No late-game save was available, so the late-game compression ratio is unknown.

## 4X AI: how shipping games structure it and how they cheat honestly

### Takeaway
Publicly documented 4X AI is thinner than its reputation. The best-sourced lesson is about perceived fairness rather than architecture: bonuses should be small, incremental and visible, while hidden or spectacular ones damage trust. Paradigm Realm currently has no difficulty ladder to apply this to.

### Cited Findings
**Soren Johnson on goals**
- "Playing to Lose" (GDC 2008) separates good AI, which plays well, from fun AI, which is enjoyable to face, and places Civ AI between them. The session was not filmed. Johnson later rebuilt it from audio and slides. — [Designer Notes](https://www.designer-notes.com/playing-to-lose-ai-and-civilization-gdc-2008/) [read]
- The abstract frames the questions: should AI act like a human, should the game bend to AI limits, should it cheat, by how much, and should it win at all? — [talk abstract](https://cedricbaudry.blogspot.com/2011/03/playing-to-lose-ai-and-civilization.html) [read]
- A slide archive exists. I did not read it, and I have not seen the talk's body. — [Designer Notes slides post](https://www.designer-notes.com/playing-to-lose-the-slides) [snippet]
- A later recording's summary has Johnson saying that players misjudge cheating in both directions: they see it where it is absent and miss it where it is present. — [Rosetta summary of the Google Tech Talk](https://rosetta.to/u/googletechtalks/playing-to-lose-ai-and-civilization-soren-johnson) [snippet]

**Soren Johnson on cheating**
All of these come from one 2009 article, [Game Developer: Game AI and Our Cheatin' Hearts](https://www.gamedeveloper.com/game-platforms/analysis-game-ai-our-cheatin-hearts) [read]:
- In symmetrical games, the AI often needs help to compete, so the real design question is which help feels acceptable.
- Small, incremental AI discounts in Civilization drew little complaint. Free units appearing under fog of war and instantly completed Wonders felt unfair.
- The original Civ's AI declaring war on a leading human looked like ganging up. Civ3's AI trading with itself looked like a cartel, so Civ4 limited AI trading.
- Puzzle Quest's AI had only occasional lucky refills, and players still assumed it cheated. Johnson suggests cheating only in the player's favour and tuning AI strength through openly explained items.
- Open catch-up rules such as Mario Kart's item boxes and Catan's robber are accepted, because everyone can see them. His summary: "transparency is the antidote to suspicion and distrust."
- Players separate difficulty, which they asked for, from cheating, which they read as arbitrary punishment.

**Civilization IV structure**
- Evidence is community reading of the SDK. City-site scoring lives in `CvPlayerAI::AI_foundValue`, and tech choice in `CvPlayerAI::AI_bestTech`, which is long and hard to follow. — [CivFanatics on AI_foundValue](https://forums.civfanatics.com/threads/does-anybody-understand-ai_foundvalue.342364/post-8643371); [CivFanatics on tech choice](https://forums.civfanatics.com/threads/3-19-ai-tech-choice-code.365163) [snippet, weak]
- One fan estimate says flavour affects tech choice about a third of the time. It is not a documented figure. — [same thread](https://forums.civfanatics.com/threads/3-19-ai-tech-choice-code.365163) [snippet, weak]

**Old World**
- Update 116 added a new easiest level, The New, and reworked AI penalties at low difficulty, so handicaps run both ways. It also fixed units attacking while healthy neighbours retreated, unprotected attacks at the fog edge, and fort spam. — [Mohawk, Update 116](https://mohawkgames.com/2023/11/22/old-world-update-116/) [read]
- The same notes show passive AI no longer capturing a city from the player who founded it. — [Mohawk, Update 116](https://mohawkgames.com/2023/11/22/old-world-update-116/) [read]
- Johnson argued that without ambition victories the AI would plainly be playing a different game from the human. — [CivFanatics, Old World designer notes #11](https://civfanatics.com/?p=5981) [snippet]

**Humankind**
- Avatars combine archetypes (tendencies such as planning horizon and loyalty versus backstabbing), biases (specific obsessions) and strengths (passive bonuses), bought from a limited score. — [Amplitude Feature Focus 09](https://community.amplitude-studios.com/amplitudestudios/humankind/blogs/751-humankind-feature-focus-09-avatar-ai?page=-4) [read]
- An Amplitude developer reportedly did not want resource bonuses to hide bad AI behaviour. Players said top-difficulty bonuses forced one style of play. — [Amplitude forum, AI feedback](https://community.amplitude-studios.com/amplitude-studios/humankind/forums/210-humankind/threads/39510-feedback-ai) [snippet, weak]
- Early AI feedback was skewed by a bug in difficulty selection, a warning to verify the setting before judging AI. — [Amplitude forum, closed beta AI feedback](https://community.amplitude-studios.com/amplitude-studios/humankind/forums/212-closed-beta/threads/40606-feedback-artificial-intelligence?page=1) [snippet]

**Civilization VI personality**
- Each leader has one visible and one hidden agenda driving likes and dislikes. Critics say fixed agendas make civilisations repeat across games. — [GameRevolution](https://www.gamerevolution.com/?p=12915) [snippet]; [Steam discussion](https://steamcommunity.com/app/289070/discussions/0/4755222397144913773) [snippet, weak]

**Technique references**
- Game AI Pro has free chapters on utility and strategy, listed by title on the site. Titles relevant here: An Introduction to Utility Theory (Graham), Choosing Effective Utility-Based Considerations (Lewis), Dual-Utility Reasoning (Dill), Spatial Reasoning for Strategic Decision Making (Dill), Modular Tactical Influence Maps (Mark), Pitfalls and Solutions When Using Monte-Carlo Tree Search for Strategy and Tactical Games (Roelofs). None is listed as a 4X case study. — [Game AI Pro index](http://www.gameaipro.com/) [read, titles only]
- Dual utility: the book preview describes utility decisions as heuristic scoring at decision time, with Zoo Tycoon 2 as a use. — [O'Reilly preview](https://www.oreilly.com/library/view/game-ai-pro/9781482254792/K23980_C003.xhtml) [snippet]
- A low-quality summary describes it as using absolute utility to pick a top category, then weighted random within it. Check this against the [chapter PDF](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter03_Dual-Utility_Reasoning.pdf), which I downloaded but could not extract. — [Steemit summary](https://steemit.steemapps.com/invengaming/@livingstonenice/utility-based-ai-games-conclusion) [snippet, weak]
- Player modelling in Civ IV found exact preference values hard to predict and suggested modelling play styles instead. — [Spronck and den Teuling, AIIDE](https://ojs.aaai.org/index.php/AIIDE/article/view/12409) [snippet]

### Inferences
- **Make `difficulty` real.** Expose it in New Game and state exactly what it does, for example per-level percentages on rival production and research. Add at least one level below normal where the AI is penalised.
- **Bonus design:** use continuous discounts, never free units in fog or instant Wonders. Show them on a screen the player can read.
- **Human-looking mistakes:** Update 116 suggests the highest-value fixes are coordination bugs, such as attack and retreat mixes and units exposed at fog edges, not smarter planning. The project's own combat balance was noted as unexercised, since no rivals went to war in six old test games (`IMPLEMENTATION-STATUS.md` line 397). Re-check this, as that line may predate later AI work.
- **Personality:** `CivLore.cs` already has an agenda per civilisation. A small randomised bias per game, in the Humankind style, would answer the fixed-agenda complaint.
- **Architecture:** weighted scoring plus a randomised choice among near-equal options fits the existing rules-based rivals. I found no sourced evidence that a 4X needs search or machine learning.

### Gaps
- No primary source on Old World's or Humankind's AI architecture, or Civ VI and VII internals. Johnson's GDC 2022 [Old World postmortem](https://www.gamedeveloper.com/press-release/soren-johnson-delivers-my-elephant-in-the-room-an-old-world-postmortem-at-gdc-2022) may cover it; I did not find the content.
- The exact bonus tables of Civ IV, Old World and Humankind were not sourced. A Steam thread asking about Humankind's got no definite answer.
- Firaxis has not, in what I found, described Civ IV's system as "utility AI". The label is my inference from scoring-function names.

## Interface: reducing busywork, automation, alerts and end-turn flow; lessons from Civ VII

### Takeaway
Firaxis's own numbers tie late-game management load to players not finishing. Civ VII then showed that a heavy systems redesign does not excuse a weak interface. Detailed published design guidance on alerts and end-turn flow is scarce.

### Cited Findings
- Beach: a game takes about 15 to 20 hours, and management grows with the empire, leaving a longer list to clear before ending a turn. Firaxis audited each system for strategic decisions asked for too often and for repetitive clicking of little interest. — [Game Developer](https://gamedeveloper.com/design/firaxis-big-swing-with-civilization-vii-convincing-players-to-actually-finish-their-games) [read]
- Firaxis's chosen fix was structural (Ages), and the article does not mention automation. — [Game Developer](https://gamedeveloper.com/design/firaxis-big-swing-with-civilization-vii-convincing-players-to-actually-finish-their-games) [read]
- After launch, Firaxis called the UI its top priority, naming more intuitive interactions, map readability and formatting polish. — [Firaxis message](https://civilization.2k.com/en-GB/civ-vii/news/message-from-firaxis-games/) [read]
- A critic reports the UI was overhauled close to release, and noted that UI polish appears four times on the roadmap, alongside auto-explore, quick move and city renaming, all features players expected. — [Aftermath](https://aftermath.site/civilization-vii-civ-7-update-ui/) [read, journalist opinion]
- Update 1.1.1 added quick move and more UI changes. — [GamingOnLinux](https://www.gamingonlinux.com/2025/03/civilization-vii-update-1-1-1-adds-quick-move-lots-of-ui-improvements-a-new-start-position-and-lots-of-fixes/comment_id=277532) [snippet]
- Reports that under half of Steam players rated it well at launch. — [80 Level](https://80.lv/articles/less-than-half-of-civilization-vii-s-players-on-steam-are-happy-with-the-game/) [title only, weak: reviews mixed many complaints, so this does not isolate the UI]
- Bart Stewart's opinion piece says strategy is often settled by the two-thirds or three-quarters mark. He proposes standing goals pursued until complete or impossible, and having the game announce a decided outcome and offer to end it. — [Game Developer blog](https://www.gamedeveloper.com/design/five-suggestions-for-4x-fun) [read, weak]
- One analysis doubts governors cure micromanagement, and other commenters want tools that reduce tedium without removing control. — [SpaceSector](https://www.spacesector.com/blog/?p=8249); [Stars in Shadow forum](https://stars-in-shadow.com/forum/viewtopic.php?p=453) [snippet, weak]
- Unity's manual says IMGUI is mainly a programmer tool, for debug displays and editor extensions, and is not generally meant for normal player-facing UI. It points to the GameObject-based UI system for that. — [Unity 6000.3 IMGUI guide](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/ui-imgui/guiscripting-guide) [read]

### Inferences
- **Busywork audit:** apply Firaxis's test to each screen, asking how often a decision really needs asking and how many clicks it takes. Late-game turn click count is a cheap harness metric.
- **Do not repeat the Civ VII trap:** list what a genre player assumes on day one, including auto-explore, quick move, repeat orders, and map lenses. The project already has several (auto-explore, worker auto-develop, R road routes, Tab for next unit, Governor, notification cards). Quick move and a yields lens are worth checking, since I did not audit them.
- **End-turn flow (proposal, not sourced):** one button that walks through blockers such as idle units, empty city queues and research, with a clear path to skip or set standing orders. Show a compact "needs you" list instead of a stream of cards.
- **Decided games:** Stewart's point suits Paradigm Realm's long Transcendence race. Offering "the outcome is decided, end or continue" is cheap, though it rests on opinion.
- **IMGUI:** Unity's warning is a tooling and layout concern, not a defect. A full rewrite would be expensive and I found no evidence it would lift player experience. Scaling and readability are the practical risks.

### Gaps
- I found no designer-authored, data-backed study of alert design or end-turn flow in 4X.
- Civ VII's exact UI changes were not itemised in what I read. Official notes for later patches were not reviewed.
- Whether Ages actually raised completion: the [GamesRadar headline](https://www.gamesradar.com/games/strategy/civ-7-director-thought-the-new-ages-system-might-not-work-but-says-it-does-fix-the-number-one-issue-players-not-getting-to-the-end-of-their-games) says Beach believes so, but the body text was not retrievable and I saw no figures.

## Onboarding: what is known about 4X new-player drop-off and tutorials

### Takeaway
The only solid genre figure is Firaxis's statement that fewer than half of Civ VI players finished a game. Reliable public data on tutorial drop-off in PC 4X was not found, so the project should measure its own funnel locally.

### Cited Findings
- Beach said under 50% of Civ VI players finished a game, a share he found depressingly low. — [Game Developer](https://gamedeveloper.com/design/firaxis-big-swing-with-civilization-vii-convincing-players-to-actually-finish-their-games) [read]; [Automaton](https://automaton-media.com/articles/newsjp/civilization-20240824-307624/) [read]
- A secondhand summary of a New York Times interview gives under 40% instead. This conflicts with the above, or may be a different measure, and I could not verify it. — [search summary, source not confirmed] [snippet, unverified]
- That figure concerns finishing games, not tutorial completion. It says nothing about first-session drop-off.
- Old World's tutorial work: Mohawk spaced out tutorial events so they cluster less on one turn, and added an easier lowest difficulty. — [Mohawk, Update 116](https://mohawkgames.com/2023/11/22/old-world-update-116/) [read]
- General guidance recommends tracking tutorial steps as a funnel to find the largest drop. — [Bugnet](https://bugnet.io/blog/what-to-do-when-your-onboarding-is-losing-players) [snippet, weak: vendor blog, mostly mobile context]
- Stardock wrote a GalCiv IV onboarding dev journal, but the page refused access (HTTP 403), so its content is unknown. — [GalCiv IV Dev Journal #9](https://forums.deadmansdrawgame.com/507233) [not read]
- Accessibility guidelines list interactive tutorials as Basic. Contextual help, objective reminders and a practice mode without failure are Intermediate. Replayable instructions are Advanced. — [Game Accessibility Guidelines](https://gameaccessibilityguidelines.com/full-list/) [read]

### Inferences
- **Twelve steps cover a first turn but not the first victory path.** Reminders of current objectives and contextual tips after the tutorial are likely to matter more than extra steps. This is reasoned, not evidenced.
- **Local funnel:** the tutorial already keeps progress in PlayerPrefs. Add a local, opt-in log of step reached, turn reached and session count, for testers to share. Any online telemetry needs consent.
- **First-game defaults:** Tiny or Small map, few rivals, an easier level, short pacing. Finishing a first game seems more valuable than breadth. Fourteen civilisations plus natives and five parallel worlds is a lot to show a new player.
- **Space out teaching:** the Old World note suggests avoiding several tutorial prompts on one turn. Check that with Divus Maximus's steps.

### Gaps
- No reliable PC 4X tutorial completion or first-session retention figures. Figures I saw in blogs were mobile, unsourced or vendor marketing, so I have deliberately not reproduced them.
- Paradox, Amplitude and Mohawk funnel data: none found.

## Accessibility: Game Accessibility Guidelines items relevant to a turn-based game

### Takeaway
Turn-based play already avoids timing barriers. What remains is mostly vision, input and settings: colour independence, font size, remapping, volume separation once audio exists, and difficulty.

### Cited Findings
All items paraphrased from the [Game Accessibility Guidelines full list](https://gameaccessibilityguidelines.com/full-list/) [read], with status from my text searches of the scripts [measured here]:

| Level | Guideline | Paradigm Realm status |
|---|---|---|
| Basic, vision | Information never carried by colour alone | **Check:** borders, civ colours and terrain probably rely on colour; no colour-blind option found |
| Basic, vision | High contrast of text and UI against background | Unchecked; dark navy and gold theme needs a contrast test |
| Basic, vision | Readable default font size | Unchecked; `hudSmall` is 12 before any scaling |
| Basic, motor | Remappable controls | **Absent** (hard-coded `KeyCode`) |
| Basic, motor | Simple controls or a simpler alternative | Partial; left selects, right directs |
| Basic, motor | Adjustable game speed | Partly natural; consider animation speed and skip for AI turns |
| Basic, cognitive | Interactive tutorial | **Present** |
| Basic, cognitive | Simple clear language; text at own pace | Unchecked; turn-based helps |
| Basic, hearing | Separate volume controls; nothing essential by sound alone | **Not applicable yet; build in from the start of audio** |
| Basic, general | Settings remembered | Partly; tutorial setting uses PlayerPrefs |
| Basic, general | Wide choice of difficulty levels | **Absent in effect** |
| Basic, general | List accessibility features in-game; ask for feedback | Absent |
| Intermediate, cognitive | Objective reminders, contextual help, practice mode | Unchecked |
| Intermediate, motor | More than one input device; resizable UI | No controller; some UI scale exists |
| Intermediate, general | Autosave and manual save | **Present** |
| Intermediate, general | Change difficulty during play; expose variables | Absent |
| Advanced, vision | Adjustable font size; screen-reader support | Absent; screen-reader support would be hard under IMGUI |
| Advanced, cognitive | Replay instructions | Unchecked |

### Inferences
- **Highest value for lowest cost:** a colour-independent cue for owners (flag or pattern on borders and plates), font scaling, key remapping, and difficulty that can change mid-game. Controller support is a later, larger job that I found no evidence to prioritise for a mouse-led 4X.
- A turn-based game is a strong fit for screen-reader work in principle. Doing it through IMGUI is likely to be costly, so treat it as long-term.

### Gaps
- I did not test contrast ratios or the running game. All statuses come from text searches and docs, and "Unchecked" means exactly that.
- No figures found for how many 4X players use these options.

## Audio: how much music and interface sound matter, and licensing for a small game

### Takeaway
Research supports that sound affects experience, but samples are small and none I found covers a 4X. Having no audio at all is a clear gap, and it can be closed cheaply with interface sounds plus a licensed small score.

### Cited Findings
- A 32-participant study of the game Loddlenaut varied ambient and character sounds on and off. Character sounds significantly affected flow and fun, and ambient sound affected all measured aspects. The authors suggest effects may differ by genre. — [Applied Sciences 2024](https://mdpi.com/2076-3417/14/2/583) [snippet: page returned 403, abstract seen in search]; [DOAJ record](https://doaj.org/article/ddde88ba732a4177853dd2f4b4cbd6a6)
- A literature review reports that background music was linked to higher immersion, emotion and flow. This is secondary, and I did not check the underlying studies. — [Mesa journal PDF](https://www.mesaonline.org/wp-content/uploads/2024/03/23.02-Journal-RSG.pdf) [snippet]
- No large peer-reviewed music-on versus music-off study turned up. An unpublished dissertation with 10 participants is too weak to use. — [Portsmouth dissertation](https://www.library.port.ac.uk/dissert/dis13459.html) [snippet, weak]
- **Incompetech:** default licence is Creative Commons Attribution 4.0, credit must not be obscured, and for games the FAQ says a settings-menu Credits screen is the usual place. Standard licences exist where credit is impossible, with price not on the FAQ page. — [Incompetech FAQ](https://incompetech.com/music/royalty-free/faq.html) [read]
- **Licence traps:** CC BY attribution is mandatory, any NC licence rules out sold or monetised games, CC0 needs no credit, and "royalty-free" is not itself a licence. One site can host mixed terms, so log each track's licence and source. — [Wayline](https://www.wayline.io/blog/creative-commons-license-explained-game-developers); [GTStu guide](https://gtstu.com/free-royalty-free-music-indie-games/) [snippet, weak: blogs; verify against licence text]
- **Commissioning:** industry range is wide, and small budgets often choose library music. No reliable rates found. — [Ninichi Music](https://ninichimusic.com/blog/understanding-how-much-an-indie-game-music-composer-costs) [snippet, weak]

### Inferences
- **Order of work:** interface sounds first (click, end turn, city founded, battle result, research done, leader greeting). They give feedback at low cost, and studies lean more firmly towards effects from sound than from music. Then 6 to 10 long-loop tracks, perhaps per era, with a pause between tracks because 4X sessions are long.
- **Audio settings from day one:** separate music, effects and ambient sliders plus mute. Never carry vital information by sound alone (Basic items above).
- **Licensing:** CC0 or CC BY only, a track log in the repo and a Credits screen. Preserve the owner's art rules by adding audio only, and keep the repo's public status in mind when committing licensed files. Check the terms allow redistribution in a public repository.
- **Unity integration:** a single persistent audio object would suit IMGUI, since `GUI.Button` returns can trigger clips. Standard API use, not checked against a doc page here.

### Gaps
- No 4X-specific audio study or retention data found.
- Claims often repeated, such as famous Civ themes lifting sales, were not sourced and are excluded.
- SpriteCook is an art tool, and I did not research AI music licensing. Treat its terms as an open question if the owner considers it.

## Save size and turn time: encoding techniques and JsonUtility limits

### Takeaway
Gzip alone is a very high-return first step, about 84:1 on a measured early save. Removing per-tile field names by packing tiles into arrays cuts the in-memory JSON string and later load cost. JsonUtility's limits (fields only, no Dictionary, no bare arrays) are easy to live with.

### Cited Findings
All from the official [Unity 6000.3 JSON serialization manual](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/script-serialization/json-serialization) [read]:
- It serialises fields only, for `[Serializable]` classes and structs, MonoBehaviours and ScriptableObjects. `Dictionary<>` is not supported. Bare primitives or arrays must be wrapped in a class or struct.
- Unknown JSON fields are ignored on load, and missing fields keep their constructed values. This is useful for migration.
- Unity says benchmarks show it is significantly faster than popular .NET JSON libraries, with fewer features in some cases.
- `ToJson` allocates GC memory only for the returned string. `FromJson` allocates for the returned object and any subobjects. `FromJsonOverwrite` allocates only as needed for written fields.
- It may be used from a background thread, provided the object is not touched by another thread during the operation.
- Pretty-printing is off by default. `[NonSerialized]` omits a field.
- Measurements for this project are in the baseline section at the top [measured here].
- Save paths today: `FoundationUI.cs` lines 77 to 90 use `JsonUtility.ToJson` and `File.WriteAllText`, keep three rotating autosaves named `earth-auto-(turn%3).json`, and restore with `FromJson` then `UpgradeSave` [measured here].

### Inferences
These follow from the measurements and docs above. None has been built or run in Unity.
1. **Gzip the file.** Lowest risk, likely under a day of work. Detect old saves by first byte (gzip begins `1F 8B`, JSON begins `{`), so plain saves still load and no version bump is strictly needed. Keep a developer switch for readable JSON. Confirm `System.IO.Compression` compiles under the project's Unity API profile and in the standalone build; **I did not verify that in Unity**.
2. **Compress and write off the main thread.** `ToJson` happens on the main thread, or on a background thread only with the world frozen. The resulting string is immutable, so gzip and file write can then go to a worker. Wait for it before starting the next save and on quit.
3. **Pack tiles.** Replace `List<Tile>` serialisation with a wrapper of parallel arrays: position is implied by index, and booleans become one bit-flag int. Run-length encode terrain and owner, as both come in long runs. Store `remembered*` fields sparsely, only for seen tiles. Keep `Tile` objects at runtime, and fill them at load. This needs a save version bump with a migration, as the working rules require, since `[NonSerialized]` on the tile list changes the format.
4. **Priority of 2 versus 3:** the string is about 7.7 million characters, or about 15 MB of UTF-16 before gzip, allocated every autosave. Packing matters most for that spike and for Large (29 MB) and above. Measure it in Unity first, since the status doc says autosave has not been profiled.
5. **Remove the every-fifth-turn autosave limit** on maps over 9,000 tiles if the above brings saves down to a small fraction. Note that a long gap between autosaves also counts against the Intermediate accessibility item.
6. **Nations next:** `nations` is 16% of the file and probably carries per-civilisation map data. I did not inspect why it is so large.
7. **Turn time:** the harness numbers (89 to 217 ms) are not a problem by themselves, at about 89 ms on Small at turn 30. Late-game autosave plus AI processing in real Unity is the unknown, and harness numbers are not Unity timings.

### Gaps
- Late-game compression and Unity-side timing are unmeasured. Autosave cost in a real build is unknown.
- Official Unity docs on `System.IO.Compression` and JsonUtility's string or array size limits: I did not find a stated limit in the page I read, so no limit is claimed.
- Commercial 4X save formats: no published detail found.
- Shipped 4X turn-time techniques: I found no primary source. Searches for Old World performance returned nothing useful.

## Retention and the rest: what lifts experience beyond core rules

### Takeaway
The best-evidenced retention problem is not finishing, and it is tied to management load and pacing. Paradigm Realm's content is already broad, so the case for more systems is weaker than the case for completion, difficulty and polish.

### Cited Findings
- Beach called poor completion a major issue and said players hated Civ VI's pacing. Firaxis split the game into chapters with partial resets. — [Game Developer](https://gamedeveloper.com/design/firaxis-big-swing-with-civilization-vii-convincing-players-to-actually-finish-their-games) [read]
- Civ VII's reception shows such restructuring brings risk of its own: critics questioned whether it still felt like Civ. — [Aftermath tag page on Firaxis](https://aftermath.site/tag/firaxis/) [snippet, weak]
- Stewart: the game should let players read opposing strategies from what they observe, and should stop asking for mechanical turns once the result is decided. — [Game Developer blog](https://www.gamedeveloper.com/design/five-suggestions-for-4x-fun) [read, weak]
- Late-game fixes proposed by the community include regional governors, stronger late threats, and victory rules that curb bloat. — [GameDev forum](https://nene.gamedev.net/forums/topic/710037-imperium-a-4x-concept-for-an-exciting-late-game-reduced-micromanagement-and-other-improvements); [Paradox forum](https://forum.paradoxplaza.com/forum/threads/how-could-4x-be-fun-till-the-end-endgame-discussion.1113720/page-6) [snippet, weak]
- Accessibility guidelines also ask for summaries of progress in long narratives and for settings to be saved. — [Game Accessibility Guidelines](https://gameaccessibilityguidelines.com/full-list/) [read]

### Inferences
Ranked suggestions. These are my judgement from the evidence above, not a sourced ranking:
1. Working difficulty with visible bonuses, including an easier level.
2. Sound: interface effects first, then licensed music.
3. Gzip saves.
4. End-turn blocker list and late-game click count metric.
5. Colour-independent owner cues, font scaling and key remapping.
6. A local tutorial and first-game funnel, plus objective reminders.
7. End-of-game timeline or chronicle, and "decided game" prompt. The existing Chronicle and three-bar victory screen are a base.
8. Hands-on play testing with real new players, since `CLAUDE.md` records that interactive play is still largely unverified. This outranks any new system.

Also for the owner: Steam achievements, mod support and workshop were not researched and have no evidence behind them here.

### Gaps
- No published 4X retention curves or evidence on which features raise replay.
- Whether adding end-of-game replay or achievements lifts retention in 4X: not sourced.
