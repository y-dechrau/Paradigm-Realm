namespace ParadigmRealm {
// The ten minotaur civilisations of the Drowned Deep (world 5; owner request, 12 Nov 2026). See SealedPeoples.cs for the rule
// class and SpeciesPeoples.cs for the Add signature. All invented: islanders, sailors and labyrinth builders, each drawn from
// a real seafaring or island culture, craft or idea.
public static partial class SpeciesPeoples {
 static void AddMinotaurs(){
  WorldHistories[adding]="The minotaurs say the sea took their world in a single night. Before the Drowning there was one great land and one great city at its heart, built as a maze so that no enemy could find its centre. Then the water rose, the land broke into ten thousand islands, and only the hilltops and the highest walls stayed dry. The survivors became islanders. Some took to boats and learned the stars; some stayed and built new labyrinths of coral stone; some went into the jungle and the mangroves and came out farmers, herders or pirates. Every minotaur people still keeps a thread somewhere, in a shrine or a knot or a sailor's braid, in memory of the way home that was lost. The sealed gateways stand on the tallest islands, crusted with salt and shell. Old songs say the sea came through one of them. Nobody is in a hurry to check.";

  // Inspired by the palace of Knossos and the labyrinth of Greek legend, and by the builders of walled island citadels.
  Add("minotaur","Mazewrights of Ombra","Archwright Tauron Deepwall","Keeper of the True Path","You found the door. Few do. The way in was the easy part, guest, so stay close to me, touch nothing that looks like a lever, and we shall get on well.",13,7,4,7,3,
   new[]{"Ombra","True Path","Deepwall","Seventh Turning","Blind Corner","Coralstone","Dead End","Threadgate","Inner Court","Wrong Way","Mason's Spiral","Centre"},
   "Builders of the True Path",
   "Massive, slow-moving minotaurs with dark brown hides, wide horns banded in bronze and stonecutters' kilts white with coral dust. They live inside their own labyrinths: whole cities of winding passages, false doors and hidden courts, which only those born there can walk without a guide. They are careful, private and proud, and they consider a straight street to be an open invitation to thieves.",
   "Ombra stands on the highest surviving walls of the drowned mother city, and its people claim descent from the masons who built the first maze. When the sea rose they simply kept building upward, in coral stone, along the old plan. Their golden age was the Age of Seven Turnings, when Ombra added a new ring of passages every generation and no raider who entered was seen again. The crisis was the Year of the Lost Children, when a wall was moved without record and a whole school could not be found for nine days. Since then every stone is logged, and the Archwright alone holds the full plan. Tauron Deepwall has added two rings in his lifetime and means to add a third. He admires any people who build to last and is baffled by those who would rather sail away.",
   "Cities no enemy can find their way through; tireless builders.","They look inward and see little beyond their walls.",
   "Walls Within Walls",new PeopleRule{Defence=1,Production=1},
   "Maze Guard",7,0,2,0,
   "Labyrinth Gate",9,new PeopleRule{Production=1,Content=1},
   "Inward Looking",new PeopleRule{Sight=-1});

  // Inspired by the Polynesian wayfinders, who crossed the open Pacific in outrigger canoes by reading stars, swells and birds.
  Add("minotaur","Tidehorn Wayfinders","Wayfinder Queen Halani Star-Reader","Navigator of the Ten Thousand Isles","I smelled your land on the wind two days before I saw it. New soil, new trees. Tell me which stars stand over your home, and I will tell you how far you have come.",13,5,0,11,4,
   new[]{"Tidehorn","Star Reader","Outrigger","First Landfall","Swell Haven","Frigate Bird","Canoe Shed","Windward","Far Atoll","Sailmat","Dawn Crossing","Next Island"},
   "Readers of the Star Paths",
   "Lean, long-horned minotaurs with sun-bleached tan hides, shell beads braided into their manes and wraps of woven bark cloth. They live as much on their great twin-hulled canoes as on land, and their villages are a row of boat sheds with houses behind. They navigate without instruments, by star, swell and the flight of birds, and a child is not counted grown until it has found an island nobody told it about.",
   "The Wayfinders descend from those who were at sea on the night of the Drowning and came home to find no home. They sailed on, and learned that the stars at least had not moved. Their golden age was the Great Scattering, when Tidehorn canoes found and settled a new island almost every year and the star paths were sung from one end of the ocean to the other. The crisis was the Silent Voyage, when a fleet of forty canoes followed a new star path east and none came back. For a generation no one sailed beyond sight of land. Halani broke that fear by sailing east herself and returning with a branch from an unknown tree. She believes there is always another island, and sees no reason why her people should not be the first to stand on it.",
   "Far-sighted explorers; fed by the sea; quick to settle new shores.","Light canoes and scattered villages are hard to defend.",
   "Star Paths",new PeopleRule{Sight=1,Food=1},
   "Voyager Scout",3,0,0,2,
   "Voyaging Hall",43,new PeopleRule{Food=1,Gold=1},
   "Light Hulls",new PeopleRule{Defence=-1});

  // Inspired by the bull-leaping frescoes of Minoan Crete and by the ancient Olympic games with their sacred truce.
  Add("minotaur","Sun Court Leapers","First Leaper Kallise Goldhoof","Victor of the Seven Games","Welcome to the Sun Court! You have arrived between events, which is the only time anyone here will talk to you. Do your people run, wrestle or leap? Be honest. We will find out.",13,3,5,9,5,
   new[]{"Sun Court","Goldhoof","Leaper's Sand","Laurel Stair","Seven Games","Wrestling Ground","Torch Relay","Victor's Isle","Discus Field","Truce Harbour","Garland","Long Jump"},
   "Athletes of the Sun Court",
   "Lithe, powerfully muscled minotaurs with golden-fawn hides, horns gilded at the tips and short bright kilts worn with wreaths of leaves. They live in airy white towns built around a sanded court, where they train every day to run, wrestle, throw and vault over charging wild sea-oxen. They are vain, cheerful and fiercely competitive, and they hold that any quarrel can be settled by a fair contest.",
   "The islands of the Sun Court were once at constant war over fishing grounds. The story goes that two rival chiefs, becalmed on the same sandbar, passed the time by daring each other to leap a wild ox, and went home friends. They agreed to meet every fourth summer and do it again. Their golden age was the Long Truce, when the Seven Games drew athletes from across the Deep and no blade was drawn in the islands while the torch burned. The crisis was the Broken Garland, when a chief used the truce to land soldiers on a rival's shore; every other island joined to throw him into the sea. Kallise Goldhoof has won all seven events, the first in a century to do so. She would rather beat a neighbour on the sand than on the battlefield, but she expects to win either way.",
   "Strong, spirited fighters; cities united by the games.","Prizes are wreaths, and wreaths buy nothing.",
   "The Games Truce",new PeopleRule{Content=1,Attack=1},
   "Games Champion",2,1,0,1,
   "Leaping Court",42,new PeopleRule{Content=2},
   "Glory Before Gain",new PeopleRule{Gold=-1});

  // Inspired by the Phoenician sea traders and by the pearl divers of the Gulf and the South Seas.
  Add("minotaur","Nacre Bay Exchange","Pearl-Factor Dromas Heavypurse","Master of the Shell Count","A new face, and new coin with it, I hope. Sit. I will not ask what you paid for that cloak, because I would only be sad for you. Let us talk about what you need.",13,2,0,1,0,
   new[]{"Nacre Bay","Shell Count","Heavypurse","Diver's Rock","Oyster Bank","Lustre Quay","Scale and Weight","Factor's Wharf","Black Pearl","Mother-of-Pearl","Bargain Steps","Customs Isle"},
   "Counters of the Shell",
   "Sleek, well-groomed minotaurs with glossy black hides, horns inlaid with mother-of-pearl and robes of dyed sea-silk heavy with rings and seals. They live in crowded harbour towns of warehouses and counting rooms, built on stilts over the reefs where their divers work. They trade with everyone, keep accounts in knotted cords of shell, and can tell the worth of a pearl by rolling it once across the tongue.",
   "Nacre Bay was a poor reef village until a diver brought up a pearl the size of a hen's egg and, instead of wearing it, cut a tally of who had helped and shared out the price. That tally became the Shell Count, and those shares became the Exchange. Their golden age was the Hundred Sails, when Exchange ships carried pearls, dye and bronze between every island people and took a small part of each cargo for the trouble. The crisis was the Glass Pearl Scandal, when a factor sold false pearls under the Exchange seal and its credit collapsed across the Deep in a season. The Exchange drowned the forger's stock in the bay and repaid every buyer. Dromas Heavypurse keeps one glass pearl on his desk as a reminder. He has never met a problem that could not be priced, and distrusts anyone who claims to have no price at all.",
   "Riches from the reefs; ships and traders in every harbour.","They pay others to fight, and get what they pay for.",
   "Pearl Beds",new PeopleRule{Gold=1,Terrains=new[]{12},TileGold=1},
   "Hired Harpooner",4,1,1,0,
   "Pearl Exchange",39,new PeopleRule{Gold=2},
   "Paid to Fight",new PeopleRule{Attack=-1});

  // Inspired by the stone-walled fishponds of Hawaii and the taro terraces of the Pacific islands.
  Add("minotaur","Reef Terrace Gardeners","Pond-Mother Sabeya Soft-Step","Warden of the Fish Walls","Step where I step, please, the taro is young. There. Now you are a guest and not a flood. Have you eaten? Nobody leaves these terraces hungry, it would shame us.",13,4,2,8,6,
   new[]{"Fish Walls","Soft Step","Taro Terrace","Sluice Gate","Breadfruit Grove","Pond Rim","Yam Mound","Tidepool","Stone Weir","Green Lagoon","Mulch Bank","Full Basket"},
   "Wardens of the Fish Walls",
   "Broad, placid minotaurs with dappled grey-and-white hides, short blunt horns and wide hats of woven palm. They live in villages of thatched houses set among flooded terraces and stone-walled fishponds, which they tend from before dawn until the tide turns. They work hard, eat well, waste nothing, and know to the basket how much each terrace and pond will yield in a season.",
   "After the Drowning there was almost no land left to farm, so the Gardeners' ancestors farmed the sea's edge instead. They walled the shallows with stone to fatten fish and stepped the hillsides so that one stream watered forty fields. Their golden age was the Time of Full Baskets, when the terraces fed three times the people who worked them and the surplus went by canoe to every hungry island. The crisis was the Great Wave, which broke the fish walls and salted the lower terraces in a single morning. It took a generation of labour to rebuild them, higher and thicker. Pond-Mother Sabeya was born in the last year of that work. She asks little of her neighbours except that they stay off her walls, and she watches her boundaries as closely as she watches the tide.",
   "Abundant food from pond and terrace; hard-working villages.","Open terraces offer an attacker little resistance.",
   "Ponds and Terraces",new PeopleRule{Food=1,Terrains=new[]{12},TileFood=1},
   "Weir Keeper",7,1,1,0,
   "Great Fishpond",7,new PeopleRule{Food=2},
   "Open Terraces",new PeopleRule{Defence=-1});

  // Inspired by the walking of labyrinths as a form of prayer, as on cathedral floors, and by the oracles of the ancient world.
  Add("minotaur","Red Thread Oracles","Oracle Meliantha of the Red Thread","Walker of the Inner Turn","You have a question. Everyone who comes here does. Walk the path with me, slowly, and by the centre you will find you already knew the answer. Most people are annoyed by this.",13,6,3,6,2,
   new[]{"Red Thread","Inner Turn","Spindle Shrine","Knotted Vow","Seven Circuits","Pilgrim's Landing","Unwinding","Loomstone","Quiet Centre","Incense Shoal","Omen Rock","Spoolhaven"},
   "Walkers of the Inner Turn",
   "Slender, grave minotaurs with white hides, horns wound in red thread and long plain robes the colour of undyed wool. They live on small quiet islands, each with a single path laid out in stones that winds inward to a shrine. They spin, weave and study, and they walk the path each day at dawn in silence, holding that the mind untangles itself as the feet turn.",
   "The Oracles tell of a weaver who, on the night of the Drowning, tied a red thread to her doorpost and led her neighbours up through the dark maze to the high ground, paying it out behind her so that others could follow. When the water stopped, the thread was all that remained of the way home. Her followers laid its course in stones and began to walk it. Their golden age was the Age of Pilgrims, when chiefs from every island came to walk the Seven Circuits before any great decision. The crisis was the Cut Thread, when an oracle sold a prophecy to one side in a war and the shrine was burned by the other. The Oracles have given no prophecies since, only questions. Meliantha holds that faith is a path walked, not a wall defended, though she thinks less of peoples who walk no path at all.",
   "Faith and learning walk together; pilgrims fill their shrines.","Those lost in thought get little done.",
   "Walking Meditation",new PeopleRule{Faith=1,Science=1},
   "Thread Guardian",2,0,1,1,
   "Walking Maze",36,new PeopleRule{Faith=2},
   "Lost in Thought",new PeopleRule{Production=-1});

  // Inspired by the sea raiders of the Bronze Age and the buccaneers of the Caribbean: hidden coves, ransom and boarding actions.
  Add("minotaur","Galewrack Horn-Reavers","Storm-Captain Brakkos Split-Horn","Terror of the Leeward Passage","Ho! A sail I do not know and a hull I have not counted. Relax, friend, I am ashore and in a good mood. Have a drink and tell me, purely from interest, which way your trade ships go.",13,0,0,9,7,
   new[]{"Galewrack","Split Horn","Leeward Passage","Boarding Hook","Rum Cove","Black Sail","Ransom Bay","Cutlass Point","Smuggler's Cut","Powder Rock","Careening Beach","Loot Landing"},
   "Terrors of the Leeward Passage",
   "Scarred, swaggering minotaurs with rust-red hides, chipped and iron-shod horns, gold earrings and whatever fine clothes they last took from someone else. They live in hidden coves among the storm-wrecked outer islands, in towns that are half shipyard and half tavern. They elect their captains, share their plunder by strict rule, and lower their heads and charge when they board a ship.",
   "The Reavers began as crews of honest fishermen whom the Great Wave left with boats and no village. They took a merchant ship out of hunger, found it easy, and took another. Their golden age was the Red Decade, when Brakkos's grandmother held the Leeward Passage and every cargo in the Deep paid her a tenth or went to the bottom. The crisis was the Night of Lanterns, when the trading islands joined fleets and burned Galewrack to the waterline. The survivors rebuilt in deeper coves and learned to take ransom instead of lives, since the dead pay nothing. Storm-Captain Brakkos lost half a horn boarding a ship twice the size of his own and counts it a fair price. He respects strength and a good fight, and he considers a rich, weak neighbour to be a kind of standing invitation.",
   "Savage boarders; plunder fills the hold.","A town of pirates is never a quiet one.",
   "Boarding Rush",new PeopleRule{Attack=1,Gold=1},
   "Boarding Brute",2,2,0,0,
   "Reavers' Dock",43,new PeopleRule{Production=1,Gold=1},
   "No Honest Trade",new PeopleRule{Content=-1});

  // Inspired by the schools of ancient Athens, where philosophers taught while walking in shaded colonnades.
  Add("minotaur","Open Colonnade Thinkers","Scholarch Eudoros the Slow-Chewing","First Walker of the Colonnade","Ah. A visitor from outside the world. This settles a wager and opens six arguments. Walk with me. We think better on our feet, and I would like to disagree with you properly.",13,1,5,6,1,
   new[]{"Open Colonnade","Slow Chewing","Dialogue","Counterpoint","Shaded Walk","Syllogism","Olive Terrace","Rebuttal","Students' Quay","Lamp Hour","First Principles","Open Question"},
   "Walkers of the Open Colonnade",
   "Stout, grey-muzzled minotaurs with dun hides, unadorned horns and simple draped cloaks, usually with a scroll under one arm. They live in small white cities of shaded walks and open squares, where citizens spend half the day in argument and the other half thinking about what they should have said. They chew slowly, speak slowly and have been known to pause a battle to settle a point of logic.",
   "The Thinkers descend from the Mazewrights' exiles: builders banished from Ombra for asking why the walls were needed at all. They settled an open island, built a city with no maze in it, and found that conversation filled the space. Their golden age was the Age of the Nine Schools, when their teachers worked out geometry, the motion of the tides and a theory of just rule, and students sailed from every island to hear them. The crisis was the Trial of the Tenth School, when the assembly voted to drown a teacher for mocking the others, and then spent thirty years ashamed of it. No opinion has been punished since. Scholarch Eudoros has taught for fifty years and changed his mind on most things at least once. He values a neighbour who asks good questions above one who brings good gifts.",
   "Outstanding scholars; persuasive envoys.","Every plan is debated until the season for it has passed.",
   "Rumination",new PeopleRule{Science=2},
   "Colonnade Shieldbearer",7,0,1,1,
   "Walking School",8,new PeopleRule{Science=1,Content=1},
   "Endless Debate",new PeopleRule{Production=-1});

  // Inspired by the bronze founders of the ancient Mediterranean: copper islands, tin routes, lost-wax casting and great bells.
  Add("minotaur","Bronze Yoke Foundries","Foundry-Master Koppra Red-Arm","Pourer of the Great Bell","Speak up, the furnaces are loud and my ears are forty years old in this trade. You want bronze? Everyone wants bronze. Show me what you have that I cannot cast myself.",13,2,1,8,3,
   new[]{"Bronze Yoke","Red Arm","Great Bell","Casting Pit","Copper Isle","Tin Strait","Wax Mould","Slipway","Anvil Reef","Charcoal Bay","Ingot Landing","Bellfounder"},
   "Pourers of the Great Bell",
   "Thick-necked minotaurs with soot-darkened russet hides, forearms scarred by sparks, horns capped in polished bronze and heavy leather aprons. They live on smoky islands rich in copper, in towns built around casting pits and slipways. They smelt, cast and hammer from dawn to dusk, build the stoutest ships in the Deep, and mark every great event by pouring a bell for it.",
   "The Foundries began on a bare red island that the other peoples had passed over as useless, until a shipwrecked smith noticed the colour of the rock. Copper from the island and tin from across the strait made bronze, and bronze made the island rich. Their golden age was the Time of the Great Bell, a casting so large that three furnaces were tapped at once and its note was heard on islands a day's sail away. The crisis was the Bare Hills, when the last trees were cut for charcoal, the soil washed into the sea and the furnaces went cold for want of fuel. The Foundries now plant two trees for each they burn and buy timber from anyone who will sell. Koppra Red-Arm means to cast a second bell greater than the first, and judges other peoples by the quality of what they make with their hands.",
   "Master smiths and shipwrights; goods every island wants.","Stripped hills grow little food.",
   "Lost-Wax Masters",new PeopleRule{Production=2},
   "Bronze Ram",9,1,1,0,
   "Bell Foundry",30,new PeopleRule{Production=1,Gold=1},
   "Felled Forests",new PeopleRule{Food=-1});

  // Inspired by cattle-herding peoples whose wealth is their herd, moved here to the sea: gentle sea cows grazed in the shallows.
  Add("minotaur","Kelp Meadow Herders","Herd-Father Ombo Kelpmantle","Shepherd of the Sea Cows","Slowly, slowly. The herd is calving and loud strangers upset them. There. Sit in the shallows with me. Nothing important was ever decided in a hurry, and nothing here needs to be.",13,5,2,11,5,
   new[]{"Kelp Meadow","Kelpmantle","Calving Shallows","Seagrass","Drift Camp","Herd's Turn","Stilt Village","Brackish Reach","Milk Tide","Long Paddle","Root Harbour","Warm Current"},
   "Shepherds of the Sea Cows",
   "Big, gentle minotaurs with shaggy brown hides bleached at the tips, wide curving horns hung with floats and shells, and cloaks of dried kelp. They live in stilt villages among the mangroves and drift with their herds of sea cows from one seagrass meadow to the next. They count their wealth in animals, sing to the calves, and do everything at the pace of the tide.",
   "The Herders say that on the night of the Drowning their ancestors' cattle swam, and kept swimming, and that the sea cows are those same cattle changed by the water. True or not, the Herders have followed the herds through the mangroves for longer than anyone has kept count. Their golden age was the Warm Current, a run of mild years when the seagrass spread, the herds doubled and new stilt villages rose on every sheltered shore. The crisis was the Red Tide, a poison bloom that killed the grass and two calves in three. The Herders drove what was left through open sea to clean water, and lost many of their own on the way. Herd-Father Ombo led that drive as a young bull. He wants only room for the herd to graze and peace to graze it in, and he will go a long way round to avoid a fight.",
   "The mangroves feed and supply them; they spread quietly along every shore.","Nothing is done quickly, learning least of all.",
   "Mangrove Herds",new PeopleRule{Terrains=new[]{19},TileFood=1,TileProduction=1},
   "Sea-Cow Rider",8,0,1,1,
   "Herd Pens",33,new PeopleRule{Food=1,Content=1},
   "Unhurried",new PeopleRule{Science=-1});
 }
}
}
