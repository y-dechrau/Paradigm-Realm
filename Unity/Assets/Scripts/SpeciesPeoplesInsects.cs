namespace ParadigmRealm {
// Fifteen more insect civilisations of the Hive Expanse (owner request, 12 Nov 2026): one for each of the owner's insect designs that
// had no people yet. With the five in SealedPeoplesHiveExpanse.cs (ants, bees, scarabs, mantises, moths) that makes twenty.
// All invented; each draws on what the real insect does and on how the owner painted it. See SpeciesPeoples.Add for the fields.
public static partial class SpeciesPeoples {
 static void AddInsects(){
  WorldHistories[adding]="The Hive Expanse is warm and golden: grass taller than towers, earth mounds the size of hills, flowers a day's walk across. Twenty insect peoples now share it, and travellers who knew only five of them are surprised by the rest. Besides the old powers of mound, comb and tall grass there are paper cities under the eaves of giant seed-heads, stitched leaf towns in the canopy, timber halls carved inside fallen stems, granaries dug into seed husks, pond kingdoms ruled from the air, and one army that has no city at all and does not want one. They quarrel over nectar, timber, water and the right of way, and they settle most of it at the Moth-carried markets. Under all of them the old hum goes on. The sealed gateways on the plains are crusted with resin, and every people has an opinion about what happens when they open.";

  // Cockroach.png: a russet cockroach in a ragged cloak with a cracked, patched shield and a rusty cleaver; inspired by the cockroach's endurance and its life in other creatures' leavings.
  Add("insect-cockroach","Gleaner Commons","Grandmother Rusk the Many-Times-Buried","Eldest of the Gleaners","Ah, you are new. We like new things; they leave such useful rubbish behind. Do not mind the cloak. It has outlived three kingdoms.",12,2,7,11,5,
   new[]{"Underfloor","Crackhaven","Patchwork","Old Midden","Thirdhand","Rustcleaver","Dimcorner","Many Exits","Crumbcatch","Hearthback","Secondskin","Gleaners' Yard"},
   "Heirs of Every Ruin",
   "Flat, russet-brown, long-feelered people who live in the cracks, cellars and ruins that other peoples leave behind. They wear patched cloaks, carry mended shields and waste nothing: a Gleaner town is built entirely of things somebody else threw away. They have no queen and no castes, only a council of grandmothers, and they can go a month without food and squeeze through a gap the width of a leaf.",
   "The Gleaners have no founding story, only a boast: they were here before the hum and expect to be here after it. They never raised mounds of their own. They moved into the cracks of everyone else's, the flooded galleries and burnt combs and abandoned granaries, and made them liveable with patches. Their golden age, if they would use such a phrase, was the century after the Mound Wars, when half the Expanse lay in ruins and every ruin had a Gleaner family in it within the week. The crisis was the Great Sweeping, when three peoples agreed to clear them from the plains for good. The Gleaners went under the floor, waited eleven years, and came back to find the three allies at war with each other. Grandmother Rusk sat through all of it. She holds no grudges, having seen how little they weigh, and her whole policy is to be still alive when the argument is over.",
   "Almost impossible to dislodge; they thrive on land nobody else wants.","Makers of nothing new; they wait for others to invent it.",
   "Outlast Everything",new PeopleRule{Defence=1,Terrains=new[]{10,20},TileFood=1},
   "Rag Shieldbearer",2,0,1,1,
   "Midden Store",7,new PeopleRule{Food=1,Production=1},
   "Nothing New",new PeopleRule{Science=-1});

  // Dragonfly.png: a blue-green dragonfly with huge jewelled eyes, glassy wings, an embossed cuirass and a leaf-bladed sword; inspired by dragonflies' flight, hunting, pond territories and underwater young.
  Add("insect-dragonfly","Skimmer Courts","Mere-Prince Vaziel the Hawker","Lord of Still Water and Open Air","I saw you from above an hour ago, and from the side, and from behind, all at once. This pond is mine. The air above it is also mine. You may use the path.",12,5,0,9,7,
   new[]{"Stillmere","Thousand Lenses","Reedspire","Hawker's Perch","Nymphwater","Skimming","Lilyhold","Glasspond","Darter's Reach","Sedge Court","Mirrorshallows","Windward Reed"},
   "Princes of Pond and Air",
   "Slender blue-green fliers with four glass-clear wings and eyes like polished domes that see in every direction at once. Each court holds one pond and the air above it, with towers of bound reed along the shore. Their young live under water for years as wingless hunters before they climb out to fly, so a Skimmer town is half nursery pool and half watchtower. They are proud, quick and fond of fine armour.",
   "Every Skimmer spends its first years under water as a wingless nymph, hunting in the mud, and climbs a reed one summer morning to split its skin and fly. They say this is why they have two of everything: two childhoods, two laws, one for the pond and one for the air. Their golden age was the Summer of Nine Meres, when nine princes held nine ponds in alliance and their patrols could see a column of ants a day before it arrived. The crisis was the Dry Year, when the meres shrank to mud, the nymphs died in thousands and the princes fought one another over the puddles that remained. Mere-Prince Vaziel won the last pool by single combat on the wing and then, to general surprise, shared it. He has not forgotten what a pond is worth. He watches every border from above, and treats a neighbour's ditch-digging as an act of war until it is proved otherwise.",
   "The keenest eyes in the Expanse; fast, deadly hunters.","Adults live only a few summers, and little gets finished.",
   "Thousand Lenses",new PeopleRule{Sight=1,Attack=1},
   "Mere Hawker",3,1,0,1,
   "Nymph Pools",37,new PeopleRule{Food=1,Production=1},
   "Short Summers",new PeopleRule{Production=-1});

  // Grasshopper.png: an olive-green grasshopper with great jumping legs, a curved sabre and a shield marked with grass blades; inspired by grasshoppers' song, their meadows and the change that turns them into a swarm.
  Add("insect-grasshopper","The Chirring Fields","First Fiddler Tessaly Longshank","Voice of the Evening Field","Welcome to the fields. Sing something, please, anything at all. We do not trust anyone until we have heard what they sound like.",12,3,2,3,2,
   new[]{"Evenfield","Longshank","Chirr","Fiddler's Green","Sweetstem","Highleap","Dewmeadow","Bladesong","Hayrest","Seedhead","Warm Furrow","Last Chorus"},
   "Singers of the Evening Field",
   "Olive-green people with long folded hind legs, who can clear a river in one jump and make music by drawing a leg across a wing. They live in open villages among the short grass, farm the sweet young shoots, and sing together every evening as an act of worship. Each village has its own tune, and a traveller can tell where a grasshopper comes from by listening for a moment.",
   "The people of the Fields say the hum under the world is a song with the words missing, and that they were made to find the words. They farm the short sweet grass between the giant stems, and every evening each village sings the day's work over from the start. Their golden age was the Thousand Evenings, when the choirs of forty fields kept time together and the harvest never failed. The crisis was the Grey Swarm. In a crowded, hungry year the young changed: they darkened, stopped singing, and marched in a mass that ate their own country bare before it broke up in shame. The Fields have feared crowding ever since, and they send out new villages early rather than let one grow too full. First Fiddler Tessaly keeps the old songs and has added one about the Swarm, so that nobody may claim they were not told. She is a friend to anyone who can keep faith and hold a tune.",
   "Rich meadows and devout, cheerful villages; scouts who leap great distances.","They sing through the summer and save very little.",
   "Evensong",new PeopleRule{Faith=1,Terrains=new[]{0,16},TileFood=1},
   "Meadow Leaper",3,0,0,2,
   "Singing Green",33,new PeopleRule{Faith=1,Content=1},
   "Sang All Summer",new PeopleRule{Gold=-1});

  // Stag_Beetle.png: a dark maroon beetle with antler jaws, oak-leaf heraldry, a war hammer and a tall oak-leaf shield; inspired by stag beetles' wrestling contests and their long years as grubs in oak wood.
  Add("insect-stag-beetle","Antlered Oak Order","Lord-Champion Hadrovar of the Unthrown Horn","Marshal of the Oak Lists","Well met! Before we talk, a small formality: we wrestle. Nothing personal. You may decline, but then I choose where the border goes.",12,0,9,5,6,
   new[]{"Oakantler","The Lists","Unthrown","Heartwood Hall","Acorn Cross","Mastfall","Hornlock","Rotten Stump","Gallant's Rise","Tannin","Grubnursery","Champion's Bough"},
   "Champions of the Oak Lists",
   "Heavy, dark-shelled beetles whose jaws have grown into branching antlers, used for gripping and throwing an opponent, not for biting. They live in halls cut into old oak stumps, paint oak leaves on everything they own, and settle disputes by formal wrestling in front of witnesses. A champion who is thrown yields the point and bears no grudge. Their manners are elaborate and their word is good.",
   "A beetle of the Order spends seven years as a grub inside a fallen oak, eating its way through the heartwood, and comes out armoured, antlered and with only a few summers left. They mean to spend those summers well. The Order was founded when two champions, wrestling for a tree, agreed that the loser would guard it for the winner instead of dying for it. Their golden age was the Season of Open Lists, when every quarrel in the western grass was brought to their tourney ground and settled by a throw. The crisis was the Felling, when a raiding army ignored a challenge and simply burned the nursery oak with a generation inside it. The Order has fortified its borders since and no longer assumes that others keep the rules. Lord-Champion Hadrovar has never been thrown. He would still rather wrestle a neighbour than fight one, and he returns every ransom.",
   "Strong, honourable soldiers; quarrels at home end in the lists, not in riots.","Seven years in the wood before a grub is any use; they grow slowly.",
   "Trial by Antler",new PeopleRule{Attack=1,Content=1},
   "Oak Hammerer",2,1,1,0,
   "Tourney Lists",42,new PeopleRule{Content=1,Production=1},
   "Years in the Wood",new PeopleRule{Food=-1});

  // Wasp.png: a slim yellow and black wasp with a bronze cuirass, a long spear and a round starred shield; inspired by paper wasps, which make paper from chewed wood and know one another's faces.
  Add("insect-wasp","Paperhall Republic","Archivist-Queen Ossaline of the Grey Page","First Scrivener","State your name slowly. Thank you; it is written down now, with a sketch of your face. We forget nothing and we file everything.",12,6,6,8,1,
   new[]{"Greypage","Pulpmill","Scrivener's Eave","Hexagon","Foldwick","Inkwasp","The Open Comb","Margin","Ledgerhall","Fibrefall","Small Print","Stemhang"},
   "Scriveners of the Grey Page",
   "Narrow-waisted yellow and black wasps who build their cities from paper: grey, layered halls hung under the eaves of giant seed-heads, made of wood fibre chewed and spread thin. They invented writing in the Expanse and have been filling pages ever since. Every citizen can read, every office is won by examination, and they recognise one another, and you, by the pattern of the face.",
   "The first paper in the Expanse was a wasp nest: wood scraped from a dead stem, chewed and spread thin. The Paperhall wasps say the first writing was a foundress scratching the names of her daughters on the wall so that she would not forget which was which. They can still tell every face in the city apart, and expect visitors to be flattered by it. Their golden age was the Filing of the Plains, when their clerks kept the land records, debts and treaties of five other peoples for a fee. The crisis was the Wet Autumn, when a month of rain turned the Great Archive to pulp and three wars started over boundaries nobody could now prove. The Republic rebuilt under broad stone eaves and began keeping every page in triplicate. Archivist-Queen Ossaline was elected, as all their queens are, by examination. She wants to know everything, and regards a secret as a document that has been misfiled.",
   "Learned and busy; paper, records and javelins in good supply.","A paper city does not stand up well to a siege.",
   "Everything in Writing",new PeopleRule{Science=1,Production=1,Gold=1},
   "Javelin Clerk",4,1,0,1,
   "Pulp Archive",8,new PeopleRule{Science=1,Production=1},
   "Paper Walls",new PeopleRule{Defence=-1});

  // Weevil.png: a blue-black, pebble-shelled weevil with a long curved snout, a scrolled cuirass, spear and round shield; inspired by grain weevils, which live inside seeds, and by the weevil's trick of playing dead.
  Add("insect-weevil","Husk Granary League","High Storekeeper Borivel of the Full Bin","Keeper of the Sealed Seed","Good day. If this is a raid, be aware that I am about to fall over and appear dead for several hours. If it is trade, the winter rate is one measure in five.",12,4,2,7,0,
   new[]{"Fullbin","Deepsnout","Huskhold","Kernel","Winter Measure","Chaffgate","Bored Acorn","Sealed Seed","Millet Rise","Tallyrow","Snoutbridge","Threshing Floor"},
   "Keepers of the Sealed Seed",
   "Small, round, blue-black beetles with pebbled shells and a long down-curved snout that works as drill, spoon and measuring rod. They hollow out the giant seeds of the Expanse and live inside them, with the grain stored in the middle and the family in the walls. They are careful, thrifty and polite, lend food at interest, and when badly frightened they drop on their backs and lie still until the danger has gone.",
   "A Husk weevil is born inside a seed and eats its way out, and the League was founded by those who thought it wiser to leave some of the seed uneaten. They hollow the giant grains of the Expanse into storehouses, seal them with clay, and live in the walls. Their golden age was the Seven Fat Winters, when the League's bins fed four peoples through the cold and its loans were repaid in land. The crisis was the Night of Open Lids, when hornets broke the great store at Kernel and carried off a year's harvest while the garrison lay on its backs pretending to be dead. The League has added spears and thicker walls to its old habit, though the habit remains. High Storekeeper Borivel has never been in debt and has never forgiven one. He is courteous and cautious, and he believes any people can be understood by asking how many days of food it keeps in hand.",
   "Full granaries, steady income and thick walls.","Timid in attack; their first instinct is to play dead.",
   "Full Bins",new PeopleRule{Food=1,Gold=1,Defence=1},
   "Husk Pikeman",7,0,2,0,
   "Sealed Seed Store",7,new PeopleRule{Food=1,Gold=1},
   "Plays Dead",new PeopleRule{Attack=-1});

  // Army_Ant.png: a red-brown ant with great hooked jaws, dark lamellar armour, two hooked blades and bells at the belt; inspired by army ants' raids and the bivouacs they make from their own bodies.
  Add("insect-army-ant","The Marching Bivouac","Column-Mother Arraketh Who Does Not Stop","Head of the March","We are passing through. Whether we pass around you or over you depends on how quickly you answer. The bells mean the column has already started.",12,1,9,11,4,
   new[]{"First Halt","Bellcamp","The Column's Head","Hooked Ford","Raidmeet","Body Bridge","Nightmarch","Fan Front","Statary Ground","Thousand Legs","Dustwake","Ever Onward"},
   "The City That Walks",
   "Lean red-brown ants with sickle jaws longer than their heads, armoured in dark overlapping plates and belted with rope and small bronze bells. They have no fixed towns. Each night the soldiers link legs and jaws to make a living camp with the brood and the Column-Mother inside, and each morning the camp unhooks and marches. What others call their cities are only the places where the column rests longest.",
   "The Bivouac has never built a wall. When the column halts, the soldiers hook their legs together and become the city: gates, roof, nursery and all, with the Column-Mother at the centre. At dawn it unhooks and marches on. They say their first mother was offered a mound by the Formic Concord and replied that a home you cannot carry is a trap. Their golden age was the Great Fan, a raid front forty miles wide that swept the southern grass and fed the column for a generation. The crisis was the Circle: a lost column began to follow its own trail and marched round and round until it died of exhaustion. Every officer now carries bells, and a column that hears its own bells ahead of it must halt and send out scouts. Column-Mother Arraketh has halted for twenty days at most in her life. She regards empty land as wasted and settled land as stored food.",
   "Relentless raiders who feed themselves on the march.","A people always moving keeps few books and fewer schools.",
   "Living Bivouac",new PeopleRule{Attack=1,Food=1},
   "Hookblade Raider",2,1,0,1,
   "Bivouac Ground",32,new PeopleRule{Production=1,Food=1},
   "No Fixed Home",new PeopleRule{Science=-1});

  // Bumblebee.png: a stout, thick-furred black and gold bee with honeycomb plate, a spiked mace and a round wooden shield; inspired by bumblebees' small ground nests, lone founding queens and flight in the cold.
  Add("insect-bumblebee","Hearthburrow Moot","Dame Hobberly Thickfur of the Cold Morning","Foundress of the Spring Burrow","Come in out of the wind, dear, you look frozen. There is clover tea. We can discuss your army after you have warmed your feet.",12,7,7,5,5,
   new[]{"Thickfur","Cold Morning","Clovercot","Mossy Door","Hum Hollow","Mousehole","Foxglove End","Teakettle","Springqueen","Snug","Bramblewarm","Heather Bank"},
   "Foundresses of the Warm Burrow",
   "Big, round, thick-furred bees in bands of black and gold, who nest in old burrows under the grass roots, a few hundred to a town. Their fur lets them fly in cold and drizzle that keeps every other flier at home. They are homely, hospitable and stubborn, shake the pollen out of flowers with a deep hum that others cannot copy, and go armoured in honeycomb plate when a guest forgets his manners.",
   "Every Hearthburrow town begins the same way: one young queen wakes alone at the end of winter, finds an old mouse hole, and raises her first daughters by herself, shivering to keep them warm. The towns stay small, and no bumblebee forgets that her ruler once did all the chores. The Moot is their yearly meeting, at which foundresses compare notes and settle quarrels over tea. Their golden age was the Long Clover, when their thick fur let them fly on mornings too cold for anyone else and they had the early flowers to themselves. The crisis was the Mowing, when a rival cut the clover meadows for fodder in a single week and a third of the burrows starved. The Moot learned to keep stores, post guards and make friends before it needs them. Dame Hobberly is the oldest foundress living. She is kind to strangers, firm about boundaries, and keeps a mace beside the tea things.",
   "Hardy, contented towns that are well fed and hard to take.","Small households; big works take them a long time.",
   "First Out in the Cold",new PeopleRule{Food=1,Content=1,Defence=1},
   "Burrow Mace",2,0,2,0,
   "Warm Ward",35,new PeopleRule{Content=1,Food=1},
   "Small Households",new PeopleRule{Production=-1});

  // Carpenter_Ant.png: a black ant with a red chest, armour of banded wood marked with oak leaves and a huge wooden maul; inspired by carpenter ants, which carve galleries in timber without eating it.
  Add("insect-carpenter-ant","Guild of Galleries","Master-Joiner Tolvane Trueline","Warden of the Grain","Mind the doorframe; it took four generations. You will notice there are no nails anywhere in this hall. We consider nails an admission of failure.",12,2,8,4,3,
   new[]{"Trueline","Sawdust","Dovetail","The Long Gallery","Mortise","Tenon","Stemhall","Shavings","Plumb","Night Shift","Lathe Cross","Grainward"},
   "Carvers of the Long Gallery",
   "Large black ants with russet chests who wear armour of shaped and banded wood and carry mauls and chisels as other peoples carry spears. They carve their cities inside fallen stems and dead trunks, hall above hall, smooth as if sanded, and they work mostly at night. Rank is by craft: apprentice, journeyman, master. They would sooner be thought slow than be caught making something badly.",
   "The Guild does not eat wood; it would consider that an insult to the material. Its people carve their cities inside the fallen stems of the giant grasses and the trunks of dead trees, gallery above gallery, working at night and following the grain. Rank is earned by a masterpiece, and the Master-Joiner is whoever the other masters agree has made the best joint of the age. Their golden age was the Building of the Long Gallery, a carved hall a mile from end to end without a single nail or drop of glue. The crisis was the Rot: a damp decade spread fungus through the oldest timbers, and the heart of the city had to be cut out and burned. The Guild now inspects every beam each spring and trusts no building it has not tapped. Master-Joiner Tolvane measures neighbours the same way. He admires anything well made, by anyone, and will pay any price to see how it was done.",
   "Superb builders; proud, settled cities full of fine work.","They spend freely on perfection and never call a thing finished.",
   "With the Grain",new PeopleRule{Production=2,Content=1},
   "Timber Ram",9,1,1,0,
   "Carved Gallery",30,new PeopleRule{Production=1,Content=1},
   "Never Quite Finished",new PeopleRule{Gold=-1});

  // Hornet.png: a big russet and amber hornet in copper lamellar armour with honeycomb banners and a tasselled glaive; inspired by giant hornets, whose scouts mark bee hives for raiding.
  Add("insect-hornet","Russet Glaive Clans","High Glaive Varrowyn the Hive-Breaker","Mistress of the Autumn Raid","You have a hive. We have counted its doors. Pay the autumn tribute and you will find us courteous, punctual and elsewhere.",12,0,9,7,7,
   new[]{"Ironsting","Autumn Gate","Hivebreak","Russet Keep","Glaive Row","Tasselhold","Copper Eave","Scout's Mark","Tribute Cross","Lamellar","Red Banner","Stinger's Rest"},
   "Collectors of the Autumn Tribute",
   "The largest fliers in the Expanse: russet-headed, amber-banded hornets in suits of copper plates laced with red cord, with a long curved glaive and a banner at the shoulder. They are a warrior nobility organised in clans, each under a clan mother, and they live by tribute taken from the honey-making peoples. Off the battlefield they are formal, punctual and devoted to lacquer work and verse.",
   "The Clans grow no crops and keep no herds. They are the best-armed fliers in the Expanse, and from the first they have lived on what the hives make. Long ago a clan mother noticed that a hive robbed bare is empty the next year, while a hive that pays a tenth is full every autumn, and so the raid became the tribute. Their golden age was the Autumn of Forty Banners, when every comb and paper city on the plains paid on time and the Clans spent the winter on poetry and lacquer. The crisis was the Ball of Heat: a hive that refused to pay swarmed over a clan's scouts and cooked them alive with the warmth of its own bodies. The Clans have scouted more carefully since, and they honour any people that stands its ground. High Glaive Varrowyn broke that hive in the end. She keeps one of its combs in her hall, intact, as a mark of respect.",
   "Heavily armoured shock troops; well-drilled, well-equipped clans.","Proud clan mothers quarrel over rank and banners.",
   "Autumn Raid",new PeopleRule{Attack=1,Production=1},
   "Russet Glaive",7,1,1,0,
   "Tribute Hall",34,new PeopleRule{Gold=2},
   "Proud Banners",new PeopleRule{Content=-1});

  // Leafcutter_Ant.png: a spiny red-brown ant with leaf-sprig badges, a broad curved axe and a shield of layered green leaves; inspired by leafcutter ants, which feed cut leaves to the fungus they farm.
  Add("insect-leafcutter-ant","Parasol Garden Union","Garden-Queen Attavelle of the White Thread","Mother of the White Garden","You are standing on a road my daughters swept this morning. Step to the side, please; forty thousand leaves are due through here before noon.",12,3,2,8,4,
   new[]{"Whitethread","Parasol Road","Greenblade","Leafmeal","The Swept Way","Sporehall","Minim Court","Midden Hill","Canopy Cut","Forty Thousand","Mulchgate","Garden Deep"},
   "Gardeners of the White Thread",
   "Spiny red-brown ants in every size from the thumbnail minims to soldiers with heads like helmets. They cut leaves with broad curved blades, carry them home overhead like parasols along roads swept clean of every twig, and feed them to the pale fungus they farm in deep chambers. They dress in leaf and carry leaf shields, and a single Union city can strip a tree in a night.",
   "The Union does not eat the leaves it cuts. It carries them home along swept roads, chews them to mulch and feeds them to a pale fungus that grows nowhere else; the fungus feeds the city. A founding queen carries a thread of it in her mouth when she leaves home, and every garden in the Union descends from the first. Their golden age was the Age of Green Roads, when leaf columns ran day and night and the gardens under Whitethread filled a thousand chambers. The crisis was the Black Mould, a blight that crept from garden to garden and starved whole cities in a season. It was the smallest workers, the minims, who found the cure, a dust they carry on their own bodies. The Union has honoured its smallest caste ever since and given it a seat in council. Garden-Queen Attavelle thinks in harvests. She needs more leaf every year, and she looks at a neighbour's forest the way a baker looks at flour.",
   "Great farmers and hauliers; large, well-fed cities.","One crop feeds everyone, and they study little else.",
   "White Garden",new PeopleRule{Food=2,Production=1},
   "Parasol Guard",7,0,1,1,
   "Leaf Garden",33,new PeopleRule{Food=2},
   "One Crop",new PeopleRule{Science=-1});

  // Stingless_Bee.png: a small dark bee with cream face stripes, armour of amber resin, amber drops and two short knives; inspired by stingless bees, with their resin nests, narrow entrance tubes and healing pot honey.
  Add("insect-stingless-bee","Amberpot Fellowship","Resin-Mistress Jocelind of the Narrow Door","Warden of the Amber Pots","Welcome, and do mind the doorway; it is narrow on purpose. No, we do not sting. We have knives, resin and very good memories instead.",12,6,1,6,1,
   new[]{"Narrowdoor","Amberpot","Resinwick","Cerumen Hall","Potters' Comb","Stickfast","Tubegate","Tincture","Hollowbough","Bitterhoney","Propolis Row","Ointment Cross"},
   "Apothecaries of the Amber Pot",
   "Small, dark, neat bees with cream stripes on the face and no sting at all. They build inside hollow boughs, behind a single narrow doorway, and store a thin sour honey in round pots of wax and resin. They are the physicians and perfumers of the Expanse. Their armour is hardened amber resin, their weapons are a pair of short knives, and an intruder is more likely to be glued to the floor than stabbed.",
   "The Fellowship's ancestors lost their stings so long ago that nobody remembers how, and made up for it with wit. They build in hollow boughs, seal every crack with resin and leave one narrow door shaped like a tube, which a single guard can hold. Their honey is thin, sour and kept in round pots, and it heals wounds; their resins set bones, sweeten breath and embalm the honoured dead. Their golden age was the Hundred Remedies, when their apothecaries were welcome in every city and a pot of their honey bought a field. The crisis was the Raid of the Robber Bees, cousins who came not for honey but for the recipes, and burned what they could not carry. The Fellowship now writes its knowledge in three codes and teaches no formula whole. Resin-Mistress Jocelind trades with anyone and trusts hardly anyone. She would like to cure every sickness in the Expanse, at a fair price.",
   "Healers, scholars and sellers of rare goods; healthy, settled towns.","No sting: they hold a doorway well and a battlefield badly.",
   "Healing Honey",new PeopleRule{Gold=1,Science=1,Content=1},
   "Amber Knife",3,1,1,0,
   "Salve Market",31,new PeopleRule{Gold=1,Science=1},
   "No Sting",new PeopleRule{Attack=-1});

  // Termite.png: a pale cream termite with a domed orange head and dark jaws, clay-coloured armour and a great stone maul; inspired by termite mounds with their air shafts, and by the king and queen who rule them.
  Add("insect-termite","Claytower Diarchy","Queen-Architect Ummorah of the Cool Shaft","Builder of the Breathing Spire","You will notice the air in here is cool and fresh, though the sun outside could cook a beetle. That is nine hundred years of mathematics. Do not lean on anything.",12,4,4,6,3,
   new[]{"Coolshaft","Claytower","Breathing Spire","Twin Thrones","Mudmortar","Chimney Nine","Mud Buttress","Damp Cellar","Spire of Sums","Fluehaven","Kilnless","Noonshade"},
   "Builders of the Breathing Spire",
   "Pale, soft-bodied people with domed amber heads and dark jaws, most of them blind, who raise towers of baked mud higher than any tree. A Claytower spire is full of shafts and flues that keep the air inside cool by day and warm by night, and of cellars where they grow their food in the damp. They are ruled by a king and a queen together, and are engineers and mathematicians by habit.",
   "Unlike the other mound peoples, the Diarchy has a king as well as a queen, and the two rule for life from a sealed chamber at the base of the tower. The queen plans; the king checks her sums. Their workers are pale, soft and blind, and have built the tallest structures in the Expanse out of mud and spit: spires riddled with shafts that draw cool air up from the cellars and let the heat out at the top. Their golden age was the Raising of the Nine Chimneys, when the towers stood so high that travellers steered by them. The crisis was the Breach, when a hard rain opened the royal tower and ants poured in before the wall could be mended. The Diarchy has studied walls, weather and ants with great care ever since. Queen-Architect Ummorah has not seen daylight in sixty years. She judges other peoples by their buildings, and finds most of them draughty.",
   "Master engineers; comfortable, productive, well-ordered towers.","Blind workers who know little of the world beyond the wall.",
   "Breathing Spires",new PeopleRule{Science=1,Production=1,Content=1},
   "Clay Mauler",2,2,0,0,
   "Air Shaft",37,new PeopleRule{Content=1,Science=1},
   "Blind Builders",new PeopleRule{Sight=-1});

  // Weaver_Ant.png: a slender orange ant dressed in stitched leaves and knotted cord, with a billhook and a lashed leaf shield; inspired by weaver ants, which pull leaves together in living chains and sew them with larval silk.
  Add("insect-weaver-ant","Stitched Canopy League","Knot-Speaker Sarivane of the Living Chain","First Link of the Chain","Look up. Higher. Yes, that whole tree is one town, and it was pulled together by hand, leaf to leaf. Take hold of the rope if you mean to be friends.",12,5,8,5,6,
   new[]{"Living Chain","Stitchbough","Leafseam","Knotwick","High Hammock","Ropewalk","Larva Loom","Canopy Reach","Orchard Watch","Greenstitch","Twelve Trees","Bindleaf"},
   "Weavers of the High Canopy",
   "Long-legged, amber-orange ants who live high in the trees in towns of green leaves pulled together and sewn shut with silk. They dress in leaves bound with plaited cord, hang rope bridges from bough to bough and keep their records as knots. Everything they do is done in teams. They are loyal friends, exact about boundaries, and quick to drop on anyone who climbs their tree unasked.",
   "No weaver can build alone. To make a nest the workers form chains of their own bodies and pull two leaves edge to edge, while others pass along the seam with larvae that give the silk to sew it. The League grew from that: a people for whom letting go is the worst crime there is. They live in leaf towns high in the trees, joined by rope bridges, and keep their records in knotted cord. Their golden age was the Hundred-Tree Peace, when one League held a whole forest and fruit growers of other peoples paid them to guard the orchards. The crisis was the Falling, when a storm tore down the capital tree and the towns, cut off from one another, began to fight over the wreckage. Knot-Speaker Sarivane's grandmother ended that by tying the first treaty cord, which still hangs in the council nest. Sarivane guards her borders fiercely and her friendships more fiercely still.",
   "Close-knit builders and defenders; at home in thick forest.","They seldom come down from the trees to trade.",
   "Living Chain",new PeopleRule{Production=1,Defence=1,Terrains=new[]{11},TileFood=1},
   "Billhook Lasher",7,1,0,1,
   "Stitched Rampart",9,new PeopleRule{Production=1,Content=1},
   "Bound to the Bough",new PeopleRule{Gold=-1});

  // Yellowjacket.png: a bright yellow and black wasp in dark studded plate with warning chevrons on tabard, shield and pennon, and a barbed spear; inspired by yellowjackets' one-year colonies, ground nests and late-summer boldness.
  Add("insect-yellowjacket","Chevron Free Companies","Captain-Queen Vexaline Sharpsummer","Paymistress of the Striped Companies","Good, you are the one in charge. Our rates are posted, our stripes are a warning, and our contracts end at first frost. Whose side would you like us on?",12,7,9,1,0,
   new[]{"Sharpsummer","Chevron Hole","Musterground","Paymaster's Burrow","First Frost","Contract Cross","Blackstripe","Sugar Raid","Pennon Field","Last Muster","Yellow Warning","Ledger Pit"},
   "Soldiers for the Season",
   "Bright yellow and black wasps in dark studded plate, with warning chevrons painted on tabard, shield and spear pennon. They are soldiers for hire. Each company is founded in spring in a burrow under the turf, fights all summer for whoever pays it, and breaks up at the first frost. They are bold, quick-tempered, fond of anything sweet, and careful to the last grain about money.",
   "A Chevron company lasts one year. A queen founds it in spring in a hole in the ground, hires it out all summer, pays off her soldiers at first frost and sleeps through the winter with the profits; her daughters found the next year's companies. So the Companies own little, plan no further than autumn, and fight extremely well for whoever is paying. Their stripes are a livery and a warning, and they paint the same chevrons on shield and banner so that nobody attacks them by mistake. Their golden age was the Summer of Six Contracts, when companies fought on both sides of three wars and were paid by all six. The crisis was the Unpaid Autumn, when an employer defaulted and the hungry companies sacked the city they had been hired to defend. No one has cheated them since, and they keep a public ledger of who tried. Captain-Queen Vexaline is cheerful, blunt and scrupulous. She keeps every contract to the letter and reads the letter very carefully.",
   "Hard-hitting soldiers who turn war into money.","Nothing is stored for winter; the larder is bare by autumn.",
   "Paid in Advance",new PeopleRule{Gold=1,Attack=1},
   "Chevron Spear",7,2,0,0,
   "Paymaster's Vault",39,new PeopleRule{Gold=2},
   "Gone by First Frost",new PeopleRule{Food=-1});
 }
}
}
