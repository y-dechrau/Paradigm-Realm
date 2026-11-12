# Dimensions: gates, colonies and local knowledge (11 Nov 2026, save version 42)

Owner brief: adapt the game to exploring and colonising parallel dimensions through gates, with the home world still the centre of the campaign. This file is the plan mapped to the code, the decisions every stage shares, what was built, the balance values, and what is left. Later work should keep to the decisions here or change them here first.

## What already existed (inspected 11 Nov, not assumed)

| Need | Already in the game | Where |
|---|---|---|
| Other worlds | Six full-size worlds below the home rows, one map shown at a time | `Realms.cs` (`AddRealms`, `RealmTop`, `Tile.realm`), `RealmsUI.cs` |
| Gates | One linked pair per world, nine designs, found by exploring | `Tile.gateLink`, `GateTwin`, `GateStyle`, `gatesFound` |
| Travel | A land unit stepping onto an open gate comes out at its twin | `GatewayTravel`, `GateLanding` |
| Opening a gate | Read the seal with a Scout, Medic or Priest (2 turns), then Stabilise / Force / Negotiate; key technology, wonder and rite | `GateExpedition.cs`, `GateUI.cs` |
| Local peoples | Ten native AI peoples per world, excluded from conquest | `AddNatives`, `IsNativeNation` |
| World hazards | Attrition and plague by world | `RealmTurn` |
| City production | Two lines per city; buildings, wonders, Transcendence projects | `BuildingIds`, `BuildReason`, `CostFor`, `CompleteBuilding` |
| Research | 99 active technologies in six ages | `TechnologyCatalog`, `MinorTechnologies.cs` |
| Diplomacy | Opinion, trust, five standing pacts including open borders | `Diplomacy.cs`, `DiplomacyDeals.cs` |
| Time | Turns | everything below is in turns |

## Shared decisions

