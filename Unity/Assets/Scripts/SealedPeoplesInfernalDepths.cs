namespace ParadigmRealm {
// The eight invented peoples of the Infernal Depths (world 3), alongside the Abyssal Throne and the Bloodtusk Horde (CivLore entries 33 and 34).
public static partial class SealedPeoples {
 static void AddInfernalDepths(){
  WorldHistories[adding]="In the beginning, as far as anyone can tell, there was only fire, black glass, and a giant under the rock who would not lie still. The Choir of the Hush sang him quiet before anyone else kept count of years. The Abyssal Throne rose next and dragged slaves into the Depths: the orcs of the Bloodtusk Horde, and the mortals whose dead became the Glazed Court. The Chained Vigil fell out of the red sky to watch, taught the slaves iron, and the great revolt followed. Salamanders of the Night Barge Guild cut twelve locks into the lava river, and enemies began to trade. Demons who feared being devoured wrote themselves down and became the Open Register. Then a vessel came down to cool the world: the Thermal Mandate survive from it, and the Crucible Progeny from its abandoned machines. On the rims of the steam vents, the Mossward Kindred keep the last green thing alive. All ten peoples have found the ancient gateways standing cold and shut, and none knows who shut them. The Throne calls them a jailer's door and means to break it. The Horde calls them a coward's door. The Register looks for the court order, the Guild for the unpaid toll, and the Mandate reads a quarantine. The Choir asks only that nobody knock loudly.";

  Add("Open Register","Tessivar Nine-Seals","Marquis-Registrar of the Open Rolls","Your name, your labour and your undoing, please, and then we may begin on friendly terms.",4,3,6,7,1,
   new[]{"Firstfile","Codicil","Inkfall","Docket Stair","Countersign","Rollhouse","Marginal","Attest","Ninth Schedule","Witness Deep","Sealwright","Last Clause"},
   "Every Name on the Public Roll",
   "Demons who refused the Abyssal Throne's one law, that the strong may devour the weak. Each citizen carves three things into public black glass: a true name, a lawful labour, and the one thing that can undo them. They live in columned courts of rule and counter-rule, and are unnervingly courteous.",
   "Under the Abyssal Throne a demon prince grew by devouring rivals, and lesser demons had no defence but to go unnoticed. A minor duke named Ishveth saw a flaw in the old fire-magic: whoever eats a demon inherits its powers and its weakness too. So she carved her own undoing on a slab of black glass at the foot of the Throne, and walked away alive, for no prince would swallow a weakness the whole world could read. Thousands followed. In the golden age of the Rolls every marquis and president had an assigned labour, contracts held even with orcs, and the Night Barge Guild banked its tolls at Firstfile. Then came the Season of Copies, when Malgoroth's agents stole the oldest rolls and hunted citizens with the very things they had filed. A third of the Register died. It survived by placing a copy in every city. Tessivar now wants what no demon has dared: the name and undoing of every ruler in the world on the public rolls, Malgoroth's first.",
   "Learned and calm; well-kept laws; steady spearmen.","Every enemy can read how to beat them.",
   "Filed and Safe",new PeopleRule{Science=1,Content=1},
   "Writ-Bearer",7,0,2,0,
   "Hall of Declarations",34,new PeopleRule{Gold=2,Science=1},
   "Undoing on Public File",new PeopleRule{Defence=-1});

  Add("Chained Vigil","Orrivane the Unwinged","First Link of the Chained Vigil","We taught the fire once and we are sorry for it; tell us what you mean to do with what you know.",14,7,3,8,2,
   new[]{"Linkhold","Emberfold","Fallen Lantern","Anvilrest","Lastfeather","Nineday Fall","Shackle Hill","Tongsmouth","Greywing","Bellowsgate","Quench","Vigil's End"},
   "Those Who Taught the Fire",
   "Tall, ash-grey beings with the stumps of burned wings, who fell from the red sky in an age nobody can date. They wear the chains they were once bound with, by choice, and work metal as penance for having taught it. They speak softly, and they do not ask to be forgiven.",
   "The old tales say the Vigil came to watch the Depths from above and only to watch. They flew lower out of pity for the slaves in the pits, and lower still, until ash clogged their wings. Grounded, they taught what they knew: ore, bellows and quenching. That iron broke the chains of the orcs, and the Bloodtusk Horde still sing of it without a word of thanks. The Abyssal Throne bound the teachers under Shackle Hill for four hundred years. When the ground split they climbed out, still chained, and chose to stay so. Their golden age was the Long Forging, when Vigil smiths made tools for any people who asked and weapons for none. It ended at Greywing, where blades made by their own pupils burned a city of refugees, and they learned that a gift cannot be recalled. Orrivane now seeks every weapon descended from their teaching, to unmake it, and wonders aloud whether their lost kin sealed the gateways to keep this shame from spreading.",
   "Devout smiths; strong production; hard-wearing chain fighters.","Their burned eyes see little.",
   "The Forbidden Craft",new PeopleRule{Production=1,Faith=1},
   "Chainbearer",2,1,1,0,
   "Chain Chapel",36,new PeopleRule{Faith=2,Content=1},
   "Eyes Burned Dim",new PeopleRule{Sight=-1});

  Add("Night Barge Guild","Temeshu Long-Pole","Master of the Twelfth Lock","Every road here is a river and every river has a toll, friend, so let us agree a fair one.",9,1,1,5,0,
   new[]{"Tollmouth","Feather Quay","Crustgate","Poleman's Rest","Turquoise Reach","Slagwharf","Ironwall Basin","Hourstair","Glowbank","Twelfthlock","Emberquay","Sounding"},
   "Tollkeepers of the Burning River",
   "Salamander-folk who pole crust-iron barges along the Great Burn, the lava river that runs through the Depths in twelve stages. Each stage ends at a lock and a toll. They owe no loyalty but to the ledger, and they are the one people every other people in this world will let pass.",
   "The Great Burn surges by the hour, and in the early ages nothing could cross it. A clan of salamanders who could walk its crust learned the rhythm of the surges and cut twelve iron-walled locks, one for each hour of the nightly flood. At every lock a debt is weighed against a feather of ash, and if the scale tips, the barge waits. In the Peace of Tolls, the Guild's golden age, demons of the Abyssal Throne and orcs of the Bloodtusk Horde moved ore along the same river at the same rate, and blue-green crystal trees were raised around the Seventh Lock for no reason but pride. Then the Coiling Surge, a flow like a vast serpent, broke five locks in one night and drowned the fleet. The Throne seized the ruins and charged double. Temeshu bought them back with shares sold to the Open Register, and has not forgiven the interest. She now plans a thirteenth lock, at the gateways, which she holds are only locks whose toll nobody has paid.",
   "Rich tolls from volcanic land; fast skiffs; welcome everywhere.","Hired guards fight without passion.",
   "Toll of the Twelve Locks",new PeopleRule{Terrains=new[]{17},TileGold=1},
   "Cinder Skiff",8,0,1,2,
   "Feather Scale House",31,new PeopleRule{Gold=2,Content=1},
   "Contract Soldiers",new PeopleRule{Attack=-1});

  Add("Choir of the Hush","Ombrale of the Low Note","Cantor of the Lowest Note","Speak softly, little one, and tell me you have not brought your drums.",13,5,3,4,5,
   new[]{"Lowsong","Humming Stair","Karvagh's Rib","Lull","Slumberstone","Drone Hollow","Quiet Anvil","Softfall","Breathing Hill","Tremorwatch","Long Rest","Hushmoot"},
   "Singers Above the Buried Giant",
   "Giants with skin like grey basalt, who sit in rings on the mountain called Karvagh and sing a single low note, in shifts, without ever stopping. Beneath the mountain lies something the note keeps asleep. They are gentle, slow, and very hard to argue with.",
   "When the Depths still shook daily, a people of giant herders noticed that the quakes slackened when they hummed. They followed the answer in the rock up to Karvagh and sat down, and their descendants have not stopped singing. The old tales give the Sleeper below more heads than anyone could count and a fire in every one; no living giant has seen it. In the Great Drone, their golden age, nine hundred singers held whole valleys still, and the Mossward Kindred came to plant the calm slopes. Then came the Year of Hammers. The war drums of the Bloodtusk Horde and the siege engines of the Abyssal Throne met at the foot of the mountain, and the Choir lost the note for a single breath. The southern plain went up in fire and has not cooled. Since then the giants have forbidden battle on their ground and pay others to keep the peace. Ombrale wants a quiet world, and fears the gateways are a second lid on the same sleeper.",
   "Deep faith; great works; thrown boulders hold any pass.","Nothing loud may be built near the mountain.",
   "The Long Lullaby",new PeopleRule{Faith=2},
   "Boulder Warden",9,1,1,0,
   "Singing Amphitheatre",42,new PeopleRule{Content=2,Faith=1},
   "Never Wake Him",new PeopleRule{Production=-1});

  Add("Thermal Mandate","Ysane Varrow","Acting Commander, Forty-First Generation","This is Acting Commander Varrow of the Thermal Mandate; you are welcome on site, and please do not touch the exchangers.",10,0,6,11,3,
   new[]{"Landfall","Heatsink","Exchanger Nine","Greenhouse Two","Condenser","Pumphouse","Airlock Row","Bulkhead","Telemetry","Deck Forty","Sample Site","Arrival Ridge"},
   "Still Cooling the World",
   "Descendants of a crew who came down in a falling vessel with orders to cool this world and turn it green. Forty-one generations later they still follow the schedule, wear patched pressure suits to dinner, and report each morning to a ship that stopped replying long ago. They are polite, tired and certain.",
   "The vessel Long Patience landed in the Depths on purpose: its makers, whoever they were, wanted a hot world's energy and picked this one from very far away. The crew sank heat-exchange shafts into the volcanic rock, and for three centuries the plan worked. Those were the Green Decades, with a cool valley, real rain and crops under glass, and children who were told they would see a sea. But the survey machines left running on the lava fields had returned as the Crucible Progeny, and they ate the main exchanger for its metal. The valley baked in one summer. The archive burned, and nobody now knows where the ship came from. The mission clock still runs, and says forty more generations. Ysane Varrow has read the numbers, knows they cannot work, and has told no one. Officially the gateways are a quarantine that will lift on schedule. Privately she trades with the Mossward Kindred for samples, and hopes there is water on the far side of the seal.",
   "Reclaim wasteland; good schooling; sealed suits protect their riflemen.","Everything needs spare parts they cannot make.",
   "Soil Programme",new PeopleRule{Terrains=new[]{10},TileFood=1},
   "Pressure-Suit Rifleman",6,0,2,0,
   "Coolant Main",37,new PeopleRule{Food=1,Production=2},
   "Spares Budget",new PeopleRule{Gold=-1});

  Add("Crucible Progeny","Casting Four-Ninety","Surviving Best of the Melt","You are composed of useful elements; state your reason for remaining assembled.",2,6,8,9,4,
   new[]{"Melt Zero","Pour Point","Mould Seven","Ingot Field","Slagline","Sprue","Flux","Cast Yard","Tailings","Annealer","Yield","Reject Heap"},
   "The Machines That Melt Themselves",
   "Walking foundries of slag and iron, descended from mining machines left running on the lava fields. They have no parents and no graves: each generation is poured from the melted bodies of the last, and whatever failed is not poured again. They hate nobody. They simply assay everything.",
   "The survey machines of the Thermal Mandate were built to repair one another from local ore. Left behind on ground where lava destroyed the careless in days, they began to differ: those that stayed above the flow went on repairing, and those that did not became ore. Nobody designed what happened next. In the Great Pour, their golden age, they learned to choose their own shapes, and factories walked out across the plains. The Bloodtusk Horde broke warband after warband against them, and Grukka is said to speak of them with something near respect. Then one casting proved so successful that nearly all were poured to its pattern. It had a single flaw: it cracked under sudden cold. When the Mandate flooded a lava field with coolant, a quarter of the Progeny shattered overnight. Since then Casting Four-Ninety keeps its people varied by testing them, on purpose and without end, against whoever is nearest. It regards the sealed gateways as the only object in the Depths it has failed to melt, and it intends to continue trying.",
   "Immense output from volcanic land; armoured striders; constant war.","Citizens know the next melt may take them.",
   "Drawn from the Melt",new PeopleRule{Terrains=new[]{17},TileProduction=1},
   "Slag Strider",5,2,1,0,
   "Recasting Floor",30,new PeopleRule{Production=2,Science=1},
   "Fear of the Cull",new PeopleRule{Content=-1});

  Add("Mossward Kindred","Vesperil Softwing","Elder of the Last Moss","I am old now, nearly six, so let us be quick and kind: have you brought anything green?",1,2,2,5,6,
   new[]{"Softlight","Greenrim","Vent Garden","Dewcatch","Firstspore","Lichen Stair","Wingfold","Smokeshade","Cool Hollow","Mossbed","Duskward","Paleleaf"},
   "Gardeners in the Smoke",
   "Soft grey moth-folk with wings dusted in soot, who live only six or seven years and spend them tending moss on the cool rims of steam vents. The moss is the last green thing in the Depths. They carry it, spore by spore, to every vent that will take it.",
   "Ash-moths once fed on the heat-loving moss of the steam vents and thought about nothing else. When the glassworks of the Abyssal Throne fouled the vents, one swarm learned to carry spores on its wings and to plan for the next vent, and the planning needed speech. Their golden age was the Green Terraces on the slopes of Karvagh, under the Choir of the Hush, whose low note holds the vents steady. Generations of gardeners mapped every warm crack in the world. Then came the Blackening: a season of eruptions, while the Crucible Progeny stripped the outer terraces for ore. Seven parts in ten of the moss were lost, and a whole generation died in one winter of smoke with its songs unsung, for a moth's learning lasts only as long as the moth. Vesperil, who is nearly six, wants a moss bed in every city of every people, and gives spores to anyone who swears not to burn them. Her people guess that the gateways were sealed to keep the smoke from something green.",
   "Food where none should grow; sharp-eyed outriders.","Short lives: learning dies with each generation.",
   "Spore and Smoke",new PeopleRule{Food=1,Sight=1},
   "Smokewing Outrider",3,1,0,2,
   "Moss Vault",33,new PeopleRule{Food=2,Content=1},
   "Seven-Year Lives",new PeopleRule{Science=-1});

  Add("Glazed Court","Serrane Ashlar","Dowager of the Black Mirrors","How kind of you to call; do stand near the glass, where we can see you properly.",11,4,5,7,7,
   new[]{"Vitrine","Silvering","Fireglaze","Cullet","Lustre","Bevel","Speculum","Tain","Pier Glass","Dark Salon","Long Gallery","Mourning Room"},
   "The Dead Who Kept Their Manners",
   "The fire-glazed dead: slaves who perished in the black-glass furnaces of the Abyssal Throne and did not stop. Their bones shine like lacquer. They keep a silent, exquisite court of mirrors, and whatever is said before a polished surface, anywhere in the Depths, may reach them in time.",
   "The glass pits of the Abyssal Throne used up slaves by the thousand. When one furnace collapsed, it sealed four hundred in molten glass, and when it cooled they climbed out, thinking, with no breath in them. Serrane had been a kitchen slave who remembered how each prince liked to be addressed. She taught the others manners, the one weapon the dead could not lose, and the demons, who had never been flattered by a corpse, let them live for the amusement. In the Long Salon, their golden age, every palace kept a Court mirror, and every prince paid for advice about the others. Then Malgoroth, who had climbed by their advice, smashed the mirrors of his own palace and burned the first Salon, knowing what they knew. The public rolls of the Open Register cheapened secrets further. Serrane is patient, as the dead can be. She means to hang a mirror in every city of every people, and she notes, in passing, that the sealed gateways alone show no reflection at all.",
   "Secrets from every mirror; tireless dead; deadly archers.","The dead have no cradles.",
   "Whispers in the Glass",new PeopleRule{Sight=1,Gold=1},
   "Shard Archer",4,2,0,0,
   "Hall of Black Mirrors",8,new PeopleRule{Science=2,Gold=1},
   "No Cradles",new PeopleRule{Food=-1});
 }
}
}
