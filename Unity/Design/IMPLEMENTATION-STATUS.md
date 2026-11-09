# Current release: Advanced diplomacy — Stage 3

## Paradigm Realms Live, Stage 1: simulation clock (save version 40)

Built from `Design/LIVE-ARCHITECTURE-SPEC.md` §4. The files are `SimulationClock.cs` and `ClockUI.cs`.

**How this was checked:**
- `ContentChecks.ClockChecks` pass in .NET:
  - 30 days make one month and four weeks, and the monthly work advances the old turn counter once.
  - An order given on day 1 waits; the unit is part of the way after half a month, and at the month's end it stands exactly where an all-at-once turn took it.
  - Two worlds from one seed stay identical over 90 days.
  - A version 39 save starts on day 1 of its month.
  - A 300-month run keeps its rivals growing. Measured about 1.3 ms a simulated day on average, monthly work included. That run ended at about month 237, when a civilisation won.
- Unity checks, build and screenshots are reported in the session.
- **Not hand-played.**

**What it does:**
- **The world runs on a clock:**
  - 30 days a month. Each month is one of the old turns, so every rule and balance figure keeps its meaning, and the monthly work is the old turn pipeline.
  - The date is shown as season, day, month and year.
- **Controls:** the End Turn button is replaced by a date plate with a month progress bar.
  - Pause, Slow (0.6 s a day), Normal (0.25 s) and Fast (0.08 s).
  - "Month" runs to the next month and pauses.
  - Keys: Space pauses or resumes, 1-3 set the speed, Enter runs to the next month. Space no longer skips a unit; use its Skip button.
  - The game starts paused.
  - It pauses itself when a leader calls or makes first contact, and the clock waits while a prompt, decision or modal screen is open.
- **Your units walk across the days:**
  - Each unit earns its month's movement day by day and spends it as it goes: orders, exploring, road routes and auto develop.
  - An order given mid-month walks as far as the movement earned so far.
  - The movement animation shows each step.
- **Rivals** still plan and move at the month (daily armies come in Stage 6). Production, growth, research and diplomacy still happen monthly.
- **Not yet:** the event panel with date stamps (the existing notices remain), the weekly economy and yearly reports.

> **Planned design, not implemented:** see [CORE-DESIGN-16BIT.md](CORE-DESIGN-16BIT.md) for the agreed 16-bit core design and draft balance formulas, and [IMPLEMENTATION-PLAN-16BIT.md](IMPLEMENTATION-PLAN-16BIT.md) for the staged plan (including map sizes from Tiny to Massive), and [WORLD-AND-LOOK-DESIGN.md](WORLD-AND-LOOK-DESIGN.md) for the planned spherical world, realistic biomes and new terrain art.

## Owner request 9 Nov: animated movement (no save change; `MotionUI.cs`)

- On the diagonal and staggered map, units glide from tile to tile along the steps they really took, with a small hop for each step. The steps come from `World.moveLog`, which is filled by `Move` and not saved.
- Animals glide to their new tile.
- Only what you can see is animated. Long walks play faster (6 steps a second, quicker beyond three steps), and a glide across the wrap seam jumps instead.
- The rules are unchanged: a unit is already on its new tile and is only drawn on the way there.
- The classic square view is not animated.
- Not hand-played.

## Owner requests 9 Nov: sieges, stacks and faith buildings (no save change; `Siege.cs`)

- **Faith buildings:** Shrine, Temple, Monastery and Cathedral are paid from the civilisation's faith total, not the city's own store.
- **Sieges:** a defender in its own city counts three times its strength (3:1). An attacker on a hill faces 2:1, unless the city is also on a hill, when it stays 3:1 (`CityRatio`). This is applied in `Exchange`, so the preview, the rivals' judgement and the battle all agree.
- **Stacks attack together:**
  - A stack fights at its soldiers' combined strength, up to 6, and the damage it takes is shared evenly (`AttackGroup`, `GroupAttack`, `ShareDamage`).
  - Every member spends its move.
  - Your stacks are the units you joined. Rivals' soldiers on one tile attack together, because rivals do not form stacks yet.
  - Ranged units fire alone.
- **Attacking alone (Skirmish):** the unit keeps half its remaining movement after the attack, so it can pull back or press on, and the rest of its stack can still act. A flat +1 attack was tried first, but it upset designed matchups such as Spearman against Horseman.
- **Preview:** the battle preview names the stack, the shared damage and the city's multiplier (`SiegeNote`).
- **Checks:** `ContentChecks.SiegeChecks`. Not hand-played: the 3:1 rule makes walled cities very hard to take, so balance needs playing.

## Owner requests 9 Nov: flag designer and auto develop (save version 39)

- **Flag designer** is now a "Flag and identity" sub-tab of the Empire screen; the separate Identity entry is gone from the left menu.
  - Flags are drawn at 96 x 64.
  - 25 layouts, shown as thumbnails in your own colours (11 new: Pale, Bend, Hoist triangle, Pall, Lozenge, Disc, Sun rays, Chequy, Four stripes, Per saltire, Bordered cross).
  - A third field colour, and a palette of 32 colours.
  - Emblems: 15 simple shapes or 32 painted heraldic charges, tinted in the emblem colour (SpriteCook, `Sprites/charge-01..32`):
    - creatures: lion, eagle, dragon, horse, wolf, bear, stag, bull, serpent, owl, phoenix, fish
    - things: ship, castle, crossed swords, axe, bow, hammer, book, key, scroll, skull, hand, eye
    - plants and nature: rose, lotus, oak, wheat, grapes, lightning, flame, comet
  - Emblem size (small, medium, large), position (centre, hoist, canton, fly) and outline.
  - Banner and plate previews, Random and Undo.
  - Save version 39 adds `flagC`, `flagSize`, `flagPos`, `flagCharge` and `flagOutline`. Older flags keep their look: the third field takes the first colour.
- **Auto develop:** your workers now take any remaining job in your territory, roads included, before going idle. Before, they stopped building roads that would not pay for themselves.
- Not hand-played.

## Owner requests 9 Nov, later (no save change)

- **Sharper minimap** (`MiniMapArt.cs`): 3-8 pixels per tile, with mottled terrain, shallows along coasts, the owner's tint and border edge, river threads, framed city dots and gold gateway marks. It is drawn smoothly with mipmaps, and the panel is wider (234 px).
- **Farmsteads around growing cities** are drawn at half size (32 px), so their houses match the city's own buildings.
- **Road upkeep:**
  - It existed (1 gold per 3 road tiles inside your borders) but was easy to miss. It is now 1 gold per 2 road tiles (`RoadTilesPerGold`).
  - The top bar's gold tooltip lists income, units, buildings and roads, with the number of road tiles. The road planner already showed the added upkeep.
- **Checked:** Unity compiles and its engine checks pass. The .NET runner was blocked by Windows Smart App Control this time. **Not hand-played.**

## Owner requests 9 Nov (no save version change; new games get a sixth world)

**How this was checked:**
- .NET engine checks pass, including new `ContentChecks.NinthNovemberChecks`:
  - Home rivers have no loops.
  - New games have the Hive Expanse and its insect peoples.
  - A Granary waits without gold and completes with it.
  - An animal does not attack a unit inside borders.
  - Sloths outnumber mammoths and rhinoceroses.
- Older checks were updated for six worlds: 84 civilisations, 12 native peoples by default, 55 in a full game, and one gateway per world.
- The art processing was compared side by side, before and after, on the supplied pictures.
- Unity checks, build and screenshots are reported in the session.
- **Not hand-played.**

**What changed:**
- **No straight edges in nature:**
  - Open scenery (groves, savanna, bog) is cut out of its ground. Forests, jungle, rocks, mountains, cities and farmsteads keep their ground, faded at the edge with an irregular border (`SceneryArt`).
  - Thin leftover rim lines are removed, and dry steppe shows no picture.
  - Grove trees are drawn as a cluster of three.
  - The apples are really gone now: dark reds and highlights are caught too.
- **Rivers on the home world** join only along their flow, forming branching trees with no mesh or loops (`RiverLinks`). Parallel worlds keep the old look.
- **New art (SpriteCook):**
  - Ruins in five architectural styles, chosen by land: classical columns, Egyptian sandstone, a Mesoamerican pyramid, a stone circle and an eastern temple (`Sprites/ruin-0..4`, `RuinStyle`).
  - Farm fields at the map's angle (`field-0..2`), and a mine and a fort (`improve-mine`, `improve-fort`).
  - Fifteen resource miniatures placed on the tile with no badge (`res-01..15`).
- **Fitting tiles:** cities are drawn 54 px with their lower tip on their tile's, so they no longer spill onto the next tile. Farmsteads and ports are drawn at 82%, and gateways at tile width.
- **Rules:**
  - Workers on a city tile may auto develop.
  - Clear woodland appears only where there are trees.
  - Mammoths and woolly rhinoceroses are each one in six grazers.
  - Buildings cost gold: a quarter of their shield cost, paid on completion. Without it the finished work waits (`BuildingGold`).
  - Wonders were already unique in the world: the first civilisation to finish one gets it, and the others get half their production back as gold.
  - Animals already could not attack inside any civilisation's borders; a check now covers it.
- **The Hive Expanse:** a sixth world in new games (`RealmsHere`; older saves keep five). It has tall grass, savanna, jungle and mounds, a key technology (Chemistry), its own gateway, and five insect peoples with full lore and painted leaders (`SealedPeoplesHiveExpanse.cs`, catalogue 79-83, `leader-79..83`):
  - the Formic Concord (ants)
  - the Apiary of the Golden Comb (bees)
  - the Scarab Wardens (beetles)
  - the Thorn Mantids (mantises)
  - the Lantern Moths (moths)
- **Diplomacy streamlined** (after Crusader Kings III, Bannerlord, Civilization VI and Galactic Civilizations IV):
  - Treaties on one line, and the three reasons that weigh most in the leader's mood.
  - One-click proposals that show whether the leader would accept, plus a gift.
  - The full offer editor folded under "Custom deal", and every other action under More.

## Owner choice 8 Nov: governors with a purpose (save version 38; `Governors.cs`, `GovernorsUI.cs`)

The owner picked ideas 1, 2 and 4 from research on other games: Old World, Civilization VI and Imperator: Rome.

**How this was checked:**
- `ContentChecks.GovernorChecks` pass:
  - A new governor has no effect while settling in.
  - Traits change yields once the governor is settled.
  - A disloyal governor refuses a new policy.
  - Tax hard gives +25% gold and one citizen fewer content, and the mood reasons still add up.
  - A corrupt governor skims half of each gain.
  - Fortify gives defenders in the city +1.
  - Swapped governors move cities and settle in again.
  - Replacing a governor costs 25 gold.
  - Version 37 saves upgrade with governors settled.
- **Not hand-played; the balance of the percentages is untested.**

**What changed:**
- **Traits change yields:** each of the ten traits raises or lowers food, shields, science or gold by 5-15% (`GovernorCatalog.TraitPercent`; for example Ambitious +15% shields and -5% gold, Stoic +10% food).
- **Settling in:** a new governor, or one moved to another city, has no effect for 5 turns (3 if Loyal). Governors already in office in older saves count as settled.
- **Policy:** each governor runs one of six policies:
  - Steady: no change.
  - Tax hard: +25% gold, -1 content.
  - Pacify: +1 content, -15% shields.
  - Grow: +15% food, -10% gold.
  - Fortify: defenders +1 and +10% shields, -10% science.
  - Assimilate: an occupation ends twice as fast with +1 content while occupied, -10% science.
- **Loyalty and corruption now bite:** a governor below 35 loyalty will not change policy, and one at 30% corruption or more keeps that share of every gain. A governor in a breakdown gives half.
- **Who chooses:** rival cities and auto-managed cities choose their policy by need (`ChoosePolicy`).
- **City screen:** click the governor line to open the governor panel. It shows traits and their effects, settling-in turns, the effect now and the policy buttons. You can swap with another city's governor (both settle in again) or replace the governor for 25 gold (your other governors lose 2 loyalty).

## Owner requests 8 Nov, evening (no save version change; `Unit.recover` and `City.autoManage` load as off from older saves)

**How this was checked:**
- .NET engine checks pass, including new `ContentChecks.EveningRequestChecks`:
  - Mood reasons add up exactly to a city's content places.
  - Growth messages name the Aqueduct only once its technology is known.
  - A worker inside borders does not run from animals, and a worker in its own city is recognised.
  - A unit on a route keeps its order after an animal attack, and an attacked explorer keeps exploring and sets out to recover.
  - Two explorers starting on one tile claim different frontiers (9 tiles apart in the test game).
  - Auto-manage trains only units it can afford.
  - Mysticism alone opens only the Mesopotamian and Egyptian faiths.
  - Ground stripping keeps the object and turns fruit into leaves.
- The ground stripping was also run on every supplied scenery and city picture and compared side by side before and after (`SceneryArt`).
- The old Strategy check "growth blocker names remedy" now checks both cases: before the technology, and after it.
- Unity checks and the build are reported in the session.
- **Not hand-played.**

**What changed:**
- **Seamless map:** supplied tree, forest, jungle, city, hamlet and terrain pictures lose their painted ground diamond when loaded (`SceneryArt.StripGround`). Mountains, hills, volcanoes and glaciers keep their ground and only lose the hard rim (`FeatherRim`). The files are unchanged.
- **Groves:** grassland grove trees are drawn at 55% so they match the forest trees. The apples on one grove picture become leaves, so they no longer look like a food resource.
- **Wild animals** have painted sprites (SpriteCook, `Sprites/animal-0..6` in `AnimalCatalog` order). The drawn ones are the fallback.
- **Animal health:** a health bar shows over an animal while it is hurt or one of your units is within two tiles. A hunter's bar has a red edge.
- **Explorers spread out:** each explorer claims the frontier tile it heads for. Targets are scored by unknown land uncovered, distance, and nearness to another explorer's claim or position (`ExploreTarget`, `ExploreClaim`).
- **Attacked explorers:** an explorer attacked by an animal keeps exploring. It moves away until no hunter is within 4 tiles, rests until healed, then explores on (`Unit.recover`, `Recover`).
- **Routes survive attacks:** a unit on a route keeps going after an animal attack, unless the player orders otherwise; the warning says so.
- **Workers:**
  - They ignore animals anywhere inside any civilisation's borders (`SafeFromBeasts`).
  - On a city tile their jobs, Skip, auto develop, road and grouping are greyed out. Cancel, Fortify/heal, Sentry, Move and Disband stay. Sentry is now offered to every civilian.
- **Move order and panning:** while the right button holds a destination, holding the middle button as well pans the map.
- **Cities:**
  - A "Why?" button beside Content/Unhappy opens every reason with its effect, and what can be done now (`MoodReasons`, `MoodRemedies`).
  - Arrows step to the previous or next city. Back to map returns to the city first opened.
  - An Auto-manage switch replaces manual focus (`CityCare.cs`). The city sets its own focus, keeps order with entertainers, and chooses buildings whose upkeep it can pay. It trains a Worker, a Settler or one defender only when gold per turn, its own food and the empire's rations stay at zero or above (`UnitAffordable`).
  - A city at its population limit names the Aqueduct or Sewer only once that technology is known; before that it explains why it cannot grow.
- **Units screen:** the Military entry is now "Units". It lists idle units (waiting for orders) first, then active ones, with place, health, moves and activity.
- **Religion:**
  - Faiths unlock in historical order: at Mysticism only Mesopotamia and Egypt; then Canaan (Currency), Hinduism and Judaism (Code of Laws), Zoroastrianism and Celtic druidism (Mathematics), Greek and Roman gods (Sailing), the Axial Age teachings (Philosophy), Christianity, Mithraism and Manichaeism (Engineering), Norse, Tengri and Shinto (Feudalism), Islam (Education), Aztec and Inca (Banking), Sikhism (Printing Press) and the Baha'i Faith (Industry).
  - The founding list shows only faiths your people know of, and names the next one.
  - The Religion screen has four tabs: Overview, Faith Tree, Beliefs and More. The Overview shows one plain next step (`FaithNextStep`), the faith tree's progress and three actions. Institutions, policy, missions, diplomacy and history are folded sections under More.
