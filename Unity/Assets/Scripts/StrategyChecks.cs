using System;
using System.Linq;
namespace ParadigmRealm {
 public static class StrategyChecks {
  static void Check(bool ok,string name){if(!ok)throw new Exception("Strategy: "+name);}
  static World Setup(){var w=World.New(421);foreach(var t in w.tiles){t.terrain=3;t.resource=0;t.farm=t.mine=false;}w.At(9,12).terrain=0;w.Found(w.units.First(u=>u.owner==0&&u.type==0));return w;}
  public static void Run(){
   var w=Setup();var c=w.cities.First(a=>a.owner==0);c.pop=3;c.focus=2;
   foreach(var t in w.Workable(c)){t.terrain=2;t.mine=true;}
   var food=w.At(c.x+1,c.y);food.terrain=0;food.farm=true;food.mine=false;
   Check(w.Worked(c).Contains(food)&&w.Yield(c)[0]>=0,"production focus feeds citizens when feasible");
   c.scientists=3;Check(w.Yield(c)[0]<0&&w.GrowthStatus(c).Contains("shortage"),"specialist trade-off is visible");
   c.scientists=0;c.focus=4;var gold=w.At(c.x-1,c.y);gold.resource=3;w.nations[c.owner].tech[TechnologyCatalog.Index("bronze")]=false;int hidden=w.EffectiveTileYield(c,gold)[2];w.nations[c.owner].tech[TechnologyCatalog.Index("bronze")]=true;Check(w.EffectiveTileYield(c,gold)[2]==hidden+World.ResourceTrade[3]&&!w.ResourceKnown(-1,0)==false,"gold gives nothing until Bronze Working (mining) is known");Check(w.Worked(c).Contains(gold),"commerce values gold while feeding city");
   int before=w.EffectiveTileYield(c,food)[0];w.tech[TechnologyCatalog.Index("chemistry")]=true;
   Check(w.EffectiveTileYield(c,food)[0]==before+1,"technology food counted in tile allocation");
   c.pop=6;Check(w.GrowthStatus(c).Contains("Aqueduct"),"growth blocker names remedy");
   foreach(var t in w.Workable(c)){t.terrain=0;t.farm=true;}
   c.food=10000;w.EndTurn();Check(c.pop==6&&c.food==w.FoodNeeded(c),"blocked storage bounded, population preserved");
   foreach(var n in w.nations)for(int i=0;i<n.tech.Length;i++)n.tech[i]=true;
   // Avoid triggering science victory while testing building decisions.
   foreach(var n in w.nations)n.tech[TechnologyCatalog.Index("transcendence")]=false;
   Check(w.PreferredBuilding(c)==37,"AI selects aqueduct at growth limit");
   c.buildings.Add(37);c.food=w.FoodNeeded(c);int population=c.pop;w.EndTurn();Check(c.pop==population+1,"growth resumes with infrastructure");
   w=Setup();c=w.cities.First(a=>a.owner==0);c.pop=3;c.scientists=3;c.food=0;w.EndTurn();Check(c.pop==2&&c.scientists==2,"starvation clamps specialists");
   Check(w.Valid(),"new state remains valid under save schema");
   w=Setup();int owner=1;
   int chosen=w.PreferredResearch(owner);Check(w.AvailableTech(owner).Contains(chosen),"AI research respects prerequisites");
   foreach(var t in w.tiles){t.terrain=0;t.resource=0;t.farm=t.mine=t.road=false;}
   var rival=w.cities.First(a=>a.owner==owner);rival.pop=3;
   w.nations[owner].tech[TechnologyCatalog.Index("agriculture")]=true;w.nations[owner].tech[TechnologyCatalog.Index("wheel")]=true;
   var worker=w.units.First(u=>u.owner==owner&&u.type==1);worker.x=rival.x+1;worker.y=rival.y;
   w.Reveal();for(int turn=0;turn<8;turn++)w.EndTurn();
   Check(w.tiles.Any(t=>t.owner==owner&&t.farm),"AI worker completes useful food improvement under normal turn rules");
   // Pure renderer invariants: deterministic fields and identical shared-edge samples.
   Func<int,float,float,TerrainInk> sample=(id,x,y)=>new TerrainInk(id/21f,.4f,.2f);
   var a=TerrainPainter.Pixel(6,5.5f,17,TerrainDemo.At,sample);var b=TerrainPainter.Pixel(6,5.5f,17,TerrainDemo.At,sample);
   Check(a.r==b.r&&a.g==b.g&&a.b==b.b,"renderer deterministic");
   for(int t=0;t<21;t++){var ink=TerrainPainter.Pixel(.5f,.5f,99,(x,y)=>t,sample);Check(ink.r>=0&&ink.r<=1&&ink.g>=0&&ink.g<=1,"all terrain colours bounded");}
  }
 }
}
