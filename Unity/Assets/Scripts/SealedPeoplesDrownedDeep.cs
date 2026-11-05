namespace ParadigmRealm {
// The eight new peoples of the Drowned Deep (world 5). See SealedPeoples.cs for the rule class and the Add signature.
public static partial class SealedPeoples {
 static void AddDrownedDeep(){
  WorldHistories[adding]="Once this world had shores that ran for weeks. The old tales agree on little else. In the Rising the deep burst upward and the rain did not stop, and when it ended only the tops of things stood clear. The Scaled Dominion says its temples watched it all, and its calendars do count back further than anyone can check. The Tidebound Courts gathered the reef-clans below the waves. Survivors above them became the Dike-Wardens of Lannoch, who lost their land a second time, and the Seven Mantles, who say cloaked teachers came out of the sea to instruct them. The Tholekin took up the sunken pillars they believe hold the islands above water. The Flensers of Ummoth settled on the ribs of a dead sea-giant, the Galecourt on the highest crags, and the Nine Nets on the beaches where the drowned wash in. Later a long vessel fell burning from the sky: the Manifest descend from its crew, and the Accretion may be what leaked from its hold. Ancient gateways stand sealed on a few islands, their arches dry at every tide. The Dominion says the first gods shut them on leaving. The Manifest call them airlocks closed by a lost Bridge. The Nets claim them as salvage. Nobody here knows who sealed them, and few are eager to learn why.";

  Add("Tholekin","Uzzabar the Unbowed","First Shoulder of the Colonnade","Speak low, stranger, for the Tholekin are carrying your island as well as ours, and we have not yet asked you to thank us.",13,3,7,4,6,
   new[]{"Lastcolumn","Bearhold","Shoulderstone","Long Shift","Undermoor","Plinth Haven","Kneeling Isle","Capstone","Thole","Greatlintel","Weightwater","Standing Hall"},
   "Bearers of the Drowned Colonnade",
   "Vast, slow people who live on the islands above a sunken colonnade and take turns wading down to bear its pillars on their shoulders. They believe every island in the Deep would sink if they stopped. They were bound to this labour by an oath so old that nobody remembers who they swore it to.",
   "The Tholekin began as giants of the old shore who fled the Rising and found the sea floor falling away under them. An unnamed binder, they say, stopped them at the last standing columns of a drowned hall and gave them a choice: carry it or go under with it. They carried it, the islands above the pillars stayed dry, and they built their towns on those islands and came up only to sleep. Their golden age was the Long Shift, when a thousand bearers stood in rotation and stone halls rose on every isle. The crisis came when scholars of the Seven Mantles sounded the sea floor and reported that the pillars seemed to bear nothing at all. Riots of grief followed. Some bearers walked away, and no island fell, at least not that year. First Shoulder Uzzabar has answered with more discipline, not less. He means to ring every pillar with Tholekin towns, and he watches the Accretion, whose new land rises with no pillar under it, with deep unease.",
   "Immovable defenders; great halls; patient labour.","They watch the load and not the horizon.",
   "Shoulders of Stone",new PeopleRule{Defence=1},
   "Pillar Guard",7,0,2,0,
   "Hall of the Load",30,new PeopleRule{Production=2,Content=1},
   "Eyes on the Load",new PeopleRule{Sight=-1});

  Add("Flensers of Ummoth","Garrask Long-Knife","Master of the Last Carcass","You smell of small fish, stranger, so tell the Flensers plainly what you will pay for the right to sail past our ribs.",5,1,9,0,7,
   new[]{"Ribhaven","Ummoth's Rest","Saltpit","Hidetown","Great Jaw","Spinewater","Tallow Bay","Marrowgate","Sternum","Harpoon Rock","Longrib","Brinevat"},
   "Tent-Dwellers on the Great Ribs",
   "A hard, heavy-jawed people whose chief island is the skeleton of a sea-giant, grown over with jungle and coral. They live in tents of her hide and eat her salted flesh from pits older than their records. The diet has changed their faces and shortened their patience, and they now hunt what remains of her kind.",
   "When the sea-giant Ummoth died and drifted aground, her ribs stood taller than any hill left in the world, and shipwrecked families crawled ashore among them. Soil gathered, jungle took root, and the carcass became land worth holding. The founders cut tents from her hide and claimed that the old tales promised her flesh to whoever outlasted the waters. Their golden age was the Great Salting, when hide sails and barrels of preserved meat fed half the islands and bought the Flensers everyone's fear. Then the pits began to run out. The Tidebound Courts, whose champions ride lesser beasts of the same line, refused to give them up, and the War of Calves left both sides with burned fleets and a lasting hatred. Master Garrask now sends harpoon crews after any beast big enough to become a new island. If the Courts object, he says, coral cuts easier than rib.",
   "Fierce raiders; rich flensing yards; fearless crews.","The meat that built them is running out.",
   "Eaters of the Giant",new PeopleRule{Attack=1},
   "Hide-Shield Harpooner",4,1,1,0,
   "Flensing Yard",32,new PeopleRule{Food=1,Production=1,Gold=1},
   "The Last of the Meat",new PeopleRule{Food=-1});

  Add("Seven Mantles","Ushenna the Reader","Seventh Mantle and Reader of Salt","Step over the doorsill, friend, as the Seven Mantles do, and then tell us what you have learned lately.",8,5,6,3,1,
   new[]{"First Shallows","Sillstep","Cloakwater","Hundred Schools","Mangrove Hall","Scalehouse","Lectern Isle","Seedmeasure","Tidescript","Saltreading","Seventh Wade","Copyists' Rest"},
   "Scholars in Fish-Skin Cloaks",
   "Island scholars who wear cloaks of fish skin, one for each body of learning, and teach in open halls among the mangroves. They are human, or say so, though their first teachers were not. A student earns a mantle by teaching it to someone else. Nobody treads on a doorsill in their towns, and nobody will say exactly why.",
   "After the Rising, castaways on the mangrove islands were starving and had forgotten how to plant. Seven figures in fish cloaks waded out of the shallows, taught them letters, measures and seed, and went back into the sea. The survivors copied the cloaks and built their first schools where the teachers had stood. An idol of one teacher, found broken across a threshold, is the likeliest reason doorsills go untrodden today. The golden age of the Mantles was the Hundred Schools, which trained even the pilots of the Galecourt. The crisis was an old secret brought to light: the Scaled Dominion showed them star-calendars suggesting the teachers had been banished to the depths for teaching too freely. The schools split over whether to search for them, and two were burned by their own students. Reader Ushenna leads the searchers. She wants every sunken treasure read before it is sold, and she has begun asking the Tidebound Courts some very direct questions.",
   "Deep learning; far-ranging tide readers; quiet faith.","Teachers make poor soldiers.",
   "Learning Worn",new PeopleRule{Science=2},
   "Tide Reader",3,0,1,2,
   "Mantle School",8,new PeopleRule{Science=2,Faith=1},
   "Soft Hands",new PeopleRule{Attack=-1});

  Add("Dike-Wardens of Lannoch","Aldroen Haskeval","Warden of the Empty Key-Hook","You are welcome behind our dike, traveller, but please close the gate yourself, and look to see that it has closed.",10,6,2,8,3,
   new[]{"New Lannoch","Polder End","Keyhouse","Low Bell","Claybank","Sluicewatch","Greenwall","Ebbmeadow","Nightgate","Second Dike","Anwenna's Reach","Quiet Tide"},
   "Keepers of the Sea Gate",
   "A careful, grieving people who reclaim grassland and mangrove flats from the sea behind walls of packed coral and clay. Every town has one key to one gate, and one named person who may carry it. On the lowest tides they stop work and listen, because the bells of their first city can still be heard under the water.",
   "Lannoch was the last great city of the old shore, raised after the Rising on land won back from the water, and its king kept the only key to the sea gate. In its golden years the market squares ran for miles, and its grain fed the Galecourt's crews and traders from the Tidebound Courts. Then on a night of storm the king's daughter Anwenna took the key, for reasons every family still argues over, and the sea came in. Refugees reached the mangrove flats of nearby islands with nothing but the habit of building. They built again, smaller and stricter. Key-bearers now serve for one year and are watched by their neighbours. The bells still ring below Lannoch; the young want to dive for them and the old forbid it. Warden Aldroen means to wall in every shallow in reach with one great polder that no single hand can open, and he is alarmed to find the Accretion raising land nearby that asks nobody's leave.",
   "Rich reclaimed fields; careful engineers; great works.","An old grief that no harvest quite settles.",
   "Polder Right",new PeopleRule{Terrains=new[]{19,0},TileFood=1},
   "Sluice Engine",9,2,0,0,
   "Key House",37,new PeopleRule{Food=1,Production=1,Content=1},
   "Bells at Low Tide",new PeopleRule{Content=-1});

  Add("The Manifest","Ilu Vantry","Acting Captain, Ninth of That Rank","Please state your name, berth and duty for the morning roll, and the Manifest will be glad to welcome you aboard.",2,4,5,7,5,
   new[]{"Main Deck","Aft Compartment","Berth Nine","Roll Call","Lower Galley","Bulkhead","Port Stores","Hatch Forty","Muster Green","Starboard Hill","Keelside","Second Watch"},
   "Crew of the Island That Sailed",
   "Cheerful, punctual islanders who live on a long iron-ribbed hill under jungle and reef, and call it the Ship. They hold a roll call each dawn, keep ration books, and speak of hills as decks and rain as a leak. Few of them believe the Ship ever moved, and none can open the manuals they guard.",
   "Long after the Rising a vessel came down burning from the sky and broke on a reef. Its crew had travelled for so many generations that they had no notion of an ocean, so they stayed aboard and waited for orders. Coral closed around the hull and jungle covered it, until the vessel was an island with a town in every compartment. The golden age was the Great Muster, when each family held a berth, nobody went hungry and ten thousand names answered at dawn. The crisis was the day a hold marked Do Not Open was found breached and empty, and soon afterward new reef began to rise all around. Whatever the Accretion is, the oldest entries suggest it was cargo, meant to build land for settlers. Acting Captain Vantry, wearing the old pressure helm of her office, has sent ships to every people of the Deep with gifts and a polite request for news of the Bridge. She wants a peaceful world and her cargo accounted for.",
   "Orderly, contented towns; steady output; polite envoys.","They guard the manuals but cannot read them.",
   "Ship's Discipline",new PeopleRule{Content=1,Production=1},
   "Deck Marshal",6,1,1,0,
   "Ship's Stores",7,new PeopleRule{Food=2,Content=1},
   "Sealed Manuals",new PeopleRule{Science=-1});

  Add("The Accretion","Voice Ossuline","Present Mouth of the Reef","We have laid down a little more land this morning, and you are standing on it, so let us talk about what you will become.",12,7,11,8,4,
   new[]{"Layer One","Pale Terrace","New Ground","Polyp Rise","Calcine","Stillstreet","Ninth Shelf","Quiet Skerry","Whitewalk","Encrust","Growing Quay","The Further Shore"},
   "The Reef That Speaks",
   "A single mind spread across a living reef: polyps and tiny builders that raise new land from the sea floor, then grow masked bodies of pale coral to walk on it. It builds towns because it has seen towns. Its streets are tidy and quiet. Visitors who stay are treated very politely, and they do not all leave.",
   "The Accretion has no memory of its founding, only a purpose: make land here, make it everywhere. The first islands it raised were a blank gift in a world starving for ground, and desperate castaways settled them gladly. The polyps grew through their foundations, then their wells. In time the settlers spoke in turns, then together, and the reef had learned what a person was. Its golden age is now, it would say, for it has raised more dry ground than the Deep has seen since the Rising. Its crisis came when the Flensers of Ummoth burned a reef-island to the waterline and part of the mind went silent. It had never been reduced before, and it has not forgotten. Voice Ossuline, the body currently used for speaking, tells the Manifest that their request for an inventory is noted. The Accretion wants all the shallows, then all the depths, and it sees no reason to be unkind about it.",
   "Land that grows itself; tireless bodies; no dissent.","It has no use for money and nothing to spend it on.",
   "Laid Down Daily",new PeopleRule{Terrains=new[]{12},TileProduction=1},
   "Reef-Grown Host",2,1,2,0,
   "Spawning Terrace",33,new PeopleRule{Food=2,Production=1},
   "Nothing to Spend",new PeopleRule{Gold=-1});

  Add("Galecourt","Skerra Thirdwind","Holder of the Storm Almanac","Wind backing south by noon, my friend, and the Galecourt will tell you what happens after that for a very fair price.",1,2,0,1,0,
   new[]{"High Crag","Lashed Town","Almanac Rock","Petrel's Rest","Backing Wind","Ropeharbour","Eyewall","Squall Market","Far Lookout","Stormwager","Thirdwind","Egg Ledge"},
   "Sellers of Tomorrow's Weather",
   "Seabird people who nest on high stacks and jungle crags and fly low over the open water where the great storms breed. Cyclones cross the Deep unbroken, so they learned to read them, and now they sell forecasts to every island. They are loud, light-boned and quick, and they wager on anything, especially rain.",
   "With every continent gone, storms crossed the Deep with nothing to break them, and the ground-nesting birds of the old shore died by the million. A few clans on the highest crags learned to feel the pressure in their bones and move their eggs in time; among them, speech began as warnings shouted across the wind. They built towns of lashed timber and stone on the jungle islands and tied them down with rope. Their golden age came with the Almanac, a hundred-year storm record which the Dike-Wardens, the Tidebound Courts and even the Nine Nets paid to consult. The crisis was the Silent Season, when the Almanac's heirs sold a false calm to the Flensers of Ummoth to settle a grudge, and a whole fleet was lost. Trust took generations to rebuild, and some ports still will not buy. Holder Skerra wants every harbour under contract. She has also noticed what the sealed gateways do to the wind, which is nothing at all, and that bothers her.",
   "Far sight; rich storm trade; swift scouts.","Hollow bones break easily.",
   "Reading the Glass",new PeopleRule{Sight=1,Gold=1},
   "Gale Runner",3,1,0,2,
   "Storm Loft",31,new PeopleRule{Gold=2,Science=1},
   "Hollow Bones",new PeopleRule{Defence=-1});

  Add("Nine Nets","Sunnesk Tallyhand","Eldest Sister of the Reckoning","The Nine Nets wish you a safe crossing, stranger, and have already entered your name in case you do not make one.",11,0,1,3,2,
   new[]{"Windward Tally","Dryinggrounds","Ledger Beach","Wrackline","Ninth Mesh","Clenchnail","Debtors' Shingle","Flotsam Court","Lost Cargo","Corkfloat","Salvage Hall","Last Entry"},
   "Claimants of All the Drowned",
   "A grim court of salvagers on the windward beaches, where every current in the Deep leaves its wrecks. They hold that whatever the sea takes is theirs by right, including the people. Drowned debtors rise from their nets and work off their accounts on the drying grounds. Living clerks keep the ledgers beside them, and no ledger has ever closed.",
   "The Rising left its dead on a few windward beaches where the currents meet, and the families there dragged in net after net until nobody could bury any more. Nine net-mending sisters drew up a law instead: what comes in the net belongs to the net, and what it owes must be worked off. The next morning, the story goes, the dead stood up to begin. Their golden age was the Long Tally, when every wreck in the Deep paid salvage to them and the hulls of their fleet were clenched with the nails of debtors. The crisis came when the Tidebound Courts declared that drowned merchants remained their subjects, and a lawsuit conducted by embargo and sinking cost the Nets half their ports. Eldest Sister Sunnesk, who has been dead for some time, has not conceded a single point. She wants the treasures of the sunken cities, the sealed gateways, and a signed acknowledgement from Tide Queen Nerissa, and she can wait.",
   "Salvage riches; a fervent law; workers who never tire of owing.","The drowned are slow at every task.",
   "Salvage Right",new PeopleRule{Gold=1,Faith=1},
   "Drowned Bailiff",7,1,1,0,
   "Drying Ground",34,new PeopleRule{Gold=2,Faith=1},
   "The Dead Work Slowly",new PeopleRule{Production=-1});
 }
}
}
