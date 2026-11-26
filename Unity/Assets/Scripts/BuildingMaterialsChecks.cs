using System;
using System.Linq;
namespace ParadigmRealm {
// Building costs (21 Nov 2026): gold, timber and stone paid when a building starts, refunded if it is cancelled, waiting when
// the nation cannot pay; stockpiles filled each turn; off unless switched on (older checks keep paying gold at the end).
public static class BuildingMaterialsChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Building costs: "+what);}
 public static void Run(){
  var w=World.New(351,"Builders",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);var c=w.Found(w.units.First(u=>u.owner==0&&u.type==0));w.nations[0].tech[TechnologyCatalog.Index("pottery")]=true;
  Check(w.BuildingTimber(7)==0&&w.PayReason(c,7)=="","off until switched on");
  w.EnableMaterials();var n=w.nations[0];Check(n.timber==MaterialRules.StartStock+MaterialRules.StartStockPerCity&&n.stone==n.timber,"a starting stockpile ("+n.timber+")");
  int g=w.BuildingGold(7),t=w.BuildingTimber(7),s=w.BuildingStone(7);Check(g>0&&t>0&&t+s>=MaterialRules.Minimum,"a Granary costs gold, timber and stone ("+w.BuildingPriceText(7)+")");
  Check(w.BuildingTimber(9)==0&&w.BuildingStone(9)>0,"Walls are stone");Check(w.BuildingTimber(36)+w.BuildingStone(36)==0,"faith buildings need no materials");
  n.gold=500;w.gold=500;int gold0=n.gold,timber0=n.timber,stone0=n.stone;w.ChooseProject(c,7,false);w.SettleBuilding(c);
  // Since 26 Nov 2026 only the gold is paid at the start; timber and stone are drawn as the work goes (MaterialEconomyChecks).
  Check(c.paidFor==7&&n.gold==gold0-g&&n.timber==timber0&&n.stone==stone0,"the gold is paid the moment it starts, timber and stone as it is built");
  w.CancelProject(c,7);w.SettleBuilding(c);Check(c.paidFor<0&&n.gold==gold0&&n.timber==timber0&&n.stone==stone0,"cancelling refunds it in full, once");
  w.SettleBuilding(c);Check(n.gold==gold0&&n.timber==timber0,"and only once");
  // Without the gold it waits and makes no progress.
  n.gold=0;w.gold=0;w.ChooseProject(c,7,false);w.SettleBuilding(c);Check(c.paidFor<0&&w.PayReason(c,7).Contains("gold"),"without gold it cannot start ("+w.PayReason(c,7)+")");
  int before=c.buildingProduction;w.Produce(c,5);Check(c.buildingProduction==before,"an unpaid building makes no progress");
  // Income: every city gathers some; then it starts, finishes without paying again.
  var inc=w.MaterialIncome(0);Check(inc[0]>=MaterialRules.CityTimber&&inc[1]>=MaterialRules.CityStone,"every city gathers timber and stone ("+inc[0]+", "+inc[1]+")");
  n.gold=gold0;w.gold=gold0;w.Produce(c,0);Check(c.paidFor==7&&n.gold==gold0-g,"once there is the gold it starts");int paidGold=n.gold;
  w.Produce(c,w.CostFor(c,7));Check(!c.granary,"its work done, it still waits for its timber and stone");
  c.paidTimber=t;c.paidStone=s;w.Produce(c,0);Check(c.granary&&n.gold==paidGold&&c.paidFor<0,"with them installed it finishes without paying again");
  // A whole game stays valid, with rivals building too.
  var r=World.New(352,"Rivals",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);r.EnableMaterials();for(int k=0;k<60;k++)r.EndTurn();
  Check(r.Valid()&&r.cities.Where(x=>x.owner>0).Sum(x=>x.buildings.Count+(x.granary?1:0)+(x.library?1:0)+(x.walls?1:0))>0,"rivals still build in a 60-turn game");
  var copy=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(r));Check(copy.buildingMaterials&&copy.nations[0].timber==r.nations[0].timber&&copy.cities.Select(x=>x.paidFor).SequenceEqual(r.cities.Select(x=>x.paidFor)),"stockpiles and payments are saved");
 }
}
}
