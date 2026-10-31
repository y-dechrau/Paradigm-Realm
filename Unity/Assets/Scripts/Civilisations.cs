using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Twelve premade civilisations, each with a leader portrait (Resources/Leaders, from the owner's leader art) and two of twelve
// traits; every trait belongs to exactly two civilisations and no two share the same pair. Modelled on leader traits in
// classic 4X games: each trait is one clear, integer rule. A custom civilisation picks its own name, leader, portrait and
// any two traits.
public static class CivilisationCatalog {
 // 17 civilisations: Sylvan Court and Crystal Dominion are hidden for now (owner request), and the last two wait for their
 // portrait art (leader-15.png, leader-16.png); RealmGame enables them once the picture is found. Hidden or waiting
 // civilisations are never offered or used as rivals, and their indices stay fixed so saves keep working.
 public const int Count=39,TraitCount=12,PortraitCount=39;
 public static readonly string[] TraitNames={"Seafaring","Mercantile","Agrarian","Spiritual","Monumental","Diplomatic","Scholarly","Stalwart","Industrious","Militaristic","Horse Lords","Expansionist"};
 public static readonly string[] TraitTexts={
  "+1 food on every worked water tile",
  "+2 gold in every city",
  "+1 food in every city",
  "+1 content citizen in every city",
  "Wonders and Transcendence stages cost a fifth less",
  "Concord victory needs 15 turns instead of 20",
  "+1 science in every city",
  "+1 defence for your units inside your borders",
  "+1 production in every city",
  "New military units start as Veterans",
  "Horsemen and Knights +1 attack",
  "Settlers cost a third less"};
 public static readonly string[] Names={"Azure Crown","Oasis Sultanate","Sylvan Court","Holy Imperium","Sahel Empire","Hellenic League","Crystal Dominion","Frostpeak Realm","Aurelian Republic","Lutetian Empire","Rising Isles","Steppe Horde","Northmarch Kingdom","Sunspire Emirate","Lionmarch Kingdom","Jade Dynasty","Serpent League","China","Egypt","Rome","Greece","Aztec Empire","Inca Empire","Maya","Ethiopia","Arabia","Persia","India","Babylon","Sylvan Elves","Aerie of Wings","The Iron Mind","Ashforge Clans","Abyssal Throne","Bloodtusk Horde","Gnawing Swarm","Spore Circle","Tidebound Courts","Scaled Dominion"};
 public static readonly string[] Leaders={"King Aldric","Queen Samira","Queen Elowen","Emperor Constantin","Mansa Kanto","Strategos Kleon","Queen Nyxara","Queen Brynja","Consul Aurelius","Emperor Lucien","Lord Kaito","Khan Temur","King Halvard","Emir Rashid","King Aldous","Empress Mei","High King Itzel","Qin Shi Huang","Ramesses II","Augustus","Pericles","Moctezuma II","Pachacuti","Pakal the Great","Menelik II","Harun al-Rashid","Cyrus the Great","Ashoka","Hammurabi","Queen Aelindra","Skylord Kerrak","Prime Unit Aurex","High Thane Durgrim","Archfiend Malgoroth","Warchief Grukka","Elder Seer Skritt","Elder Mycel","Tide Queen Nerissa","Coatl Priest Xoth"};
 public static readonly int[] Portraits={0,1,3,11,10,9,4,2,12,5,6,14,7,8,13,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38};
 public static readonly int[][] Traits={new[]{0,1},new[]{1,2},new[]{2,3},new[]{3,4},new[]{4,5},new[]{5,6},new[]{6,7},new[]{7,8},new[]{8,11},new[]{9,10},new[]{9,0},new[]{10,11},new[]{0,8},new[]{1,6},new[]{3,9},new[]{4,6},new[]{2,9},
  // Real civilisations: China, Egypt, Rome, Greece, Aztec, Inca, Maya, Ethiopia, Arabia, Persia, India, Babylon.
  new[]{6,8},new[]{4,2},new[]{9,11},new[]{6,0},new[]{9,4},new[]{2,7},new[]{6,3},new[]{3,7},new[]{1,3},new[]{5,1},new[]{3,5},new[]{4,7},
  // Native peoples of the parallel worlds (not offered as playable or home rivals): Elves, Avians, Machines, Dwarves, Demons, Orcs,
  // Ratkin, Myconids, Merfolk, Lizardfolk.
  new[]{2,6},new[]{5,11},new[]{4,8},new[]{1,7},new[]{8,9},new[]{7,10},new[]{6,11},new[]{2,11},new[]{0,3},new[]{0,7}};
 // Offered and used as rivals. 2 and 6 hidden for now; 15 and 16 until their portrait art exists.
 public static readonly bool[] Enabled={true,true,false,true,true,true,false,true,true,true,true,true,true,true,true,false,false,true,true,true,true,true,true,true,true,true,true,true,true,false,false,false,false,false,false,false,false,false,false};
 public static bool[] AwaitingArt={false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,true,true,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false};
 public static IEnumerable<int> Offered(){return Enumerable.Range(0,Count).Where(i=>Enabled[i]);}
 // The New Game screen offers two groups (owner request): Fantasy Kingdoms (0-16) and Real World Civilisations (17 on).
 public const int FirstReal=17;
 public const int FirstNative=29;
 public static bool IsReal(int civ){return civ>=FirstReal&&civ<FirstNative;}
 public static bool IsNative(int civ){return civ>=FirstNative&&civ<Count;}
 public static bool RealPortrait(int portrait){return portrait>=FirstReal&&portrait<FirstNative;}
 // Which parallel world each native people lives in (1 Fae Wilds ... 5 Drowned Deep), and the order they are placed in so that
 // every world gets one people before any gets a second.
 public static int NativeRealm(int civ){return IsNative(civ)?1+(civ-FirstNative)/2:0;}
 public static readonly int[] NativeOrder={29,33,31,35,37,30,34,32,36,38};
 public static IEnumerable<int> Offered(bool real){return Offered().Where(i=>IsReal(i)==real);}
 public static readonly string[] PortraitTitles={"King of the coastal kingdom","Desert queen of the oasis","Mountain queen of the snow citadel","Enchanted forest queen","Void queen of the crystal realm","Emperor of the grand capital","Samurai lord","King of a coastal kingdom","Guardian of the golden citadel","Hellenistic commander","Emperor of the golden city","Emperor of the grand cathedral","Caesar of the eternal city","Crusader king","Steppe warlord","Empress of the jade court (art needed)","King of the serpent coast (art needed)","First Emperor of Qin","Pharaoh of the New Kingdom","First Emperor of Rome","Statesman of Athens","Huey Tlatoani of Tenochtitlan","Sapa Inca of Tawantinsuyu","Ajaw of Lakamha (Palenque)","Emperor of Ethiopia","Abbasid Caliph of Baghdad","King of Kings of Persia","Mauryan Emperor","King of Babylon","Elf queen of the Fae Wilds","Avian lord of the high aeries","Machine mind of the Ashen Waste","Dwarf thane of the ash-forges","Demon lord of the Infernal Depths","Orc warchief of the burning plains","Ratkin seer of the Plague Marshes","Myconid elder of the spore groves","Merfolk queen of the Drowned Deep","Lizardfolk priest of the reef temples"};
 // What each leader says on meeting you (peace); war and friendship have their own lines in World.Greeting.
 public static readonly string[] Greetings={
  "The Azure Crown greets you. Our sails have found your shores; may our harbours trade in peace.",
  "Peace be upon you, traveller. The oasis is generous to those who come as friends.",
  "The forest has watched you approach. Walk softly, and the Sylvan Court will listen.",
  "In the name of the Imperium, welcome. Our cathedrals stand for all who keep the peace.",
  "The Sahel Empire welcomes you. Our caravans carry gold to friends, and remember those who are not.",
  "Greetings from the League. Let us reason together before we reach for our spears.",
  "The crystals foretold your coming. Speak, and the Dominion will weigh your words.",
  "You have climbed far to reach Frostpeak. Our halls are warm for honest guests.",
  "Hail, from the Senate and people of the Aurelian Republic. The roads we build can lead to you.",
  "The Emperor receives you. Order and glory await those who stand beside us.",
  "Our islands have long watched the sea. You are welcome, if you come with honour.",
  "The steppe is wide and our horses are many. Speak plainly, stranger.",
  "The Northmarch hails you from the cold coasts. Our ships are many and our word is iron.",
  "Welcome beneath the Sunspire. Knowledge and coin flow freely to friends of the Emirate.",
  "The Lion greets you. Keep faith with us, and our swords will keep faith with you.",
  "The Jade Court acknowledges you. Patience and learning are the roots of lasting peace.",
  "The Serpent League sees you. Our fields feed many, and our warriors guard them well.",
  "The Son of Heaven receives you. All under heaven is one realm; show us whether you will be part of its order.",
  "Ramesses, beloved of the gods, greets you. The Nile has carried our name across the ages; may yours prove worthy of it.",
  "Augustus greets you in the name of the Senate and People of Rome. Peace is ours to give, to those who keep it.",
  "Pericles of Athens welcomes you. Let us debate as free citizens before we take up the spear.",
  "Moctezuma greets you. The Sun demands much of us; tell us what you will offer it.",
  "Pachacuti, Earth-Shaker, greets you. Our roads cross the mountains; walk them as a friend.",
  "Pakal of Lakamha greets you. The stars have counted your coming; let us see what the days will bring.",
  "Menelik greets you. Our highlands have never bowed to invaders, but they are open to honest friends.",
  "Harun al-Rashid welcomes you to Baghdad. Bring us your scholars and your merchants, and they will be honoured.",
  "Cyrus, King of Kings, greets you. Every people in my realm keeps its gods and customs; yours may too, as friends.",
  "Ashoka greets you. I have seen what conquest costs; let our peoples meet through the law of dharma.",
  "Hammurabi greets you. Our laws are written in stone so that the strong may not oppress the weak.",
  "The trees whispered of you, outsider. Walk lightly in the Wilds and the Sylvan Elves may yet call you friend.",
  "Kraa! The Aerie has watched you from the high winds. Speak quickly; the sky does not wait.",
  "GREETING PROTOCOL ENGAGED. The Iron Mind registers your presence. State your function.",
  "Hail, surface-walker. The Ashforge Clans keep long ledgers of friends and longer ones of grudges. Which will you be?",
  "Mortal. You have wandered far from your little world. The Abyssal Throne will decide what you are worth.",
  "Oi! You lot look soft. The Bloodtusk Horde only talks to the strong - so show us you are.",
  "Yes-yes, welcome-welcome! The Swarm is many, the Swarm is everywhere. Friends? Friends, yes... for now.",
  "The Spore Circle feels your coming through the roots. Rest, and let us grow together.",
  "The tide brings strangers to our courts. Be welcome, land-dweller, while the waters are calm.",
  "The stars foretold a warm-blooded visitor. The Scaled Dominion remembers when the world was young."};
 public static string TraitLine(int[] traits){return string.Join("  •  ",traits.Where(t=>t>=0&&t<TraitCount).Select(t=>TraitNames[t]+": "+TraitTexts[t]).ToArray());}
}
public partial class World {
 public const int TraitSeafaring=0,TraitMercantile=1,TraitAgrarian=2,TraitSpiritual=3,TraitMonumental=4,TraitDiplomatic=5,TraitScholarly=6,TraitStalwart=7,TraitIndustrious=8,TraitMilitaristic=9,TraitHorseLords=10,TraitExpansionist=11;
 public bool HasTrait(int owner,int trait){if(owner<0||owner>=civCount)return false;var t=nations[owner].traits;return t!=null&&t.Contains(trait);}
 // Earlier saves had one trait: 1 Builders, 2 Scholars, 3 Traders.
 public static int[] TraitsFromLegacy(int legacy){return new[]{legacy==1?TraitIndustrious:legacy==2?TraitScholarly:legacy==3?TraitMercantile:-1,-1};}
 // Rivals take the premade civilisations the player did not choose, in a seeded order; beyond twelve they get a stock name
 // and two seeded traits.
 public void AssignRivalCivilisations()
 {
  var r=new Random(seed^0x5eed);
  // Rivals come from the same group as the player first (Real World or Fantasy), then from the other group if more are needed.
  var pool=CivilisationCatalog.Offered(realWorld).Where(i=>i!=nations[0].civ).OrderBy(i=>r.Next()).Concat(CivilisationCatalog.Offered(!realWorld).OrderBy(i=>r.Next())).ToList();
  for(int o=1;o<civCount;o++)
  {
   var n=nations[o];
   if(pool.Count>0){int civ=pool[0];pool.RemoveAt(0);n.civ=civ;n.name=CivilisationCatalog.Names[civ];n.leader=CivilisationCatalog.Leaders[civ];n.portrait=CivilisationCatalog.Portraits[civ];n.traits=(int[])CivilisationCatalog.Traits[civ].Clone();}
   else{n.civ=-1;n.name=o<=RivalNames.Length?RivalNames[o-1]:"Rival "+o;n.leader="";n.portrait=-1;int a=r.Next(CivilisationCatalog.TraitCount),b=(a+1+r.Next(CivilisationCatalog.TraitCount-1))%CivilisationCatalog.TraitCount;n.traits=new[]{a,b};}
  }
  // Cities already founded follow their owner's (new) name.
  for(int o=1;o<civCount;o++){int k=1;foreach(var c in cities.Where(c=>c.owner==o).OrderBy(c=>c.id))c.name=NationName(o)+" "+k++;}
 }
 // The player's choice from the New Game screen: a premade civilisation (0-11), or custom (civ -1) with its own name, leader,
 // portrait and two different traits.
 public bool SetPlayerCivilisation(int civ,string customName,string customLeader,int portrait,int traitA,int traitB)
 {
  var n=nations[0];
  if(civ>=0&&civ<CivilisationCatalog.Count&&CivilisationCatalog.Enabled[civ]){n.civ=civ;civilisationName=CivilisationCatalog.Names[civ];n.leader=CivilisationCatalog.Leaders[civ];n.portrait=CivilisationCatalog.Portraits[civ];n.traits=(int[])CivilisationCatalog.Traits[civ].Clone();}
  else if(civ>=0)return false;
  else
  {
   if(traitA<0||traitA>=CivilisationCatalog.TraitCount||traitB<0||traitB>=CivilisationCatalog.TraitCount||traitA==traitB)return false;
   n.civ=-1;civilisationName=CleanName(customName);n.leader=string.IsNullOrWhiteSpace(customLeader)?"":customLeader.Trim().Length>32?customLeader.Trim().Substring(0,32):customLeader.Trim();
   n.portrait=Math.Max(0,Math.Min(CivilisationCatalog.PortraitCount-1,portrait));n.traits=new[]{traitA,traitB};
  }
  n.name=civilisationName;AssignRivalCivilisations();return true;
 }
 // A short greeting from another civilisation to the player, by the state of relations.
 public string Greeting(int other)
 {
  if(other<=0||other>=civCount)return "";var n=nations[other];string who=string.IsNullOrEmpty(n.leader)?NationName(other):n.leader+" of the "+NationName(other);
  if(AtWar(0,other))return who+": \"Your armies stand on our land. We will speak again when they leave.\"";
  if(Allied(0,other))return who+": \"Ally and friend, our banners fly together. What do you need?\"";
  if(Friends(0,other))return who+": \"Welcome back, friend. Our people remember your kindness.\"";
  return who+": \""+(n.civ>=0&&n.civ<CivilisationCatalog.Count?CivilisationCatalog.Greetings[n.civ].Trim('"'):"We greet you, stranger. Let us see what kind of neighbour you will be.")+"\"";
 }
 public int ConcordTargetFor(int owner){return HasTrait(owner,TraitDiplomatic)?15:ConcordTarget;}
 // Building and training cost for this city's owner: Monumental cuts wonders and Transcendence stages by a fifth,
 // Expansionist cuts Settlers by a third.
 // Owner request (31 Oct): in the Dawn Age, buildings (not units, wonders or projects) take twice as long to build.
 public int CostFor(City c,int b){int cost=Cost(b);if(c==null)return cost;if(BuildUnit(b)<0&&!IsWonder(b)&&!IsProject(b)&&Era(c.owner)==0)cost*=2;if((IsWonder(b)||IsProject(b))&&HasTrait(c.owner,TraitMonumental))cost-=cost/5;if(BuildUnit(b)==0&&HasTrait(c.owner,TraitExpansionist))cost-=cost/3;return CivCost(c,b,cost);}
 bool CivilisationsValid(){return nations.All(n=>n!=null&&n.traits!=null&&n.traits.Length==2&&n.traits.All(t=>t>=-1&&t<CivilisationCatalog.TraitCount)&&n.civ>=-1&&n.civ<CivilisationCatalog.Count&&n.portrait>=-1&&n.portrait<CivilisationCatalog.PortraitCount);}
 // First contacts not yet shown to the player (the screen opens Diplomacy with each in turn). Not saved.
 [NonSerialized] public List<int> newContacts=new List<int>();
}
}
