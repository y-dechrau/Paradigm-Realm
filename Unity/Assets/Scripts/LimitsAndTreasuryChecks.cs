using System;
using System.Linq;
namespace ParadigmRealm {
// Owner requests 27 Nov 2026, late: unit limits (auto-managed cities no longer flood the map with Scouts), camps and villages
// under new borders disappear, and the treasurer's advice when the treasury runs dry.
public static class LimitsAndTreasuryChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Limits and treasury: "+what);}
 public static void Run(){
  // Auto-managed cities over thirty turns: Scouts stay within the limit, and every unit kind with a limit stays within it.
  {var w=World.ValleyTalkScenario("plenty");w.nationalForces=true;foreach(var c in w.cities.Where(c=>c.owner==0)){c.autoManage=true;c.pop=Math.Max(c.pop,4);}
   for(int t=0;t<30;t++)w.EndTurn();int scouts=w.units.Count(u=>u.owner==0&&u.type==3);int lim=w.UnitLimit(0,3);
   Check(scouts<=lim,"auto-managed cities keep Scouts within the limit ("+scouts+" of "+lim+")");
   foreach(int type in new[]{0,1,3,10,11,15}){int have=w.units.Count(u=>u.owner==0&&u.type==type),l=w.UnitLimit(0,type);Check(l<0||have<=l,World.UnitNames[type]+" within its limit ("+have+" of "+l+")");}}
  // The limit is a reason in the build list, and counts what other cities are already training.
  {var w=World.ValleyTalkScenario("plenty");var c=w.cities.First(x=>x.owner==0);int lim=w.UnitLimit(0,3);while(w.units.Count(u=>u.owner==0&&u.type==3)<lim)w.Add(0,3,c.x,c.y);
   Check(w.BuildReason(c,World.UnitBuild(3)).StartsWith("Limit reached"),"a city cannot start a Scout over the limit ("+w.BuildReason(c,World.UnitBuild(3))+")");
   foreach(var u in w.units.Where(u=>u.owner==0&&u.type==3).ToList())w.units.Remove(u);Check(w.BuildReason(c,World.UnitBuild(3))=="","and can again when there are none");}
  // A camp and a tribal village inside new borders disappear; the village's people join the nearest town.
  {var w=World.ValleyTalkScenario("plenty");w.barbarians=true;var c=w.cities.First(x=>x.owner==0);
   var near=w.tiles.Where(t=>t.realm==0&&!PixelArt.IsWater(t.terrain)&&w.Distance(t.x,t.y,c.x,c.y)==2&&!w.cities.Any(o=>o.x==t.x&&o.y==t.y)).Take(2).ToList();
   w.camps.Add(new BarbarianCamp{id=88001,x=near[0].x,y=near[0].y});w.raiders.Add(new Raider{id=88002,x=near[0].x,y=near[0].y,camp=88001});near[1].village=true;int pop=c.pop;
   w.RebuildTerritory();Check(near[0].owner==0&&near[1].owner==0,"the test tiles lie inside your borders");
   Check(!w.camps.Any(k=>k.id==88001)&&!w.raiders.Any(r=>r.id==88002),"a camp inside borders breaks up, and its raiders go");
   Check(!near[1].village&&c.pop==pop+1,"a village inside borders settles among your people (size "+pop+" to "+c.pop+")");}
  // The treasurer: advice when the treasury will run dry, best first, and the priorities put back as they were.
  {var w=World.ValleyTalkScenario("plenty");var n=w.nations[0];n.scienceRate=60;n.militaryRate=20;w.gold=n.gold=0;n.arrears=1;{var away=w.tiles.First(t=>t.realm==0&&t.owner<0&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain));w.Add(0,2,away.x,away.y);w.Add(0,7,away.x,away.y);}
   Check(w.NeedsTreasurer(0),"an empty treasury calls the treasurer");var advice=w.DebtAdvice(0);
   Check(advice.Count>=2&&advice.Any(a=>a.gain>0),"with advice that says what it would gain ("+string.Join(" | ",advice.Select(a=>a.text+" +"+a.gain).ToArray())+")");
   Check(n.scienceRate==60&&n.militaryRate==20,"trying the priorities leaves them as they were");
   for(int k=1;k<advice.Count;k++)Check(advice[k-1].gain>=advice[k].gain,"the advice is in order of what it gains");}
 }
}
}
