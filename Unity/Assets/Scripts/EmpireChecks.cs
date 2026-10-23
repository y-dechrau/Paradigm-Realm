using System;
using System.Linq;
namespace ParadigmRealm {
public static class EmpireChecks {
 static void Check(bool ok,string name){if(!ok)throw new Exception("Empire: "+name);}
 static World Setup(){var w=World.New(917);foreach(var t in w.tiles){t.terrain=0;t.village=false;}var c=w.Found(w.units.First(u=>u.owner==0&&u.type==0));w.gold=w.nations[0].gold=1000;return w;}
 static void Grant(World w,int owner,string id){w.nations[owner].tech[TechnologyCatalog.Index(id)]=true;}
 public static void Run(){
 Check(World.UnitNames.Length==World.Power.Length&&World.Power.Length==World.Moves.Length&&World.Moves.Length==World.UnitCosts.Length&&World.UnitCosts.Length==World.UnitTech.Length&&World.UnitTech.Length==World.UnitResource.Length&&World.UnitResource.Length==World.Upkeep.Length,"unit definition alignment");
 Check(World.BuildingIds.Length==World.BuildingNames.Length&&World.BuildingIds.Length==World.BuildingCosts.Length&&World.BuildingIds.Length==World.BuildingTech.Length,"building definition alignment");
 var w=Setup();var c=w.cities.First(a=>a.owner==0);c.pop=3;int worked=w.Worked(c).Count;Check(w.AdjustSpecialist(c,0,1)&&w.Worked(c).Count==worked-1,"specialists replace workers");Check(w.AdjustSpecialist(c,1,1)&&w.AdjustSpecialist(c,2,1)&&!w.AdjustSpecialist(c,0,1),"specialist population cap");c.pop=1;w.NormaliseSpecialists(c);Check(c.scientists+c.merchants+c.entertainers==1,"starvation specialist clamp");
 Check(!w.ChangeGovernment(0,2),"government research gate");Grant(w,0,"philosophy");Check(w.ChangeGovernment(0,2)&&w.nations[0].revolution==2&&!w.ChangeGovernment(0,0),"revolution blocks repeated switch");w.EndTurn();w.EndTurn();Check(w.nations[0].revolution==0,"revolution expires");
 Check(w.GrowthLimit(c)==6,"default growth cap");c.buildings.Add(37);Check(w.GrowthLimit(c)==12,"aqueduct cap");c.buildings.Add(41);Check(w.GrowthLimit(c)==20,"sewer cap");
 w=Setup();c=w.cities.First(a=>a.owner==0);Grant(w,0,"mysticism");Check(w.ChooseProject(c,36,false),"new building selectable");int money=w.gold;Check(w.Rush(c,true)&&w.gold==money-w.Cost(36)*3,"rush price");w.Produce(c,1);Check(w.HasBuilding(c,36),"rushed building completes");
 var rival=w.cities.First(a=>a.owner==1);Grant(w,0,"writing");Grant(w,1,"writing");w.ChooseProject(c,60,false);w.ChooseProject(rival,60,false);rival.buildingProduction=20;int rivalGold=w.nations[1].gold;w.Produce(c,w.Cost(60));Check(w.OwnWonder(0,60)&&rival.buildingProject==-1&&w.nations[1].gold==rivalGold+10&&w.BuildReason(rival,60)!="","world wonder uniqueness and refund");
 var settler=w.Add(0,0,c.x+6,c.y);var city2=w.Found(settler);Check(city2!=null,"second city");var caravan=w.Add(0,11,city2.x,city2.y);caravan.homeCity=c.id;money=w.gold;Check(w.DeliverCaravan(caravan,city2)&&!w.units.Contains(caravan)&&w.gold>money&&w.TradeIncome(0)==6,"domestic caravan delivery and both endpoint income");caravan=w.Add(0,11,city2.x,city2.y);caravan.homeCity=c.id;Check(!w.DeliverCaravan(caravan,city2),"duplicate route blocked");
 w.met[1]=true;w.SetBorders(1);caravan=w.Add(0,11,rival.x-1,rival.y);caravan.homeCity=c.id;Check(w.DeliverCaravan(caravan,rival)&&w.TradeIncome(0)==9,"foreign route");w.SetWar(1,true);Check(w.TradeIncome(0)==6,"war suspends trade");w.SetWar(1,false);Check(w.TradeIncome(0)==9,"peace restores trade");
 w=Setup();w.met[1]=true;Grant(w,1,"toolmaking");Check(w.BuyTechnology(1,TechnologyCatalog.Index("toolmaking"))&&w.HasTech(0,"toolmaking"),"technology purchase");Check(!w.BuyTechnology(1,TechnologyCatalog.Index("toolmaking")),"no repeat technology purchase");money=w.gold;Check(w.GiftGold(1)&&w.gold==money-25&&w.nations[1].relations==10,"diplomatic gift");
 var scout=w.Add(0,3,12,15);w.At(13,15).village=true;Check(w.Move(scout,13,15)&&!w.At(13,15).village,"village reward consumed once");w.AutoExplore(scout);w.CancelActivity(scout);Check(!scout.exploring&&!scout.hasOrder,"X stops exploration");
 Check(w.Valid(),"expanded state validates");w.tradeRoutes=null;w.UpgradeSave();Check(w.tradeRoutes!=null&&w.Valid(),"legacy absent routes migrated");
 for(int seed=0;seed<3;seed++){w=World.New(seed);w.Found(w.units.First(u=>u.owner==0&&u.type==0));for(int turn=0;turn<50;turn++){w.EndTurn();Check(w.Valid(),"campaign state, seed "+seed+" turn "+turn);}}
 }
}
}