- **Not built:** governor rework (research done; waiting on the owner's answers).

## Owner reports 8 Nov: menu seam and Empire priorities (no save change)

**How this was checked:**
- Unity `RealmTools.Checks` pass, including new checks: shares round to the nearest point, most science steps change science, and a city not training a soldier keeps its military share.
- A .NET test game was run at several priority settings.
- Screenshots were taken at 1280 x 800 and 1920 x 1080.
- **Not hand-played.**

**What changed:**
- **Seam line on the left menu:** each bar is now one picture rather than three stretched pieces, so no seam runs down the menu.
- **Priorities have an effect:**
  - Science and Military shares of each city's Trade are rounded to the nearest point, not down. A city with 4 Trade at 20% science now makes 1 science (it made 0).
  - A city not training a combat unit keeps its military share as gold, instead of losing it.

## Owner request 8 Nov: interface in the style of the mockup (no save change)

**How this was checked:**
- Unity `RealmTools.Checks` and `GraphicsChecks` pass.
- A Windows build ran `-autoShot`; screenshots are in `Documentation/Unity-Screenshots-2026-11-08-MockupUI`.
- **Not hand-played.**

**New art** (SpriteCook, with the owner's Religion mockup as the style reference; files in `Resources/UI`):
- 46 painted icons that replace every drawn interface icon (resources, menu, unit orders, moods, zoom, help and menu buttons).
- The compass-rose logo.
- A gold-cornered navy frame used by every panel in the game.
- A gold-trimmed menu bar and a round gold medallion.

**Layout:**
- **Left menu:** wider, each entry a gold-trimmed bar with its painted icon and name. The research card moved right to make room.
- **Top bar:** the logo on a framed plate; each resource shows a larger icon, the amount and the change per turn (green rising, red falling). Hovering a resource names it.
- **Bottom bar:** nine round medallions open Empire, Research, Civics, Chronicle, Victory, Diplomacy, Military, Realms and Religion. The hovered-tile card moved up above them.

**Not the same as the mockup:** the mockup is itself a painting, so this is the same style rather than a copy. Detail screens keep their own layouts inside the new frame, and the left menu shows only on the map, not over other screens. The mockup's Economy, Policies, Culture and World Events entries have no screens of their own yet.

## Owner requests 8 Nov, latest: sealed peoples' portraits, hills, volcanoes and gateway designs (save version 37)

**How this was checked:**
- Unity `RealmTools.Checks` (new gateway checks: sealed at start, two different designs per pair, opening one pair opens both ends and no other) and `GraphicsChecks` pass.
- A Windows build ran `-autoShot` with no exceptions.
- Screenshots in `Documentation/Unity-Screenshots-2026-11-08-Gates` show hills, Infernal Depths volcanoes, a sealed and an opened gateway on the map, the sealed scene and the opened scene.
- **Not hand-played.**

**Portraits:** all 40 sealed peoples of the five worlds now have painted portraits matching the others (`leader-39..78`).

**Terrain:**
- Hills are rounded grassy hills, not flat mounds.
- Volcanic terrain uses the mountains' cluster of peaks in dark volcanic rock, with a glowing crater and smoke.

**Gateways:**
- **Nine designs** from the owner's pictures: frost, grove, obsidian, void, sun, arcane, sky scaffold, steam works and rift.
- **Which design goes where:** a gateway in a parallel world wears its world's design (Fae Wilds grove, Ashen Waste sun, Infernal Depths obsidian, Plague Marshes rift, Drowned Deep frost). A home-world gateway wears one of the four home designs.
- **Sealed until opened:** every gateway starts sealed (a carved, rune-locked door in the same design) and shows its open portal once that gateway is opened. Gateways now open in pairs, one pair at a time, rather than all at once.
- **The gateway scene** shows the sealed picture, or once open the owner's painting with its own text.

**How gateways open** (owner: "by both"):
- **Knowledge:** the first home civilisation to learn a world's key technology breaks the seals of every gateway to that world. The keys are Fae Wilds Philosophy, Drowned Deep Printing Press, Plague Marshes Medicine, Ashen Waste Industry and Infernal Depths Nuclear Fission.
- **A wonder:** the Gatekeeper's Key (Mysticism) can only be built in a city within 10 tiles of a sealed gateway. It opens the nearest one.
- **A quest:** a Priest standing on a sealed gateway can perform the rite of unsealing for 100 faith; the priest is spent.
- Once every pair is open, the gateways count as unlocked.

**Measured:** headless game on a Small map, 400 turns. The first gateway opened around turns 200-250, the second by turn 300, the third by 350 and the fourth by 400.

**Not hand-played.**

## Owner requests 8 Nov, later: religion screen in the mockup's style, and new SpriteCook art (no save change)

**How this was checked:**
- Unity `RealmTools.Checks` and `GraphicsChecks` pass.
- A Windows build ran `-autoShot` with no exceptions. Screenshots of the Religion screen, the event panel, the native leaders' diplomacy and New Game are in `Documentation/Unity-Screenshots-2026-11-08-ReligionStyle`.
- **Not hand-played.**

**What changed:**
- **Religion screen:** now drawn in the style of the owner's mockup (`FaithScreen.cs`):
  - Header: sun emblem, faith name and motto, followers, faith and authority.
  - Gold tabs.
  - Overview: holy-city painting, doctrine medallions, a faith map of the explored world, one city's demographics with religious satisfaction, key institutions, and faith actions (the new Fund Pilgrimage: 40 faith, +1 gold in the holy city up to 3, +3 authority).
  - Right column: religious summary, recent events with turns ago, and the faiths of the world.
- **New art** (SpriteCook, matching the existing painterly pixel-art portraits):
  - Jade Dynasty and Serpent League leaders, so both civilisations are now playable.
  - The ten native leaders, replacing their code-drawn faces.
  - Modern-age portraits for eleven real-world leaders, and ancient and modern portraits for Mei and Itzel. Menelik II's was refused twice by the service's content filter, so he keeps his single portrait.
  - Five holy-city paintings, one per core belief.
  - Four religious event paintings.
  - A Priest unit sprite.

## Owner design 8 Nov: Faith, Belief and Religious Power (save version 36)

**How this was checked:**
- Unity `RealmTools.Checks` (including `ContentChecks.FaithDesignChecks`) and `GraphicsChecks` pass.
- A Windows build ran `-autoShot`, including all six Religion tabs and a decision panel, with no exceptions.
- **Not hand-played.**

**Measured** (.NET harness, Small map, Standard speed, 300 turns):
- Rivals founded three faiths, at turns 196, 230 and 287.
- Prophets appeared and state churches were chosen.
- Institutions reached level 6.
- No schism happened in that run.
- Technology pacing was unchanged.

**What changed:** see `Design/RELIGION-DESIGN-2026-11-08.md` for the full mapping from the owner's design. In short:
- **Origins and beliefs:** origins from events; beliefs chosen at founding (core, social, economic, afterlife).
- **Authority and institutions:** Religious Authority and eight institution levels; church and state; religious policy.
- **Faith in cities:** each city has faith shares; conversion is gradual with resistance.
- **Holy places:** holy cities and sacred sites.
- **Diplomacy:** religious opinion and five religious actions.
- **Evolution:** reformation, the reform movement and schisms into named branches.
- **Events and victory:** four religious events; religious victory.
- **Screen:** a six-tab Religion screen.

## Owner requests 8 Nov: Colossal maps, legions, hills, shared shields, workers, faith buildings and deeper diplomacy (save version 35)

**How this was checked:** Unity `RealmTools.Checks` (including `ContentChecks.EighthNovemberChecks`) and `GraphicsChecks` pass, and a Windows build ran `-autoShot` with no exceptions. **Not hand-played.** Balance is unproven.

**Measured:**
- .NET harness, Small map, Standard speed:
  - By turn 300 rivals know about 22 of 99 technologies.
  - They reach the Classical Age by turn 100 and the Medieval Age by turn 200.
- Colossal turn time: real build, `-frameProbe` Colossal globe with 16 civilisations and 50 sealed peoples (579,000 tiles): turns average 1.2 s at turn 20 and 2.3 s at turn 60 (worst 3.5 s); the map still draws in 6-16 ms a frame; a save takes about 4 s (autosave runs on a worker thread).

**What changed:**
- **Colossal maps:** a seventh map size, about 1.6 to 1.75 times Massive.
- **Legionary and Legion:** they replace the Warrior and the Spearman only once Aurelia or Rome reaches the Classical Age. Before then those civilisations train the ordinary unit.
- **Hills:** they already gave +2 defence (mountains +3, forest and river +1). The tile card, the map tooltip and the attack preview now show it.
- **Production:** a city's shields are one pool.
  - With a unit and a building both under way, each gets half (the unit the odd shield).
  - Either one alone gets all of it.
  - A building is finished only when the shields put into it reach its cost.
  - The city screen counts turns from each line's share.
  - Before this change each line received the city's full output.
- **New cities:** they start with nothing in training or construction. The founder chooses.
- **Fleeing workers:**
  - A worker never flees animals while on its own civilisation's land (animals do not cross borders).
  - Outside its land it runs for the nearest tile of its own land, or of a civilisation it has open borders, friendship or an alliance with, rather than for the nearest city.
  - Enemy soldiers (a civilisation at war with it, seen within 3 tiles) make it run to its nearest city, even on its own land.
  - A soldier on its tile, or being in a city, keeps it at work.
- **Faith:**
  - Faith is in the top bar.
  - Every city keeps its own faith store, filled by its faith each turn (the national pool fills as before), and makes at least 1 faith.
  - Religious buildings are bought with the city's own faith:

    | Building | Faith (Standard) | Effect | Needs |
    |---|---|---|---|
    | Shrine | 25 | +1 faith | — |
    | Temple | 60 | +2 faith, +2 content | Mysticism and a Shrine |
    | Monastery | 100 | +2 faith, +2 science | Philosophy and a Temple |
    | Cathedral | 160 | +3 faith, +3 content | Education and a Temple |

  - They appear under the Buildings tab.
  - Temples can no longer be built with production; a Temple waiting in a queue is removed when an old save loads.
  - Rivals buy them too.
- **Diplomacy** (see `Design/DIPLOMACY-RESEARCH-2026-11-08.md` for sources):
  - Opinion as named reasons with a panel on the diplomacy screen.
  - Influence spent on envoys and incitement.
  - Alarm at expansion, and coalitions that defend their members.
  - Ruler traits in opinion.
  - Deals valued by need.
  - Four rival postures.
  - Leader flavours.

**Not built:** hidden agendas, sanctions, event-driven diplomacy, and the Loyalty flavour's effect.

## Owner requests 7 Nov, latest: tree size, game speed, opening gold, animals and population (no save change)

Checked by Unity `RealmTools.Checks` (`ContentChecks.PacingAndPopulationChecks`) and `GraphicsChecks`, a Windows build, `-inputProbe` and `-autoShot`, and a 400-turn game in the .NET harness. Not hand-played.

- Technology tree: 99 researchable technologies, 10 in the Dawn Age, 14 Classical, 15 Medieval, 20 Industrial, 25 Modern and 15 Dimensional. Each age keeps its 6 keystones (4 still complete an age) and adds minor technologies spread over the keystones, each with its prerequisite. The other 81 minors stay retired; older saves keep any they learned.
- Game speed: research in a Long game now costs what Standard did (Agriculture took about 75 turns on Standard at the start). Standard is two thirds of that (about 50 turns) and Quick one third. A technology needs at least 10, 7 or 4 turns.
- Opening gold: before the first city a people pays no upkeep and forages +1 gold a turn, so a new game with a Warrior and a Worker shows +1. The capital keeps one soldier as its guard for free.
- Animals: three grazers for each hunter. Hunters stalk grazers as well as units (whichever is nearer); a grazer fights back, and a hunter that kills one heals. Hunters of different kinds keep a tile apart; forced side by side, they fight.
- Population after history (Chandler/Modelski city estimates, HYDE and UN urban shares, the demographic transition): a city's people grow with the square of its size, scaled by age (a size 8 Dawn city about 64,000, a size 14 Classical city about a million, a size 20 Industrial city 4.8 million, a size 38 Modern city about 36 million). Countryside people are added so that 5% live in cities in the Dawn Age, 8% Classical, 10% Medieval, 40% Industrial, 75% Modern, 85% Dimensional. Before Medicine, cities above size 8 need 15% more food to grow for each size over 8 and risk an epidemic (1% a turn per size, halved by an Aqueduct); Medicine cuts the food needed by a quarter and ends epidemics; in the Modern Age, cities above 12 slow again. Life expectancy follows the age (25 years in the Dawn Age to 72 in the Modern). Shown on the city screen, the Cities table, the Economy city list and Civics ("Living in cities").
- Measured (Small map, Standard, 400 turns, harness): rivals know 31 of 99 technologies and reach the Industrial Age around turn 350; 84 animals of which 21 hunters at turn 50; largest nation 41 million and largest city 4.8 million at turn 400.

## Owner request 7 Nov: the living world, after RimWorld and Dwarf Fortress (save version 34)

Checked by Unity `RealmTools.Checks` (`ContentChecks.LivingWorldChecks`) and `GraphicsChecks`, a Windows build and `-frameProbe`. Not hand-played; balance unproven.

Implemented, a first version of each of the four ideas:
- People: every home civilisation has a ruler and each city a governor, with a name and two of ten traits (Ambitious, Loyal, Greedy, Honest, Paranoid, Stoic, Neurotic, Pious, Brave, Drunkard). Stress comes from unrest, hunger, war, debt, occupation and the city's scars. At 85 a governor snaps (tantrum: the city's production stops two turns; binge: gold lost; withdrawal: loyalty lost) and so can a ruler (purge of the least loyal governor, melancholy, follies). Ambitious or neurotic governors start feuds. Corruption grows with distance from the capital and greed, falls with honesty and a Courthouse, and skims production and gold. A governor below 15 loyalty for five turns leads a rising: a garrison of a third of the city's size puts it down (leaving a scar); otherwise the city defects to the nearest home neighbour, or strikes if there is none.
- Memory: cities keep permanent scars (Famine, Conquest, a put-down rising; up to two content citizens lost). Civilisations keep grudges that never fade (10 opinion each, up to 40): capture, rebellion, and two to six wrongs written into a short history before turn 1, shown in the Chronicle.
- Materials: soldiers get gear (Poor, Common, Fine, Masterwork) from a Workshop or Barracks, connected iron or bronze, and Military Science; gear wears in battle and is mended in a city with a Workshop or Barracks. A breakthrough's blueprint is kept in the city with the most science; capturing it gives half that technology.
- Living map: every tile remembers its battles; 2 make a battlefield and 6 dead a graveyard, marked on the map. A graveyard gives nearby cities +2 faith from pilgrims, can spread sickness, draws scavenging animals, and a people resents whoever holds the graves of its dead.
- Imperial priority (Civics, The court): None, Fortify, Feed the people, Expand, Learning or Commerce. A city follows it only if its governor's loyalty, corruption and scars allow; the rest see to their own needs, and corrupt ones skim gold.

Not built yet from the proposal: physical caravans, warehouses and supply lines; interest-group factions and ideological schisms; migration; per-material equipment slots; artifacts as map objects.

## Owner requests 7 Nov, later: faith tree, debt, stacks and a drunken guide (save version 33)

Checked by Unity `RealmTools.Checks` and `GraphicsChecks`, a Windows build, `-inputProbe`, and `-unitProbe` on a copy of the owner's TEST GAME 2 save. Not hand-played.

- Crownmere's stuck units (owner's save): a stack formed inside the city held a Spearman, four Horsemen and three Settlers. Open land holds two civilians, so the whole stack could not take a single step and nothing said why. Now units without room stay behind and leave the stack, stacks too large for open land are trimmed when a save loads, and an order that cannot start explains the reason. On the owner's save the stack now leaves the city with seven units; one Settler stays.
- Faith tree: 18 doctrines in three lines (Rites, Learning, Community) across the six ages, after Civilization VI's civics tree beside its technology tree. Devotion equal to faith per turn fills the chosen doctrine without spending faith. A later age's doctrines open only when science has reached that age, and each needs the one before it in its line. Effects use the technology effect types, plus faith percentages.
- Debt: with an empty treasury every city has one citizen fewer content and soldiers fight a point weaker (as before); from the second turn the costliest soldier deserts each turn; from the third, creditors seize and sell a building each turn. The top bar says IN DEBT and Civics explains.
- Upkeep: units standing in their own territory cost half.
- Wild animals: half their old attack and defence, never step inside borders and never attack a unit inside them. Workers inside borders no longer flee.
- Stacks: "All N" on the unit card and "All N … units together" in the tile picker gather every unit of one kind on a mixed tile into its own stack.
- Divus Maximus: the tutorial and his prompts are now written drunk and slobbish; the instructions are unchanged.

## Owner requests 7 Nov: streamlined technology, expansion, water and climate (save version 32)

Checked by Unity `RealmTools.Checks` and `GraphicsChecks`, a Windows build, `-autoShot`, `-inputProbe` and `-frameProbe`. Not hand-played; balance is unproven.

- Technology: 36 technologies, six in each of six ages, instead of 180. Each now has one lasting effect drawn from the small technologies it replaces. An age stays locked until a known technology leads into it. Older saves keep any small technologies already learned.
- Research time: the pace table is raised and each technology takes at least 10 turns. On a Small map the best rival learns about one technology per 29 turns, reaching new ages around turns 110 and 300 and 14 of 36 by turn 400. The earliest Transcendence was not measured and is probably past turn 800.
- Expansion: no cost up to 4 cities, then rising (6 cities 1 gold, 10 cities 7, 16 cities 26). A city not joined to another by road, river or sea costs 2 gold and has one citizen fewer content.
- Water: a port, boats and ships need a body of water of at least 50 tiles beside the city or a port site in its borders.
- Lists: buildings and units a place can never make are hidden, and a unit leaves the training list when its replacement can really be trained (technology known, resource connected). Upgrades show only once the technology is known.
- Workers: an animal within 3 tiles makes a worker with no soldier beside it run to the nearest city; with none within 5 it walks back and resumes.
- New Game: three columns, with Climate (Varied, Temperate, Frozen, Tropical, Arid) and Wild animals (None to Teeming). Climate applies to the home world only.
- Rivals average a small gold deficit on crowded Small maps under the new costs.

## Owner requests 6 Nov, fourth batch: freeze fix, armies, roads and gateways (save version 31)

Checked by Unity `RealmTools.Checks` and `GraphicsChecks`, a Windows build, `-inputProbe` and `-routeProbe` on a copy of the owner's save. Not hand-played; balance is unproven.

- Freeze: sending a unit to a tile with no room searched the entire map, several times over, and the unit card repeated a route search on every interface pass. On the owner's save this was about 15 seconds an order; it is now under 0.2 seconds. Any route search stops at 50,000 tiles. No error was logged at the time of the reported crash, so the freeze is the likely cause, not a confirmed one.
- Armies: soldiers cost about 2.5 times the production (Warrior 30, Spearman and Archer 50, Horseman 65, Catapult 100, Knight and War Galley 110, Rifleman 150) and 2 to 4 gold a turn, with +1 gold for every three soldiers beyond two per city.
- Stacks and escorts: units on a tile can be stacked (button or G) and move as one at the slowest member's speed. Six soldiers and two civilians fit a tile; your own cities shelter any number of civilians. Attackers and animals must get through soldiers before civilians. Rivals do not form stacks yet.
- Roads: 1 gold a turn per 3 road tiles in your territory, not counting city tiles. Rivals and auto-develop workers build roads only while they can afford it, at most 8 per city, except to ports and improved resources. Road plotting: choose Road or R, point to see the course, new tiles, turns and upkeep; click to build, Shift-click for a bend, right-click to cancel.
- Wild animals move at half speed inside borders. Scouts exploring by themselves keep two tiles from animals they can see.
- Sealed gateway scene on first sight, once per gateway. A found gateway keeps a label, stays visible through mist and is marked on the minimap.
- Cities screen: every city's unit and building lines, changeable from the table.
- Auto-developing workers say what they are doing.

## Owner requests 6 Nov, third batch: faith prompt, food, animals, art and speed (save version 30)

Checked by Unity `RealmTools.Checks` and `GraphicsChecks`, a Windows build, `-inputProbe` and `-frameProbe`. Not hand-played; food and animal balance are unproven.

- Faith: when enough faith gathers, Divus Maximus asks you to name a god from the pantheon and explains why. It cannot be put off, and End Turn waits for the answer.
- Food: every unit eats (half a food; mounted, siege and large ships one). Each city's fields feed one food's worth; the rest comes out of city growth. When that runs out the empire is Hungry: the newest units cannot act or heal, and every city loses a content citizen. Plenty to spare (at least one food per city) is Thriving: +1 content citizen in every city. Shown on the top bar, unit card and Civics. Rivals stop training units and put cities on food when short. Saves from earlier versions start using this rule.
- Wild animals: one per 40 tiles of land (was 100), still three times that until a home civilisation passes the Medieval Age. Hunters do not hunt before turn 8.
- Art: unit pictures keep their full 128 x 128 detail; icons are four times their design size; sprites and zoomed-in ground twice; smooth filtering on sprites. Text already stayed sharp when scaled.
- Speed: End Turn no longer freezes the game; the turn runs in slices with the interface locked and a progress bar. Autosave runs on another thread. On a Huge map at turn 140 in the real build a turn fell from about 1,215 ms to about 460 ms. Rivals have fewer cities at that point (214 against 283), so some of the gain is smaller empires.

## Owner requests 6 Nov, second batch: research time, civics, legends and speed (save version 29)

Checked by engine checks, Unity `RealmTools.Checks` and `GraphicsChecks`, a Windows build's `-autoShot` screenshots, and `-inputProbe` (real clicks and keys sent to the game's own window; 9 of 9 pass). Not hand-played.

- Research: costs doubled for everyone. Time is science left divided by science per turn, at least 4 turns a technology (the Civilization III rule), and left-over science goes to the next. Rival eras now complete around turns 100 and 250 on a Small map; earliest rival Transcendence not yet measured.
- Army support: every soldier and warship costs gold (1, or 2 for Knight, Rifleman, Horseman, Catapult and War Galley). Free support now covers civilians only. Rivals' settler gold rule was loosened to match.
- Trade: +1 for each other city joined by road, river or sea, at most 4. River and sea need Sailing; sea needs linked ports on the same water.
- Workers cannot develop the tile a city stands on; cities count as on the road.
- Wild animals: three times as many and arriving from the start until a home civilisation passes the Medieval Age. None appear within borders; they can still wander in.
- City screen: Zoom to city; queue shown only above Rush with a scroll bar when long; chosen buildings greyed until built, right-click cancels; era-patterned influence bar. City plates show what is being made. Tile card grows until all text shows.
- Input: wheel ignored while a prompt waits; double click opens a city whatever stands on it; Ctrl+S shows "Game saved"; Save As asks before saving over; Load Game in the Esc menu (saved games are no longer loaded from the Save As list).
- Realms: globe north is at the top and Tilt north goes north (it was drawn upside down); Face my units removed.
- Legends: a world shows once a home civilisation has found a gateway to it; a people's detail grows through contact, friendship or agents (`SendAgents`: 50, 100, 150 gold). This is the only espionage in the game so far. While the gates are locked, agents are the only way to learn about these peoples. Close on a people's page now works.
- The Open Register is renamed the Ninefold Ledger.
- Civics tab: demographics ranked against home civilisations you have met, finances line by line, economy, military and society.
- Speed: Huge map at turn 200 fell from about 3.9 s to about 0.9 s a turn in the .NET harness (the real game on Mono will be slower). Autosave on maps over 60,000 tiles is every 10 turns. Rival paths may be slightly less direct; sealed peoples plan one turn in three after turn 40. Crowded Small maps with all 56 nations still reach 1 to 2.5 s late on.

