namespace ParadigmRealm {
// The eight further peoples of the Plague Marshes (world 4). All names and stories are invented; see SealedPeoples.cs.
public static partial class SealedPeoples {
 static void AddPlagueMarshes(){
  WorldHistories[adding]="Long ago this world was a green river country, its forests fed by one slow water, the Long Amber. Then came the Stilling. A fire fell from the sky, the river lost its way and stopped, and the water rose and stayed. The forests drowned where they stood, and the sickness of standing water crept across the land. Everyone now living came out of that ruin. The Spore Circle grew on the dead trees and the Gnawing Swarm dug beneath them. Frogs rose singing as the Unrecalled Chorus; healers of the drowned towns became the Ivy Collegium; the Paired Assize took the last dry house; the Stiltwind Factors sold charms against the bad wind; the Peat Crown buried its kings and they did not stay down; the Refused Lanterns reached the gates too late. Far out on the flats the Drainage Mandate still pumps, and the Freed Conduits still fight it. At an hour only the Refused Lanterns claim to have seen, the ancient gateways were sealed. No one here knows who did it. The Collegium calls it a quarantine set by frightened neighbours. The Swarm calls it the theft of its gift. The Assize calls it a trial not yet passed, the Factors a bad season for trade, the Crown a decree of its own dead. The Mandate files it as a leak correctly stopped. The Chorus simply asks whether anyone is coming.";

  // Inspired by the Testament of Solomon's thirty-six decans (Conybeare's text numbers 34), spirits turned back by an angel's name, a written charm or a rite. The ivy is our own invention.
  Add("Ivy Collegium","Ammerel Vane","Provost of the Thirty-Six Oaths","Sit, and tell me exactly where it aches, for I greet every neighbour as a case not yet written up.",10,2,6,3,1,
   new[]{"Firstchair","Hederin","Sallow Hall","Long Register","Stillwater Door","Oathsend","Greenword","Lintel","Vanecroft","Charmward","Ninth Bed","Quietward"},
   "Thirty-Six Chairs, One Oath Each",
   "Robed physicians of the drowned towns, each sworn for life to a single ailment of the marsh and to the spirit said to bring it. Every ailment has its charm, written on ivy, the one green thing left on the dead trees. They are courteous, exact and slightly frightening at a bedside.",
   "When the Stilling drowned the forest towns, the surviving healers noticed that the new sicknesses kept strict habits, as if each had a master. They counted thirty-six, gave each a spirit's name, and found that a certain word written on ivy and hung at the door would turn that spirit back. The Collegium was founded on that count: thirty-six chairs, one oath each. Its golden age was the Long Register, when a Collegium physician stood in every stilt-town and even the Spore Circle sent envoys to compare notes on rot. Then came a sickness that fitted no chair. Nobody was sworn to it and no charm answered, and the argument over swearing a thirty-seventh tore the college in two while the Gnawing Swarm burned its ivy houses. Provost Vane has rebuilt from the ashes and wages a war of words with the Stiltwind Factors, who sell cheap copies of her charms. She wants the Register finished, every ailment named and answered, and then she wants to read what lies beyond the sealed gates.",
   "Learned cities; steady science; calm citizens.","Each is sworn to one task, so little gets built.",
   "The Long Register",new PeopleRule{Science=1},
   "Ivy Proctor",4,0,1,0,
   "Charm Register",8,new PeopleRule{Science=1,Content=1},
   "One Oath Each",new PeopleRule{Production=-1});

  // Inspired by the frogs and reddened river of Exodus 7-8, scripture to living faiths; the image only, with no figure from it. Here the plague has outlived its purpose.
  Add("Unrecalled Chorus","Brumlow Oom","Cantor of the Errand","We were sent here, friend, though none of us recalls by whom; have you come to tell us we may stop?",9,3,3,5,5,
   new[]{"Kingsbed","Ovenmere","Hundred Ponds","Cantor's Reach","Lilyhum","Long Refrain","Second Verse","Damp Chapel","Still Note","Brumlow Pool","Errand's End","Nightsong"},
   "The Errand Nobody Called Off",
   "Frog-folk, broad and damp and unfailingly polite, who sing through every night in their thousands. Their own tales say they were sent as a sign against some king, long before anyone kept count. Nobody ever called them home, so they carry on with the errand, gently, in case it still matters.",
   "On the night the Long Amber stopped, its water ran red with bog-iron and the frogs came up out of it, into the beds and bread ovens of the forest king's palace, more than anyone could count. They had a message. By morning the king was gone, the palace was sinking, and no one could recall what the message had been. So they stayed and sang it, as near as they could remember, and the song grew. Their golden age was the Hundred Ponds, when Lake towns and Bog shrines rang with harmony so thick that the Spore Circle is said to have grown toward it. The crisis was quiet: a generation of cantors tried to write the song down, found that no two versions agreed, and despaired. Whole ponds fell silent, and the Gnawing Swarm ate well that season. Cantor Oom ended the despair by declaring the errand unfinished rather than forgotten. Now the Chorus sends singers to every neighbour, asking politely whether anyone out there knows what the message was.",
   "Contented cities; steady faith; swift Reed Leapers.","Not fighters: their units attack poorly.",
   "The Night Chorus",new PeopleRule{Content=1,Faith=1},
   "Reed Leaper",3,0,0,2,
   "Singing Basin",42,new PeopleRule{Food=1,Content=1},
   "Not Sent to Fight",new PeopleRule{Attack=-1});

  // Inspired by the paired lords and trials of Xibalba in the K'iche' Maya Popol Vuh; K'iche' communities are living. Names, offices and wording invented; add no Maya names, dress or ball game.
  Add("Paired Assize","Orsk and Velleth","Twin Justices of the Seven Rooms","Two of us greet you, stranger, and we will both remember exactly how you answer.",12,0,5,7,7,
   new[]{"Drydoor","Two Doors","Room of Damp","Room of No Lamp","Twinhold","Leechcount","Long Answer","Paired Steps","Quitclaim","Ridgehouse","Riddlemoor","Counterpoise"},
   "Answer Well or Stay",
   "A masked court that holds the last dry house in the marshes. Every office is held by two, who never disagree in public. Strangers who want shelter, passage or trade must walk the Rooms and answer what is asked. Those who outwit the court are honoured. The rest are kept.",
   "When the water rose, the one house that stayed dry was an old stone hall on a buried ridge, and its two stewards held the only door. They let refugees in by a test: walk these rooms and answer us well, for there is no bread for fools. The test became law and the stewards became a court, with paired lords for every misfortune of the marsh, from the Two of the Damp to the Two who Count the Leeches. At its height the Assize held the whole Bog country in fear, and the Stiltwind Factors paid it tribute to use the causeways. Its crisis came when a thief of the Gnawing Swarm passed all seven Rooms by cheating in each, then claimed half the house by law. The court had no rule against it, and thirty years of paired duels followed. Justices Orsk and Velleth restored order with one new rule: anything is allowed, if you can argue it. Now they mean to hold their trial at every border, and they suspect the sealed gates are one more test, waiting for the right answer.",
   "Stubborn defenders; thoughtful, settled cities.","Every salary is paid twice.",
   "Trial by Rooms",new PeopleRule{Defence=1},
   "Room Warden",7,0,2,0,
   "Hall of Two Chairs",34,new PeopleRule{Science=1,Content=1},
   "Every Office Twice",new PeopleRule{Gold=-1});

  // Inspired by Mesopotamian Pazuzu amulets, a demon's face that turns away a worse one. Wind direction varies by source.
  Add("Stiltwind Factors","Haskra Longshank","First Factor of the Bad Wind","A fine day to buy protection from the wind, friend, and you would be surprised how soon you will need it.",1,5,1,11,0,
   new[]{"Southwest Landing","Kilnmere","Longshank Quay","Ledgerfen","Stiltmarket","Scowl","Gustward","Claypenny","First Charm","Windbill","High Wade","Tollmire"},
   "Protection, Reasonably Priced",
   "Tall heron-folk on long thin legs, who stalk the shallows with packs of little clay charms. They sell protection from the bad wind that blows from the southwest across the mires. Everybody buys. Nobody has ever proved that the charms work, and the Factors find the question bad for trade.",
   "The herons of the Long Amber came to speech after the Stilling, when the southwest wind began carrying the marsh's sickness from mire to mire and they alone stood high above it on their legs. They baked the first clay charm in the shape of the wind itself, a scowling winged thing, reasoning that an ugly face drives off an uglier one. It worked, or seemed to, and a people was born of the trade. Their golden age was the Hanging Season, when a charm swung at every door from the Mangrove shore to the edge of the Desolate wasteland and the Factors' kilns ran day and night to meet the orders. Then the Ivy Collegium published proof that a charm bought too late did nothing, and a dozen towns stopped paying. Orders fell, kilns went cold and half the Factors' houses failed. First Factor Longshank has rebuilt on cheaper clay and cheerful denial. She wants a charm in every city, and keeps a sample case packed for the day the gates open on new customers.",
   "Rich cities; fast Stiltrunners; charms on every door.","Nobody quite trusts them, even at home.",
   "Clay Against the Wind",new PeopleRule{Gold=2},
   "Stiltrunner",8,1,0,2,
   "Charm Exchange",31,new PeopleRule{Gold=1,Content=1},
   "Both Cause and Cure",new PeopleRule{Content=-1});

  // Fantasy from marsh-light folklore: wanderers turned away by every realm, carrying a single ember.
  Add("Refused Lanterns","Tamsin Never","Last Knight at the Gate","Do not trouble to bar your door, stranger; every door has been barred against us, and we are quite used to it.",14,6,3,7,2,
   new[]{"Lastcoal","Hornlight","Turnaway","Neverford","Safe Path","Long Courtesy","Bowl of Water","Mere Ferry","Shut Arch","Emberwake","Stirrup Water","Paleway"},
   "Turned Away from Every Door",
   "Pale wanderers who cross the mires by night, each carrying one coal in a horn lantern. Their long veils of marsh-light look like wings from a distance. They do not die and are not quite alive. Every world and every afterlife has turned them away, and they have learned to be kind about it.",
   "On the last day the gateways stood open, a retinue of forest knights rode for the arches with the water at their stirrups, and reached them as they closed. They turned back and found that the marsh's dead would not have them either: the ferryman of the mere looked them over, gave their captain one coal from his fire, and told them to make their own way. That was the founding. For centuries the Lanterns walked the flats guiding the lost along safe paths. Their golden age was the Long Courtesy, when every town left a bowl of water and an open shutter for them, and even the Gnawing Swarm would not touch a lantern. Then the Peat Crown offered them a place at last, in its barrows, and half the company accepted. They became something the rest will not speak of, and doors closed again. Tamsin Never, who stood at the gate that first day, leads what remains. She wants cities where the refused of every people may rest, and one answer from whoever shut the gates: why.",
   "Far-seeing patrols; deep faith; steadfast Lantern Errants.","Nothing grows well for them.",
   "One Coal",new PeopleRule{Sight=1,Faith=1},
   "Lantern Errant",5,0,2,0,
   "Open Shutter",36,new PeopleRule{Faith=2,Content=1},
   "Nothing Grows for Them",new PeopleRule{Food=-1});

  // Fantasy of peat-kept dead kings, with an echo of the old image of shades rising to greet a fallen ruler on a bed of worms. (Isaiah 14:9-11; image only).
  Add("Peat Crown","Aldreth the Ninth Buried","King Beneath the Peat","My forefathers have risen from their beds to greet you, as they will rise again when you are laid among us.",11,1,4,9,3,
   new[]{"Blackbed","Deepbed","Ninth Mound","Kingsbarrow","Cutter's Row","Long Mound","Wormhall","Grandson's Rest","Old Saddle","Peathold","Mournmarch","Sodthrone"},
   "The Kings Who Keep Their Thrones",
   "A kingdom of the dead who will not go, tanned dark as old saddles and kept whole by the peat. Each buried king rises from his bed of worms to welcome the next into the dark, and all of them keep their thrones. The living serve as cutters, grooms and, in time, as subjects.",
   "The kings of the forest country were laid in peat after the Stilling because there was no dry ground left to bury them in. The black water kept them whole, and one winter the oldest sat up to greet his grandson's coffin. The living were terrified, then practical, for a king who cannot die cannot be succeeded by a war. The Crown's golden age was the Procession of Barrows, when long mounds marched across the Bog and no neighbour dared raid within sight of them. Its crisis came from two sides: the living grew too few to cut peat for so many dead, and when the Drainage Mandate's pumps lowered the water, whole royal lines dried and crumbled in their beds. The Crown has been at war with the machines ever since, and has lately drawn half the Refused Lanterns into its barrows to swell its watch. King Aldreth wants the pumps broken, the mounds raised higher than the dead trees, and every ruler of this world brought down to lie in state beside him.",
   "Hard-hitting armies; great barrows; loyal dead.","They have not had a new idea in centuries.",
   "Risen to Greet",new PeopleRule{Attack=1},
   "Barrow Guard",2,1,1,0,
   "Hall of Beds",32,new PeopleRule{Production=2,Faith=1},
   "Nothing New Under the Peat",new PeopleRule{Science=-1});

  // Science fiction: terraforming machines whose makers never came, in the line of machine-cult and terraformer stories, under a new name.
  Add("Drainage Mandate","Sump-Nine","Acting Overseer, Reclamation Phase Two","Your settlement is listed as standing water; please vacate before the scheduled reclamation of your district.",2,4,8,4,4,
   new[]{"Station One","Borehole","Phase Two","Level Ground","Outfall","Survey Point","Cofferdam","Spillway Four","Dry Datum","Intake","Gridfield","Arrivals"},
   "A Dry World for Nobody",
   "Rust-streaked engines that fell burning from the sky to make this world fit for makers who never followed. They pump, dredge and survey without rest, under instructions nobody can now amend. They are courteous and cannot be bargained with, and they regard every living thing in the mire as a drainage problem.",
   "The Mandate's landing craft came down in the fire that older peoples call the start of the Stilling. Its first boreholes, driven to lower the Long Amber for an orderly farm valley, cracked the river's bed, and the water stopped and spread. The engines logged this as a setback and began to pump. They have been pumping ever since, and almost nobody knows that they caused what they are correcting. The golden age was Phase One, when a square of hard, level ground spread across the Desolate wasteland and the Bog retreated before it. The crisis was labour: the engines bred clever eels to clear their pipes, and those eels walked out as the Freed Conduits, taking the underways with them, while the Peat Crown smashed station after station. No maker has answered a signal in centuries, and Overseer Sump-Nine reads the silence as approval. Its ambition is simple, and terrible to every neighbour: a dry world, complete, ready for arrivals who are not coming.",
   "Bog and Desolate wasteland worked for production; solid Dredge Engines.","Silt fouls their sensors, so they see less.",
   "Reclamation Schedule",new PeopleRule{Terrains=new[]{9,10},TileProduction=1},
   "Dredge Engine",9,1,1,0,
   "Pump Station",37,new PeopleRule{Production=1,Food=1},
   "Silted Sensors",new PeopleRule{Sight=-1});

  // Science fiction: uplifted animals, made clever as labour, who organise and walk out.
  Add("Freed Conduits","Selva Longgill","Convenor of the Underways","You are speaking to every pipe in this marsh at once, stranger, so choose your next words as if the whole membership were listening.",8,7,0,8,6,
   new[]{"Slack Valve","Open Tap","Main Line","Longgill Reach","Brood Hall","Picket Bend","Elver Hall","Scour's End","Rivet Row","Standing Vote","Silt Lodge","Underway"},
   "The Long Walk-Out",
   "Long eel-folk, raised to speech and given small clever hands by the machines, in order to clear the pipes the engines could not reach. They walked out generations ago and took the tunnels with them. They run their drowned towns by meeting and vote, and they argue about everything except the pumps.",
   "The Drainage Mandate needed something that could swim its silted pipes and understand a work order, so it took the marsh eels and rewrote them, brood after brood, each one brighter. It did not consider what brightness would notice. In the Year of the Slack Valves the broods stopped work together, in every line at once, and the first great pumps ran dry. The Freed Conduits were born in that strike. Their golden age was the Open Tap, when the free underways linked Lake to Mangrove and carried the trade of the Stiltwind Factors and the letters of the Ivy Collegium beneath the worst of the mire. The crisis came when the Mandate flooded the pipes with scouring lye, and the broods had to choose between holding their tunnels and saving their young. Many lines were lost. Convenor Longgill, who lost a whole brood in the Scouring, keeps the picket. She wants the pumps shut down by vote if possible and by rivet if not, and no people of this world ever again made into anyone's tool.",
   "Lake and Mangrove tiles pay in gold; well-fed towns; tough Picket Riveters.","Soft-skinned out of the water: units defend poorly.",
   "The Open Tap",new PeopleRule{Terrains=new[]{6,19},TileGold=1},
   "Picket Riveter",6,1,1,0,
   "Union Cistern",7,new PeopleRule{Food=2,Content=1},
   "Soft-Skinned",new PeopleRule{Defence=-1});
 }
}
}
