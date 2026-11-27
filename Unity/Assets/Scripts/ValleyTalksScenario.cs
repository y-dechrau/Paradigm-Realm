using System;
using System.Linq;
namespace ParadigmRealm {
// The valley encounter (owner brief, 27 Nov 2026), "-valleyTalk <variant>" on the command line. One map (seed 2701), you and one
// neighbour in the Dawn Age. Between your capitals lies a wooded place by a river (the ford). Your Settler stands two tiles from
// it and the neighbour's Scout beside it sees your Settler; the neighbour's Settler is setting out for it. The neighbour is
// building a Workshop and then a Granary. Only its access to timber changes between variants:
//   plenty       40 timber in store                                  -> it says nothing about timber
//   short        a little short of what the Workshop needs              -> it offers to buy a few timber from you
//   lasting      no store, no woods of its own                         -> it asks you to leave the ford to it, and pays
//   alternative  as lasting, but an unclaimed wood near its capital    -> it quietly goes for that wood instead
//   farmland     40 timber, but the ford is far better farmland than any other site it knows -> it asks for the ford for the land
public static class ValleyVariants{public static readonly string[] Names={"plenty","short","lasting","alternative","farmland"};}
public partial class World {
 public static World ValleyTalkScenario(string variant="lasting",int seed=2701){
  for(int attempt=0;attempt<16;attempt++){
   var w=World.New(seed+attempt,"Riverfold",0,1,1,0,1,2,2,hex:true,natives:false,rations:true);w.EnableMaterials();
   var cap=w.Found(w.units.First(u=>u.owner==0&&u.type==0));if(cap==null)continue;
   // The ford: woods by a river, unclaimed, 4-6 tiles from your capital.
   Tile site=w.tiles.Where(t=>t.realm==0&&t.owner<0&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&t.terrain!=7&&w.Distance(t.x,t.y,cap.x,cap.y)>=4&&w.Distance(t.x,t.y,cap.x,cap.y)<=6&&w.TilesAround(t.x,t.y,1).Count(Wooded)>=3)
    .OrderByDescending(t=>w.TilesAround(t.x,t.y,1).Any(n=>n.terrain==7)?1:0).ThenBy(t=>t.y*w.Width+t.x).FirstOrDefault();if(site==null)continue;
   var spot=w.tiles.Where(s=>s.realm==0&&s.owner<0&&!IsWater(s.terrain)&&!IsImpassable(s.terrain)&&!Barren(s.terrain)&&s.terrain!=7&&w.Distance(s.x,s.y,site.x,site.y)>=5&&w.Distance(s.x,s.y,site.x,site.y)<=6&&w.Distance(s.x,s.y,cap.x,cap.y)>=9).OrderBy(s=>s.y*w.Width+s.x).FirstOrDefault();if(spot==null)continue;
   foreach(var u in w.units.Where(u=>u.owner==1).ToList())w.units.Remove(u);foreach(var c in w.cities.Where(c=>c.owner==1).ToList())w.cities.Remove(c);
   var rival=w.Found(w.Add(1,0,spot.x,spot.y));if(rival==null)continue;
   foreach(int o in new[]{0,1})foreach(var id in new[]{"toolmaking","pottery","bronze","agriculture"}){int k=TechnologyCatalog.Index(id);if(k>=0)w.nations[o].tech[k]=true;}
   w.tech=w.nations[0].tech;w.met[1]=true;foreach(var n in w.nations){n.gold=200;n.stone=30;}w.gold=200;w.nations[0].timber=40;
   // The neighbour's plan: a Workshop under way, a Granary next, and a Settler bound for the ford.
   w.ChooseProject(rival,30,false);w.SettleBuilding(rival);rival.buildingProduction=w.CostFor(rival,30)-4;w.ChooseProject(rival,7,true);
   rival.pop=Math.Max(rival.pop,4);
   foreach(var u in w.units.Where(u=>u.owner==0&&u.type!=0).ToList())w.units.Remove(u);
   var near=w.TilesAround(site.x,site.y,2).Where(t=>w.Distance(t.x,t.y,site.x,site.y)==2&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&t.owner<0).OrderBy(t=>w.Distance(t.x,t.y,cap.x,cap.y)).FirstOrDefault();if(near==null)continue;
   foreach(var u in w.units.Where(u=>u.owner==0&&u.type==0).ToList())w.units.Remove(u);var mySettler=w.Add(0,0,near.x,near.y);
   var look=w.TilesAround(site.x,site.y,1).FirstOrDefault(t=>t!=site&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&t.owner<0&&!(t.x==near.x&&t.y==near.y));if(look==null)continue;w.Add(1,3,look.x,look.y);
   var theirs=w.Add(1,0,rival.x,rival.y);w.Order(theirs,site.x,site.y);
   // What the neighbour knows: its own surroundings and your land in view of its scout or traders.
   foreach(var t in w.tiles)if(w.Distance(t.x,t.y,rival.x,rival.y)<=8||w.Distance(t.x,t.y,cap.x,cap.y)<=3)w.nations[1].known[t.y*w.Width+t.x]=true;
   foreach(var t in w.TilesAround(cap.x,cap.y,2).Where(t=>t.owner==0).Take(4))if(!Wooded(t)&&!IsWater(t.terrain)&&t.terrain!=2&&!(t.x==cap.x&&t.y==cap.y)){t.terrain=1;t.cleared=false;}
   ApplyValleyVariant(w,variant,rival,site,cap);w.Reveal();return w;}
  return World.New(seed,"Riverfold",0,1,1,0,1,2,2,hex:true,natives:false,rations:true);
 }
 static void ApplyValleyVariant(World w,string v,City rival,Tile site,City cap){
  Action<int> clearWoods=r=>{foreach(var t in w.TilesAround(rival.x,rival.y,r)){if(w.Distance(t.x,t.y,site.x,site.y)<=1)continue;if(Forested(t.terrain))t.terrain=Cleared(t.terrain);if(Wooded(t))t.cleared=true;if(t.resource==MaterialRules.TimberResource)t.resource=0;}};
  var n=w.nations[1];
  if(v=="plenty"){n.timber=40;return;}
  if(v=="short"){n.timber=0;var need=w.AssessValley(1);n.timber=Math.Max(0,need.shortfall-4);return;}
  if(v=="lasting"){clearWoods(7);n.timber=0;rival.buildingQueue.Add(31);return;}
  if(v=="alternative"){clearWoods(7);n.timber=0;rival.buildingQueue.Add(31);
   // An unclaimed wood a few tiles from its capital, away from the ford.
   var grove=w.TilesAround(rival.x,rival.y,4).Where(t=>t.owner<0&&w.Distance(t.x,t.y,rival.x,rival.y)>=3&&w.Distance(t.x,t.y,site.x,site.y)>=5&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&t.terrain!=2).OrderBy(t=>t.y*w.Width+t.x).Take(4).ToList();
   foreach(var t in grove){t.terrain=1;t.cleared=false;}return;}
  if(v=="farmland"){n.timber=40;
   foreach(var t in w.TilesAround(site.x,site.y,2))if(!IsWater(t.terrain)&&!IsImpassable(t.terrain)&&t.terrain!=7&&!(t.x==site.x&&t.y==site.y)){t.terrain=0;t.cleared=true;if(t.resource==0&&(t.x+t.y)%3==0)t.resource=1;}
   foreach(var t in w.TilesAround(rival.x,rival.y,8))if(w.Distance(t.x,t.y,site.x,site.y)>3&&w.Distance(t.x,t.y,rival.x,rival.y)>2&&t.owner<0&&!IsWater(t.terrain)&&!IsImpassable(t.terrain)){t.terrain=3;t.resource=0;}
   return;}
 }
}
}
