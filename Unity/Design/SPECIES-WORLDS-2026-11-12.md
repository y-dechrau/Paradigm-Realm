# One species to a world (12 Nov 2026, no save version change)

Owner request, with the owner's character artwork (57 pictures): each of the fantasy species has a world of its own with ten civilisations, the demon world gains civilisations based on the ten demon designs, and the insect world gains civilisations based on the insect designs.

## Worlds and species

| World | Species | Civilisations in the catalogue | Unit pictures |
|---|---|---|---|
| 1 Fae Wilds | Elves | 10 new (84-93) | warrior, settler, worker, scout |
| 2 Ashen Waste | Orcs | 10 new (94-103) | warrior, settler, worker, scout |
| 3 Infernal Depths | Demons | 10 new, one per design (104-113) | one picture each |
| 4 Plague Marshes | Tree people | 10 new (114-123) | warrior, settler, worker, scout |
| 5 Drowned Deep | Minotaurs | 10 new (124-133) | warrior, settler, worker, scout |
| 6 Hive Expanse | Insects | 15 new (134-148) and the 5 that existed (79-83) | one picture each, 20 in all |
| 7 Stonehold Deeps (new) | Dwarves | 10 new (149-158) | warrior, settler, worker, scout |

- The **Stonehold Deeps** is a seventh world: mountains and hills with grass and forest in the valleys, iron and gold. Its gateway is sealed (home physics) and its key technology is Feudalism. Its land feeds its own people as the other harsh worlds' land does.
- A new game peoples each world from its own species (`World.NativePlacingOrder`, `SpeciesPeoples.Candidates`). Up to ten are placed in a world; the Hive Expanse has twenty to draw on, so the seed decides which ten appear.
- The 55 earlier peoples (29-83) stay in the catalogue with their portraits. Older saves keep them. New games no longer place them, except the five insect peoples.
- An older save has six worlds and is unchanged (`RealmsHere`).
- `MaxNations` is 90 (16 home civilisations and 70 peoples).

## Pictures

- Source: the owner's pack, scaled to 128 x 128 with the feet on the bottom edge, in `Resources/Visuals/Species` (51 files). The originals are not in the repository.
- `SpeciesPeoples.ArtOf(civ)` gives a civilisation's art key and `ArtFile(art, unitType)` the file. Settler, Worker and Scout have their own pictures for the five fantasy species; every other unit type uses the warrior. A demon or insect people uses its one picture for every unit.
- Second pack (12 Nov, later): every art key also has an archer, a mounted soldier and a siege engine with its crew (`<art>-archer`, `<art>-cavalry`, `<art>-siege`; 108 files), worn by the Archer, by the Horseman and Knight, and by the Catapult. With these there are 159 files.
- Ships keep the ordinary pictures. Species units do not use the walking frames or the guard pose.
- Earlier peoples that are plainly one of these species wear the same pictures in old saves: Sylvan Elves (elf), Ashforge Clans (dwarf), Bloodtusk Horde (orc), the five insect peoples, and the demons of the Infernal Depths (the owner's earlier humanoid demon).
- All four demon pictures the pack's note called "shaded" were clean cut-outs when checked, so they are used as supplied.
- Leader portraits (`Resources/Leaders/leader-84` to `leader-158`, 300 x 400) were made with SpriteCook from a written description of each leader.

## The civilisations

**Elves (Fae Wilds):** Argent Lake League, Glowgrove Lyceum, Thornhunt Clans, White Stair Masons, Starglass Votaries, Amber Road Company, Hollow Oak Anchorites, Greensward Commons, Even Bough Tribunal, Driftseed Wayfarers.

**Orcs (Ashen Waste):** Emberhoof Riders, Ash Gardeners of Lowvale, Quiet Pyre Bone-Singers, Ruin Readers of Vessk, Karrag Ring Champions, Slag Field Scrapsmiths, Salt Road Caravaners, Black Shingle Wreckers, Even Scale Tribunal, Still Tusk Order.

**Demons (Infernal Depths), one per design:** Abyssal Oracle Conclave, Basalt Maw Holds, Choir of Talons, Coil Sovereignty, Graveback Herds, Gullet Monarchy, Vigil of Lanterns, Shardbound Lattice, Twinjaw Burrow League, Veil Strider Reaches.

**Tree people (Plague Marshes):** Weeping Willow Conclave, Mangrove Tide League, Bog Oak Holdfast, Alder Menders, Cypress Bench, Birch Vanguard, Yew Remembrancers, Blackthorn Reavers, Strangler Fig Court, Stilt Palm Wayfarers.

**Minotaurs (Drowned Deep):** Mazewrights of Ombra, Tidehorn Wayfinders, Sun Court Leapers, Nacre Bay Exchange, Reef Terrace Gardeners, Red Thread Oracles, Galewrack Horn-Reavers, Open Colonnade Thinkers, Bronze Yoke Foundries, Kelp Meadow Herders.

**Insects (Hive Expanse), new:** Gleaner Commons (cockroach), Skimmer Courts (dragonfly), The Chirring Fields (grasshopper), Antlered Oak Order (stag beetle), Paperhall Republic (wasp), Husk Granary League (weevil), The Marching Bivouac (army ant), Hearthburrow Moot (bumblebee), Guild of Galleries (carpenter ant), Russet Glaive Clans (hornet), Parasol Garden Union (leafcutter ant), Amberpot Fellowship (stingless bee), Claytower Diarchy (termite), Stitched Canopy League (weaver ant), Chevron Free Companies (yellowjacket). The five that existed: Formic Concord (red wood ant), Apiary of the Golden Comb (honeybee), the scarab people (rhinoceros beetle), the mantis people (praying mantis), the moth people (luna moth).

**Dwarves (Stonehold Deeps):** Deepvein Compact, Amber Hall Brewers, Cogwrights of Tannik, Ledger Houses of Guldmar, Frostpeak Reavers, Silent Stair Order, High Fold Ramherds, Vaultbuilders of Karst Harrow, Graven Bench Lawspeakers, Under-Road Lampwalkers.

Each has a leader, a greeting, twelve city names, a background, a history, an ability, a unique unit, a unique building and a weakness, in the same form as the earlier peoples (`SpeciesPeoples<Species>.cs`).

## Limits

- The names are invented and were checked against each other and against the game's own files, not legally cleared.
- Several histories mention other peoples of the same world; with fewer than ten placed, a history can name a people that is not in that game.
- Ability sizes were written to a guide and have not been balanced by play. Some give a bonus per worked tile and can grow large in a big city.
- Riflemen, spearmen, medics, caravans and priests still look like the warrior (or the single picture for insects and demons).
- The Plague Marshes and Drowned Deep keep their names and land; the tree people and minotaurs are written to suit them.
- A game with ten peoples in each of seven worlds has up to 86 nations. Turn time at that size is reported in `IMPLEMENTATION-STATUS.md`.
