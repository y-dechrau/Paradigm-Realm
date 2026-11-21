using System;
using System.Linq;
namespace ParadigmRealm {
// Dormant worlds (21 Nov 2026): an undiscovered world's peoples act one turn in three with three turns' worth of yields, and wake
// when a home civilisation reaches the world.
public static class DormantChecks {
 static void Check(bool ok,string what){if(!ok)throw new Exception("Dormant worlds: "+what);}
 public static void Run(){
  var w=World.New(341,"Dormant",0,1,1,0,1,2,3,hex:true,peoplesPerWorld:2,rations:true);w.dormantWorlds=true;
  var natives=Enumerable.Range(1,w.civCount-1).Where(o=>w.IsNativeNation(o)).ToList();Check(natives.Count>0,"the test world has parallel-world peoples");
  int o1=natives[0];int r=CivilisationCatalog.NativeRealm(w.nations[o1].civ);
  Check(!w.RealmAwake(r)&&w.Dormant(o1),"an undiscovered world is dormant");
  int skips=Enumerable.Range(0,6).Count(k=>{w.turn=10+k;return w.DormantSkips(o1);});Check(skips==4,"a dormant people acts one turn in "+World.DormantEvery+" ("+(6-skips)+" of 6)");
  // Played: cities still grow and the people still advances.
  var copy=UnityEngine.JsonUtility.FromJson<World>(UnityEngine.JsonUtility.ToJson(w));copy.dormantWorlds=false;w.turn=copy.turn=10;
  for(int k=0;k<30;k++){w.EndTurn();copy.EndTurn();}
  int popDormant=w.cities.Where(c=>natives.Contains(c.owner)).Sum(c=>c.pop),popAwake=copy.cities.Where(c=>natives.Contains(c.owner)).Sum(c=>c.pop);
  Check(popDormant*10>=popAwake*6,"dormant peoples keep growing (population "+popDormant+" against "+popAwake+" awake)");
  Check(w.Valid(),"the game stays valid");
  // A home unit in the world wakes it.
  var spot=w.tiles.First(t=>t.realm==r&&!PixelArt.IsWater(t.terrain)&&!World.IsImpassable(t.terrain));w.Add(0,3,spot.x,spot.y);w.turn++;
  Check(w.RealmAwake(r)&&!w.Dormant(o1),"a home unit in the world wakes it");
 }
}
}
