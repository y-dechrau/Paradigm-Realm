# What 4X designers themselves said: primary-source notes, second round (researched 6 Nov 2026)

Labels:
- [READ] = I fetched the page, but through a summarising tool, so I saw a model's digest and not the raw text. Wording and fine detail must be checked at source before reuse.
- [READ-2ND] = a page I fetched that reports the designer at second hand (news article, reviewer, attendee).
- [SNIPPET] = search-result text only.
- [MEMORY] = my own recollection, not on the page. Verify.

Text is paraphrased throughout, apart from one short quotation, to respect copyright limits. Open the source for exact wording.

Implications for Paradigm Realm are my inference. They are based on CLAUDE.md and the first-round report; I did not read game code for this round. This file avoids repeating first-round material (Orders, event deck design, Ambitions, double-score win, undo, Civ IV difficulty table, Beach's completion figure).

## Jon Shafer: "At the Helm" (book and blog), Civ V postmortem remarks, At the Gates postmortem

### Takeaway
I found no book or blog by Shafer called "At the Helm"; that title should be treated as unconfirmed. His real blog is "Jon Shafer on Design". His 2013 Civ V retrospective is the main primary source and is rich: 1UPT needed about four times the map, opaque AI "humanity" read as noise, global happiness acted as a wall, and removing sliders removed the ability to change course. His At the Gates postmortem exists but I could not open it, and the coverage reports it as a personal account rather than a design one.

### Cited Findings

**Source hunt**
- Two searches for the title returned nothing linking "At the Helm" to Shafer [SNIPPET] — [search hits include his Wikipedia page](https://en.wikipedia.org/wiki/Jon_Shafer). The front page of his blog shows no such title [READ] — [Jon Shafer on Design](https://www.jonshaferondesign.com/).
- `jonshafer.wordpress.com` redirects to a WordPress not-found page, so that blog address looks dead [READ, fetch result]. The live blog's front page lists At the Gates updates from 2015 to Oct 2018, and none mentions Civ V or 1UPT [READ] — [same](https://www.jonshaferondesign.com/). Individual post URLs returned a "page missing" error to my tool, and archive.org is unavailable to it, so these posts are unread, including [Playtest Thoughts, Apr 2018](https://jonshaferondesign.com/2018/04/18/atg-update-april-2018-playtest-thoughts/) and [Diplomacy, Mar 2018](https://jonshaferondesign.com/2018/03/02/atg-update-march-2018-diplomacy/). A person with a browser may do better.

**"Revisiting the Design of Civ 5", Game Developer, 18 Feb 2013** [READ] — [link](https://gamedeveloper.com/design/revisiting-the-design-of-civ-5)
- **1UPT and maps:** he rates Civ V's combat as the series' best in itself, but says AI manoeuvre in tight spaces was very hard and bottlenecks strangled movement. He estimates 1UPT needs a map at least four times larger with fewer choke points; engine performance ruled that out.
- **Knock-on effect:** units filled the map early, so production was slowed, which meant more waiting. Once the map is full, even war stops being fun. Sub-tiles were rejected as they blur the grid.
- **Diplomacy:** he names this the lesson that most shaped his later thinking. Making leaders act human produced random noise, and late betrayal made earlier relationships feel pointless. What counts is what the player perceives. At the Gates uses a visible relations number: at -5 a leader never trades, at +10 always helps in war; leader requests tied to crises are the main way to earn it.
- **AI:** his AI code was elegant but underperformed; designers can fall for a system's architecture and lose sight of its effect. Heavy randomness cost strategies their cohesion. Next time: simpler and results-focused.
- **Resources:** a resource visible on the map that does not matter is a flaw to fix. Near self-sufficiency made trade worthless. Removing health devalued wheat-type resources. He moved from a fixed unit cap per resource to a stockpile, and proposes scarcer resources that unlock stronger, more unique units.
- **Happiness:** global happiness made large empires nearly impossible; penalties should be overcome, not stop you. Later preference: free expansion with food upkeep per settlement.
- **Sliders:** removing research, commerce and culture sliders cut busywork but also cut the ability to change direction; long-term planning needs tools to change course.
- **Government:** Policies build lasting identity, Civics allow reinvention; he now prefers reinvention. At the Gates reallocation costs temporary stability, so change has a price.
- **Interface:** element size should show importance; rare actions go in sub-screens. Regret: no expert switch adding detail to screens and mouseovers for hardcore players.

**Interview at At the Gates launch, PCGamesN, updated 31 Jan 2019** [READ] — [link](https://www.pcgamesn.com/at-the-gates/at-the-gates-interview)
- Aims for a few tough decisions a turn, about two or three, rather than managing many workers and cities. He argues most Civ cities do not matter and that founding more can remove fun.
- Complexity builds up gradually so busywork does not escalate, and the start feels restricted while later stages feel powerful. Admits some may find the late game too strong.
- Wanted asymmetric starts, possible because the game has no multiplayer, against the genre's habit of symmetry. Clans are handled as characters, citing Crusader Kings and XCOM.

**At the Gates postmortem, 9 Jan 2019**
- Original: [How At the Gates took 7 years of my life](https://jonshaferondesign.com/2019/01/09/how-at-the-gates-took-7-years-of-my-life-and-nearly-the-rest/). **My tool got a "page missing" result, so this is not read.**
- Coverage reports extreme overwork, isolation, health decline and spending about $70k of savings; recovery came through hobbies, diet and exercise [READ-2ND] — [GameWatcher](https://www.gamewatcher.com/news/at-the-gates-ruined-jon-shafer); [Wccftech, 10 Jan 2019](https://wccftech.com/jon-shafer-reveals-troubles-at-the-gates/amp/) (this one says the article itself has no design lessons).
- He accepts responsibility for delays, including a new feature added late that forced a major redesign; $106,283 was raised from 3,000+ backers in 2013 [SNIPPET, secondary] — [search-result coverage, Wccftech](https://wccftech.com/jon-shafer-reveals-troubles-at-the-gates/amp/). Attribution of each point within coverage was not verified.
- An early build ran about 18 months with no clans or professions; clans were added so the game felt like it progressed [SNIPPET, secondary, source page not tied down; a fetched [Play the Past piece](http://www.playthepast.org/?p=6203) did not contain it].
- Some reviewers found the result needlessly obtuse; others liked it [SNIPPET, secondary] — [OpenCritic list](https://opencritic.com/game/7151/-/reviews?page=2); [Digitally Downloaded](https://www.digitallydownloaded.net/2019/02/review-jon-shafers-at-gates-pc.html).

**Others on Shafer's remarks**
- Firaxis's Ed Beach is reported as calling Shafer's self-criticism a little harsh; only the headline was readable, and it does not itself name Beach in what I saw [SNIPPET] — [PC Gamer](https://www.pcgamer.com/au/civilization-v-designer-ed-beach-jon-shafer).
- Firaxis producer Dennis Shirk: 1UPT exposed that building everything in the city centre was dull, and this led to Civ VI's districts [READ-2ND] — [Game Developer, 21 Jun 2016](https://www.gamedeveloper.com/design/city-management-mayhem-and-sid-meier-s-wisdom-making-i-civilization-vi-i-).

### Inferences
- **Map fill:** check whether Paradigm enforces one unit per tile. CLAUDE.md's "best defender" wording suggests stacking is allowed, which would sidestep Shafer's main problem. If any limit exists, his 4x estimate matters, especially on small maps with 66 possible nations.
- **Diplomacy:** agendas and spoken replies should sit over visible numbers with fixed thresholds. "Why did he do that?" should always be answerable on screen. This is cheap and addresses complaints about random AI swings from two designers (Johnson in round one, Shafer here).
- **Resources:** audit all 16 land and sea resources against his rule that anything visible on the map must matter. Ten sea resources is a plausible risk. Tech-hidden resources help, but only once revealed ones have a use.
- **Happiness:** tune disorder as something to manage. Rivals holding only 4-6 cities (first round) could mean the penalties act as a wall for AI at least; test whether this comes from happiness or from settling behaviour.
- **Change of course:** the science-rate split survives, which Shafer would approve of. Edicts chosen one-of-three at each era could use a respec with a temporary price.
- **Expert detail switch:** a held key or setting that deepens tooltips suits Paradigm, whose systems are already numerous, and pairs with first-round item #11.
- **Warning against overcorrection:** Shafer's follow-up took seven years and drew complaints of being obtuse, so "fewer, harder decisions" is a direction and not proof.
- **Solo-developer lesson:** late feature additions forcing redesign is the project risk closest to this one. Opening the sealed gateways is that kind of decision, so decide before building more on top.

### Gaps
- "At the Helm": unconfirmed. Ask whoever supplied the title for a link or ISBN.
- The Shafer postmortem and 2018 posts remain unread. PC Gamer's "criticizes every decision" piece was truncated to its headline ([link](https://www.pcgamer.com/jon-shafer-criticizes-every-decision-he-made-in-designing-civ-v-explains-how-at-the-gates-will-differ)).
- Unheard: [Designer Notes #4, Fåhraeus with Shafer sitting in, 10 Feb 2015](https://www.designer-notes.com/podcast-henrik-fahraeus-with-jon-shafer/) (audio; no transcript looked for).

## Sid Meier: memoir and "interesting decisions"; Brian Reynolds on Civ II and Alpha Centauri

### Takeaway
I did not read the memoir itself, only reviews and one long notes article. Its usable points concern perception: players blame the game, never themselves, and misjudge odds, so explain bad outcomes and avoid unexplained randomness. Reynolds's clearest statements are that he regrets multiplayer effort in Alpha Centauri, that fiction rescued a dry design, and that future sequels may need less rather than more.

### Cited Findings

**Meier, memoir (W. W. Norton, 2020), seen through others**
- Has a chapter titled "Interesting Decisions"; the origin of the phrase puzzles even Meier [SNIPPET] — [Haverhill catalogue](https://link.haverhillpl.org/resource/tUr7vxXJSXM); [Strategy Bridge review](https://thestrategybridge.org/the-bridge/2021/1/11/reviewing-sid-meiers-lessons-in-game-design-civilization-and-wargames).
- "A game is a series of interesting decisions." A decision qualifies when the player feels personally invested and wonders how a different choice would have gone next time [READ-2ND] — [Brikman review, 18 Jun 2026](https://www.ybrikman.com/blog/2026/06/18/sid-meier-memoir/).
- Perception outranks arithmetic: if players perceive a rule as unfair, it harms fun even if mathematically correct [READ-2ND] — [same](https://www.ybrikman.com/blog/2026/06/18/sid-meier-memoir/).
- He prefers prototypes to design documents, and tunes by doubling or halving instead of nudging by 5% [READ-2ND] — [same](https://www.ybrikman.com/blog/2026/06/18/sid-meier-memoir/).
- Fun must be the player's: his Railroad Tycoon flood mechanic amused the designer and not the player, and he fixed it by offering wooden versus flood-resistant stone bridges so that risk became a choice. Other memoir points: new players need early wins; removing too much micromanagement hurt a dinosaur prototype; rule of thirds for sequels [READ-2ND; the author separates Meier's points from his own] — [Keller, Game Developer, 14 Oct 2021](https://www.gamedeveloper.com/game-platforms/just-one-more-turn---game-development-tips-and-tricks-from-the-creator-of-civilization-sid-meier-).
- Gandhi's nuclear-aggression overflow bug is a myth, per the memoir [READ-2ND] — [Brikman](https://www.ybrikman.com/blog/2026/06/18/sid-meier-memoir/); [Keller](https://www.gamedeveloper.com/game-platforms/just-one-more-turn---game-development-tips-and-tricks-from-the-creator-of-civilization-sid-meier-).
- Admitted failures: Dinosaurs was a promising theme that never became fun [READ-2ND] — [Buteau's digest of lessons](https://www.antoinebuteau.com/lessons-from-sid-meier/); people proved uncooperative in CivWorld testing, by his own unscientific observation [READ-2ND] — [Keller](https://www.gamedeveloper.com/game-platforms/just-one-more-turn---game-development-tips-and-tricks-from-the-creator-of-civilization-sid-meier-).

**Meier, GDC keynote "The Psychology of Game Design (Everything You Know Is Wrong)", 12 Mar 2010**
- Player psychology is not rational; players see themselves as commanding figures; setbacks hit harder than rewards, so explain why a loss happened and how to avoid it; losers blame the game or claim the AI cheats; unexplained randomness produces paranoia and can make players leave [READ-2ND] — [VentureBeat, Perry, 12 Mar 2010](https://venturebeat.com/2010/03/12/quotes-from-sid-meiers-keynote-gdc-speech).
- Players expect to win at far higher rates than the odds imply, and good design sometimes favours psychology over simulation [READ-2ND] — [Dyson, Museum of Play, 31 Mar 2010](https://www.museumofplay.org/blog/gdc-2010-game-psychology-101/).
- **Conflict on the odds figure:** the VentureBeat digest I received muddles which ratio was which. Brikman reports the cleaner version: players treat 3:1 as near-certain, so Meier's games make it effectively certain, and they quietly help after losing streaks. Brikman's explanation is his own reading [READ-2ND] — [Brikman](https://www.ybrikman.com/blog/2026/06/18/sid-meier-memoir/). Watch the talk before reusing any number. The abstract argues AIs should not be too smart [SNIPPET] — [GDC Vault](https://gdcvault.com/browse/gdc-10/play/1012186). An attendee's notes returned 403 — [Medium](https://medium.com/ada-chen-rekhi/gdc10-notes-sid-meier-on-why-everything-you-know-is-wrong-53d7bc889419).

**Reynolds**
- Civ II aimed to improve each system and worsen none; multiplayer was cut to protect the ship date [READ] — [Arcade Attack, 18 Oct 2019](https://www.arcadeattack.co.uk/brian-reynolds/).
- Alpha Centauri felt dry when history-based design met a future setting; without fiction, characters and factions it would likely have failed. Learned from Meier: start by getting one turn playable, then build out, finding the fun [READ] — [same](https://www.arcadeattack.co.uk/brian-reynolds/).
- Regrets Alpha Centauri's multiplayer work; he would have spent it on solo play. Designers may need to add less or replace old things. Modding and scenarios made much of the franchise's later history possible. Civ II's AI was rewritten so that wonders and caravans no longer relied on the old cheats, and AI remembered reputations so a bully did not start fresh with each new target [READ, Goodfellow interview] — [CivFanatics, 29 Sep 2006](https://civfanatics.com/misc/interview-with-brian-reynolds/).
- Long-form audio, unheard: [Designer Notes 38](https://www.designer-notes.com/?p=1478), [39](https://www.designer-notes.com/?p=1484), [40](https://www.designer-notes.com/designer-notes-40-brian-reynolds-part-3/) (2018); [Game Design Round Table 51](https://tgdrt.libsyn.com/51-alpha-centauri-evolving-as-a-designer-with-brian-reynolds).

### Inferences
- **Explain every loss.** The combat preview is already exact. Extend the idea: a city in disorder, a lost battle or a broken deal should state why. The Chronicle could carry this. Cheap and strongly aligned with the talk.
- **Deterministic combat suits Meier's warning.** The damage formula in CLAUDE.md appears to dodge odds perception altogether; check whether any random element remains before adding "help after losing streaks".
- **Tuning method:** apply halve-or-double to the era pace table and the worst outliers in the diary from first-round item #1. Small nudges from harness data will not reveal feel.
- **Reputation memory:** check that rival attitude remembers treatment of third parties, as Civ II already did.
- **Fiction is structural.** Reynolds's remark supports the 22 lore civilisations and the sealed peoples, but also suggests the sealed peoples only pay off if players meet them.
- **Multiplayer:** Reynolds's regret supports keeping it unbuilt, which is already the state of the project.
- **Modding:** Reynolds and Sas (below) both credit it. This was a first-round gap. If civilisations, events and buildings live in data files, Workshop-style sharing becomes cheap later. Keep event cards data-driven from day one.

### Gaps
- Memoir text not read; only reviews. Library copy advised for the "Interesting Decisions" chapter.
- GameSpot's Reynolds interview and diary returned 403 ([interview](https://www.gamespot.com/articles/an-interview-with-brian-reynolds/1100-2564268)). The diary's AI-tuning entry is noted only from a snippet: find out why the AI made a dumb move, then fix that [SNIPPET].

## Soren Johnson beyond Old World's systems; Beach, Civ VI and VII leads; Amplitude; Triumph; Paradox

### Takeaway
Johnson's GDC postmortem slides add three things first-round did not use: AI does not get events or Ambitions, so losses never come as surprises; AI difficulty can come from extra starting cities rather than bonuses; and costless micro is a failure pattern. Fåhraeus's Stellaris postmortem has the most directly useful warning: writing early and late content left the mid-game hollow. Fischer's Millennia diary explains why developers cannot see their own problems. Firaxis is the thinnest, as Civ VI interviews are mostly producer remarks and pages were truncated.

### Cited Findings

**Johnson: "My Elephant in the Room" Part 2, 1 Apr 2023, from GDC 2022** [READ] — [link](https://www.designer-notes.com/my-elephant-in-the-room-part-2/)
- Fixed tech trees create golden paths: in Civ 4 you click Gunpowder and see 11 prerequisites. His fix treats techs as a deck: four drawn at a time, unpicked cards return through discard, so skipping may mean a long wait. Some cards are one-off rewards (free Settler, Great Scientist, Stone) competing with long-term techs. It costs extra complexity.
- Moving citizens is free each turn and may gain 1%, which pulls hardcore players into tedious optimisation. Lesson: decisions need cost or permanence. Old World citizens become permanent Specialists on improvements (+50-100% output), with food and opportunity costs.
- Splitting generic Production into separate yields let events offer odd trades. Buy price is double sell price, an idea from Age of Empires, so buying is available but producing is better.

**Johnson: Part 3, 2 Apr 2023** [READ] — [link](https://www.designer-notes.com/my-elephant-in-the-room-part-3/)
- Civ 3's bargaining table, his admitted mistake: AI traded each tech with everyone since it cost nothing, so humans saw a cartel selling cheaply; humans re-contacted every civ each turn adding tiny amounts up to the AI's limit. He classes it as a "cursed problem", after Alex Jaffe's GDC 2019 talk. Coverage: [PC Gamer](https://pcgamer.com/soren-johnson-says-civ-3s-bargaining-table-was-a-big-mistake) [SNIPPET].
- Old World replaces it with event-based diplomacy: a trade, truce or alliance request draws a random applicable event, so nothing can be min-maxed.
- AI gets no events and cannot win through Ambitions, so surprise losses do not occur. He argues AI ganging up on humans the way humans gang up would feel unfair.
- Difficulty: AI starts with more cities at higher levels and otherwise plays by the same rules.
- Adding events adds variety and not complexity. Competitive players kept events on, so a maintained no-events variant was probably wasted effort. Victory tied to city sites avoids ICS. Ten rulers as "lives" (early name "Ten Crowns") was hard to balance since ten rulers could last 50 or 500 turns.

**Johnson: "Water Finds a Crack", Game Developer Mar 2011, posted 12 Jun 2011** [READ] — [link](https://www.designer-notes.com/game-developer-column-17-water-finds-a-crack/)
- Players optimise any hole, and once found an exploit is hard to unsee. Key tests: does it crowd out other playstyles, and does it waste time?
- Civ examples: Civ 3 forest replant-and-chop; ICS (a size-2 city works 1.5 tiles per citizen, a size-20 only 1.05); discarded overflow, which made players micromanage rates and citizens every turn. Civ 4 carried overflow forward and added per-city maintenance scaling with total cities. Civ 3's combat seed preservation upset reloaders, so it became an option; the default discourages the tedious route.

**Johnson: other unread leads** [READ, index only] — [Writings](http://www.designer-notes.com/writings/); [search list](https://www.designer-notes.com/?s=Shafer)
- [Seven Deadly Sins for Strategy Games](http://www.designer-notes.com/?p=106), [Sid's Rules](http://www.designer-notes.com/?p=119), [Playing the Odds](http://www.designer-notes.com/?p=171), [Challenging Design](http://www.designer-notes.com/?p=179), [Theme is Not Meaning I](http://www.designer-notes.com/?p=237), [Less Than Zero](http://www.designer-notes.com/?p=438), [When Choice is Bad](http://www.designer-notes.com/?p=609), [Old World Notes #3: One Unit per Tile, 10 Aug 2021](https://www.designer-notes.com/old-world-3-one-unit-per-tile/), [25 Years of Civilization, 14 Apr 2016](https://www.designer-notes.com/25-years-of-civilization/), and the [Civ4 Afterword](https://drive.google.com/open?id=1vlmAHiuH-DQAaKkeOM7N5tpnYve63ACV). I ran out of allowed calls; these look like the best next reads, the 1UPT note above all.

**Firaxis: Civ VI (producer Shirk speaking of designer Beach, 21 Jun 2016)** [READ-2ND] — [Game Developer](https://www.gamedeveloper.com/design/city-management-mayhem-and-sid-meier-s-wisdom-making-i-civilization-vi-i-)
- Pacing is named as the biggest challenge; Beach added an AI "Mayhem Level" system so the game does not stay calm too long.
- Civ V's last third was not very interesting and felt empty. Guideline: keep it simple until the player is ready.
- Beach is not directly quoted here. My digest's line about religion and Shafer looks garbled (Shafer left religion out of Civ V's base game, by my [MEMORY]), so do not reuse it.

**Firaxis: Civ VII** 
- Unsigned message: UI is the top priority, with map readability and formatting; later asks include co-op teams, full player counts across Ages, single- or double-Age games, more maps. It admits no specific design choice was wrong [READ, undated, shortly after early access] — [2K](https://civilization.2k.com/en-GB/civ-vii/news/message-from-firaxis-games/).
- Beach feared Ages might not work but later said they address non-completion; headlines only, body truncated [SNIPPET] — [GamesRadar](https://www.gamesradar.com/games/strategy/civ-7-director-thought-the-new-ages-system-might-not-work-but-says-it-does-fix-the-number-one-issue-players-not-getting-to-the-end-of-their-games); [GamesRadar on the update](https://www.gamesradar.com/games/strategy/one-year-after-civilization-7-tried-to-fix-the-strategy-genres-biggest-problem-firaxis-is-meeting-fans-in-the-middle-with-an-expansion-level-update-i-think-were-going-to-blow-some-fans-minds). Compare the first-round Zelnick remark and patch 1.4.0.

**Firaxis: Beyond Earth (McDonough and Miller, GDC 2015)** [READ-2ND] — [Game Developer](https://gamedeveloper.com/audio/video-a-design-retrospective-of-i-civilization-beyond-earth-i-)
- Both leads judged the design too conservative for the chance a new setting gave them. Compare Civ VII: too bold by Take-Two's account. Together they bracket the risk.

**Amplitude**
- De Waubert: for every faction but humans the team tries to break at least one major rule; he accepts likely imbalance because informed players can enjoy it. Starting with little 4X knowledge, the team learned from community feedback. Designed-in imbalance may intimidate newcomers [READ-2ND, based on a PC Gamer interview] — [Game Developer, 4 Jan 2018](https://gamedeveloper.com/design/how-imbalance-in-the-i-endless-i-series-led-to-a-new-take-on-the-4x-genre).
- Balance was what really scared him early; Humankind was the first idea but was judged too ambitious, so they learned on other 4X games first [SNIPPET] — [Gaming Trend](https://gamingtrend.com/news/it-takes-more-than-a-village-in-depth-interview-with-studio-head-creative-director-of-humankind-romain-de-waubert). Humankind post-launch self-criticism: none found.

**Triumph (Lennart Sas)**
- A sequel's balance is deciding how far to stick to roots: keep a few pillars and treat all else as open. Moved from dense fixed factions (years per species in Planetfall) to modular content that is easier to test and mod. Some exploits are worth keeping because people like finding them [READ] — [DBLTAP, 4 May 2023](https://www.dbltap.com/features/age-of-wonders-4-interview-launch).
- A tech tree across 54 tomes was daunting to design; friction is needed to reach new ground; the main competitor is often the previous game [READ] — [GOG, Apr 2023](https://www.gog.com/news/gog_interview_age_of_wonders_4_is_almost_here_read_our_interview_with_the_games_director). Neither page discusses late game or admits design failure.

**Paradox: Stellaris postmortem (Fåhraeus, Åslund, 14 Jun 2016)** [READ] — [Game Developer](https://gamedeveloper.com/design/postmortem-paradox-development-studio-s-i-stellaris-i-)
- Went right: early-game focus on mystery; in-game advisor VIR replacing a separate tutorial (Fåhraeus expects no more out-of-game tutorials); first dedicated audio director; all state in one object, plus headless tests, which helped multiplayer sync.
- Went wrong: too little scripted mid-game content because of focus on early and late. Minimalist, colourless first UI, with remnants remaining. Resource balance was harder than it looked: adjacency bonuses were dropped because of inflation, and stockpile caps came late. Drift between 4X and grand strategy; set identity early. Tech research iterated far too long until a late idea of a reshuffled deck worked. Skipped prototyping of fleet combat, which Åslund calls plain stupid. Federations, Alliances and Uplift were thin at launch.
- Same admission in 2016 coverage: neglect of colony events [SNIPPET] — [GameWatcher](https://www.gamewatcher.com/2016-17-05-stellaris-director-says-he-is-not-surprised-about-criticism-related-to-mid-game-promises-big-changes); [PC Gamer](https://pcgamer.com/stellaris-director-promises-big-changes-and-more-content-in-future-updates).

**Paradox/C Prompt: Millennia (Ian Fischer, 5 Mar 2024)** [READ] — [After the Demo](https://www.paradoxinteractive.com/games/millennia/news/after-the-demo)
- Long development blinds you: a placement bug stayed hidden because everyone on the team placed Improvements the same way. Public demo feedback surfaces what internal play cannot.
- Changes from demo feedback: tooltip flicker, map edge padding, clearer attack range, weak raiders buffed, autosave reduced to five recent files with an option. Community saves helped improve AI war behaviour. Method: skeleton first, then a loop of playtest, feedback, change.
- A Slitherine recap has him favouring interesting choices over complexity [SNIPPET] — [Slitherine](https://www6.slitherine.com/news/the-hot-seat-episode-6-recap). No postmortem after Paradox ended development was found.

### Inferences
- **Event cards (first-round #6):** adopt Johnson's asymmetry, with cards for the human only and rivals never winning through them. Write mid-game cards first, given Stellaris's admitted gap. This is the most directly useful finding for that item. Keep them on in all modes; no "off" variant needs maintaining unless the owner wants one.
- **Difficulty (first-round #2):** Old World's "more starting cities, same rules" is a new option that is honest and simple. Worth testing beside any percentage bonuses. It needs care on maps where settling space is tight.
- **Rival deals:** check whether Paradigm's diplomacy lets AI trade techs or research pacts freely, and whether a human can repeat small offers without cost. Johnson calls this cursed. Limits or turn cooldowns are cheap.
- **Citizen micro:** click-to-lock tiles and one-click specialists are free every turn, which is Johnson's failure pattern. Automatic assignment good enough to make micro rarely worthwhile may be the fix, or a cooldown on changes. The earlier stall with an all-specialist city shows this system already bites. Also check that overflow food, shields and science carry over, as discarding it forces turn-by-turn micro.
- **180-tech tree:** first-round found rivals using about half. Johnson and Stellaris both ended up with decks for research. A light form might draw a few minor techs per era while keeping the 36 keystones fixed. This is a real system change; try after the human diary, not before.
- **Late-game pace:** Beach's Mayhem idea suggests measuring turns since the last conflict. If long calm stretches appear in the diary, a rival-aggression dial is cheaper than a crisis system.
- **Asymmetry:** sealed peoples with `PeopleRule` fit De Waubert's rule-breaking. His remark suggests strict balance is not needed if rules are readable, but this cuts against the first game for newcomers, so default to gentler civilisations.
- **Tutorial:** VIR's in-game approach supports Divus Maximus, and the case for extending him (#15) rather than building something separate.
- **Blind spots:** Fischer's bug is an argument for watching someone else play, not only the owner's hand-played game (#1). Paradigm's owner and agents share a single view of the interface.
- **Prototype before committing:** opening the gates should be tried as a scrappy switch, not fully built, on Åslund's and Meier's advice.
- **Identity:** Åslund's advice applies to the sealed-world idea too. Is this a home-world 4X with optional far worlds, or a many-worlds game? This matches the open question in the first report.

### Gaps
- Stellaris Dev Diary 361 "The Vision" failed: HTTP 500 and a browser check on both forum domains, not bypassed ([link](https://admin-forum.paradoxplaza.com/forum/developer-diary/stellaris-dev-diary-361-the-vision.1712933/post-30171103)). Moregård-era admissions and the pops rework are therefore unresearched.
- Civ VI and VII lead designers (Beach, Carl Harrison and others): nothing in their own words read in depth this round; PC Gamer and GamesRadar pages came back truncated to headlines. Endless Space 2's Senate: still no source. Designer Notes audio not heard; no transcripts sought.

## GDC Vault talks on 4X and turn-based strategy

### Takeaway
At least ten Firaxis- and Johnson-related sessions exist, and Johnson's 2022 slides are freely readable on his blog. A 2004 Civ franchise deck is a free PDF. I found no GDC talks from Amplitude, Triumph or Paradox on these searches, though that rests on one domain-limited search and does not rule them out. Vault access for most sessions was not checked: one page showed a login limit and a note on members-only content.

### Cited Findings

| Talk | Speaker(s) | Year | Freely readable? | Status |
|---|---|---|---|---|
| [My Elephant in the Room: An Old World Postmortem](https://www.gamedeveloper.com/press-release/soren-johnson-delivers-my-elephant-in-the-room-an-old-world-postmortem-at-gdc-2022) | Soren Johnson | 2022 | Yes: [Part 1](https://www.designer-notes.com/my-elephant-in-the-room-part-1/), [2](https://www.designer-notes.com/my-elephant-in-the-room-part-2/), [3](https://www.designer-notes.com/my-elephant-in-the-room-part-3/) | [READ] |
| [Psychology of Game Design (Everything You Know Is Wrong)](https://gdcvault.com/browse/gdc-10/play/1012186) | Sid Meier | 2010 | Coverage above; video reportedly on [Game Developer](https://gamedeveloper.com/design/video-sid-meier-s-psychology-of-game-design), unverified | [READ-2ND] |
| [Classic Game Postmortem: Sid Meier's Civilization](https://gdcvault.com/play/1024294/Classic-Game-Postmortem-Sid-Meier) | Sid Meier, Bruce Shelley | 2017 | Abstract only; login notice shown | [READ, abstract] |
| [Absolutely No Pressure: Continuing a Successful Game Series with Civilization VI](https://gdcvault.com/play/1024421/Absolutely-No-Pressure-Continuing-a) | Ed Beach | 2017 [MEMORY] | Abstract: reviews key subsystems and which changes were adopted or dropped | [SNIPPET]; likely the best Civ VI source, unread |
| [More Than This World: Civ Beyond Earth retrospective](https://gdcvault.com/play/1022162/More-Than-This-World-A) | David McDonough, Will Miller | 2015 | Coverage above | [READ-2ND] |
| [Playing to Lose: AI and Civilization](https://gdcvault.com/play/364/Playing-to-Lose-AI-and) | Soren Johnson (then EA2D) | 2008 [MEMORY] | Not checked | [SNIPPET] |
| [Play Early, Play Often: Prototyping Civ IV](https://www.gdcvault.com/play/1013460/Play-Early-Play-Often-Prototyping) | Johnson, Dorian Newcomb | 2006 [MEMORY] | Not checked | [SNIPPET] |
| [The Civilization Series: How to Maintain a Successful Franchise](https://www.gdcvault.com/play/1022858/The-i-Civilization-i-Series) | Johnson among the franchise's designers, per slide text | 2004 | Slides PDF: [Don't Blow It!](https://media.gdcvault.com/gdc04/slides/civilization_series_how.pdf) | [SNIPPET]; cites cutting unfun elements, borrowing from rivals, and improving single-player random maps through world generator and AI |
| [Firaxis' Civilization V: A Case Study in Scalable Game Performance](https://gdcvault.com/play/1012192/Firaxis-Civilization-V-A-Case) | not seen | about 2010 (inferred from ID) | Not checked | [SNIPPET] |
| [Ahistorical Accuracy: New Myths and Representation in Civilization](https://gdcvault.com/play/1027798/Ahistorical-Accuracy-Bringing-New-Myths) | not seen | not seen | Not checked | [SNIPPET] |
| Cursed Problems in Game Design | Alex Jaffe | 2019 | Cited by Johnson; Vault page not reached | [READ, citation only] — [Part 3](https://www.designer-notes.com/my-elephant-in-the-room-part-3/) |

Also: Johnson's "Theme Is Not Meaning" series reportedly became a GDC lecture [SNIPPET] — [Designer Notes](https://www.designer-notes.com/page/2/). [Designer Notes Live at GDC with Frank Lantz, 8 Mar 2026](https://www.designer-notes.com/designer-notes-live-at-gdc-with-frank-lantz/) exists [READ, listing only]. CivFanatics has a [discussion of the 2022 talk](https://forums.civfanatics.com/threads/681293) [SNIPPET].

### Inferences
- Read next, in this order: "Absolutely No Pressure", the free 2004 PDF, "Playing to Lose", "Ahistorical Accuracy" (bearing on the 23 real faiths and first-round sensitivity notes), then the Civ V performance talk if map profiling becomes urgent.
- The earlier claim of "no GDC talk" is now closed in part: titles are known, content largely is not.

### Gaps
- Exact free or paid status of each session; speaker and year where marked. No Civ VII, Stellaris, Age of Wonders, Humankind or Ara GDC talk was found, and absence is unproven.

## What failed, by the designers' own admission

### Takeaway
The admitted failures cluster around: costless repetition, opaque AI behaviour, mid-game content gaps, mechanics with knock-on effects, and work spent on modes few needed. Almost none admit a failed core concept unprompted; Civ VII's reversal came from players and the publisher.

### Cited Findings
| Designer | Admitted failure | Source |
|---|---|---|
| Shafer | 1UPT on maps too small, AI that could not use it, production slowed to compensate | [Civ 5 retrospective](https://gamedeveloper.com/design/revisiting-the-design-of-civ-5) |
| Shafer | Human-like AI read as noise; elegant AI code underperformed | [same](https://gamedeveloper.com/design/revisiting-the-design-of-civ-5) |
| Shafer | Global happiness as a wall; self-sufficient resources; sliders lost | [same](https://gamedeveloper.com/design/revisiting-the-design-of-civ-5) |
| Shafer | Late feature forcing redesign; overwork over 7 years [secondary coverage only] | [GameWatcher](https://www.gamewatcher.com/news/at-the-gates-ruined-jon-shafer) |
| Johnson | Civ 3 bargaining table, themed victories, free citizen micro, discarded overflow, no-events variant, ten-ruler lives | [Part 2](https://www.designer-notes.com/my-elephant-in-the-room-part-2/); [Part 3](https://www.designer-notes.com/my-elephant-in-the-room-part-3/); [Water Finds a Crack](https://www.designer-notes.com/game-developer-column-17-water-finds-a-crack/) |
| Fåhraeus and Åslund | Hollow mid-game, colourless UI, resource inflation, no combat prototype, unclear identity, thin late features | [Stellaris postmortem](https://gamedeveloper.com/design/postmortem-paradox-development-studio-s-i-stellaris-i-) |
| McDonough and Miller | Beyond Earth too conservative | [Game Developer](https://gamedeveloper.com/audio/video-a-design-retrospective-of-i-civilization-beyond-earth-i-) |
| Shirk (Firaxis) | Civ V's last third empty [producer on a predecessor's game] | [Game Developer](https://www.gamedeveloper.com/design/city-management-mayhem-and-sid-meier-s-wisdom-making-i-civilization-vi-i-) |
| Firaxis team | Civ VII interface named top priority after launch; no specific design admission in that post | [2K](https://civilization.2k.com/en-GB/civ-vii/news/message-from-firaxis-games/) |
| Reynolds | Alpha Centauri multiplayer effort better spent on solo play; early dry versions | [CivFanatics](https://civfanatics.com/misc/interview-with-brian-reynolds/); [Arcade Attack](https://www.arcadeattack.co.uk/brian-reynolds/) |
| Meier | Dinosaurs never became fun; early demo AI crushed newcomers; flood mechanic amused only the designer [via others] | [Buteau](https://www.antoinebuteau.com/lessons-from-sid-meier/); [Keller](https://www.gamedeveloper.com/game-platforms/just-one-more-turn---game-development-tips-and-tricks-from-the-creator-of-civilization-sid-meier-) |
| Fischer | Team-wide blind spots from long familiarity; weak raiders until demo feedback | [After the Demo](https://www.paradoxinteractive.com/games/millennia/news/after-the-demo) |

### Inferences
- Checklist for the owner's first hand-played diary, drawn from the above: Is any action free and repeatable for a tiny gain? Can every rival action be explained from the screen? Does any visible resource not matter? Is there a stretch with nothing happening? Is any penalty a wall? Is anything you see never used?
- **Designers disagree, and this is useful.** Stacking versus 1UPT (Shafer moved back; Old World keeps 1UPT, title only, unread). Automation (Johnson wary, Civ VII adding). Exploits (Sas keeps some, Johnson removes those that waste time). AI smartness (Meier wary, Reynolds and Johnson want it honest). Balance (De Waubert relaxed, Fåhraeus worried).

### Gaps
- All admissions are self-selected and most were made years later; none comes with data on the effect of a fix. Humankind, Age of Wonders 4 and Civ VI have no first-person admission in what I read.
- Everything labelled [READ] passed through a summarising model. Two digests were visibly muddled (noted above), so verify any point before quoting or building a rule on it.
