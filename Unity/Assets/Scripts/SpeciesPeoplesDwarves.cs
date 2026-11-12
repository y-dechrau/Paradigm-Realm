namespace ParadigmRealm {
// The ten dwarven civilisations of the Stonehold Deeps (world 7; owner request, 12 Nov 2026). See SealedPeoples.cs for the rule
// class and SpeciesPeoples.cs for the Add signature. All invented; each draws on a real craft, trade or institution.
public static partial class SpeciesPeoples {
 static void AddDwarves(){
  WorldHistories[adding]="The Stonehold Deeps are a cold, high world where one mountain range runs into the next and the valleys between hold thin strips of grass and pine. The dwarves say they were not born here but dug their way in, following a seam of iron from somewhere warmer, and that the tunnel closed behind them. The first halls were mines, the mines became cities, and the cities were joined by roads that never see the sky. Iron made them strong and gold made them quarrel: the War of the Split Seam divided one people into many holds, and each has kept its own craft, law and grudge. Brewers, bankers, clockmakers, monks and raiders now share the same mountains and seldom the same opinion. The sealed gateways stand on the high passes, rimed with frost. Most holds agree that anything worth having is already under their feet.";

  // Inspired by the tin and coal miners of Cornwall and Wales: shift work, pit props, safety lamps and the miners' union.
  Add("dwarf","Deepvein Compact","Delve-Warden Brunna Ironlamp","Keeper of the Lowest Lamp","Mind your head, stranger, the ceiling was cut for us. If you have come about ore, sit down. If you have come about anything else, sit down anyway and we will get to ore.",3,1,8,7,6,
   new[]{"Deepvein","Lowest Lamp","Pickfall","Shaftmouth","Ore Cross","Timber Prop","Canary Gate","Black Adit","Sump Hall","Winch Head","Ninth Level","Brunna's Cut"},
   "Delvers of the Lowest Lamp",
   "Broad, soot-marked dwarves with cropped beards tucked into leather aprons, who live by the shift bell in towns built around a single great shaft. Every adult belongs to a crew, every crew to a level, and every level sends a voice to the Compact. They eat together, dig together and share the ore by weight, and they distrust anyone who has never held a pick.",
   "The Compact began as nine mining crews trapped by a roof fall in the Year of Bad Air. They dug towards each other for forty days, met in the dark, and swore that no crew would ever again dig alone. Their golden age was the Rich Century, when the Ninth Level struck iron and gold in the same seam and every hold in the Deeps bought Compact steel. The crisis was the Split Seam itself: when other holds claimed the lode, the Compact flooded its own lower galleries rather than share them. Delve-Warden Brunna has spent thirty years pumping them dry. She holds that the mountain gives only to those who stay together, and that a neighbour is someone who has not yet tried to dig under you.",
   "Rich mines in hills and mountains; stubborn defenders.","They see little of the world above their own shafts.",
   "The Long Shift",new PeopleRule{Terrains=new[]{2,5},TileProduction=1},
   "Shaft Guard",7,0,2,0,
   "Pithead Works",30,new PeopleRule{Production=2},
   "Sun-Shy",new PeopleRule{Sight=-1});

  // Inspired by the monastery breweries and beer halls of Bavaria and Belgium, and the barley terraces that feed them.
  Add("dwarf","Amber Hall Brewers","Brewmother Hildra Goldenfoam","First Taster of the Amber Hall","Welcome, welcome! No one talks business here with a dry throat. Drink first, then tell me what you want, and then drink again while I think about it.",3,3,2,1,5,
   new[]{"Amberhall","Barleyterrace","Maltings","Cooper's Row","Hopgarth","Kegbridge","Foamwell","Yeast Cellar","Tapster's Rest","Stoutgate","Harvest Stair","Goldenfoam"},
   "Keepers of the Amber Hall",
   "Round-cheeked, red-bearded dwarves in linen shirts and copper-buckled belts, who farm barley and hops on stepped terraces in the few green valleys of the Deeps. Their towns are built around a great hall with long tables and cool cellars cut into the hillside. They settle most disputes over a shared cask and count a guest's empty mug as an insult to the house.",
   "The Brewers were valley farmers whom the mining holds looked down on, until the Thirsty Winter, when the deep wells froze and only the Amber Hall's cellars held anything fit to drink. That winter made their fortune. Their golden age was the Time of Open Taps, when every hold sent wagons for Amber ale and every treaty in the Deeps was signed at a Brewer table. The crisis came with the Blight of the Grey Ear, which killed the barley for three harvests and emptied the halls. Brewmother Hildra crossed the mountains on foot to find clean seed, and came back with it sewn into her coat. She believes that a people who drink together rarely fight, and she has the ledgers to prove it.",
   "Well-fed, contented cities; welcome guests at every table.","Their scholars do their best thinking after lunch, which is to say rarely.",
   "Valley Barley",new PeopleRule{Content=1,Terrains=new[]{0},TileFood=1},
   "Hall Bouncer",2,0,2,0,
   "Great Taproom",42,new PeopleRule{Content=1,Gold=1},
   "Foggy Heads",new PeopleRule{Science=-1});

  // Inspired by the clockmakers of Switzerland and the Black Forest, and by the town clock as the ruler of working life.
  Add("dwarf","Cogwrights of Tannik","Master Escapement Orvin Tocke","Winder of the Great Clock","You are four minutes late, though you could not have known that. Come in. I can spare you eleven minutes, and I suggest we do not waste the first one on the weather.",3,5,6,8,1,
   new[]{"Tannik","Mainspring","Pendulum Hall","Gearwell","Brasswind","Chimegate","Balance Wheel","Hourstone","Ratchet Lane","Cogmere","Striking Tower","Quarter Bell"},
   "Winders of the Great Clock",
   "Neat, narrow-eyed dwarves with waxed grey beards, brass eyeglasses and waistcoats full of small tools. They live in tidy stone towns where every street can see a clock tower, and they rise, eat, work and sleep by its bell. They build clocks, locks, scales and engines of astonishing fineness, and they find untidiness of any kind physically painful.",
   "Tannik was a poor hold with no ore worth the digging, so its smiths learned to make small things well instead of large things often. The first Great Clock was built to share out the hours of a single water wheel between forty workshops. It worked so well that the hold began to run everything by it. Their golden age was the Age of True Time, when Tannik clocks hung in every hall of the Deeps and Tannik engineers were paid their weight in gold. The crisis was the Stopped Hour, when the Great Clock seized and the whole hold stood idle and arguing for a month. Master Orvin rebuilt it with two movements, so that one may always check the other. He admires precision in a neighbour and forgives almost anything else.",
   "Fine science and industry; engineers without equal.","They trust mechanism more than any god.",
   "Measured Hours",new PeopleRule{Science=1,Production=1},
   "Spring Arbalest",4,1,1,0,
   "Horologe Guild",38,new PeopleRule{Science=2},
   "Cold Reason",new PeopleRule{Faith=-1});

  // Inspired by the banking houses of Renaissance Florence and Augsburg: ledgers, letters of credit and loans to kings.
  Add("dwarf","Ledger Houses of Guldmar","First Teller Marga Sevenseals","Holder of the Master Ledger","Good day. Before we speak, be aware that this conversation is being minuted, and that our friendship, should it arise, will accrue interest.",3,2,1,5,0,
   new[]{"Guldmar","Sevenseals","Countinghouse","Strongroom","Assay Court","Tally Bridge","Coiner's Deep","Surety","Notary Steps","Bullion Gate","Interest","Vault of Promises"},
   "Holders of the Master Ledger",
   "Sleek, well-fed dwarves with oiled black beards bound in gold rings, dressed in dark velvet and never without a seal on a chain. They live in quiet, guarded cities of counting houses and vaults, and most of them have never lifted a pick. They lend to every hold in the Deeps, remember every debt, and prefer a signed contract to a drawn axe.",
   "Guldmar was founded by seven assayers whom the mining holds trusted to weigh gold fairly, because the seven distrusted each other too much to cheat. They soon found that keeping other people's gold paid better than digging their own. Their golden age was the Peace of the Ledger, when no hold could afford a war without a Guldmar loan and so there were very few wars. The crisis was the Great Default, when three holds refused their debts at once and the vaults of Guldmar stood nearly empty. First Teller Marga called in every favour owed for a century and broke the defaulters without raising a single company. She regards war as an expensive way to lose customers, and peace as something that can be bought if one starts early.",
   "Great wealth; every neighbour is a client.","Their soldiers are hired, and fight like it.",
   "Letters of Credit",new PeopleRule{Gold=2},
   "Bonded Company",6,1,1,0,
   "House of Seals",39,new PeopleRule{Gold=2},
   "Hired Courage",new PeopleRule{Attack=-1});

  // Inspired by Norse raiders and by the ski troops and winter warfare of the far north.
  Add("dwarf","Frostpeak Reavers","High Reaver Skadrik Wolfcloak","Lord of the Winter Raid","Ha! You walked up here on your own legs, so you are braver than you look. Say what you have, and I will say whether we trade for it or take it.",3,0,9,11,7,
   new[]{"Frosthorn","Wolfcloak Hall","Sledge Haven","Axefall","Icebeard","Raider's Col","Snowblind","Plunder Stair","Cairn of Shields","Whitefang Pass","Longwinter","Avalanche Gate"},
   "Wolves of the Winter Raid",
   "Lean, weather-burned dwarves with braided white-blond beards, wolf-fur cloaks and round shields painted with their kin marks. They live above the snow line in timber halls dug half into the rock, where nothing grows and little can be mined. They hunt, herd a few goats and, when the passes freeze hard enough to bear a sledge, come down on skis to take what the lower holds have stored.",
   "The Reavers descend from crews cast out of the lower holds during the War of the Split Seam, who were given the high peaks because nobody else wanted them. They learned to live on snow and to move across it faster than any army could follow. Their golden age was the Nine Winters, when Frostpeak sledges came home loaded every year and the richest holds paid them to raid someone else. The crisis was the Battle of the Thaw, when an early spring trapped a whole raiding host in the valleys and few returned. High Reaver Skadrik was a boy who carried his father's shield home from that field. He raids less often than his grandfathers did and chooses his targets better, and he has no respect at all for a people who cannot defend their own larder.",
   "Fierce raiders who profit from war; quick to claim new ground.","Restless and quarrelsome whenever there is nothing to raid.",
   "Winter Raid",new PeopleRule{Attack=1,Gold=1},
   "Ski Raider",2,1,0,1,
   "Hall of Spoils",32,new PeopleRule{Gold=1,Production=1},
   "Restless in Peace",new PeopleRule{Content=-1});

  // Inspired by rock-cut monasteries such as those of Ethiopia and Cappadocia, and by monastic vows of silence.
  Add("dwarf","Silent Stair Order","Abbot Thurn the Unspeaking","Carver of the Last Step","(The abbot hands you a slate.) It reads: You are welcome here. Please speak softly. The mountain is listening, and so are we.",3,6,3,6,2,
   new[]{"Silent Stair","Chisel Cloister","Echoless","Thousand Steps","Slatehand","Vesper Cut","Quiet Cell","Graven Vow","Lampless Chapel","Novice Landing","Whisper Gallery","Last Step"},
   "Carvers of the Silent Stair",
   "Spare, calm dwarves with shaven heads and long undyed beards, dressed in grey wool and stone dust. They live in monasteries cut downward into solid rock, one step and one cell at a time, and most of them have taken a vow never to speak aloud. They talk with slates and hand signs, carve their scriptures into the walls, and believe the mountain itself is slowly reading them.",
   "The Order was founded by a mine foreman called Thurn the First, who lost his crew to a cave-in caused, he believed, by his own shouted order. He never spoke again, and began carving a stair down to where they lay as a penance. Others joined him. Their golden age was the Thousand Steps, when the Stair passed its thousandth tread and pilgrims from every hold came to read the walls. The crisis was the Loud Heresy, when a faction of young monks declared that silence was pride and began to preach in the lower galleries; the roof fell there within the year. The present abbot took the founder's name and his vow. He holds that most of the world's troubles come from speaking too soon, and he will wait a long time for a neighbour to finish a sentence.",
   "Deep faith and patient learning; libraries cut in stone.","A people who will not speak make poor traders.",
   "Carved Scripture",new PeopleRule{Faith=1,Science=1},
   "Vow-Keeper",2,0,1,0,
   "Rock-Cut Cloister",36,new PeopleRule{Faith=2},
   "Vow of Silence",new PeopleRule{Gold=-1});

  // Inspired by Alpine herders who move their flocks up to summer pastures, and by mountain cheese-making.
  Add("dwarf","High Fold Ramherds","Flockmother Edda Longhorn","Shepherd of the Thousand Rams","Sit by the fire and have some cheese. The rams have already looked you over and they did not butt you, which is the best reference a stranger can get up here.",3,4,2,10,4,
   new[]{"High Fold","Longhorn","Summer Pasture","Shearing Green","Cheese Cave","Bellwether","Ram's Leap","Wintering","Drover's Notch","Saltlick","Cloudmeadow","Woolmarket"},
   "Riders of the Thousand Rams",
   "Wiry, sun-browned dwarves with wind-tangled chestnut beards, felt hats and heavy wool coats hung with brass bells. They live in the open, which other dwarves find alarming, moving great flocks of mountain rams between valley and high pasture with the seasons. They ride the largest rams into battle, age their cheese in caves and know every path across the hills.",
   "The Ramherds say their first mother was a miner's daughter who followed a lost ram out of the tunnels, saw the sky and refused to go back in. Her children kept the flock and the habit. Their golden age was the Green Decades, when mild summers opened the high meadows and Ramherd wool clothed half the Deeps. The crisis was the Fence War, when the mining holds began walling off the valleys and the flocks starved on the wrong side of the stone. The Ramherds answered by riding, and found that a charging ram is a persuasive argument. Flockmother Edda has more rams than she has grass, and she looks at every empty hillside, and a good many occupied ones, as pasture that has not yet been introduced to her flock.",
   "Rich hill pastures; swift ram riders; sharp eyes.","Scattered camps build little and build slowly.",
   "High Pastures",new PeopleRule{Sight=1,Terrains=new[]{2},TileFood=1},
   "Ram Rider",8,0,1,1,
   "Shearing Hall",31,new PeopleRule{Gold=1,Food=1},
   "Scattered Folds",new PeopleRule{Production=-1});

  // Inspired by the master masons and cathedral builders of medieval Europe, whose works took generations to finish.
  Add("dwarf","Vaultbuilders of Karst Harrow","Grand Mason Dorrik Keystone","Raiser of the Hundred Arches","Stand there a moment. No, a little to the left. Good. Now look up. That vault took my grandfather forty years, and it will still be standing when both our peoples are a footnote.",3,7,4,8,3,
   new[]{"Karst Harrow","Keystone Deep","Hundred Arches","Flying Buttress","Plumb and Level","Capstone Hall","Mason's Mark","Ashlar","Scaffold Town","Quarryface","Lintel Gate","Cornerstone"},
   "Raisers of the Hundred Arches",
   "Heavy-shouldered dwarves with square-cut iron-grey beards, pale with stone dust, who wear a mason's apron as other peoples wear a crown. They live in vast vaulted halls that are never quite finished, with scaffolding as a permanent part of every skyline. A dwarf of Karst Harrow is judged by the work that carries their mark, and the proudest of them begin buildings they know they will not live to see roofed.",
   "Karst Harrow began as a quarry that sold dressed stone to richer holds. Its masons grew tired of seeing their work credited to others, kept the best blocks and began to build for themselves. Their golden age was the Raising, three centuries in which the Hundred Arches went up one by one and every lodge in the Deeps sent apprentices to learn the craft. The crisis was the Fall of the Ninety-Fourth, when an arch built in haste to please a visiting lord collapsed during its dedication. Since then no mason of Karst Harrow may be hurried, on pain of exile. Grand Mason Dorrik has seven arches left to raise and expects to finish two. He measures every people by what they leave standing, and thinks poorly of those who leave only tents.",
   "Great builders; solid, proud cities; strong siege craft.","The stone is fed before the people are.",
   "Built to Last",new PeopleRule{Production=1,Content=1},
   "Siege Mason",9,1,1,0,
   "Masons' Lodge",9,new PeopleRule{Production=2},
   "Stone Before Bread",new PeopleRule{Food=-1});

  // Inspired by the law-speakers of the Icelandic Althing, who recited the whole law from memory at an open assembly.
  Add("dwarf","Graven Bench Lawspeakers","Lawspeaker Ingra Stonevoice","Reciter of the Whole Law","I greet you under the first law, which protects guests, and remind you of the second, which concerns their behaviour. There are four hundred and six more. We have time.",3,3,5,7,5,
   new[]{"Graven Bench","Stonevoice","Oathring","Final Verdict","Witness Hall","Old Precedent","Moot Hollow","Twelve Plaits","Gavelstone","Fairhearing","Appeal Stair","Wergild"},
   "Reciters of the Whole Law",
   "Grave, straight-backed dwarves with long white beards divided into twelve plaits, one for each seat of the bench, and plain blue robes over mail. They live in orderly towns built around a moot hall, where any dwarf may bring a case and every verdict is cut into the wall. They would rather argue for a year than fight for a day, and they keep their word to the letter.",
   "After the War of the Split Seam, twelve survivors from twelve ruined holds met in a hollow and agreed that the war had begun because no two dwarves remembered the same law. They cut one law into one bench and chose a speaker to learn all of it by heart. Their golden age was the Long Hearing, when holds across the Deeps brought their quarrels to the Bench instead of the field, and paid well for the service. The crisis was the Case of the Two Heirs, which the Bench heard for sixty years while the estate in question fell into ruin. Lawspeaker Ingra ended it in an afternoon, and has been both praised and sued for it since. She believes that peace is simply law that everyone has agreed to remember, and she watches closely to see who forgets.",
   "Orderly, contented cities; steadfast in defence; trusted arbiters.","Nothing is built until everyone has been heard.",
   "Rule of Law",new PeopleRule{Content=1,Defence=1},
   "Oath Bailiff",7,1,1,0,
   "Hall of Verdicts",34,new PeopleRule{Content=1,Gold=1},
   "Due Process",new PeopleRule{Production=-1});

  // Inspired by the Roman roads and the relay runners of the Inca road system: surveyors, milestones and way stations.
  Add("dwarf","Under-Road Lampwalkers","Roadmistress Halka Farlight","Surveyor of the Sunless Mile","Well met! I have walked nine hundred miles of tunnel and never once met someone coming the other way from outside the world. Tell me everything, starting with the roads.",3,5,1,11,1,
   new[]{"Farlight","Milestone One","Waystation","Lantern Junction","Toll Arch","Sunless Mile","Courier's Rest","Crosscut","Surveyor's Peg","Long Heading","Echo Bridge","Roadhead"},
   "Walkers of the Sunless Mile",
   "Long-legged, by dwarven standards, and always a little travel-worn, with short practical beards, oilskin coats and a lantern at the belt. They live strung out along the great tunnel roads in way stations a day's walk apart, and many are born, married and buried without ever leaving the road. They carry letters and goods for every hold, map everything they pass, and cannot see a blank wall without wondering what lies behind it.",
   "The Lampwalkers were once the tunnel crews of a hold whose name they no longer use, sent to cut a road to a distant seam. The hold fell while they were digging. With no home to return to, they kept the road instead, and then extended it. Their golden age was the Joining, when the Under-Road at last linked every great hold of the Deeps and a letter could cross the world in twenty days. The crisis was the Closing of the Tolls, when rival holds blocked the tunnels against each other and the couriers were stranded in the dark between gates. Roadmistress Halka reopened them one bargain at a time. She thinks of the sealed gateways as the only roads she has not yet walked, and she finds it very hard to think about anything else.",
   "Far-reaching trade and scouts; quick to spread along new roads.","Way stations are thinly held and easily cut off.",
   "The Under-Road",new PeopleRule{Gold=1,Sight=1},
   "Tunnel Courier",3,0,0,2,
   "Provision Depot",7,new PeopleRule{Food=1,Gold=1},
   "Stretched Thin",new PeopleRule{Defence=-1});
 }
}
}
