# Paradigm Realms Live — architecture assessment and technical specification

Version 1, 9 Nov 2026. This is for whoever implements it (Claude or Codex). **Nothing in it is built yet.**

**Scope.** This applies to **Paradigm Realms Live** only (`Desktop/Paradigm Realm Work File/Paradigm Realms Live`, GitHub branch `paradigm-realms-live`). The original Paradigm Realm stays as it is on `main`.

**Sources:**
- The owner's direction paper "Paradigm Realm – Grand Strategy Architecture" (9 Nov 2026): hybrid real time, moderate simulation, eight systems, seven stages.
- The owner's answers of 9 Nov:
  - Population replaces city size completely.
  - Captured peoples keep their traits and beliefs.
  - No war exhaustion.
  - An optional timer, now superseded by real time with pause, plus a "next month" command.

---

## 1. Corrections to the direction paper

The paper describes an older build. Where it differs from the code, the code wins:

| Paper says | The code now (Live 0.27 copy, save version 39) |
|---|---|
| "Alpha 0.7", 124 technologies in 14 eras | **99 researched technologies in 6 ages** (36 keystones + 63 minors; `TechnologyCatalog.Active`). The 124-tree survives only in `LegacyTechnologyCatalog.cs` to load very old saves. |
| Earth-based, other dimensions later | Six parallel worlds with sealed gateways already exist (`Realms.cs`, 55 sealed peoples). **Proposal:** new Live games default to *Earth only* (a New Game switch, default off). The worlds stay in code for the planned expansion, and older saves keep them. |
| Diplomacy, trade, governments | Present and deeper than the paper assumes: opinion reasons, influence, alarm and coalitions, deals, postures (`DiplomacyDepth.cs`); trade routes and caravans; governments, edicts, revolutions; governors with traits and policies (`Governors.cs`); a living world of rulers, stress, grudges and rebellions (`LivingWorld.cs`). |
| Religion | Full faith system: founding, beliefs, spread, schisms, events, a faith tree (`FaithWorld.cs`, `Doctrines.cs`). |

## 2. Assessment of the current code

