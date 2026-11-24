using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// National forces (owner brief, 20 Nov 2026; design notes in Design/FORCES-2026-11-20.md). Cities build buildings; the
// civilisation raises, refits and keeps its soldiers and warships ("formations": one ordinary map unit each) through one
// national programme on the Empire > Forces screen. Three capabilities limit it:
//  - service capacity: personnel the population can spare (citizens x policy share, plus a small base); each formation type
//    commits a number of service points while it exists, and an approved order reserves them at once;
//  - arms output: equipment made each turn (a base, workshops, factories, barracks, connected Iron and the Empire screen's
//    military priority), spent down the ordered list of raise, refit and replacement orders, top first;
//  - specialist support: here horses; each connected pasture keeps a number of mounted formations in the field.
// Upkeep stays where it always was (World.BudgetDetail), so a formation costs gold from the turn it stands on the map.
// Scouts, settlers, workers, medics, caravans, boats, galleys and priests stay on the cities' unit line.
[Serializable] public class ForceOrder {
 // kind: 0 raise a new formation, 1 refit (modernise) one, 2 replace its losses. unit: the formation refitted or reinforced.
 // city: where a new formation is deployed. paid/cost in arms. service/horses: what the order reserves until it completes.
 public int id,kind,type,unit=-1,city=-1,paid,cost,service,horses;public bool held;
}
public static class ForcesRules {
 // Service points each unit type commits (0: not a formation).
 public static readonly int[] Service={0,0,1,0,1,2,2,1,2,2,0,0,0,0,3,0};
 public const int BaseService=2,PeacePercent=50,MobilisedPercent=100;
 public const int BaseArms=3,CityArms=1,WorkshopArms=3,FactoryArms=5,BarracksArms=1,IronPercent=25;
 public const int HorsesPerSource=2,HorseResource=4;
 // Mobilisation: the share of every city's shields given up, and the least number of turns a policy must stand.
 public const int MobiliseShieldPercent=25,PolicyMinTurns=10;
 // Gold a turn for each service point over capacity (after a demobilisation or a fall in population).
 public const int ShortfallGold=2;
 public const int MaxOrders=8;
 public static readonly string[] KindNames={"Raise","Refit","Replace losses"};
 public static bool Formation(int type){return type>=0&&type<World.UnitNames.Length&&Service[type]>0;}
 public static bool Mounted(int type){return type==5||type==8;}
}
public partial class World {
 public bool nationalForces;
 List<ForceOrder> Orders(int owner){var n=nations[owner];if(n.forceOrders==null)n.forceOrders=new List<ForceOrder>();return n.forceOrders;}
 public IList<ForceOrder> ForceOrders(int owner){return Orders(owner);}
 bool Formation(Unit u){return u!=null&&ForcesRules.Formation(u.type);}
 // ---- service capacity ----
 public int Citizens(int owner){int n=0;foreach(var c in cities)if(c.owner==owner)n+=c.pop;return n;}
 public int ServiceCapacity(int owner,bool mobilised){return ForcesRules.BaseService+Citizens(owner)*(mobilised?ForcesRules.MobilisedPercent:PeaceServicePercent(owner))/100;}
 public int ServiceCapacity(int owner){return ServiceCapacity(owner,nations[owner].mobilised);}
 public int ServiceServing(int owner){int n=0;foreach(var u in units)if(u.owner==owner)n+=ForcesRules.Service[u.type];return n;}
 public int ServiceReserved(int owner){return Orders(owner).Sum(o=>o.service);}
 public int ServiceFree(int owner){return ServiceCapacity(owner)-ServiceServing(owner)-ServiceReserved(owner);}
 public int ServiceShortfall(int owner){return nationalForces?Math.Max(0,-ServiceFree(owner)):0;}
 public string ServiceText(int owner){return "Base "+ForcesRules.BaseService+" + "+Citizens(owner)+" citizens x "+(nations[owner].mobilised?ForcesRules.MobilisedPercent+"% (mobilised)":PeaceServicePercent(owner)+"% (peacetime"+(PeaceServicePercent(owner)!=ForcesRules.PeacePercent?", Expansive":"")+")")+" = "+ServiceCapacity(owner);}
 // ---- horse support ----
 public int HorseSources(int owner){int n=0;foreach(int cell in Network(owner)){var t=tiles[cell];if(t.owner==owner&&t.resource==ForcesRules.HorseResource&&t.farm)n++;}if(ImportedResource(owner,ForcesRules.HorseResource))n++;return n;}
 public int HorseSupport(int owner){return HorseSources(owner)*ForcesRules.HorsesPerSource;}
 public int HorsesCommitted(int owner){return units.Count(u=>u.owner==owner&&ForcesRules.Mounted(u.type))+Orders(owner).Sum(o=>o.horses);}
 // ---- arms output ----
 public List<KeyValuePair<string,int>> ArmsLines(int owner){
  var lines=new List<KeyValuePair<string,int>>{new KeyValuePair<string,int>("Base (smiths and levies)",ForcesRules.BaseArms)};
  int work=0,fact=0,barr=0,priority=0,levies=0;foreach(var c in cities){if(c.owner!=owner||Disorder(c))continue;levies++;if(HasBuilding(c,30))work++;if(HasBuilding(c,40))fact++;if(HasBuilding(c,32))barr++;priority+=ArmyBudget(c);}
  if(levies>0)lines.Add(new KeyValuePair<string,int>("Local levies ("+levies+" cities x "+ForcesRules.CityArms+")",levies*ForcesRules.CityArms));
  if(work>0)lines.Add(new KeyValuePair<string,int>("Workshops ("+work+" x "+ForcesRules.WorkshopArms+")",work*ForcesRules.WorkshopArms));
  if(fact>0)lines.Add(new KeyValuePair<string,int>("Factories ("+fact+" x "+ForcesRules.FactoryArms+")",fact*ForcesRules.FactoryArms));
  if(barr>0)lines.Add(new KeyValuePair<string,int>("Barracks ("+barr+" x "+ForcesRules.BarracksArms+")",barr*ForcesRules.BarracksArms));
  if(priority>0)lines.Add(new KeyValuePair<string,int>("Military priority ("+nations[owner].militaryRate+"% of trade)",priority));
  int sum=lines.Sum(l=>l.Value);if(HasResource(owner,2))lines.Add(new KeyValuePair<string,int>("Connected Iron (+"+ForcesRules.IronPercent+"%)",sum*ForcesRules.IronPercent/100));
  return lines;
 }
 public int ArmsOutput(int owner){return ArmsLines(owner).Sum(l=>l.Value);}
 // The military priority takes trade out of gold only while the programme has work (as it once did only while a soldier trained).
 public bool ArmsWorking(int owner){return Orders(owner).Any(o=>!o.held);}
 public bool MilitaryShareOn(City c){return nationalForces?ArmsWorking(c.owner):IsCombatBuild(c.build);}
 // What a building would add to national arms output (city screen preview). -1 if it adds nothing.
 public int ArmsWith(City c,int building){if(!nationalForces||building!=30&&building!=40&&building!=32||HasBuilding(c,building))return -1;PreviewSet(c,building,true);try{return ArmsOutput(c.owner);}finally{PreviewSet(c,building,false);}}
 // Turns until each order completes, arms flowing down the list as ForcesTurn spends them.
 public List<int> OrderTurns(int owner){
  var list=Orders(owner);var left=list.Select(o=>Math.Max(0,o.cost-o.paid)).ToList();var turns=new List<int>();int arms=ArmsOutput(owner);
  for(int i=0;i<list.Count;i++)turns.Add(list[i].held?0:-1);if(arms<=0)return turns;
  for(int t=1;t<=999&&turns.Contains(-1);t++){int a=arms;for(int i=0;i<list.Count&&a>0;i++){if(turns[i]!=-1)continue;int give=Math.Min(a,left[i]);left[i]-=give;a-=give;if(left[i]==0)turns[i]=t;}}
  return turns;
 }
 // ---- what can be ordered, and exactly why not ----
 public bool FormationKnown(int owner,int type){return ForcesRules.Formation(type)&&(UnitTech[type]<0||nations[owner].tech[UnitTech[type]]);}
 public City DeployCity(int owner,int type){
  var n=nations[owner];int id=IsNaval(type)?n.seaDeploy:n.landDeploy;var c=cities.Find(x=>x.id==id&&x.owner==owner);
  if(c!=null&&DeployReason(c,type)=="")return c;return cities.Where(x=>x.owner==owner&&DeployReason(x,type)=="").OrderBy(x=>x.id==n.capital?0:1).ThenBy(x=>x.id).FirstOrDefault();
 }
 public string DeployReason(City c,int type){if(c==null)return "No city to deploy to.";if(IsNaval(type)&&!PlaceAllows(c,UnitBuild(type)))return c.name+" has no port for warships.";return "";}
 public string RaiseReason(int owner,int type,City at){
  if(!nationalForces)return "Soldiers are trained in cities in this game.";if(!ForcesRules.Formation(type))return World.UnitNames[type]+" is trained in cities.";
  if(!FormationKnown(owner,type))return "Requires "+TechNames[UnitTech[type]]+".";
  if(Orders(owner).Count>=ForcesRules.MaxOrders)return "The programme is full ("+ForcesRules.MaxOrders+" orders).";
  int r=UnitResource[type];if(r>0&&r!=ForcesRules.HorseResource&&!HasResource(owner,r))return "Requires connected "+ResourceNames[r]+" (an improved source joined to your capital by road).";
  int need=ForcesRules.Service[type],free=ServiceFree(owner);
  if(need>free)return "Requires "+need+" service; "+Math.Max(0,free)+" free ("+ServiceServing(owner)+" serving + "+ServiceReserved(owner)+" reserved of "+ServiceCapacity(owner)+(ServiceShortfall(owner)>0?"; "+ServiceShortfall(owner)+" over capacity":"")+").";
  if(ForcesRules.Mounted(type)){int support=HorseSupport(owner),used=HorsesCommitted(owner);if(support==0)return "Requires horse support: a pasture on Horses joined to your capital by road.";if(used>=support)return "Requires 1 free horse support; all "+support+" are committed.";}
  if(at==null)at=DeployCity(owner,type);var why=DeployReason(at,type);if(why!="")return why;
  return "";
 }
 public string Raise(int owner,int type,City at){
  if(at==null)at=DeployCity(owner,type);var why=RaiseReason(owner,type,at);if(why!="")return why;
  Orders(owner).Add(new ForceOrder{id=nextId++,kind=0,type=type,city=at.id,cost=Math.Max(1,CostFor(at,UnitBuild(type))),service=ForcesRules.Service[type],horses=ForcesRules.Mounted(type)?1:0});
  return "";
 }
 ForceOrder OrderFor(Unit u){return u==null?null:Orders(u.owner).FirstOrDefault(o=>o.unit==u.id);}
 public bool InOrder(Unit u){return OrderFor(u)!=null;}
 City CityUnder(Unit u){return cities.Find(c=>c.owner==u.owner&&c.x==u.x&&c.y==u.y);}
 public int RefitCost(Unit u,int target){var c=CityUnder(u)??cities.Find(x=>x.owner==u.owner);if(c==null)return 0;int full=CostFor(c,UnitBuild(target)),old=CostFor(c,UnitBuild(u.type));return Math.Max(full/4,full-old);}
 public string RefitReason(int owner,Unit u){
  if(!nationalForces)return "Upgrades are bought with gold in this game.";if(u==null||u.owner!=owner||!Formation(u))return "Not one of your formations.";
  int t=UpgradeOf(u.type);if(t<0)return World.UnitNames[u.type]+" has no refit yet.";
  if(InOrder(u))return "Already in the programme.";if(UnitTech[t]>=0&&!nations[owner].tech[UnitTech[t]])return "Refit to "+UnitNames[t]+" requires "+TechNames[UnitTech[t]]+".";
  int r=UnitResource[t];if(r>0&&r!=ForcesRules.HorseResource&&!HasResource(owner,r))return "Refit to "+UnitNames[t]+" requires connected "+ResourceNames[r]+".";
  if(CityUnder(u)==null)return "Must stand in one of your cities to refit.";
  if(Orders(owner).Count>=ForcesRules.MaxOrders)return "The programme is full ("+ForcesRules.MaxOrders+" orders).";
  int extra=Math.Max(0,ForcesRules.Service[t]-ForcesRules.Service[u.type]);if(extra>ServiceFree(owner))return "Refit needs "+extra+" more service; "+Math.Max(0,ServiceFree(owner))+" free.";
  // A mounted formation keeps its own horse support through a refit.
  if(ForcesRules.Mounted(t)&&!ForcesRules.Mounted(u.type)&&HorsesCommitted(owner)>=HorseSupport(owner))return "Requires 1 free horse support; all "+HorseSupport(owner)+" are committed.";
  return "";
 }
 public string Refit(int owner,Unit u){
  var why=RefitReason(owner,u);if(why!="")return why;int t=UpgradeOf(u.type);
  Orders(owner).Add(new ForceOrder{id=nextId++,kind=1,type=t,unit=u.id,cost=Math.Max(1,RefitCost(u,t)),service=Math.Max(0,ForcesRules.Service[t]-ForcesRules.Service[u.type]),horses=ForcesRules.Mounted(t)&&!ForcesRules.Mounted(u.type)?1:0});
  u.refitting=true;CancelOrder(u);u.fortified=false;u.moves=0;return "";
 }
 public int ReplaceCost(Unit u){var c=CityUnder(u)??cities.Find(x=>x.owner==u.owner);if(c==null)return 0;return Math.Max(1,CostFor(c,UnitBuild(u.type))*(10-u.hp)/20);}
 public string ReplaceReason(int owner,Unit u){
  if(!nationalForces)return "";if(u==null||u.owner!=owner||!Formation(u))return "Not one of your formations.";if(u.hp>=10)return "At full strength.";if(InOrder(u))return "Already in the programme.";
  if(CityUnder(u)==null)return "Must stand in one of your cities to take on replacements.";if(Orders(owner).Count>=ForcesRules.MaxOrders)return "The programme is full.";return "";
 }
 public string Replace(int owner,Unit u){var why=ReplaceReason(owner,u);if(why!="")return why;Orders(owner).Add(new ForceOrder{id=nextId++,kind=2,type=u.type,unit=u.id,cost=ReplaceCost(u)});u.refitting=true;CancelOrder(u);u.moves=0;return "";}
 // Cancelling releases the order's reservations (they are read from the list, so exactly once) and frees a locked formation.
 // Arms already put into it are lost.
 public bool CancelForceOrder(int owner,int id){var list=Orders(owner);var o=list.Find(x=>x.id==id);if(o==null)return false;list.Remove(o);var u=units.Find(x=>x.id==o.unit);if(u!=null)u.refitting=false;return true;}
 public bool MoveForceOrder(int owner,int index,int dir){var list=Orders(owner);int j=index+dir;if(index<0||index>=list.Count||j<0||j>=list.Count)return false;var k=list[index];list[index]=list[j];list[j]=k;return true;}
 public string Redeploy(int owner,int id,City c){var o=Orders(owner).Find(x=>x.id==id);if(o==null||o.kind!=0)return "No such order.";if(c==null||c.owner!=owner)return "Not your city.";var why=DeployReason(c,o.type);if(why!="")return why;o.city=c.id;return "";}
 // Disbanding removes the formation; its service and horse support are free at once. Its equipment is not recovered.
 public bool Disband(int owner,Unit u){if(u==null||u.owner!=owner||!Formation(u))return false;var o=OrderFor(u);if(o!=null)Orders(owner).Remove(o);units.Remove(u);DrownStranded();return true;}
 // ---- mobilisation ----
 public string MobiliseReason(int owner,bool on){var n=nations[owner];if(!nationalForces)return "Not available in this game.";if(n.mobilised==on)return on?"Already mobilised.":"Already at peacetime service.";int since=turn-n.policySince;if(since<ForcesRules.PolicyMinTurns)return (on?"Demobilised":"Mobilised")+" "+since+" turns ago; the policy must stand "+ForcesRules.PolicyMinTurns+" turns ("+(ForcesRules.PolicyMinTurns-since)+" more).";return "";}
 public string SetMobilised(int owner,bool on){var why=MobiliseReason(owner,on);if(why!="")return why;nations[owner].mobilised=on;nations[owner].policySince=turn;budgetMemo=null;return "";}
 // ---- each turn ----
 void ForcesTurn(int owner){
  if(!nationalForces)return;var list=Orders(owner);
  // No soldier on a city's line (an older plan, the imperial priority or auto-manage may have put one there).
  foreach(var c in cities)if(c.owner==owner){if(c.build>=0&&ForcesRules.Formation(BuildUnit(c.build))){c.build=c.queue.Count>0?c.queue[0]:-1;if(c.queue.Count>0)c.queue.RemoveAt(0);c.production=0;}c.queue.RemoveAll(b=>ForcesRules.Formation(BuildUnit(b)));}
  // Orders whose formation has gone (killed, captured) end; their reservations go with them.
  list.RemoveAll(o=>o.kind!=0&&!units.Any(u=>u.id==o.unit&&u.owner==owner));
  int arms=ArmsOutput(owner);
  foreach(var o in list.ToList()){
   if(o.held){Deploy(owner,o);continue;}
   int give=Math.Min(arms,o.cost-o.paid);o.paid+=give;arms-=give;
   if(o.paid>=o.cost)Finish(owner,o);
  }
 }
 void Finish(int owner,ForceOrder o){
  if(o.kind==0){Deploy(owner,o);return;}
  var u=units.Find(x=>x.id==o.unit);Orders(owner).Remove(o);if(u==null)return;u.refitting=false;
  if(o.kind==1){string was=UnitName(u);u.type=o.type;u.moves=0;var c=CityUnder(u);if(c!=null)u.gear=Math.Max(u.gear,GearFor(c,u.type));if(owner==0)Note(was+" refitted as "+UnitName(u)+"; its experience is kept.");}
  else{u.hp=10;if(owner==0)Note(UnitName(u)+" is back at full strength.");}
 }
 // A finished formation stands in its chosen city (warships at its port). If that city is lost or full the formation is held,
 // with its commitments, until there is room or another city is chosen; rivals send it to another city at once.
 void Deploy(int owner,ForceOrder o){
  var c=cities.Find(x=>x.id==o.city&&x.owner==owner);
  if((c==null||!CanDeployAt(c,o.type))&&owner!=0){c=cities.Where(x=>x.owner==owner&&CanDeployAt(x,o.type)).OrderBy(x=>x.id==nations[owner].capital?0:1).FirstOrDefault();if(c!=null)o.city=c.id;}
  if(c==null||!CanDeployAt(c,o.type)){if(!o.held&&owner==0)Note("A new "+UnitNames[o.type]+" is ready but "+(c==null?"its city is no longer yours":c.name+" has no room")+"; choose another city on the Forces screen.");o.held=true;return;}
  var launch=IsNaval(o.type)?LaunchTile(c,o.type):At(c.x,c.y);var made=Add(owner,o.type,launch.x,launch.y);made.moves=0;made.homeCity=c.id;made.gear=GearFor(c,o.type);
  if(HasBuilding(c,32)||OwnWonder(owner,63)||HasEdict(owner,3,2)||HasTrait(owner,TraitMilitaristic)||StatecraftOf(owner)==StatecraftRules.Centralised){made.victories=2;made.promotion=1;}
  Orders(owner).Remove(o);if(owner==0){Note(UnitNames[o.type]+" raised at "+c.name+".");AddBuildNotice(c,UnitBuild(o.type),false,"raised");}
 }
 bool CanDeployAt(City c,int type){if(DeployReason(c,type)!="")return false;var probe=new Unit{owner=c.owner,type=type,id=-1};var launch=IsNaval(type)?LaunchTile(c,type):At(c.x,c.y);return launch!=null&&Capacity(probe,launch.x,launch.y);}
 // Older games: soldiers on a city's line become national orders with the shields already put in; queued ones are dropped.
 void MigrateToForces(){
  nationalForces=true;
  foreach(var c in cities){
   if(c.build>=0&&ForcesRules.Formation(BuildUnit(c.build))){int type=BuildUnit(c.build);var list=Orders(c.owner);if(list.Count<ForcesRules.MaxOrders){int cost=Math.Max(1,CostFor(c,c.build));list.Add(new ForceOrder{id=nextId++,kind=0,type=type,city=c.id,cost=cost,paid=Math.Min(cost-1,Math.Max(0,c.production)),service=ForcesRules.Service[type],horses=ForcesRules.Mounted(type)?1:0});}
    c.build=c.queue.Count>0?c.queue[0]:-1;if(c.queue.Count>0)c.queue.RemoveAt(0);c.production=0;}
   c.queue.RemoveAll(b=>ForcesRules.Formation(BuildUnit(b)));if(c.build>=0&&ForcesRules.Formation(BuildUnit(c.build)))c.build=-1;
  }
 }
 bool ForcesValid(){
  if(!nationalForces)return true;
  foreach(var n in nations){if(n==null)return false;if(n.forceOrders==null)continue;foreach(var o in n.forceOrders)if(o==null||o.kind<0||o.kind>2||o.type<0||o.type>=UnitNames.Length||o.cost<=0||o.paid<0||o.service<0||o.horses<0||o.horses>1)return false;}
  return true;
 }
}
}