## Owner requests 6 Nov, map, city screen, prompts and empire priorities (save version 28)

Implemented and checked by the engine checks, Unity `RealmTools.Checks` and `GraphicsChecks`, and a Windows build's `-autoShot` screenshots. Not hand-played: dragging sliders and scroll bars, hovering and clicking were not exercised by a person.

- Empire priorities: Science, Military and Finance sliders on the Empire screen, linked and always totalling 100 (`EmpireBalance.cs`). Science keeps its ceiling. Military is a share of Trade, added as shields to any combat unit in training; if none is training, the share goes unused and the screen says so. Finance of 40% or less is low tax, 70% or more high tax. Old saves start at 0 military, which matches the old rules exactly. Rivals stay at 0 military for now.
- After a discovery, the guide asks what to research next (the cheapest choice stays selected until you answer), then asks each city with an idle building line that gained a building, one city at a time. The tutor picture is local only; without it the panels show no face.
- City screen: influence bar with numbers (`NextBorderGrowth`); land view keeps its tile size and scrolls by bars, wheel (Shift for sideways) and right or middle drag.
- Map: unit feet stand on their ring (art is aligned on its lowest solid row), bolder borders, black mist over unexplored land, a faint route line for a selected unit with an order, guard pictures for fortified combat units, units listed on the tile card.
- Guard pictures are generated from the unit art (shield and upright weapon; pavise for archers, sandbags for riflemen, stakes for catapults). Supplied art named `Visuals/unit-NN-guard` replaces them; no such art exists yet.
- Chronicle uses Gabriola, an operating system font that is not shipped with the game; other systems fall back through Lucida Calligraphy, Monotype Corsiva and Palatino Linotype to the default font.
- Flashing horizontal lines: frame comparison traced them to the drifting cloud-shadow sheet (hard-edged blocks in separate rectangles). It is now one smooth sheet. `-flickerProbe <folder>` found 63 thin-band hits before and 0 after at 1422 x 783, over 180 idle frames at each of three zooms, and no map seams while panning. It cannot see display-level tearing.
- Unit suggestion on the city message only appears when both production lines and their queues are empty.

## Owner requests 6 Nov, wild animals and larger maps (save version 27)
- **Settling:** the found-city message no longer shows grid coordinates. The older settlement adviser screen still does.
- **Larger maps:** every map size in new games has 20% more tiles, on flat and globe maps, sealed worlds included, so saves and turns grow a little (Small globe 54 x 57 instead of 49 x 52). Saved games keep their size.
- **Wild animals (`Wildlife.cs`, `World.animals`, `World.wildlife`, new games only):** Cave lion, Smilodon, Cave bear, Dire wolf (hunters); Megatherium, Woolly mammoth, Woolly rhinoceros (grazers). Attack, defence and steps per turn are in `AnimalCatalog`. Animals belong to nobody, are kept in their own list and take no part in diplomacy or victory.
- **Behaviour:** hunters go for any civilisation's unit within 7 tiles, and strike when adjacent. Grazers attack only a unit right beside them, and never wander next to one, so one clear tile is safe. Units in cities or on water are safe; animals never enter cities. They spawn at random from turn 8, one try a turn at 1 in 3, on unowned land at least 6 tiles from any unit or city, favouring their own terrain, up to one per 100 tiles of open land, scaled by difficulty (40% to 150%). Home world only, not the sealed worlds. They heal 1 health a turn.
- **Fighting:** a soldier attacks by moving onto an animal, with the usual no-dice formula, and a kill counts as a victory. Workers and Settlers cannot attack. Routes go round animals, and rival soldiers hunt animals within 5 tiles when the result favours them; rival settlers avoid hunters. Attacks on your units appear in the notices.
- **Pictures:** drawn by code (`PixelArt.AnimalSprite`), no SpriteCook credits used. Rough; painted ones could replace them.
- **Measured (.NET harness, 4 pairs of full games with and without animals, 3 rivals, stand-in player kept out of rival war, not playtesting):** animals reach their limit by about turn 60 (6 on Tiny, about 15 on Small, 26 on Medium). Rival Transcendence came at turns 312 against 288, 292 against 294, 298 against 280, and 348 against 341. Rival city counts at turn 120 were about the same. Turn time rose about 0-45% in these runs, which ran side by side (Medium 221 ms against 153 ms). How dangerous they feel in a human opening is unknown: nobody has played it.
- **Checked:** engine and UI harnesses pass, Unity `RealmTools.Checks` (now including animals surviving a real save) and `GraphicsChecks` pass, and a build ran `-autoShot` with no exceptions (`Documentation/Unity-Screenshots-2026-11-06-Animals`, animals staged for the picture).

## Owner requests 6 Nov, glowing technologies, difficulty and messages (save version 26)
- **Technology borders:** on the research screen known technologies have a glowing gold border and the one being researched a glowing silver one, breathing slowly; the research card on the map has the silver glow too (`GlowBorder`).
- **Eight difficulty levels (`Difficulty.cs`, `World.difficultyLevel`):** Pilgrim, Novice, Apprentice, Steward (the plain rules, and what older saves play at), Veteran, Champion, Paragon, Ascendant. Chosen on New Game, which shows every number; also shown on the Identity screen. They change rival unit, building, wonder, growth and research costs, target cities, army limit, wish for war, the turn they may first attack you (140 down to 40), attitude to you and starting extras, and on the three easy levels your research and build costs. Rivals never see through fog; the sealed peoples are unchanged. The old `difficulty` field remains unread.
- **Measured (.NET harness, 2 Small games per level, 3 rivals, stand-in player kept out of war, not playtesting):** rival Transcendence at Pilgrim turn 435 and none by 450; Novice none by 450 on either; Apprentice 361 and none by 450; Steward 282 and 371; Veteran 280 and 284; Champion 244 and 266; Paragon 223 and 262; Ascendant 200 and 213. Ascendant is the only level where a rival completed era 5. Pilgrim and Novice are close to each other. How hard each level is against a person is unknown.
- **City finished message (`CityNotices.cs`, `CityNoticesUI.cs`):** when a city finishes a unit or building, a small message on the map says what, says what is next if something is queued, and otherwise suggests something with a reason and a button to make it. Open city and OK are always there; several wait in turn, and any left go when you end the next turn. Transcendence stages do not show one.
- **Leaders calling:** a proposal, counteroffer, denunciation or declaration of war brings up a message with the leader's portrait and a button to Diplomacy (`RivalCall`). A proposal's message goes when it is answered or expires. Denunciations and war were added beyond the request, because they are the warning before an attack.
- **Checked:** engine and UI harnesses pass, Unity `RealmTools.Checks` and `GraphicsChecks` pass, and a build ran `-autoShot` with no exceptions (`Documentation/Unity-Screenshots-2026-11-06-Messages`). Those screenshots stage the messages; the buttons have not been clicked by hand.

## Owner request 6 Nov, rival armies and war (`RivalWar.cs`, no save version change)
- **Why rivals never fought (logged):** they signed friendships and alliances with everyone they met, opinion climbed towards 100, and a friend cannot be attacked. Armies stayed Warriors at a limit of 14 units that also counted workers, and gold piled up.
- **Armies:** home rivals count only soldiers against a limit of 3 + 2 per city, half as many again at war; train line troops with one ranged unit in four; upgrade in cities at the player's price; replace an outdated soldier that cannot be upgraded; and buy what cities are making above a gold reserve, at the player's price (projects cannot be bought). War Galleys are capped at 3-4. No hidden bonuses or sight.
- **Going to war (`Ambition`):** from turn 40 (turn 80 against the player, `PlayerGrace`) a rival scores what it can see: agenda, traits, a small roll fixed per game, distance, relative strength, opinion, a neighbour with twice its cities, and any Transcendence project (+40). At 50 or more it offers no new treaties, denounces, and declares war 3 or more turns later once any friendship has run out. Reasons appear in the diplomacy log. **Owner decision, 6 Nov:** rivals may attack a peaceful player.
- **Fighting (`WarUnitAI`, `GoodFight`):** combat has no dice, so units use the exact result: they attack only if they kill, or deal at least what they take and survive. Hurt units (3 health or less) fall back to a city; units gather before storming a defended city.
- **Peace:** a rival asks only when clearly weaker or after 40 turns of war, and a clearly stronger rival refuses before then, including peace offered by the player.
- **Speed:** a rival's plain move no longer refreshes all fog, ports and victory checks, and rival workers plan once per empire per turn and look for work one turn in three after turn 80. Harness, late game: 16 nations at war about 0.5 s a turn; 54 nations about 1.1 s (was about 1.5 s at peace). Unity's runtime was not timed.
- **Measured (.NET harness, 8 side-by-side full games with computer rivals; player idle or kept out of war; not playtesting):** Small maps and five-rival Medium maps had war on 145 of 284 turns (the one Small game that ran to the end) and in every shorter game from about turn 46-88, with 2-18 cities changing hands and armies of Spearmen, Knights and Riflemen. Rivals still spend little gold when no war is near. Three-rival Medium maps (2 of 2) had no war at all: rivals were too far apart, and held 1,157-7,800 gold. An undefended idle player fell at turns 92-172 in 5 of 6 games. Games that reached the end were still won by Transcendence, at turns 284-321, with eras 5 and 6 still not completed. One earlier run had a rival reach 25 cities while three others lost everything, before the rule that a far larger neighbour draws enemies; that seed did not change with it, so runaways remain possible.
- **Checks added (`EmpireChecks.RivalWarChecks`):** soldier limit, upgrades, buying, reserve, exact fights, fallback, no hopeless attack, peace by the state of the war, ambition timing, warning before war, friends left alone, sealed peoples left out, a whole Tiny game. The pacing check keeps its idle stand-in out of war, because it now would be conquered.
- **Not hand-played:** nobody knows yet whether this is too hard for a new player; there is no difficulty setting to soften it.

## Owner request 5 Nov, fifty peoples of the sealed worlds and improvement research (no save version change)
- **Ten peoples per world:** the existing ten (two per world) plus forty new (`SealedPeoples.cs`, `SealedPeoples<World>.cs`, catalogue 39-78), each with a world history, epithet, background, history, leader, greeting, twelve cities, two traits, agenda, unique ability, unit, building and weakness. Inspired by biblical and apocryphal writing, other religions and myths, science fiction and fantasy; every name is invented and no figure sacred to a living faith appears. Source notes are in `Design/research_notes/Hex 4X improvement research/civilisation_inspirations.md`. Names were not checked against existing games or trademarks.
- **Rules from numbers:** the new peoples' rules are small integers (`PeopleRule`) and the text the player reads is written from them.
- **In a game:** New Game has a "Peoples in each sealed world" slider, 1 to 10, default 10. New `World.New` calls default to 2, so older checks are unchanged. Nations can reach 66 (`World.MaxNations`).
- **Legends of the sealed worlds:** the gates are locked, so the Realms screen has this new button to read each world's history and all ten peoples. Portraits are drawn by code (`PixelArt.PeoplePortrait`) because SpriteCook credits ran out; painted ones can be dropped in as `Resources/Leaders/leader-39.png` to `leader-78.png`.
- **Turn time (.NET harness, one game each, 54 nations, late game, averaged over 5 turns at turn 165):** Tiny 1.6 s, Small 1.5 s, Medium 1.5 s. Save about 6, 12 and 20 MB. Large and up not measured; Unity's runtime is probably slower than this harness, not measured either. To make this possible: a move no longer refreshes fog for every civilisation (also helps normal games: Small with 14 nations went from about 400 to about 190 ms at turn 165), contact checks skip peoples behind different gates, and sealed peoples at peace rouse idle troops, workers and caravans one turn in four. Natives still track home technology and reach about 5 cities each.
- **Research:** `Design/reports/Hex 4X improvement research deeper.md` replaces the first report (`Hex 4X improvement research.md`, which a fact-check found overstated in places); ranked list with sources and measured games, none of it built yet.
- **Text review, 6 Nov (`research_notes/Hex 4X improvement research deeper/peoples_review.md`):** priority A and B text changes applied: the Stiltwind Factors no longer read as plague, debt and expulsion; The Manifest and Flensers of Ummoth softened; unit Ravener renamed Grey Famisher (a Warhammer clash), the Unfed's leader renamed Gorrum; duplicate names and contradictions fixed; inspiration comments corrected or added. Optional renames (Firebreak Companies and others) not applied. Most leader, unit, building and city names were not searched, and no sensitivity reader was consulted; replacement names were not searched either.

