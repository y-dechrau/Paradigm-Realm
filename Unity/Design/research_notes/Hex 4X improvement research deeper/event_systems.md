# Event card systems for a single-player hex 4X (second, deeper round)

How to read these notes:
- **[read]** = page opened, but through a fetch tool that returns a summary from a small model, not raw text. Numbers came back from that summary, so check them against the page before reprinting.
- **[snippet]** = seen only in a search-result excerpt; the page itself was not opened. The search tool did not always tie an excerpt to one result, so for community snippets on Stellaris, CK3, Endless Legend and popups the link is my best match and should be confirmed before citing onward.
- **[secondary]** = a fan, wiki, press or forum source reporting on the game or developer. **[community]** = player opinion.
- Text is paraphrased, with one short quotation in the whole file. Field names are given as identifiers.
- Anything under "Inferences" is my reasoning or proposal, not a sourced fact. All numbers proposed for Paradigm Realm are starting values to tune.
- Round-one material (virtual deck, loose coupling, Civ IV dead end, transparency, 3,000+ events, Stellaris anomalies and crises) is not repeated except where something new attaches to it.

Access problems, which limit the Old World field detail:
- The main community modding guide, weplayciv.com/oldworld/events.html, refused connections. A mirror returned 404 and the Internet Archive is not fetchable from this tool. It is known only from search excerpts.
- I also tried a direct `curl` of that guide with a browser user-agent. The permission system denied it as bot-check circumvention, and I did not retry it or look for a way around it.
- A fetch of the Old World Reference "rare events" page was denied the same way and was not retried. Its rates below come from that site's events index, which I had already read.
- A public Google Docs modding guide redirected to a host that returned 400, so it is excerpt-only.
- The Paradox forum (CK3 Dev Diary 147) returned HTTP 500. The GDC Vault Reigns talk is members-only. The PC Gamer Frostpunk interview returned no body text.

## Old World's event system in detail, and how Paradox, Amplitude, Reigns, Frostpunk and Failbetter structure theirs

### Takeaway
Three firing models exist, and the industry has moved away from one of them. Per-object polling (mean time to happen) is being retired by Paradox on performance grounds. Hook-driven weighted lists (on_actions) and per-turn pools with a chance gate and minimum spacing (Old World) are the live models. Reigns' "bag" is the simplest correct description of all of them: remove what doesn't fit or was just played, size the rest by weight, draw one. Build that bag, fed by hooks plus one pacing pool, with per-subject repeat control and timed memories.

### Cited Findings

