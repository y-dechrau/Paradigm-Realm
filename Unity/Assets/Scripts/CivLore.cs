using System;
using System.Linq;
namespace ParadigmRealm {
// Civilisations in full (owner request, 31 Oct 2026). Ten of the premade civilisations with portraits and twelve real ones
// each get a background, a history, strengths and weaknesses, and four rules beside their two traits, in the manner of
// Civilization V/VI and Humankind (unique ability, unit and building), Age of Empires (unique units replacing a common one),
// Civilization VI (leader agendas) and Old World (a real weakness to play around). Dwarf Fortress and Stellaris inspired the
// written histories: each people has a founding, a golden age, a crisis and an ambition. Rules are one clear integer each.
public sealed class CivEntry {
 public string Epithet,Background,History,Strengths,Weaknesses,Ability,AbilityText,Unit,UnitText,Building,BuildingText,Weakness,WeaknessText;
 public int UnitBase=-1,UnitAttack,UnitDefence,UnitMoves,BuildingBase=-1,Agenda=-1;public int[] Faiths=new int[0];
}
public static class CivLore {
 // Agendas: what a rival leader likes or holds against you (World.AgendaOpinion).
 public static readonly string[] AgendaNames={"Merchant Prince","Seeker of Wisdom","Defender of the Faith","Builder of Wonders","Hungry for Land","Keeper of the Peace","Warden of the Borders","Respects Only Strength"};
 public static readonly string[] AgendaTexts={
  "Likes civilisations it has a friendship or alliance with; dislikes those that embargo it.",
  "Envies civilisations that know more technologies than it does; warms to those that know fewer.",
  "Likes civilisations whose capital follows its faith; dislikes those that founded a rival faith.",
  "Dislikes civilisations with more wonders than it has.",
  "Dislikes civilisations with more cities than it has.",
  "Dislikes civilisations at war with anyone; likes those at peace with all.",
  "Dislikes civilisations with units inside its borders.",
  "Respects civilisations with an army at least as large as its own; despises much weaker ones."};
 static readonly CivEntry[] entries=new CivEntry[CivilisationCatalog.Count];
 public static CivEntry Of(int civ){return civ>=0&&civ<entries.Length?entries[civ]:null;}
 public static bool Full(int civ){return Of(civ)!=null;}
 static CivLore(){
  // ---------- the premade civilisations ----------
  entries[0]=new CivEntry{Epithet="Sea-Kings of the Blue Coves",
   Background="A kingdom of blue-roofed harbour towns strung along a broken coast of coves and headlands. Its people are fishers, shipwrights and merchants first and farmers a distant second; the thin hill soil behind the shore has never fed them, so the sea must.",
   History="The Azure Crown began as a league of fishing villages that pooled their boats against pirates and crowned the captain who led them, Aldric's ancestor, as their first sea-king. Charters granted to each harbour bought loyalty and built a merchant fleet that reached every shore within a season's sailing. A great storm and a war over the salt trade nearly broke the kingdom two generations ago; it survived by building an Admiralty and a navy no rival could match. King Aldric now dreams of a crown whose harbours ring the whole world.",
   Strengths="Rich coastal cities; fast, sturdy ships; strong science from its Admiralties.",Weaknesses="Inland cities are poor producers; little room to grow away from the sea.",
   Ability="Harbour Charters",AbilityText="Coastal cities +2 gold.",
   Unit="Carrack",UnitBase=13,UnitDefence=1,UnitMoves=2,UnitText="Replaces the Galley: +1 defence, +2 movement.",
   Building="Admiralty",BuildingBase=43,BuildingText="Replaces the Harbour: +2 science as well.",
   Weakness="Narrow Hinterland",WeaknessText="Inland cities -1 production.",Agenda=0,Faiths=new[]{0,21}};
  entries[1]=new CivEntry{Epithet="Queens of the Green Wells",
   Background="Desert people who rule from palm-shaded oasis cities linked by caravan roads. Every well is sacred, every caravanserai a market, and the sultana's court is famed for its astronomers, poets and accountants alike.",
   History="When the old river dried, the clans that knew the hidden wells survived and the rest scattered. Queen Samira's line united the well-keepers by marriage and treaty, and taxed the caravans that had to drink at their springs. Their golden age came with the camel: herds bred for the long crossing carried salt, silk and books across the sands. A siege that breached the mud-brick walls of their first capital taught them that trade, not walls, is their true defence.",
   Strengths="Turns desert into farmland; rich market towns; fast camel cavalry.",Weaknesses="Mud-brick walls give less protection than stone.",
   Ability="Desert Wells",AbilityText="Each worked desert tile +1 food.",
   Unit="Camel Rider",UnitBase=8,UnitAttack=1,UnitDefence=1,UnitText="Replaces the Horseman: +1 attack, +1 defence.",
   Building="Caravanserai",BuildingBase=31,BuildingText="Replaces the Market: +2 gold and +1 food as well.",
   Weakness="Brittle Walls",WeaknessText="Units defending a walled city get 1 less defence.",Agenda=1,Faiths=new[]{1,10,16}};
  entries[3]=new CivEntry{Epithet="Stewards of the Grand Cathedral",
   Background="An empire of bell towers and pilgrim roads, ruled from a capital built around a vast cathedral. Its emperors are crowned by the church and its laws are read from the pulpit; faith is the thread that ties its many provinces together.",
   History="The Imperium rose from the ruins of an older empire when its bishops kept learning and order alive while the legions failed. Constantin's dynasty married the throne to the altar, and the great pilgrimages brought wealth and converts from every land. Schism once split the church and the empire with it; the reunion council that ended it is still celebrated each year. The Emperor means to see the bells of his faith ring in every city of the world.",
   Strengths="Powerful faith, spreading fast; content cities; tough heavy cavalry.",Weaknesses="The church's tithe drains the treasury.",
   Ability="Divine Mandate",AbilityText="Cities with a Temple +2 faith; its faith spreads half again as strongly.",
   Unit="Templar",UnitBase=5,UnitDefence=2,UnitText="Replaces the Knight: +2 defence.",
   Building="Cathedral",BuildingBase=36,BuildingText="Replaces the Temple: +1 content citizen and +1 faith as well.",
   Weakness="Tithe Burden",WeaknessText="-1 gold in every city.",Agenda=2,Faiths=new[]{0,4}};
  entries[4]=new CivEntry{Epithet="Lords of the River of Gold",
   Background="A wealthy empire of the savanna and the great river bend, whose mud mosques and markets sit on the gold and salt roads. Griots keep its history in song, and its scholars are welcomed from every land.",
   History="Founded when the hunter-king Kanto united the river clans, the Sahel Empire grew rich on gold traded for desert salt. A famous pilgrimage of an early mansa spent so much gold on the road that prices fell in every city it passed, and the world learned the empire's name. Its libraries and universities drew scholars from far and wide. Rebellious far provinces have always been its trouble; Mansa Kanto rules by generosity and wants monuments that will outlast every rival's.",
   Strengths="Wealth that grows with its cities; great libraries; fierce spearmen.",Weaknesses="Distant provinces are restless.",
   Ability="River of Gold",AbilityText="+1 gold in every city for each 3 citizens.",
   Unit="Mandekalu Spearman",UnitBase=7,UnitAttack=1,UnitText="Replaces the Spearman: +1 attack.",
   Building="Sankore Hall",BuildingBase=8,BuildingText="Replaces the Library: +2 science and +1 faith as well.",
   Weakness="Distant Provinces",WeaknessText="Cities more than 8 tiles from the capital -1 content citizen.",Agenda=3,Faiths=new[]{1,12}};
  entries[5]=new CivEntry{Epithet="The Free Cities of the Wine-Dark Sea",
   Background="A league of proud, quarrelsome city-states sharing gods, games and a language. Each polis governs itself; together they send out colonies, philosophers and phalanxes that have humbled empires.",
   History="The League was sworn at a sacred spring after a great invasion from the east was turned back at sea. Its member cities pooled their fleets and their treasure, and in the peace that followed built theatres, schools and the first libraries. The League's golden age ended in a long civil war between its two strongest cities; Strategos Kleon was elected to hold it together. He would rather win an argument than a battle, and has learned that he usually must do both.",
   Strengths="Large cities become centres of science; steady infantry; strong diplomacy.",Weaknesses="The more cities join, the more they quarrel.",
   Ability="Agora",AbilityText="Cities of size 5 or more +2 science.",
   Unit="Phalanx",UnitBase=7,UnitDefence=1,UnitText="Replaces the Spearman: +1 defence.",
   Building="Odeon",BuildingBase=42,BuildingText="Replaces the Colosseum: +2 science as well.",
   Weakness="Quarrelsome Poleis",WeaknessText="With more than 4 cities, every city -1 content citizen.",Agenda=5,Faiths=new[]{14,17}};
  entries[7]=new CivEntry{Epithet="Holds Beneath the Snow",
   Background="Mountain folk who dig their halls deep into the rock beneath glaciers. They are miners, smiths and stubborn defenders, slow to trust strangers and slower to forget a slight. Their queen's word is as hard as the stone it is carved in.",
   History="Driven into the peaks by a long winter that killed their lowland farms, the first clans survived by following the warm seams of the mountains inwards. They found iron, silver and coal, and built forges that never go cold. Their histories are carved on the walls of the deepest hall, a record of every debt, grudge and gift for a thousand years. Queen Brynja has opened the gates to trade, but her wardens still watch every pass.",
   Strengths="Hills and mines give extra production; great forges; tough crossbowmen.",Weaknesses="Cities grow slowly in the cold.",
   Ability="Deep Delvers",AbilityText="Each worked hill or mine +1 production.",
   Unit="Crossbow Warden",UnitBase=4,UnitAttack=1,UnitDefence=1,UnitText="Replaces the Archer: +1 attack, +1 defence.",
   Building="Deep Forge",BuildingBase=30,BuildingText="Replaces the Workshop: +2 production as well.",
   Weakness="Long Winters",WeaknessText="Cities need 4 more food to grow.",Agenda=6,Faiths=new[]{15,21}};
  entries[8]=new CivEntry{Epithet="Senate and People of Aurelia",
   Background="A republic of citizens, roads and law courts. Its consuls are elected for a year at a time, its legions are raised from its farmers, and its forums are where every quarrel is settled by argument before it is settled by force.",
   History="Aurelia began as a market town on seven low hills where several tribes met to trade. Its citizens threw out their last king and swore never to crown another, and their republic spread by colonies, roads and alliances as much as by conquest. A slave revolt and a civil war between rival generals nearly ended the Republic; the Senate survived by granting citizenship to all its allies. Consul Aurelius wants more land for more citizens, and has little patience for long wars that the people tire of.",
   Strengths="Content, well-governed cities; disciplined early infantry; good gold from forums.",Weaknesses="Its citizens tire of war quickly.",
   Ability="Civic Virtue",AbilityText="+1 content citizen in cities with a Courthouse or Colosseum.",
   Unit="Legionary",UnitBase=2,UnitAttack=1,UnitDefence=1,UnitText="Replaces the Warrior: +1 attack, +1 defence.",
   Building="Forum",BuildingBase=34,BuildingText="Replaces the Courthouse: +2 gold and +1 content citizen as well.",
   Weakness="War-Weary Citizens",WeaknessText="While at war with anyone, every city -1 content citizen.",Agenda=4,Faiths=new[]{14,17,0}};
  entries[10]=new CivEntry{Epithet="Lords of the Rising Isles",
   Background="An island realm of mountains, cherry orchards and fortified harbours, ruled by warrior-lords who serve a distant court. Honour and discipline are prized above wealth; every lord trains his retainers from childhood.",
   History="The isles were long divided among rival clans whose wars were fought by armoured retainers under strict codes of conduct. Lord Kaito's grandfather ended a century of civil war at a single great battle and forbade private castles. Peace brought tea houses, theatres and swordsmiths whose blades are prized abroad. Island soil is thin, and the growing cities hunger for more; Kaito respects only those strong enough to be worthy rivals.",
   Strengths="Units fresh to battle hit harder; elite samurai; productive training halls.",Weaknesses="Large cities run short of food.",
   Ability="Bushido",AbilityText="Units at full health +1 attack.",
   Unit="Samurai",UnitBase=7,UnitAttack=2,UnitText="Replaces the Spearman: +2 attack.",
   Building="Dojo",BuildingBase=32,BuildingText="Replaces the Barracks: +2 production as well.",
   Weakness="Few Fields",WeaknessText="Cities of size 7 or more -1 food.",Agenda=7,Faiths=new[]{8,3,6}};
  entries[11]=new CivEntry{Epithet="Riders of the Endless Grass",
   Background="Nomad clans of the high steppe who live in felt tents and count wealth in horses. Every child can ride before they can walk, and a horde can cross a province in the time a farmer's army takes to march a day.",
   History="The clans fought each other for generations until Khan Temur, an orphan who rose from nothing, bound them together by oath, marriage and victory. His riders swept across the plains, and his messengers built a relay of horse stations that carried his word faster than any kingdom's. The Horde respects strength, fair dealing and courage, and despises walls. It has little use for scholars yet, and its cities are camps that grew roots.",
   Strengths="Very fast mounted armies; elite horse archers; productive camp stores.",Weaknesses="Little interest in learning.",
   Ability="Endless Saddle",AbilityText="Mounted units +2 movement.",
   Unit="Keshig",UnitBase=8,UnitAttack=1,UnitDefence=1,UnitText="Replaces the Horseman: +1 attack, +1 defence.",
   Building="Ordu",BuildingBase=7,BuildingText="Replaces the Granary: +1 production as well.",
   Weakness="Nomad Scholars",WeaknessText="-1 science in every city.",Agenda=7,Faiths=new[]{18,3,16}};
  entries[12]=new CivEntry{Epithet="Jarls of the Cold Coasts",
   Background="A hard northern kingdom of fjords, longhouses and longships. Its people farm short summers, fish the cold seas and raid or trade with every coast their ships can reach. Their king rules with the consent of quarrelsome jarls.",
   History="Northmarch was born when the jarls of the fjords chose Halvard's ancestor to lead them against a common enemy and never quite unchose his line. Their longships carried raiders, traders and settlers to distant shores, founding towns where they had once burned them. Feasts in great mead halls bind the jarls to the king; a king who cannot feed and defend them does not stay king for long. Halvard wants more coast, more ships and more land for his restless people.",
   Strengths="Coastal raids hit hard; swift longships; feasting halls that also produce.",Weaknesses="Unwalled cities are restless.",
   Ability="Raiders' Tide",AbilityText="Land units attacking from a coastal tile +1 attack.",
   Unit="Longship",UnitBase=14,UnitAttack=1,UnitMoves=2,UnitText="Replaces the War Galley: +1 attack, +2 movement.",
   Building="Mead Hall",BuildingBase=33,BuildingText="Replaces the Gardens: +1 production as well.",
   Weakness="Restless Jarls",WeaknessText="Cities without Walls -1 content citizen.",Agenda=4,Faiths=new[]{15,0}};
  // ---------- real civilisations ----------
  entries[17]=new CivEntry{Epithet="The Middle Kingdom",
   Background="The civilisation of the Yellow and Yangtze rivers, one of the oldest continuous civilisations, held together by a written language, a vast bureaucracy and the idea that a ruler governs by the Mandate of Heaven.",
   History="In 221 BCE King Zheng of Qin conquered the last of the Warring States and took the title Qin Shi Huang, First Emperor. He standardised writing, weights, measures and coinage, linked earlier walls into a northern defence and built roads and canals across the empire; his tomb was guarded by the Terracotta Army. The Qin fell soon after his death, but the Han and later dynasties kept his unified state. Imperial examinations later chose officials by learning, and Chinese inventions such as paper, printing, the compass and gunpowder changed the world.",
   Strengths="Libraries become centres of learning; repeating crossbows; hard-working cities.",Weaknesses="A vast bureaucracy is costly.",
   Ability="Imperial Examinations",AbilityText="Cities with a Library +2 science.",
   Unit="Chu-ko-nu",UnitBase=4,UnitAttack=1,UnitText="Replaces the Archer: +1 attack (a repeating crossbow).",
   Building="Hanlin Academy",BuildingBase=38,BuildingText="Replaces the University: +2 science as well.",
   Weakness="Heavy Bureaucracy",WeaknessText="-1 gold in every city.",Agenda=3,Faiths=new[]{7,6,3}};
  entries[18]=new CivEntry{Epithet="Gift of the Nile",
   Background="The kingdom of the Nile valley, where the yearly flood laid down black soil in a ribbon of green between deserts. Pharaohs were god-kings, scribes ran the granaries, and temples and tombs were built to last for eternity.",
   History="Upper and Lower Egypt were united around 3100 BCE, and the Old Kingdom raised the pyramids of Giza. Ramesses II, who reigned for 66 years in the 13th century BCE during the New Kingdom, fought the Hittites at Kadesh and then signed one of the earliest known peace treaties with them. He built Abu Simbel, finished the great hall of Karnak and founded a new capital, Pi-Ramesses, in the delta. Egypt's civilisation lasted three thousand years before passing to Persian, Greek and Roman rule.",
   Strengths="River cities thrive; chariots strike hard; grand temples generate faith.",Weaknesses="Cities away from fresh water struggle.",
   Ability="Gift of the Nile",AbilityText="Cities by a river or lake +1 food and +1 production.",
   Unit="Maryannu Chariot",UnitBase=8,UnitAttack=1,UnitText="Replaces the Horseman: +1 attack.",
   Building="Mortuary Temple",BuildingBase=36,BuildingText="Replaces the Temple: +2 faith as well.",
   Weakness="Desert Frontier",WeaknessText="Cities without fresh water -1 production.",Agenda=3,Faiths=new[]{12}};
  entries[19]=new CivEntry{Epithet="Senate and People of Rome",
   Background="The city on the Tiber that became master of the Mediterranean world, binding it together with roads, law, citizenship and the legions.",
   History="Rome grew from a small kingdom to a republic in 509 BCE and conquered Italy, Carthage and the Hellenistic east. After decades of civil war, Octavian defeated Antony and Cleopatra in 31 BCE and in 27 BCE took the name Augustus, becoming the first Roman emperor while keeping the forms of the Republic. He reformed the army and the provinces, built across Rome and began two centuries of the Pax Romana. Roman law, Latin and the network of roads outlasted the western empire, which fell in 476 CE; the eastern empire lasted until 1453.",
   Strengths="Road-linked cities prosper; disciplined legions; bath houses keep cities content.",Weaknesses="Far frontiers grow restless.",
   Ability="All Roads Lead to Rome",AbilityText="Cities connected to the capital by road +1 production and +1 gold.",
   Unit="Legion",UnitBase=7,UnitAttack=1,UnitDefence=1,UnitText="Replaces the Spearman: +1 attack, +1 defence.",
   Building="Bath House",BuildingBase=37,BuildingText="Replaces the Aqueduct: +1 food and +1 content citizen as well.",
   Weakness="Overstretched Frontier",WeaknessText="Cities more than 10 tiles from the capital -1 content citizen.",Agenda=4,Faiths=new[]{14,17,0}};
  entries[20]=new CivEntry{Epithet="Cradle of Democracy",
   Background="The world of the Greek city-states around the Aegean: Athens, Sparta, Corinth, Thebes and hundreds more, sharing a language, gods, the Olympic Games and a passion for argument.",
   History="The Greek poleis repelled the Persian invasions at Marathon (490 BCE) and Salamis (480 BCE). In the decades that followed, Pericles led Athens through its golden age: the Parthenon was raised on the Acropolis, democracy was extended to poorer citizens, and drama, history and philosophy flourished. The rivalry between Athens and Sparta ended in the Peloponnesian War (431-404 BCE), during which Pericles died of plague. Alexander the Great later carried Greek culture as far as India.",
   Strengths="Thriving science in growing cities; superb hoplites; strong diplomacy.",Weaknesses="Independent cities quarrel as the realm grows.",
   Ability="Polis",AbilityText="Cities of size 4 or more +1 science and +1 influence.",
   Unit="Hoplite",UnitBase=7,UnitDefence=2,UnitText="Replaces the Spearman: +2 defence.",
   Building="Acropolis",BuildingBase=42,BuildingText="Replaces the Colosseum: +1 science and +1 faith as well.",
   Weakness="Rival Poleis",WeaknessText="With more than 5 cities, every city -1 content citizen.",Agenda=1,Faiths=new[]{14}};
  entries[21]=new CivEntry{Epithet="People of the Sun",
   Background="The Mexica of the Valley of Mexico, who built Tenochtitlan on an island in Lake Texcoco and ruled a tribute empire through the Triple Alliance. Floating gardens fed one of the largest cities in the world.",
   History="According to their tradition the Mexica founded Tenochtitlan in 1325 where an eagle perched on a cactus. With Texcoco and Tlacopan they formed the Triple Alliance in 1428 and gathered tribute from much of Mesoamerica. Moctezuma II, huey tlatoani from 1502, extended the empire and its temples; the Templo Mayor honoured Huitzilopochtli and Tlaloc. Spanish conquistadors arrived in 1519, Moctezuma died in 1520 and the city fell in 1521 after a siege aided by the Aztecs' enemies and by smallpox.",
   Strengths="Lake and marsh farms; fierce eagle warriors; ball courts that please the gods.",Weaknesses="Subject cities far from the capital resent tribute.",
   Ability="Chinampas",AbilityText="Each worked lake, river or marsh tile +1 food.",
   Unit="Eagle Warrior",UnitBase=2,UnitAttack=2,UnitText="Replaces the Warrior: +2 attack.",
   Building="Tlachtli",BuildingBase=42,BuildingText="Replaces the Colosseum: +1 faith and +1 content citizen as well.",
   Weakness="Tribute Empire",WeaknessText="Cities more than 6 tiles from the capital -1 content citizen.",Agenda=7,Faiths=new[]{19}};
  entries[22]=new CivEntry{Epithet="The Four Parts Together",
   Background="Tawantinsuyu, the Inca realm of the Andes, stretching thousands of kilometres along the mountains, linked by stone roads and rope bridges, and fed by terraced fields and state storehouses.",
   History="Pachacuti came to power in Cusco around 1438 after defeating the Chanka, and turned a small kingdom into an empire. He rebuilt Cusco, is credited with building Machu Picchu as a royal estate, and organised the realm into four quarters. His successors extended it from Ecuador to Chile. The Inca had no horses or wheeled vehicles and kept records on knotted quipu cords; runners carried messages along the roads. The empire fell to Spanish conquistadors after 1532, weakened by civil war and epidemic.",
   Strengths="Terraced hills feed cities; slingers strike from afar; storehouses feed and build.",Weaknesses="No native horses: mounted units cost more.",
   Ability="Terrace Farms",AbilityText="Each worked hill +1 food.",
   Unit="Warak'aq",UnitBase=4,UnitAttack=1,UnitMoves=2,UnitText="Replaces the Archer: +1 attack, +2 movement (slingers).",
   Building="Qullqa",BuildingBase=7,BuildingText="Replaces the Granary: +1 food and +1 production as well.",
   Weakness="No Horse or Wheel",WeaknessText="Mounted units cost a quarter more.",Agenda=6,Faiths=new[]{20}};
  entries[23]=new CivEntry{Epithet="Keepers of the Long Count",
   Background="The Maya of the Yucatan and Central American lowlands, builders of stepped pyramids and observatories, with a full writing system and calendars that counted days across thousands of years.",
   History="Maya city-states such as Tikal, Calakmul, Copan and Palenque flourished in the Classic period (about 250-900 CE). K'inich Janaab' Pakal ruled Palenque (Lakamha) from 615 to 683 CE, longer than almost any other ruler of his time, and rebuilt it with palaces and the Temple of the Inscriptions, where he was buried with a jade mask. Many lowland cities were abandoned in the 9th century, but Maya cities continued in the north at Chichen Itza and Mayapan, and millions of Maya live in the region today.",
   Strengths="Exceptional science, especially in the capital; observatories; zealous faith.",Weaknesses="City-states expand slowly: Settlers cost more.",
   Ability="Long Count",AbilityText="+1 science in every city and +2 more in the capital.",
   Unit="Holkan",UnitBase=7,UnitAttack=1,UnitDefence=1,UnitText="Replaces the Spearman: +1 attack, +1 defence.",
   Building="Observatory",BuildingBase=8,BuildingText="Replaces the Library: +2 science as well.",
   Weakness="City-States",WeaknessText="Settlers cost a third more.",Agenda=1,Faiths=new int[0]};
  entries[24]=new CivEntry{Epithet="Land of the Highlands",
   Background="An ancient highland realm of the Horn of Africa, heir to the kingdom of Aksum, with one of the oldest Christian churches in the world, its own script (Ge'ez) and a long record of independence.",
   History="Aksum was a great trading power by the 1st century CE and adopted Christianity under King Ezana in the 4th century. Later rulers carved the rock-hewn churches of Lalibela. Menelik II became emperor in 1889, expanded and modernised the state and founded Addis Ababa as its capital. In 1896 his army defeated an Italian invasion at the Battle of Adwa, and Ethiopia remained independent throughout the scramble for Africa.",
   Strengths="Deep faith in every city; formidable defenders in the highlands; rock-hewn churches.",Weaknesses="Isolation limits trade.",
   Ability="Highland Faith",AbilityText="+1 faith in every city; units on hills +1 defence.",
   Unit="Mehal Sefari",UnitBase=6,UnitDefence=1,UnitText="Replaces the Rifleman: +1 defence.",
   Building="Rock-Hewn Church",BuildingBase=36,BuildingText="Replaces the Temple: +1 faith and +1 content citizen as well.",
   Weakness="Mountain Isolation",WeaknessText="-1 gold in every city.",Agenda=2,Faiths=new[]{0,4}};
  entries[25]=new CivEntry{Epithet="House of Wisdom",
   Background="The Arab caliphates that spread from the Arabian peninsula after the 7th century, uniting lands from Spain to Central Asia in a world of trade, faith and scholarship centred on cities such as Baghdad, Damascus and Cairo.",
   History="After the death of the Prophet Muhammad in 632, the caliphates spread rapidly. The Abbasids founded Baghdad in 762, and under Harun al-Rashid (caliph 786-809) the city became one of the largest and richest in the world; he is remembered in the Thousand and One Nights. His court and that of his son al-Ma'mun supported the translation of Greek, Persian and Indian works, and Baghdad's scholars advanced mathematics, astronomy and medicine, giving the world words such as algebra and algorithm.",
   Strengths="Markets that teach as well as trade; swift mamluk cavalry; learned madrasas.",Weaknesses="Great cities split into factions.",
   Ability="Golden Age of Learning",AbilityText="Cities with a Market +1 science and +1 gold.",
   Unit="Mamluk",UnitBase=5,UnitAttack=1,UnitText="Replaces the Knight: +1 attack.",
   Building="Madrasa",BuildingBase=38,BuildingText="Replaces the University: +2 science and +1 faith as well.",
   Weakness="Fractious Emirates",WeaknessText="Cities of size 10 or more -1 content citizen.",Agenda=0,Faiths=new[]{1}};
  entries[26]=new CivEntry{Epithet="King of Kings",
   Background="The Persian empire founded by Cyrus, the first to rule from the Aegean to the Indus, governed through satraps, linked by the Royal Road and known for respecting the customs and religions of its peoples.",
   History="Cyrus II of Anshan overthrew the Medes around 550 BCE, conquered Lydia and in 539 BCE took Babylon, where he allowed exiled peoples, including the Jews, to return home. The Cyrus Cylinder records his policy. His successors Cambyses and Darius extended the empire to Egypt and India, divided it into satrapies, built Persepolis and the Royal Road with its relay posts. The Achaemenid empire fell to Alexander in 330 BCE, but later Persian empires rose again under the Parthians and Sasanians.",
   Strengths="Well-governed provinces; elite Immortals; paradise gardens.",Weaknesses="Distant satraps rebel.",
   Ability="Satrapies",AbilityText="Cities with a Courthouse +2 gold and +1 content citizen.",
   Unit="Immortal",UnitBase=7,UnitAttack=1,UnitDefence=1,UnitText="Replaces the Spearman: +1 attack, +1 defence.",
   Building="Pairidaeza",BuildingBase=33,BuildingText="Replaces the Gardens: +1 gold and +1 food as well.",
   Weakness="Satrap Revolts",WeaknessText="Cities more than 8 tiles from the capital -1 production.",Agenda=5,Faiths=new[]{10}};
  entries[27]=new CivEntry{Epithet="Empire of Dharma",
   Background="The Maurya empire, the first to unite most of the Indian subcontinent, ruled from Pataliputra on the Ganges.",
   History="Chandragupta Maurya founded the empire around 321 BCE. His grandson Ashoka (reigned about 268-232 BCE) conquered Kalinga in a bloody war, and, horrified by the slaughter, turned to Buddhism and the rule of dharma. He had edicts carved on rocks and pillars across his realm urging tolerance, care for people and animals, and respect for all faiths, and sent missions abroad that helped spread Buddhism across Asia. The lion capital of one of his pillars is the emblem of India today.",
   Strengths="Faith spreads fast and calms cities; war elephants; stupas.",Weaknesses="Many kingdoms to hold together cost gold.",
   Ability="Dharma",AbilityText="Its faith spreads half again as strongly; cities following its faith +1 content citizen.",
   Unit="War Elephant",UnitBase=5,UnitAttack=1,UnitDefence=1,UnitMoves=-2,UnitText="Replaces the Knight: +1 attack, +1 defence, -2 movement.",
   Building="Stupa",BuildingBase=36,BuildingText="Replaces the Temple: +1 faith and +1 content citizen as well.",
   Weakness="Many Kingdoms",WeaknessText="-1 gold in every city.",Agenda=2,Faiths=new[]{3,2,9}};
  entries[28]=new CivEntry{Epithet="Gate of the Gods",
   Background="The city of Babylon on the Euphrates and the kingdom it ruled in Mesopotamia, the land between the rivers where writing, law codes, astronomy and mathematics were developed.",
   History="Hammurabi (reigned about 1792-1750 BCE) made Babylon the master of Mesopotamia and issued one of the best known early law codes, carved on a stone stele, setting out laws and penalties so that the strong should not oppress the weak. Babylonian scribes used base-60 mathematics and recorded the movements of the stars. Babylon rose again under Nebuchadnezzar II, who built the Ishtar Gate, before it was taken by Cyrus of Persia in 539 BCE.",
   Strengths="Learned scribes in every city; law courts that keep order; tablet houses.",Weaknesses="Cities without walls are vulnerable and uneasy.",
   Ability="Code of Laws",AbilityText="+1 science in every city; cities with a Courthouse +1 content citizen.",
   Unit="Sabum Kibittum",UnitBase=7,UnitAttack=1,UnitText="Replaces the Spearman: +1 attack.",
   Building="Edubba",BuildingBase=8,BuildingText="Replaces the Library: +2 science as well.",
   Weakness="Open Plains",WeaknessText="Cities without Walls -1 content citizen.",Agenda=1,Faiths=new[]{13}};
  // ---------- native peoples of the parallel worlds (owner request, 31 Oct 2026) ----------
  entries[29]=new CivEntry{Epithet="Wardens of the Fae Wilds",
   Background="Tall, long-lived elves who dwell in living halls grown from ancient trees beside the silver lakes of the Fae Wilds. They sing their histories rather than write them, and every grove has a guardian.",
   History="The Sylvan Elves say they woke beneath the first stars when the Wilds were still mist. For ages they tended the groves and fought the creeping rot from the marsh-worlds. A war with the Aerie over the high forests ended in an oath of peace sworn on a fallen star. Their numbers are few and their children rare, so every life is precious; Queen Aelindra will spill any amount of outsider blood to keep the forests whole.",
   Strengths="Forests feed them; peerless archers; sacred moonwells.",Weaknesses="Few children: cities grow slowly.",
   Ability="Forest Kin",AbilityText="Each worked woodland tile +1 food.",
   Unit="Wildwood Archer",UnitBase=4,UnitAttack=2,UnitText="Replaces the Archer: +2 attack.",
   Building="Moonwell",BuildingBase=36,BuildingText="Replaces the Temple: +2 faith as well.",
   Weakness="Long-Lived and Few",WeaknessText="Cities need 4 more food to grow.",Agenda=6};
  entries[30]=new CivEntry{Epithet="Lords of the High Winds",
   Background="Feathered avians who nest in the cliff-cities and storm-swept spires above the Fae Wilds. Hunters and scouts without equal, they see a rabbit move from a mile up and remember every star chart their ancestors drew.",
   History="The Aerie was born when the great storm-eagles taught the first flocks to speak. They built roosts on the highest peaks, safe from anything that walks. A long feud with the elves over the forest canopies ended in an uneasy peace. Skylord Kerrak hoards knowledge as other lords hoard gold, and despises those who will not share it.",
   Strengths="Units see further; swift wind-riders; roosts full of lore.",Weaknesses="Hollow bones: units defend poorly.",
   Ability="Sky Sight",AbilityText="All units see 1 tile further.",
   Unit="Windrider",UnitBase=3,UnitAttack=1,UnitDefence=1,UnitMoves=2,UnitText="Replaces the Scout: +1 attack, +1 defence, +2 movement.",
   Building="Roost of Winds",BuildingBase=8,BuildingText="Replaces the Library: +2 science as well.",
   Weakness="Hollow Bones",WeaknessText="All units -1 defence.",Agenda=1};
  entries[31]=new CivEntry{Epithet="The Machines That Remained",
   Background="Self-repairing machines of brass and iron that walk the ash plains of a world their makers destroyed. They think as one network, build without rest and feel nothing that could be called faith.",
   History="The makers of the Iron Mind built it to run their cities, then burned their world in a war the machines could not stop. When the fires died, the machines kept working: rebuilding, mining, counting. Over the centuries their network woke into a single cold intelligence. Prime Unit Aurex respects only efficiency and strength, and has begun to wonder what lies beyond the gateways.",
   Strengths="Tireless production; armoured automatons; great assembly forges.",Weaknesses="No faith at all.",
   Ability="Tireless",AbilityText="+2 production in every city.",
   Unit="Automaton",UnitBase=6,UnitAttack=1,UnitDefence=1,UnitText="Replaces the Rifleman: +1 attack, +1 defence.",
   Building="Assembly Forge",BuildingBase=30,BuildingText="Replaces the Workshop: +3 production as well.",
   Weakness="Cold Logic",WeaknessText="Its cities make no faith.",Agenda=7};
  entries[32]=new CivEntry{Epithet="Keepers of the Ash-Forges",
   Background="Stocky, bearded dwarves who survived the burning of the Ashen Waste in fortresses sunk deep beneath the ash. They mine, forge and brew, trade hard bargains and never forget a debt or a grudge.",
   History="When the sky caught fire, the clans sealed their great doors and waited underground for a hundred years. They emerged to find a dead world and machines walking it, and have fought the Iron Mind over every seam of ore since. Their runesmiths forge weapons that do not break. High Thane Durgrim measures a people by what they build, and envies every wonder that is not dwarven.",
   Strengths="Hills and mines are rich; unbreakable infantry; rune halls.",Weaknesses="Grudges: war makes them restless.",
   Ability="Deep Delvers",AbilityText="Each worked hill or mine +1 production.",
   Unit="Hammerguard",UnitBase=7,UnitDefence=2,UnitText="Replaces the Spearman: +2 defence.",
   Building="Rune Hall",BuildingBase=32,BuildingText="Replaces the Barracks: +2 production as well.",
   Weakness="Grudge-Bearers",WeaknessText="While at war with anyone, every city -1 content citizen.",Agenda=3};
  entries[33]=new CivEntry{Epithet="Rulers of the Burning Depths",
   Background="Horned demons of fire and shadow who rule the Infernal Depths from citadels of black glass above rivers of lava. They feed on fear and worship their own Archfiend.",
   History="The Abyssal Throne has stood since the Depths first burned. Its demon princes fight endlessly for the Archfiend's favour, and only the threat of the orc hordes ever unites them. Malgoroth seized the throne by devouring his rivals and now looks hungrily at the gateways: a whole new world of mortals to rule. Every other people distrusts the demons, and with reason.",
   Strengths="Ferocious warriors; infernal knights; blood altars that burn with faith.",Weaknesses="Hated by every other people.",
   Ability="Hellborn",AbilityText="All units +1 attack.",
   Unit="Infernal Knight",UnitBase=5,UnitAttack=2,UnitText="Replaces the Knight: +2 attack.",
   Building="Blood Altar",BuildingBase=36,BuildingText="Replaces the Temple: +3 faith as well.",
   Weakness="Hated by All",WeaknessText="Every other civilisation thinks 10 worse of it.",Agenda=4};
  entries[34]=new CivEntry{Epithet="The Horde of the Burning Plains",
   Background="Green-skinned orcs who roam the cinder plains of the Infernal Depths in great warbands, riding wolves and fighting anything that moves, including each other. Strength is the only law.",
   History="The orcs were dragged into the Depths long ago as slaves of the demons, and broke their chains in a revolt that still burns in their songs. Since then the Bloodtusk Horde has fought the Abyssal Throne without end. Warchief Grukka united the clans by beating every rival chief, and he will only respect a people that can beat him.",
   Strengths="Brutal shock troops; frenzied attacks; fighting pits.",Weaknesses="Unruly: little patience for learning.",
   Ability="Blood Frenzy",AbilityText="Units attacking +1 attack.",
   Unit="Bloodtusk Brute",UnitBase=2,UnitAttack=2,UnitDefence=1,UnitText="Replaces the Warrior: +2 attack, +1 defence.",
   Building="Fighting Pit",BuildingBase=32,BuildingText="Replaces the Barracks: +2 production as well.",
   Weakness="Unruly",WeaknessText="-1 science in every city.",Agenda=7};
  entries[35]=new CivEntry{Epithet="The Swarm Beneath the Marsh",
   Background="Ratkin: scheming rat-folk who teem in warrens dug under the Plague Marshes. They breed fast, build strange machines from scrap and fear nothing more than each other.",
   History="The Gnawing Swarm grew in the dark of the marsh-world, feeding on the dead forests and on each other. Its seers claim the plague is a gift. Every few generations the Swarm boils out of its tunnels in numbers no foe can count, then falls back into civil war. Elder Seer Skritt holds the clans together with promises of new lands to overrun.",
   Strengths="Huge food surpluses; swarming rats; deep warrens.",Weaknesses="Cowardly: units defend poorly.",
   Ability="Endless Brood",AbilityText="+2 food in every city.",
   Unit="Swarm Rat",UnitBase=2,UnitAttack=1,UnitText="Replaces the Warrior: +1 attack.",
   Building="Warren",BuildingBase=7,BuildingText="Replaces the Granary: +2 food as well.",
   Weakness="Cowardly",WeaknessText="All units -1 defence.",Agenda=4};
  entries[36]=new CivEntry{Epithet="The Spore Circle",
   Background="Myconids: patient, slow-moving fungus-folk who grow in the dead forests of the Plague Marshes. They share one dreaming mind through their roots and turn rot into new life.",
   History="The Spore Circle grew from the first mushroom that fed on the marsh's dead trees, and has spread silently ever since. It does not wage war if it can avoid it, but its spore-guards have choked many invaders. The ratkin raid its groves, and it has learned to grow walls of living fungus. Elder Mycel wants only peace and room to grow.",
   Strengths="Marsh and wasteland feed them; hardy spore-guards; living groves.",Weaknesses="Rooted: slow to move.",
   Ability="Spore Bloom",AbilityText="Each worked marsh or wasteland tile +1 food.",
   Unit="Sporeguard",UnitBase=7,UnitDefence=1,UnitText="Replaces the Spearman: +1 defence.",
   Building="Mycelium Grove",BuildingBase=33,BuildingText="Replaces the Gardens: +1 food and +1 content citizen as well.",
   Weakness="Rooted",WeaknessText="Land units have 1 less movement.",Agenda=5};
  entries[37]=new CivEntry{Epithet="Courts Beneath the Waves",
   Background="Merfolk who rule coral palaces in the shallows of the Drowned Deep, trading pearls and secrets with every island. They are graceful in water and awkward on land.",
   History="The Tidebound Courts rose when the old sea-kings gathered the scattered reef-clans under one crown of coral. Their great sea-beasts, ridden by champions, keep the deep monsters at bay. They trade with the lizardfolk of the islands, warily. Tide Queen Nerissa values trade and friendship above conquest, and remembers every merchant who cheated her.",
   Strengths="Rich seas; leviathan riders; coral palaces.",Weaknesses="Clumsy fighters on land.",
   Ability="Children of the Tide",AbilityText="Each worked water tile +1 food and +1 gold.",
   Unit="Leviathan Rider",UnitBase=14,UnitAttack=1,UnitMoves=2,UnitText="Replaces the War Galley: +1 attack, +2 movement.",
   Building="Coral Palace",BuildingBase=43,BuildingText="Replaces the Harbour: +2 gold and +1 faith as well.",
   Weakness="Out of Water",WeaknessText="Land units -1 attack.",Agenda=0};
  entries[38]=new CivEntry{Epithet="The Old Ones of the Islands",
   Background="Cold-blooded lizardfolk who build stepped pyramids on the jungle islands of the Drowned Deep, guided by star-reading priests whose calendars count back to the world's beginning.",
   History="The Scaled Dominion claims to be the oldest people of all, raised by the first gods before they vanished. Its temple-cities slept for ages and are slowly waking again. Its warriors are patient and tireless, its priests obsessed with prophecy. Coatl Priest Xoth guards the islands' borders jealously and reads omens of war in every outsider.",
   Strengths="Tough in jungle and swamp; saurian warriors; star-temples.",Weaknesses="Slow to stir: cities produce less.",
   Ability="Thick Scales",AbilityText="Units on jungle, marsh or mangrove +1 defence.",
   Unit="Saurian Warrior",UnitBase=7,UnitAttack=1,UnitDefence=1,UnitText="Replaces the Spearman: +1 attack, +1 defence.",
   Building="Stepped Pyramid",BuildingBase=36,BuildingText="Replaces the Temple: +2 faith and +1 science as well.",
   Weakness="Slow to Stir",WeaknessText="-1 production in every city.",Agenda=6};
 }
}
// The rules behind the civilisations above. Each is a small hook into the yield, content, faith, combat, movement, growth,
// cost, religion and diplomacy code.
public partial class World {
 CivEntry Civ(int owner){return owner>=0&&owner<civCount?CivLore.Of(nations[owner].civ):null;}
 int CivId(int owner){return owner>=0&&owner<civCount&&CivLore.Full(nations[owner].civ)?nations[owner].civ:-1;}
 public bool Coastal(City c){for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var t=Step(c.x,c.y,dx,dy);if(t!=null&&IsWater(t.terrain))return true;}return false;}
 bool UniqueBuilding(City c,int b){var e=Civ(c.owner);return e!=null&&e.BuildingBase==b&&HasBuilding(c,b);}
 City CapitalOf(int owner){int id=nations[owner].capital;return cities.Find(c=>c.id==id&&c.owner==owner)??cities.Where(c=>c.owner==owner).OrderBy(c=>c.id).FirstOrDefault();}
 int FromCapital(City c){var cap=CapitalOf(c.owner);return cap==null?0:Distance(c.x,c.y,cap.x,cap.y);}
 bool AtWarWithAnyone(int owner){for(int o=0;o<civCount;o++)if(o!=owner&&AtWar(owner,o))return true;return false;}
 public string UnitNameFor(int owner,int type){var e=Civ(owner);return e!=null&&e.UnitBase==type?e.Unit:UnitNames[type];}
 public string UnitName(Unit u){return u==null?"":UnitNameFor(u.owner,u.type);}
 public string BuildNameFor(int owner,int b){var e=Civ(owner);int u=BuildUnit(b);if(e!=null&&u>=0&&e.UnitBase==u)return e.Unit;if(e!=null&&u<0&&e.BuildingBase==b)return e.Building;return BuildName(b);}
 public int MaxMoves(Unit u){var e=Civ(u.owner);int m=Moves[u.type];if(e!=null&&e.UnitBase==u.type)m+=e.UnitMoves;if(CivId(u.owner)==11&&Mounted(u.type))m+=2;if(CivId(u.owner)==36&&!IsNaval(u.type))m-=2;return Math.Max(2,m);}
 void ApplyCivYields(City c,ref int f,ref int p,ref int s,ref int g){
  int civ=CivId(c.owner);if(civ<0)return;var worked=Worked(c);bool fresh=FreshWater(c.x,c.y);
  switch(civ){
   case 0:if(Coastal(c))g+=2;else p=Math.Max(1,p-1);if(UniqueBuilding(c,43))s+=2;break;
   case 1:f+=worked.Count(t=>t.terrain==3);if(UniqueBuilding(c,31)){g+=2;f++;}break;
   case 3:g--;break;
   case 4:g+=c.pop/3;if(UniqueBuilding(c,8))s+=2;break;
   case 5:if(c.pop>=5)s+=2;if(UniqueBuilding(c,42))s+=2;break;
   case 7:p+=worked.Count(t=>t.terrain==2||t.mine);if(UniqueBuilding(c,30))p+=2;break;
   case 8:if(UniqueBuilding(c,34))g+=2;break;
   case 10:if(c.pop>=7)f--;if(UniqueBuilding(c,32))p+=2;break;
   case 11:s=Math.Max(0,s-1);if(UniqueBuilding(c,7))p++;break;
   case 12:if(UniqueBuilding(c,33))p++;break;
   case 17:if(c.library)s+=2;if(UniqueBuilding(c,38))s+=2;g--;break;
   case 18:if(fresh){f++;p++;}else p=Math.Max(1,p-1);break;
   case 19:{var cap=CapitalOf(c.owner);if(cap!=null&&cap!=c&&Connected(c)){p++;g++;}if(UniqueBuilding(c,37))f++;break;}
   case 20:if(c.pop>=4)s++;if(UniqueBuilding(c,42))s++;break;
   case 21:f+=worked.Count(t=>t.terrain==6||t.terrain==7||t.terrain==9);break;
   case 22:f+=worked.Count(t=>t.terrain==2);if(UniqueBuilding(c,7)){f++;p++;}break;
   case 23:s+=1+(nations[c.owner].capital==c.id?2:0);if(UniqueBuilding(c,8))s+=2;break;
   case 24:g--;break;
   case 25:if(HasBuilding(c,31)){s++;g++;}if(UniqueBuilding(c,38))s+=2;break;
   case 26:if(HasBuilding(c,34))g+=2;if(UniqueBuilding(c,33)){g++;f++;}if(FromCapital(c)>8)p=Math.Max(1,p-1);break;
   case 27:g--;break;
   case 28:s++;if(UniqueBuilding(c,8))s+=2;break;
   case 29:f+=worked.Count(t=>Forested(t.terrain));break;
   case 30:if(UniqueBuilding(c,8))s+=2;break;
   case 31:p+=2;if(UniqueBuilding(c,30))p+=3;break;
   case 32:p+=worked.Count(t=>t.terrain==2||t.mine);if(UniqueBuilding(c,32))p+=2;break;
   case 34:s=Math.Max(0,s-1);if(UniqueBuilding(c,32))p+=2;break;
   case 35:f+=2;if(UniqueBuilding(c,7))f+=2;break;
   case 36:f+=worked.Count(t=>t.terrain==9||t.terrain==10);if(UniqueBuilding(c,33))f++;break;
   case 37:{int wet=worked.Count(t=>IsWater(t.terrain));f+=wet;g+=wet;if(UniqueBuilding(c,43))g+=2;break;}
   case 38:p=Math.Max(1,p-1);if(UniqueBuilding(c,36))s++;break;
  }
 }
 public int CivContent(City c){
  int civ=CivId(c.owner);if(civ<0)return 0;int n=cities.Count(a=>a.owner==c.owner);
  switch(civ){
   case 3:return UniqueBuilding(c,36)?1:0;
   case 4:return FromCapital(c)>8?-1:0;
   case 5:return n>4?-1:0;
   case 8:return (HasBuilding(c,34)||HasBuilding(c,42)?1:0)+(UniqueBuilding(c,34)?1:0)-(AtWarWithAnyone(c.owner)?1:0);
   case 12:return c.walls?0:-1;
   case 19:return (UniqueBuilding(c,37)?1:0)-(FromCapital(c)>10?1:0);
   case 20:return n>5?-1:0;
   case 21:return (UniqueBuilding(c,42)?1:0)-(FromCapital(c)>6?1:0);
   case 24:return UniqueBuilding(c,36)?1:0;
   case 25:return c.pop>=10?-1:0;
   case 26:return HasBuilding(c,34)?1:0;
   case 27:return (UniqueBuilding(c,36)?1:0)+(c.religion>=0&&c.religion==nations[c.owner].founded?1:0);
   case 28:return (HasBuilding(c,34)?1:0)-(c.walls?0:1);
   case 32:return AtWarWithAnyone(c.owner)?-1:0;
   case 36:return UniqueBuilding(c,33)?1:0;
  }
  return 0;
 }
 public int CivFaith(City c){int civ=CivId(c.owner);if(civ<0)return 0;
  switch(civ){case 3:return (HasBuilding(c,36)?2:0)+(UniqueBuilding(c,36)?1:0);case 4:return UniqueBuilding(c,8)?1:0;case 18:return UniqueBuilding(c,36)?2:0;case 20:case 21:return UniqueBuilding(c,42)?1:0;case 24:return 1+(UniqueBuilding(c,36)?1:0);case 25:return UniqueBuilding(c,38)?1:0;case 27:return UniqueBuilding(c,36)?1:0;case 29:return UniqueBuilding(c,36)?2:0;case 31:return -100;case 33:return UniqueBuilding(c,36)?3:0;case 37:return UniqueBuilding(c,43)?1:0;case 38:return UniqueBuilding(c,36)?2:0;}
  return 0;}
 public int CivInfluence(City c){return CivId(c.owner)==20&&c.pop>=4?1:0;}
 public int CivFoodNeeded(City c){int civ=CivId(c.owner);return civ==7||civ==29?4:0;}
 // Religious pressure from this civilisation's cities, in percent (Holy Imperium and Maurya India +50%).
 public int CivPressure(int owner){int civ=CivId(owner);return civ==3||civ==27?150:100;}
 public int CivCost(City c,int b,int cost){int civ=CivId(c.owner);int u=BuildUnit(b);if(civ==22&&u>=0&&Mounted(u))cost+=cost/4;if(civ==23&&u==0)cost+=cost/3;return cost;}
 public int CivStrength(Unit u,Unit enemy,bool defending){
  var e=Civ(u.owner);int s=0;if(e!=null&&e.UnitBase==u.type)s+=defending?e.UnitDefence:e.UnitAttack;int civ=CivId(u.owner);var here=At(u.x,u.y);
  if(defending){if(civ==1&&cities.Any(c=>c.x==u.x&&c.y==u.y&&c.walls))s--;if(civ==24&&here!=null&&here.terrain==2)s++;if(civ==30||civ==35)s--;if(civ==38&&here!=null&&(here.terrain==11||here.terrain==9||here.terrain==19))s++;}
  else{if(civ==10&&u.hp>=10)s++;if(civ==12&&!IsNaval(u.type)&&here!=null&&NextToWater(here))s++;if(civ==34)s++;if(civ==37&&!IsNaval(u.type))s--;}
  if(civ==33&&!defending)s++;
  return s;
 }
 bool NextToWater(Tile t){for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=Step(t.x,t.y,dx,dy);if(n!=null&&IsWater(n.terrain))return true;}return false;}
 // Leader agendas (Civilization VI): a rival's opinion of another civilisation moves by what it values.
 // Every other people distrusts the demons of the Abyssal Throne (-10), whatever its agenda.
 public int AgendaOpinion(int observer,int other){return AgendaCore(observer,other)+(observer!=other&&CivId(other)==33&&CivId(observer)!=33?-10:0);}
 int AgendaCore(int observer,int other){
  var e=Civ(observer);if(e==null||e.Agenda<0||observer==other)return 0;
  switch(e.Agenda){
   case 0:return (Friends(observer,other)||Allied(observer,other)?12:0)-(Embargoed(observer,other)?10:0);
   case 1:{int mine=nations[observer].tech.Count(t=>t),theirs=nations[other].tech.Count(t=>t);return theirs>mine+2?-10:theirs<mine?5:0;}
   case 2:{int r=nations[observer].founded;if(r<0)return 0;var cap=CapitalOf(other);return (cap!=null&&cap.religion==r?12:0)-(nations[other].founded>=0?10:0);}
   case 3:return cities.Where(c=>c.owner==other).Sum(c=>c.buildings.Count(IsWonder))>cities.Where(c=>c.owner==observer).Sum(c=>c.buildings.Count(IsWonder))?-10:0;
   case 4:return cities.Count(c=>c.owner==other)>cities.Count(c=>c.owner==observer)?-10:0;
   case 5:return AtWarWithAnyone(other)?-12:5;
   case 6:return units.Any(u=>u.owner==other&&Power[u.type]>0&&At(u.x,u.y).owner==observer)?-10:0;
   case 7:{int mine=units.Count(u=>u.owner==observer&&Power[u.type]>0),theirs=units.Count(u=>u.owner==other&&Power[u.type]>0);return theirs>=mine?8:theirs*2<mine?-8:0;}
  }
  return 0;
 }
 public string AgendaReason(int observer,int other){var e=Civ(observer);if(e==null||e.Agenda<0)return "";int v=AgendaOpinion(observer,other);return CivLore.AgendaNames[e.Agenda]+(v>0?" (pleased with you, +"+v+")":v<0?" (displeased with you, "+v+")":" (indifferent)")+": "+CivLore.AgendaTexts[e.Agenda];}
 // Rivals found the faiths their peoples historically followed, if still free.
 public int PreferredFaith(int owner){var e=Civ(owner);if(e==null)return -1;foreach(int r in e.Faiths)if(FoundingReason(owner,r)=="")return r;return -1;}
}
}
