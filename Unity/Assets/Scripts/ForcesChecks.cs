using System;
using System.Linq;
namespace ParadigmRealm {
// The owner's acceptance scenarios for national forces (20 Nov 2026), run with the other content checks.
public static class ForcesChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Forces: "+what);}
 static World Fresh(int seed){var w=World.New(seed,"Forces",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);w.nationalForces=true;var c=w.Found(w.units.First(u=>u.owner==0&&u.type==0));c.pop=6;w.nations[0].gold=500;w.gold=500;return w;}
 static City Home(World w){return w.cities.First(c=>c.owner==0);}
 static void Turn(World w,int n=1){for(int k=0;k<n&&w.result=="";k++){foreach(int o in Enumerable.Range(1,w.civCount-1))w.Relation(0,o).peaceUntil=w.turn+10;w.EndTurn();}}
 public static void Run(){
  // 1. A city builds a Granary while the nation raises infantry; both go on in the normal turn.
  {var w=Fresh(301);var c=Home(w);w.nations[0].tech[TechnologyCatalog.Index("pottery")]=true;Check(w.ChooseProject(c,7,false),"the city can start a Granary ("+w.BuildReason(c,7)+")");int units=w.units.Count(u=>u.owner==0&&u.type==2);
   Check(w.Raise(0,2,c)=="","a Warrior can be ordered at the start ("+w.RaiseReason(0,2,c)+")");Check(w.BuildReason(c,World.UnitBuild(2)).Contains("nation"),"cities no longer train soldiers");
   Check(w.BuildReason(c,World.UnitBuild(3))==""||w.BuildReason(c,World.UnitBuild(3)).StartsWith("Requires"),"Scouts stay with the cities");
   Turn(w);Check(c.buildingProduction>0,"the Granary progresses while the nation raises a Warrior");Check(w.ForceOrders(0).Count==0||w.ForceOrders(0)[0].paid>0,"the Warrior's order progresses");
   Turn(w,30);Check(w.units.Count(u=>u.owner==0&&u.type==2)==units+1,"the Warrior is raised and deployed once");Check(w.ForceOrders(0).Count==0,"the finished order leaves the programme");}
  // 2. Horse support for two mounted formations, both in service: no third, but a refit to a Knight keeps its slot and its experience.
  {var w=Fresh(302);var c=Home(w);c.pop=16;var t=Enumerable.Range(0,6).Select(k=>w.Step(c.x,c.y,World.HexDi[k],World.HexDj[k])).First(x=>x!=null&&!PixelArt.IsWater(x.terrain)&&!World.IsImpassable(x.terrain));
   t.owner=0;t.road=true;t.farm=true;t.resource=ForcesRules.HorseResource;w.ResetNetworksForChecks();foreach(var id in new[]{"horseback","chivalry"})w.nations[0].tech[TechnologyCatalog.Index(id)]=true;
   Check(w.HorseSupport(0)==ForcesRules.HorsesPerSource,"one connected pasture gives "+ForcesRules.HorsesPerSource+" horse support ("+w.HorseSupport(0)+")");
   var a=w.Add(0,8,c.x,c.y);var b=w.Add(0,8,c.x,c.y);a.victories=3;a.promotion=1;
   string why=w.RaiseReason(0,8,c);Check(why.Contains("all "+ForcesRules.HorsesPerSource+" are committed"),"a third Horseman is refused, saying why ("+why+")");
   Check(w.RaiseReason(0,2,c)=="","horse support does not block infantry");
   Check(w.RaiseReason(0,5,c).Contains("committed"),"nor can a new Knight be raised");
   Check(w.Refit(0,a)=="","a Horseman can be refitted to a Knight with every horse committed ("+w.RefitReason(0,a)+")");Check(a.refitting&&!w.Move(a,t.x,t.y),"a refitting formation cannot move");
   Turn(w,40);Check(a.type==5&&a.victories==3&&a.promotion==1&&w.units.Contains(a)&&!a.refitting,"the refit finishes as a Knight with its experience");Check(w.HorsesCommitted(0)==2,"it still uses one horse slot");
   // Losing the pasture keeps every mounted formation; infantry can still be raised.
   t.farm=false;w.ResetNetworksForChecks();Check(w.HorseSupport(0)==0&&w.units.Contains(a)&&w.units.Contains(b),"losing the horses keeps the mounted formations");Check(w.RaiseReason(0,2,c)=="","and infantry can still be raised");}
  // 3. Enough people, little industry: delivery is slow and says so; a refit put first delays the new formation.
  {var w=Fresh(303);var c=Home(w);w.nations[0].tech[TechnologyCatalog.Index("bronze")]=true;var old=w.units.First(u=>u.owner==0&&u.type==2);old.x=c.x;old.y=c.y;
   Check(w.Raise(0,7,c)=="","a Spearman can be ordered");int alone=w.OrderTurns(0)[0];Check(alone>1,"with base output a Spearman takes "+alone+" turns");
   Check(w.Refit(0,old)=="","the Warrior can be refitted ("+w.RefitReason(0,old)+")");Check(w.MoveForceOrder(0,1,-1),"the refit can be put first");
   int after=w.OrderTurns(0)[1];Check(after>alone,"putting the refit first delays the Spearman ("+alone+" to "+after+" turns)");}
  // 4. Mobilisation allows an order peacetime refused, costs shields, and demobilising over capacity is a shortfall, not a loss.
  {var w=Fresh(304);var c=Home(w);c.pop=4;int cap=w.ServiceCapacity(0);while(w.ServiceFree(0)>0)w.Add(0,2,c.x,c.y);
   Check(w.RaiseReason(0,2,c).StartsWith("Requires 1 service"),"peacetime capacity refuses another Warrior ("+w.RaiseReason(0,2,c)+")");
   int shields=w.Yield(c)[1];Check(w.SetMobilised(0,true)=="","mobilisation can be declared");Check(w.ServiceCapacity(0)>cap,"mobilisation raises capacity ("+cap+" to "+w.ServiceCapacity(0)+")");
   Check(w.Yield(c)[1]<=shields&&(shields<4||w.Yield(c)[1]<shields),"mobilisation costs shields ("+shields+" to "+w.Yield(c)[1]+")");
   Check(w.Raise(0,2,c)=="","mobilised, the Warrior can be ordered");Check(w.SetMobilised(0,false)!="","the policy cannot be dropped at once");
   while(w.ServiceFree(0)>0)w.Add(0,2,c.x,c.y);w.turn+=ForcesRules.PolicyMinTurns;int count=w.units.Count(u=>u.owner==0);Check(w.SetMobilised(0,false)=="","after the minimum the nation can stand down");
   Check(w.ServiceShortfall(0)>0&&w.units.Count(u=>u.owner==0)==count,"standing down over capacity keeps every formation and shows a shortfall");Check(w.BudgetDetail(0).forceShortfall==w.ServiceShortfall(0)*ForcesRules.ShortfallGold,"the shortfall costs gold");
   Check(w.RaiseReason(0,2,c)!="","and blocks orders that add personnel");Check(w.SetMobilised(0,true)!="","and mobilising again must wait");}
  // 5. Cancelling releases reservations exactly once; a lost deployment city holds the formation without duplicates.
  {var w=Fresh(305);var c=Home(w);int before=w.ServiceReserved(0);w.Raise(0,2,c);w.Raise(0,2,c);Check(w.ServiceReserved(0)==before+2,"two orders reserve two service");
   var first=w.ForceOrders(0)[0];Check(w.CancelForceOrder(0,first.id)&&w.ServiceReserved(0)==before+1&&!w.CancelForceOrder(0,first.id)&&w.ServiceReserved(0)==before+1,"a cancelled order releases its service exactly once");
   var o=w.ForceOrders(0)[0];o.paid=o.cost-1;c.owner=1;int mine=w.units.Count(u=>u.owner==0&&u.type==2);Turn(w);
   Check(o.held&&w.ForceOrders(0).Contains(o)&&w.units.Count(u=>u.owner==0&&u.type==2)==mine,"a finished formation with no city is held, not lost or duplicated");
   c.owner=0;var other=w.cities.First(x=>x.owner==0);Check(w.Redeploy(0,o.id,other)=="","it can be sent to another city");Turn(w);Turn(w);
   Check(w.units.Count(u=>u.owner==0&&u.type==2)==mine+1&&w.ForceOrders(0).Count==0,"it deploys exactly once");}
  // 6. A save in the middle of an order and a refit resumes with the same cost, reservations and outcome.
  {var w=Fresh(306);var c=Home(w);w.nations[0].tech[TechnologyCatalog.Index("bronze")]=true;var old=w.units.First(u=>u.owner==0&&u.type==2);old.x=c.x;old.y=c.y;w.Raise(0,2,c);w.Refit(0,old);Turn(w,2);
   var copy=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(w));Check(copy.Valid(),"a save with orders is valid");
   Check(copy.ForceOrders(0).Count==w.ForceOrders(0).Count&&copy.ForceOrders(0).Select(x=>x.paid+"/"+x.cost+"/"+x.service).SequenceEqual(w.ForceOrders(0).Select(x=>x.paid+"/"+x.cost+"/"+x.service)),"orders survive a save");
   Check(copy.units.First(u=>u.id==old.id).refitting,"a refit survives a save");Turn(w,30);Turn(copy,30);
   Check(w.units.Count(u=>u.owner==0)==copy.units.Count(u=>u.owner==0)&&w.units.First(u=>u.id==old.id).type==copy.units.First(u=>u.id==old.id).type,"both copies end the same way");}
  // 7. An older save: a soldier on a city's line becomes a national order with its shields kept.
  {var w=World.New(307,"Old",0,1,1,0,1,2,3,hex:true,natives:false,rations:true);var c=w.Found(w.units.First(u=>u.owner==0&&u.type==0));c.build=World.UnitBuild(2);c.production=12;c.queue.Add(World.UnitBuild(2));
   w.version=43;w.UpgradeSave();Check(w.nationalForces&&w.Valid(),"an older save upgrades to national forces");Check(c.build<0||!ForcesRules.Formation(World.BuildUnit(c.build)),"its city line no longer holds a soldier");
   Check(w.ForceOrders(0).Count==1&&w.ForceOrders(0)[0].paid==12&&w.ForceOrders(0)[0].type==2,"the Warrior in training becomes an order with its 12 shields");}
  // 8. Rivals raise and deploy soldiers through the same programme; a new player keeps the starting units.
  {var w=World.New(308,"Rivals",0,1,1,0,1,2,4,hex:true,natives:false,rations:true);w.nationalForces=true;int start=w.units.Count(u=>u.owner==0);
   Check(w.units.Any(u=>u.owner==0&&u.type==2)&&w.units.Any(u=>u.owner==0&&u.type==1),"a new game keeps the starting Warrior and Worker");
   int rivalsBefore=Enumerable.Range(1,w.civCount-1).Sum(o=>w.SoldierCount(o));Turn(w,80);int rivalsAfter=Enumerable.Range(1,w.civCount-1).Sum(o=>w.SoldierCount(o));
   Check(rivalsAfter>rivalsBefore,"rivals raise soldiers through the programme ("+rivalsBefore+" to "+rivalsAfter+")");
   Check(w.cities.All(c=>c.build<0||!ForcesRules.Formation(World.BuildUnit(c.build))),"no city trains a soldier");
   Check(Enumerable.Range(1,w.civCount-1).All(o=>w.ForceOrders(o).Count<=3&&w.ForceOrders(o).All(x=>x.kind!=0||w.FormationKnown(o,x.type))),"rival programmes stay short and possible");Check(w.Valid(),"the game stays valid");}
 }
}
public partial class World {internal void ResetNetworksForChecks(){networks=null;}}
}
