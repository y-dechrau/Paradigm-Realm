namespace ParadigmRealm {
// The ten civilisations of the tree people of the Plague Marshes (world 4; owner request, 12 Nov 2026: one species to a world).
// See SpeciesPeoples.cs for the Add signature (the art key first) and SealedPeoples.cs for the rule class.
// All invented; each draws on what a real tree does (named in the comment above it) and on a real way of life.
public static partial class SpeciesPeoples {
 static void AddTreefolk(){
  WorldHistories[adding]="The tree people say the Plague Marshes were a green forest once, and that they were its trees. Then the Sickness came up through the water. It turned the lakes sour, killed the forest where it stood and left bog, mangrove and grey waste behind. The trees that lived were the ones that pulled up their roots and walked. That was the First Step, and every people of the marsh counts its years from it. They have never agreed on what to do next. The Alder Menders try to cure the ground a field at a time. The Bog Oak Holdfast means to outlast the Sickness. The Willows mourn the dead forest, the Birches plant on its ashes and the Blackthorns take what the weak cannot hold. Old gateways stand sealed among the dead trunks. Some say the Sickness came through them. Others say the cure is waiting on the far side.";

  // Inspired by the weeping willow on the riverbank and by professional mourners and the keening of the dead.
  Add("treefolk","Weeping Willow Conclave","Mother Sallowen of the Long Tears","First Mourner of the Banks","Hush, stranger from another soil. We were just speaking the names of the lost. Stand with us a moment, and then tell us yours.",7,3,3,5,2,
   new[]{"Longtear","Sallowbank","Keening Pool","Trailing Bough","Mourner's Ford","Catkin Hall","Griefwater","Soft Landing","Bowed Crown","Witheybed","Consolation","Lament's End"},
   "Mourners of the Dead Forest",
   "Tall, stooped tree people with grey-green bark and long trailing branches that hang about them like veils. They live along the banks of slow rivers and sour lakes, rooted for half of each day with their feet in the water. They keep the names of everything the Sickness has killed and recite them at dusk. They are gentle, patient and easier to grieve with than to cheer up.",
   "When the old forest died, the willows on the riverbanks watched it happen from the water's edge and could do nothing. The Conclave was formed so that at least it would be remembered. Their golden age was the Time of Long Rites, when every people of the marsh sent its dead to the willow banks and its quarrels as well, since nobody cared to shout in front of mourners. The crisis was the Dry Lament, a drought that dropped the rivers and left whole groves of willows stranded and dying on cracked mud. The Conclave walked for the first time in an age and learned that grief travels. Mother Sallowen led that march as a sapling. She thinks the marsh has seen enough death, and she will go to great lengths, and some sharp ones, to keep the faiths of the living from being trampled.",
   "Devout, well-loved peacemakers with calm cities.","Too generous with alms to grow rich.",
   "Shared Grief",new PeopleRule{Content=1,Faith=1},
   "Withy Binder",4,0,2,0,
   "House of Tears",36,new PeopleRule{Content=1,Faith=1},
   "Alms for All",new PeopleRule{Gold=-1});

  // Inspired by mangroves, which drink salt water and send their seedlings off on the tide, and by the traders of the Swahili coast.
  Add("treefolk","Mangrove Tide League","Stilt-Captain Rhizara Saltknee","Pilot of the Brackish Channels","Ahoy, dry-foot! Mind the roots, they bite at low tide. Have you anything to sell, or only questions?",7,2,0,1,0,
   new[]{"Saltknee","Brackwater","Propagule Bay","Tidefork","Oystershade","Stiltmarket","Low Channel","Crabwalk","Mudlark Quay","Spring Tide","Rootmaze","Drifting Seed"},
   "Pilots of the Brackish Channels",
   "Wiry tree people with reddish bark and arching stilt roots that lift them clear of the mud. Salt crystals glitter on their leathery leaves. They live in the tidal channels where the marsh meets brackish water, in towns that are half forest and half harbour, and they pole flat boats between the roots. Their seedlings float away on the tide and come home rich or not at all.",
   "The League says its ancestors were the only trees the Sickness could not kill, because they had already learned to drink salt. While the inland forests died, the mangroves spread along every coast and channel and found themselves holding all the harbours. Their golden age was the Hundred Tides, when League boats carried clean water, salt fish and news to every stranded grove and charged fairly for all three. The crisis was the Black Ebb, when a foul tide rolled in from the deep marsh and rotted the roots of three towns in a month. The League cut new channels to flush its harbours and has guarded the sluices ever since. Stilt-Captain Rhizara was a sluice keeper before she was a captain. She trusts the tide, the ledger and her own knees, in that order.",
   "Rich tidal trade and well-fed harbour towns.","Their soft waterlogged wood breaks easily in a fight.",
   "Roots in the Tide",new PeopleRule{Terrains=new[]{19},TileFood=1,TileGold=1},
   "Tide Skirmisher",3,1,0,1,
   "Root Wharf",31,new PeopleRule{Gold=2},
   "Soft Wood",new PeopleRule{Defence=-1});

  // Inspired by bog oak: trunks that lie in peat for thousands of years and come out black, hard and unrotted.
  Add("treefolk","Bog Oak Holdfast","Old Thane Durmast the Unrotted","Pillar of the Sunken Hall","Hm. Another new people. We have seen eleven of those. Stand where you are and let us look at you for a year or two.",7,0,4,7,6,
   new[]{"Sunken Hall","Blackgrain","Peatgate","Unrotted","Heartwood Keep","Tannin Mere","Old Ring","Stumpwall","Drowned Timber","Hardgrain","Thousand Winters","Slow Door"},
   "Pillars of the Sunken Hall",
   "Massive, slow tree people with wood stained black by centuries in the peat and as hard as iron. Their crowns are bare and antlered, hung with grey moss. They live in great halls sunk into the bog, roofed with their own dead, and they move so rarely that birds nest on the sentries. They count a hundred years as a short visit.",
   "The oldest of the Holdfast fell into the bog before the Sickness came and lay there for three thousand years without rotting. When the forest died above them they woke, black and hard, and stood up into a world that had forgotten them. Their golden age was the Raising of the Sunken Halls, when they dragged the preserved trunks of their ancestors from the peat and built fortresses no axe could mark. The crisis was the Peat Fire, which smouldered underground for forty years and burned out two halls from below before anyone noticed the smoke. The Holdfast now keeps a watch on the ground as well as the gate. Old Thane Durmast has stood at the same door for nine centuries. He has seen plagues, kings and bright ideas come and go, and he intends to be standing there when the gateways are forgotten too.",
   "Almost unbreakable defenders with halls built to last.","Their riches lie deep in the mud and are slow to reach.",
   "Wood That Does Not Rot",new PeopleRule{Defence=1,Terrains=new[]{9},TileProduction=1},
   "Heartwood Bulwark",7,0,2,0,
   "Blackwood Palisade",9,new PeopleRule{Production=1,Content=1},
   "Deep in the Mud",new PeopleRule{Gold=-1});

  // Inspired by the alder, whose root nodules enrich poor wet ground, and by physicians who fight disease with patient record-keeping.
  Add("treefolk","Alder Menders","Mender-Chief Alnessa Cleanroot","Sweetener of Sour Ground","Hold still, please. Your colour is good and your soil smells clean. Remarkable. May we take a small sample?",7,1,2,6,1,
   new[]{"Cleanroot","Sweet Ground","Nodule","Carr's Edge","Poultice","Clearwater Carr","Remedy","Greenwound","Leafmould","Second Opinion","Mender's Rest","Good Mud"},
   "Physicians of the Sour Ground",
   "Brisk, dark-barked tree people with round green leaves and small woody cones worn like beads. Their roots carry pale nodules that sweeten sour ground wherever they stand. They live in tidy wet woodlands called carrs, laid out like hospital wards, and they keep notes on everything. They cannot pass a sick patch of soil without stopping to treat it.",
   "The alders were the first to notice that the ground grew a little cleaner where they had stood for a season. They did not know why, so they tested it, moved, and tested it again. That habit made them the marsh's physicians. Their golden age was the Green Return, when the Menders reclaimed a wide belt of bog and living grass grew in the Plague Marshes for the first time since the First Step. The crisis was the Relapse, when the Sickness came back into the reclaimed land stronger than before and killed the Menders who had cured it. They learned that a cure must be tended for ever, and they organised themselves accordingly. Mender-Chief Alnessa walks her rounds every day with a notebook of bark. She believes the Sickness can be understood, and that whatever can be understood can be beaten.",
   "Fertile reclaimed land and careful scholars.","Healers by nature, they hit softly.",
   "Sweetening the Ground",new PeopleRule{Science=1,Terrains=new[]{9,10},TileFood=1},
   "Carr Warden",2,0,1,1,
   "Mending House",35,new PeopleRule{Food=1,Science=1},
   "Gentle Hands",new PeopleRule{Attack=-1});

  // Inspired by the bald cypress, which stands in still water ringed by woody root knees, and by courts of standing elders.
  Add("treefolk","Cypress Bench","High Justice Cyprene Nine-Knees","Senior Knee of the Standing Bench","The Bench recognises the stranger. You may approach as far as the third knee. Speak plainly and we shall consider it by autumn.",7,5,5,7,5,
   new[]{"Nine Knees","Standing Bench","Buttress","Stillmere Court","Swornwater","Plaintiff's Landing","Kneeling Wood","Longstanding","Hearing Pool","Settled Matter","Greybeard Moss","Adjourn"},
   "Judges of the Standing Water",
   "Tall, straight tree people with flared, buttressed trunks and feathery leaves that turn rust-red each autumn. Knobbly root knees rise from the water around them, and the eldest have nine or more. They stand in still lakes in solemn rows, bearded with grey moss, and hear the marsh's lawsuits. A hearing can last a season, and nobody is allowed to sit.",
   "The cypresses took up judging because they were already standing in a circle. Two walking groves brought them a dispute over a patch of clean water, and the cypresses considered it for a year and then divided the water so exactly that neither side could complain. Their golden age was the Long Session, four hundred years in which every border in the marsh was drawn by the Bench and none was crossed. The crisis was the Contempt, when the Blackthorn Reavers ignored a ruling and nothing happened to them, and the whole marsh saw it. The Bench has kept wardens to enforce its judgements ever since. High Justice Cyprene has nine knees and has never reversed a decision. She is courteous to all parties and unmoved by any of them, and she regards an uninvited army as a very poor argument.",
   "Lawful, settled cities and a firm hold on what is theirs.","Every decision takes a season.",
   "Standing Bench",new PeopleRule{Content=1,Terrains=new[]{6},TileGold=1},
   "Bench Warden",7,1,1,0,
   "Knee Court",34,new PeopleRule{Content=1,Gold=1},
   "Slow Judgement",new PeopleRule{Production=-1});

  // Inspired by the birch, a short-lived pioneer tree that is first to grow on burnt or bare ground and makes soil for others.
  Add("treefolk","Birch Vanguard","Forerunner Silverpeel the Young","First Root in New Ground","Oh, good, somebody new! Is there bare ground where you come from? How much? We can be there by spring.",7,7,2,11,4,
   new[]{"Whitebark","New Ground","Firstroot","Paperpeel","Seedling Rise","Ashbed","Quickgrowth","Pale Stand","Clearing","Thin Soil","Next Spring","Vanguard's Rest"},
   "First Roots on the Ash",
   "Slim, restless tree people with white papery bark that peels in curls and small bright leaves that are never still. They are the youngest-looking folk in the marsh and the shortest-lived. They settle the grey wasteland that nobody else will touch, in light airy groves planted close together, and they move on as soon as the soil is good enough for others.",
   "After the forest died, the first green thing to stand on the ash was a birch. The Vanguard has built a whole way of life on that fact. They go first, root fast, drop their leaves to make new soil and die young, and their children go on to the next bare place. Their golden age was the White Spring, when birch groves spread across the wasteland faster than the Sickness could follow. The crisis was the Crowding, when slower peoples moved into the ground the birches had prepared and shaded them out of their own groves. The Vanguard stopped giving land away after that. Forerunner Silverpeel is barely forty, which is old for a birch and a child to everyone else. She speaks quickly, plans in seasons and looks at every stretch of empty waste as a place her people have simply not reached yet.",
   "They thrive on wasteland and spread faster than anyone.","Short lives leave little time for study.",
   "First on Bare Ground",new PeopleRule{Terrains=new[]{10},TileFood=1,TileProduction=1},
   "Sapling Runner",3,0,0,2,
   "Nursery Grove",7,new PeopleRule{Food=2},
   "Short Lives",new PeopleRule{Science=-1});

  // Inspired by the yew: it lives for thousands of years, grows beside old burial grounds, regrows inside its own hollow trunk and made the best bows.
  Add("treefolk","Yew Remembrancers","Remembrancer Iwen of the Hollow Trunk","Oldest Ring of the Barrow Grove","We remember your kind, I think, from four rings in. You were kinder then. Do not eat the berries.",7,6,3,6,2,
   new[]{"Hollow Trunk","Barrow Grove","Red Aril","Ringcount","Evergreen Vigil","Old Names","Memory Hollow","Darkneedle","The Second Trunk","Longwake","Ancestor's Shade","Unforgotten"},
   "Keepers of the Rings",
   "Dark, dense tree people with flaking red-brown bark, black-green needles and a scatter of bright red berries. The oldest are hollow, and a younger trunk grows inside the shell of the old one. They live in walled groves beside the burial mounds of the marsh and keep its histories in their growth rings. Every part of them is poisonous, and they mention it politely.",
   "The yews were old before the Sickness and expect to be old after it. When the forest died they did not walk far. They gathered at the burial grounds, where the dead of every people lie, and began to write down what had been lost. Their golden age was the Reading of the Rings, when the Remembrancers learned to read their own heartwood and recovered a thousand years of weather, war and harvest. The crisis was the Hollowing, when their eldest rotted from within and the records in them were lost. Then new trunks rose inside the old shells and the yews understood that memory could be handed on. Remembrancer Iwen is on her second trunk. She believes the Sickness is a judgement that has been passed before and survived before, and that the proof is in the rings for anyone with the faith to look.",
   "Deep learning, deep faith and deadly archers.","They grow and multiply very slowly.",
   "Rings of Memory",new PeopleRule{Science=1,Faith=1},
   "Yew Bowman",4,2,0,0,
   "Hall of Rings",8,new PeopleRule{Science=1,Faith=1},
   "Slow to Seed",new PeopleRule{Food=-1});

  // Inspired by the blackthorn: a spiny hedge tree with bitter sloes whose wood makes cudgels, and by the cattle raiders of the old Scottish border.
  Add("treefolk","Blackthorn Reavers","Warchief Prunna Sloe-Eye","Sharpest Spine of the Hedge","Close enough, soft-bark. Show us a weapon or show us your stores. Either will tell us what you are worth.",7,4,9,11,7,
   new[]{"Sloe Hollow","Spinehedge","Cudgel Row","Bitterfruit","Raider's Thicket","Hedgebreak","Frostberry","Tangle","Scratch","Long Thorn","Plunder Ditch","Winter Blossom"},
   "Spines of the Dry Ridges",
   "Squat, tangled tree people with black bark and long spines on every branch. They flower white on bare wood in the last frosts and bear small bitter blue fruit. They live in dense thickets on the dry ridges of the marsh, ringed with living hedges, and they come down from them to raid. They carry heavy knotted cudgels cut from their own dead and are proud of every scar.",
   "The blackthorns were hedge trees once, planted to keep others out of better land. When the forest died there was no better land left to guard, so they went looking for some. Their golden age was the Winter of White Blossom, when the Reavers overran four groves in a single cold season and feasted on stores laid up by more careful peoples. The crisis was the Hedge War, when the Bog Oak Holdfast simply shut its doors and the raiders starved outside them for two years. The Reavers learned to pick softer targets and to keep moving. Warchief Prunna took her place by breaking the old chief's cudgel across her own trunk. She has a rough sort of honour. She will not raid anyone who stands and fights well, and she has nothing but spines for those who send gifts in place of soldiers.",
   "Fierce raiders who seize land quickly.","Nobody trusts them, and their own towns grumble.",
   "Hedge of Spines",new PeopleRule{Attack=1,Production=1},
   "Cudgel Reaver",2,1,0,1,
   "Reavers' Muster",32,new PeopleRule{Production=2},
   "Bitter Neighbours",new PeopleRule{Content=-1});

  // Inspired by strangler figs and banyans, whose hanging roots become new trunks and wrap ruined temples such as those at Angkor.
  Add("treefolk","Strangler Fig Court","Matriarch Ficanna Hundred-Trunks","Embrace of the Old Walls","Welcome, little one. What charming walls your people must have. Do come closer, I have plenty of arms.",7,2,4,8,3,
   new[]{"Hundred Trunks","Old Walls","Embrace","Rootfall","Lattice","Overgrown Throne","Hanging Root","Wasp Garden","Canopy Court","Cracked Vault","Host's Memory","Pillarwood"},
   "Embracers of the Old Walls",
   "Sprawling tree people with smooth grey bark and dozens of hanging roots that thicken into extra trunks. An elder may stand on a hundred legs and cover a courtyard. They live in the ruins of stone cities older than the Sickness, wrapped around the walls and towers until root and masonry hold each other up. They are grand, patient and slightly smothering.",
   "The figs began as seeds dropped on the roofs of a dead city. They sent roots down the walls, found the ground and discovered that the stone gave them a shape. The Court grew up, quite literally, around the palaces of a people nobody now remembers. Their golden age was the Great Embrace, when the figs raised fallen towers with their own roots and made the old ruins grander than they had been in life. The crisis was the Cracking, when their roots split the very walls they stood on and a whole quarter of the capital came down. Since then the Court has studied building as carefully as growing. Matriarch Ficanna rests on a hundred trunks and can be in several rooms at once. She admires anyone who builds for the ages, and she keeps a long list of things she means to grow over.",
   "Master builders with strong workshops in forest and ruin.","Their crowded cities are hard to feed.",
   "Roots Over Stone",new PeopleRule{Production=1,Terrains=new[]{1},TileProduction=1},
   "Root Hurler",9,1,1,0,
   "Lattice Works",30,new PeopleRule{Production=2},
   "Crowded Canopy",new PeopleRule{Food=-1});

  // Inspired by the stilt-rooted walking palm, said to shift slowly toward the light, and by travelling pedlars.
  Add("treefolk","Stilt Palm Wayfarers","Guide Socrenna Slowstride","She Who Moves Toward the Light","Well met on the way! I would stop to talk, but I am already stopping. Walk with me, there is light over there.",7,0,1,11,5,
   new[]{"Slowstride","Toward Light","Stiltwalk","Pedlar's Halt","Sunpatch","Roving Fair","Yesterday's Grove","Footloose","Bright Gap","Packroot","Tomorrow's Stand","Wayside"},
   "Walkers Toward the Light",
   "Thin, tall tree people with a single ringed trunk, a tuft of long fronds and a cone of stilt roots that they lift and plant one at a time. They are always moving, very slowly, toward wherever the light is best. They travel in loose bands with packs slung from their fronds and trade as they go. A town of theirs is a market that has stopped for a few years.",
   "The stilt palms never rooted deeply, and when the forest died they found it easier than most to leave. They have been walking ever since, a step a season, following the gaps in the canopy of dead trunks. Their golden age was the Bright Circuit, when their slow caravans linked every grove in the marsh and a stilt palm pedlar was the only news some places heard in a decade. The crisis was the Long Shade, when the bands walked into a country of fog where no light came from any side, and stood lost for years. The survivors learned to send scouts ahead and to mark their trails. Guide Socrenna brought the last band out of the fog. She likes everyone, owes nobody and holds that there is always a better place a little further on, which is why her people keep founding towns and never finish one.",
   "Welcome traders who see far and settle widely.","Never in one place long enough to build much.",
   "A Step a Season",new PeopleRule{Gold=1,Sight=1},
   "Stilt Strider",7,0,1,1,
   "Moving Market",31,new PeopleRule{Food=1,Gold=1},
   "Never Settled",new PeopleRule{Production=-1});
 }
}
}
