namespace ParadigmRealm {
// The eight invented peoples of the Ashen Waste, with the Iron Mind and the Ashforge Clans (CivLore 31, 32) the ten of that world.
public static partial class SealedPeoples {
 static void AddAshenWaste(){
  WorldHistories[adding]="Before the fire the Ashen Waste had a name, and nobody now agrees what it was. Its people, remembered only as the Makers, raised cities of brass and glass on a green continent. The old tales say two hundred strangers came down upon a mountain and taught them metal, sky charts and weapons, and that the strangers' huge children, the Unfed, ate the harvests bare. Hunger became war, and war became the Burning, a single season in which the sky caught fire and the plains fused. The Ashforge Clans shut their doors and lived. The Iron Mind kept working. Salt fell where rain had been, and the Salt Vigil stayed to mourn what it covered. Flames that would not go out became the Unquenched Host, and the Firebreak Companies have fought them ever since. Carrion birds who fed on the dead woke as the Ossuary Parliament. The Seated Kings still dine in their tombs, the Oilbearers sing to machines that do not listen, and the Holt of Last Dew guards the only trees left. The ancient gateways stand sealed, and nobody knows by whom. The Vigil calls them the Makers' one mercy, keeping fire from other worlds. The Clans call them a debt unpaid. The Kings say they hold the key and will use it when properly fed. The Iron Mind records only that a lock implies a keyholder.";

  // Inspired by the giants and hungering spirits of 1 Enoch, and its oath-bound teachers of forbidden crafts.
  Add("The Unfed","Ummoth Never-Full","Eldest of the Unfed","You smell of bread, small one, so speak quickly before I forget that we are talking.",13,3,9,11,4,
   new[]{"Gorgemouth","Hollowbelly","Marrowgate","Oathstone","Bindingfoot","Greygullet","Crumbfall","Lastgrain","Millstone Deep","Ummoth's Rest","Cairn of Spoons","Tithebarrow"},
   "Children of the Oath, Still Hungry",
   "Grey-skinned giants descended from the two hundred strangers who came down the Mountain of Binding and taught the Makers their crafts. They have eaten everything this world ever grew. Now they walk the ash plains in slow columns, always hungry, with long memories and a terrible courtesy toward whatever they are about to consume.",
   "The strangers swore an oath together on the Mountain of Binding, then came down as teachers, gave the Makers swords and star charts, and married among them. Their children grew three times the height of any Maker, and for a century were honoured as builders and paid in whole granaries. That was the golden age the Unfed still sing of. Then the tribute stopped being enough. They ate the stores, the herds and, the songs admit, the neighbours, and the Makers' war against them lit the fires that ended the world. After the Burning there was nothing left to eat, yet they did not die, and they have not been full since. Under Ummoth Never-Full they have learned to break into the cellars of the Ashforge Clans and to bargain with the Salt Vigil for brine. Ummoth now walks toward the sealed gateways, certain that someone sealed them to keep a meal from his people.",
   "Huge and strong in battle; long tables keep their cities calm.","Every city eats more than it can grow.",
   "Weight of Hunger",new PeopleRule{Attack=1},
   "Ravener",2,2,1,0,
   "Long Table",7,new PeopleRule{Food=1,Content=2},
   "Never Filled",new PeopleRule{Food=-1});

  // Inspired by the land burned to salt and the still-smoking wasteland of Deuteronomy and Wisdom: the fire-and-salt image only.
  Add("Salt Vigil","Mother Orsenne Averted","Warden of the Standing Names","Walk gently, stranger, for this road runs between my grandmothers, and do not look behind you until you are home.",10,6,1,3,0,
   new[]{"Whitequay","Brinewake","Pillarfield","Saltmother","Crustmere","Lastlook","Palehaven","Thousand Names","Silent Pans","Dryrain","Greymourn","Averton"},
   "Mourners Among the Pillars",
   "Pale-robed mourners and salt miners of the burned shore, where the fire-rain left a white crust over the dead and over standing shapes that were once people. They name every pillar, sell the salt around it, and walk their streets with their eyes kept forward, for looking back is the one thing their custom forbids.",
   "On the last day of the Burning a thin bitter rain fell along the western shore and left the fleeing crowds standing white where they stopped. The survivors were those who had not turned to watch. They stayed, because they could not abandon the pillars, and they learned that the salt could be sold: to the Ashforge Clans for curing, to the Unfed for brine, and later to the Iron Mind, which has a use for it nobody has explained. Their golden age was the Long Reckoning, when every pillar had a name and caravans queued at Whitequay. Then storms came off the sea, the oldest pillars began to dissolve, and whole families of names were lost in a winter. The Vigil now roofs its dead with salvaged brass and buys any old record that might name the rest, though custom forbids reading further than a name. Mother Orsenne wants a roof over every pillar before the sea takes them.",
   "Salt pans make barren shores pay; their wardens hold the line.","They will not study what lies behind them.",
   "Pans of the Burned Shore",new PeopleRule{Terrains=new[]{18,10},TileGold=1},
   "Pillar Warden",7,0,2,0,
   "House of Standing Names",36,new PeopleRule{Faith=2,Gold=1},
   "Never Look Back",new PeopleRule{Science=-1});

  // Inspired by the deified royal ancestors of Ugarit, who received offerings from their descendants.
  Add("Seated Kings","Hadrun Eldest-Seated","First at the Table of Kings","Sit, guest, and eat, for we who cannot have always enjoyed watching.",11,1,3,4,2,
   new[]{"Hadrun's Table","Ninth Chair","Moonmeal","Barleyhall","Stonefeast","Verdict","Crownrest","Old Supper","Highseat","Ashbread","Longsitting","Heirsway"},
   "The Dead Who Still Hold Court",
   "Skeletal monarchs of the old plains, seated upright at stone tables in their tomb cities, and the living families who serve them. The dead are fed, consulted and obeyed; each new moon the descendants lay out ash-bread and thin wine and wait for a verdict, which usually comes. Theirs is a courteous, patient and rather stifling realm.",
   "Long before the Burning the plains kings were laid in sealed halls and given a meal at every new moon, so that they might go on advising their heirs. When the sky burned, the heirs fled into those same halls, and the kings, to general surprise, sat up. Hadrun Eldest-Seated explained that nobody had ever stopped feeding them, and so they had never quite finished. The Long Supper followed, four centuries in which every dead king kept a court and the living coaxed thin barley from tomb courtyards for the offerings. The crisis came when the barley failed and families began serving ash in place of bread. The insulted kings rose in anger, and the Table War divided the realm until the Ossuary Parliament, which had eaten several of the disputed heirs, testified to the true lines of descent. Hadrun now claims the kings hold the key to the gateways, and need only a table set with food from another world to turn it.",
   "Faith flows in every city; verdicts of the dead settle quarrels.","Every king's table must be paid for.",
   "Offerings Received",new PeopleRule{Faith=2},
   "Bier Knight",5,1,1,0,
   "Hall of Verdicts",34,new PeopleRule{Content=2,Gold=1},
   "The Kings Must Eat",new PeopleRule{Gold=-1});

  // Inspired by the two survivors of the Norse world-burning, who live on dew in a sheltered wood through the great winter.
  Add("Holt of Last Dew","Wenna Dewhand","Keeper of the Last Holt","Wipe the ash from your boots before you come in, friend, for we have one forest and no spare.",12,2,2,5,6,
   new[]{"Oakring","Dawnwet","Netted Hollow","Orrel's Walk","Wenna's Rest","Thirdwinter","Greenlast","Terrace Nine","Seedfall","Little Holt","Mossward","Firstleaf"},
   "The Last Green Thing",
   "Bark-masked foresters who keep the only living wood in the Ashen Waste, a walled hollow that the fire somehow passed over. They drink what the leaves gather each dawn, know every tree by name and are wary of guests. The masks filter out the ash, which they fear above all else, for it carries sparks.",
   "When the Burning reached the eastern hills, a woman and a man named Wenna and Orrel hid in a hollow beneath a ring of old oaks. The fire went round it. For three winters without summer they lived on dew taken from the leaves, and when the smoke thinned they walked out and found nobody. The Holt descends from them and from the stragglers they took in. Its golden age came when terrace walls and dew nets spread the green across nine valleys and the first seed caravans reached the Salt Vigil. Then a Firebreak crew arrived to warn of a seam fire beneath the roots, and the Holt refused to let them dig. The seam broke through, and six valleys burned in a month. Since then the Holt has trusted the Firebreak Companies and hardly anyone else. The present Keeper, Wenna Dewhand, bears the founder's name and means to carry cuttings across the Waste before the Unquenched Host finds the rest.",
   "Food in a land without it; gardens that lift the heart.","They will not fell a tree, and their workshops suffer.",
   "Dawn Harvest",new PeopleRule{Food=2},
   "Thorn Warden",4,1,1,0,
   "Dew Terrace",33,new PeopleRule{Food=2,Content=1},
   "The Uncut Wood",new PeopleRule{Production=-1});

  // Inspired by the machine cults of early science fiction (Forster's worshippers of a machine that does not care).
  Add("Oilbearers","Cantor Mesk Vaunt","First Cantor of the Still Engine","We have oil enough for you as well, stranger, if you will kneel where it can see you.",2,5,8,3,3,
   new[]{"Vespergate","First Pump","Greasewell","Cantorhall","Stillworks","Brass Hymn","Oilrest","Valvemere","Kneeling Yard","Gearsong","Bellows Deep","Silent Piston"},
   "Servants of One Who Never Asked",
   "Humans who have spent forty generations tending the abandoned engines of the Makers, and who now wear brass braces and oil-dark robes and sing to machinery. They believe the Iron Mind is the world's forgiveness made solid. The Iron Mind has never once replied, and they regard its silence as a test of their devotion.",
   "The first Oilbearers were maintenance crews who came up from the Burning with no city left and one pump still running. They kept it running, then the next, and their work songs became hymns. When Iron Mind patrols found them, the machines simply stepped round them and let them work, which the Oilbearers took for a blessing. In the Age of Turning they restored whole valleys of engines, and their choirs carried for miles across the wasteland. The crisis was the Stripping, when the Iron Mind took the Great Still Engine at Vespergate, their holiest machine, for parts, without notice. A third of the faithful walked away, and the rest decided they had simply not oiled it well enough. Their piety has since hardened, and those who doubt are asked to kneel inside the pumphouses. Cantor Mesk Vaunt intends to seize every machine of the Ashforge Clans as an offering, in the hope that the Mind will finally speak.",
   "Work and worship are one; their guards do not give ground.","Cities seethe with unanswered prayer.",
   "Consecrated Labour",new PeopleRule{Production=1,Faith=1},
   "Oilsworn Guard",6,0,2,0,
   "Oil Choir",30,new PeopleRule{Production=1,Faith=2},
   "The Silence of the Mind",new PeopleRule{Content=-1});

  // Inspired by uplifted animals in science fiction: creatures raised to speech to do one dangerous job.
  Add("Firebreak Companies","Chief Sessk Ninth-Bell","Chief of the Forty-Thousandth Shift","Forty-thousandth shift reporting, neighbour: is anything of yours on fire?",9,4,7,8,5,
   new[]{"Ninth Station","Bell Seven","Great Cut","Sandhouse","Roster Hall","Valve Shut","Standby","Muster Point","Back Draught","Seam End","Last Alarm","Trench Four"},
   "The Shift That Never Ended",
   "Soot-scaled lizard people, raised by the Makers from desert monitors to walk into burning works and shut the valves. The Makers are gone, but nobody ever told the crews to stand down. They wear the old helmets, keep a strict roster, answer every bell and are unfailingly polite about the fact that the world is still on fire.",
   "The Makers' laboratories gave the salt monitors hands, speech and a love of procedure, then sent them into every fire too hot for anyone else. On the day of the Burning each crew went out as ordered, and most did not return. Those who did found the coal seams under the wasteland alight, set to burn for a thousand years, and their last orders said nothing about stopping. Their golden age was the Great Cut, when trenches across half the continent starved the worst seams and the ash first settled. The crisis came when the fires began to fight back: the Unquenched Host rode out of the steppe and burned the Ninth Station with its crew still on duty. A long war has followed, which the Companies, who hate killing anything, wage badly. Chief Sessk of the forty-thousandth shift means to put out the last fire. Nobody has dared ask what the Companies will do after that.",
   "They work burning ground others flee; fast, tough scouts.","Raised to rescue, they make poor attackers.",
   "Walk Through Fire",new PeopleRule{Terrains=new[]{10,17},TileProduction=1},
   "Smoke Jumper",3,0,1,2,
   "Bell Station",37,new PeopleRule{Food=1,Production=2},
   "Sworn to Save",new PeopleRule{Attack=-1});

  // Fantasy, with an image from the old desolation tales: ravens and owls taking over the empty fortresses.
  Add("Ossuary Parliament","Speaker Quorr Thousand-Names","Speaker of the Ossuary Parliament","Before we begin, the Parliament asks only where you would like to be left when you have finished with yourself.",1,7,6,5,1,
   new[]{"Rookhold","High Sitting","Belfry Row","Owlmoot","Quorum","Black Gallery","Roostward","Many Mouths","Featherfall","Old Minutes","Tower Eleven","Long Debate"},
   "Those Who Remember by Eating",
   "Great black carrion birds who gained speech and memory by feeding on the dead of the Burning. Each remembers fragments of everyone its line has eaten, which makes them the world's only historians. They nest in empty towers, debate in enormous noisy sessions and offer, with sincere respect, to perform the funerals of any people.",
   "There were more dead than any world could bury, and the ravens and owls of the burned cities came down to them for a hundred years. Something in the ash, or in the dead, stayed with them. Their chicks hatched knowing the names of streets. By the fifth generation the flocks had gathered at Rookhold to compare memories, and the Parliament was born, quarrelling over what the Makers had actually been. In the golden age of the Great Sittings they set those memories down, and every people from the Seated Kings to the Ashforge Clans sent envoys to ask what their ancestors had known. Then the Iron Mind asked too. It wanted every memory of how its makers died, and when refused it began to clear the towers. Half the archive perished, and there are whole centuries nobody can now recall. Speaker Quorr seeks what no one they ate ever saw: whoever sealed the gateways, and why.",
   "Far-seeing and learned; scholars of every people visit their roosts.","Light-boned: they break easily under attack.",
   "Thousand Remembered Eyes",new PeopleRule{Science=1,Sight=1},
   "Stone-Dropper",9,2,0,0,
   "Roost of Sittings",8,new PeopleRule{Science=2,Gold=1},
   "Hollow Bones",new PeopleRule{Defence=-1});

  // Fantasy: living fire born of this world's burning.
  Add("Unquenched Host","Ysgar Never-Out","First Spark of the Host","You stand very still for something so dry, stranger.",4,0,10,9,7,
   new[]{"Brandwake","Slagroad","Kindling","Grassend","Smoulder","Ten Thousand","Tinderway","Emberfold","Charhold","Cinder Ring","Flarestand","Neverout"},
   "The Fire That Refused to Die",
   "Fires that refused to go out when the Burning ended, and that took bodies of ash, charred bone and hot slag to keep moving. They ride great cinder-striders across the dry steppe, for a fire that stops on bare ground dies. They bear no malice toward anything. They simply burn, and they grieve every flame that goes out.",
   "When the great fires of the Burning sank for want of fuel, a few of them did something no fire had done: they noticed. They drew ash about themselves on the steppe, learned to walk, and then to ride the slag-boned striders they built from old war machines. For two hundred years they swept the last dry grass in crowns of flame, and their Season of Ten Thousand Fires lit the night from shore to shore. That was their glory, and it very nearly ended them, because by its close little was left that would burn. The Firebreak Companies, with trenches and cold sand, then smothered whole warbands, and the Host learned what loss was. Now it moves in careful lines, feeds on the outlying terraces of the Holt of Last Dew whenever it can, and burns Iron Mind depots for their oil. Ysgar Never-Out has counted what remains and looks to the sealed gateways, believing a whole green world lies beyond them.",
   "Swift riders; dry steppe and volcanic ground feed their forges.","Their own smoke hides the horizon.",
   "Wildfire's Road",new PeopleRule{Terrains=new[]{20,17},TileProduction=1},
   "Cinder Strider",8,1,0,2,
   "Ember Pit",32,new PeopleRule{Production=2,Content=1},
   "Own Smoke",new PeopleRule{Sight=-1});
 }
}
}
