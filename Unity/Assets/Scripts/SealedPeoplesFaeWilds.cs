namespace ParadigmRealm {
// The eight further peoples of the Fae Wilds (world 1). All invented; see SealedPeoples.cs for the rule class and Add.
public static partial class SealedPeoples {
 static void AddFaeWilds(){
  WorldHistories[adding]="The Fae Wilds began as mist over silver water. The Sylvan Elves say they woke first, under the first stars; the Standing Kin, who were already there, have not yet finished disagreeing. Storm-eagles taught the first flocks of the Aerie to speak, and the Orchard Without a Door stood shut at the centre of everything, its seep turning every lake to balm, which is why wounds close here faster than anywhere. Then lights fell from the sky. The Unsleeping came down to teach, and their huge children, the Hungering, ate the land bare. The Lingering Folk fled into the hills and the Underglass through the lakes; elves and Aerie, at war over the canopy, made peace on the last star to fall. Afterwards came the quieter powers: the Tallied Court, who count every gift left at the stone circles; the Grove Survey, whose ship sank in a lake under three days of dark cloud; and the Lumen Accord, the thought in the glowing roots. The gateway arches stand in the deep woods, older than any of them will admit, and nothing passes. Nobody knows who sealed them. The elves call it a wall against the marsh rot. The Balmwalkers say the hand that shut the Orchard shut these too. The Court calls it a debt in default. The Survey logs it as an anomaly. The Kin were asked, and are still answering.";

  Add("The Balmwalkers","Tessaly Oun","Walker of the Long Petition","You look tired, friend; sit, let us see to that wound, and then tell us if you have news of the wall.",10,0,3,2,5,
   new[]{"Firstmile","Wallshade","Seepwater","Longwalk","Mercy Ford","Thornwell","Alder Hospice","Vialstead","Kneeling Green","Lastmile","Ennet's Rest","Salvebrook"},
   "Pilgrims of the Closed Orchard",
   "A gentle, road-worn people who live along the pilgrim paths leading to the Orchard Without a Door, the walled wood at the heart of the Fae Wilds. They keep hospices, gather the healing seep that runs beneath its wall, and still walk to that wall to ask, politely, to be let in.",
   "Their story begins with a dying grandmother and a child sent to find a cure. The child, Ennet, walked to the Orchard's wall and begged at its only gap, where a wheel of pale fire turns without fuel. Nothing answered, but a thread of oil ran out from under the stones, and that was enough to ease the old woman's death, though not to stop it. Others followed Ennet's road, and villages grew along it. In their golden age the Hundred Hospices took in the wounded of every war, Sylvan Elves and Aerie lying side by side, and no one was asked whom they had fought. Then the Hungering came down from the hills and ate the hospice gardens, and the pilgrims learned what their refusal to strike first would cost. The survivors kept the vow anyway, and lost much of the road. Today, led by Tessaly Oun, they are rebuilding it hospice by hospice. They want only two things: a road safe from the sleeping groves of the Lumen Accord, and one clear answer from the wall.",
   "Calm, devout cities; stubborn staff-bearers; welcome almost everywhere.","Sworn never to strike first: poor attackers.",
   "The Orchard's Seep",new PeopleRule{Content=1,Faith=1},
   "Staffbearer",7,0,2,0,
   "Hospice of the Seep",33,new PeopleRule{Food=1,Content=1},
   "The Vow of the Road",new PeopleRule{Attack=-1});

  Add("The Unsleeping","Serevane the Unlidded","Speaker of the Bound Star","We taught your kind fire and the edge of a blade; I have not closed my eyes in nine ages, so choose your next words with care.",14,1,6,9,7,
   new[]{"Ornath's Fall","Lidless Hall","Vigil Height","Ember Scarp","Brightforge","Petition Stair","Starcutter","Tallmere","Skyward Rest","Unlit Gate","Wakehold","The Long Watch"},
   "Teachers Who Fell Burning",
   "Tall, radiant, winged exiles and their descendants, who do not sleep and do not forget. They came down in falling light to teach the lakeside peoples, and they are still, in their own view, owed thanks. Their halls stand on high ground above the silver lakes, and none of them has closed their eyes since.",
   "The old tales of the Wilds say they were once sky-lights set to watch the silver lakes, and that they came down because they could no longer bear merely watching. These lakes show the sky so exactly that a watcher might mistake the water for home. They taught the lake peoples metal, gem-cutting, the reading of stars and the making of swords, and for an age their halls were the brightest things on the ground. Their children grew enormous and were named the Hungering; when the harvests ran out, these ate the herds, then the villages. Something in the sky answered by throwing down their lord Ornath, whom the Sylvan Elves and the Aerie then took as the fallen star of their oath. Ever since, the Unsleeping have sent petitions upward asking for pardon, and no reply has come. Serevane now says the petition needs a better messenger. She wants Ornath's stone back from the elves, by treaty or by sword, and she wants to learn what the Hungering still under the hills would do if someone woke them.",
   "Ancient knowledge; hard-hitting swordsmen; forges no one else can match.","An endless unanswered plea: cities stay restless.",
   "Forbidden Teaching",new PeopleRule{Science=1,Attack=1},
   "Lidless Blade",2,2,0,0,
   "House of Petition",30,new PeopleRule{Production=1,Science=1},
   "The Long Refusal",new PeopleRule{Content=-1});

  Add("The Tallied Court","Ambril Quince","Keeper of the Exact Ledger","You have walked on our grass, and that is entered; you have also bowed, and that is entered too, so let us see where we stand.",12,2,5,9,6,
   new[]{"Ninering","Evencount","Duskmark","The Fair Copy","Hazel Tithe","Quittance","Ring of Thanks","Redress","Moonreckon","Candle Audit","Lastfavour","Pledgestone"},
   "Keepers of Every Kindness",
   "Masked, courteous fae who gather at the stone circles and leave the gifts the Wilds are famous for. A gift from the Court is never free and never cruel: a kindness is repaid to the grain, an insult to the hair, and both are written down. They have never once been known to round up.",
   "The stone circles of the Wilds hold the echoes of whatever is said inside them, and the first of the Court were little more than those echoes, given masks. They learned that a promise spoken in a circle can never quite be unsaid, and built a whole law upon it. In their golden age, the Long Reckoning, every quarrel in the Wilds came to them, and an elf and an Aerie captain could settle a feud over one stolen feather with an exact exchange of insults. The crisis came from their own books. An auditor found that every circle they met in had been borrowed, without a word of thanks, from the Standing Kin, and that the debt, running since before the ledger began, could never be repaid. The Court nearly tore itself apart in the Bitter Audit, when masked riders took names and good fortune in settlement. Ambril Quince has restored order. She intends to have every people of the Wilds in her ledger, owing a little, so that someone will finally be in a position to clear the Court's own debt.",
   "Sharp bargains; firm in defence of a slight; swift hare-riders.","Every obligation comes before their own work: slow production.",
   "The Exact Return",new PeopleRule{Gold=1,Defence=1},
   "Hare-Rider of the Tally",3,1,0,2,
   "Ledger Circle",34,new PeopleRule{Gold=2,Content=1},
   "Bound by the Letter",new PeopleRule{Production=-1});

  Add("The Lingering Folk","Dorn Ellowmere","Lord of the Unended Noon","Is the winter over yet? No matter; come in, the harps are tuned, and I shall need your report on the giants.",3,3,7,4,2,
   new[]{"Evenmound","Harpswell","Gold Barrow","Longnoon","Turfgate","Stillhollow","Underhay","Dimhallow","Barrowsong","Honeyhollow","Latebell","Greenbarrow"},
   "The Folk of the Hollow Hills",
   "A small, fair-spoken folk in hollow hills where silver springs rise and time pools like water. One season inside passes while a century goes by above. They keep their harps tuned, their tables laid and their old war plans ready, and cannot step on outside turf without paying for every missed year at once.",
   "When the Hungering stripped the southern hills, the Lingering Folk's king led his people into the hollow mounds, meaning to wait out a single winter. The silver springs in those hills hold time as lakes hold rain, and the winter did not end. Within, it was their golden age: one long noon of feasts, harpers and treasure-working, in which no one grew old. Then envoys rode out to the Sylvan Elves to ask for news. One dismounted at the elven border, and the centuries he had missed fell on him at once; his companions carried back dust and a single glove. Only then did the hill learn how long it had been gone. Dorn Ellowmere, who was a young man that afternoon and still is, commands his riders never to touch the ground. He is still, by his own calendar, at war with giants bound for ages past, and his counsellors are only now, gently, trying to tell him. He wants the Underglass, who shared the old flight, to teach him a way out that does not kill.",
   "Rich hill farms; great halls; riders who never dismount.","All news reaches them centuries late: slow research.",
   "The Hollow Larders",new PeopleRule{Terrains=new[]{2},TileFood=1},
   "Turfless Rider",8,1,0,2,
   "Hall of Harps",42,new PeopleRule{Content=2},
   "Late to Everything",new PeopleRule{Science=-1});

  Add("The Grove Survey","Ansa Kell","Surveyor-General, Ninth Expedition","Please hold still and state your species; you are the first entry in this column, and I would like to get it right.",2,4,6,8,1,
   new[]{"Base Camp One","Chart Room","Sample Station","Hull Landing","Benchmark","Control Plot","Datum","Transect End","Instrument Row","Lakeshore Annex","Station Nine","Anomaly Pending"},
   "Census-Takers of the Unexplained",
   "The descendants of a seed-ship's crew, mended over the generations with salvaged parts and lake silver until it is hard to say where the person stops and the instrument begins. They carry on a mission nobody living remembers ordering, and they refuse to use the word magic, preferring 'anomaly, pending'.",
   "Their ship came down in three days of dark cloud, carrying sleepers meant to turn a dead planet green. It woke them above a world already the greenest thing their instruments had ever recorded, and then it sank in a silver lake. With the mission pointless, they did the only thing the manual allowed and began to survey. Their golden age was the Great Census: every grove numbered, every stone circle measured, charts exchanged with the star-keepers of the Aerie, and one memorable core sample taken from a boulder that turned out to belong to the Standing Kin. The crisis arrived quietly. The lake's silver was eating the hull and the archive with it, and their oldest logs became unreadable. A schism split those who would finish the survey from those who would stop counting and simply live here. Surveyor-General Ansa Kell holds the majority and the last working chart room. Her ambition is modest and unshakable: complete the census, then open the gateway arches, the only structures in the Wilds that return no reading at all.",
   "Tireless research; skilled workshops; salvage firearms.","They plant by an old manual written for another world: thin harvests.",
   "Field Notes",new PeopleRule{Science=2},
   "Hull Guard",6,1,1,0,
   "Sample Archive",38,new PeopleRule{Science=2,Production=1},
   "Planting by the Manual",new PeopleRule{Food=-1});

  Add("The Lumen Accord","Nine Hundred Groves","Present Voice of the Accord","We have been listening through your roots for some time; do sit down, you look so tired.",7,5,2,11,4,
   new[]{"Greywillow Scar","Softlight","Rootmeet","Dreaming Alder","The Quiet Stand","Palecap","Lanternwood","All-Together","Threadmoor","Glowfall","Long Rest","Welcome Hollow"},
   "The Thought Beneath the Groves",
   "A patient mind that lives in the thread-fungus under the glowing groves. Its citizens are pale, capped figures grown from the roots, and travellers who slept too long under the trees and woke up agreeing with everyone. They are unfailingly kind. They say we, and they have never understood why anyone would want to say no.",
   "The glow in the groves of the Wilds comes from a thread-fungus that feeds on the silver in lake water, and for ages it was only light. Then it grew long enough to carry a signal from one grove to the next, and the first foresters who lay down among the roots began to wake with one another's memories. The Accord's golden age, the Quiet Thousand Years, was truly gentle: no one in those groves was lonely, lied or went hungry, and many came willingly. The crisis was the Burning at Greywillow, when Sylvan Elf wardens set fire to a grove they thought diseased. Every mind in the network felt it at once, for eleven days. The Accord had never known fear, and it learned quickly. Since then it has told the Aerie and the Balmwalkers alike that it means no harm, and it is telling the truth as it sees it. The voice now called Nine Hundred Groves proposes simply that there should be no more burning, and that the only sure way is for every forest, and everyone who walks in one, to be included.",
   "Forests think for them; full granaries; spreads without tiring.","One pain for all: a wound anywhere weakens every defender.",
   "Root Thought",new PeopleRule{Terrains=new[]{1},TileScience=1},
   "Sporecaster",4,1,1,0,
   "Sleeping Hall",7,new PeopleRule{Food=2,Science=1},
   "One Pain for All",new PeopleRule{Defence=-1});

  Add("The Underglass","Verrine of the Still Side","Sovereign of the Reverse Shore","You dropped something in our ceiling, some years ago; I am sure we can agree on a fair price for its return.",8,6,0,1,0,
   new[]{"Reverse Shore","Stillside","Lostmarket","Silverskin","Ripple Quay","Anchorfall","Fosterhall","Mirror Reach","Ring Exchange","Underbank","Crooked Tide","Thrownsword"},
   "The Kingdom Under the Silver",
   "A court of pale lake-dwellers living beneath what only looks like water. The silver surface of the great lakes is a skin, and under it lies a reversed country with its own dim sky. Everything ever lost in a lake is theirs by right, and they trade it back with immaculate manners.",
   "When the Hungering came down to the lake shores, a fishing clan waded out to drown rather than be eaten, and found that the water let them through. Below the silver skin lay a country that was the shore turned over, left and right reversed and a dim sky beneath their feet. In their golden age they grew rich on everything the upper world dropped: rings, anchors, swords thrown away in grief. They sold these back through the Tallied Court, and they raised abandoned children found on the banks as the Fosterlings, returning them to the surface grown, armoured and loyal. The crisis came with the falling star on which the Sylvan Elves now swear. Its impact cracked the skin of the largest lake, and the Great Ripple broke half the capital out of true; there are streets there that still run into themselves. Verrine, who rules from the side that survived, is buying every sliver of star-metal in the Wilds to mend the surface. She would also very much like to know what the Grove Survey's sunken ship is doing in her ceiling.",
   "Lakes and rivers pay well; rich markets; fierce fostered knights.","They see the upper world only in reflection: short sight.",
   "Everything Lost",new PeopleRule{Terrains=new[]{6,7},TileGold=1},
   "Fosterling Champion",5,2,0,0,
   "Hall of Lost Things",31,new PeopleRule{Gold=2,Science=1},
   "Seen in Reflection",new PeopleRule{Sight=-1});

  Add("The Standing Kin","Old Harrowdown","Eldest of the Slow Ring","Stay a while, small quick one, and by autumn we shall have answered you.",13,7,4,8,3,
   new[]{"First Ring","Harrowdown","Sunwise","Great Dance","Lichenhold","Ninth Stride","Mistworn","Long Shadow","Coldshoulder","Kinstones","Ringmoor","Slowstep"},
   "Those Who Were Already Here",
   "Great figures of living stone, worn smooth by mist, who stand in rings on the hills of the Wilds and walk, when they walk at all, about a stride a year. They are older than anything else that speaks. A conversation takes a generation, and they are generally the only ones present at both ends.",
   "Ice once dragged boulders across these hills and left them in the mist off the silver lakes, and the silver soaked in for longer than anyone can reckon. One day, a boulder had an opinion. It took the Kin a thousand years to wake one another, and they have been in no hurry since. Their golden age was the Great Dance, when every ring on the hills shifted one place sunwise over nine centuries, and the Sylvan Elves made a song about it, believing it was the hills that moved. The crisis, by their count, is recent: the Hungering used sleeping Kin as throwing stones, and later the Grove Survey drilled a core sample from an elder's shoulder. It took ninety years to notice and another forty to object. Old Harrowdown now leads by being the one most others are facing. The Kin mean to close one great ring around the whole of the Wilds, and they regard the Tallied Court's circles as a temporary misuse of their relatives, which they will mention in due course.",
   "Hills and mountains work for them; vast building projects; very hard to move.","Merchants give up waiting for a reply: poor trade.",
   "Patience of Rock",new PeopleRule{Terrains=new[]{2,5},TileProduction=1},
   "Menhir Hurler",9,1,2,0,
   "The Slow Ring",36,new PeopleRule{Faith=1,Production=2},
   "The Long Silences",new PeopleRule{Gold=-1});
 }
}
}
