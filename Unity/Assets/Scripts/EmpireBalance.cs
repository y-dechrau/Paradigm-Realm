using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// The empire's priorities (owner request, 6 Nov 2026, after Galactic Civilizations II): three linked sliders share each city's
// Trade between science, the army and the treasury. They always total 100; moving one moves the other two in proportion.
// The military share becomes extra shields for any combat unit a city is training. Rivals keep it at 0 for now.
public sealed class CivicStats {public long population,urban;public int urbanShare;public int cities,land,roads,literacy,approval,lifeExpectancy,gnp,goods,crops,science,soldiers,strength;}
public sealed class BorderGrowth {public int radius,done,span,turnsLeft=-1,citizensAt=-1;public bool full;}
// What a discovery asks of the player (owner request, 6 Nov 2026): what to research next, then each idle city that gained a building.
public sealed class BuildPrompt {public int city,tech;public List<int> options=new List<int>();}
public partial class World {
 public const int AxisScience=0,AxisMilitary=1,AxisFinance=2;
 public int FinanceRate(int owner){return 100-nations[owner].scienceRate-nations[owner].militaryRate;}
 static int RoundTen(int v){return (v+5)/10*10;}
 public bool SetPriority(int owner,int axis,int value){
  if(owner<0||owner>=civCount||axis<0||axis>2)return false;var n=nations[owner];int cap=ScienceCap(owner);
  value=Math.Max(0,Math.Min(axis==AxisScience?cap:100,RoundTen(value)));
  var rates=new[]{n.scienceRate,n.militaryRate,FinanceRate(owner)};int a=(axis+1)%3,b=(axis+2)%3,rest=100-value,total=rates[a]+rates[b];
  int first=total>0?RoundTen(rest*rates[a]/total):RoundTen(rest/2);first=Math.Max(0,Math.Min(rest,first));rates[axis]=value;rates[a]=first;rates[b]=rest-first;
  // Science has a ceiling (60%, or 80% with the Academies edict); what it cannot take goes to the treasury.
  if(rates[AxisScience]>cap){int spill=rates[AxisScience]-cap;rates[AxisScience]=cap;rates[axis==AxisFinance?AxisMilitary:AxisFinance]+=spill;}
  n.scienceRate=rates[AxisScience];n.militaryRate=rates[AxisMilitary];n.tax=TaxLevel(n.scienceRate+n.militaryRate);return true;
 }
 // Shields this city's military share adds per turn, whether or not it is training anything.
 public int ArmyBudget(City c){int rate=nations[c.owner].militaryRate;if(rate<=0||Disorder(c))return 0;int shields=Trade(c)*rate/100;return c.occupation>0?shields/2:shields;}
 public static bool IsCombatBuild(int project){int u=BuildUnit(project);return u>=0&&Power[u]>0;}
 public int ArmyShields(City c,int project){return IsCombatBuild(project)?ArmyBudget(c):0;}
 // Shields per turn on the line that holds this project.
 public int LineOutput(City c,int project){return Math.Max(1,Yield(c)[1])+ArmyShields(c,project);}
 // When the city's influence next spreads a ring: with age (one ring per 25 turns up to 4) or with size (ring 5 at size 30).
 public BorderGrowth NextBorderGrowth(City c){
  int r=TerritoryRadius(c);var g=new BorderGrowth{radius=r};
  if(r>=5){g.full=true;return g;}
  g.citizensAt=(r-1)*10;
  if(r<4){int from=(r-2)*25,to=(r-1)*25;g.span=to-from;g.done=Math.Max(0,Math.Min(g.span,c.age-from));g.turnsLeft=Math.Max(1,to-c.age);}
  else{g.span=g.citizensAt;g.done=Math.Min(g.span,c.pop);}
  return g;
 }
 // The path a unit's current order will take, for the route line; it goes through other units if they block the way today.
 public List<Tile> PlannedRoute(Unit u){
  if(u==null||!(u.hasOrder||u.roadOrder))return new List<Tile>();int x=u.roadOrder?u.roadX:u.destinationX,y=u.roadOrder?u.roadY:u.destinationY;
  var path=Route(u,x,y);return path.Count>0?path:Route(u,x,y,true);
 }
 // Why a unit cannot step onto a neighbouring tile, in words, or "" when it can (also used to explain a stuck order).
 public string MoveProblem(Unit u,int x,int y){
  if(u==null||!units.Contains(u))return "No such unit.";var t=At(x,y);if(t==null)return "Edge of the world.";if(Distance(u.x,u.y,t.x,t.y)!=1)return "Not next to it.";
  if(u.unfed)return "No food: it cannot act until there is more.";if(u.moves<=0)return "No movement left this turn.";
  if(!Passable(u,t,t.terrain))return "It cannot cross "+TerrainNames[t.terrain]+".";if(!CanEnter(u.owner,t))return "Closed borders.";
  if(!Capacity(u,t.x,t.y))return "That tile is full.";int cost=StepCost(u,u.x,u.y,t);if(u.moves<cost)return "Not enough movement left ("+u.moves+" of "+cost+").";
  if(u.stack!=0){var mates=StackOf(u).Where(a=>a!=u&&!a.unfed).ToList();foreach(var m in mates)if(m.moves<StepCost(m,u.x,u.y,t))return "Its stack waits for "+UnitName(m)+", which has no movement left.";}
  return "";
 }
 // ---------- an empty treasury has consequences (owner request, 7 Nov 2026) ----------
 // While the treasury cannot pay its way: every city has one citizen fewer content and unpaid soldiers fight one point weaker (from the
 // first turn); from the second turn the costliest soldier deserts each turn; from the third a building is sold off each turn.
 void DebtConsequences(int owner){
  var n=nations[owner];if(!modernRules||n.arrears<=0)return;
  if(n.arrears>=2){var deserter=units.Where(u=>u.owner==owner&&Power[u.type]>0).OrderBy(u=>AtHome(u)?1:0).ThenByDescending(u=>Upkeep[u.type]).ThenByDescending(u=>u.id).FirstOrDefault();if(deserter!=null){units.Remove(deserter);if(owner==0)Note("Unpaid, a "+UnitName(deserter)+" has deserted. The treasury is empty: cut costs or raise money.");}}
  if(n.arrears>=3){City from=null;int sold=-1;foreach(var c in cities.Where(a=>a.owner==owner)){foreach(int b in c.buildings){if(IsWonder(b)||IsProject(b)||BuildingUpkeep(b)<=0)continue;if(sold<0||BuildingUpkeep(b)>BuildingUpkeep(sold)){sold=b;from=c;}}}
   if(from!=null){from.buildings.Remove(sold);int price=Cost(sold)/4;n.gold+=price;if(owner==0){gold=n.gold;Note("Creditors seized the "+BuildName(sold)+" in "+from.name+" and sold it for "+price+" gold.");}}}
 }
 public string DebtState(int owner){var n=nations[owner];return n.arrears<=0?"":n.arrears==1?"Treasury empty: every city has one citizen fewer content and soldiers fight weaker.":n.arrears==2?"In debt: soldiers have begun to desert.":"Deep in debt: soldiers desert and buildings are being sold.";}
 // ---------- stacks: room for all, and one kind at a time ----------
 public List<Unit> FitMates(Unit u,List<Unit> mates,int x,int y){
  bool city=cities.Any(c=>c.owner==u.owner&&c.x==x&&c.y==y);int soldiers=Power[u.type]>0?1:0,civilians=Power[u.type]==0&&u.type!=1?1:0;
  foreach(var a in units)if(a.owner==u.owner&&a.x==x&&a.y==y&&!IsNaval(a.type)){if(Power[a.type]>0)soldiers++;else if(a.type!=1)civilians++;}
  var kept=new List<Unit>();foreach(var m in mates.OrderByDescending(a=>Power[a.type]>0).ThenBy(a=>a.id)){if(m.type==1||IsNaval(m.type)){kept.Add(m);continue;}if(Power[m.type]>0){if(soldiers<SoldierStack){soldiers++;kept.Add(m);}}else if(city||civilians<CivilianStack){civilians++;kept.Add(m);}}
  return kept;
 }
 // Stacks larger than open land can hold (made inside a city before this rule) are cut down to what can travel.
 void TrimStacks(){foreach(var g in units.Where(u=>u.stack!=0).GroupBy(u=>u.stack).ToList()){var list=g.ToList();var lead=list.OrderByDescending(a=>Power[a.type]>0).ThenBy(a=>a.id).First();var fit=FitMatesOpen(lead,list.Where(a=>a!=lead).ToList());foreach(var m in list)if(m!=lead&&!fit.Contains(m))m.stack=0;}}
 List<Unit> FitMatesOpen(Unit u,List<Unit> mates){int soldiers=Power[u.type]>0?1:0,civilians=Power[u.type]==0&&u.type!=1?1:0;var kept=new List<Unit>();foreach(var m in mates.OrderByDescending(a=>Power[a.type]>0).ThenBy(a=>a.id)){if(m.type==1){kept.Add(m);continue;}if(Power[m.type]>0){if(soldiers<SoldierStack){soldiers++;kept.Add(m);}}else if(civilians<CivilianStack){civilians++;kept.Add(m);}}return kept;}
 // All the units of one kind on a tile, taken out of any other stack and made a stack of their own (owner request).
 public bool JoinType(Unit u){if(u==null||IsNaval(u.type))return false;var same=units.Where(a=>a.owner==u.owner&&a.x==u.x&&a.y==u.y&&a.type==u.type).ToList();if(same.Count<2)return false;int id=nextId++;foreach(var a in same)a.stack=id;return true;}
 public int SameTypeHere(Unit u){return u==null?0:units.Count(a=>a.owner==u.owner&&a.x==u.x&&a.y==u.y&&a.type==u.type);}
 // ---------- owner requests, 7 Nov 2026 ----------
 // modernRules (save version 32): the shorter technology tree's bonuses, expansion costs and penalties for cities cut off from the rest.
 public bool modernRules;public int animalShare=100,climate;
 public static readonly string[] ClimateNames={"Varied","Temperate","Frozen","Tropical","Arid"};
 public static readonly string[] ClimateTexts={"From polar ice to equatorial jungle, like the Earth.","Mild forests and grassland from pole to pole, with few extremes.","An ice age: tundra and pine forest across most of the world.","Hot and wet: jungle and savanna, with little cold land.","Dry steppe and desert, with fertile land only near water."};
 public static readonly string[] AnimalChoices={"None","Scarce","Some","Common","Teeming"};
 public static readonly int[] AnimalShares={0,25,50,100,200};
 // Climate moves the temperature and moisture a sphere is generated with; see BuildClimateWorld.
 public void ClimateShift(ref double temp,ref double wet){switch(climate){case 1:temp=.5+(temp-.5)*.45;wet=.5+(wet-.5)*.8;break;case 2:temp-=.36;break;case 3:temp=temp*.55+.5;wet+=.18;break;case 4:temp+=.08;wet-=.38;break;}}
 void RemapFlatClimate(){int[][] swaps={null,new[]{3,0,8,0,13,1,11,1,19,1,16,0},new[]{0,8,1,13,3,8,16,8,11,13,19,13,20,8},new[]{0,16,1,11,8,16,13,11,20,16},new[]{0,20,1,16,11,16,19,3,16,3}};var map=swaps[climate];foreach(var t in tiles){if(t.realm!=0)continue;for(int k=0;k<map.Length;k+=2)if(t.terrain==map[k]){t.terrain=map[k+1];break;}}}
 void RetireMinorResearch(){foreach(var n in nations){if(n==null||n.research<0||TechnologyCatalog.IsActive(n.research))continue;int o=Array.IndexOf(nations,n);n.research=AvailableTech(o).DefaultIfEmpty(-1).First();}research=nations[0].research;science=research<0?0:nations[0].progress[research];}
 // Growing too fast costs money: nothing up to four cities, then more with each one. A city not joined to any other by road, river or
 // sea costs more again and has one citizen fewer content, so it pays to connect what you settle (owner request).
 public const int FreeCities=4,CutOffGold=2;
 public static int ExpansionCost(int cityCount){return cityCount>FreeCities?(cityCount-FreeCities)*(cityCount-FreeCities+1)/6:0;}
 public bool CutOff(City c){LinkTrade(c);int n;return linkCount.TryGetValue(c.id,out n)&&n==0;}
 // Ports and ships need a body of water of at least this many tiles beside the city or its port (owner request).
 public const int BigWater=50;
 [NonSerialized] Dictionary<int,int> seaSizes;
 internal void ResetWaters(){seaRegion=null;seaSizes=null;}
 public int WaterBodySize(int x,int y){int region=SeaRegionOf(y*Width+x);if(region==0)return 0;if(seaSizes==null){seaSizes=new Dictionary<int,int>();foreach(int r in seaRegion)if(r!=0){int n;seaSizes.TryGetValue(r,out n);seaSizes[r]=n+1;}}int size;seaSizes.TryGetValue(region,out size);return size;}
 public bool NextToBigWater(int x,int y){for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){if(dx==0&&dy==0)continue;var n=Step(x,y,dx,dy);if(n!=null&&IsWater(n.terrain)&&WaterBodySize(n.x,n.y)>=BigWater)return true;}return false;}
 // Whether this place can ever make this: anything it cannot is left off its lists (owner request).
 public bool PlaceAllows(City c,int b){int u=BuildUnit(b);if(u>=0){if(!IsNaval(u))return true;return u==BoatUnit?NextToBigWater(c.x,c.y):HasBuilding(c,PortBuilding)||PortSite(c)!=null;}if(b==43)return AdjacentWater(c);if(b==PortBuilding)return HasBuilding(c,PortBuilding)||PortSite(c)!=null;return true;}
 // ---------- workers run from wild animals (owner request, 7 Nov 2026) ----------
 // A worker with no soldier beside it stops work when an animal comes within FleeNear tiles and runs for the nearest city. Once no animal
 // is within FleeFar it walks back and takes up what it was doing.
 public const int FleeNear=3,FleeFar=5;
 [NonSerialized] Dictionary<int,List<Animal>> beastGrid;[NonSerialized] int beastGridTurn=-1,beastGridCount=-1;
 public bool AnimalNear(int x,int y,int range){
  if(!wildlife||animals==null||animals.Count==0)return false;
  if(beastGrid==null||beastGridTurn!=turn||beastGridCount!=animals.Count){beastGrid=new Dictionary<int,List<Animal>>();beastGridTurn=turn;beastGridCount=animals.Count;foreach(var a in animals){int key=(a.y/8)*100000+a.x/8;List<Animal> list;if(!beastGrid.TryGetValue(key,out list)){list=new List<Animal>();beastGrid[key]=list;}list.Add(a);}}
  int columns=(Width+7)/8;for(int r=y/8-1;r<=y/8+1;r++)for(int k=-1;k<=1;k++){int col=((x/8+k)%columns+columns)%columns;List<Animal> list;if(r>=0&&beastGrid.TryGetValue(r*100000+col,out list))foreach(var a in list)if(a.hp>0&&Distance(a.x,a.y,x,y)<=range)return true;}
  return false;
 }
 bool WorkerSafe(Unit u){if(cities.Any(c=>c.owner==u.owner&&c.x==u.x&&c.y==u.y)||AtHome(u))return true;foreach(var a in units)if(a.owner==u.owner&&a.x==u.x&&a.y==u.y&&Power[a.type]>0)return true;return false;}
 City NearestOwnCity(Unit u){City best=null;int least=int.MaxValue;foreach(var c in cities)if(c.owner==u.owner){int d=Distance(u.x,u.y,c.x,c.y);if(d<least||d==least&&best!=null&&c.id<best.id){least=d;best=c;}}return best;}
 void ResumeWork(Unit u){u.flee=0;u.job=u.fleeJob;u.jobTurns=u.fleeJobTurns;u.hasOrder=u.fleeOrder;u.destinationX=u.fleeDX;u.destinationY=u.fleeDY;u.roadOrder=u.fleeRoad;u.autoWork=u.fleeAuto;if(u.owner==0)Note("A Worker has gone back to work.");}
 void RunForCity(Unit u){var c=NearestOwnCity(u);if(c==null)return;int keep=u.flee;Order(u,c.x,c.y);u.flee=keep;}
 internal void WorkersFlee(int owner){
  if(!wildlife)return;
  foreach(var u in units.Where(a=>a.owner==owner&&a.type==1).ToList()){
   if(u.flee==0){
    if(!AnimalNear(u.x,u.y,FleeNear)||WorkerSafe(u)||NearestOwnCity(u)==null)continue;
    u.fleeX=u.x;u.fleeY=u.y;u.fleeJob=u.job;u.fleeJobTurns=u.jobTurns;u.fleeOrder=u.hasOrder;u.fleeDX=u.destinationX;u.fleeDY=u.destinationY;u.fleeRoad=u.roadOrder;u.fleeAuto=u.autoWork;
    u.roadOrder=false;u.autoWork=false;CancelOrder(u);u.flee=1;RunForCity(u);if(owner==0)Note("A Worker has stopped work and is running from wild animals.");
   }else if(u.flee==1){
    if(AnimalNear(u.x,u.y,FleeFar)){if(!u.hasOrder&&!WorkerSafe(u))RunForCity(u);continue;}
    u.flee=2;if(u.x==u.fleeX&&u.y==u.fleeY){ResumeWork(u);continue;}Order(u,u.fleeX,u.fleeY);u.flee=2;if(!u.hasOrder&&!(u.x==u.fleeX&&u.y==u.fleeY))ResumeWork(u);
   }else{
    if(AnimalNear(u.x,u.y,FleeNear)&&!WorkerSafe(u)){u.flee=1;RunForCity(u);continue;}
    if(!u.hasOrder||u.x==u.fleeX&&u.y==u.fleeY)ResumeWork(u);
   }
  }
 }
 // ---------- stacks and escorts (owner request, 6 Nov 2026; after Civilization IV's stacks and Civilization VI's escort formations) ----------
 // Units on one tile can be joined into a stack that moves as one, at the speed of its slowest member. A soldier stacked with a worker
 // or settler is its escort: an attacker has to get through every soldier on the tile first.
 public List<Unit> StackOf(Unit u){if(u==null)return new List<Unit>();if(u.stack==0)return new List<Unit>{u};return units.Where(a=>a.stack==u.stack&&a.owner==u.owner&&a.x==u.x&&a.y==u.y).ToList();}
 bool Stackable(Unit u){return !IsNaval(u.type);}
 public List<Unit> UnitsToStack(Unit u){return units.Where(a=>a.owner==u.owner&&a.x==u.x&&a.y==u.y&&Stackable(a)).ToList();}
 public bool JoinStack(Unit u){if(u==null||!Stackable(u))return false;var all=UnitsToStack(u);if(all.Count<2)return false;int id=nextId++;foreach(var a in all)a.stack=id;return true;}
 public void LeaveStack(Unit u){if(u==null||u.stack==0)return;var all=StackOf(u);foreach(var a in all)a.stack=0;}
 bool StackFits(Unit u,List<Unit> mates,int x,int y){
  int soldiers=Power[u.type]>0?1:0,civilians=Power[u.type]==0&&u.type!=1?1:0;foreach(var m in mates){if(Power[m.type]>0)soldiers++;else if(m.type!=1)civilians++;}
  foreach(var a in units)if(a.owner==u.owner&&a.x==x&&a.y==y&&!IsNaval(a.type)){if(Power[a.type]>0)soldiers++;else if(a.type!=1)civilians++;}
  return soldiers<=SoldierStack&&(civilians<=CivilianStack||cities.Any(c=>c.owner==u.owner&&c.x==x&&c.y==y));
 }
 // ---------- road upkeep (owner request, 6 Nov 2026; as in Civilization V, where roads cost gold each turn) ----------
 public const int RoadTilesPerGold=3;
 [NonSerialized] Dictionary<int,int> roadCount;[NonSerialized] int roadCountTurn=-1;
 internal void ResetRoadCount(){roadCountTurn=-1;}
 public int RoadTiles(int owner){if(roadCount==null||roadCountTurn!=turn){roadCount=new Dictionary<int,int>();roadCountTurn=turn;var centres=CityCentres();foreach(var t in tiles){if(!t.road||t.owner<0||centres.Contains(t.y*Width+t.x))continue;int n;roadCount.TryGetValue(t.owner,out n);roadCount[t.owner]=n+1;}}int found;roadCount.TryGetValue(owner,out found);return found;}
 public int RoadUpkeep(int owner){return (RoadTiles(owner)+RoadTilesPerGold-1)/RoadTilesPerGold;}
 // ---------- plotting a road ----------
 // The way a road would run from one tile to another, for a worker of this civilisation; existing roads are followed where they help.
 public List<Tile> RoadCourse(int owner,int fromX,int fromY,int toX,int toY){var probe=new Unit{id=-1,owner=owner,type=1,x=fromX,y=fromY,moves=4};return Route(probe,toX,toY,true);}
 public int NewRoadTiles(IEnumerable<Tile> course){int n=0;foreach(var t in course)if(!t.road&&!IsWater(t.terrain))n++;return n;}
 public string PlanRoad(Unit u,List<Tile> course){
  if(u==null||u.type!=1)return "Only workers build roads.";if(!HasTech(u.owner,"wheel"))return "Requires The Wheel.";if(course==null||course.Count==0)return "No land route to that destination.";
  var last=course[course.Count-1];foreach(var a in WorkerTeam(u)){CancelOrder(a);a.autoWork=false;a.roadOrder=true;a.roadX=last.x;a.roadY=last.y;if(a.roadPath==null)a.roadPath=new List<int>();a.roadPath.Clear();foreach(var t in course)a.roadPath.Add(t.y*Width+t.x);}
  foreach(var a in WorkerTeam(u))RoadStep(a);return "Road ordered: "+NewRoadTiles(course)+" new tiles.";
 }
 // ---------- sealed gateways the player has found ----------
 public List<int> gatesFound=new List<int>();
 [NonSerialized] List<int> gateCells;
 List<int> GateCells(){if(gateCells==null){gateCells=new List<int>();for(int i=0;i<tiles.Length;i++)if(tiles[i].realm==0&&tiles[i].gateLink>0)gateCells.Add(i);}return gateCells;}
 void MarkKnownGates(){if(gatesFound==null)gatesFound=new List<int>();foreach(int i in GateCells())if(tiles[i].seen&&!gatesFound.Contains(i))gatesFound.Add(i);}
 // A gateway the player has only just seen, once each, or null.
 public Tile NextNewGate(){if(gatesFound==null)gatesFound=new List<int>();foreach(int i in GateCells())if(tiles[i].seen&&!gatesFound.Contains(i)){gatesFound.Add(i);return tiles[i];}return null;}
 // ---------- food: an army marches on its stomach (owner request, 6 Nov 2026; save version 30) ----------
 // Every unit eats. Each city's fields feed one food's worth of units; the rest comes out of what its cities would have stored for
 // growth. When even that runs out the empire goes hungry: the newest units stop where they are until there is food, and every
 // city loses a content citizen. An empire with food to spare thrives and every city gains one.
 public bool foodRations;
 public const int FieldRations=1;
 // In halves of a food a turn: horses, siege trains and big crews eat twice as much.
 public static int UnitRation(int type){return type==5||type==8||type==9||type==13||type==14?2:1;}
 public void FoodOutlook(int owner,List<City> own,int[][] yields,out int surplus,out int need){
  int halves=0;foreach(var u in units)if(u.owner==owner)halves+=UnitRation(u.type);
  need=Math.Max(0,(halves+1)/2-own.Count*FieldRations);surplus=0;for(int i=0;i<own.Count;i++)surplus+=Math.Max(0,yields[i][0]);
 }
 // What the next turn looks like, for the interface.
 public int FoodBalance(int owner,out int need){var own=cities.Where(c=>c.owner==owner).ToList();var y=own.Select(Yield).ToArray();int surplus;FoodOutlook(owner,own,y,out surplus,out need);return surplus-need;}
 public string FoodStateName(int owner){var n=nations[owner];return n.foodState>0?"Thriving":n.foodState<0?"Hungry":"Fed";}
 public int UnfedUnits(int owner){int count=0;foreach(var u in units)if(u.owner==owner&&u.unfed)count++;return count;}
 void FeedUnits(int owner,List<City> own,int[][] yields){
  var n=nations[owner];foreach(var c in own)c.ration=0;
  if(!foodRations||own.Count==0){n.foodState=0;n.rationShort=0;foreach(var u in units)if(u.owner==owner)u.unfed=false;return;}
  int surplus,need;FoodOutlook(owner,own,yields,out surplus,out need);
  // Each city gives in proportion to what it has, then any odd food left comes from the best fed.
  int left=Math.Min(need,surplus);if(surplus>0){for(int i=0;i<own.Count&&left>0;i++){int spare=Math.Max(0,yields[i][0]);int take=Math.Min(left,spare*Math.Min(need,surplus)/surplus);own[i].ration=take;left-=take;}for(int i=0;i<own.Count&&left>0;i++){int take=Math.Min(left,Math.Max(0,yields[i][0])-own[i].ration);if(take>0){own[i].ration+=take;left-=take;}}}
  int shortBy=Math.Max(0,need-surplus);n.rationShort=shortBy;n.foodState=shortBy>0?-1:surplus-need>=own.Count?1:0;
  int hungry=shortBy*2,before=0;foreach(var u in units)if(u.owner==owner){if(u.unfed)before++;u.unfed=false;}
  if(hungry>0){var list=units.Where(u=>u.owner==owner).OrderByDescending(u=>u.id).ToList();foreach(var u in list){if(hungry<=0)break;u.unfed=true;hungry-=UnitRation(u.type);}}
  if(owner==0){int now=UnfedUnits(0);if(now>0&&before==0)Note("Your people are hungry. "+now+(now==1?" unit has":" units have")+" no food and cannot act until there is more.");else if(now==0&&before>0)Note("There is food again, and all your units can act.");}
 }
 // ---------- owner requests, 6 Nov 2026, second batch ----------
 // How long research takes: what is left divided by the science coming in, as in the Civilization series. One technology takes at
 // least MinResearchTurns however much science there is (the Civilization III rule), and what is left over carries to the next.
 public const int MinResearchTurns=10;
 public int SciencePerTurn(int owner){int sum=0;foreach(var c in cities)if(c.owner==owner)sum+=Yield(c)[2];return sum;}
 public int ResearchCap(int t,int owner){int least=MinTurns();return Math.Max(1,(ResearchCost(t,owner)+least-1)/least);}
 // Game speed (owner request, 7 Nov 2026): research in a Long game now takes what Standard used to; Standard takes two thirds of that
 // and Quick one third, and so does the least number of turns a technology needs.
 public static readonly int[] SpeedMinTurns={4,7,10};
 public int MinTurns(){return modernRules?SpeedMinTurns[Math.Max(0,Math.Min(2,speed))]:MinResearchTurns;}
 // Turns to finish, or -1 when no science is coming in.
 public int ResearchTurns(int t,int owner){int rate=Math.Min(SciencePerTurn(owner),ResearchCap(t,owner));if(rate<=0)return -1;int done=owner==0&&t==research?science:nations[owner].progress[t];int left=Math.Max(0,ResearchCost(t,owner)-done);return Math.Max(1,(left+rate-1)/rate);}
 bool ResearchOpen(int owner,int t){var n=nations[owner];if(n.tech[t])return false;foreach(int r in Requirements[t])if(!n.tech[r])return false;return true;}
 [NonSerialized] public int researchOverflow;
 // Choosing a different technology after a discovery takes the left-over science with it.
 public void ChooseNextResearch(int t){int spare=researchOverflow;researchOverflow=0;if(research>=0){int take=Math.Min(spare,science);science-=take;nations[0].progress[research]=science;spare=take;}else spare=0;SelectResearch(t);if(research==t){science+=spare;nations[0].progress[t]=science;}}
 // What the budget is made of; Budget is its totals.
 public sealed class BudgetLines {public int cityGold,cityBonus,ports,routes,treaties,tribute,cityUpkeep,buildingUpkeep,army,civilians,luxury,treatyCost,roads,expansion,cutOff,foraging;public int Income{get{return cityGold+cityBonus+ports+routes+treaties+tribute+foraging;}}public int Expense{get{return cityUpkeep+buildingUpkeep+army+civilians+luxury+treatyCost+roads+expansion+cutOff;}}public int Net{get{return Income-Expense;}}}
 public static int SupportCost(int type){return Upkeep[type];}
 public BudgetLines BudgetDetail(int owner){
  var b=new BudgetLines();var own=cities.Where(c=>c.owner==owner).OrderBy(c=>c.id).ToList();
  foreach(var c in own){b.cityGold+=Yield(c)[3];b.ports+=PortIncome(c);if(!Disorder(c))b.cityBonus+=(HasBuilding(c,31)?2:0)+(Connected(c)&&c!=own[0]?3:0);int distance=Distance(c.x,c.y,own[0].x,own[0].y)/10;b.cityUpkeep+=Math.Max(0,1+c.pop/4+distance-(HasBuilding(c,34)?2:0));b.buildingUpkeep+=c.buildings.Sum(BuildingUpkeep);}
  // Every soldier and warship is paid for (owner request); only civilians have free support from cities and government.
  int free=0,soldiers=0;
  // A wandering people with no city yet pays nothing, and the capital keeps one soldier as its guard for free (owner request,
  // 7 Nov 2026: a new game with a Warrior and a Worker makes +1 gold a turn once the capital stands).
  if(modernRules){int armyHalves=0,freeHalves=0,guard=0;foreach(var u in units)if(u.owner==owner){int halves=Upkeep[u.type]*(AtHome(u)?1:2);if(Power[u.type]>0){armyHalves+=halves;soldiers++;guard=Math.Max(guard,Math.Min(4,halves));}else freeHalves+=halves;}if(own.Count==0){armyHalves=0;b.foraging=1;}else armyHalves-=guard;b.army=(armyHalves+1)/2;free=(freeHalves+1)/2;}
  else foreach(var u in units)if(u.owner==owner){if(Power[u.type]>0){b.army+=Upkeep[u.type];soldiers++;}else free+=Upkeep[u.type];}
  // A big standing army costs more than the sum of its soldiers (as unit maintenance climbs in Civilization V): +1 gold for every three
  // soldiers beyond two per city.
  b.army+=Math.Max(0,soldiers-2*own.Count)/3;b.roads=RoadUpkeep(owner);
  if(modernRules){b.expansion=ExpansionCost(own.Count);foreach(var c in own)if(CutOff(c))b.cutOff+=CutOffGold;}
  b.civilians=Math.Max(0,free-FreeUnitsPerCity*own.Count-GovernmentSupport(owner));b.luxury=own.Count*nations[owner].luxury;
  b.routes=TradeIncome(owner);b.treaties=DiplomaticIncome(owner);b.tribute=VillageTribute(owner)+ReligionTithe(owner);b.treatyCost=DiplomaticExpense(owner);return b;
 }
 // Cities joined by road, by river (Sailing) or by sea between linked ports (Sailing) trade with each other (owner request):
 // +1 Trade for each other city joined, at most LinkTradeLimit. Worked out once a turn for each civilisation.
 public const int LinkTradeLimit=4;
 [NonSerialized] Dictionary<int,int> linkCount;[NonSerialized] int linkTurn=-1;[NonSerialized] HashSet<int> linkDone;[NonSerialized] int[] seaRegion;
 internal void ResetLinks(){linkTurn=-1;}
 public int LinkTrade(City c){
  if(linkTurn!=turn||linkCount==null){linkCount=new Dictionary<int,int>();linkDone=new HashSet<int>();linkTurn=turn;}
  if(linkDone.Add(c.owner))CountLinks(c.owner);int n;linkCount.TryGetValue(c.id,out n);return Math.Min(LinkTradeLimit,n);
 }
 int SeaRegionOf(int cell){
  if(seaRegion==null){seaRegion=new int[tiles.Length];int next=0;var stack=new Stack<int>();for(int i=0;i<tiles.Length;i++){if(seaRegion[i]!=0||!IsWater(tiles[i].terrain))continue;next++;seaRegion[i]=next;stack.Push(i);while(stack.Count>0){var t=tiles[stack.Pop()];for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var w=Step(t.x,t.y,dx,dy);if(w==null||!IsWater(w.terrain))continue;int at=w.y*Width+w.x;if(seaRegion[at]==0){seaRegion[at]=next;stack.Push(at);}}}}}
  return seaRegion[cell];
 }
 void CountLinks(int owner){
  var own=cities.Where(c=>c.owner==owner).ToList();if(own.Count<2)return;bool sailing=HasTech(owner,"sailing");
  var group=new Dictionary<int,int>();foreach(var c in own)group[c.id]=c.id;Func<int,int> find=null;find=a=>{int g=group[a];if(g==a)return a;int top=find(g);group[a]=top;return top;};
  var cityAt=new Dictionary<int,City>();foreach(var c in own)cityAt[c.y*Width+c.x]=c;
  var blocked=new HashSet<int>();foreach(var u in units)if(u.owner!=owner&&AtWar(owner,u.owner))blocked.Add(u.y*Width+u.x);
  var seen=new HashSet<int>();var q=new Queue<Tile>();
  foreach(var c in own){int home=c.y*Width+c.x;if(seen.Contains(home))continue;seen.Add(home);q.Enqueue(tiles[home]);
   while(q.Count>0){var t=q.Dequeue();for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var n=Step(t.x,t.y,dx,dy);if(n==null||n.owner!=owner)continue;int at=n.y*Width+n.x;City there;bool isCity=cityAt.TryGetValue(at,out there);if(!isCity&&!n.road&&!(sailing&&n.terrain==7))continue;if(blocked.Contains(at)||!seen.Add(at))continue;if(isCity)group[find(there.id)]=find(c.id);q.Enqueue(n);}}
  }
  if(sailing){var bySea=new Dictionary<int,int>();foreach(var c in own){if(!c.portLinked)continue;var port=PortTile(c);if(port==null)continue;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var w=Step(port.x,port.y,dx,dy);if(w==null||!IsWater(w.terrain))continue;int region=SeaRegionOf(w.y*Width+w.x),other;if(bySea.TryGetValue(region,out other))group[find(c.id)]=find(other);else bySea[region]=c.id;}}}
  var sizes=new Dictionary<int,int>();foreach(var c in own){int g=find(c.id),s;sizes.TryGetValue(g,out s);sizes[g]=s+1;}
  foreach(var c in own)linkCount[c.id]=sizes[find(c.id)]-1;
 }
 // Takes one building (or unit) off the city's list, wherever it is on it.
 public bool CancelProject(City c,int project){
  if(c==null)return false;bool building=BuildUnit(project)<0;var queue=building?c.buildingQueue:c.queue;
  if(queue.Remove(project))return true;
  if(building){if(c.buildingProject!=project)return false;c.buildingProduction=0;c.buildingProject=queue.Count>0?queue[0]:-1;if(queue.Count>0)queue.RemoveAt(0);return true;}
  if(c.build!=project)return false;c.production=0;c.build=queue.Count>0?queue[0]:-1;if(queue.Count>0)queue.RemoveAt(0);return true;
 }
 // ---------- the sealed worlds as your civilisation knows them ----------
 [NonSerialized] bool[] worldsKnown;[NonSerialized] int worldsKnownTurn=-1;
 // A world is known once a home civilisation has found a gateway to it or set eyes on any part of it.
 public bool WorldKnownAtHome(int r){
  if(r<=0||r>RealmCount||!HasRealms)return r==0;
  if(worldsKnown==null||worldsKnownTurn!=turn){worldsKnown=new bool[RealmCount+1];worldsKnownTurn=turn;var home=Enumerable.Range(0,civCount).Where(o=>!IsNativeNation(o)).ToList();
   for(int i=0;i<tiles.Length;i++){var t=tiles[i];int world=t.realm>0?t.realm:0;if(t.realm==0&&t.gateLink>0){var twin=GateTwin(t);if(twin!=null)world=twin.realm;}if(world<=0||worldsKnown[world])continue;foreach(int o in home)if(o==0?t.seen:nations[o].known[i]){worldsKnown[world]=true;break;}}}
  return worldsKnown[r];
 }
 public int NationOfPeople(int civ){for(int o=1;o<civCount;o++)if(nations[o].civ==civ&&IsNativeNation(o))return o;return -1;}
 // How much is known of another world's people: 0 nothing, 1 name and face, 2 customs and history, 3 everything. It grows with
 // contact and friendship, or by sending agents.
 public int PeopleDetail(int o){
  if(o<=0||o>=civCount)return 0;int level=nations[o].intel;
  if(met[o]||Enumerable.Range(1,civCount-1).Any(a=>a!=o&&!IsNativeNation(a)&&HasContact(a,o)))level=Math.Max(level,1);
  if(met[o]){if(Opinion(o,0)>=25)level=Math.Max(level,2);if(Friends(0,o))level=Math.Max(level,3);}
  return Math.Min(3,level);
 }
 public int SpyPrice(int o){return 50*(PeopleDetail(o)+1);}
 public string SpyReason(int o){if(!IsNativeNation(o))return "Not a people of another world.";if(!WorldKnownAtHome(CivilisationCatalog.NativeRealm(nations[o].civ)))return "That world is still unknown.";if(PeopleDetail(o)>=3)return "Nothing more to learn.";if(gold<SpyPrice(o))return "Needs "+SpyPrice(o)+" gold.";return "";}
 public bool SendAgents(int o){if(result!=""||SpyReason(o)!="")return false;int price=SpyPrice(o);gold-=price;nations[0].gold=gold;nations[o].intel=PeopleDetail(o)+1;Note("Agents return from the "+RealmNames[CivilisationCatalog.NativeRealm(nations[o].civ)]+" with news of "+CivilisationCatalog.Names[nations[o].civ]+".");return true;}
 // ---------- Civics ----------
 public CivicStats StatsFor(int owner){
  var s=new CivicStats();var own=cities.Where(c=>c.owner==owner).ToList();s.cities=own.Count;int citizens=0,read=0,calm=0,hungry=0;
  foreach(var c in own){s.population+=CityPeople(c);s.urban+=CityPeople(c);citizens+=c.pop;if(HasBuilding(c,8)||HasBuilding(c,38))read+=c.pop;var y=Yield(c);s.gnp+=Trade(c);s.goods+=y[1];s.crops+=y[0]+c.pop*2;s.science+=y[2];calm+=ContentCitizens(c)-UnhappyCitizens(c);if(y[0]<0)hungry++;}
  foreach(var t in tiles)if(t.owner==owner){s.land++;if(t.road)s.roads++;}
  foreach(var u in units)if(u.owner==owner&&Power[u.type]>0){s.soldiers++;s.strength+=MilitaryValue(u.type)+u.promotion*2;}
  s.literacy=citizens==0?0:read*100/citizens;
  // Approval as in Civilization III: half, plus the balance of content over unhappy citizens.
  s.approval=citizens==0?50:Math.Max(0,Math.Min(100,50+calm*50/citizens));
  s.lifeExpectancy=own.Count==0?0:(modernRules?LifeExpectancy(owner):28+Era(owner)*8)+own.Count(c=>c.granary)*6/own.Count-hungry*6/own.Count;
  if(modernRules){s.population+=RuralPeople(owner,s.urban);s.urbanShare=s.population==0?0:(int)(s.urban*100/s.population);}
  return s;
 }
 [NonSerialized] public List<int> researchPrompts=new List<int>();
 [NonSerialized] public List<BuildPrompt> buildPrompts=new List<BuildPrompt>();
 [NonSerialized] Dictionary<int,List<int>> buildableBefore;
 bool BuildingLineIdle(City c){return c.buildingProject<0&&c.buildingQueue.Count==0;}
 void BeginDiscovery(){buildableBefore=new Dictionary<int,List<int>>();foreach(var c in cities.Where(a=>a.owner==0&&BuildingLineIdle(a)))buildableBefore[c.id]=BuildingIds.Where(b=>TechAllows(c,b)).ToList();}
 void EndDiscovery(int found,int spare=0){
  researchOverflow=spare;if(researchPrompts==null)researchPrompts=new List<int>();if(buildPrompts==null)buildPrompts=new List<BuildPrompt>();
  researchPrompts.Add(found);
  foreach(var c in cities.Where(a=>a.owner==0&&buildableBefore.ContainsKey(a.id))){var before=buildableBefore[c.id];var fresh=BuildingIds.Where(b=>!before.Contains(b)&&TechAllows(c,b)&&BuildReason(c,b)=="").ToList();if(fresh.Count>0)buildPrompts.Add(new BuildPrompt{city=c.id,tech=found,options=fresh});}
  buildableBefore=null;
 }
 // The next city still waiting for an answer; one that has since started a building, or is gone, no longer needs asking.
 public BuildPrompt NextBuildPrompt(){
  if(buildPrompts==null)return null;
  while(buildPrompts.Count>0){var p=buildPrompts[0];var c=cities.Find(a=>a.id==p.city&&a.owner==0);if(c!=null&&BuildingLineIdle(c)){p.options=p.options.Where(b=>BuildReason(c,b)=="").ToList();if(p.options.Count>0)return p;}buildPrompts.RemoveAt(0);}
  return null;
 }
}
}
