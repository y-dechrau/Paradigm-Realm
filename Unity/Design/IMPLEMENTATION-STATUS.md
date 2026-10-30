# Current release: Advanced diplomacy — Stage 3

> **Planned design, not implemented:** see [CORE-DESIGN-16BIT.md](CORE-DESIGN-16BIT.md) for the agreed 16-bit core design and draft balance formulas, and [IMPLEMENTATION-PLAN-16BIT.md](IMPLEMENTATION-PLAN-16BIT.md) for the staged plan (including map sizes from Tiny to Massive), and [WORLD-AND-LOOK-DESIGN.md](WORLD-AND-LOOK-DESIGN.md) for the planned spherical world, realistic biomes and new terrain art.

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
