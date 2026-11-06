# Difficulty levels and rival AI design in 4X games, from inspectable data and source (second round, 6 Nov 2026)

Labels:
- **[read: file]**: I downloaded the raw source or data file and read the lines myself.
- **[read: page]**: fetched through an automated reader that summarises, so wording may be paraphrase. Check before reprinting.
- **[snippet]**: search summary only.
- **[secondary]**: forum, wiki or a mod rather than the developer.
- **[measured here]**: my own reading of this repository on 6 Nov 2026. Nothing was compiled or run. No game was played.

Corrections to the first round: it said no official difficulty tables or AI architecture primary sources were found. Both now exist below: Johnson's own slides, raw Civ VI game data, Unciv and Freeciv source, and the K-Mod copy of the Civ IV SDK. Still missing: first-party Civ V, Old World numbers and Humankind numbers.

Licensing caution: copy methods, not code. Freeciv's files state GPL v2 or later ([difficulty.c header](https://github.com/freeciv/freeciv/blob/main/ai/difficulty.c)) [read: file]. The Civ IV and Civ VI files are Firaxis material re-uploaded by third parties, so do not vendor them. I did not check Unciv's or K-Mod's licence terms.

## Actual handicap numbers: Civ IV, Civ V, Civ VI, Freeciv, Unciv, Old World, Humankind

### Takeaway
Real tables are now in hand. Shipping games cluster on one pattern: an even middle level, rival unit and building costs running from about 160-175% down to 50-60%, no discount on wonders, and extra starting units and technologies at the top. Freeciv is the outlier: it weakens play through deliberate mistakes and restricted choices, and gives yield bonuses only at its "Cheating" level.

### Cited Findings

**This game: difficulty does nothing [measured here]**
- `difficulty` occurs only three times in `Unity/Assets/Scripts`: `World.cs:110` (field, default 1), `World.cs:207` (parameter, default 1), `World.cs:215` (assignment). Nothing reads it.
- `RealmGame.cs:72` passes a literal `1` in that argument position, and `FoundationUI.cs` has no difficulty control. A case-insensitive search for `difficulty` confirms there is no other mention.

**Civ IV: Johnson's own slide (the most authoritative single row)** [read: file, text extracted from the .ppt in the [slide archive](http://www.designer-notes.com/PlayingToLose.zip)]
- "Civ IV AI Production Modifiers": Settler +50%, Chieftain +20%, Warlord +10%, Noble even, Prince -5%, Monarch -10%, Emperor -15%, Immortal -20%, Deity -40%. The sign means cost change for the AI.
- The next slide: "Noble has other cheats", listing animal and barbarian combat, unit support, unit upgrades, inflation and war weariness, then "AI needs more help in these areas" and "Cheats should NOT be linear!"
- Note the jump from -20% to -40% at the top. That is the non-linearity in practice.

**Civ IV: XML values**

