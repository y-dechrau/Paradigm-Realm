using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
 public partial class World {
  // The same tile outputs drive allocation, yield totals and the city UI.
  public int[] EffectiveTileYield(City c,Tile t){
   var v=TileYield(t);
   if(t.resource>0&&t.resource<ResourceFood.Length&&!ResourceKnown(c.owner,t.resource)){v[0]-=ResourceFood[t.resource];v[1]-=ResourceShields[t.resource];v[2]-=ResourceTrade[t.resource];}
   if(t.farm&&HasTech(c.owner,"chemistry"))v[0]++;
   if(t.farm&&HasTech(c.owner,"irrigation")&&NearRiver(t))v[0]++;
   if(t.mine&&HasTech(c.owner,"steam"))v[1]++;
   if(t.resource==15&&HasTech(c.owner,"refining"))v[1]+=3;
   if(HasBuilding(c,43)&&(t.terrain==4||t.terrain==6||t.terrain==12))v[0]++;
   if(HasTrait(c.owner,TraitSeafaring)&&(t.terrain==4||t.terrain==6||t.terrain==12))v[0]++;
   if(c.portLinked&&IsSeaResource(t.resource)&&ResourceKnown(c.owner,t.resource))v[0]++;
   return v;
  }
  bool NearRiver(Tile t){for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++){var a=At(t.x+x,t.y+y);if(a!=null&&a.terrain==7)return true;}return false;}
  public List<Tile> Workable(City c){
   // Only the 5x5 block around the city can qualify, so nothing scans the whole map.
   var result=new List<Tile>();
   for(int dy=-2;dy<=2;dy++)for(int dx=-2;dx<=2;dx++){
    if(dx==0&&dy==0)continue;var t=At(c.x+dx,c.y+dy);if(t==null||t.owner!=c.owner)continue;
    if(Distance(t.x,t.y,c.x,c.y)>2||result.Contains(t))continue;
    bool occupied=false;foreach(var other in cities)if(other.x==t.x&&other.y==t.y){occupied=true;break;}
    if(occupied)continue;
    City nearest=null;int nearestDistance=int.MaxValue;
    foreach(var other in cities){if(other.owner!=c.owner)continue;int d=Distance(other.x,other.y,t.x,t.y);if(nearest==null||d<nearestDistance||(d==nearestDistance&&other.id<nearest.id)){nearest=other;nearestDistance=d;}}
    if(nearest==c)result.Add(t);
   }
   result.Sort((p,q)=>(p.y*Width+p.x).CompareTo(q.y*Width+q.x));
   return result;
  }
  static int FocusScore(int focus,int[] v){return focus==1?v[0]*4+v[1]+v[2]:focus==2?v[1]*4+v[0]+v[2]:focus==4?v[2]*6+v[0]+v[1]:v[0]*2+v[1]+v[2];}
  public List<Tile> Worked(City c){
   var candidates=Workable(c).Select(t=>new{Tile=t,V=EffectiveTileYield(c,t)}).OrderByDescending(t=>FocusScore(c.focus,t.V)).ThenBy(t=>t.Tile.y*Width+t.Tile.x).ToList();
   int count=Math.Max(0,c.pop-c.scientists-c.merchants-c.entertainers);
   // Locked tiles are worked first; the Governor (focus) fills the rest and never swaps a locked tile away.
   var locked=c.locked==null?candidates.Take(0).ToList():candidates.Where(t=>c.locked.Contains(t.Tile.y*Width+t.Tile.x)).Take(count).ToList();
   var chosen=locked.Concat(candidates.Where(t=>!locked.Contains(t)).Take(count-locked.Count)).ToList();
   int need=HasTech(c.owner,"uploading")?0:c.pop*2;
   // Keep each focus where feasible, swapping only enough workers to feed the city.
   while(CityCentreYield(c)[0]+chosen.Sum(t=>t.V[0])<need){
    var extra=candidates.Where(t=>!chosen.Contains(t)).OrderByDescending(t=>t.V[0]).ThenByDescending(t=>FocusScore(c.focus,t.V)).FirstOrDefault();
    var least=chosen.Where(t=>!locked.Contains(t)).OrderBy(t=>t.V[0]).ThenBy(t=>FocusScore(c.focus,t.V)).FirstOrDefault();
    if(extra==null||least==null||extra.V[0]<=least.V[0])break;
    chosen.Remove(least);chosen.Add(extra);
   }
   return chosen.Select(t=>t.Tile).ToList();
  }
  public int FoodNeeded(City c){return 10+c.pop*4;}
  // Stage 7 (design): click a tile to lock a citizen onto it, click again to release. Locks are capped at the number of working
  // citizens; a lock on a tile the city can no longer work is simply ignored.
  public bool ToggleLock(City c,Tile t){
   if(c==null||t==null||result!=""||!Workable(c).Contains(t))return false;if(c.locked==null)c.locked=new List<int>();int key=t.y*Width+t.x;
   if(c.locked.Remove(key))return true;if(c.locked.Count(k=>Workable(c).Any(w=>w.y*Width+w.x==k))>=Workers(c))return false;c.locked.Add(key);return true;
  }
  public bool IsLocked(City c,Tile t){return c!=null&&t!=null&&c.locked!=null&&c.locked.Contains(t.y*Width+t.x);}
  // Citizens with no tile left to work become entertainers automatically (design).
  public void AutoEntertainers(City c){int idle=Workers(c)-Workable(c).Count;if(idle>0)c.entertainers+=idle;}
  // One click turns an entertainer into a scientist or tax collector, or back (types: 0 scientist, 1 tax collector, 2 entertainer).
  public bool ConvertSpecialist(City c,int from,int to){if(c==null||from==to||from<0||from>2||to<0||to>2)return false;if(!AdjustSpecialist(c,from,-1))return false;if(AdjustSpecialist(c,to,1))return true;AdjustSpecialist(c,from,1);return false;}
  public string GrowthStatus(City c){
   int net=Yield(c)[0];
   // Disorder comes first: it stops production and gold as well as growth.
   if(Disorder(c))return "Disorder: "+UnhappyCitizens(c)+" unhappy outnumber "+ContentCitizens(c)+" content. No production, no gold and no growth; add entertainers, build a Temple or raise the science rate to 60%.";
   if(net<0)return "Food shortage: "+(-net)+"/turn. "+(c.food/(-net)+1)+" turns until stores run out; reduce specialists or improve food tiles.";
   if(c.pop>=GrowthLimit(c))return "Growth blocked: population limit "+GrowthLimit(c)+". "+(HasBuilding(c,41)?"Maximum city size reached.":HasBuilding(c,37)?"Build a Sewer system.":"Build an Aqueduct.");
   if(c.food>=FoodNeeded(c))return "Population will grow next turn.";
   if(net==0)return "No food surplus: growth is stalled.";
   return "Growth in "+Math.Max(1,(FoodNeeded(c)-c.food+net-1)/net)+" turns ("+c.food+" / "+FoodNeeded(c)+" food).";
  }
  public string CityEconomyReport(City c){
   var worked=Worked(c);var v=Yield(c);int consumption=HasTech(c.owner,"uploading")?0:c.pop*2;
   return "Food: city centre "+CityCentreYield(c)[0]+" + worked tiles "+worked.Sum(t=>EffectiveTileYield(c,t)[0])+" - consumption "+consumption+" = "+v[0]+"/turn.\n"+
    "Workers "+worked.Count+" | Specialists "+(c.scientists+c.merchants+c.entertainers)+" | Production "+v[1]+" per line | Trade "+Trade(c)+" at "+nations[c.owner].scienceRate+"% science: Science "+v[2]+" | Gold "+v[3]+" (upkeep and routes shown in Economy).\n"+
    GrowthStatus(c);
  }
  void ProcessCityFood(City c,int net){
   int oldPop=c.pop,previous=c.food,threshold=FoodNeeded(c);c.food+=net;
   if(c.food>=threshold&&c.pop<GrowthLimit(c)&&Happiness(c)>=0){
    c.food=c.granary?threshold/2:0;c.pop++;
    if(c.owner==0)Note(c.name+" grew to population "+c.pop+".");
   }
   if(c.food<0){c.pop=Math.Max(1,c.pop-1);c.food=0;NormaliseSpecialists(c);if(c.owner==0)Note(c.name+(c.pop<oldPop?" lost a citizen to starvation.":" has no food reserves.")+" Reduce specialists or improve food tiles.");}
   // Blocked cities cannot bank centuries of food and grow every turn later.
   c.food=Math.Min(c.food,FoodNeeded(c));
   if(c.owner==0&&previous<threshold&&c.food>=threshold&&c.pop==oldPop)Note(c.name+": "+GrowthStatus(c));
  }
  public int PreferredBuilding(City c){
   var available=Choices(c).Where(b=>BuildUnit(b)<0).ToList();var v=Yield(c);
   var priority=new List<int>();
   if(Happiness(c)<1)priority.AddRange(new[]{36,33,42});
   if(c.pop>=GrowthLimit(c)-1)priority.AddRange(new[]{37,41});
   if(v[0]>0&&c.pop<GrowthLimit(c))priority.Add(7);
   if(Budget(c.owner)[2]<2)priority.AddRange(new[]{31,34,39});
   if(v[0]<=0)priority.Add(43);
   if(units.Any(u=>AtWar(c.owner,u.owner)&&VisibleTo(c.owner,u.x,u.y)&&Distance(c.x,c.y,u.x,u.y)<=4))priority.Add(9);
   if(AIWantsPort(c))priority.Add(PortBuilding);
   priority.AddRange(c.focus==3?new[]{8,38,30,40}:new[]{30,8,38,40});
   priority.AddRange(available.OrderBy(Cost));
   return priority.Where(b=>available.Contains(b)).DefaultIfEmpty(-1).First();
  }
  public City ProjectCityFor(int owner){return ProjectCity(owner)??cities.Where(a=>a.owner==owner&&a.pop>=ProjectSize).OrderByDescending(a=>Yield(a)[1]).ThenBy(a=>a.id).FirstOrDefault();}
  // AI does not receive extra yields, free specialists or hidden terrain knowledge.
  void ManageCityAI(City c){
   c.scientists=c.merchants=c.entertainers=0;
   int balance=Budget(c.owner)[2];
   c.focus=Yield(c)[0]<=0?1:balance<0?4:c.pop>=GrowthLimit(c)?2:0;
   if(Happiness(c)<0){while(Happiness(c)<0&&c.entertainers<c.pop){c.entertainers++;if(Yield(c)[0]<0){c.entertainers--;break;}}}
   if(c.pop>=3&&Happiness(c)>=0){
    if(balance<0)c.merchants=1;else if(c.library)c.scientists=1;
    if(Yield(c)[0]<0){c.merchants=0;c.scientists=0;}
   }
  }
 }
}
