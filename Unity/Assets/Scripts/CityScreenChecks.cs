using System;
using System.Collections.Generic;
using System.Linq;
namespace ParadigmRealm {
// The redesigned city screen's rule helpers (16 Nov 2026): breakdowns add up to the totals, growth turns match the growth line,
// queues reorder and shrink, and previewing a building leaves the city exactly as it was.
public static class CityScreenChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("City screen: "+what);}
 public static void Run(){
  var w=World.New(181,"Town",0,1,1,0,1,2,4,hex:true,natives:false);var c=w.Found(w.units.First(u=>u.owner==0&&u.type==0));c.pop=4;c.scientists=1;c.food=5;
  var v=w.Yield(c);for(int k=0;k<4;k++)Check(w.YieldBreakdown(c,k).Sum(p=>p.Value)==v[k],"the "+new[]{"food","shields","science","gold"}[k]+" breakdown adds up to "+v[k]);
  foreach(int focus in new[]{3,4}){c.focus=focus;var f=w.Yield(c);Check(w.YieldBreakdown(c,2).Sum(p=>p.Value)==f[2]&&w.YieldBreakdown(c,3).Sum(p=>p.Value)==f[3],"breakdowns add up under focus "+focus);}c.focus=0;
  int turns=w.GrowthTurns(c);Check(turns<0||w.GrowthStatus(c).Contains("Growth in "+turns+" turn"),"growth turns match the growth line ("+turns+")");
  w.ChooseProject(c,World.UnitBuild(2),false);w.ChooseProject(c,World.UnitBuild(1),true);w.ChooseProject(c,World.UnitBuild(0),true);
  Check(c.queue.Count==2&&w.LineTurns(c,false).Count==3&&w.LineTurns(c,false).SequenceEqual(w.LineTurns(c,false).OrderBy(t=>t)),"line turns count up through the queue");
  int first=c.queue[0],second=c.queue[1];Check(w.MoveQueued(c,false,1,-1)&&c.queue[0]==second&&c.queue[1]==first,"a queued item moves up");Check(!w.MoveQueued(c,false,0,-1),"the first queued item cannot move above the item in progress");
  Check(w.RemoveQueued(c,false,0)&&c.queue.Count==1&&c.queue[0]==first&&c.build==World.UnitBuild(2),"a queued item is removed and the item in progress stays");
  // Previews leave no trace, and a Harbour lights the sea tiles it feeds.
  string Snapshot(City x)=>string.Join(",",x.buildings)+"|"+x.granary+x.library+x.walls+"|"+string.Join(",",w.Yield(x));
  string before=Snapshot(c);var lit=new List<Tile>();foreach(int b in World.BuildingIds)w.PreviewBuilding(c,b,lit);Check(Snapshot(c)==before,"previewing every building leaves the city unchanged");
  var sea=w.Workable(c).Where(t=>t.terrain==4||t.terrain==6||t.terrain==12).ToList();var d=w.PreviewBuilding(c,43,lit);
  Check(d!=null&&lit.Count==sea.Count&&lit.All(t=>sea.Contains(t)),"a Harbour lights exactly the city's sea tiles ("+sea.Count+")");
  Check(w.PreviewBuilding(c,World.UnitBuild(2),lit)==null&&lit.Count==0,"units have no preview");
 }
}
}
