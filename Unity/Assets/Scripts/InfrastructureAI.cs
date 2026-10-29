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
   return available.OrderBy(t=>ResearchCost(t,owner)/(1.0+ (ActiveTechBenefits(t).Length>0?1:0)+wanted.Count(target=>LeadsTo(t,target))*4)).ThenBy(t=>t).First();
  }
  void ImproveAI(Unit u){
   if(u.moves<=0)return;
   var own=cities.Where(c=>c.owner==u.owner).ToList();
   var worked=new HashSet<Tile>(own.SelectMany(Worked));
   bool hunger=own.Any(c=>Yield(c)[0]<=0);
   Tile target=null;int chosenJob=0,best=int.MinValue;var portRoads=PortRoadNeeds(u.owner);
   foreach(var tile in tiles.Where(t=>t.owner==u.owner&&nations[u.owner].known[t.y*Width+t.x]&&!Barren(t.terrain))){
    if(units.Any(a=>a.id!=u.id&&a.owner==u.owner&&a.type==1&&(a.job>0&&a.x==tile.x&&a.y==tile.y||a.hasOrder&&a.destinationX==tile.x&&a.destinationY==tile.y)))continue;
    if(units.Any(a=>AtWar(u.owner,a.owner)&&VisibleTo(u.owner,a.x,a.y)&&Distance(tile.x,tile.y,a.x,a.y)<=2))continue;
    var probe=new Unit{owner=u.owner,type=1,x=tile.x,y=tile.y,moves=u.moves};
    for(int job=1;job<=3;job++){
     if(JobReason(probe,job)!="")continue;
     int value=job==1?4:worked.Contains(tile)?24:10;
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
   if(u.x==target.x&&u.y==target.y)StartJob(u,chosenJob);
   else Order(u,target.x,target.y);
  }
 }
}