**Engine (pure C#, no Unity types):**
- `World` is one `partial class` spread over about 60 files. It holds all state (`tiles`, `units`, `cities`, `nations`, `characters`, `animals`, …) and all rules.
- It is serialised whole with `JsonUtility` (compact JSON). It carries `version` (39) and an `UpgradeSave()` chain, and `Valid()` checks the saved state.

**One turn** is the coroutine-friendly `TurnSteps()` (`World.cs`), which yields between slices. The order is:
1. Diplomatic payments.
2. For each rival: `ProcessNation(owner)`, then `AI(owner)`, unless it is a resting sealed people.
3. `ProcessNation(0)` for the player. It runs, in order:
   - budget
   - feeding (`FeedUnits`)
   - city food, growth and production (`ProcessCityFood`, `Produce`)
   - research
   - healing and refilling moves
   - `LivingTurn`
   - jobs
   - empire orders (exploring, auto develop, fleeing)
   - priests, ages and realms
4. `turn++`, `AdvanceDiplomacy`, the religious-victory check.
5. `AnimalsTurn`.
6. `DiplomacyAI`.
7. The player's standing movement orders (`ContinueOrder`), then `CheckVictory`.

**Interface:**
- `RealmGame` is a single `MonoBehaviour` (IMGUI, `OnGUI`) split over about 40 files.
- `RunTurn()` (`GuidePromptsUI.cs`) runs `TurnSteps()` over several frames, locking the interface while it runs (`turnRunning`).
- Saving happens on a worker thread (`AtomicSave`).

**Randomness:** seeded per purpose, for example `new Random(seed*31+turn*101+owner)`. Given the same inputs and the same order of calls, a turn is reproducible. This is the basis for deterministic ticks.

**Checks:**
- `Tests/EngineChecks.csproj` runs the engine checks in .NET: Foundation, Content, Empire, Strategy and Diplomacy suites, including 50-turn campaigns. Windows Smart App Control sometimes blocks the compiled runner.
- `RealmTools.Checks` runs the same checks inside Unity in batch mode; this is the reliable gate.
- `AutoShot.cs` drives the built game with probes (`-autoShot`, `-frameProbe`, `-routeProbe`, …).

**Performance today:** a Huge map takes about 0.3-0.5 s per turn in the .NET harness. The real game, under Mono, is slower.

**Risks in moving to real time:**
1. **Turn-shaped rules.** Many timers and constants count turns: occupation 4 turns, healing per turn, research turns, governor settling, gate opening, `HuntersFrom`, era pacing. If one month equals one old turn, they all keep their meaning. That is the main reason for the mapping in §4.
2. **Movement points per turn** (`u.moves`, `MaxMoves`). Continuous movement needs moves spread over days, without changing how far a unit can go in a month.
3. **AI cost.** `AI(owner)` and route searches are designed to run once a turn. They must stay monthly, or be spread out, never every frame.
4. **The monolithic `World`.** Systems are not separate classes yet. Splitting everything at once would be a wholesale rewrite, which the owner rules out. Use a **strangler approach**: thin system classes that call the existing code first, with the logic moved inside them later, one system at a time, behind checks.

## 3. Target structure (incremental)

```
GameState (World, unchanged, still the one saved object)
 ├─ SimulationClock        new  – calendar, speed, tick scheduling, deterministic tick order
 ├─ EventBus               new  – typed world events, notification feed, auto-pause rules (replaces ad-hoc Note())
 ├─ Systems (ISimSystem: OnDay / OnWeek / OnMonth / OnYear), first as facades over existing code
 │   ├─ PopulationSystem   Demography.cs, CityCare.cs, ProcessCityFood
 │   ├─ EconomySystem      BudgetDetail, Produce, trade routes, resources
 │   ├─ PoliticalSystem    governments, edicts, LivingWorld, Governors
 │   ├─ DiplomacySystem    Diplomacy*.cs, DiplomacyAI, AdvanceDiplomacy
 │   ├─ MilitarySystem     movement, Siege.cs, RivalWar.cs, Naval
 │   ├─ ResearchSystem     research progress, TechnologyCatalog, ages
 │   ├─ ReligionSystem     FaithWorld.cs, Religion.cs, Doctrines.cs
 │   └─ CivilisationAI     AI(owner), EmpireAI, NavalAI, settlement planner
 └─ Presentation (RealmGame IMGUI): map, top bar with date and speed, notification panel, management screens
```

Rules:
- **Engine has no Unity types.** Systems run inside the .NET harness.
- **One clock.** No system keeps its own timer. The order of systems within a tick is fixed (the list above), and so is the order of nations within a system (0 first, then by index).
- **Events, not calls:** systems publish events (`FoodShortage`, `WarDeclared`, `CityGrew`…), and others subscribe. Begin with notifications and auto-pause only. Move cross-system calls to events where it simplifies.
- **Data-driven tables** stay as they are now (catalogue arrays). New content goes in the same style.

## 4. Stage 1 — Simulation clock (first target)

### 4.1 Calendar
- `World.day` (int, saved): days since the start. 30 days a month, 12 months a year, four seasons of three months (Spring = months 3-5, and so on). A week is a quarter-month: days 1, 8, 16 and 23 of each month.
- The year comes from the existing turn-to-year table. **One month = one old turn**, so `turn` keeps advancing once a month and every turn-based rule and balance figure stays valid in Stage 1.
- Displayed as "Spring, 4000 BCE, day 12". Later stages may compress the early millennia as the old year table does.

### 4.2 Ticks

| Tick | When | Work in Stage 1 |
|---|---|---|
| Day | every day | Movement: units on orders, exploring, auto develop and animals advance (§4.3); finished workers' jobs; daily notices |
| Week | days 1, 8, 16, 23 | Reserved (Stage 3 economy) |
| Month | day 30 → 1 | The existing `TurnSteps()` monthly work, minus movement (now daily), sliced over frames as today |
| Year | month 12 → 1 | Reserved (yearly reports, Stage 7) |

The monthly tick runs the existing turn pipeline. Ticks never run inside `OnGUI`. `RealmGame.Update` advances the clock, and a long monthly tick pauses the clock until its slices finish, as `RunTurn` does now.

### 4.3 Movement over days
- Each unit gets its usual moves at the start of a month (`MaxMoves`, unchanged).
- A unit with an order spends them across the month: on each day it takes the next step of its route once it has accumulated enough of its monthly allowance. Store `Unit.moveCredit` (saved), which gains `MaxMoves/30` a day, as a fixed-point integer. **The total distance in a month is unchanged.**
- The rivals' `AI(owner)` still chooses its orders monthly, and the orders are walked daily.
- Combat happens when a step enters an enemy tile, on that day. Animals step on their own schedule (`AnimalSteps` spread over the month).
- The movement animation (`MotionUI.cs`) already draws steps from `World.moveLog`, so continuous movement needs no new drawing.

### 4.4 Speed and control
- Speeds: **Paused**, **Slow** (1 day / 0.6 s), **Normal** (1 day / 0.25 s) and **Fast** (1 day / 0.08 s). At most 3 day-ticks per frame, and never more ticks than real time allows.
- Space toggles pause, keys 1, 2 and 3 set the speed, and **"Next month"** runs to the next month boundary and pauses (the deliberate style the paper keeps).
- The game **starts paused**. Opening a city, diplomacy or any management screen does **not** pause by default; this is a setting.
- **Auto-pause** (each a setting, on by default): war declared on you, one of your cities attacked or lost, a decision event (faith events, first contact), a famine starts, research finished.
- The End Turn button becomes the speed control with a date plate. "Next month" sits in its place.

### 4.5 Events and the notification panel
- `WorldEvent {day, kind, owner, cityId, x, y, text, severity, pause}` goes into `World.events` (saved, last 200).
- The panel on the right lists them newest first, filtered to what you can know (`VisibleTo`/met). Clicking an event centres the map on it.
- `Note()` keeps working: it posts a `Notice` event, so nothing that already exists is lost.

### 4.6 Saving
- Save version 40 adds `day`, `Unit.moveCredit` and `events`. The migration sets `day=(turn-1)*30`, `moveCredit=0` and `events=[]`.
- A save may be made at any day. Loading resumes **paused** on that day.

### 4.7 Determinism
- Every random draw in a tick comes from a `Random` seeded by `(seed, day, system, owner)`. The engine never reads wall-clock time.
- A state hash is written for checks: a stable hash of `tiles`, `units`, `cities`, `nations`, `day` and gold.

### 4.8 Files

| New | Purpose |
|---|---|
| `SimulationClock.cs` | `World.day`, the calendar, `TickDay()`, `TickWeek()`, `TickMonth()` (wraps `TurnSteps`), `DateText()` |
| `SimEvents.cs` | `WorldEvent`, `PostEvent`, auto-pause flags, the `Note()` bridge |
| `ClockUI.cs` | speed bar, date plate, Next month, keyboard, auto-pause handling in `Update` |
| `EventPanelUI.cs` | the notification panel |

| Changed | Change |
|---|---|
| `World.cs` | `EndTurn` becomes `AdvanceMonth`; movement leaves the monthly pipeline; save version 40 |
| `EmpireSystems.cs`, `Workers.cs`, `Wildlife.cs` | exploring, auto develop and animals step from the daily tick |
| `GuidePromptsUI.cs` (`RunTurn`), `HudTheme.cs` (End Turn) | clock-driven instead of button-driven |

### 4.9 Acceptance tests for Stage 1

Engine checks (new `ClockChecks` in `ContentChecks.cs` or its own file):
1. 30 day-ticks produce exactly one month-tick and four week-ticks, and `turn` rises by 1.
2. While paused, advancing real time produces no ticks.
3. Two worlds from the same seed, run for 360 days with the same orders, have the same state hash.
4. Save on day 17, load, run to day 60: the same hash as an unbroken run.
5. A unit ordered 6 tiles away arrives over several days, and the distance covered in a month equals the old per-turn distance.
6. A headless 100-year run (1,200 months, Small map, 4 civilisations) finishes with no exception, `Valid()` true, at least 2 rivals with cities, and an average under 2 ms per day-tick in the .NET harness.
7. A version 39 save loads, upgrades to version 40, and has `day=(turn-1)*30`.

In the built game (`-autoShot` probe `-clockProbe`):
1. Run Normal speed for 2 minutes. The date advances, units walk, and there are no exceptions.
2. Pause holds still.
3. Next month stops on day 1.

Hand-play checklist for the owner:
- The existing map loads.
- The clock runs, and pause and speed work.
- Population and food change over time.
- At least 2 rivals grow on their own.
- Events appear in the panel.
- Saving and loading keep the date.
- Cities and research still work.

## 5. Later stages (outline; each gets its own detailed spec before starting)

| Stage | Built on | Main new data | Key checks |
|---|---|---|---|
| 2 Population and cities | `Demography.cs`, `CityCare.cs` | `City.groups[]`: {class (farmer, labourer, artisan, merchant, soldier, clergy, noble), **people (civ id)**, size, employed, wealth, happiness, ethic}. **Groups replace `pop`** (owner: replace completely); `pop` becomes a derived total for old code paths during the change. Housing, jobs from buildings and tiles, migration between cities | 100-year run: no runaway growth; famine shrinks cities; migration flows towards jobs |
| 3 Economy | `BudgetDetail`, trade routes, resources | Stocks per city (food, goods, iron…), weekly market price per region from supply and demand, merchants moving goods along roads and sea | No unexplained bankruptcy over 100 years; shortages raise prices and pull imports |
| 4 Politics and religion | governments, `LivingWorld`, `Governors`, faith | Factions grown from groups' ethics and faiths, approval, monthly demands; unrest leads to rebellion and independence | Rebellions happen, but not endlessly; stability recovers when demands are met |
| 5 Diplomacy and AI | `DiplomacyDepth`, postures, flavours | The paper's AI decision cycle (assess economy, stability, neighbours, threats; set objectives; act; review), personality profile per civilisation, espionage | Rivals trade, ally and fight among themselves with the player idle; none permanently passive |
| 6 Military | `Siege.cs`, stacks, `RivalWar` | Armies (stacks with a general), recruitment and upkeep, supply from owned or road-linked land, sieges over days, automatic battles. **No war exhaustion** (owner) | Wars start and end; no endless war over 100 years (peace by AI judgement) |
| 7 Integration | all | Reports, history log, balancing, long campaign tests (100 and 500 years), performance over time | Section 9 of the paper |

Captured peoples keep their traits and beliefs (owner): a group carries its `people` and `ethic` through capture, migration and conversion. Assimilation is a slow, separate process, not automatic.

## 6. Working rules for the implementer
- Work in the Live folder only. After each stage: checks, a Windows build to `C:\Users\Matth\Games\Paradigm Realms Live` (desktop icon "Paradigm Realms Live", zip `Paradigm Realms Live.zip`), then commit and push to `paradigm-realms-live`.
- Bump the save version and add a migration for every saved change. Never break older Live saves.
- Keep `Resources/Tutor` out of GitHub.
- Report honestly what was checked and what was not hand-played.

## 7. Open question for the owner
- The late-game crisis to build first: the hum beneath the Hive Expanse, the plague of the Marshes, or the fire of the Infernal Depths. This is not needed until Stage 7, and becomes moot if new games are Earth only.
