using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Dimensional colonies (plan stage 4, Design/DIMENSIONS-PLAN-2026-11-11.md): compact settlements beyond a gateway that support
// the home empire without being cities. A home civilisation's Settler founds one in another world (a native people still
// founds cities there); a Scout can plant a foothold that grows into one. A colony has one specialisation (a second once it is
// established), a few buildings bought from home, a stock of supplies, and a level of self-sufficiency that rises with time.
// Nothing in it is managed tile by tile.
//
// Transport is counted in LOADS. Each gateway carries a number of loads a turn for each civilisation that may use it
// (World.GateCapacityFor). One load is three supplies going out, or one load of exports coming home, or one unit crossing.
// A priority preset (Supply, Trade or Military) decides who goes first when there is not room for all; the rest is automatic.
//
// A colony cut off from home lives on its stock, then on what it grows; it never vanishes and never changes owner because a
// gateway shut or was captured. Its exports simply stop until loads flow again.
public static class ColonyCatalog {
 public const int Extraction=0,Research=1,Trade=2,Military=3;
 public static readonly string[] Specialisations={"Extraction","Research","Trade","Military support"};
 public static readonly string[] SpecialisationTexts={
  "Sends loads of ore, stone and timber home: +2 production in its home city for each load delivered.",
  "Studies the world it stands in: +2 points a turn toward that family's local discoveries. Needs no gateway to do it.",
  "Sends goods found nowhere at home: +3 gold in its home city for each load delivered.",
  "A base for your soldiers: units on it or beside it heal 2 a turn and suffer none of that world's attrition, and it defends with +2."};
 public const int Exploitation=0,SelfSufficiency=1,Cooperation=2;
 public static readonly string[] Policies={"Exploitation","Self-sufficiency","Cooperation"};
 public static readonly string[] PolicyTexts={
  "One more load of exports a turn; the colony grows no more self-sufficient.",
  "Self-sufficiency rises 2 points a turn faster.",
  "The local people think better of you each turn, and the colony learns from them: +1 point a turn toward local discoveries."};
 public const int Storehouse=0,Workings=1,GardenPlots=2,Palisade=3,StudyHall=4,FamilyWorkshop=5;
 public static readonly string[] Buildings={"Storehouse","Workings","Garden plots","Palisade","Study hall","Local workshop"};
 public static readonly string[] BuildingTexts={"Holds 20 more supplies.","One more load of exports a turn.","Self-sufficiency rises 1 point a turn faster.","Defends with +2 strength and holds out longer.","+1 point a turn toward local discoveries.","A Thermal Forge or Crystal Workshop: +1 load of exports and +1 point a turn. Works only by that world's rules."};
 public static readonly int[] BuildingGold={30,40,30,35,45,60};
 public const int BaseStorage=20,SuppliesPerLoad=3,StartStock=10,EstablishedAfter=25,EstablishedAt=50,MinSpacing=3,FootholdGrows=6;
 public const int ShieldsPerLoad=2,GoldPerLoad=3;
 public static readonly string[] Levels={"Foothold","Colony","Established colony"};
 public const int PrioritySupply=0,PriorityTrade=1,PriorityMilitary=2;
 public static readonly string[] Priorities={"Supply first","Trade first","Military first"};
 public static readonly string[] PriorityTexts={
  "Supplies go out before anything else, then exports come home. Units cross with whatever room is left (at least 2).",
  "Exports come home first, after one load of supplies for each colony that is short. Colonies build their stock more slowly.",
  "Half of every gateway is kept for your units. Supplies and exports share the other half."};
}
public partial class World {
 public int[] gatePriority;
 [NonSerialized] public List<string> dimensionNews=new List<string>();
 [NonSerialized] Dictionary<long,int> troopLoads,cargoLoads;[NonSerialized] int troopLoadsTurn=-1;
 public int GatePriorityOf(int owner){return gatePriority!=null&&owner>=0&&owner<gatePriority.Length?gatePriority[owner]:0;}
 public void SetGatePriority(int owner,int p){if(gatePriority==null||gatePriority.Length<MaxNations)gatePriority=new int[MaxNations];gatePriority[owner]=Math.Max(0,Math.Min(2,p));}
 public Colony ColonyAt(int x,int y){if(colonies==null)return null;foreach(var c in colonies)if(c.x==x&&c.y==y)return c;return null;}
 public List<Colony> ColoniesOf(int owner){var list=new List<Colony>();if(colonies!=null)foreach(var c in colonies)if(c.owner==owner)list.Add(c);return list;}
 // ---------- founding ----------
 // Beyond a gateway a home civilisation founds colonies, not cities.
 public bool ColonyGround(Unit u){if(u==null)return false;var t=At(u.x,u.y);return t!=null&&t.realm>0&&!IsNativeNation(u.owner);}
 public string ColonyReason(Unit u)
 {
  if(u==null)return "No unit.";var t=At(u.x,u.y);if(t==null||t.realm<=0)return "Colonies are founded beyond a gateway.";if(IsNativeNation(u.owner))return "This is your own world.";
  if(u.type!=0&&u.type!=3)return "Only a Settler founds a colony; a Scout can plant a foothold.";if(u.moves<=0)return "The unit needs movement.";
  if(IsWater(t.terrain)||IsImpassable(t.terrain))return "Colonies need land.";if(t.gateLink>0)return "Not on the gateway itself.";
  if(cities.Any(c=>At(c.x,c.y).realm==t.realm&&Distance(c.x,c.y,t.x,t.y)<ColonyCatalog.MinSpacing))return "Too close to a city.";
  if(colonies.Any(c=>At(c.x,c.y).realm==t.realm&&Distance(c.x,c.y,t.x,t.y)<ColonyCatalog.MinSpacing))return "Too close to another colony.";
  if(t.owner>=0&&t.owner!=u.owner)return "This land belongs to "+NationName(t.owner)+".";
  // A recognised claim is kept: no colony within 4 tiles of one you have promised to respect.
  foreach(var c in colonies)if(c.owner!=u.owner&&At(c.x,c.y).realm==t.realm&&Distance(c.x,c.y,t.x,t.y)<=AccordCatalog.ClaimReach&&HasAccord(u.owner,c.owner,AccordCatalog.Claims))return "You have recognised "+NationName(c.owner)+"'s claim here.";
  return "";
 }
 public Colony FoundColony(Unit u,string name=null)
 {
  if(ColonyReason(u)!="")return null;var t=At(u.x,u.y);bool foothold=u.type==3;
  var c=new Colony{id=nextId++,owner=u.owner,x=u.x,y=u.y,founded=turn,level=foothold?0:1,stock=foothold?ColonyCatalog.StartStock/2:ColonyCatalog.StartStock,name=name!=null&&CityNameProblem(name)==""?CleanName(name):NewCityName(u.owner)};
  c.specialisation=u.owner==0?ColonyCatalog.Extraction:ChooseSpecialisation(u.owner);
  colonies.Add(c);units.Remove(u);LinkColony(c);
  if(u.owner==0){Note((foothold?"Foothold planted: ":"Colony founded: ")+c.name+" in the "+RealmNames[t.realm]+".");}
  else if(KnowsNation(u.owner)&&RealmDiscovered(0,t.realm))Note(NationName(u.owner)+" has founded the colony of "+c.name+" in the "+RealmNames[t.realm]+".");
  AddEraScore(c.owner,2,c.name+" founded beyond a gateway");ColonySight(c);Reveal();return c;
 }
 // The best places for a colony near the far gate of a world: open ground with land round it, a few tiles from the arch.
 public List<Tile> ColonySites(int owner,int realm)
 {
  var list=new List<Tile>();Tile far=null;foreach(var t in AllGates())if(t.realm==realm){far=t;break;}if(far==null)return list;
  var scored=new List<KeyValuePair<int,Tile>>();
  foreach(var t in tiles)
  {
   if(t.realm!=realm||IsWater(t.terrain)||IsImpassable(t.terrain)||Barren(t.terrain)||t.gateLink>0)continue;int d=Distance(t.x,t.y,far.x,far.y);if(d<2||d>9)continue;
   if(t.owner>=0&&t.owner!=owner)continue;if(cities.Any(c=>At(c.x,c.y).realm==realm&&Distance(c.x,c.y,t.x,t.y)<ColonyCatalog.MinSpacing)||colonies.Any(c=>At(c.x,c.y).realm==realm&&Distance(c.x,c.y,t.x,t.y)<ColonyCatalog.MinSpacing))continue;
   int land=0;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=Step(t.x,t.y,dx,dy);if(n!=null&&n.realm==realm&&!IsWater(n.terrain)&&!IsImpassable(n.terrain))land+=n.resource>0?3:1;}
   scored.Add(new KeyValuePair<int,Tile>(land*4-d,t));
  }
  foreach(var s in scored.OrderByDescending(s=>s.Key).ThenBy(s=>s.Value.y*Width+s.Value.x)){if(list.Any(o=>Distance(o.x,o.y,s.Value.x,s.Value.y)<ColonyCatalog.MinSpacing))continue;list.Add(s.Value);if(list.Count>=4)break;}
  return list;
 }
 // ---------- links ----------
 // The gateway a colony is served through (the nearest one in its world the owner may use, else the nearest) and its home city.
 public Tile ColonyGate(Colony c)
 {
  var here=At(c.x,c.y);Tile best=null,usable=null;int bd=int.MaxValue,ud=int.MaxValue;
  foreach(var t in AllGates()){if(t.gateLink<=0||t.realm!=here.realm)continue;int d=Distance(c.x,c.y,t.x,t.y);if(d<bd){bd=d;best=t;}if(d<ud&&GateUsable(c.owner,t)){ud=d;usable=t;}}
  return usable??best;
 }
 void LinkColony(Colony c)
 {
  var gate=ColonyGate(c);c.gate=gate==null?-1:GatePair(gate);var home=gate==null?null:HomeSide(gate);
  var city=cities.Find(x=>x.id==c.homeCity&&x.owner==c.owner);if(city!=null)return;
  City best=null;int least=int.MaxValue;foreach(var x in cities){if(x.owner!=c.owner||At(x.x,x.y).realm!=0)continue;int d=home==null?0:Distance(x.x,x.y,home.x,home.y);if(d<least){least=d;best=x;}}
  c.homeCity=best==null?-1:best.id;
 }
 public City ColonyHome(Colony c){return cities.Find(x=>x.id==c.homeCity&&x.owner==c.owner);}
 void ColonySight(Colony c){int reach=FamilyAt(At(c.x,c.y))==DimensionFamily.Resonant&&Knows(c.owner,DimensionFamily.Resonant,AdaptationCatalog.ListeningStones)?4:2;var known=nations[c.owner].known;var here=At(c.x,c.y);for(int k=0;k<tiles.Length;k++){var t=tiles[k];if(t.realm!=here.realm||Distance(t.x,t.y,c.x,c.y)>reach)continue;if(known!=null&&k<known.Length)known[k]=true;if(c.owner==0)t.seen=true;}}
 // ---------- numbers ----------
 bool HasSpec(Colony c,int s){return c.specialisation==s||c.second==s;}
 public int ColonyStorage(Colony c){return ColonyCatalog.BaseStorage+(c.buildings.Contains(ColonyCatalog.Storehouse)?20:0);}
 public int SelfGrowth(Colony c)
 {
  int family=FamilyAt(At(c.x,c.y)),g=c.policy==ColonyCatalog.Exploitation?0:1;if(c.policy==ColonyCatalog.SelfSufficiency)g+=2;if(c.buildings.Contains(ColonyCatalog.GardenPlots))g++;
  if(family==DimensionFamily.Ember&&Knows(c.owner,family,AdaptationCatalog.VentReading)||family==DimensionFamily.Resonant&&Knows(c.owner,family,AdaptationCatalog.HarmonicFarming))g+=2;return g;
 }
 public int SupplyNeed(Colony c)
 {
  if(c.selfSufficiency>=100)return 0;int family=FamilyAt(At(c.x,c.y)),need=((2+c.level)*(100-c.selfSufficiency)+99)/100;
  if(family==DimensionFamily.Ember&&Knows(c.owner,family,AdaptationCatalog.GeothermalKilns)||family==DimensionFamily.Resonant&&Knows(c.owner,family,AdaptationCatalog.SympatheticChimes))need--;return Math.Max(0,need);
 }
 // Loads of exports a colony has ready each turn, by kind. A foothold makes none.
 public void ColonyOutput(Colony c,out int ore,out int goods)
 {
  ore=0;goods=0;if(c.level<1)return;int family=FamilyAt(At(c.x,c.y)),extra=(c.buildings.Contains(ColonyCatalog.Workings)?1:0)+(c.policy==ColonyCatalog.Exploitation?1:0)+(c.buildings.Contains(ColonyCatalog.FamilyWorkshop)&&WorkshopWorks(c)?1:0);
  if(HasSpec(c,ColonyCatalog.Extraction))ore=1+(c.specialisation==ColonyCatalog.Extraction?extra:0)+(family==DimensionFamily.Ember&&Knows(c.owner,family,AdaptationCatalog.DeepTapping)?1:0);
  if(HasSpec(c,ColonyCatalog.Trade))goods=1+(c.specialisation==ColonyCatalog.Trade?extra:0)+(family==DimensionFamily.Resonant&&Knows(c.owner,family,AdaptationCatalog.CrystalEngines)?1:0);
 }
 // A Thermal Forge works only by Ember rules and a Crystal Workshop only by Resonant ones, and only for those who learned how.
 public bool WorkshopWorks(Colony c){int family=FamilyAt(At(c.x,c.y));return family==DimensionFamily.Ember&&Knows(c.owner,family,AdaptationCatalog.ThermalForging)||family==DimensionFamily.Resonant&&Knows(c.owner,family,AdaptationCatalog.CrystalLathe);}
 public string WorkshopName(Colony c){int family=FamilyAt(At(c.x,c.y));return family==DimensionFamily.Ember?"Thermal Forge":family==DimensionFamily.Resonant?"Crystal Workshop":"Local workshop";}
 public int ColonyDefence(Colony c)
 {
  int family=FamilyAt(At(c.x,c.y)),d=c.level<1?0:1;if(c.buildings.Contains(ColonyCatalog.Palisade))d+=2;if(HasSpec(c,ColonyCatalog.Military))d+=2;
  if(family==DimensionFamily.Ember&&Knows(c.owner,family,AdaptationCatalog.Ashcloth)||family==DimensionFamily.Resonant&&Knows(c.owner,family,AdaptationCatalog.TunedMail))d+=2;return d;
 }
 // A colony shelters its owner's units from its world's attrition: on it, or beside it when it supports soldiers.
 bool ColonyShelter(Unit u){if(colonies==null)return false;foreach(var c in colonies)if(c.owner==u.owner&&c.level>=1&&(c.x==u.x&&c.y==u.y||HasSpec(c,ColonyCatalog.Military)&&At(c.x,c.y).realm==At(u.x,u.y).realm&&Distance(c.x,c.y,u.x,u.y)<=1))return true;return false;}
 public int ColonyStrength(Unit u,bool defending){if(!defending||u==null)return 0;var c=ColonyAt(u.x,u.y);return c!=null&&c.owner==u.owner?ColonyDefence(c):0;}
 // ---------- choices ----------
 public string ColonyBuildReason(Colony c,int b)
 {
  if(c==null||b<0||b>=ColonyCatalog.Buildings.Length)return "No such building.";if(c.buildings.Contains(b))return "Already built.";if(c.level<1)return "A foothold must grow into a colony first.";
  if(c.buildings.Count>=(c.level>=2?5:3))return c.level>=2?"A colony has room for five buildings.":"Room for three buildings until the colony is established.";
  if(b==ColonyCatalog.FamilyWorkshop&&!WorkshopWorks(c))return FamilyAt(At(c.x,c.y))==DimensionFamily.Home?"This world has no craft of its own to build on.":"Needs the local discovery "+(FamilyAt(At(c.x,c.y))==DimensionFamily.Ember?"Thermal Forging":"Crystal Lathe")+".";
  if(Treasury(c.owner)<ColonyCatalog.BuildingGold[b])return "Needs "+ColonyCatalog.BuildingGold[b]+" gold.";
  if(c.isolated>0&&c.selfSufficiency<100)return "Cut off from home: nothing can be sent.";return "";
 }
 public bool ColonyBuild(Colony c,int b){if(ColonyBuildReason(c,b)!="")return false;AddDiplomaticGold(c.owner,-ColonyCatalog.BuildingGold[b]);c.buildings.Add(b);if(c.owner==0)Note(c.name+" has built "+(b==ColonyCatalog.FamilyWorkshop?"a "+WorkshopName(c):ColonyCatalog.Buildings[b].ToLower())+".");return true;}
 public string SecondSpecReason(Colony c,int s){if(c.level<2)return "Only an established colony takes a second specialisation.";if(s==c.specialisation)return "That is its first specialisation.";if(s<0||s>3)return "No such specialisation.";return "";}
 public bool SetSpecialisation(Colony c,int s,bool second=false){if(c==null||s<0||s>3)return false;if(second){if(SecondSpecReason(c,s)!="")return false;c.second=s;return true;}if(c.second==s)c.second=-1;c.specialisation=s;return true;}
 public bool SetColonyPolicy(Colony c,int p){if(c==null||p<0||p>2)return false;c.policy=p;return true;}
 int ChooseSpecialisation(int owner){for(int o=0;o<civCount;o++)if(o!=owner&&AtWar(owner,o))return ColonyCatalog.Military;if(Budget(owner)[2]<0)return ColonyCatalog.Trade;return ColoniesOf(owner).Any(c=>HasSpec(c,ColonyCatalog.Extraction))?ColonyCatalog.Research:ColonyCatalog.Extraction;}
 // ---------- units through a gateway ----------
 long LoadKey(int pair,int owner){return (long)pair*200+owner;}
 int Loads(Dictionary<long,int> d,int pair,int owner){int v;return d!=null&&d.TryGetValue(LoadKey(pair,owner),out v)?v:0;}
 // Room left for a civilisation's units this turn at a gateway.
 public int TroopRoom(int owner,Tile gate)
 {
  if(troopLoadsTurn!=turn){troopLoads=new Dictionary<long,int>();troopLoadsTurn=turn;}int pair=GatePair(gate),cap=GateCapacityFor(owner,gate);
  int room=GatePriorityOf(owner)==ColonyCatalog.PriorityMilitary?cap:Math.Max(2,cap-Loads(cargoLoads,pair,owner));return Math.Max(0,room-Loads(troopLoads,pair,owner));
 }
 // "" when the unit may cross now; otherwise why it waits. It stays where it is, on the gateway.
 public string GateTroopHold(Unit u,Tile gate)
 {
  if(IsNativeNation(u.owner)||!dimensionRules)return "";if(TroopRoom(u.owner,gate)<=0)return "The gateway has carried all it can for you this turn ("+GateCapacityFor(u.owner,gate)+" loads). Your "+UnitName(u)+" waits on it.";
  if(troopLoads==null)troopLoads=new Dictionary<long,int>();long key=LoadKey(GatePair(gate),u.owner);int v;troopLoads.TryGetValue(key,out v);troopLoads[key]=v+1;return "";
 }
 // ---------- each turn ----------
 void ColonyTurn(int owner)
 {
  if(colonies==null||IsNativeNation(owner))return;var mine=ColoniesOf(owner);if(cargoLoads==null)cargoLoads=new Dictionary<long,int>();
  foreach(var key in cargoLoads.Keys.Where(k=>k%200==owner).ToList())cargoLoads.Remove(key);
  if(mine.Count==0)return;int priority=GatePriorityOf(owner);
  foreach(var c in mine){LinkColony(c);c.supplied=0;c.shipShields=0;c.shipGold=0;}
  foreach(var group in mine.GroupBy(c=>c.gate))
  {
   var gate=group.Key<0?null:GatePairTile(group.Key);bool open=gate!=null&&GateUsable(owner,gate);
   int cap=open?GateCapacityFor(owner,gate):0;if(open)cap=Math.Max(0,cap-Loads(troopLoads,group.Key,owner));if(open&&priority==ColonyCatalog.PriorityMilitary)cap=Math.Min(cap,GateCapacityFor(owner,gate)-GateCapacityFor(owner,gate)/2);
   int used=0;var list=group.OrderBy(c=>c.stock).ThenBy(c=>c.id).ToList();
   Action<bool> supplies=only=>{foreach(var c in list){int need=SupplyNeed(c);int want=Math.Max(0,Math.Min(2,(need*4-c.stock+ColonyCatalog.SuppliesPerLoad-1)/ColonyCatalog.SuppliesPerLoad));if(only)want=c.stock<need?Math.Min(1,want):0;
     while(want>0&&used<cap&&c.stock<ColonyStorage(c)){c.stock=Math.Min(ColonyStorage(c),c.stock+ColonyCatalog.SuppliesPerLoad);used++;want--;c.supplied++;}}};
   Action exports=()=>{foreach(var c in list){if(c.stock<=0&&c.selfSufficiency<100&&SupplyNeed(c)>0)continue;int ore,goods;ColonyOutput(c,out ore,out goods);while(ore>0&&used<cap){ore--;used++;c.shipShields+=ColonyCatalog.ShieldsPerLoad;}while(goods>0&&used<cap){goods--;used++;c.shipGold+=ColonyCatalog.GoldPerLoad;}}};
   if(priority==ColonyCatalog.PriorityTrade){supplies(true);exports();supplies(false);}else{supplies(false);exports();}
   if(gate!=null)cargoLoads[LoadKey(group.Key,owner)]=used;
   foreach(var c in list)
   {
    bool was=c.isolated>0;int need=SupplyNeed(c);
    if(!open&&!was&&owner==0)dimensionNews.Add(c.name+" is cut off: "+(gate==null?"no gateway serves it":GateBlock(owner,gate))+" It has supplies for "+(need<=0?"as long as it needs":(c.stock/Math.Max(1,need))+" turns")+".");
    if(open&&was&&owner==0)dimensionNews.Add(c.name+" is in touch with home again.");
    c.isolated=open?0:c.isolated+1;
    bool short_=c.stock<need;c.stock=Math.Max(0,c.stock-need);
    if(short_&&owner==0&&c.stock==0&&c.isolated<=1)dimensionNews.Add(c.name+" has run out of supplies and has stopped work.");
    if(!short_)c.selfSufficiency=Math.Min(100,c.selfSufficiency+SelfGrowth(c));
    if(c.level==0&&c.stock>=ColonyCatalog.FootholdGrows){c.level=1;if(owner==0)dimensionNews.Add("The foothold of "+c.name+" has grown into a colony.");}
    if(c.level==1&&turn-c.founded>=ColonyCatalog.EstablishedAfter&&c.selfSufficiency>=ColonyCatalog.EstablishedAt){c.level=2;if(owner==0)dimensionNews.Add(c.name+" is established: it can take a second specialisation and two more buildings.");}
    ColonySight(c);
    // Military support: soldiers on or beside the colony mend.
    if(HasSpec(c,ColonyCatalog.Military))foreach(var u in units)if(u.owner==owner&&At(u.x,u.y).realm==At(c.x,c.y).realm&&Distance(u.x,u.y,c.x,c.y)<=1)u.hp=Math.Min(10,u.hp+2);
    // Cooperation: the peoples of that world warm to the colony's owner.
    if(c.policy==ColonyCatalog.Cooperation&&turn%3==0)for(int o=0;o<civCount;o++)if(o!=owner&&IsNativeNation(o)&&CivilisationCatalog.NativeRealm(nations[o].civ)==At(c.x,c.y).realm&&HasContact(owner,o)&&!AtWar(owner,o))AddOpinion(o,owner,OpinionMods.Dealings,1);
   }
  }
  // Taken by force: an enemy standing on an undefended colony wears its militia down, and then it is theirs.
  foreach(var c in mine)
  {
   Unit foe=null;foreach(var u in units)if(u.x==c.x&&u.y==c.y&&u.owner!=owner&&u.owner<civCount&&AtWar(owner,u.owner)&&Power[u.type]>0){foe=u;break;}
   if(foe==null||units.Any(u=>u.x==c.x&&u.y==c.y&&u.owner==owner)){c.siege=0;continue;}
   int push=Math.Max(1,Attack[foe.type]+foe.promotion+DimensionStrength(foe,false));c.siege+=push;
   if(c.siege>=2+ColonyDefence(c)*2){int old=c.owner;c.owner=foe.owner;c.siege=0;c.homeCity=-1;LinkColony(c);Note((old==0?"Your colony of ":NationName(old)+"'s colony of ")+c.name+" has fallen to "+(foe.owner==0?"your soldiers":NationName(foe.owner))+".");AddOpinion(old,foe.owner,OpinionMods.Conquest,-15);}
   else if(owner==0&&c.siege<=push)dimensionNews.Add(c.name+" is under attack by "+NationName(foe.owner)+" and has no garrison.");
  }
 }
 // What colonies bring a home city, added to its yields.
 internal void ApplyColonyYields(City c,ref int p,ref int s,ref int g)
 {
  if(colonies==null||colonies.Count==0&&(adaptations==null||adaptations.Count==0))return;
  foreach(var col in colonies)
  {
   if(col.owner!=c.owner||col.homeCity!=c.id)continue;int family=FamilyAt(At(col.x,col.y));p+=col.shipShields;g+=col.shipGold;
   if(col.shipShields>0&&family==DimensionFamily.Ember&&Knows(c.owner,family,AdaptationCatalog.FlamelessFurnace))p++;
   if(col.isolated==0&&col.level>=1&&family==DimensionFamily.Resonant&&Knows(c.owner,family,AdaptationCatalog.FarSpeakingLattice))s++;
  }
  if(HasBuilding(c,30)&&SynthesisKnown(c.owner,AdaptationCatalog.FlamelessFoundry))p++;
 }
 // ---------- the turn ----------
 void DimensionsTurn(int owner)
 {
  if(gateStates==null||colonies==null||adaptations==null)EnsureDimensionData();if(gatePriority==null||gatePriority.Length<MaxNations)gatePriority=new int[MaxNations];
  AccordTurn(owner);GateLegends(owner);GateRumours(owner);AdaptationTurn(owner);ColonyTurn(owner);if(owner!=0)ColonyAI(owner);
  if(troopLoads!=null)foreach(var key in troopLoads.Keys.Where(k=>k%200==owner).ToList())troopLoads.Remove(key);
  // One message a turn for everything about gates and colonies that changed.
  if(owner==0&&dimensionNews!=null&&dimensionNews.Count>0){Note("Gates and colonies: "+string.Join(" ",dimensionNews.Distinct().Take(4).ToArray())+(dimensionNews.Count>4?" (and "+(dimensionNews.Count-4)+" more; see Gates and colonies)":""));dimensionNews.Clear();}
 }
 // Problems the player can act on now, for the overview. Nothing here repeats as a message.
 public List<string> DimensionProblems(int owner)
 {
  var list=new List<string>();
  foreach(var c in ColoniesOf(owner))
  {
   int need=SupplyNeed(c);var gate=c.gate<0?null:GatePairTile(c.gate);
   if(c.isolated>0)list.Add(c.name+": cut off for "+c.isolated+" turn"+(c.isolated==1?"":"s")+" ("+(gate==null?"no gateway":GateBlock(owner,gate))+")"+(need<=0?"; it feeds itself.":"; supplies for "+(c.stock/Math.Max(1,need))+" more turns."));
   else if(need>0&&c.stock<need*2)list.Add(c.name+": supplies low ("+c.stock+", uses "+need+" a turn). Raise Supply priority or build a Storehouse.");
   if(c.siege>0)list.Add(c.name+": under attack with no garrison.");
   if(ColonyHome(c)==null)list.Add(c.name+": no home city to send its exports to.");
  }
  foreach(var g in HomeGates()){if(!GateKnownTo(owner,g))continue;var st=FindState(g);var study=StudyOf(owner,g);
   if(st!=null&&st.condition==GateCondition.Damaged&&study!=null&&study.known)list.Add("The "+GateStyleNames[GateStyle(g)].ToLower()+" is damaged: a city within "+DimensionSettings.InvestigateRange+" tiles can repair it.");}
  return list;
 }
 // What the three priorities would do at one gateway this turn, in loads, so the trade-off can be seen before choosing.
 public string PriorityPreview(int owner,Tile gate,int priority)
 {
  int cap=GateCapacityFor(owner,gate),pair=GatePair(gate),supply=0,export=0;
  foreach(var c in ColoniesOf(owner)){if(c.gate!=pair)continue;int need=SupplyNeed(c);supply+=Math.Max(0,Math.Min(2,(need*4-c.stock+2)/3));int ore,goods;ColonyOutput(c,out ore,out goods);export+=ore+goods;}
  int cargo=priority==ColonyCatalog.PriorityMilitary?cap-cap/2:cap,s,e;
  if(priority==ColonyCatalog.PriorityTrade){e=Math.Min(export,cargo);s=Math.Min(supply,cargo-e);}else{s=Math.Min(supply,cargo);e=Math.Min(export,cargo-s);}
  int troops=priority==ColonyCatalog.PriorityMilitary?cap-s-e:Math.Max(2,cap-s-e);
  return s+" of "+supply+" supply loads, "+e+" of "+export+" export loads, room for "+troops+" units ("+cap+" loads in all).";
 }
}
}
