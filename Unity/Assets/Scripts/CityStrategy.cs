using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
 public partial class World {
  // The same tile outputs drive allocation, yield totals and the city UI.
  public int[] EffectiveTileYield(City c,Tile t){
   var v=TileYield(t);
   if(t.farm&&HasTech(c.owner,"chemistry"))v[0]++;
   if(t.farm&&HasTech(c.owner,"irrigation")&&NearRiver(t))v[0]++;
   if(t.mine&&HasTech(c.owner,"steam"))v[1]++;
   if(HasBuilding(c,43)&&(t.terrain==4||t.terrain==6||t.terrain==12))v[0]++;
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
   var chosen=candidates.Take(count).ToList();
   int need=HasTech(c.owner,"uploading")?0:c.pop*2;
   // Keep each focus where feasible, swapping only enough workers to feed the city.
   while(CityCentreYield(c)[0]+chosen.Sum(t=>t.V[0])<need){
    var extra=candidates.Where(t=>!chosen.Contains(t)).OrderByDescending(t=>t.V[0]).ThenByDescending(t=>FocusScore(c.focus,t.V)).FirstOrDefault();
    var least=chosen.OrderBy(t=>t.V[0]).ThenBy(t=>FocusScore(c.focus,t.V)).FirstOrDefault();
    if(extra==null||least==null||extra.V[0]<=least.V[0])break;
    chosen.Remove(least);chosen.Add(extra);
   }
   return chosen.Select(t=>t.Tile).ToList();
  }
  public int FoodNeeded(City c){return 12+c.pop*6;}
  public string GrowthStatus(City c){
   int net=Yield(c)[0];
   if(net<0)return "Food shortage: "+(-net)+"/turn. "+(c.food/(-net)+1)+" turns until stores run out; reduce specialists or improve food tiles.";
   if(c.pop>=GrowthLimit(c))return "Growth blocked: population limit "+GrowthLimit(c)+". "+(HasBuilding(c,41)?"Maximum city size reached.":HasBuilding(c,37)?"Build a Sewer system.":"Build an Aqueduct.");
   if(Happiness(c)<0)return "Growth blocked by unhappiness; improve happiness or lower taxes.";
   if(c.food>=FoodNeeded(c))return "Population will grow next turn.";
   if(net==0)return "No food surplus: growth is stalled.";
   return "Growth in "+Math.Max(1,(FoodNeeded(c)-c.food+net-1)/net)+" turns ("+c.food+" / "+FoodNeeded(c)+" food).";
  }
  public string CityEconomyReport(City c){
   var worked=Worked(c);var v=Yield(c);int consumption=HasTech(c.owner,"uploading")?0:c.pop*2;
   return "Food: city centre "+CityCentreYield(c)[0]+" + worked tiles "+worked.Sum(t=>EffectiveTileYield(c,t)[0])+" - consumption "+consumption+" = "+v[0]+"/turn.\n"+
    "Workers "+worked.Count+" | Specialists "+(c.scientists+c.merchants+c.entertainers)+" | Production "+v[1]+" per line | Science "+v[2]+" | Tile/specialist gold "+v[3]+" (tax and trade shown in Economy).\n"+
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
   priority.AddRange(c.focus==3?new[]{8,38,30,40}:new[]{30,8,38,40});
   priority.AddRange(available.OrderBy(Cost));
   return priority.Where(b=>available.Contains(b)).DefaultIfEmpty(-1).First();
  }
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
