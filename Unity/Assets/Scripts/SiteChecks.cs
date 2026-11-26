using System;
using System.Linq;
namespace ParadigmRealm {
// Resource competition (owner brief, 26 Nov 2026): the woodland scenario's outcomes, checked on the rules.
public static class SiteChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Sites: "+what);}
 public static void Run(){
  var w=World.WoodlandScenario();var wood=w.WoodlandSite();Check(wood!=null,"the scenario has its woodland in sight");
  var cap=w.cities.First(c=>c.owner==0);var rival=w.cities.First(c=>c.owner==1);
  // 1. Both sides want it now, each for its own work, with what they know.
  var mine=w.AssessSite(0,wood);var theirs=w.AssessSite(1,wood);
  Check(mine.known&&mine.understood&&mine.reachable&&mine.kind==0,"you know it, know a use for it now and can reach it ("+string.Join(" | ",mine.lines.ToArray())+")");
  Check(theirs.known&&theirs.understood,"the neighbour knows it and wants it for its own work");
  int gainMine=w.CampBenefit(0,wood,0,0),gainTheirs=w.CampBenefit(1,wood,0,0);{var dbg=w.CampForecast(0,wood,0,World.CampWork+1);var inc0=w.MaterialIncome(0);Check(gainMine>0&&gainTheirs>0,"a camp would bring both sides' work forward ("+gainMine+", "+gainTheirs+" turns; income "+inc0[0]+"/"+inc0[1]+", stock "+w.nations[0].timber+", granary "+cap.buildingProduction+"/"+w.CostFor(cap,7)+" paidFor "+cap.paidFor+" timber "+w.BuildingTimber(7)+" stone "+w.BuildingStone(7)+"; jobs "+w.MaterialJobs(0).Count+"; plan "+string.Join(",",dbg.Select(p=>p.Key+":"+p.Value[0]+">"+p.Value[1]).ToArray())+")");}
  Check(!mine.lines.Any(l=>l.Contains(World.BuildName(rival.buildingProject)+" in "+rival.name)),"your assessment does not reveal what the neighbour is building");
  // Fog: a place never seen is not assessed.
  {var unseen=w.tiles.FirstOrDefault(t=>!t.seen&&World.Wooded(t));if(unseen!=null)Check(!w.AssessSite(0,unseen).known,"a woodland you have not seen tells you nothing");}
  // 2. You commit the worker: the camp is built and the predicted benefit arrives.
  var plan=w.CampForecast(0,wood,0,99);int without=plan.Values.Max(v=>v[0]);
  Check(w.SendWorkerToCamp(0,wood)=="","a Worker is sent to set up the camp");
  var copy=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(w));
  int turns=0,granaryTurn=-1;var sent=w.units.Where(u=>u.owner==0&&u.type==1).OrderBy(u=>w.Distance(u.x,u.y,wood.x,wood.y)).First();while((granaryTurn<0||w.OutpostAt(wood.x,wood.y)==null)&&turns<40){w.EndTurn();turns++;if(cap.granary&&granaryTurn<0)granaryTurn=turns;}
  var camp=w.OutpostAt(wood.x,wood.y);
  Check(camp!=null&&camp.owner==0,"the camp stands at the woodland (worker at "+sent.x+","+sent.y+" job "+sent.job+" next "+sent.nextJob+" order "+sent.hasOrder+"; woods "+wood.x+","+wood.y+"; "+w.CampSiteReason(0,wood)+")");turns=granaryTurn;Check(cap.granary&&turns<without,"the Granary finishes sooner than without the camp ("+turns+" against "+without+")");
  w.EndTurn();Check(w.ledger.made.Any(l=>l.StartsWith("Timber camp")),"the ledger names the camp as a producer");
  // Saving keeps the camp; the neighbour's view of it is its own.
  var saved=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(w));Check(saved.OutpostAt(wood.x,wood.y)!=null&&saved.Valid(),"the camp is saved");
  // 3. Buying instead: a people that cannot spare it says so; one with plenty sells, and the stock moves once.
  {var b=copy;b.nations[1].timber=0;Check(b.ShipmentReason(0,1,0,10).Contains("cannot spare"),"a neighbour short of timber will not sell it");
   b.nations[1].timber=200;int gold0=b.nations[0].gold,t0=b.nations[0].timber,price=b.ShipmentPrice(1,0,10);string why=b.BuyShipment(0,1,0,10);
   Check(why==""&&b.nations[0].timber==t0+10&&b.nations[0].gold==gold0-price&&b.nations[1].timber==190,"bought from a neighbour with plenty: 10 timber for "+price+" gold ("+why+")");}
  // 4. Knowledge is each side's own: Mining shows you stone in the hills, not your neighbour.
  {var hill=w.tiles.FirstOrDefault(t=>t.terrain==2&&!World.Wooded(t)&&t.seen&&t.owner!=0&&(w.nations[1].known==null||w.nations[1].known[t.y*w.Width+t.x]));
   if(hill!=null){int mi=TechnologyCatalog.Index("mining");w.nations[0].tech[mi]=false;w.nations[1].tech[mi]=false;Check(w.CampKind(0,hill)<0&&w.CampKind(1,hill)<0,"without Mining hills are nothing to either");w.nations[0].tech[TechnologyCatalog.Index("mining")]=true;Check(w.CampKind(0,hill)==1&&w.CampKind(1,hill)<0,"with Mining you see a quarry; your neighbour still does not");}}
  // 5. A broken way: if the woodland becomes the neighbour's land without passage, nothing arrives and the forecast says so.
  {var t=w.tiles[camp.cell];int before=w.MaterialIncome(0)[0];t.owner=1;w.ForgetCampRoutes();var inc=w.MaterialIncome(0)[0];Check(inc<before,"a camp on land closed to you sends nothing home ("+before+" to "+inc+")");t.owner=-1;w.ForgetCampRoutes();}
  // 6. The neighbour acts on its own reasons: given time and a shortage, it may set up a camp of its own; every stock stays whole.
  for(int k=0;k<30;k++)w.EndTurn();Check(w.Valid()&&w.nations.All(n=>n.timber>=0&&n.stone>=0),"thirty more turns keep every stock whole");
 }
}
}
