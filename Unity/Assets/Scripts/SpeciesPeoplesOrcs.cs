namespace ParadigmRealm {
// The ten orc civilisations of the Ashen Waste (world 2; owner request, 12 Nov 2026). See SealedPeoples.cs for the rule class
// and SpeciesPeoples.cs for the Add signature. All invented. The orcs are a people like any other: riders, farmers, scholars,
// judges and monks as well as fighters.
public static partial class SpeciesPeoples {
 static void AddOrcs(){
  WorldHistories[adding]="The orcs call their world the Ash and do not pretend it was always so. Under the grey dunes lie the roads, towers and dry canals of a great civilisation that burned in a single season, and the orcs are honest that it was their own. The old empire of Vessk dug too deep for fire-stone and lit something it could not put out. Those who lived were the ones far from the cities: herders, fishers, exiles and prisoners. From them come the peoples of today, who agree on little except that the fire must never be lit again. Some ride the dry steppe, some coax gardens out of cinders, some read the ruins and some simply fight over them. The sealed gateways stand half buried on the old imperial roads. The orcs remember, dimly, that their ancestors closed them on purpose, and nobody alive can say whether that was wisdom or shame.";

  // Inspired by the horse peoples of the Eurasian steppe: felt tents, mare's milk, horse-tail banners and the mounted archer.
  Add("orc","Emberhoof Riders","High Rider Tazgra Nine-Banners","First Saddle of the Dry Steppe","You stand on grass my horses will want by autumn. Do not look worried. Autumn is far off, and I like to know a neighbour before I decide what kind he is.",5,0,10,9,4,
   new[]{"Nine Banners","Emberhoof","Dustmane","Felt Camp","Mare's Milk","Wind Saddle","Thunder Ford","Far Graze","Bowstring","Yurt Ring","Stallion Rock","Autumn Muster"},
   "Riders of the Nine Banners",
   "Tall, rangy orcs with grey-green skin, short tusks capped in copper and long black hair tied in a rider's knot. They live in felt tents that can be struck in an hour, and they follow their horse herds across the dry steppe from one thin pasture to the next. A child of the Emberhoof rides before it walks, and an orc without a horse is pitied as others would pity the lame.",
   "When Vessk burned, its horse herds broke loose and ran north to the last grass. The herders who followed them became the Emberhoof. For generations they were nine quarrelling clans, each under its own horse-tail banner, raiding one another for mares. Their golden age began when Tazgra's great-grandmother won all nine banners in a single summer of races and duels, and the united clans rode from the mountains to the sea. The crisis was the Dry Decade, when the grass failed and half the herds were eaten. High Rider Tazgra holds the banners together by finding them new pasture every year. She is not cruel and does not hate her neighbours, but her horses must eat, and she counts every border as a fence that someone else has put on her grass.",
   "Swift, hard-hitting riders; the dry steppe feeds them well.","Tent camps have no walls worth the name.",
   "Born in the Saddle",new PeopleRule{Attack=1,Terrains=new[]{20},TileFood=1},
   "Ember Lancer",8,1,0,1,
   "Horse Fair",32,new PeopleRule{Gold=1,Food=1},
   "Tents, Not Walls",new PeopleRule{Defence=-1});

  // Inspired by dryland farmers who make soil where there was none: the dark earths of the Amazon, compost and cistern gardens.
  Add("orc","Ash Gardeners of Lowvale","Elder Sower Mubba Greenhand","Tender of the First Furrow","Hold out your hand. There: that is soil, and forty years ago it was cinders. We have beans on the fire and room at the table, if you can be patient about both.",5,3,2,5,5,
   new[]{"Lowvale","First Furrow","Beanrow","Compost Hill","Greenhand","Cistern Garden","Marrow Patch","Windbreak","Seedbank","Ashloam","Gourd Terrace","Second Spring"},
   "Tenders of the First Furrow",
   "Stocky, round-shouldered orcs with moss-green skin, blunt worn tusks and earth permanently under their nails. They wear straw hats and patched smocks and live in low villages ringed by windbreak hedges, where every scrap of waste is turned back into the ground. They are slow to speak, generous with food, and they regard a well-kept compost heap as proof of good character.",
   "The Gardeners descend from prisoners of Vessk who were set to farm the empire's worst land and so were far from the cities when the fire came. Afterwards they found that ash, bone and patience would grow beans where nothing else would. They have been widening that first furrow ever since. Their golden age was the Second Spring, when Lowvale turned green from rim to rim and hungry clans came to trade rather than raid. The crisis was the Night of Torches, when riders burned the outer fields in a single raid and undid thirty years of work. The Gardeners replanted, and then planted thorn hedges as well. Elder Sower Mubba has outlived two husbands and four droughts. She believes that the world can be mended a handful at a time, and that anyone who will dig beside her is kin.",
   "Food from dead land; patient, generous neighbours.","Slow to anger and slower to strike.",
   "Soil from Cinders",new PeopleRule{Food=1,Terrains=new[]{10},TileFood=1},
   "Hedge Warden",7,0,2,0,
   "Seed Vault",7,new PeopleRule{Food=2},
   "Slow to Anger",new PeopleRule{Attack=-1});

  // Inspired by ancestor veneration and bone houses in many cultures, and by the drum as the voice of the shaman.
  Add("orc","Quiet Pyre Bone-Singers","Grandmother Ukha Hollow-Drum","Voice of the Remembered Dead","Come closer to the fire, child of somewhere else. My grandmothers are curious about you. They say you carry your own dead very lightly. We could teach you to listen.",5,6,3,7,2,
   new[]{"Quiet Pyre","Hollow Drum","Ossuary Hill","Grandmothers' Circle","Ember Vigil","Skull Lantern","Ribcage Hall","Naming Stone","Smoke Ladder","Mourner's Well","Long Memory","Antler Lodge"},
   "Singers to the Remembered Dead",
   "Grey-skinned orcs who paint their faces with white ash and wear the carved bones of their own forebears as beads and charms. They live in round lodges built around a hearth that is never allowed to go out, and every family keeps a house of bones where the dead are named aloud each evening. They are gentle with children, grave with strangers and entirely unafraid of death.",
   "After the burning, the wastes were full of the unburied, and the survivors could not rest for the weight of them. A woman called the First Grandmother began to gather the bones and sing over each one the name it had carried, or a new name if none was known. Those who helped her became the Bone-Singers. Their golden age was the Great Naming, when the last field of the dead was cleared and every clan sent its fallen to the Quiet Pyre. The crisis was the Year of False Voices, when a young singer claimed the ancestors demanded war and half the lodges followed him to ruin. Grandmother Ukha was the one who drummed him down. She teaches that the dead want only to be remembered and to see the living behave, and she judges other peoples by how they treat their own graves.",
   "Deep faith; steady in defence; the dead keep them company.","Offerings to the ancestors come before the treasury.",
   "Ancestors Watch",new PeopleRule{Faith=1,Defence=1},
   "Spirit Drummer",4,0,1,1,
   "House of Bones",36,new PeopleRule{Faith=1,Content=1},
   "The Dead Come First",new PeopleRule{Gold=-1});

  // Inspired by archaeologists and by the scholars who preserved the learning of older civilisations after their fall.
  Add("orc","Ruin Readers of Vessk","Archivist Dhurl the Twice-Read","Keeper of the Unburned Shelf","Please do not touch that wall. It has survived fire, nine centuries and my students. Now, you come from beyond the gates? Then I have about two thousand questions. Shall we begin?",5,5,6,4,1,
   new[]{"Unburned Shelf","Old Vessk","Dig Nine","Catalogue","Glyph Court","Sifting Yard","Rubbing Hall","Broken Aqueduct","Tablet Row","Footnote","Cracked Dome","Lecture Pit"},
   "Readers of the Burned Empire",
   "Stooped, ink-stained orcs with olive skin, tusks worn smooth from chewing styluses, and dusty robes hung with brushes, lenses and measuring cords. They live inside the ruins of the old imperial capital, in rooms roofed over with salvaged stone, and spend their days digging, copying and arguing about what they find. They honour a careful footnote as other orcs honour a battle scar.",
   "The Readers began with a single cellar. A junk dealer named Dhurl the First broke into it looking for metal and found a shelf of clay tablets the fire had baked hard instead of destroying. He could not read them, and it bothered him for the rest of his life. His apprentices learned how. Their golden age was the Deciphering, when the Readers recovered the old empire's arithmetic, medicine and law and taught them to anyone who asked. The crisis was the Second Shelf, a sealed vault whose tablets described how the fire-stone had been mined; the Readers debated for ten years and then walled it up again. Archivist Dhurl is the ninth to bear the name. He believes his people burned once from knowing too little about what they knew, and he does not intend to see it happen twice.",
   "Learning recovered from the ruins; great works of scholarship.","Scholars make hesitant soldiers.",
   "Lessons of the Ruins",new PeopleRule{Science=2},
   "Dig Sentry",3,0,1,1,
   "Hall of Fragments",8,new PeopleRule{Science=2},
   "Scholars, Not Soldiers",new PeopleRule{Defence=-1});

  // Inspired by the gladiators of Rome and by wrestling traditions in which a champion fights so that armies need not.
  Add("orc","Karrag Ring Champions","Champion-Queen Vosha the Unbroken","Holder of the Iron Belt","Good shoulders. Bad stance. No, do not be offended, I say the same to my own generals. Come and watch the afternoon bouts, and we will see what your people cheer for.",5,1,9,4,7,
   new[]{"Karrag Ring","Iron Belt","Sandfloor","Challenger's Gate","Roaring Tiers","Liniment","First Blood Yard","Victor's Arch","Sparring Flats","Bellstrike","Crowd's Favour","Last Round"},
   "Champions of the Iron Belt",
   "Huge, heavily scarred orcs with dark green skin, tusks ringed in iron and heads shaved except for a single champion's braid. They live in cities built around great stone arenas, where every office from tax collector to queen is won and held by open challenge. They train from childhood, eat enormously, and treat a clean defeat as an honour to both sides.",
   "In the years after the burning, the clans around the Karrag crater were killing each other over the last wells. A wrestler named Karrag the Even proposed that each clan send one fighter to the crater floor, and that the wells go to the winner for a year. It worked, mostly because the fights were better entertainment than the war. Their golden age was the Hundred Seasons, when the Ring grew stone tiers and champions came from every corner of the Ash to try for the Belt. The crisis was the Bought Bout, when a champion took gold to lose and the crowd tore down half the arena. Since then a thrown fight is the only crime punished by death. Vosha has held the Belt for nineteen years. She has no interest in peoples who will not test themselves, and real affection for any who will.",
   "Fearsome fighters; cities kept happy by the games.","Wits are trained less often than fists.",
   "Trial by Ring",new PeopleRule{Attack=1,Content=1},
   "Ring Champion",2,2,0,0,
   "Great Ring",42,new PeopleRule{Content=1,Production=1},
   "Brawn Over Books",new PeopleRule{Science=-1});

  // Inspired by scrap dealers, ship breakers and tinkers: people who make a living from what a richer age threw away.
  Add("orc","Slag Field Scrapsmiths","Forge-Boss Krenna Ironsalvage","Mistress of the Great Heap","Mind the sharp bits. Everything you see was something else once, and most of it was broken. Give me a week and a look at your wagons and I will tell you what you are doing wrong.",5,2,8,1,3,
   new[]{"Great Heap","Slagfield","Rivet Lane","Meltdown","Girder Camp","Cogpile","Bellows Yard","Rustwater","Salvage Cross","Hammer Shed","Wire Market","Crucible"},
   "Menders of the Great Heap",
   "Squat, burn-scarred orcs with soot-black hands, leather aprons and goggles made from old bottle glass, their tusks often capped with salvaged brass. They live among the slag heaps and fallen foundries of the old empire, in workshops bolted together from whatever was lying nearest. They can mend anything, improve most things, and consider buying something new to be a failure of imagination.",
   "The foundry district of Vessk burned hottest and longest, and for a century nobody went near it. The first Scrapsmiths were outcasts with nowhere else to go, who found that the slag fields held more worked iron than all the living clans could mine. They learned to break it, melt it and sell it. Their golden age was the Age of the Great Heap, when Scrapsmith ploughs, pumps and wagon springs were traded from the steppe to the coast. The crisis was the Black Summer, when smoke from a thousand furnaces sickened the camps and killed the few crops they had. Forge-Boss Krenna moved the furnaces downwind and set her best smiths to building a single great engine from imperial parts. She will not say what it does. She likes neighbours who make things and is short with those who only take.",
   "Tireless makers; profit from the wasteland; goods for every market.","Little grows near the furnaces.",
   "Nothing Wasted",new PeopleRule{Production=1,Terrains=new[]{10},TileProduction=1},
   "Scrap Thrower",9,1,0,1,
   "Breaker's Yard",30,new PeopleRule{Production=1,Gold=1},
   "Smoke and Soot",new PeopleRule{Food=-1});

  // Inspired by the salt caravans of the Sahara and the veiled desert traders who led them from well to well.
  Add("orc","Salt Road Caravaners","Caravan Mother Zebeth of the Grey Veil","Guide of the Long Salt Road","Peace on your road, stranger, and water at the end of it. I have salt, glass, news and three rumours about you already. Which shall we open first?",5,4,1,11,0,
   new[]{"Grey Veil","Saltpan","Well of Nine Days","Glass Dune","Caravanserai","Tether Post","Mirage","Brine Market","Star Camp","Waterskin","Dune Gate","Noon Shadow"},
   "Guides of the Long Salt Road",
   "Lean, sand-coloured orcs wrapped from head to foot in grey cloth against the dust, with only their eyes and the tips of their tusks showing. They live on the move, leading long trains of pack beasts between the wells of the deep desert, and their few towns are markets that fill and empty with the season. They are courteous, sharp in a bargain and impossible to lose in open country.",
   "The Caravaners say their first mother was a water-carrier who walked out of burning Vessk with a salt block on her back and traded it, crumb by crumb, for her life. Her descendants have never stopped walking. Their golden age was the Time of Full Wells, when the Salt Road ran unbroken across the desert and a Caravaner's word was accepted as coin by every clan. The crisis was the Poisoned Well, when a rival clan fouled the water at Nine Days and a whole caravan died within sight of it. The Caravaners named the guilty at every market in the Ash, and within a year no one would sell them so much as a rope. Caravan Mother Zebeth prefers that kind of war. She values a neighbour by the goods and news they carry, and thinks borders are a thing for people who cannot find their way.",
   "Wealth from the desert; traders who reach everywhere.","A people on the move builds little.",
   "The Salt Road",new PeopleRule{Gold=1,Terrains=new[]{3},TileGold=1},
   "Dune Outrider",8,0,0,2,
   "Caravan Court",31,new PeopleRule{Gold=2},
   "Always Moving",new PeopleRule{Production=-1});

  // Inspired by the wreckers and fishing villages of rocky coasts, who lived on what the sea and passing ships delivered.
  Add("orc","Black Shingle Wreckers","Tide-Captain Hrogga Saltjaw","Light-Keeper of the False Beacon","Your ship did not founder on the way in, so either you have a good pilot or we were at supper. Sit. Eat. We do not rob guests, only cargo.",5,7,0,9,7,
   new[]{"Black Shingle","False Beacon","Saltjaw","Keelbreak","Flotsam","Gull Rock","Tarred Rope","Riptide","Wrack Line","Lantern Cove","Driftwood Hall","Low Water"},
   "Keepers of the False Beacon",
   "Barrel-chested orcs with salt-cracked blue-grey skin, yellowed tusks and beards stiff with tar, dressed in oilskins and sea boots. They live in driftwood halls wedged into the cliffs of the rocky coast, where they fish, gather wrack and keep a careful watch on every passing sail. They are loud, superstitious and hospitable, and they know every rock on their shore by name.",
   "The Wreckers were fisher clans too poor for Vessk to tax, which is why they survived it. After the burning, ships from the dying ports still tried the coast, and many broke on the rocks; the clans grew rich on what washed in. In time they stopped waiting for luck and began lighting fires on the wrong headlands. Their golden age was the Fat Tides, when a wreck came ashore every month and Black Shingle halls were roofed with ship's oak. The crisis came when the ships simply stopped. For two generations they went back to fishing and near starved. Tide-Captain Hrogga built ships of his own from the hoarded timber and took the trade to sea. He respects a captain who can bring a hull through his rocks, and regards one who cannot as a delivery.",
   "The rocky coast feeds and enriches them; bold at sea and ashore.","Nobody is quite at ease near them, least of all their own people.",
   "Coast of Teeth",new PeopleRule{Terrains=new[]{18},TileFood=1,TileGold=1},
   "Shore Reaver",2,1,0,1,
   "Wreckers' Quay",43,new PeopleRule{Gold=1,Production=1},
   "Feared on Every Shore",new PeopleRule{Content=-1});

  // Inspired by early law codes that replaced blood feud with fixed compensation, and by courts that weigh both sides aloud.
  Add("orc","Even Scale Tribunal","High Arbiter Morgath Fairhand","Weigher of Blood and Bread","State your name, your people and your grievance, if you have one. If you have none, you are the first visitor this year, and I shall have it entered in the record with some pleasure.",5,0,5,7,6,
   new[]{"Even Scale","Fairhand","Blood-Price","Arbiter's Seat","Bread Court","Feud's End","Sworn Stone","Plaintiff's Gate","Three Witnesses","Truce Field","Scale House","Settled"},
   "Weighers of Blood and Bread",
   "Upright, deliberate orcs with slate-green skin, tusks filed square as a sign of office and plain black coats fastened with a small bronze scale. They live in walled towns laid out around a court, where every feud, debt and boundary is heard in public and settled by a fixed price. They speak slowly, write everything down and are very hard to provoke.",
   "The clans of the eastern Ash once kept feuds that ran for ten generations, and after the burning there were too few orcs left to afford them. A widow named Morgath the Elder, who had lost four sons to four different quarrels, set a scale in the road and refused to move until the clans agreed a price for a life, a limb and a loaf. Their golden age was the Quiet Generation, the first in memory when no orc of the east died by a kinsman's hand. The crisis was the Refusal of the Red Clan, which would not pay a judged price and had to be made to; the Tribunal has kept soldiers ever since, and dislikes it. High Arbiter Morgath Fairhand is her descendant. He wants his borders known, marked and respected, and will be a patient friend to anyone who keeps to their own side.",
   "Peaceful, orderly cities; borders firmly kept.","Every decision waits for every voice.",
   "Feuds Ended",new PeopleRule{Content=2},
   "Truce Keeper",7,1,1,0,
   "Court of the Scale",34,new PeopleRule{Content=1,Gold=1},
   "Every Voice Heard",new PeopleRule{Production=-1});

  // Inspired by mountain monasteries where martial training and meditation are one discipline, and by Stoic self-command.
  Add("orc","Still Tusk Order","Abbess Yurra Calm-Breath","Teacher of the Lowered Fist","You have climbed a long way to reach us, and you are out of breath. Good. Notice it. That is the first lesson, and it is free. Tea?",5,6,3,6,5,
   new[]{"Still Tusk","Lowered Fist","Calm Breath","Cloud Step","Bell Terrace","Sweeping Court","Plain Bowl","Thin Air","Novice Gate","Stone Garden","Morning Form","Empty Hand"},
   "Students of the Lowered Fist",
   "Calm, strongly built orcs with pale green skin, shaven heads and tusks left plain, dressed in undyed robes belted with rope. They live in monasteries on the high ledges of the mountains, where they rise before dawn to practise slow fighting forms, sweep the courts, study and sit in silence. They own almost nothing, laugh easily and are extremely dangerous to attack.",
   "The Order was founded by a war chief called Yurra the Red, who won a battle over a ruined well and found, when the killing was done, that the well was dry. She walked into the mountains and did not come down for twenty years. Those who went up to fetch her stayed. Their golden age was the Time of Open Gates, when the monasteries taught reading, healing and the forms to any orc who climbed the stair. The crisis was the Siege of Bell Terrace, when a warlord demanded the monks fight for him; they refused, held the stair for a winter without killing a single attacker, and outlasted him. Abbess Yurra takes the founder's name, as all abbesses do. She teaches that anger is a fire like the one that burned the world, and that the strongest fist is the one a person chooses to lower.",
   "Faith and learning in the mountains; disciplined defenders.","Few are called to the life, and fewer stay.",
   "The Lowered Fist",new PeopleRule{Faith=1,Terrains=new[]{5},TileScience=1},
   "Staff Adept",2,0,1,1,
   "Mountain Cloister",38,new PeopleRule{Science=1,Faith=1},
   "Few Novices",new PeopleRule{Food=-1});
 }
}
}
