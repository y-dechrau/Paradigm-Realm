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
 public const int Count=17,TraitCount=12,PortraitCount=17;
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
 public static readonly string[] Names={"Azure Crown","Oasis Sultanate","Sylvan Court","Holy Imperium","Sahel Empire","Hellenic League","Crystal Dominion","Frostpeak Realm","Aurelian Republic","Lutetian Empire","Rising Isles","Steppe Horde","Northmarch Kingdom","Sunspire Emirate","Lionmarch Kingdom","Jade Dynasty","Serpent League"};
 public static readonly string[] Leaders={"King Aldric","Queen Samira","Queen Elowen","Emperor Constantin","Mansa Kanto","Strategos Kleon","Queen Nyxara","Queen Brynja","Consul Aurelius","Emperor Lucien","Lord Kaito","Khan Temur","King Halvard","Emir Rashid","King Aldous","Empress Mei","High King Itzel"};
 public static readonly int[] Portraits={0,1,3,11,10,9,4,2,12,5,6,14,7,8,13,15,16};
 public static readonly int[][] Traits={new[]{0,1},new[]{1,2},new[]{2,3},new[]{3,4},new[]{4,5},new[]{5,6},new[]{6,7},new[]{7,8},new[]{8,11},new[]{9,10},new[]{9,0},new[]{10,11},new[]{0,8},new[]{1,6},new[]{3,9},new[]{4,6},new[]{2,9}};
 // Offered and used as rivals. 2 and 6 hidden for now; 15 and 16 until their portrait art exists.
 public static readonly bool[] Enabled={true,true,false,true,true,true,false,true,true,true,true,true,true,true,true,false,false};
 public static bool[] AwaitingArt={false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,true,true};
 public static IEnumerable<int> Offered(){return Enumerable.Range(0,Count).Where(i=>Enabled[i]);}
 public static readonly string[] PortraitTitles={"King of the coastal kingdom","Desert queen of the oasis","Mountain queen of the snow citadel","Enchanted forest queen","Void queen of the crystal realm","Emperor of the grand capital","Samurai lord","King of a coastal kingdom","Guardian of the golden citadel","Hellenistic commander","Emperor of the golden city","Emperor of the grand cathedral","Caesar of the eternal city","Crusader king","Steppe warlord","Empress of the jade court (art needed)","King of the serpent coast (art needed)"};
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
  "The Serpent League sees you. Our fields feed many, and our warriors guard them well."};
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
  var r=new Random(seed^0x5eed);var pool=CivilisationCatalog.Offered().Where(i=>i!=nations[0].civ).OrderBy(i=>r.Next()).ToList();
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
 public int CostFor(City c,int b){int cost=Cost(b);if(c==null)return cost;if((IsWonder(b)||IsProject(b))&&HasTrait(c.owner,TraitMonumental))cost-=cost/5;if(BuildUnit(b)==0&&HasTrait(c.owner,TraitExpansionist))cost-=cost/3;return cost;}
 bool CivilisationsValid(){return nations.All(n=>n!=null&&n.traits!=null&&n.traits.Length==2&&n.traits.All(t=>t>=-1&&t<CivilisationCatalog.TraitCount)&&n.civ>=-1&&n.civ<CivilisationCatalog.Count&&n.portrait>=-1&&n.portrait<CivilisationCatalog.PortraitCount);}
 // First contacts not yet shown to the player (the screen opens Diplomacy with each in turn). Not saved.
 [NonSerialized] public List<int> newContacts=new List<int>();
}
}
