using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
 public partial class World {
  bool LeadsTo(int available,int target){return available==target||Requirements[target].Any(p=>LeadsTo(available,p));}
  public int PreferredResearch(int owner){
   var available=AvailableTech(owner);if(available.Count==0)return -1;
   var wanted=new List<int>();var own=cities.Where(c=>c.owner==owner).ToList();
   if(!HasTech(owner,"agriculture"))wanted.Add(TechnologyCatalog.Index("agriculture"));
   if(!HasTech(owner,"wheel"))wanted.Add(TechnologyCatalog.Index("wheel"));
   if(!HasTech(owner,"mining"))wanted.Add(TechnologyCatalog.Index("mining"));
   if(own.Any(c=>c.pop>=GrowthLimit(c)-1))wanted.Add(BuildingTech[Array.IndexOf(BuildingIds,own.Any(c=>!HasBuilding(c,37))?37:41)]);
   if(own.Any(c=>Happiness(c)<1))wanted.Add(TechnologyCatalog.Index("mysticism"));
   if(Budget(owner)[2]<0)wanted.Add(BuildingTech[Array.IndexOf(BuildingIds,31)]);
   if(PortReach(owner)||FerryWanted.Contains(owner)){wanted.Add(TechnologyCatalog.Index("sailing"));wanted.Add(TechnologyCatalog.Index("engineering"));}
   if(Enumerable.Range(0,civCount).Any(other=>other!=owner&&Friends(owner,other))){wanted.Add(TechnologyCatalog.Index("writing"));wanted.Add(TechnologyCatalog.Index("laws"));}
   wanted=wanted.Where(t=>t>=0&&!nations[owner].tech[t]).ToList();
   // Rivals favour keystones (they advance the era); minor technologies count as three times their cost.
   return available.OrderBy(t=>ResearchCost(t,owner)*(TechnologyCatalog.IsKeystone(t)?1.0:3.0)/(1.0+ (ActiveTechBenefits(t).Length>0?1:0)+wanted.Count(target=>LeadsTo(t,target))*4)).ThenBy(t=>t).First();
  }
  int aiWorkOwner=-1,aiWorkTurn=-1;bool aiHunger;HashSet<Tile> aiWorked;HashSet<int> aiPortRoads;List<Tile> aiOwnTiles;
  // One walk over the map each turn sorts the tiles by owner, instead of one walk for every civilisation with a worker.
  [NonSerialized] Dictionary<int,List<Tile>> ownedTiles;[NonSerialized] int ownedTilesTurn=-1;
  List<Tile> TilesOwnedBy(int owner){if(ownedTiles==null||ownedTilesTurn!=turn){ownedTiles=new Dictionary<int,List<Tile>>();ownedTilesTurn=turn;foreach(var t in tiles){if(t.owner<0)continue;List<Tile> list;if(!ownedTiles.TryGetValue(t.owner,out list)){list=new List<Tile>();ownedTiles[t.owner]=list;}list.Add(t);}}List<Tile> found;return ownedTiles.TryGetValue(owner,out found)?found:new List<Tile>();}
  [NonSerialized] bool aiRoadOk;const int RoadsPerCity=8;
  void ImproveAI(Unit u){
   if(u.moves<=0)return;
   // Idle sealed workers look for work one turn in four: nobody can see them, and the search is the costliest part of a crowded turn.
   if(gatesLocked&&IsNativeNation(u.owner)&&(turn+u.id)%4!=0)return;
   // Home rivals likewise, once the early rush of building is over: one turn in three.
   if(u.owner!=0&&turn>80&&(turn+u.id)%3!=0)return;
   // What the whole empire works and needs is the same for each of its workers this turn, so it is found once.
   if(u.owner==0||aiWorkOwner!=u.owner||aiWorkTurn!=turn){aiWorkOwner=u.owner;aiWorkTurn=turn;var empire=cities.Where(c=>c.owner==u.owner).ToList();aiWorked=new HashSet<Tile>(empire.SelectMany(Worked));aiHunger=empire.Any(c=>Yield(c)[0]<=0);aiPortRoads=PortRoadNeeds(u.owner);aiRoadOk=Budget(u.owner)[2]>=(modernRules&&empire.Any(CutOff)?0:3)&&RoadTiles(u.owner)<empire.Count*RoadsPerCity;aiOwnTiles=TilesOwnedBy(u.owner).Where(t=>nations[u.owner].known[t.y*Width+t.x]&&!Barren(t.terrain)).ToList();}
   var worked=aiWorked;
   // Tiles another of this people's workers has taken, and whether any enemy is in play at all, are worked out once.
   var taken=new HashSet<int>();foreach(var a in units)if(a.owner==u.owner&&a.type==1&&a.id!=u.id){if(a.job>0)taken.Add(a.y*Width+a.x);if(a.hasOrder)taken.Add(a.destinationY*Width+a.destinationX);}
   bool enemies=AtWarWithAnyone(u.owner);
   bool hunger=aiHunger;
   Tile target=null;int chosenJob=0,best=int.MinValue;var portRoads=aiPortRoads;
   // Your auto-developing workers never stand idle while anything in your territory can still be improved (owner request, 9 Nov 2026):
   // a second pass takes any remaining job, roads included, even where a road would not pay for itself.
   for(int pass=0;pass<(u.owner==0?2:1)&&target==null;pass++)
   foreach(var tile in aiOwnTiles){
    if(taken.Contains(tile.y*Width+tile.x))continue;
    if(enemies&&units.Any(a=>AtWar(u.owner,a.owner)&&VisibleTo(u.owner,a.x,a.y)&&Distance(tile.x,tile.y,a.x,a.y)<=2))continue;
    var probe=new Unit{owner=u.owner,type=1,x=tile.x,y=tile.y,moves=u.moves};
    foreach(int job in new[]{1,2,3,5}){
     if(JobReason(probe,job)!="")continue;
     // Roads cost upkeep, so they are built only where they pay: to a port or an improved resource, while the treasury can bear it.
     if(pass==0&&job==1&&!aiRoadOk&&!portRoads.Contains(tile.y*Width+tile.x)&&!((tile.farm||tile.mine)&&tile.resource>0))continue;
     // Clearing woodland is worth a little less than the farm it makes room for.
     int value=job==1?4:job==5?(worked.Contains(tile)?18:6):worked.Contains(tile)?24:10;
     if(job==2&&hunger)value+=20;
     bool improvement=tile.resource>0&&(tile.resource==2||tile.resource==3?job==3:job==2);
     if(improvement)value+=24;
     if(job==1&&(tile.farm||tile.mine)&&tile.resource>0)value+=30;
     // A port only works while a road joins it to its city.
     if(job==1&&portRoads.Contains(tile.y*Width+tile.x))value+=60;
     // Grow the existing road network rather than scattering isolated road tiles.
     if(job==1&&Network(u.owner).Any(n=>Distance(n%Width,n/Width,tile.x,tile.y)==1))value+=10;
     value-=Distance(u.x,u.y,tile.x,tile.y)*2;
     if(value>best){best=value;target=tile;chosenJob=job;}
    }
   }
   if(target==null)return;
   u.autoJob=chosenJob;if(u.x==target.x&&u.y==target.y)StartJob(u,chosenJob);
   else Order(u,target.x,target.y);
  }
 }
}
