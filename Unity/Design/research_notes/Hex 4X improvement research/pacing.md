# Pacing and game balance in 4X strategy games

Reliability labels used below:
- **[Primary]**: the developer's own page or words, or Steam's own statistics page.
- **[Press]**: an interview or report that quotes the developer.
- **[Community]**: forum or player report, not official.

Research dated 5 Nov 2026. Most pages were read through an automated page reader that summarises and extracts quotes, so check any quote against the page before reprinting it. Steam percentages are live and will drift.

## Typical game length, era length, turn counts and session lengths

### Takeaway
Only a few numbers are officially confirmed: Civ VII at 150-200 turns and 3-4 hours per Age (three Ages), and Beach's 15-20 hours per Civ game. Old World's 200-turn cap comes from press. Other turn tables here are community-sourced. Paradigm Realm's roughly 222-250 turns is close to Old World's cap and well under Civ VI's reported 500-turn Standard limit.

### Cited Findings
**Civ VII**
- [Primary] An Age lasts "around 150 to 200 turns on standard speed", roughly 3-4 hours. Each turn adds a small amount of Age Progress, and Legacy Path milestones add larger amounts. The same diary warns that older diaries may not match the current game. — [Civ VII dev diary: Ages](https://civilization.2k.com/civ-vii/game-guide/dev-diary/ages)
- [Press] "We're only at three ages to get through the game." — [Game Developer, 23 Aug 2024](https://www.gamedeveloper.com/design/firaxis-big-swing-with-civilization-vii-convincing-players-to-actually-finish-their-games)
- [Community] Unconfirmed claims: the Crisis triggers when a counter reaches 200 on Standard ([CivFanatics](https://forums.civfanatics.com/threads/overview-of-mechanics.691596)). Speed scaling is reported as 50/66/100/150/300 percent, with age length scaling 80/100/120 percent ([Steam thread, one of those returned by search; the specific thread was not isolated](https://steamcommunity.com/app/1295660/discussions/0/591762729907270463)).

**Civ VI**
- [Press] Ed Beach, paraphrased by the article: an average player might spend 15-20 hours on a single game. — [Game Developer, 23 Aug 2024](https://www.gamedeveloper.com/design/firaxis-big-swing-with-civilization-vii-convincing-players-to-actually-finish-their-games)
- [Community, weak] Turn limits reported as Online 250, Quick 330, Standard 500, Epic 750, Marathon 1500. No Firaxis source was found. — [cyberpost guide](https://cyberpost.co/how-many-turns-is-the-average-civ-6-game/); Standard 500 also in [GLS wiki FAQ](https://www.gameslearningsociety.org/wiki/what-happens-when-you-reach-turn-500-in-civ-6/)

**Old World**
- [Press] Games are "capped at 200 turns" (the article's wording, not a Johnson quote), and each turn is one year. — [PCGamesN](https://www.pcgamesn.com/old-world/civilization-too-long)
- [Primary] Johnson refers to "a 20-hour 4X game" when arguing about endings. This is an illustrative figure, not a measurement. — [Designer Notes #11, 14 Oct 2021](https://www.designer-notes.com/?p=1748)

**Humankind**
- [Community] Normal speed ends at 300 turns and Endless at 600. One user calculates Endless as about 7.77x slower than Blitz from sample costs (first tech 70 vs 9 science). — [Amplitude forum, Aug 2021](https://community.amplitude-studios.com/amplitude-studios/humankind/forums/168-general/threads/43542-game-speed-blitz-to-endless-comparison)

**Paradigm Realm for comparison (figures supplied in the brief)**
- Eras finish around turns 36-47, 87-108, 141-166 and 187-202. Earliest Transcendence comes at turns 222-250.

### Inferences
- Civ VII's official numbers imply about 450-600 turns and 9-12 hours for a full campaign if all three Ages run the stated length. They also imply about 1-1.5 minutes per turn. Beach's 15-20 hours was said about the average Civ game before the change, so the comparison is suggestive only.
- Paradigm Realm's implied era lengths are about 36-47, then about 50-60, about 55-60 and about 40-45 turns, and earliest Transcendence comes only about 20-60 turns after the fourth era. The brief gives no finish turns for eras 5 and 6, so it is not known whether those games reach them. That is far shorter per era than a Civ VII Age. **The last two eras may be compressed or never finished**, which could be a good "accelerating finish" or could mean late technology and content goes unseen. Check by logging era 5 and 6 completion in the headless runs.
- A target of about 15-20 hours is plausible only if turns late in the game take about 4-5 minutes. Turn time should be logged before setting any session target.

### Gaps
- **Civ IV and Civ V:** no verified turn or speed table. The Fandom page for Civ IV speeds returned an error, and no Firaxis source was found. Figures from memory are deliberately left out.
- **Civ VII per-speed turn tables:** no official source found.
- **Endless Legend:** nothing reliable found. Search snippets conflicted (150 vs 300 vs 600 turns), could not be tied to specific pages and came from forums or a review. Treat as unknown.
- **Humankind, unverified snippets not tied to a page:** a 75-600 turn range, a "50 turns per era" standard, Contemporary era around turn 200. No Amplitude dev diary was found to confirm per-era targets.
- **Measured median hours or turns to finish:** none found for any of these games.

## What designers have said about snowball, runaway leaders, the late-game slump, decisions per turn and busywork, and which fixes worked

### Takeaway
The problem is openly acknowledged across the genre. Fixes fall into three families: reset or chapter structures, late-game pressure or crises, and earlier or better endings. The strongest low-risk lesson for Paradigm Realm comes from Old World's early-win mechanic. Civ VII's resets show the cost of a heavy-handed fix: commercial and reception problems followed, though no source here shows the resets caused them.

### Cited Findings
**Diagnosis**
- [Primary] Firaxis names three root causes behind Ages: snowballing, micromanagement and Civ balancing. "late-game Civilization is just not that fun to play." — [Civ VII Ages diary](https://civilization.2k.com/civ-vii/game-guide/dev-diary/ages)
- [Primary] Snowballing means "your choices and decisions have little effect on the ultimate outcome." As empires grow, more required actions make "every decision feel less important." — [same](https://civilization.2k.com/civ-vii/game-guide/dev-diary/ages)
- [Primary] Johnson: "The hardest part of a 4X to design is the ending." Past a certain point "the player still needs to push End Turn fifty more times." — [Designer Notes #11](https://www.designer-notes.com/?p=1748)
- [Press] William Dyce (Humankind): the "economic snowball" problem "is pretty endemic to turn-based strategy games." Ed Beach: "you want to reward players who are making smart decisions", which the article notes can cause snowballing past AIs. Stephen Muray (Stellaris): "The AI often can't keep up with the growth of a high-skill player." Jack Lusted (Total War): the "same old issue of rising player power" persists. — [PCGamesN feature, Dec 2021](https://pcgamesn.com/civilization-vi/strategy-game-endgame-design)
- [Press] Beach said players "hated" Civ VI's pacing. Firaxis reviewed which systems needed "important, strategic decisions you don't have to make more than every five to 10 minutes" and which were "tiresome 'click here, click here, click, here' decisions". — [Game Developer](https://www.gamedeveloper.com/design/firaxis-big-swing-with-civilization-vii-convincing-players-to-actually-finish-their-games)
- [Primary] Civ IV: "Removing unnecessary micromanagement from the game was a high-level design goal", and "Beaker and hammer overflow was introduced to end the incentive for min-maxing your citizens each and every turn." — [Slashdot Q&A, relayed by CivFanatics](https://forums.civfanatics.com/threads/slashdot-interview-of-the-civ4-team.134142)

**Fixes tried**
- *Age resets and catch-up (Civ VII).* [Primary] At a transition "All civilizations begin anew with an updated Civics and Technology Tree"; leaders, commander experience and wonders persist. Falling behind is offset: "an Age Transition will offer you a way to get back into the game." Golden and Dark Age Legacies follow Legacy Path results. — [Ages diary](https://civilization.2k.com/civ-vii/game-guide/dev-diary/ages)
- *Reception of resets.* [Press, secondary] May 2026 coverage says Take-Two's Zelnick "openly recognizes that the publisher took things too far", and that Civ VII trails Civ VI and V in Steam concurrents with a Mixed user score. It remains "a profitable enterprise". Exact Zelnick wording was not read. — [TheGamer via OpenCritic](https://opencritic.com/news/30779/take-two-boss-admits-civilization-7-went-a-bridge-too-far-with-its-massive-changes)
- *Crises (Civ VII).* [Primary] Crises come near an Age's end, "in stages", and intensify. They are meant to make late-Age play climactic. — [Ages diary](https://civilization.2k.com/civ-vii/game-guide/dev-diary/ages)
- *Early win for dominance (Old World).* [Primary] Double victory triggers "when the player doubles the score of the next closest AI". This avoids dragging out a decided game. Johnson chose not to let AIs win by ambitions because "a surprise ending to a 20-hour 4X game is not a good ending". "a single-player 4X always orbits the human." — [Designer Notes #11](https://www.designer-notes.com/?p=1748)
- *Coalition against the leader.* [Press] Shogun 2's Realm Divide was "a tipping point" where most factions turn on the leader. Humankind's team was improving AI to recognize and gang up on a runaway. — [PCGamesN](https://pcgamesn.com/civilization-vi/strategy-game-endgame-design)
- *Endgame time pressure.* [Press] Warhammer II's Vortex shifted the endgame "to one of time pressure", because a rival can finish first. — [PCGamesN](https://pcgamesn.com/civilization-vi/strategy-game-endgame-design)
- *Guided finishes.* [Press] Beach said Civ VI's Future Era options are "very carefully designed to power you towards a specific victory". The article says many players still feel the endgame needs work. — [PCGamesN](https://pcgamesn.com/civilization-vi/strategy-game-endgame-design)
- *Soft caps done wrong.* [Primary] Jon Shafer on Civ V: global happiness "strongly encouraged you to stay small". His rule: "penalties should be challenges to overcome, not an insurmountable wall". His 1UPT production throttle also caused "more waiting around for buckets to fill up". — [Revisiting the Design of Civ 5, Feb 2013](https://gamedeveloper.com/design/revisiting-the-design-of-civ-5)
- *Keep the AI's late behaviour legible.* [Primary] Shafer: an ally late in the game "has no qualms backstabbing you in order to win", which undercut relationships. — [same](https://gamedeveloper.com/design/revisiting-the-design-of-civ-5)
- *Limit orders.* [Press] Old World limits orders stored per turn, and techs and buildings raise the cap. Decisions stay scarcer as the empire grows. — [PCGamesN](https://www.pcgamesn.com/old-world/civilization-too-long)

### Inferences
Ranked for Paradigm Realm as a judgement call, not from data:
1. **Early-win rule:** something like Old World's double victory, or a "Concede and see the score" offer once a rival race is clearly lost. Cheap, and it directly removes "push End Turn fifty more times".
2. **Visible race:** show rival Transcendence progress and warnings well before activation, since Johnson warns against surprise AI wins. The three-stage project already fits this, but only if the player sees it coming.
3. **Late-game action count:** add per-turn action logs and automate low-value actions, since Beach's test (a "five to 10 minute" decision vs clicking) is simple to apply system by system.
4. **Crisis or coalition pressure late:** the existing Ages (Golden, Dark, Heroic) are a natural hook. Prefer stages with warning.
5. **Avoid hard resets:** the Civ VII reception suggests caution, though the sources do not prove resets were the cause.

"Which fixes worked" has no controlled evidence. The claims above are designers' stated intent or indirect signals.

### Gaps
- No source directly tests the effect of any single fix.
- Jon Shafer's "At the Helm": no direct text was found, so it is not used here.
- Sid Meier's own writing on decisions: not read for this topic, so no claims are made.
- Decisions per turn: no published numbers found. Only qualitative statements.

## How designers measure pacing, and published completion figures

### Takeaway
The best public number is Beach's "less than 50 percent" for Civ VI, supported by Steam's 37.4 percent first-win achievement. No studio was found publishing its telemetry methods. AI autoplay as a pacing tool is largely undocumented in public, so Paradigm Realm's headless runs are a reasonable practice, not a published standard.

### Cited Findings
- [Press] Beach on Civ VI: completion was "surprisingly, depressingly—whatever adjective you want to put in there—low. It was less than 50 percent." The article mentions scheduled single-player and multiplayer playtests but no telemetry method. — [Game Developer, 23 Aug 2024](https://www.gamedeveloper.com/design/firaxis-big-swing-with-civilization-vii-convincing-players-to-actually-finish-their-games)
- [Primary] Firaxis: "more than half of the Civilization VI players have never finished a single campaign!" — [Ages diary](https://civilization.2k.com/civ-vii/game-guide/dev-diary/ages)
- [Primary] Civ VI Steam stats: "Win a regular game at Settler difficulty or harder" 37.4%; Prince or harder 29.9%; Ancient Era start 36.8%; Science win 21.3%; Domination win 20.0%. Most common visible achievement 74.3%. Only about 30 of 320 achievements were visible. — [Steam Global Stats, Civ VI](https://steamcommunity.com/stats/289070/achievements)
- [Primary] Civ VII Steam stats: most common visible achievement 27.9% (3 Major Cultural Triumphs in Antiquity). "Win the modern age as X" runs 4.1-10.0% per leader. "Win a game on Deity" 4.5%. No "win with anyone" achievement was seen in the portion read. — [Steam Global Stats, Civ VII](https://steamcommunity.com/stats/1295660/achievements)
- [Community] Under-40% win analyses of Civ VI from 2018-2020 reach a similar conclusion. Posters caution that a win may not register unless the victory screen is reached. — [CivFanatics, 2020](https://forums.civfanatics.com/threads/fewer-than-40-of-players-have-a-won-a-game-re-revisited.659631/page-2)
- [Press, snippet only] GamesRadar's headline reports that the director says Ages fix "players not getting to the end of their games". Search snippets describe a "much higher completion rate", apparently from Edge issue 405, with no percentage. The full text could not be read. — [GamesRadar](https://www.gamesradar.com/games/strategy/civ-7-director-thought-the-new-ages-system-might-not-work-but-says-it-does-fix-the-number-one-issue-players-not-getting-to-the-end-of-their-games)
- [Community, third-party] Benchmark outside the genre: median about 36% across 19 single-player, non-strategy Steam games, using ending achievements. The page has a date inconsistency and a small sample. — [Two Average Gamers, June 2026](https://www.twoaveragegamers.com/game-completion-rates/)
- [Community] A forum poster citing Gamalytic gave about 19.8 hours average playtime on Steam for Civ VII. Third-party estimate. — [CivFanatics](https://forums.civfanatics.com/threads/firaxis-has-posted-a-first-glimpse-of-civ-7-player-stats.695775/page-4)
- [Press] Johnson on Civ IV: the core game is a standalone DLL, so rules can be updated and playtested often, and AI logic is data-driven. — [Game Watch report of GDC 2008, in Japanese](https://game.watch.impress.co.jp/docs/20080222/ai.htm)

### Inferences
- Steam's percentages are understood to be of all owners (not checked against a Valve source), and Civ VI's most-earned visible achievement is 74.3%. Dividing, about half of active players (37.4 / 74.3 = 50%) have a first win, which fits Beach's "less than 50 percent". This is a rough estimate, not a true completion rate.
- A 36-ish percent all-owner win rate is not unusually bad compared with the 36% non-strategy median. Firaxis's alarm may reflect the length of the game and how much content goes unseen, rather than an outlier rate.
- Civ VII's per-leader figures cannot be summed because they overlap, so its overall completion cannot be derived from what was read. These numbers do not confirm or refute Firaxis's "higher completion" claim.
- **Suggested practice for Paradigm Realm (not from a source):**
  - Keep headless autoplay as a regression check on era timing and victory turn.
  - Record in player saves: turn reached, turns since the last meaningful decision, actions per turn and seconds per turn, per era.
  - Keep any logging local and opt-in.
  - Treat autoplay as checking *timing*, not *fun*. AI runs cannot detect the slump a human feels.
  - Warning signs to check: rising actions per turn with falling outcome variance; a clear score leader long before the end; many runs quitting in one era; content (techs, units) rarely used before the game ends.

### Gaps
- No published telemetry methods or internal AI autoplay practice found for Firaxis, Mohawk or Amplitude. One targeted search returned nothing, so this does not prove none exists.
- No published completion figures found for Old World, Humankind or Endless Legend. Steam achievement pages exist and could be checked (Old World's and Humankind's app IDs were not verified here).
- The Firaxis "first glimpse of Civ 7 player stats" post itself was not found. Only the forum discussion was read, and it did not contain the figures.
- Academic work on game balance was not reviewed. No academic numbers are offered.

## How AI difficulty is handled honestly (bonus tables versus better play)

### Takeaway
The clearest evidence is Soren Johnson on Civ IV. The AI is a game-content problem, difficulty is mostly explicit production and research modifiers, with Noble and Prince close to even, and information cheats are reduced over time. A published, plain bonus table is the honest design. Paradigm Realm should scale both play quality and an open table.

### Cited Findings
- [Primary] Johnson: "Much of the information cheating has been removed from the game"; "the AI has production and research penalties at the lower levels and bonuses at the higher levels"; "the AI never receives any bonuses whatsoever for building wonders"; "Noble and Prince are the difficulty levels where the AI's rules are closest to the human's." — [Slashdot Q&A, relayed by CivFanatics](https://forums.civfanatics.com/threads/slashdot-interview-of-the-civ4-team.134142)
- [Press, translated by a reader tool] GDC 2008: production bonus is zero at Noble and roughly 40% at Deity. Even at Noble, the AI retains some advantages (combat odds and fog-of-war items, extra diplomatic information). Civ I gave free wonders, Civ II had implicit alliances against the human, and Civ III's AI ignored fog in choosing city targets. Later games moved to milder cheats, with a rule to avoid making the AI feel unfair. **Verify the 40% against the original if quoting.** — [Game Watch](https://game.watch.impress.co.jp/docs/20080222/ai.htm)
- [Primary] Johnson says writing AI for Civ is "tricky because it fits somewhere between those two extremes", meaning "good" AI vs "fun" AI. — [Designer Notes: Playing to Lose](https://www.designer-notes.com/playing-to-lose-ai-and-civilization-gdc-2008/)
- [Press] Same talk: fun AI's purpose is to lose enjoyably, and its behaviour is itself content (Tokugawa isolating himself: inefficient but characterful). The Civ IV AI was about 25,000 lines. Blake's Better AI, a community mod, was downloaded over 15,000 times and licensed for Beyond the Sword, with further cheat reductions. — [Game Watch](https://game.watch.impress.co.jp/docs/20080222/ai.htm)
- [Press] GDC 2011 panel: Jon Shafer, "I don't think it makes financial sense to make great AI"; Dustin Browder, the "difficulty lies in making AI feel human". — [Game Developer](https://www.gamedeveloper.com/game-platforms/gdc-2011-strategy-game-veterans-look-at-the-genre-s-biggest-challenges)
- [Primary] Shafer on Civ V: the AI was "too random" and opponents "all crazy, and in the exact same way"; his fix idea is "the fewer moving parts you have the easier it is to tell what's going on". — [Revisiting the Design of Civ 5](https://gamedeveloper.com/design/revisiting-the-design-of-civ-5)
- [Community, weak] Civ VI Deity AI is said to get extra starting settlers (one post says 3). No official table was found. — [CivFanatics thread on Civ VI difficulty bonuses](https://forums.civfanatics.com/threads/civ-specific-difficulty-bonuses.620073/) (search-result snippets; the exact poster's page was not confirmed)
- [Primary] Civ VII: only 4.5% have "Win a game on Deity". — [Steam Global Stats](https://steamcommunity.com/stats/1295660/achievements)
- [Press] Daniel Moregard (Stellaris): strategy games have not taught players that "it is ok to lose". — [PCGamesN](https://pcgamesn.com/civilization-vi/strategy-game-endgame-design)

### Inferences
- Publish a plain difficulty table in-game, with named modifiers, so the player sees what is bought. Civ IV's rule of no wonder bonus is a useful precedent: bonuses that scale economies but do not take the player's signature goals away.
- Keep a "fair" tier like Noble/Prince with zero modifiers, and use it for headless tests. Paradigm Realm's pacing targets should be checked at this tier, then re-checked at the top tier, since a 40%-style bonus would change era timing.
- Scale play quality separately from bonuses. Since Muray says AIs fall behind high-skill growth, higher tiers should improve rival expansion and war decisions first, with yield bonuses only covering what remains.
- Avoid hidden information cheats. Where one remains (for example rival pathfinding through fog), disclose it. Johnson's "feel unfair" rule is the test.
- The Paradigm Realm rivals' 5-6 cities should be compared with human play. If humans routinely reach 10+ cities, snowball and AI pacing diverge and rival-only runs will flatter the balance. This needs measurement.

### Gaps
- No official Civ V, VI or VII difficulty bonus table was obtained. Community tables may exist but were not verified.
- No Old World, Humankind or Endless Legend bonus tables were researched in the time available.
- No figures found on what share of players choose each difficulty, other than achievement proxies.
