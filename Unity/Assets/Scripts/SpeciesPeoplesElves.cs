namespace ParadigmRealm {
// The ten elven civilisations of the Fae Wilds (world 1; owner request, 12 Nov 2026: one species to a world).
// See SpeciesPeoples.cs for the Add signature (the art key first) and SealedPeoples.cs for the rule class.
// All invented; each draws on a real culture, trade or idea named in the comment above it.
public static partial class SpeciesPeoples {
 static void AddElves(){
  WorldHistories[adding]="The elves say the Fae Wilds were sung, not made, and that the song has never quite finished. Its groves glow after dusk, its lakes lie like poured silver and its hills keep their own slow weather. Every people here is elven, but they stopped being one people long ago. The oldest tale they share is the Parting of the Ten Roads, when the first great court quarrelled over what the light in the groves was for. Some said learning, some worship, some trade, and some said it was for nothing at all and should be left alone. Each party took a road and did not come back. Ancient gateways stand sealed in the deepest glades. Every court has a different story about who shut them, and each story blames another court.";

  // Inspired by the Hanseatic League of trading towns and by prehistoric lake villages built on piles.
  Add("elf","Argent Lake League","Admiral-Queen Sevelle of the Still Water","Mistress of the Nine Harbours","Mind the gangplank, stranger. If you have come by land I am sorry for your feet, and if you have come to trade I am glad of your purse.",0,3,0,1,0,
   new[]{"Stillharbour","Nine Quays","Pearlwake","Lanternquay","Swanmoor","Heron Stair","Mistferry","Silverkeel","Reedhaven","Tidewatch Isle","Oarsong","Glasswater"},
   "Sailors of the Silver Lakes",
   "Tall, pale elves with silver-grey hair worn in sailors' braids, dressed in oiled linen with pearl-shell buttons. They live in towns built on piles over the silver lakes and travel everywhere by swan-prowed boat. A League child can sail before it can run. They judge a person by the state of their hull and the honesty of their weights.",
   "The League began as nine fishing villages that agreed to share one lighthouse, and then found that sharing a harbour law was cheaper than fighting over it. Their golden age was the Pearl Century, when League boats carried grain, glass and gossip to every shore of the silver lakes and nobody else could afford a fleet. The crisis came in the Winter of Hard Water, when the lakes froze from shore to shore for the first time and the trade stopped for two years. Half the League's houses went bankrupt, and the rest learned to keep a full granary and a second trade. Admiral-Queen Sevelle was elected by the nine harbour masters, which makes her a queen who can be voted out. She believes that anything worth having can be reached by water, and that anything that cannot be is someone else's problem.",
   "Rich lake trade and fast boats on every shore.","Uneasy and outmatched far from the water.",
   "Lakewise",new PeopleRule{Terrains=new[]{6},TileFood=1,TileGold=1},
   "Swanprow Marine",7,1,0,1,
   "Harbour Exchange",31,new PeopleRule{Gold=2},
   "Unsteady Ashore",new PeopleRule{Defence=-1});

  // Inspired by the Library of Alexandria, monastic scriptoria and the glow of bioluminescent fungi.
  Add("elf","Glowgrove Lyceum","Archivist Thereval the Unhurried","Warden of the Lit Shelves","Please lower your voice, the groves are reading. Now, where are you from, and how do you spell it?",0,5,6,4,1,
   new[]{"Lampleaf","Quillrest","Inkwell Glade","Thousand Shelves","Candlebough","Vellum Hall","Brightmoss","Scholar's Ring","Glowmere","Index","Marginal","Last Chapter"},
   "Readers by Grove Light",
   "Slender, ink-fingered elves in long grey-green robes, who wear their hair cut short so that it does not fall on the page. Their cities are libraries built among groves of glowing trees, and they read by that light all night. Every citizen must copy one book in a lifetime. They are kind to guests and tend to forget that guests need feeding.",
   "The Lyceum was founded by a single scribe who noticed that the glowing groves grew brighter when someone read aloud beneath them. Nobody has explained it, and the Lyceum has spent four thousand years trying. Their golden age was the Age of the Thousand Shelves, when every court in the Fae Wilds sent its children to study there and paid in books. The crisis was the Dimming, a blight that put out a third of the groves in one season. The scholars copied by candle for a generation and lost a great many eyes and manuscripts doing it. The groves recovered, but the Lyceum has kept its candles. Archivist Thereval has held the post for six hundred years and is in no hurry about anything. He thinks most wars could be avoided if both sides read the last one properly.",
   "Unmatched learning and great libraries that draw the whole world.","Slow to build and slower to decide.",
   "Reading Light",new PeopleRule{Science=2},
   "Lyceum Sentinel",4,0,2,0,
   "Lit Shelves",8,new PeopleRule{Science=2},
   "No Hurry",new PeopleRule{Production=-1});

  // Inspired by the horse archers of the steppe and by old folk tales of a ghostly hunt riding through the autumn sky.
  Add("elf","Thornhunt Clans","Huntmistress Kaerwyn Stag-Rider","First Spear of the Autumn Ride","You walked here? On your own legs? Then you are braver than you look or slower than you should be. We shall see which.",0,1,9,10,7,
   new[]{"Antler Camp","Redbriar","Hornfall","Stag's Leap","Autumn Ride","Hound Hollow","Spearmeadow","Bramblefire","Longchase","Trophy Rock","Rutting Field","Windmane"},
   "Riders of the Red Stag",
   "Lean, sun-browned elves with copper hair tied back with thorn pins, dressed in leather and russet wool. They ride great red stags across the open grassland and live in hide camps that move with the herds. A clan's wealth is counted in antlers and in the stories told about its riders. They despise walls, ledgers and anyone who has never slept outdoors.",
   "The Clans descend from the huntsmen of the first great court, who rode out one autumn and decided not to come back to a hall where they had to wipe their boots. They tamed the red stags of the grassland and learned to shoot from the saddle. Their golden age was the Hundred Rides, when no caravan crossed the plains without paying the Thornhunt in salt and iron. The crisis came when the clans turned on each other in the Feud of the Broken Antler, which lasted three generations and ended only when there were too few riders left to continue it. Huntmistress Kaerwyn united the survivors by beating every other chief in single combat, one after another, in a single week. She respects those who ride out to meet her. Those who hide behind a gate she considers game.",
   "Fast, fierce riders who see trouble coming from far off.","Little learning and no love of building.",
   "The Autumn Ride",new PeopleRule{Attack=1,Sight=1},
   "Stag Lancer",8,1,0,1,
   "Hunt Lodge",32,new PeopleRule{Food=1,Production=1},
   "No Patience for Books",new PeopleRule{Science=-1});

  // Inspired by medieval cathedral masons and the mortarless terraces and stairways of the Inca.
  Add("elf","White Stair Masons","Mason-King Orren the Level","Layer of the Last Step","Stand there a moment. Yes, you are slightly out of true. Never mind, most things are until we have finished with them.",0,7,4,8,3,
   new[]{"Whitestair","Keystone","Plumbline","Quarryhead","Terrace Royal","Chiselford","Archway","Hundred Steps","Capstone","Mortarless","Highcourse","Lintel"},
   "Builders of the White Stair",
   "Broad-shouldered elves, short for their kind, with stone dust in their pale hair and hands like mallets. They wear aprons of white leather and carry a plumb line as others carry a sword. Their cities climb the hills in terraces joined by great white stairways, every block cut so finely that no mortar is needed. They trust what has been measured twice.",
   "The Masons say their founder was a gardener who built a single step so that her old mother could reach the top of a hill, and then could not see a reason to stop. Their golden age was the Reign of Seven Stairs, when each ruler added a terrace to the capital and a new stairway to reach it, and the hill became the tallest built thing in the Fae Wilds. The crisis was the Great Slip, when a spring of heavy rain brought the fifth terrace down upon the fourth and buried two thousand people. The Masons spent a century learning about drains. Mason-King Orren rebuilt the fifth terrace himself as a young stonecutter and was crowned on it. He measures other rulers by what they will leave standing, and he finds most of them short.",
   "Tireless builders whose hill cities are rich in stone.","They would rather quarry than farm, and their tables show it.",
   "Hill Quarries",new PeopleRule{Production=1,Terrains=new[]{2},TileProduction=1},
   "Stair Guard",7,0,2,0,
   "Masons' Yard",30,new PeopleRule{Production=2},
   "Stone Before Bread",new PeopleRule{Food=-1});

  // Inspired by the oracle at Delphi and the old practice of scrying, reading the future in still water.
  Add("elf","Starglass Votaries","Oracle Vesmira the Veiled","First Reader of the Reflected Sky","We saw you in the water nine nights ago. You were taller in the reflection. Be welcome all the same.",0,6,3,6,2,
   new[]{"Starglass","Veil Shore","Omen Reach","Nightmirror","Seer's Step","Hushwater","Third Vision","Candle Isle","Pilgrim's End","Dreamers' Cloister","Low Moon","Answer"},
   "Readers of the Reflected Sky",
   "Quiet, dark-haired elves who go veiled in fine blue-black gauze sewn with small glass beads. They live on the shores and islands of the stillest lakes, where the night sky is reflected without a ripple, and they read the future in that reflection. They speak softly, eat little and keep careful records of every omen and whether it came true.",
   "The first Votary was a ferrywoman who saw a star in the lake that was not in the sky, and three nights later saw it rise. Her followers built the first cloister on that shore and have watched the water ever since. Their golden age was the Era of Clear Nights, when kings would not marry, sow or march without a reading, and the cloisters grew rich on gifts. The crisis was the False Dawn, when the oracles foretold a comet that never came, and half the faithful walked away in a single season. The Votaries answered by writing down every reading and checking it, and they found to their surprise that this made them better at it. Oracle Vesmira keeps the tally herself. She believes the reflected sky shows what ought to happen, and that faithful people should help it along.",
   "Devout and learned, with seers who are rarely surprised.","They wait for a sign when they should strike.",
   "Reflected Sky",new PeopleRule{Faith=1,Science=1},
   "Veiled Seer",3,0,1,1,
   "Scrying Pool",36,new PeopleRule{Faith=2},
   "Waiting on Omens",new PeopleRule{Attack=-1});

  // Inspired by the caravans of the Silk Road, the Baltic amber trade and the merchant houses of Venice.
  Add("elf","Amber Road Company","Factor-Prince Dellamar Honeytongue","Holder of the Long Purse","A new face, and I hope a new customer! Sit, drink, and tell me what your people have too much of.",0,2,1,5,0,
   new[]{"Amberfair","Tollbridge","Caravan Green","Weighhouse","Longpurse","Honest Scale","Crossroads Elm","Sapmarket","Bargain Ford","Gilded Bough","Ledger Hill","Farthing"},
   "Sellers of Bottled Light",
   "Smiling, well-fed elves with honey-coloured hair, dressed in layered travelling coats with a great many pockets. They live in market towns strung along the old forest roads and deal in amber, tree sap, dyes and anything else that can be carried. Every child learns four languages and double-entry bookkeeping. They are generous hosts and remember every debt.",
   "The Company started as a family of sap-tappers who discovered that the hardened sap of the glowing trees kept its light for a hundred years. They sold the first jar of amber light to a king for the price of a village. Their golden age was the Long Fair, a market that ran without closing for sixty years and moved one town along the road each season. The crisis was the Counterfeit Summer, when a rival flooded the roads with painted glass and nobody would buy amber at any price. The Company bought every fake, smashed them in public and has stamped its goods ever since. Factor-Prince Dellamar inherited the stamp and a great many partners. He would rather buy a quarrel than fight it, and he keeps soldiers the way he keeps umbrellas, for other people's weather.",
   "Wealthy traders with friends at every court.","They pay others to pray and hold little sacred themselves.",
   "Open Roads",new PeopleRule{Gold=2},
   "Caravan Guard",8,0,1,1,
   "Amber Bourse",39,new PeopleRule{Gold=2},
   "Nothing Sacred",new PeopleRule{Faith=-1});

  // Inspired by the hermit monks of the Egyptian desert and the mountain ascetics of Japan, moved into a forest.
  Add("elf","Hollow Oak Anchorites","Elder Brannoch the Seldom-Seen","Keeper of the Closed Path","You found the path. That was not meant to be possible. Say what you came to say, quietly, and then let the hedge close.",0,4,3,7,6,
   new[]{"Closed Path","Hollow Oak","Silence","Mossgate","Hermit's Burrow","Unseen Glade","Rootcell","Watchful Fern","Thicket Hold","No Road","Stillbough","Far Enough"},
   "Hermits of the Closed Path",
   "Spare, weathered elves with moss-green cloaks and hair left to grow wild, often with leaves in it. Each lives alone in a cell cut into a living oak, and a city is simply a wood where many such trees stand within earshot. They keep vows of silence for years at a time. Strangers find the paths to their groves grown over with thorn, and this is deliberate.",
   "The Anchorites began with a courtier who grew tired of talk, walked into the deep forest and sat down inside a hollow oak. Others who were tired followed her and were careful not to sit too close. Their golden age was the Great Quiet, three centuries in which no Anchorite grove was entered by an army or a tax collector, because none could find the way. The crisis came when the Thornhunt burned a path in and sacked nine groves in a summer. The hermits learned archery with the same patience they had given to prayer, and the Thornhunt has not come back. Elder Brannoch speaks perhaps ten times a year. He asks only that the world stay on its own side of the hedge, and he has arranged matters so that it generally does.",
   "Hard to find and harder to dislodge from their forests.","They trade with almost no one and are poorer for it.",
   "Forest Fastness",new PeopleRule{Defence=1,Terrains=new[]{1},TileProduction=1},
   "Path Warden",4,1,1,0,
   "Thorn Hedge",9,new PeopleRule{Content=1,Faith=1},
   "Shut Gates",new PeopleRule{Gold=-1});

  // Inspired by the open-field villages and common land of medieval Europe, where strips were shared out by lot.
  Add("elf","Greensward Commons","High Reeve Maelinne Barleyhand","Speaker of the Shared Furrow","Come in, come in, you look half starved. There is bread on the table and a spare scythe by the door, whichever you would like first.",0,0,2,8,5,
   new[]{"Barleymead","Shared Furrow","Orchard Row","Honeyfield","Scythe Green","Milldance","Harvest Home","Clover Commons","Granary Hill","Plumthorpe","Haywain","Seedbarrow"},
   "Keepers of the Shared Furrow",
   "Cheerful, freckled elves with straw-gold hair under wide hats, dressed in undyed linen with embroidered hems. They live in large open villages surrounded by shared fields, orchards and beehives, and they decide everything at a meeting after harvest. Nobody among them is very rich or very poor. They sing while they work and consider a fenced field bad manners.",
   "The Commons were founded by the kitchen servants and field hands of the first great court, who were left behind at the Parting of the Ten Roads and found they managed well without their betters. They divided the meadow into strips and drew lots for them every seventh year, as they still do. Their golden age was the Fat Decades, when Commons grain fed half the Fae Wilds and their harvest fairs lasted a month. The crisis was the Year of Two Frosts, which killed the seed corn and forced them to beg from the Company at a ruinous price. Since then every village keeps three years of grain and trusts no merchant. High Reeve Maelinne was chosen by lot, like every reeve before her. She wants good neighbours, long summers and enough new meadow for the grandchildren.",
   "Plentiful food and contented villages.","Farmers make reluctant soldiers.",
   "Shared Furrow",new PeopleRule{Content=1,Terrains=new[]{0},TileFood=1},
   "Scythe Levy",2,0,1,0,
   "Tithe Barn",7,new PeopleRule{Food=2},
   "Farmers First",new PeopleRule{Attack=-1});

  // Inspired by the Icelandic Althing with its law rock, and by judges riding a circuit to hear cases in the open.
  Add("elf","Even Bough Tribunal","Lady Justice Aurenne the Unswayed","First Seat of the Even Bough","State your name, your people and your business, in that order. You will be heard fairly. Everyone is.",0,5,5,7,5,
   new[]{"Evenbough","Oathstone","Verdict","Witness Green","Twelve Seats","Lawspeaker's Rock","Plea","Fair Hearing","Gavelwood","Assembly Field","Precedent","Last Appeal"},
   "Judges of the Even Bough",
   "Grave, upright elves in plain black robes with a single white collar, their dark hair bound with a cord for each year served on the bench. They live in orderly towns laid out around an assembly field and a law rock. Every dispute, from a stolen goose to a stolen crown, is heard in the open air. They are scrupulously fair and rather hard to hurry.",
   "The Tribunal began when two feuding courts agreed to let a shepherd judge between them, since she had no stake in either. Her ruling pleased nobody and was obeyed by both, and people have been bringing her successors their quarrels ever since. Their golden age was the Peace of Twelve Seats, when the Tribunal's judges rode a circuit through every elven land and their word ended wars. The crisis was the Bought Verdict, when a single judge took amber from the Company and the whole circuit was shut out for a century. The Tribunal now tries its own judges first and hardest. Lady Justice Aurenne unseated her own teacher for a smaller fault than that. She holds that a border is a kind of promise, and she takes a broken promise personally.",
   "Orderly, loyal cities and steady soldiers.","Nothing is done until it has been properly heard.",
   "Rule of Law",new PeopleRule{Content=1,Gold=1},
   "Bailiff of the Bough",7,1,1,0,
   "Hall of Even Measure",34,new PeopleRule{Content=1,Gold=1},
   "Due Process",new PeopleRule{Production=-1});

  // Inspired by Polynesian wayfinders settling island after island, and by seed carried on the wind.
  Add("elf","Driftseed Wayfarers","Pathfinder Tolveth Far-Sown","Bearer of the First Seed","What fine empty hills you have behind you! Do not look so worried. We only want to plant a few trees.",0,1,3,11,4,
   new[]{"First Seed","Windsown","Thistledown","Next Hill","Far Pasture","Onward","Tentgrove","Wanderer's Well","Newleaf","Seventh Camp","Horizon","Not Yet Home"},
   "Planters of the Last Road",
   "Wiry, wind-burned elves with pale flyaway hair, wrapped in patched cloaks of many greens. They travel in long walking columns with handcarts and goats, and wherever they camp for a season they plant a grove and leave a few families to tend it. They carry seed in pouches round their necks and count their age in groves planted, not in years.",
   "The Wayfarers hold that the first elves were told to plant the whole world and that the other courts stopped too soon. They took the last of the ten roads and never reached the end of it. Their golden age was the Green Century, when a new Wayfarer grove took root every spring and their columns were welcomed as a blessing. The crisis came when they reached the edges of other peoples' land and went on planting. The Masons pulled up a whole season's saplings at the Battle of the Uprooting, and the Wayfarers have gone armed since. Pathfinder Tolveth carries the oldest seed pouch, said to hold an acorn from the first grove. He means no harm to anyone. He simply believes that empty land is unfinished, and that most land is emptier than its owners think.",
   "Quick to settle new land and hard to surprise.","Their young towns are thinly defended.",
   "Sown on the Wind",new PeopleRule{Food=1,Sight=1},
   "Wayfinder",3,0,0,2,
   "Seed Shrine",33,new PeopleRule{Food=1,Faith=1},
   "Shallow Roots",new PeopleRule{Defence=-1});
 }
}
}
