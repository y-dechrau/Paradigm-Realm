using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// Parallel worlds and ancient gateways (owner request, 31 Oct 2026). Five worlds lie beyond the home world: the Fae Wilds,
// the Ashen Waste, the Infernal Depths, the Plague Marshes and the Drowned Deep. Each is a band of extra map rows below the
// home world, walled off by two rows of impassable mountains, so movement, fog, cities and combat work there exactly as at
// home, and the map shows one world at a time (switched on the Realms screen). An ancient gateway on the home world and its
// twin in the other world are linked: a land unit that steps onto a gateway comes out at its twin. Each world has its own
// danger (attrition, plague) and its own riches (relic sites with richer rewards, rare resources).
public partial class World {
 // Six worlds since 9 Nov 2026 (the Hive Expanse); a game made before then has five, and RealmsHere says how many this one has.
 public const int RealmCount=6,RealmGap=2;
 public int RealmsHere{get{return realmRows<=0?0:Math.Min(RealmCount,realmRows/(RealmHeight+RealmGap));}}
 // The gates are locked (owner request, 5 Nov 2026): the arches stand on both sides, but nothing crosses. Saved, so a later
 // key or event can open them; the crossing code below stays for that day.
 public bool gatesLocked=true;
 // Each parallel world is as tall as the home world (owner request); saved, so older games keep their own size.
 public int realmHeight;
 public int RealmHeight{get{return realmHeight>0?realmHeight:20;}}
 public static readonly string[] RealmNames={"Home world","Fae Wilds","Ashen Waste","Infernal Depths","Plague Marshes","Drowned Deep","Hive Expanse"};
 public static readonly string[] RealmTexts={
  "The world your people were born in.",
  "An enchanted land of glowing groves and silver lakes. Wounds heal faster here, and the fae leave gifts at their stone circles.",
  "A dead world of ash and bone where a great civilisation burned. Nothing grows; units lose 1 health a turn outside cities, but its ruins hold great treasures.",
  "A burning underworld of lava and black glass. Units lose 2 health a turn outside cities; its rocks are rich in iron and gold, and its relics in gold.",
  "Fetid bogs and dead forests. Units lose 1 health a turn and carry plague home for 5 turns: a city they enter loses 2 food a turn while the plague lasts.",
  "An endless ocean dotted with small islands and coral. Arrivals land on an island; found a city and build boats to explore its sunken treasures.",
  "A warm golden world of towering grass, great earth mounds and giant flowers, home to five insect peoples: ants, bees, beetles, mantises and moths. Rich in food and spices."};
 // Extra rows after the home world (mapHeight) that hold the parallel worlds; 0 for saves made before they existed.
 public int realmRows;
 public int MainHeight{get{return mapHeight;}}
 public int RealmTop(int r){return r<=0?0:mapHeight+(r-1)*(RealmHeight+RealmGap)+RealmGap;}
 public int RealmBottom(int r){return r<=0?mapHeight:RealmTop(r)+RealmHeight;}
 public bool HasRealms{get{return realmRows>0;}}
 public int RealmOf(int x,int y){var t=At(x,y);return t==null?0:t.realm;}
 public static int RealmBonus(int r){return r==2||r==3?3:r>0?2:1;}
 public bool RealmDiscovered(int owner,int r){if(r==0)return true;if(!HasRealms||r>RealmsHere)return false;for(int y=RealmTop(r);y<RealmBottom(r);y++)for(int x=0;x<Width;x++){var t=tiles[y*Width+x];if(owner==0?t.seen:nations[owner].known[y*Width+x])return true;}return false;}
 public Tile GateTwin(Tile t){return t==null||t.gateLink<=0||t.gateLink>tiles.Length?null:tiles[t.gateLink-1];}
 int GateRealm(Tile t){var twin=GateTwin(t);return twin==null?0:t.realm==0?twin.realm:t.realm;}
 // Gate designs (owner pictures, 8 Nov 2026): nine looks. A gateway in a parallel world wears its world's design (Fae Wilds forest,
 // Ashen Waste desert, Infernal Depths obsidian, Plague Marshes rift, Drowned Deep ice); one on the home world wears one of the four
 // home designs (arcane castle, steam works, sky scaffold, void spires) chosen by the world it leads to. Every gateway starts sealed
 // and opens on its own (OpenGate), showing its open picture once it does.
 public static readonly string[] GateStyleNames={"Frost gate","Grove gate","Obsidian gate","Void gate","Sun gate","Arcane gate","Sky gate","Steam gate","Rift gate"};
 public static readonly int[] RealmGateStyle={5,1,4,2,8,0,1},HomeGateStyle={5,5,7,6,3,5,3};
 public int GateStyle(Tile t){if(t==null)return 5;if(t.realm>0)return RealmGateStyle[Math.Max(0,Math.Min(RealmGateStyle.Length-1,t.realm))];var twin=GateTwin(t);int r=twin!=null?twin.realm:1;return HomeGateStyle[Math.Max(0,Math.Min(HomeGateStyle.Length-1,r))];}
 int GatePair(Tile t){var twin=GateTwin(t);int a=t.y*Width+t.x;return twin==null?a:Math.Min(a,twin.y*Width+twin.x);}
 // Save version 37: gateway pairs opened one by one while the rest stay sealed.
 public List<int> openedGates=new List<int>();
 public bool GateOpen(Tile t){if(t==null||t.gateLink<=0)return false;if(!gatesLocked)return true;return openedGates!=null&&openedGates.Contains(GatePair(t));}
 public bool OpenGate(Tile t,string how=""){if(t==null||t.gateLink<=0||GateOpen(t))return false;if(openedGates==null)openedGates=new List<int>();openedGates.Add(GatePair(t));var twin=GateTwin(t);int r=t.realm>0?t.realm:twin!=null?twin.realm:0;Note((how!=""?how+" ":"")+"An ancient gateway has opened: the "+GateStyleNames[GateStyle(t)].ToLower()+" to the "+RealmNames[r]+".");if(GateCells().Count>0&&openedGates.Count>=GateCells().Count)gatesLocked=false;return true;}
 // How gateways open (owner request, 8 Nov 2026: "by both"):
 //  - Knowledge: the first home civilisation to learn a world's key technology breaks the seals of every gateway to that world.
 //  - A wonder: the Gatekeeper's Key (Mysticism), built in a city within 10 tiles of a sealed gateway, opens the nearest one.
 //  - A quest: a Priest standing on a sealed gateway performs the rite of unsealing for 100 faith (Mysticism); the priest is spent.
 public static readonly string[] RealmKeyTech={"","philosophy","industrialisation","fission","medicine","printing","chemistry"};
 public const int GateKeyWonder=64,GateKeyRange=10,RiteFaith=100;
 public string RealmKeyName(int r){if(r<=0||r>=RealmKeyTech.Length)return "";int t=TechnologyCatalog.Index(RealmKeyTech[r]);return t>=0?TechNames[t]:"";}
 internal void OpenGatesByKnowledge(){
  if(!gatesLocked)return;
  for(int r=1;r<RealmKeyTech.Length;r++){
   int learner=-1;for(int o=0;o<civCount&&learner<0;o++)if(!IsNativeNation(o)&&HasTech(o,RealmKeyTech[r]))learner=o;if(learner<0)continue;
   foreach(int i in GateCells().ToList()){var t=tiles[i];if(t.realm!=0||GateOpen(t))continue;var twin=GateTwin(t);if(twin==null||twin.realm!=r)continue;OpenGate(t,(learner==0?"Your":NationName(learner)+"'s")+" scholars of "+RealmKeyName(r)+" have broken a seal.");}
  }
 }
 public Tile NearestSealedGate(int x,int y,int range){Tile best=null;int least=int.MaxValue;foreach(int i in GateCells()){var t=tiles[i];if(t.realm!=0||GateOpen(t))continue;int d=Distance(x,y,t.x,t.y);if(d<=range&&d<least){least=d;best=t;}}return best;}
 public string RiteReason(Unit u){if(u==null||u.type!=PriestUnit)return "Only a priest can perform the rite.";var t=At(u.x,u.y);if(t==null||t.gateLink<=0)return "Stand on a gateway.";if(GateOpen(t))return "This gateway is already open.";if(!HasTech(u.owner,"mysticism"))return "Requires Mysticism.";if(u.moves<=0)return "The priest needs movement.";if(nations[u.owner].faith<RiteFaith)return "Needs "+RiteFaith+" faith.";return "";}
 public bool PerformRite(Unit u){if(RiteReason(u)!="")return false;nations[u.owner].faith-=RiteFaith;var t=At(u.x,u.y);units.Remove(u);OpenGate(t,(u.owner==0?"Your":NationName(u.owner)+"'s")+" priest gave their life to the rite of unsealing.");return true;}
 public string GateName(Tile t){int r=GateRealm(t);if(!GateOpen(t))return "Sealed ancient gateway";return t.realm==0?"Ancient gateway to the "+RealmNames[r]:"Ancient gateway home";}
 // Called once a new world has been generated: appends the five worlds and links their gateways to the home world.
 void AddRealms(){
  if(HasRealms||mapShape==1)return;realmHeight=mapHeight;int W=Width,oldH=mapHeight,extra=RealmCount*(RealmHeight+RealmGap);var grown=new Tile[W*(oldH+extra)];
  Array.Copy(tiles,grown,tiles.Length);for(int y=oldH;y<oldH+extra;y++)for(int x=0;x<W;x++)grown[y*W+x]=new Tile{x=x,y=y,terrain=5,realm=-1};
  tiles=grown;realmRows=extra;
  foreach(var n in nations)if(n.known!=null&&n.known.Length<tiles.Length)n.known=Grow(n.known,tiles.Length);
  var rnd=new Random(seed^0x6a7e);
  for(int r=1;r<=RealmCount;r++){
   for(int y=RealmTop(r);y<RealmBottom(r);y++)for(int x=0;x<W;x++){var t=tiles[y*W+x];t.realm=r;t.terrain=RealmTerrain(r,x,y-RealmTop(r));t.resource=0;}
   foreach(var t in RealmTiles(r).Where(t=>!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&!Barren(t.terrain)))
   {double roll=rnd.NextDouble();if(roll<.035)t.ruin=true;else if(roll<.08)t.resource=r==3?(rnd.Next(2)==0?2:3):r==1||r==6?(rnd.Next(2)==0?5:1):r==2?3:r==4?5:1;}
   if(r==5)foreach(var t in RealmTiles(r).Where(t=>t.terrain==4&&rnd.NextDouble()<.03))t.resource=6+rnd.Next(10);
  }
  PlaceGateways(rnd);
 }
 IEnumerable<Tile> RealmTiles(int r){if(r>RealmsHere)yield break;for(int y=RealmTop(r);y<RealmBottom(r);y++)for(int x=0;x<Width;x++)yield return tiles[y*Width+x];}
 // Terrain of each world from smooth noise; the top and bottom rows of each world fade to its edge terrain.
 int RealmTerrain(int r,int x,int y){
  double n=Fbm(seed+r*977,x/7.0,y/5.0,.5,3),m=Fbm(seed+r*577,x/4.0,y/3.0,.5,2);bool edge=y==0||y==RealmHeight-1;
  switch(r){
   case 1:if(edge)return 1;return n<.3?6:n<.42?1:n<.6?0:n<.72?1:m>.6?2:0;
   case 2:if(edge)return 18;return n<.25?18:n<.55?10:n<.7?3:n<.82?20:m>.55?5:10;
   case 3:if(edge)return 5;return n<.22?5:n<.62?17:m>.5?10:17;
   case 4:if(edge)return 19;return n<.3?6:n<.6?9:n<.75?10:m>.5?19:9;
   case 6:if(edge)return 20;return n<.2?11:n<.45?16:n<.62?0:n<.74?20:m>.62?2:16;// Hive Expanse: tall grass, savanna, jungle groves, mounds (hills)
   default:if(edge)return 4;return n<.5?4:n<.55?12:m>.55?11:n<.68?19:0;// islands large enough for its peoples to grow as fast as the home world
  }
 }
 void PlaceGateways(Random rnd){
  var starts=units.Where(u=>u.type==0).ToList();var placed=new List<Tile>();
  for(int r=1;r<=RealmCount;r++){
   Tile home=tiles.Where(t=>t.realm==0&&t.y>=3&&t.y<mapHeight-3&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&!Barren(t.terrain)&&!t.village&&!t.ruin&&t.resource==0
     &&starts.All(u=>Distance(u.x,u.y,t.x,t.y)>=8)&&placed.All(p=>Distance(p.x,p.y,t.x,t.y)>=10)).OrderBy(t=>Lattice(seed+r*31,t.x,t.y,7)).FirstOrDefault();
   var land=RealmTiles(r).Where(t=>!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&!Barren(t.terrain)&&t.y>RealmTop(r)&&t.y<RealmBottom(r)-1).ToList();
   if(land.Count==0){int cx=Width/2,cy=RealmTop(r)+RealmHeight/2;for(int dy=-1;dy<=1;dy++)for(int dx=-2;dx<=2;dx++){var t=At(cx+dx,cy+dy);if(t!=null){t.terrain=dy==0&&dx==0?0:11;land.Add(t);}}}
   Tile far=land.OrderBy(t=>Lattice(seed+r*53,t.x,t.y,7)).First();
   if(home==null)continue;home.gateLink=far.y*Width+far.x+1;far.gateLink=home.y*Width+home.x+1;far.ruin=false;placed.Add(home);
  }
 }
 // Native peoples (owner request, 31 Oct 2026): up to ten civilisations that live in the parallel worlds, two to a world,
 // added after the home civilisations while the total stays within 16. Each starts with a city, a warrior and a worker
 // on open land in its own world, away from the gateway and from the other people there.
 public void AddNatives(int perWorld=2){
  // Up to perWorld peoples from each world this game has (the Hive Expanse has five), in the catalogue's placing order.
  if(!HasRealms)return;int home=civCount;var chosen=new List<int>();var taken=new int[RealmCount+1];
  foreach(int c in CivilisationCatalog.NativeOrder){int rr=CivilisationCatalog.NativeRealm(c);if(rr<1||rr>RealmsHere||taken[rr]>=Math.Max(0,Math.Min(10,perWorld)))continue;taken[rr]++;chosen.Add(c);}
  int k=Math.Min(chosen.Count,MaxNations-home);if(k<=0)return;
  SetCivCount(home+k);var placed=new List<Tile>();
  for(int i=0;i<k;i++){
   int o=home+i,civ=chosen[i],r=CivilisationCatalog.NativeRealm(civ);var n=nations[o];
   n.civ=civ;n.name=CivilisationCatalog.Names[civ];n.leader=CivilisationCatalog.Leaders[civ];n.portrait=CivilisationCatalog.Portraits[civ];n.traits=(int[])CivilisationCatalog.Traits[civ].Clone();n.known=new bool[tiles.Length];
   var gate=RealmTiles(r).FirstOrDefault(t=>t.gateLink>0);
   Func<Tile,int> room=t=>{int land=0;for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++){var q=StepIn(t.x,t.y,dx,dy,2);if(q!=null&&q.realm==r&&!IsWater(q.terrain)&&!IsImpassable(q.terrain))land++;}return land;};
   // Peoples keep well apart, closing up only as far as a crowded world needs.
   Tile start=null;for(int apart=Math.Max(8,Width/3);apart>=4&&start==null;apart--)start=RealmTiles(r).Where(t=>!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&!Barren(t.terrain)&&t.gateLink==0&&t.y>=RealmTop(r)+3&&t.y<RealmBottom(r)-3
     &&(gate==null||Distance(t.x,t.y,gate.x,gate.y)>=Math.Min(6,apart))&&placed.All(p=>Distance(p.x,p.y,t.x,t.y)>=apart)).OrderByDescending(room).ThenBy(t=>Lattice(seed+civ,t.x,t.y,5)).FirstOrDefault();
   if(start==null)continue;placed.Add(start);start.ruin=false;start.village=false;
   // A little open ground round the capital, as the home starts get.
   for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var q=Step(start.x,start.y,dx,dy);if(q!=null&&q.realm==r&&(IsWater(q.terrain)||IsImpassable(q.terrain)||Barren(q.terrain))&&q.gateLink==0)q.terrain=r==5?11:r==3?17:r==2?10:r==4?9:r==6?16:0;}
   var settler=Add(o,0,start.x,start.y);var east=Step(start.x,start.y,1,0);var south=Step(start.x,start.y,0,1);Add(o,2,east.x,east.y);Add(o,1,south.x,south.y);
   Found(settler);
  }
  EnsureFlags();
 }
 // Behind different locked gates: no sight or contact can pass between them, so nothing need check.
 public bool SealedApart(int a,int b){return gatesLocked&&CivilisationCatalog.NativeRealm(nations[a].civ)!=CivilisationCatalog.NativeRealm(nations[b].civ);}
 public bool IsNativeNation(int owner){return owner>=0&&owner<civCount&&CivilisationCatalog.IsNative(nations[owner].civ);}
 // A land unit that enters a gateway it knows of is carried to the twin gateway (or the nearest free tile beside it).
 void GatewayTravel(Unit u){
  var here=At(u.x,u.y);var twin=GateTwin(here);if(twin==null||IsNaval(u.type))return;
  if(!GateOpen(here)){if(u.owner==0&&!u.exploring)Note("The ancient gateway is sealed. Your "+UnitName(u)+" cannot open it.");return;}
  Tile landing=GateLanding(u,twin);
  if(landing==null){if(u.owner==0)Note("The gateway flickers but will not open: the far side is blocked.");return;}
  int from=here.realm;CancelOrder(u);u.exploring=false;u.x=landing.x;u.y=landing.y;u.moves=0;
  if(u.owner==0){Note("Your "+UnitName(u)+" stepped through the ancient gateway into the "+RealmNames[landing.realm]+(landing.realm>0?". "+RealmTexts[landing.realm]:"."));lastGateArrival=landing.realm;}
  else if(KnowsNation(u.owner)&&(landing.realm>0&&RealmDiscovered(0,landing.realm)))Note("A "+UnitName(u)+" of "+NationName(u.owner)+" came through a gateway into the "+RealmNames[landing.realm]+".");
  networks=null;Reveal();
 }
 // The free land at a gateway or beside it where a unit can come out; null when it is all blocked.
 Tile GateLanding(Unit u,Tile twin){
  if(Capacity(u,twin.x,twin.y)&&!units.Any(a=>a.owner!=u.owner&&a.x==twin.x&&a.y==twin.y))return twin;
  for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=Step(twin.x,twin.y,dx,dy);if(n!=null&&n.realm==twin.realm&&!IsWater(n.terrain)&&!IsImpassable(n.terrain)&&Capacity(u,n.x,n.y)&&!units.Any(a=>a.owner!=u.owner&&a.x==n.x&&a.y==n.y))return n;}
  return null;
 }
 // Save version 24: the gates closed with travellers on the wrong side. Land units with no city of their own in that world
 // go back through its gateway; a colony already founded over there stays, with its garrison, cut off.
 public void RecallTravellers(){
  foreach(var u in units.Where(a=>!IsNaval(a.type)).ToList()){
   var t=At(u.x,u.y);if(t==null||t.realm<0)continue;int home=u.owner<nations.Length?CivilisationCatalog.NativeRealm(nations[u.owner].civ):0;
   if(t.realm==home||cities.Any(c=>c.owner==u.owner&&RealmOf(c.x,c.y)==t.realm))continue;
   var gate=tiles.FirstOrDefault(g=>g.gateLink>0&&g.realm==t.realm&&(t.realm>0||GateTwin(g).realm==home));var twin=GateTwin(gate);var landing=twin==null?null:GateLanding(u,twin);if(landing==null)continue;
   CancelOrder(u);u.exploring=false;u.x=landing.x;u.y=landing.y;if(u.owner==0)Note("The ancient gateways have sealed. Your "+UnitName(u)+" came back through just in time.");
  }
  networks=null;
 }
 // Each people of a parallel world lives off its own land as well as a home people does (owner request, 5 Nov 2026): what
 // starves an outsider feeds them, so ash, lava and bog feed like grassland (3 food), other land gives at least 2, and all of it at least 1 trade, on land that is theirs or free.
 void NativeGround(Tile t,int[] yields){
  if(t.realm<2||t.realm>4||IsWater(t.terrain)||IsImpassable(t.terrain)||(t.owner>=0&&!IsNativeNation(t.owner)))return;
  yields[0]=Math.Max(yields[0],t.terrain==9||t.terrain==10||t.terrain==17?3:2);yields[2]=Math.Max(yields[2],1);if(t.terrain==17)yields[1]=2;
 }
 // Pace (owner request, 5 Nov 2026): the sealed worlds keep step with the home world. Each turn a people behind the average
 // of the home civilisations gains a share of its current research (one eighth per technology behind), and one well ahead
 // waits for the next technology, so it tracks the home world whatever its land is like. Home civilisations are never affected.
 public const int PaceShare=8,PaceLead=2;
 public int HomeTechPace(){int sum=0,count=0;for(int o=0;o<civCount;o++){if(IsNativeNation(o)||!Survives(o))continue;sum+=nations[o].tech.Count(a=>a);count++;}return count==0?0:sum/count;}
 internal void KeepPace(int owner){
  var n=nations[owner];if(!gatesLocked||!IsNativeNation(owner)||n.research<0||!AvailableTech(owner).Contains(n.research))return;
  int behind=HomeTechPace()-n.tech.Count(a=>a);
  if(behind>0)n.progress[n.research]+=Math.Max(1,ResearchCost(n.research,owner)*Math.Min(behind,PaceShare)/PaceShare);
  else if(behind<-PaceLead)n.progress[n.research]=Math.Min(n.progress[n.research],ResearchCost(n.research,owner)-1);
 }
 // The realm the player's unit just arrived in (read and cleared by the interface to switch the view). Not saved.
 [NonSerialized] public int lastGateArrival=-1;
 // Each turn: the dangers of the other worlds, and plague carried home.
 void RealmTurn(int owner){
  foreach(var u in units.Where(a=>a.owner==owner).ToList()){
   var t=At(u.x,u.y);if(t==null)continue;bool inCity=cities.Any(c=>c.x==u.x&&c.y==u.y);
   // A native people is at home in its own world and suffers none of its dangers.
   if(t.realm>0&&CivilisationCatalog.NativeRealm(nations[owner].civ)==t.realm){if(u.plague>0)u.plague--;continue;}
   if(t.realm==1)u.hp=Math.Min(10,u.hp+2);
   else if((t.realm==2||t.realm==4)&&!inCity)u.hp=Math.Max(1,u.hp-1);
   else if(t.realm==3&&!inCity)u.hp=Math.Max(1,u.hp-2);
   if(t.realm==4)u.plague=5;
   else if(u.plague>0){u.plague--;var c=cities.Find(a=>a.x==u.x&&a.y==u.y&&a.owner==u.owner);if(c!=null&&c.plague<5){c.plague=5;if(owner==0)Note("Plague! Your "+UnitName(u)+" brought sickness from the Plague Marshes into "+c.name+": -2 food a turn for 5 turns.");}}
  }
  foreach(var c in cities.Where(a=>a.owner==owner))if(c.plague>0)c.plague--;
 }
 bool RealmsValid(){if(realmRows<0||(realmRows>0&&(realmRows%(RealmHeight+RealmGap)!=0||RealmsHere<5)))return false;return tiles.All(t=>t.realm>=-1&&t.realm<=RealmCount&&t.gateLink>=0&&t.gateLink<=tiles.Length)&&units.All(u=>u.plague>=0&&u.plague<=5)&&cities.All(c=>c.plague>=0&&c.plague<=5);}
}
}