## Owner request 5 Nov, hex grid (save version 25)
- **New games are on a hex grid (`World.hexGrid`, `Hex`, `Rows`; `World.New(...,hex:true)` from the New Game screen):** six neighbours in rows with odd rows half a tile east, on both flat and globe maps. Movement, distance (`Distance`), vision, borders, city reach, roads, rivers, pathfinding, settlement, religion spread, naval AI and map generation all follow it. `Step` is axial on hex maps and gives null at the two corners of a 3 x 3 block; loops wider than one tile use `StepIn(...,radius)`, which covers exactly the hexagon; `StepToward` replaces straight-line stepping.
- **Sizes:** the same tile count as before, nearly square in columns and rows, because rows are 30 of 64 pixels apart (Small 37 x 38 flat, 49 x 52 globe).
- **Picture:** pointed-top hexagons 64 x 40 (`PixelArt.HW/HH/HR`), `ComposeHex` for ground with the same organic borders, coasts and textures, hex versions of borders, roads, rivers, selection, farms and fog (`HexEdge`, `HexRoad`, `HexRiver`, `ToHex`), picking (`HexPickOf`), city screen, minimap and Realms map. Sprites are unchanged.
- **Not hex:** older saves keep the square or diagonal map they were made with (they cannot be converted without scrambling their land). Those games still draw and play as before. The classic view only draws square tiles, so it is switched off in hex games. The globe overview still paints each tile as a block.
- **Measured (.NET harness, Small, 4 civilisations plus natives, one game each, 200 turns, player's technology copied from a rival):** first rival era completions square globe 28/87/147/191 and 36/89/155; hex globe 35/93/143/187 and 38/95/153; square flat 39/93/149 and 47/100/157/202; hex flat 39/92/141/189 and 43/108/166. Technology at turn 200: square 81-88, hex 80-84. Cities and population similar, natives still track home. No rebalancing done. Turn time about the same.
- **Checks added (`ContentChecks.HexChecks`):** six reciprocal neighbours everywhere, distance equals walking the grid, radius 1-5 hexagons have exactly 3r(r+1)+1 tiles, globe wrap, flat edges, picking round trip, neighbours one hexagon apart on screen, tile picture shape with no gaps, 40-turn games on both shapes, version 24 save keeps its square map. The other engine checks still use square worlds (`World.New` defaults to `hex:false`).
- **Verified:** see the Unity check line in `CLAUDE.md`. Not played by hand; no human has yet clicked around a hex map.

## Owner request 5 Nov, gates locked and sealed worlds keep pace (save version 24)
- **Gates locked (`World.gatesLocked`, default true; `Realms.cs`):** the ancient gateways stand on both sides and are drawn as stone doors barred with iron and a gold lock (`PixelArt.SealedGate`), named "Sealed ancient gateway" on the tile card. A unit can stand on one but nothing crosses, for any civilisation, so the home world never meets the native peoples and the Realms screen lists only worlds already discovered. The crossing code is kept behind the flag. There is no way to open the gates in play yet. **Not requested by the owner:** while locked, native peoples cannot build Transcendence, since nobody could stop them.
- **Older saves:** version 24 locks the gates and sends stray land units back through their gateway (`RecallTravellers`). A colony already founded beyond a gate stays, with its garrison, cut off; it can still be viewed from the Realms screen.
- **Same rate as the home world:** before this change the Infernal and Ashen peoples often stalled at one size-1 city on no food. Fixes: (1) land in the Ashen Waste, Infernal Depths and Plague Marshes feeds its own people (`NativeGround`: ash, lava and bog 3 food, other land at least 2, all at least 1 trade, lava 2 shields) and stays harsh for outsiders; (2) the Drowned Deep has larger islands in new games (about 30% land, was about 5%); (3) research tracking (`KeepPace`): a people behind the home average (`HomeTechPace`, player included) gains one eighth of its current technology's cost per technology behind, and one more than 2 ahead cannot finish its next technology; (4) AI settlers scouting with no known site keep walking instead of turning back each turn, and a slow unit can always take one step (Spore Circle's 2-move units could never enter bog, cost 3; `StepCost`). Fix (4) also applies to home rivals.
- **Measured (.NET harness, Small, seeds 702 and 703, player's technology copied from a rival because nobody plays it):** home rivals at turns 60/120/180/240 hold 11/45/77/101 and 10/42/66/92 technologies on average, natives 13/46/74/100 and 12/44/68/92. Natives have 4 to 6 cities (home 4 to 6). Population at turn 240 is 130 and 97 for natives against 96 and 83 at home, so they run somewhat larger; not tuned further. Not played by hand: a slow human player pulls the average, and so the natives, down.
- **Checks added:** locked gate for player and rival, crossing when unlocked, version 23 upgrade, native and outsider yields, catch-up and ceiling, no native Transcendence, slow-unit step, natives within 12 technologies of home at turn 200.
- **Verified:** engine harness and UI harnesses pass; Unity `RealmTools.Checks` and `GraphicsChecks` pass; a Windows build ran `-autoShot` with no exceptions (`Documentation/Unity-Screenshots-2026-11-05-SealedGates`). Not hand-played; loading a real older save not tried (the upgrade is checked in code).

## Owner requests 31 Oct, full-size parallel worlds with native peoples (no save version change)
- **Full size:** each parallel world is as tall as the home world (`World.realmHeight` = `mapHeight`; games from the previous build keep their 20-row worlds). A map is now about six times the tiles of the home world. Measured in the .NET harness with 14 civilisations at turn 30: Small 15,912 tiles 89 ms a turn (save about 7.5 MB), Medium 28,128 tiles 118 ms (13 MB), Large 62,928 tiles 217 ms (29 MB); Huge and Massive not measured. Saves are now written as compact JSON.
- **Native peoples (`CivLore.cs` entries 29-38, `Realms.cs` `AddNatives`, `NativePortraits.cs`):** Sylvan Elves and Aerie of Wings (avians) in the Fae Wilds, The Iron Mind (machines) and Ashforge Clans (dwarves) in the Ashen Waste, Abyssal Throne (demons) and Bloodtusk Horde (orcs) in the Infernal Depths, Gnawing Swarm (ratkin) and Spore Circle (myconids) in the Plague Marshes, Tidebound Courts (merfolk) and Scaled Dominion (lizardfolk) in the Drowned Deep. Each has a leader, greeting, two unique traits, twelve city names, lore (background, history, strengths, weaknesses), a unique ability, unit, building and weakness, and an agenda. Up to ten join a new game (16 civilisations in all, so fewer with many home rivals; the New Game page says how many), each starting with a city, warrior and worker in its own world away from the gateway. They are full AI civilisations (research, expansion, diplomacy, war, gateways); they do not suffer their own world's dangers, do not found Earth religions, everyone distrusts the demons (-10 opinion), and they are not needed for conquest or Concord victories. Portraits are drawn placeholders with species features until painted ones are supplied as `Resources/Leaders/leader-29.png` ... `leader-38.png`. Names avoid trademarked game terms (ratkin, not the Warhammer name).
- **Interface:** the diplomacy chooser fits up to 15 leaders in two rows; long civilisation names shrink to fit.
- **Checks:** natives are created two per world with a city each, a 14-civilisation game has room for two, conquest and Concord count home rivals only; checks that fake very old saves or measure home-only rules use worlds without natives (`World.New(...,natives:false)`); pacing is checked for home rivals (in one Small game the natives reached the first era between turns 32 and 100). Engine harness, UI harness and Unity checks pass; a Windows build ran `-autoShot` with no exceptions, with screenshots of the native leaders in diplomacy and a full-size Infernal Depths. Not hand-played; natives' balance is untested.

## Owner requests 31 Oct, ancient gateways and parallel worlds (no save version change; new fields default to 0)
- **Five parallel worlds (`Realms.cs`):** Fae Wilds (enchanted forests and lakes; units heal 2 a turn), Ashen Waste (ash and ruins; -1 health a turn outside cities), Infernal Depths (lava and volcanic rock; -2 health a turn outside cities; iron and gold), Plague Marshes (bogs and dead forest; -1 health a turn, and units carry plague home for 5 turns: a city they enter loses 2 food a turn while it lasts) and the Drowned Deep (ocean, coral and small islands). Each is a band of 20 extra rows below the home world (`World.realmRows`, `Tile.realm`), walled off by two rows of impassable mountains (`realm` -1), so movement, fog, cities, combat and the AI work there unchanged and no unit can walk or sail between worlds. Relic sites (ruins) there give 2x rewards, 3x in the Ashen Waste and Infernal Depths. New games only; saves from before have no parallel worlds.
- **Ancient gateways:** one stone arch per world on the home world (open land, at least 8 tiles from any start and 10 from each other) and a twin inside that world (`Tile.gateLink`). A land unit that steps onto a gateway comes out at its twin (or the nearest free land beside it) with no moves left; rival units can cross too. Gateways cannot be settled. Satellites show the home world only.
- **Separate view per world (owner request):** the map shows one world at a time. The Realms button (screen now titled Realms) lists the home world and every parallel world you have discovered and switches the map between them; the map, minimap and Realms map show only that world, a banner on the map names the world with a Home world button, the globe is home only, and the map follows your unit through a gateway. The tile card names a gateway once you have been beyond it ("destination unknown" before). The tutorial mentions gateways.
- **Checked:** engine harness passes (all sections, with new checks for gateway pairs, sealed worlds, crossing into the Infernal Depths, burning, plague and its food cost, unsettleable gateways and validity; older map checks now measure the home world); UI harness passes; Unity RealmTools.Checks passes (fixed a migration case so every civilisation's map memory covers the parallel worlds); a Windows build ran `-autoShot` with no exceptions and screenshots show a gateway, all five worlds and the Realms switcher. Not hand-played; turn time on large maps with the extra rows is unmeasured.

## Owner requests 31 Oct, tutorial fix and civilisation groups
- **Tutorial stuck at 6/11:** steps 1-5 finish by themselves, but step 6 is the first with a Next button, and the map underneath took the click. The tutorial panel (or its minimised portrait) is now a HUD area (`TutorialRect` in `BuildHudRects`), so Next, Hide and End tutorial work.
- **Civilisation groups:** the New Game screen has two tabs, Fantasy Kingdoms (13) and Real World Civilisations (12), each ending with "Create your own"; a custom civilisation's portrait choice stays within its group (count shown within the group). The group is saved (`World.realWorld`, defaults false) and rivals are drawn from the same group first, then from the other if more are needed (`CivilisationCatalog.Offered(bool)`, `FirstReal` = 17). Checks: a Real World game, a Fantasy game and a Real World custom civilisation all meet rivals of their own group.
- **Checked:** engine harness passes (all sections); UI harness passes; Unity RealmTools.Checks passes; a Windows build ran `-autoShot` with no exceptions, and screenshots show both tabs, the Real World "Create your own" page and the tutorial's step 6 with its Next button. The click fix is reasoned from the code, not clicked by hand.

## Owner requests 31 Oct, tutorial (no save change)
- **Tutorial with a guide (`Tutorial.cs`):** "Divus Maximus", drawn from the owner's picture (`Resources/Tutor/guide.png` 1024 x 768 and `guide-face.png` 256 x 256, converted from the owner's WebP), talks the player through 12 steps: welcome (large card, skip possible), found a city, open the city, research, give a unit an order, end the turn (these five finish by themselves when done), then workers, growth and happiness, borders and the wider world, faith, other civilisations, ages and victory (Next buttons). The panel sits under the top bar, can be hidden to his portrait or ended; it replaces the opening help page when on. A checkbox on the New Game world page and in Settings switches it on or off, and Settings can restart it. Progress is kept on this computer (PlayerPrefs `RealmTutorialOn`, `RealmTutorialStep`), not in saves; the automatic screenshots restore the player's values afterwards.
- **Checked:** UI harness passes; Unity RealmTools.Checks and GraphicsChecks pass; a Windows build ran `-autoShot` with no exceptions, and `1a-tutorial-welcome` and `1b-tutorial-step` show the welcome card and the first step panel. Not hand-played through all steps.

## Owner requests 31 Oct, fifth batch: civilisations, priests and ages (save version 23)
- **Research:** a comparison with Civilization IV-VII, Humankind, Old World, Age of Empires II/IV, Stellaris and Dwarf Fortress is in `Design/GAME-COMPARISON-2026-10-31.md` (ranked improvement list with sources; the Civ fandom wiki refused automated reading, so some numbers there come from memory and are marked). Implemented from its list: unique units and buildings per civilisation (#4), leader agendas (#5), priests (#7, section c), and era score with ages (#2). Not yet: event cards (#1), barbarian camps (#3), great people (#6), score victory (#8), history log (#9), conquest unrest (#10).
- **Civilisations in full (`CivLore.cs`, `CivLoreUI.cs`):** 22 civilisations have an epithet, background, history, strengths, weaknesses, a unique ability, a unique unit (replaces a common one: name plus attack, defence or movement), a unique building (replaces a common one, extra yield), a real weakness, a leader agenda and traditional faiths. Ten are premade civilisations with portraits (Azure Crown, Oasis Sultanate, Holy Imperium, Sahel Empire, Hellenic League, Frostpeak Realm, Aurelian Republic, Rising Isles, Steppe Horde, Northmarch Kingdom); Lutetian Empire, Sunspire Emirate and Lionmarch Kingdom keep their two traits only.
- **Real civilisations (indices 17-28):** China (Qin Shi Huang), Egypt (Ramesses II), Rome (Augustus), Greece (Pericles), Aztec Empire (Moctezuma II), Inca Empire (Pachacuti), Maya (Pakal the Great), Ethiopia (Menelik II), Arabia (Harun al-Rashid), Persia (Cyrus the Great), India (Ashoka), Babylon (Hammurabi), each with two traits (unique pairs), twelve historical city names, a greeting and full lore. Their portraits (`Resources/Leaders/leader-17.png` ... `leader-28.png`, 300 x 400) were made with SpriteCook (Nano Banana 2 Lite, 8 credits each, 96 in all, owner approved; three of the owner's original portraits as style references); the generated silhouette (`PixelArt.LeaderPlaceholder`) remains only as a fallback if a file is missing. The owner's SpriteCook account had 8 credits left afterwards.
- **New Game screen:** 26 cards in two rows; the chosen civilisation shows its traits, four rules, strengths, weaknesses and agenda; "History and lore" opens the full background and history. The Identity screen shows your own civilisation's lore; the diplomacy screen shows the other leader's agenda and how it judges you.
- **Agendas:** eight kinds (merchant, wisdom, faith, wonders, land, peace, borders, strength) add -12..+12 to a rival's opinion (`World.AgendaOpinion`). The diplomacy and empire checks run with agenda-free rivals so they still measure the treaty rules alone.
- **Priests (`Priests.cs`, unit type 15):** trained (40 production) in a city that follows a faith and has a Temple, or bought for 50 faith on the Religion screen; carry their city's faith; Preach in or next to a city for +200 pressure (halved where a priest of another faith lives in that city), two sermons; keep 1 citizen content in their own city; heal your units on and beside their tile by 2 a turn. Rivals buy up to two and send them to the nearest city of another faith. Drawn robed figure with a staff (`PixelArt.PriestSprite`; slot `Visuals/unit-15`).
- **Era score and ages (`Ages.cs`):** deeds score points (city +2, keystone +1, wonder +4, battle won +1 up to 6, city converted to your faith +2, faith founded +3, first contact +1). Entering the next era starts a Golden Age (score >= 14 + 2 x era: +1 production, +2 gold, +1 content per city), a Heroic Age (Golden straight after Dark, plus +1 science), a Dark Age (score < 6 + era: -1 content, +2 faith per city) or a Normal Age, for 15 turns. Shown in the top bar and on the Empire screen.
- **Bug fixed:** religion used building 39 (Bank) where the Temple is 36, so founding a faith needed a Bank; now the Temple.
- **Saves:** version 23 adds a unit's faith and sermons (older units get none); Nation era-score fields default to 0.
- **Checked:** engine harness passes (all sections, with new checks for lore completeness, Keshig and Egyptian rules, agendas, priest training, buying, preaching, retiring, healing and content, Golden and Dark ages, and 22-to-23 migration); UI harness passes; Unity RealmTools.Checks passes; a Windows build ran `-autoShot` with no exceptions, and screenshots show the 26-card civilisation screen, the lore panel, the Religion screen and a priest on the map. Not hand-played; balance of the 22 rule sets is untested.

## Owner requests 31 Oct, diplomacy look (no save change)
- **Sharper leader portraits:** the 300 x 400 pixel-art portraits were imported resized to 256 x 512, compressed and mipmapped. `Assets/Editor/LeaderImport.cs` now imports `Resources/Leaders` at true size, uncompressed and point-filtered (the picture files are unchanged; their `.meta` import settings changed, GUIDs kept), and `RealmGame.Crisp` draws each from a 2x pixel-for-pixel copy with mipmaps, so they stay crisp when enlarged and shrink cleanly on cards.
- **Diplomacy background:** the leader chooser and the talks are drawn over the painted title landscape under a navy veil instead of plain black, and the chooser heading sits on a panel.
- **Checked:** Unity reimport and RealmTools.Checks pass; a 1920 x 1080 Windows build ran `-autoShot` with no exceptions; screenshots `4d-first-contact` and the new `4e-diplomacy-chooser` show the crisp portrait and the painted background. Not hand-played.

## Owner requests 31 Oct, fourth batch (no save version change; Tile.cleared is a new field that defaults to false)
- **City not growing (owner's game, turn 120):** Temur's Camp at size 1 had its only citizen as a scientist, so no tile was worked and food was +0. The city screen now has "Specialist → back to work" (entertainer first, then scientist, then tax collector) next to "Worker → Entertainer" (`World.ReturnToWork`), and the growth line says when specialists are why food has stalled. Loading the owner's autosave in the harness: without the scientist the city makes +8 food and grows in 2 turns.
- **City plates:** taller (24 px for your cities, 20 for others) and the name is never clipped vertically.
- **Trees are woodland:** savanna acacias and the tree groves on some grassland count as woodland like forests (`World.Wooded`): a worker clears them (Clear) before a farm or mine. Savanna and grassland keep their terrain and only lose the trees (`Tile.cleared`).
- **Farms replace the scenery:** a farmed tile shows only the farmland, with no grove, acacias or other scenery on it.
- **Savanna:** acacias drawn at 60% so the tile reads as grassland with scattered trees.
- **Clouds and units:** units are drawn again over the cloud banks of unexplored land, so clouds never cover them.
- **Developed land always visible:** tiles in your territory with a road, farm, mine or fort are always in view, never under the mist (`World.Developed`; also for rivals' own view).
- **Hills:** grassy mounds (2-3 per tile, lit from the north-west, `PixelArt.HillSprite`) over soft rolling relief in the ground (`HillDomes`); the supplied boulder picture `terrain-02` is no longer drawn for hills (file kept).
- **Grassland and steppe:** painterly ground: broad warm and cool patches, diagonal brush strokes and occasional blade dabs for grass; wind-combed straw streaks, bare earth patches and dry tufts for steppe.
- **Checked:** engine harness passes (all sections); UI harness passes; Unity RealmTools.Checks passes; a Windows build ran the `-autoShot` sequence with no exceptions, including new close-ups of hills (`6a-hills`) and open land (`6a2-open-land`). Not hand-played. Windows Smart App Control blocked a scratch preview tool once; it was not worked around.

## Owner requests 31 Oct, third batch (save version 22)
- **Menu:** the game menu has Return to Main Menu; the game stays open there under Resume Game.
- **City names on the map:** plates widen to fit the whole name; only very long names step the type down (to 8 pt at the smallest).
- **Settlers** cost 90 production (three times 30), so they take three times as many turns.
- **Worker activities:** Road, Farm, Mine and Clear (woodland, axe icon) are always on the worker's bar, and More lists every activity (Road, Farm, Mine, Clear woodland, Fort) with turns; any the civilisation lacks the technology for is greyed out and names it (`World.JobTechMissing`). There is no bridge activity in the game.
- **Borders:** cities of one civilisation share one territory, so their borders merge (already so; now covered by a check). Where two civilisations' reaches meet, a tile stays with its holder until influence moves it: each nearby city exerts influence (2 + half its size + Temple 2 + Colosseum 1 + 2 per wonder + its faith + 1 if it follows its owner's own faith), divided by 1 + distance. When a rival's total is stronger the tile swings 2-6% a turn towards it (shown on the tile card); at 90% it changes hands; if the holder is stronger the swing fades 3% a turn. Tiles next to a city never change hands (`Religion.cs`: `SpreadInfluence`; `RebuildTerritory` keeps current holders). A check shows a much stronger city takes a tile in 15-45 turns.
- **Religion (`Religion.cs`, `ReligionUI.cs`, Religion button on the left bar):** faith from the capital (+1), Temples (+2), a holy city (+3) and beliefs. A pantheon (6 folk beliefs, 25 faith) comes first. Founding a faith needs Mysticism, a city with a Temple (the holy city) and 60 faith (+20 for each faith already founded); a world holds 3-8 faiths. 23 faiths: 12 living (Christianity, Islam, Hinduism, Buddhism, Judaism, Sikhism, Taoism, Confucianism, Shinto, Jainism, Zoroastrianism, Baha'i Faith) and 11 of the ancient world (Egyptian, Mesopotamian, Greco-Roman, Norse, Manichaeism, Mithraism, Tengrism, Aztec, Inca, Celtic Druidism, Canaanite), each with its real origin, followers today and a tenet from its practice, and a founding technology matching its age (ancient faiths Mysticism; most classical ones Philosophy; Islam Education; Sikhism Printing Press; Baha'i Industry). Cities press their majority faith on every city within ten tiles (holy city 8, others 4, Temple +2); a faith needs 60 pressure to hold a city. Missionaries (40 faith, +150 pressure) reach cities within 12 tiles of a following city. Tenets help every city that follows the faith; the founder gets +1 gold per two following cities and +1 content in its own following cities. Rivals adopt pantheons, found faiths and send missionaries. The city screen shows faith, faith followed and influence. Sources: Pew Research Center (2015, 2025) and general histories; the mechanic follows Civilization V/VI and Old World.
- **Checked (31 Oct 2026, by Claude):** engine harness passes (all sections, including new religion, missionary, border-influence, merged-territory, settler-cost and version 21 to 22 checks, and the 50-turn rival campaigns); UI harness passes; Unity RealmTools.Checks passes; a Windows build runs the `-autoShot` sequence with no exceptions, and screenshots show the Religion screen, the Religion button, full city name plates and the city screen faith line. Not hand-played: the menu option, greyed worker buttons, border swings over many turns, rival religion behaviour and balance (rivals also pay three times as much for Settlers, so expansion and Stage 8 pacing will be slower; tune after playing).

## Owner requests 31 Oct, follow-up (no save change)
- **Technology zoom:** Ctrl + wheel now redraws the cards, text and buttons at the zoomed size (zooming around the pointer) instead of scaling the picture, which Unity clipped short inside the scroll view; cards now fill the whole tree area at any zoom and are cut only at its edge. The line above the tree is shorter and clipped to the panel. Checked in a 1920 x 1080 Windows build with automatic screenshots at 60% and 140% zoom (`AutoShot` 4b2, 4b3); not hand-played.

## Owner requests 31 Oct, second batch (save version 21)
- **Controls:** the left button selects; the right button directs (right-click a tile, or hold and drag to it). The Move button still works. City plates are small and see-through (name, size, flag, a thin production bar; what is being built shows on hover) sit just below the city tile, drawn after the land, and any unit overlapping a plate is drawn again on top, so plates never hide units. Unit and city card text shrinks to fit instead of being cut off. Continue is first in the game menu.
- **Diplomacy:** one leader at a time. The Diplomacy button opens a chooser of met leaders (mood and waiting proposals shown); the talk that follows has no switching; first contact and the leader portraits open the one-on-one talk directly. A leader answers each proposal in words (agree or refuse) in the greeting box (`World.LeaderReply`).
- **Technologies x5 (180):** the 36 keystones keep indices 0-35 and the era rule (4 of 6 keystones); 144 minor technologies (24 per era, `MinorTechnologies.cs`) each need one keystone of their era (some also a minor) and give a small lasting effect: +1 food/shields/trade on a terrain or resource, +% science/gold/production (each capped at +40%), +1 content (capped at +3), +1 attack/defence for a unit class (capped at +3). Minors cost about 40% of a keystone; rivals rate them at three times their cost so eras keep their pace. The Research screen shows one era at a time; a later era's tab opens once a known technology leads into it; Ctrl + wheel zooms the tree. Old saves gain the new technologies unknown.
- **Cities:** a city works tiles within 2, one ring more at size 10, 20 and 30 (then no more); territory follows; the city screen's land view widens to match. Where reaches overlap, the larger (then older) city works the tile; locking it from the other city takes it over. Cities grow past 20 with a Sewer system: to 30 with Electricity and 40 with Computing.
- **Rules:** clearing woodland needs Stone Tools (6 worker-turns), 4 with Bronze Working, 3 with Iron Working. The first Settler sees 4 tiles until it moves; a unit on a hill sees one tile further except past an adjacent hill or mountain (`SightRange`, `CanSee`).
- **Workers:** Auto develop (More panel) hands a worker to the same planner the rivals use; R arms road mode, then a right-drag builds a road along the cheapest route (one tile at a time), R again builds on the spot (`Workers.cs`).
- **World:** rare ancient ruins on open land (about one per 320 tiles), explored once by the first land unit for gold, research, a local map or veteran status (`SeedRuins`, `ExploreRuin`). Rivers are twice as common, rise on hills or mountains (or beside them), flow steepest-downhill and end in the sea, a lake or another river; one trapped in a hollow ends in a new lake.
- **Confirmed by the owner (31 Oct 2026):** "every 10 levels" means city size (2 at the start, 5 from size 30).
- **Checked (31 Oct 2026, by Claude):** the UI harness and the engine harness pass; Unity RealmTools.Checks passes in batch mode; a Windows build runs its `-autoShot` sequence with no exceptions in Player.log, and the screenshots show the era research tabs, the smaller plates below cities with units on top, and the widened city land view. Not hand-played: right-drag orders, road mode, auto develop, ruins, overlap takeover, Ctrl+wheel tech zoom and the leader replies are compiled and covered by checks only where noted in ContentChecks.

## Owner requests 31 Oct (save version 20)
- **Diplomacy:** the agreement type, its length and both resource choices are drop-down boxes; Discuss opens full width with no scroll bars (the mouse wheel scrolls up and down); the leader is framed at the art's true shape.
- **Leader portraits were stretched everywhere:** Unity imports the 300 x 400 pictures padded to power-of-two sizes (256 x 512), so code that kept the texture's shape drew them tall and thin. All portraits now go through `DrawPortrait` at 3:4 (the `.meta` files are untouched); the diplomacy backdrop likewise (`DrawCropped`).
- **Map:** city plates are taller so the production line is no longer cut off, and carry the owner's flag; borders have a wider bright line with a dark inner rim, and owned land a faint wash of the owner's colour; the unit card states the unit's activity (and time left) on its own line; the tile card names tribal villages (and who they joined), trails and forts.
- **Founding:** "Are you sure you want to settle here?", then a name box offering the next name (Enter or Found city). Every city in the world has its own name: each premade civilisation has its own list of twelve, then a shared pool (`CityNames.cs`); repeats and blanks are refused.
- **City and unit on one tile:** clicking it asks whether you mean the city or which unit (Esc or a click elsewhere closes it).
- **Rules:** in the Dawn Age buildings (not units, wonders or projects) cost twice as much; a road through woodland is a trail and gives combat units no road speed; land units on auto-explore keep to land, head for the nearest land beside undiscovered ground and skip targets they cannot reach. Arrow keys on staggered maps move straight north, south, east and west. Map orders no longer refuse destinations without a direct land route, so the head-for-the-coast fallback works from the map.
- **World:** mountains follow plate tectonics: 8-14 plates turning about their own axes; converging boundaries lift long ranges (fading over three tiles), diverging ones sink into rifts; hills fringe the ranges (`TectonicUplift`). Rocky shores only where high ground meets the sea or on some cold coasts. Resources follow real distribution: iron and gold in hills and volcanic ground (more beside ranges), placer gold on rivers, rare gold in desert shields, horses on steppe, grassland and savanna, wheat on temperate grassland and steppe (twice as likely beside fresh water), spices in the tropics (`PlaceLandResources`).
- **Flags (save version 20):** 14 layouts, 16 emblems and a 16-colour palette for the two fields and the emblem, with a live preview and a random button on the Identity screen; rivals get generated flags; flags fly on army banners, city plates and the diplomacy screen (`Flags.cs`, `FlagUI.cs`). Older saves migrate (the old Leaf, Star or Sun becomes the emblem).
- **Classic view on staggered maps:** half-height tiles in offset rows, with picking, scrolling, labels, plates, order lines and terrain blending following the rows on screen.
- **Leader attire by age (owner approved 360 credits):** 30 new portraits made with SpriteCook (Nano Banana 2.1, 12 credits each) from the owner's 15 originals as references: an ancient look (`leader-NN-e0.png`, Dawn and Classical ages) and a modern look (`leader-NN-e3.png`, Industrial age onwards); the Medieval age keeps the original portraits. `Portrait()` now falls back to the nearest earlier picture in the same group. Full-size pictures and a review sheet are in `Documentation/Leader-Era-Originals`.
- **Checked:** see the end of this batch's report.

## Owner requests 30 Oct evening (save version 19)
- **Clearing woodland:** Forest, Jungle, Arctic forest and Mangrove must be cleared before a farm or mine (new worker job 5, 4 worker-turns, needs Bronze Working for axes). They become Grassland, Grassland, Arctic tundra and Bog. Roads can still go through woodland. On a wooded tile the worker's Farm button reads Clear; rival workers clear tiles they want to farm (`InfrastructureAI`).
- **Esc** lets go of the selected unit (and any pending Move); with nothing selected it opens the pause menu as before.
- **Technology screen:** hold the mouse wheel (middle button) and drag to pan around the tree.
- **View button:** hovering names the view it switches to beside the pointer.
- **Orders into the unknown:** the player's routes treat unexplored tiles as passable land (without peeking at the real terrain) and are planned again each step. A land unit sent to an unexplored or known water tile heads for the reachable land closest to it and stops at the coast with a note (`NearestLandToward`). Rival routing is unchanged.
- **Mountains:** a connected range keeps at most about sqrt(tiles)/4 peaks (8 on Tiny); the lower rest become hills or tundra (`CapMountainRanges`), so small maps are not walled off.
- **Fog of war as dark mist:** unexplored land under dark drifting banks (the cloud puffs, darkened), land seen before but not watched now under a thin drifting veil and a darker tint; the void beyond the map is near-black.
- **Diplomacy screen** in the layout of the owner's mockup (`Design/Mockups/diplomacy-mockup.png`): the partner's leader framed before a painted coast cut from the mockup (`Resources/Visuals/diplomacy-backdrop.png`), a card per met civilisation, name and mood badge, greeting, active treaties, Your offer / Their offer with steppers, Propose Agreement, Discuss (the full detail panel with every earlier action, the inbox and confirmations), Leave and Declare War (`DiplomacyScreen.cs`).
- **Checked:** EngineChecks (with new Content checks for clearing and the mountain cap) and DiplomacyUIChecks pass; RealmTools.Checks and GraphicsChecks pass in Unity; Windows build and `-autoShot` run with no exceptions (diplomacy, mist and map screenshots reviewed). **Not yet played by hand.**
- **Staggered rows (save version 19):** every new globe game uses `mapShape` 2. Rows run east-west and wrap, the poles are the top and bottom rows (no crossing over them), odd rows sit half a tile east, so the map pans freely in every direction like Civilization V/VI instead of showing a diagonal band.
  - The rules did not change: they still move on the same 8-neighbour grid, now reached through `World.Step(x,y,di,dj)` (grid position i = x + (y + y%2)/2, j = (y - y%2)/2 - x). Every neighbour lookup in the game (about 45: movement, routes, borders, vision, territory, rivers, coasts, ports, naval AI, settling, generation) goes through it. `Distance` is the grid distance with the east-west wrap.
  - Map sizes keep their tile counts: W columns by 2W rows (Tiny 24 x 48 ... Massive 160 x 320), a 2:1 picture. The climate generator shifts odd rows' longitude by half a column.
  - Renderer: row-by-row drawing from the north, straight panning (right is east), diamond picking (`PixelArt.StaggerScreenOf/StaggerPickOf`), the minimap and atlas in half-height brick rows.
  - Older saves (shapes 0 and 1) keep their diagonal layout and play exactly as before.
  - **Not done:** the classic square view is not drawn for staggered maps (the view button explains this; older maps keep it). Flat maps (the New Game "flat" shape) still use the diagonal layout.
  - Harness: staggered globes are W x 2W and end at the polar rows; steps are reversible and neighbours one move apart; distance is symmetric and wraps; screen/pick round trips at every zoom; grid steps point the same way as in the diagonal view.

## Every art slot filled from SpriteCook (30 Oct 2026, later; no save change)
- **Sprites added** (SpriteCook Pixel Engine Image, 56 images, 336 credits on the owner's Starter plan): all 32 settlement slots (`city-0..3-0..3` with and without `-walled`; the three medieval pictures from the first batch were re-fitted the same way), farmsteads `hamlet-0..3`, cloud puffs `cloud-0..3`, ships `Visuals/unit-12..14` (Boat, Galley, War Galley; 128 x 128, reduced to the game's 64 x 64 unit sprite at load), terrain scenery `terrain-09/11/13/15/16/18/19/20`, grassland groves `terrain-00-0..3` (open meadow variants 4-7 stay drawn) and extra variants `terrain-01-1/2` and `terrain-05-1/2`.
- **Fitting:** most pictures came with their own small ground block. Each block's earth skirt was trimmed along its own lower edges and the picture shifted so the block's tip row sits on the tile centre row (44 for settlements, 40 for everything else). Leftover backgrounds (sunlight wedges, sun discs, a stray dot) were removed. Nothing was redrawn by hand. Savanna, glacier, steppe and similar scenery therefore show a painted patch of their own ground over the drawn ground.
- **Fix:** cloud puffs were looked up as `cloud-0` whatever their variant, because the variant is passed in the other argument; `cloud-1..3` now load (`IsoRenderer.IsoPicture`).
- **Checked:** RealmTools.Checks and GraphicsChecks (114 graphics assets) pass in Unity; the Windows build succeeds; the `-autoShot` run has no exceptions. Screenshots and a sheet of every map sprite over its tile are in `Documentation/Unity-Screenshots-2026-10-30-Sprites`: cloud banks, savanna, jungle, mangrove, mountain and forest variants, groves, farmsteads, the Galley and the castle city all draw in place. The DiplomacyUIChecks harness passes. **Not yet played by hand.** Hills have no slot and stay drawn; painted ground patches can show faint diamond edges on savanna and steppe.

## First painted sprites from SpriteCook (30 Oct 2026, no save change)
- **Sprites added** to `Assets/Resources/Sprites` (SpriteCook Pixel Engine Image, 64 x 64, transparent, 6 images, 36 of the owner's 40 free credits): `city-3-0` (medieval town), `city-3-1-walled` (walled town with blue spires), `city-3-3-walled` (castle city with blue spires; the same picture is also saved as `city-3-2-walled`), `terrain-05` (snowy peaks), `terrain-01` (conifer and autumn forest) and `port` (jetty, crane and warehouse).
- **Clean-up:** leftover background patches were removed from the castle city and the port, and the earth skirts under the town bases were trimmed below the tile diamond. Nothing was redrawn by hand.
- **Fix:** `ClearIso` (run on every zoom change) destroyed every cached map picture, including supplied sprites that the slot cache still held, so supplied art would vanish after the first zoom. It now leaves supplied art alone (`IsoRenderer.cs`).
- **Still drawn art (no sprite yet):** cities of the Stone, Bronze and Classical ages, medieval cities of size 4-11 without walls, 8 and over without walls and 1-3 with walls, other terrain scenery, farmsteads, clouds and ships. Each terrain has one picture, so long mountain ranges and forests repeat it.
- **Checked:** RealmTools.Checks and GraphicsChecks pass in Unity; the Windows build succeeds; the `-autoShot` run has no exceptions. Screenshots in `Documentation/Unity-Screenshots-2026-10-30-Sprites` show the castle city, port, peaks and forests on their tiles after zoom changes. The DiplomacyUIChecks harness (compiles all scripts) passes. **Not yet played by hand.**

## Diagonal view and closer match to the owner's mockup (30 Oct 2026, no save change)
The owner asked for the interface and the game to look exactly like the mockup, chose the mockup's diagonal view, and chose SpriteCook for painted art. SpriteCook was not installed when this was done, so the painted art is still to come. Everything else is in place, including slots the art drops into.
- **Diagonal (isometric) map:** each grid tile is drawn as a 64 x 32 diamond. Moving east goes down-right on screen and moving south goes down-left (`PixelArt.ScreenOf`/`PickOf`).
  - The ground is still composed on a square grid (now 48 x 48, `PixelArt.TW/TH`) with all its organic blending, and reshaped pixel for pixel into the diamond (`PixelArt.ToDiamond`). Farms, roads, borders, the selection marker and unexplored ground are reshaped the same way.
  - Tiles are drawn back to front by diagonal, so scenery overlaps correctly. Panning, picking, the minimap's view outline and the city screen's land view all follow the diagonal.
  - This replaces the horizontal view the owner asked for on 29 Oct.
- **Fog of war as cloud banks:** unexplored land lies under overlapping white cloud puffs (`PixelArt.CloudPuff`) that spill a little over the edge of the known world, as at the mockup's edges. Drifting cloud shadows are now drawn beneath them.
- **Mockup details:**
  - Armies carry a banner in their owner's colour.
  - Leader portraits show a mood face with the opinion score in green, gold or red.
  - The unit card gains **Move**: click it, then click a destination; right-click cancels.
  - The bottom status strip and Next unit button are gone, as in the mockup. The current message now leads the notification cards, and Tab still selects the next unit.
  - Unit name labels are no longer drawn on the diagonal map; stacks keep their count badge.
- **Art slots for painted sprites** (`Design/ART-SLOTS.md`): pictures in `Assets/Resources/Sprites` replace the drawn ones by name, one slot at a time. The slots are terrain scenery, settlements by age, size and walls, farmsteads, the port and cloud puffs; ships use the existing `Visuals/unit-NN`. The guide gives canvas sizes and the tile centre so generated art lines up.
- **Harness:** the projection check now tests the diagonal round trip and half-diamond steps, and that ground becomes an opaque diamond with clear corners. The northern-coast check measures ten positions, because the wavy coastline can lie wholly in the sea tile at some positions. The Unity stand-ins gained `Vector3` fields, `Color` addition and `Mathf.Abs`.
- **Checked:** all harnesses, RealmTools.Checks and GraphicsChecks pass; Windows build and screenshot run with no exceptions; screenshots of the diagonal map, fog, harbour and city screen reviewed. **Not yet played by hand.**
- **Not exact yet:** the mockup's painted castles, towns, ships, trees and mountains need painted sprites (SpriteCook or the owner). The drawn art is a stand-in. The mockup's Culture and Stability figures are not game mechanics.

## Interface in the style of the owner's mockup (30 Oct 2026, no save change)
- **Theme:** dark navy panels framed in gold with a dark inner line, cream text, and teal for progress bars and the active button. It replaces the light theme everywhere (menus, city, research, diplomacy and every other screen) through the shared skin.
- **Lettering:** titles, panel headings, city names and the End Turn button use **Cinzel** (`Assets/Resources/Fonts/Cinzel.ttf`, SIL Open Font Licence, with `Cinzel-OFL.txt`; downloaded from Google Fonts with the owner's approval). Body text stays sans-serif.
- **Map:** fills the whole 1280 x 800 screen (`HudTheme.MapView`). The widgets float over it, and clicks, dragging and the mouse wheel reach the map only where no widget is drawn.
- **Widgets** (`HudTheme.cs`):
  - **Top bar:** gold (with net income), science per turn, current age, order (share of your cities not in disorder), turn and date, help and menu buttons. The mockup's Culture and Stability are not mechanics in this game, so real figures are shown instead.
  - **Left rail:** Empire (Economy), Research, Diplomacy (red badge with waiting offers), Cities, Military, Realms (atlas and globe), Victory, Chronicle and Identity.
  - **Research card:** current technology, turns left and a progress bar; click to open Research.
  - **Leader portraits** of met civilisations (up to five) in their age's portrait, with a mood dot and how they feel about you. A red edge means war; click to open Diplomacy with them.
  - **Notification cards:** the newest messages from this turn and last; click for the Chronicle.
  - **Minimap:** the known world with the visible area outlined; click or drag to move. Zoom in, zoom out and view buttons sit beside it.
  - **End Turn:** a gold button with an Enter key cap, plus a Next unit button.
  - **Unit card:** picture, name, health bar, movement, strength, rank and activity. Action buttons with icons: Found, Road/Farm/Mine, Explore, Skip, Fortify or Sentry, and More, which opens every other order (the former side panel) above the card. For a city: settlement picture, size, yields, current production with progress, Open city and Next unit.
  - **Tile card** (bottom centre): terrain, road, farm, mine and known resource with food, shield and trade yields. Holding Alt shows the full tile details tooltip.
  - **City plates:** framed plates with a crown for capitals, the city name and size; your cities also show production and turns left with a progress bar.
- **Icons:** 30 gold-and-enamel pixel icons drawn by `PixelArt.HudIcon`.
- **Fix:** first contact now opens Diplomacy from `Update` rather than inside the GUI pass. Before, the screen could change between Unity's layout and repaint passes and raise a GUILayout exception.
- **Harness:** `DiplomacyUIChecks` now checks the rail's Diplomacy badge instead of the removed header button. The Unity stand-ins gained `Font`, `GUIStyle.font/clipping/padding/CalcSize` and the styled scroll view.
- **Checked:** all harnesses, RealmTools.Checks and GraphicsChecks pass; Windows build and screenshot run with no exceptions; screenshots of the map, city, research, diplomacy and New Game screens reviewed. **Not yet played by hand.**

## Stage 8: AI and balance (30 Oct 2026, no save change)
Measured with headless rival games in the .NET harness (the player founds a capital and then passes; 4 civilisations; standard speed).
- **Why rivals won early:** science, not cheap technology. The design's costs (20 x e^2 + 5 x e x i) were already in, but older rules multiplied science: the University gave +50% after every flat bonus, and each research pact added 2 science in every city. A city with 10 trade made about 52 science by turn 120.
- **Changes:**
  - University: +3 science, flat, as the design states ("buildings give flat bonuses").
  - Research pacts add their science once per civilisation, at the capital.
  - Research cost: the design cost times a per-era pace (`World.ResearchPace`): 150, 190, 280, 400, 420 and 420 percent for Dawn to Dimensional. Speed settings still scale on top.
- **Pacing measured:** nine games (Tiny, Small and Medium, three seeds each), averages over every rival.

  | Milestone | Target turn | Before | Now |
  |---|---|---|---|
  | Era 1 complete | 40 | ~31 | ~36 |
  | Era 2 complete | 80 | ~58 | ~77 |
  | Era 3 complete | 125 | ~80 | ~124 |
  | Era 4 complete | 170 | ~100 | ~170 |
  | Earliest Transcendence | ~230 | 138-155 | 222-250 |

  Era 5 is not completed before Transcendence: rivals go for the project once Computing is known.
- **Turn time:** pathfinding was about 95% of a slow turn. Routes now use A* with an admissible estimate (the tile distance times the cheapest step), so they are still the cheapest. Occupied tiles are collected once per search. Rival land units give up at once on goals they cannot reach over land, and do not chase detours longer than three times the direct cost. Your own units keep the full search.

  | Map size | Before (average / slowest) | Now (average / slowest) |
  |---|---|---|
  | Large | 207 ms / 0.76 s | 20 ms / 56 ms |
  | Huge | 38 ms / 0.47 s | 22 ms / 60 ms |
  | Massive | 246 ms / 1.4 s | 38 ms / 106 ms |

  Measured in .NET to turn 120, not in Unity; Unity drawing is separate.
- **Harness** (`EmpireChecks.StageEightChecks`): University flat +3; a pact adds 2 science only at the capital; research cost equals design cost times the era's pace; a full Small game in which no rival wins before turn 200 and every rival completes the first era between turns 25 and 60.
- **Still open:** combat and economy balance need real play; the pace values are tuned for a passive player and will want checking against a human game.

## Owner request: invasions by sea (29 Oct 2026, no save change)
- A rival at war with a civilisation whose city lies on other land (`NavalAI.InvasionTarget`: known city tile, on a landmass none of the rival's cities stand on, nearest a connected port) builds a Galley at a connected port.
- Landmasses ignore polar ice: a crossing over the pole is possible but is not treated as a road for an army.
- Spare troops (never a city's last defender), with no enemy to fight on their own land, march to the galley and board.
- The galley sails when two are aboard, or when no more are coming. It lands them on coastal land within 2 tiles of the target city, or attacks straight into the city from the deck when it is next to it. It then goes home for the next wave.
- Troops ashore follow the usual attack AI. A rival soldier at sea that is not on a galley steps ashore.
- Harness (`NavalAIChecks`, real turns): the overseas city is chosen as the target, the rival chooses to build a Galley, and the troops are carried across and land beside the enemy city.
- **Not seen in the simulations:** the generator puts every start on the largest landmass, so in 150-turn games the player could always be reached on foot and rivals attacked by land. Overseas targets appear only once someone settles another island, which those runs did not produce. The landing is covered by the harness scenario only.
- **Not done:** War Galleys do not escort invasion galleys; no shore bombardment.

## Owner request: naval AI (29 Oct 2026, no save change)
Rivals now use the sea (`NavalAI.cs`), with only what they know:
- **Research:** Sailing and Engineering are wanted once a city's borders reach the sea.
- **Ports:** coastal-reach cities put a port high on their list, and workers give the road tiles to an unlinked port a large bonus.
- **Boats:** one per civilisation, counting any being built, explores the seas.
- **War Galleys:** at war, one per linked port (two when enemy ships are seen). They hunt enemy ships within 8 tiles and otherwise return to guard their port.
- **Galleys:** when a settler has no site and nothing left to explore on foot, it waits on the shore and the civilisation builds one Galley. The settler boards; the galley sails to the nearest coastal land within 15 tiles that the rival has seen, is legal to settle and has room. The settler lands there and plans its city as usual.
- **Limits:** ships skip the general 14-unit cap, but not when the treasury is empty.
- **Land routes:** routes for land units no longer cross water (a unit boards a ship only when the ship is its destination), so armies on attack orders no longer get carried off by passing boats. A rival's land unit found at sea steps ashore.
- **Harness** (`EmpireChecks.NavalAIChecks`, played through real turns):
  - a rival ferries a settler from a full island to another island and founds a city there;
  - a rival War Galley attacks an enemy boat at war;
  - a rival worker builds the road that links its port.
- **Simulations** (6 games of 150 turns on Archipelago and Continents, 3 rivals): every rival built one boat; Galleys appeared where a ferry was needed; no land units left stranded at sea; state stayed valid. With war declared at turn 70, rivals built 5-12 War Galleys. These were measured with the .NET harness, not Unity play.
- Invasions by sea: see the section above.

## Owner requests: ships, ports, villages, hidden resources, camera, raised mountains (29 Oct 2026, save version 18)
- **Mountains:** raised peaks again (`PixelArt.MountainSprite`), drawn as clusters of 4-5 peaks in three variants.
  - Each peak has a jagged silhouette from ridged noise, a lit north-west face and a shaded south-east face split by a wandering crest, gullies, and snow reaching down the gullies.
  - The foot dithers into the ground, which stays rock and foothill relief without snow.
  - Volcanoes stay relief in the ground.
- **Camera:** ground tiles are 64 x 40 instead of 64 x 48. The ground is more foreshortened, so trees, mountains, towns and units stand taller against it: a more third-person view. The city screen's land view uses the same shape.
- **Hidden resources:** a resource is not shown, not named in tooltips and gives nothing until its owner knows the technology for it (`World.ResourceTech`). Fish, crabs, kelp and oysters are known from the start.

  | Resource | Technology |
  |---|---|
  | Wheat | Agriculture |
  | Iron | Iron Working |
  | Gold | Bronze Working (mining) |
  | Horses | Horseback Riding |
  | Spices | Currency |
  | Whales, pearls, seals, tuna, sponges | Sailing |
  | Offshore oil | Refining |

- **City names:** a name plate with the city's size sits under every settlement in both map views, in the owner's colour.
- **Tribal villages:**
  - The first visitor still gets a gift.
  - Any civilisation whose unit occupies a village gets the land within 4 tiles revealed, and the village's allegiance (`Tile.villageOwner`); a small flag in the owner's colour marks it.
  - An allied village pays 2 gold a turn while it lies inside its civilisation's borders or within 4 tiles of them.
  - Another civilisation can win it over by occupying it.
- **Boats, ships and ports** (`Naval.cs`):

  | Unit | Technology | Attack / Defence / Moves | Cost | Needs | Carries |
  |---|---|---|---|---|---|
  | Boat | Sailing | 0 / 1 / 6 | 20 | City beside the sea or a lake | 1 land unit; keeps to water next to land |
  | Galley | Engineering | 0 / 2 / 8 | 40 | Connected port | 3 land units |
  | War Galley | Engineering | 3 / 2 / 8 | 45 | Connected port | Fights ships |

  - **Port** (Engineering, 60 shields): needs the city's borders to reach the sea. It stands in the city if the city is on the coast, otherwise on the nearest coastal land tile of the city's territory within 3 tiles.
  - A port away from the city works only while a chain of road tiles joins them and no enemy unit at war stands on that road or on the port. The link is rechecked after every move and every turn, and the owner is told when it breaks or returns.
  - A connected port gives +2 gold a turn, +1 food on worked sea-resource tiles, and lets the city build Galleys and War Galleys, which launch beside the port.
  - Land units board a ship with room by moving onto it, travel with it and step off onto land. Units left at sea when their ship sinks are lost.
  - Ships move 2 points per water tile. Auto-explore works for ships.
  - The pixel view draws the ship on its tile rather than its cargo, a jetty with a crane and warehouse on the port tile, and a red "!" on your port while it is cut off.
- **Save version 18:** cities get no port and unconnected status; existing villages are unallied.
- **Harness:**
  - `EmpireChecks.NavalChecks`: boat and port requirements, port placement, road link, enemy and broken-road cut-off and return, launching, boarding, carrying, sinking with cargo, land units kept off open sea, lakeside boats.
  - Village checks: joining, revealing, tax in and out of reach, being won over.
  - The hidden-gold check in `StrategyChecks`.
  - The auto-screenshot run adds `6c-harbour`.
- **Checked:** all harnesses, RealmTools.Checks and GraphicsChecks pass; Windows build and screenshot run with no exceptions; screenshots reviewed. **Not yet played by hand.** Ships, ports and villages have not been tried in a real game.

## Owner request: cities that grow on the map (29 Oct 2026, no save change)
- A city is drawn from small buildings (`PixelArt.Settlement`) instead of one fixed picture. The supplied `Visuals/city-*.png` pictures stay in the project but are no longer drawn on the map, city screen or classic view.
- **Population:** 2 + size houses (up to 18), spreading wider as the city grows.
- **Age** (Stone Tools, Bronze, Iron, Feudalism, as before) sets the building style:
  - Stone Age: thatched round huts.
  - Bronze Age: mud-brick blocks.
  - Classical: whitewashed houses with terracotta roofs.
  - Medieval onwards: timber houses with red and blue roofs and chimney smoke.
- **Walls** appear only once the Walls building exists. They are a ring with a gate and towers, in the age's material: wooden palisade with watchtowers, mud brick, dressed stone, then castle stone with blue spires.
- **Seat of power** for the capital from size 4, and for any walled city from size 8: a chief's longhouse, a stepped mud-brick palace, a columned palace, then a castle keep with towers.
- **Buildings shown once built:**
  - Temple: standing stones, ziggurat, columned temple, then church with spire.
  - Market: striped stalls.
  - Granary: silo, then barn.
  - Library: domed hall.
- **Farmsteads:** from size 5, one neighbouring land tile of the city's own territory per two citizens (up to 8) gets two or three houses, in a fixed order per city. Not on forest, jungle, mangrove, volcano, mountains, water or ice. Pixel view only; the classic textures view shows only the city tile.
- Harness: `FoundationChecks.PixelChecks` confirms that settlements grow with size, gain walls and a castle only when flagged, and that farmsteads draw in every age. The auto-screenshot run adds `6b-grown-city`.
- Checked: town preview across all ages and sizes, all harnesses, RealmTools.Checks and GraphicsChecks, Windows build and screenshots with no exceptions. Not yet played by hand.

## Owner request: look like the gallery kingdom maps (29 Oct 2026, no save change)
Compared against the gallery's coastal kingdom, fantasy realm and enchanted forest maps:
- **Water:** brighter royal blue with more small white wave marks; turquoise shallows along every shore.
- **Coasts:** mostly grey rock with a cliff face where the sea lies to the south and a rocky rim elsewhere; sandy beaches in places and along deserts and savanna. The "rocky coastline" terrain is green turf with pale rock breaking through instead of bare grey stone; its boulders are paler and fewer.
- **Grass and trees:** richer yellow-green grass. Forests are denser, mixing dark conifers and round broadleaf trees with a little autumn colour, and spread further so neighbouring forest tiles join up. Open grassland has small groves and lone trees: eight variants placed differently so no pattern repeats, and none on farms, roads or cities.
- **Farms:** fenced plots of golden wheat beside a green crop plot, instead of stripes.
- **Borders:** a glowing line in the owner's colour that wanders gently, with a soft halo, instead of a thin straight line.
- **Start sites (sphere worlds):** the start clearing is round and only softens harsh ground (desert, tundra, bog, wasteland, jungle, rocky coast, mangrove), so capitals no longer sit in a square of plain grass. This changes newly generated worlds only.
- Checked: pixel previews, all harnesses, RealmTools.Checks and GraphicsChecks pass, Windows build and auto-screenshot run with no exceptions. Not yet played by hand.
- Not attempted: buildings, towers, ships, bridges and waterfalls on the map as in the gallery pictures (those need new art), and drifting clouds at the map edge.

## Owner request: organic mountains and volcanoes (29 Oct 2026, no save change)
- **Mountains** are no longer a peak sprite standing on the tile. They are relief in the ground, like hills. The height is a massif centred on each mountain tile plus ridged noise (1 - |2n - 1|, the usual technique for sharp crests and valleys), sampled on rotated axes so no grid shows. The relief fades in from the range's edge, is hill-shaded and is coloured by height: brown foothills, grey rock, then snow above a wandering snow line, blue-grey in shadow.
- **Volcanoes** are a cone in the ground with concave flanks, a summit crater of glowing lava, radial gullies, two lava streams that cool from orange to dark red, dark basalt, and a lighter south flank facing the viewer.
- **Hills:** stronger relief (shading x4.8 instead of x3.4), brighter crests and darker hollows.
- Fixed: the relief light came from the south-east, which made raised ground read as hollows; it now comes from the north-west as intended.
- Checked: pixel preview of a test patch, all harnesses pass, RealmTools.Checks and GraphicsChecks pass, Windows build and auto-screenshot run with no exceptions. Not yet played by hand.

## Owner requests, 29 Oct 2026 late (save version 17; in the 0.27 folder)
Implemented in source and checked by the .NET harness; see "Verification" at the end of this section for what ran in Unity.
- **Civilisations:** Crystal Dominion and Sylvan Court are set aside (still in the catalogue for old saves, never offered or given to rivals). Five new civilisations: Northmarch Kingdom (King Halvard; Seafaring, Industrious), Sunspire Emirate (Emir Rashid; Mercantile, Scholarly), Lionmarch Kingdom (King Aldous; Spiritual, Militaristic), Jade Dynasty (Empress Mei; Monumental, Scholarly) and Serpent League (High King Itzel; Agrarian, Militaristic). The first three use unused portraits from the owner's set. **Jade Dynasty and Serpent League have no portrait yet** and stay hidden until `Assets/Resources/Leaders/leader-15.png` and `leader-16.png` exist (they switch on by themselves). 13 civilisations are offered now; no two share a trait pair.
- **First contact** opens the Diplomacy screen by itself. Diplomacy shows both leaders side by side, with a greeting from the other leader that depends on relations (war, alliance, friendship, first meeting).
- **Leader clothing by age:** the loader looks for `Leaders/leader-NN-eE.png` (E = era 0 Dawn to 5 Dimensional) and falls back to the single portrait. **No era portraits exist yet**; they need new art (stone age hides and furs, classical robes, and so on).
- **Land types** on the New Game screen: Continents, One landmass, Archipelago (78% sea), Small continents, Inland seas (more land; small seas become lakes). Flat maps support them too.
- **Realistic biome neighbours** (Whittaker temperature and moisture classes): after generation, any land tile two temperature steps hotter than a neighbour is cooled one step, and dry ground beside wet ground is moistened, repeated until settled. No hot desert or jungle touches tundra or ice; no desert touches jungle or bog. Water, rivers, mountains and rocky coast are neutral.
- **Poles:** ice sheet and glacier can now be crossed, but each step takes a unit's whole move, units standing on ice lose 1 hit point a turn instead of healing, and nothing can be founded or improved there.
- **Ten sea resources** (placed by realistic zone: warm reefs, cold coasts, open ocean, lakes): Fish, Whales, Pearls, Crabs, Kelp, Oysters, Seals, Tuna, Sponges, Offshore oil (+3 shields once Refining is known). Each has its own pixel icon, shows on the map and in the city view, and the tile tooltip lists its yield.
- **Terrain art:** hills are drawn as shaded relief on their own biome ground instead of a separate sprite, so they sit in the grassland; desert has no cacti.
- **Orders:** dragging from a unit draws a dotted line to the cursor (both views). An order to a tile that cannot be entered (another civilisation's unit or border at peace, a full stack) goes to the nearest reachable tile beside it; a path blocked only by units waits and resumes; an order ends only when there is no way at all.
- **Auto-explore** now searches outward over known land for the nearest tile beside unexplored ground and follows a proper route there, so it no longer sticks in dead ends; it ends when nothing reachable is left.
- **Classic textures view** pans east and west for ever on sphere worlds.
- **Globe zoom:** on a sphere, once Mysticism (which includes astronomy) is known, zooming out past the smallest map size shows the known world as a globe; drag spins it, a click or zooming in returns to the map there.
- **Save version 17:** no data changes; the version marks the new resources, land types and passable poles.
- Harness additions: civilisation catalogue (17 entries, offered set, set-aside and art-less civilisations never used, art switching one on), first-contact queue and greetings, passable poles (cost, attrition, no founding), every land type valid with no biome clashes, sea resources only on water with yields, astronomy alias, order rerouting round a wall, beside an occupied tile and through a temporary jam, auto-explore coverage.
- Fixed while testing: once ice became passable, the coastline pass treated sea ice as land and turned polar ice into rocky coast (Small sphere 53% land instead of about 38%). World generation now uses `Barren` (water, peaks or ice) where it used `IsImpassable`.
- **Verification (29 Oct 2026, Unity 6000.3.25f1 batch mode):** all five .NET harnesses pass; RealmTools.Checks and GraphicsChecks pass; Windows build succeeded; the auto-screenshot run finished with no exceptions. Screenshots reviewed: New Game land types, first-contact Diplomacy (both portraits and greeting), city land view with resources, revealed map with sea-resource icons, globe zoom, classic view across the wrap line. The "Inland seas" label shortening came after that build and was only harness-compiled. Not yet played by hand: drag line, rerouting and auto-explore were checked by the harness only. Known limits: no portraits for Jade Dynasty and Serpent League, no era portraits, the classic view has a texture seam at the wrap line on large maps, and archipelago worlds may keep civilisations apart until ships.

## Owner requests, 29 Oct 2026 evening (save version 16; in the 0.27 folder)
- **Organic, level terrain.** Land and sea are on one level again everywhere, rocky coast included (the bank faces are gone). Each pixel looks up the terrain at its own position pushed sideways by smooth world-space noise (domain warping, the "noise-driven transition zone" approach used for tile-map terrain transitions), so coasts and biome borders meander across tile edges instead of following the grid; textures are sampled in world coordinates so they run seamlessly from tile to tile; near a border the two terrains dither into each other; shores get a wet-sand (or rock, or ice) lip, foam that moves with the water animation, and a band of lighter shallows. Rivers bend in an S between tile centre and edge and still join. Soft cloud shadows drift slowly across the map. Known limit: on sphere worlds the texture has a seam along the east-west wrap line.
- **Research screen:** every technology that can be researched now has a Research button on its card with the turns it will take; locked technologies only offer Inspect, and the details panel shows no research button for them.
- **City screen:** the build lists show only units and buildings the civilisation's known technology allows and that are not already built (completed wonders and finished Transcendence stages disappear); other limits such as size or resources still show as reasons. The land view is now a continuous slice of the pixel-art map (blended ground, scenery, roads, the city), with land beyond the city's reach dimmed; worked tiles carry small pips (green food, orange shields, gold trade), locked tiles a blue marker, hover shows a soft outline and a one-line description, click locks or releases.
- **Auto-explore** is available for every combat unit as well as Scouts; it stops when an enemy comes near.
- **12 civilisations** (`Civilisations.cs`), each with a leader portrait and two of twelve traits; every trait belongs to exactly two civilisations and no two share a pair (the two-traits-per-leader model of classic 4X games). Azure Crown (Seafaring, Mercantile), Oasis Sultanate (Mercantile, Agrarian), Sylvan Court (Agrarian, Spiritual), Holy Imperium (Spiritual, Monumental), Sahel Empire (Monumental, Diplomatic), Hellenic League (Diplomatic, Scholarly), Crystal Dominion (Scholarly, Stalwart), Frostpeak Realm (Stalwart, Industrious), Aurelian Republic (Industrious, Expansionist), Lutetian Empire (Militaristic, Horse Lords), Rising Isles (Militaristic, Seafaring), Steppe Horde (Horse Lords, Expansionist). Names, leaders and pairings are my proposal.
- **Traits:** Seafaring +1 food on worked water; Mercantile +2 gold per city; Agrarian +1 food per city; Spiritual +1 content per city; Monumental wonders and Transcendence stages a fifth cheaper; Diplomatic Concord in 15 turns; Scholarly +1 science per city; Stalwart +1 defence inside your borders; Industrious +1 production per city; Militaristic new military units are Veterans; Horse Lords Horsemen and Knights +1 attack; Expansionist Settlers a third cheaper.
- **Custom civilisation:** name, leader name, any of the 15 portraits, and any two different traits from the twelve.
- **New Game screen:** page 1 chooses the civilisation (12 portrait cards and Custom, with leader and traits, or the custom fields); page 2 is the world and game settings with a summary. Rivals take premade civilisations the player did not choose (seeded); beyond twelve they get stock names and two seeded traits. The Diplomacy and Identity screens show leader portraits, leaders and traits.
- **Leader art:** the owner's 15 portraits (`Paradigm-Realm-16bit-Leaders.zip`) are used as the characters: 300 x 400 copies in `Assets/Resources/Leaders/leader-00..14.png`, full-size originals untouched in `Documentation/Leader-Originals`. Generated variants (mirrored, recoloured robes) were tried and rejected: the recolour bled into skin, gold trim and sea. New characters need new art.
- **Save version 16:** the old single trait (Builders, Scholars, Traders) becomes Industrious, Scholarly or Mercantile for the player; existing rivals keep their names and get no traits, so a game in progress does not change.
- Harness: `FoundationChecks.PixelChecks` rewritten (level, opaque, deterministic tiles; borders blend in and wander; coasts shape only their side; cloud sheet), `EmpireChecks.CivilisationChecks` (catalogue structure, premade and custom set-up, rivals without repeats and with named cities, all twelve trait rules, version 15 conversion, tech-filtered build lists, auto-explore for combat units).

## Stage 7: interface (save version 15; source written 29 Oct 2026, in the 0.27 folder)
- **City screen in three panels**, as in the design. Left: the 5 x 5 tile ring the city can work (it used to grow to 9 x 9 and show tiles that could never be worked); each tile shows Food/Shields/Trade and whether it is the city, locked, worked or free. Middle: rename, Food/Shields/Trade totals with the science and gold split, a growth bar with the growth report, a content/unhappy bar with a disorder warning, citizens and specialists, and the list of buildings. Right: the build queue, with a Units / Buildings switch; the top of each line lists the next three items with cost and the turns until each is done.
- **Governor** along the bottom: Balanced, Growth (food), Build (shields), Science, Commerce (gold). These are the existing city focus settings; Commerce is kept alongside the design's four.
- **Tile locks:** click a tile to lock a citizen on it, click again to release. Locked tiles are always worked, even over the Governor's choice and even when the city is short of food; the Governor fills the remaining citizens. Locks are capped at the number of working citizens. Saved per city.
- **Specialists:** citizens with no tile left to work become entertainers automatically each turn. One click turns an entertainer into a scientist or a tax collector (the old "merchant", +3 gold) and back, and one more makes a worker an entertainer. The separate Citizens screen still exists.
- **Deviation from the design:** the design describes a single build queue of up to three items. The game keeps its two production lines (units and buildings, each getting full production) and its 8-item queues, which were earlier owner features; the screen shows three items per line.
- **New Game screen:** two columns. "Your civilisation": name, trait (with what each does) and game speed (with what it changes), seed. "Your world": shape, size (with tile count), land type and number of civilisations (you and N rivals, with the size's limit). A one-line summary above the start button.
- **Save version 15:** cities gain a list of locked tiles; older saves start with none.
- Harness: `EmpireChecks.StageSevenCity` (lock over the Governor, lock cap, both locks kept over a better tile, release, ring limit, lock kept when hungry, automatic entertainers, one-click conversions and refusal, version 14 conversion, invalid lock rejected); the UI program checks the three panels are drawn. All engine suites and API-double UI harnesses pass outside Unity.

## Stage 6: victory (save version 14; source written 29 Oct 2026, in the 0.27 folder)
- **Conquest:** hold every rival capital, or eliminate every rival. A capital is the first city a civilisation founds (saved per civilisation). Rivals can win the same way: one holding every other capital, yours included, ends the game in defeat. Eliminating everyone still wins.
- **Transcendence:** three projects in the building line, stage 1/2/3 at 300/400/500 shields, needing Computing and a city of size 8 or more. Stages go in order in one city; each activates over 10 turns; the third activation wins. Losing the project city (capture, or any loss of control) resets the whole project. Projects cannot be bought with gold and cost no upkeep. Rivals you have met trigger a warning when they complete a stage.
- **Researching the Transcendence technology no longer wins.** It now halves activation to 5 turns (my proposal, so the keystone still does something).
- **Concord** unchanged (20 turns of friendship or alliance with every surviving rival).
- **Victory screen:** three bars. Conquest counts rivals conquered; Transcendence shows stage, shields or activation turns left, plus the leading rival's status; Concord as before.
- **AI** builds Transcendence first in its project city, or its most productive city of size 8 or more, and only there.
- **Save version 14:** older saves record each civilisation's lowest-numbered city it still holds as its capital, and start with an empty project. A save that already knew the Transcendence technology does not win on load.
- **Fix found while testing:** an AI choosing an edict after the game had ended (for example a conquest earlier in the same turn) would have retried forever. It now stops.
- **Measured pacing (outside Unity, 4 seeds, 300 turns):** a rival still wins by Transcendence at turns 140-153, because rivals reach Computing around turn 95-100 (in the design it is a Modern-age technology, roughly turn 170+) and their best cities reach size 20 and build each stage in about 5 turns. The rules match the design; the speed of research and production is the Stage 8 balance task.
- Harness: `EmpireChecks.StageSixVictory` (capitals recorded and fixed, conquest by capitals with rivals still alive, rival conquest defeat, conquest by elimination, project costs and no upkeep, Computing and size gates, stage order, no rush, 10-turn activation, same-city rule, bar progress, loss on losing the city, 5-turn activation with the technology, three-stage win, rival defeat, AI choice, version 13 conversion, three bars) and the edict retry guard in `ContentChecks`. All engine suites and API-double UI harnesses pass outside Unity; Engine Checks pass inside Unity.

## Stage 5: technology compression, eras and edicts (save version 13; source written 29 Oct 2026, in the 0.27 folder)
- **36 keystone technologies in 6 eras** (`TechnologyCatalog.cs`). Dawn: Stone Tools (known at start), Agriculture, Archery, Bronze Working, The Wheel, Writing. Classical: Currency, Code of Laws, Mathematics, Mysticism, Sailing, Philosophy. Medieval: Engineering, Feudalism, Education, Banking, Printing Press, Military Science. Industrial: Industry, Electricity, Rifling, Chemistry, Medicine, Flight. Modern: Computing, Satellites, Nuclear Fission, Robotics, Genetic Engineering, Globalisation. Dimensional: Nanotechnology, Quantum Computing, Longevity Medicine, Fusion Power, Mind Uploading, Transcendence. All eight design milestones are keystones. The list and grouping are my proposal.
- **Every one of the 124 earlier technologies is folded into exactly one keystone** (for example Fire, Hunting and Cordage into Stone Tools; Steam and Railways into Industry; Calendar and Astronomy into Mysticism). Earlier ids still resolve, so every existing unlock and effect keeps working. A keystone's card lists the effects and original design unlocks of everything folded into it. The earlier tree is kept in `LegacyTechnologyCatalog.cs` only to convert old saves.
- **Costs** follow the design: 20 x era^2 + 5 x era x position (Dawn 25-50 up to Dimensional 750-900), times the game-speed multiplier, and a fifth cheaper once two civilisations you have met know it.
- **Settlers, Warriors and Scouts are available from turn 1** (Stone Tools), as in the design's turn-1 description. Settlers used to need Agriculture.
- **Eras and edicts** (`Edicts.cs`): knowing 4 of an era's 6 keystones completes it and offers 1 of 3 permanent edicts. The design names none; mine (proposal): Dawn: Common Granaries (+1 food/city), Stone Masons (+1 production), Sky Watchers (+1 science). Classical: Coinage (+2 gold), Civic Festivals (+1 content), Academies (science rate up to 80%, the design's post-edict cap). Medieval: Standing Army (+1 defence), Guild Charters (+2 production), Royal Charters (+2 science). Industrial: Rail Network (+3 production), Free Press (+1 content, +1 science), Conscription (new units are Veterans). Modern: Global Markets (+4 gold), Research Grants (+4 science), Welfare State (+2 content). Dimensional: Planetary Grid (+5 production), Open Knowledge (+5 science), Harmony (+3 content). The player chooses on the Research screen (a note announces each new age); the AI chooses content when unhappy, gold in deficit, otherwise science.
- **Bridging rule retired:** the +2 Trade per citizen from Stage 3 is gone; the city centre gives 3 base Trade instead of 2. Measured outside Unity (4 seeds, AI civilisations, 170 turns): Dawn complete around turn 40 (target 40), Classical 59 (target 80), Medieval 82 (125), Industrial 99 (170). Rivals research Transcendence, which currently wins at once, around turns 135-146 (with the old bridging rule: turns 93-96; design target about 230). Stage 6 (Transcendence as a three-stage project) will push this back; the remaining speed-up is Stage 8 tuning.
- **Save version 13.** Version 12 saves convert: a keystone is known if any technology folded into it was known (plus its prerequisites), progress on unknown keystones is the sum of their folded technologies' progress (kept short of completion), research moves to the matching keystone, and every civilisation gets empty edict slots to fill for eras already complete. Saves from version 1 upward still migrate step by step (the version 4 to 5 step now reads the earlier catalogue).
- Harness: `ContentChecks` rewritten (36 keystones in 6 x 6, all 124 earlier ids folded and resolving, milestones, cost formula and range, no prerequisite from a later era, turn-1 units, the one-fifth discount, era completion at 4 of 6, edict choice, permanence and effect, Academies 80% cap, AI edicts, version 12 and version 4 conversion, all earlier content checks). All engine suites and API-double UI harnesses pass outside Unity.

## Unity verification, 29 Oct 2026 (run by Claude on Matt's PC with Unity 6000.3.25f1 in batch mode, on the 0.24 copy)
- The whole project compiles in Unity with no errors (everything through Stage 4 and the new look).
- **Run Engine Checks** passes inside Unity, including Unity JsonUtility save round trip and migration up to save version 12.
- **Run Graphics Checks** passes: 114 art assets.
- A Windows player was built (`RealmTools.BuildWindowsBatch`) and run with `-autoShot`, which starts seed 421, founds the capital and saves screenshots. It ran with no exceptions in the player log. Screenshots are in `Documentation/Unity-Screenshots-2026-10-29`: horizontal map with level water, processed units on shadows and team rings, city screen and economy screen in the light theme.
- Fixed after the first screenshots: unit name labels covered the units' feet, shadow and ring (units now stand higher; health shows above the head only when hurt), and scrollbars and sliders were still Unity's dark default (now light).
- Not verified: interactive play (clicking, dragging, ending turns, save/load through the menus), performance on large maps, and combat balance in real wars. Matt's own editor project (`Desktop/Paradigm-Realm-main/Unity`) was not touched and does not contain this work yet.
- Dev aids added: `AutoShot.cs` (runs only with the `-autoShot <folder>` argument; writes BMP files because the project has no screenshot or image-encoding modules) and `RealmTools.BuildWindowsBatch` (batch build with `-buildPath`).

## Stage 4: combat rules (source written 29 Oct 2026; compiled and engine-checked in Unity, not yet played)
- Units have separate Attack / Defence (health 10). Design table: Spearman 1/2, Horseman 3/1, Archer 2/1. Scaled to match (my proposal): Warrior 1/1, Scout 1/1, Knight 4/2, Rifleman 5/4, Catapult 4/1; non-combat units defend at 1.
- Damage each side deals = max(1, 3 + own strength - enemy strength), both at once; ranged units (Archer, Catapult) take no retaliation, whether firing or attacking in melee. One exchange per attack, no randomness.
- Defence bonuses are added points: mountains and volcanic +3, hills +2, forest/jungle/arctic forest/mangrove +1, river +1, fortified +1, fort +1, walls +2; Spearman +2 against Knights and Horsemen. Rank adds to both values. Unpaid upkeep costs 1 point. (Old: percentage-like Power 0-13 with +2/+3 bonuses.)
- Only the best defender on a tile fights (highest defence, then health), and only it can die. Ranged fire now also hits the best defender on the target tile. Stack limit 3 military units per tile (unchanged).
- Horsemen ignore the forest slowdown. The exact preview ("You deal 2 (enemy health 10 to 8), you take 4 ...") uses the same function as the attack, and is shown in the map tooltip and under each Fire button.
- AI builds by Attack + Defence. Diplomatic military strength uses Attack + Defence.
- No new saved fields; save version stays 12.
- Harness: `EmpireChecks.StageFourCombat` (formula, design table, each defence bonus, the Spearman > Horseman > Archer > Spearman triangle, preview equals result, best defender only, no retaliation for ranged, walls, Horseman forest cost, stack limit). Pacing unchanged at 100 turns, but no AI rivals went to war in the six test games, so combat balance is untested by play.

## Look: horizontal map, level water, defined units, light interface (source written 29 Oct 2026; compiled and screenshotted in a Unity build, not yet played)
Owner request (29 Oct 2026): the map must run horizontally, not diagonally; water level with land with no step except rocky coastline; more defined units with a 3D look that stays 16-bit; a more ergonomic, smoother, lighter interface.
- **Map:** the diamond view is now a horizontal three-quarter view. Tiles are 64 x 48 rectangles in level rows (five zoom steps), drawn back row to front row so trees, hills, mountains and cities stand up over the row behind. Picking, panning, wrap-around on sphere worlds, selection and labels follow the new grid. The file is still `IsoRenderer.cs` so Unity references stay intact. The V key still switches to the classic textured view, which was already horizontal.
- **Water and banks (revised the same day at the owner's request: "3D look, horizontal rows"):** land stands 8 px above water. Wherever water lies in front (south) of land, a bank face shows below the raised surface: soil under grass, sand under desert, rock under rocky and mountain ground, ice under snow. Raised land shades the water at its foot, shores have shallows and irregular animated foam, west banks catch the light and east banks darken. Units, cities, roads, borders and selection sit on the raised surface; clicks pick the raised land first. (An earlier version that day had water level with land and only rocky coastline raised; the owner asked for the 3D look back.)
- **Units:** the supplied unit pictures are not changed. As the game loads, a copy of each is read through the GPU (no import-setting change), reduced to a crisp 64 x 64 sprite, given a top-left rim light, bottom-right shade and dark outline, and drawn 1:1 at the default zoom on a ground shadow inside a ring in the owner's colour, with a health bar below. If processing fails the original picture is used.
- **Interface:** light theme: blue-grey background, near-white panels and buttons with rounded corners and a soft shine, dark slate text, blue accent on hover and press; lighter modal dimming, light tooltips, labels and badges; softer map fog colour. Hard-coded dark backgrounds behind text were changed so nothing is dark-on-dark.
- Screenshots from a Unity build are in `Documentation/Unity-Screenshots-2026-10-29` (5-world shows the whole revealed map with banks; 6-coast-close a polar ice bank). `Documentation/Unit-Sprites-Preview.png` compares units before (top) and after (bottom).
- Harness: horizontal pick round trip at every zoom, level rows, land raised and water level, a bank face on every land type with water to the south and none with land to the south, water sitting Elev lower in its composite, unit sprite size, outline and footing. UI stand-ins extended (RenderTexture, Graphics.Blit, ReadPixels, Color.Lerp, Rect.center).

## Stage 3 slice 5: healing, victories and ranks (save version 12; source written 29 Oct 2026; NOT yet compiled or run in Unity)
Stage 3 of the 16-bit plan is now complete in source. None of it has run in Unity.
- Healing per turn, for units that did not move or are fortified: 1 in the open, 2 fortified, 3 in your own city, full with a Barracks in that city (design). Hospital (+2 in its city) and an adjacent Medic (+2) are kept on top. The old +1 for being inside your borders is gone.
- Ranks are earned automatically from victories: 2 make a Veteran (+1 strength), 5 an Elite (+2). A victory is killing the enemy unit in melee (attacking or defending) or with a ranged attack. The manual Promote button and experience points are retired (the field stays in saves, unused). The old system allowed three promotions (+3); Elite (+2) is now the top.
- Barracks and the War Academy wonder used to give new units 5 experience, worth exactly one promotion; they now train Veterans. A village "experience" reward now gives a victory.
- Save version 12 adds `victories`. Version 11 saves: ranks above Elite cap at Elite, unspent experience of 5 or more becomes Veteran, and victories are set to match the rank.
- UI: unit panel shows "Rank Veteran | Victories 3 (next rank at 5)"; map tooltip shows the rank beside strength; Barracks, War Academy and village texts updated.
- Measured outside Unity (6 seeds, three civilisations, passive test player): pacing unchanged from slice 4 (100 turns: 35.3 technologies, 11.3 cities, average size 12.6). Few units earn ranks in these runs because there is little fighting; balance of ranks needs real games.
- Harness: `EmpireChecks.StageThreeUnits` (each healing rate, Hospital, Medic, Barracks full heal and Veteran training, rank thresholds, top rank, a melee kill counts, v11 migration, invalid ranks rejected); the Foundation promotion check now uses victories. All engine suites and API-double UI harnesses pass outside Unity.

## Stage 3 slice 4: content and unhappy citizens, disorder (source written 29 Oct 2026; NOT yet compiled or run in Unity)
- Working citizens (population minus specialists) are content up to the city's content slots; the rest are unhappy. Slots = 4 + happiness buildings and other bonuses + 2 per entertainer. A city is in **disorder** when unhappy outnumber content: no production, no gold (including Market and road-connection income) and no growth. Science continues; the design only names production and tax.
- The existing modifiers become content slots at their old values: Temple +2, Gardens +2, Colosseum +3, Sun Gardens wonder +2, Monarchy +1, luxury +2 per level, Longevity +2, connected Spices +2; war weariness, revolution, Democracy's overseas wars, occupation -3 and unpaid upkeep -1 remove slots. Tax is now +/-1 slot (60% science is low tax, 30% or less is high tax), as in the design (was +/-2 happiness).
- The old score (5 - population / 2 + bonuses) is gone. `Happiness(c)` now returns content minus unhappy, so every existing "below zero" check (AI entertainers, AI luxury, growth) means disorder. Disorder used to halve production and science; it now stops production and gold.
- UI: city header and city list show "Content X / Unhappy Y" and DISORDER; the growth report puts disorder first. Building, government, luxury, entertainer and war-weariness text says "content citizens" instead of "happiness".
- No new saved fields; save version stays 11.
- Measured outside Unity (6 seeds, three civilisations), against 0.23: 50 turns 13.3 technologies, 11.3 cities, average size 5.6 (0.23: 12.2, 11.2, 4.5); 100 turns 35.3 technologies, 11.3 cities, average size 12.6, no city in disorder (0.23: 33.1, 11.3, 10.9). Gold keeps building up: about 1,889 per civilisation at turn 100 against 612.
- Harness: `EmpireChecks.StageThreeHappiness` (4 base slots, content up to slots, ties are not disorder, disorder stops production, gold and Market income, growth report, entertainer +2 slots, Temple +2 slots, specialists neither content nor unhappy); tax and war-weariness checks now compare content slots. All engine suites and API-double UI harnesses pass outside Unity. Nothing has run in Unity.

## Stage 3 slice 3: Trade and the science rate (save version 11; source written 29 Oct 2026; NOT yet compiled or run in Unity)
- Tiles now yield Food / Production / **Trade** (the third yield used to be gold from gold deposits only). Trade: Ocean, Lake and Coral reef 2; River, Rocky coastline and Mangrove 1; other land 0. A road adds 1 on open flat land (Grassland, Desert, Savanna, Dry steppe; the design's "plains" map onto Savanna and Steppe). A gold deposit adds 2.
- City centre: its own terrain Trade, +1 for the automatic road on open land, plus 2 base Trade (my addition, so every city makes some science and gold).
- **Bridging rule, not in the design:** each citizen adds 2 Trade (`World.CitizenTrade`). Measured outside Unity (6 seeds x 50 turns, three civilisations), design tile Trade alone gave 1.7 technologies and 3 cities per game against 12.2 and 11.2 under the old rules: grassland cities made 1 science, nobody reached Agriculture, so nobody trained Settlers. With 1 Trade per citizen it was 6.6 and 5.3; with 2 it is 13.3 and 11.3. Remove or reduce this once Stage 5 compresses the technology tree to the design's cheaper costs.
- Science rate: 0-60% in steps of 10 (Economy screen, Science -10% / +10%), default 50%. Each city's Trade x rate / 100, rounded down, becomes science and the rest becomes gold, so nothing is lost. The 80% cap after an edict waits for edicts (Stage 5). The old per-population tax income is gone.
- 60% science counts as Low tax, 30% or less as High tax, otherwise Normal (now +1 / -1 content slot; see slice 4).
- Library is now +2 science (was +4) and Market +2 gold (was +3), as in the design. University, Bank and Factory keep their old effects.
- AI moves its science rate between 30% (in deficit) and 60% (comfortable surplus).
- Save version 11 adds `scienceRate`. Version 10 saves migrate Low / Normal / High tax to 60% / 50% / 30%. Earlier versions migrate through 10 as before.
- Measured pacing gap still open: average gold after 50 turns is about 318 per civilisation, against 106 before (partly the cheaper upkeep from slice 2). Expect to tune upkeep or the default rate after playing.
- UI: city screen tiles show F / P / T; the city header and economy report show Trade, Science and Gold; map tooltips show Trade and road Trade. Harness: `EmpireChecks.StageThreeTrade` (terrain and road Trade, centre Trade, bridging rule, rate clamping, the split at every rate, Library, Market, happiness mapping, v10 migration, invalid rates rejected). All engine suites and API-double UI harnesses pass outside Unity. Nothing has run in Unity.

## Stage 3 slice 2: production costs, Settler population and upkeep (source written 29 Oct 2026; NOT yet compiled or run in Unity)
- Unit costs follow CORE-DESIGN-16BIT.md: Warrior 10 (was 18), Worker 20, Spearman 20 (was 24), Archer 20 (was 28), Horseman 25 (was 36), Catapult 40 (was 45), Settler 30 (was 36). Scout, Knight, Rifleman, Medic and Caravan are not in the design table and keep their old costs. Game speed multipliers still apply.
- A Settler now needs city size 2 and removes 1 population when it is trained. The AI already waited for size 2.
- Buildings sit in tiers of 40, 60 and 80 shields. 40: Granary, Library, Walls, Barracks, Gardens, Temple, Harbour. 60: Workshop, Market, Courthouse, Hospital, Aqueduct, Colosseum. 80: University, Bank, Factory, Sewer system. The tier choices are a proposal for Matt to confirm.
- Wonders cost 100 + 50 x era. Until the technology compression (Stage 5) the 14 catalogue eras are folded onto 6 (`1 + era x 6 / 14`), giving Grand Archive 150 (was 150), Sun Gardens 150 (was 140), Merchant Exchange 200 (was 180) and War Academy 250 (was 170).
- Upkeep: each city supports 3 units free, plus the government's existing support; every further unit costs 1 gold (per-unit upkeep of 1-4 is gone). 40-tier buildings and wonders are free; 60 and 80 tier buildings cost 1 gold each (previously every building cost 1, and the Library 1 more). City upkeep (1 + population / 4 + distance) is not in the design and is unchanged.
- No new saved fields, so the save version stays 10. Projects already in progress keep their stored shields and may finish sooner at the lower costs.
- Harness: `EmpireChecks.StageThreeEconomy` (each cost, tiers, wonder formula, Settler size gate and population cost, free unit support, building upkeep). All engine suites and the API-double UI harnesses pass outside Unity. The UI harness stand-ins (`Tests/UnityStubs.cs`) were extended for calls added by the globe, diamond view and victory screen. Nothing has run in Unity.
- Still to align in Stage 3 after slice 3: the happiness model (content and unhappy citizens, disorder), and healing and experience.

## Victory screen, Concord victory and first rules alignment (source written 29 Oct 2026; NOT yet compiled or run in Unity)
- `Victory.cs` / `VictoryUI.cs`: new "Victory" button in the header opens one screen with three bars (Conquest, Transcendence, Concord). Concord is a new win: friendship or alliance with every surviving rival for 20 consecutive turns (a lapsed treaty or an unmet rival resets it). The count is a new saved field `concordTurns` (defaults to 0, so older saves load unchanged).
- Rules alignment from CORE-DESIGN-16BIT.md, first two items only: growth now needs 10 + 4 x population food (was 12 + 6 x population), and buying costs 2 gold per remaining shield, doubled when nothing has been built yet (was 3 per shield). The tax split, upkeep, happiness, experience and cost tables are NOT yet aligned; Stage 3 is partly done.
- These two changes make cities grow faster and buying cheaper once production has started, so early pacing will shift; expect to tune in play.
- Harness: `VictoryChecks` (concord counting and reset, three bars, growth formula) and the rush price check in `EmpireChecks` were updated. Nothing has run in Unity.

## Stage 2: variable civilisation count (source written 29 Oct 2026; NOT yet compiled or run in Unity)
- New games can have 2 to 16 civilisations (slider on the New Game screen). Limit per map size: Tiny 4, Small 6, Medium 8, Large 12, Huge 14, Massive 16. Three civilisations is still the default and keeps the old start positions on flat maps.
- Save version 10 adds `civCount`. All per-civilisation arrays (`nations`, `met`, `war`, `openBorders`, war weariness, diplomacy pairs) are sized from it. Diplomacy pairs are stored in (0,1),(0,2)... order and found by index. The single `rivalWar` flag is replaced by a per-pair `atWar`, so rivals can be at war with some civilisations and at peace with others. Version 9 saves migrate (the old flag is copied onto the rival pair).
- Start sites: sphere worlds pick one site per civilisation on the largest landmass with spacing that shrinks as the count grows; flat maps use farthest-point spacing for counts other than three.
- Diplomacy AI runs for every rival: contact between any two civilisations, proposals, war and peace. The diplomacy screen lists met civilisations in rows of four. Sixteen map colours and fifteen rival names exist.
- Harness: `CivChecks` (array sizes, pair lookup, distinct starts, six turns stay valid, clamping, v9 migration). Only the logic is checked there; nothing has run in Unity. Turn time with many civilisations on large maps is unmeasured.

## Diamond view, procedural pixel art and globe (source written 29 Oct 2026; NOT yet compiled or run in Unity)
- `PixelArt.cs`: procedural 64x32 diamond tiles with 8 px land elevation, cliff skirts, ramp-grouped neighbour blending, animated water, scenery sprites, farms, roads and overlays. Deterministic; no image files needed.
- `IsoRenderer.cs`: diamond renderer (back-to-front), camera in lattice coordinates that wraps east-west on sphere worlds, picking (raised surface first), five zoom steps, selection/destination/stack-label overlay, per-frame paint budget and texture cache.
- The old square renderer is kept; press V or use the "View" button to switch. Diamond is the default.
- WASD / middle-drag pan, wheel and Zoom buttons change diamond zoom. The Known World screen shows a rotating globe on sphere worlds (drag to spin; click land to centre the map).
- Art pass 2 (29 Oct 2026): redone toward the gallery look: saturated deep-blue animated water with ripple dashes and thick shoreline foam, mottled grass with flowers, dense forests of tiered pines and round trees, jagged snow-capped mountains, rocky cliff faces. Resource badges are now drawn after all tiles so they are no longer half covered. Art pass 3: much denser forests (about 24 trees per tile), taller jagged rocky coast with pebbly ground, larger mountains.
- Harness: `PixelChecks` (size, determinism, non-empty, pick round trip). These only prove the maths; nothing here has been compiled or looked at in Unity. Art quality and performance are unverified.

## Performance pass 1 (source written 28 Oct 2026; NOT yet measured in Unity)

Cause found by reading the code (no profiler data yet): every frame the map drew each tile three times, rebuilt a text key and an array for every tile's terrain picture, asked a slow visibility question per tile, and painted any new tile's 64 x 64 picture pixel by pixel in one go. City yields scanned the whole map. Changes:
- Map drawing: pictures are drawn on Repaint events only; the player's visibility is computed once per frame; each tile's remembered view, unit stack and city are looked up once, not per pass; roads use a few strips instead of hundreds of small squares.
- Terrain pictures: flat ground with the same type all round now shares one picture per type (pixel-identical to before). Other tiles are cached with a number key (no text). New tile pictures are built within a 6 ms per-frame budget; tiles still waiting show the plain supplied texture for a moment, then switch. Tiles with no river nearby skip the river search.
- Flicker fix (owner report): the plain-texture placeholder looked different from the finished tile, so tiles flickered and seemed to rearrange. Each tile now keeps showing its previous picture while a new one is built; a tile never drawn before waits as flat ground of its own type (the same picture most finished tiles use). Not yet confirmed in Unity.
- Rules code: city workable tiles, adjacent water, resource checks, territory rebuild and the settler frontier search look only near the cities instead of scanning every tile. Pathfinding uses a heap and reusable arrays (same routes as before). Technology lookups use a dictionary.
- Not done: the Known World screen, AI turn processing and autosave have not been profiled. The 64 x 64 art size is unchanged. The new diamond renderer will replace much of the drawing code.
- Verification: `SpeedChecks()` compares territory, workable tiles, adjacent water and the visibility cache with the plain definitions they replaced, checks routes are valid and repeatable on flat and sphere maps, and times a Massive sphere. It was NOT compiled or run here. No frame times have been measured; Unity feedback is needed.

## Sphere worlds and climate generator (source written 28 Oct 2026; NOT yet tested in Unity)

- Save version is now 9. New field `mapShape`: 0 = flat map with edges (every older save, and the default for `World.New`), 1 = sphere. Version 8 saves load unchanged as flat maps.
- Sphere maps are 2:1: Tiny 48x24, Small 72x36, Medium 96x48, Large 144x72, Huge 192x96, Massive 320x160. The New Game screen has a Flat / Sphere picker (Sphere is the default there).
- On a sphere, columns wrap east-west and leaving the top or bottom row re-enters the same row half a world away. Movement, distance, fog, borders, the settlement adviser and ranged attacks all use the wrapped geometry. `World.Distance` is now an instance method.
- `WorldClimate.cs` generates sphere worlds from 3D noise, temperature and moisture bands, a Whittaker table, rivers that run downhill, lakes, coral, polar ice and coasts. All three civilisations start on the largest landmass.
- Not done: the on-screen map still draws as a flat rectangle and does not scroll seamlessly across the wrap (the diamond renderer will); no globe view; new procedural art.
- Verification: `SphereChecks()` in FoundationChecks.cs covers wrap and pole crossing, distance, walking across the seam and a pole, fog equivalence, shared landmass, land share, biome coherence and v8 migration. It was NOT compiled or run here (no C# compiler in the session). The generator algorithm was only prototyped in Python. Unity results are needed.

## Stage 1: map sizes and save deletion (source written 28 Oct 2026; NOT yet tested in Unity)

- Map size is chosen on the New Game screen: Tiny 32x20, Small 48x30 (default, same as before), Medium 80x50, Large 120x75, Huge 180x110, Massive 320x200. Save version is now 8; version 7 saves load as Small.
- Rivals and starts scale with the map but there are still exactly three civilisations on every size. More civilisations are Stage 2.
- Only on-screen tiles are drawn, fog of war updates only around units and cities, and the terrain-art snapshot refreshes only the visible window on maps over 4,000 tiles. The Known World screen scales to the map; maps over 4,000 tiles draw it as one small texture. There is no separate minimap overlay yet.
- The Load Game screen has a Delete button with a confirmation for every save, including autosaves. Deleting also removes the .bak copy.
- Autosave on maps over 9,000 tiles runs every fifth turn because saves are large.
- Pathfinding is still a brute-force search; its speed on Large, Huge and Massive maps is unmeasured.
- Verification: the pure C# checks were extended (all map sizes, fog equivalence, v7 migration, delete) but could NOT be compiled or run in the session that wrote them; only bracket balance was checked. Compile and Play Mode results from Unity are needed.

See [Diplomacy implementation](DIPLOMACY-STAGE-3.md) and [Play instructions](../START-HERE-DIPLOMACY.md).

Implemented: bilateral relationships for all three civilisation pairs; negotiated timed borders, friendship, defensive alliances, research, trade, peace and border promises; joint war; gold/resource contracts; grievances, denunciation, embargoes and war weariness; AI offers/counteroffers; inbox, acceptance previews, save v7 migration and regression checks.

Not implemented: world congress, espionage, city-states, vassalage, diplomatic victory, city transfers, advanced war objectives or naval systems. Unity playtesting remains outstanding. Existing artwork and terrain are preserved.

## Previous stage retained below

# Settlement strategy — Stage 2

This stage adds gameplay to the existing terrain/city update. Artwork and terrain
rendering are unchanged. No ships or naval gameplay are added.

## What the player can do

Select a Settler and choose **Compare settlement sites**. The adviser lists up to
five reachable, safe suggestions within 12 tiles. It explains estimated output
at three citizens, nearby known resources, early population limit, city upkeep,
travel distance and how many surrounding tiles remain unexplored. Hovering a tile
with a Settler selected also shows settlement benefits or a specific blocker.

**Travel to...** issues ordinary movement orders and returns to the map. It never
founds a player city automatically. Found the city with B or the Found city button.

## Rules and trade-offs

- River city centre: 3 food / 2 production, instead of 2 / 2.
- Hill city centre: 2 food / 3 production, instead of 2 / 2.
- A lake or river on or adjacent to the centre gives an early population limit
  of 8; other sites have 6. Ocean and reef water do not count as fresh water.
- Aqueducts set the cap to 12 and sewers to 20; water bonuses do not stack on top.
- Existing food consumption, happiness, specialists, tile ownership, resource
  connection requirements and upkeep still apply. Nearby resources are potential
  access; they do not become connected automatically when a city is founded.
- Existing cities also receive the derived site bonuses. No existing population,
  terrain, researched technology or orders are deleted, and no city cap is reduced.

Decision: choose food and population capacity, productive terrain, resource access
and defensibility while considering travel and distance upkeep. Better sites may
require longer travel, exploration and roads. A higher cap still requires food and
happiness. These are site-dependent advantages, not free policy bonuses.

## AI and information boundaries

AI settlers score known food, production, resources, fresh water, upkeep and
travel. They keep valid routes, avoid visible military threats and competing
settler destinations, and explore a local frontier when no suitable site is known.
Settler training requires population 2+, positive food, Agriculture and a treasury
reserve, and retains the current four-city target. AI uses the same founding,
movement, production and city bonus rules as the player.

Player suggestions use visible or remembered terrain and ownership, not hidden
live changes. Rival nations have no per-tile memory structures in save v6, so
this planner conservatively uses only their currently visible tiles. The planner
tries the best 24 scored candidates for reachability; it is a bounded heuristic,
not a guarantee of the globally best site. Resource and output estimates omit
future improvements, buildings, specialists and policy modifiers.

## Compatibility and tests

Save schema remains version 6; this stage adds no serialized fields. Existing
movement orders and saves round-trip through the supplementary test harness.
Foundation, Content, Empire, Strategy and new Settlement checks pass under .NET.
Coverage includes site yield/cap rules, cold founding, hidden geography/resources/
foreign cities, sustainable site ranking, stable ties, threats, treasury constraints
and an AI founding a fresh-water colony. Existing campaign simulations also pass.

API-double UI tests cover adviser opening, displayed explanations, travel without
automatic founding, no state mutation by recommendations, and saved travel orders.
Existing UI and renderer harnesses pass. Actual Unity compilation, JsonUtility,
Play Mode layout and frame-time checks remain unverified in this environment.

Next gameplay priorities: richer land-combat decisions and AI, diplomatic
consequences and objectives, and additional victory progress/counterplay.

---

The previous-stage audit and implementation notes follow for history. Current
rules above supersede the earlier description of the base 6/12/20 growth caps.

# Terrain and city strategy — stage 1

## Audit of the supplied graphics-update project

The project has working code for movement orders, fog, cities, separate unit and
building production, improvements, resource networks, research, economy,
diplomacy, trade, combat, AI, conquest/science victory and save version 6.
Foundation, Content and Empire test suites exercise those systems. The supplied
project had no AGENTS.md or Design directory; its START-HERE instructions and
artwork notes were read. Unity target: 6000.3.25f1.

The renderer used straight rectangular crossfades, sampled river tiles without
connections and repeatedly queried world visibility during texture generation.
The city allocator ranked base yields, ignored improvements unlocked by research
while choosing tiles and could select low-food production tiles during famine.
Growth blockers were poorly explained, food could accumulate without a bound at
the cap, and AI buildings were selected in catalog order. AI research selected
the cheapest available technology and workers heavily favoured roads.

The technology catalog has 124 entries but many future unlocks remain design text.
Ships, aircraft, other realms, difficulty presets, additional victory routes and
advanced diplomacy have not been implemented in this stage.

## Implemented: terrain

- Pure C# TerrainPainter samples a shared world-space field. Edge and diagonal
  contributions aggregate repeated biomes correctly, including multi-biome corners.
- Warped boundaries, limited dithering, 5-bit colour channels and intermediate dry
  colours replace straight soft rectangular crossfades.
- Shared water fields shape land banks, shallow water, lake outlines and ocean coast.
- River tiles draw connected channels with rounded bends, junctions and mouths.
  Their movement costs, yields and gameplay terrain types are unchanged.
- TerrainPalette prepares periodic samples from existing textures, keeping their
  orientation and lighting rather than mirroring them. The ocean artwork supplies
  lake interiors; coast artwork is sampled from its land portion so baked shorelines
  do not appear twice. All original PNG files remain unchanged.
- Seeded variations remain stable between frames and save/load.
- A per-draw remembered-terrain snapshot prevents unseen live terrain from
  influencing edges. Unknown neighbours use neutral grass, never their hidden type.
- Cache keys contain seed, coordinates and the complete influencing 5x5 neighbourhood.
  The 2048-entry cache fits the fixed 1440-tile map with headroom for changed edges.
  Textures and source caches are released on world changes and destruction.
- Raised scenery, resources and existing connected roads remain. Unseen terrain
  receives a final opaque mask so raised sprites cannot protrude into unexplored land.
- Terrain Transition Review is an independent Editor diagnostic window with all
  21 terrains, islands, narrow land, bends, junctions and lake/ocean mouths. It uses
  the exact new rasteriser and can export PNG without editing the current world.

## Implemented: city strategy

Decision: choose a city focus, specialist mix and infrastructure appropriate to
local food, production, science and treasury needs.

Connections: population consumes food; specialists replace tile workers;
improvements and research affect usable tile yields; happiness and infrastructure
control growth; buildings incur the existing upkeep; production remains split
between the existing two concurrent production lines.

Trade-offs: automatic workers try to cover food needs before their focus, but
specialists can still cause a shortage. Science focus retains its existing +2
science / up-to-2-production cost. Commerce values gold deposits without a free
gold bonus. Growth focus sacrifices other output for food. Higher caps require
research, production and upkeep. No new free AI yields are added.

- EffectiveTileYield is now shared by allocation, city totals and tile labels.
  Irrigation, chemistry, steam and harbour effects are counted exactly once.
- Workers swap low-food tiles for higher-food alternatives only as necessary to
  cover consumption. Deterministic ties and existing shared-city tile ownership
  are retained. The allocator is a heuristic, not a global optimiser.
- Citizens screen explains food sources/consumption, workers, specialists and
  output. City screen names growth blockers and estimates growth/starvation turns.
- Growth, starvation and newly reached growth blocks have event messages.
- Blocked storage is bounded to the next growth threshold on a turn; granaries
  retain half that threshold after growth. Existing population is not reduced by
  a growth cap. Starvation still reduces population and normalises specialists.
- AI manages its own focus and specialists using the same yield rules, prioritises
  relevant growth/happiness/economic buildings and uses prerequisite-aware research
  scoring toward usable improvements and current city needs.
- AI workers score worked tiles, hunger, resource improvements, connected roads,
  distance, other workers' jobs and visible threats, using normal job restrictions.

## Save and art compatibility

Existing v6 worlds load without regeneration. No new serialized fields are needed;
focus 4 is Commerce. All existing asset bytes/GUIDs, Packages and ProjectSettings
are retained. Original units, cities, resources, theme and gallery remain available.
Back up saves: turn processing now changes stored food and worker assignments.

## Validation

See Documentation/Validation.txt. Actual Unity compilation and Play Mode have not
been run. CPU-rendered comparison uses the supplied artwork at 56 pixels/tile;
it does not include raised sprites or the game UI and is not a Unity screenshot.

## Remaining priorities, in order

1. Unity visual/performance acceptance: play through the diagnostic cases and
   actual campaign fog, inspect overlays and zoom at the user's screen resolution.
   IMGUI scaling remains fractional on some windows; point filtering avoids blur
   but this is not an integer-only pixel-perfect camera. Dense source art can still
   show repetition; shore and relief palettes need further artistic tuning.
2. Coherent new-world geography and stronger settlement-site scoring. Existing
   generated worlds remain unchanged; landform generation is still simple.
3. Extend city roles with site-dependent building choices and full production,
   science, happiness and treasury modifier breakdowns. Current breakdown is most
   detailed for food. Housing remains the existing 6/12/20 population cap system.
4. Land combat: tactical AI, defensive fronts and clearer strategic war objectives.
5. Diplomacy: persistent grievances, treaties and readable rival intentions.
6. Additional victory paths with progress and counterplay. Keep future technology
   unlocks explicitly planned until their complete gameplay and AI support exist.

No claims of full Civilisation VI-equivalent depth or finished balancing are made.
