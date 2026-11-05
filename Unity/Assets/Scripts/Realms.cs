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
 public const int RealmCount=5,RealmGap=2;
 // The gates are locked (owner request, 5 Nov 2026): the arches stand on both sides, but nothing crosses. Saved, so a later
 // key or event can open them; the crossing code below stays for that day.
 public bool gatesLocked=true;
 // Each parallel world is as tall as the home world (owner request); saved, so older games keep their own size.
 public int realmHeight;
 public int RealmHeight{get{return realmHeight>0?realmHeight:20;}}
 public static readonly string[] RealmNames={"Home world","Fae Wilds","Ashen Waste","Infernal Depths","Plague Marshes","Drowned Deep"};
 public static readonly string[] RealmTexts={
  "The world your people were born in.",
  "An enchanted land of glowing groves and silver lakes. Wounds heal faster here, and the fae leave gifts at their stone circles.",
  "A dead world of ash and bone where a great civilisation burned. Nothing grows; units lose 1 health a turn outside cities, but its ruins hold great treasures.",
  "A burning underworld of lava and black glass. Units lose 2 health a turn outside cities; its rocks are rich in iron and gold, and its relics in gold.",
  "Fetid bogs and dead forests. Units lose 1 health a turn and carry plague home for 5 turns: a city they enter loses 2 food a turn while the plague lasts.",
  "An endless ocean dotted with small islands and coral. Arrivals land on an island; found a city and build boats to explore its sunken treasures."};
 // Extra rows after the home world (mapHeight) that hold the parallel worlds; 0 for saves made before they existed.
 public int realmRows;
 public int MainHeight{get{return mapHeight;}}
 public int RealmTop(int r){return r<=0?0:mapHeight+(r-1)*(RealmHeight+RealmGap)+RealmGap;}
 public int RealmBottom(int r){return r<=0?mapHeight:RealmTop(r)+RealmHeight;}
 public bool HasRealms{get{return realmRows>0;}}
 public int RealmOf(int x,int y){var t=At(x,y);return t==null?0:t.realm;}
 public static int RealmBonus(int r){return r==2||r==3?3:r>0?2:1;}
 public bool RealmDiscovered(int owner,int r){if(r==0)return true;if(!HasRealms)return false;for(int y=RealmTop(r);y<RealmBottom(r);y++)for(int x=0;x<Width;x++){var t=tiles[y*Width+x];if(owner==0?t.seen:nations[owner].known[y*Width+x])return true;}return false;}
 public Tile GateTwin(Tile t){return t==null||t.gateLink<=0||t.gateLink>tiles.Length?null:tiles[t.gateLink-1];}
 int GateRealm(Tile t){var twin=GateTwin(t);return twin==null?0:t.realm==0?twin.realm:t.realm;}
 public string GateName(Tile t){int r=GateRealm(t);if(gatesLocked)return "Sealed ancient gateway";return t.realm==0?"Ancient gateway to the "+RealmNames[r]:"Ancient gateway home";}
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
   {double roll=rnd.NextDouble();if(roll<.035)t.ruin=true;else if(roll<.08)t.resource=r==3?(rnd.Next(2)==0?2:3):r==1?(rnd.Next(2)==0?5:1):r==2?3:r==4?5:1;}
   if(r==5)foreach(var t in RealmTiles(r).Where(t=>t.terrain==4&&rnd.NextDouble()<.03))t.resource=6+rnd.Next(10);
  }
  PlaceGateways(rnd);
 }
 IEnumerable<Tile> RealmTiles(int r){for(int y=RealmTop(r);y<RealmBottom(r);y++)for(int x=0;x<Width;x++)yield return tiles[y*Width+x];}
 // Terrain of each world from smooth noise; the top and bottom rows of each world fade to its edge terrain.
 int RealmTerrain(int r,int x,int y){
  double n=Fbm(seed+r*977,x/7.0,y/5.0,.5,3),m=Fbm(seed+r*577,x/4.0,y/3.0,.5,2);bool edge=y==0||y==RealmHeight-1;
  switch(r){
   case 1:if(edge)return 1;return n<.3?6:n<.42?1:n<.6?0:n<.72?1:m>.6?2:0;
   case 2:if(edge)return 18;return n<.25?18:n<.55?10:n<.7?3:n<.82?20:m>.55?5:10;
   case 3:if(edge)return 5;return n<.22?5:n<.62?17:m>.5?10:17;
   case 4:if(edge)return 19;return n<.3?6:n<.6?9:n<.75?10:m>.5?19:9;
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
 public void AddNatives(){
  if(!HasRealms)return;int home=civCount,k=Math.Min(CivilisationCatalog.NativeOrder.Length,16-home);if(k<=0)return;
  SetCivCount(home+k);var placed=new List<Tile>();
  for(int i=0;i<k;i++){
   int o=home+i,civ=CivilisationCatalog.NativeOrder[i],r=CivilisationCatalog.NativeRealm(civ);var n=nations[o];
   n.civ=civ;n.name=CivilisationCatalog.Names[civ];n.leader=CivilisationCatalog.Leaders[civ];n.portrait=CivilisationCatalog.Portraits[civ];n.traits=(int[])CivilisationCatalog.Traits[civ].Clone();n.known=new bool[tiles.Length];
   var gate=RealmTiles(r).FirstOrDefault(t=>t.gateLink>0);
   Func<Tile,int> room=t=>{int land=0;for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++){var q=StepIn(t.x,t.y,dx,dy,2);if(q!=null&&q.realm==r&&!IsWater(q.terrain)&&!IsImpassable(q.terrain))land++;}return land;};
   var start=RealmTiles(r).Where(t=>!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&!Barren(t.terrain)&&t.gateLink==0&&t.y>=RealmTop(r)+3&&t.y<RealmBottom(r)-3
     &&(gate==null||Distance(t.x,t.y,gate.x,gate.y)>=6)&&placed.All(p=>Distance(p.x,p.y,t.x,t.y)>=Math.Max(8,Width/3))).OrderByDescending(room).ThenBy(t=>Lattice(seed+civ,t.x,t.y,5)).FirstOrDefault();
   if(start==null)continue;placed.Add(start);start.ruin=false;start.village=false;
   // A little open ground round the capital, as the home starts get.
   for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var q=Step(start.x,start.y,dx,dy);if(q!=null&&q.realm==r&&(IsWater(q.terrain)||IsImpassable(q.terrain)||Barren(q.terrain))&&q.gateLink==0)q.terrain=r==5?11:r==3?17:r==2?10:r==4?9:0;}
   var settler=Add(o,0,start.x,start.y);var east=Step(start.x,start.y,1,0);var south=Step(start.x,start.y,0,1);Add(o,2,east.x,east.y);Add(o,1,south.x,south.y);
   Found(settler);
  }
  EnsureFlags();
 }
 public bool IsNativeNation(int owner){return owner>=0&&owner<civCount&&CivilisationCatalog.IsNative(nations[owner].civ);}
 // A land unit that enters a gateway it knows of is carried to the twin gateway (or the nearest free tile beside it).
 void GatewayTravel(Unit u){
  var here=At(u.x,u.y);var twin=GateTwin(here);if(twin==null||IsNaval(u.type))return;
  if(gatesLocked){if(u.owner==0&&!u.exploring)Note("The ancient gateway is sealed. Your "+UnitName(u)+" cannot open it.");return;}
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
 bool RealmsValid(){if(realmRows<0||(realmRows>0&&realmRows!=RealmCount*(RealmHeight+RealmGap)))return false;return tiles.All(t=>t.realm>=-1&&t.realm<=RealmCount&&t.gateLink>=0&&t.gateLink<=tiles.Length)&&units.All(u=>u.plague>=0&&u.plague<=5)&&cities.All(c=>c.plague>=0&&c.plague<=5);}
}
}
