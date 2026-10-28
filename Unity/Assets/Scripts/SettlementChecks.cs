using System;
using System.Linq;
namespace ParadigmRealm {
 public static class SettlementChecks {
  static void Check(bool ok,string why){if(!ok)throw new Exception("Settlement: "+why);}
  static World Flat(){var w=World.New(213);foreach(var t in w.tiles){t.terrain=t.rememberedTerrain=0;t.resource=t.rememberedResource=0;t.seen=true;t.rememberedOwner=t.owner;t.rememberedCity="";t.farm=t.mine=t.fort=t.village=false;for(int owner=0;owner<3;owner++)w.nations[owner].known[t.y*w.Width+t.x]=true;}return w;}
  static void Terrain(World w,int x,int y,int id){w.At(x,y).terrain=w.At(x,y).rememberedTerrain=id;}
  public static void Run(){
   var w=Flat();var settler=w.units.First(u=>u.owner==0&&u.type==0);Terrain(w,settler.x,settler.y,2);var c=w.Found(settler);
   Check(c!=null&&w.CityCentreYield(c).SequenceEqual(new[]{2,3,0}),"hill centre production bonus");int production=w.Yield(c)[1];Terrain(w,c.x,c.y,0);Check(w.Yield(c)[1]==production-1,"hill centre bonus enters actual production once");
   Check(w.GrowthLimit(c)==6,"dry base growth limit");Terrain(w,c.x+1,c.y,6);Check(w.GrowthLimit(c)==8,"lake adjacency gives fresh water housing");Terrain(w,c.x+1,c.y,4);Check(w.GrowthLimit(c)==6,"ocean is not fresh water");Terrain(w,c.x,c.y,7);Check(w.CityCentreYield(c)[0]==3&&w.GrowthLimit(c)==8,"river centre food and fresh water");
   c.buildings.Add(37);Check(w.GrowthLimit(c)==12,"aqueduct remains 12 rather than stacking water bonus");c.buildings.Add(41);Check(w.GrowthLimit(c)==20,"sewer remains 20");
   Check(w.CityEconomyReport(c).Contains("city centre 3"),"food explanation includes river bonus");
   w=Flat();settler=w.units.First(u=>u.owner==0&&u.type==0);settler.moves=0;Check(w.Found(settler)==null&&w.FoundReason(settler).Contains("movement"),"founding consumes movement eligibility");settler.moves=4;Terrain(w,settler.x,settler.y,8);Check(w.Found(settler)==null&&w.FoundReason(settler).Contains("Fire"),"cold settlement gate explained");w.tech[TechnologyCatalog.Index("fire")]=true;Check(w.Found(settler)!=null,"fire unlocks cold settlement");
   w=Flat();var before=w.AssessSettlement(0,18,12,9,12);Check(before.Suitable,"remote explored site available");var hidden=w.At(19,12);hidden.seen=false;
   before=w.AssessSettlement(0,18,12,9,12);hidden.terrain=6;hidden.resource=3;hidden.mine=true;
   var after=w.AssessSettlement(0,18,12,9,12);Check(before.score==after.score&&before.food==after.food&&before.freshWater==after.freshWater&&before.resources.SequenceEqual(after.resources),"hidden geography and resources do not leak into adviser");
   w.cities.Add(new City{id=w.nextId++,owner=1,x=20,y=12,name="Unseen settlement"});w.RebuildTerritory();after=w.AssessSettlement(0,18,12,9,12);Check(after.Suitable&&after.score==before.score,"unseen foreign city and territory do not leak");
   w.Add(0,3,18,12);w.Reveal();Check(!w.AssessSettlement(0,18,12,9,12).Suitable,"revealed territory updates legality");
   w=Flat();settler=w.units.First(u=>u.owner==0&&u.type==0);settler.x=15;settler.y=15;
   foreach(var t in w.tiles)Terrain(w,t.x,t.y,3);
   for(int y=13;y<=17;y++)for(int x=18;x<=22;x++)Terrain(w,x,y,0);
   Terrain(w,20,15,7);var rich=w.AssessSettlement(0,20,15,15,15);var poor=w.AssessSettlement(0,10,15,15,15);
   Check(rich.score>poor.score&&rich.food>poor.food&&rich.housing==8,"fertile fresh-water site outranks equally distant barren site");
   var options=w.SettlementOptions(settler);Check(options.Count>0&&options[0].food>=0,"adviser recommends sustainable known food");
   var first=options.Select(r=>r.x+","+r.y).ToArray();Check(first.SequenceEqual(w.SettlementOptions(settler).Select(r=>r.x+","+r.y)),"stable recommendation ties");
   w.war[1]=true;w.Add(1,2,options[0].x,options[0].y);w.Add(0,3,options[0].x-1,options[0].y);w.Reveal();Check(w.SettlementOptions(settler).All(r=>w.Distance(r.x,r.y,options[0].x,options[0].y)>3),"adviser avoids visible hostile troops");
   w=Flat();c=w.cities.First(a=>a.owner==1);c.pop=3;w.nations[1].tech[0]=true;w.nations[1].gold=100;Check(w.ShouldTrainSettler(c),"healthy treasury and food allow expansion");w.nations[1].gold=1;Check(!w.ShouldTrainSettler(c),"poor treasury delays expansion");
   w=Flat();w.Found(w.units.First(u=>u.owner==0&&u.type==0));for(int y=12;y<=18;y++)for(int x=13;x<=19;x++)Terrain(w,x,y,3);Terrain(w,17,15,7);Terrain(w,17,14,0);Terrain(w,17,16,0);
   var ai=w.Add(1,0,15,15);for(int i=0;i<8&&w.units.Contains(ai);i++)w.EndTurn();
   var colony=w.cities.FirstOrDefault(a=>a.owner==1&&w.Distance(a.x,a.y,15,15)<=4);
   Check(colony!=null&&w.FreshWater(colony.x,colony.y),"AI travels to and founds a fresh-water colony");Check(w.Valid(),"expanded world remains save-valid");
  }
 }
}