Sources and their limits:
- 2005 base-game file with a Firaxis header naming Soren Johnson: [Broken-BTS copy, base folder](https://github.com/rigiechang/Broken-BTS/blob/master/sid-meiers-civilization-iv/Assets/XML/GameInfo/CIV4HandicapInfo.xml) [read: file, in a mod's repository].
- BtS file: [Broken-BTS copy, BtS folder](https://github.com/rigiechang/Broken-BTS/blob/master/sid-meiers-civilization-iv-beyond-the-sword/Beyond-the-Sword/Assets/XML/GameInfo/CIV4HandicapInfo.xml) [read: file]. **This copy is modded in some human-side columns**: for example Noble `iResearchPercent` is 150 here, while [Modiki](https://modiki.civfanatics.com/index.php/Civ4HandicapInfo) gives 100.
- So only rows that agree with Modiki at its three listed levels are reproduced as BtS. Modiki's "Chieftain" column actually holds the Settler values [read: page, secondary]. The levels in between are not independently verified.
- The slide and the 2005 XML disagree slightly: Settler +50% against 160, Chieftain +20% against 130. The XML is what the game reads, so prefer it.

| Field (AI cost unless noted, %) | Settler | Chieftain | Warlord | Noble | Prince | Monarch | Emperor | Immortal | Deity |
|---|---|---|---|---|---|---|---|---|---|
| Train, Construct, Create (units, buildings, projects) | 160 | 130 | 110 | 100 | 95 | 90 | 85 | 80 | 60 |
| World train, construct, create (wonders) | 160 | 130 | 110 | 100 | 100 | 100 | 100 | 100 | 100 |
| Growth (BtS) | 160 | 130 | 110 | 100 | 100 | 95 | 90 | 85 | 80 |
| Growth (2005 base) | 160 | 130 | 110 | 100 | 95 | 90 | 85 | 80 | 60 |
| Unit support, civic upkeep | 100 | 100 | 100 | 100 | 95 | 90 | 85 | 80 | 60 |
| Unit upgrade (BtS, flat) | 50 | 50 | 50 | 50 | 50 | 50 | 50 | 50 | 50 |
| War weariness (BtS) | 100 | 90 | 80 | 80 | 80 | 75 | 70 | 60 | 50 |
| Inflation (BtS) | 100 | 90 | 80 | 80 | 80 | 80 | 80 | 80 | 80 |
| Per-era modifier (points per era) | 0 | 0 | 0 | 0 | -1 | -2 | -3 | -4 | -5 |
| Worker rate bonus | 0 | 0 | 0 | 0 | 10 | 20 | 50 | 75 | 100 |
| Free AI technologies (count) | 0 | 0 | 0 | 0 | 0 | 1 | 2 | 3 | 4 |
| Extra AI defenders at start | 0 | 0 | 0 | 0 | 0 | 1 | 2 | 3 | 4 |
| War declaration factor (2005) | 25 | 50 | 75 | 100 | 100 | 100 | 100 | 100 | 100 |
| Attitude change | +2 | +1 | 0 | -1 | -1 | -1 | -1 | -1 | -1 |
| **Human** tech cost (2005) | 60 | 75 | 90 | 100 | 110 | 115 | 120 | 125 | 130 |
| **Human** free units (2005) | 24 | 18 | 12 | 8 | 6 | 4 | 3 | 2 | 1 |
| **Human** unit cost (2005) | 20 | 30 | 40 | 50 | 60 | 70 | 80 | 90 | 100 |

Other points on Civ IV:
- Even at Noble the AI has -25% barbarian and -40% animal combat modifiers in BtS (-40 and -70 in 2005), and the BtS file gives the AI 50% upgrades on every level. These are the "other cheats" on Johnson's slide.
- How it is applied: the handicap percent multiplies the production need, then is multiplied again by `max(0, perEra*era + 100)`, per [K-Mod's CvPlayer.cpp lines 6671-6680](https://github.com/karadoc/Civ4-K-Mod/blob/master/CvGameCoreDLL/CvPlayer.cpp) [read: file, K-Mod not vanilla, lines read by search].
- Human research cost is part of difficulty too. The AI appears to gain relative to the human partly because the human pays more, so comparing AI rows alone understates the gap.
- A respected fan rebalance is a useful second opinion: [AdvCiv](https://github.com/f1rpo/AdvCiv/blob/master/Assets/XML/GameInfo/CIV4HandicapInfo.xml) [read: file, secondary] uses a narrower AI range (train 125 to 85, research 90 to 124 with human 72 to 150) and raises human costs at the top instead.

**Civ V** [read: page, one forum post citing `CIV5HandicapInfos.XML`, 3 Oct 2010; not first-party, and patches may differ]. Source: [CivFanatics post #7](https://forums.civfanatics.com/threads/ai-bonuses-at-different-difficulty-levels.387484/post-9711367). Unciv's numbers below match the King to Deity costs, growth and upkeep closely, which corroborates it.

| AI % | Settler | Chieftain | Warlord | Prince | King | Emperor | Immortal | Deity |
|---|---|---|---|---|---|---|---|---|
| Growth | 160 | 130 | 110 | 100 | 90 | 85 | 75 | 60 |
| Train | 175 | 130 | 110 | 100 | 85 | 80 | 65 | 50 |
| Construct, Create | 160 | 130 | 110 | 100 | 85 | 80 | 65 | 50 |
| Wonders (World*) | 160 | 130 | 110 | 100 | 100 | 100 | 100 | 100 |
| Building and unit upkeep | 100 | 100 | 100 | 100 | 85 | 80 | 65 | 50 |
| Unit supply | 0 | 0 | 10 | 20 | 30 | 30 | 40 | 50 |
| Unit upgrade | 50 | 50 | 50 | 50 | 50 | 50 | 50 | 50 |
| Unhappiness | 100 | 100 | 100 | 100 | 90 | 85 | 75 | 60 |
| Worker rate | 0 | 0 | 0 | 0 | 20 | 50 | 75 | 100 |
| Per era | 0 | 0 | 0 | 0 | -2 | -3 | -4 | -5 |
| War declaration | 0 | 75 | 85 | 100 | 100 | 100 | 100 | 100 |
| Free techs | 0 | 0 | 0 | 0 | 1 | 2 | 3 | 4 |

A Vox Populi (Community Patch) table has been reported with growth 110 at Settler to 70 at Deity and AI free XP. That is a mod, not vanilla ([thread](https://forums.civfanatics.com/threads/way-too-many-questions-about-difficulty-handicaps.631797)) [read: page, secondary].

**Civ VI** [read: file, raw `Leaders.xml` and `Eras.xml` from [mrobaczyk/civ6](https://github.com/mrobaczyk/civ6), a third-party dump of game files; base-game folder only, expansions may override]
- Modifiers apply only to `PLAYER_IS_HIGH_DIFFICULTY_AI` (handicap at or above Prince), and low-difficulty ones only to the human at Warlord or below ([Leaders.xml](https://github.com/mrobaczyk/civ6/blob/main/Base/Assets/Gameplay/Data/Leaders.xml), lines about 1433-1490, 3487-3602, 5942-5954, 6664-6669).
- All use `LinearScaleFromDefaultHandicap`. The per-step amounts are: science, culture and faith +8; production and gold +20; combat +1 (starting value -1); unit XP +10; free tech and civic boosts +1. For a low-difficulty human: combat -1 step and XP -15 step (the sign is against the level, so lower levels help the human).
- The XML gives only the slope. The zero point is an engine rule and not in the file.
- [Eras.xml](https://github.com/mrobaczyk/civ6/blob/main/Base/Assets/Gameplay/Data/Eras.xml) lines 43-45: AI-only extra Ancient start units are Settler from Emperor (delta 0.5), Warrior from King (delta 1), Builder from King (delta 0.5). Rounding was not traced.
- Community tables say +40% science and +100% production at Deity ([snippet, disputed by a poster who says the wiki and XML differ](https://steamcommunity.com/app/289070/discussions/0/135512931355462387/?l=schinese)). A modder says +4 combat at Deity ([snippet](https://forums.civfanatics.com/resources/deity.25865/)).

**Unciv** [read: file, [Difficulties.json, Gods & Kings ruleset](https://github.com/yairm210/Unciv/blob/master/android/assets/jsons/Civ%20V%20-%20Gods%20%26%20Kings/Difficulties.json)]. Modifiers are multipliers.

| Field | Settler | Chieftain | Warlord | Prince | King | Emperor | Immortal | Deity |
|---|---|---|---|---|---|---|---|---|
| AI city growth | 1.6 | 1.3 | 1.1 | 1 | 0.9 | 0.85 | 0.75 | 0.6 |
| AI unit cost | 1.75 | 1.3 | 1.1 | 1 | 0.85 | 0.8 | 0.65 | 0.5 |
| AI building cost | 1.6 | 1.3 | 1.1 | 1 | 0.85 | 0.8 | 0.65 | 0.5 |
| AI wonder cost | 1.6 | 1.3 | 1.1 | 1 | 1 | 1 | 1 | 1 |
| AI building and unit maintenance | 1, 1 | 1, 1 | 1, 1 | 1, 1 | .85, .8 | .8, .75 | .65, .65 | .5, .5 |
| AI unit supply bonus | 0 | 0 | 0.1 | 0.2 | 0.3 | 0.3 | 0.4 | 0.5 |
| AI unhappiness | 1 | 1 | 1 | 1 | 0.9 | 0.85 | 0.75 | 0.6 |
| AI free techs | 0 | 0 | 0 | 0 | 1 | 2 | 3 | 4 |
| AI extra start units ("soldier" = the file's Era Starting Unit) | 0 | 0 | 0 | 0 | 1 soldier | soldier, Scout | Worker, Scout, 2 soldiers | Settler, Worker, Scout, 3 soldiers |
| AIs trade techs | no | no | no | yes | yes | yes | yes | yes |
| Human research cost | 0.9 | 0.95 | 1 | 1 | 1 | 1 | 1 | 1 |
| Human unit, building, policy cost | 0.5 | 0.67 | 0.85 | 1 | 1 | 1 | 1 | 1 |
| Human base happiness | 15 | 12 | 12 | 9 | 9 | 9 | 9 | 9 |

The field `aiDifficultyLevel` is "Chieftain" on all levels but Prince. I did not trace what reads it.

**Freeciv** [read: file, [ai/difficulty.c](https://github.com/freeciv/freeciv/blob/main/ai/difficulty.c), [ai/handicaps.c](https://github.com/freeciv/freeciv/blob/main/ai/handicaps.c), main branch]

| Level | Fuzzy (chance in 1000 of mis-deciding) | Science cost % | Expansion % | Restrictions (handicaps set) |
|---|---|---|---|---|
| Novice, Restricted | 400 | 250 | 10 | RATES, TARGETS, HUTS, NOPLANES, DIPLOMAT, LIMITEDHUTS, DEFENSIVE, DIPLOMACY, REVOLUTION, EXPANSION, DANGER, CEASEFIRE, NOBRIBE_WF, PRODCHGPEN |
| Easy | 300 | 100 | 10 | As above without DANGER and PRODCHGPEN |
| Normal | 0 | 100 | 100 | RATES, TARGETS, HUTS, DIPLOMAT, CEASEFIRE, NOBRIBE_WF |
| Hard | 0 | 100 | 100 | RATES only ("No actual handicaps" in a code comment) |
| Cheating | 0 | 100 | 100 | As Hard, plus ruleset effects |

- Cheating effects in [civ2civ3 effects.ruleset](https://github.com/freeciv/freeciv/blob/main/data/civ2civ3/effects.ruleset) lines 48-141: +40 science, luxury, gold and shields; food upkeep -100; +1 content; veteran combat +40; bribe and incite cost +40; pollution -20; AI "love" +40. The `sandbox` ruleset has the same rows and `classic` has none, so this is per ruleset [read: file].
- `H_FOG` and `H_MAP` are set only for Away mode, so by the bit list and `handicap_desc`, all ordinary levels may see through fog and know the whole map. `H_TARGETS` and `H_HUTS` also come off at Hard. This is my reading of the code and is opposite to Johnson's direction of travel.
- [README.AI](https://github.com/freeciv/freeciv/blob/main/doc/README.AI): "'hard' level is no-holds-barred"; it also says the AI remains "too easy for experienced players" [read: file].

**Old World (first-party, words only, no numbers)** [read: page] Update #93, 6 July 2022 ([Mohawk](https://mohawkgames.com/?p=3363)):
- The Able: passive AI with moderate penalty, no Force March. The Just: peaceful AI with small penalty, no Force March. The Good: peaceful, fledgling development, weak tribes. The Strong: normal aggression, fledgling. The Glorious: aggressive, established. The Magnificent: aggressive, advanced development, small advantage. The Great: competitive, advanced development, moderate advantage.
- Update 116 added The New and reworked low-level penalties ([Mohawk](https://mohawkgames.com/2023/11/22/old-world-update-116/)) [read in round one].
- The reader flagged its wording for The Glorious as uncertain. This shows a different structure: difficulty also sets aggression and starting development, and only the top two levels have an explicit "advantage".

**Humankind:** no table found. Players on [Amplitude's forum](https://community.amplitude-studios.com/amplitude-studios/humankind/forums/168-general/threads/56119-game-difficulty?page=1) ask and get no exact answer [snippet]. The [Fandom page](https://humankind.fandom.com/wiki/Difficulty) refused automated reading (HTTP 402), and I did not try to get around that.

### Inferences
- Common shape: about 8-9 levels, an even level near the middle, top cost multiplier 0.5-0.6 (1.67x to 2x effective output), with wonders never discounted in Civ IV, Civ V or Unciv.
- Effective output comparison at the top, from cost or yield: Civ V and Unciv units 2x; Civ IV 1.67x before the era modifier and 2.4x at Future (60% x 70% = 42%, my calculation); Freeciv Cheating 1.4x plus free food upkeep.
- Rival research is rarely discounted directly in Civ IV, Civ V or Unciv. Those games use free early technologies and, in Civ IV, dearer research for the human. Civ VI and Freeciv Cheating do boost science.
- A small project does not need 8-9 levels. Old World's mix of aggression and advantage plus Freeciv's mistakes suggests 5-6 levels that change behaviour as well as costs.

### Gaps
- No first-party Civ V table. The file exists in an install's `Assets/Gameplay/XML/GameInfo/`, and a local copy would settle it.
- A clean, unmodded BtS XML could not be found online. Intermediate BtS levels are therefore not independently verified.
- Old World XML numbers and any Humankind numbers were not found. Civ VII not sought.
- Civ VI's zero point for the linear scale and any expansion overrides were not traced.

## How shipping and open AI is structured, in forms a small team could copy

### Takeaway
None of these uses search or machine learning at strategic level. The common recipe is layered scoring: a once-a-turn empire state, weighted values with named reasons, a little noise and hard safety rules per unit. The best method to copy is Unciv's named modifier list for war decisions, Civ IV's per-role attack odds, and Dill's rank-plus-weight choice.

### Cited Findings

**Johnson's slides** [read: file, [archive](http://www.designer-notes.com/PlayingToLose.zip)]
- Civ IV AI: "10 Files", "25,000 lines of code", "Soft-Coded Decision Making".
- Method: "Assign weights to various factors", then "Adjust values to situation", "Add random noise", "Make a decision".
- "No AI scripts", "No enums"; less predictable, and his own caveat is "not always a good thing".
- Testing: traditional testing fails, automation helps greatly, hard-core fans are needed; 1.5-year closed beta peaking at 100 users, bi-weekly patches.
- Naval fix: "Randomly update assault targets" and "Ignore temporary data, such as nearby units". This followed "Amphibious Assault Judo", where players exploited Civ III's fog-ignoring targeting with empty port cities.
- Designing for the AI: global unit support, closed borders, collateral damage, enforced peace treaties, and maintenance against city spam. "Design cannot ignore AI" for symmetrical games.
- A speaker note warns about learning AI: Civ III's advisor learned from choices and caused hard-to-find bugs.
- The talk also [says](https://rosetta.to/u/googletechtalks/playing-to-lose-ai-and-civilization-soren-johnson) targets were updated every 8 or 12 turns, and that naval and air combat were hardest [read: page, auto-caption summary, secondary].

**Civ IV SDK via K-Mod** [read: file, parts only; [CvPlayerAI.cpp](https://github.com/karadoc/Civ4-K-Mod/blob/master/CvGameCoreDLL/CvPlayerAI.cpp), [CvCityAI.cpp](https://github.com/karadoc/Civ4-K-Mod/blob/master/CvGameCoreDLL/CvCityAI.cpp), [CvUnitAI.cpp](https://github.com/karadoc/Civ4-K-Mod/blob/master/CvGameCoreDLL/CvUnitAI.cpp). K-Mod extends Firaxis and Better AI code, so some details are not vanilla]
- Empire state: a bitfield `m_iStrategyHash` recomputed per player with flags such as `AI_STRATEGY_PRODUCTION`, `TURTLE`, `DAGGER`, `CRUSH`, `LAST_STAND`, `FINAL_WAR`, `ALERT1/2`, `ECONOMY_FOCUS` (CvPlayerAI.cpp lines about 20474-21393). City and unit code then check flags.
- City build: `AI_chooseProduction` (CvCityAI.cpp:520) is an ordered list of conditions and about 95 `AI_chooseUnit` / `AI_chooseBuilding` calls, not one grand score.
- Unit roles: units carry a `UNITAI_` role, with 87 distinct `case UNITAI_` lines by a rough text count in CvUnitAI.cpp (not a count of roles).
- Per-role attack thresholds: `AI_anyAttack(1, 65)`, `(1, 90)`, `(1, 20)`, `(1, 0)` (lines 473, 1329, 2034, 2057, 2332), with `AI_heal()` as an early branch at lines such as 1335 and 2161.
- Site and tech scoring: `AI_foundValue` (2646), `AI_techValue` (5478), with flavour multipliers such as line 6664.

**Unciv** [read: file, [automation folder](https://github.com/yairm210/Unciv/tree/master/core/src/com/unciv/logic/automation)]. The best small-team model; plain data-driven Kotlin.
- War decision: [MotivationToAttackAutomation.kt](https://github.com/yairm210/Unciv/blob/master/core/src/com/unciv/logic/automation/civilization/MotivationToAttackAutomation.kt) builds a list of `Pair("Reason", value)` and sums them. War is considered when the total exceeds 0 ([DiplomacyAutomation.kt](https://github.com/yairm210/Unciv/blob/master/core/src/com/unciv/logic/automation/civilization/DiplomacyAutomation.kt) line 394). Modifiers include base `-15 x inverse(DeclareWar)`; relative strength bands (ratio over 5 is +20, down through steps); other wars -20 each; target's wars +3 each; relative score and production; distance; friendship -10; defensive pact -15; research agreement -5; isolated city +10; and **"About to win" +15**. Slower path checks only run if the cheap sum passes (`Short-circuit to avoid A-star`).
- Self-correction for difficulty: for AI against human, strength ratio is multiplied by `aiUnitCostModifier^1.5`, so a discounted AI does not become over-aggressive merely from its bonus (lines 286-291). This is an unusually clean idea.
- Unit decision order: [UnitAutomation.kt](https://github.com/yairm210/Unciv/blob/master/core/src/com/unciv/logic/automation/unit/UnitAutomation.kt) lines 97-115: accompany settlers, ruins, below 50 health retreat or heal, heal if safe, relieve besieged city, attack in range, retake captured cities.
- Attack filter: [BattleHelper.kt](https://github.com/yairm210/Unciv/blob/master/core/src/com/unciv/logic/automation/unit/BattleHelper.kt) lines 21-22 only allows an attack if damage taken is less than current health, and `getUnitAttackValue` prefers wounded targets.
- Order of a whole turn: `automateCivMoves` in [NextTurnAutomation.kt](https://github.com/yairm210/Unciv/blob/master/core/src/com/unciv/logic/automation/civilization/NextTurnAutomation.kt) lines 47-92: diplomacy first, then gold, then units.

**Freeciv** [read: file, [README.AI](https://github.com/freeciv/freeciv/blob/main/doc/README.AI)]
- Common currency: every build candidate has a single "want", with bands such as `DAI_WANT_MILITARY_EMERGENCY`.
- Discount for delay: `amortize(benefit, delay) = benefit * ((MORT-1)/MORT)^delay`, with MORT 24 as about 4.3% per turn.
- Attack value: `Want = Operation_Profit * Amortization_Factor`; `Battle_Profit = Shields_Lost_By_Enemy * P(win) - Shields_Lost_By_Us * P(lose)`.
- Deliberate error: `ai_fuzzy` is used at decision points such as [daicity.c:573](https://github.com/freeciv/freeciv/blob/main/ai/default/daicity.c).
- Honest warning: diplomacy "usually gains only human players" (paraphrase of the README section).
- A/B testing practice: an `experimental` level exists to test new code against `hard`.

**Game AI Pro: dual utility** [read: file, Kevin Dill, [Game AI Pro 2, chapter 3](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter03_Dual-Utility_Reasoning.pdf), used in Zoo Tycoon 2]
- Each option gets a **rank** (absolute importance, categories) and a **weight**.
- Steps: (1) drop options with weight 0 or less; (2) keep only the highest rank; (3) optionally drop options under a percentage of the top weight; (4) weight-based random among what remains.
- Pure absolute utility is predictable, and pure random can pick stupid options, so this combines them.
- Not a 4X case study. Round one's concern about checking the summary is resolved: the summary was accurate.

### Inferences
Suggested architecture for this game, in order of value:
1. A small per-nation **posture** recalculated each turn or every few turns (Peace, Expand, Build, Defend, Attack(target), Race). Civ IV's strategy flags in miniature. Rival city and unit code reads it.
2. **Named modifier lists** for war, peace and victory race, returned with their reasons. They make testing and in-game explanations ("why is Rome hostile?") nearly free, which also serves the transparency finding below.
3. **Hard unit rules before scores**: heal or retreat, then attack if the exchange is good, then move. Copy Unciv's order.
4. **Dual utility for build and research choice**, using rank for emergencies and weight with personality for flavour.
5. **Retarget slowly** (Johnson's 8-12 turns) and ignore temporary nearby units in target choice. This also saves turn time.
6. **A/B harness:** allow a per-nation handicap and "AI version" in tests, so old and new rivals can play in one game.

### Gaps
- Game AI Pro has no 4X-specific chapter that I found. Other chapter titles from round one were not read.
- No primary source on Civ V, VI, VII, Old World or Humankind AI architecture beyond data files. Johnson's GDC 2022 Old World postmortem was not found.
- Only part of each large source file was read: 85,000 lines in the K-Mod set. Claims are limited to what is cited.

## How players judge fairness

### Takeaway
By Johnson's account, perception decides and players are poor judges of actual cheating. Slow, small and explained bonuses are accepted. Sudden, visible or hidden-looking events are not. Note he treats difficulty and cheating as different things in the player's mind, so a difficulty table the player chose and can read is the honest route.

### Cited Findings
Article: Soren Johnson, "Analysis: Game AI & Our Cheatin' Hearts", first in Game Developer magazine, May 2009; online 14 Sep 2009 ([Game Developer](https://www.gamedeveloper.com/game-platforms/analysis-game-ai-our-cheatin-hearts)) [read: page; magazine original not seen]. Quotes are as returned by the reader.
- "Cheating is not the same thing as difficulty levels"
- "transparency is the antidote to suspicion and distrust"
- "When the question is one of fairness, the player is always right."
- "For asymmetrical games, cheating is simply bad game design"
- Accepted: small incremental Civ discounts; open catch-up rules, such as Mario Kart item boxes and the Catan robber.
- Rejected: free units in fog and instant Wonders; hidden rubber-banding in racing games; AI that seems to gang up (original Civ); AI tech trading seen as a cartel (Civ III), which led Civ IV to limit it.
- Puzzle Quest: random refills that help the AI read as cheating although they were not. He suggests hidden luck should only favour the human, and AI power should come from explained items.
- Slides [read: file, [archive](http://www.designer-notes.com/PlayingToLose.zip)]: "Should Never Feel Unfair"; "Cheating is for Challenge, which is one source of Fun"; "We want players to win or, at least, understand why they lost"; a speaker note on starting players at Settler since challenge is not the only source of fun. For Narrative players: "aim for personality, maintain memory, fall for traps".
- Slides on what an AI should do with diplomacy: a speaker note answers that the AI should not use the same tricks as humans, such as "fake peace".
- A 2010 caption summary says players often see cheating where there is none and miss it where there is ([Rosetta](https://rosetta.to/u/googletechtalks/playing-to-lose-ai-and-civilization-soren-johnson)) [read: page, secondary].
- The GDC report repeats the arc: strong cheats in Civ I and II, milder ones later, further cuts in BtS ([Game Watch, in Japanese](https://game.watch.impress.co.jp/docs/20080222/ai.htm)) [read: page, translated]. Its "up to 40%" now matches the slide.
- Round one's Humankind complaint: forced style of play at the top ([forum](https://community.amplitude-studios.com/amplitude-studios/humankind/forums/210-humankind/threads/39510-feedback-ai)) [snippet, weak].

### Inferences
Rules for this game:
- Show the whole table on the New Game screen and in-game (Realms or Civilopedia). State plainly: "rivals never see through fog at any level", which is already nearly true (see weakness section).
- Avoid free units during play, instant Wonders and any catch-up that triggers without notice. One-off extras at game start are disclosed and were common in all sources.
- Never discount Wonders and Transcendence stages. Civ IV, Civ V and Unciv all protect wonders on upper levels, and losing the race to a discount would feel worst.
- Avoid scripted anti-human gang-ups. If a rival moves against a player close to Transcendence, show why (a named reason), as open catch-up.
- Show the reason for rival attitudes and war. The `Opinion` and `AgendaReason` functions already give a start.
- Ship an honest, player-friendly Settler equivalent, since Johnson says challenge is not the only fun.

### Gaps
- All of this is designers' judgement from experience. I found no controlled study of player fairness perception in 4X. Round one found Spronck and den Teuling on player models, which is a different question.
- The original magazine layout was not seen; the Gamasutra reprint is the source.

## How AI personality can vary per game cheaply

### Takeaway
Fixed leaders plus a small seeded roll is enough, and costs one integer or nothing in the save. Civ IV, Freeciv, Civ VI and Unciv each show a piece. This game has per-civilisation data already but it hardly affects what rivals build or research.

### Cited Findings
- **Civ IV base plus random, by leader**: `iBaseAttackOddsChange`, `iAttackOddsChangeRand` (8), `iWonderConstructRand` (10 to 40 across samples), `iBasePeaceWeight` plus `iPeaceWeightRand` (3), `iMaxWarRand`, `iLimitedWarRand`, `iBuildUnitProb` (20 to 35), `iRazeCityProb`, and a few flavours such as `FLAVOR_MILITARY=5, FLAVOR_GROWTH=2` for Alexander ([K-Mod CIV4LeaderHeadInfos.xml](https://github.com/karadoc/Civ4-K-Mod/blob/master/Assets/xml/Civilizations/CIV4LeaderHeadInfos.xml)) [read: file, sample of seven leaders, mod copy].
- Peace weight is rolled once at initialisation (CvPlayerAI.cpp line 159). Attack odds change is rolled in `AI_doMilitary` as base + rand + rand (lines 16165-16167), and added to computed combat odds in CvUnitAI.cpp lines 815, 893, 25067 [read: file].
- **One seed, many decisions**: K-Mod stores a 24-bit `m_iStrategyRand` per player, set at game start and saved. `AI_getStrategyRand(shift)` mixes it with era and a golden-ratio multiplier to give a stable pseudo-random value for any question (lines 20401-20421) [read: file, K-Mod addition]. It also staggers expensive work: `(turn + AI_getStrategyRand(0)) % 3 == 0` (line 372).
- Civ IV has a `GAMEOPTION_RANDOM_PERSONALITIES` option ([CvPlayer.cpp lines 157, 367, 1150](https://github.com/karadoc/Civ4-K-Mod/blob/master/CvGameCoreDLL/CvPlayer.cpp)) [read: file, grep only].
- **Freeciv ranges in data**: four traits (Expansionist, Trader, Aggressive, Builder), default 50, with per-nation min and max. If not fixed, `val = fc_rand(max + 1 - min) + min` ([aitraits.c](https://github.com/freeciv/freeciv/blob/main/ai/aitraits.c), [traits.h](https://github.com/freeciv/freeciv/blob/main/common/traits.h)) [read: file]. Used as simple multipliers, for example `want * (0.5 + trait / 50 / 2)` ([daimilitary.c:1784](https://github.com/freeciv/freeciv/blob/main/ai/default/daimilitary.c)) and `founder_want *= trait...` (daidomestic.c:558). `mod` lets a trait drift in play.
- **Unciv scales**: 15 values from 0 to 10, default 5; `scaledFocus` maps to 0..2, `modifierFocus(value, weight)` gives `1 + (scaled - 1) * weight`, with `preferredVictoryType` and policy priorities ([Personality.kt](https://github.com/yairm210/Unciv/blob/master/core/src/com/unciv/models/ruleset/nation/Personality.kt)) [read: file]. These are per leader in JSON. I saw no per-game roll. A typical weight is 0.3 ([ConstructionAutomation.kt line 180](https://github.com/yairm210/Unciv/blob/master/core/src/com/unciv/logic/automation/city/ConstructionAutomation.kt)).
- **Civ VI random agendas**: `RandomAgendas` rows with limits like `AGENDA_DARWINIST GameLimit="3"`, plus `HistoricalAgendas` and incompatibility rows ([Agendas.xml lines 274-325](https://github.com/mrobaczyk/civ6/blob/main/Base/Assets/Gameplay/Data/Agendas.xml)) [read: file, third-party dump]. Round one noted criticism that fixed agendas repeat.
- **Humankind**: archetypes, biases and strengths bought from a limited score ([Amplitude](https://community.amplitude-studios.com/amplitudestudios/humankind/blogs/751-humankind-feature-focus-09-avatar-ai?page=-4)) [read in round one].
- **Warnings**: Shafer called Civ V opponents crazy "in the exact same way" and argued for fewer moving parts ([Game Developer](https://gamedeveloper.com/design/revisiting-the-design-of-civ-5)) [read in round one]. Johnson says Tokugawa's isolation is inefficient but characterful ([Game Watch](https://game.watch.impress.co.jp/docs/20080222/ai.htm)) [read: page].
- **This game [measured here]:** `CivLore.cs` agendas reach decisions only through `AgendaOpinion`, used in `Opinion` (`Diplomacy.cs:37`). `PreferredResearch` (`InfrastructureAI.cs:7`), `PreferredBuilding` (`CityStrategy.cs:104`) and the unit choice in `AI` (`World.cs:638`) have no per-civilisation term. A search of the AI files found `HasTrait` only in rule effects (yields, strength, completed units).

### Inferences
Suggested design (not built):
- **Six values, 0 to 10:** Expansion, Military, Science, Wonders, Boldness, Loyalty. Base from the civilisation's agenda and traits, or leave a table of 22 + natives.
- **Per-game roll without save change:** `new Random(seed ^ (owner * 7919))` at each read or cached in a `[NonSerialized]` array, giving -2..+2 on each value. Deterministic from `seed`, so no save bump, and old games gain personality with no migration. The cost is that old saves change behaviour slightly, which is acceptable for rivals.
- **Uses, weight 0.3 to start:** settler target (Expansion), army cap and attack margin (Military, Boldness), keystone against minor research weights (Science), wonder rank (Wonders), break treaty threshold (Loyalty).
- **Victory type per game:** a rolled lean to Transcendence, Conquest or Concord, the single variation players would actually notice.
- **Show it:** "Rome this game: expansive, cautious" from a few traits. Shows the player what to expect and addresses Shafer's point.
- Do not make individual values large. Civ IV's 8-point attack-odds roll and Freeciv's 50% range of effect show mild is enough.

### Gaps
- I found no evidence measuring how much personality variation players notice or value, or what roll size is best.
- Tuning must be tested through harness and play. Stage 8's pacing could drift with a Science roll.

## Clearest weakness in this game's AI (code read, not run)

### Takeaway
The rival cannot press a war. It attacks without using its exact combat knowledge, never retreats, hoards no force, and proposes peace on a timer. War is the clearest weakness, ahead of the 4-city settler cap and no reaction to anyone's victory. All are cheap to fix, and the deterministic combat code makes the first fix almost free.

### Cited Findings
All [measured here] by reading `Unity/Assets/Scripts`, version in the 0.27 folder. Not run.

1. **Combat without odds or retreat (`World.cs:638-648`).**
   - Spare units take the **nearest** visible enemy unit (`World.cs:646`) or else nearest visible enemy city (`:647`), and go. Nothing checks strength, wounds or support.
   - No code in this loop reads `u.hp`, so there is no retreat or heal decision here.
   - Combat is exact by design: comment at `EmpireSystems.cs:49` says "combat is deterministic", and `Exchange(attacker,defender,out dealt,out taken)` is public at `World.cs:572`.
   - Each unit acts alone. Only one garrison is kept (`:641`). Invasions by sea have grouping (`NavalAI.cs:170`), but land war does not.
   - With no target, soldiers **wander at random** up to 8 tries (`:648`).
   - By contrast: Civ IV uses per-role minimum odds and an early heal, and Unciv has retreat under 50 health and refuses suicidal attacks (see above).
2. **Wars end on a clock (`DiplomacyDeals.cs:62`, `:31`).**
   - At war, any rival offers peace once `turn-warStarted>=6`, whether winning or not.
   - An AI recipient's willingness is `(turn-warStarted)*4 + weariness + (15 if the other side is at least as strong) + value/3 - 20`. At 6 turns this is at least +4, so it accepts (`OfferProblem` may still block in rare cases; not traced).
   - `DiplomacyAI` looks at each nation every 5 turns (`:61`), so **AI against AI wars look capped at around 6 to 10 turns**. That is an inference from code, and needs a harness count. It would make conquest by rivals very unlikely.
3. **Army choice and cap (`World.cs:638`).**
   - Every city trains the highest `MilitaryValue` (Attack+Defence, `World.cs:82`) available, ignoring the Spearman, Horseman, Archer triangle (status doc line 405) and the enemy.
   - Rivals stop training at `own.Count>=14` for the **whole** empire, including workers, settlers and ships, at all map sizes and eras.
4. **Settler cap (`SettlementStrategy.cs:100`).** `cities.Count(a=>a.owner==owner)<4` stops new settlers at 4 cities. Status doc line 26 measures 4 to 6 cities. So rivals are likely to be well below a capable human, though a human benchmark has not been measured. This is probably the largest economic gap and the reason any "even" level will feel easy.
5. **No race awareness.**
   - No rival looks at others' Transcendence or Concord progress: searches for `Transcend` in `InfrastructureAI.cs`, `DiplomacyDeals.cs`, `Diplomacy.cs` and `NavalAI.cs` found nothing. Unciv has the +15 "About to win" modifier.
   - The build order ends `available.OrderBy(Cost)` (`CityStrategy.cs:115`), so projects compete as the most expensive remaining building. `PreferredResearch` favours cheap keystones with no goal.
6. **War only by grudge (`DiplomacyDeals.cs:63`).** War requires `Opinion<-20` and 1.5x strength as seen. A peaceful player may never be attacked by a rival. Whether this is right is a design matter, but war rarely tests the player. Status doc line 397 noted no wars in old test games (round one; may be dated).
7. **Treasury unused and no upgrades.**
   - `Rush` requires `c.owner==0` (`EmpireSystems.cs:33`), so rivals never buy. `Upgrade` (`World.cs:591`) is called from UI and checks only.
   - Civ IV and Civ V give the AI 50% upgrades at every level, showing the shipped view that AI needs help here. Rival gold levels were not measured.
8. **What is good, keep it.**
   - `ManageCityAI` is commented "AI does not receive extra yields, free specialists or hidden terrain knowledge" (`CityStrategy.cs:119`), and targets and `KnownMilitary` (`DiplomacyDeals.cs:29`) use `VisibleTo`. The base is honest and can truthfully be advertised.
   - Pathing checks and settler safety (`SafeSettlerPath`) are careful. Naval AI has real coordination.
   - Pacing tuning is measured: eras around 36/77/124/170, earliest Transcendence 222-250 (CLAUDE.md, Stage 8).

Possible first patch (a suggestion; not applied or compiled). Place before the target choice at `World.cs:646`:
```csharp
// Retreat when hurt, unless already healing in a city.
if(u.hp<=4){var home=NearestOwnCity(u);if(home!=null&&!(u.x==home.x&&u.y==home.y)){Order(u,home.x,home.y);continue;}u.fortified=true;continue;}
// Attack a neighbour only when the exact exchange favours us.
var foe=AdjacentEnemies(u).Select(e=>{int d,t;Exchange(u,e,out d,out t);return new{e,d,t};})
  .Where(x=>x.d>=x.e.hp||(x.d>x.t&&x.t<u.hp)).OrderByDescending(x=>x.d-x.t).FirstOrDefault();
if(foe!=null){Move(u,foe.e.x,foe.e.y);continue;}
```
`NearestOwnCity` and `AdjacentEnemies` are illustrative and would need writing. `Order` then continues to advance on targets, though only with a suitable force. `BestDefender` at `EmpireSystems.cs:46` should be used for the true opposing unit. The threshold is `hp<=4` of 10; tune by harness.

### Inferences
Suggested fix order, each small:
1. Exchange-based attack and retreat rule (above).
2. Peace rule: offer only when losing or tired, with a modifier list like Unciv's, plus a war goal (target city list).
3. Army cap proportional to cities, and defender or counter mix. Rival land-unit limit by level.
4. Settler target by available land, map size and personality; level-dependent. Re-measure pace, since this breaks Stage 8's tuning.
5. Victory race posture: at activation stage 2, rivals move toward the leader's project city with a reason shown.
6. Half-price upgrades and a gold sink. Disclose them in the table.

Fixing 1 and 2 should mostly leave pacing alone if Stage 8's rival games have few wars. That needs measuring.

### Gaps
- Nothing here was run. `Order`'s final attack was only read in part (`ContinueOrder` at `World.cs:543`, `Move` at `:573`), and `OfferProblem` was not traced.
- No measure of war length, rival gold, or how often rivals attack at poor odds. A harness log would settle them cheaply.
- No human benchmark for city count, so "well below a human" is an expectation, not a measurement.

## Proposed difficulty table for Paradigm Realm (SUGGESTION, unverified, needs harness and hand play)

### Takeaway
Six levels with Even exactly matching today's rules, so Stage 8's pacing work remains valid. Rival costs follow Civ IV's non-linear shape (150 to 65). Wonders and Transcendence are never discounted. The lower levels also change behaviour, in the style of Freeciv and Old World. All figures below are my starting proposal, not measured, and none comes from a source table except where a comparison is named.

### Cited Findings
Comparison anchors, all from the tables above:
- Top cost: Civ IV 60, Civ V and Unciv 50. Lowest: Civ IV 160, Civ V and Unciv 175 for units.
- Research: Civ VI +8 per level; Freeciv Novice cost 250; human 60-130 in Civ IV, 0.9-1 in Unciv.
- Mistakes: Freeciv 400/1000 and 300/1000. Attitude: Civ IV +2 to -1. War factor: Civ IV 25, 50, 75, 100.
- Implementation hooks [measured here]: `CostFor` (`Civilisations.cs:156`), `ResearchCost(t,owner)` (`World.cs:421`), `FoodNeeded` (`CityStrategy.cs:60`), `Opinion` (`Diplomacy.cs:37`), settler cap (`SettlementStrategy.cs:100`), unit cap and unit choice (`World.cs:638`), war test (`DiplomacyDeals.cs:63`), start units (`World.cs:247`).

**Suggested table:**

| Setting | Novice | Easy | **Even** (today's rules) | Hard | Very Hard | Paragon |
|---|---|---|---|---|---|---|
| Rival unit and building cost % | 150 | 125 | **100** | 90 | 80 | 65 |
| Rival Wonder and Transcendence stage cost % | 150 | 125 | **100** | 100 | 100 | 100 |
| Rival food to grow % | 140 | 120 | **100** | 95 | 90 | 80 |
| Rival research cost % | 130 | 115 | **100** | 95 | 90 | 85 |
| Rival unit upkeep % | 100 | 100 | **100** | 90 | 80 | 65 |
| Rival extras at start | none | none | **none** | 1 soldier | 1 soldier, 1 Worker | 2 soldiers, 1 Worker, 1 Settler, 1 free first-era technology |
| Rival target cities (settler cap) | 3 | 3 | **4** | 6 | 8 | 10, all by land available |
| Rival total unit cap | 8 | 10 | **14** | 18 | 24 | 30 |
| Chance a rival build or war choice is mis-decided | 30% | 15% | **0** | 0 | 0 | 0 |
| Strength needed to declare war, as seen (default 1.5) | 3.0 | 2.0 | **1.5** | 1.5 | 1.25 | 1.25 |
| Rival attitude to you (added to Opinion) | +15 | +5 | **0** | 0 | -5 | -5 |
| Rivals see through fog | never | never | **never** | never | never | never |
| Player research cost % | 90 | 95 | **100** | 100 | 100 | 100 |
| Player unit upkeep % | 75 | 90 | **100** | 100 | 100 | 100 |
| Per-era extra discount | none | none | **none** | none | none | none first; add -1 or -2 per era only if harness says needed |

Also, independent of level once built: better tactics (exchange rule, retreat, army mix) apply from Even up. Below Even the mistakes row covers them. This means **Even stays pacing-neutral on economy but is a stronger opponent in war**. A one-off free unit at Paragon follows Civ IV, Civ V and Unciv.

**Effective power (calculated):** cost 65 is 1.54x output, research 85 is 1.18x speed, and food 80 is 1.25x growth, a little under Civ IV's 1.67x and well under Civ V's 2x. Extra cities add on top, so the combined effect may be greater than the table reads. Raise only after play shows need.

**Who gets this:** home rivals only. Exempt native and sealed peoples (`IsNativeNation`), since `KeepPace` follows `HomeTechPace` (`Realms.cs:140`), so they will track the level through it. Keep them out of rule hooks to reduce risk. Multiplayer is not relevant yet.

**Small code plan:**
- Add `static class Handicap` with arrays by level (data, not branches). A single getter: `H.Cost(c.owner)`, which returns 100 for the human.
- Integer maths in `CostFor`: `cost=cost*H.UnitBuildingPct(owner)/100` after existing edits, for non-wonder non-project; `H.WonderPct` for those.
- Show the table: New Game button tooltip and in-game panel reads the same arrays, so the text can never drift from the rules.
- **Save handling:** existing saves hold `difficulty=1`, and `World.New` defaults to 1 in many checks. Use levels 0 to 5 with Even = 2. Save version 26: `if(version==25){difficulty=2;version=26;}`. Change the default parameter to 2, `RealmGame.cs:72` to `setupDifficulty`, and `Valid()` (`World.cs:687`, which tests `version!=25`) to 26 and a range check. Existing games then play exactly as now until they choose otherwise.
- Allow changing level during play (Game Accessibility Guidelines item, round one). It is one int, with no migration, because the level is read at use. Record lowest-level-used on the victory screen so a win stays honest.
- Add `Tests/` harness per working rules, including a table-integrity check.

**Suggested harness acceptance bands (guesses to be tested, Stage 8 method, nine games):**

| Level | Earliest rival Transcendence (turn) |
|---|---|
| Novice | none by 300, or later than 300 |
| Easy | after 270 |
| Even | 222-250, unchanged |
| Hard | 205-240 |
| Very Hard | 190-225 |
| Paragon | 175-210; below that, research too generous |

Also report per level: average rival cities at 100/200, rival wars and their lengths, and average rival score against the Even games. Then a per-nation override test: one Even rival against one Paragon rival in the same game should clearly separate, and new-tactics against old-tactics rivals should give a win-rate number (Freeciv's experimental-against-hard practice).

### Inferences
- Default level: Even for now, but with the tutorial suggesting Easy on a first game. Bear in mind rival strength is unknown against a real player: with a 4-city cap, Even may well be easy for a genre veteran, and the first thing to learn from hand-play is whether the top level is hard enough.
- Keep the first build to three rows that matter (costs, research, settler cap) and the fog-free claim, then add the rest when each is shown to matter. The full table is a target, not a day-one requirement.
- Level names here are placeholders. The owner may want names in the game's own voice, such as Regent or Sovereign.

### Gaps
- None of these figures has been simulated. Stage 8's rival-only games say nothing about strength against a person. Hand play remains necessary, and CLAUDE.md records interactive play as largely unverified.
- Interactions are unknown: settler target and era pace, mid-game natives tracking, turn time with more rival cities and units (`Measured turn time` figures use today's caps).
- Whether players want a change-level option, and how they'd read a "lowest level used" badge, is not sourced beyond the accessibility guidelines.
