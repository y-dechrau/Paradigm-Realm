namespace ParadigmRealm {
// The five insect peoples of the Hive Expanse (world 6; owner request, 9 Nov 2026: an insectoid world with five insect types):
// ants, bees, beetles, mantises and moths. Catalogue 79-83. See SealedPeoples.cs for the rule class and the Add signature.
// All invented; they draw on what real insects do (colonies, waggle dances, dung-rolling, ambush hunting, night flight).
public static partial class SealedPeoples {
 static void AddHiveExpanse(){
  WorldHistories[adding]="A warm, golden world of grass taller than towers, mounds the size of hills and flowers whose scent carries for days. Nothing here walks on two legs. Long ago, the old songs say, the Expanse was an ordinary meadow, and something under it began to hum; within a few thousand summers the small things that lived there had grown large, then clever, then proud. The Formic Concord dug the first cities and still holds the deepest of them. The Apiary of the Golden Comb learned to speak by dancing, and its queens keep the oldest written law in the Expanse, pressed into wax. The Scarab Wardens roll the sun across the sky each morning, or say they do, and nobody has been able to prove them wrong. The Thorn Mantids wait. The Lantern Moths fly at night between the others and carry their quarrels and their gossip. Ancient gateways stand sealed on the open plains, their arches crusted with old resin. The Concord thinks the hum beneath the world will grow loud again when they open. The Moths are rather hoping it does.";

  // Inspired by real ant colonies: castes, tunnelling, farming of fungus and aphids, and the long columns of foragers.
  Add("Formic Concord","Queen Ysmerra of the Thousand Halls","Mother of the Deep Nest","The Concord has counted your soldiers, stranger, and your farmers, and your farmers' children. We count everything. Sit, and be counted.",6,1,8,11,4,
   new[]{"Deepnest","First Gallery","Aphid Fold","Fungus Vaults","Long Column","Brood Hall","Red Mound","Soldier's Gate","Leafcutter Way","Queen's Chamber","Tunnel Market","Thousandfold"},
   "Diggers of the Deep Nest",
   "A colony people of armoured red-brown ants who live in vast tunnelled cities under great earth mounds. Every one of them belongs to a caste: workers, soldiers, foragers, nurses. They farm fungus in dark vaults and keep herds of sweet-dew aphids on the stems above. They think of a city as a single living thing and of a lone ant as a sad mistake.",
   "The Concord began when a single queen survived the first hum and laid an egg that hatched thinking. Her daughters dug, and they never stopped. Their golden age was the Long Column, when foraging lines a hundred miles long fed nests in every corner of the plain. The crisis came with the Flood of the Hollow Year, when rain filled the lower galleries and drowned a generation of brood. The Concord answered by building higher and wider, and by putting soldiers at every entrance. Queen Ysmerra counts the other peoples of the Expanse the way her ancestors counted the rain: carefully, constantly, and ready to dig when the numbers turn.",
   "Endless workers; strong cities; soldiers at every gate.","They follow the column and rarely think beyond it.",
   "The Colony Works",new PeopleRule{Production=2},
   "Soldier Caste",7,1,1,0,
   "Fungus Vaults",7,new PeopleRule{Food=2},
   "One Mind",new PeopleRule{Science=-1});

  // Inspired by honeybees: the waggle dance that tells others where food lies, queens, wax comb and the gathering of nectar.
  Add("Apiary of the Golden Comb","Queen Melisande the Sweet-Voiced","Keeper of the Wax Law","Be welcome, little traveller. Watch closely: I am dancing the way to our hall, and the way out again, should you need it.",14,2,2,1,0,
   new[]{"Goldcomb","Waxhall","Nectar Rise","Dance Court","Clover Reach","Brood Comb","Royal Cell","Pollen Market","Summerhive","Drone Terrace","Honeyflow","Swarm's Rest"},
   "Dancers of the Golden Comb",
   "A gold-and-black people of great bees who build their cities as towering wax combs in the hollows of giant trees. They speak in dances as well as in hums, and a skilled dancer can describe a field a day's flight away to the width of a petal. Their honey is the richest food in the Expanse, and their queens write their laws in pressed wax.",
   "The first Apiary rose in a lightning-split tree whose wound filled with flowers. The bees there learned that a dance could carry a thought, and then that a thought could carry a law. Their golden age was the Great Flow, when honey ran from the combs into trade with every people of the plain. The crisis came in the Year of the Silent Hives, when a sickness stopped the dancing and whole combs fell quiet. Queen Melisande was a forager then. She danced the way to new meadows when no one else would fly, and the hives followed her. She rules with sweetness and a sting held in reserve.",
   "Rich food; skilled traders; a city that speaks with one voice.","Soft in battle until their swarm is roused.",
   "Waggle Dance",new PeopleRule{Food=1,Gold=1},
   "Swarm Guard",8,1,0,1,
   "Royal Comb",31,new PeopleRule{Gold=2,Content=1},
   "Soft Bodies",new PeopleRule{Defence=-1});

  // Inspired by scarab and dung beetles: rolling dung balls and the old Egyptian image of the scarab rolling the sun.
  Add("Scarab Wardens","High Roller Khepet","Warden of the Morning","The sun rose this morning because we rolled it, stranger. You are welcome to its light. Please do not stand in front of it.",13,4,7,3,6,
   new[]{"Dawnroll","Shellhold","Sun Path","Ironback","Rolling Gate","Morning Mound","Carapace Keep","Long Furrow","Duskburrow","Wardenhall","Horn Ridge","Brightshell"},
   "Rollers of the Sun",
   "A people of great horned beetles in shells of black, bronze and green, slow and almost impossible to hurt. They believe that each morning their priests roll the sun up over the rim of the world, and they plan their whole lives around that duty. They are patient builders, tireless cleaners of the land and the most stubborn defenders in the Expanse.",
   "The Wardens say the first sunrise was an accident: a young beetle pushed a glowing ball over a hill and the light stayed. Since then they have never dared stop. Their golden age was the Age of Bright Mornings, when their shrines lined every ridge and their armies were never once broken. The crisis came with the Long Night, an eclipse that the other peoples blamed on the Wardens and that the Wardens blamed on themselves. High Roller Khepet rebuilt the shrines and hardened the shells of the guard. The sun has risen every day since, and the Wardens would like everyone to remember who to thank.",
   "Nearly unbreakable soldiers; devout and steady cities.","Slow to move and slower to change their minds.",
   "Shell of Bronze",new PeopleRule{Defence=1,Faith=1},
   "Horned Bulwark",7,0,2,0,
   "Shrine of the Morning",36,new PeopleRule{Faith=2,Content=1},
   "Slow Plodders",new PeopleRule{Production=-1});

  // Inspired by praying mantises: stillness, camouflage, the ambush strike and the old image of the mantis as a quiet sage.
  Add("Thorn Mantids","Saint-Hunter Iriveth","The Patient Blade","...We saw you three days ago. We have been deciding whether you are a guest or a meal. You may stay. For now.",12,5,9,6,7,
   new[]{"Stillwater","Thornreach","Green Hollow","Ambush Rise","Patience","Folded Blade","Leafshade","Quietmoor","Stalkers' Rest","The Waiting Tree","Mirror Grass","Silent Court"},
   "The Patient Blades",
   "A green and brown people of tall mantises who live in scattered towns hidden in the tallest grass. They can stand perfectly still for a day, and they move faster than the eye can follow when they finally strike. Between hunts they are thoughtful, polite and very fond of philosophy, which unsettles their neighbours more than their blades do.",
   "The Mantids say their first ancestor was a hunter who waited so long for prey that she began to think, and then could not stop. Their golden age was the Age of the Quiet Courts, when their sages wrote the Patience Sutras and no army dared enter the tall grass. The crisis came when the Formic Concord cut and burned the grass to open a road. The Mantids struck once, terribly, and then went back into the green to argue about whether they should have. Saint-Hunter Iriveth believes they should, and should again, and has been very patient about it.",
   "Deadly ambushers; keen sight; thoughtful scholars.","Few in number and slow to grow.",
   "Stillness and Strike",new PeopleRule{Attack=1,Sight=1},
   "Thorn Stalker",4,2,0,0,
   "Court of Patience",8,new PeopleRule{Science=2},
   "Solitary Ways",new PeopleRule{Food=-1});

  // Inspired by night-flying moths: drawn to light, the dust on their wings, long migrations and the silk spun by their caterpillars.
  Add("Lantern Moths","Dowager Pellanine of the Silver Dust","Keeper of the Night Roads","Oh, how lovely, a visitor with a lamp! Do bring it closer. We only want to look at it. Mostly.",1,6,1,5,5,
   new[]{"Lampwick","Silverdust","Night Road","Cocoon Hall","Moonmarket","Silkspan","Duskveil","Glimmer","Featherfall","Lantern Rest","Paleflight","Spinner's Hollow"},
   "Travellers of the Night Roads",
   "A soft, feathery people of great moths who sleep through the day in silk halls and fly the night roads between the other peoples of the Expanse. They carry goods, letters and gossip, and they are welcome almost everywhere. Their children spin the finest silk in any world, and every moth is drawn, a little helplessly, to any bright light.",
   "The Moths say they began as caterpillars who spun a cocoon around the first hum and came out able to understand it. Their golden age was the Silk Peace, when Moth envoys carried the treaties that ended the Mound Wars between the Concord and the Wardens. The crisis came with the Burning Lamps, when a careless trade in lantern-oil led a whole generation of young moths into the fire. Dowager Pellanine banned open flame in every Moth town and turned the trade to silk and news. She knows every secret in the Expanse and keeps most of them, at least until the price is right.",
   "Trade and envoys everywhere; fine silk; welcome at every court.","Fragile in war and dazzled by any bright prize.",
   "Night Roads",new PeopleRule{Gold=2},
   "Dust Rider",8,0,0,2,
   "Silk Hall",31,new PeopleRule{Gold=1,Content=1},
   "Drawn to the Flame",new PeopleRule{Defence=-1});
 }
}
}