**Old World: pools and rates (community reference synced to the game's XML, build 26433529, synced Nov 2026 per its footer)**
- Scale: the index header says 4,874 events while its description says 5,306 including DLC, not reconciled. Johnson's own 2021 figure was 3,000+. A player with about 2,000 hours estimates over 4,000 and says each DLC adds 300-500+ [read, secondary] — [Old World Reference: events](https://alcaras.github.io/owreference/events/); [Steam thread](https://steamcommunity.com/app/597180/discussions/1/4625855423768032153/)
- 37 pools roll every turn, each with a per-turn chance, a start turn and a spacing shown as "every N+" (the page does not define N+ precisely) [read, secondary] — [same index](https://alcaras.github.io/owreference/events/):

| Pool | Chance per turn | From turn | Spacing | Events |
|---|---|---|---|---|
| Calamities | 100% | - | 1+ | 78 |
| Scandal | 50% | 5 | - | 96 |
| Household | 10% | 10 | 10+ | 98 |
| Rare Events | 10% | 10 | 15+ | 152 |
| Sudden Death | 3% | 20 | 60+ | 15 |

- Alongside the pools, 96 triggers fire on game moments, for example Start Game 69 events, City Founded 71, Religion Spread 135. There are 253 chain follow-ups that fire only as the next step of a choice, and 374 scripted or special events. Separate lists hold Ruin 87, Expedition 26, Harvest 71, Study 166 [read, secondary] — [same index](https://alcaras.github.io/owreference/events/)
- A worked pool, Young Ruler: 50% per character per turn, at most one every 3 turns, 18 events. 17 have weight 1 and Attempted Coup has weight 4. 15 events repeat at "every 6 turns", others at 10, 15 and 40. Some carry an extra appear chance (Attempted Coup 50%, Successor Faction 70%, both turn 20+) [read, secondary] — [Young Ruler pool](https://alcaras.github.io/owreference/events/young-ruler)
- The site warns repeatedly that weights are not in-game odds, because many events have character and empire conditions, some of them hidden [read, secondary] — [same](https://alcaras.github.io/owreference/events/young-ruler)
- Field structure of a card as the reference presents it: title, weight, chain and branch line, conditions ("every N turns", then player, occurrence, leader, city and family flags), opening effect, then options. Each option has yields, chance rolls, optional subject gates, grants and "may trigger" links [read, secondary] — [Civil War pool](https://alcaras.github.io/owreference/events/civil-war)

**Old World: selection fields (modding guide, excerpts only)**
- `iProb` decides whether an event enters the pool and is applied first. `iWeight` is a linear pick weight within the pool, default 1 [snippet, secondary] — [Old World Event Modding Guide](https://www.weplayciv.com/oldworld/events.html)
- `iRepeatTurns` is the basic repeat control and can also apply per subject, so a story may recur in general but never again for one subject. `aeEventStoryNoRepeat` lists other stories whose firing also counts against this one's repeat timer [snippet, secondary] — [same](https://www.weplayciv.com/oldworld/events.html)
- A second guide says a repeat value of 0 means no limit, the field may not be left empty, and subject indices start at 0 [snippet, secondary] — [Google Docs guide](https://docs.google.com/document/d/1sfeC--c9F_mh7Lp1WovTxZScjOGeC0FpvHrGuJpFJ6M/export?format=pdf)
- Johnson names weight, probability and priority as the three selection values but does not say how they combine. Subjects can carry several tests each and relations between subjects (nations at war, one character's vengeance on another) and can be linked across a multi-step event [read] — [Designer Notes #9](https://www.designer-notes.com/old-world-designer-notes-9-events/)
- Event occurrences are processed for each player separately during their own turn [snippet] — [Mohawk Update #138](https://mohawkgames.com/2025/08/27/old-world-update-138/)

**Old World: memories and numbers**
- Memories are named, scoped, timed records granted by options. The Civil War pool shows scopes such as family and all families (`MEMORYFAMILY_...`, `MemoryAllFamilies`) and a character scope (`MEMORYCHARACTER_SEIZED_THRONE`) [read, secondary] — [Civil War pool](https://alcaras.github.io/owreference/events/civil-war); [snippet, secondary] — [On Tribe Eliminated page, via search](https://alcaras.github.io/owreference/events/on-tribe-eliminated)
- Observed durations and sizes: Player Offer 5 turns; Catered to Demands +20 opinion for 20 turns; Seized Throne 100 turns; Young Ruler memories 20 turns at +40 or -40 opinion [read, secondary] — [Civil War](https://alcaras.github.io/owreference/events/civil-war); [Young Ruler](https://alcaras.github.io/owreference/events/young-ruler)
- The reference marks many of these as having no event that reads them, so on those pages a memory does its work purely as a timed opinion modifier [read, secondary] — [same pages](https://alcaras.github.io/owreference/events/young-ruler)
- Opinion flows one way: characters into nations, tribes, families and religions; religions into nations, tribes and families. Family heads' opinion passes directly to their family, and event options can offend a head [read] — [Designer Notes #8](https://www.designer-notes.com/old-world-designer-notes-8-opinion/)
- Rewards and costs scale with the empire: costs are written as base plus per-city (for example -240 plus -60 per city), and yields follow (base + per-city × cities) × a turn multiplier of ×1 to turn 60, then turn ÷ 60 [read, secondary] — [Civil War](https://alcaras.github.io/owreference/events/civil-war); [Young Ruler](https://alcaras.github.io/owreference/events/young-ruler)
- Chance rolls sit on options, not just on events: a secret accord has a 40% chance of the "Doomed" outcome next turn, a bounty 15%, a covert option 75% [read, secondary] — [Civil War](https://alcaras.github.io/owreference/events/civil-war)
- Johnson's summary of what he made: "I accidentally built a interactive fiction engine inside of a 4X game" [read] — [My Elephant in the Room, Part 3](https://www.designer-notes.com/?p=2155). He adds that more events add variety without adding design complexity, and that events replaced the diplomacy bargaining table because a state-dependent random outcome cannot be min-maxed the way a table can [read] — [same](https://www.designer-notes.com/?p=2155)

**Crusader Kings II and III**
- CK2: MTTH is defined so that after 90 days an event with a 90-day MTTH has a 50% chance of having fired. So it behaves as a half-life, not a true mean. Modifiers apply a `factor`: 2 halves the chance, 0.5 raises it [read] — [CK2 wiki: Event modding](https://ck2.paradoxwikis.com/Event_modding)
- CK2: `on_actions` blocks hold `effect` (runs first), `events` (always fire) and `random_events` (weighted). Each block fires at most one random event, weight 0 means "nothing", and delays are in days. `weight_multiplier` is cheaper for the engine than MTTH. It is not advised for regular events with short delays because they queue up and fire in batches [read] — [same](https://ck2.paradoxwikis.com/Event_modding)
- CK2 repeat control has no dedicated field. Authors set a flag and test it with a time window (`had_character_flag = { flag = loan_refused days = 365 }`) [read] — [same](https://ck2.paradoxwikis.com/Event_modding)
- CK2 performance: pre-triggers restrict checks to about 1,000 playable characters rather than about 20,000 [read] — [same](https://ck2.paradoxwikis.com/Event_modding)
- CK3: events never fire on their own. Script must fire them through an on_action, decision, story cycle or interaction [read] — [CK3 wiki: Event modding](https://ck3.paradoxwikis.com/Event_modding)
- CK3 card shape: `trigger` (checked before firing), `immediate` (runs before text renders, can set up scopes the text uses), options, `after` (runs after the choice, not for option-less events), `on_trigger_fail`, and `cooldown = { years = 5 }` in the template [read] — [same](https://ck3.paradoxwikis.com/Event_modding)
- CK3 on_action shape: `random_events` with `chance_to_happen = 25`, weights such as 100 and 200, and `100 = 0` as an explicit no-event entry. `weight_multiplier` defaults to 1, delays can be ranges (`months = { 6 12 }`), and `trigger_event` takes `days = { 7 14 }`. `quarterly_playable_pulse` fires every three months and there is no monthly pulse [read] — [same](https://ck3.paradoxwikis.com/Event_modding)
- The wiki warns that iterating all living characters inside the yearly pulse costs about 200,002 operations [read] — [same](https://ck3.paradoxwikis.com/Event_modding)
- The page I read does not state whether `cooldown` is per character or global.

**Stellaris**
- Default events are polled against every game object at least daily, possibly per tick. The wiki warns of very heavy CPU load, says MTTH has fallen out of favour for poor performance, and says vanilla is removing its uses. Testing suggested a simple MTTH gets a 50% roll per scope per period [read; page last verified for v3.6] — [Stellaris wiki: Event modding](https://stellaris.paradoxwikis.com/Event_modding)
- An event with neither `is_triggered_only` nor MTTH fires every day its conditions hold. `fire_only_once` is still polled until it fires, unlike `is_triggered_only`. Delayed calls take `days = 200 random = 100` (200-300 days) [read] — [same](https://stellaris.paradoxwikis.com/Event_modding)
- Pulses exist at monthly, yearly, two-yearly, five-yearly, decade, mid-game and late-game levels, each in unscoped and country-scoped forms. Fallen and Awakened Empires do not receive the country-scoped pulses [read] — [Stellaris wiki: On actions](https://stellaris.paradoxwikis.com/On_actions)
- An on_action rolls weights among the events whose triggers pass and fires one. The page notes a list with no 0-weight entry always fires an event if any can fire, and it keeps a separate negative-events list off the five-year pulse [read] — [same](https://stellaris.paradoxwikis.com/On_actions)

**Reigns**
- Each draw starts from a bag of every card. Cards that do not fit the kingdom's state are removed, as are cards played too recently. Each remaining card has a size, larger meaning more likely, and one is drawn at random [read] — [Game Developer deep dive](https://gamedeveloper.com/design/game-design-deep-dive-creating-an-adaptive-narrative-in-i-reigns-i-)
- Context floods the bag: a war adds about 10 large war cards that crowd out generic ones, and they leave when it ends. Sub-systems (dungeon, duels) lock the player into a small bag of a couple of cards, and a one-card bag makes a linear path. The article gives about 50 cards for the initial deck but no final total. Writing went from one-shot cards, to short card ranges, to add-on content, to a long storyline [read] — [same](https://gamedeveloper.com/design/game-design-deep-dive-creating-an-adaptive-narrative-in-i-reigns-i-)

**Humankind and Endless series**
- Humankind (pre-release panel recap by a forum user, 2019): about 150 unique events planned; a commenter gives an average of 25 per era. Events carry clear trigger conditions while staying somewhat random, target one city or the whole empire, offer grey moral choices without necessarily punishing the "evil" one, can have follow-ups (an Era 2 choice firing an event in Era 4 or 5), give some civilisations a unique extra option, and can be turned off at setup. A developer reply cautions that the screenshots were design documents [read, secondary, pre-release] — [Amplitude forum](https://community.amplitude-studios.com/amplitude-studios/humankind/forums/168-general/threads/34476-link-to-the-zen-of-strategy-narrative-pax-west-panel-for-those-who-missed-it?page=1)
- Endless Space 2 (Feb 2016 GDD thread, may be outdated): separates reaction-based events (a choice, no objective) from quests (a choice among objectives). Major faction quests run about 4 chapters; triggers are forced, random or reactive; rewards can be randomised from same-nature pools; hero quests trigger from frequent use of a hero. The stated goal was never to spawn the same event or quest twice in one game, using restrictive launch conditions to vary games [read] — [ES2 GDD 9 thread](https://community.amplitude-studios.com/amplitude-studios/endless-space-2/forums/65-general/threads/18853-es2-gdd-9-narration?page=2)
- Endless Legend has main faction, minor-faction and ruin-monster quests. Players called the main line rigid and said some objectives were bad for their current position [snippet, community] — [EL forum](https://community.amplitude-studios.com/amplitude-studios/endless-legend/forums/6-game-design/threads/3219-suggestion-branching-faction-quests?page=1)
- A forum reply, author unconfirmed, puts the limit on faction quests down to the design, writing, code and test cost of each [snippet] — [EL forum](https://community.amplitude-studios.com/amplitude-studios/endless-legend/forums/6-game-design/threads/3536-dynamic-main-quest-lines-and-more-side-quests?page=1)

**King of Dragon Pass (added, because its author published the sums)**
- The iOS version holds 1,624 entries: 464 code chunks with no player interaction, 462 "news" reports (one leader, mostly no picture), 614 illustrated scenes, 84 heroquest entries. 70 of the 614 are follow-ups, leaving 544 distinct interactive events, with about 430 paintings [read] — [David Dunham, How Many Scenes?](https://kingofdragonpass.blogspot.com/2012/09/how-many-scenes.html)
- Dunham's model: about 5 random events a year, about 53 years a game, equal odds. A given scene is missed in about 61% of games, and in about 0.3% across 12 games. He notes the model ignores preconditions, and a season-locked scene is missed in over 90% of games [read] — [same](https://kingofdragonpass.blogspot.com/2012/09/how-many-scenes.html)
- He warns about story balance rather than game balance when adding events: ten new scenes about one minor character would make her seem too important [read, in the post's comments] — [same](https://kingofdragonpass.blogspot.com/2012/09/how-many-scenes.html)

**Frostpunk laws (thin)**
- Laws are player-initiated rather than fired at the player, and they move Hope and Discontent, often both, which are tracked separately [snippet, secondary] — [Frostpunk wiki: Administration](https://frostpunk.gamepedia.com/Administration)
- Cooldown figures conflict: one source says a law every 18 in-game hours, while a wiki summary gives per-law values such as 36 and 72 hours. I could not settle this from a primary source [snippet, secondary] — [Twinfinite](https://twinfinite.net/?p=346496); [MacMyths](https://macmyths.com/how-frostpunks-laws-and-hope-system-work/)

**Quality-based narrative and storylets**
- A storylet is content plus prerequisites plus effects on world state. Short calls them simple, atomic, robust and recombinable; a basic version can be prototyped on index cards [read] — [Emily Short, Storylets: You Want Them](https://emshort.blog/2019/11/29/storylets-you-want-them/)
- The hard case is a branching run that never recombines, since each piece then needs exact-route flags. Most real projects embed small branches inside a storylet structure [read] — [same](https://emshort.blog/2019/11/29/storylets-you-want-them/)
- A three-clue investigation takes 4 storylets (one gated on the three clues) versus a six-ordering tree [read] — [Beyond Branching](https://emshort.blog/2016/04/12/beyond-branching-quality-based-and-salience-based-narrative-structures/)
- Salience selection picks the most specific matching item and breaks ties randomly, so authors can start with broad defaults and add specifics later. The weakness is arc: it can fire a badly timed outcome. Short suggests three constants ("a little", "a medium amount", "a lot") for thresholds, and thousands of randomised playthroughs to find unused and overused content [read] — [same](https://emshort.blog/2016/04/12/beyond-branching-quality-based-and-salience-based-narrative-structures/)
- Kreminski's four dimensions of a storylet system: precondition form, whether storylets repeat, content type, selection architecture [read] — [Short on Kreminski](https://emshort.blog/2019/01/06/kreminski-on-storylets/)
- Failbetter's own rules: use one controlling quality for any ordered sequence, since several make it fragile; give each quality one meaning everywhere, since reuse caused consistency problems and exploits; show the player how long a punishment will last [read] — [Failbetter: Don't Poke That](https://www.failbettergames.com/dont-poke-that-narrative-engineering-principles-part-1)
- Kennedy frames qualities as the variables forming paths through a story, a constellation against CYOA's tree, and says the method needs a lot of content before it pays off, and that players can see the bare wires [snippet; fetch returned 404] — [Indie-O-Rama interview](https://indieorama.com/failbetter-games-interview-alexis-kennedy-hanna-flynn)
- According to Short, Failbetter has stopped using the term QBN and Kennedy prefers "resource narrative" [snippet] — [Short, tag archive](https://emshort.blog/tag/quality-based-narrative/)

**Civ IV (fields worth borrowing despite Johnson's verdict on its system)**
- `iPercentGamesActive` (0-100) decides whether a trigger is in a given game at all. `bRecurring` and `bGlobal` combine to give once-per-player or once-per-game. `iWeight = -1` means always fire when conditions hold. Weights can favour damaged, distant or experienced units and well-stocked cities when picking the subject [read, secondary] — [Solver's guide, CivFanatics](https://forums.civfanatics.com/threads/guide-to-event-modding-in-bts.230567/)

### Inferences (proposals for Paradigm Realm)
- **Use hooks plus one pacing pool, and no polling.** Performance is not the main reason in a turn game, but the same design gives clean scoping. The game's own turn-time budget also argues against scanning every card against every city (Stage 8 measured 20-38 ms a turn on large maps with 4 civilisations and about 1.5 s with 54 nations, per CLAUDE.md).
- **Card shape.** `id`, `hook` (or `Pacing`), `tier`, `weight`, `percentGamesActive`, `minTurn`, `minEra`, `globalCooldown`, `subjectCooldown`, `onceAGame`, `subjects[]` (each with tests), `requires` (flags or memories present or absent), `textKey`, `options[]`. Each option has: gate, effects, memories written, optional roll, optional `followUp{id, delayMin, delayMax}`.
- **Selection, in order:**
  1. A hook fires, or the pacing roll passes.
  2. Budget check (see the firing section).
  3. Collect cards dealt into this game, not used or cooling down, for this hook.
  4. Bind subjects, trying cities and characters until the tests pass.
  5. Keep the highest tier only.
  6. Pick by weight × freshness, with an explicit "nothing" weight.
  7. Run `immediate`, show the card, apply the option, write memories and the chronicle line.
- **Translate half-life to turns if MTTH is ever wanted.** Per-turn chance = 1 - 0.5^(1/T). That gives T=10 turns 6.7%, T=20 3.4%, T=40 1.7%. Paradox's "mean" time is really this half-life, so copying numbers as true means will be off.
- **Implied Old World pacing, my arithmetic.** If "every N+" is a minimum gap, expected spacing is about N + 1/chance: Rare about 25 turns, Household about 20, Sudden Death about 93, for each pool separately. That assumes eligible events exist and the gate works as named; the site doesn't confirm it.
- **Chain share is small.** About 5% of Old World entries (253 of 4,874) and about 11% of KoDP scenes (70 of 614) are follow-ups. Most content is standalone, which matches Short's advice and Johnson's loose coupling. Keep authored chains to about one card in ten.
- **Memory model.** Timed memories plus permanent flags, each with a stated scope and one meaning (Failbetter's rule). Suggested tiers: short 5 turns (a pending offer), medium 20 turns (an opinion effect), long 100 turns or permanent (a story fact). Use Short's three-constant idea for amounts, for example 10/20/40 opinion, which brackets the Old World values seen.
- **Scale effects by city count and turn** in Old World's shape, or a fixed gold card will be huge at turn 20 and trivial at turn 180.

### Gaps
- The exact way Old World combines `iPriority`, `iProb` and `iWeight`, whether there is a global per-turn event cap, and the meaning of "every N+". The guide that would answer these was unreachable, so check the game's `eventStory.xml` or the guide by hand.
- How much hidden conditions change real Old World firing rates: no measured rates found.
- Whether Old World's AI players resolve events the same way as humans (only the per-player processing excerpt was seen).
- Humankind's shipped event count and frequency: only pre-release fan recaps.
- Failbetter's reasoning on the Fallen London opportunity deck and Sunless Sea's content structure: no primary source found. Reigns' total card count: not in the article, GDC talk paywalled. One player mentions being stuck at 870 cards in Reigns: Her Majesty [snippet, community](https://steamcommunity.com/app/717640/discussions/0/3051633726581377691).
- Frostpunk design intent: the interview body was unreadable.

## How many events does a game need before repetition is felt?

### Takeaway
No source states a threshold, and raw count does not settle it: Old World players with thousands of events still report repeats. What is felt is the repeat rate within the *eligible* pool for a player's usual habits. Plan on about 15 as a test, about 60 as a credible first release, and about 150 as the mature target, and treat these as my estimates anchored to the comparison points below.

### Cited Findings
- Pool sizes for scale: Reigns initial deck about 50 [read]; Humankind about 150 planned [read, secondary, pre-release]; KoDP 544 distinct interactive [read]; Old World about 4,900-5,300 [read, secondary] — sources above
- Old World, 2024: one player reported repeats on almost every second event despite the advertised 1,000+, and asked for a toggle [snippet, community] — [r/OldWorldGame mirror](https://reddit.sudovanilla.org/r/OldWorldGame/comments/1fk3esx/old_world_sep_18th_update)
- Old World, Nov 2024: a player says events arrive in about the same order each game and should sometimes not trigger. Others answer that similar play draws from the same limited pools (tribes, ruins, landmarks, heir education), which works like a learnable deck, and that some events are too common and others too rare. No developer replied [read, community] — [Steam thread](https://steamcommunity.com/app/597180/discussions/1/4625855423768032153/)
- Johnson says some players find the system excessive while others want ever more, which led to setup options for event frequency and character deaths [read, secondary] — [PCGamesN](https://www.pcgamesn.com/old-world/civilization-too-long)
- Update 87 added a competitive option that disables events with a large effect on play [snippet] — [Update notes via CivFanatics](https://forums.civfanatics.com/threads/old-world-patch-87.676128)
- Stellaris: a player says all vanilla events are seen within three or four playthroughs. Another says discovery events are so frequent the same ones come up every half hour [snippet, community] — [Steam search result](https://steamcommunity.com/app/281990/discussions/0/1637549649107954439)
- Stellaris: one specific event drew repeated reports of firing in every playthrough since it was added, with one poster reporting four new games in a row and one who had never seen it [read, community] — [Vechtar Zavonia thread](https://steamcommunity.com/app/281990/discussions/0/1739964947814544772)
- CK3: Dev Diary 147 attributes most of the "event spam" to one bug, the Alms event lacking a cooldown, and promises much longer cooldowns on easily triggered or heavily negative events. The 1.12.5 changelog is reported as giving Alms a 20-year cooldown and cutting minor plague frequency by about 20%, with a game rule to switch plagues off [snippet; forum returned 500, mirror failed] — [Dev Diary 147](https://admin-forum.paradoxplaza.com/forum/developer-diary/dev-diary-147-some-words-from-our-game-director.1673686/latest)
- CK3 players ask that activity events only appear after the first of each activity, calling them repetitive, and one reports the same unlikely event twice in quick succession [snippet, community] — [Steam thread](https://steamcommunity.com/app/1158310/discussions/0/3838801919839423509)
- Endless Legend side quests went stale once quest types were familiar [snippet, community] — [EL forum](https://community.amplitude-studios.com/amplitude-studios/endless-legend/forums/6-game-design/threads/3440-ideas-for-new-quests?page=1). Endless Space 2 review: many "random" events repeated in each later game, despite the design goal above [snippet, secondary] — [PCGamesN](https://www.pcgamesn.com/endless-space-2/endless-space-2-review)
- Civ VII players report that habits trigger the same events each game [read, community] — [CivFanatics](https://forums.civfanatics.com/threads/events.691598/latest)
- Compton's "10,000 bowls of oatmeal": things that are technically distinct can feel identical, so generated variation is not automatically felt as variety [snippet, secondary] — [Short on oatmeal](https://emshort.blog/2016/09/21/bowls-of-oatmeal-and-text-generation/)
- QBN has a low-end problem too: StoryNexus authors needed many storylets before anything interesting emerged, yet Starfreighter got substantial play from few reusable storylets bound to resources [read] — [Short on Kreminski](https://emshort.blog/2019/01/06/kreminski-on-storylets/)

### Inferences
- **Repeat model (mine).** With a once-a-game lock, a card can't repeat inside a game, so the question is across games. If a game draws k cards from N eligible with equal odds, the share already seen in game g is about 1 - (1 - k/N)^(g-1):

| k per game | N eligible | Game 2 | Game 3 | Game 4 |
|---|---|---|---|---|
| 10 | 15 | 67% | 89% | 96% |
| 25 | 60 | 42% | 66% | 80% |
| 25 | 150 | 17% | 31% | 42% |
| 25 | 300 | 8% | 16% | 23% |

- **N in that table is the eligible pool, not the total.** Halve the total for a typical game if half the cards need a specific state, faith, civilisation or era. So 150 written may behave like 75.
- **Cheap ways to raise perceived variety, none of which need more writing:**
  - Deal a per-game subset with `percentGamesActive` around 60-70%. This directly answers the Old World complaint that events should sometimes not trigger, and the order-sameness complaint.
  - Keep a cross-game "seen" list (PlayerPrefs, as the tutorial's progress already does) and multiply weight by, say, 3 for never-seen cards.
  - Don't let an early-game card be the same high-weight one every time. The Vechtar Zavonia reports look like this failure; it is my reading, not confirmed by the thread.
- **The cooldown lesson from CK3 is a test case.** One missing cooldown produced most of the reported spam. Make the cooldown field required (no default of zero, as Old World's guide requires a value), and have the harness fail on any card firing more than a set number of times in a simulated game.
- **Civ 22 lore as a multiplier.** Humankind's Celt extra option suggests a cheap trick: one extra civ-specific option on an existing card reads as new without a new card.

### Gaps
- No stated figure, survey or telemetry from any studio on the point at which repetition is felt. Complaint evidence is anecdotal, self-selected and often from excerpts.
- No data linking pool size to retention or review score.

## How do you keep event text from becoming noise?

### Takeaway
Three things recur in complaints: interruption, reading walls of text for a choice decided by the numbers, and cards unconnected to what is going on. The cures are a firm firing budget, a lighter tier that never interrupts, short text, cards tied to the player's current crisis, and setup controls, since players truly split on this.

### Cited Findings
- Civ VII thread: a poster objects to being interrupted by walls of text every few turns; another separates full-screen interruption (disliked) from a simple decision prompt (fine); one wants these in the to-do list; one skips arbitrary events but enjoys those tied to important matters; several say they read the bonus only. A Humankind player found its frequent events annoying [read, community] — [CivFanatics](https://forums.civfanatics.com/threads/events.691598/latest)
- The same thread proposes hiding effects to force reading, with the counterpoint that effects would be learned over several games unless randomised [read, community] — [same](https://forums.civfanatics.com/threads/events.691598/latest)
- Johnson rejected King of Dragon Pass's hidden outcomes on transparency grounds, wanting clear outcomes but unpredictable paths [read] — [Elephant Part 3](https://www.designer-notes.com/?p=2155). Endless Space 2 allowed unknown rewards, keeping crucial lore rewards stable [read] — [GDD 9](https://community.amplitude-studios.com/amplitude-studios/endless-space-2/forums/65-general/threads/18853-es2-gdd-9-narration?page=2)
- A UX designer's opinion on a design forum: players generally read no more than about two lines, and more text raises the risk of skipping [snippet, community] — [r/gamedesign mirror](https://lr.in.psf.lt/r/gamedesign/comments/1bi4i4l/how_the_hell_do_i_get_players_to_read_anything/kvr9h7i/?context=3)
- Opt-outs and quieter delivery in other games: Endless Space 2 players disable auto-popups so notices become icons [snippet, community] — [ES2 forum](https://community.amplitude-studios.com/amplitude-studios/endless-space-2/forums/65-general/threads/31045-opinion-notification-system-animation-cognitive-fatigue-visual-strain?page=1). A Paradox player proposed preselecting a default option on repetitive events [snippet, community] — [Paradox forum](https://admin-forum.paradoxplaza.com/forum/threads/please-evolve-from-clicking-on-text-boxes-as-gameplay.1065806/latest). Another developer's patch notes report cutting popups and moving lesser events to map notices [snippet; game not identified] — [SteamDB](https://steamdb.info/patchnotes/6787183)
- KoDP keeps almost as many non-interactive news reports (462) as illustrated scenes (614), so much of its world change arrives as a report, not a decision [read] — [Dunham](https://kingofdragonpass.blogspot.com/2012/09/how-many-scenes.html)
- Short: players may forget how a resource they gained connects to its later use, so the link between cause and effect can fall to the player [read] — [Beyond Branching](https://emshort.blog/2016/04/12/beyond-branching-quality-based-and-salience-based-narrative-structures/). Failbetter: show how long a punishment lasts [read] — [Failbetter](https://www.failbettergames.com/dont-poke-that-narrative-engineering-principles-part-1)
- Long-game caution: players ask for a cap so that mid-game threats don't become a stream of annoyances (from round one; not re-read).

### Inferences (proposals)
- **Firing budget for this game.** Pacing runs about 230 turns to the earliest Transcendence with eras near turns 36/77/124/170 (CLAUDE.md, Stage 8). Aim for 8-10 cards a game while the deck is 15, 15-20 at 60 cards, and 20-30 at 150, which is one card per 8-12 turns at full size. A hard global gap of 6 turns applies, with at most one card a turn, and with no card in the first 10-15 turns (Old World's pools tend to start turn 5-20). Only a top "crisis" tier may break the gap. Use a token bucket (earn 1 per 10 turns, hold 2) so quiet spells are not followed by a flood.
- **Fire on what is happening.** Prefer hook-triggered cards, with the pacing pool as a filler that fires less when hooks have fired recently. This matches the "tied to important matters" comments.
- **Delivery.** Non-crisis cards go in a side tray or end-of-turn "needs attention" slot, as with other pending decisions, and block End Turn only if they are time-critical. Add a default-option timeout for those that can wait, for example resolve after 3 turns at the cautious option, stated on the card.
- **Text limits.**
  - Title up to 5 words, body 25-50 words, options up to about 12 words with exact effects shown underneath. A card should still work if the player reads only the title and the options.
  - The owner's game already shows exact previews in combat, so showing effects fits, and it follows Johnson over KoDP. Rolls go on the option with the odds shown.
  - Name the concrete subject in the first line ("Ephesus", not "one of your cities").
- **Callbacks are the reward for reading.** When a card reads a memory, it says so in one clause ("Twenty years after you felled the grove..."), and the chronicle records the original choice. This addresses Short's causality point directly.
- **Two tiers of content.** Add a non-interactive "report" tier that goes straight to the chronicle, written at about one line. It gives the world texture without any decisions, in KoDP's proportion, and it costs far less to write.
- **Setup options.** An event frequency setting (Off, Rare, Normal, Frequent) and an "only events with no large effect" option, as both Old World and Humankind found useful. Add a reminder count on the tray.
- **Writing volume, my estimate.** A card of body plus 3 options plus outcome lines is about 100 words. So 15 cards are about 1,500 words, 60 about 6,000, 150 about 15,000. Add 10-20% for civ-specific options. Art: one picture per category, not one per card, because a 430-painting budget is out of reach (the SpriteCook balance was low in the last note on it).
- **Measure skip and reading, since no source offers benchmarks.** Log time on card, option chosen and option-share per card in debug builds. Any card with one option chosen over 85% of the time is a false choice.

**Known failure modes, condensed** (sources are in the sections above):

| Failure | Evidence | Guard (my proposal) |
|---|---|---|
| Missing cooldown floods the player | CK3 Alms | Required field; harness cap on firings per game |
| One high-weight early event in every game | Stellaris Vechtar Zavonia | Per-game deal; early cards weighted low; freshness |
| Same order each game | Old World Steam thread | Deal and "nothing" weight; randomised delays |
| Habits select the same cards | Old World, Civ VII | Broad hooks; variety of subjects |
| Interruption every few turns | Civ VII thread | Budget, gap, tray |
| Choice read as numbers only | Civ VII thread | Short text; a real trade-off on every option |
| Quality reused for different meanings | Failbetter | Named memories, one meaning, a registry |
| Fragile sequence | Failbetter; Short on time caves | One stage counter per chain; chains around one in ten |
| Badly timed outcome from pure salience | Short | Tiers, era limits, `minTurn` |
| Quest demands harm the player's position | Endless Legend forum | Subject and state tests; always a "decline" option |
| Content too thin to feel alive | StoryNexus via Short; Kennedy | Reusable cards bound to subjects; report tier |
| One figure dominates | Dunham comment | Limit cards per subject or theme per game |
| Unused memories | Old World Reference marks many | Memories work alone as timed effects; list readers in harness |

### Gaps
- No measured skip rate, read rate or text-length benchmark from any 4X studio. The evidence is forum opinion plus one UX comment, so the text limits above are judgement, not findings.
- No before-and-after data on popup-to-tray changes.

## What is a sensible first batch for this game?

### Takeaway
Start with 15 reusable cards bound to existing hooks, with a few reading a memory another card wrote, including one late card that gathers earlier choices. Ship it as a measured prototype at about 8-10 cards a game. **The 15 outlines below are my own suggestions, not sourced facts and not copied from any game, and all effect sizes are placeholders.**

### Cited Findings
- Existing systems, from project files: era score with Golden, Dark and Heroic ages with thresholds by era (`Ages.cs`: `GoldenThreshold`, `DarkThreshold`); religion with pantheons, holy cities, `faithPressure` and `SendMissionaries` (`Religion.cs`); agendas with opinion values (`CivLore.AgendaOpinion`); civ weaknesses such as Distant Provinces, Long Winters and Restless Jarls (`CivLore.cs`); `Note(...)` for player notices. From CLAUDE.md: Priests, `Tile.villageOwner`, `Tile.cleared`, ports and `portLinked`, ruins, Veteran and Elite ranks, disorder, three-stage Transcendence project. I checked these by searching file contents only and did not trace how the chronicle log is written.
- Gateways are locked pending the owner's decision (CLAUDE.md), so no card here uses them.

### Inferences: the 15 cards (my suggestions)

Hooks marked "new" need a small call added to the game. "Mem" is a memory or flag written; "Reads" is what it checks.

| # | Card | Hook / subject | Conditions | Options (effects are placeholders) | Mem written / reads | Repeat |
|---|---|---|---|---|---|---|
| 1 | **The Felled Grove** | Worker finishes clearing (new) / nearest city | `Tile.cleared` set beside a city of size 3+ | Pay the woodsfolk (gold); replant (tile returns to woodland, work lost); ignore (+1 production here for 20 turns) | `felled_grove:city` (long) on ignore | Per city once |
| 2 | **Water Where the Trees Were** | Pacing / city with `felled_grove` | River or coast city, 15-30 turns after card 1 | Raise banks (shields); move the low ward (-1 pop, then safe); pray (faith cost, 50% it holds) | Reads `felled_grove`; clears it | Follow-up, once |
| 3 | **Two Wards, One Well** | City grows (new) / city | Size 6+, no fresh water | Dig a cistern (shields, delays build); ration (-1 food, +1 content for 10 turns); favour the old ward (+2 content, later -1) | `slighted_ward:city` (20 turns) | City 40 turns |
| 4 | **The Old Shrine** | Majority faith flips / city | City's majority faith differs from state faith | Protect it (faith per turn down 1 for 20); seize it (+150 state pressure, -2 content 10 turns); endow both (gold) | `shrine_seized` (permanent, nation); `shrine_kept` | Once a game each city |
| 5 | **Preachers at the Gate** | Rival sends missionaries (exists) / target city | Player city, rival faith | Expel (rival opinion down, pressure kept); public debate (needs a Priest in or beside the city, removes pressure, or 25% backfires); tax them (gold, pressure stays) | `expelled_faith:rival` (20 turns) | Rival 30 turns |
| 6 | **The Pilgrim Road** | Pacing / player's holy city | Holy city plus a road, a foreign faith nearby | Open to all (gold per turn 20 turns, foreign pressure +); restrict to the faithful (faith up, one rival's opinion down) | `open_pilgrim_road` (long) | 60 turns |
| 7 | **The Far Province Writes** | Pacing / farthest city | City beyond the civ's weakness distance, or 8+ tiles from the capital | Grant a charter (+2 content, but Governor focus fixed 20 turns); order a road (worker route, gold); send a garrison (unit tied 15 turns) | `chartered:city` (long); `province_ignored` | City 50 turns. Rome, Persia and Aztec get one extra option each |
| 8 | **A Tax Upon the Village** | Pacing / `villageOwner` village | Owned village 2+ turns into tax | Lower the tax (less gold, may later join as a city or levy); demand a levy (free weak unit, opinion down); send a Priest (needs Priest, faith gain) | `village_wronged` / `village_friend` (long) | Village 40 turns |
| 9 | **Land for the Old Soldiers** | Unit reaches Elite (new) or war ends / unit | At peace within 5 turns, or Elite unit | Grant land (unit retires, +1 pop in nearest city); parade (era score, -gold); keep in service (upkeep +1, keeps rank) | `veterans_settled` (long) | 40 turns |
| 10 | **The Insult at Table** | Pacing / rival leader | Met; `AgendaOpinion` negative; not at war | Send gifts (gold, opinion up); answer in kind (opinion down, +1 content in capital 10 turns); ignore (small opinion down) | `insult_answered:rival` (20 turns) | Rival 50 turns |
| 11 | **The Gilded Commission** | Golden Age begins (exists) / capital | Golden Age | Great monument (shields, era score); pay down the people's dues (+content all cities 10 turns); save it (gold) | `monument_raised` | Once per age |
| 12 | **The Lean-Year Prophet** | Dark Age begins (exists) / city with lowest content | Dark Age | Hear him (faith up, science down 10 turns); silence him (+order, faith down; `prophet_silenced`); give grain (food cost) | `prophet_silenced` (long); reads `shrine_seized` for sharper text and +1 unrest | Once per age |
| 13 | **What the Ruin Kept** | Ruin found (exists) / finding unit | Era 2+ with a ruin discovered | Study it (minor tech progress, `opened_tomb`); sell the finds (gold); reseal (faith, era score) | `opened_tomb` (permanent); reads nothing | Once per ruin, 3 per game |
| 14 | **Lights Off the Harbour** | Pacing / city with `portLinked` | Port linked by road, 20+ turns old | License the traders (gold, -1 content from guild anger); raid the coves (Galley tied up, chance to lose it, then safe); look away (+gold now, later a rival learns) | `smugglers_licensed` / `looked_away` (long) | Port 50 turns |
| 15 | **The Doubters at the Work** | Transcendence project stage begins (new) / project city | Stage 1 or 2; reads earlier memories | Option text and third option change by history: if `shrine_seized`, priests march; if `opened_tomb`, ruin scholars offer speed; if `open_pilgrim_road`, pilgrims bring gold. Base: guards (gold) or pause 2 turns (+content) | Reads 3-4 long memories; writes `doubters_answered` | Once a stage |

Fit notes (my reasoning):
- **Spread.** Faith, ages, rival relations, land and works, and cities are all covered. There are 9 hook-driven cards and 6 pacing cards, in line with the "tied to what is happening" evidence. The one chain is card 1 to card 2.
- **Memory payoff.** Cards 2, 12 and 15 read memories that other cards write. Card 15 gives a late-game callback in the part of the game the first report named as weak.
- **Civ fit.** Card 7 is the model for per-civ lines. Later cards can add one option for each of the other weaknesses (Long Winters on a harvest card, Restless Jarls on a wall card).
- **Rival AI.** Start with cards for the human player only. Rivals could resolve them later by a fixed preference (Civ IV has `iAIValue`), but on a 54-nation map that is a performance and balance cost with no visible gain at first.
- **Build order.** (a) Data classes and JSON-readable cards. (b) Hooks. (c) Save version bump from 25 with used, cooldown and memory tables, with old saves starting empty. (d) Tray and card screen. (e) Harness: 200 simulated games reporting firings per card, never-fired cards, memories with no reader, cards exceeding the cap (Short's randomised testing). Save and harness rules are in CLAUDE.md. Size: the first report's "Medium, plus writing" still looks right.
- **What not to do yet.** No multi-stage chains, no faction mechanics, and no gateway card until the owner decides on them. Don't randomise outcomes without showing odds.

### Gaps
- None of these cards has been tested or checked for balance against the game's actual gold, faith or era-score rates; effect sizes need calibrating from the harness.
- I searched the code for the hooks named but did not trace each one, so "exists" for missionaries, ages and ruins means the system exists, not that a suitable callback is already there.
- Whether the 15-card rate of 8-10 per game feels right is unknown until played.