1. **The existing worlds are the dimensions.** They stay full size (the owner's brief says "comparable size"). Nothing was added or removed. Two physical **families** to begin with: **Ember** (Ashen Waste, Infernal Depths) and **Resonant** (Fae Wilds, Drowned Deep). The Plague Marshes and Hive Expanse keep home physics until the first two play well.
2. **A gate's condition is separate from what anyone knows about it.** Condition is one record per gate pair (`GateState`); knowledge is one record per civilisation per pair (`GateStudy`, which already existed).
3. **Conditions.** Naturally cycling, dormant, damaged, sealed, constructed (data only). New games: of the Ember gates one cycles and one is dormant; of the Resonant gates one is dormant and one damaged; home-physics gates stay sealed. Older saves: every gate sealed, exactly as before (`dimensionRules` false).
4. **No dice on the essential path.** Every step has a fixed cost and a shown result. Better technology makes the same action cheaper, more informative or larger; it never replaces it.
5. **Rules are tags, not unit lists.** A family lists capability tags that fail there (`combustion`, `electrical`). A unit declares the tag a *function* depends on, so only that function stops. Knowledge is never blocked.
6. **Beyond a gate a home civilisation founds colonies, not cities.** A colony is a point on the map with a handful of numbers. Native peoples still found cities in their own worlds. Cities that older saves already have beyond a gate stay cities.
7. **Transport is counted in loads.** A gate carries a number of loads a turn for each civilisation allowed to use it. One load = 3 supplies out, or one load of exports home, or one unit crossing.
8. **Nothing is destroyed by a closed gate.** Units wait where they are. A cut-off colony uses its stock, then lives on what it grows, and stays its owner's. Capturing a gate changes who may pass, nothing else.
9. **Benefits land in a named home city**, the colony's home city, through that city's ordinary yields. The only empire-wide effects are the two synthesis discoveries.
10. **Three layers of knowledge.** Universal (the existing tree, untouched). Local adaptation (per civilisation per family, earned on the spot). Synthesis (needs both families; paid for with half of the home cities' science while studied, so it competes with normal research). The main tree's 180 slots were not changed, because its arrays and old saves depend on that count.
11. **Rivals use the same functions as the player** and know of a gate only when their own map shows it. They put home first.
12. **Access is the existing open borders pact.** No sixth pact was added: the pact arrays, opinion weights and save checks all assume five. New exchanges are direct actions with stated prices.

## Stages and where each lives

| Stage | Built | Files |
|---|---|---|
| 1 Foundations | Gate states, families, tags, colony and adaptation records, save version 42 | `Dimensions.cs`; `World.UpgradeSave` (41 to 42), `Valid` |
| 2 Home-world gates | Cycling schedule; Investigate Gateway and Repair Gateway city projects; methods by technology; dormant activation; status and schedule text | `DimensionGates.cs`; hooks in `World.cs` (`BuildingIds` 65, 66, `BuildReason`, `CompleteBuilding`), `EmpireBalance.PlaceAllows`, `Civilisations.CostFor`, `CityCare.BuildingGold`, `Realms.GateOpen` / `GateSteady`, `GateExpedition.GateNeed` |
| 3 Travel and rules | Failing functions in `Strength`; compatibility report (unknown / confirmed / adapted); colony sites; camera kept on the home world | `Adaptation.cs` (`DimensionStrength`, `DimensionNote`), `DimensionGates.cs` (`CompatibilityReport`), `Colonies.cs` (`ColonySites`), `RealmsUI.ShowRealm` |
| 4 Colonies | Founding, footholds, specialisations, policies, six buildings, stock, self-sufficiency, shared gate capacity with three priorities, exports to the home city, isolation, capture | `Colonies.cs`; hooks in `World.Found`, `World.Yield`, `Realms.GatewayTravel`, `Realms.RealmTurn`, per-nation turn (`DimensionsTurn`) |
| 5 Research | Nine local discoveries per family with effects, funding, related-knowledge discount, family sharing, native peoples' own craft, two synthesis discoveries | `Adaptation.cs` |
| 6 Rivals and diplomacy | Rival investigation, gate errands, colony settlers, colony upkeep; claims, joint investigation, local lessons and supplies; passage by open borders | `DimensionAI.cs`; hooks in `SettlementStrategy` and `World.AI` |
| 7 Finish | Gates and colonies screen (five tabs), colonies on the map, gate panel report, one batched message a turn, checks, this file | `DimensionUI.cs`, `GateUI.cs`, `IsoRenderer.cs`, `DimensionChecks.cs` |

## The rules as built

**Gates**
- Cycling: open 4 turns in every 10, on a fixed offset per gate. Usable while open with no investigation. Stabilise makes it open for good.
- Dormant: read the seal, then a small activation (20 gold; 10 for the first gate of the game) through the existing approaches.
- Opening times (quadrupled 12 Nov at the owner's request): Stabilise 24 turns (12 for the first gate of the game), Negotiate 12. Forcing is still immediate; reading a seal still takes 2 turns.
- Damaged: read the seal, then a city within 12 tiles with Bronze Working runs Repair Gateway (90 production); it is then dormant.
- Sealed: the 10 Nov requirement by gate design.
- Investigation: a Scout, Medic or Priest for 2 turns as before, or a city within 12 tiles running Investigate Gateway (40 production, no gold). Method by technology: observation and trial (100%), scholarly study with Writing (80%), instruments with the Printing Press (60%, measures danger exactly), probes with Computing (40%, counts colony sites).
- Capacity: 6 loads a turn; +2 Engineering, +2 Industry, +4 Computing, +2 from the family's haulage discovery, +3 Gate Harmonics.
- Who may use a gate: it is open, the land under either end is not held by someone without open borders or at war with you, and no such unit stands on either end.

**Families**
- Ember: `combustion` fails. Riflemen lose their firearms (attack 5 to 2) and keep movement and defence.
- Resonant: `electrical` fails. No unit depends on it yet; it governs what colonies there can build.
- Existing world hazards (attrition, plague) are listed by the investigation as confirmed hazards.

**Colonies**
- Founded by a Settler (colony) or a Scout (foothold, which becomes a colony at 6 supplies). 3 tiles from any city or colony, not on a gate.
- Specialisation: extraction (+2 production per load in the home city), trade (+3 gold per load), research (+2 local points a turn), military support (heals, shelters from attrition, +2 defence). A second one once established (25 turns and 50% self-sufficient).
- Policy: exploitation (+1 export load, no growth in self-sufficiency), self-sufficiency (+2 a turn), cooperation (local peoples warm to you).
- Buildings, bought with gold from home: storehouse 30, workings 40, garden plots 30, palisade 35, study hall 45, local workshop 60 (Thermal Forge or Crystal Workshop, needs its discovery). Three until established, then five.
- Supply: needs (2 + level) a turn, scaled down by self-sufficiency, none at 100%. Stock 20 (+20 storehouse), starts at 10.
- Priorities: Supply first, Trade first, Military first (half of each gate kept for units). The screen previews all three per gate.
- Capture: an enemy soldier standing on an ungarrisoned colony wears it down and takes it. Losing the gate never does.

**Research**
- Points a turn per family: 1 per colony, +2 research specialisation, +1 study hall, +1 local workshop, +1 per funding level (2 gold each, up to 3). With no colony but a unit there: 1.
- Cost falls 10% for each related universal technology known, to half at most.
- Ember: Banked Embers, Vent Reading, Thermal Forging, Pressure Arms, Ashcloth, Geothermal Kilns, Pressure Haulage, Deep Tapping, Flameless Furnace.
- Resonant: Listening Stones, Sympathetic Chimes, Crystal Lathe, Tuned Mail, Harmonic Farming, Resonance Relay, Singing Blades, Crystal Engines, Far-Speaking Lattice.
- Synthesis: Flameless Foundry (+1 production in every home city with a Workshop), Gate Harmonics (+3 loads on every gate, cycling gates open 2 turns longer).

**Diplomacy**
- Passage and military transit: open borders (existing).
- Colonial claims: 15 influence; neither founds within 4 tiles of the other's colonies; +8 opinion each way; war ends it.
- Joint investigation: 20 gold to a civilisation that has read a seal you have not.
- Local peoples: lessons (30 gold, opinion 10, 8 points) and supplies (20 gold, 9 supplies, no gate needed).

## Added 12 Nov 2026: rivals compete, borders escort

- **Legends.** From turn 40 a home civilisation that knows of no gateway hears of the one nearest its capital, within 40 tiles. The player hears the same way.
- **Word travels.** A civilisation at peace with one that knows a gateway, and not disliked by it, hears where it lies: one gateway every six turns. The player hears the same way.
- **Rival errands** (`DimensionAI.cs`): a Scout walks to an unread gateway and reads it (`GateErrand`); the nearest spare soldier stands on a read gateway so the work can begin and finish; a soldier goes through to garrison an ungarrisoned colony; at war, a soldier marches through the gateway that feeds an enemy's colonies and on to the colonies (`BeyondErrand`). All of them cross like anyone, when the gateway is open and has room.
- **Rivals at war still invest** unless an enemy soldier is within 5 tiles of one of their cities (`EnemyAtTheDoor`). Up to 2 colonies each, 3 from the Industrial Age; from 2 cities.
- **Standing on an open gateway is not possible:** whoever steps on it goes through. A blockade is therefore made on the far end, or by holding the land the home end stands on.
- **Border escort** (`Trespass.cs`): a land unit standing on a tile that has become another civilisation's land, with no agreement that would let it walk in (`World.CanEnter`: open borders, a defensive pact, or war), is set down on the nearest tile of the same world it may stand on, within 14 tiles, with its orders cancelled. The same for every civilisation. Units in their own city and ships are left alone.
- **Measured** (one 300-turn game, Small map, idle player): first seal read by a rival on turn 67, first gateway opened for good on turn 143, first rival colony on turn 156; 3 gateways opened, 2 rival colonies and 9 local discoveries by turn 300.

## Risks and limits

- **Rivals are slower than a player beyond the gates.** See the measurement above. They never force a gate and never close one, and with opening now taking 24 turns their first colony comes around turn 150.
- **Balance is placeholder.** Every number above is in `DimensionSettings`, `ColonyCatalog`, `AdaptationCatalog` or `AccordCatalog` and has not been tuned by play.
- **Resonant rules have little to bite on** until units or buildings that depend on electricity exist.
- **`MilitaryValue` and the forced-gate fight ignore local physics**, so a rival may overrate Riflemen it sends to an Ember world.
- **One gate per world.** The code picks the nearest usable gate in a colony's world, so several gates to one world would work, but map generation still makes one.
- **Constructed gates** are a condition in the data only.
- **Colonies have no territory** and are not shown on the minimap.
- **Not hand-played.** Checked by `DimensionChecks` in the engine harness and in Unity, and photographed with `-dimShot`.

## Worth testing first

1. Is 4 open turns in 10 enough to feel usable without steadying the gate?
2. Does one extraction colony (+2 to +6 production in one city) matter in the middle game?
3. Supply need against gate capacity with two colonies and an army crossing.
4. Local discovery pace: about 8 to 30 turns each with one colony.
5. Whether rivals should be given gold or a nearer gate so they compete more often.
