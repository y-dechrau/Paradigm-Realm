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
 public const int RealmCount=5,RealmHeight=20,RealmGap=2;
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
 public string GateName(Tile t){int r=GateRealm(t);return t.realm==0?"Ancient gateway to the "+RealmNames[r]:"Ancient gateway home";}
 // Called once a new world has been generated: appends the five worlds and links their gateways to the home world.
 void AddRealms(){
  if(HasRealms||mapShape==1)return;int W=Width,oldH=mapHeight,extra=RealmCount*(RealmHeight+RealmGap);var grown=new Tile[W*(oldH+extra)];
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
   default:if(edge)return 4;return n<.6?4:n<.66?12:m>.55?11:n<.75?19:0;
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
 // A land unit that enters a gateway it knows of is carried to the twin gateway (or the nearest free tile beside it).
 void GatewayTravel(Unit u){
  var here=At(u.x,u.y);var twin=GateTwin(here);if(twin==null||IsNaval(u.type))return;
  Tile landing=Capacity(u,twin.x,twin.y)&&!units.Any(a=>a.owner!=u.owner&&a.x==twin.x&&a.y==twin.y)?twin:null;
  if(landing==null)for(int dy=-1;dy<=1&&landing==null;dy++)for(int dx=-1;dx<=1&&landing==null;dx++){var n=Step(twin.x,twin.y,dx,dy);if(n!=null&&n.realm==twin.realm&&!IsWater(n.terrain)&&!IsImpassable(n.terrain)&&Capacity(u,n.x,n.y)&&!units.Any(a=>a.owner!=u.owner&&a.x==n.x&&a.y==n.y))landing=n;}
  if(landing==null){if(u.owner==0)Note("The gateway flickers but will not open: the far side is blocked.");return;}
  int from=here.realm;CancelOrder(u);u.exploring=false;u.x=landing.x;u.y=landing.y;u.moves=0;
  if(u.owner==0){Note("Your "+UnitName(u)+" stepped through the ancient gateway into the "+RealmNames[landing.realm]+(landing.realm>0?". "+RealmTexts[landing.realm]:"."));lastGateArrival=landing.realm;}
  else if(KnowsNation(u.owner)&&(landing.realm>0&&RealmDiscovered(0,landing.realm)))Note("A "+UnitName(u)+" of "+NationName(u.owner)+" came through a gateway into the "+RealmNames[landing.realm]+".");
  networks=null;Reveal();
 }
 // The realm the player's unit just arrived in (read and cleared by the interface to switch the view). Not saved.
 [NonSerialized] public int lastGateArrival=-1;
 // Each turn: the dangers of the other worlds, and plague carried home.
 void RealmTurn(int owner){
  foreach(var u in units.Where(a=>a.owner==owner).ToList()){
   var t=At(u.x,u.y);if(t==null)continue;bool inCity=cities.Any(c=>c.x==u.x&&c.y==u.y);
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
