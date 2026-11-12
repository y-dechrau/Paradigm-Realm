namespace ParadigmRealm {
// The ten demon civilisations of the Infernal Depths (owner request, 12 Nov 2026): one for each of the owner's ten demon designs.
// "Demon" here only means a people of the burning world; each has its own body, law, craft and way of war, taken from its picture.
// All invented; no name is taken from any religion, myth or published fiction. See SealedPeoples.cs for the rule class and SpeciesPeoples.Add for the fields.
public static partial class SpeciesPeoples {
 static void AddDemons(){
  WorldHistories[adding]="The Infernal Depths lie under a roof of smoke that glows red from below. Rivers of lava cut the land into plates of black glass, cinder plains and mountains that are still being built. Nothing here shares an ancestor. The old furnace-songs say the fire made each kind separately, from whatever was nearest: one from stone, one from crystal, one from the bones in the ash, one from the steam over the boiling lakes. So the peoples of the Depths have ten shapes and ten ideas of what a body is for, and they learned early to trade, argue and write things down, because nobody could assume a neighbour wanted the same things. Ancient gateways stand sealed on the high ground, too hot to touch. Every people has a different account of who shut them.";

  // Abyssal_Oracle_Tentacled.png: a purple, three-eyed, eight-armed seer in pearls with a mother-of-pearl trident; a people of scrying pools and grown pearls.
  Add("demon-abyssal-oracle","Abyssal Oracle Conclave","Mother-Diviner Thuloma the Three-Sighted","Reader of the Boiling Deep","We saw you arrive in a pool of still water nine years ago. You are a little shorter than we expected. Come in; the answer to your first question is no.",4,5,3,6,2,
   new[]{"Brinehollow","Pearlwell","Third Eye","Steamveil","Scrying Deep","Nacre Stair","Eightfold","Kettle Lake","The Still Pool","Inkwater","Farsight Basin","Pearlmother"},
   "Readers of the Boiling Deep",
   "A soft-bodied people with domed violet heads, three golden eyes and eight banded arms, who live in the hot brine lakes of the Depths and in the steam caves around them. They grow pearls the way others grow crops and wear them as rank, record and money. Each arm is trained to a separate task, so one Oracle can write, cook, weigh a pearl and hold a trident at once.",
   "The Conclave says its first mother crawled out of a boiling lake with a pearl in each arm and a question in each pearl. Her children learned to grow pearls around written questions and to open them a generation later, when the answer had ripened. Their golden age was the Age of Clear Water, when every people of the Depths sent envoys to the scrying pools and paid well for a single sentence. The crisis was the Clouded Century: an eruption fouled the great lake, the pools showed nothing, and the Conclave discovered how many of its neighbours had only been polite because they feared being foreseen. Mother-Diviner Thuloma reopened the pools by hand, one filter of ash-cloth at a time. She tells the truth more gently than her predecessors did, and keeps a trident within reach while she tells it.",
   "Far-seeing scholars and priests; rich in pearls and patience.","Reluctant fighters who have foreseen too many battles to enjoy one.",
   "Three-Sighted",new PeopleRule{Science=1,Faith=1,Sight=1},
   "Pearl Tridentier",7,1,1,0,
   "Scrying Pool",36,new PeopleRule{Faith=1,Science=1},
   "Seen It End Badly",new PeopleRule{Attack=-1});

  // Basalt_Maw.png: a squat giant of cracked basalt with magma in the seams, a huge toothed mouth, a riveted belt and a stone hammer; rock-eating mason guilds.
  Add("demon-basalt-maw","Basalt Maw Holds","Anvil-Mother Kragda Emberjaw","First Hammer of the Holds","Stand where I can see you. Good. You are made of meat and will not last, but a guest is a guest. Have some gravel.",4,0,8,7,6,
   new[]{"Emberjaw","Slagmouth","Hammerstroke","Cinderholt","Basalt Stair","Rivet","The Cold Anvil","Moltenkeep","Gritstone","Clinker","Furnace Row","Deepcrack"},
   "Chewers of the Mountain",
   "Broad, heavy people of dark basalt blocks with magma glowing in the cracks between them, small ember eyes and a mouth wide enough to bite a boulder in half. They eat ore and rubble and spit out dressed stone, so their quarries, kitchens and building sites are the same place. They are organised in guilds, wear their guild seal on belt and fist, and settle most arguments by weighing something.",
   "The Holds teach that the first of them was a boulder that fell into a lava river and came out annoyed. Its descendants found that rock chewed slowly could be spat out as good building stone, and that a wall built by one mouth fits the wall built by the next. Their golden age was the Raising of the Nine Stairs, when the guilds cut roads up every volcano in the eastern range and charged a toll in ore. The crisis was the Great Cooling, three generations in which the magma under the oldest hold sank away and its people stiffened where they stood; they stand there still, and are visited on feast days. Anvil-Mother Kragda moved the guild seats to hotter ground and wrote the Hammer Law, which says that no hold may dig below another without paying for the heat. She distrusts anything that cannot be weighed.",
   "Tireless builders and miners; very hard to break.","They eat much of the ore they might have sold.",
   "Bones of the Mountain",new PeopleRule{Defence=1,Terrains=new[]{5,17},TileProduction=1},
   "Maw Breaker",2,0,2,0,
   "Magma Forge",30,new PeopleRule{Production=2},
   "Eats Its Own Ore",new PeopleRule{Gold=-1});

  // Choir_Of_Talons_Avian.png: a black-feathered, crow-headed figure with rows of red eyes, crimson sashes, gold feather charms and a hooked glaive; a singing host of carrion birds.
  Add("demon-choir-of-talons","Choir of Talons","Precentor Akkravel the Ninth Voice","First Voice of the Choir","Hush. Listen. That note is your name as the Choir will sing it when you are gone. Whether it is sung soon or late is what we should now discuss.",4,1,3,9,7,
   new[]{"Redhymn","Rookspire","Ninth Voice","Carrion Loft","Ashwing","The Long Note","Talonreach","Quillfall","Dirgehold","Updraught","Blackfeather","Chantry Crag"},
   "Singers Over the Ash",
   "Tall crow-like people with black wings, bone-pale hooked beaks and a row of small red eyes on each side of the head. They nest on cliff ledges above the lava, ride the hot air that rises from it, and wear crimson sashes hung with gilded feathers, one for each hymn they have learned. They sing in massed choirs, and their singing serves as scripture, drill and news at once.",
   "The Choir began as scavenger flocks that followed other peoples' wars and sang over what was left. In time the song mattered more than the meal. They learned that a thousand voices on a rising thermal can be heard across a lava plain, and that an enemy who hears his own funeral hymn before the battle fights badly. Their golden age was the Hundred-Year Chord, when a single note was held in relay above Rookspire for a century and no army crossed their cliffs. The crisis was the Silence of Ashwing: a rival people poisoned a battlefield, and half the Choir died at the rite of cleaning. Since then they sing first and eat only what they have killed themselves. Precentor Akkravel holds that every people deserves a hymn, and that the Choir alone should choose the date of its first performance.",
   "Fierce, devout soldiers who strike from the air.","Light-boned, and poor farmers who live on lean pickings.",
   "War Hymn",new PeopleRule{Attack=1,Faith=1},
   "Talon Glaivist",7,1,0,1,
   "Hall of Echoes",42,new PeopleRule{Content=1,Faith=1},
   "Lean Pickings",new PeopleRule{Food=-1});

  // Coil_Sovereign_Serpent.png: a green and gold hooded serpent with many eyes and four arms, two holding sabres and two holding shields, in purple and gold; a people of courts and law.
  Add("demon-coil-sovereign","Coil Sovereignty","Sovereign Vesshira of the Fourfold Hand","Keeper of the Coiled Law","Approach to the third coil and no nearer. Two of my hands hold blades and two hold shields. Which pair you deal with is entirely your decision.",4,3,5,4,3,
   new[]{"Sevencoil","Hoodcrown","Third Hearing","The Gilded Hood","Sealhaven","Scalecourt","Fourhand","Venom Stair","Tasselgate","Oathcoil","Jade Assize","Emerald Bench"},
   "Judges of the Coiled Law",
   "Great hooded serpents with green and gold scales, several pairs of amber eyes and four arms. By custom two hands carry blades and two carry shields, and a serpent of rank is never seen without all four in use. They live in terraced cities of warm stone, dress in purple cloth and gold tassels, and hold that a dispute is not over until it has been heard in court and written down.",
   "The Sovereignty counts its history in hoods: each ruler's shed hood is gilded and hung in the Court of Coils, and there are sixty-one. The first Sovereign is said to have ended a feud between two nests by holding a blade over one and a shield over the other until both sat down. From that came the Coiled Law, written in a spiral so that no clause comes first. Their golden age was the Long Assize, when other peoples of the Depths brought border quarrels to the serpents' bench and paid in gold and purple dye. The crisis was the Year of Two Hoods, when twin heirs each claimed the bench and the courts split for a generation. Sovereign Vesshira reunited them by trying her own claim in open court and accepting the verdict. She expects the same patience from her neighbours, and has very little for those who refuse to be judged.",
   "Orderly, wealthy cities; guards who attack and defend at once.","Every decision waits on precedent and ceremony.",
   "Fourfold Hand",new PeopleRule{Gold=1,Content=1,Defence=1},
   "Twinblade Warden",2,1,1,0,
   "Court of Coils",34,new PeopleRule{Gold=1,Content=1},
   "Bound by Precedent",new PeopleRule{Science=-1});

  // Graveback_Quadruped.png: an armoured, tusked, four-legged beast with a dome of bone on its back, red woven cloths and a pair of arms swinging a great axe; herds that carry their ancestors.
  Add("demon-graveback","Graveback Herds","Old Mother Dravhoon Ninefathers","Bearer of the Ninth Canopy","Walk beside me, small one, not in front. I carry nine generations on my back and none of them like to be kept waiting.",4,2,7,11,4,
   new[]{"Ninefathers","Bonecanopy","Longtrek","Ossuary Ford","Tuskwater","The Slow Road","Cairnback","Rib Hall","Ashgraze","Tasselcamp","Forebears' Rest","Dustwallow"},
   "Bearers of the Dead",
   "Low, long, four-legged people with tusked jaws, plated hides and a spiked tail, each carrying a canopy of bone on its back. The canopy is built from the bones of the bearer's own forebears, lashed on with red woven cloth and bronze clasps. A second pair of limbs under the canopy serves as arms and swings a long axe. They travel in herds, graze the cinder plains and weave as they walk.",
   "The Herds say that the first Graveback refused to leave her dead mother on the cinder plain, lifted the bones onto her own back and found she could still walk. Every herd has carried its forebears since, and a place is theirs once an ancestor's bone has been set in its soil. Their golden age was the Great Circuit, when nine herds walked a road right around the Lake of Slag and planted boundary bones the whole way. The crisis came when miners from another people dug up a line of those bones for lime. The War of the Empty Sockets lasted forty years and ended only when the bones were returned, each wrapped in red cloth. Old Mother Dravhoon was a calf in that war. She is slow to anger and has never been seen to turn round, and she regards any land she has walked across as land her grandchildren will need.",
   "Steady settlers of poor land; content, close-knit herds.","Slow to build and slower to hurry.",
   "Ancestors Aboard",new PeopleRule{Content=1,Terrains=new[]{10,17},TileFood=1},
   "Canopy Axebearer",2,2,0,0,
   "Ossuary Wall",9,new PeopleRule{Content=1,Faith=1},
   "The Slow Road",new PeopleRule{Production=-1});

  // Gullet_Monarch_Toad.png: a warty purple toad that is mostly mouth, armoured in an iron cauldron, with a cleaver and a shield painted with teeth; a kingdom of cooks and feasts.
  Add("demon-gullet-monarch","Gullet Monarchy","King Obbrum the Ever-Hungry","Lord of the Long Table","Sit! Eat! We do not talk to the hungry here; it is bad manners and worse policy. Afterwards you may tell me your business, or your recipes. Preferably both.",4,6,2,1,0,
   new[]{"Widemouth","Cauldron Cross","The Long Table","Brothwick","Gulp","Secondhelping","Ladle Bridge","Warty Hollow","Simmer","Cleaver Market","Stockpot","Belchford"},
   "Hosts of the Long Table",
   "Round, warty, plum-coloured toads whose mouths are wider than their shoulders. They live around hot springs and mud pools, farm eels, fungus and pepper-ash, and wear their cooking pots as armour: a Gullet soldier goes to war inside a cauldron with a cleaver in one hand. Hospitality is their law, cookery their chief art, and their rulers are chosen by the size of the table they can set.",
   "The Monarchy began at a single pot. The story goes that a famine drove every toad of the mudflats to one hot spring, where a cook named Obb fed them all from a cracked cauldron and was crowned on the spot. The crown has gone ever since to whoever sets the largest table, and a monarch who lets a guest leave hungry may be lawfully deposed. Their golden age was the Reign of Seven Courses, when caravans crossed the Depths for Gullet pickles, smoked eel and pepper-ash. The crisis was the Lean King, who taxed the kitchens to pay for an army and was eaten out of office in a single week of protest banquets. King Obbrum, forty-first of the pot, has drawn the lesson that soldiers march on their stomachs and so do voters. He is generous, shrewd and very hard to refuse with your mouth full.",
   "Well-fed, fast-growing cities; busy food markets.","Slow workers after a heavy meal, which is every meal.",
   "The Long Table",new PeopleRule{Food=2,Gold=1},
   "Cauldron Guard",7,0,2,0,
   "Great Kitchen",7,new PeopleRule{Food=1,Content=1},
   "After-Dinner Sleep",new PeopleRule{Production=-1});

  // Lantern_Wraith.png: a floating figure in tattered teal robes with a hollow brass helm of three stacked eyes, four thin pale arms, bells, paper charms and a scythe; guides and archivists.
  Add("demon-lantern-wraith","Vigil of Lanterns","Bellwarden Oshevin the Thrice-Lit","Keeper of the Quiet Hours","You heard the bells before you saw us. That is by design; we dislike surprising anyone. Tell us where you wish to go and we will light the way, or explain why you should not.",4,4,6,5,5,
   new[]{"Thricelit","Bellmere","The Unlit Helm","Vesper Drift","Talisman Row","Quiet Hours","Reapers' Lea","Wickhaven","Hollowchime","Ashlantern","Cloudhem","Last Toll"},
   "Keepers of the Quiet Hours",
   "Drifting figures in layered teal robes that trail away into smoke where legs should be. Each wears a hollow brass helm with three golden eyes set one above another, lit from inside like a lantern, and has four thin white arms. They hang themselves with small bells and strips of written paper, cut ember-grass with long scythes, and keep the largest libraries in the Depths.",
   "Nobody, the Vigil included, is sure what is inside a Vigilant's robe. Their own account is that the first of them were the smoke of a burning library, which gathered inside the librarians' empty helms because it could not bear to disperse. They have collected writing ever since. Their golden age was the Era of Open Roads, when Vigil guides with bells and lit helms led every caravan across the glass flats and took a copied page as their fee. The crisis was the Night of Cut Ropes, when a warlord demanded that they guide his army and, being refused, hung their bells in silence along his road. The Vigil answered by putting out every lantern in the Depths for a year, and his army walked into a lava channel in the dark. Bellwarden Oshevin regrets it and says so often. He would much rather be asked for directions.",
   "Learned, far-sighted and trusted as guides by everyone.","Too thin and gentle to hit hard.",
   "Lit From Within",new PeopleRule{Science=1,Sight=1,Content=1},
   "Bell Guide",3,1,0,1,
   "Lamp Archive",8,new PeopleRule{Science=2},
   "Thin as Smoke",new PeopleRule{Attack=-1});

  // Shardbound_Crystal.png: a being of jagged purple crystal held together by rune-cut metal bands, with one eye in a diamond head, a great blade for one arm and spikes for legs.
  Add("demon-shardbound","Shardbound Lattice","Prime Facet Ixorel the Unflawed","Binder of the Great Lattice","We have refracted you and found eleven flaws. Do not be offended. We tell everyone their flaws; it is how we show interest.",4,7,4,9,7,
   new[]{"Unflawed","Geode Prime","Latticehold","Amethyst Stair","Cleavage Plane","The Singing Vein","Facet's Edge","Bandwright","Prism Deep","Twelve Angles","Violet Seam","Clearheart"},
   "The Bound and Unflawed",
   "Living crystal: clusters of violet shards that hang together around a diamond-shaped head with a single eye. Bands of pale metal cut with runes hold each one in shape, one arm is a blade as long as the body, and they walk on four points. They do not eat as others do but grow slowly in mineral heat, and they raise their cities the same way, seeding them and waiting.",
   "A Shardbound is not born. It is grown from a seed crystal in a geode for a hundred years and then bound with inscribed bands that state who it is and what it owes. Lose the bands and the person comes apart into mere stone. Their golden age was the Great Lattice, when they grew whole cities as single crystals and tuned them to ring at dawn. The crisis was the Shattering: a quake ran along a flaw nobody had mapped, and three cities broke in one morning. The survivors concluded that a weakness hidden is a weakness doubled. They began to examine one another, and everyone else, for flaws, and to announce what they found. Prime Facet Ixorel has stood for nine centuries without a crack. It respects peoples who test themselves, has no use for those who will not, and believes the kindest thing it can do for a neighbour is to strike it and listen to the note.",
   "Sharp in attack; exact, patient thinkers and builders.","Crystal is hard but it shatters.",
   "Grown, Not Built",new PeopleRule{Attack=1,Science=1},
   "Shard Caster",4,2,0,0,
   "Tuning Yard",32,new PeopleRule{Production=1,Science=1},
   "Brittle Facets",new PeopleRule{Defence=-1});

  // Twinjaw_Burrower.png: an orange, eyeless, two-headed digger with whiskered snouts, four arms, two picks and a miner's harness with lamp and rope; a league of miners who must agree with themselves.
  Add("demon-twinjaw-burrower","Twinjaw Burrow League","Overseer Dabb-and-Dorrow of the Deep Shift","Twice-Sworn Shiftmaster","Left head: Welcome, we have ore to sell. Right head: We have not agreed a price for the welcome yet. Left head: Ignore him. Right head: Do not.",4,0,8,1,3,
   new[]{"Deepshift","Twopick","Whiskerdelve","Both Heads","Orecart","Lampless","Propwood","Left Fork","Right Fork","Tallyshaft","Spoil Heap","Underbargain"},
   "Diggers of Two Minds",
   "Stocky, orange-scaled burrowers with two eyeless heads on one pair of shoulders, each with a fanged snout and a spray of pink feelers that read the rock. They have four arms: two for picks and two with heavy claws for shifting spoil. They live in mine towns hung with lamps they do not need themselves, kept lit for customers, and every one of them goes about in a harness of pouches, rope and tools.",
   "The League holds that the first Twinjaw was two brothers who shared a tunnel so narrow that they grew together rather than argue over who went first. They have argued about everything else ever since. Each Twinjaw's left head is by custom the optimist and the right the auditor, and no bargain binds until both have said yes aloud. Their golden age was the Deep Road, a trade tunnel cut under three mountain ranges that carried ore beneath other peoples' wars. The crisis was the Flooded Shift, when a careless crew broke into a lava vein and the lower galleries were lost with the tally books. The League has kept two copies of every record since, one for each head. Overseer Dabb-and-Dorrow have run the shifts for thirty years. Dabb wants to dig a tunnel to every city in the world. Dorrow wants to see the costings first.",
   "Rich mines and careful trade; sappers who bring down walls.","Blind above ground, and every order needs a second opinion.",
   "Two Heads on the Job",new PeopleRule{Production=1,Gold=1,Terrains=new[]{2},TileProduction=1},
   "Tunnel Sapper",9,1,0,1,
   "Deep Exchange",31,new PeopleRule{Gold=1,Production=1},
   "Whisker-Blind",new PeopleRule{Sight=-1});

  // Veil_Strider.png: a bone-pale, stilt-legged walker with four long legs, long arms, a smooth beaked mask for a head, dark red drapes and a tall spear; surveyors who walk above the fire.
  Add("demon-veil-strider","Veil Strider Reaches","Tall Regent Ilvashe the Farstepping","Surveyor of the Cooling Roads","We saw your camp from four valleys away. It is pitched on crust that will last perhaps a season. We have marked a better site on this map. The first map is free.",4,1,11,6,1,
   new[]{"Farstep","Stiltmoor","Crustwatch","Fourvalleys","The High Walk","Maskfall","Chartwell","Cooling Road","Redveil","Spindle Ford","Longshadow","Surveyors' Cairn"},
   "Walkers Above the Fire",
   "Very tall, bone-coloured people who stand on four legs as thin as poles, with a narrow upright body, long hands and a smooth mask where a face would be. The legs lift them above the heat and the smoke, so they can cross fresh lava fields that stop everyone else. They dress in dark red drapes pinned with pale metal, speak seldom and softly, and make the best maps in the Depths.",
   "The Striders were ordinary walkers once, they say, until the ground of their homeland began to melt. Those with the longest legs crossed the new lava and lived, and each generation after stood a little taller. They learned to judge a crust by its colour and its sound, and then to draw what they knew. Their golden age was the Age of the Thousand Charts, when no caravan in the Depths set out without a Strider map and no map was more than a year old. The crisis was the False Chart: a forged map bearing their seal sent a whole migration onto thin crust, and the Striders were blamed. They have worn blank masks in public since, as a penance, and sign nothing they have not walked themselves. Tall Regent Ilvashe has walked further than any Strider living. She wants the gateways open for the plainest of reasons: there is nothing left down here to map.",
   "Swift explorers who see far and profit from land others cannot cross.","Long thin limbs break easily in a close fight.",
   "Above the Fire",new PeopleRule{Sight=1,Terrains=new[]{17},TileGold=1,TileScience=1},
   "Stilt Lancer",7,0,0,2,
   "Chart House",38,new PeopleRule{Science=1,Gold=1},
   "Thin Limbs",new PeopleRule{Defence=-1});
 }
}
}
