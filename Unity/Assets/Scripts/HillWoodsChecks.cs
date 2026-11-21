using System;
using System.Linq;
namespace ParadigmRealm {
// Wooded hills (21 Nov 2026): placed beside forest and jungle, +1 shield, cleared like groves, farms and mines wait for clearing.
public static class HillWoodsChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Wooded hills: "+what);}
 public static void Run(){
  var w=World.New(331,"Hills",0,1,1,0,2,2,3,hex:true,natives:false,rations:true);var hills=w.tiles.Where(t=>t.terrain==2).ToList();var wooded=hills.Where(World.HillWoods).ToList();
  Check(wooded.Count>0&&wooded.Count<hills.Count,"some hills are wooded ("+wooded.Count+" of "+hills.Count+")");Check(w.tiles.All(t=>t.woods==0||t.terrain==2),"only hills carry woods");
  Check(wooded.Any(t=>t.woods==2)||!w.tiles.Any(t=>t.terrain==11),"jungle hills appear where there is jungle");
  var h=wooded[0];int with=w.TileYield(h)[1];h.cleared=true;int bare=w.TileYield(h)[1];h.cleared=false;Check(with==bare+World.HillWoodsShields,"woods add a shield ("+bare+" to "+with+")");
  Check(World.Wooded(h),"wooded hills count as woodland, so Workers can clear them");
  var worker=w.Add(0,1,h.x,h.y);worker.moves=4;w.nations[0].tech[TechnologyCatalog.Index("agriculture")]=true;Check(w.JobReason(worker,2).Contains("Clear"),"a farm waits for the woods to be cleared ("+w.JobReason(worker,2)+")");
  h.cleared=true;Check(!World.HillWoods(h)&&!w.JobReason(worker,2).Contains("Clear the"),"once cleared the hill can be farmed");
  var copy=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(w));Check(copy.tiles.Count(t=>t.woods>0)==w.tiles.Count(t=>t.woods>0),"woods are saved");
 }
}
}
